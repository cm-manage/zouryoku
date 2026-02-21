using System.Collections.Generic;
using CommonLibrary.Extensions;
using LanguageExt;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class IDictionaryExtensionsTest
    {
        [TestMethod]
        public void Get_And_GetOrElse()
        {
            var dic = new Dictionary<string, int> { { "a", 1 } };
            Assert.AreEqual(1, dic.Get("a").IfNone(0));
            Assert.AreEqual(0, dic.Get("b").IfNone(0));
            Assert.AreEqual(1, dic.GetOrElse("a", 99));
            Assert.AreEqual(99, dic.GetOrElse("b", 99));
            Assert.AreEqual(5, dic.GetOrElse("c", () => 5));
        }

        [TestMethod]
        public void GetOrNull_Variants()
        {
            var dicClass = new Dictionary<string, string> { { "x", "y" } };
            Assert.AreEqual("y", dicClass.GetOrNull("x"));
            Assert.IsNull(dicClass.GetOrNull("z"));

            var dicStruct = new Dictionary<string, int> { { "a", 10 } };
            Assert.AreEqual(10, dicStruct.GetOrNullReference("a"));
            Assert.IsNull(dicStruct.GetOrNullReference("b"));

            var dicNullableStruct = new Dictionary<string, int?> { { "k", 3 }, { "n", null } };
            Assert.AreEqual(3, dicNullableStruct.GetOrNullReference("k"));
            Assert.IsNull(dicNullableStruct.GetOrNullReference("n"));
        }
    }
}
