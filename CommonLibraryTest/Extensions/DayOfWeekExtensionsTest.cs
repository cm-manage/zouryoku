using CommonLibrary.Extensions;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class DayOfWeekExtensionsTest
    {
        [TestMethod]
        public void ToJpString_AllDays()
        {
            Assert.AreEqual("日曜日", DayOfWeek.Sunday.ToJpString());
            Assert.AreEqual("月曜日", DayOfWeek.Monday.ToJpString());
            Assert.AreEqual("火曜日", DayOfWeek.Tuesday.ToJpString());
            Assert.AreEqual("水曜日", DayOfWeek.Wednesday.ToJpString());
            Assert.AreEqual("木曜日", DayOfWeek.Thursday.ToJpString());
            Assert.AreEqual("金曜日", DayOfWeek.Friday.ToJpString());
            Assert.AreEqual("土曜日", DayOfWeek.Saturday.ToJpString());
        }

        [TestMethod]
        public void ToJpShortString_AllDays()
        {
            Assert.AreEqual("日", DayOfWeek.Sunday.ToJpShortString());
            Assert.AreEqual("月", DayOfWeek.Monday.ToJpShortString());
            Assert.AreEqual("火", DayOfWeek.Tuesday.ToJpShortString());
            Assert.AreEqual("水", DayOfWeek.Wednesday.ToJpShortString());
            Assert.AreEqual("木", DayOfWeek.Thursday.ToJpShortString());
            Assert.AreEqual("金", DayOfWeek.Friday.ToJpShortString());
            Assert.AreEqual("土", DayOfWeek.Saturday.ToJpShortString());
        }
    }
}
