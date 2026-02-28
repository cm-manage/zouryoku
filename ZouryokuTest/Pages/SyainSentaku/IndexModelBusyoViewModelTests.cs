using Model.Model;
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
            var busyo = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };

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
            var busyo = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };

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
            var busyo = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };

            // Act
            var viewModel = new BusyoViewModel(busyo);

            // Assert
            Assert.AreEqual(1, viewModel.Jyunjyo);
        }

        /// <summary>
        /// 正常系：親ID取得
        /// </summary>
        [TestMethod]
        public void BusyoViewModel_正常系_親ID取得()
        {
            // Arrange
            var busyo = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = 2,
                ShoninBusyoId = null
            };

            // Act
            var viewModel = new BusyoViewModel(busyo);

            // Assert
            Assert.AreEqual(2, viewModel.OyaId);
        }
    }
}
