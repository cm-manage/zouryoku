using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Model.Data;
using System.Security.Claims;
using Zouryoku.Data;

namespace Zouryoku.Utils
{
    /// <summary>
    /// Entra ID認証ヘルパークラス
    /// </summary>
    public static class EntraAuthHelper
    {
        /// <summary>
        /// Entra IDのClaimsPrincipalからLoginInfoを作成
        /// </summary>
        /// <param name="principal">ClaimsPrincipal</param>
        /// <param name="db">データベースコンテキスト</param>
        /// <param name="graphServiceClient">Microsoft Graph Client（オプション）</param>
        /// <returns>LoginInfo</returns>
        public static async Task<LoginInfo?> CreateLoginInfoFromEntraAsync(
            ClaimsPrincipal principal, 
            ZouContext db,
            GraphServiceClient? graphServiceClient = null)
        {
            if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
            {
                return null;
            }

            // Entra IDからのClaim情報を取得
            var oid = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                ?? principal.FindFirst("oid")?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value
                ?? principal.FindFirst("preferred_username")?.Value;
            var displayName = principal.FindFirst(ClaimTypes.Name)?.Value
                ?? principal.FindFirst("name")?.Value;

            if (string.IsNullOrEmpty(oid) || string.IsNullOrEmpty(email))
            {
                return null;
            }

            // Graph APIからユーザー情報を取得（オプション）
            if (graphServiceClient != null)
            {
                try
                {
                    var user = await graphServiceClient.Me.GetAsync();
                    if (user != null)
                    {
                        displayName = user.DisplayName ?? displayName;
                        email = user.Mail ?? user.UserPrincipalName ?? email;
                    }
                }
                catch
                {
                    // Graph API呼び出しに失敗してもClaimの情報を使用
                }
            }

            // メールアドレスをもとにSyainを検索
            var syain = await db.Syains
                .Include(s => s.SyainBase)
                .Include(s => s.Busyo)
                .FirstOrDefaultAsync(s => s.EMail == email);

            if (syain == null)
            {
                // 社員が見つからない場合、デフォルトユーザーを作成するか、nullを返す
                return null;
            }

            return new LoginInfo
            {
                User = syain,
                EntraUserId = oid,
                EntraDisplayName = displayName,
                EntraEmail = email,
                AuthenticationMethod = "Entra",
                LastRefreshedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Entra ID認証が有効かチェック
        /// </summary>
        /// <param name="principal">ClaimsPrincipal</param>
        /// <returns>true: Entra ID認証, false: その他</returns>
        public static bool IsEntraAuthentication(ClaimsPrincipal principal)
        {
            if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
            {
                return false;
            }

            // Entra IDのClaimが存在するか確認
            var oid = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                ?? principal.FindFirst("oid")?.Value;

            return !string.IsNullOrEmpty(oid);
        }
    }
}
