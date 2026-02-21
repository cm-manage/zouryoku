using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTest.Extensions
{
    public enum HtmlTestEnum
    {
        [Display(Name = "有効")]
        Enable = 0,
        Disable = 1,
    }


    [TestClass]
    public class IHtmlHelperExtensionsTest
    {
        [TestMethod]
        public void GetEnumSelectList_Filter()
        {
            IHtmlHelper? helper = null; // 拡張メソッド内部で htmlHelper を使用しないため null で呼び出し
            var list = helper.GetEnumSelectList<HtmlTestEnum>(e => e == HtmlTestEnum.Enable).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("有効", list[0].Text);
            Assert.AreEqual("0", list[0].Value);
        }
    }
}
