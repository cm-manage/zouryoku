using CommonLibrary.Extensions;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Utils;
using ZouryokuTest.Builder;

namespace ZouryokuTest.Utils
{
    [TestClass]
    public class AnkenSansyouRirekiUtilTests : BaseInMemoryDbContextTest
    {
        // ---------------------------------------------------------------------
        // MaintainAnkenSansyouRirekiAsync Tests
        // ---------------------------------------------------------------------
        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------

        // =================================================================
        // #01 / #02: 登録済みの案件の参照履歴がない場合
        // =================================================================
        /// <summary>
        /// 検索・表示: 更新対象のレコードが存在しない場合、参照履歴を追加する
        /// #01 案件参照履歴が50件を超えない場合、参照履歴を追加しても全ての履歴を保持する。
        /// #02 案件参照履歴が50件を超える場合、参照履歴を追加すると古い履歴が削除される。
        /// </summary>
        [TestMethod]
        [DataRow(AnkenSansyouRirekisUtil.MaxAnkenHistoryCount - 1, false, DisplayName = "#01 案件参照履歴が50件以下 → 削除なし")]
        [DataRow(AnkenSansyouRirekisUtil.MaxAnkenHistoryCount, true, DisplayName = "#02 案件参照履歴が50件を超過 → 古い履歴削除")]
        public async Task MaintainAnkenSansyouRirekiAsync_NoExistingRecord_AddsRecord(
            int existingCount,
            bool shouldDeleteOldest)
        {
            // ---------- Arrange ----------
            // シード: 社員Base
            var syainBase = CreateSyainBase(1);
            // シード: 案件
            var anken = CreateAnken(1);

            // シード: ログインユーザーの案件参照履歴
            var userRirekis = CreateAnkenSansyouRirekis(syainBase.Id, existingCount);
            // シード: 別ユーザーの案件参照履歴（3件）
            var otherUserRirekis = CreateOtherAnkenSansyouRirekis(anken.Id, existingCount + 1);

            // 必要データ登録
            SeedEntities(syainBase, anken, userRirekis, otherUserRirekis);

            // ---------- Act ----------
            var beforeActTime = fakeTimeProvider.Now();
            await AnkenSansyouRirekisUtil.MaintainAnkenSansyouRirekiAsync(db, anken, syainBase.Id, fakeTimeProvider.Now());
            await db.SaveChangesAsync();
            var afterActTime = fakeTimeProvider.Now();

            // ---------- Assert ----------
            // 登録された履歴を取得
            var registeredRireki = await db.AnkenSansyouRirekis
                .FirstOrDefaultAsync(x => x.AnkenId == anken.Id && x.SyainBaseId == syainBase.Id);

            // 新規参照履歴が登録されていること
            Assert.IsNotNull(registeredRireki, "新規参照履歴が登録されていません。");

            // 参照時間が実行開始時間から実行終了時間の間であること
            AssertSansyouTime(registeredRireki, beforeActTime, afterActTime);

            // ログインユーザーの参照履歴が50件になっていること
            await AssertRirekiCountAsync(syainBase.Id, AnkenSansyouRirekisUtil.MaxAnkenHistoryCount);

            // 件数超過の場合、最も古い履歴が削除されていること
            if (shouldDeleteOldest)
            {
                await AssertOldestRirekiDeletedAsync(userRirekis);
            }

            // 別ユーザーの参照履歴に変化がないこと
            await AssertOtherUserRirekiCountAsync(syainBase.Id, otherUserRirekis.Count);
        }

        // =================================================================
        // #03 / #04: 登録済みの案件の参照履歴がある場合
        // =================================================================
        /// <summary>
        /// 検索・表示: 更新対象のレコードが存在する場合、参照履歴を更新する
        /// #03 案件参照履歴が50件を超えない場合、参照履歴を更新しても全ての履歴を保持する。
        /// #04 案件参照履歴が50件を超える場合、参照履歴を更新すると古い履歴が削除される。
        /// </summary>
        [TestMethod]
        [DataRow(AnkenSansyouRirekisUtil.MaxAnkenHistoryCount - 1, false, DisplayName = "#03 案件参照履歴が50件以下 → 削除なし")]
        [DataRow(AnkenSansyouRirekisUtil.MaxAnkenHistoryCount, true, DisplayName = "#04 案件参照履歴が50件を超過 → 古い履歴削除")]
        public async Task MaintainAnkenSansyouRirekiAsync_ExistingRecord_UpdatesRecord(
            int existingCount,
            bool shouldDeleteOldest)
        {
            // ---------- Arrange ----------
            // シード: 社員Base
            var syainBase = CreateSyainBase(1);
            // シード: 案件
            var anken = CreateAnken(1);

            // シード: ログインユーザーの案件参照履歴 + 更新対象の案件参照履歴
            var userRirekis = CreateAnkenSansyouRirekis(syainBase.Id, existingCount);
            var existingRireki = CreateExistingAnkenSansyouRireki(anken.Id, syainBase.Id, existingCount + 1);

            // シード: 別ユーザーの案件参照履歴（3件）
            var otherUserRirekis = CreateOtherAnkenSansyouRirekis(anken.Id, existingCount + 2);

            // 必要データ登録
            SeedEntities(syainBase, anken, userRirekis, existingRireki, otherUserRirekis);

            // ---------- Act ----------
            var beforeActTime = fakeTimeProvider.Now();
            await AnkenSansyouRirekisUtil.MaintainAnkenSansyouRirekiAsync(db, anken, syainBase.Id, fakeTimeProvider.Now());
            await db.SaveChangesAsync();
            var afterActTime = fakeTimeProvider.Now();

            // ---------- Assert ----------
            var updatedRireki = await db.AnkenSansyouRirekis
                .FirstOrDefaultAsync(x => x.Id == existingRireki.Id);

            // 既存参照履歴が更新されていること
            Assert.IsNotNull(updatedRireki, "既存参照履歴が存在しません。");

            // 参照時間が実行開始時間から実行終了時間の間であること
            AssertSansyouTime(updatedRireki, beforeActTime, afterActTime);

            // ログインユーザーの参照履歴が50件であること
            await AssertRirekiCountAsync(syainBase.Id, AnkenSansyouRirekisUtil.MaxAnkenHistoryCount);

            // 件数超過の場合、最も古い履歴が削除されていること
            if (shouldDeleteOldest)
            {
                await AssertOldestRirekiDeletedAsync(userRirekis);
            }

            // 別ユーザーの参照履歴に変化がないこと
            await AssertOtherUserRirekiCountAsync(syainBase.Id, otherUserRirekis.Count);
        }

        // =================================================================
        // #05 / #06: 新規の案件を登録する場合
        // =================================================================
        /// <summary>
        /// 登録: 新規の案件を登録する場合、連番が割り振られた案件IDで参照履歴が登録される。
        /// #05 案件参照履歴が50件を超えない場合、参照履歴を追加しても全ての履歴を保持する。
        /// #06 案件参照履歴が50件を超える場合、参照履歴を追加すると古い履歴が削除される。
        /// </summary>
        [TestMethod]
        [DataRow(AnkenSansyouRirekisUtil.MaxAnkenHistoryCount - 1, false, DisplayName = "#05 案件参照履歴が50件以下 → 削除なし")]
        [DataRow(AnkenSansyouRirekisUtil.MaxAnkenHistoryCount, true, DisplayName = "#06 案件参照履歴が50件を超過 → 古い履歴削除")]
        public async Task MaintainAnkenSansyouRirekiAsync_AnkenIsAdded_AddsRecord(
            int existingCount,
            bool shouldDeleteOldest)
        {
            // ---------- Arrange ----------
            // シード: 社員Base
            var syainBase = CreateSyainBase(1);

            // シード: ログインユーザーの案件参照履歴
            var userRirekis = CreateAnkenSansyouRirekis(syainBase.Id, existingCount);
            // シード: 別ユーザーの案件参照履歴（3件）
            var otherUserRirekis = CreateOtherAnkenSansyouRirekis(0, existingCount + 1);

            // 必要データ登録
            SeedEntities(syainBase, userRirekis, otherUserRirekis);

            // 新規登録用の案件情報
            var anken = CreateAnken(0);

            // ---------- Act ----------
            await db.Ankens.AddAsync(anken);
            var beforeActTime = fakeTimeProvider.Now();
            await AnkenSansyouRirekisUtil.MaintainAnkenSansyouRirekiAsync(db, anken, syainBase.Id, fakeTimeProvider.Now());
            await db.SaveChangesAsync();
            var afterActTime = fakeTimeProvider.Now();

            // ---------- Assert ----------
            var registeredRireki = await db.AnkenSansyouRirekis
                .FirstOrDefaultAsync(x => x.AnkenId == anken.Id && x.SyainBaseId == syainBase.Id);

            // 新規参照履歴が登録されていること
            Assert.IsNotNull(registeredRireki, "新規参照履歴が登録されていません。");

            // 参照時間が実行開始時間から実行終了時間の間であること
            AssertSansyouTime(registeredRireki, beforeActTime, afterActTime);

            // ログインユーザーの参照履歴が50件になっていること
            await AssertRirekiCountAsync(syainBase.Id, AnkenSansyouRirekisUtil.MaxAnkenHistoryCount);

            // 件数超過の場合、最も古い履歴が削除されていること
            if (shouldDeleteOldest)
            {
                await AssertOldestRirekiDeletedAsync(userRirekis);
            }

            // 別ユーザーの参照履歴に変化がないこと
            await AssertOtherUserRirekiCountAsync(syainBase.Id, otherUserRirekis.Count);
        }

        // =================================================================
        // #07 / #08: 既存の案件を更新する場合
        // =================================================================
        /// <summary>
        /// 登録: 既存案件を更新する場合、参照履歴が更新される。
        /// #07 案件参照履歴が50件を超えない場合、参照履歴を更新しても全ての履歴を保持する。
        /// #08 案件参照履歴が50件を超える場合、参照履歴を更新すると古い履歴が削除される。
        /// </summary>
        [TestMethod]
        [DataRow(AnkenSansyouRirekisUtil.MaxAnkenHistoryCount - 1, false, DisplayName = "#07 案件参照履歴が50件以下 → 削除なし")]
        [DataRow(AnkenSansyouRirekisUtil.MaxAnkenHistoryCount, true, DisplayName = "#08 案件参照履歴が50件を超過 → 古い履歴削除")]
        public async Task MaintainAnkenSansyouRirekiAsync_AnkenIsUpdated_UpdateRecord(
            int existingCount,
            bool shouldDeleteOldest)
        {
            // ---------- Arrange ----------
            // シード: 社員Base
            var syainBase = CreateSyainBase(1);
            // シード: 案件
            var anken = CreateAnken(1);

            // シード: ログインユーザーの案件参照履歴 + 更新対象の案件参照履歴
            var userRirekis = CreateAnkenSansyouRirekis(syainBase.Id, existingCount);
            var existingRireki = CreateExistingAnkenSansyouRireki(anken.Id, syainBase.Id, existingCount + 1);

            // シード: 別ユーザーの案件参照履歴（3件）
            var otherUserRirekis = CreateOtherAnkenSansyouRirekis(anken.Id, existingCount + 2);

            // 必要データ登録
            SeedEntities(syainBase, anken, userRirekis, existingRireki, otherUserRirekis);

            // ---------- Act ----------
            anken.Name = "更新後案件";
            var beforeActTime = fakeTimeProvider.Now();
            await AnkenSansyouRirekisUtil.MaintainAnkenSansyouRirekiAsync(db, anken, syainBase.Id, fakeTimeProvider.Now());
            await db.SaveChangesAsync();
            var afterActTime = fakeTimeProvider.Now();

            // ---------- Assert ----------
            var updatedRireki = await db.AnkenSansyouRirekis
                .FirstOrDefaultAsync(x => x.Id == existingRireki.Id);

            // 既存参照履歴が更新されていること
            Assert.IsNotNull(updatedRireki, "既存参照履歴が存在しません。");

            // 参照時間が実行開始時間から実行終了時間の間であること
            AssertSansyouTime(updatedRireki, beforeActTime, afterActTime);

            // ログインユーザーの参照履歴が50件になっていること
            await AssertRirekiCountAsync(syainBase.Id, AnkenSansyouRirekisUtil.MaxAnkenHistoryCount);

            // 件数超過の場合、最も古い履歴が削除されていること
            if (shouldDeleteOldest)
            {
                await AssertOldestRirekiDeletedAsync(userRirekis);
            }

            // 別ユーザーの参照履歴に変化がないこと
            await AssertOtherUserRirekiCountAsync(syainBase.Id, otherUserRirekis.Count);
        }

        // ---------------------------------------------------------------------
        // Helper Methods
        // ---------------------------------------------------------------------
        
        /// <summary>
        /// シード処理
        /// </summary>
        private void SeedEntities(params object[] entities)
        {
            foreach (var e in entities)
            {
                if (e is IEnumerable<object> list)
                {
                    db.AddRange(list);
                }
                else
                {
                    db.Add(e);
                }
            }
            db.SaveChanges();
        }

        /// <summary>
        /// シード: 社員Base生成
        /// </summary>
        private static SyainBasis CreateSyainBase(long id)
        {
            return new SyainBasisBuilder()
                .WithId(id)
                .Build();
        }

        /// <summary>
        /// シード: 案件生成
        /// </summary>
        private static Anken CreateAnken(long id)
        {
            return new AnkenBuilder()
                .WithId(id)
                .Build();
        }

        /// <summary>
        /// シード: ログインユーザーの案件参照履歴を複数生成
        /// </summary>
        /// <param name="syainBaseId">社員BaseID</param>
        /// <param name="count">生成件数</param>
        private List<AnkenSansyouRireki> CreateAnkenSansyouRirekis(long syainBaseId, int count)
        {
            return new AnkenSansyouRirekiBuilder()
                .WithAnkenId(0) // ダミー
                .WithSyainBaseId(syainBaseId)
                .BuildMany(1, count, data =>
                {
                    data.SansyouTime = fakeTimeProvider.Now().AddMinutes(-data.Id); // IDの小さい順に古い日時とする
                });

        }

        /// <summary>
        /// シード: 既存の案件参照履歴生成
        /// </summary>
        private AnkenSansyouRireki CreateExistingAnkenSansyouRireki(long ankenId, long syainBaseId, long id)
        {
            return new AnkenSansyouRirekiBuilder()
                .WithId(id)
                .WithAnkenId(ankenId)
                .WithSyainBaseId(syainBaseId)
                .WithSansyouTime(fakeTimeProvider.Now().AddMinutes(-100)) // 古い日時とする
                .Build();
        }

        /// <summary>
        /// シード: 別ユーザーの案件参照履歴を複数生成
        /// </summary>
        /// <param name="ankenId">案件ID</param>
        /// <param name="startId">一番最初の参照履歴ID</param>
        private List<AnkenSansyouRireki> CreateOtherAnkenSansyouRirekis(long ankenId, int startId)
        {
            return new AnkenSansyouRirekiBuilder()
                .WithAnkenId(ankenId)
                .WithSyainBaseId(0) // ダミー
                .BuildMany(startId, 3, data =>
                {
                    data.SansyouTime = fakeTimeProvider.Now().AddMinutes(-data.Id); // IDの小さい順に古い日時とする
                });
        }

        /// <summary>
        /// 参照時間が指定範囲内であることを確認する
        /// </summary>
        /// <param name="ankenSansyouRireki">確認対象の案件参照履歴</param>
        /// <param name="beforeUpdateTime">更新前の時間</param>
        /// <param name="afterUpdateTime">更新後の時間</param>
        private static void AssertSansyouTime(
            AnkenSansyouRireki ankenSansyouRireki,
            DateTime beforeUpdateTime,
            DateTime afterUpdateTime)
        {
            Assert.IsTrue(beforeUpdateTime <= ankenSansyouRireki.SansyouTime && ankenSansyouRireki.SansyouTime <= afterUpdateTime,
                "参照時間が正しく更新されていません。");
        }

        /// <summary>
        /// 参照履歴件数を確認する
        /// </summary>
        /// <param name="syainBaseId">確認対象の社員BaseID</param>
        /// <param name="expectedCount">期待する参照履歴件数</param>
        private async Task AssertRirekiCountAsync(long syainBaseId, int expectedCount)
        {
            var count = await db.AnkenSansyouRirekis
                .CountAsync(x => x.SyainBaseId == syainBaseId);
            Assert.AreEqual(expectedCount, count, "ログインユーザーの参照履歴の件数が正しくありません。");
        }

        /// <summary>
        /// 別ユーザーの参照履歴件数を確認する
        /// </summary>
        /// <param name="loginSyainBaseId">ログインユーザーの社員BaseID</param>
        /// <param name="expectedCount">期待する別ユーザーの参照履歴件数</param>
        private async Task AssertOtherUserRirekiCountAsync(long loginSyainBaseId, int expectedCount)
        {
            var count = await db.AnkenSansyouRirekis
                .CountAsync(x => x.SyainBaseId != loginSyainBaseId);
            Assert.AreEqual(expectedCount, count, "別ユーザーの参照履歴に変化があります。");
        }

        /// <summary>
        /// 最も古い履歴が削除されていることを確認する
        /// </summary>
        /// <param name="ankenSansyouRirekis">確認対象の案件参照履歴リスト</param>
        private async Task AssertOldestRirekiDeletedAsync(List<AnkenSansyouRireki> ankenSansyouRirekis)
        {
            var oldestRireki = ankenSansyouRirekis.OrderBy(x => x.SansyouTime).First();
            var existsOldest = await db.AnkenSansyouRirekis
                .AnyAsync(x => x.Id == oldestRireki.Id);
            Assert.IsFalse(existsOldest, "最も古い履歴が削除されていません。");
        }
    }
}