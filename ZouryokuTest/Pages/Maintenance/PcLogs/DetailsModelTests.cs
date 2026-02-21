using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zouryoku.Pages.Maintenance.PcLogs;
using ZouryokuTest;
using Model.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages; // PageResult 型参照

namespace ZouryokuTest.Pages.Maintenance.PcLogs
{
    /// <summary>
    /// DetailsModel (PCログ詳細ページ) のユニットテスト
    /// </summary>
    [TestClass]
    public class DetailsModelTests : BaseInMemoryDbContextTest
    {
        private DetailsModel CreateModel()
        {
            var model = new DetailsModel(db, GetLogger<DetailsModel>(), options);
            model.PageContext = GetPageContext();
            model.TempData = GetTempData();
            return model;
        }

        /// <summary>
        /// 正常: 対象IDのレコードが存在する場合 PageResult が返り PcLog が設定されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_Returns_Page_When_Record_Exists()
        {
            // Arrange
            var existing = new PcLog
            {
                Datetime = System.DateTime.UtcNow,
                PcName = "PC-DETAIL",
                UserName = "detail-user"
            };
            db.PcLogs.Add(existing);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync(existing.Id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult), "レコードが存在する場合は PageResult が返るべきです。");
            Assert.IsNotNull(model.PcLog, "PcLog が設定されていません。");
            Assert.AreEqual(existing.Id, model.PcLog.Id);
            Assert.AreEqual("PC-DETAIL", model.PcLog.PcName);
        }

        /// <summary>
        /// 異常: IDが null の場合 NotFoundResult が返ること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_Returns_NotFound_When_Id_Is_Null()
        {
            // Arrange
            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult), "ID未指定時は NotFoundResult が返るべきです。");
        }

        /// <summary>
        /// 異常: 指定IDのレコードが存在しない場合 NotFoundResult が返ること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_Returns_NotFound_When_Record_Not_Found()
        {
            // Arrange
            var model = CreateModel();
            var notExistsId =9999L; // 存在しないID

            // Act
            var result = await model.OnGetAsync(notExistsId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult), "存在しないID指定時は NotFoundResult が返るべきです。");
            Assert.IsNull(model.PcLog, "レコード未取得時に PcLog が設定されてはいけません。");
        }
    }
}
