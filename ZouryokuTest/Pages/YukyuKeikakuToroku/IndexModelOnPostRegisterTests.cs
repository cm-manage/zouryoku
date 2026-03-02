using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Pages.YukyuKeikakuToroku;
using Zouryoku.Utils;
using static Model.Enums.LeavePlanStatus;

namespace ZouryokuTest.Pages.YukyuKeikakuToroku
{
    /// <summary>
    /// 計画有給休暇登録画面のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnPostRegisterTests : IndexModelTestsBase
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 異常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 異常: #06 申請（ボタン押下）／再申請（ボタン押下） 単項目バリデーションに失敗している場合
        /// 実際の単項目バリデーションは <see cref="IndexModelViewModelTests"/> でテストする
        /// </summary>
        [TestMethod(DisplayName = "#06 申請（ボタン押下）／再申請（ボタン押下） 単項目バリデーションに失敗している場合")]
        public async Task OnPostRegisterAsync_単項目バリデーションに失敗している場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            model.ModelState.AddModelError("AnyKey", "AnyError");
            var request = CreateRequest();

            var result = await OnPostRegisterAsync(model, request); // Act

            // Assert
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result);
            var errors = GetErrors(jsonResult, "AnyKey");
            Assert.IsNotNull(errors, "エラーメッセージが設定されていません。");
            Assert.HasCount(1, errors, "エラーメッセージが一致しません。");
            Assert.AreEqual("AnyError", errors[0], "エラーメッセージが一致しません。");
        }

        /// <summary>
        /// 異常: #07 申請（ボタン押下）／再申請（ボタン押下） 特別休暇が1日分以下チェックされている場合
        /// </summary>
        [TestMethod(DisplayName = "#07 申請（ボタン押下）／再申請（ボタン押下） 特別休暇が1日分以下チェックされている場合")]
        public async Task OnPostRegisterAsync_特別休暇が1日分以下チェックされている場合()
        {
            // Arrange
            var model = CreateModel(new Syain());
            var request = CreateRequest(
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 1), IsTokukyu = true }, // チェックが1つのみ（エラーとなる想定）
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 2), IsTokukyu = false },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 3), IsTokukyu = false },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 4), IsTokukyu = false },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 5), IsTokukyu = false },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 6), IsTokukyu = false },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 7), IsTokukyu = false });

            var result = await OnPostRegisterAsync(model, request); // Act
            AssertErrors(result, Const.ErrorThereAreNotExactly2Tokukyus); // Assert
        }

        /// <summary>
        /// 異常: #08 申請（ボタン押下）／再申請（ボタン押下） 特別休暇が3日分以上チェックされている場合
        /// </summary>
        [TestMethod(DisplayName = "#08 申請（ボタン押下）／再申請（ボタン押下） 特別休暇が3日分以上チェックされている場合")]
        public async Task OnPostRegisterAsync_特別休暇が3日分以上チェックされている場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateRequest(
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 1), IsTokukyu = true },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 2), IsTokukyu = true },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 3), IsTokukyu = true }, // 特別休暇のチェック3日目（エラーとなる想定）
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 4), IsTokukyu = false },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 5), IsTokukyu = false },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 6), IsTokukyu = false },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 7), IsTokukyu = false });

            var result = await OnPostRegisterAsync(model, request); // Act
            AssertErrors(result, Const.ErrorThereAreNotExactly2Tokukyus); // Assert
        }

        /// <summary>
        /// 異常: #09 申請（ボタン押下）／再申請（ボタン押下） 休暇予定日に同じ日付が入力されている場合
        /// </summary>
        [TestMethod(DisplayName = "#09 申請（ボタン押下）／再申請（ボタン押下） 休暇予定日に同じ日付が入力されている場合")]
        public async Task OnPostRegisterAsync_休暇予定日に同じ日付が入力されている場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateRequest(
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 1), IsTokukyu = true },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 1), IsTokukyu = true }, // 重複した日付（エラーとなる想定）
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 3), IsTokukyu = false },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 4), IsTokukyu = false },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 5), IsTokukyu = false },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 6), IsTokukyu = false },
                new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 7), IsTokukyu = false });

            var result = await OnPostRegisterAsync(model, request); // Act
            AssertErrors(result, Const.ErrorYmdDuplicate); // Assert
        }

        /// <summary>
        /// 異常:
        /// #10 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
        /// 社員BaseID：ログインユーザーのBaseID」
        /// 新規登録リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #10 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
            社員BaseID：ログインユーザーのBaseID」
            新規登録リクエストが送信された場合
            """)]
        public async Task OnPostRegisterAsync_計画有給休暇データあり_今年度フラグ_ログインユーザー_新規登録リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(loginUserSyain, yukyuNendoOfThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateInsertRequest();

            var result = await OnPostRegisterAsync(model, request); // Act
            AssertErrorJson(result, IndexModel.ErrorConflictYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 異常:
        /// #11 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
        /// 社員BaseID：ログインユーザーのBaseID」
        /// 更新リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #11 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
            社員BaseID：ログインユーザーのBaseID」
            更新リクエストが送信された場合
            """)]
        public async Task OnPostRegisterAsync_計画有給休暇データあり_非今年度_ログインユーザー_更新リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var yukyuNendoOfNotThisYear = AddYukyuNendoOfNotThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(loginUserSyain, yukyuNendoOfNotThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateUpdateRequest();

            var result = await OnPostRegisterAsync(model, request); // Act
            AssertErrorJson(result, IndexModel.ErrorConflictYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 異常:
        /// #12 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
        /// 社員BaseID：**非**ログインユーザーのBaseID」
        /// 更新リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #12 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
            社員BaseID：**非**ログインユーザーのBaseID」
            更新リクエストが送信された場合
            """)]
        public async Task OnPostRegisterAsync_計画有給休暇データあり_今年度_非ログインユーザー_更新リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var notLoginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(notLoginUserSyain, yukyuNendoOfThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateUpdateRequest();

            var result = await OnPostRegisterAsync(model, request); // Act
            AssertErrorJson(result, IndexModel.ErrorConflictYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 異常:
        /// #13 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
        /// 社員BaseID：**非**ログインユーザーのBaseID」
        /// 更新リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #13 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
            社員BaseID：**非**ログインユーザーのBaseID」
            更新リクエストが送信された場合
            """)]
        public async Task OnPostRegisterAsync_計画有給休暇データあり_非今年度_非ログインユーザー_更新リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var notLoginUserSyain = AddNewSyain();
            var yukyuNendoOfNotThisYear = AddYukyuNendoOfNotThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(notLoginUserSyain, yukyuNendoOfNotThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateUpdateRequest();

            var result = await OnPostRegisterAsync(model, request); // Act
            AssertErrorJson(result, IndexModel.ErrorConflictYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 異常:
        /// #14 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**なし**
        /// 更新リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #14 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**なし**
            更新リクエストが送信された場合
            """)]
        public async Task OnPostRegisterAsync_計画有給休暇データなし_更新リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateUpdateRequest();

            var result = await OnPostRegisterAsync(model, request); // Act
            AssertErrorJson(result, IndexModel.ErrorConflictYukyuKeikaku); // Assert
        }

        /// <summary>
        /// 異常:
        /// #15 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
        /// 社員BaseID：ログインユーザーのBaseID」
        /// 更新リクエストによって同時実行制御が発動した場合
        /// </summary>
        [TestMethod(DisplayName = """
            #15 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
            社員BaseID：ログインユーザーのBaseID」
            更新リクエストによって同時実行制御が発動した場合
            """)]
        public async Task OnPostRegisterAsync_計画有給休暇データあり_今年度_ログインユーザー_更新リクエストによって同時実行制御が発動した場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            var yukyuKeikaku = AddYukyuKeikakuAndMeisaiWithShuffledYmds(loginUserSyain, yukyuNendoOfThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateUpdateRequest(yukyuKeikaku.Version + 1); // バージョン不整合を発生させる

            var result = await OnPostRegisterAsync(model, request); // Act
            AssertErrors(result, IndexModel.ErrorConflictYukyuKeikaku); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 正常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 正常:
        /// #16 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
        /// 社員BaseID：ログインユーザーのBaseID」
        /// 新規登録リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #16 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
            社員BaseID：ログインユーザーのBaseID」
            新規登録リクエストが送信された場合
            """)]
        public async Task OnPostRegisterAsync_計画有給休暇データあり_非今年度_ログインユーザー_新規登録リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            var yukyuNendoOfNotThisYear = AddYukyuNendoOfNotThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(loginUserSyain, yukyuNendoOfNotThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateInsertRequest();

            var result = await OnPostRegisterAsync(model, request); // Act
            await AssertAddsNewRecord(loginUserSyain, yukyuNendoOfThisYear, true, result); // Assert
        }

        /// <summary>
        /// 正常:
        /// #17 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
        /// 社員BaseID：**非**ログインユーザーのBaseID」
        /// 新規登録リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #17 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
            社員BaseID：**非**ログインユーザーのBaseID」
            新規登録リクエストが送信された場合
            """)]
        public async Task OnPostRegisterAsync_計画有給休暇データあり_今年度_非ログインユーザー_新規登録リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var notLoginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(notLoginUserSyain, yukyuNendoOfThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateInsertRequest();

            var result = await OnPostRegisterAsync(model, request); // Act
            await AssertAddsNewRecord(loginUserSyain, yukyuNendoOfThisYear, true, result); // Assert
        }

        /// <summary>
        /// 正常:
        /// #18 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
        /// 社員BaseID：**非**ログインユーザーのBaseID」
        /// 新規登録リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #18 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**FALSE**」のID、
            社員BaseID：**非**ログインユーザーのBaseID」
            新規登録リクエストが送信された場合
            """)]
        public async Task OnPostRegisterAsync_計画有給休暇データあり_非今年度_非ログインユーザー_新規登録リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var notLoginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            var yukyuNendoOfNotThisYear = AddYukyuNendoOfNotThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(notLoginUserSyain, yukyuNendoOfNotThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateInsertRequest();

            var result = await OnPostRegisterAsync(model, request); // Act
            await AssertAddsNewRecord(loginUserSyain, yukyuNendoOfThisYear, true, result); // Assert
        }

        /// <summary>
        /// 正常:
        /// #19 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**なし**
        /// 新規登録リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #19 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**なし**
            新規登録リクエストが送信された場合
            """)]
        public async Task OnPostRegisterAsync_計画有給休暇データなし_新規登録リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateInsertRequest();

            var result = await OnPostRegisterAsync(model, request); // Act
            await AssertAddsNewRecord(loginUserSyain, yukyuNendoOfThisYear, false, result); // Assert
        }

        /// <summary>
        /// 正常:
        /// #20 申請（ボタン押下）／再申請（ボタン押下）
        /// 計画有給休暇データ**あり**
        /// 「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
        /// 社員BaseID：ログインユーザーのBaseID」
        /// 更新リクエストが送信された場合
        /// </summary>
        [TestMethod(DisplayName = """
            #20 申請（ボタン押下）／再申請（ボタン押下）
            計画有給休暇データ**あり**
            「有給年度ID：有給年度「今年度フラグ：**TRUE**」のID、
            社員BaseID：ログインユーザーのBaseID」
            更新リクエストが送信された場合
            """)]
        public async Task OnPostRegisterAsync_計画有給休暇データあり_今年度_ログインユーザー_更新リクエストが送信された場合()
        {
            // Arrange
            var loginUserSyain = AddNewSyain();
            var yukyuNendoOfThisYear = AddYukyuNendoOfThisYear();
            AddYukyuKeikakuAndMeisaiWithShuffledYmds(loginUserSyain, yukyuNendoOfThisYear);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);
            var request = CreateUpdateRequest();

            var result = await OnPostRegisterAsync(model, request); // Act
            await AssertUpdatesExistingRecord(loginUserSyain, yukyuNendoOfThisYear, result); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Arrange用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 新規登録系リクエストの作成
        /// </summary>
        private static IndexModel.YukyuKeikakuViewModel CreateInsertRequest() => CreateRequest(
            new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 1), IsTokukyu = true },
            new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 2), IsTokukyu = true },
            new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 3), IsTokukyu = false },
            new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 4), IsTokukyu = false },
            new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 5), IsTokukyu = false },
            new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 6), IsTokukyu = false },
            new IndexModel.Meisai { Ymd = new DateOnly(2024, 12, 7), IsTokukyu = false });

        /// <summary>
        /// 更新系リクエストの作成
        /// </summary>
        private static IndexModel.YukyuKeikakuViewModel CreateUpdateRequest(uint version = default) => CreateRequest(
            version,
            new IndexModel.Meisai { Id = 1, Ymd = new DateOnly(2024, 12, 1), IsTokukyu = true },
            new IndexModel.Meisai { Id = 2, Ymd = new DateOnly(2024, 12, 2), IsTokukyu = true },
            new IndexModel.Meisai { Id = 3, Ymd = new DateOnly(2024, 12, 3), IsTokukyu = false },
            new IndexModel.Meisai { Id = 4, Ymd = new DateOnly(2024, 12, 4), IsTokukyu = false },
            new IndexModel.Meisai { Id = 5, Ymd = new DateOnly(2024, 12, 5), IsTokukyu = false },
            new IndexModel.Meisai { Id = 6, Ymd = new DateOnly(2024, 12, 6), IsTokukyu = false },
            new IndexModel.Meisai { Id = 7, Ymd = new DateOnly(2024, 12, 7), IsTokukyu = false });

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Act用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private static async Task<IActionResult> OnPostRegisterAsync(IndexModel model, IndexModel.YukyuKeikakuViewModel request)
        {
            model.LoginUsersYukyuKeikaku = request;
            return await model.OnPostRegisterAsync();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Assert用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 新規登録系アサーション
        /// </summary>
        private async Task AssertAddsNewRecord(
            Syain loginUserSyain, YukyuNendo yukyuNendoOfThisYear, bool expectedIsSecondRecord, IActionResult actualResult)
        {
            // 成功レスポンスが返却されること
            AssertSuccess(actualResult);

            // 計画有給休暇
            // IDが自動採番されること
            // 有給年度IDに有給年度「今年度フラグ：TRUE」のIDが設定されること
            // 社員BaseIDにログインユーザーのBaseIDが設定されること
            // ステータスに「事業部承認待ち」が設定されること
            var yukyuKeikaku = await db.YukyuKeikakus
                .Include(yk => yk.YukyuKeikakuMeisais)
                .Where(yk => yk.SyainBaseId == loginUserSyain.SyainBaseId && yk.YukyuNendoId == yukyuNendoOfThisYear.Id)
                .SingleAsync();
            var expectedYukyuKeikakuId = expectedIsSecondRecord ? 2 : 1;
            Assert.AreEqual(expectedYukyuKeikakuId, yukyuKeikaku.Id, "ID が一致しません。");
            Assert.AreEqual(yukyuNendoOfThisYear.Id, yukyuKeikaku.YukyuNendoId, "有給年度ID が一致しません。");
            Assert.AreEqual(loginUserSyain.SyainBaseId, yukyuKeikaku.SyainBaseId, "社員BaseID が一致しません。");
            Assert.AreEqual(事業部承認待ち, yukyuKeikaku.Status, "ステータスが一致しません。");

            // 計画有給休暇明細
            // IDが自動採番されること
            // 計画有給IDに上で追加した計画有給休暇のIDが設定されること
            // 計画有給年月日にテスト入力値が設定されること
            // 特休フラグにテスト入力値が設定されること
            const int previousYukyuKeikakuMeisaisCount = 7;
            var expectedYukyuKeikakuMeisaiIdOffset = expectedIsSecondRecord ? previousYukyuKeikakuMeisaisCount : 0;
            var expectedMeisais = new[]
            {
                new YukyuKeikakuMeisai
                {
                    Id = 1 + expectedYukyuKeikakuMeisaiIdOffset,
                    Ymd = new DateOnly(2024, 12, 1), IsTokukyu = true, YukyuKeikakuId = yukyuKeikaku.Id
                },
                new YukyuKeikakuMeisai
                {
                    Id = 2 + expectedYukyuKeikakuMeisaiIdOffset,
                    Ymd = new DateOnly(2024, 12, 2), IsTokukyu = true, YukyuKeikakuId = yukyuKeikaku.Id
                },
                new YukyuKeikakuMeisai
                {
                    Id = 3 + expectedYukyuKeikakuMeisaiIdOffset,
                    Ymd = new DateOnly(2024, 12, 3), IsTokukyu = false, YukyuKeikakuId = yukyuKeikaku.Id
                },
                new YukyuKeikakuMeisai
                {
                    Id = 4 + expectedYukyuKeikakuMeisaiIdOffset,
                    Ymd = new DateOnly(2024, 12, 4), IsTokukyu = false, YukyuKeikakuId = yukyuKeikaku.Id
                },
                new YukyuKeikakuMeisai
                {
                    Id = 5 + expectedYukyuKeikakuMeisaiIdOffset,
                    Ymd = new DateOnly(2024, 12, 5), IsTokukyu = false, YukyuKeikakuId = yukyuKeikaku.Id
                },
                new YukyuKeikakuMeisai
                {
                    Id = 6 + expectedYukyuKeikakuMeisaiIdOffset,
                    Ymd = new DateOnly(2024, 12, 6), IsTokukyu = false, YukyuKeikakuId = yukyuKeikaku.Id
                },
                new YukyuKeikakuMeisai
                {
                    Id = 7 + expectedYukyuKeikakuMeisaiIdOffset,
                    Ymd = new DateOnly(2024, 12, 7), IsTokukyu = false, YukyuKeikakuId = yukyuKeikaku.Id
                }
            };
            AssertAreEqual(expectedMeisais, [.. yukyuKeikaku.YukyuKeikakuMeisais]);
        }

        /// <summary>
        /// 更新系アサーション
        /// </summary>
        private async Task AssertUpdatesExistingRecord(Syain loginUserSyain, YukyuNendo yukyuNendoOfThisYear, IActionResult actualResult)
        {
            // 成功レスポンスが返却されること
            AssertSuccess(actualResult);

            // 計画有給休暇
            // 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：ログインユーザーのBaseID」
            // ステータスに「事業部承認待ち」が設定されること
            var yukyuKeikaku = await db.YukyuKeikakus
                .Include(yk => yk.YukyuKeikakuMeisais)
                .Where(yk => yk.YukyuNendoId == yukyuNendoOfThisYear.Id && yk.SyainBaseId == loginUserSyain.SyainBaseId)
                .SingleAsync();
            Assert.AreEqual(事業部承認待ち, yukyuKeikaku.Status, "ステータスが一致しません。");

            // 計画有給休暇明細
            // 「計画有給ID：計画有給休暇のID」
            // 計画有給年月日にテスト入力値が設定されること
            // 特休フラグにテスト入力値が設定されること
            var expectedMeisais = new[]
            {
                new YukyuKeikakuMeisai { Id = 1, Ymd = new DateOnly(2024, 12, 1), IsTokukyu = true, YukyuKeikakuId = yukyuKeikaku.Id },
                new YukyuKeikakuMeisai { Id = 2, Ymd = new DateOnly(2024, 12, 2), IsTokukyu = true, YukyuKeikakuId = yukyuKeikaku.Id },
                new YukyuKeikakuMeisai { Id = 3, Ymd = new DateOnly(2024, 12, 3), IsTokukyu = false, YukyuKeikakuId = yukyuKeikaku.Id },
                new YukyuKeikakuMeisai { Id = 4, Ymd = new DateOnly(2024, 12, 4), IsTokukyu = false, YukyuKeikakuId = yukyuKeikaku.Id },
                new YukyuKeikakuMeisai { Id = 5, Ymd = new DateOnly(2024, 12, 5), IsTokukyu = false, YukyuKeikakuId = yukyuKeikaku.Id },
                new YukyuKeikakuMeisai { Id = 6, Ymd = new DateOnly(2024, 12, 6), IsTokukyu = false, YukyuKeikakuId = yukyuKeikaku.Id },
                new YukyuKeikakuMeisai { Id = 7, Ymd = new DateOnly(2024, 12, 7), IsTokukyu = false, YukyuKeikakuId = yukyuKeikaku.Id }
            };
            AssertAreEqual(expectedMeisais, [.. yukyuKeikaku.YukyuKeikakuMeisais]);
        }
    }
}
