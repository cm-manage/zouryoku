using CommonLibrary.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPOI.XSSF.UserModel;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class NPOIExpandTest
    {
        [TestMethod]
        public void WriteAndReadCell_Test()
        {
            var book = new XSSFWorkbook();
            var sheet = book.CreateSheet("s");
            sheet.WriteCell(0, 0, "abc");
            sheet.WriteCell(1, 0, 123.0);
            Assert.AreEqual("abc", sheet.GetCellValue(0, 0));
            Assert.AreEqual("123", sheet.GetCellValue(1, 0));
        }
    }
}
