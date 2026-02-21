using System.Collections.Generic;
using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class HttpRequestExtensionsTest
    {
        [TestMethod]
        public void GridParameter_PostMethod()
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Method = "POST";
            var form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "pageIndex", "2" },
                { "pageSize", "10" },
                { "sortField", "Name" },
                { "sortOrder", "desc" }
            });
            ctx.Features.Set<IFormFeature>(new FormFeature(form));

            var param = ctx.Request.GridParameter();
            Assert.AreEqual(2, param.PageIndex);
            Assert.AreEqual(10, param.PageSize);
            Assert.AreEqual(10, param.Start);
            Assert.AreEqual("Name", param.SortField);
            Assert.AreEqual(AscOrDesc.Desc, param.SortOrder);
        }
    }
}
