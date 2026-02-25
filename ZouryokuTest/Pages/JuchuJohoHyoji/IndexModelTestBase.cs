using Microsoft.EntityFrameworkCore;
using Model.Model;
using System.Globalization;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.JuchuJohoHyoji;
using ZouryokuTest.Builder;
using ZouryokuTest.Pages.Builder;

namespace ZouryokuTest.Pages.JuchuJohoHyoji
{
    public class IndexModelTestBase : BaseInMemoryDbContextTest
    {
        // =============================================
        // 定数
        // =============================================
        /// <summary>
        /// ログインユーザーの社員BaseID
        /// </summary>
        protected const long LoggedInUserId = 999;

        /// <summary>
        /// ログインユーザーの社員コード
        /// </summary>
        protected const string LoggedInUserCode = "00000";

        /// <summary>
        /// ログインユーザーの社員名
        /// </summary>
        protected const string LoggedInUserName = "社員A";

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
        /// シード: 受注情報作成
        /// </summary>
        protected static KingsJuchu CreateKingsJuchu(long id)
        {
            return new KingsJuchuBuilder()
                .WithId(id)
                .Build();
        }

        /// <summary>
        /// シード: 部署情報作成
        /// </summary>
        protected static Busyo CreateBusyo(long id)
        {
            return new BusyoBuilder()
                .WithId(id)
                .Build();
        }

        /// <summary>
        /// シード: 受注参照履歴作成
        /// </summary>
        protected static KingsJuchuSansyouRireki CreateKingsJuchuSansyouRireki(long id)
        {
            return new KingsJuchuSansyouRirekiBuilder()
                .WithId(id)
                .Build();
        }

        /// <summary>
        /// シード: 受注参照履歴を複数生成
        /// </summary>
        /// <param name="syainBaseId">社員BaseID</param>
        /// <param name="count">生成件数</param>
        protected static List<KingsJuchuSansyouRireki> CreateKingsJuchuSansyouRireki(long syainBaseId, int count)
        {
            var baseTime = DateTime.ParseExact(
                "2025/04/01 09:00",
                "yyyy/MM/dd HH:mm",
                CultureInfo.InvariantCulture
                );

            return Enumerable.Range(1, count)
                .Select(i => new KingsJuchuSansyouRirekiBuilder()
                    .WithId(i)
                    .WithKingsJuchuId(i)
                    .WithSyainBaseId(syainBaseId)
                    .WithSansyouTime(baseTime.AddMinutes(i - 1))
                    .Build()
                    )
                .ToList();
        }

        /// <summary>
        /// シード: 別ユーザーの受注参照履歴を複数生成
        /// </summary>
        /// <param name="juchuId">受注ID</param>
        /// <param name="startId">一番最初の参照履歴ID</param>
        protected static List<KingsJuchuSansyouRireki> CreateOtherKingsJuchuSansyouRireki(long juchuId, int startId)
        {
            return new KingsJuchuSansyouRirekiBuilder()
                .WithKingsJuchuId(juchuId)
                .WithSyainBaseId(0) // ダミー
                .BuildMany(startId, 3, data =>
                {
                    data.SansyouTime = DateTime.Now.AddMinutes(+data.Id); // IDの小さい順に古い日時とする
                });
        }

        /// <summary>
        /// 参照時間が指定範囲内であることを確認する
        /// </summary>
        /// <param name="juchuSansyouRireki">確認対象の受注参照履歴</param>
        /// <param name="beforeUpdateTime">更新前の時間</param>
        /// <param name="afterUpdateTime">更新後の時間</param>
        protected static void AssertSansyouTime(
            KingsJuchuSansyouRireki juchuSansyouRireki,
            DateTime now)
        {
            Assert.AreEqual(
                juchuSansyouRireki.SansyouTime,
                now,
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
            var count = await db.KingsJuchuSansyouRirekis
                .CountAsync(x => x.SyainBaseId != loginSyainBaseId);
            Assert.AreEqual(expectedCount, count, "別ユーザーの参照履歴に変化があります。");
        }

        /// <summary>
        /// 最も古い履歴が削除されていることを確認する
        /// </summary>
        /// <param name="juchuSansyouRirekis">確認対象の受注参照履歴リスト</param>
        protected async Task AssertOldestRirekiDeletedAsync(List<KingsJuchuSansyouRireki> juchuSansyouRirekis)
        {
            var oldestRireki = juchuSansyouRirekis.OrderBy(x => x.SansyouTime).First();
            var existsOldest = await db.KingsJuchuSansyouRirekis
                .AnyAsync(x => x.Id == oldestRireki.Id);
            Assert.IsFalse(existsOldest, "最も古い履歴が削除されていません。");
        }

        /// <summary>
        /// 更新対象外の受注参照履歴.参照時間が更新されていないことを確認する
        /// </summary>
        /// <param name="juchuSansyouRirekis">確認対象の受注参照履歴リスト</param>
        protected static void AssertOtherRirekiNotUpdated(List<KingsJuchuSansyouRireki> juchuSansyouRirekis, DateTime now)
        {
            foreach (var other in juchuSansyouRirekis)
            {
                Assert.AreNotEqual(
                    now,
                    other.SansyouTime,
                    "対象外の受注参照履歴が更新されています。"
                    );
            }
        }
    }
}
