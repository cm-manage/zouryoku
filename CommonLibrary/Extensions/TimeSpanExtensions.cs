using System;

namespace CommonLibrary.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string HHmmColon(this TimeSpan ts)
            => ts.ToString(@"hh\:mm");

        public static string HHmm(this TimeSpan ts)
            => ts.ToString(@"hhmm");

        /// <summary>
        /// 00:00 or 00:00:00 形式の文字列を TimeSpan型に変換します。
        /// </summary>
        /// <param name="tsStr"></param>
        /// <returns></returns>
        public static TimeSpan ToTimeSpan(this string tsStr)
        {
            if (tsStr.Length == 5)
            {
                return new TimeSpan(int.Parse(tsStr.Substring(0, 2)), int.Parse(tsStr.Substring(3)), 0);
            }
            else
            {
                return new TimeSpan(int.Parse(tsStr.Substring(0, 2)), int.Parse(tsStr.Substring(3,2)), int.Parse(tsStr.Substring(6, 2)));
            }
        }

        public static TimeSpan Min(this TimeSpan x, TimeSpan y)
            => x <= y ? x : y;

        public static TimeSpan Max(this TimeSpan x, TimeSpan y)
            => x <= y ? y : x;
    }
}
