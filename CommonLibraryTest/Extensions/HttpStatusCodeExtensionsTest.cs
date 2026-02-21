using System.Net;
using CommonLibrary.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class HttpStatusCodeExtensionsTest
    {
        [TestMethod]
        public void ToInt_Test()
        {
            Assert.AreEqual(200, HttpStatusCode.OK.ToInt());
            Assert.AreEqual(404, HttpStatusCode.NotFound.ToInt());
        }
    }
}
