using Zouryoku.Pages.SyainSentaku;

namespace ZouryokuTest.Pages.SyainSentaku
{
    /// <summary>
    /// SyainViewModelのテスト
    /// </summary>
    [TestClass]
    public class IndexModelSyainViewModelTests : IndexModelTestsBase
    {
        /// <summary>
        /// 正常系：社員IDを取得
        /// </summary>
        [TestMethod]
        public void SyainViewModel_正常系_社員Id取得()
        {
            // Arrange
            var syain = AddSyain(10, "社員1", "01", null, true, 1, 2);

            // Act
            var viewModel = new SyainViewModel(syain);

            // Assert
            Assert.AreEqual(10, viewModel.Id);
        }

        /// <summary>
        /// 正常系：社員名取得
        /// </summary>
        [TestMethod]
        public void SyainViewModel_正常系_社員名取得()
        {
            // Arrange
            var syain = AddSyain(10, "社員1", "01", null, true, 1, 2);

            // Act
            var viewModel = new SyainViewModel(syain);

            // Assert
            Assert.AreEqual("社員1", viewModel.Name);
        }

        /// <summary>
        /// 正常系：部署ID取得
        /// </summary>
        [TestMethod]
        public void SyainViewModel_正常系_部署Id取得()
        {
            // Arrange
            var syain = AddSyain(10, "社員1", "01", null, true, 1, 2);

            // Act
            var viewModel = new SyainViewModel(syain);

            // Assert
            Assert.AreEqual(2, viewModel.BusyoId);
        }

        /// <summary>
        /// 正常系：社員BaseId取得
        /// </summary>
        [TestMethod]
        public void SyainViewModel_正常系_社員BaseId取得()
        {
            // Arrange
            var syain = AddSyain(10, "社員1", "01", null, true, 1, 2);

            // Act
            var viewModel = new SyainViewModel(syain);

            // Assert
            Assert.AreEqual(1, viewModel.SyainBaseId);
        }

        /// <summary>
        /// 正常系：社員番号取得
        /// </summary>
        [TestMethod]
        public void SyainViewModel_正常系_社員番号取得()
        {
            // Arrange
            var syain = AddSyain(10, "社員1", "01", null, true, 1, 2);

            // Act
            var viewModel = new SyainViewModel(syain);

            // Assert
            Assert.AreEqual("01", viewModel.Code);
        }

        /// <summary>
        /// 正常系：退職フラグ取得
        /// </summary>
        [TestMethod]
        public void SyainViewModel_正常系_退職フラグ取得()
        {
            // Arrange
            var syain = AddSyain(10, "社員1", "01", null, true, 1, 2);

            // Act
            var viewModel = new SyainViewModel(syain);

            // Assert
            Assert.IsTrue(viewModel.Retired);
        }
    }
}
