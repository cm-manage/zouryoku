using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.KokyakuJohoHyoji;
using static Model.Enums.BusinessTripRole;
using static Model.Enums.EmployeeAuthority;

namespace ZouryokuTest.Pages.KokyakuJohoHyoji
{
    public class IndexModelTestBase : BaseInMemoryDbContextTest
    {
        /// <summary>
        /// ログインユーザーの社員BASE ID
        /// </summary>
        protected const long LoggedInUserId = 999;

        /// <summary>
        /// ログインユーザーの社員コード
        /// </summary>
        private const string LoggedInUserCode = "00000";

        /// <summary>
        /// ログインユーザーの社員名
        /// </summary>
        private const string LoggedInUserName = "社員A";

        /// <summary>
        /// <see cref="IndexModel"/>インスタンスの作成
        /// </summary>
        protected IndexModel CreateModel()
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, fakeTimeProvider)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData(),
            };

            return SetLoggedInUser(model);
        }

        /// <summary>
        /// モデルのセッションにログイン情報を作成する
        /// </summary>
        /// <param name="model">ログイン情報を作成するモデル</param>
        /// <returns>ログイン情報が格納されたモデル</returns>
        protected IndexModel SetLoggedInUser(IndexModel model)
        {
            // ログインユーザーの社員情報を追加
            var empBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };
            db.SyainBases.Add(empBase);

            var emp = new Syain()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
                KanaName = LoggedInUserName,
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = LoggedInUserId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };
            db.Syains.Add(emp);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = emp };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            return model;
        }

        // ================================================
        // Helper Method
        // ================================================

        /// <summary>
        /// シード処理
        /// </summary>
        protected void SeedEntities(params object[] entities)
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
        /// <param name="now">現在の日時</param>
        protected static void AssertSansyouTime(
            KokyakuKaisyaSansyouRireki kokyakuKaisyaSansyouRireki,
            DateTime now)
        {
            Assert.AreEqual(
                now,
                kokyakuKaisyaSansyouRireki.SansyouTime,
                "参照時間が正しく更新されていません。"
                );
        }

        /// <summary>
        /// 別ユーザーの参照履歴件数を確認する
        /// </summary>
        /// <param name="loginSyainBaseId">ログインユーザーの社員BaseID</param>
        /// <param name="expectedCount">期待する別ユーザーの参照履歴件数</param>
        protected async Task AssertOtherUserRirekiCountAsync(long loginSyainBaseId, int expectedCount)
        {
            var count = await db.KokyakuKaisyaSansyouRirekis
                .CountAsync(x => x.SyainBaseId != loginSyainBaseId);
            Assert.AreEqual(expectedCount, count, "別ユーザーの参照履歴に変化があります。");
        }

        /// <summary>
        /// 最も古い履歴が削除されていることを確認する
        /// </summary>
        /// <param name="kokyakuKaisyaSansyouRirekis">確認対象の顧客会社参照履歴リスト</param>
        protected async Task AssertOldestRirekiDeletedAsync(List<KokyakuKaisyaSansyouRireki> kokyakuKaisyaSansyouRirekis)
        {
            var oldestRireki = kokyakuKaisyaSansyouRirekis.OrderBy(x => x.SansyouTime).First();
            var existsOldest = await db.KokyakuKaisyaSansyouRirekis
                .AnyAsync(x => x.Id == oldestRireki.Id);
            Assert.IsFalse(existsOldest, "最も古い履歴が削除されていません。");
        }

        /// <summary>
        /// 更新対象外の顧客会社参照履歴.参照時間が更新されていないことを確認する
        /// </summary>
        /// <param name="kokyakuKaisyaSansyouRirekis">確認対象の顧客会社参照履歴リスト</param>
        /// <param name="now">現在の日時</param>
        protected static void AssertOtherRirekiNotUpdated(List<KokyakuKaisyaSansyouRireki> kokyakuKaisyaSansyouRirekis, DateTime now)
        {
            foreach (var other in kokyakuKaisyaSansyouRirekis)
            {
                Assert.AreNotEqual(
                    now,
                    other.SansyouTime,
                    "対象外の顧客参照履歴が更新されています。"
                    );
            }
        }
    }
}
