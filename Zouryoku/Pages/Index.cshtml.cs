using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Data;
using Zouryoku.Extensions;
using ZouryokuCommonLibrary;

namespace Zouryoku.Pages
{
    /// <summary>
    /// トップページ（ルートページ）
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// ログイン情報
        /// </summary>
        public LoginInfo? LoginInfo { get; set; }

        public IActionResult OnGet()
        {
            // セッションからログイン情報を取得
            var loginInfoOption = HttpContext.Session.Get<LoginInfo>();

            if (loginInfoOption.IsNone)
            {
                // 未ログイン時はログインページにリダイレクト
                logger.LogInformation("未ログインのため、ログインページにリダイレクトします");
                return RedirectToPage("/Logins/Index");
            }

            // ログイン済みの場合は情報を表示
            loginInfoOption.IfSome(info =>
            {
                LoginInfo = info;
                logger.LogInformation("ログイン済みユーザー: Email={Email}, UserId={UserId}",
                    info.EntraEmail ?? "不明",
                    info.EntraUserId ?? "不明");
            });

            return Page();
        }
    }
}

