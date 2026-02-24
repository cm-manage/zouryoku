using CommonLibrary.Extensions;
using Model.Enums;
using Model.Model;
using static CommonLibrary.Extensions.DateOnlyExtensions;
using static Model.Enums.BusinessTripRole;
using static Model.Enums.DailyReportStatusClassification;
using static Model.Enums.EmployeeAuthority;
using static Model.Enums.EmployeeWorkType;
using static Model.Enums.NippousCompanyCode;
using static Zouryoku.Pages.KinmuNippouMiKakuteiCheck.IndexModel;
using static Zouryoku.Pages.KinmuNippouMiKakuteiCheck.IndexModel.BusyoRange;

namespace ZouryokuTest.Pages.KinmuNippouMiKakuteiCheck
{
    [TestClass]
    public class OnGetSearchNippousAsyncTests : TestBase
    {
        // ======================================
        // フィールド
        // ======================================

        /// <summary>
        /// 不正データに付与されるサフィックス。
        /// </summary>
        private const string BadNippouSuffix = "　　（データ不正あり）";

        // ======================================
        // ヘルパーメソッド
        // ======================================

        /// <summary>
        /// ビューモデルの値の検証を行うAssertをまとめたメソッド。
        /// </summary>
        /// <param name="viewModel">検証するビューモデル</param>
        /// <param name="expectedBusyoName">期待される部署名</param>
        /// <param name="expectedSyainCode">期待される社員番号</param>
        /// <param name="expectedSyainName">期待される社員氏名</param>
        /// <param name="expectedSyainBaseId">期待される社員BaseID</param>
        /// <param name="expectedLastKateiNippouYmd">期待される最終確定日（デフォルトはnull）</param>
        private void AssertCorrectViewModel(
            MikakuteiSyainViewModel viewModel, string expectedBusyoName, string expectedSyainCode,
            string expectedSyainName, long expectedSyainBaseId, DateOnly? expectedLastKateiNippouYmd = null)
        {
            Assert.AreEqual(expectedBusyoName, viewModel.BusyoName);
            Assert.AreEqual(expectedSyainCode, viewModel.SyainCode);
            Assert.AreEqual(expectedSyainName, viewModel.SyainName);
            Assert.AreEqual(expectedSyainBaseId, viewModel.SyainBaseId);
            if (expectedLastKateiNippouYmd.HasValue)
            {
                Assert.IsTrue(viewModel.LastKakuteiYmd.HasValue);
                Assert.AreEqual(expectedLastKateiNippouYmd.Value, viewModel.LastKakuteiYmd.Value);
            }
            else
            {
                Assert.IsFalse(viewModel.LastKakuteiYmd.HasValue);
            }
        }

        // ======================================
        // テスト
        // ======================================
        [TestMethod]
        [DataRow(みなし対象者, DisplayName = "みなし対象者")]
        [DataRow(_3か月60時間, DisplayName = "3か月60時間")]
        [DataRow(フリー, DisplayName = "フリー")]
        [DataRow(管理, DisplayName = "管理")]
        [DataRow(パート, DisplayName = "パート")]
        [DataRow(月45時間, DisplayName = "月45時間")]
        public async Task OnGetSearchNippousAsync_標準社員外でない社員_取得する(EmployeeWorkType workType)
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 7, 15));
            var inputDate = new DateOnly(2026, 2, 15);
            var model = CreateIndexModel();

            var expectedBusyoName = "部署名称";
            var expectedSyainCode = "12345";
            var expectedSyainName = "社員氏名";
            var expectedSyainBaseId = 12345;

            var syain = new Syain()
            {
                Name = expectedSyainName,
                Code = expectedSyainCode,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Retired = false,
                SyainBase = new()
                {
                    Id = expectedSyainBaseId,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    StartYmd = DateOnly.MinValue,
                    EndYmd = DateOnly.MaxValue,
                    Name = expectedBusyoName,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = workType,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            db.Add(syain);
            db.SaveChanges();

            // 検索条件の設定
            model.SearchConditions.Busyo.Id = null;
            model.SearchConditions.Busyo.Range = 全社;
            model.SearchConditions.Date = inputDate;

            // Act
            // ----------------------------------

            await model.OnGetSearchNippousAsync();
            var viewModels = model.MikakuteiSyains;

            // Assert
            // ----------------------------------

            // データを取得していること
            Assert.IsNotEmpty(viewModels);
            Assert.HasCount(1, viewModels);
            var actualViewModel = viewModels.First();

            AssertCorrectViewModel(actualViewModel, expectedBusyoName, expectedSyainCode, expectedSyainName,
                expectedSyainBaseId);
        }

        [TestMethod]
        [DataRow(0, 15, -15, 15, DisplayName = "社員.有効開始日の境界値")]
        [DataRow(-15, 0, -15, 15, DisplayName = "社員.有効終了日の境界値")]
        [DataRow(-15, 15, 0, 15, DisplayName = "部署.有効開始日の境界値")]
        [DataRow(-15, 15, -15, 0, DisplayName = "部署.有効終了日の境界値")]
        [DataRow(-15, 15, -15, 15, DisplayName = "代表値")]
        public async Task OnGetSearchNippousAsync_有効な社員と部署_取得する(
             int syainStartYmdDayOffset, int syainEndYmdDayOffset, int busyoStartYmdDayOffset, int busyoEndYmdDayOffset)
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 7, 15));
            var today = fakeTimeProvider.Today();
            var inputDate = new DateOnly(2026, 2, 15);
            var model = CreateIndexModel();

            var expectedBusyoName = "部署名称";
            var expectedSyainCode = "12345";
            var expectedSyainName = "社員氏名";
            var expectedSyainBaseId = 12345;

            var syain = new Syain()
            {
                Name = expectedSyainName,
                Code = expectedSyainCode,
                StartYmd = today.AddDays(syainStartYmdDayOffset),
                EndYmd = today.AddDays(syainEndYmdDayOffset),
                Retired = false,
                SyainBase = new()
                {
                    Id = expectedSyainBaseId,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    StartYmd = today.AddDays(busyoStartYmdDayOffset),
                    EndYmd = today.AddDays(busyoEndYmdDayOffset),
                    Name = expectedBusyoName,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = フリー,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            db.Add(syain);
            db.SaveChanges();

            // 検索条件の設定
            model.SearchConditions.Busyo.Id = null;
            model.SearchConditions.Busyo.Range = 全社;
            model.SearchConditions.Date = inputDate;

            // Act
            // ----------------------------------

            await model.OnGetSearchNippousAsync();
            var viewModels = model.MikakuteiSyains;

            // Assert
            // ----------------------------------

            // データを取得していること
            Assert.IsNotEmpty(viewModels);
            Assert.HasCount(1, viewModels);
            var actualViewModel = viewModels.First();

            AssertCorrectViewModel(actualViewModel, expectedBusyoName, expectedSyainCode, expectedSyainName,
                expectedSyainBaseId);
        }

        [TestMethod]
        [DataRow(標準社員外, false, -15, 15, -15, 15, DisplayName = "勤怠属性 == 標準社員外")]
        [DataRow(フリー, true, -15, 15, -15, 15, DisplayName = "退職フラグ == true")]
        [DataRow(フリー, false, 1, 15, -15, 15, DisplayName = "社員.有効開始日の境界値")]
        [DataRow(フリー, false, 10, 15, -15, 15, DisplayName = "社員.有効開始日の代表値")]
        [DataRow(フリー, false, -15, -1, -15, 15, DisplayName = "社員.有効終了日の境界値")]
        [DataRow(フリー, false, -15, -10, -15, 15, DisplayName = "社員.有効終了日の代表値")]
        [DataRow(フリー, false, -15, 15, 1, 15, DisplayName = "部署.有効開始日の境界値")]
        [DataRow(フリー, false, -15, 15, 10, 15, DisplayName = "部署.有効開始日の代表値")]
        [DataRow(フリー, false, -15, 15, -15, -1, DisplayName = "部署.有効終了日の境界値")]
        [DataRow(フリー, false, -15, 15, -15, -10, DisplayName = "部署.有効終了日の代表値")]
        public async Task OnGetSearchNippousAsync_社員に関する検索条件を満たさない_取得しない(EmployeeWorkType workType, bool isRetired,
            int syainStartYmdDayOffset, int syainEndYmdDayOffset, int busyoStartYmdDayOffset, int busyoEndYmdDayOffset)
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 7, 15));
            var today = fakeTimeProvider.Today();
            var inputDate = new DateOnly(2026, 2, 15);
            var model = CreateIndexModel();

            var expectedBusyoName = "部署名称";
            var expectedSyainCode = "12345";
            var expectedSyainName = "社員氏名";
            var expectedSyainBaseId = 12345;

            var syain = new Syain()
            {
                Name = expectedSyainName,
                Code = expectedSyainCode,
                StartYmd = today.AddDays(syainStartYmdDayOffset),
                EndYmd = today.AddDays(syainEndYmdDayOffset),
                Retired = isRetired,
                SyainBase = new()
                {
                    Id = expectedSyainBaseId,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    StartYmd = today.AddDays(busyoStartYmdDayOffset),
                    EndYmd = today.AddDays(busyoEndYmdDayOffset),
                    Name = expectedBusyoName,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = workType,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            db.Add(syain);
            db.SaveChanges();

            // 検索条件の設定
            model.SearchConditions.Busyo.Id = null;
            model.SearchConditions.Busyo.Range = 全社;
            model.SearchConditions.Date = inputDate;

            // Act
            // ----------------------------------

            await model.OnGetSearchNippousAsync();
            var viewModels = model.MikakuteiSyains;

            // Assert
            // ----------------------------------

            // データを取得していないこと
            Assert.IsEmpty(viewModels);
        }

        [TestMethod]
        public async Task OnGetSearchNippousAsync_検索範囲が全社_全部署を検索する()
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 7, 15));
            var inputDate = new DateOnly(2026, 2, 15);
            var model = CreateIndexModel();

            var expectedBusyoName1 = "部署名称1";
            var expectedSyainCode1 = "12345";
            var expectedSyainName1 = "社員氏名1";
            var expectedSyainBaseId1 = 12345;
            var busyoId1 = 1;
            var expectedBusyoName2 = "部署名称2";
            var expectedSyainCode2 = "123456";
            var expectedSyainName2 = "社員氏名2";
            var expectedSyainBaseId2 = 123456;
            var busyoId2 = 2;

            var syain1 = new Syain()
            {
                Name = expectedSyainName1,
                Code = expectedSyainCode1,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Retired = false,
                SyainBase = new()
                {
                    Id = expectedSyainBaseId1,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    Id = busyoId1,
                    StartYmd = DateOnly.MinValue,
                    EndYmd = DateOnly.MaxValue,
                    Name = expectedBusyoName1,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = フリー,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            var syain2 = new Syain()
            {
                Name = expectedSyainName2,
                Code = expectedSyainCode2,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Retired = false,
                SyainBase = new()
                {
                    Id = expectedSyainBaseId2,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    Id = busyoId2,
                    StartYmd = DateOnly.MinValue,
                    EndYmd = DateOnly.MaxValue,
                    Name = expectedBusyoName2,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = フリー,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            db.AddRange([syain1, syain2]);
            db.SaveChanges();

            // 検索条件の設定
            model.SearchConditions.Busyo.Id = null;
            model.SearchConditions.Busyo.Range = 全社;
            model.SearchConditions.Date = inputDate;

            // Act
            // ----------------------------------

            await model.OnGetSearchNippousAsync();
            var viewModels = model.MikakuteiSyains;

            // Assert
            // ----------------------------------

            // データを取得していること
            Assert.IsNotEmpty(viewModels);
            Assert.HasCount(2, viewModels);
            var actualViewModel1 = viewModels.First(v => v.SyainBaseId == expectedSyainBaseId1);
            var actualViewModel2 = viewModels.First(v => v.SyainBaseId == expectedSyainBaseId2);

            AssertCorrectViewModel(actualViewModel1, expectedBusyoName1, expectedSyainCode1, expectedSyainName1,
                expectedSyainBaseId1);
            AssertCorrectViewModel(actualViewModel2, expectedBusyoName2, expectedSyainCode2, expectedSyainName2,
                expectedSyainBaseId2);
        }

        [TestMethod]
        public async Task OnGetSearchNippousAsync_検索範囲が部署_指定の部署を取得する()
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 7, 15));
            var inputDate = new DateOnly(2026, 2, 15);
            var model = CreateIndexModel();

            var expectedBusyoName1 = "部署名称1";
            var expectedSyainCode1 = "12345";
            var expectedSyainName1 = "社員氏名1";
            var expectedSyainBaseId1 = 12345;
            var busyoId1 = 1;
            var expectedBusyoName2 = "部署名称2";
            var expectedSyainCode2 = "123456";
            var expectedSyainName2 = "社員氏名2";
            var expectedSyainBaseId2 = 123456;
            var busyoId2 = 2;

            var syain1 = new Syain()
            {
                Name = expectedSyainName1,
                Code = expectedSyainCode1,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Retired = false,
                SyainBase = new()
                {
                    Id = expectedSyainBaseId1,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    Id = busyoId1,
                    StartYmd = DateOnly.MinValue,
                    EndYmd = DateOnly.MaxValue,
                    Name = expectedBusyoName1,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = フリー,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            var syain2 = new Syain()
            {
                Name = expectedSyainName2,
                Code = expectedSyainCode2,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Retired = false,
                SyainBase = new()
                {
                    Id = expectedSyainBaseId2,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    Id = busyoId2,
                    StartYmd = DateOnly.MinValue,
                    EndYmd = DateOnly.MaxValue,
                    Name = expectedBusyoName2,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = フリー,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            db.AddRange([syain1, syain2]);
            db.SaveChanges();

            // 検索条件の設定
            model.SearchConditions.Busyo.Id = busyoId1;
            model.SearchConditions.Busyo.Range = 部署;
            model.SearchConditions.Date = inputDate;

            // Act
            // ----------------------------------

            await model.OnGetSearchNippousAsync();
            var viewModels = model.MikakuteiSyains;

            // Assert
            // ----------------------------------

            // データを取得していること
            var actualViewModel = viewModels.FirstOrDefault(v => v.SyainBaseId == expectedSyainBaseId1);
            Assert.IsNotNull(actualViewModel, "指定された部署が検索されていません。");

            AssertCorrectViewModel(actualViewModel, expectedBusyoName1, expectedSyainCode1, expectedSyainName1,
                expectedSyainBaseId1);
        }

        [TestMethod]
        public async Task OnGetSearchNippousAsync_検索範囲が部署_指定の部署以外を取得しない()
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 7, 15));
            var inputDate = new DateOnly(2026, 2, 15);
            var model = CreateIndexModel();

            var expectedBusyoName1 = "部署名称1";
            var expectedSyainCode1 = "12345";
            var expectedSyainName1 = "社員氏名1";
            var expectedSyainBaseId1 = 12345;
            var busyoId1 = 1;
            var expectedBusyoName2 = "部署名称2";
            var expectedSyainCode2 = "123456";
            var expectedSyainName2 = "社員氏名2";
            var expectedSyainBaseId2 = 123456;
            var busyoId2 = 2;

            var syain1 = new Syain()
            {
                Name = expectedSyainName1,
                Code = expectedSyainCode1,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Retired = false,
                SyainBase = new()
                {
                    Id = expectedSyainBaseId1,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    Id = busyoId1,
                    StartYmd = DateOnly.MinValue,
                    EndYmd = DateOnly.MaxValue,
                    Name = expectedBusyoName1,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = フリー,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            var syain2 = new Syain()
            {
                Name = expectedSyainName2,
                Code = expectedSyainCode2,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Retired = false,
                SyainBase = new()
                {
                    Id = expectedSyainBaseId2,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    Id = busyoId2,
                    StartYmd = DateOnly.MinValue,
                    EndYmd = DateOnly.MaxValue,
                    Name = expectedBusyoName2,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = フリー,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            db.AddRange([syain1, syain2]);
            db.SaveChanges();

            // 検索条件の設定
            model.SearchConditions.Busyo.Id = busyoId1;
            model.SearchConditions.Busyo.Range = 部署;
            model.SearchConditions.Date = inputDate;

            // Act
            // ----------------------------------

            await model.OnGetSearchNippousAsync();
            var viewModels = model.MikakuteiSyains;

            // Assert
            // ----------------------------------

            // データを取得していること
            var actualViewModel = viewModels.FirstOrDefault(v => v.SyainBaseId == expectedSyainBaseId2);
            Assert.IsNull(actualViewModel, "指定された部署以外が検索されています。");
        }

        [TestMethod]
        [DataRow(-1, DisplayName = "境界値")]
        [DataRow(-5, DisplayName = "代表値")]
        public async Task OnGetSearchNippousAsync_確定済みの日報が存在して最終確定日が検索条件の日付未満_取得する(int lastKakuteiNippouYmdOffset)
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 7, 15));
            var inputDate = new DateOnly(2026, 2, 15);
            var model = CreateIndexModel();

            var expectedBusyoName = "部署名称";
            var expectedSyainCode = "12345";
            var expectedSyainName = "社員氏名";
            var expectedSyainBaseId = 12345;
            var expectedLastKakuteiNippouYmd = inputDate.AddDays(lastKakuteiNippouYmdOffset);

            var syain = new Syain()
            {
                Name = expectedSyainName,
                Code = expectedSyainCode,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Retired = false,
                Nippous = [
                    new()
                    {
                        NippouYmd = expectedLastKakuteiNippouYmd,
                        TourokuKubun = 確定保存,
                        // 不要なNOT NULLカラムに値を詰める
                        Youbi = 0,
                        KaisyaCode = 協和,
                        IsRendouZumi = false,
                        SyukkinKubunId1 = 0,
                    },
                    // 確定済みだが取得されない日報
                    new()
                    {
                        NippouYmd = inputDate.AddDays(-6),
                        TourokuKubun = 確定保存,
                        // 不要なNOT NULLカラムに値を詰める
                        Youbi = 0,
                        KaisyaCode = 協和,
                        IsRendouZumi = false,
                        SyukkinKubunId1 = 0,
                    },
                    // 未確定日報
                    // 実績年月日が期待される日報よりも後だが最大値の計算に使用されない
                    new()
                    {
                        NippouYmd = inputDate,
                        TourokuKubun = 一時保存,
                        // 不要なNOT NULLカラムに値を詰める
                        Youbi = 0,
                        KaisyaCode = 協和,
                        IsRendouZumi = false,
                        SyukkinKubunId1 = 0,
                    },
                ],
                SyainBase = new()
                {
                    Id = expectedSyainBaseId,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    StartYmd = DateOnly.MinValue,
                    EndYmd = DateOnly.MaxValue,
                    Name = expectedBusyoName,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = フリー,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            db.Add(syain);
            db.SaveChanges();

            // 検索条件の設定
            model.SearchConditions.Busyo.Id = null;
            model.SearchConditions.Busyo.Range = 全社;
            model.SearchConditions.Date = inputDate;

            // Act
            // ----------------------------------

            await model.OnGetSearchNippousAsync();
            var viewModels = model.MikakuteiSyains;

            // Assert
            // ----------------------------------

            // データを取得していること
            Assert.IsNotEmpty(viewModels);
            Assert.HasCount(1, viewModels);
            var actualViewModel = viewModels.First();

            AssertCorrectViewModel(actualViewModel, expectedBusyoName, expectedSyainCode, expectedSyainName,
                expectedSyainBaseId, expectedLastKakuteiNippouYmd);
        }

        [TestMethod]
        [DataRow(0, DisplayName = "境界値")]
        [DataRow(5, DisplayName = "代表値")]
        public async Task OnGetSearchNippousAsync_確定済みの日報が存在して最終確定日が検索条件の日付以降_取得しない(int lastKakuteiNippouYmdOffset)
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 7, 15));
            var inputDate = new DateOnly(2026, 2, 15);
            var model = CreateIndexModel();

            var expectedBusyoName = "部署名称";
            var expectedSyainCode = "12345";
            var expectedSyainName = "社員氏名";
            var expectedSyainBaseId = 12345;
            var expectedLastKakuteiNippouYmd = inputDate.AddDays(lastKakuteiNippouYmdOffset);

            // 日報
            // 歯抜けだと不正データ扱いで取得されてしまうので一か月前くらいまで確定保存状態の日報で埋める
            var nippous = new List<Nippou>();
            for (int i = 0; i < 50; i++)
            {
                nippous.Add(new()
                {
                    NippouYmd = expectedLastKakuteiNippouYmd.AddDays(-i),
                    TourokuKubun = 確定保存,
                    // 不要なNOT NULLカラムに値を詰める
                    Youbi = 0,
                    KaisyaCode = 協和,
                    IsRendouZumi = false,
                    SyukkinKubunId1 = 0,
                });
            }

            var syain = new Syain()
            {
                Name = expectedSyainName,
                Code = expectedSyainCode,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Retired = false,
                Nippous = nippous,
                SyainBase = new()
                {
                    Id = expectedSyainBaseId,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    StartYmd = DateOnly.MinValue,
                    EndYmd = DateOnly.MaxValue,
                    Name = expectedBusyoName,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = フリー,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            db.Add(syain);
            db.SaveChanges();

            // 検索条件の設定
            model.SearchConditions.Busyo.Id = null;
            model.SearchConditions.Busyo.Range = 全社;
            model.SearchConditions.Date = inputDate;

            // Act
            // ----------------------------------

            await model.OnGetSearchNippousAsync();
            var viewModels = model.MikakuteiSyains;

            // Assert
            // ----------------------------------

            // データを取得していないこと
            Assert.IsEmpty(viewModels);
        }

        [TestMethod]
        [DataRow(true, DisplayName = "一時保存の日報のみ存在")]
        [DataRow(false, DisplayName = "日報が存在しない")]
        public async Task OnGetSearchNippousAsync_確定済みの日報が存在しない_最終確定日がnullの状態で取得する(bool isExistNippous)
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 7, 15));
            var inputDate = new DateOnly(2026, 2, 15);
            var model = CreateIndexModel();

            var expectedBusyoName = "部署名称";
            var expectedSyainCode = "12345";
            var expectedSyainName = "社員氏名";
            var expectedSyainBaseId = 12345;

            // 日報
            var nippous = new List<Nippou>();
            if (isExistNippous)
            {
                // 一時保存で過去も未来も埋める
                for (int i = -50; i < 50; i++)
                {
                    nippous.Add(new()
                    {
                        NippouYmd = inputDate.AddDays(i),
                        TourokuKubun = 一時保存,
                        // 不要なNOT NULLカラムに値を詰める
                        Youbi = 0,
                        KaisyaCode = 協和,
                        IsRendouZumi = false,
                        SyukkinKubunId1 = 0,
                    });
                }
            }

            var syain = new Syain()
            {
                Name = expectedSyainName,
                Code = expectedSyainCode,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Retired = false,
                Nippous = nippous,
                SyainBase = new()
                {
                    Id = expectedSyainBaseId,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    StartYmd = DateOnly.MinValue,
                    EndYmd = DateOnly.MaxValue,
                    Name = expectedBusyoName,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = フリー,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            db.Add(syain);
            db.SaveChanges();

            // 検索条件の設定
            model.SearchConditions.Busyo.Id = null;
            model.SearchConditions.Busyo.Range = 全社;
            model.SearchConditions.Date = inputDate;

            // Act
            // ----------------------------------

            await model.OnGetSearchNippousAsync();
            var viewModels = model.MikakuteiSyains;

            // Assert
            // ----------------------------------

            // データを取得していること
            Assert.IsNotEmpty(viewModels);
            Assert.HasCount(1, viewModels);
            var actualViewModel = viewModels.First();

            AssertCorrectViewModel(actualViewModel, expectedBusyoName, expectedSyainCode, expectedSyainName,
                expectedSyainBaseId, null);
        }

        [TestMethod]
        [DataRow(0, -10, DisplayName = "最終確定日の境界値")]
        [DataRow(5, 10, DisplayName = "件数が多い場合の代表値")]
        [DataRow(5, 1, DisplayName = "件数が多い場合の境界値")]
        [DataRow(5, -1, DisplayName = "件数が少ない場合の境界値")]
        [DataRow(5, -10, DisplayName = "件数が少ない場合の代表値")]
        public async Task OnGetSearchNippousAsync_最終確定日が検索条件日付以降だが過去一か月に確定状態でない日報が存在する_不正データとして取得する(
            int nippouYmdDayOffset, int nippousCountOffset)
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 7, 15));
            var inputDate = new DateOnly(2026, 2, 15);
            var expectedLastKakuteiNippouYmd = inputDate.AddDays(nippouYmdDayOffset);
            var model = CreateIndexModel();

            var expectedBusyoName = "部署名称";
            var expectedSyainCode = "12345";
            var syainName = "社員氏名";
            var expectedSyainBaseId = 12345;

            // 日報データを作成
            var nippous = new List<Nippou>()
            {
                // 最終確定日が検索条件の日付以降になるような日報
                new()
                {
                    NippouYmd = expectedLastKakuteiNippouYmd,
                    TourokuKubun = 確定保存,
                    // 不要なNOT NULLカラムに値を詰める
                    Youbi = 0,
                    KaisyaCode = 協和,
                    IsRendouZumi = false,
                    SyukkinKubunId1 = 0,
                }
            };
            // 過去一か月間の日数
            var cnt = GetDayCount(inputDate.AddMonths(-1), inputDate);
            DateOnly nippouYmd;
            // 過去一か月間の日報データを作成
            for (int i = 0; i < cnt + nippousCountOffset - 1; i++)
            {
                // 0 <= i < cnt - 1 ⇒ inputDateの1, 2, ..., cnt - 1日前が順番に設定される
                // cnt - 1 <= i ⇒ 再び1, 2, ...日前が追加される
                nippouYmd = inputDate.AddDays(-(i % (cnt - 1) + 1));
                nippous.Add(new()
                {
                    NippouYmd = nippouYmd,
                    TourokuKubun = 確定保存,
                    // 不要なNOT NULLカラムに値を詰める
                    Youbi = 0,
                    KaisyaCode = 協和,
                    IsRendouZumi = false,
                    SyukkinKubunId1 = 0,
                });
            }
            // もし検索条件の日付の日報がなければ追加
            // この時点で、日報データは
            // [cnt - |nippousCountOffset| - 1日前, ..., 1日前, 当日, nippouYmdDayOffset日後] + 重複分
            // となっている
            if (nippouYmdDayOffset > 0)
            {
                nippous.Add(new()
                {
                    NippouYmd = inputDate,
                    TourokuKubun = 確定保存,
                    // 不要なNOT NULLカラムに値を詰める
                    Youbi = 0,
                    KaisyaCode = 協和,
                    IsRendouZumi = false,
                    SyukkinKubunId1 = 0,
                });
            }

            var syain = new Syain()
            {
                Name = syainName,
                Code = expectedSyainCode,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Retired = false,
                Nippous = nippous,
                SyainBase = new()
                {
                    Id = expectedSyainBaseId,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    StartYmd = DateOnly.MinValue,
                    EndYmd = DateOnly.MaxValue,
                    Name = expectedBusyoName,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = フリー,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            db.Add(syain);
            db.SaveChanges();

            // 検索条件の設定
            model.SearchConditions.Busyo.Id = null;
            model.SearchConditions.Busyo.Range = 全社;
            model.SearchConditions.Date = inputDate;

            // Act
            // ----------------------------------

            await model.OnGetSearchNippousAsync();
            var viewModels = model.MikakuteiSyains;

            // Assert
            // ----------------------------------

            // データを取得していること
            Assert.IsNotEmpty(viewModels);
            Assert.HasCount(1, viewModels);
            var actualViewModel = viewModels.First();

            // データ不正ありとして社員氏名は取得されているはず
            var expectedSyainName = $"{syainName}{BadNippouSuffix}";
            AssertCorrectViewModel(actualViewModel, expectedBusyoName, expectedSyainCode, expectedSyainName,
                expectedSyainBaseId, expectedLastKakuteiNippouYmd);
        }

        [TestMethod]
        public async Task OnGetSearchNippousAsync_最終確定日が検索条件日付以降で過去一か月に確定状態でない日報が存在しない_取得しない()
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 7, 15));
            var inputDate = new DateOnly(2026, 2, 15);
            var expectedLastKakuteiNippouYmd = inputDate.AddDays(5);
            var model = CreateIndexModel();

            var expectedBusyoName = "部署名称";
            var expectedSyainCode = "12345";
            var expectedSyainName = "社員氏名";
            var expectedSyainBaseId = 12345;

            // 日報
            // 一か月前くらいまで確定保存状態の日報で埋める
            var nippous = new List<Nippou>();
            for (int i = 0; i < 50; i++)
            {
                nippous.Add(new()
                {
                    NippouYmd = expectedLastKakuteiNippouYmd.AddDays(-i),
                    TourokuKubun = 確定保存,
                    // 不要なNOT NULLカラムに値を詰める
                    Youbi = 0,
                    KaisyaCode = 協和,
                    IsRendouZumi = false,
                    SyukkinKubunId1 = 0,
                });
            }

            var syain = new Syain()
            {
                Name = expectedSyainName,
                Code = expectedSyainCode,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Retired = false,
                Nippous = nippous,
                SyainBase = new()
                {
                    Id = expectedSyainBaseId,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                },
                Busyo = new()
                {
                    StartYmd = DateOnly.MinValue,
                    EndYmd = DateOnly.MaxValue,
                    Name = expectedBusyoName,
                    // 不要なNOT NULLカラムに値を詰める
                    Code = string.Empty,
                    KanaName = string.Empty,
                    OyaCode = string.Empty,
                    Jyunjyo = 0,
                    KasyoCode = string.Empty,
                    KaikeiCode = string.Empty,
                    IsActive = false,
                    BusyoBaseId = 0,
                },
                KintaiZokusei = new()
                {
                    Code = フリー,
                    // 不要なNOT NULLカラムに値を詰める
                    Name = string.Empty,
                    SeigenTime = 0,
                    IsMinashi = false,
                    MaxLimitTime = 0,
                    IsOvertimeLimit3m = false,
                },
                // 不要なNOT NULLカラムに値を詰める
                KanaName = string.Empty,
                Seibetsu = '0',
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = _7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 0,
                UserRoleId = 0,
            };
            db.Add(syain);
            db.SaveChanges();

            // 検索条件の設定
            model.SearchConditions.Busyo.Id = null;
            model.SearchConditions.Busyo.Range = 全社;
            model.SearchConditions.Date = inputDate;

            // Act
            // ----------------------------------

            await model.OnGetSearchNippousAsync();
            var viewModels = model.MikakuteiSyains;

            // Assert
            // ----------------------------------

            // データを取得していないこと
            Assert.IsEmpty(viewModels);
        }

        [TestMethod]
        public async Task OnGetSearchNippousAsync_並び順_daiソートが部署番号の昇順()
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 7, 15));
            var now = fakeTimeProvider.Now();
            var inputDate = new DateOnly(2026, 2, 15);
            var model = CreateIndexModel();

            for (int i = 0; i < 3; i++)
            {
                // 順番をシャッフルするための変数
                // これを格納したカラムは2, 3, 1の順に格納される
                var shuffleNumber = (short)((i + 1) % 3 + 1);
                var shuffleStr = shuffleNumber.ToString() + 1;

                db.Add(new Syain()
                {
                    Code = shuffleStr,
                    Busyo = new()
                    {
                        // 0, 1, 2の順で格納される
                        // 部署番号でソートされているなら、他カラムは2, 3, 1の順に並ぶ
                        Code = i.ToString(),
                        // 他カラムでソートされていないことを確認するためにすべてシャッフルする
                        Id = shuffleNumber,
                        StartYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                        EndYmd = DateOnly.MaxValue.AddDays(-shuffleNumber),
                        Name = shuffleStr,
                        KanaName = shuffleStr,
                        OyaCode = shuffleStr,
                        Jyunjyo = shuffleNumber,
                        KasyoCode = shuffleStr,
                        KaikeiCode = shuffleStr,
                        IsActive = false,
                        BusyoBaseId = shuffleNumber,
                        KeiriCode = shuffleStr,
                        Ryakusyou = shuffleStr,
                        OyaId = shuffleNumber,
                        ShoninBusyoId = shuffleNumber,
                    },
                    Nippous = [
                        new()
                        {
                            Id = shuffleNumber,
                            NippouYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                            Youbi = shuffleNumber,
                            SyukkinHm1 = new(shuffleNumber),
                            SyukkinHm2 = new(shuffleNumber),
                            SyukkinHm3 = new(shuffleNumber),
                            TaisyutsuHm1 = new(shuffleNumber),
                            TaisyutsuHm2 = new(shuffleNumber),
                            TaisyutsuHm3 = new(shuffleNumber),
                            HJitsudou  = shuffleNumber,
                            HZangyo = shuffleNumber,
                            HWarimashi = shuffleNumber,
                            HShinyaZangyo = shuffleNumber,
                            DJitsudou = shuffleNumber,
                            DZangyo = shuffleNumber,
                            DWarimashi = shuffleNumber,
                            DShinyaZangyo = shuffleNumber,
                            NJitsudou = shuffleNumber,
                            NShinya = shuffleNumber,
                            TotalZangyo = shuffleNumber,
                            KaisyaCode = 協和,
                            IsRendouZumi = false,
                            RendouYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                            TourokuKubun = 一時保存,
                            KakuteiYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                            SyukkinKubunId1 = shuffleNumber,
                            SyukkinKubunId2 = shuffleNumber,
                        }
                    ],
                    Id = shuffleNumber,
                    Name = shuffleStr,
                    StartYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                    EndYmd = DateOnly.MaxValue.AddDays(-shuffleNumber),
                    Retired = false,
                    KanaName = shuffleStr,
                    Seibetsu = shuffleStr.ToCharArray()[0],
                    BusyoCode = shuffleStr,
                    SyokusyuCode = shuffleNumber,
                    NyuusyaYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                    Kyusyoku = shuffleNumber,
                    SyucyoSyokui = _7_8級,
                    KingsSyozoku = shuffleStr,
                    KaisyaCode = shuffleNumber,
                    IsGenkaRendou = false,
                    Kengen = None,
                    Jyunjyo = shuffleNumber,
                    UserRoleId = shuffleNumber,
                    EMail = shuffleStr,
                    KeitaiMail = shuffleStr,
                    GyoumuTypeId = shuffleNumber,
                    PhoneNumber = shuffleStr,
                    SyainBase = new()
                    {
                        Id = shuffleNumber,
                        Name = shuffleStr,
                        Code = shuffleStr,
                    },
                    KintaiZokusei = new()
                    {
                        Code = フリー,
                        Name = shuffleStr,
                        SeigenTime = shuffleNumber,
                        IsMinashi = false,
                        MaxLimitTime = shuffleNumber,
                        IsOvertimeLimit3m = false,
                    },
                });
            }
            db.SaveChanges();

            // 検索条件の設定
            model.SearchConditions.Busyo.Id = null;
            model.SearchConditions.Busyo.Range = 全社;
            model.SearchConditions.Date = inputDate;

            // Act
            // ----------------------------------

            await model.OnGetSearchNippousAsync();
            var viewModels = model.MikakuteiSyains;

            // Assert
            // ----------------------------------

            var expectedSyainBaseIds = new List<long>() { 2, 3, 1 };
            CollectionAssert.AreEqual(expectedSyainBaseIds, viewModels.Select(v => v.SyainBaseId).ToList());
        }

        [TestMethod]
        public async Task OnGetSearchNippousAsync_同一部署コード内の並び順_社員番号の昇順()
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 7, 15));
            var now = fakeTimeProvider.Now();
            var inputDate = new DateOnly(2026, 2, 15);
            var model = CreateIndexModel();

            for (int i = 0; i < 3; i++)
            {
                // 順番をシャッフルするための変数
                // これを格納したカラムは2, 3, 1の順に格納される
                var shuffleNumber = (short)((i + 1) % 3 + 1);
                var shuffleStr = shuffleNumber.ToString() + 1;

                db.Add(new Syain()
                {
                    // 0, 1, 2の順で格納される
                    // 社員番号でソートされているなら、他カラムは2, 3, 1の順に並ぶ
                    Code = i.ToString(),
                    Busyo = new()
                    {
                        // 同一部署にする
                        Code = "11111",
                        // 他カラムでソートされていないことを確認するためにすべてシャッフルする
                        Id = shuffleNumber,
                        StartYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                        EndYmd = DateOnly.MaxValue.AddDays(-shuffleNumber),
                        Name = shuffleStr,
                        KanaName = shuffleStr,
                        OyaCode = shuffleStr,
                        Jyunjyo = shuffleNumber,
                        KasyoCode = shuffleStr,
                        KaikeiCode = shuffleStr,
                        IsActive = false,
                        BusyoBaseId = shuffleNumber,
                        KeiriCode = shuffleStr,
                        Ryakusyou = shuffleStr,
                        OyaId = shuffleNumber,
                        ShoninBusyoId = shuffleNumber,
                    },
                    Nippous = [
                        new()
                        {
                            Id = shuffleNumber,
                            NippouYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                            Youbi = shuffleNumber,
                            SyukkinHm1 = new(shuffleNumber),
                            SyukkinHm2 = new(shuffleNumber),
                            SyukkinHm3 = new(shuffleNumber),
                            TaisyutsuHm1 = new(shuffleNumber),
                            TaisyutsuHm2 = new(shuffleNumber),
                            TaisyutsuHm3 = new(shuffleNumber),
                            HJitsudou  = shuffleNumber,
                            HZangyo = shuffleNumber,
                            HWarimashi = shuffleNumber,
                            HShinyaZangyo = shuffleNumber,
                            DJitsudou = shuffleNumber,
                            DZangyo = shuffleNumber,
                            DWarimashi = shuffleNumber,
                            DShinyaZangyo = shuffleNumber,
                            NJitsudou = shuffleNumber,
                            NShinya = shuffleNumber,
                            TotalZangyo = shuffleNumber,
                            KaisyaCode = 協和,
                            IsRendouZumi = false,
                            RendouYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                            TourokuKubun = 一時保存,
                            KakuteiYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                            SyukkinKubunId1 = shuffleNumber,
                            SyukkinKubunId2 = shuffleNumber,
                        }
                    ],
                    Id = shuffleNumber,
                    Name = shuffleStr,
                    StartYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                    EndYmd = DateOnly.MaxValue.AddDays(-shuffleNumber),
                    Retired = false,
                    KanaName = shuffleStr,
                    Seibetsu = shuffleStr.ToCharArray()[0],
                    BusyoCode = shuffleStr,
                    SyokusyuCode = shuffleNumber,
                    NyuusyaYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                    Kyusyoku = shuffleNumber,
                    SyucyoSyokui = _7_8級,
                    KingsSyozoku = shuffleStr,
                    KaisyaCode = shuffleNumber,
                    IsGenkaRendou = false,
                    Kengen = None,
                    Jyunjyo = shuffleNumber,
                    UserRoleId = shuffleNumber,
                    EMail = shuffleStr,
                    KeitaiMail = shuffleStr,
                    GyoumuTypeId = shuffleNumber,
                    PhoneNumber = shuffleStr,
                    SyainBase = new()
                    {
                        Id = shuffleNumber,
                        Name = shuffleStr,
                        Code = shuffleStr,
                    },
                    KintaiZokusei = new()
                    {
                        Code = フリー,
                        Name = shuffleStr,
                        SeigenTime = shuffleNumber,
                        IsMinashi = false,
                        MaxLimitTime = shuffleNumber,
                        IsOvertimeLimit3m = false,
                    },
                });
            }
            db.SaveChanges();

            // 検索条件の設定
            model.SearchConditions.Busyo.Id = null;
            model.SearchConditions.Busyo.Range = 全社;
            model.SearchConditions.Date = inputDate;

            // Act
            // ----------------------------------

            await model.OnGetSearchNippousAsync();
            var viewModels = model.MikakuteiSyains;

            // Assert
            // ----------------------------------

            var expectedSyainBaseIds = new List<long>() { 2, 3, 1 };
            CollectionAssert.AreEqual(expectedSyainBaseIds, viewModels.Select(v => v.SyainBaseId).ToList());
        }
    }
}

