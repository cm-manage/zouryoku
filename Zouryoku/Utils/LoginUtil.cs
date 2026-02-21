using CommonLibrary.Extensions;
using Zouryoku.Extensions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Enums;
using static Model.Extensions.ModelExtensions;
using Zouryoku.Data;

namespace Zouryoku.Utils
{
    public static class LoginUtil
    {
        /// <summary>
        /// user-agent からブラウザ・端末情報を取得
        /// </summary>
        /// <param name="httpRequest"></param>
        public static (string Browser, string Device) GetBrowserAndDevice(HttpRequest httpRequest)
        {
            // user-agent取得
            var userAgent = httpRequest.Headers.Find(x => x.Key == "User-Agent").Match(x => x.Value.ToString(), () => "");

            // ブラウザ特定
            var browser = userAgent.IndexOf("Trident") != -1
                ? "Internet Explorer"
                    : userAgent.IndexOf("Edg") != -1
                        ? "Edg"
                        : userAgent.IndexOf("Chrome") != -1
                            ? "Chrome"
                            : userAgent.IndexOf("Safari") != -1
                                ? "Safari"
                                : userAgent.IndexOf("Firefox") != -1
                                    ? "Firefox"
                                    : userAgent;

            // 端末特定
            var device = userAgent.IndexOf("Windows") != -1
                ? "Windows "
                : userAgent.IndexOf("Mac OS") != -1
                    ? "Mac OS"
                    : userAgent.IndexOf("iPhone") != -1
                        ? "iPhone"
                        : userAgent.IndexOf("ipad") != -1
                            ? "ipad"
                            : "Android";

            return (browser, device);
        }

        /// <summary>
        /// user-agent 情報をもとにアクセスログ作成
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="db"></param>
        /// <param name="user"></param>
        public static async Task CreateAccessLogAsync(HttpRequest httpRequest, ZouContext db, LoginInfo user)
        {
            // ブラウザ・端末特定
            var (browser, device) = GetBrowserAndDevice(httpRequest);

            //アクセスログ保存
            await CreateAccessLogAsync(browser, device, db, user);
        }

        /// <summary>
        /// user-agent 情報をもとにアクセスログ作成
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="device"></param>
        /// <param name="db"></param>
        /// <param name="user"></param>
        public static async Task CreateAccessLogAsync(string browser, string device, ZouContext db, LoginInfo user)
        {
            //アクセスログ保存
            //await db.AccessLogs.AddAsync(new AccessLog
            //{
            //    UserId = user.Id,
            //    LoginBrowser = browser,
            //    LoginOs = device,
            //    LoginDatetime = DateTime.Now,
            //});
            await db.SaveChangesAsync();
        }

    }
}
