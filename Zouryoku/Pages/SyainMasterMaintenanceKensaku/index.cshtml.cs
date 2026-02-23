using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using static Model.Enums.EmployeeWorkType;

namespace Zouryoku.Pages.SyainMasterMaintenanceKensaku
{
    /// <summary>
    /// 社員検索ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class IndexModel : BasePageModel<IndexModel>
    {
        public IndexModel(ZouContext db, ILogger<IndexModel> logger,
            IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine)
            : base(db, logger, optionsAccessor, viewEngine) { }

        public override bool UseInputAssets => true;

        /// <summary>
        /// 検索条件
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public SyainSearchCondition Condition { get; set; } = new SyainSearchCondition();

        /// <summary>
        /// 検索結果社員リスト
        /// </summary>
        public List<SyainViewModel> Results { get; set; } = [];

        /// <summary>
        /// 画面初期表示
        /// </summary>
        /// <returns>社員マスタメンテナンスページ</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            await InitializeSearchConditionOptionsAsync();
            Results = await GetSyainListAsync();
            return Page();
        }

        /// <summary>
        /// 検索ボタン押下
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetSearchAsync()
        {
            Results = await GetSyainListAsync();

            var data = await PartialToJsonAsync("_SyainSearchResults", this);
            return SuccessJson(data: data);
        }

        private async Task InitializeSearchConditionOptionsAsync()
        {
            // 勤怠属性一覧
            var kintaiList = await db.KintaiZokuseis
                .AsNoTracking()
                .Select(k => new { k.Id, k.Name })
                .ToListAsync();
            var defaultKintaiZokuseiId = kintaiList.FirstOrDefault(k => k.Id == (short)みなし対象者)?.Id;

            if (!Condition.KintaiZokuseiId.HasValue && defaultKintaiZokuseiId.HasValue)
            {
                Condition.KintaiZokuseiId = defaultKintaiZokuseiId.Value;
            }

            Condition.KintaiZokuseiOptions = new SelectList(
                kintaiList,
                "Id",
                "Name",
                Condition.KintaiZokuseiId);

            // ロール一覧
            var roles = await db.UserRoles
                .AsNoTracking()
                .Select(r => new { r.Id, r.Name })
                .ToListAsync();
            Condition.UserRoleOptions = new SelectList(roles, "Id", "Name");

            // 社員権限一覧
            var kengenList = Enum.GetValues(typeof(EmployeeAuthority))
                .Cast<EmployeeAuthority>()
                .Select(e => new { Id = (int)e, Name = e.ToString() })
                .ToList();
            Condition.KengenOptions = new SelectList(kengenList, "Id", "Name");
        }

        /// <summary>
        /// 社員一覧取得
        /// </summary>
        private async Task<List<SyainViewModel>> GetSyainListAsync()
        {
            var query = db.Syains
                .Include(s => s.Busyo)
                .Include(s => s.KintaiZokusei)
                .Include(s => s.UserRole)
                .AsNoTracking()
                .AsQueryable();

            // 社員番号
            if (!string.IsNullOrEmpty(Condition.SyainNo))
            {
                query = query.Where(s => s.Code.Contains(Condition.SyainNo));
            }

            // 社員名
            if (!string.IsNullOrEmpty(Condition.SyainName))
            {
                query = query.Where(s => s.Name.Contains(Condition.SyainName));
            }

            // 部署
            if (!string.IsNullOrEmpty(Condition.BusyoName))
            {
                query = query.Where(s => s.Busyo.Name.Contains(Condition.BusyoName));
            }

            // 退職者を含む
            if (!Condition.IncludeRetired)
            {
                query = query.Where(s => s.Retired == false);
            }

            // 勤怠属性
            if (Condition.KintaiZokuseiId.HasValue)
            {
                query = query.Where(s => s.KintaiZokuseiId == Condition.KintaiZokuseiId.Value);
            }

            // ロール
            if (Condition.UserRoleId.HasValue)
            {
                query = query.Where(s => s.UserRoleId == Condition.UserRoleId.Value);
            }

            // 社員権限
            if (Condition.Kengen.HasValue)
            {
                query = query.Where(s => (s.Kengen & Condition.Kengen.Value) != 0);
            }

            // 社員級職
            if (Condition.Grade.HasValue)
            {
                query = query.Where(s => s.Kyusyoku == Condition.Grade.Value);
            }

            var syainList = await query.OrderBy(s => s.Busyo.Jyunjyo)
                .ThenByDescending(s => s.Jyunjyo)
                .ThenByDescending(s => s.Code)
                .Select(s => new SyainViewModel(s))
                .ToListAsync();

            return syainList;
        }
    }

    /// <summary>
    /// 検索結果モデル
    /// </summary>
    public class SyainViewModel
    {
        // 部署エンティティ
        private readonly Syain _syain;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="busyo">部署エンティティ </param>
        public SyainViewModel(Syain syain)
        {
            _syain = syain;
        }

        /// <summary>部署ID</summary>
        [Display(Name = "社員BASEマスタのID")]
        public long SyainBaseId => _syain.SyainBaseId;

        /// <summary>社員番号</summary>
        [Display(Name = "社員番号")]
        public string SyainNo => _syain.Code;

        /// <summary>社員名</summary>
        [Display(Name = "社員名")]
        public string Name => _syain.Name;

        /// <summary>部署名</summary>
        [Display(Name = "部署")]
        public string BusyoName => _syain.Busyo.Name;

        /// <summary>社員級職</summary>
        [Display(Name = "級職")]
        public short Grade => _syain.Kyusyoku;

        /// <summary>勤怠属性</summary>
        [Display(Name = "勤怠属性")]
        public string KintaiZokuseiName => _syain.KintaiZokusei.Name;

        /// <summary>ロール</summary>
        [Display(Name = "ロール")]
        public string UserRoleName => _syain.UserRole.Name;

        /// <summary>退職</summary>
        [Display(Name = "退職")]
        public string RetiredDisplay => _syain.Retired == true ? "退職" : string.Empty;
    }

    /// <summary>
    /// 検索条件モデル
    /// </summary>
    public class SyainSearchCondition
    {
        /// <summary>社員番号称</summary>
        [Display(Name = "社員番号")]
        public string? SyainNo { get; set; }

        /// <summary>社員名称</summary>
        [Display(Name = "社員名")]
        public string? SyainName { get; set; }

        /// <summary>部署名称</summary>
        [Display(Name = "部署")]
        public string? BusyoName { get; set; }

        /// <summary>級職</summary>
        [Display(Name = "級職")]
        public short? Grade { get; set; }

        /// <summary>退職者を含む</summary>
        [Display(Name = "退職者を含む")]
        public bool IncludeRetired { get; set; }

        // / <summary>勤怠属性</summary>
        [Display(Name = "勤怠属性")]
        public long? KintaiZokuseiId { get; set; }
        /// <summary>ロール</summary>
        [Display(Name = "ロール")]
        public long? UserRoleId { get; set; }
        /// <summary>社員権限</summary>
        [Display(Name = "社員権限")]
        public EmployeeAuthority? Kengen { get; set; }

        /// <summary>
        /// 勤怠属性の選択肢
        /// </summary>
        public SelectList KintaiZokuseiOptions { get; set; } = default!;

        /// <summary>
        /// ロールの選択肢
        /// </summary>
        public SelectList UserRoleOptions { get; set; } = default!;

        /// <summary>
        /// 社員権限の選択肢
        /// </summary>
        public SelectList KengenOptions { get; set; } = default!;
    }
}

