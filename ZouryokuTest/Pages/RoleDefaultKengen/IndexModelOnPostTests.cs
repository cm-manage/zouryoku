using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using Model.Model;
using Zouryoku.Pages.RoleDefaultKengen;
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
        private void CreateUserRoleEntity()
        {
            var firstUserRoleEntity = CreateUserRole(
                id: FirstRoleId);
            var secondUserRoleEntity = CreateUserRole(
                id: SecondRoleId);
            SeedEntities(firstUserRoleEntity, secondUserRoleEntity);
        }

        /// <summary>
        /// 選択ロールIDを設定したモデルを生成します。
        /// </summary>
        /// <param name="selectedRoleId">モデルに設定する選択ロールID</param>
        /// <returns>選択ロールIDが設定された IndexModel</returns>
        private IndexModel CreateModelWithSelectRoleId(long selectedRoleId)
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
        private static IndexViewModel CreateIndexViewModelForUpdate(long roleId, long kengenValue)
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
        private long? GetUserRoleKengen(long roleId)
        {
            return db.UserRoles
                .Where(role => role.Id == roleId)
                .Select(role => (long?)role.Kengen)
                .SingleOrDefault();
        }

        /// <summary>
        /// ①詳細データ取得: ロールが存在する場合、成功ステータスのJsonResultを返すことを確認します。
        /// </summary>
        [TestMethod(DisplayName = "OnPostRoleDataAsync: ロールありで成功JSONを返す")]
        public async Task OnPostRoleDataAsync_ロールありで成功Jsonを返す()
        {
            // Arrange
            CreateUserRoleEntity();
            var model = CreateModelWithSelectRoleId(SelectedRoleId);

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
            CreateUserRoleEntity();
            var model = CreateModelWithSelectRoleId(SelectedRoleId);

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
            var targetUserRoleEntity = CreateUserRole(
                id: ExistingRoleId);
            SeedEntities(targetUserRoleEntity);

            var model = CreateModel();
            model.ViewModel = CreateIndexViewModelForUpdate(
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
            var targetUserRoleEntity = CreateUserRole(
                id: ExistingRoleId);
            SeedEntities(targetUserRoleEntity);

            var model = CreateModel();
            model.ViewModel = CreateIndexViewModelForUpdate(
                ExistingRoleId,
                UpdatedKengenValue
            );

            // Act
            await model.OnPostUpdateRoleAsync();

            // Assert
            var actualKengenValue = GetUserRoleKengen(ExistingRoleId);
            Assert.AreEqual(
                UpdatedKengenValue,
                actualKengenValue,
                "対象ロールの社員権限が更新値と一致するべきです。"
            );
        }

        /// <summary>
        /// ⑤権限更新: 更新対象のロールが存在しない場合、存在チェックのエラーが含まれている
        /// </summary>
        [TestMethod(DisplayName = "OnPostUpdateRoleAsync: 対象ロールが存在しない場合、存在チェックのエラーが" +
            "含まれているはずです")]
        public async Task OnPostUpdateRoleAsync_対象ロールが存在しない場合存在チェックのエラーが含まれている()
        {
            // Arrange
            var model = CreateModel();
            model.ViewModel = CreateIndexViewModelForUpdate(
                MissingRoleId,
                UpdatedKengenValue
            );

            // Act
            var result = await model.OnPostUpdateRoleAsync();

            // ---------- Assert ----------
            AssertErrorJson(result, ErrorSelectedDataNotExists);
        }

        /// <summary>
        /// ⑤権限更新: 楽観的同時実行制御エラーが発生した場合、データが更新されていないことを確認
        /// </summary>
        [TestMethod(DisplayName = "OnPostUpdateRoleAsync: 楽観的同時実行制御エラーが発生した場合、" +
            "データが更新されていない")]
        public async Task OnPostUpdateRoleAsync_楽観的同時実行制御エラーが発生した場合データが更新されていない()
        {
            // Arrange
            var targetUserRoleEntity = CreateUserRole(
                id: ExistingRoleId,
                version: 1u); // バージョンを1に設定

            SeedEntities(targetUserRoleEntity);

            var model = CreateModel();
            model.ViewModel = new IndexViewModel
            {
                SelectedRoleId = ExistingRoleId,
                KengenValue = UpdatedKengenValue,
                Version = 0u // 古いバージョン
            };

            // Act
            var result = await model.OnPostUpdateRoleAsync();

            // ---------- Assert ----------
            AssertErrors(result, string.Format(ErrorConflictReload, "ロール"));
        }

        private static UserRole CreateUserRole(
            long? id = 1,
            short? code = 0,
            string? name = null,
            short? jyunjo = 0,
            EmployeeAuthority? kengen = EmployeeAuthority.None,
            uint? version = 0u)
        {
            var result = new UserRole()
            {
                Code = code ?? 0,
                Name = name ?? "株式会社サンプル",
                Jyunjo = jyunjo ?? 0,
                Kengen = kengen ?? EmployeeAuthority.None,
                Version = version ?? 0u
            };

            if (id.HasValue)
            {
                result.Id = id.Value;
            }

            return result;
        }
    }
}
