using Zouryoku.Pages.DakokuJikanSyusei;

namespace ZouryokuTest.Pages.DakokuJikanSyusei
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
    }
}
