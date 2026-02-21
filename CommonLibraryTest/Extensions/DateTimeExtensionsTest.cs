using CommonLibrary.Extensions;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class DateTimeExtensionsTest
    {
        [TestMethod]
        public void TestDateList()
        {
            var endDayMonth = DateTime.Parse("2024/11/01").GetEndDayOfMonth();
            Assert.AreEqual(DateTime.Parse("2024/11/30 23:59:59"), endDayMonth);

            //新年度用テスト
            var endDayMonth2 = DateTime.Parse("2025/01/01").GetEndDayOfMonth();
            Assert.AreEqual(DateTime.Parse("2025/01/31 23:59:59"), endDayMonth2);
        }
        [TestMethod]
        public void ToISO8601Test()
        {
            var actual = DateTime.Parse("2025/01/31 23:59:59").ToISO8601();
            Assert.AreEqual("2025-01-31T23:59:59+09:00", actual);
        }
        [TestMethod]
        public void ToISO8601ForRFC3986Test()
        {
            var actual = DateTime.Parse("2025/01/31 23:59:59").ToISO8601ForRFC3986();
            Assert.AreEqual("2025-01-31T23:59:59%2B09:00", actual);
        }

        [TestMethod]
        public void FormattingStringsTest()
        {
            var ja = System.Globalization.CultureInfo.GetCultureInfo("ja-JP");
            System.Globalization.CultureInfo.CurrentCulture = ja;
            System.Globalization.CultureInfo.CurrentUICulture = ja;

            var dt = DateTime.Parse("2024/10/15 13:45:27");
            Assert.AreEqual("2024/10/15", dt.ToStrByYYYYMMDDSlash());
            DateTime? nullable = dt;
            Assert.AreEqual("2024/10/15", nullable.ToStrByYYYYMMDDSlash("x"));
            DateTime? nullDt = null;
            Assert.AreEqual("x", nullDt.ToStrByYYYYMMDDSlash("x"));
            Assert.AreEqual("2024/10", dt.ToStrByYYYYMMSlash());
            Assert.AreEqual("2024/10/15 13", dt.ToStrByYYYYMMDDSlashHH());
            Assert.AreEqual("2024/10/15 13:45", dt.ToStrByYYYYMMDDSlashHHmm());
            Assert.AreEqual("2024/10/15 13:45:27", dt.ToStrByYYYYMMDDSlashHHmmss());
            Assert.AreEqual("13:45:27", dt.ToDateByYYYYMMDDSlashHHmmss().ToString("HH:mm:ss")); // round-trip
            StringAssert.Contains(dt.MDJp(), "10月15日");
            Assert.AreEqual("2024年10月15日", dt.YMDJp());
            Assert.AreEqual("2024年10月", dt.YMJp());
        }

        [TestMethod]
        public void ReverseParsingTest()
        {
            var dt = DateTime.Parse("2024/10/15 13:45:27");
            Assert.AreEqual(DateTime.Parse("2024/10/15 00:00:00"), dt.ToDateByYYYYMMDDSlash());
            Assert.AreEqual(DateTime.Parse("2024/10/15 13:00:00"), dt.ToDateByYYYYMMDDSlashHH());
            Assert.AreEqual(DateTime.Parse("2024/10/15 13:45:00"), dt.ToDateByYYYYMMDDSlashHHmm());
            Assert.AreEqual(DateTime.Parse("2024/10/15 13:45:27"), dt.ToDateByYYYYMMDDSlashHHmmss());
        }

        [TestMethod]
        public void WeekAndParseYearMonthIntTest()
        {
            var dt = DateTime.Parse("2024/10/15"); // 15 => (15 / 7)+1 = 3
            Assert.AreEqual(3, dt.GetWeek());
            Assert.AreEqual(202410, dt.ParseYearMonthInt());
        }

        [TestMethod]
        public void DesignatedDateMethodsTest()
        {
            var dt = DateTime.Parse("2024/10/10 09:12:05");
            var hhmm = dt.GetDateDesignatedHHmm(14, 30);
            Assert.AreEqual(DateTime.Parse("2024/10/10 14:30:00"), hhmm);
            var dayChanged = dt.GetDateDesignatedDay(5);
            Assert.AreEqual(DateTime.Parse("2024/10/05 09:12:05"), dayChanged);
            var monthEnd = dt.GetDateDesignatedDaysInMonth();
            Assert.AreEqual(DateTime.Parse("2024/10/31 09:12:05"), monthEnd);
            // 第2月曜日 (2024/10 月曜は 7,14,21,28)
            var secondMonday = dt.GetDateDesignatedWeekDay(2, (int)DayOfWeek.Monday);
            Assert.AreEqual(DateTime.Parse("2024/10/14 09:12:05"), secondMonday);
        }

        [TestMethod]
        public void DateListOverloadsTest()
        {
            var start = DateTime.Parse("2024/10/01 08:00:00");
            var listCount = start.DateList(3);
            Assert.HasCount(3, listCount);
            Assert.AreEqual(DateTime.Parse("2024/10/01 00:00:00"), listCount[0]);
            Assert.AreEqual(DateTime.Parse("2024/10/02 00:00:00"), listCount[1]);
            Assert.AreEqual(DateTime.Parse("2024/10/03 00:00:00"), listCount[2]);

            var end = DateTime.Parse("2024/10/05 23:59:59");
            var listSpan = start.DateList(end);
            Assert.HasCount(5, listSpan);
            Assert.AreEqual(DateTime.Parse("2024/10/05 00:00:00"), listSpan[^1]);
        }

        [TestMethod]
        public void ToTimeAndDateOnlyTest()
        {
            var dt = DateTime.Parse("2024/10/15 13:45:27");
            var time = dt.ToTimeOnly();
            Assert.AreEqual(13, time.Hour);
            Assert.AreEqual(45, time.Minute);
            Assert.AreEqual(27, time.Second);

            var dateOnly = dt.ToDateOnly();
            Assert.AreEqual(DateOnly.Parse("2024/10/15"), dateOnly);

            DateTime? nullable = dt;
            var dateOnlyNullable = nullable.ToDateOnly();
            Assert.AreEqual(DateOnly.Parse("2024/10/15"), dateOnlyNullable);
            DateTime? nullDt = null;
            Assert.IsNull(nullDt.ToDateOnly());
        }

        [TestMethod]
        public void MonthBoundaryMethodsTest()
        {
            var mid = DateTime.Parse("2024/10/15 12:00:00");
            Assert.AreEqual(DateTime.Parse("2024/10/01 00:00:00"), mid.GetStartOfMonth());
            Assert.AreEqual(DateTime.Parse("2024/09/01 00:00:00"), mid.GetStartOfLastMonth());
            Assert.AreEqual(DateTime.Parse("2024/10/31 00:00:00"), mid.GetEndOfMonth());
            Assert.AreEqual(DateTime.Parse("2024/09/30 00:00:00"), mid.GetEndOfLastMonth());
        }

        [TestMethod]
        public void HourAndHalfHourListTest()
        {
            var date = DateTime.Parse("2024/10/01 00:00:00");
            var hours = date.DateHourList(new TimeSpan(8,0,0), new TimeSpan(10,0,0));
            Assert.HasCount(3, hours); // 8,9,10
            Assert.AreEqual(8, hours[0].Hour);
            Assert.AreEqual(10, hours[^1].Hour);

            var halfHours = date.DateHalfHourList(new TimeSpan(8,0,0), new TimeSpan(9,30,0), true);
            // 8:00,8:30,9:00,9:30
            Assert.HasCount(4, halfHours);
            Assert.AreEqual(8, halfHours[0].Hour);
            Assert.AreEqual(30, halfHours[1].Minute);
            Assert.AreEqual(9, halfHours[2].Hour);
            Assert.AreEqual(30, halfHours[3].Minute);

            // isLimitHalfHour=false includes also beyond boundaries inside expansion until sorting
            var halfHoursLoose = date.DateHalfHourList(new TimeSpan(8,0,0), new TimeSpan(9,30,0), false);
            Assert.IsGreaterThanOrEqualTo(halfHoursLoose.Count, halfHours.Count);
        }

        [TestMethod]
        public void GetMonthDiffTest()
        {
            var from = DateTime.Parse("2024/10/01");
            var to = DateTime.Parse("2025/01/05");
            Assert.AreEqual(3, DateTimeExtensions.GetMonthDiff(from, to));
        }

        [TestMethod]
        public void ToHHmmTest()
        {
            var dt1 = DateTime.Parse("2024/10/15 09:05:27");
            Assert.AreEqual("09:05", dt1.ToHHmm());

            var dt2 = DateTime.Parse("2024/10/15 13:45:27");
            Assert.AreEqual("13:45", dt2.ToHHmm());

            var dt3 = DateTime.Parse("2024/10/15 00:00:00");
            Assert.AreEqual("00:00", dt3.ToHHmm());

            var dt4 = DateTime.Parse("2024/10/15 23:59:59");
            Assert.AreEqual("23:59", dt4.ToHHmm());

            var dt5 = DateTime.Parse("2024/10/15 12:30:45");
            Assert.AreEqual("12:30", dt5.ToHHmm());
        }

        /// <summary>
        /// ToStrByHHOrEmptyのテスト
        /// </summary>
        [TestMethod]
        public void ToStrByHHOrEmptyTest()
        {
            DateTime? date1 = new DateTime(1, 1, 1, 0, 1, 1);
            DateTime? date2 = new DateTime(1, 2, 3, 4, 5, 6);
            DateTime? date3 = new DateTime(2025, 12, 31, 12, 30, 0);
            DateTime? nullableDateTimeNull = null;

            Assert.AreEqual("00", date1.ToStrByHHOrEmpty());
            Assert.AreEqual("04", date2.ToStrByHHOrEmpty());
            Assert.AreEqual("12", date3.ToStrByHHOrEmpty());
            Assert.AreEqual(string.Empty, nullableDateTimeNull.ToStrByHHOrEmpty());
        }

        /// <summary>
        /// ToStrBymmOrEmptyのテスト
        /// </summary>
        [TestMethod]
        public void ToStrBymmOrEmptyTest()
        {
            DateTime? date1 = new DateTime(1, 1, 1, 1, 0, 1);
            DateTime? date2 = new DateTime(1, 2, 3, 4, 5, 6);
            DateTime? date3 = new DateTime(2025, 12, 31, 12, 30, 0);
            DateTime? nullableDateTimeNull = null;

            Assert.AreEqual("00", date1.ToStrBymmOrEmpty());
            Assert.AreEqual("05", date2.ToStrBymmOrEmpty());
            Assert.AreEqual("30", date3.ToStrBymmOrEmpty());
            Assert.AreEqual(string.Empty, nullableDateTimeNull.ToStrBymmOrEmpty());
        }

        [TestMethod]
        public void TestYMDJpWeek()
        {
            var date = DateTime.Parse("2026/01/01");
            Assert.AreEqual("2026年01月01日（木）", DateTimeExtensions.YMDJpWeek(date));
        }
    }
}
