using CommonLibrary.Extensions;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class TimeOnlyExtensionsTest
    {
        /// <summary>
        /// ToStrByHHmmOrEmptyのテスト
        /// </summary>
        [TestMethod]
        public void ToStrByHHmmOrEmpty_Test()
        {
            TimeOnly? time1 = new TimeOnly(0, 0);
            TimeOnly? time2 = new TimeOnly(1, 2);
            TimeOnly? time3 = new TimeOnly(12, 34);
            TimeOnly? nullableTimeNull = null;

            Assert.AreEqual("00:00", time1.ToStrByHHmmOrEmpty());
            Assert.AreEqual("01:02", time2.ToStrByHHmmOrEmpty());
            Assert.AreEqual("12:34", time3.ToStrByHHmmOrEmpty());
            Assert.AreEqual(string.Empty, nullableTimeNull.ToStrByHHmmOrEmpty());
        }
    }
}
