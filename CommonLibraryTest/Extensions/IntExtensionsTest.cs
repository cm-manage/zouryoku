using CommonLibrary.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class IntExtensionsTest
    {
        [TestMethod]
        public void ToYear_Test()
        {
            Assert.AreEqual(2024, 202410.ToYear());
            Assert.AreEqual(2024, 20241015.ToYear());
            Assert.IsNull(123.ToYear());
        }
    }
}
