using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.KokyakuMeiKensaku
{
    /// <summary>
    /// BindPropertyに関するテスト
    /// </summary>
    [TestClass]
    public class IndexModelValidationTests : IndexModelTestBase
    {
        // ======================================
        // エラーメッセージ
        // ======================================

        /// <summary>
        /// 顧客名の必須チェック違反時
        /// </summary>
        private readonly string _errorMessageCustomerNameRequired = string.Format(ErrorRequired, "顧客名");

        // ======================================
        // テストメソッド
        // ======================================

        /// <summary>
        /// 正常系：チェック違反が無い場合にバリデーションエラーが起きないこと
        /// </summary>
        [TestMethod]
        public void CustomerName_チェックに違反しないとき_バリデーションエラーにならない()
        {
            // Arrange
            var model = CreateModel();
            model.CustomerName = "テスト";

            // Act
            var (isValid, _) = ValidateModel(model);

            // Assert
            Assert.IsTrue(isValid);
        }

        /// <summary>
        /// 異常系：顧客名の必須チェック
        /// </summary>
        /// <param name="input">顧客名</param>
        [TestMethod]
        [DataRow(null, DisplayName = "NULLのとき")]
        [DataRow("", DisplayName = "空文字のとき")]
        public void CustomerName_必須チェックに違反_バリデーションエラーになる(string? input)
        {
            // Arrange
            var model = CreateModel();
            model.CustomerName = input;

            // Act
            var (isValid, results) = ValidateModel(model);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual(_errorMessageCustomerNameRequired, results.Single().ErrorMessage);
        }
    }
}
