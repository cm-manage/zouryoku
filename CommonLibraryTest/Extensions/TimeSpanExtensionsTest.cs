using System;
using CommonLibrary.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class TimeSpanExtensionsTest
    {
        [TestMethod]
        public void HHmmColon_正常系()
        {
            var ts = new TimeSpan(1, 23, 0);
            Assert.AreEqual("01:23", ts.HHmmColon());
        }

        [TestMethod]
        public void HHmm_正常系()
        {
            var ts = new TimeSpan(9, 5, 0);
            Assert.AreEqual("0905", ts.HHmm());
        }

        [TestMethod]
        public void ToTimeSpan_5桁_正常系()
        {
            var str = "12:34";
            var ts = str.ToTimeSpan();
            Assert.AreEqual(new TimeSpan(12, 34, 0), ts);
        }

        [TestMethod]
        public void ToTimeSpan_8桁_正常系()
        {
            var str = "12:34:56";
            var ts = str.ToTimeSpan();
            Assert.AreEqual(new TimeSpan(12, 34, 56), ts);
        }

        [TestMethod]
        public void ToTimeSpan_不正な文字列_FormatException()
        {
            var str = "abcde";
            Assert.ThrowsExactly<FormatException>(() => str.ToTimeSpan());
        }

        [TestMethod]
        public void Min_正常系()
        {
            var t1 = new TimeSpan(1, 0, 0);
            var t2 = new TimeSpan(2, 0, 0);
            Assert.AreEqual(t1, t1.Min(t2));
            Assert.AreEqual(t1, t2.Min(t1));
        }

        [TestMethod]
        public void Max_正常系()
        {
            var t1 = new TimeSpan(1, 0, 0);
            var t2 = new TimeSpan(2, 0, 0);
            Assert.AreEqual(t2, t1.Max(t2));
            Assert.AreEqual(t2, t2.Max(t1));
        }
    }
}
