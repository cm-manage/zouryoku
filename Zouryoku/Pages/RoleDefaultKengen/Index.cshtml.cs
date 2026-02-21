using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.RoleDefaultKengen
{
    [FunctionAuthorization]
    public partial class IndexModel : BasePageModel<IndexModel>
    {
        // ======================================
        // 定数
        // ======================================

        // ======================================
        // DI
        // ======================================

        public IndexModel(ZouContext db, ILogger<IndexModel> logger,
            IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine)
            : base(db, logger, optionsAccessor, viewEngine) { }

        // ======================================
        // フィールド
        // ======================================

        public List<RoleViewModel> Roles { get; set; } = [];

        // ======================================
        // イベント
        // ======================================

        // GET
        // --------------------------------------

        public async Task OnGetAsync()
        {
            // 表示を確認するためにとりあえず取得して返しているだけ
            Roles = await GetUserRolesAsync();
        }

        /// <summary>
        /// ロールテーブルのデータをビューモデルの形で全件取得する
        /// </summary>
        /// <returns>ユーザーロールのビューモデルのリスト</returns>
        private async Task<List<RoleViewModel>> GetUserRolesAsync()
        {
            return await db.UserRoles
                .AsNoTracking()
                .Select(userRole => new RoleViewModel(userRole))
                .ToListAsync();
        }
    }
}
