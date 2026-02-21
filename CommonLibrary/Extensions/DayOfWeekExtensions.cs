using System;
using static System.DayOfWeek;

namespace CommonLibrary.Extensions
{
    public static class DayOfWeekExtensions
    {
        public static string ToJpString(this DayOfWeek dow)
            => dow switch
            {
                Sunday => "日曜日",
                Monday => "月曜日",
                Tuesday => "火曜日",
                Wednesday => "水曜日",
                Thursday => "木曜日",
                Friday => "金曜日",
                Saturday => "土曜日",
                _ => throw new Exception("DayOfWeekとして認識できない値です。" + dow),
            };

        public static string ToJpShortString(this DayOfWeek dow)
            => dow switch
            {
                Sunday => "日",
                Monday => "月",
                Tuesday => "火",
                Wednesday => "水",
                Thursday => "木",
                Friday => "金",
                Saturday => "土",
                _ => throw new Exception("DayOfWeekとして認識できない値です。" + dow),
            };
    }
}
