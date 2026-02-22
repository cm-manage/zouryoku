using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.RoleDefaultKengen;
using ZouryokuTest.Builder;

namespace ZouryokuTest.Pages.RoleDefaultKengen
{
    /// <summary>
    /// RoleDefaultKengenテストの基底クラスです。
    /// </summary>
    public class IndexModelTestsBase : BaseInMemoryDbContextTest
    {
        /// <summary>ログインユーザーの社員BASE ID</summary>
        protected const long LoggedInUserId = 999L;

        private const string LoggedInUserCode = "00000";
        private const string LoggedInUserName = "社員A";

        /// <summary>
        /// テスト対象の IndexModel を生成します。
        /// </summary>
        /// <returns>生成された IndexModel</returns>
        protected IndexModel CreateModel()
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData(),
                ViewModel = new IndexViewModel()
            };

            return SetLoggedInUser(model);
        }

        /// <summary>
        /// モデルにログインユーザーを設定します。
        /// </summary>
        /// <param name="model">設定対象の IndexModel</param>
        /// <returns>ログインユーザーが設定された IndexModel</returns>
        protected IndexModel SetLoggedInUser(IndexModel model)
        {
            var syainBaseEntity = new SyainBasisBuilder()
                .WithId(LoggedInUserId)
                .WithCode(LoggedInUserCode)
                .WithName(LoggedInUserName)
                .Build();
            db.SyainBases.Add(syainBaseEntity);

            var syainEntity = new SyainBuilder()
                .WithId(LoggedInUserId)
                .WithCode(LoggedInUserCode)
                .WithSyainBaseId(LoggedInUserId)
                .WithName(LoggedInUserName)
                .WithKanaName(LoggedInUserName)
                .Build();
            db.Syains.Add(syainEntity);

            var loginInfo = new LoginInfo { User = syainEntity };
            model.HttpContext.Session.Set(loginInfo);

            return model;
        }

        /// <summary>
        /// 指定IDのUserRoleエンティティを生成します。
        /// </summary>
        /// <param name="roleId">ロールID</param>
        /// <returns>生成された UserRole エンティティ</returns>
        protected static UserRole CreateUserRoleEntityById(long roleId)
        {
            return new UserRoleBuilder()
                .WithId(roleId)
                .Build();
        }

        /// <summary>
        /// テストデータをDBに登録します。
        /// </summary>
        /// <param name="entities">登録するエンティティ一覧</param>
        protected void SeedEntities(params object[] entities)
        {
            foreach (var entity in entities)
            {
                db.Add(entity);
            }

            db.SaveChanges();
        }
    }
}
