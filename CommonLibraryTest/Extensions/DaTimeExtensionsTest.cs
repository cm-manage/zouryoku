using System;
using System.Linq;
using CommonLibrary.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class DaTimeExtensionsTest
    {
        [TestMethod]
        public void RoundHalfHour_Test()
        {
            Assert.AreEqual(DateTime.Parse("2024/10/01 08:00:00"), DateTime.Parse("2024/10/01 08:10:00").RoundHalfHour());
            Assert.AreEqual(DateTime.Parse("2024/10/01 08:30:00"), DateTime.Parse("2024/10/01 08:40:00").RoundHalfHour());
        }

        [TestMethod]
        public void MinuteSpans_Test()
        {
            var list = DateTime.Parse("2024/10/01 08:00:00").MinuteSpans(DateTime.Parse("2024/10/01 08:02:00"));
            Assert.HasCount(3, list);
            Assert.AreEqual("08:01", list[1].ToString("HH:mm"));
        }

        [TestMethod]
        public void HalfHourSpans_Test()
        {
            var list = DateTime.Parse("2024/10/01 08:10:00").HalfHourSpans(DateTime.Parse("2024/10/01 09:40:00"));
            CollectionAssert.AreEqual(new[] { "08:00", "08:30", "09:00", "09:30" }, list.Select(x => x.ToString("HH:mm")).ToList());
        }
    }
}
