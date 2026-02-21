using CommonLibrary.Extensions;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public sealed class StringExtensionsTest
    {
        [TestMethod]
        public void TestToDate()
        {
            var expected = DateTime.Parse("2025/04/15 00:00:00");
            Assert.AreEqual(expected, "20250415".ToDate("yyyyMMdd"));
        }
        [TestMethod]
        public void TestToDateOnly()
        {
            var expected = DateTime.Parse("2025/04/16 00:00:00").ToDateOnly();
            Assert.AreEqual(expected, "20250416".ToDateOnly());
        }
        [TestMethod]
        public void TestToDateTimeFromISO8601()
        {
            var expected = DateTime.Parse("2023/06/30 04:30:00");
            Assert.AreEqual(expected, "2023-06-30T04:30:00+09:00".ToDateTimeFromISO8601());
        }
    }
}
