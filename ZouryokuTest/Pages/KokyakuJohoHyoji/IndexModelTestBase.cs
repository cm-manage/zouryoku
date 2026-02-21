using Microsoft.EntityFrameworkCore;
using Model.Model;
using System.Globalization;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.KokyakuJohoHyoji;
using ZouryokuTest.Builder;
using ZouryokuTest.Pages.Builder;

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
            var model = new IndexModel(db, GetLogger<IndexModel>(), options)
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
            var empBase = new SyainBasisBuilder()
                .WithId(LoggedInUserId)
                .WithCode(LoggedInUserCode)
                .WithName(LoggedInUserName)
                .Build();
            db.SyainBases.Add(empBase);

            var emp = new SyainBuilder()
                .WithId(LoggedInUserId)
                .WithCode(LoggedInUserCode)
                .WithSyainBaseId(LoggedInUserId)
                .WithName(LoggedInUserName)
                .WithKanaName(LoggedInUserName)
                .Build();
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
        /// シード: 社員Base生成
        /// </summary>
        protected static SyainBasis CreateSyainBase(long id)
        {
            return new SyainBasisBuilder()
                .WithId(id)
                .Build();
        }

        /// <summary>
        /// シード: 社員生成
        /// </summary>
        protected static Syain CreateSyain(long id)
        {
            return new SyainBuilder()
                .WithId(id)
                .Build();
        }

        /// <summary>
        /// シード: 顧客会社生成
        /// </summary>
        protected static KokyakuKaisha CreateKokyakuKaisha(long id)
        {
            return new KokyakuKaishaBuilder()
                .WithId(id)
                .Build();
        }

        /// <summary>
        /// シード: 部署作成
        /// </summary>
        protected static Busyo CreateBusyo(long id)
        {
            return new BusyoBuilder()
                .WithId(id)
                .Build();
        }

        /// <summary>
        /// シード: 業種作成
        /// </summary>
        protected static Gyousyu CreateGyousyu(long id)
        {
            return new GyousyuBuilder()
                .WithId(id)
                .Build();
        }

        /// <summary>
        /// シード: ログインユーザーの顧客会社参照履歴を複数生成
        /// </summary>
        /// <param name="syainBaseId">社員BaseID</param>
        /// <param name="count">生成件数</param>
        protected static List<KokyakuKaisyaSansyouRireki> CreateKokyakuKaisyaSansyouRireki(long syainBaseId, int count)
        {
            var baseTime = DateTime.ParseExact(
                "2025/04/01 09:00",
                "yyyy/MM/dd HH:mm",
                CultureInfo.InvariantCulture
                );

            return Enumerable.Range(1, count)
                .Select(i => new KokyakuKaisyaSansyouRirekiBuilder()
                    .WithId(i)
                    .WithKokyakuKaisyaId(i)
                    .WithSyainBaseId(syainBaseId)
                    .WithSansyouTime(baseTime.AddMinutes(i - 1))
                    .Build()
                    )
                .ToList();
        }

        /// <summary>
        /// シード: 別ユーザーの顧客会社参照履歴を複数生成
        /// </summary>
        /// <param name="kokyakuId">顧客会社ID</param>
        /// <param name="startId">一番最初の参照履歴ID</param>
        protected static List<KokyakuKaisyaSansyouRireki> CreateOtherKokyakuKaisyaSansyouRireki(long kokyakuId, int startId)
        {
            return new KokyakuKaisyaSansyouRirekiBuilder()
                .WithKokyakuKaisyaId(kokyakuId)
                .WithSyainBaseId(0) // ダミー
                .BuildMany(startId, 3, data =>
                {
                    data.SansyouTime = DateTime.Now.AddMinutes(+data.Id); // IDの小さい順に古い日時とする
                });
        }

        /// <summary>
        /// 参照時間が指定範囲内であることを確認する
        /// </summary>
        /// <param name="kokyakuKaisyaSansyouRireki">確認対象の顧客会社参照履歴</param>
        /// <param name="beforeUpdateTime">更新前の時間</param>
        /// <param name="afterUpdateTime">更新後の時間</param>
        protected static void AssertSansyouTime(
            KokyakuKaisyaSansyouRireki kokyakuKaisyaSansyouRireki,
            DateTime beforeUpdateTime,
            DateTime afterUpdateTime)
        {
            Assert.IsTrue(
                beforeUpdateTime.AddSeconds(-2) <= kokyakuKaisyaSansyouRireki.SansyouTime
                && kokyakuKaisyaSansyouRireki.SansyouTime <= afterUpdateTime.AddSeconds(2),
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
        protected static void AssertOtherRirekiNotUpdated(List<KokyakuKaisyaSansyouRireki> kokyakuKaisyaSansyouRirekis, DateTime before, DateTime after)
        {
            foreach (var other in kokyakuKaisyaSansyouRirekis)
            {
                Assert.IsFalse(
                    before.AddSeconds(-2) < other.SansyouTime
                    && other.SansyouTime < after.AddSeconds(2),
                    "対象外の顧客参照履歴が更新されています。"
                    );
            }
        }
    }
}
