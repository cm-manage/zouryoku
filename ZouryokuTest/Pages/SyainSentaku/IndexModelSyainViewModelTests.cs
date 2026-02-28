using Model.Model;
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
            var syain = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };

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
            var syain = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };

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
            var syain = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };

            // Act
            var viewModel = new SyainViewModel(syain);

            // Assert
            Assert.AreEqual(1, viewModel.BusyoId);
        }

        /// <summary>
        /// 正常系：社員BaseId取得
        /// </summary>
        [TestMethod]
        public void SyainViewModel_正常系_社員BaseId取得()
        {
            // Arrange
            var syain = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };

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
            var syain = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };

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
            var syain = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = true,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };

            // Act
            var viewModel = new SyainViewModel(syain);

            // Assert
            Assert.IsTrue(viewModel.Retired);
        }
    }
}
