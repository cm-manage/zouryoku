using Zouryoku.Pages.SyainSentaku;

namespace ZouryokuTest.Pages.SyainSentaku
{
    /// <summary>
    /// BusyoModelのテスト
    /// </summary>
    [TestClass]
    public class IndexModelBusyoViewModelTests : IndexModelTestsBase
    {
        /// <summary>
        /// 正常系：部署ID取得
        /// </summary>
        [TestMethod]
        public void BusyoViewModel呼び出し_正常系_部署Id取得()
        {
            // Arrange
            var busyo = AddBusyo(1, "部署1", 1, true);

            // Act
            var viewModel = new BusyoViewModel(busyo);

            // Assert
            Assert.AreEqual(1, viewModel.Id);
        }

        /// <summary>
        /// 正常系：部署名取得
        /// </summary>
        [TestMethod]
        public void BusyoViewModel_正常系_部署名称取得()
        {
            // Arrange
            var busyo = AddBusyo(1, "部署1", 1, true);

            // Act
            var viewModel = new BusyoViewModel(busyo);

            // Assert
            Assert.AreEqual("部署1", viewModel.Name);
        }

        /// <summary>
        /// 正常系：並び順序取得
        /// </summary>
        [TestMethod]
        public void BusyoViewModel_正常系_並び順序取得()
        {
            // Arrange
            var busyo = AddBusyo(1, "部署1", 1, true);

            // Act
            var viewModel = new BusyoViewModel(busyo);

            // Assert
            Assert.AreEqual(1, viewModel.Jyunjyo);
        }

        /// <summary>
        /// 正常系：親ID取得
        /// </summary>
        /// <param name="oyaId"></param>
        [TestMethod]
        public void BusyoViewModel_正常系_親ID取得()
        {
            // Arrange
            var busyo = AddBusyo(1, "部署1", 1, true, 2);

            // Act
            var viewModel = new BusyoViewModel(busyo);

            // Assert
            Assert.AreEqual(2, viewModel.OyaId);
        }
    }
}
