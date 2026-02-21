using Zouryoku.Pages.SyainSentaku;

namespace ZouryokuTest.Pages.SyainSentaku
{
    [TestClass]
    public class IndexModelValidationTests : IndexModelTestsBase
    {
        /// <summary>
        /// 正常系：入力時、エラーにならない
        /// </summary>
        [TestMethod]
        public void SyainName_チェックに違反しないとき_バリデーションエラーにならない()
        {
            // Arrange
            var model = CreateModel();
            model.SyainName = "社員名";

            // Act
            var (isValid, _) = ValidateModel(model);

            // Assert
            Assert.IsTrue(isValid);
        }

        /// <summary>
        /// 正常系：選択時、エラーにならない
        /// </summary>
        [TestMethod]
        public void SelectCounts_0以上の場合_バリデーションエラーにならない()
        {
            // Arrange
            var input = new IndexModel.ValidateSelectionRequest
            {
                SelectCounts = 1
            };

            // Act
            var (isValid, _) = ValidateModel(input);

            // Assert
            Assert.IsTrue(isValid);
        }

        /// <summary>
        /// 異常系：検索欄未入力時、エラーになる
        /// </summary>
        /// <param name="input"></param>
        [TestMethod]
        [DataRow(null, DisplayName = "NULLのとき")]
        [DataRow("", DisplayName = "空文字のとき")]
        public void SyainName_必須チェックに違反_バリデーションエラーメッセージが表示される(string? input)
        {
            // Arrange
            var model = CreateModel();
            model.SyainName = input;

            // Act
            var (isValid, results) = ValidateModel(model);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual(ErrorMsgSyainNameRequired, results.Single().ErrorMessage);
        }

        /// <summary>
        /// 異常系：未選択時、エラーになる
        /// </summary>
        [TestMethod]
        public void SelectCounts_0の場合_バリデーションエラーメッセージが表示される()
        {
            // Arrange
            var input = new IndexModel.ValidateSelectionRequest
            {
                SelectCounts = 0
            };
            // Act
            var (isValid, results) = ValidateModel(input);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual(ErrorMsgSyainSelectRequired, results.Single().ErrorMessage);
        }
    }
}
