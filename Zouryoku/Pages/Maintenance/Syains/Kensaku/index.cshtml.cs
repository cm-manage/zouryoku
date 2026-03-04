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

namespace Zouryoku.Pages.Maintenance.Syains.Kensaku
{
    /// <summary>
    /// 社員検索ページモデル
    /// </summary>
    [FunctionAuthorization]
    public class IndexModel : BasePageModel<IndexModel>
    {
        public IndexModel(
            ZouContext db,
            ILogger<IndexModel> logger,
            IOptions<AppConfig> optionsAccessor,
            ICompositeViewEngine viewEngine,
            TimeProvider? timeProvider = null)
            : base(db, logger, optionsAccessor, viewEngine, timeProvider) { }

        public override bool UseInputAssets => true;

        /// <summary>
        /// 検索条件
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public SyainSearchCondition SearchCondition { get; set; } = new SyainSearchCondition();

        /// <summary>
        /// 検索結果社員リスト
        /// </summary>
        public List<SyainViewModel> Results { get; set; } = [];

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

        /// <summary>
        /// 画面初期表示
        /// </summary>
        /// <returns>社員マスタメンテナンスページ</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            SearchCondition = await InitializeSearchConditionOptionsAsync();
            Results = await GetSyainListAsync(SearchCondition);
            return Page();
        }

        /// <summary>
        /// 検索ボタン押下
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetSearchAsync()
        {
            Results = await GetSyainListAsync(SearchCondition);

            var data = await PartialToJsonAsync("_SyainSearchResults", this);
            return SuccessJson(data: data);
        }

        private async Task<SyainSearchCondition> InitializeSearchConditionOptionsAsync()
        {
            var searchCondition = new SyainSearchCondition();

            // 勤怠属性一覧
            var kintaiList = Enum.GetValues<EmployeeWorkType>()
                .Select(e => new SelectOption
                {
                    Code = e,
                    Name = e.ToString()
                })
                .ToList();
            searchCondition.KintaiZokuseiId = kintaiList.FirstOrDefault(k => k.Code == みなし対象者)?.Code;

            KintaiZokuseiOptions = new SelectList(kintaiList, "Code", "Name", SearchCondition.KintaiZokuseiId);

            // ロール一覧
            var roles = await db.UserRoles
                .AsNoTracking()
                .Select(r => new { r.Code, r.Name })
                .ToListAsync();
            UserRoleOptions = new SelectList(roles, "Code", "Name", SearchCondition.UserRoleId);

            // 社員権限一覧
            var kengenList = Enum.GetValues<EmployeeAuthority>()
                .Where(e => e != EmployeeAuthority.None)
                .Select(e => new { Code = e, Name = e.ToString() })
                .ToList();
            KengenOptions = new SelectList(kengenList, "Code", "Name", SearchCondition.Kengen);

            return searchCondition;
        }

        /// <summary>
        /// 社員一覧取得
        /// </summary>
        private async Task<List<SyainViewModel>> GetSyainListAsync(SyainSearchCondition searchCondition)
        {
            var query = db.Syains
                .Include(s => s.Busyo)
                .Include(s => s.KintaiZokusei)
                .Include(s => s.UserRole)
                .AsNoTracking()
                .AsQueryable();

            // 社員番号
            if (!string.IsNullOrEmpty(SearchCondition.SyainNo))
            {
                query = query.Where(s => s.Code.Contains(SearchCondition.SyainNo));
            }

            // 社員名
            if (!string.IsNullOrEmpty(SearchCondition.SyainName))
            {
                query = query.Where(s => s.Name.Contains(SearchCondition.SyainName));
            }

            // 部署
            if (!string.IsNullOrEmpty(SearchCondition.BusyoName))
            {
                query = query.Where(s => s.Busyo.Name.Contains(SearchCondition.BusyoName));
            }

            // 退職者を含む
            if (!SearchCondition.IncludeRetired)
            {
                query = query.Where(s => !s.Retired);
            }

            // 勤怠属性
            if (SearchCondition.KintaiZokuseiId.HasValue)
            {
                query = query.Where(s => (EmployeeWorkType)s.KintaiZokuseiId == searchCondition.KintaiZokuseiId.Value);
            }

            // ロール
            if (SearchCondition.UserRoleId.HasValue)
            {
                query = query.Where(s => s.UserRoleId == SearchCondition.UserRoleId.Value);
            }

            // 社員権限
            if (SearchCondition.Kengen.HasValue)
            {
                query = query.Where(s => s.Kengen == searchCondition.Kengen.Value);
            }

            // 社員級職
            if (SearchCondition.Kyusyoku.HasValue)
            {
                query = query.Where(s => s.Kyusyoku == SearchCondition.Kyusyoku.Value);
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
        /// <param name="syain">社員エンティティ</param>
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
        public short Kyusyoku => _syain.Kyusyoku;

        /// <summary>勤怠属性</summary>
        [Display(Name = "勤怠属性")]
        public string KintaiZokuseiName => _syain.KintaiZokusei.Name;

        /// <summary>ロール</summary>
        [Display(Name = "ロール")]
        public string UserRoleName => _syain.UserRole.Name;

        /// <summary>退職</summary>
        [Display(Name = "退職")]
        public string RetiredDisplay => _syain.Retired ? "退職" : string.Empty;
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

        public long? BusyoId { get; set; }

        /// <summary>級職</summary>
        [Display(Name = "級職")]
        public short? Kyusyoku { get; set; }

        /// <summary>退職者を含む</summary>
        [Display(Name = "退職者を含む")]
        public bool IncludeRetired { get; set; }

        // / <summary>勤怠属性</summary>
        [Display(Name = "勤怠属性")]
        public EmployeeWorkType? KintaiZokuseiId { get; set; }

        /// <summary>ロール</summary>
        [Display(Name = "ロール")]
        public long? UserRoleId { get; set; }

        /// <summary>社員権限</summary>
        [Display(Name = "社員権限")]
        public EmployeeAuthority? Kengen { get; set; }
    }

    /// <summary>
    /// 共通セレクトボックス用オプション
    /// </summary>
    public class SelectOption
    {
        public EmployeeWorkType Code { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
