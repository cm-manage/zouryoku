using CommonLibrary.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class DecimalExtensionsTest
    {
        [TestMethod]
        public void ToInt_Nullable()
        {
            decimal? v = 12.0m;
            Assert.AreEqual(12, v.ToInt());
            v = null;
            Assert.IsNull(v.ToInt());
        }

        [TestMethod]
        public void ToInt()
        {
            decimal v = 99.0m;
            Assert.AreEqual(99, v.ToInt());
        }
    }
}
