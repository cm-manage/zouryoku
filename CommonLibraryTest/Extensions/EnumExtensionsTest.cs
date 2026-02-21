using System;
using System.ComponentModel.DataAnnotations;
using CommonLibrary.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    [Flags]
    public enum TestFlag
    {
        None = 0,
        One = 1,
        Two = 2,
        Four = 4,
    }

    public enum DisplayEnum
    {
        [Display(Name = "表示A")]
        A = 0,
        B = 1,
    }

    [TestClass]
    public class EnumExtensionsTest
    {
        [TestMethod]
        public void IsInclude()
        {
            var v = TestFlag.One | TestFlag.Two;
            Assert.IsTrue(v.IsInclude(TestFlag.One));
            Assert.IsFalse(v.IsInclude(TestFlag.Four));
        }

        [TestMethod]
        public void GetDisplayName()
        {
            Assert.AreEqual("表示A", DisplayEnum.A.GetDisplayName());
            Assert.AreEqual(nameof(DisplayEnum.B), DisplayEnum.B.GetDisplayName());
        }
    }
}
