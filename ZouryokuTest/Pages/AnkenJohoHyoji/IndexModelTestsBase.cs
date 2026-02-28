using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.AnkenJohoHyoji;

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

        protected readonly DateOnly today = new(2025, 7, 1);

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
    }
}
