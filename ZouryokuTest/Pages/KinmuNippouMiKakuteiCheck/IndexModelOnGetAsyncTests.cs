using Model.Model;
using Zouryoku.Pages.KinmuNippouMiKakuteiCheck;
using static Model.Enums.EmployeeAuthority;
using static Zouryoku.Pages.KinmuNippouMiKakuteiCheck.IndexModel.BusyoRange;

namespace ZouryokuTest.Pages.KinmuNippouMiKakuteiCheck
{
    /// <summary>
    /// <see cref="IndexModel.OnGetAsync"/>のテストクラス。
    /// </summary>
    [TestClass]
    public class IndexModelOnGetAsyncTests : TestBase
    {
        // ======================================
        // テストメソッド
        // ======================================

        // 検索条件の初期化
        // --------------------------------------

        [TestMethod]
        public async Task OnGetAsync_検索条件初期化_ログインユーザーの部署IDが設定されている()
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 2, 10));
            var model = CreateIndexModel();

            // 先月分の確定期限をモックしておく
            db.Add(new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new(2026, 2, 3)
            });
            db.SaveChanges();

            // Act
            // ----------------------------------

            await model.OnGetAsync();

            // Assert
            // ----------------------------------

            // 部署ID
            Assert.AreEqual(LoginUser.Busyo.Id, model.SearchConditions.Busyo.Id);
        }

        [TestMethod]
        public async Task OnGetAsync_検索条件初期化_ログインユーザーの部署名が設定されている()
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 2, 10));
            var model = CreateIndexModel();

            // 先月分の確定期限をモックしておく
            db.Add(new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new(2026, 2, 3)
            });
            db.SaveChanges();

            // Act
            // ----------------------------------

            await model.OnGetAsync();

            // Assert
            // ----------------------------------

            // 部署名
            Assert.AreEqual(LoginUser.Busyo.Name, model.SearchConditions.Busyo.Name);
        }

        [TestMethod]
        public async Task OnGetAsync_検索条件初期化_検索範囲が部署に設定されている()
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 2, 10));
            var model = CreateIndexModel();

            // 先月分の確定期限をモックしておく
            db.Add(new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new(2026, 2, 3)
            });
            db.SaveChanges();

            // Act
            // ----------------------------------

            await model.OnGetAsync();

            // Assert
            // ----------------------------------

            // 検索範囲
            Assert.AreEqual(部署, model.SearchConditions.Busyo.Range);
        }

        [TestMethod]
        [DataRow(5, DisplayName = "境界値: 前実績期間の確定期限の翌営業日の翌日")]
        [DataRow(12, DisplayName = "代表値: 前実績期間の確定期限の翌営業日の翌々日から確定期限の翌営業日前日")]
        [DataRow(18, DisplayName = "境界値: 確定期限の翌営業日")]
        public async Task OnGetAsync_検索条件初期化_日付が適切な実績締め日に設定されている(int nowDay)
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 2, nowDay));
            var model = CreateIndexModel();

            // 先月分の確定期限をモックしておく
            db.Add(new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new(2026, 2, 3)
            });
            // 当月の中締めを設定
            db.Add(new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new(2026, 2, 17)
            });
            db.SaveChanges();

            // Act
            // ----------------------------------

            await model.OnGetAsync();

            // Assert
            // ----------------------------------

            // 日付
            Assert.AreEqual(new DateOnly(2026, 2, 15), model.SearchConditions.Date);
        }

        // 通知可能フラグの設定
        // --------------------------------------

        [TestMethod]
        public async Task OnGetAsync_ログインユーザーが権限を持っていて通知可能な期間_通知可能フラグがtrue()
        {
            // Arrange
            // ----------------------------------

            // 確定期限の翌営業日
            fakeTimeProvider.SetLocalNow(new(2026, 2, 4));
            var model = CreateIndexModel(勤務日報未確定チェック);

            // 先月分の確定期限をモックしておく
            db.Add(new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new(2026, 2, 3)
            });
            db.SaveChanges();

            // Act
            // ----------------------------------

            await model.OnGetAsync();

            // Assert
            // ----------------------------------

            // 通知可能フラグ
            Assert.IsTrue(model.CanNotify);
        }

        [TestMethod]
        [DataRow(false, 4, DisplayName = "権限を持たないとき")]
        [DataRow(true, 5, DisplayName = "通知可能期間外のとき")]
        public async Task OnGetAsync_ログインユーザーが権限を持っていないか通知可能な期間外_通知可能フラグがfalse(bool hasAuthority, int nowDay)
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 2, nowDay));
            var model = CreateIndexModel(hasAuthority ? 勤務日報未確定チェック : None);

            // 先月分の確定期限をモックしておく
            db.Add(new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new(2026, 2, 3)
            });
            db.SaveChanges();

            // Act
            // ----------------------------------

            await model.OnGetAsync();

            // Assert
            // ----------------------------------

            // 通知可能フラグ
            Assert.IsFalse(model.CanNotify);
        }
    }
}