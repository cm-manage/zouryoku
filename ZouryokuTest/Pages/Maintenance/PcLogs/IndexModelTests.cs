using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zouryoku.Pages.Maintenance.PcLogs;
using ZouryokuTest;
using Model.Model;

namespace ZouryokuTest.Pages.Maintenance.PcLogs
{
    /// <summary>
    /// IndexModel (PCログ一覧ページ) のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {
        private IndexModel CreateModel()
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options);
            model.PageContext = GetPageContext();
            model.TempData = GetTempData();
            return model;
        }

        /// <summary>
        /// 正常: レコードが存在する場合 PcLog 一覧が取得されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_Populates_List_When_Records_Exist()
        {
            // Arrange
            var syain = new Syain
            {
                Id = 1,
                Name = "社員A",
                Code = "000001",
                BusyoCode = "001",
                KanaName = "シャインエー",
                KingsSyozoku = "00001"
            };
            db.Syains.Add(syain);
            db.PcLogs.Add(new PcLog { Datetime = System.DateTime.UtcNow.AddMinutes(-5), PcName = "PC-1", UserName = "user1", SyainId = syain.Id });
            db.PcLogs.Add(new PcLog { Datetime = System.DateTime.UtcNow.AddMinutes(-1), PcName = "PC-2", UserName = "user2", SyainId = syain.Id });
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.PcLog, "PcLog プロパティが設定されていません。");
            Assert.HasCount(2, model.PcLog, "PcLog 一覧の件数が一致しません。");
            Assert.AreEqual("PC-1", model.PcLog[0].PcName, "1件目の PcName が一致しません。");
            Assert.IsNotNull(model.PcLog[0].Syain, "Include により Syain が読み込まれているべきです。");
        }

        /// <summary>
        /// 正常: レコードが存在しない場合 PcLog 一覧が空となること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_Sets_Empty_List_When_No_Records()
        {
            // Arrange
            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.PcLog, "PcLog プロパティが nullです。");
            Assert.IsEmpty(model.PcLog, "空のはずの PcLog 一覧に要素があります。");
        }
    }
}
