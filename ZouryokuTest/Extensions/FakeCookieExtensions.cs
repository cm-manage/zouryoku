using LanguageExt;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using static CommonLibrary.Extensions.OptionExtensions;
using static LanguageExt.Prelude;

namespace ZouryokuTest.Extensions
{
    /// <summary>
    /// UT用 Cookie 拡張メソッド
    /// </summary>
    public static class FakeCookieExtensions
    {
        /// <summary>
        /// JsonSerialize可能なオブジェクトをCookieに設定します。
        /// </summary>
        /// <typeparam name="T">保存するオブジェクトの型</typeparam>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="value">保存する値</param>
        /// <param name="cookieName">Cookie名（省略時は型名）</param>
        /// <returns>保存した値</returns>
        public static T AddTestCookie<T>(this HttpContext httpContext, T value, string? cookieName = null)
        {
            // Cookie名の決定
            var name = cookieName ?? typeof(T).FullName ?? string.Empty;

            // オブジェクトをJSONにシリアライズ
            var jsonValue = JsonConvert.SerializeObject(
                value,
                Formatting.None,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
            );

            // 既存の Cookie を Header から取得
            var rawCookies = httpContext.Request.Headers["Cookie"].ToString() ?? "";
            // 解析
            var cookies = ParseCookies(rawCookies);

            // Cookie を設定
            cookies[name] = Uri.EscapeDataString(jsonValue);

            // 再セット
            WriteCookies(httpContext, cookies);

            return value;
        }

        /// <summary>
        /// 指定したキーのCookieをクリアします（型名をキーとして使用）
        /// </summary>
        /// <typeparam name="T">クリアする型</typeparam>
        /// <param name="httpContext">HttpContext</param>
        public static void RemoveTestCookie<T>(this HttpContext httpContext, string? cookieName = null)
        {
            // Cookie名の決定
            var name = cookieName ?? typeof(T).FullName ?? string.Empty;

            // 既存の Cookie を Header から取得
            var rawCookies = httpContext.Request.Headers["Cookie"].ToString() ?? "";
            // 解析
            var cookies = ParseCookies(rawCookies);

            // Cookie を削除
            cookies.Remove(name);

            // 再セット
            WriteCookies(httpContext, cookies);
        }

        // Cookie ヘッダーを解析して辞書に変換します。
        private static Dictionary<string, string> ParseCookies(string rawCookies)
        {
            return rawCookies
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Select(c =>
                {
                    var parts = c.Split('=', 2);
                    return new KeyValuePair<string, string>(
                        parts[0],
                        parts.Length > 1 ? parts[1] : ""
                    );
                })
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        }

        // Cookie ヘッダーを書き換えます。
        private static void WriteCookies(HttpContext httpContext, Dictionary<string, string> cookies)
        {
            httpContext.Request.Headers["Cookie"] =
                string.Join("; ", cookies.Select(kv => $"{kv.Key}={kv.Value}"));
        }

    }
}
