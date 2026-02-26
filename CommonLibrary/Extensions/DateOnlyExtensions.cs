using System;
using System.Collections.Generic;
using System.Linq;
using static LanguageExt.Prelude;

namespace CommonLibrary.Extensions
{
    public static class DateOnlyExtensions
    {
        public static string YMDSlash(this DateOnly dateOnly)
            => dateOnly.ToDateTime().ToStrByYYYYMMDDSlash();

        public static string YMSlash(this DateOnly dateOnly)
            => dateOnly.ToDateTime().ToStrByYYYYMMSlash();

        public static string YMDJpWeek(this DateOnly dateOnly)
            => dateOnly.ToDateTime().YMDJpWeek();

        public static string MDJp(this DateOnly dateOnly)
            => dateOnly.ToDateTime().MDJp();

        public static string YMDJp(this DateOnly dateOnly)
            => dateOnly.ToDateTime().YMDJp();

        public static string YMJp(this DateOnly dateOnly)
            => dateOnly.ToDateTime().YMJp();

        public static int GetWeek(this DateOnly dateOnly)
            => dateOnly.ToDateTime().GetWeek();

        public static string ToYYYYMMDD(this DateOnly dateOnly)
            => dateOnly.ToDateTime().ToYYYYMMDD();

        /// <summary>
        /// 年月の数値に変換します。
        /// 例）2025/04/01 => 202504
        /// </summary>
        public static int ParseYearMonthInt(this DateOnly dt)
            => dt.Year * 100 + dt.Month;

        /// <summary>
        /// 日のみ変更した日時を取得
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <param name="targetDay"></param>
        /// <returns></returns>
        public static DateOnly GetDateDesignatedDay(this DateOnly dateOnly, int targetDay)
            => dateOnly.ToDateTime().GetDateDesignatedDay(targetDay).ToDateOnly();

        /// <summary>
        /// 日のみ変更した日時を取得(月末で取得)
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <returns></returns>
        public static DateOnly GetDateDesignatedDaysInMonth(this DateOnly dateOnly)
            => dateOnly.ToDateTime().GetDateDesignatedDaysInMonth().ToDateOnly();

        /// <summary>
        /// 日のみ変更した日時を取得(第〇週〇曜日)
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <param name="week">第〇週</param>
        /// <param name="dayOfWeek">曜日</param>
        /// <returns></returns>
        public static DateOnly GetDateDesignatedWeekDay(this DateOnly dateOnly, int week, int dayOfWeek)
            => dateOnly.ToDateTime().GetDateDesignatedWeekDay(week, dayOfWeek).ToDateOnly();

        /// <summary>
        /// fromからtoIncludeまでの日付リストを返します
        /// </summary>
        public static List<DateOnly> DateList(this DateOnly from, DateOnly toInclude)
            => from.DateList(toInclude.DayNumber - from.DayNumber + 1);

        /// <summary>
        /// 指定日数分の日付リストを返します
        /// </summary>
        public static List<DateOnly> DateList(this DateOnly dateOnly, int count)
            => Enumerable.Range(0, count).Select(i => dateOnly.AddDays(i)).ToList();

        public static DateTime ToDateTime(this DateOnly dateOnly, TimeOnly? to = null)
            => to.HasValue ? dateOnly.ToDateTime(to.Value) : dateOnly.ToDateTime(new TimeOnly());

        public static DateTime? ToDateTime(this DateOnly? dateOnly, TimeOnly? to = null)
            => Optional(dateOnly).Map(x => (DateTime?)x.ToDateTime()).IfNoneUnsafe(() => null);


        /// <summary>
        /// 月初を返します
        /// </summary>
        public static DateOnly GetStartOfMonth(this DateOnly dateOnly)
            => new DateOnly(dateOnly.Year, dateOnly.Month, 1);

        /// <summary>
        /// 月末を返します
        /// </summary>
        public static DateOnly GetEndOfMonth(this DateOnly dateOnly)
        {
            int days = DateTime.DaysInMonth(dateOnly.Year, dateOnly.Month);
            return new DateOnly(dateOnly.Year, dateOnly.Month, days);
        }

        /// <summary>
        /// 先月の月初を返します。
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateOnly GetStartOfLastMonth(this DateOnly dateTime)
        {
            DateOnly days = dateTime.AddMonths(-1);
            return new DateOnly(days.Year, days.Month, 1);
        }

        /// <summary>
        /// 先月の月末を返します
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateOnly GetEndOfLastMonth(this DateOnly dateTime)
        {
            DateOnly days = dateTime.GetStartOfMonth().AddDays(-1);
            return new DateOnly(days.Year, days.Month, days.Day);
        }

        /// <summary>
        /// DateOnly型の月の差分を整数で取得するFunction
        /// </summary>
        /// <param name="fromDate">日付From</param>
        /// <param name="toDate">日付To</param>
        /// <returns>月数の差分</returns>
        public static int GetMonthDiff(DateOnly fromDate, DateOnly toDate)
            => DateTimeExtensions.GetMonthDiff(fromDate.ToDateTime(), toDate.ToDateTime());

        /// <summary>
        /// <paramref name="date1"/>と<paramref name="date2"/>に含まれる日数を取得する。
        /// </summary>
        /// <param name="date1">日付1</param>
        /// <param name="date2">日付2</param>
        /// <returns><paramref name="date1"/> ≦ D ≦ <paramref name="date2"/>または<paramref name="date2"/> ≦ D ≦ <paramref name="date1"/>を満たす<see cref="DateOnly"/> Dの個数</returns>
        public static int GetDayCount(DateOnly date1, DateOnly date2)
            => (date1 <= date2 ? date2.DayNumber - date1.DayNumber : date1.DayNumber - date2.DayNumber) + 1;

        /// <summary>
        /// 指定日付が開始日から何年目かを取得します。
        /// </summary>
        /// <param name="startDate">年度計算の開始日</param>
        /// <param name="targetDate">指定日付（省略時は本日）</param>
        /// <returns>(何年目, その年度の開始日, その年度の終了日)</returns>
        public static (int YearIndex, DateOnly StartOfYear, DateOnly EndOfYear) GetYearSpan(this DateOnly startDate, DateOnly? targetDate = null)
        {
            var date = targetDate ?? DateTime.Today.ToDateOnly();
            var endDate = startDate.AddYears(1);
            var nendo = 1;
            while (date >= endDate)
            {
                endDate = endDate.AddYears(1);
                nendo++;
            }
            return (nendo, endDate.AddYears(-1), endDate.AddDays(-1));
        }

        /// <summary>
        /// 日単位の DateOnly 配列を取得します。
        /// </summary>
        public static List<DateOnly> DaySpans(this DateOnly startDate, DateOnly endDate)
            => startDate.ToDateTime().DaySpans(endDate.ToDateTime()).Select(x => x.ToDateOnly()).OrderBy(x => x).ToList();

        /// <summary>
        /// 月単位の DateOnly 配列を取得します。
        /// </summary>
        public static List<DateOnly> MonthSpans(this DateOnly startDate, DateOnly endDate)
            => startDate.ToDateTime().MonthSpans(endDate.ToDateTime()).Select(x => x.ToDateOnly()).OrderBy(x => x).ToList();

        /// <summary>
        /// 年単位の DateOnly 配列を取得します。
        /// </summary>
        public static List<DateOnly> YearSpans(this DateOnly startDate, DateOnly endDate)
            => startDate.ToDateTime().YearSpans(endDate.ToDateTime()).Select(x => x.ToDateOnly()).OrderBy(x => x).ToList();

        /// <summary>
        /// 指定した日付が属する「年度開始年（YYYY）」を取得します。
        /// 既定の年度は「10月開始～翌年9月終了」です。
        /// 例：2025/09/30 → 2025（= 2025年度）、2025/10/01 → 2026（= 2026年度）
        /// </summary>
        /// <param name="dt">年度判定の対象日</param>
        /// <returns>
        /// 年度の「開始年（YYYY）」。
        /// </returns>
        public static int GetFiscalYear(this DateOnly dt)
         => dt.Month < 10 ? dt.Year : dt.Year + 1;
    }
}
