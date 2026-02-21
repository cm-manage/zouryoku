using Model.Model;
using Zouryoku.Pages.YukyuKeikakuToroku;
using static Model.Enums.LeavePlanStatus;

namespace ZouryokuTest.Pages.YukyuKeikakuToroku
{
    /// <summary>
    /// 計画有給休暇登録画面のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetTests : IndexModelTestsBase
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 正常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 正常:
        /// #01 初期表示
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
        /// 社員BaseID：ログインユーザーのBaseID」
        /// </summary>
        [TestMethod(DisplayName = """
            #01 初期表示
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
            社員BaseID：ログインユーザーのBaseID」
            """)]
        public async Task OnGetAsync_計画有給休暇データあり_今年度_ログインユーザー()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(未申請, loginUserSyain, yukyuNendoOfThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            await model.OnGetAsync(); // Act
            AssertPopulatesList(yukyuNendoOfThisYear, model.LoginUsersYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 正常:
        /// #02 初期表示
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
        /// 社員BaseID：ログインユーザーのBaseID」
        /// </summary>
        [TestMethod(DisplayName = """
            #02 初期表示
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
            社員BaseID：ログインユーザーのBaseID」
            """)]
        public async Task OnGetAsync_計画有給休暇データあり_非今年度_ログインユーザー()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            var yukyuNendoOfNotThisYear = AddYukyuNendoOfNotThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(loginUserSyain, yukyuNendoOfNotThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            await model.OnGetAsync(); // Act
            AssertSetsEmptyList(yukyuNendoOfThisYear, model.LoginUsersYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 正常:
        /// #03 初期表示
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
        /// 社員BaseID：**非**ログインユーザーのBaseID」
        /// </summary>
        [TestMethod(DisplayName = """
            #03 初期表示
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
            社員BaseID：**非**ログインユーザーのBaseID」
            """)]
        public async Task OnGetAsync_計画有給休暇データ_今年度_非ログインユーザー()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var notLoginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(notLoginUserSyain, yukyuNendoOfThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            await model.OnGetAsync(); // Act
            AssertSetsEmptyList(yukyuNendoOfThisYear, model.LoginUsersYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 正常:
        /// #04 初期表示
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
        /// 社員BaseID：**非**ログインユーザーのBaseID」
        /// </summary>
        [TestMethod(DisplayName = """
            #04 初期表示
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
            社員BaseID：**非**ログインユーザーのBaseID」
            """)]
        public async Task OnGetAsync_計画有給休暇データ_非今年度_非ログインユーザー()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var notLoginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            var yukyuNendoOfNotThisYear = AddYukyuNendoOfNotThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(notLoginUserSyain, yukyuNendoOfNotThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            await model.OnGetAsync(); // Act
            AssertSetsEmptyList(yukyuNendoOfThisYear, model.LoginUsersYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 正常:
        /// #05 初期表示
        /// 計画有給休暇データ**なし**
        /// </summary>
        [TestMethod(DisplayName = """
            #05 初期表示
            計画有給休暇データ**なし**
            """)]
        public async Task OnGetAsync_計画有給休暇データなし()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            await model.OnGetAsync(); // Act
            AssertSetsEmptyList(yukyuNendoOfThisYear, model.LoginUsersYukyuKeikaku); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Assert用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 有効系アサーション
        /// </summary>
        private static void AssertPopulatesList(YukyuNendo yukyuNendoOfThisYear, IndexModel.YukyuKeikakuViewModel? actualYukyuKeikakuModel)
        {
            Assert.IsNotNull(actualYukyuKeikakuModel);

            // 計画有給休暇のステータスが取得されること
            Assert.AreEqual(未申請, actualYukyuKeikakuModel.YukyuKeikakuStatus, "Status が一致しません。");

            // 計画有給休暇明細「計画有給ID：計画有給休暇のID」のID・計画有給年月日・特休フラグの7件が取得されること
            // 計画有給休暇明細の計画有給年月日の昇順で並ぶこと
            var expectedMeisais = new[]
            {
                new IndexModel.Meisai { Id = 7, Ymd = new DateOnly(2024, 11, 1), IsTokukyu = false },
                new IndexModel.Meisai { Id = 3, Ymd = new DateOnly(2024, 11, 2), IsTokukyu = false },
                new IndexModel.Meisai { Id = 5, Ymd = new DateOnly(2024, 11, 3), IsTokukyu = false },
                new IndexModel.Meisai { Id = 4, Ymd = new DateOnly(2024, 11, 4), IsTokukyu = false },
                new IndexModel.Meisai { Id = 6, Ymd = new DateOnly(2024, 11, 5), IsTokukyu = false },
                new IndexModel.Meisai { Id = 2, Ymd = new DateOnly(2024, 11, 6), IsTokukyu = true },
                new IndexModel.Meisai { Id = 1, Ymd = new DateOnly(2024, 11, 7), IsTokukyu = true }
            };
            AssertAreEqual(expectedMeisais, actualYukyuKeikakuModel.Meisais);

            // 有給年度「今年度フラグ：TRUE」の開始日・終了日が取得されること
            Assert.AreEqual(
                yukyuNendoOfThisYear.StartDate, actualYukyuKeikakuModel.YukyuNendoStartDate, "YukyuNendoStartDate が一致しません。");
            Assert.AreEqual(
                yukyuNendoOfThisYear.EndDate, actualYukyuKeikakuModel.YukyuNendoEndDate, "YukyuNendoEndDate が一致しません。");
        }

        /// <summary>
        /// 無効系アサーション
        /// </summary>
        private static void AssertSetsEmptyList(
            YukyuNendo yukyuNendoOfThisYear, IndexModel.YukyuKeikakuViewModel? actualYukyuKeikakuViewModel)
        {
            Assert.IsNotNull(actualYukyuKeikakuViewModel);

            // 計画有給休暇のステータスが空となること
            Assert.IsNull(actualYukyuKeikakuViewModel.YukyuKeikakuStatus, "Status が一致しません。");

            // 計画有給休暇明細のID・計画有給年月日が空の7件が取得されること
            // 計画有給休暇明細の特休フラグがFALSEの7件が取得されること
            var expectedMeisais = new[]
            {
                new IndexModel.Meisai { Ymd = null, IsTokukyu = false },
                new IndexModel.Meisai { Ymd = null, IsTokukyu = false },
                new IndexModel.Meisai { Ymd = null, IsTokukyu = false },
                new IndexModel.Meisai { Ymd = null, IsTokukyu = false },
                new IndexModel.Meisai { Ymd = null, IsTokukyu = false },
                new IndexModel.Meisai { Ymd = null, IsTokukyu = false },
                new IndexModel.Meisai { Ymd = null, IsTokukyu = false }
            };
            AssertAreEqual(expectedMeisais, actualYukyuKeikakuViewModel.Meisais);

            // 有給年度「今年度フラグ：TRUE」の開始日・終了日が取得されること
            Assert.AreEqual(
                yukyuNendoOfThisYear.StartDate, actualYukyuKeikakuViewModel.YukyuNendoStartDate, "YukyuNendoStartDate が一致しません。");
            Assert.AreEqual(
                yukyuNendoOfThisYear.EndDate, actualYukyuKeikakuViewModel.YukyuNendoEndDate, "YukyuNendoEndDate が一致しません。");
        }
    }
}
