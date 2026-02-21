using Zouryoku.Extensions;
using Model.Data;
using Zouryoku.Data;

namespace Zouryoku.Middleware
{
    public class SessionTimeoutMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // セッション切れ
            if (context.Session.Get<LoginInfo>().IsNone)
            {
                // ajaxリクエスト時は401エラーを返す
                if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }

            // 通常リクエスト(ページ遷移)は、原則としてFunctionAuthAttribute.csでセッションのチェックを行ってから
            // ページ遷移するため、ここではチェックしない
            await next(context);
        }
    }
}
