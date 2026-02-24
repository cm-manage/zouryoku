using CommonLibrary.Extensions;
using Model.Model;
using static Zouryoku.Pages.KinmuNippouMiKakuteiCheck.NotifyModel;
using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.KinmuNippouMiKakuteiCheck
{
    /// <summary>
    /// <see cref="MikakuteiTsuchiRirekiViewModel"/>のテストクラス。
    /// </summary>
    [TestClass]
    public class NotifyModelMikakuteiTsuchiViewModelTests
    {
        // ======================================
        // テスト
        // ======================================

        [TestMethod]
        public void MikakuteiTsuchiRirekiViewModel_送信日時_取得している()
        {
            // Arrange
            // ----------------------------------

            var expectedSendDateTime = new DateTime(2026, 2, 15, 9, 0, 0);
            var rireki = new MikakuteiTsuchiRireki()
            {
                TuutiSousinNitizi = expectedSendDateTime,
            };

            // Act
            // ----------------------------------

            var viewModel = new MikakuteiTsuchiRirekiViewModel(rireki);

            // Assert
            // ----------------------------------

            Assert.AreEqual(expectedSendDateTime, viewModel.SendDateTime);
        }

        [TestMethod]
        public void MikakuteiTsuchiRirekiViewModel_社員氏名_取得している()
        {
            // Arrange
            // ----------------------------------

            var expectedSyainName = "社員氏名";
            var rireki = new MikakuteiTsuchiRireki()
            {
                SendSyainBase = new()
                {
                    Syains = [new() { Name = expectedSyainName }],
                },
            };

            // Act
            // ----------------------------------

            var viewModel = new MikakuteiTsuchiRirekiViewModel(rireki);

            // Assert
            // ----------------------------------

            Assert.AreEqual(expectedSyainName, viewModel.SyainName);
        }

        [TestMethod]
        public void MikakuteiTsuchiRirekiViewModel_送信先の社員数_取得している()
        {
            // Arrange
            // ----------------------------------

            var expecteCount = 5;
            var rireki = new MikakuteiTsuchiRireki()
            {
                SyainTsuchiRirekiRels = [.. Enumerable
                .Range(0, expecteCount)
                .Select(static i => new SyainTsuchiRirekiRel())]
            };

            // Act
            // ----------------------------------

            var viewModel = new MikakuteiTsuchiRirekiViewModel(rireki);

            // Assert
            // ----------------------------------

            Assert.AreEqual(expecteCount, viewModel.SendCount);
        }

        [TestMethod]
        public void MikakuteiTsuchiRirekiViewModel_表示用文章_取得している()
        {
            // Arrange
            // ----------------------------------

            var sendDateTime = new DateTime(2026, 2, 15, 9, 0, 0);
            var syainName = "社員氏名";
            var count = 5;
            var rireki = new MikakuteiTsuchiRireki()
            {
                TuutiSousinNitizi = sendDateTime,
                SendSyainBase = new()
                {
                    Syains = [new() { Name = syainName }],
                },
                SyainTsuchiRirekiRels = [.. Enumerable
                .Range(0, count)
                .Select(static i => new SyainTsuchiRirekiRel())]
            };

            // Act
            // ----------------------------------

            var viewModel = new MikakuteiTsuchiRirekiViewModel(rireki);

            // Assert
            // ----------------------------------

            var expectedMessage = NippouMikakuteiTsuchiSendHistoryStr.Format(sendDateTime.ToString("MM/dd"), syainName, count);
            Assert.AreEqual(expectedMessage, viewModel.RirekiDisplay);
        }
    }
}
