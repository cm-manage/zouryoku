using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.AnkenJohoHyoji;
using ZouryokuTest.Builder;

namespace ZouryokuTest.Pages.AnkenJohoHyoji
{
    public abstract class IndexModelTestsBase : BaseInMemoryDbContextTest
    {
        /// <summary>
        /// 案件情報表示画面用の <see cref="IndexModel"/> を生成し、テスト実行に必要なコンテキスト情報を設定します。
        /// </summary>
        /// <returns>ページコンテキスト、TempData、およびログイン情報が設定された <see cref="IndexModel"/> インスタンス。</returns>
        protected IndexModel CreateModel(Syain? loginUser = null)
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine, fakeTimeProvider);
            model.PageContext = GetPageContext();
            model.TempData = GetTempData();
            if (loginUser != null)
            {
                // ログイン情報の設定
                model.HttpContext.Session.Set(new LoginInfo { User = loginUser });
            }
            return model;
        }

        protected static IndexModel.ViewModel CreateViewModel(bool canAdd, Anken anken, Syain loginUser)
            => new ()
            {
                CanAdd = canAdd,
                Anken = anken,
                LoginInfo = new LoginInfo { User = loginUser }
            };

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

        // ---------------------------------------------------------------------
        // Helper Methods
        // ---------------------------------------------------------------------

        /// <summary>
        /// シード：受注種類
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected static JyutyuSyurui CreateJyutyuSyurui(int id)
        {
            return new JyutyuSyuruiBuilder()
                .WithId(id)
                .WithName("受注種類A")
                .Build();
        }

        // シード：顧客会社
        protected static KokyakuKaisha CreateKokyakuKaisha(int id)
        {
            return new KokyakuKaishaBuilder()
                .WithId(id)
                .WithName("顧客会社A")
                .WithShiten("本店")
                .Build();
        }

        // シード：KINGS受注
        protected static KingsJuchu CreateKingsJuchu(long id, string? sekouBumonCd = null)
        {
            var builder = new KingsJuchuBuilder()
                .WithId(id)
                .WithProjectNo("PRJ-001")
                .WithJuchuuNo("JUCHU-001")
                .WithJuchuuGyoNo(1);

            if (sekouBumonCd is null)
            {
                return builder.Build();
            }

            // 施工部門コードを設定
            return builder.WithSekouBumonCd(sekouBumonCd).Build();
        }

        // シード：社員Base
        protected static SyainBasis CreateSyainBasis(long id)
        {
            return new SyainBasisBuilder()
                .WithId(id)
                .Build();
        }

        // シード：社員
        protected static Syain CreateSyain(long id, long syainBaseId)
        {
            return new SyainBuilder()
                .WithId(id)
                .WithSyainBaseId(syainBaseId)
                .WithName("社員A")
                .Build();
        }

        // シード：ログインユーザー
        protected static Syain CreateSyainLogin(long id, string? busyoCode = null)
        {
            var builder = new SyainBuilder()
                .WithId(id)
                .WithSyainBaseId(9999);

            if (busyoCode is null)
            {
                return builder.Build();
            }

            return builder.WithBusyoCode(busyoCode).Build();
        }
    }
}
