using CommonLibrary.Extensions;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Utils;

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
            var syainBase = new SyainBasis
            {
                Id = 1,
                Name = "社員A",
                Code = "00000",
            };

            // シード: 案件
            var anken = new Anken()
            {
                Id = 1,
                Name = "案件A",
                SearchName = "案件A",
            };

            // シード: ログインユーザーの案件参照履歴
            var userRirekis = Enumerable.Range(1, existingCount)
                .Select(i => new AnkenSansyouRireki()
                {
                    Id = i,
                    AnkenId = 0, // ダミー
                    SyainBaseId = syainBase.Id,
                    SansyouTime = Now.AddMinutes(-i)
                })
                .ToList();

            // シード: 別ユーザーの案件参照履歴（3件）
            var otherUserRirekis = Enumerable.Range(existingCount + 1, 3)
                .Select(i => new AnkenSansyouRireki()
                {
                    Id = i,
                    AnkenId = anken.Id,
                    SyainBaseId = 0, // ダミー
                    SansyouTime = Now.AddMinutes(-i)
                })
                .ToList();

            // 必要データ登録
            SeedEntities(syainBase, anken, userRirekis, otherUserRirekis);

            // ---------- Act ----------
            await AnkenSansyouRirekisUtil.MaintainAnkenSansyouRirekiAsync(db, anken, syainBase.Id, Now);
            await db.SaveChangesAsync();

            // ---------- Assert ----------
            // 登録された履歴を取得
            var registeredRireki = await db.AnkenSansyouRirekis
                .FirstOrDefaultAsync(x => x.AnkenId == anken.Id && x.SyainBaseId == syainBase.Id);

            // 新規参照履歴が登録されていること
            Assert.IsNotNull(registeredRireki, "新規参照履歴が登録されていません。");

            // 参照時間が実行開始時間から実行終了時間の間であること
            AssertSansyouTime(registeredRireki, Now);

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
            var syainBase = new SyainBasis
            {
                Id = 1,
                Name = "社員A",
                Code = "00000",
            };

            // シード: 案件
            var anken = new Anken()
            {
                Id = 1,
                Name = "案件A",
                SearchName = "案件A",
            };

            // シード: ログインユーザーの案件参照履歴 + 更新対象の案件参照履歴
            var userRirekis = Enumerable.Range(1, existingCount)
                .Select(i => new AnkenSansyouRireki()
                {
                    Id = i,
                    AnkenId = 0, // ダミー
                    SyainBaseId = syainBase.Id,
                    SansyouTime = Now.AddMinutes(-i)
                })
                .ToList();

            var existingRireki = new AnkenSansyouRireki()
            {
                Id = existingCount + 1,
                AnkenId = anken.Id,
                SyainBaseId = syainBase.Id,
                SansyouTime = Now.AddMinutes(-100), // 古い日時とする
            };

            // シード: 別ユーザーの案件参照履歴（3件）
            var otherUserRirekis = Enumerable.Range(existingCount + 2, 3)
                .Select(i => new AnkenSansyouRireki()
                {
                    Id = i,
                    AnkenId = anken.Id,
                    SyainBaseId = 0, // ダミー
                    SansyouTime = Now.AddMinutes(-i)
                })
                .ToList();

            // 必要データ登録
            SeedEntities(syainBase, anken, userRirekis, existingRireki, otherUserRirekis);

            // ---------- Act ----------
            await AnkenSansyouRirekisUtil.MaintainAnkenSansyouRirekiAsync(db, anken, syainBase.Id, Now);
            await db.SaveChangesAsync();

            // ---------- Assert ----------
            var updatedRireki = await db.AnkenSansyouRirekis
                .FirstOrDefaultAsync(x => x.Id == existingRireki.Id);

            // 既存参照履歴が更新されていること
            Assert.IsNotNull(updatedRireki, "既存参照履歴が存在しません。");

            // 参照時間が実行開始時間から実行終了時間の間であること
            AssertSansyouTime(updatedRireki, Now);

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
            var syainBase = new SyainBasis
            {
                Id = 1,
                Name = "社員A",
                Code = "00000",
            };

            // シード: ログインユーザーの案件参照履歴
            var userRirekis = Enumerable.Range(1, existingCount)
                .Select(i => new AnkenSansyouRireki()
                {
                    Id = i,
                    AnkenId = 0, // ダミー
                    SyainBaseId = syainBase.Id,
                    SansyouTime = Now.AddMinutes(-i)
                })
                .ToList();

            // シード: 別ユーザーの案件参照履歴（3件）
            var otherUserRirekis = Enumerable.Range(existingCount + 1, 3)
                .Select(i => new AnkenSansyouRireki()
                {
                    Id = i,
                    AnkenId = 0,
                    SyainBaseId = 0, // ダミー
                    SansyouTime = Now.AddMinutes(-i)
                })
                .ToList();

            // 必要データ登録
            SeedEntities(syainBase, userRirekis, otherUserRirekis);

            // 新規登録用の案件情報
            var anken = new Anken()
            {
                Name = "案件A",
                SearchName = "案件A",
            };

            // ---------- Act ----------
            await db.Ankens.AddAsync(anken);
            await AnkenSansyouRirekisUtil.MaintainAnkenSansyouRirekiAsync(db, anken, syainBase.Id, Now);
            await db.SaveChangesAsync();

            // ---------- Assert ----------
            var registeredRireki = await db.AnkenSansyouRirekis
                .FirstOrDefaultAsync(x => x.AnkenId == anken.Id && x.SyainBaseId == syainBase.Id);

            // 新規参照履歴が登録されていること
            Assert.IsNotNull(registeredRireki, "新規参照履歴が登録されていません。");

            // 参照時間が実行開始時間から実行終了時間の間であること
            AssertSansyouTime(registeredRireki, Now);

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
            var syainBase = new SyainBasis
            {
                Id = 1,
                Name = "社員A",
                Code = "00000",
            };

            // シード: 案件
            var anken = new Anken()
            {
                Id = 1,
                Name = "案件A",
                SearchName = "案件A",
            };

            // シード: ログインユーザーの案件参照履歴 + 更新対象の案件参照履歴
            var userRirekis = Enumerable.Range(1, existingCount)
                .Select(i => new AnkenSansyouRireki()
                {
                    Id = i,
                    AnkenId = 0, // ダミー
                    SyainBaseId = syainBase.Id,
                    SansyouTime = Now.AddMinutes(-i)
                })
                .ToList();

            var existingRireki = new AnkenSansyouRireki()
            {
                Id = existingCount + 1,
                AnkenId = anken.Id,
                SyainBaseId = syainBase.Id,
                SansyouTime = Now.AddMinutes(-100), // 古い日時とする
            };

            // シード: 別ユーザーの案件参照履歴（3件）
            var otherUserRirekis = Enumerable.Range(existingCount + 2, 3)
                .Select(i => new AnkenSansyouRireki()
                {
                    Id = i,
                    AnkenId = anken.Id,
                    SyainBaseId = 0, // ダミー
                    SansyouTime = Now.AddMinutes(-i)
                })
                .ToList();

            // 必要データ登録
            SeedEntities(syainBase, anken, userRirekis, existingRireki, otherUserRirekis);

            // ---------- Act ----------
            anken.Name = "更新後案件";
            await AnkenSansyouRirekisUtil.MaintainAnkenSansyouRirekiAsync(db, anken, syainBase.Id, Now);
            await db.SaveChangesAsync();

            // ---------- Assert ----------
            var updatedRireki = await db.AnkenSansyouRirekis
                .FirstOrDefaultAsync(x => x.Id == existingRireki.Id);

            // 既存参照履歴が更新されていること
            Assert.IsNotNull(updatedRireki, "既存参照履歴が存在しません。");

            // 参照時間が実行開始時間から実行終了時間の間であること
            AssertSansyouTime(updatedRireki, Now);

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

        private static readonly DateTime Now = new(2024, 1, 1, 12, 0, 0);

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
        /// 参照時間を確認する
        /// </summary>
        /// <param name="ankenSansyouRireki">確認対象の案件参照履歴</param>
        /// <param name="expectedTime">期待する参照時間</param>
        private static void AssertSansyouTime(
            AnkenSansyouRireki ankenSansyouRireki,
            DateTime expectedTime)
        {
            Assert.AreEqual(expectedTime, ankenSansyouRireki.SansyouTime,
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