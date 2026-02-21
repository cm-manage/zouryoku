using ZouryokuTest.Builder;
using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.KokyakuMeiKensaku
{
    /// <summary>
    /// 参照履歴削除機能のテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnPostDeleteTests : IndexModelTestBase
    {
        /// <summary>
        /// 異常系：削除対象のデータが存在しないときにエラーを返却すること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostDeleteHistory_削除対象データに紐づく顧客情報が存在しない_エラー()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForManage();
            db.SaveChanges();
            // 存在しない顧客会社IDを作成
            var customerId = db.KokyakuKaishas.Count() + 1;

            // Act
            // 排他制御用のバージョンはダミー
            var response = await model.OnPostDeleteHistoryAsync(customerId, 1);

            // Assert
            AssertError(response, ErrorSelectedDataNotExists);
        }

        /// <summary>
        /// 正常系：削除対象のデータが存在するとき、削除処理を行うこと
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostDeleteHistory_削除対象のデータが存在_該当データを削除()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForManage();
            db.SaveChanges();
            // 削除対象のデータが紐づく顧客会社ID
            var customerId = 1;
            // 削除対象のデータのバージョンを取得
            var version = db.KokyakuKaisyaSansyouRirekis
                .Single(history => history.KokyakuKaisya.Id == customerId)
                .Version;
            // 削除前のデータ件数を取得
            var countBefore = db.KokyakuKaisyaSansyouRirekis.Count();

            // Act
            await model.OnPostDeleteHistoryAsync(customerId, version);

            // Assert
            // 対象が削除されていること
            var isExist = db.KokyakuKaisyaSansyouRirekis
                .Any(x => x.KokyakuKaisya.Id == customerId);
            Assert.IsFalse(isExist);
            // 1件のみ削除されていること
            var countAfter = db.KokyakuKaisyaSansyouRirekis.Count();
            Assert.AreEqual(countBefore - 1, countAfter);
        }

        /// <summary>
        /// 異常系：削除対象でないデータを削除しないこと
        /// </summary>
        /// <param name="customerId">削除するデータの顧客会社ID</param>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostDeleteHistory_該当データ以外を削除しない()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForManage();
            db.SaveChanges();
            // 削除対象のデータが紐づく顧客会社ID
            var customerId = 1;
            // 削除対象のデータのバージョンを取得
            var version = db.KokyakuKaisyaSansyouRirekis
                .Single(history => history.KokyakuKaisya.Id == customerId)
                .Version;

            // Act
            await model.OnPostDeleteHistoryAsync(customerId, version);

            // Assert
            var isExist = db.KokyakuKaisyaSansyouRirekis
                .Any(x => x.Id == 2);
            Assert.IsTrue(isExist);
        }

        /// <summary>
        /// 異常系：顧客会社IDが一致するデータが存在しないときにデータを削除しないこと
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostDeleteHistory_指定された顧客会社の履歴が存在しない_データを削除しない()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForManage();
            db.SaveChanges();
            // 削除対象のデータ（実際は存在していない）が紐づく顧客会社ID
            var customerId = 4;
            // 削除実行前の件数
            var countBefore = db.KokyakuKaisyaSansyouRirekis.Count();

            // Act
            // 存在しない履歴なのでバージョンはダミー
            await model.OnPostDeleteHistoryAsync(customerId, 1);

            // Assert
            var countAfter = db.KokyakuKaisyaSansyouRirekis.Count();
            Assert.AreEqual(countBefore, countAfter);
        }

        /// <summary>
        /// 異常系：社員BASE IDが一致するデータが存在しないときにデータを削除しないこと
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostDeleteHistory_ログインユーザーのものでない_削除しない()
        {
            // Arrange
            var model = CreateModel();
            db.Add(new KokyakuKaisyaSansyouRirekiBuilder()
                .WithId(3)
                .WithSyainBaseId(1)
                .WithKokyakuKaisyaId(3)
                .WithSansyouTime(DateTime.Now.AddDays(-1))
                .Build());
            db.SaveChanges();
            // 削除対象のデータが紐づく顧客会社ID
            var customerId = 3;
            // 削除対象のデータのバージョンを取得
            var version = db.KokyakuKaisyaSansyouRirekis
                .Single(history => history.KokyakuKaisyaId == customerId)
                .Version;
            // 削除実行前の件数
            var countBefore = db.KokyakuKaisyaSansyouRirekis.Count();

            // Act
            await model.OnPostDeleteHistoryAsync(customerId, version);

            // Assert
            var countAfter = db.KokyakuKaisyaSansyouRirekis.Count();
            Assert.AreEqual(countBefore, countAfter);
        }
    }
}