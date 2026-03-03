using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Extensions;
using Model.Model;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using static Model.Enums.EmployeeAuthority;
using static Zouryoku.Utils.Const;

namespace Zouryoku.Pages.RoleDefaultKengen
{
    /// <summary>
    /// ロールデフォルト権限設定画面です。
    /// </summary>
    [FunctionAuthorization]
    public partial class IndexModel : BasePageModel<IndexModel>
    {
        private const long DefaultSelectedRoleId = 0L;

        public override bool UseInputAssets { get; } = true;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public IndexModel(
            ZouContext db,
            ILogger<IndexModel> logger,
            IOptions<AppConfig> optionsAccessor,
            ICompositeViewEngine viewEngine,
            TimeProvider? timeProvider = null)
            : base(db, logger, optionsAccessor, viewEngine, timeProvider)
        {
        }

        /// <summary>
        /// 画面表示用 ViewModel
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public IndexViewModel ViewModel { get; set; } = new IndexViewModel();

        /// <summary>
        /// 初期表示
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            await InitializeViewModelAsync();
            return Page();
        }

        /// <summary>
        /// ロールデータ取得（AJAX）
        /// </summary>
        public async Task<IActionResult> OnPostRoleDataAsync()
        {
            await InitializeViewModelAsync();

            var html = await PartialToJsonAsync("_UserRoleList", ViewModel);
            return SuccessJson(data: html);
        }

        /// <summary>
        /// ロール権限更新
        /// </summary>
        public async Task<IActionResult> OnPostUpdateRoleAsync()
        {
            // 単項目チェック
            JsonResult? errorJson = ModelState.ErrorJson();
            if (errorJson is not null) return errorJson;

            var role = await db.UserRoles.FindAsync(ViewModel.SelectedRoleId);
            if (role is null) return ErrorJson(ErrorSelectedDataNotExists);

            role.Kengen = (EmployeeAuthority)ViewModel.KengenValue;

            // 更新処理
            await UpdateRoleAsync(role, ViewModel.Version);

            // 同時実行制御が働いたとき
            errorJson = ModelState.ErrorJson();
            if (errorJson is not null) return errorJson;

            return SuccessJson(data: role);
        }

        private async Task UpdateRoleAsync(UserRole role, uint version)
        {
            // 同時実行制御用にバージョンを設定（重要）
            db.SetOriginalValue(role, entity => entity.Version, version);

            await SaveWithConcurrencyCheckAsync(string.Format(ErrorConflictReload, "ロール"));
        }

        /// <summary>
        /// ViewModelの表示情報を初期化します。
        /// </summary>
        private async Task InitializeViewModelAsync()
        {
            var roles = await db.UserRoles
                .OrderBy(role => role.Jyunjo)
                .AsNoTracking()
                .ToListAsync();

            ViewModel.UserRoles = roles
                .Select(role => new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name
                })
                .ToList();

            var selectedRole = roles
                .FirstOrDefault(role => role.Id == ViewModel.SelectedRoleId)
                ?? roles.FirstOrDefault();

            ViewModel.SelectedRoleId = selectedRole?.Id ?? DefaultSelectedRoleId;
            ViewModel.Kengen = selectedRole?.Kengen ?? None;
            ViewModel.Version = selectedRole?.Version ?? 0;
        }
    }
}
