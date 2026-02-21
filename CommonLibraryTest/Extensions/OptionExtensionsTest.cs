using System;
using CommonLibrary.Extensions;
using LanguageExt;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class OptionExtensionsTest
    {
        [TestMethod]
        public void OptionalT_Test()
        {
            Assert.IsTrue(CommonLibrary.Extensions.OptionExtensions.OptionalT("abc").IsSome);
            Assert.IsTrue(CommonLibrary.Extensions.OptionExtensions.OptionalT(" ").IsNone);
            Assert.IsTrue(CommonLibrary.Extensions.OptionExtensions.OptionalT(null).IsNone);
        }

        [TestMethod]
        public void GetOrThrowException_Test()
        {
            var op = Option<int>.Some(5);
            Assert.AreEqual(5, op.GetOrThrowException("error"));
            var none = Option<int>.None;
            Assert.ThrowsExactly<Exception>(() => none.GetOrThrowException("x"));
        }
    }
}
