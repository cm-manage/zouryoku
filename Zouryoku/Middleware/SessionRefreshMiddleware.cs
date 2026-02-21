using CommonLibrary.Extensions;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Zouryoku.Data;
using Zouryoku.Extensions;

namespace Zouryoku.Middleware
{
    /// <summary>
    /// セッション情報を定期的にDBから再取得して更新するミドルウェア
    /// </summary>
    /// <remarks>
    /// ログイン後、一定時間経過したセッション情報を自動的に最新化します。
    /// これにより、ユーザー情報の変更（部署異動、権限変更等）が
    /// セッションに反映され、陳腐化した情報による不整合を防ぎます。
    /// </remarks>
    public class SessionRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionRefreshMiddleware> _logger;
        
        /// <summary>セッション情報の再取得間隔（デフォルト: 1分）</summary>
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(1);

        public SessionRefreshMiddleware(RequestDelegate next, ILogger<SessionRefreshMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ZouContext db)
        {
            var loginInfoOpt = context.Session.Get<LoginInfo>();
            if (loginInfoOpt.IsSome)
            {
                var loginInfo = loginInfoOpt.GetOrThrowException("存在確認済み");
                
                // 一定時間経過していたらDBから最新情報を再取得
                if (DateTime.Now - loginInfo.LastRefreshedAt > RefreshInterval)
                {
                    try
                    {
                        var today = DateTime.Today.ToDateOnly();
                        // DBから最新のユーザー情報を取得
                        var latestUser = await db.Syains
                            .Include(s => s.SyainBase)
                            .Include(s => s.Busyo)
                            .Where(s => s.StartYmd <= today && today <= s.EndYmd)
                            .FirstOrDefaultAsync(s => s.SyainBaseId == loginInfo.User.SyainBaseId);

                        if (latestUser != null)
                        {
                            // セッション情報を更新
                            loginInfo.User = latestUser;
                            loginInfo.LastRefreshedAt = DateTime.Now;
                            context.Session.Set(loginInfo);

                            _logger.LogDebug("セッション情報を更新しました: UserId={UserId}, Name={Name}",
                                latestUser.SyainBaseId, latestUser.Name);
                        }
                        else
                        {
                            // ユーザーが削除されている場合はログアウト
                            _logger.LogWarning("ユーザーが見つかりません。セッションをクリアします: UserId={UserId}",
                                loginInfo.User.SyainBaseId);

                            context.Session.Clear();

                            // Ajaxリクエストの場合は401を返す
                            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return;
                            }

                            // 通常リクエストの場合はログインページにリダイレクト
                            context.Response.Redirect("/Logins/Index?reason=user_deleted");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "セッション情報の更新に失敗しました。既存のセッション情報を維持します: UserId={UserId}",
                            loginInfo.User.SyainBaseId);
                        // エラー時は既存のセッション情報を維持して処理を継続
                    }
                }
            }

            await _next(context);
        }
    }
}
