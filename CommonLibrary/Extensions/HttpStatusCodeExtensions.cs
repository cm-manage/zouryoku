using System.Net;

namespace CommonLibrary.Extensions
{

    public static class HttpStatusCodeExtensions
    {
        /// <summary>
        /// 数値に変換する拡張メソッド
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <returns></returns>
        public static int ToInt(this HttpStatusCode httpStatusCode)
            => (int)httpStatusCode;

    }
}
