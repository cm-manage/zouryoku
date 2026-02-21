using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Model.Data;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;

namespace Zouryoku.Pages
{
    /// <summary>
    /// Entra ID認証コールバックページ
    /// </summary>
    public class EntraCallbackModel : NotSessionBasePageModel<EntraCallbackModel>
    {
        private readonly GraphServiceClient graphServiceClient;

        public EntraCallbackModel(
            ZouContext db,
            ILogger<EntraCallbackModel> logger,
            IOptions<AppConfig> optionsAccessor,
            GraphServiceClient graphServiceClient)
            : base(db, logger, optionsAccessor)
        {
            this.graphServiceClient = graphServiceClient;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Entra ID認証が完了しているか確認
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    logger.LogWarning("Entra認証が完了していません");
                    return RedirectToPage("/Index");
                }

                // Entra IDからLoginInfoを作成
                var loginInfo = await EntraAuthHelper.CreateLoginInfoFromEntraAsync(
                    User,
                    db,
                    graphServiceClient);

                if (loginInfo == null)
                {
                    logger.LogWarning("Entra認証は成功しましたが、ユーザー情報の取得に失敗しました。Email: {Email}",
                        User.FindFirst("preferred_username")?.Value ?? "不明");

                    // 認証をクリア
                    await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

                    TempData["ErrorMessage"] = "ユーザー情報の取得に失敗しました。システム管理者にお問い合わせください。";
                    return RedirectToPage("/Index");
                }

                // セッションにLoginInfoを保存
                HttpContext.Session.Set(loginInfo);

                // アクセスログを作成
                await LoginUtil.CreateAccessLogAsync(Request, db, loginInfo);

                logger.LogInformation("Entra認証成功: UserId={UserId}, Email={Email}",
                    loginInfo.EntraUserId,
                    loginInfo.EntraEmail);

                // トップページにリダイレクト
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Entra認証コールバック処理でエラーが発生しました");

                // 認証をクリア
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

                TempData["ErrorMessage"] = "認証処理中にエラーが発生しました。もう一度お試しください。";
                return RedirectToPage("/Logins/Index");
            }
        }
    }
}
