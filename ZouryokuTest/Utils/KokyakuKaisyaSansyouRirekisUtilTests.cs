using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Utils;

namespace ZouryokuTest.Utils
{
    [TestClass]
    public class KokyakuKaisyaSansyouRirekiUtilTests : BaseInMemoryDbContextTest
    {
        // ---------------------------------------------------------------------
        // MaintainKokyakuKaisyaSansyouRirekiAsync Tests
        // ---------------------------------------------------------------------
        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------

        // =================================================================
        // 登録済みの顧客会社の参照履歴がない場合
        // =================================================================
        /// <summary>
        /// 検索・表示: 更新対象のレコードが存在しない場合、参照履歴を追加する
        ///     顧客会社参照履歴が50件を超えない場合、参照履歴を追加しても全ての履歴を保持する。
        ///     顧客会社参照履歴が50件を超える場合、参照履歴を追加すると古い履歴が削除される。
        /// </summary>
        [TestMethod]
        [DataRow(KokyakuKaisyaSansyouRirekisUtil.MaxHistoryCount - 1, false, DisplayName = "顧客会社参照履歴が50件以下 → 削除なし")]
        [DataRow(KokyakuKaisyaSansyouRirekisUtil.MaxHistoryCount, true, DisplayName = "顧客会社参照履歴が50件を超過 → 古い履歴削除")]
        public async Task MaintainKokyakuKaisyaSansyouRirekiAsync_更新対象のレコードが存在しない_参照履歴を追加(
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

            // シード: 顧客
            var kokyaku = new KokyakuKaisha
            {
                Id = 1,
                Code = 00000,
                Name = "A会社",
                NameKana = "エーカイシャ",
                Ryakusyou = "A",
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            var now = DateTime.Now;

            // シード: ログインユーザーの顧客会社参照履歴
            var userRirekis = Enumerable.Range(1, existingCount).Select(i =>
            {
                return new KokyakuKaisyaSansyouRireki
                {
                    Id = i,
                    KokyakuKaisyaId = 0, // ダミー
                    SyainBaseId = syainBase.Id,
                    SansyouTime = now.AddMinutes(-i), // IDの小さい順に古い日時とする
                };
            }).ToList();

            // シード: 別ユーザーの顧客会社参照履歴（3件）
            var otherUserRirekis = Enumerable.Range(existingCount + 1, 3).Select(i =>
            {
                return new KokyakuKaisyaSansyouRireki
                {
                    Id = i,
                    KokyakuKaisyaId = kokyaku.Id,
                    SyainBaseId = 0, // ダミー
                    SansyouTime = now.AddMinutes(-i), // IDの小さい順に古い日時とする
                };
            }).ToList();

            // 必要データ登録
            SeedEntities(syainBase, kokyaku, userRirekis, otherUserRirekis);

            // ---------- Act ----------
            var beforeActTime = DateTime.Now;
            await KokyakuKaisyaSansyouRirekisUtil.MaintainKokyakuKaisyaSansyouRirekiAsync(db, kokyaku.Id, syainBase.Id);
            await db.SaveChangesAsync();
            var afterActTime = DateTime.Now;

            // ---------- Assert ----------
            // 登録された履歴を取得
            var registeredRireki = await db.KokyakuKaisyaSansyouRirekis
                .FirstOrDefaultAsync(x => x.KokyakuKaisyaId == kokyaku.Id && x.SyainBaseId == syainBase.Id);

            // 新規参照履歴が登録されていること
            Assert.IsNotNull(registeredRireki, "新規参照履歴が登録されていません。");

            // 参照時間が実行開始時間から実行終了時間の間であること
            AssertSansyouTime(registeredRireki, beforeActTime, afterActTime);

            // ログインユーザーの参照履歴が50件になっていること
            await AssertRirekiCountAsync(syainBase.Id, KokyakuKaisyaSansyouRirekisUtil.MaxHistoryCount);

            // 件数超過の場合、最も古い履歴が削除されていること
            if (shouldDeleteOldest)
            {
                await AssertOldestRirekiDeletedAsync(userRirekis);
            }

            // 別ユーザーの参照履歴に変化がないこと
            await AssertOtherUserRirekiCountAsync(syainBase.Id, otherUserRirekis.Count);
        }

        // =================================================================
        // 登録済みの顧客会社の参照履歴がある場合
        // =================================================================
        /// <summary>
        /// 検索・表示: 更新対象のレコードが存在する場合、参照履歴を更新する
        ///     顧客会社参照履歴が50件を超えない場合、参照履歴を更新しても全ての履歴を保持する。
        ///     顧客会社参照履歴が50件を超える場合、参照履歴を更新すると古い履歴が削除される。
        /// </summary>
        [TestMethod]
        [DataRow(KokyakuKaisyaSansyouRirekisUtil.MaxHistoryCount - 1, false, DisplayName = "顧客会社参照履歴が50件以下 → 削除なし")]
        [DataRow(KokyakuKaisyaSansyouRirekisUtil.MaxHistoryCount, true, DisplayName = "顧客会社参照履歴が50件を超過 → 古い履歴削除")]
        public async Task MaintainKokyakuKaisyaSansyouRirekiAsync_登録済みの顧客会社の参照履歴がある_参照履歴を更新(
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

            // シード: 顧客
            var kokyaku = new KokyakuKaisha
            {
                Id = 1,
                Code = 00000,
                Name = "A会社",
                NameKana = "エーカイシャ",
                Ryakusyou = "A",
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            var now = DateTime.Now;

            // シード: ログインユーザーの顧客会社参照履歴
            var userRirekis = Enumerable.Range(1, existingCount).Select(i =>
            {
                return new KokyakuKaisyaSansyouRireki
                {
                    Id = i,
                    KokyakuKaisyaId = 0, // ダミー
                    SyainBaseId = syainBase.Id,
                    SansyouTime = now.AddMinutes(-i), // IDの小さい順に古い日時とする
                };
            }).ToList();

            // シード: 別ユーザーの顧客会社参照履歴（3件）
            var otherUserRirekis = Enumerable.Range(existingCount + 1, 3).Select(i =>
            {
                return new KokyakuKaisyaSansyouRireki
                {
                    Id = i,
                    KokyakuKaisyaId = kokyaku.Id,
                    SyainBaseId = 0, // ダミー
                    SansyouTime = now.AddMinutes(-i), // IDの小さい順に古い日時とする
                };
            }).ToList();

            // シード: 更新対象の顧客会社参照履歴
            var existingRireki = new KokyakuKaisyaSansyouRireki
            {
                Id = 100,
                KokyakuKaisyaId = kokyaku.Id,
                SyainBaseId= syainBase.Id,
                SansyouTime = DateTime.Now.AddMinutes(-100),
            };

            // 必要データ登録
            SeedEntities(syainBase, kokyaku, userRirekis, existingRireki, otherUserRirekis);

            // ---------- Act ----------
            var beforeActTime = DateTime.Now;
            await KokyakuKaisyaSansyouRirekisUtil.MaintainKokyakuKaisyaSansyouRirekiAsync(db, kokyaku.Id, syainBase.Id);
            await db.SaveChangesAsync();
            var afterActTime = DateTime.Now;

            // ---------- Assert ----------
            var updatedRireki = await db.KokyakuKaisyaSansyouRirekis
                .FirstOrDefaultAsync(x => x.Id == existingRireki.Id);

            // 既存参照履歴が更新されていること
            Assert.IsNotNull(updatedRireki, "既存参照履歴が存在しません。");

            // 参照時間が実行開始時間から実行終了時間の間であること
            AssertSansyouTime(updatedRireki, beforeActTime, afterActTime);

            // ログインユーザーの参照履歴が50件であること
            await AssertRirekiCountAsync(syainBase.Id, KokyakuKaisyaSansyouRirekisUtil.MaxHistoryCount);

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
        /// 参照時間が指定範囲内であることを確認する
        /// </summary>
        /// <param name="kokyakuKaisyaSansyouRireki">確認対象の顧客会社参照履歴</param>
        /// <param name="beforeUpdateTime">更新前の時間</param>
        /// <param name="afterUpdateTime">更新後の時間</param>
        private static void AssertSansyouTime(
            KokyakuKaisyaSansyouRireki kokyakuKaisyaSansyouRireki,
            DateTime beforeUpdateTime,
            DateTime afterUpdateTime)
        {
            Assert.IsTrue(beforeUpdateTime <= kokyakuKaisyaSansyouRireki.SansyouTime && kokyakuKaisyaSansyouRireki.SansyouTime <= afterUpdateTime,
                "参照時間が正しく更新されていません。");
        }

        /// <summary>
        /// 参照履歴件数を確認する
        /// </summary>
        /// <param name="syainBaseId">確認対象の社員BaseID</param>
        /// <param name="expectedCount">期待する参照履歴件数</param>
        private async Task AssertRirekiCountAsync(long syainBaseId, int expectedCount)
        {
            var count = await db.KokyakuKaisyaSansyouRirekis
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
            var count = await db.KokyakuKaisyaSansyouRirekis
                .CountAsync(x => x.SyainBaseId != loginSyainBaseId);
            Assert.AreEqual(expectedCount, count, "別ユーザーの参照履歴に変化があります。");
        }

        /// <summary>
        /// 最も古い履歴が削除されていることを確認する
        /// </summary>
        /// <param name="kokyakuKaisyaSansyouRirekis">確認対象の顧客会社参照履歴リスト</param>
        private async Task AssertOldestRirekiDeletedAsync(List<KokyakuKaisyaSansyouRireki> kokyakuKaisyaSansyouRirekis)
        {
            var oldestRireki = kokyakuKaisyaSansyouRirekis.OrderBy(x => x.SansyouTime).First();
            var existsOldest = await db.KokyakuKaisyaSansyouRirekis
                .AnyAsync(x => x.Id == oldestRireki.Id);
            Assert.IsFalse(existsOldest, "最も古い履歴が削除されていません。");
        }
    }
}