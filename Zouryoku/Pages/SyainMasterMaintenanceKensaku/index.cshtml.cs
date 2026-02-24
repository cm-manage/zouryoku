using LanguageExt.UnsafeValueAccess;
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
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
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
        /// 保存するセッション名
        /// </summary>
        private const string SaveSessionName = "selectedSyainBaseId";

        public class ValidateSelectionRequest
        {
            public long SyainBaseId { get; set; }
        }

        /// <summary>
        /// 画面初期表示
        /// </summary>
        /// <returns>社員マスタメンテナンスページ</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            //　勤怠属性のTrueステータス
            Condition.IsKintaiZoukuseiStatus = true;
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
            //　勤怠属性のFalseステータス
            Condition.IsKintaiZoukuseiStatus = false;
            Results = await GetSyainListAsync();

            var data = await PartialToJsonAsync("_SyainSearchResults", this);
            return SuccessJson(data: data);
        }

        private async Task InitializeSearchConditionOptionsAsync()
        {
            // 勤怠属性一覧
            var kintaiList = await InitializeKintaiZokuseis();
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
            if (Condition.KintaiZokuseiId.HasValue && !Condition.IsKintaiZoukuseiStatus)
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

        /// <summary>
        /// 勤怠属性登録
        /// </summary>
        /// <returns></returns>
        public async Task<List<KintaiZokusei>> InitializeKintaiZokuseis()
        {
            var kintaiZokuseiList = new List<KintaiZokusei>()
            {
                new KintaiZokusei { Id = (long)EmployeeWorkType.みなし対象者, Name = EmployeeWorkType.みなし対象者.ToString() },
                new KintaiZokusei { Id = (long)EmployeeWorkType._3か月60時間, Name = EmployeeWorkType._3か月60時間.ToString() },
                new KintaiZokusei { Id = (long)EmployeeWorkType.フリー, Name = EmployeeWorkType.フリー.ToString() },
                new KintaiZokusei { Id = (long)EmployeeWorkType.管理, Name = EmployeeWorkType.管理.ToString() },
                new KintaiZokusei { Id = (long)EmployeeWorkType.標準社員外, Name = EmployeeWorkType.標準社員外.ToString() },
                new KintaiZokusei { Id = (long)EmployeeWorkType.パート, Name = EmployeeWorkType.パート.ToString() },
                new KintaiZokusei { Id = (long)EmployeeWorkType.月45時間, Name = EmployeeWorkType.月45時間.ToString()}
            };
            return kintaiZokuseiList;
        }

        /// <summary>
        /// バリデーションチェック、セッション保存
        /// </summary>
        /// <param name="data">検証リクエスト</param>
        /// <returns>選択検証結果JSONデータ</returns>
        public async Task<IActionResult> OnPostValidateSelectionAsync(ValidateSelectionRequest data)
        {
            // 未使用パラメータ警告抑制
            _ = data;

            // セッションに部署IDを保存
            HttpContext.Session.Set(data.SyainBaseId, SaveSessionName);

            return SuccessJson();
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

        /// <summary>
        /// 勤怠属性のステータス
        /// </summary>
        public bool IsKintaiZoukuseiStatus { get; set;  }
    }

    /// <summary>
    /// 勤怠属性リスト
    /// </summary>
    public class KintaiZoukusei
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

