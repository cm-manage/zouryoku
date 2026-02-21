using System;
using System.Collections.Generic;
using System.Linq;
using CommonLibrary.Extensions;
using LanguageExt;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class IEnumerableExtensionsTest
    {
        [TestMethod]
        public void FirstLastOption()
        {
            var list = new[] { 1, 2, 3 };
            Assert.AreEqual(1, list.FirstOption().IfNone(0));
            Assert.AreEqual(3, list.LastOption().IfNone(0));
            Assert.AreEqual(2, list.FirstOption(x => x == 2).IfNone(0));
            Assert.AreEqual(3, list.LastOption(x => x >= 3).IfNone(0));
        }

        [TestMethod]
        public void MaxMinOption()
        {
            var list = new[] { 1, 5, 3 };
            Assert.AreEqual(5, list.MaxOption(x => x).IfNone(0));
            Assert.AreEqual(1, list.MinOption(x => x).IfNone(0));
            var empty = Array.Empty<int>();
            Assert.IsTrue(empty.MaxOption(x => x).IsNone);
        }

        [TestMethod]
        public void OriginalChunk_Test()
        {
            var chunks = Enumerable.Range(1, 5).OriginalChunk(2).Select(c => c.ToList()).ToList();
            Assert.AreEqual(3, chunks.Count);
            CollectionAssert.AreEqual(new[] { 1, 2 }, chunks[0]);
            CollectionAssert.AreEqual(new[] { 3, 4 }, chunks[1]);
            CollectionAssert.AreEqual(new[] { 5 }, chunks[2]);
        }

        [TestMethod]
        public void ForEach_Test()
        {
            var list = new List<int> { 1, 2, 3 };
            var sum = 0;
            list.ForEach(x => sum += x);
            Assert.AreEqual(6, sum);
        }

        [TestMethod]
        public void ToList_Array_IEnumerator()
        {
            var array = new[] { 1, 2 };
            var list = array.Cast<int>().ToList();
            CollectionAssert.AreEqual(array, list);
        }

        [TestMethod]
        public void IndexOption_Test()
        {
            var list = new[] { "a", "b" };
            Assert.AreEqual("b", list.IndexOption(1).IfNone("x"));
            Assert.IsTrue(list.IndexOption(5).IsNone);
        }

        [TestMethod]
        public void ZipWithIndex_Test()
        {
            var list = new[] { "x", "y" };
            var zipped = list.ZipWithIndex(10).ToList();
            Assert.AreEqual("x", zipped[0].Item1);
            Assert.AreEqual(10, zipped[0].Item2);
        }

        [TestMethod]
        public void AddReturn_Test()
        {
            var list = new List<int>();
            var ret = list.AddReturn(5);
            Assert.AreEqual(5, ret);
            CollectionAssert.AreEqual(new[] { 5 }, list);
        }

        [TestMethod]
        public void GridSortOrder_Test()
        {
            var data = new[] { 3, 1, 2 };
            var asc = data.GridSortOrder(AscOrDesc.Asc)(x => x).ToList();
            var desc = data.GridSortOrder(AscOrDesc.Desc)(x => x).ToList();
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, asc);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, desc);
        }

        [TestMethod]
        public void GridLimitOffset_Test()
        {
            var data = Enumerable.Range(1, 10);
            var limited = data.GridLimitOffset(2, 3);
            CollectionAssert.AreEqual(new[] { 3, 4, 5 }, limited);
        }

        [TestMethod]
        public void Empty_NotEmpty_Test()
        {
            IEnumerable<int>? nullList = null;
            Assert.IsTrue(nullList.Empty());
            Assert.IsFalse(nullList.NotEmpty());
            var list = new[] { 1 };
            Assert.IsFalse(list.Empty());
            Assert.IsTrue(list.NotEmpty());
        }

        [TestMethod]
        public void Join_Test()
        {
            var list = new object?[] { "a", null, "b" };
            Assert.AreEqual("a,b", list.Join(","));
            Assert.AreEqual("", Array.Empty<string>().Join());
        }

        [TestMethod]
        public void ToListOption_ToSingleOption_Test()
        {
            var list = new[] { 5 };
            Assert.AreEqual(5, list.ToListOption().IfNone(new List<int>()).First());
            Assert.AreEqual(5, list.ToSingleOption().IfNone(0));
            var many = new[] { 1, 2 };
            Assert.IsTrue(many.ToSingleOption().IsNone);
        }

        [TestMethod]
        public void SumNullable_Test()
        {
            int?[] list = { 1, null, 2 };
            Assert.AreEqual(3, list.SumNullable());
            int?[] allNull = { null, null };
            Assert.IsNull(allNull.SumNullable());
            var sel = new[] { new { V = (int?)4 }, new { V = (int?)null } };
            Assert.AreEqual(4, sel.SumNullable(x => x.V));
        }

        [TestMethod]
        public void Buffer_Test()
        {
            var src = Enumerable.Range(1, 5);
            var buffered = src.Buffer(2).Select(b => b.ToList()).ToList();
            CollectionAssert.AreEqual(new[] { 1, 2 }, buffered[0]);
            CollectionAssert.AreEqual(new[] { 3, 4 }, buffered[1]);
            CollectionAssert.AreEqual(new[] { 5 }, buffered[2]);
        }
    }
}
