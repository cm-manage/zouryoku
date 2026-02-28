using CommonLibrary.Extensions;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class DateOnlyExtensionsTest
    {
        [TestMethod]
        public void GetYearSpan_Test()
        {
            var start = new DateOnly(2024, 4, 1);
            var target = new DateOnly(2025, 3, 31);
            var (index, startOfYear, endOfYear) = start.GetYearSpan(target);
            Assert.AreEqual(1, index);
            Assert.AreEqual(start, startOfYear);
            Assert.AreEqual(new DateOnly(2025, 3, 31), endOfYear);
        }

        [TestMethod]
        public void DayMonthSpans_Test()
        {
            var daySpan = new DateOnly(2024, 10, 1).DaySpans(new DateOnly(2024, 10, 3));
            CollectionAssert.AreEqual(new[] { new DateOnly(2024,10,1), new DateOnly(2024,10,2), new DateOnly(2024,10,3) }, daySpan);

            var monthSpan = new DateOnly(2024, 10, 1).MonthSpans(new DateOnly(2025, 1, 1));
            CollectionAssert.AreEqual(new[] { new DateOnly(2024,10,1), new DateOnly(2024,11,1), new DateOnly(2024,12,1), new DateOnly(2025,1,1) }, monthSpan);
        }

        [TestMethod]
        public void TestDateList()
        {
            var list = DateOnly.Parse("2024/10/01").DateList(DateOnly.Parse("2024/10/01"));
            
            Assert.HasCount(1, list);
            Assert.AreEqual(DateOnly.Parse("2024/10/01"), list[0]);


            list = DateOnly.Parse("2024/10/01").DateList(DateOnly.Parse("2024/10/05"));
            Assert.AreEqual(DateOnly.Parse("2024/10/01"), list[0]);
            Assert.AreEqual(DateOnly.Parse("2024/10/03"), list[2]);
            Assert.AreEqual(DateOnly.Parse("2024/10/05"), list[4]);

            var dateTime = DateOnly.Parse("2024/11/1");

            var startLastMonth = dateTime.GetStartOfLastMonth();
            var endLastMonth = dateTime.GetEndOfLastMonth();
            var endMonth = dateTime.GetEndOfMonth();

            Assert.AreEqual(DateOnly.Parse("2024/10/01"), startLastMonth);
            Assert.AreEqual(DateOnly.Parse("2024/10/31"), endLastMonth);
            Assert.AreEqual(DateOnly.Parse("2024/11/30"), endMonth);

            //新年度用テスト
            var dateTime2 = DateOnly.Parse("2025/1/1");

            var startLastMonth2 = dateTime2.GetStartOfLastMonth();
            var endLastMonth2 = dateTime2.GetEndOfLastMonth();
            var endMonth2 = dateTime2.GetEndOfMonth();

            Assert.AreEqual(DateOnly.Parse("2024/12/01"), startLastMonth2);
            Assert.AreEqual(DateOnly.Parse("2024/12/31"), endLastMonth2);
            Assert.AreEqual(DateOnly.Parse("2025/1/31"), endMonth2);
        }

        [TestMethod]
        public void TestFormattingAndWeek()
        {
            // 日本語カルチャに依存するフォーマットのためカルチャを設定
            var ja = System.Globalization.CultureInfo.GetCultureInfo("ja-JP");
            System.Globalization.CultureInfo.CurrentCulture = ja;
            System.Globalization.CultureInfo.CurrentUICulture = ja;

            var d = DateOnly.Parse("2024/10/15"); // 2024-10-15 は火曜日

            Assert.AreEqual("2024/10/15", d.YMDSlash());
            Assert.AreEqual("2024/10", d.YMSlash());
            Assert.AreEqual("2024年10月15日", d.YMDJp());
            Assert.AreEqual("2024年10月", d.YMJp());
            // "10月15日(火)" の形式 (曜日は火)
            StringAssert.Contains(d.MDJp(), "10月15日");

            // 週番号: 15日 => (15 / 7) + 1 = 3
            Assert.AreEqual(3, d.GetWeek());
        }

        [TestMethod]
        public void TestParseYearMonthInt()
        {
            var d = DateOnly.Parse("2025/04/01");
            Assert.AreEqual(202504, d.ParseYearMonthInt());
        }

        [TestMethod]
        public void TestDesignatedDayMethods()
        {
            var baseDate = DateOnly.Parse("2024/10/10");
            // 日のみ変更
            var changed = baseDate.GetDateDesignatedDay(15);
            Assert.AreEqual(DateOnly.Parse("2024/10/15"), changed);

            // 月末 (うるう年 2024/02)
            var febDate = DateOnly.Parse("2024/02/10");
            var monthEnd = febDate.GetDateDesignatedDaysInMonth();
            Assert.AreEqual(DateOnly.Parse("2024/02/29"), monthEnd);

            // 第2月曜日 (2024/10 の月曜日: 7,14,21,28)
            var secondMonday = baseDate.GetDateDesignatedWeekDay(2, (int)DayOfWeek.Monday);
            Assert.AreEqual(DateOnly.Parse("2024/10/14"), secondMonday);
        }

        [TestMethod]
        public void TestDateListCountOverload()
        {
            var start = DateOnly.Parse("2024/10/01");
            var list = start.DateList(3);
            Assert.HasCount(3, list);
            Assert.AreEqual(DateOnly.Parse("2024/10/01"), list[0]);
            Assert.AreEqual(DateOnly.Parse("2024/10/02"), list[1]);
            Assert.AreEqual(DateOnly.Parse("2024/10/03"), list[2]);
        }

        [TestMethod]
        public void TestToDateTimeConversions()
        {
            var date = DateOnly.Parse("2024/10/01");
            var dtMidnight = date.ToDateTime();
            Assert.AreEqual(new DateTime(2024,10,01,0,0,0), dtMidnight);

            var time = new TimeOnly(13,45,30);
            var dtWithTime = date.ToDateTime(time);
            Assert.AreEqual(new DateTime(2024,10,01,13,45,30), dtWithTime);

            DateOnly? nullableDate = date;
            var nullableResult = nullableDate.ToDateTime();
            Assert.IsNotNull(nullableResult);
            Assert.AreEqual(new DateTime(2024,10,01,0,0,0), nullableResult!.Value);

            DateOnly? nullDate = null;
            var nullConverted = nullDate.ToDateTime();
            Assert.IsNull(nullConverted);
        }

        [TestMethod]
        public void TestStartOfMonth()
        {
            var mid = DateOnly.Parse("2024/10/15");
            Assert.AreEqual(DateOnly.Parse("2024/10/01"), mid.GetStartOfMonth());
        }

        [TestMethod]
        public void TestMonthDiff()
        {
            var from = DateOnly.Parse("2024/10/01");
            var to = DateOnly.Parse("2025/01/05");
            Assert.AreEqual(3, DateOnlyExtensions.GetMonthDiff(from, to));
        }

        [TestMethod]
        public void TestToYYYYMMDD()
        {
            var date = DateOnly.Parse("2026/01/01");
            Assert.AreEqual("20260101", DateOnlyExtensions.ToYYYYMMDD(date));
        }

        [TestMethod]
        public void TestYMDJpWeek()
        {
            var date = DateOnly.Parse("2026/01/01");
            Assert.AreEqual("2026年01月01日（木）", DateOnlyExtensions.YMDJpWeek(date));
        }

        /// <summary>
        /// 日付から年度を取得する
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="day">日</param>
        /// <param name="expected">正しい年度</param>
        [TestMethod]
        [DataRow(2025, 10, 1, 2026, DisplayName = "10月1日は翌年")]
        [DataRow(2025, 9, 30, 2025, DisplayName = "9月末は当年")]
        [DataRow(2025, 12, 31, 2026, DisplayName = "年末は翌年")]
        [DataRow(2025, 1, 1, 2025, DisplayName = "年始は当年")]
        [DataRow(1, 1, 1, 1, DisplayName = "最小年月日は当年")]
        [DataRow(9999, 12, 31, 10000, DisplayName = "最大年月日は翌年")]
        public void GetFiscalYear_年度変換(int year, int month, int day, int expected)
        {
            //Arrange
            var date = new DateOnly(year, month, day);

            //Act
            var result = date.GetFiscalYear();

            //Assert
            Assert.AreEqual(expected, result, "変換した年度が正しくありません");
        }

        [TestMethod]
        [DataRow(15, 1, DisplayName = "境界値: 同日のとき")]
        [DataRow(16, 2, DisplayName = "baseDate < toDateの境界値: 1日差のとき")]
        [DataRow(20, 6, DisplayName = "baseDate < toDateの代表値")]
        [DataRow(14, 2, DisplayName = "baseDate > toDateの境界値: 1日差のとき")]
        [DataRow(10, 6, DisplayName = "baseDate > toDateの代表値")]
        public void GetDayCount_日数取得(int dayTo, int expectedCount)
        {
            // Arrange
            var baseDate = new DateOnly(2026, 1, 15);
            var toDate = new DateOnly(2026, 1, dayTo);

            // Act
            var count = DateOnlyExtensions.GetDayCount(baseDate, toDate);

            // Assert
            Assert.AreEqual(expectedCount, count);
        }
    }
}