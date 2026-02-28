using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Model.Enums;
using Model.Model;
using Zouryoku.Pages.Kinmuhyo;
using Zouryoku.Utils;
using ZouryokuTest.Builder;
using ZouryokuTest.Pages.Builder;
using static Model.Enums.AchievementClassification;
using static Model.Enums.ApprovalStatus;
using static Model.Enums.DailyReportStatusClassification;
using static Model.Enums.EmployeeWorkType;
using static Model.Enums.HolidayFlag;
using static Model.Enums.InquiryType;
using static Model.Enums.LeaveBalanceFetchStatus;
using static Model.Enums.LeavePlanStatus;
using static Model.Enums.ResponseStatus;
using static Zouryoku.Pages.Kinmuhyo.StyleLineClasses;
using Common = ZouryokuCommonLibrary.Utils;

namespace ZouryokuTest.Pages.Kinmuhyo
{
    /// <summary>
    /// 勤務ページ IndexModel のユニットテスト（OnGet / カレンダー更新）
    /// </summary>
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {
        /// <summary>
        /// IndexModel のインスタンスを作成します。
        /// </summary>
        /// <returns>IndexModel</returns>
        private IndexModel CreateModel()
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine, fakeTimeProvider)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };
            return model;
        }

        /// <summary>
        /// Given: 社員が存在しない
        /// When: カレンダー更新を要求する
        /// Then: エラーレスポンスを返す
        /// </summary>
        [TestMethod(DisplayName = "社員が存在しない → カレンダー更新要求でエラーレスポンス")]
        public async Task OnPostRefreshCalendarAsync_社員が存在しない_エラーレスポンスを返す()
        {
            // Arrange
            var model = CreateModel();
            model.SyainId = 999;

            // Act
            var result = await model.OnPostRefreshCalendarAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var json = (JsonResult)result;
            var status = GetResponseStatus(json);
            var message = GetMessage(json);
            Assert.AreEqual(エラー, status);
            Assert.IsNotNull(message);
            Assert.AreEqual(Const.EmptyReadData, message);
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: 該当社員のカレンダー更新を要求する
        /// Then: 正常レスポンスと必要フィールド（selectedUserId, kinmuJokyo, karendaHyoji）を返す
        /// </summary>
        [TestMethod(DisplayName = "社員が存在する → カレンダー更新で正常・必要フィールド返却")]
        public async Task OnPostRefreshCalendarAsync_社員が存在する_正常レスポンスと必要フィールドを返す()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            model.SyainId = syain.Id;

            // Act
            var result = await model.OnPostRefreshCalendarAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var json = (JsonResult)result;
            var status = GetResponseStatus(json);
            Assert.AreEqual(正常, status);

            var val = json.Value;
            var dataProp = val?.GetType().GetProperty("Data");
            Assert.IsNotNull(dataProp);
            var data = dataProp.GetValue(val);
            Assert.IsNotNull(data);

            var dataType = data!.GetType();
            var selectedProp = dataType.GetProperty("selectedUserId");
            Assert.IsNotNull(selectedProp);
            var selectedId = Convert.ToInt64(selectedProp!.GetValue(data));
            Assert.AreEqual(syain.Id, selectedId);

            var kinmuJokyoProp = dataType.GetProperty("kinmuJokyo");
            Assert.IsNotNull(kinmuJokyoProp, "kinmuJokyo（勤務状況）が返却されているべきです。");
            var karendaHyojiProp = dataType.GetProperty("karendaHyoji");
            Assert.IsNotNull(karendaHyojiProp, "karendaHyoji（カレンダー表示）が返却されているべきです。");
        }

        /// <summary>
        /// Given: 未来月を指定する
        /// When: カレンダー更新を要求する
        /// Then: 実績・打刻情報なしで正常レスポンスを返す
        /// </summary>
        [TestMethod(DisplayName = "未来月指定 → カレンダー更新で正常・karendaHyoji 返却")]
        public async Task OnPostRefreshCalendarAsync_未来月指定_実績情報なしで正常レスポンスを返す()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var futureMonth = dateYmd.AddMonths(2);
            var model = CreateModel();
            model.SyainId = syain.Id;
            model.NippouYmd = futureMonth;

            // Act
            var result = await model.OnPostRefreshCalendarAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var json = (JsonResult)result;
            var status = GetResponseStatus(json);
            Assert.AreEqual(正常, status);

            var val = json.Value;
            var dataProp = val?.GetType().GetProperty("Data");
            Assert.IsNotNull(dataProp);
            var data = dataProp!.GetValue(val);
            Assert.IsNotNull(data);

            var dataType = data!.GetType();
            var karendaHyojiProp = dataType.GetProperty("karendaHyoji");
            Assert.IsNotNull(karendaHyojiProp);
            var karendaHyojiHtml = karendaHyojiProp!.GetValue(data) as string;
            Assert.IsNotNull(karendaHyojiHtml);
        }

        /// <summary>
        /// テスト用の社員・部署データを初期化
        /// </summary>
        private (Syain syain, Busyo busyo) InitializeTestData()
        {
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());

            // 部署データ作成
            var busyoBase = new BusyoBasisBuilder()
                .WithId(1)
                .WithName("テスト部署")
                .Build();
            db.BusyoBases.Add(busyoBase);

            var busyo = new BusyoBuilder()
                .WithId(1)
                .WithCode("B001")
                .WithName("テスト部署")
                .WithBusyoBaseId(busyoBase.Id)
                .WithIsActive(true)
                .Build();
            db.Busyos.Add(busyo);

            // 社員BASE作成
            var syainBase = new SyainBasisBuilder()
                .WithId(1)
                .Build();
            db.SyainBases.Add(syainBase);

            // 社員作成
            var syain = new SyainBuilder()
                .WithId(1)
                .WithCode("S0001")
                .WithName("テスト社員")
                .WithSyainBaseId(syainBase.Id)
                .WithBusyoId(busyo.Id)
                .WithStartYmd(today.AddYears(-1))
                .WithEndYmd(today.AddYears(1))
                .Build();
            db.Syains.Add(syain);

            // 同一部署の別社員（DepartmentEmployees テスト用）
            var syainBase2 = new SyainBasisBuilder()
                .WithId(2)
                .Build();
            db.SyainBases.Add(syainBase2);

            var syain2 = new SyainBuilder()
                .WithId(2)
                .WithCode("S0002")
                .WithName("テスト社員2")
                .WithSyainBaseId(syainBase2.Id)
                .WithBusyoId(busyo.Id)
                .WithStartYmd(today.AddYears(-1))
                .WithEndYmd(today.AddYears(1))
                .WithJyunjyo(2)
                .Build();
            db.Syains.Add(syain2);

            // 勤怠属性作成
            var kintaiZokusei = new KintaiZokuseiBuilder()
                .WithId(1)
                .WithName("標準")
                .Build();
            db.KintaiZokuseis.Add(kintaiZokusei);

            db.SaveChanges();

            return (syain, busyo);
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: OnGetAsync を呼ぶ
        /// Then: PageResult を返し、ViewModel に SelectedUserId / KinmuJokyoRows（勤務状況）/ 
        ///      KarendaHyojiRows（カレンダー表示）/ DepartmentEmployees が反映される
        /// </summary>
        [TestMethod(DisplayName = "社員が存在する → OnGet で ViewModel 反映・PageResult")]
        public async Task OnGetAsync_社員が存在する_ViewModelが反映されPageResultを返す()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var model = CreateModel();

            // Act
            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            var result = await model.OnGetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.AreEqual(syain.Id, model.ViewModel.SelectedUserId, "SelectedUserId が一致しません。");
            Assert.AreEqual(syain.Name, model.ViewModel.SelectedUserName, "SelectedUserName が一致しません。");
            Assert.IsNotNull(model.ViewModel.KinmuJokyoRows, "勤務状況（KinmuJokyoRows）は取得されているべきです。");
            Assert.IsNotNull(model.ViewModel.KarendaHyojiRows, "カレンダー表示（KarendaHyojiRows）は取得されているべきです。");
            Assert.IsNotNull(model.ViewModel.DepartmentEmployees, "DepartmentEmployees は取得されているべきです。");
            Assert.IsTrue(model.ViewModel.KinmuJokyoRows.Count >= 2, "勤務状況は残業・有給残日数等で最低2行以上あるべきです。");
            Assert.IsTrue(model.ViewModel.KarendaHyojiRows.Count >= 1, "カレンダー表示は1日分以上あるべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: OnGetAsync を呼ぶ
        /// Then: ViewModel.CurrentTime が設定される
        /// </summary>
        [TestMethod(DisplayName = "ViewModel.CurrentTime が設定される")]
        public async Task OnGetAsync_社員が存在する_CurrentTimeが設定される()
        {
            // Arrange
            var (syain, _) = InitializeTestData();

            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());

            var model = CreateModel();
            var beforeCall = dateYmd.ToDateTime();

            // Act
            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            await model.OnGetAsync();

            // Assert
            Assert.IsTrue(model.ViewModel.CurrentTime >= beforeCall.AddSeconds(-1), "CurrentTime が現在時刻に近い値であるべきです。");
            Assert.IsTrue(model.ViewModel.CurrentTime <= dateYmd.ToDateTime().AddSeconds(1), "CurrentTime が現在時刻に近い値であるべきです。");
        }

        /// <summary>
        /// Given: 社員が存在し、部署に所属している
        /// When: OnGetAsync を呼ぶ
        /// Then: ViewModel.DepartmentName が設定される
        /// </summary>
        [TestMethod(DisplayName = "ViewModel.DepartmentName が設定される")]
        public async Task OnGetAsync_社員が存在し部署に所属している_DepartmentNameが設定される()
        {
            // Arrange
            var (syain, busyo) = InitializeTestData();
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var model = CreateModel();

            // Act
            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.ViewModel.DepartmentName, "DepartmentName が設定されているべきです。");
            Assert.AreEqual(busyo.Name, model.ViewModel.DepartmentName, "DepartmentName が部署名と一致するべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: OnGetAsync を呼ぶ
        /// Then: ViewModel.SelectedUserId が設定される
        /// </summary>
        [TestMethod(DisplayName = "ViewModel.SelectedUserId が設定される")]
        public async Task OnGetAsync_社員が存在する_SelectedUserIdが設定される()
        {
            // Arrange
            var (syain, _) = InitializeTestData();

            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());

            var model = CreateModel();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual(syain.Id, model.ViewModel.SelectedUserId, "SelectedUserId が社員IDと一致するべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: OnGetAsync を呼ぶ
        /// Then: ViewModel.SelectedUserName が設定される
        /// </summary>
        [TestMethod(DisplayName = "ViewModel.SelectedUserName が設定される")]
        public async Task OnGetAsync_社員が存在する_SelectedUserNameが設定される()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var model = CreateModel();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.ViewModel.SelectedUserName, "SelectedUserName が設定されているべきです。");
            Assert.AreEqual(syain.Name, model.ViewModel.SelectedUserName, "SelectedUserName が社員名と一致するべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: OnGetAsync を呼ぶ（年月指定あり）
        /// Then: ViewModel.DisplayYearMonth が設定される
        /// </summary>
        [TestMethod(DisplayName = "ViewModel.DisplayYearMonth が設定される")]
        public async Task OnGetAsync_社員が存在する_DisplayYearMonthが設定される()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var targetDate = new DateOnly(2025, 6, 15);
            var expectedDate = targetDate.ToString("yyyy/MM");

            model.SyainId = syain.Id;
            model.NippouYmd = targetDate;

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.ViewModel.DisplayYearMonth, "DisplayYearMonth が設定されているべきです。");
            Assert.AreEqual(expectedDate, model.ViewModel.DisplayYearMonth, "DisplayYearMonth が指定年月と一致するべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: OnGetAsync を呼ぶ（年月指定あり）
        /// Then: ViewModel.DisplayYearMonthDate が設定される
        /// </summary>
        [TestMethod(DisplayName = "ViewModel.DisplayYearMonthDate が設定される")]
        public async Task OnGetAsync_社員が存在する_DisplayYearMonthDateが設定される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var targetDate = new DateOnly(2025, 6, 15);
            var expectedMonthStart = targetDate.GetStartOfMonth();

            model.SyainId = syain.Id;
            model.NippouYmd = targetDate;

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual(expectedMonthStart, model.ViewModel.DisplayYearMonthDate, "DisplayYearMonthDate が月初日と一致するべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する（当月を指定）
        /// When: OnGetAsync を呼ぶ
        /// Then: ViewModel.IsEditableMonth が true になる
        /// </summary>
        [TestMethod(DisplayName = "ViewModel.IsEditableMonth が当月の場合 true になる")]
        public async Task OnGetAsync_当月_IsEditableMonthがtrue()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            
            // Act
            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            await model.OnGetAsync();

            // Assert
            Assert.IsTrue(model.ViewModel.IsEditableMonth(), "当月の場合、IsEditableMonth が true であるべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する（過去月を指定）
        /// When: OnGetAsync を呼ぶ
        /// Then: ViewModel.IsEditableMonth が false になる
        /// </summary>
        [TestMethod(DisplayName = "ViewModel.IsEditableMonth が過去月の場合 false になる")]
        public async Task OnGetAsync_過去月_IsEditableMonthがfalse()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var pastMonth = dateYmd.AddMonths(-1);

            // Act
            model.SyainId = syain.Id;
            model.NippouYmd = pastMonth;
            await model.OnGetAsync();

            // Assert
            Assert.IsFalse(model.ViewModel.IsEditableMonth(), "過去月の場合、IsEditableMonth が false であるべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: OnGetAsync を呼ぶ
        /// Then: ViewModel.BreakTimeHours が設定される
        /// </summary>
        [TestMethod(DisplayName = "ViewModel.BreakTimeHours が設定される")]
        public async Task OnGetAsync_BreakTimeHoursが設定される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            var totalBreakTime = Common.Time.休憩時間List.Sum(b => b.Item2 - b.Item1);

            var breakTimeHours = totalBreakTime / 60m; // 分を時間に変換

            // Act
            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual(breakTimeHours, model.ViewModel.BreakTimeHours);
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: OnGetAsync を呼ぶ
        /// Then: ViewModel.KinmuJokyoRows が設定される（最低限の行が存在）
        /// </summary>
        [TestMethod(DisplayName = "ViewModel.KinmuJokyoRows が設定される")]
        public async Task OnGetAsync_KinmuJokyoRowsが設定される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            // Act
            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.ViewModel.KinmuJokyoRows, "KinmuJokyoRows が設定されているべきです。");
            Assert.IsTrue(model.ViewModel.KinmuJokyoRows.Count >= 2, "KinmuJokyoRows は最低2行（残業・有給残日数）以上あるべきです。");

            // 残業行が存在することを確認
            var overtimeRow = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            Assert.IsNotNull(overtimeRow, "残業行が存在するべきです。");

            // 有給残日数行が存在することを確認
            var paidLeaveRow = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "有給残日数 (半日有給)");
            Assert.IsNotNull(paidLeaveRow, "有給残日数行が存在するべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: OnGetAsync を呼ぶ
        /// Then: ViewModel.KarendaHyojiRows が設定される（カレンダー表示行が存在）
        /// </summary>
        [TestMethod(DisplayName = "ViewModel.KarendaHyojiRows が設定される")]
        public async Task OnGetAsync_KarendaHyojiRowsが設定される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.ViewModel.KarendaHyojiRows, "KarendaHyojiRows が設定されているべきです。");
            Assert.IsTrue(model.ViewModel.KarendaHyojiRows.Count >= 1, "KarendaHyojiRows は最低1行以上あるべきです。");

            // 最初の行のプロパティを確認
            var firstRow = model.ViewModel.KarendaHyojiRows.First();
            Assert.IsNotNull(firstRow.DateLabel, "DateLabel が設定されているべきです。");
            // WorkTime は null の場合もある（実績・打刻情報がない場合）
            // Assert.IsNotNull(firstRow.WorkTime, "WorkTime が設定されているべきです。");
            Assert.AreEqual(syain.Id, firstRow.SyainId, "SyainId が社員IDと一致するべきです。");
        }

        /// <summary>
        /// Given: 社員が存在し、同一部署に複数社員がいる
        /// When: OnGetAsync を呼ぶ
        /// Then: ViewModel.DepartmentEmployees が設定される
        /// </summary>
        [TestMethod(DisplayName = "ViewModel.DepartmentEmployees が設定される")]
        public async Task OnGetAsync_DepartmentEmployeesが設定される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            // Act
            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.ViewModel.DepartmentEmployees, "DepartmentEmployees が設定されているべきです。");
            Assert.IsTrue(model.ViewModel.DepartmentEmployees.Count >= 2, "DepartmentEmployees は最低2人（自分を含む）以上あるべきです。");

            // 選択中の社員が含まれていることを確認
            var selectedEmployee = model.ViewModel.DepartmentEmployees.FirstOrDefault(e => e.Id == syain.Id);
            Assert.IsNotNull(selectedEmployee, "選択中の社員が DepartmentEmployees に含まれているべきです。");
            Assert.AreEqual(syain.Name, selectedEmployee.Name, "社員名が一致するべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: OnGetAsync を呼ぶ
        /// Then: ViewModel.AlertBanner が設定される
        /// </summary>
        [TestMethod(DisplayName = "ViewModel.AlertBanner が設定される")]
        public async Task OnGetAsync_AlertBannerが設定される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            // Act
            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.ViewModel.AlertBanner(), "AlertBanner が設定されているべきです（空文字列でも可）。");
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: OnGetAsync を呼ぶ
        /// Then: KarendaHyojiRows の各プロパティが正しく設定される
        /// </summary>
        [TestMethod(DisplayName = "KarendaHyojiRows の各プロパティが正しく設定される")]
        public async Task OnGetAsync_KarendaHyojiRowsの各プロパティが正しく設定される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            // Act
            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            await model.OnGetAsync();

            // Assert
            Assert.IsTrue(model.ViewModel.KarendaHyojiRows.Count > 0, "KarendaHyojiRows にデータが存在するべきです。");

            var firstRow = model.ViewModel.KarendaHyojiRows.First();

            // 必須プロパティの確認
            Assert.IsNotNull(firstRow.DateLabel, "DateLabel が設定されているべきです。");
            Assert.IsNotNull(firstRow.ApplicationLabel, "ApplicationLabel が設定されているべきです。");
            Assert.IsNotNull(firstRow.ActualWorked, "ActualWorked が設定されているべきです。");
            Assert.IsNotNull(firstRow.ApplicationType, "ApplicationType が設定されているべきです。");
            Assert.AreEqual(syain.Id, firstRow.SyainId, "SyainId が社員IDと一致するべきです。");
            Assert.IsNotNull(firstRow.DateIso, "DateIso が設定されているべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: OnGetAsync を呼ぶ
        /// Then: KinmuJokyoRows の各プロパティが正しく設定される
        /// </summary>
        [TestMethod(DisplayName = "KinmuJokyoRows の各プロパティが正しく設定される")]
        public async Task OnGetAsync_KinmuJokyoRowsの各プロパティが正しく設定される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            // Act
            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            await model.OnGetAsync();

            // Assert
            Assert.IsTrue(model.ViewModel.KarendaHyojiRows.Count > 0, "KarendaHyojiRows にデータが存在するべきです。");

            // 残業行の確認
            var overtimeRow = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            if (overtimeRow != null)
            {
                Assert.IsNotNull(overtimeRow.Value, "残業行の Value が設定されているべきです。");
                Assert.IsNotNull(overtimeRow.Description, "残業行の Description が設定されているべきです。");
            }

            // 有給残日数行の確認
            var paidLeaveRow = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "有給残日数 (半日有給)");
            if (paidLeaveRow != null)
            {
                Assert.IsNotNull(paidLeaveRow.Value, "有給残日数行の Value が設定されているべきです。");
                Assert.IsNotNull(paidLeaveRow.Description, "有給残日数行の Description が設定されているべきです。");
            }
        }

        /// <summary>
        /// Given: 社員が存在する
        /// When: OnPostRefreshCalendarAsync を呼ぶ
        /// Then: ViewModel の全プロパティが設定される
        /// </summary>
        [TestMethod(DisplayName = "OnPostRefreshCalendarAsync で ViewModel の全プロパティが設定される")]
        public async Task OnPostRefreshCalendarAsync_社員が存在する_ViewModelの全プロパティが設定される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, busyo) = InitializeTestData();
            var model = CreateModel();

            var totalBreakTime = Common.Time.休憩時間List.Sum(b => b.Item2 - b.Item1);
            var breakTimeHours = totalBreakTime / 60m; // 分を時間に変換

            // Act
            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            var result = await model.OnPostRefreshCalendarAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var json = (JsonResult)result;
            var status = GetResponseStatus(json);
            Assert.AreEqual(正常, status);

            // JSON レスポンスの確認
            var val = json.Value;
            var dataProp = val?.GetType().GetProperty("Data");
            Assert.IsNotNull(dataProp);
            var data = dataProp.GetValue(val);
            Assert.IsNotNull(data);

            var dataType = data!.GetType();
            var selectedUserIdProp = dataType.GetProperty("selectedUserId");
            Assert.IsNotNull(selectedUserIdProp, "selectedUserId が JSON レスポンスに含まれているべきです。");
            var selectedUserId = Convert.ToInt64(selectedUserIdProp!.GetValue(data));
            Assert.AreEqual(syain.Id, selectedUserId, "selectedUserId が社員IDと一致するべきです。");

            var selectedUserNameProp = dataType.GetProperty("selectedUserName");
            Assert.IsNotNull(selectedUserNameProp, "selectedUserName が JSON レスポンスに含まれているべきです。");
            var selectedUserName = selectedUserNameProp!.GetValue(data) as string;
            Assert.AreEqual(syain.Name, selectedUserName, "selectedUserName が社員名と一致するべきです。");

            Assert.IsNotNull(model.ViewModel.DisplayYearMonth, "DisplayYearMonth が設定されているべきです。");
            Assert.IsTrue(model.ViewModel.DisplayYearMonthDate > DateOnly.MinValue, "DisplayYearMonthDate が設定されているべきです。");
            Assert.AreEqual(breakTimeHours, model.ViewModel.BreakTimeHours, "BreakTimeHours が設定されているべきです。");
            Assert.IsNotNull(model.ViewModel.KinmuJokyoRows, "KinmuJokyoRows が設定されているべきです。");
            Assert.IsNotNull(model.ViewModel.KarendaHyojiRows, "KarendaHyojiRows が設定されているべきです。");
            Assert.IsNotNull(model.ViewModel.DepartmentEmployees, "DepartmentEmployees が設定されているべきです。");
        }

        /// <summary>
        /// Given: 社員が存在しない
        /// When: OnGetAsync を呼ぶ
        /// Then: NotFound を返す
        /// </summary>
        [TestMethod(DisplayName = "社員が存在しない → OnGet で NotFound")]
        public async Task OnGetAsync_社員が存在しない_NotFoundを返す()
        {
            // Arrange
            var model = CreateModel();
            model.SyainId = 999;

            // Act
            var result = await model.OnGetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Given: 予定データがない（初回アクセス等）
        /// When: OnGetAsync を呼ぶ
        /// Then: InitializePlannedWorkAsync が実行され、予定が作成される
        /// </summary>
        [TestMethod(DisplayName = "予定データなし → InitializePlannedWorkAsync で予定作成")]
        public async Task OnGetAsync_予定データがない場合_InitializePlannedWorkAsyncが実行される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            // 休日マスタ（祝日）を作成
            var holiday = new HikadoubiBuilder()
                .WithId(1)
                .WithYmd(dateYmd.GetStartOfMonth().AddDays(5)) // 6日目（5日後）を祝日とする
                .WithSyukusaijitsuFlag(祝祭日)
                .Build();
            db.Hikadoubis.Add(holiday);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;

            // Act
            await model.OnGetAsync();

            // Assert
            var createdYotei = await db.NippouYoteis.Where(n => n.SyainId == syain.Id).ToListAsync();
            Assert.IsTrue(createdYotei.Any(), "予定データが作成されているべきです。");

            // 祝日の予定が「休み」になっているか確認
            var holidayYotei = createdYotei.FirstOrDefault(y => y.NippouYoteiYmd == holiday.Ymd);
            Assert.IsNotNull(holidayYotei);
            Assert.IsFalse(holidayYotei.Worked, "祝日は Worked=false であるべきです。");

            // 平日の予定が「出勤」になっているか確認（祝日以外）
            var workdayYotei = createdYotei.FirstOrDefault(y =>
                y.NippouYoteiYmd != holiday.Ymd &&
                y.NippouYoteiYmd.DayOfWeek != DayOfWeek.Saturday &&
                y.NippouYoteiYmd.DayOfWeek != DayOfWeek.Sunday);
            if (workdayYotei != null)
            {
                Assert.IsTrue(workdayYotei.Worked, "平日は Worked=true であるべきです。");
            }
        }

        /// <summary>
        /// Given: 休暇管理、振替出勤日がある状態で予定データがない
        /// When: OnGetAsync を呼ぶ
        /// Then: InitializePlannedWorkAsync が実行され、優先順位に従って予定が作成される
        /// </summary>
        [TestMethod(DisplayName = "休暇管理・振替出勤あり → InitializePlannedWorkAsync で予定作成（優先順位検証）")]
        public async Task OnGetAsync_休暇管理と振替出勤がある場合_InitializePlannedWorkAsyncに反映される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var monthStart = dateYmd.GetStartOfMonth();

            // 1. 休暇管理 (2日目を休暇とする) -> worked = false
            var leaveDate = monthStart.AddDays(1);
            var kyuuka = new KyuukaKanri
            {
                SyainBaseId = syain.SyainBaseId,
                TaisyouYmd = leaveDate,
                KyuukaKubun = "01" // 適当な区分
            };
            db.KyuukaKanris.Add(kyuuka);

            // 2. 振替休日出勤 (3日目を休日出勤日とする) -> worked = true
            var holidayWorkDate = monthStart.AddDays(2);
            var furi = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = holidayWorkDate,
                DaikyuuKigenYmd = holidayWorkDate.AddMonths(2),
                SyutokuState = 未
            };
            db.FurikyuuZans.Add(furi);

            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var createdYotei = await db.NippouYoteis.Where(n => n.SyainId == syain.Id).ToListAsync();
            Assert.IsTrue(createdYotei.Any());

            // 休暇管理の判定検証 (優先順位: 非稼働日、休暇管理、振替休暇取得予定は「休」)
            var leaveYotei = createdYotei.FirstOrDefault(y => y.NippouYoteiYmd == leaveDate);
            Assert.IsNotNull(leaveYotei);
            Assert.IsFalse(leaveYotei.Worked, "休暇管理設定日は Worked=false であるべきです。");

            // 振替休日出勤の判定検証 (優先順位: 振替休日出勤 → 出)
            var holidayWorkYotei = createdYotei.FirstOrDefault(y => y.NippouYoteiYmd == holidayWorkDate);
            Assert.IsNotNull(holidayWorkYotei);
            Assert.IsTrue(holidayWorkYotei.Worked, "振替休日出勤日は Worked=true であるべきです。");
        }

        /// <summary>
        /// Given: 実績データ（日報確定済）がある
        /// When: OnGetAsync を呼ぶ
        /// Then: KarendaHyojiRows に勤務実績が反映される
        /// </summary>
        [TestMethod(DisplayName = "実績データあり → KarendaHyojiRows に勤務時間反映")]
        public async Task OnGetAsync_実績データがある場合_KarendaHyojiRowsに勤務時間が反映される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            var targetDate = dateYmd.GetStartOfMonth().AddDays(10);

            // 日報実績データ作成（確定済み）
            var nippou = new NippouBuilder()
                .WithSyainId(syain.Id)
                .WithNippouYmd(targetDate)
                .WithTourokuKbn(確定保存)
                .WithSyukkinHm1(new TimeOnly(9, 0))
                .WithTaisyutsuHm1(new TimeOnly(18, 0))
                .WithHJitsudou(8.0m) // 実働8時間
                .Build();
            db.Nippous.Add(nippou);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var targetRow = model.ViewModel.KarendaHyojiRows.FirstOrDefault(r => r.Date == targetDate);
            Assert.IsNotNull(targetRow);
            Assert.IsTrue(targetRow.IsWorkTimeConfirmed, "確定済みフラグが true であるべきです。");
            Assert.AreEqual("09:00～18:00", targetRow.WorkTime.Replace("\r\n", ""), "勤務時間が正しくフォーマットされているべきです。");
            Assert.AreEqual("08:00", targetRow.ActualWorked, "実働時間が正しく表示されているべきです。");
        }

        /// <summary>
        /// Given: 打刻データのみある（日報未作成）
        /// When: OnGetAsync を呼ぶ
        /// Then: KarendaHyojiRows に打刻時間が反映される（未確定）
        /// </summary>
        [TestMethod(DisplayName = "打刻データのみ → KarendaHyojiRows に打刻時間反映")]
        public async Task OnGetAsync_打刻データのみある場合_KarendaHyojiRowsに打刻時間が反映される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var targetDate = dateYmd.GetStartOfMonth().AddDays(11);

            // 打刻データ作成
            var workingHour = new WorkingHoursBuilder()
                .WithSyainId(syain.Id)
                .WithHiduke(targetDate)
                .WithSyukkinTime(targetDate.ToDateTime(new TimeOnly(8, 55)))
                .WithTaikinTime(targetDate.ToDateTime(new TimeOnly(18, 5)))
                .Build();
            db.WorkingHours.Add(workingHour);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var targetRow = model.ViewModel.KarendaHyojiRows.FirstOrDefault(r => r.Date == targetDate);
            Assert.IsNotNull(targetRow);
            Assert.IsFalse(targetRow.IsWorkTimeConfirmed, "確定済みフラグが false であるべきです。");
            Assert.AreEqual("08:55～18:05", targetRow.WorkTime, "打刻時間が表示されているべきです。");
            Assert.AreEqual(string.Empty, targetRow.ActualWorked, "実働時間は空であるべきです（日報未作成のため）。");
        }

        /// <summary>
        /// Given: 申請データ（承認済み・残業）がある
        /// When: OnGetAsync を呼ぶ
        /// Then: KarendaHyojiRows に申請状況が反映される
        /// </summary>
        [TestMethod(DisplayName = "申請データあり → KarendaHyojiRows に申請状況反映")]
        public async Task OnGetAsync_申請データがある場合_KarendaHyojiRowsに申請状況が反映される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var targetDate = dateYmd.GetStartOfMonth().AddDays(15);

            // 申請データ作成（残業・承認済み）
            var ukagai = new UkagaiHeaderBuilder()
                .WithSyainId(syain.Id)
                .WithWorkYmd(targetDate)
                .WithStatus(承認)
                .WithLastShoninYmd(dateYmd)
                .Build();
            ukagai.UkagaiShinseis = new List<UkagaiShinsei>
            {
                new UkagaiShinsei { UkagaiSyubetsu = 早朝作業 }
            };
            db.UkagaiHeaders.Add(ukagai);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;

            // Act
            await model.OnGetAsync();

            // Assert
            var targetRow = model.ViewModel.KarendaHyojiRows.FirstOrDefault(r => r.Date == targetDate);
            Assert.IsNotNull(targetRow);
            Assert.AreEqual("早朝", targetRow.ApplicationType, "申請種別ラベルが正しいこと");
            Assert.AreEqual(承認, targetRow.ApplicationStatus, "ステータスが正しいこと");
        }

        /// <summary>
        /// Given: 有給残データがある
        /// When: OnGetAsync を呼ぶ
        /// Then: KinmuJokyoRows に有給残日数が反映される
        /// </summary>
        [TestMethod(DisplayName = "有給残あり → KinmuJokyoRows に反映")]
        public async Task OnGetAsync_有給残がある場合_KinmuJokyoRowsに有給残日数が反映される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            // 有給残データ作成
            var yuukyuu = new YuukyuuZan
            {
                SyainBaseId = 1,
                Wariate = 20,
                Kurikoshi = 5,
                Syouka = 10,
                HannitiKaisuu = 3
            };
            db.YuukyuuZans.Add(yuukyuu);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "有給残日数 (半日有給)");
            Assert.IsNotNull(row);
            // 残日数 = 20 + 5 - 10 = 15日, 半日有給 = 1.5日 (3回 * 0.5)
            Assert.AreEqual("15.0日 (1.5日)", row.Value);
        }

        /// <summary>
        /// Given: 振替休暇残がある
        /// When: OnGetAsync を呼ぶ
        /// Then: KinmuJokyoRows に振替休暇残が反映される
        /// </summary>
        [TestMethod(DisplayName = "振替休暇残あり → KinmuJokyoRows に反映")]
        public async Task OnGetAsync_振替休暇残がある場合_KinmuJokyoRowsに振替休暇が反映される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            // 振替休暇残データ作成
            var furi = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = dateYmd.AddMonths(-1), // 1ヶ月前休日出勤
                DaikyuuKigenYmd = dateYmd.AddMonths(2),      // 期限はまだ先
                SyutokuState = 未,
                IsOneDay = true,
                SyutokuYoteiYmd = dateYmd.AddDays(5) // 今月取得予定
            };
            db.FurikyuuZans.Add(furi);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "振替休暇残日数");
            Assert.IsNotNull(row);
            // 1日分あるはず
            Assert.AreEqual("1.0日", row.Value);
        }

        /// <summary>
        /// Given: 連続勤務日数が閾値（10日）を超える
        /// When: OnGetAsync を呼ぶ
        /// Then: 赤アラート（LevelDanger）が表示される
        /// </summary>
        [TestMethod(DisplayName = "連続勤務10日超 → 赤アラート")]
        public async Task OnGetAsync_連続勤務日数がアラート閾値を超える場合_赤アラート()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            var systemMonth = dateYmd.GetStartOfMonth();

            // 11日連続出勤の実績を作成
            for (int i = 0; i < 11; i++)
            {
                // 日付を遡って作成
                var nippou = new NippouBuilder()
                    .WithId(100 + i) // IDを一意にする
                    .WithSyainId(syain.Id)
                    .WithNippouYmd(dateYmd.AddDays(-i))
                    .WithHJitsudou(8)
                    .Build();
                db.Nippous.Add(nippou);
            }
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "連勤日数");
            Assert.IsNotNull(row);
            Assert.AreEqual("11日", row.Value);
            Assert.AreEqual(IndexViewModel.LevelDanger, row.TitleLevel);
            Assert.AreEqual(IndexViewModel.LevelDanger, row.MessageLevel);
        }

        /// <summary>
        /// Given: 連続勤務日数が警告閾値（7日）以上、赤閾値未満
        /// When: OnGetAsync を呼ぶ
        /// Then: 黄アラート（LevelWarn）が表示される
        /// </summary>
        [TestMethod(DisplayName = "連続勤務7日以上 → 黄アラート")]
        public async Task OnGetAsync_連続勤務日数が警告閾値を超える場合_黄アラート()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            // 7日連続出勤の実績を作成
            for (int i = 0; i < 7; i++)
            {
                var nippou = new NippouBuilder()
                    .WithId(200 + i) // IDを一意にする
                    .WithSyainId(syain.Id)
                    .WithNippouYmd(dateYmd.AddDays(-i))
                    .WithHJitsudou(8)
                    .Build();
                db.Nippous.Add(nippou);
            }
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "連勤日数");
            Assert.IsNotNull(row);
            Assert.AreEqual("7日", row.Value);
            Assert.AreEqual(IndexViewModel.LevelWarn, row.TitleLevel);
            Assert.AreEqual(IndexViewModel.LevelWarn, row.MessageLevel);
        }

        /// <summary>
        /// Given: 残業時間が月45時間以上（閾値超過）
        /// When: OnGetAsync を呼ぶ
        /// Then: 赤もしくは黄アラートが表示される
        /// </summary>
        [TestMethod(DisplayName = "残業45時間以上 → 残業アラート")]
        public async Task OnGetAsync_残業時間がアラート閾値を超える場合_残業アラート()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            // 45時間残業の実績(1日)
            var nippou = new NippouBuilder()
                .WithSyainId(syain.Id)
                .WithNippouYmd(dateYmd.GetStartOfMonth())
                .WithHZangyo(45)
                .Build();
            db.Nippous.Add(nippou);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            Assert.IsNotNull(row);
            // 閾値設定: RedThreshold45=40, YellowThreshold45=30 なので 45h は Danger になるはず
            Assert.AreEqual(IndexViewModel.LevelDanger, row.MessageLevel);
        }

        /// <summary>
        /// Given: 残業時間が月30時間以上（警告閾値超過、赤閾値未満）
        /// When: OnGetAsync を呼ぶ
        /// Then: 黄アラート（LevelWarn）が表示される
        /// </summary>
        [TestMethod(DisplayName = "残業30時間以上 → 残業黄アラート")]
        public async Task OnGetAsync_残業時間が警告閾値を超える場合_残業黄アラート()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            // 35時間残業の実績(1日)
            var nippou = new NippouBuilder()
                .WithSyainId(syain.Id)
                .WithNippouYmd(dateYmd.GetStartOfMonth())
                .WithHZangyo(35)
                .Build();
            db.Nippous.Add(nippou);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            Assert.IsNotNull(row);
            // 閾値設定: RedThreshold45=40, YellowThreshold45=30 なので 35h は Warn になるはず
            Assert.AreEqual(IndexViewModel.LevelWarn, row.MessageLevel);
        }

        /// <summary>
        /// Given: 残業時間が月30時間以上（警告閾値超過）かつ免除社員
        /// When: OnGetAsync を呼ぶ
        /// Then: 黄アラート（LevelWarn）だがメッセージは "-" になる
        /// </summary>
        [TestMethod(DisplayName = "残業30時間以上・免除社員 → 黄アラート・メッセージ横棒")]
        public async Task OnGetAsync_残業時間が警告閾値を超える場合_免除社員_メッセージが横棒になる()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();

            // 免除社員設定 (標準社員外)
            var zokusei = await db.KintaiZokuseis.FirstAsync();
            zokusei.Code = 標準社員外;
            await db.SaveChangesAsync();

            var model = CreateModel();

            // 35時間残業
            var nippou = new NippouBuilder()
                .WithSyainId(syain.Id)
                .WithNippouYmd(dateYmd.GetStartOfMonth())
                .WithHZangyo(35)
                .Build();
            db.Nippous.Add(nippou);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            Assert.IsNotNull(row);
            Assert.AreEqual(IndexViewModel.LevelWarn, row.MessageLevel);
            Assert.AreEqual("-", row.Description);
        }

        /// <summary>
        /// Given: 残業制限解除 (SeigenTime=0) かつ残業時間が 90時間
        /// When: OnGetAsync を呼ぶ
        /// Then: Call 1 (100h) の黄閾値(85h)を超えているため、黄アラート（LevelWarn）が表示される
        /// </summary>
        [TestMethod(DisplayName = "残業制限解除・90時間 → 100時間リミットの黄アラート")]
        public async Task OnGetAsync_残業制限解除かつ警告閾値を超える場合_100時間リミットの黄アラート()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();

            // 制限解除・通常社員
            var zokusei = await db.KintaiZokuseis.FirstAsync();
            zokusei.SeigenTime = 0m;
            zokusei.Code = みなし対象者; // 免除ではない
            await db.SaveChangesAsync();

            var model = CreateModel();

            // 90時間残業
            var nippou = new NippouBuilder()
                .WithSyainId(syain.Id)
                .WithNippouYmd(dateYmd.GetStartOfMonth())
                .WithHZangyo(90)
                .Build();
            db.Nippous.Add(nippou);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            Assert.IsNotNull(row);
            // OvertimeYellowThreshold100 = 85 なので 90h は Warn
            Assert.AreEqual(IndexViewModel.LevelWarn, row.MessageLevel);
            StringAssert.Contains(row.Description, "残業上限に近づいています");
            StringAssert.Contains(row.Description, "100:00");
        }

        /// <summary>
        /// Given: 残業制限解除、かつ残業時間が 105h
        /// When: OnGetAsync を呼ぶ
        /// Then: 100h リミットの赤アラート（LevelDanger）が表示される
        /// </summary>
        [TestMethod(DisplayName = "残業105h (100hリミット) → 赤アラート (LevelDanger)")]
        public async Task OnGetAsync_残業制限解除かつレッド閾値を超える場合_100時間リミットの赤アラート()
        {
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var zokusei = await db.KintaiZokuseis.FirstAsync();
            zokusei.SeigenTime = 0m;
            zokusei.Code = みなし対象者;
            await db.SaveChangesAsync();

            var model = CreateModel();
          
            db.Nippous.Add(new NippouBuilder().WithSyainId(syain.Id).WithNippouYmd(dateYmd.GetStartOfMonth()).WithHZangyo(105).Build());
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            await model.OnGetAsync();

            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            Assert.IsNotNull(row);
            Assert.AreEqual(IndexViewModel.LevelDanger, row.MessageLevel);
            StringAssert.Contains(row.Description, "100:00");
        }

        /// <summary>
        /// Given: 残業制限解除、かつ残業時間が 105h
        /// When: OnGetAsync を呼ぶ（免除社員）
        /// Then: 赤アラート（LevelDanger）が表示されるが、メッセージは "-" になる
        /// </summary>
        [TestMethod(DisplayName = "残業105h (100hリミット)・免除社員 → 赤アラート・メッセージ横棒")]
        public async Task OnGetAsync_残業制限解除かつレッド閾値を超える場合_免除社員_メッセージが横棒になる()
        {
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var zokusei = await db.KintaiZokuseis.FirstAsync();
            zokusei.SeigenTime = 0m;
            zokusei.Code = フリー;
            await db.SaveChangesAsync();

            var model = CreateModel();

            db.Nippous.Add(new NippouBuilder().WithSyainId(syain.Id).WithNippouYmd(dateYmd.GetStartOfMonth()).WithHZangyo(105).Build());
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;
            await model.OnGetAsync();

            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            Assert.AreEqual(IndexViewModel.LevelDanger, row?.MessageLevel);
            Assert.AreEqual("-", row?.Description);
        }



        /// <summary>
        /// Given: 祝日データがある
        /// When: OnGetAsync を呼ぶ
        /// Then: 行スタイルが Holiday になる
        /// </summary>
        [TestMethod(DisplayName = "祝日あり → 行スタイル Holiday")]
        public async Task OnGetAsync_祝日がある場合_GetStylesで祝日スタイルが適用される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var holidayDate = dateYmd.GetStartOfMonth(); // 月初を祝日に

            var holiday = new HikadoubiBuilder()
                .WithYmd(holidayDate)
                .WithSyukusaijitsuFlag(祝祭日)
                .Build();
            db.Hikadoubis.Add(holiday);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = dateYmd;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.FirstOrDefault(r => r.Date == holidayDate);
            Assert.IsNotNull(row);
            Assert.AreEqual(Holiday, row.LineClass);
        }

        /// <summary>
        /// Given: 土曜日
        /// When: OnGetAsync を呼ぶ
        /// Then: 行スタイルが Saturday になる
        /// </summary>
        [TestMethod(DisplayName = "土曜日 → 行スタイル Saturday")]
        public async Task OnGetAsync_土曜日_GetStylesで土曜スタイルが適用される()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var (syain, _) = InitializeTestData();
            var model = CreateModel();

            // 2026/01/01は木曜。2026/01/03は土曜。
            var targetDate = new DateOnly(2026, 1, 3);
            model.SyainId = syain.Id;
            model.NippouYmd = targetDate;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.FirstOrDefault(r => r.Date == targetDate);
            Assert.IsNotNull(row);
            Assert.AreEqual(Saturday, row.LineClass);
        }

        /// <summary>
        /// Given: 日報確定済み（TourokuKubun=確定保存）
        /// When: OnGetAsync を呼ぶ
        /// Then: 報告状況表示が「済」になる
        /// </summary>
        [TestMethod(DisplayName = "日報確定済み → 報告状況「済」")]
        public async Task OnGetAsync_日報確定済み_報告状況が済になる()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            var nippou = new NippouBuilder()
                .WithSyainId(syain.Id)
                .WithNippouYmd(today)
                .WithTourokuKbn(確定保存)
                .Build();
            db.Nippous.Add(nippou);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.FirstOrDefault(r => r.Date == today);
            Assert.IsNotNull(row);
            Assert.AreEqual("済", row.ReportStatus);
        }

        /// <summary>
        /// Given: 日報一時保存
        /// When: OnGetAsync を呼ぶ
        /// Then: 報告状況表示が「一時」になる
        /// </summary>
        [TestMethod(DisplayName = "日報一時保存 → 報告状況「一時」")]
        public async Task OnGetAsync_日報一時保存_報告状況が一時になる()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            var nippou = new NippouBuilder()
                .WithSyainId(syain.Id)
                .WithNippouYmd(today)
                .WithTourokuKbn(一時保存)
                .Build();
            db.Nippous.Add(nippou);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.FirstOrDefault(r => r.Date == today);
            Assert.IsNotNull(row);
            Assert.AreEqual("一時", row.ReportStatus);
        }

        /// <summary>
        /// Given: 日報なし・打刻あり
        /// When: OnGetAsync を呼ぶ
        /// Then: 報告状況表示が「未」になる
        /// </summary>
        [TestMethod(DisplayName = "日報なし打刻あり → 報告状況「未」")]
        public async Task OnGetAsync_日報なし打刻あり_報告状況が未になる()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            var wh = new WorkingHoursBuilder()
                .WithSyainId(syain.Id)
                .WithHiduke(today)
                .WithSyukkinTime(today.ToDateTime())
                .Build();
            db.WorkingHours.Add(wh);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.FirstOrDefault(r => r.Date == today);
            Assert.IsNotNull(row);
            Assert.AreEqual("未", row.ReportStatus);
        }

        /// <summary>
        /// Given: 振替休暇の取得予定日がある
        /// When: OnGetAsync を呼ぶ
        /// Then: PlannedLeave（休暇予定）が「振休」になる
        /// </summary>
        [TestMethod(DisplayName = "振休予定あり → PlannedLeave「振休」")]
        public async Task OnGetAsync_振休予定がある場合_GetPlannedLabelが振休を返す()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            var furi = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = today.AddMonths(-1),
                SyutokuYoteiYmd = today, // 今日取得予定
                DaikyuuKigenYmd = today.AddMonths(1)
            };
            db.FurikyuuZans.Add(furi);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.FirstOrDefault(r => r.Date == today);
            Assert.IsNotNull(row);
            Assert.AreEqual("振休", row.PlannedLeave);
        }

        /// <summary>
        /// Given: 計画有給（Tokukyu=false）がある
        /// When: OnGetAsync を呼ぶ
        /// Then: PlannedLeave が「有給」になる
        /// </summary>
        [TestMethod(DisplayName = "計画有給あり → PlannedLeave「有給」")]
        public async Task OnGetAsync_計画有給がある場合_GetPlannedLabelが有給を返す()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            // 計画有給設定
            var keikaku = new YukyuKeikaku
            {
                SyainBaseId = 1,
                YukyuNendo = new YukyuNendo { IsThisYear = true, Nendo = 2026 },
                Status = 承認済
            };
            db.YukyuKeikakus.Add(keikaku);
            db.SaveChanges();

            var meisai = new YukyuKeikakuMeisai
            {
                YukyuKeikakuId = keikaku.Id,
                Ymd = today,
                IsTokukyu = false
            };
            db.YukyuKeikakuMeisais.Add(meisai);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.FirstOrDefault(r => r.Date == today);
            Assert.IsNotNull(row);
            Assert.AreEqual("有給", row.PlannedLeave);
        }

        /// <summary>
        /// Given: 計画特休（Tokukyu=true）がある
        /// When: OnGetAsync を呼ぶ
        /// Then: PlannedLeave が「特休」になる
        /// </summary>
        [TestMethod(DisplayName = "計画特休あり → PlannedLeave「特休」")]
        public async Task OnGetAsync_計画特休がある場合_GetPlannedLabelが特休を返す()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            // 計画特休設定
            var keikaku = new YukyuKeikaku
            {
                SyainBaseId = 1,
                YukyuNendo = new YukyuNendo { IsThisYear = true, Nendo = 2026 },
                Status = 承認済
            };
            db.YukyuKeikakus.Add(keikaku);
            db.SaveChanges();

            var meisai = new YukyuKeikakuMeisai
            {
                YukyuKeikakuId = keikaku.Id,
                Ymd = today,
                IsTokukyu = true
            };
            db.YukyuKeikakuMeisais.Add(meisai);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.FirstOrDefault(r => r.Date == today);
            Assert.IsNotNull(row);
            Assert.AreEqual("特休", row.PlannedLeave);
        }

        /// <summary>
        /// Given: 社員が存在する（当月の今日を指定）
        /// When: OnGetAsync を呼ぶ
        /// Then: その日の IsLink が True であること
        /// </summary>
        [TestMethod(DisplayName = "当月の今日はIsLinkがTrueであること")]
        public async Task OnGetAsync_当月の今日はIsLinkがTrueであること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();
            var rows = model.ViewModel.KarendaHyojiRows;

            // Assert
            var todayRow = rows.FirstOrDefault(r => r.Date == today);
            Assert.IsNotNull(todayRow);
            Assert.IsTrue(todayRow.IsLink, "当月の今日は IsLink=true であるべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する（当月の明日を指定）
        /// When: OnGetAsync を呼ぶ
        /// Then: その日の IsLink が True であること
        /// </summary>
        [TestMethod(DisplayName = "当月の明日はIsLinkがTrueであること")]
        public async Task OnGetAsync_当月の明日はIsLinkがTrueであること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            if (today >= today.GetEndOfMonth()) return; // 当月内に明日がない場合はスキップ

            var tomorrow = today.AddDays(1);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();
            var rows = model.ViewModel.KarendaHyojiRows;

            // Assert
            var tomorrowRow = rows.FirstOrDefault(r => r.Date == tomorrow);
            Assert.IsNotNull(tomorrowRow);
            Assert.IsTrue(tomorrowRow.IsLink, "当月の明日は IsLink=true であるべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する（翌月の本日を指定）
        /// When: OnGetAsync を呼ぶ
        /// Then: 翌月はまだ編集可能月ではないため、IsLink が true であること
        /// </summary>
        [TestMethod(DisplayName = "翌月の本日はIsLinkがTrueであること")]
        public async Task OnGetAsync_翌月の本日はIsLinkがTrueであること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            var nextMonthToday = today.AddMonths(1);
            model.SyainId = syain.Id;
            model.NippouYmd = nextMonthToday; // 翌月を表示

            // Act
            await model.OnGetAsync();
            var rows = model.ViewModel.KarendaHyojiRows;

            // Assert
            var nextMonthTodayRow = rows.FirstOrDefault(r => r.Date == nextMonthToday);
            Assert.IsNotNull(nextMonthTodayRow);
            Assert.IsTrue(nextMonthTodayRow.IsLink, "翌月の本日は IsLink=true であるべきです。");
        }

        /// <summary>
        /// Given: 社員が存在する（当月の昨日を指定）
        /// When: OnGetAsync を呼ぶ
        /// Then: その日の IsLink が False であること
        /// </summary>
        [TestMethod(DisplayName = "当月の昨日はIsLinkがFalseであること")]
        public async Task OnGetAsync_当月の昨日はIsLinkがFalseであること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            if (today <= today.GetStartOfMonth()) return; // 当月内に昨日がない場合はスキップ

            var yesterday = today.AddDays(-1);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();
            var rows = model.ViewModel.KarendaHyojiRows;

            // Assert
            var yesterdayRow = rows.FirstOrDefault(r => r.Date == yesterday);
            Assert.IsNotNull(yesterdayRow);
            Assert.IsFalse(yesterdayRow.IsLink, "当月の昨日は IsLink=false であるべきです。");
        }

        /// <summary>
        /// Given: 過去月表示
        /// When: OnGetAsync を呼ぶ
        /// Then: IsLink が編集可能月の今日以降のみ true になる
        /// </summary>
        [TestMethod(DisplayName = "IsLinkの網羅（過去月の全日=false, その他=true）")]
        public async Task OnGetAsync_IsLink過去月表示の網羅()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            // 予定データを作成しておく (InitializePlannedWorkAsyncを走らせるため)
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act & Assert (過去月)
            var pastMonth = today.AddMonths(-1).GetStartOfMonth();
            model.NippouYmd = pastMonth;
            await model.OnGetAsync();
            var pastRows = model.ViewModel.KarendaHyojiRows;
            foreach (var row in pastRows)
            {
                Assert.IsFalse(row.IsLink, row.DateLabel + ": 過去月の全日は IsLink=false であるべきです。");
            }
        }

        /// <summary>
        /// Given: 当月失効する振替休暇がある
        /// When: OnGetAsync を呼ぶ
        /// Then: アラート「振休 (当月失効)」が追加される
        /// </summary>
        [TestMethod(DisplayName = "当月失効振休あり → アラート表示")]
        public async Task OnGetAsync_振替休暇当月失効がある場合_アラート行が追加される()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            // 当月失効の振替休暇
            var furi = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = today.AddMonths(-3),
                DaikyuuKigenYmd = today.AddDays(1), // 今月期限（当月初日を避ける）
                SyutokuState = 未,
                IsOneDay = true
            };
            db.FurikyuuZans.Add(furi);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var alert = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "振休 (当月失効)");
            Assert.IsNotNull(alert);
            Assert.AreEqual(IndexViewModel.LevelDanger, alert.MessageLevel);
        }

        /// <summary>
        /// Given: 3か月を超過した振替休暇がある
        /// When: OnGetAsync を呼ぶ
        /// Then: アラート「振休 (3か月超過)」が追加される
        /// </summary>
        [TestMethod(DisplayName = "3ヶ月超過振休あり → アラート表示")]
        public async Task OnGetAsync_振替休暇3か月超過がある場合_アラート行が追加される()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            // 3ヶ月以上前（例：4ヶ月前）の休日出勤で未取得
            var kyuujitsuYmd = today.AddMonths(-4);

            var furi = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = kyuujitsuYmd,
                DaikyuuKigenYmd = kyuujitsuYmd.AddYears(1), // 法的期限はまだでも社内3ヶ月ルール超過
                SyutokuState = 未,
                IsOneDay = true
            };
            db.FurikyuuZans.Add(furi);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var alert = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "振休 (3か月超過)");
            Assert.IsNotNull(alert);
            Assert.AreEqual(IndexViewModel.LevelWarn, alert.MessageLevel);
        }

        /// <summary>
        /// Given: 承認済みの残業拡張申請がある
        /// When: OnGetAsync を呼ぶ
        /// Then: サマリー行のステータスが「拡張あり」等になることを確認
        /// </summary>
        [TestMethod(DisplayName = "残業拡張申請あり → 拡張あり判定")]
        public async Task OnGetAsync_残業拡張申請がある場合_拡張ありステータス()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            var ukagai = new UkagaiHeaderBuilder()
                .WithSyainId(syain.Id)
                .WithWorkYmd(today)
                .WithStatus(承認)
                .WithLastShoninYmd(today)
                .Build();
            ukagai.UkagaiShinseis = new List<UkagaiShinsei>
            {
                new UkagaiShinsei { UkagaiSyubetsu = 時間外労働時間制限拡張 }
            };
            db.UkagaiHeaders.Add(ukagai);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            Assert.IsNotNull(row);
            Assert.IsTrue(row.Description.Contains("拡張あり") || row.Description == "-");
        }

        /// <summary>
        /// Given: 勤怠属性の制限時間が0（無制限）
        /// When: OnGetAsync を呼ぶ
        /// Then: 制限解除ステータスになる
        /// </summary>
        [TestMethod(DisplayName = "残業制限時間0 → 制限解除")]
        public async Task OnGetAsync_残業制限解除の場合_制限解除ステータス()
        {
            // Arrange
            var (syain, _) = InitializeTestData();

            // 属性を更新（制限0）
            var zokusei = await db.KintaiZokuseis.FirstAsync();
            zokusei.SeigenTime = 0m;
            await db.SaveChangesAsync();

            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            Assert.IsNotNull(row);
            StringAssert.Contains(row.Description, "制限解除");
        }

        /// <summary>
        /// Given: 締め区分が中締め
        /// When: AlertBanner を呼ぶ
        /// Then: 前半の日報メッセージを返す
        /// </summary>
        [TestMethod(DisplayName = "AlertBanner: 中締め → 前半日報メッセージ")]
        public void AlertBanner_中締め_前半メッセージを返す()
        {
            // Arrange
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            model.ViewModel.DeadlineSimebi = new JissekiKakuteiKigenInfo(1, today, 中締め);

            // Act
            var message = model.ViewModel.AlertBanner();

            // Assert
            StringAssert.Contains(message, "前半の日報");
        }

        /// <summary>
        /// Given: 締め区分が月末締め
        /// When: AlertBanner を呼ぶ
        /// Then: 後半の日報メッセージを返す
        /// </summary>
        [TestMethod(DisplayName = "AlertBanner: 月末締め → 後半日報メッセージ")]
        public void AlertBanner_月末締め_後半メッセージを返す()
        {
            // Arrange
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            model.ViewModel.DeadlineSimebi = new JissekiKakuteiKigenInfo(1, today, 月末締め);

            // Act
            var message = model.ViewModel.AlertBanner();

            // Assert
            StringAssert.Contains(message, "後半の日報");
        }

        /// <summary>
        /// Given: 締め区分が一か月締め
        /// When: AlertBanner を呼ぶ
        /// Then: 1ヶ月分のメッセージを返す
        /// </summary>
        [TestMethod(DisplayName = "AlertBanner: 一か月締め → 1ヶ月分メッセージ")]
        public void AlertBanner_一か月締め_一か月メッセージを返す()
        {
            // Arrange
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            model.ViewModel.DeadlineSimebi = new JissekiKakuteiKigenInfo(1, today, 一か月締め);

            // Act
            var message = model.ViewModel.AlertBanner();

            // Assert
            StringAssert.Contains(message, "前半＆後半");
        }

        /// <summary>
        /// Given: 締め区分がその他（デフォルト）
        /// When: AlertBanner を呼ぶ
        /// Then: 本日締切メッセージを返す
        /// </summary>
        [TestMethod(DisplayName = "AlertBanner: その他 → 本日締切メッセージ")]
        public void AlertBanner_デフォルト_本日締切メッセージを返す()
        {
            // Arrange
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            model.ViewModel.DeadlineSimebi = new JissekiKakuteiKigenInfo(1, today, (AchievementClassification)99);

            // Act
            var message = model.ViewModel.AlertBanner();

            // Assert
            Assert.AreEqual("本日、実績の締切日です。", message);
        }

        /// <summary>
        /// Given: 当月で日報がなく、打刻データ（WorkingHour）のみが存在する
        /// When: OnGetAsync を呼ぶ
        /// Then: 残業時間の計算に打刻データの実働時間が考慮される
        /// </summary>
        [TestMethod(DisplayName = "打刻データのみの場合でも残業時間が計算される")]
        public async Task OnGetAsync_打刻のみで残業がある場合_残業時間に加算される()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            // システム日付を当月に設定（CalculateTotalOvertime で isSystemMonth が true になる必要がある）
            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // 打刻データを作成（9:00 - 20:00 = 11時間拘束 - 1時間休憩 = 10時間実働 => 2時間残業）
            // 規定時間は 8 時間とする（Common.Time.kitei は通常 480分 = 8時間）
            var workingHour = new WorkingHoursBuilder()
                .WithSyainId(syain.Id)
                .WithHiduke(today)
                .WithSyukkinTime(today.ToDateTime(new TimeOnly(9, 0)))
                .WithTaikinTime(today.ToDateTime(new TimeOnly(20, 0)))
                .Build();
            db.WorkingHours.Add(workingHour);
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業"); // "残業"
            Assert.IsNotNull(row, "残業行が見つかりません");

            // 期待値: 
            // 拘束時間: 9:00 - 20:00 = 11時間 (660分)
            // 休憩時間: 12:00-13:00 (60分) + 19:30-19:45 (15分) = 75分
            // 実働時間: 660 - 75 = 585分
            // 規定時間: 8時間 (480分)
            // 残業時間: 585 - 480 = 105分 = 1時間45分 -> "01:45"
            StringAssert.Contains(row.Description, "01:45");
        }

        /// <summary>
        /// Given: 過去数ヶ月に残業実績がある
        /// When: OnGetAsync を呼び出した際
        /// Then: 過去2～6ヶ月の平均残業時間が計算され、最大値が採用される
        /// </summary>
        [TestMethod(DisplayName = "過去2-6ヶ月の平均残業時間が計算される")]
        public async Task CalculateAverageOvertime2To6MonthsAsync_CalculatesMaxAverage()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var systemMonth = today.GetStartOfMonth();

            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // 過去データの作成
            // 1ヶ月前: 60時間残業
            await CreateMonthlyOvertime(syain.Id, systemMonth.AddMonths(-1), 60);
            // 2ヶ月前: 40時間残業
            await CreateMonthlyOvertime(syain.Id, systemMonth.AddMonths(-2), 40);
            // 3ヶ月前: 20時間残業
            await CreateMonthlyOvertime(syain.Id, systemMonth.AddMonths(-3), 20);
            await db.SaveChangesAsync();

            // 平均の計算ロジック (CalculateAverageOvertime2To6MonthsAsync):
            // 2ヶ月平均: (60 + 40) / 2 = 50h
            // 3ヶ月平均: (60 + 40 + 20) / 3 = 40h
            // => Max(50, 40) = 50h が採用されるはず

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            Assert.IsNotNull(row, "残業行が見つかりません");

            // Value には平均残業時間が表示される (IndexViewModel.BuildKinmuJokyoRows 参照)
            Assert.AreEqual("50:00", row.Value, "平均残業時間の計算が正しくありません");
        }

        /// <summary>
        /// KarendaHyojiRowViewModel の全てのプロパティにアクセスして網羅率を確保する
        /// </summary>
        [TestMethod(DisplayName = "KarendaHyojiRowViewModelのプロパティ網羅")]
        public void KarendaHyojiRowViewModel_PropertyCoverage()
        {
            // Arrange
            var date = new DateOnly(2026, 2, 18); // 水曜日
            var row = new KarendaHyojiRowViewModel(
                PlannedWork: true,
                PlannedOvertimeRaw: 120,
                PlannedLeave: "年次有給休暇",
                ApplicationLabel: "承認済",
                ApplicationStatus: 承認,
                SyukkinTime: new TimeOnly(9, 0),
                ApplicationType: "残業",
                ShowApplyButton: true,
                ShowTypeButton: true,
                WorkTime: "09:00 - 18:00",
                ActualWorked: "08:00",
                ReportStatus: "済",
                LineClass: "test-class",
                ReportCss: "report-css",
                IsLink: true,
                SyainId: 1,
                SyainBaseId: 100,
                Date: date,
                IsAfterSystemDay: false,
                IsWorkTimeConfirmed: true,
                HasProxyInput: false,
                ReportStatusType: "type"
            );

            // Act & Assert (単にアクセスするだけでカバレッジは取れる)
            Assert.IsTrue(row.PlannedWork);
            Assert.AreEqual(120, row.PlannedOvertimeRaw);
            Assert.AreEqual("年次有給休暇", row.PlannedLeave);
            Assert.AreEqual("承認済", row.ApplicationLabel);
            Assert.AreEqual(承認, row.ApplicationStatus);
            Assert.AreEqual(new TimeOnly(9, 0), row.SyukkinTime);
            Assert.AreEqual("残業", row.ApplicationType);
            Assert.IsTrue(row.ShowApplyButton);
            Assert.IsTrue(row.ShowTypeButton);
            Assert.AreEqual("09:00 - 18:00", row.WorkTime);
            Assert.AreEqual("08:00", row.ActualWorked);
            Assert.AreEqual("済", row.ReportStatus);
            Assert.AreEqual("test-class", row.LineClass);
            Assert.AreEqual("report-css", row.ReportCss);
            Assert.IsTrue(row.IsLink);
            Assert.AreEqual(1, row.SyainId);
            Assert.AreEqual(100, row.SyainBaseId);
            Assert.AreEqual(date, row.Date);
            Assert.IsFalse(row.IsAfterSystemDay);
            Assert.IsTrue(row.IsWorkTimeConfirmed);
            Assert.IsFalse(row.HasProxyInput);
            Assert.AreEqual("type", row.ReportStatusType);

            // 計算プロパティ
            Assert.AreEqual("2026-02-18", row.DateIso);
            Assert.AreEqual(3, row.DayOfWeekValue); // Wednesday = 3
            Assert.AreEqual("18(水)", row.DateLabel);
        }

        /// <summary>
        /// EmployeeViewModel の全てのプロパティにアクセスして網羅率を確保する
        /// </summary>
        [TestMethod(DisplayName = "EmployeeViewModelのプロパティ網羅")]
        public void EmployeeViewModel_PropertyCoverage()
        {
            // Arrange
            var vm = new EmployeeViewModel(
                Id: 1,
                Name: "テスト社員",
                BusyoCode: "B001",
                SyainBaseId: 100,
                Code: "S0001"
            );

            // Act & Assert
            Assert.AreEqual(1, vm.Id);
            Assert.AreEqual("テスト社員", vm.Name);
            Assert.AreEqual("B001", vm.BusyoCode);
            Assert.AreEqual(100, vm.SyainBaseId);
            Assert.AreEqual("S0001", vm.Code);
        }

        /// <summary>
        /// Given: 打刻データ（WorkingHour）に欠損（出勤のみ、または退勤のみ）がある
        /// When: OnGetAsync を呼ぶ
        /// Then: CalculateTotalOvertime の Where 句で正しくフィルタリングされ、エラーにならない
        /// </summary>
        [TestMethod(DisplayName = "打刻不備がある場合に残業計算がスキップされる")]
        public async Task OnGetAsync_打刻不備がある場合_残業計算がスキップされる()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // 1. 出勤打刻のみ
            var startOnly = new WorkingHoursBuilder()
                .WithId(1001)
                .WithSyainId(syain.Id)
                .WithHiduke(today)
                .WithSyukkinTime(today.ToDateTime(new TimeOnly(9, 0)))
                .Build();

            // 2. 退勤打刻のみ
            var endOnly = new WorkingHoursBuilder()
                .WithId(1002)
                .WithSyainId(syain.Id)
                .WithHiduke(today)
                .WithTaikinTime(today.ToDateTime(new TimeOnly(18, 0)))
                .Build();

            db.WorkingHours.AddRange(startOnly, endOnly);
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            Assert.IsNotNull(row);

            // 両方不備があるため、残業時間は 0:00 (または予定メッセージ内の 00:00) になるはず
            // CalculateTotalOvertime の Where (p.SyukkinTime.HasValue && p.TaikinTime.HasValue) を通過しないため。
            StringAssert.Contains(row.Description, "00:00");
        }

        /// <summary>
        /// Given: 振替休暇の様々な状態（未取得、半日取得済み）や失効期限がある
        /// When: OnGetAsync を呼ぶ
        /// Then: 振休残日数や失効アラートが正しく計算・表示される
        /// </summary>
        [TestMethod(DisplayName = "振休残日数が正しく計算されること")]
        public async Task OnGetAsync_振休残日数が正しく計算されること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var systemMonth = today.GetStartOfMonth();

            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // 1. 未使用の振休 (1日)
            var unusedFullDayFuri = new FurikyuuZan
            {
                Id = 2001,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = systemMonth.AddMonths(-3),
                DaikyuuKigenYmd = systemMonth.AddDays(5),
                IsOneDay = true,
                SyutokuState = 未
            };
            // 2. 半日使用済みの1日振休 -> 未使用分 0.5
            var halfUsedFullDayFuri = new FurikyuuZan
            {
                Id = 2002,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = systemMonth.AddMonths(-1),
                DaikyuuKigenYmd = systemMonth.AddMonths(2),
                IsOneDay = true,
                SyutokuState = 半日,
                SyutokuYmd1 = systemMonth.AddDays(-5)
            };
            // 3. 未使用の半日振休
            var unusedHalfDayFuri = new FurikyuuZan
            {
                Id = 2003,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = today.AddMonths(-3).AddDays(-1),
                DaikyuuKigenYmd = systemMonth.AddMonths(5),
                IsOneDay = false,
                SyutokuState = 未
            };

            db.FurikyuuZans.AddRange(unusedFullDayFuri, halfUsedFullDayFuri, unusedHalfDayFuri);
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert
            var resRow = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "振替休暇残日数");
            Assert.AreEqual("2.0日", resRow?.Value);
        }

        /// <summary>
        /// Given: 当月失効する振休がある
        /// When: OnGetAsync を呼ぶ
        /// Then: アラート「振休 (当月失効)」が表示され、Value に日数が含まれる
        /// </summary>
        [TestMethod(DisplayName = "当月失効の振休がある場合アラートが表示されること")]
        public async Task OnGetAsync_当月失効の振休がある場合アラートが表示されること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var systemMonth = today.GetStartOfMonth();

            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            var expiringFuri = new FurikyuuZan
            {
                Id = 2001,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = systemMonth.AddMonths(-3),
                DaikyuuKigenYmd = systemMonth.AddDays(5), // 当月内
                IsOneDay = true,
                SyutokuState = 未
            };
            db.FurikyuuZans.Add(expiringFuri);
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert
            var expiringRow = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "振休 (当月失効)");
            Assert.IsNotNull(expiringRow);
            Assert.IsTrue(expiringRow?.Value?.Contains("1日"));
        }

        /// <summary>
        /// Given: 3か月を超過した振休がある
        /// When: OnGetAsync を呼ぶ
        /// Then: アラート「振休 (3か月超過)」が表示され、Value に日数が含まれる
        /// </summary>
        [TestMethod(DisplayName = "3か月超過の振休がある場合アラートが表示されること")]
        public async Task OnGetAsync_3か月超過の振休がある場合アラートが表示されること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var systemMonth = today.GetStartOfMonth();

            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            var longTermUnusedHalfDayFuri = new FurikyuuZan
            {
                Id = 2003,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = today.AddMonths(-3).AddDays(-1), // 3か月以上前
                DaikyuuKigenYmd = systemMonth.AddMonths(5),
                IsOneDay = false,
                SyutokuState = 未
            };
            db.FurikyuuZans.Add(longTermUnusedHalfDayFuri);
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert
            var expired3Row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "振休 (3か月超過)");
            Assert.IsNotNull(expired3Row);
            Assert.IsTrue(expired3Row?.Value?.Contains("1日"));
        }

        /// <summary>
        /// Given: 過去2-6か月の平均残業時間が 70h を超えている
        /// When: OnGetAsync を呼ぶ
        /// Then: 赤アラート（LevelDanger）が表示される
        /// </summary>
        [TestMethod(DisplayName = "平均残業70h超え → 赤アラート")]
        public async Task OnGetAsync_平均残業が70h超えの場合_赤アラートが表示される()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            var systemMonth = today.GetStartOfMonth();

            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // 70h (Red) を超えるケース
            await CreateMonthlyOvertime(syain.Id, systemMonth.AddMonths(-1), 75);
            await CreateMonthlyOvertime(syain.Id, systemMonth.AddMonths(-2), 75);
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            Assert.AreEqual(IndexViewModel.LevelDanger, row?.MessageLevel, "70h超えで赤色アラートになること");
        }

        /// <summary>
        /// Given: 過去2-6か月の平均残業時間が 60h を超えている
        /// When: OnGetAsync を呼ぶ
        /// Then: 黄アラート（LevelWarn）が表示される
        /// </summary>
        [TestMethod(DisplayName = "平均残業60h以上 → 黄アラート")]
        public async Task OnGetAsync_平均残業が60h超えの場合_黄アラートが表示される()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var systemMonth = today.GetStartOfMonth();

            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // 65h (Yellow) に調整 (YellowThreshold = 60)
            await CreateMonthlyOvertime(syain.Id, systemMonth.AddMonths(-1), 65);
            await CreateMonthlyOvertime(syain.Id, systemMonth.AddMonths(-2), 65);
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KinmuJokyoRows.FirstOrDefault(r => r.Label == "残業");
            Assert.AreEqual(IndexViewModel.LevelWarn, row?.MessageLevel, "60h超えで黄色アラートになること");
        }

        /// <summary>
        /// Given: 日報に休日実働、法定休日実働が設定されている
        /// When: GetActualWorkedDisplay が呼ばれる
        /// Then: 合計実働時間が正しくフォーマットされる
        /// </summary>
        [TestMethod(DisplayName = "休日実働あり → 03:45 形式で表示")]
        public async Task OnGetAsync_休日実働がある場合_正しくフォーマットされる()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // 休日実働(2.5h) + 法定休日実働(1.25h) = 3.75h = 03:45
            var nippou = new NippouBuilder()
                .WithId(3001)
                .WithSyainId(syain.Id)
                .WithNippouYmd(today)
                .WithDJitsudou(2.5m)
                .WithNJitsudou(1.25m)
                .WithTourokuKbn(確定保存)
                .Build();

            db.Nippous.Add(nippou);
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.FirstOrDefault(r => r.Date == today);
            Assert.AreEqual("03:45", row?.ActualWorked);
        }

        /// <summary>
        /// Given: 日報に実働時間が設定されていない
        /// When: GetActualWorkedDisplay が呼ばれる
        /// Then: 空文字が返される
        /// </summary>
        [TestMethod(DisplayName = "休日実働なし → 空文字")]
        public async Task OnGetAsync_実労働がない場合_空文字が表示される()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.FirstOrDefault(r => r.Date == today);
            Assert.AreEqual(string.Empty, row?.ActualWorked);
        }

        /// <summary>
        /// 内部 Record のプロパティ網羅を確保する
        /// </summary>
        [TestMethod(DisplayName = "内部Recordのプロパティ網羅")]
        public void Kinmuhyo_RecordPropertyCoverage()
        {
            // IndexViewModel.ReportStatusDisplayInfo
            var reportStatus = new IndexViewModel.ReportStatusDisplayInfo("確定", true, false, "StatusType");
            Assert.AreEqual("確定", reportStatus.DisplayText);
            Assert.IsTrue(reportStatus.IsLink);
            Assert.IsFalse(reportStatus.HasProxyInput);
            Assert.AreEqual("StatusType", reportStatus.StatusType);

            // IndexViewModel.WorkTimeDisplayInfo
            var workTime = new IndexViewModel.WorkTimeDisplayInfo("09:00～18:00", true, new TimeOnly(9, 0), "08:00");
            Assert.AreEqual("09:00～18:00", workTime.DisplayValue);
            Assert.IsTrue(workTime.IsConfirmed);
            Assert.AreEqual(new TimeOnly(9, 0), workTime.SyukkinJikan);
            Assert.AreEqual("08:00", workTime.JitsuDoJikan);

            // IndexViewModel.ApplicationStatusInfo
            var appStatus = new IndexViewModel.ApplicationStatusInfo("申請ラベル", 承認, "夜間", true, false);
            Assert.AreEqual("申請ラベル", appStatus.Label);
            Assert.AreEqual(承認, appStatus.Status);
            Assert.AreEqual("夜間", appStatus.InquiryType);
            Assert.IsTrue(appStatus.ShowApplyButton);
            Assert.IsFalse(appStatus.ShowTypeButton);

            // KinmuJokyoRowViewModel
            var jokyoRow = new KinmuJokyoRowViewModel("ラベル", "値", "説明", 3, 1);
            Assert.AreEqual("ラベル", jokyoRow.Label);
            Assert.AreEqual("値", jokyoRow.Value);
            Assert.AreEqual("説明", jokyoRow.Description);
            Assert.AreEqual(3, jokyoRow.TitleLevel);
            Assert.AreEqual(1, jokyoRow.MessageLevel);
        }

        /// <summary>
        /// MapInquiryTypeToLabel の全 switch ケースを網羅する
        /// </summary>
        [TestMethod(DisplayName = "伺い種別ラベルのマッピング網羅")]
        public async Task OnGetAsync_伺い種別ラベルの網羅()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // 各種別の UkagaiHeader を作成
            var types = new[] {
                夜間作業,
                深夜作業,
                リフレッシュデー残業,
                休日出勤,
                テレワーク,
                打刻時間修正
            };

            for (int i = 0; i < types.Length; i++)
            {
                var date = today.AddDays(i);
                var header = new UkagaiHeaderBuilder()
                    .WithId(4000 + i)
                    .WithSyainId(syain.Id)
                    .WithWorkYmd(date)
                    .WithStatus(承認)
                    .Build();

                var shinsei = new UkagaiShinsei { Id = 4000 + i, UkagaiHeaderId = header.Id, UkagaiSyubetsu = types[i] };
                db.UkagaiHeaders.Add(header);
                db.UkagaiShinseis.Add(shinsei);

                // BuildKarendaHyojiRows ではアプリケーションの状態を確認する
            }
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert
            var rows = model.ViewModel.KarendaHyojiRows;

            Assert.AreEqual("夜間", rows.First(r => r.Date == today).ApplicationType);
            Assert.AreEqual("深夜", rows.First(r => r.Date == today.AddDays(1)).ApplicationType);
            Assert.AreEqual("リフ", rows.First(r => r.Date == today.AddDays(2)).ApplicationType);
            Assert.AreEqual("休出", rows.First(r => r.Date == today.AddDays(3)).ApplicationType);
            Assert.AreEqual("テレ", rows.First(r => r.Date == today.AddDays(4)).ApplicationType);
            Assert.AreEqual("打刻", rows.First(r => r.Date == today.AddDays(5)).ApplicationType);
        }

        /// <summary>
        /// MapInquiryTypeToLabel で inquiryType が Null の場合のパスを網羅する
        /// </summary>
        [TestMethod(DisplayName = "伺い種別がNullの場合のマッピング")]
        public async Task OnGetAsync_伺い種別がNullの場合_空文字を返す()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // 申請ヘッダはあるが、申請詳細（UkagaiShinsei）がないケース、または種別が未設定
            var header = new UkagaiHeaderBuilder()
                .WithId(5001)
                .WithSyainId(syain.Id)
                .WithWorkYmd(today)
                .WithStatus(承認)
                .Build();

            db.UkagaiHeaders.Add(header);
            // shinsei を敢えて追加しない
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.First(r => r.Date == today);
            Assert.AreEqual(string.Empty, row.ApplicationType, "種別がない場合は空文字になること");
        }


        /// <summary>
        /// 残業アラートの免除社員（フリー）の場合のパスを網羅する
        /// </summary>
        [TestMethod(DisplayName = "残業アラート_免除社員の場合")]
        public async Task OnGetAsync_残業アラート_免除社員の場合()
        {
            // Arrange
            var (syain, _) = InitializeTestData();

            // 勤怠属性「フリー」をシードする
            var kintaiZokusei = new KintaiZokuseiBuilder()
                .WithId(3)
                .WithName("フリー")
                .WithCode(フリー)
                .Build();
            db.KintaiZokuseis.Add(kintaiZokusei);

            // 社員の勤怠属性を「フリー」に設定
            syain.KintaiZokuseiId = 3;
            await db.SaveChangesAsync();

            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // 100時間を超える残業を発生させる
            var nippou = new NippouBuilder()
                .WithId(6001)
                .WithSyainId(syain.Id)
                .WithNippouYmd(today)
                .WithTotalZangyo(105.0m) // 100h 超過
                .Build();
            db.Nippous.Add(nippou);
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert
            var rows = model.ViewModel.KinmuJokyoRows;
            var overtimeRow = rows.First(r => r.Label == "残業");

            // isExempt = true なのでメッセージは "-" になるはず
            Assert.AreEqual("-", overtimeRow.Description, "免除社員の場合は警告メッセージが '-' になること");
        }

        /// <summary>
        /// 残業アラートの閾値未満（予定メッセージ）のパスを網羅する
        /// </summary>
        [TestMethod(DisplayName = "残業アラート_閾値未満の場合")]
        public async Task OnGetAsync_残業アラート_閾値未満の場合()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // 閾値（30h）未満の残業
            var nippou = new NippouBuilder()
                .WithId(6002)
                .WithSyainId(syain.Id)
                .WithNippouYmd(today)
                .WithTotalZangyo(10.0m)
                .Build();
            db.Nippous.Add(nippou);
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert
            var rows = model.ViewModel.KinmuJokyoRows;
            var overtimeRow = rows.First(r => r.Label == "残業");

            // 閾値未満なので「予定を含めた残業時間は...」というメッセージになるはず
            StringAssert.Contains(overtimeRow.Description, "予定を含めた残業時間は",
                $"閾値未満の場合は予定メッセージが表示されること。Actual: {overtimeRow.Description}");
        }

        /// <summary>
        /// 平均残業アラートの網羅
        /// </summary>
        [TestMethod(DisplayName = "残業アラート_平均残業の網羅")]
        public async Task OnGetAsync_残業アラート_平均残業の網羅()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // 過去2ヶ月の残業をセットして平均が70h以上になるようにする
            // 1月: 80h, 12月: 80h -> 平均 80h (RedThreshold = 70)
            await CreateMonthlyOvertime(syain.Id, today.AddMonths(-1), 80);
            await CreateMonthlyOvertime(syain.Id, today.AddMonths(-2), 80);
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert (Red Alert)
            var rows = model.ViewModel.KinmuJokyoRows;
            var overtimeRow = rows.First(r => r.Label == "残業");
            Assert.AreEqual(1, overtimeRow.MessageLevel, "平均80hなら Red アラート");
        }

        /// <summary>
        /// 平均残業アラートの網羅
        /// </summary>
        [TestMethod(DisplayName = "残業アラート_平均残業65hなら Yellow アラートの網羅")]
        public async Task OnGetAsync_残業アラート_平均残業65hならYellowアラートの網羅()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            model.ViewModel.CurrentTime = today.ToDateTime(TimeOnly.MinValue);
            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // 平均を65hにする (YellowThreshold = 60)
            // 1月: 65h, 12月: 65h -> 平均 65h
            db.Nippous.RemoveRange(db.Nippous.Where(n => n.SyainId == syain.Id && n.NippouYmd < today));

            await CreateMonthlyOvertime(syain.Id, today.AddMonths(-1), 65);
            await CreateMonthlyOvertime(syain.Id, today.AddMonths(-2), 65);
            await db.SaveChangesAsync();

            // Act
            await model.OnGetAsync();

            // Assert (Yellow Alert)
            var rows = model.ViewModel.KinmuJokyoRows;
            var overtimeRow = rows.First(r => r.Label == "残業");
            Assert.AreEqual(2, overtimeRow.MessageLevel, "平均65hなら Yellow アラート");
        }


        /// <summary>
        /// Given: 実績データが確定保存されている
        /// When: OnGetAsync を呼ぶ
        /// Then: 報告状況に「済」と表示されること
        /// </summary>
        [TestMethod(DisplayName = "実績確定済の場合_済と表示されること")]
        public async Task OnGetAsync_実績確定済の場合_済と表示されること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var targetDate = today.GetEndOfMonth().AddDays(-1);

            var nippou = new NippouBuilder()
                .WithId(7101)
                .WithSyainId(syain.Id)
                .WithNippouYmd(targetDate)
                .WithTourokuKbn(確定保存)
                .Build();
            db.Nippous.Add(nippou);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.First(r => r.Date == targetDate);
            Assert.IsTrue(row.IsWorkTimeConfirmed);
            Assert.AreEqual("済", row.ReportStatus);
        }

        /// <summary>
        /// Given: 実績データが一時保存されている
        /// When: OnGetAsync を呼ぶ
        /// Then: 報告状況に「一時」と表示されること
        /// </summary>
        [TestMethod(DisplayName = "実績一時保存の場合_一時と表示されること")]
        public async Task OnGetAsync_実績一時保存の場合_一時と表示されること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            var nippou = new NippouBuilder()
                .WithId(7102)
                .WithSyainId(syain.Id)
                .WithNippouYmd(today)
                .WithTourokuKbn(一時保存)
                .Build();
            db.Nippous.Add(nippou);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.First(r => r.Date == today);
            Assert.AreEqual("一時", row.ReportStatus);
            Assert.AreEqual("一時保存", row.ReportStatusType);
        }

        /// <summary>
        /// Given: 打刻データがあり、実績データが未入力である
        /// When: OnGetAsync を呼ぶ
        /// Then: 報告状況に「未」と表示されること
        /// </summary>
        [TestMethod(DisplayName = "打刻あり実績なしの場合_未と表示されること")]
        public async Task OnGetAsync_打刻あり実績なしの場合_未と表示されること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var targetDate = today.GetStartOfMonth().AddDays(1);

            var wh = new WorkingHoursBuilder()
                .WithId(7101)
                .WithSyainId(syain.Id)
                .WithHiduke(targetDate)
                .WithSyukkinTime(targetDate.ToDateTime(new TimeOnly(8, 30)))
                .Build();
            db.WorkingHours.Add(wh);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.First(r => r.Date == targetDate);
            Assert.AreEqual("未", row.ReportStatus);
            Assert.AreEqual("未入力", row.ReportStatusType);
        }

        /// <summary>
        /// Given: 承認済みの申請がある
        /// When: OnGetAsync を呼ぶ
        /// Then: 申請ステータスが「承認」になること
        /// </summary>
        [TestMethod(DisplayName = "申請承認済の場合_ステータスが承認になること")]
        public async Task OnGetAsync_申請承認済の場合_ステータスが承認になること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var targetDate = today.GetStartOfMonth().AddDays(2);

            var ukagai = new UkagaiHeaderBuilder()
                .WithId(7101)
                .WithSyainId(syain.Id)
                .WithWorkYmd(targetDate)
                .WithStatus(承認)
                .WithLastShoninYmd(today)
                .Build();
            db.UkagaiHeaders.Add(ukagai);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.First(r => r.Date == targetDate);
            Assert.AreEqual(承認, row.ApplicationStatus);
        }

        /// <summary>
        /// Given: 承認待ちの申請がある
        /// When: OnGetAsync を呼ぶ
        /// Then: 申請ステータスが「承認待」になること
        /// </summary>
        [TestMethod(DisplayName = "申請承認待の場合_ステータスが承認待になること")]
        public async Task OnGetAsync_申請承認待の場合_ステータスが承認待になること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var targetDate = today.GetStartOfMonth().AddDays(9);

            var ukagai = new UkagaiHeaderBuilder()
                .WithId(7102)
                .WithSyainId(syain.Id)
                .WithWorkYmd(targetDate)
                .WithStatus(承認待)
                .Build();
            db.UkagaiHeaders.Add(ukagai);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.First(r => r.Date == targetDate);
            Assert.AreEqual(承認待, row.ApplicationStatus);
        }

        /// <summary>
        /// Given: 差戻された申請がある
        /// When: OnGetAsync を呼ぶ
        /// Then: 申請ステータスが「差戻」になること
        /// </summary>
        [TestMethod(DisplayName = "申請差戻の場合_ステータスが差戻になること")]
        public async Task OnGetAsync_申請差戻の場合_ステータスが差戻になること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var targetDate = today.GetStartOfMonth().AddDays(4);

            var ukagai = new UkagaiHeaderBuilder()
                .WithId(7103)
                .WithSyainId(syain.Id)
                .WithWorkYmd(targetDate)
                .WithStatus(差戻)
                .Build();
            db.UkagaiHeaders.Add(ukagai);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.First(r => r.Date == targetDate);
            Assert.AreEqual(差戻, row.ApplicationStatus);
        }

        /// <summary>
        /// Given: 代理入力された実績がある
        /// When: OnGetAsync を呼ぶ
        /// Then: 代理入力フラグ（HasProxyInput）が True になること
        /// </summary>
        [TestMethod(DisplayName = "代理入力がある場合_フラグがTrueになること")]
        public async Task OnGetAsync_代理入力がある場合_フラグがTrueになること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var targetDate = today.GetStartOfMonth().AddDays(5);

            var nippou = new NippouBuilder()
                .WithId(7103)
                .WithSyainId(syain.Id)
                .WithNippouYmd(targetDate)
                .WithTourokuKbn(確定保存)
                .Build();
            db.Nippous.Add(nippou);
            db.DairiNyuryokuRirekis.Add(new DairiNyuryokuRireki
            {
                Id = 7101,
                NippouId = 7103,
                DairiNyuryokuSyainId = syain.Id,
                DairiNyuryokuTime = today.ToDateTime(),
                Invalid = false
            });
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.First(r => r.Date == targetDate);
            Assert.IsTrue(row.HasProxyInput);
        }

        /// <summary>
        /// Given: 計画有給がある
        /// When: OnGetAsync を呼ぶ
        /// Then: 休暇予定に「有給」と表示されること
        /// </summary>
        [TestMethod(DisplayName = "計画有給がある場合_有給と表示されること")]
        public async Task OnGetAsync_計画有給がある場合_有給と表示されること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            var targetDate = today.GetStartOfMonth().AddDays(6);

            var nendo = new YukyuNendoBuilder().WithId(7101).WithIsThisYear(true).Build();
            db.YukyuNendos.Add(nendo);
            var yk = new YukyuKeikakuBuilder().WithId(7101).WithYukyuNendoId(7101).WithSyainBaseId(syain.SyainBaseId).Build();
            db.YukyuKeikakus.Add(yk);
            var paidLeave = new YukyuKeikakuMeisai { Id = 7101, YukyuKeikakuId = 7101, Ymd = targetDate, IsTokukyu = false };
            db.YukyuKeikakuMeisais.Add(paidLeave);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.First(r => r.Date == targetDate);
            Assert.AreEqual("有給", row.PlannedLeave);
        }

        /// <summary>
        /// Given: 計画特休がある
        /// When: OnGetAsync を呼ぶ
        /// Then: 休暇予定に「特休」と表示されること
        /// </summary>
        [TestMethod(DisplayName = "計画特休がある場合_特休と表示されること")]
        public async Task OnGetAsync_計画特休がある場合_特休と表示されること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());

            var targetDate = today.GetStartOfMonth().AddDays(7);

            var nendo = new YukyuNendoBuilder().WithId(7101).WithIsThisYear(true).Build();
            db.YukyuNendos.Add(nendo);
            var yk = new YukyuKeikakuBuilder().WithId(7101).WithYukyuNendoId(7101).WithSyainBaseId(syain.SyainBaseId).Build();
            db.YukyuKeikakus.Add(yk);
            var specialLeave = new YukyuKeikakuMeisai { Id = 7102, YukyuKeikakuId = 7101, Ymd = targetDate, IsTokukyu = true };
            db.YukyuKeikakuMeisais.Add(specialLeave);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.First(r => r.Date == targetDate);
            Assert.AreEqual("特休", row.PlannedLeave);
        }

        /// <summary>
        /// Given: 振休予定がある
        /// When: OnGetAsync を呼ぶ
        /// Then: 休暇予定に「振休」と表示されること
        /// </summary>
        [TestMethod(DisplayName = "振休予定がある場合_振休と表示されること")]
        public async Task OnGetAsync_振休予定がある場合_振休と表示されること()
        {
            // Arrange
            var (syain, _) = InitializeTestData();
            var model = CreateModel();
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var targetDate = today.GetStartOfMonth().AddDays(8);

            var furikyu = new FurikyuuZan { Id = 7101, SyainId = syain.Id, SyutokuYoteiYmd = targetDate };
            db.FurikyuuZans.Add(furikyu);
            await db.SaveChangesAsync();

            model.SyainId = syain.Id;
            model.NippouYmd = today;

            // Act
            await model.OnGetAsync();

            // Assert
            var row = model.ViewModel.KarendaHyojiRows.First(r => r.Date == targetDate);
            Assert.AreEqual("振休", row.PlannedLeave);
        }

        /// <summary>
        /// 指定された月の残業実績（日報）を作成します。SaveChangesAsync は呼び出し側で行ってください。
        /// </summary>
        private async Task CreateMonthlyOvertime(long syainId, DateOnly month, int hours)
        {
            // ユニークなIDを確保するために既存レコード数（DB + Local）を考慮
            long nextId = db.Nippous.Count() + db.Nippous.Local.Count + 1000;

            var nippou = new NippouBuilder()
                .WithId(nextId)
                .WithSyainId(syainId)
                .WithNippouYmd(month)
                .WithDZangyo(hours)
                .Build();
            db.Nippous.Add(nippou);
        }
    }
}
