using LanguageExt;
using Newtonsoft.Json;
using static CommonLibrary.Extensions.OptionExtensions;
using static LanguageExt.Prelude;

namespace Zouryoku.Extensions
{
    public static class CookieExtensions
    {
        /// <summary>
        /// デフォルトの Cookie 有効期限（日数）
        /// </summary>
        private const int DefaultCookieExpireDays = 30;

        /// <summary>
        /// JsonSerialize可能なオブジェクトをCookieに設定します。
        /// </summary>
        /// <typeparam name="T">保存するオブジェクトの型</typeparam>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="value">保存する値</param>
        /// <param name="cookieName">Cookie名（省略時は型名）</param>
        /// <param name="expireDays">有効期限（日数、省略時は30日）</param>
        /// <returns>保存した値</returns>
        public static T SetCookie<T>(this HttpContext httpContext, T value, string? cookieName = null, int? expireDays = null)
        {
            var name = OptionalT(cookieName).IfNone(() => typeof(T).FullName ?? string.Empty);
            var jsonValue = JsonConvert.SerializeObject(
                value,
                Formatting.None,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
            );

            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(expireDays ?? DefaultCookieExpireDays),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            };

            httpContext.Response.Cookies.Append(name, jsonValue, options);
            return value;
        }

        /// <summary>
        /// Cookieから値を取得します（型名をキーとして使用）
        /// </summary>
        /// <typeparam name="T">取得する型</typeparam>
        /// <param name="httpContext">HttpContext</param>
        /// <returns>取得した値（存在しない場合はNone）</returns>
        public static Option<T> GetCookie<T>(this HttpContext httpContext)
        {
            var name = typeof(T).FullName ?? string.Empty;
            return httpContext.GetCookie<T>(name);
        }

        /// <summary>
        /// Cookieから値を取得します
        /// </summary>
        /// <typeparam name="T">取得する型</typeparam>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="cookieName">Cookie名</param>
        /// <returns>取得した値（存在しない場合はNone）</returns>
        public static Option<T> GetCookie<T>(this HttpContext httpContext, string? cookieName = null)
        {
            var name = OptionalT(cookieName).IfNone(() => typeof(T).FullName ?? string.Empty);

            if (httpContext.Request.Cookies.TryGetValue(name, out var cookieValue))
            {
                try
                {
                    return Optional(JsonConvert.DeserializeObject<T>(cookieValue));
                }
                catch
                {
                    return None;
                }
            }
            else
            {
                return None;
            }
        }

        /// <summary>
        /// Cookieから値を取得します。存在しない場合はデフォルトインスタンスを生成して返します
        /// </summary>
        /// <typeparam name="T">取得する型（パラメータなしコンストラクタが必要）</typeparam>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="cookieName">Cookie名</param>
        /// <returns>取得した値、または新しいインスタンス</returns>
        public static T GetCookieOrDefault<T>(this HttpContext httpContext, string? cookieName = null) where T : new()
        {
            return httpContext.GetCookie<T>(cookieName)
                .IfNone(() => new T());
        }

        /// <summary>
        /// Cookieから値を取得します。存在しない場合は指定されたデフォルト値を返します
        /// </summary>
        /// <typeparam name="T">取得する型</typeparam>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="cookieName">Cookie名</param>
        /// <param name="defaultValue">デフォルト値</param>
        /// <returns>取得した値、またはデフォルト値</returns>
        public static T GetCookieOrDefault<T>(this HttpContext httpContext, string? cookieName, T defaultValue)
        {
            return httpContext.GetCookie<T>(cookieName)
                .IfNone(defaultValue);
        }

        /// <summary>
        /// Cookieをクリアします（型名をキーとして使用）
        /// </summary>
        /// <typeparam name="T">クリアする型</typeparam>
        /// <param name="httpContext">HttpContext</param>
        public static void ClearCookie<T>(this HttpContext httpContext, string? cookieName = null)
        {
            var name = OptionalT(cookieName).IfNone(() => typeof(T).FullName ?? string.Empty);
            httpContext.Response.Cookies.Delete(name);
        }

        /// <summary>
        /// Cookieをクリアします
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="cookieName">Cookie名</param>
        public static void ClearCookie(this HttpContext httpContext, string cookieName)
        {
            httpContext.Response.Cookies.Delete(cookieName);
        }

        /// <summary>
        /// Cookieから値を取得してクリアします
        /// </summary>
        /// <typeparam name="T">取得する型</typeparam>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="cookieName">Cookie名</param>
        /// <returns>取得した値（存在しない場合はNone）</returns>
        public static Option<T> GetAndClearCookie<T>(this HttpContext httpContext, string? cookieName = null)
        {
            var result = httpContext.GetCookie<T>(cookieName);
            httpContext.ClearCookie<T>(cookieName);
            return result;
        }
    }
}
