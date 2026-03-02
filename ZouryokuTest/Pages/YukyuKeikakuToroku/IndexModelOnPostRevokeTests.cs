using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Pages.YukyuKeikakuToroku;
using static Model.Enums.LeavePlanStatus;

namespace ZouryokuTest.Pages.YukyuKeikakuToroku
{
    /// <summary>
    /// 計画有給休暇登録画面のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnPostRevokeTests : IndexModelTestsBase
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 異常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 異常:
        /// #21 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
        /// 社員BaseID：ログインユーザーのBaseID」
        /// 更新リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #21 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
            社員BaseID：ログインユーザーのBaseID」
            更新リクエストが送信された場合
            """)]
        public async Task OnPostRevokeAsync_計画有給休暇データあり_非今年度_ログインユーザー_更新リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var yukyuNendoOfNotThisYear = AddYukyuNendoOfNotThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(loginUserSyain, yukyuNendoOfNotThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateUpdateRequest();

            var result = await OnPostRevokeAsync(model, request); // Act
            AssertErrorJson(result, IndexModel.ErrorConflictYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 異常:
        /// #22 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
        /// 社員BaseID：**非**ログインユーザーのBaseID」
        /// 更新リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #22 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
            社員BaseID：**非**ログインユーザーのBaseID」
            更新リクエストが送信された場合
            """)]
        public async Task OnPostRevokeAsync_計画有給休暇データあり_今年度_非ログインユーザー_更新リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var notLoginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(notLoginUserSyain, yukyuNendoOfThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateUpdateRequest();

            var result = await OnPostRevokeAsync(model, request); // Act
            AssertErrorJson(result, IndexModel.ErrorConflictYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 異常:
        /// #23 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
        /// 社員BaseID：**非**ログインユーザーのBaseID」
        /// 更新リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #23 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
            社員BaseID：**非**ログインユーザーのBaseID」
            更新リクエストが送信された場合
            """)]
        public async Task OnPostRevokeAsync_計画有給休暇データあり_非今年度_非ログインユーザー_更新リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var notLoginUserSyain = AddNewSyain();
            var yukyuNendoOfNotThisYear = AddYukyuNendoOfNotThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(notLoginUserSyain, yukyuNendoOfNotThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateUpdateRequest();

            var result = await OnPostRevokeAsync(model, request); // Act
            AssertErrorJson(result, IndexModel.ErrorConflictYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 異常:
        /// #24 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**なし**
        /// 更新リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #24 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**なし**
            更新リクエストが送信された場合
            """)]
        public async Task OnPostRevokeAsync_計画有給休暇データなし_更新リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateUpdateRequest();

            var result = await OnPostRevokeAsync(model, request); // Act
            AssertErrorJson(result, IndexModel.ErrorConflictYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 異常:
        /// #25 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
        /// 社員BaseID：ログインユーザーのBaseID」
        /// 更新リクエストによって同時実行制御が発動した場合
        /// </summary>
        [TestMethod(DisplayName = """
            #25 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
            社員BaseID：ログインユーザーのBaseID」
            更新リクエストによって同時実行制御が発動した場合
            """)]
        public async Task OnPostRevokeAsync_計画有給休暇データあり_今年度_ログインユーザー_更新リクエストによって同時実行制御が発動した場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            var yukyuKeikaku = AddYukyuKeikakuAndMeisaiWithShuffledYmds(loginUserSyain, yukyuNendoOfThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateUpdateRequest(yukyuKeikaku.Version + 1); // バージョン不整合を発生させる

            var result = await OnPostRevokeAsync(model, request); // Act
            AssertErrors(result, IndexModel.ErrorConflictYukyuKeikaku); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 正常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 正常:
        /// #26 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
        /// 社員BaseID：ログインユーザーのBaseID」
        /// 更新リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #26 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
            社員BaseID：ログインユーザーのBaseID」
            更新リクエストが送信された場合
            """)]
        public async Task OnPostRevokeAsync_計画有給休暇データあり_今年度_ログインユーザー_更新リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(loginUserSyain, yukyuNendoOfThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateUpdateRequest();

            var result = await OnPostRevokeAsync(model, request); // Act
            await AssertUpdatesExistingRecord(loginUserSyain, result); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Arrange用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 更新系リクエストの作成
        /// </summary>
        private static IndexModel.YukyuKeikakuViewModel CreateUpdateRequest(uint version = default) => CreateRequest(
            version,
            new IndexModel.Meisai { Ymd = null, IsTokukyu = false },
            new IndexModel.Meisai { Ymd = null, IsTokukyu = false },
            new IndexModel.Meisai { Ymd = null, IsTokukyu = false },
            new IndexModel.Meisai { Ymd = null, IsTokukyu = false },
            new IndexModel.Meisai { Ymd = null, IsTokukyu = false },
            new IndexModel.Meisai { Ymd = null, IsTokukyu = false },
            new IndexModel.Meisai { Ymd = null, IsTokukyu = false });

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Act用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private static async Task<IActionResult> OnPostRevokeAsync(IndexModel model, IndexModel.YukyuKeikakuViewModel request)
        {
            model.LoginUsersYukyuKeikaku = request;
            return await model.OnPostRevokeAsync();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Assert用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 更新系アサーション
        /// </summary>
        private async Task AssertUpdatesExistingRecord(Syain loginUserSyain, IActionResult actualResult)
        {
            // 成功レスポンスが返却されること
            AssertSuccess(actualResult);

            // 計画有給休暇
            // 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：ログインユーザーのBaseID」
            // ステータスに「未申請」が設定されること
            var yukyuKeikaku = await db.YukyuKeikakus
                .Include(yk => yk.YukyuKeikakuMeisais)
                .Where(yk => yk.YukyuNendo.IsThisYear && yk.SyainBaseId == loginUserSyain.SyainBaseId)
                .SingleAsync();
            Assert.AreEqual(未申請, yukyuKeikaku.Status, "ステータスが一致しません。");
        }
    }
}
