namespace Zouryoku.Utils
{
    public static class UrlUtil
    {
        /// <summary>
        /// URLにパラメータを？、＆を判断して追加する
        /// </summary>
        public static string GetURL(string url, string param)
        {
            var urlParam = (url.IndexOf("?") >= 0)
                ? param.StartsWith("&")
                    ? param
                    : $"&{param}"
                : $"?{param}";
            return url + urlParam;
        }
    }
}
