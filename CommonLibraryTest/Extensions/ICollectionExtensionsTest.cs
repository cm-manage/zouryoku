using System.Collections.Generic;
using CommonLibrary.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class ICollectionExtensionsTest
    {
        [TestMethod]
        public void AddRange_Test()
        {
            var list = new List<int> { 1 };
            list.AddRange(new[] { 2, 3 });
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
        }
    }
}
