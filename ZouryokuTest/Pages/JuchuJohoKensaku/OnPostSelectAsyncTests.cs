using Zouryoku.Pages.JuchuJohoKensaku;
using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.JuchuJohoKensaku
{
    /// <summary>
    /// <see cref="IndexModel.OnPostSelectAsync"/>のテストクラス
    /// </summary>
    [TestClass]
    public class OnPostSelectAsyncTests : IndexModelTestBase
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

        //  KINGS受注登録存在チェック
        // --------------------------------------

        /// <summary>
        /// KINGS受注ID 不一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostSelectAsync_選択したKINGS受注登録情報が存在しない_エラー()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            // Act
            var response = await Model!.OnPostSelectAsync(2);

            // Assert
            AssertError(response, ErrorSelectedDataNotExists);
        }

        // 共通処理呼び出しの確認
        // --------------------------------------

        /// <summary>
        /// 共通仕様_受注参照履歴登録の呼び出し確認
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostSelectAsync_受注参照履歴更新の共通処理が呼ばれている()
        {
            // Arrange
            AddKingsJuchus(50);
            // KINGS受注登録のみ51件目登録
            AddKingsJuchu(id: 51, rirekiInsert: false);
            db.SaveChanges();

            // Act
            await Model!.OnPostSelectAsync(51);

            // Assert
            // 追加されていること
            var isExist = db.KingsJuchuSansyouRirekis
                .Any(x => x.Id == 51);
            Assert.IsTrue(isExist);
            // 超過データ削除が行われていること
            var count = db.KingsJuchuSansyouRirekis.ToList().Count;
            Assert.AreEqual(50, count);
        }
    }
}