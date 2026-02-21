using Zouryoku.Pages.YukyuKeikakuToroku;
using Zouryoku.Utils;

namespace ZouryokuTest.Pages.YukyuKeikakuToroku
{
    /// <summary>
    /// 計画有給休暇登録画面ViewModelのユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelViewModelTests : BaseInMemoryDbContextTest
    {
        /// <summary>
        /// 異常: #27 単項目バリデーション 休暇予定日が1日分以上未入力の場合
        /// </summary>
        [TestMethod(DisplayName = "#27 単項目バリデーション 休暇予定日が1日分以上未入力の場合")]
        public void MeisaiValidation_休暇予定日が1日分以上未入力の場合()
        {
            // Arrange
            var model = new IndexModel.Meisai
            {
                Ymd = null,
                IsTokukyu = true,
            };

            // Act
            var (isValid, results) = ValidateModel(model);

            // Assert
            Assert.IsFalse(isValid, "バリデーションは失敗するべきです。");
            Assert.AreEqual(
                string.Format(Const.ErrorRequired, "休暇予定日"), results[0].ErrorMessage,
                "メッセージが正しくありません。");
        }
    }
}
