using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using System.Linq.Expressions;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Model.Enums.HolidayFlag;

namespace Zouryoku.Pages.KinmuNippouKakunin
{
    /// <summary>
    /// 勤務日報確認画面
    /// </summary>
    [FunctionAuthorization]
    public partial class IndexModel : BasePageModel<IndexModel>
    {
        // ---------------------------------------------
        // 1. 定数
        // ---------------------------------------------
        private static string ErrorReadNippou { get; } = string.Format(Const.ErrorRead, "日報実績");
        private static string ErrorReadBusyo { get; } = string.Format(Const.ErrorRead, "部署マスタ");

        // ---------------------------------------------
        // 2. DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        public IndexModel(
            ZouContext context, ILogger<IndexModel> logger, IOptions<AppConfig> options, ICompositeViewEngine viewEngine)
            : base(context, logger, options, viewEngine)
        {
        }

        // ---------------------------------------------
        // 3. 通常のプロパティ（画面表示用）
        // ---------------------------------------------
        public override bool UseInputAssets => true;

        /// <summary>
        /// 勤務日報表の検索条件。ページを返す <see cref="OnGetAsync"/> でのみ設定されます。
        /// 引数バインディングを使用するため、URLクエリパラメータからのモデルバインディングは発生しません。
        /// </summary>
        public DaysQuery TargetDaysQuery { get; private set; } = new DaysQuery();

        /// <summary>
        /// 勤務日報表の ViewModel 。ページを返す <see cref="OnGetAsync"/> でのみ設定されます。
        /// 引数バインディングを使用するため、URLクエリパラメータからのモデルバインディングは発生しません。
        /// </summary>
        public DaysViewModel TargetDaysViewModel { get; private set; } = DaysViewModel.Empty;

        // ---------------------------------------------
        // 4. OnGet
        // ---------------------------------------------
        /// <summary>
        /// イベント仕様_初期処理
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            TargetDaysQuery = new DaysQuery();
            var targetSyain = LoginInfo.User;

            var (daysViewModel, errorMessage) = await CreateDaysViewModelAsync(TargetDaysQuery.TargetYm, targetSyain);
            if (daysViewModel is null)
            {
                // エラーメッセージを空画面の上部に表示する。
                ModelState.AddModelError("", errorMessage);
                TargetDaysViewModel = DaysViewModel.Empty;
                return Page();
            }

            TargetDaysViewModel = daysViewModel;
            return Page();
        }

        /// <summary>
        /// イベント仕様_対象年月変更
        /// イベント仕様_月送りボタン
        /// イベント仕様_自分を表示ボタン
        /// イベント仕様_社員選択ボタン
        /// イベント仕様_表示切替ボタン
        /// </summary>
        /// <param name="targetDaysQuery">
        /// 検索条件を含むオブジェクト。
        /// 必須項目:
        /// - <see cref="DaysQuery.TargetSyainId"/>: 現在表示中の社員のID。未設定 (null) 時はログインユーザー。
        /// - <see cref="DaysQuery.TargetYm"/>: 対象年月。未設定時は初期値 (当年月) 。
        /// </param>
        public async Task<IActionResult> OnGetSearchAsync(DaysQuery targetDaysQuery)
        {
            // 全社員を指定可能
            var targetSyain = await GetTargetSyainAsync(targetDaysQuery.TargetSyainId);
            if (targetSyain is null) return CommonErrorResponseWithMessage(Const.ErrorSelectedDataNotExists);

            var (daysViewModel, errorMessage) = await CreateDaysViewModelAsync(targetDaysQuery.TargetYm, targetSyain);
            if (daysViewModel is null)
            {
                // エラーメッセージを画面上部に表示する。
                return CommonErrorResponseWithMessage(errorMessage);
            }

            return SuccessJson(data: new
            {
                targetSyainJoinedBusyoName = daysViewModel.TargetSyainJoinedBusyoName,
                targetSyainName = daysViewModel.TargetSyainName,
                daysPartial = await PartialToJsonAsync("_DaysPartial", daysViewModel)
            });
        }

        /// <summary>
        /// イベント仕様_人送りボタン(左)
        /// </summary>
        /// <param name="targetDaysQuery">
        /// 検索条件を含むオブジェクト。
        /// 必須項目:
        /// - <see cref="DaysQuery.TargetSyainId"/>: 現在表示中の社員のID。未設定 (null) 時はログインユーザー。
        /// </param>
        public async Task<IActionResult> OnGetPrevSyainAsync(DaysQuery targetDaysQuery)
        {
            // 現在表示中の社員を取得
            var currentSyain = await GetTargetSyainAsync(targetDaysQuery.TargetSyainId);
            if (currentSyain is null) return CommonErrorResponseWithMessage(Const.ErrorSelectedDataNotExists);

            // 逆順で次 (＝元の順で手前) または最初（＝元の順で最後）の社員を取得
            // 並び順序の昇順、社員番号の降順に並べつつ、現在社員よりも並び順序および社員番号が後の社員を検索する。
            var prevSyainId = await FindAdjacentSyainIdAsync(
                currentSyain.BusyoCode,
                s => currentSyain.Jyunjyo < s.Jyunjyo ||
                    (currentSyain.Jyunjyo == s.Jyunjyo &&
                    string.Compare(s.Code, currentSyain.Code) < 0),
                q => q.OrderBy(s => s.Jyunjyo).ThenByDescending(s => s.Code));
            if (prevSyainId is null) return CommonErrorResponseWithMessage(Const.ErrorSelectedDataNotExists);

            // 検索結果情報を取得（ビューで処理）
            return SuccessJson(data: prevSyainId);
        }

        /// <summary>
        /// イベント仕様_人送りボタン(右)
        /// </summary>
        /// <param name="targetDaysQuery">
        /// 検索条件を含むオブジェクト。
        /// 必須項目:
        /// - <see cref="DaysQuery.TargetSyainId"/>: 現在表示中の社員のID。未設定 (null) 時はログインユーザー。
        /// </param>
        public async Task<IActionResult> OnGetNextSyainAsync(DaysQuery targetDaysQuery)
        {
            // 現在表示中の社員を取得
            var currentSyain = await GetTargetSyainAsync(targetDaysQuery.TargetSyainId);
            if (currentSyain is null) return CommonErrorResponseWithMessage(Const.ErrorSelectedDataNotExists);

            // 次または最初の社員を取得
            // 並び順序の降順、社員番号の昇順に並べつつ、現在社員よりも並び順序および社員番号が後の社員を検索する。
            var nextSyainId = await FindAdjacentSyainIdAsync(
                currentSyain.BusyoCode,
                s => s.Jyunjyo < currentSyain.Jyunjyo ||
                    (s.Jyunjyo == currentSyain.Jyunjyo &&
                    string.Compare(currentSyain.Code, s.Code) < 0), // DBでの currentSyain.Code < s.Code に相当
                q => q.OrderByDescending(s => s.Jyunjyo).ThenBy(s => s.Code));
            if (nextSyainId is null) return CommonErrorResponseWithMessage(Const.ErrorSelectedDataNotExists);

            // 検索結果情報を取得（ビューで処理）
            return SuccessJson(data: nextSyainId);
        }

        // ---------------------------------------------
        // 5. private メソッド
        // ---------------------------------------------
        /// <summary>
        /// メッセージを指定して共通エラーレスポンスを返す
        /// </summary>
        private IActionResult CommonErrorResponseWithMessage(string message)
        {
            ModelState.AddModelError("", message);
            return CommonErrorResponse();
        }

        /// <summary>
        /// 指定された社員IDの社員マスタ情報を取得します。指定されていない場合はログインユーザを取得します。
        /// </summary>
        private async Task<Syain?> GetTargetSyainAsync(long? targetSyainId = null)
        {
            // 指定されていないときログインユーザを対象とする
            if (targetSyainId is null) return LoginInfo.User;

            // 社員マスタ情報の取得
            return await db.Syains.AsNoTracking().FirstOrDefaultAsync(s => s.Id == targetSyainId);
        }

        /// <summary>
        /// 指定の条件で絞り込み・並び替えた社員マスタデータで、循環ありで隣接する社員IDを取得します。
        /// </summary>
        /// <param name="busyoCode">絞り込み条件の部署コード</param>
        /// <param name="condition">隣接データの絞り込み条件</param>
        /// <param name="orderBy">並び順の指定</param>
        /// <returns>循環ありで隣接している社員ID</returns>
        private async Task<long?> FindAdjacentSyainIdAsync(
            string busyoCode, Expression<Func<Syain, bool>> condition, Func<IQueryable<Syain>, IOrderedQueryable<Syain>> orderBy)
        {
            // 並び替え済みの社員マスタデータ
            var orderedBusyoSyains = orderBy(
                db.Syains
                .AsNoTracking()
                .Where(s => s.BusyoCode == busyoCode));

            // 次の社員を取得
            var adjacentSyain = await orderedBusyoSyains.Where(condition).FirstOrDefaultAsync();

            // 最初と最後で循環させる
            // 次の社員が存在しなければ最初の社員を取得
            adjacentSyain ??= await orderedBusyoSyains.FirstOrDefaultAsync();

            return adjacentSyain?.Id;
        }

        // ---------------------------------------------
        // ViewModel 生成メソッド
        // ---------------------------------------------
        /// <summary>
        /// 指定された年月・社員の勤務日報表の ViewModel を生成します。
        /// </summary>
        private async Task<(DaysViewModel? daysViewModel, string errorMessage)> CreateDaysViewModelAsync(
            DateOnly targetYm, Syain targetSyain)
        {
            // 検索結果情報を取得
            // 日報実績情報の取得
            var startNippouYmd = new DateOnly(targetYm.Year, targetYm.Month, 1);
            var endNippouYmd = startNippouYmd.AddMonths(1);
            var nippouList = await db.Nippous
                .AsNoTracking()
                .AsSplitQuery()
                .Where(n => n.SyainId == targetSyain.Id) // 対象社員に絞り込む
                .Where(n => startNippouYmd <= n.NippouYmd && n.NippouYmd < endNippouYmd) // 対象年月に絞り込む
                .Include(n => n.SyukkinKubunId1Navigation)
                .Include(n => n.SyukkinKubunId2Navigation)
                .Include(n => n.NippouAnkens.OrderBy(na => na.AnkensId))
                    .ThenInclude(na => na.Ankens)
                    .ThenInclude(a => a.KingsJuchu)
                .ToListAsync();

            // 重複データが含まれる場合、エラー「日報実績は正しく読み込めませんでした。」を表示する。
            // 平均時間計算量: O(1)
            var visited = new HashSet<DateOnly>();
            foreach (var n in nippouList)
            {
                if (!visited.Add(n.NippouYmd)) return (null, ErrorReadNippou);
            }

            // 非稼働日情報の取得
            var hikadoubiYmdSet = await db.Hikadoubis
                .AsNoTracking()
                .Where(h => h.SyukusaijitsuFlag == 祝祭日)
                .Where(h => startNippouYmd <= h.Ymd && h.Ymd < endNippouYmd)
                .Select(h => h.Ymd)
                .ToHashSetAsync();

            // ViewModel の辞書を生成
            // ここではキーの重複は発生しない（上記の重複チェックで確認済み）
            var dayDict = nippouList
                .Select(n => new Day(n.NippouYmd, hikadoubiYmdSet.Contains(n.NippouYmd), n))
                .ToDictionary(d => d.Date);

            // 月初から月末までの表を作成
            var daysInMonth = DateTime.DaysInMonth(targetYm.Year, targetYm.Month);
            var days = Enumerable.Range(1, daysInMonth)
                .Select(day => new DateOnly(targetYm.Year, targetYm.Month, day))
                .Select(date => dayDict.TryGetValue(date, out var day) ? day : new Day(date, hikadoubiYmdSet.Contains(date)))
                .ToList();

            // 部署マスタ情報の取得
            // テストコードで使用する InMemory DB は再帰CTEをサポートしていないため、再帰CTE を使用しない方法にする必要がある。
            // テストコードのみ切り替えるような実装をしてはならない。
            // 匿名クラスやDTOを使用しない要件上 Select を使用してはならない。
            // N+1 問題回避のため while ループでの逐次取得をしてはならない。
            // 部署マスタの規模が拡大する場合、再帰CTEやキャッシュの使用を検討してください。
            var allBusyos = await db.Busyos.AsNoTracking().ToDictionaryAsync(b => b.Id);

            // 指定された社員の部署マスタ情報が存在しない、または循環が検出された場合、
            // エラー「部署マスタは正しく読み込めませんでした。」を表示する。
            if (!TryGetHierarchicalBusyoNames(targetSyain.BusyoId, allBusyos, out var hierarchicalBusyoNames)) return (null, ErrorReadBusyo);

            var daysViewModel = new DaysViewModel
            {
                TargetSyainName = targetSyain.Name,
                TargetSyainHierarchicalBusyoNames = hierarchicalBusyoNames,
                Days = days
            };
            return (daysViewModel, "");
        }

        /// <summary>
        /// 指定された部署IDの階層的な部署名を取得します。
        /// </summary>
        private static bool TryGetHierarchicalBusyoNames(
            long targetSyainBusyoId, Dictionary<long, Busyo> allBusyos, out List<string> hierarchicalBusyoNames)
        {
            var visited = new HashSet<long>();
            var busyoNames = new LinkedList<string>();

            // 全社 (Id = null) まで親部署をたどる
            long? busyoId = targetSyainBusyoId;
            while (busyoId is not null)
            {
                // 指定されている部署が存在しない場合、または循環が検出された場合、false を返す。
                if (!allBusyos.TryGetValue(busyoId.Value, out var busyo) || !visited.Add(busyo.Id))
                {
                    hierarchicalBusyoNames = [];
                    return false;
                }

                busyoNames.AddFirst(busyo.Name);
                busyoId = busyo.OyaId;
            }

            hierarchicalBusyoNames = [.. busyoNames];
            return true;
        }
    }
}
