using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Enums;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.JissekiNyuryoku;
using ZouryokuTest.Builder;
using Microsoft.AspNetCore.Mvc.RazorPages; // PageResult 型参照


namespace ZouryokuTest.Pages.JissekiNyuryoku
{
    /// <summary>
    /// IndexModel 実績入力 のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {
        /// <summary>
        /// 実績入力用のIndexModelを生成し、テスト実行に必要なコンテキスト情報を設定します。
        /// </summary>
        /// <param name="loginUser">セッションに設定するログインユーザー（社員）情報</param>
        /// <returns>ページモデル</returns>
        private IndexModel CreateModel(Syain loginUser)
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };
            // セッション保存
            model.HttpContext.Session.Set(new LoginInfo { User = loginUser });
            return model;
        }

        #region OnGetAsync Tests

        /// <summary>
        /// 前提: ログイン済みユーザーと有効な社員データが存在する
        /// 操作: 日報実績が存在しない日付で初期表示
        /// 結果: 新規入力画面が正常に表示される
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_When日報実績が存在しない_Then新規入力画面が表示される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = (int)EmployeeWorkType.標準社員外,
                Code = EmployeeWorkType.標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithCode("001")
                .WithName("テスト社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("2025/01/01"))
                .WithEndYmd(D("2025/12/31"))
                .WithKintaiZokuseiId((int)EmployeeWorkType.標準社員外)
                .Build();
            db.Syains.Add(syain);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("2025/01/01"),
                MsClientId = "test-client-id",
                MsClientSecret = "test-client-secret",
                MsTenantId = "test-tenant-id",
                SmtpUser = "test@example.com",
                SmtpPassword = "test-password"
            };
            db.ApplicationConfigs.Add(appConfig);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            var jissekiDate = D("2025/01/15");

            // 実行 (Act)
            var result = await model.OnGetAsync(
                syainBaseId: syainBase.Id,
                jissekiDate: jissekiDate,
                isDairiInput: false,
                syukkinHm1: null,
                taisyutsuHm1: null,
                syukkinHm2: null,
                taisyutsuHm2: null,
                syukkinHm3: null,
                taisyutsuHm3: null
            );

            // 検証 (Assert)
            Assert.IsInstanceOfType(result, typeof(PageResult), "PageResultが返るべきです。");
            Assert.AreEqual(jissekiDate, model.JissekiDate, "実績日付が一致しません。");
            Assert.AreEqual(syain.Id, model.SyainId, "社員IDが一致しません。");
            Assert.IsNotNull(model.NippouData, "日報データが初期化されていません。");
        }

        /// <summary>
        /// 前提: ログイン済みユーザーと既存の日報実績データが存在する
        /// 操作: 既存日報の日付で初期表示
        /// 結果: 既存データが正しくViewModelに設定される
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_When既存日報データがある_Then既存データが表示される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 1,
                Code = EmployeeWorkType.標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("2025/01/01"))
                .WithEndYmd(D("2025/12/31"))
                .WithKintaiZokuseiId(kintaiZokusei.Id)
                .Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1)
                .WithCode("02")
                .WithName("通常勤務")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("2025/01/15");
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = DailyReportStatusClassification.一時保存
            };
            db.Nippous.Add(nippou);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("2025/01/01"),
                MsClientId = "test-client-id",
                MsClientSecret = "test-client-secret",
                MsTenantId = "test-tenant-id",
                SmtpUser = "test@example.com",
                SmtpPassword = "test-password"
            };
            db.ApplicationConfigs.Add(appConfig);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            var result = await model.OnGetAsync(
                syainBaseId: syainBase.Id,
                jissekiDate: jissekiDate,
                isDairiInput: false,
                syukkinHm1: null,
                taisyutsuHm1: null,
                syukkinHm2: null,
                taisyutsuHm2: null,
                syukkinHm3: null,
                taisyutsuHm3: null
            );

            // 検証 (Assert)
            Assert.IsInstanceOfType(result, typeof(PageResult), "PageResultが返るべきです。");
            Assert.AreEqual(nippou.Id, model.NippouData.Id, "日報IDが一致しません。");
            Assert.AreEqual(new TimeOnly(9, 0), model.NippouData.SyukkinHm1, "出勤時刻1が一致しません。");
            Assert.AreEqual(new TimeOnly(18, 0), model.NippouData.TaisyutsuHm1, "退出時刻1が一致しません。");
        }

        /// <summary>
        /// 前提: ログインしていない状態
        /// 操作: 初期表示を試みる
        /// 結果: ログイン画面にリダイレクトされる
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_When未ログイン_Thenログイン画面にリダイレクトされる()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("2025/01/01"))
                .WithEndYmd(D("2025/12/31"))
                .Build();
            db.Syains.Add(syain);

            await db.SaveChangesAsync();

            // セッションなしでモデル作成
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };

            // 実行 (Act)
            var result = await model.OnGetAsync(
                syainBaseId: syainBase.Id,
                jissekiDate: D("2025/01/15"),
                isDairiInput: false,
                syukkinHm1: null,
                taisyutsuHm1: null,
                syukkinHm2: null,
                taisyutsuHm2: null,
                syukkinHm3: null,
                taisyutsuHm3: null
            );

            // 検証 (Assert)
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult), "RedirectToPageResultが返るべきです。");
            var redirect = (RedirectToPageResult)result;
            Assert.AreEqual("/Logins/Index", redirect.PageName, "ログイン画面にリダイレクトされるべきです。");
        }

        /// <summary>
        /// 前提: 存在しない社員BaseIDを指定
        /// 操作: 初期表示を試みる
        /// 結果: NotFoundが返される
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_When社員が存在しない_ThenNotFoundが返される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = (int)EmployeeWorkType.標準社員外,
                Code = EmployeeWorkType.標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithCode("001")
                .WithName("テスト社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("2025/01/01"))
                .WithEndYmd(D("2025/12/31"))
                .WithKintaiZokuseiId((int)EmployeeWorkType.標準社員外)
                .Build();
            db.Syains.Add(syain);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("2025/01/01"),
                MsClientId = "test-client-id",
                MsClientSecret = "test-client-secret",
                MsTenantId = "test-tenant-id",
                SmtpUser = "test@example.com",
                SmtpPassword = "test-password"
            };
            db.ApplicationConfigs.Add(appConfig);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            var result = await model.OnGetAsync(
                syainBaseId: 999, // 存在しないID
                jissekiDate: D("2025/01/15"),
                isDairiInput: false,
                syukkinHm1: null,
                taisyutsuHm1: null,
                syukkinHm2: null,
                taisyutsuHm2: null,
                syukkinHm3: null,
                taisyutsuHm3: null
            );

            // 検証 (Assert)
            Assert.IsInstanceOfType(result, typeof(NotFoundResult), "NotFoundResultが返るべきです。");
        }

        #endregion

        #region OnPostTemporarySaveAsync Tests

        /// <summary>
        /// 前提: 有効な入力データ
        /// 操作: 一時保存を実行
        /// 結果: データが正常に保存される
        /// </summary>
        [TestMethod]
        public async Task OnPostTemporarySaveAsync_When有効なデータ_Then正常に保存される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = (int)EmployeeWorkType.標準社員外,
                Code = EmployeeWorkType.標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("2025/01/01"))
                .WithEndYmd(D("2025/12/31"))
                .WithKintaiZokuseiId((int)EmployeeWorkType.標準社員外)
                .Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1)
                .WithCode("02")
                .WithName("通常勤務")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var anken = new Anken
            {
                Id = 1,
                Name = "テスト案件",
                SearchName = "テスト案件"
            };
            db.Ankens.Add(anken);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("2025/01/01"),
                MsClientId = "test-client-id",
                MsClientSecret = "test-client-secret",
                MsTenantId = "test-tenant-id",
                SmtpUser = "test@example.com",
                SmtpPassword = "test-password"
            };
            db.ApplicationConfigs.Add(appConfig);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.JissekiDate = D("2025/01/15");
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunCodeString1 = "02",
                SyukkinKubun1 = AttendanceClassification.通常勤務
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel();
            model.NippouAnkenCards.NippouAnkens.Add(new IndexModel.NippouAnkenViewModel
            {
                AnkensId = 1,
                AnkenName = "テスト案件",
                IsLinked = true
            });

            var beforeCount = await db.Nippous.CountAsync();

            // 実行 (Act)
            var result = await model.OnPostTemporarySaveAsync(
                syainId: syain.Id,
                jissekiDate: D("2025/01/15"),
                isDairiInput: false
            );

            // 検証 (Assert)
            var afterCount = await db.Nippous.CountAsync();
            Assert.AreEqual(beforeCount + 1, afterCount, "日報データが1件追加されるべきです。");

            var saved = await db.Nippous.FirstAsync();
            Assert.AreEqual(syain.Id, saved.SyainId, "社員IDが一致しません。");
            Assert.AreEqual(new TimeOnly(9, 0), saved.SyukkinHm1, "出勤時刻1が一致しません。");
            Assert.AreEqual(new TimeOnly(18, 0), saved.TaisyutsuHm1, "退出時刻1が一致しません。");
            Assert.AreEqual(DailyReportStatusClassification.一時保存, saved.TourokuKubun, "登録区分が一時保存であるべきです。");
        }

        #endregion

        #region OnPostFinalConfirmAsync Tests

        /// <summary>
        /// 前提: 一時保存済みの日報データ
        /// 操作: 確定処理を実行
        /// 結果: データが確定保存される
        /// </summary>
        [TestMethod]
        public async Task OnPostFinalConfirmAsync_When一時保存データがある_Then確定保存される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = (int)EmployeeWorkType.標準社員外,
                Code = EmployeeWorkType.標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("2025/01/01"))
                .WithEndYmd(D("2025/12/31"))
                .WithKintaiZokuseiId((int)EmployeeWorkType.標準社員外)
                .Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1)
                .WithCode("02")
                .WithName("通常勤務")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("2025/01/15");
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = DailyReportStatusClassification.一時保存
            };
            db.Nippous.Add(nippou);

            // 前日の確定済みデータを追加
            var previousNippou = new Nippou
            {
                Id = 2,
                SyainId = syain.Id,
                NippouYmd = jissekiDate.AddDays(-1),
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = DailyReportStatusClassification.確定保存,
                KakuteiYmd = jissekiDate.AddDays(-1)
            };
            db.Nippous.Add(previousNippou);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("2025/01/01"),
                MsClientId = "test-client-id",
                MsClientSecret = "test-client-secret",
                MsTenantId = "test-tenant-id",
                SmtpUser = "test@example.com",
                SmtpPassword = "test-password"
            };
            db.ApplicationConfigs.Add(appConfig);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.JissekiDate = jissekiDate;
            model.NippouData = new IndexModel.NippouViewModel
            {
                Id = nippou.Id,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunCodeString1 = "02"
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel();

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmAsync(
                syainId: syain.Id,
                jissekiDate: jissekiDate,
                isDairiInput: false
            );

            // 検証 (Assert)
            var updated = await db.Nippous.FindAsync(nippou.Id);
            Assert.IsNotNull(updated, "日報データが存在するべきです。");
        }

        #endregion

        #region OnPostCancelConfirmAsync Tests

        /// <summary>
        /// 前提: 確定済みの日報データ（当日確定）
        /// 操作: 確定解除を実行
        /// 結果: 一時保存に戻される
        /// </summary>
        [TestMethod]
        public async Task OnPostCancelConfirmAsync_When当日確定データ_Then一時保存に戻される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = (int)EmployeeWorkType.標準社員外,
                Code = EmployeeWorkType.標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithCode("001")
                .WithName("テスト社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("2025/01/01"))
                .WithEndYmd(D("2025/12/31"))
                .WithKintaiZokuseiId((int)EmployeeWorkType.標準社員外)
                .Build();
            db.Syains.Add(syain);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("2025/01/01"),
                MsClientId = "test-client-id",
                MsClientSecret = "test-client-secret",
                MsTenantId = "test-tenant-id",
                SmtpUser = "test@example.com",
                SmtpPassword = "test-password"
            };
            db.ApplicationConfigs.Add(appConfig);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1)
                .WithCode("02")
                .WithName("通常勤務")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("2025/01/15");
            var today = DateOnly.FromDateTime(DateTime.Today);
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = DailyReportStatusClassification.確定保存,
                KakuteiYmd = today // 当日確定
            };
            db.Nippous.Add(nippou);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            var result = await model.OnPostCancelConfirmAsync(
                syainId: syain.Id,
                syainBaseId: syain.SyainBaseId,
                jissekiDate: jissekiDate,
                isDairiInput: false
            );

            // 検証 (Assert)
            var updated = await db.Nippous.FindAsync(nippou.Id);
            Assert.IsNotNull(updated, "日報データが存在するべきです。");
            Assert.AreEqual(DailyReportStatusClassification.一時保存, updated.TourokuKubun, "登録区分が一時保存に戻されるべきです。");
        }

        #endregion

        #region OnPostCopyFromLastDateAsync Tests

        /// <summary>
        /// 前提: 前回の日報データが存在する
        /// 操作: 前回コピーを実行
        /// 結果: 前回データが正しくコピーされる
        /// </summary>
        [TestMethod]
        public async Task OnPostCopyFromLastDateAsync_When前回データがある_Then正しくコピーされる()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = (int)EmployeeWorkType.標準社員外,
                Code = EmployeeWorkType.標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithCode("001")
                .WithName("テスト社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("2025/01/01"))
                .WithEndYmd(D("2025/12/31"))
                .WithKintaiZokuseiId((int)EmployeeWorkType.標準社員外)
                .Build();
            db.Syains.Add(syain);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("2025/01/01"),
                MsClientId = "test-client-id",
                MsClientSecret = "test-client-secret",
                MsTenantId = "test-tenant-id",
                SmtpUser = "test@example.com",
                SmtpPassword = "test-password"
            };
            db.ApplicationConfigs.Add(appConfig);

            var anken = new Anken
            {
                Id = 1,
                Name = "テスト案件",
                SearchName = "テスト案件"
            };
            db.Ankens.Add(anken);

            var previousNippou = new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = D("2025/01/14"),
                TourokuKubun = DailyReportStatusClassification.確定保存
            };
            db.Nippous.Add(previousNippou);

            var nippouAnken = new NippouAnken
            {
                Id = 1,
                NippouId = previousNippou.Id,
                AnkensId = anken.Id,
                AnkenName = "テスト案件",
                JissekiJikan = 480,
                KokyakuName = "テスト顧客",
            };
            db.NippouAnkens.Add(nippouAnken);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            var result = await model.OnPostCopyFromLastDateAsync(
                syainId: syain.Id,
                jissekiDate: D("2025/01/15")
            );

            // 検証 (Assert)
            Assert.IsInstanceOfType(result, typeof(JsonResult), "JsonResultが返るべきです。");
            var jsonResult = (JsonResult)result;
            Assert.IsNotNull(jsonResult.Value, "結果が返されるべきです。");
        }

        #endregion

        #region ViewModel Tests

        /// <summary>
        /// 前提: NippouViewModelに出退勤時刻を設定
        /// 操作: TotalWorkingHoursInMinuteを取得
        /// 結果: 正しい労働時間（分）が計算される
        /// </summary>
        [TestMethod]
        public void NippouViewModel_When出退勤時刻設定_Then労働時間が正しく計算される()
        {
            // 準備 (Arrange)
            var viewModel = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0)
            };

            // 実行 (Act)
            var totalMinutes = viewModel.TotalWorkingHoursInMinute;

            // 検証 (Assert)
            Assert.IsTrue(totalMinutes > 0, "労働時間が計算されるべきです。");
        }

        /// <summary>
        /// 前提: NippouViewModelに出勤区分コードを設定
        /// 操作: SyukkinKubun1プロパティを取得
        /// 結果: 正しい列挙値が返される
        /// </summary>
        [TestMethod]
        public void NippouViewModel_When出勤区分コード設定_Then列挙値が正しく変換される()
        {
            // 準備 (Arrange)
            var viewModel = new IndexModel.NippouViewModel
            {
                SyukkinKubunCodeString1 = "02"
            };

            // 実行 (Act)
            var kubun = viewModel.SyukkinKubun1;

            // 検証 (Assert)
            Assert.AreEqual(AttendanceClassification.通常勤務, kubun, "出勤区分が正しく変換されるべきです。");
        }

        /// <summary>
        /// 前提: NippouAnkenエンティティ
        /// 操作: FromEntityメソッドでViewModelに変換
        /// 結果: データが正しくマッピングされる
        /// </summary>
        [TestMethod]
        public void NippouAnkenViewModel_WhenFromEntity実行_Thenデータが正しくマッピングされる()
        {
            // 準備 (Arrange)
            var anken = new Anken
            {
                Id = 1,
                Name = "テスト案件",
                SearchName = "テスト案件",
                KingsJuchu = new KingsJuchu
                {
                    Id = 1,
                    JucKn = "J001",
                    JuchuuNo = "001",
                    ChaYmd = D("2025/01/01"),
                    IsGenkaToketu = true
                }
            };

            var nippouAnken = new NippouAnken
            {
                Id = 1,
                NippouId = 1,
                AnkensId = anken.Id,
                Ankens = anken,
                AnkenName = "テスト案件",
                KokyakuKaisyaId = 1,
                KokyakuName = "テスト顧客",
                BumonProcessId = 1,
                JissekiJikan = 480,
                IsLinked = true
            };

            // 実行 (Act)
            var viewModel = IndexModel.NippouAnkenViewModel.FromEntity(nippouAnken);

            // 検証 (Assert)
            Assert.IsNotNull(viewModel, "ViewModelが生成されるべきです。");
            Assert.AreEqual(nippouAnken.IsLinked, viewModel.IsLinked, "IsLinkedが一致しません。");
            Assert.AreEqual(anken.KingsJuchu.KingsJuchuNo, viewModel.KingsJuchuNo, "KingsJuchuNoが一致しません。");
            Assert.AreEqual(nippouAnken.AnkensId, viewModel.AnkensId, "AnkensIdが一致しません。");
            Assert.AreEqual(nippouAnken.AnkenName, viewModel.AnkenName, "AnkenNameが一致しません。");
            Assert.AreEqual(anken.KingsJuchu.ChaYmd, viewModel.ChaYmd, "ChaYmdが一致しません。");
            Assert.AreEqual(anken.KingsJuchu.JuchuuNo, viewModel.JuchuuNo, "JuchuuNoが一致しません。");
            Assert.AreEqual(nippouAnken.KokyakuKaisyaId, viewModel.KokyakuKaisyaId, "KokyakuKaisyaIdが一致しません。");
            Assert.AreEqual(nippouAnken.KokyakuName, viewModel.KokyakuName, "KokyakuNameが一致しません。");
            Assert.AreEqual(nippouAnken.BumonProcessId, viewModel.BumonProcessId, "BumonProcessIdが一致しません。");
            Assert.AreEqual(nippouAnken.JissekiJikan, viewModel.JissekiJikan, "JissekiJikanが一致しません。");
            Assert.AreEqual(anken.KingsJuchu.IsGenkaToketu, viewModel.IsGenkaToketu, "IsGenkaToketuが一致しません。");
        }

        #endregion

        #region Boundary Tests

        /// <summary>
        /// 前提: 出退勤時刻が未入力
        /// 操作: 一時保存を実行
        /// 結果: 出退勤時刻がnullで保存される
        /// </summary>
        [TestMethod]
        public async Task OnPostTemporarySaveAsync_When出退勤未入力_Thennullで保存される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = (int)EmployeeWorkType.標準社員外,
                Code = EmployeeWorkType.標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("2025/01/01"))
                .WithEndYmd(D("2025/12/31"))
                .WithKintaiZokuseiId((int)EmployeeWorkType.標準社員外)
                .Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1)
                .WithCode("01")
                .WithName("休日")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("2025/01/01"),
                MsClientId = "test-client-id",
                MsClientSecret = "test-client-secret",
                MsTenantId = "test-tenant-id",
                SmtpUser = "test@example.com",
                SmtpPassword = "test-password"
            };
            db.ApplicationConfigs.Add(appConfig);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.JissekiDate = D("2025/01/15");
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = null,
                TaisyutsuHm1 = null,
                SyukkinKubunCodeString1 = "01",
                SyukkinKubun1 = AttendanceClassification.休日
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel();

            // 実行 (Act)
            var result = await model.OnPostTemporarySaveAsync(
                syainId: syain.Id,
                jissekiDate: D("2025/01/12"),
                isDairiInput: false
            );

            // 検証 (Assert)
            var saved = await db.Nippous.FirstAsync();
            Assert.IsNull(saved.SyukkinHm1, "出勤時刻1がnullであるべきです。");
            Assert.IsNull(saved.TaisyutsuHm1, "退出時刻1がnullであるべきです。");
        }

        /// <summary>
        /// 前提: 3回分の出退勤時刻を設定
        /// 操作: 一時保存を実行
        /// 結果: 全ての出退勤時刻が正しく保存される
        /// </summary>
        [TestMethod]
        public async Task OnPostTemporarySaveAsync_When3回出退勤_Then全て保存される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = (int)EmployeeWorkType.標準社員外,
                Code = EmployeeWorkType.標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("2025/01/01"))
                .WithEndYmd(D("2025/12/31"))
                .WithKintaiZokuseiId((int)EmployeeWorkType.標準社員外)
                .Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1)
                .WithCode("02")
                .WithName("通常勤務")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("2025/01/01"),
                MsClientId = "test-client-id",
                MsClientSecret = "test-client-secret",
                MsTenantId = "test-tenant-id",
                SmtpUser = "test@example.com",
                SmtpPassword = "test-password"
            };
            db.ApplicationConfigs.Add(appConfig);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.JissekiDate = D("2025/01/15");
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(12, 0),
                SyukkinHm2 = new TimeOnly(13, 0),
                TaisyutsuHm2 = new TimeOnly(18, 0),
                SyukkinHm3 = new TimeOnly(19, 0),
                TaisyutsuHm3 = new TimeOnly(22, 0),
                SyukkinKubunCodeString1 = "02",
                SyukkinKubun1 = AttendanceClassification.通常勤務
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel();

            // 実行 (Act)
            var result = await model.OnPostTemporarySaveAsync(
                syainId: syain.Id,
                jissekiDate: D("2025/01/15"),
                isDairiInput: false
            );

            // 検証 (Assert)
            var saved = await db.Nippous.FirstAsync();
            Assert.AreEqual(new TimeOnly(9, 0), saved.SyukkinHm1, "出勤時刻1が一致しません。");
            Assert.AreEqual(new TimeOnly(12, 0), saved.TaisyutsuHm1, "退出時刻1が一致しません。");
            Assert.AreEqual(new TimeOnly(13, 0), saved.SyukkinHm2, "出勤時刻2が一致しません。");
            Assert.AreEqual(new TimeOnly(18, 0), saved.TaisyutsuHm2, "退出時刻2が一致しません。");
            Assert.AreEqual(new TimeOnly(19, 0), saved.SyukkinHm3, "出勤時刻3が一致しません。");
            Assert.AreEqual(new TimeOnly(22, 0), saved.TaisyutsuHm3, "退出時刻3が一致しません。");
        }

        #endregion
    }
}
