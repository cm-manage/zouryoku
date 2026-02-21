using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using ZouryokuCommonLibrary;

namespace Zouryoku.Pages
{
    /// <summary>
    /// ログアウトページ
    /// </summary>
    public class SignOutModel : NotSessionBasePageModel<SignOutModel>
    {
        public SignOutModel(
            ZouContext db,
            ILogger<SignOutModel> logger,
            IOptions<AppConfig> optionsAccessor)
            : base(db, logger, optionsAccessor)
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // セッションからログイン情報を取得
                var loginInfoOption = HttpContext.Session.Get<LoginInfo>();

                // セッションをクリア
                HttpContext.Session.Clear();

                // ログ出力
                loginInfoOption.IfSome(loginInfo =>
                {
                    logger.LogInformation("Entra IDログアウト: UserId={UserId}", loginInfo.EntraUserId ?? "不明");
                });

                // Entra IDからサインアウト
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ログアウト処理でエラーが発生しました");
                return RedirectToPage("/Index");
            }
        }
    }
}
