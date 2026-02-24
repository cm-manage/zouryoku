using Model.Enums;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.KinmuNippouMiKakuteiCheck;
using Zouryoku.Pages.Shared;
using static Model.Enums.BusinessTripRole;
using static Model.Enums.EmployeeAuthority;

namespace ZouryokuTest.Pages.KinmuNippouMiKakuteiCheck
{
    public class TestBase : BaseInMemoryDbContextTest
    {
        // ==========================================
        // フィールド
        // ==========================================

        /// <summary>
        /// ログインユーザーの社員マスタエンティティ
        /// </summary>
        protected Syain LoginUser = new()
        {
            Busyo = new()
            {
                Id = 999,
                Name = "ログイン部署",
                // 不要なNOT NULLカラムに値を詰める
                Code = string.Empty,
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 0,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                IsActive = true,
                BusyoBaseId = 0,
            },
            // 不要なNOT NULLカラムに値を詰める
            Id = 999,
            Code = string.Empty,
            Name = string.Empty,
            KanaName = string.Empty,
            Seibetsu = '0',
            BusyoCode = string.Empty,
            SyokusyuCode = 0,
            NyuusyaYmd = DateOnly.MinValue,
            StartYmd = DateOnly.MinValue,
            EndYmd = DateOnly.MaxValue,
            Kyusyoku = 0,
            SyucyoSyokui = _7_8級,
            KingsSyozoku = string.Empty,
            KaisyaCode = 0,
            IsGenkaRendou = false,
            Kengen = None,
            Jyunjyo = 0,
            Retired = false,
            SyainBaseId = 0,
            KintaiZokuseiId = 0,
            UserRoleId = 0,
        };

        // ======================================
        // 補助メソッド
        // ======================================

        /// <summary>
        /// テストするための<see cref="IndexModel"/>インスタンスを取得する。
        /// </summary>
        /// <param name="auth">ログインユーザーの権限</param>
        /// <returns>テスト用<see cref="IndexModel"/>インスタンス</returns>
        protected IndexModel CreateIndexModel(EmployeeAuthority auth = None)
        {
            // IndexModelのインスタンス
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine, fakeTimeProvider)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };

            // DBにログインユーザーの情報を登録する
            LoginUser.Kengen = auth;
            db.Add(LoginUser);
            db.SaveChanges();

            // ログイン情報のモック
            SetLoginInfo(model, LoginUser);

            return model;
        }

        /// <summary>
        /// モデルにログイン情報をモックする。
        /// </summary>
        /// <typeparam name="T">モデルの型</typeparam>
        /// <param name="model">モデル</param>
        /// <param name="syain">ログインユーザーの社員エンティティ</param>
        protected void SetLoginInfo<T>(BasePageModel<T> model, Syain syain)
        {
            // ログイン情報
            var loginInfo = new LoginInfo() { User = syain };
            model.HttpContext.Session.Set(loginInfo);
        }
    }
}
