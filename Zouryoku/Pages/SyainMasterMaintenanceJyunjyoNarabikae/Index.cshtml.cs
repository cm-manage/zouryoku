using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;

namespace Zouryoku.Pages.SyainMasterMaintenanceJyunjyoNarabikae
{
    /// <summary>
    /// 社員並び替えページモデル
    /// </summary>
    [FunctionAuthorization]
    public class IndexModel : BasePageModel<IndexModel>
    {
        public IndexModel(
            ZouContext db,
            ILogger<IndexModel> logger,
            IOptions<AppConfig> options,
            ICompositeViewEngine viewEngine)
            : base(db, logger, options, viewEngine)
        {
        }

        // ---------------------------------------------
        // 通常のプロパティ（画面表示用など）
        // ---------------------------------------------
        public override bool UseInputAssets => true;

        /// <summary>
        /// 部署ID（選択された部署）
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public SearchCondition Condition { get; set; } = new SearchCondition();

        /// <summary>
        /// 表示用社員リスト
        /// </summary>
        public IList<SyainViewModel> Syains { get; set; } = [];

        // 排他エラーメッセージ
        public static string ErrorConflictSyain { get; } = string.Format(Const.ErrorConflictReload, "社員マスタ");

        /// <summary>
        /// 初期表示
        /// </summary>
        /// <returns>ページリザルト</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }


        /// <summary>
        /// 並び順保存
        /// </summary>
        /// <param name="syains">並び順変更対象の社員リスト</param>
        /// <returns>実行結果（JSON）</returns>
        public async Task<IActionResult> OnPostRegisterAsync(List<SyainOrderModel> syains)
        {
            // 更新対象IDのみ抽出
            var updateIds = syains.Select(s => s.Id).ToHashSet();

            // 更新対象を一度に取得
            var targetSyains = await db.Syains
                .Where(s => updateIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id);

            foreach (var dto in syains)
            {
                // 並び替え操作中に部署マスタが削除されている可能性があるため、存在を確認する
                if (!targetSyains.TryGetValue(dto.Id, out var syain))
                    return Error(ErrorConflictSyain);

                syain.Jyunjyo = dto.Jyunjyo;
            }

            // 更新を保存
            await SaveWithConcurrencyCheckAsync(ErrorConflictSyain);
            return Success();
        }

        /// <summary>
        /// 社員一覧取得API（部署ID指定）
        /// </summary>
        /// <returns>社員リスト（JSON）</returns>
        public async Task<IActionResult> OnGetSyainListAsync()
        {
            var today = DateTime.Today.ToDateOnly();

            var syains = await db.Syains
                .Include(s => s.Busyo)
                .Where(s => s.BusyoId == Condition.BusyoId
                            && s.StartYmd <= today
                            && today <= s.EndYmd
                            && s.Retired == false)
                .OrderByDescending(s => s.Jyunjyo)
                .AsNoTracking()
                .ToListAsync();

            Syains = syains.Select(SyainViewModel.FromEntity).ToList();

            var html = await PartialToJsonAsync("_SyainListPartial", this);
            return SuccessJson(data: html);
        }
    }

    /// <summary>
    /// 表示用社員モデル
    /// </summary>
    public class SyainViewModel
    {
        /// <summary>
        /// 社員ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 社員名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 並び順
        /// </summary>
        public short Jyunjyo { get; set; }

        /// <summary>
        /// エンティティから表示用モデルを作成します。
        /// </summary>
        /// <param name="syain">変換元の社員エンティティ</param>
        /// <returns>変換後のSyainViewModel</returns>
        public static SyainViewModel FromEntity(Syain syain)
        {

            return new SyainViewModel
            {
                Id = syain.Id,
                Name = syain.Name,
                Jyunjyo = syain.Jyunjyo
            };
        }
    }

    /// <summary>
    /// 検索条件モデル
    /// </summary>
    public class SearchCondition
    {
        /// <summary>
        /// 選択された部署ID（NULL許容）
        /// </summary>
        public long? BusyoId { get; set; }
    }

    /// <summary>
    /// 並び順保存用社員モデル
    /// </summary>
    public class SyainOrderModel
    {
        /// <summary>
        /// 社員ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 設定する並び順
        /// </summary>
        public short Jyunjyo { get; set; }
    }

}
