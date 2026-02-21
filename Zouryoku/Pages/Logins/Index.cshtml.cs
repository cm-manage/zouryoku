using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Pages.Shared;
using ZouryokuCommonLibrary;

namespace Zouryoku.Pages.Logins
{
    /// <summary>
    /// ログインページモデル
    /// </summary>
    public class IndexModel : NotSessionBasePageModel<IndexModel>
    {
        public IndexModel(
            ZouContext db,
            ILogger<IndexModel> logger,
            IOptions<AppConfig> optionsAccessor)
            : base(db, logger, optionsAccessor)
        {
        }

        /// <summary>
        /// ページ表示
        /// </summary>
        public void OnGet()
        {
            // ログインページを表示
        }
    }
}
