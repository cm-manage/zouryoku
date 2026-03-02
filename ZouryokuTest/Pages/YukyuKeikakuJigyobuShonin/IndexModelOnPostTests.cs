using Microsoft.AspNetCore.Mvc;
using Model.Model;
using Zouryoku.Pages.YukyuKeikakuJigyobuShonin;
using Zouryoku.Utils;
using static Model.Enums.EmployeeAuthority;
using static Model.Enums.LeavePlanStatus;

namespace ZouryokuTest.Pages.YukyuKeikakuJigyobuShonin
{
    /// <summary>
    /// 計画有給休暇事業部承認画面のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnPostTests : IndexModelTestsBase
    {
        private const string BumoncyoDisplayName = """
            ログインユーザー社員マスタ
            「ID：1、部署ID：1、社員権限：計画休暇承認(32768)」
            
            部署マスタデータあり
            「ID：1、部署Baseの部門長ID：1」
            """;

        private const string JinzaiDisplayName = """
            ログインユーザー社員マスタ
            「ID：1、部署ID：1、社員権限：指示最終承認者(8192)」
            
            部署マスタデータあり
            「ID：1、部署Baseの部門長ID：2」
            """;

        private const string BumoncyoAndJinzaiDisplayName = """
            ログインユーザー社員マスタ
            「ID：1、部署ID：1、社員権限：指示最終承認者(8192)」
            
            部署マスタデータあり
            「ID：1、部署Baseの部門長ID：1」
            """;

        private const string ErrorNotCheckedDisplayName = "チェックボックスが1件以上チェックされていない場合";

        private const string ErrorCircularBusyoDisplayName = """
            ログインユーザー社員マスタ
            「部署ID：1」

            部署マスタデータあり
            「ID：1、親ID：2」
            「ID：2、親ID：1」

            チェックボックスが1件以上チェックされている場合
            """;

        private const string ErrorOyaBusyoNotExistedDisplayName = """
            ログインユーザー社員マスタ
            「部署ID：1」
            
            部署マスタデータあり
            「ID：1、親ID：2」
            
            チェックボックスが1件以上チェックされている場合
            """;

        private const string ErrorBumoncyoNotExistedDisplayName = """
            ログインユーザー社員マスタ
            「部署ID：1」
            
            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            
            チェックボックスが1件以上チェックされている場合
            """;

        private const string ErrorUnauthorizedDisplayName = """
            ログインユーザー社員マスタ
            「ID：1、部署ID：1、社員権限：空」
            
            部署マスタデータあり
            「ID：1、部署Baseの部門長ID：2」
            
            チェックボックスが1件以上チェックされている場合
            """;

        private const string ErrorNotExistedDisplayName = """
            計画有給休暇データなし
            
            チェックボックスが1件以上チェックされている場合
            """;

        private const string ErrorConcurrencyDisplayName = """
            計画有給休暇データあり
            
            チェックボックスが1件以上チェックされている　かつ
            同時実行制御が発動した場合
            """;

        private const string CheckedDisplayName = """
            計画有給休暇データあり
            「チェック：TRUE」
            
            チェックボックスが1件以上チェックされている　かつ
            同時実行制御が発動していない場合
            """;

        private const string NotCheckedDisplayName = """
            計画有給休暇データあり
            「チェック：FALSE」

            チェックボックスが1件以上チェックされている　かつ
            同時実行制御が発動していない場合
            """;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 異常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 異常: #12 差し戻し/#29 承認
        /// </summary>
        [TestMethod]
        [DataRow(IndexModel.ActionType.SendBack, DisplayName = $"#12 差し戻し {ErrorNotCheckedDisplayName}")]
        [DataRow(IndexModel.ActionType.Approve, DisplayName = $"#29 承認 {ErrorNotCheckedDisplayName}")]
        public async Task OnPostAsync_チェックボックスが1件以上チェックされていない場合_エラー(IndexModel.ActionType actionType)
        {
            // Arrange
            var loginUserSyain = await SeedLoginUserSyain(true, false);
            loginUserSyain.Kengen = 計画休暇承認;
            var yukyuNendoOfThisYear = CreateYukyuNendo(true);
            var yukyuKeikaku = CreateYukyuKeikaku(loginUserSyain, yukyuNendoOfThisYear, 事業部承認待ち);
            db.AddRange(yukyuNendoOfThisYear, yukyuKeikaku);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateViewModelForRequest(false, (yukyuKeikaku, false));

            var result = await PostAsync(model, actionType, request); // Act
            AssertErrors(result, Const.ErrorNotChecked); // Assert
        }

        /// <summary>
        /// 異常: #13 差し戻し/#30 承認
        /// </summary>
        [TestMethod]
        [DataRow(IndexModel.ActionType.SendBack, DisplayName = $"""
            #13 差し戻し
            {ErrorCircularBusyoDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, DisplayName = $"""
            #30 承認
            {ErrorCircularBusyoDisplayName}
            """)]
        public async Task OnPostAsync_親子部署が循環した場合_エラー(IndexModel.ActionType actionType)
        {
            // Arrange
            var busyo1 = CreateBusyo();
            var busyo2 = CreateBusyo();
            busyo1.Oya = busyo2;
            busyo2.Oya = busyo1;
            var loginUserSyain = CreateSyainWithBusyo(busyo1);
            loginUserSyain.Kengen = 計画休暇承認;
            var yukyuNendoOfThisYear = CreateYukyuNendo(true);
            var yukyuKeikaku = CreateYukyuKeikaku(loginUserSyain, yukyuNendoOfThisYear, 事業部承認待ち);
            db.AddRange(busyo1, busyo2, loginUserSyain, yukyuNendoOfThisYear, yukyuKeikaku);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateViewModelForRequest(false, (yukyuKeikaku, true));

            var result = await PostAsync(model, actionType, request); // Act
            AssertErrorJson(result, string.Format(Const.ErrorRead, "部署マスタ")); // Assert
        }

        /// <summary>
        /// 異常: #14 差し戻し/#31 承認
        /// </summary>
        [TestMethod]
        [DataRow(IndexModel.ActionType.SendBack, DisplayName = $"""
            #14 差し戻し
            {ErrorOyaBusyoNotExistedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, DisplayName = $"""
            #31 差し戻し
            {ErrorOyaBusyoNotExistedDisplayName}
            """)]
        public async Task OnPostAsync_親部署が存在しない場合_エラー(IndexModel.ActionType actionType)
        {
            // Arrange
            var busyo1 = CreateBusyo();
            busyo1.OyaId = 2;
            var loginUserSyain = CreateSyainWithBusyo(busyo1);
            loginUserSyain.Kengen = 計画休暇承認;
            var yukyuNendoOfThisYear = CreateYukyuNendo(true);
            var yukyuKeikaku = CreateYukyuKeikaku(loginUserSyain, yukyuNendoOfThisYear, 事業部承認待ち);
            db.AddRange(busyo1, loginUserSyain, yukyuNendoOfThisYear, yukyuKeikaku);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateViewModelForRequest(false, (yukyuKeikaku, true));

            var result = await PostAsync(model, actionType, request); // Act
            AssertErrorJson(result, string.Format(Const.ErrorRead, "部署マスタ")); // Assert
        }

        /// <summary>
        /// 異常: #15 差し戻し/#32 承認
        /// </summary>
        [TestMethod]
        [DataRow(IndexModel.ActionType.SendBack, DisplayName = $"""
            #15 差し戻し
            {ErrorBumoncyoNotExistedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, DisplayName = $"""
            #32 承認
            {ErrorBumoncyoNotExistedDisplayName}
            """)]
        public async Task OnPostAsync_部門長が存在しない場合_エラー(IndexModel.ActionType actionType)
        {
            // Arrange
            var loginUserSyain = CreateSyainWithBusyo();
            loginUserSyain.Kengen = 計画休暇承認;
            var yukyuNendoOfThisYear = CreateYukyuNendo(true);
            var yukyuKeikaku = CreateYukyuKeikaku(loginUserSyain, yukyuNendoOfThisYear, 事業部承認待ち);
            db.AddRange(loginUserSyain, yukyuNendoOfThisYear, yukyuKeikaku);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateViewModelForRequest(false, (yukyuKeikaku, true));

            var result = await PostAsync(model, actionType, request); // Act
            AssertErrorJson(result, string.Format(Const.ErrorRead, "部署マスタ")); // Assert
        }

        /// <summary>
        /// 異常: #16 差し戻し/#33 承認
        /// </summary>
        [TestMethod]
        [DataRow(IndexModel.ActionType.SendBack, DisplayName = $"""
            #16 差し戻し
            {ErrorUnauthorizedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, DisplayName = $"""
            #33 承認
            {ErrorUnauthorizedDisplayName}
            """)]
        public async Task OnPostAsync_ログインユーザーが権限不適格の場合_エラー(IndexModel.ActionType actionType)
        {
            // Arrange
            var loginUserSyain = await SeedLoginUserSyain(false, false);
            var yukyuNendoOfThisYear = CreateYukyuNendo(true);
            var yukyuKeikaku = CreateYukyuKeikaku(loginUserSyain, yukyuNendoOfThisYear, 事業部承認待ち);
            db.AddRange(yukyuNendoOfThisYear, yukyuKeikaku);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateViewModelForRequest(false, (yukyuKeikaku, true));

            var result = await PostAsync(model, actionType, request); // Act
            AssertErrorJson(result, string.Format(Const.ErrorRegister, "ログインユーザー", "権限不適格")); // Assert
        }

        /// <summary>
        /// 異常: #17-#19 差し戻し/#34-#36 承認
        /// </summary>
        [TestMethod]
        [DataRow(IndexModel.ActionType.SendBack, true, false, DisplayName = $"""
            #17 差し戻し
            {BumoncyoDisplayName}
            {ErrorNotExistedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.SendBack, false, true, DisplayName = $"""
            #18 差し戻し
            {JinzaiDisplayName}
            {ErrorNotExistedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.SendBack, true, true, DisplayName = $"""
            #19 差し戻し
            {BumoncyoAndJinzaiDisplayName}
            {ErrorNotExistedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, true, false, DisplayName = $"""
            #34 承認
            {BumoncyoDisplayName}
            {ErrorNotExistedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, false, true, DisplayName = $"""
            #35 承認
            {JinzaiDisplayName}
            {ErrorNotExistedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, true, true, DisplayName = $"""
            #36 承認
            {BumoncyoAndJinzaiDisplayName}
            {ErrorNotExistedDisplayName}
            """)]
        public async Task OnPostAsync_計画有給休暇データなし_エラー(IndexModel.ActionType actionType, bool bumoncyo, bool jinzai)
        {
            // Arrange
            var loginUserSyain = await SeedLoginUserSyain(bumoncyo, jinzai);
            var model = CreateModel(loginUserSyain);
            var request = CreateViewModelForRequest(jinzai, (new YukyuKeikaku(), true));

            var result = await PostAsync(model, actionType, request); // Act
            AssertErrors(result, IndexModel.ErrorConflictReloadYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 異常: #20-#22 差し戻し/#37-#39 承認
        /// </summary>
        [TestMethod]
        [DataRow(IndexModel.ActionType.SendBack, true, false, DisplayName = $"""
            #20 差し戻し
            {BumoncyoDisplayName}
            {ErrorConcurrencyDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.SendBack, false, true, DisplayName = $"""
            #21 差し戻し
            {JinzaiDisplayName}
            {ErrorConcurrencyDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.SendBack, true, true, DisplayName = $"""
            #22 差し戻し
            {BumoncyoAndJinzaiDisplayName}
            {ErrorConcurrencyDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, true, false, DisplayName = $"""
            #37 承認
            {BumoncyoDisplayName}
            {ErrorConcurrencyDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, false, true, DisplayName = $"""
            #38 承認
            {JinzaiDisplayName}
            {ErrorConcurrencyDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, true, true, DisplayName = $"""
            #39 承認
            {BumoncyoAndJinzaiDisplayName}
            {ErrorConcurrencyDisplayName}
            """)]
        public async Task OnPostAsync_同時実行制御が発動した場合_エラー(IndexModel.ActionType actionType, bool bumoncyo, bool jinzai)
        {
            // Arrange
            var loginUserSyain = await SeedLoginUserSyain(bumoncyo, jinzai);
            var yukyuNendoOfThisYear = CreateYukyuNendo(true);
            var status = jinzai ? 人財承認待ち : 事業部承認待ち;
            var yukyuKeikaku = CreateYukyuKeikaku(loginUserSyain, yukyuNendoOfThisYear, status);
            db.AddRange(yukyuNendoOfThisYear, yukyuKeikaku);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateViewModelForRequest(jinzai, (yukyuKeikaku, true));
            request.Keikakus[0].Version = yukyuKeikaku.Version + 1; // バージョン不整合を発生させる

            var result = await PostAsync(model, actionType, request); // Act
            AssertErrors(result, IndexModel.ErrorConflictReloadYukyuKeikaku); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 正常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 正常: #23-#25 差し戻し/#40-#42 承認
        /// </summary>
        [TestMethod]
        [DataRow(IndexModel.ActionType.SendBack, true, false, DisplayName = $"""
            #23 差し戻し
            {BumoncyoDisplayName}
            {CheckedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.SendBack, false, true, DisplayName = $"""
            #24 差し戻し
            {JinzaiDisplayName}
            {CheckedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.SendBack, true, true, DisplayName = $"""
            #25 差し戻し
            {BumoncyoAndJinzaiDisplayName}
            {CheckedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, true, false, DisplayName = $"""
            #40 承認
            {BumoncyoDisplayName}
            {CheckedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, false, true, DisplayName = $"""
            #41 承認
            {JinzaiDisplayName}
            {CheckedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, true, true, DisplayName = $"""
            #42 承認
            {BumoncyoAndJinzaiDisplayName}
            {CheckedDisplayName}
            """)]
        public async Task OnPostAsync_1件チェック_チェックされている計画有給休暇データが更新される(
            IndexModel.ActionType actionType, bool bumoncyo, bool jinzai)
        {
            // Arrange
            var loginUserSyain = await SeedLoginUserSyain(bumoncyo, jinzai);
            var yukyuNendoOfThisYear = CreateYukyuNendo(true);
            var status = jinzai ? 人財承認待ち : 事業部承認待ち;
            var yukyuKeikaku = CreateYukyuKeikaku(loginUserSyain, yukyuNendoOfThisYear, status);
            db.AddRange(yukyuNendoOfThisYear, yukyuKeikaku);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateViewModelForRequest(jinzai, (yukyuKeikaku, true));

            var result = await PostAsync(model, actionType, request); // Act

            // Assert
            AssertSuccessJson(result);
            if (actionType == IndexModel.ActionType.Approve)
            {
                if (jinzai)
                {
                    Assert.AreEqual(承認済, yukyuKeikaku.Status);
                }
                else
                {
                    Assert.AreEqual(人財承認待ち, yukyuKeikaku.Status);
                }
            }
            else
            {
                Assert.AreEqual(未申請, yukyuKeikaku.Status);
            }
        }

        /// <summary>
        /// 正常: #26-#28 差し戻し/#43-#45 承認
        /// </summary>
        [TestMethod]
        [DataRow(IndexModel.ActionType.SendBack, true, false, DisplayName = $"""
            #26 差し戻し
            {BumoncyoDisplayName}
            {NotCheckedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.SendBack, false, true, DisplayName = $"""
            #27 差し戻し
            {JinzaiDisplayName}
            {NotCheckedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.SendBack, true, true, DisplayName = $"""
            #28 差し戻し
            {BumoncyoAndJinzaiDisplayName}
            {NotCheckedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, true, false, DisplayName = $"""
            #43 承認
            {BumoncyoDisplayName}
            {NotCheckedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, false, true, DisplayName = $"""
            #44 承認
            {JinzaiDisplayName}
            {NotCheckedDisplayName}
            """)]
        [DataRow(IndexModel.ActionType.Approve, true, true, DisplayName = $"""
            #45 承認
            {BumoncyoAndJinzaiDisplayName}
            {NotCheckedDisplayName}
            """)]
        public async Task OnPostAsync_1件チェック_チェックされていない計画有給休暇データが更新されない(
            IndexModel.ActionType actionType, bool bumoncyo, bool jinzai)
        {
            // Arrange
            var loginUserSyain = await SeedLoginUserSyain(bumoncyo, jinzai);
            var sameBusyoSyain = CreateSyainWithBusyo(loginUserSyain.Busyo);
            var yukyuNendoOfThisYear = CreateYukyuNendo(true);
            var status = jinzai ? 人財承認待ち : 事業部承認待ち;
            var yukyuKeikakuChecked = CreateYukyuKeikaku(loginUserSyain, yukyuNendoOfThisYear, status);
            var yukyuKeikakuNotChecked = CreateYukyuKeikaku(sameBusyoSyain, yukyuNendoOfThisYear, status);
            db.AddRange(sameBusyoSyain, yukyuNendoOfThisYear, yukyuKeikakuChecked, yukyuKeikakuNotChecked);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateViewModelForRequest(jinzai, (yukyuKeikakuChecked, true), (yukyuKeikakuNotChecked, false));

            var result = await PostAsync(model, actionType, request); // Act

            // Assert
            AssertSuccessJson(result);
            Assert.AreEqual(status, yukyuKeikakuNotChecked.Status);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Act用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Task<IActionResult> PostAsync(
            IndexModel model, IndexModel.ActionType actionType, IndexModel.JigyoubuShoninViewModel request)
        {
            if (actionType == IndexModel.ActionType.Approve) return model.OnPostApproveAsync(request);
            else return model.OnPostSendBackAsync(request);
        }
    }
}
