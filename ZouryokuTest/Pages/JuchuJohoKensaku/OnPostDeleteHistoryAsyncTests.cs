using Model.Model;
using Zouryoku.Pages.JuchuJohoKensaku;
using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.JuchuJohoKensaku
{
    /// <summary>
    /// <see cref="IndexModel.OnPostDeleteHistoryAsync"/>のテストクラス
    /// </summary>
    [TestClass]
    public class OnPostDeleteHistoryAsyncTests : IndexModelTestBase
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
        // データ登録
        // ======================================

        /// <summary>
        /// 「参照履歴削除」確認用データ登録
        /// </summary>
        /// <param name="toBeDeleted">削除対象データを追加するフラグ デフォルトtrue</param>
        private void AddForDeleteRireki(bool toBeDeleted = true)
        {
            AddKingsJuchu(id: 1, syainBaseId: 888);
            AddKingsJuchu(id: 2);

            if (toBeDeleted)
            {
                var rireki = new KingsJuchuSansyouRireki
                {
                    Id = 3,
                    SyainBaseId = 111,
                    SansyouTime = new DateTime(2026, 1, 1, 0, 0, 01),
                    KingsJuchuId = 1
                };
                db.Add(rireki);
            }
        }

        // ======================================
        // テストメソッド
        // ======================================

        //  KINGS受注参照履歴存在チェック
        // --------------------------------------

        /// <summary>
        /// 社員BaseID 不一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostDeleteHistoryAsync_社員BaseID不一致_エラー()
        {
            // Arrange
            AddForDeleteRireki(toBeDeleted: false);
            db.SaveChanges();

            // Act
            var result = await Model!.OnPostDeleteHistoryAsync(1, 1);

            // Assert
            AssertErrorJson(result, ErrorSelectedDataNotExists);
        }

        /// <summary>
        /// KINGS受注ID 不一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostDeleteHistoryAsync_KINGS受注ID不一致_エラー()
        {
            // Arrange
            AddForDeleteRireki();
            db.SaveChanges();

            // Act
            var result = await Model!.OnPostDeleteHistoryAsync(3, 1);

            // Assert
            AssertErrorJson(result, ErrorSelectedDataNotExists);
        }

        //  KINGS受注参照履歴削除処理
        // --------------------------------------

        /// <summary>
        /// 削除処理で対象以外が削除されない
        /// ①KINGS受注参照履歴.社員BaseID＝ログインユーザー.社員BaseID を満たす
        /// ②KINGS受注参照履歴.KINGS受注ID＝パラメータ.KINGS受注ID を満たさない
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostDeleteHistoryAsync_社員BaseID一致KINGS受注ID不一致_削除されない()
        {
            // Arrange
            AddForDeleteRireki();
            db.SaveChanges();

            // 削除対象のデータのバージョンを取得
            var version = db.KingsJuchuSansyouRirekis
                .Single(j => j.Id == 1)
                .Version;

            // Act
            await Model!.OnPostDeleteHistoryAsync(1, version);

            // Assert
            var isExist = db.KingsJuchuSansyouRirekis
                .Any(x => x.Id == 1);
            Assert.IsTrue(isExist);
        }

        /// <summary>
        /// 削除処理で対象以外が削除されない
        /// ①KINGS受注参照履歴.社員BaseID＝ログインユーザー.社員BaseID を満たさない
        /// ②KINGS受注参照履歴.KINGS受注ID＝パラメータ.KINGS受注ID を満たす
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostDeleteHistoryAsync_社員BaseID不一致KINGS受注ID一致_削除されない()
        {
            // Arrange
            AddForDeleteRireki();
            db.SaveChanges();

            // 削除対象のデータのバージョンを取得
            var version = db.KingsJuchuSansyouRirekis
                .Single(j => j.Id == 2)
                .Version;

            // Act
            await Model!.OnPostDeleteHistoryAsync(1, version);

            // Assert
            var isExist = db.KingsJuchuSansyouRirekis
                .Any(x => x.Id == 2);
            Assert.IsTrue(isExist);
        }

        /// <summary>
        /// 削除処理
        /// ①KINGS受注参照履歴.社員BaseID＝ログインユーザー.社員BaseID を満たす
        /// ②KINGS受注参照履歴.KINGS受注ID＝パラメータ.KINGS受注ID を満たす
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostDeleteHistoryAsync_社員BaseID一致KINGS受注ID一致_削除()
        {
            // Arrange
            AddForDeleteRireki();
            db.SaveChanges();

            // 削除対象のデータのバージョンを取得
            var version = db.KingsJuchuSansyouRirekis
                .Single(j => j.Id == 3)
                .Version;

            // Act
            await Model!.OnPostDeleteHistoryAsync(1, version);

            // Assert
            var isExist = db.KingsJuchuSansyouRirekis
                .Any(x => x.Id == 3);
            Assert.IsFalse(isExist);
        }
    }
}