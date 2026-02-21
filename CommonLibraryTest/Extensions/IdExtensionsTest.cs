using CommonLibrary.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class IdExtensionsTest
    {
        [TestMethod]
        public void IsNew_Test()
        {
            Assert.IsTrue(0.IsNew());
            Assert.IsFalse(5.IsNew());
            long l = 0;
            Assert.IsTrue(l.IsNew());
        }

        [TestMethod]
        public void IsNotNew_Test()
        {
            Assert.IsFalse(0.IsNotNew());
            Assert.IsTrue(5.IsNotNew());
        }
    }
}
