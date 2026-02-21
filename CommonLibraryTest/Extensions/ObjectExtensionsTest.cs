using CommonLibrary.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class ObjectExtensionsTest
    {
        public class Sample
        {
            public int A { get; set; } = 1;
            public string B { get; set; } = "b";
        }

        [TestMethod]
        public void ToJson_Test()
        {
            var s = new Sample();
            var json = s.ToJson();
            Assert.IsTrue(json.Contains("\"A\":1"));
            Assert.IsTrue(json.Contains("\"B\":\"b\""));
        }
    }
}
