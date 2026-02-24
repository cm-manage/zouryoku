using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.AnkenMeiKensaku;

namespace ZouryokuTest.Pages.AnkenMeiKensaku
{
    /// <summary>
    /// <see cref="IndexModel"/>のテストに使用する補助クラス
    /// </summary>
    public class IndexModelTestBase : BaseInMemoryDbContextTest
    {
        // =====================================
        // 定数
        // =====================================

        // ログインユーザー情報
        // -------------------------------------

        /// <summary>
        /// ログインユーザーの社員BASE ID
        /// </summary>
        protected const long LoginUserSyainBaseId = 999;

        /// <summary>
        /// ログインユーザーの部署コード
        /// </summary>
        protected const string LoginUserBusyoCode = "000";

        // ======================================
        // プロパティ
        // ======================================

        /// <summary>
        /// テスト対象のモデル。
        /// TestInitializeでここにモデルを詰める。
        /// </summary>
        protected IndexModel? Model { get; set; }

        // ======================================
        // 補助メソッド
        // ======================================

        /// <summary>
        /// ログインユーザー情報を含んだ<see cref="IndexModel"/>インスタンスの作成
        /// </summary>
        /// <returns><see cref="Model"/>のインスタンス</returns>
        protected IndexModel CreateModel()
        {
            // IndexModelのインスタンスを作成
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine, fakeTimeProvider)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData(),
                SearchConditions = new()
                {
                    JuchuuNo = new(),
                    ChaYmd = new()
                }
            };

            // ログインユーザー用の社員BASEデータと社員データをインメモリDBに作成
            CreateSyainForLoginUser();
            db.SaveChanges();

            // モデルのセッションにLoginInfoを作成
            var loginUser = db.Syains
                .Single(emp => emp.SyainBaseId == LoginUserSyainBaseId);
            SetLoginUser(model, loginUser);

            return model;
        }

        /// <summary>
        /// ログインユーザー用の社員データを作成する
        /// </summary>
        protected void CreateSyainForLoginUser()
        {
            var syainBase = new SyainBasis
            {
                Id = LoginUserSyainBaseId,
                // 必要ないNOT NULLカラムには適当に値を詰める
                Code = string.Empty,
            };
            var syain = new Syain
            {
                SyainBase = syainBase,
                BusyoCode = LoginUserBusyoCode,
                // 必要ないNOT NULLカラムには適当に値を詰める
                Id = 999,
                Code = string.Empty,
                Name = string.Empty,
                KanaName = string.Empty,
                Seibetsu = '0',
                SyokusyuCode = 999,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 999,
                SyucyoSyokui = global::Model.Enums.BusinessTripRole._7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 999,
                IsGenkaRendou = false,
                Kengen = global::Model.Enums.EmployeeAuthority.None,
                Jyunjyo = 999,
                Retired = false,
                BusyoId = 999,
                KintaiZokuseiId = 999,
                UserRoleId = 999
            };
            db.Syains.Add(syain);
        }

        /// <summary>
        /// <paramref name="model"/>のセッションにログイン情報を作成する
        /// </summary>
        /// <param name="model">ログイン情報を作成するモデル</param>
        /// <param name="loginUser">ログインユーザーの<see cref="Syain"/>インスタンス</param>
        protected void SetLoginUser(IndexModel model, Syain loginUser)
        {
            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = loginUser };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);
        }
    }
}