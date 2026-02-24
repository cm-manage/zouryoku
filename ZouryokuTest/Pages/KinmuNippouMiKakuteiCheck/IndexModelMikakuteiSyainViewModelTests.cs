using Model.Model;
using static Model.Enums.DailyReportStatusClassification;
using static Zouryoku.Pages.KinmuNippouMiKakuteiCheck.IndexModel;

namespace ZouryokuTest.Pages.KinmuNippouMiKakuteiCheck
{
    /// <summary>
    /// <see cref="MikakuteiSyainViewModel"/>のテストクラス。
    /// </summary>
    [TestClass]
    public class IndexModelMikakuteiSyainViewModelTests
    {
        // ======================================
        // テスト
        // ======================================

        [TestMethod]
        public void MikakuteiSyainViewModel_部署名を取得すること()
        {
            // Arrange
            // ----------------------------------

            var syain = new Syain()
            {
                Busyo = new()
                {
                    Name = "部署名称",
                },
            };

            // Act
            // ----------------------------------

            var viewModel = new MikakuteiSyainViewModel(syain);

            // Arrange
            // ----------------------------------

            Assert.AreEqual(syain.Busyo.Name, viewModel.BusyoName);
        }

        [TestMethod]
        public void MikakuteiSyainViewModel_社員番号を取得すること()
        {
            // Arrange
            // ----------------------------------

            var syain = new Syain()
            {
                Code = "12345",
            };

            // Act
            // ----------------------------------

            var viewModel = new MikakuteiSyainViewModel(syain);

            // Arrange
            // ----------------------------------

            Assert.AreEqual(syain.Code, viewModel.SyainCode);
        }

        [TestMethod]
        public void MikakuteiSyainViewModel_社員氏名を取得すること()
        {
            // Arrange
            // ----------------------------------

            var syain = new Syain()
            {
                Name = "社員氏名",
            };

            // Act
            // ----------------------------------

            var viewModel = new MikakuteiSyainViewModel(syain);

            // Arrange
            // ----------------------------------

            Assert.AreEqual(syain.Name, viewModel.SyainName);
        }

        [TestMethod]
        [DataRow(true, DisplayName = "空のとき")]
        [DataRow(false, DisplayName = "すべて未確定のとき")]
        public void MikakuteiSyainViewModel_日報が空またはすべて未確定のとき_最終確定日がnullとなること(bool isEmpty)
        {
            // Arrange
            // ----------------------------------

            var nippous = new List<Nippou>();

            if (!isEmpty)
            {
                nippous.AddRange([
                    new()
                    {
                        NippouYmd = new(2026, 2, 15),
                        TourokuKubun = 一時保存,
                    },
                ]);
            }

            var syain = new Syain()
            {
                Nippous = nippous,
            };

            // Act
            // ----------------------------------

            var viewModel = new MikakuteiSyainViewModel(syain);

            // Arrange
            // ----------------------------------

            Assert.IsNull(viewModel.LastKakuteiYmd);
        }

        [TestMethod]
        public void MikakuteiSyainViewModel_確定保存の日報が存在する_最終確定日が確定保存日報の実績年月日の最大値()
        {
            // Arrange
            // ----------------------------------

            var expectedNippouYmd = new DateOnly(2026, 2, 15);
            var syain = new Syain()
            {
                Nippous = [
                    new()
                    {
                        NippouYmd = expectedNippouYmd,
                        TourokuKubun = 確定保存
                    },
                    new()
                    {
                        NippouYmd = expectedNippouYmd.AddDays(-1),
                        TourokuKubun = 確定保存,
                    }
                ],
            };

            // Act
            // ----------------------------------

            var viewModel = new MikakuteiSyainViewModel(syain);

            // Arrange
            // ----------------------------------

            Assert.AreEqual(expectedNippouYmd, viewModel.LastKakuteiYmd);
        }
    }
}