using Microsoft.AspNetCore.Mvc;
using Zouryoku.Pages.RoleDefaultKengen;
using Zouryoku.Pages.Shared;
using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.RoleDefaultKengen
{
    /// <summary>
    /// RoleDefaultKengen の OnPost 系テストです。
    /// </summary>
    [TestClass]
    public class IndexModelOnPostTests : IndexModelTestsBase
    {
        private const long FirstRoleId = 1L;
        private const long SecondRoleId = 2L;
        private const long SelectedRoleId = FirstRoleId;
        private const int ExpectedRoleCount = 2;

        private const long ExistingRoleId = 1L;
        private const long MissingRoleId = 99L;
        private const long UpdatedKengenValue = 7L;

        /// <summary>
        /// OnPostRoleData 用のロールデータを登録します。
        /// </summary>
        private void RoleData用UserRoleEntityを登録する()
        {
            var firstUserRoleEntity = CreateUserRoleEntityById(FirstRoleId);
            var secondUserRoleEntity = CreateUserRoleEntityById(SecondRoleId);
            SeedEntities(firstUserRoleEntity, secondUserRoleEntity);
        }

        /// <summary>
        /// 選択ロールIDを設定したモデルを生成します。
        /// </summary>
        /// <param name="selectedRoleId">モデルに設定する選択ロールID</param>
        /// <returns>選択ロールIDが設定された IndexModel</returns>
        private IndexModel SelectedRoleIdを設定したModelを作成する(long selectedRoleId)
        {
            var model = CreateModel();
            model.ViewModel.SelectedRoleId = selectedRoleId;
            return model;
        }

        /// <summary>
        /// 更新用 ViewModel を生成します。
        /// </summary>
        /// <param name="roleId">ロールID</param>
        /// <param name="kengenValue">更新後の権限値</param>
        /// <returns>生成された更新用の IndexViewModel</returns>
        private static IndexViewModel 更新用IndexViewModelを作成する(long roleId, long kengenValue)
        {
            return new IndexViewModel
            {
                SelectedRoleId = roleId,
                KengenValue = kengenValue
            };
        }

        /// <summary>
        /// 指定IDの UserRole の権限値を取得します。
        /// </summary>
        /// <param name="roleId">検索対象のロールID</param>
        /// <returns>権限値。該当するロールがない場合は null</returns>
        private long? UserRoleEntityのKengen値をIdで取得する(long roleId)
        {
            return db.UserRoles
                .Where(role => role.Id == roleId)
                .Select(role => (long?)role.Kengen)
                .SingleOrDefault();
        }

        /// <summary>
        /// IActionResult から ResponseJson を取得します。
        /// </summary>
        /// <param name="actionResult">検証対象の IActionResult</param>
        /// <returns>抽出された ResponseJson</returns>
        private static ResponseJson ActionResultからResponseJsonを取得する(IActionResult actionResult)
        {
            if (actionResult is not JsonResult jsonResult)
            {
                throw new AssertFailedException(
                    "戻り値は JsonResult であるべきです。"
                );
            }

            if (jsonResult.Value is not ResponseJson responseJson)
            {
                throw new AssertFailedException(
                    "JsonResult.Value は ResponseJson であるべきです。"
                );
            }

            return responseJson;
        }

        /// <summary>
        /// ①詳細データ取得: ロールが存在する場合、成功ステータスのJsonResultを返すことを確認します。
        /// </summary>
        [TestMethod(DisplayName = "OnPostRoleDataAsync: ロールありで成功JSONを返す")]
        public async Task OnPostRoleDataAsync_ロールありで成功Jsonを返す()
        {
            // Arrange
            RoleData用UserRoleEntityを登録する();
            var model = SelectedRoleIdを設定したModelを作成する(SelectedRoleId);

            // Act
            var result = await model.OnPostRoleDataAsync();

            // Assert
            AssertSuccessJson(result);
        }

        /// <summary>
        /// ②詳細データ取得: ロールが存在する場合、ViewModelのUserRolesにロール一覧が設定されることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "OnPostRoleDataAsync: ロールありでViewModelにUserRolesを設定する")]
        public async Task OnPostRoleDataAsync_ロールありでUserRolesを設定する()
        {
            // Arrange
            RoleData用UserRoleEntityを登録する();
            var model = SelectedRoleIdを設定したModelを作成する(SelectedRoleId);

            // Act
            await model.OnPostRoleDataAsync();

            // Assert
            Assert.HasCount(
                ExpectedRoleCount,
                model.ViewModel.UserRoles,
                "OnPostRoleDataAsync 実行後に UserRoles が2件であるべきです。"
            );
        }

        /// <summary>
        /// ③権限更新: 更新対象のロールが存在する場合、成功ステータスのJsonResultを返すことを確認します。
        /// </summary>
        [TestMethod(DisplayName = "OnPostUpdateRoleAsync: 対象ロールありで成功JSONを返す")]
        public async Task OnPostUpdateRoleAsync_対象ロールありで成功Jsonを返す()
        {
            // Arrange
            var targetUserRoleEntity = CreateUserRoleEntityById(ExistingRoleId);
            SeedEntities(targetUserRoleEntity);

            var model = CreateModel();
            model.ViewModel = 更新用IndexViewModelを作成する(
                ExistingRoleId,
                UpdatedKengenValue
            );

            // Act
            var result = await model.OnPostUpdateRoleAsync();

            // Assert
            AssertSuccessJson(result);
        }

        /// <summary>
        /// ④権限更新: 更新対象のロールが存在する場合、DB上の権限（Kengen）が指定した値に更新されることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "OnPostUpdateRoleAsync: 対象ロールありで権限値を更新する")]
        public async Task OnPostUpdateRoleAsync_対象ロールありで社員権限を更新する()
        {
            // Arrange
            var targetUserRoleEntity = CreateUserRoleEntityById(ExistingRoleId);
            SeedEntities(targetUserRoleEntity);

            var model = CreateModel();
            model.ViewModel = 更新用IndexViewModelを作成する(
                ExistingRoleId,
                UpdatedKengenValue
            );

            // Act
            await model.OnPostUpdateRoleAsync();

            // Assert
            var actualKengenValue = UserRoleEntityのKengen値をIdで取得する(ExistingRoleId);
            Assert.AreEqual(
                UpdatedKengenValue,
                actualKengenValue,
                "対象ロールの社員権限が更新値と一致するべきです。"
            );
        }

        /// <summary>
        /// ⑤権限更新: 更新対象のロールが存在しない場合、JsonResultを返すことを確認します。
        /// </summary>
        [TestMethod(DisplayName = "OnPostUpdateRoleAsync: 対象ロールなしでJsonResultを返す")]
        public async Task OnPostUpdateRoleAsync_対象ロールなしでJsonResultを返す()
        {
            // Arrange
            var model = CreateModel();
            model.ViewModel = 更新用IndexViewModelを作成する(
                MissingRoleId,
                UpdatedKengenValue
            );

            // Act
            var result = await model.OnPostUpdateRoleAsync();

            // Assert
            Assert.IsInstanceOfType<JsonResult>(
                result,
                "対象ロールが存在しない場合は JsonResult を返すべきです。"
            );
        }

        /// <summary>
        /// ⑥権限更新: 更新対象のロールが存在しない場合、レスポンスメッセージに既定のエラーメッセージ（EmptyReadData）が
        /// 設定されることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "OnPostUpdateRoleAsync: 対象ロールなしでEmptyReadDataを返す")]
        public async Task OnPostUpdateRoleAsync_対象ロールなしでEmptyReadDataを返す()
        {
            // Arrange
            var model = CreateModel();
            model.ViewModel = 更新用IndexViewModelを作成する(
                MissingRoleId,
                UpdatedKengenValue
            );

            // Act
            var result = await model.OnPostUpdateRoleAsync();

            // Assert
            var response = ActionResultからResponseJsonを取得する(result);
            Assert.AreEqual(
                EmptyReadData,
                response.Message,
                "対象ロールが存在しない場合は EmptyReadData を返すべきです。"
            );
        }
    }
}
