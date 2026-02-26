using Model.Model;
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
