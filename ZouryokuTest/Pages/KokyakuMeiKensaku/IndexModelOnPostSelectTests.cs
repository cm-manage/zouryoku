using CommonLibrary.Extensions;
using Microsoft.EntityFrameworkCore;
using ZouryokuTest.Builder;
using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.KokyakuMeiKensaku
{
    /// <summary>
    /// 顧客情報選択時のテスト（参照履歴テーブルへの追加）
    /// </summary>
    [TestClass]
    public class IndexModelOnPostSelectTests : IndexModelTestBase
    {
        /// <summary>
        /// 異常系：対象の顧客情報が存在しないときエラーを返却すること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostSelect_顧客会社が存在しない_エラー()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();
            // 存在しない顧客会社IDを作成
            var customerId = db.KokyakuKaishas.Count() + 1;

            // Act
            var response = await model.OnPostSelectAsync(customerId);

            // Assert
            AssertError(response, ErrorSelectedDataNotExists);
        }

        /// <summary>
        /// 正常系：対象の参照履歴が既に存在する場合に、該当データの参照時間を更新すること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostSelect_参照履歴が存在_参照時間を更新()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForManage();
            db.SaveChanges();
            var customerId = 1;
            // 対象データの更新前の参照時間を取得
            var referDateTimeBefore = db.KokyakuKaisyaSansyouRirekis
                .Single(x => x.KokyakuKaisyaId == customerId)
                .SansyouTime;

            // Act
            await model.OnPostSelectAsync(customerId);

            // Assert
            var referDateTimeAfter = db.KokyakuKaisyaSansyouRirekis
                .Single(x => x.KokyakuKaisyaId == customerId)
                .SansyouTime;
            Assert.AreNotEqual(referDateTimeBefore, referDateTimeAfter);
        }

        /// <summary>
        /// 正常系：対象以外の参照履歴を更新しないこと
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostSelect_参照履歴が存在_他データを更新しない()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForManage();
            db.SaveChanges();
            var customerId = 2;
            var referDateTimeBefore = db.KokyakuKaisyaSansyouRirekis
                .Single(x => x.KokyakuKaisyaId == customerId)
                .SansyouTime;

            // Act
            // Assert対象以外の顧客会社IDを指定する
            await model.OnPostSelectAsync(customerId - 1);

            // Assert
            var referDateTimeAfter = db.KokyakuKaisyaSansyouRirekis
                .Single(x => x.KokyakuKaisyaId == customerId)
                .SansyouTime;
            Assert.AreEqual(referDateTimeBefore, referDateTimeAfter);
        }

        /// <summary>
        /// 正常系：顧客会社IDが一致する参照履歴が存在しない場合、新しいデータを挿入すること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostSelect_選択した顧客の参照履歴が存在しない_データを挿入()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForManage();
            db.SaveChanges();
            var customerId = 4;

            // Act
            await model.OnPostSelectAsync(customerId);

            // Assert
            var history = db.KokyakuKaisyaSansyouRirekis
                .SingleOrDefault(x => x.KokyakuKaisyaId == customerId);
            Assert.IsNotNull(history);
            Assert.AreEqual(LoginUserSyainBaseId, history.SyainBaseId);
            Assert.AreEqual(customerId, history.KokyakuKaisyaId);
        }

        /// <summary>
        /// 正常系：社員BASE IDが一致する参照履歴が存在しない場合、新しいデータを挿入すること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostSelect_ログインユーザーの参照履歴が存在しない_データを挿入()
        {
            // Arrange
            var model = CreateModel();
            db.Add(new KokyakuKaisyaSansyouRirekiBuilder()
                .WithId(3)
                .WithSyainBaseId(1)
                .WithKokyakuKaisyaId(3)
                .WithSansyouTime(fakeTimeProvider.Now().AddDays(-1))
                .Build());
            db.Add(new KokyakuKaishaBuilder()
                .WithId(3)
                .Build());
            db.SaveChanges();
            var customerId = 3;

            // Act
            await model.OnPostSelectAsync(customerId);

            // Assert
            var isExist = db.KokyakuKaisyaSansyouRirekis
                .Any(x => x.KokyakuKaisyaId == 3 && x.SyainBaseId == LoginUserSyainBaseId);
            Assert.IsTrue(isExist);
        }

        /// <summary>
        /// 正常系：参照履歴の総数が50件以下の場合、データを削除しないこと
        /// </summary>
        /// <param name="countBefore"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(25, DisplayName = "件数の代表値")]
        [DataRow(50, DisplayName = "件数の境界値")]
        public async Task OnPostSelect_更新時かつ参照履歴が50件以下_データを削除しない(int countBefore)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForAcquire(countBefore);
            db.SaveChanges();

            // Act
            await model.OnPostSelectAsync(1);

            // Assert
            var countAfter = db.KokyakuKaisyaSansyouRirekis.Count();
            Assert.AreEqual(countBefore, countAfter);
        }

        /// <summary>
        /// 正常系：参照履歴の総数が50件を超過するとき、50件になるようにデータを削除すること
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(51, DisplayName = "件数の境界値")]
        [DataRow(52, DisplayName = "件数の代表値")]
        public async Task OnPostSelect_更新時かつ参照履歴が50件超過_50件になるようデータを削除(int count)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForAcquire(count);
            db.SaveChanges();

            // Act
            await model.OnPostSelectAsync(1);

            // Assert
            var countAfter = db.KokyakuKaisyaSansyouRirekis.Count();
            Assert.AreEqual(50, countAfter);
        }

        /// <summary>
        /// 正常系：参照履歴の総数が50件以下の場合、データを削除しないこと
        /// </summary>
        /// <param name="countBefore"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(24, DisplayName = "件数の代表値")]
        [DataRow(49, DisplayName = "件数の境界値")]
        public async Task OnPostSelect_追加時かつ参照履歴が50件以下_データを削除しない(int countBefore)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForAcquire(countBefore);
            // 履歴に追加するための顧客会社を用意する
            var customerId = 100;
            db.Add(new KokyakuKaishaBuilder()
                .WithId(customerId)
                .Build());
            db.SaveChanges();

            // Act
            await model.OnPostSelectAsync(customerId);

            // Assert
            var countAfter = db.KokyakuKaisyaSansyouRirekis.Count();
            Assert.AreEqual(countBefore + 1, countAfter);
        }

        /// <summary>
        /// 正常系：参照履歴の総数が50件を超過するとき、50件になるようにデータを削除すること
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(50, DisplayName = "件数の境界値")]
        [DataRow(51, DisplayName = "件数の代表値")]
        public async Task OnPostSelect_追加時かつ参照履歴が50件超過_50件になるようデータを削除(int count)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForAcquire(count);
            db.SaveChanges();
            // 履歴に追加するための顧客会社を用意する
            var customerId = 100;
            db.Add(new KokyakuKaishaBuilder()
                .WithId(customerId)
                .Build());
            db.SaveChanges();

            // Act
            await model.OnPostSelectAsync(customerId);

            // Assert
            var countAfter = db.KokyakuKaisyaSansyouRirekis.Count();
            Assert.AreEqual(50, countAfter);
        }

        /// <summary>
        /// 正常系：削除されたデータが一番古いものであること
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(51, DisplayName = "件数の境界値")]
        [DataRow(52, DisplayName = "件数の代表値")]
        public async Task OnPostSelect_削除データは一番古い(int count)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForAcquire(count);
            db.SaveChanges();

            // Act
            // 参照時間の更新なのでDB内の件数は変化しない
            await model.OnPostSelectAsync(1);

            // Assert
            // テストデータはIDが大きいほど古くなるように作られているため
            // IDが51, 52のデータが削除されていることを確認
            Assert.IsFalse(db.KokyakuKaisyaSansyouRirekis.Any(x => x.Id == 51));
            Assert.IsFalse(db.KokyakuKaisyaSansyouRirekis.Any(x => x.Id == 52));
        }
    }
}