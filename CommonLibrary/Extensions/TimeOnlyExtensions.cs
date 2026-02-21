using System;

namespace CommonLibrary.Extensions
{
    public static class TimeOnlyExtensions
    {
        /// <summary>
        /// TimeOnly?を"HH:mm"形式の文字列に変換します。nullの場合は空文字を返します。
        /// </summary>
        /// <param name="timeOnly">変換するTimeOnly?型の値</param>
        /// <returns>"HH:mm"形式の文字列、またはnullの場合は空文字</returns>
        public static string ToStrByHHmmOrEmpty(this TimeOnly? timeOnly)
            => timeOnly?.ToString("HH:mm") ?? string.Empty;
    }
}
