using Microsoft.AspNetCore.Mvc.RazorPages;
using Model.Enums;
using Model.Model;
using Zouryoku.Pages.RoleDefaultKengen;
using static Model.Enums.EmployeeAuthority;

namespace ZouryokuTest.Pages.RoleDefaultKengen
{
    /// <summary>
    /// RoleDefaultKengen OnGet のテストです。
    /// </summary>
    [TestClass]
    public class IndexModelOnGetTests : IndexModelTestsBase
    {
        private const long LowestOrderRoleId = 11L;
        private const long HighestOrderRoleId = 22L;
        private const short LowestOrderRoleCode = 1;
        private const short HighestOrderRoleCode = 2;
        private const string LowestOrderRoleName = "一般";
        private const string HighestOrderRoleName = "管理者";
        private const short LowestOrder = 1;
        private const short HighestOrder = 2;
        private const int ExpectedRoleCount = 2;
        private const long DefaultSelectedRoleId = 0L;
        private const long DefaultKengenValue = 0L;
        private const long LowestOrderRoleKengenValue = 1L;
        private const long HighestOrderRoleKengenValue = 2L;

        private static readonly EmployeeAuthority LowestOrderRoleKengen =
            (EmployeeAuthority)LowestOrderRoleKengenValue;

        private static readonly EmployeeAuthority HighestOrderRoleKengen =
            (EmployeeAuthority)HighestOrderRoleKengenValue;

        /// <summary>
        /// 既定のロールデータを登録します。
        /// </summary>
        private void CreateUserRoleEntity()
        {
            // 並び順の検証のため、あえて順序を逆に登録する
            var higherOrderRole = CreateUserRole(
                HighestOrderRoleId,
                HighestOrderRoleCode,
                HighestOrderRoleName,
                HighestOrder,
                HighestOrderRoleKengen);

            var lowerOrderRole = CreateUserRole(
                LowestOrderRoleId,
                LowestOrderRoleCode,
                LowestOrderRoleName,
                LowestOrder,
                LowestOrderRoleKengen);

            SeedEntities(higherOrderRole, lowerOrderRole);
        }

        /// <summary>
        /// ①初期表示: ロールが存在する場合、PageResultを返すことを確認します。
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: ロールがある場合はPageResultを返す")]
        public async Task OnGetAsync_ロールがある場合はPageResultを返す()
        {
            // Arrange
            CreateUserRoleEntity();
            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync();

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(PageResult),
                "ロールデータがある場合はPageResultを返すべきです。");
        }

        /// <summary>
        /// ②初期表示: ロールが存在する場合、ViewModelの選択ロールIDに並び順が最小（先頭）のロールIDが
        /// 設定されることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 選択ロールIDに並び順先頭ロールを設定する")]
        public async Task OnGetAsync_選択ロールIDに並び順先頭ロールを設定する()
        {
            // Arrange
            CreateUserRoleEntity();
            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual(
                LowestOrderRoleId,
                model.ViewModel.SelectedRoleId,
                "選択ロールIDは並び順先頭ロールのIDになるべきです。");
        }

        /// <summary>
        /// ③初期表示: ロールが存在する場合、ViewModelのUserRolesに並び順に従ったロール一覧が設定されることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: UserRolesを並び順で設定する")]
        public async Task OnGetAsync_UserRolesを並び順で設定する()
        {
            // Arrange
            CreateUserRoleEntity();
            var model = CreateModel();
            var expectedIds = new[]
            {
                LowestOrderRoleId.ToString(),
                HighestOrderRoleId.ToString()
            };
            var expectedNames = new[]
            {
                LowestOrderRoleName,
                HighestOrderRoleName
            };

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.HasCount(
                ExpectedRoleCount,
                model.ViewModel.UserRoles,
                "UserRoles件数が期待値と一致するべきです。");

            CollectionAssert.AreEqual(
                expectedIds,
                model.ViewModel.UserRoles.Select(role => role.Value).ToArray(),
                "UserRolesのID順序は並び順に従うべきです。");

            CollectionAssert.AreEqual(
                expectedNames,
                model.ViewModel.UserRoles.Select(role => role.Text).ToArray(),
                "UserRolesの名称順序は並び順に従うべきです。");
        }

        /// <summary>
        /// ④初期表示: ロールが存在する場合、ViewModelの権限（Kengen）に選択されたロールの権限値が設定されることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 選択ロールの権限を設定する")]
        public async Task OnGetAsync_選択ロールの権限を設定する()
        {
            // Arrange
            CreateUserRoleEntity();
            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual(
                LowestOrderRoleKengen,
                model.ViewModel.Kengen,
                "Kengenは選択ロールの権限値と一致するべきです。");
        }

        /// <summary>
        /// ⑤初期表示: ロールが存在する場合、ViewModelの権限値（KengenValue）は初期値（0）のままであることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: KengenValueは既定値を保持する")]
        public async Task OnGetAsync_KengenValueは既定値を保持する()
        {
            // Arrange
            CreateUserRoleEntity();
            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual(
                DefaultKengenValue,
                model.ViewModel.KengenValue,
                "OnGetではKengenValueを変更しないべきです。");
        }

        /// <summary>
        /// ⑥初期表示: ロールが存在しない場合、ViewModelの選択ロールIDに既定値（0）が設定されることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: ロールがない場合は既定値を設定する")]
        public async Task OnGetAsync_ロールがない場合は既定値を設定する()
        {
            // Arrange
            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual(
                DefaultSelectedRoleId,
                model.ViewModel.SelectedRoleId,
                "ロールがない場合は選択ロールIDを0に設定するべきです。");
        }

        /// <summary>
        /// ⑦権限一覧: ViewModelの全権限一覧（AllAuthorities）に「None」が含まれていないことを確認します。
        /// </summary>
        [TestMethod(DisplayName = "IndexViewModel: AllAuthoritiesにNoneを含めない")]
        public void IndexViewModel_AllAuthoritiesにNoneを含めない()
        {
            // Arrange
            var viewModel = new IndexViewModel();
            var expectedAuthorities = Enum.GetValues<EmployeeAuthority>()
                .Where(authority => authority != None)
                .ToArray();

            // Act
            var actualAuthorities = viewModel.AllAuthorities.ToArray();

            // Assert
            CollectionAssert.AreEqual(
                expectedAuthorities,
                actualAuthorities,
                "AllAuthoritiesはNoneを除いたEmployeeAuthority一覧と一致するべきです。");

            Assert.IsFalse(
                actualAuthorities.Contains(None),
                "AllAuthoritiesにNoneを含めてはいけません。");
        }

        private static UserRole CreateUserRole(
            long? id = 1,
            short? code = 0,
            string? name = null,
            short? jyunjo = 0,
            EmployeeAuthority? kengen = EmployeeAuthority.None)
        {
            var result = new UserRole()
            {
                Code = code ?? 0,
                Name = name ?? "株式会社サンプル",
                Jyunjo = jyunjo ?? 0,
                Kengen = kengen ?? EmployeeAuthority.None
            };

            if (id.HasValue)
            {
                result.Id = id.Value;
            }

            return result;
        }
    }
}
