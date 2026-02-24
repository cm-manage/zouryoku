using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Extensions;
using Model.Model;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;

namespace Zouryoku.Pages.BusyoMasterMaintenanceJyunjyoNarabikae
{
    /// <summary>
    /// 部署順序並び替えページモデル
    /// </summary>
    [FunctionAuthorization]
    public class IndexModel : BasePageModel<IndexModel>
    {
        // ---------------------------------------------
        // 1. 定数
        // ---------------------------------------------
        /// <summary>
        /// このページの CSS スタイルで扱う部署階層数（レベル数）を表します。
        /// この値を超える階層構造であっても、同一または類似の色調が再利用される形で表示自体は可能です。
        /// </summary>
        public static int CssLevelCount => 6;

        // 排他エラーメッセージ
        public static string ErrorConflictBusyo { get; } = string.Format(Const.ErrorConflictReload, "部署マスタ");

        // ---------------------------------------------
        // 2. DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        public IndexModel(ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> options)
            : base(db, logger, options)
        {
        }

        // ---------------------------------------------
        // 3. 通常のプロパティ（画面表示用）
        // ---------------------------------------------
        public override bool UseInputAssets { get; } = true;

        /// <summary>
        /// 表示用部署リスト
        /// </summary>
        public IList<BusyoOrder> RootBusyoOrders { get; set; } = [];

        // ---------------------------------------------
        // 4. OnGet
        // ---------------------------------------------
        /// <summary>
        /// 初期表示
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // アクティブな部署をすべて取得し、親部署IDでルックアップを作成
            // ※並び順は Jyunjyo 昇順
            var activeBusyos = (await db.Busyos
                .Where(b => b.IsActive)
                .OrderBy(b => b.Jyunjyo)
                .AsNoTracking()
                .ToListAsync())
                .ToLookup(b => b.OyaId);

            // 再帰的に部署ツリーを構築
            // 循環参照検出用に処理済みノードセット（visited）を渡す
            // ※循環参照が発生している場合、InvalidOperationExceptionがスローされる
            // ※本来はDB設計や業務ロジックで防止すべきだが、念のため保険として実装している
            var visited = new HashSet<long>();
            RootBusyoOrders = activeBusyos[null]
                .Select(b => BusyoOrder.FromEntity(b, activeBusyos, visited))
                .ToList();

            return Page();
        }

        // ---------------------------------------------
        // 5. OnPost
        // ---------------------------------------------
        /// <summary>
        /// 並び順保存
        /// </summary>
        public async Task<IActionResult> OnPostRegisterAsync(IEnumerable<BusyoOrder> busyoOrders)
        {
            // 更新リクエストを抽出
            // フラット化して、並び順が指定されている部署のみ抽出
            var updateRequestBusyoOrders = busyoOrders
                .SelectMany(Flatten)
                .Where(bo => bo.Jyunjyo is not null)
                .ToList();

            // 更新対象部署IDの集合を作成
            var updateRequestBusyoOrderIds = updateRequestBusyoOrders
                .Select(bo => bo.Id)
                .ToHashSet();

            // 更新対象部署マップを作成（有効部署のみを対象とする）
            var updateBusyoMap = await db.Busyos
                .Where(b => b.IsActive && updateRequestBusyoOrderIds.Contains(b.Id))
                .ToDictionaryAsync(b => b.Id);

            // 更新対象部署を更新
            foreach (var bo in updateRequestBusyoOrders)
            {
                // 並び替え操作中に部署マスタが削除されている可能性があるため、存在を確認する
                if (!updateBusyoMap.TryGetValue(bo.Id, out var b))
                    return Error(ErrorConflictBusyo);

                b.Jyunjyo = bo.Jyunjyo!.Value;
                db.SetOriginalValue(b, b => b.Version, bo.Version);
            }

            // 更新を保存
            await SaveWithConcurrencyCheckAsync(ErrorConflictBusyo);

            // 排他エラーが発生した場合、エラーレスポンスを返す
            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            return Success();
        }

        // ---------------------------------------------
        // 6. private メソッド
        // ---------------------------------------------
        /// <summary>
        /// ツリー構造をフラット化する
        /// </summary>
        private static IEnumerable<BusyoOrder> Flatten(BusyoOrder busyoOrder) =>
            new[] { busyoOrder }.Concat(busyoOrder.Children?.SelectMany(Flatten) ?? []);

        // ---------------------------------------------
        // 画面固有の型
        // ---------------------------------------------
        /// <summary>
        /// 部署表示・更新ビューモデル
        /// </summary>
        public class BusyoOrder
        {
            /// <summary>
            /// <see cref="Busyo.Id"/>
            /// </summary>
            public required long Id { get; init; }

            /// <summary>
            /// <see cref="Busyo.Name"/>
            /// </summary>
            public string Name { get; init; } = "";

            /// <summary>
            /// <see cref="Busyo.Jyunjyo"/>
            /// </summary>
            public short? Jyunjyo { get; init; }

            /// <summary>
            /// <see cref="Busyo.Version"/>
            /// </summary>
            public uint Version { get; init; }

            /// <summary>
            /// <see cref="Busyo.InverseOya"/>
            /// RazorPages の仕様上、初期値を null にしないとマッピングできないため null を許容する
            /// </summary>
            public IList<BusyoOrder>? Children { get; init; }

            /// <summary>
            /// 部署エンティティから <see cref="BusyoOrder"/> ビューモデルを生成し、
            /// 子部署を再帰的にたどりながら循環参照を検出します。
            /// </summary>
            /// <param name="busyo">変換元となる部署エンティティ。</param>
            /// <param name="allBusyos">親部署 ID をキーとして子部署一覧を取得するためのルックアップ。</param>
            /// <param name="visited">循環参照検出のために、処理済み部署 ID を保持するセット。</param>
            /// <returns>指定された部署エンティティを基点とした <see cref="BusyoOrder"/>。</returns>
            /// <exception cref="InvalidOperationException">部署の親子関係に循環が検出された場合にスローされます。</exception>
            public static BusyoOrder FromEntity(Busyo busyo, ILookup<long?, Busyo> allBusyos, HashSet<long> visited)
            {
                // 循環検出
                if (!visited.Add(busyo.Id))
                    throw new InvalidOperationException("部署マスタの親子関係が循環しています。システム管理者に連絡してください。");

                return new BusyoOrder()
                {
                    Id = busyo.Id,
                    Name = busyo.Name,
                    Version = busyo.Version,
                    Children = allBusyos[busyo.Id]
                        .Select(b => FromEntity(b, allBusyos, visited))
                        .ToList()
                };
            }
        }

        // ---------------------------------------------
        // レコード・構造体
        // ---------------------------------------------
        /// <summary>
        /// 部署順序レンダリングコンテキスト
        /// </summary>
        /// <param name="BusyoOrder">部署表示・更新ビューモデル</param>
        /// <param name="InputNamePrefix">name属性用のプレフィックス文字列</param>
        /// <param name="CssLevel">適用するCSSの階層 (1ベース)</param>
        /// <remarks>
        /// 部署順序レンダリング部分ビューに渡すコンテキストです。
        /// </remarks>
        public record BusyoOrderRenderContext(BusyoOrder BusyoOrder, string InputNamePrefix, int CssLevel)
        {
            public string BusyoLevelClass => $"app-busyo-level-{CssLevel}";
        }
    }
}
