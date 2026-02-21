using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static LanguageExt.Prelude;

namespace CommonLibrary.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToStrByYYYYMMDDSlash(this DateTime dt)
            => dt.ToString("yyyy/MM/dd");
        public static string ToStrByYYYYMMDDSlash(this DateTime? dt, string str = "")
            => dt.HasValue ? dt.Value.ToString("yyyy/MM/dd") : str;
        public static string ToStrByYYYYMMSlash(this DateTime dt)
            => dt.ToString("yyyy/MM");

        public static string ToStrByYYYYMMDDSlashHH(this DateTime dt)
            => dt.ToString("yyyy/MM/dd HH");

        public static string ToStrByYYYYMMDDSlashHHmm(this DateTime dt)
            => dt.ToString("yyyy/MM/dd HH:mm");

        public static string ToStrByYYYYMMDDSlashHHmmss(this DateTime dt)
            => dt.ToString("yyyy/MM/dd HH:mm:ss");

        public static string ToHHmm(this DateTime dt)
            => dt.ToString("HH:mm");

        /// <summary>
        /// DateTime?を"HH"形式の文字列に変換します。nullの場合は空文字を返します。
        /// </summary>
        /// <param name="dateTime">変換するDateTime?型の値</param>
        /// <returns>"HH"形式の文字列、またはnullの場合は空文字</returns>
        public static string ToStrByHHOrEmpty(this DateTime? dateTime)
            => dateTime?.ToString("HH") ?? string.Empty;

        /// <summary>
        /// DateTime?を"mm"形式の文字列に変換します。nullの場合は空文字を返します。
        /// </summary>
        /// <param name="dateTime">変換するDateTime?型の値</param>
        /// <returns>"mm"形式の文字列、またはnullの場合は空文字</returns>
        public static string ToStrBymmOrEmpty(this DateTime? dateTime)
            => dateTime?.ToString("mm") ?? string.Empty;

        public static DateTime ToDateByYYYYMMDDSlash(this DateTime dt)
            => DateTime.Parse(dt.ToStrByYYYYMMDDSlash());

        public static DateTime ToDateByYYYYMMDDSlashHH(this DateTime dt)
            => DateTime.Parse(dt.ToStrByYYYYMMDDSlashHH() + ":00");

        public static DateTime ToDateByYYYYMMDDSlashHHmm(this DateTime dt)
            => DateTime.Parse(dt.ToStrByYYYYMMDDSlashHHmm());

        public static DateTime ToDateByYYYYMMDDSlashHHmmss(this DateTime dt)
            => DateTime.Parse(dt.ToStrByYYYYMMDDSlashHHmmss());

        public static string MDJp(this DateTime dt)
            => dt.ToString("M月d日(ddd)");

        public static string YMDJp(this DateTime dt)
            => dt.ToString("yyyy年MM月dd日");

        public static string YMDJpWeek(this DateTime dt)
            => dt.ToString("yyyy年MM月dd日（ddd）", new CultureInfo("ja-JP"));

        public static string YMJp(this DateTime dt)
            => dt.ToString("yyyy年MM月");

        public static int GetWeek(this DateTime dt)
            => (dt.Day / 7) + 1;

        public static string ToISO8601(this DateTime dt)
            => dt.ToString("yyyy-MM-dd'T'HH:mm:ss'+09:00'");

        public static string ToISO8601ForRFC3986(this DateTime dt)
           => dt.ToString("yyyy-MM-dd'T'HH:mm:ss'%2B09:00'");

        public static string ToYYYYMMDD(this DateTime dt)
            => dt.ToString("yyyyMMdd");

        /// <summary>
        /// 年月の数値に変換します。
        /// 例）2025/04/01 => 202504
        /// </summary>
        public static int ParseYearMonthInt(this DateTime dt)
            => dt.Year * 100 + dt.Month;

        /// <summary>
        /// 時分のみ変更した日時を取得（秒は0）
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime GetDateDesignatedHHmm(this DateTime dt, int targetHour, int targetMinute)
            => new DateTime(dt.Year, dt.Month, dt.Day, targetHour, targetMinute, 0);

        /// <summary>
        /// 日のみ変更した日時を取得
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="targetDay"></param>
        /// <returns></returns>
        public static DateTime GetDateDesignatedDay(this DateTime dt, int targetDay)
            => new DateTime(dt.Year, dt.Month, targetDay, dt.Hour, dt.Minute, dt.Second);

        /// <summary>
        /// 日のみ変更した日時を取得(月末で取得)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime GetDateDesignatedDaysInMonth(this DateTime dt)
            => new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month), dt.Hour, dt.Minute, dt.Second);

        /// <summary>
        /// 日のみ変更した日時を取得(第〇週〇曜日)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="week">第〇週</param>
        /// <param name="dayOfWeek">曜日</param>
        /// <returns></returns>
        public static DateTime GetDateDesignatedWeekDay(this DateTime dt, int week, int dayOfWeek)
        {
            var count = 0;
            var countDate = 0;
            while (count <= week)
            {
                countDate++;
                var date = new DateTime(dt.Year, dt.Month, countDate);
                if ((int)date.DayOfWeek == dayOfWeek)
                {
                    count++;
                }
                if (count == week)
                {
                    break;
                }
            }
            return new DateTime(dt.Year, dt.Month, countDate, dt.Hour, dt.Minute, dt.Second);
        }

        /// <summary>
        /// 指定日数分の日付リストを返します
        /// </summary>
        public static List<DateTime> DateList(this DateTime dt, int count)
        {
            var date = dt.Date;
            return Enumerable.Range(0, count).Select(i => date.AddDays(i)).ToList();
        }

        /// <summary>
        /// 指定期間の日付リストを返します
        /// </summary>
        public static List<DateTime> DateList(this DateTime start, DateTime end)
            => start.DateList((end.Date - start.Date).Days + 1);

        public static TimeOnly ToTimeOnly(this DateTime dateTime)
        {
            return TimeOnly.FromDateTime(dateTime);
        }

        public static DateOnly ToDateOnly(this DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }

        public static DateOnly ToDateOnly(this DateTimeOffset dateTimeOffset)
        {
            return DateOnly.FromDateTime(dateTimeOffset.DateTime);
        }

        public static DateOnly? ToDateOnly(this DateTime? dateTime)
        {
            return Optional(dateTime).Map(x => (DateOnly?)x.ToDateOnly()).IfNoneUnsafe(() => null);
        }

        /// <summary>
        /// 月初を返します
        /// </summary>
        public static DateTime GetStartOfMonth(this DateTime dateTime)
            => new DateTime(dateTime.Year, dateTime.Month, 1);

        /// <summary>
        /// 先月の月初を返します。
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetStartOfLastMonth(this DateTime dateTime)
            => new DateTime(dateTime.Year, dateTime.Month - 1, 1);

        /// <summary>
        /// 月初を返します
        /// </summary>
        //public static DateOnly GetStartOfMonth(this DateOnly dateTime)
        //    => dateTime.ToDateTime(new TimeOnly()).GetStartOfMonth().ToDateOnly();

        /// <summary>
        /// 月末を返します
        /// </summary>
        public static DateTime GetEndOfMonth(this DateTime dateTime)
        {
            int days = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            return new DateTime(dateTime.Year, dateTime.Month, days);
        }

        /// <summary>
        /// 月末を返します(23:59)
        /// </summary>
        public static DateTime GetEndDayOfMonth(this DateTime dateTime)
        {
            int days = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            return new DateTime(dateTime.Year, dateTime.Month, days, 23, 59, 59);
        }

        /// <summary>
        /// 先月の月末を返します
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetEndOfLastMonth(this DateTime dateTime)
        {
            DateTime days = dateTime.GetStartOfMonth().AddDays(-1);
            return new DateTime(days.Year, days.Month, days.Day);
        }

        /// <summary>
        /// 月末を返します
        /// </summary>
        //public static DateOnly GetEndOfMonth(this DateOnly dateTime)
        //    => dateTime.ToDateTime(new TimeOnly()).GetEndOfMonth().ToDateOnly();

        /// <summary>
        /// 対象日・開始時刻・終了時刻から時間リスト作成
        /// </summary>
        /// <param name="dateTime">対象日</param>
        /// <param name="start">開始時刻</param>
        /// <param name="end">終了時刻</param>
        /// <returns></returns>
        public static List<DateTime> DateHourList(this DateTime date, TimeSpan? startTime = null, TimeSpan? endTime = null)
        {
            var start = Optional(startTime).IfNone(() => new TimeSpan(0, 0, 0));
            var end = Optional(endTime).IfNone(() => new TimeSpan(23, 0, 0));
            return Enumerable.Range(start.Hours, (end.Hours - start.Hours) + 1)
                .Select(x => date.AddHours(x))
                .ToList();
        }

        /// <summary>
        /// 対象日・開始時刻・終了時刻から30分リスト作成
        /// </summary>
        /// <param name="dateTime">対象日</param>
        /// <param name="start">開始時刻</param>
        /// <param name="end">終了時刻</param>
        /// <param name="isLimitHalfHour">30分単位でフィルターするか（30分単位で startTime, endTime を入力している場合は true）</param>
        /// <returns></returns>
        public static List<DateTime> DateHalfHourList(this DateTime date, TimeSpan? startTime = null, TimeSpan? endTime = null, bool isLimitHalfHour = false)
        {
            var start = Optional(startTime).IfNone(() => new TimeSpan(0, 0, 0));
            var end = Optional(endTime).IfNone(() => new TimeSpan(23, 30, 0));
            var list = date.DateHourList(start, end)
                .SelectMany(x => new List<DateTime>()
                {
                    x,
                    x.AddMinutes(30),
                });

            if (isLimitHalfHour)
            {
                // オーバーした時間を除く
                list = list.Where(x => start <= x.TimeOfDay)
                    .Where(x => x.TimeOfDay <= end);
            }

            return list.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// DateTime型の月の差分を整数で取得するFunction
        /// </summary>
        /// <param name="fromDate">日付From</param>
        /// <param name="toDate">日付To</param>
        /// <returns>月数の差分</returns>
        public static int GetMonthDiff(DateTime fromDate, DateTime toDate)
        {
            var yDiff = (toDate.Year - fromDate.Year) * 12;
            var mDiff = (toDate.Month - fromDate.Month);

            return yDiff + mDiff;
        }
        /// <summary>
        /// 指定日付が開始日から何年目かを取得します。
        /// </summary>
        /// <param name="startDate">年度計算の開始日</param>
        /// <param name="targetDate">指定日付（省略時は本日）</param>
        /// <returns>(何年目, その年度の開始日, その年度の終了日)</returns>
        public static (int YearIndex, DateOnly StartOfYear, DateOnly EndOfYear) GetYearSpan(this DateTime startDate, DateTime? targetDate = null)
            => startDate.ToDateOnly().GetYearSpan(targetDate.ToDateOnly());

        /// <summary>
        /// DateTime を30分単位で丸めます。（分を 0 または 30 に調整）
        /// </summary>
        /// <param name="dateTime">対象日時</param>
        public static DateTime RoundHalfHour(this DateTime dateTime)
        {
            var minutes = dateTime.Minute / 30 * 30;
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0).AddMinutes(minutes);
        }

        /// <summary>
        /// 1分刻みの日時配列を取得します。
        /// </summary>
        /// <param name="startDateTime">開始日時</param>
        /// <param name="endDateTime">終了日時</param>
        public static List<DateTime> MinuteSpans(this DateTime startDateTime, DateTime endDateTime)
        {
            var start = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, startDateTime.Minute, 0);
            var end = new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day, endDateTime.Hour, endDateTime.Minute, 0);
            return GetSpans(start, end, DateUtilSpanType.Minute);
        }

        /// <summary>
        /// 30分刻みの日時配列を取得します（開始・終了ともに30分丸め）。
        /// </summary>
        /// <param name="startDateTime">開始日時</param>
        /// <param name="endDateTime">終了日時</param>
        public static List<DateTime> HalfHourSpans(this DateTime startDateTime, DateTime endDateTime)
        {
            var start = startDateTime.RoundHalfHour();
            var end = endDateTime.RoundHalfHour();
            return GetSpans(start, end, DateUtilSpanType.HalfHour);
        }

        /// <summary>
        /// 日単位の日時配列を取得します。（時刻は Date プロパティで揃え）
        /// </summary>
        /// <param name="startDateTime">開始日時</param>
        /// <param name="endDateTime">終了日時</param>
        public static List<DateTime> DaySpans(this DateTime startDateTime, DateTime endDateTime)
        {
            var start = startDateTime.Date;
            var end = endDateTime.Date;
            return GetSpans(start, end, DateUtilSpanType.Day);
        }

        /// <summary>
        /// 月単位の日時配列を取得します。（日付は月初）
        /// </summary>
        /// <param name="startDateTime">開始日時</param>
        /// <param name="endDateTime">終了日時</param>
        public static List<DateTime> MonthSpans(this DateTime startDateTime, DateTime endDateTime)
        {
            var start = startDateTime.Date;
            var end = endDateTime.Date;
            return GetSpans(start, end, DateUtilSpanType.Month);
        }

        /// <summary>
        /// 年単位の日時配列を取得します。（日付は年初）
        /// </summary>
        /// <param name="startDateTime">開始日時</param>
        /// <param name="endDateTime">終了日時</param>
        public static List<DateTime> YearSpans(this DateTime startDateTime, DateTime endDateTime)
        {
            var start = startDateTime.Date;
            var end = endDateTime.Date;
            return GetSpans(start, end, DateUtilSpanType.Year);
        }

        /// <summary>
        /// 前年同月同曜日を返します。
        /// </summary>
        /// <param name="dateTime">対象日時</param>
        public static DateTime SameDayOfWeekLastYear(this DateTime dateTime)
        {
            var lastYearDateTime = dateTime.AddYears(-1);
            var dayOfWeek = dateTime.DayOfWeek;
            var lastYearDayOfWeek = lastYearDateTime.DayOfWeek;
            if (dayOfWeek == lastYearDayOfWeek)
            {
                return lastYearDateTime;
            }
            var prevDiff = ((int)dayOfWeek - (int)lastYearDayOfWeek - 7) % 7;
            var nextDiff = ((int)dayOfWeek - (int)lastYearDayOfWeek + 7) % 7;
            return Math.Abs(prevDiff) < Math.Abs(nextDiff)
                ? lastYearDateTime.AddDays(prevDiff)
                : lastYearDateTime.AddDays(nextDiff);
        }

        /// <summary>
        /// SpanType に応じた日時一覧を生成（内部処理用）
        /// </summary>
        private static List<DateTime> GetSpans(DateTime start, DateTime end, DateUtilSpanType spanType)
        {
            var result = new List<DateTime>();
            if (start > end) return result;
            var currentDate = start;
            while (currentDate <= end)
            {
                result.Add(currentDate);
                currentDate = spanType switch
                {
                    DateUtilSpanType.Minute => currentDate.AddMinutes(1),
                    DateUtilSpanType.HalfHour => currentDate.AddMinutes(30),
                    DateUtilSpanType.Day => currentDate.AddDays(1),
                    DateUtilSpanType.Month => currentDate.AddMonths(1),
                    DateUtilSpanType.Year => currentDate.AddYears(1),
                    _ => currentDate
                };
            }
            return result;
        }

        private enum DateUtilSpanType
        {
            Minute,
            HalfHour,
            Day,
            Month,
            Year,
        }
    }
}
