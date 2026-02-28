using Zouryoku.Pages.JuchuJohoKensaku;
using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.JuchuJohoKensaku
{
    /// <summary>
    /// <see cref="IndexModel.OnGetCheckExistenceAsync"/>のテストクラス
    /// </summary>
    [TestClass]
    public class OnGetCheckExistenceAsyncTests : IndexModelTestBase
    {
        // ======================================
        // テストの初期化処理
        // ======================================

        /// <summary>
        /// IndexModelを作成する。
        /// </summary>
        [TestInitialize]
        public void TestInit()
        {
            Model = CreateModel();
        }

        // ======================================
        // テストメソッド
        // ======================================

        // KINGS受注登録存在チェック
        // --------------------------------------

        /// <summary>
        /// KINGS受注登録に該当データあり
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetCheckExistenceAsync_受注登録情報が存在する_正常()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            // Act
            var response = await Model!.OnGetCheckExistenceAsync(1);

            // Assert
            AssertSuccess(response);
        }

        /// <summary>
        /// KINGS受注登録に該当データなし
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetCheckExistenceAsync_受注登録情報が存在しない_エラー()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            // Act
            var response = await Model!.OnGetCheckExistenceAsync(2);

            // Assert
            AssertError(response, ErrorSelectedDataNotExists);
        }
    }
}
