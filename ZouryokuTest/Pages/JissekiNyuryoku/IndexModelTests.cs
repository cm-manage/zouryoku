using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages; // PageResult 型参照
using Microsoft.EntityFrameworkCore;
using Model.Enums;
using Model.Model;
using static Model.Enums.ApprovalStatus;
using static Model.Enums.AttendanceClassification;
using static Model.Enums.DailyReportStatusClassification;
using static Model.Enums.EmployeeWorkType;
using static Model.Enums.InquiryType;
using static Model.Enums.LeaveBalanceFetchStatus;
using System.Reflection;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.JissekiNyuryoku;
using ZouryokuCommonLibrary.Utils;
using ZouryokuTest.Builder;
using ZouryokuTest.Pages.Builder;

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

        private CompensatoryPaidLeave CreateCompensatoryPaidLeave(
            DateOnly jissekiDate,
            long syainId,
            AttendanceClassification syukkinKubun1,
            AttendanceClassification syukkinKubun2 = None,
            bool isWorkDay = true,
            TimeOnly? syukkinHm1 = null,
            TimeOnly? taisyutsuHm1 = null,
            DateOnly? furikyuYoteiDate = null)
        {
            var nippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = syukkinKubun1,
                SyukkinKubun2 = syukkinKubun2,
                SyukkinHm1 = syukkinHm1,
                TaisyutsuHm1 = taisyutsuHm1
            };

            return new CompensatoryPaidLeave(
                jissekiDate,
                syainId,
                nippouData,
                isWorkDay,
                db,
                furikyuYoteiDate,
                options.Value);
        }

        private static async Task InvokePrivateAsync(object instance, string methodName, params object?[] args)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"{methodName} が見つかりません。");
            var task = method.Invoke(instance, args) as Task;
            Assert.IsNotNull(task, $"{methodName} の戻り値は Task である必要があります。");
            await task;
        }

        private static async Task<T?> InvokePrivateWithResultAsync<T>(
            object instance,
            string methodName,
            params object?[] args)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"{methodName} が見つかりません。");
            var task = method.Invoke(instance, args) as Task;
            Assert.IsNotNull(task, $"{methodName} の戻り値は Task である必要があります。");
            await task;
            var resultProperty = task.GetType().GetProperty("Result");
            if (resultProperty == null) return default;
            return (T?)resultProperty.GetValue(task);
        }

        private static void InvokePrivateVoid(object instance, string methodName, params object?[]? args)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"{methodName} が見つかりません。");
            _ = method.Invoke(instance, args);
        }

        private static TimeOnly? ParseHHmmOrNull(string? hhmm)
            => string.IsNullOrWhiteSpace(hhmm) ? null : TimeOnly.ParseExact(hhmm, "HHmm", null);

        private ConfirmValidation CreateConfirmValidation(
            Syain syain,
            DateOnly jissekiDate,
            AttendanceClassification syukkinKubun1,
            AttendanceClassification syukkinKubun2 = None,
            bool isWorkDay = true,
            DateOnly? furiyoteiDate = null,
            TimeOnly? syukkinHm1 = null,
            TimeOnly? taisyutsuHm1 = null,
            decimal? dJitsudou = null,
            decimal? nJitsudou = null,
            ModelStateDictionary? modelState = null)
        {
            var nippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = syukkinKubun1,
                SyukkinKubun2 = syukkinKubun2,
                SyukkinHm1 = syukkinHm1,
                TaisyutsuHm1 = taisyutsuHm1
            };
            if (dJitsudou.HasValue) nippouData.DJitsudou = dJitsudou;
            if (nJitsudou.HasValue) nippouData.NJitsudou = nJitsudou;

            return new ConfirmValidation(
                syain,
                jissekiDate,
                nippouData,
                isWorkDay,
                furiyoteiDate,
                db,
                modelState ?? new ModelStateDictionary());
        }

        private Syain SeedConfirmValidationSyain(
            long syainId = 1,
            long syainBaseId = 1,
            char seibetsu = '1',
            bool isGenkaRendou = false,
            EmployeeAuthority kengen = 0)
        {
            var syainBase = new SyainBasisBuilder().WithId(syainBaseId).Build();
            db.SyainBases.Add(syainBase);

            var busyoBase = new BusyoBasisBuilder().WithId(1).Build();
            var busyo = new BusyoBuilder().WithId(1).WithBusyoBaseId(busyoBase.Id).Build();
            db.BusyoBases.Add(busyoBase);
            db.Busyos.Add(busyo);

            var kintai = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外",
                SeigenTime = 45m,
                MaxLimitTime = 80m
            };
            db.KintaiZokuseis.Add(kintai);

            var syain = new SyainBuilder()
                .WithId(syainId)
                .WithSyainBaseId(syainBaseId)
                .WithBusyoId(busyo.Id)
                .WithKintaiZokuseiId(kintai.Id)
                .WithSeibetsu(seibetsu)
                .WithIsGenkaRendou(isGenkaRendou)
                .WithKengen(kengen)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);
            return syain;
        }

        private void AddSyukkinKubun(
            AttendanceClassification code,
            bool isSyukkin,
            bool isVacation,
            bool isHoliday = false)
        {
            var codeValue = Convert.ToInt64(code);
            db.SyukkinKubuns.Add(new SyukkinKubun
            {
                Id = codeValue,
                CodeString = codeValue.ToString("D2"),
                Name = code.ToString(),
                NameRyaku = code.ToString(),
                IsSyukkin = isSyukkin,
                IsVacation = isVacation,
                IsHoliday = isHoliday,
                IsNeedKubun1 = true,
                IsNeedKubun2 = true
            });
        }

        private void EnsureKintaiZokusei(long id, string? name = null)
        {
            if (db.KintaiZokuseis.Any(x => x.Id == id)) return;
            db.KintaiZokuseis.Add(new KintaiZokusei
            {
                Id = id,
                Name = name ?? $"勤怠属性{id}",
                SeigenTime = 45m,
                MaxLimitTime = 80m,
                IsMinashi = false,
                IsOvertimeLimit3m = false
            });
        }

        private static string GetConfirmValidationConstant(string constantName)
        {
            var field = typeof(ConfirmValidation).GetField(constantName, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field, $"{constantName} が ConfirmValidation に見つかりません。");
            return (string)(field.GetRawConstantValue() ?? field.GetValue(null) ?? string.Empty);
        }

        private static void AssertConfirmValidationError(ModelStateDictionary modelState, string constantName)
        {
            var expected = GetConfirmValidationConstant(constantName);
            var errors = GetModelStateErrors(modelState);
            Assert.Contains(
expected,
                errors, $"期待エラー '{constantName}' が見つかりません。実際: {string.Join(" | ", errors)}");
        }

        private static List<string> GetModelStateErrors(ModelStateDictionary modelState)
            => modelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

        private static List<IndexModel.NippouAnkenViewModel> CreateNippouAnkens(int count, int linkedCount)
        {
            var result = new List<IndexModel.NippouAnkenViewModel>();
            for (int i = 0; i < count; i++)
            {
                result.Add(new IndexModel.NippouAnkenViewModel
                {
                    AnkensId = i + 1,
                    IsLinked = i < linkedCount
                });
            }
            return result;
        }

        private (Syain syain, SyukkinKubun kubun, Anken anken) SeedFinalConfirmMinimumData(
            long syainId = 1,
            long syainBaseId = 1,
            long busyoId = 1,
            EmployeeAuthority kengen = 0)
        {
            db.SyainBases.Add(new SyainBasisBuilder().WithId(syainBaseId).Build());

            var kintaiId = 5;
            db.KintaiZokuseis.Add(new KintaiZokusei
            {
                Id = kintaiId,
                Code = 標準社員外,
                Name = "標準社員外",
                SeigenTime = 45m,
                MaxLimitTime = 80m,
                IsOvertimeLimit3m = false
            });

            var syain = new SyainBuilder()
                .WithId(syainId)
                .WithSyainBaseId(syainBaseId)
                .WithBusyoId(busyoId)
                .WithKintaiZokuseiId(kintaiId)
                .WithKengen(kengen)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);

            var kubun = new SyukkinKubunBuilder()
                .WithId(2)
                .WithCode("02")
                .WithName("通常勤務")
                .WithNameRyaku("通常勤務")
                .WithIsSyukkin(true)
                .WithIsVacation(false)
                .WithIsHoliday(false)
                .WithIsNeedKubun1(true)
                .WithIsNeedKubun2(true)
                .Build();
            db.SyukkinKubuns.Add(kubun);

            var kings = new KingsJuchuBuilder()
                .WithId(1)
                .WithBusyoId(busyoId)
                .WithHiyouShubetuCd(1)
                .WithIsGenkaToketu(false)
                .Build();
            db.KingsJuchus.Add(kings);

            var anken = new AnkenBuilder()
                .WithId(1)
                .WithKingsJuchuId(kings.Id)
                .WithKokyakuKaisyaId(1)
                .Build();
            db.Ankens.Add(anken);

            return (syain, kubun, anken);
        }

        private void AssertFinalConfirmSuccess(IActionResult result)
        {
            if (result is JsonResult jsonResult)
            {
                Assert.AreEqual(
                    ResponseStatus.正常,
                    GetResponseStatus(jsonResult),
                    $"JsonResult の status が正常であるべきです。message={GetMessage(jsonResult)}");
                return;
            }
            AssertSuccess(result);
        }

        #region OnGetAsync Tests

        /// <summary>
        /// Given: ログイン済みユーザーと有効な社員データが存在する
        /// When: 日報実績が存在しない日付で初期表示
        /// Then: 新規入力画面が正常に表示される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 日報実績が存在しない場合は新規入力画面が表示される")]
        public async Task OnGetAsync_日報実績が存在しない場合は新規入力画面が表示される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithCode("001")
                .WithName("テスト社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(5)
                .Build();
            db.Syains.Add(syain);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250101"),
                MsClientId = "test-client-id",
                MsClientSecret = "test-client-secret",
                MsTenantId = "test-tenant-id",
                SmtpUser = "test@example.com",
                SmtpPassword = "test-password"
            };
            db.ApplicationConfigs.Add(appConfig);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            var jissekiDate = D("20250115");

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
        /// Given: ログイン済みユーザーと既存の日報実績データが存在する
        /// When: 既存日報の日付で初期表示
        /// Then: 既存データが正しくViewModelに設定される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 既存日報実績が存在する場合は既存データが表示される")]
        public async Task OnGetAsync_既存日報実績が存在する場合は既存データが表示される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 1,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id)
                .Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1)
                .WithCode("02")
                .WithName("通常勤務")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("20250115");
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 一時保存
            };
            db.Nippous.Add(nippou);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250101"),
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
        /// Given: ログインしていない状態
        /// When: 初期表示を試みる
        /// Then: ログイン画面にリダイレクトされる
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 未ログインの場合はログイン画面にリダイレクトされる")]
        public async Task OnGetAsync_未ログインの場合はログイン画面にリダイレクトされる()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
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
                jissekiDate: D("20250115"),
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
        /// Given: 存在しない社員BaseIDを指定
        /// When: 初期表示を試みる
        /// Then: NotFoundが返される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 存在しない社員BaseIDの場合はNotFoundが返される")]
        public async Task OnGetAsync_存在しない社員BaseIDの場合はNotFoundが返される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithCode("001")
                .WithName("テスト社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(5)
                .Build();
            db.Syains.Add(syain);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250101"),
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
                jissekiDate: D("20250115"),
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
        /// Given: 有効な入力データ
        /// When: 一時保存を実行
        /// Then: データが正常に保存される
        /// </summary>
        [TestMethod(DisplayName = "OnPostTemporarySaveAsync: 有効な入力データの場合はデータが正常に保存される")]
        public async Task OnPostTemporarySaveAsync_有効な入力データの場合はデータが正常に保存される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(5)
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
                NippoStopDate = D("20250101"),
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
            model.JissekiDate = D("20250115");
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunCodeString1 = "02",
                SyukkinKubun1 = 通常勤務
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
                jissekiDate: D("20250115"),
                isDairiInput: false
            );

            // 検証 (Assert)
            var afterSavedCount = await db.Nippous.CountAsync();
            Assert.AreEqual(beforeCount + 1, afterSavedCount, "日報データが1件追加されるべきです。");

            var savedNippou = await db.Nippous.FirstAsync();
            Assert.AreEqual(syain.Id, savedNippou.SyainId, "社員IDが一致しません。");
            Assert.AreEqual(new TimeOnly(9, 0), savedNippou.SyukkinHm1, "出勤時刻1が一致しません。");
            Assert.AreEqual(new TimeOnly(18, 0), savedNippou.TaisyutsuHm1, "退出時刻1が一致しません。");
            Assert.AreEqual(一時保存, savedNippou.TourokuKubun, "登録区分が一時保存であるべきです。");
        }

        #endregion

        #region OnPostFinalConfirmAsync Tests

        /// <summary>
        /// Given: 一時保存済みの日報データ
        /// When: 確定処理を実行
        /// Then: データが確定保存される
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 一時保存済みデータが確定保存される")]
        public async Task OnPostFinalConfirmAsync_一時保存済みデータが確定保存される()
        {
            // 準備 (Arrange)
            var (syain, syukkinKubun, anken) = SeedFinalConfirmMinimumData();

            var jissekiDate = D("20250115");
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 一時保存
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
                TourokuKubun = 確定保存,
                KakuteiYmd = jissekiDate.AddDays(-1)
            };
            db.Nippous.Add(previousNippou);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.JissekiDate = jissekiDate;
            model.NippouData = new IndexModel.NippouViewModel
            {
                Id = nippou.Id,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubun1 = 通常勤務,
                SyukkinKubun2 = None
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = true,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 480
                    }
                }
            };

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmAsync(
                syainId: syain.Id,
                jissekiDate: jissekiDate,
                isDairiInput: false
            );

            // 検証 (Assert)
            var updatedNippou = await db.Nippous.FindAsync(nippou.Id);
            Assert.IsNotNull(updatedNippou, "日報データが存在するべきです。");
            Assert.AreEqual(確定保存, updatedNippou.TourokuKubun, "登録区分が確定保存であるべきです。");
        }

        /// <summary>
        /// Given: OnPostFinalConfirmAsync の条件を満たしている
        /// When: 社員が見つからない
        /// Then: エラーJsonが返る
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 社員が見つからない場合はエラーJsonが返される")]
        public async Task OnPostFinalConfirmAsync_社員が見つからない場合はエラーJsonが返される()
        {
            var loginUser = new SyainBuilder().WithId(999).Build();
            var model = CreateModel(loginUser);

            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = 通常勤務,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0)
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>()
            };

            var result = await model.OnPostFinalConfirmAsync(1, D("20250115"), false);

            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.エラー, GetResponseStatus(json));
        }

        /// <summary>
        /// Given: OnPostFinalConfirmAsync の条件を満たしている
        /// When: バリデーションエラー
        /// Then: エラーJsonが返る
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: バリデーションエラーの場合はエラーJsonが返される")]
        public async Task OnPostFinalConfirmAsync_バリデーションエラーの場合はエラーJsonが返される()
        {
            var (syain, _, _) = SeedFinalConfirmMinimumData();
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.NippouData = new IndexModel.NippouViewModel();
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>()
            };

            var result = await model.OnPostFinalConfirmAsync(syain.Id, D("20250115"), false);

            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.エラー, GetResponseStatus(json));
            Assert.IsFalse(string.IsNullOrWhiteSpace(GetMessage(json)), "エラーメッセージが返るべきです。");
        }

        /// <summary>
        /// Given: OnPostFinalConfirmAsync の条件を満たしている
        /// When: 新規確定
        /// Then: 日報と案件行が作成される
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 新規確定の場合は日報と案件行が作成される")]
        public async Task OnPostFinalConfirmAsync_新規確定の場合は日報と案件行が作成される()
        {
            var (syain, _, anken) = SeedFinalConfirmMinimumData();
            await db.SaveChangesAsync();

            var jissekiDate = D("20250115");
            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = 通常勤務,
                SyukkinKubun2 = None,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0)
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = true,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 480
                    }
                }
            };

            var result = await model.OnPostFinalConfirmAsync(syain.Id, jissekiDate, false);

            AssertFinalConfirmSuccess(result);

            var savedNippou = await db.Nippous.SingleAsync(n => n.SyainId == syain.Id && n.NippouYmd == jissekiDate);
            Assert.AreEqual(確定保存, savedNippou.TourokuKubun, "登録区分が確定保存になるべきです。");
            Assert.AreEqual(2, savedNippou.SyukkinKubunId1, "出勤区分1が保存されるべきです。");

            var savedNippouAnkens = await db.NippouAnkens.Where(x => x.NippouId == savedNippou.Id).ToListAsync();
            Assert.HasCount(1, savedNippouAnkens, "案件行が1件作成されるべきです。");
            Assert.AreEqual(anken.Id, savedNippouAnkens[0].AnkensId, "案件IDが保存されるべきです。");
        }

        /// <summary>
        /// Given: OnPostFinalConfirmAsync の条件を満たしている
        /// When: 既存日報あり InMemoryExecuteDelete例外
        /// Then: エラーを返す
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 既存日報ありInMemoryExecuteDelete例外の場合はエラーJsonが返される")]
        public async Task OnPostFinalConfirmAsync_既存日報ありInMemoryExecuteDelete例外の場合はエラーJsonが返される()
        {
            var (syain, kubun, anken) = SeedFinalConfirmMinimumData();
            var jissekiDate = D("20250115");
            var nippou = new Nippou
            {
                Id = 10,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(8, 30),
                TaisyutsuHm1 = new TimeOnly(17, 0),
                SyukkinKubunId1 = kubun.Id,
                TourokuKubun = 一時保存
            };
            db.Nippous.Add(nippou);
            db.NippouAnkens.Add(new NippouAnken
            {
                NippouId = nippou.Id,
                AnkensId = anken.Id,
                KokyakuKaisyaId = 1,
                KokyakuName = "旧顧客",
                AnkenName = "旧案件",
                JissekiJikan = 60,
                IsLinked = true
            });
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.NippouData = new IndexModel.NippouViewModel
            {
                Id = nippou.Id,
                SyukkinKubun1 = 通常勤務,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0)
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = true,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "新顧客",
                        AnkenName = "新案件",
                        JissekiJikan = 480
                    }
                }
            };

            var result = await model.OnPostFinalConfirmAsync(syain.Id, jissekiDate, false);

            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.エラー, GetResponseStatus(json));
            StringAssert.Contains(
                GetMessage(json) ?? string.Empty,
                "サーバー内部でエラー",
                "InMemory環境ではExecuteDeleteAsyncが失敗しエラーになるべきです。");
        }

        /// <summary>
        /// Given: OnPostFinalConfirmAsync の条件を満たしている
        /// When: 時間外拡張伺いが存在し条件一致
        /// Then: 伺いが無効化される
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 時間外拡張伺いが存在し条件一致の場合は伺いが無効化される")]
        public async Task OnPostFinalConfirmAsync_時間外拡張伺いが存在し条件一致の場合は伺いが無効化される()
        {
            var (syain, _, anken) = SeedFinalConfirmMinimumData(kengen: EmployeeAuthority.勤怠データ出力);
            var jissekiDate = D("20250115");
            var ukagai = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(syain.Id)
                .WithWorkYmd(jissekiDate)
                .WithStatus(承認)
                .WithInvalid(false)
                .Build();
            var shinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(ukagai.Id)
                .WithUkagaiSyubetsu(時間外労働時間制限拡張)
                .Build();
            db.UkagaiHeaders.Add(ukagai);
            db.UkagaiShinseis.Add(shinsei);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = 通常勤務,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0)
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = true,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 480
                    }
                }
            };

            var result = await model.OnPostFinalConfirmAsync(syain.Id, jissekiDate, false);

            AssertFinalConfirmSuccess(result);
            var updatedUkagai = await db.UkagaiHeaders.FindAsync(ukagai.Id);
            Assert.IsNotNull(updatedUkagai);
            Assert.IsTrue(updatedUkagai.Invalid, "時間外拡張伺いが無効化されるべきです。");
        }

        /// <summary>
        /// Given: OnPostFinalConfirmAsync の条件を満たしている
        /// When: 代理入力
        /// Then: 代理入力履歴を追加する
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 代理入力の場合は代理入力履歴が追加される")]
        public async Task OnPostFinalConfirmAsync_代理入力の場合は代理入力履歴が追加される()
        {
            var (syain, _, anken) = SeedFinalConfirmMinimumData();
            var proxyUser = new SyainBuilder()
                .WithId(99)
                .WithSyainBaseId(99)
                .WithKintaiZokuseiId(5)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(proxyUser);
            await db.SaveChangesAsync();

            var jissekiDate = D("20250115");
            var model = CreateModel(proxyUser);
            model.SyainId = syain.Id;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = 通常勤務,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0)
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = true,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 480
                    }
                }
            };

            var result = await model.OnPostFinalConfirmAsync(syain.Id, jissekiDate, true);

            AssertFinalConfirmSuccess(result);

            var savedNippou = await db.Nippous.SingleAsync(x => x.SyainId == syain.Id && x.NippouYmd == jissekiDate);
            var history = await db.DairiNyuryokuRirekis
                .FirstOrDefaultAsync(h => h.NippouId == savedNippou.Id && h.NippouSousa == DailyReportOperation.確定);
            Assert.IsNotNull(history, "代理入力履歴が作成されるべきです。");
            Assert.AreEqual(proxyUser.Id, history.DairiNyuryokuSyainId, "代理入力者IDが保存されるべきです。");
            Assert.IsFalse(history.Invalid, "新規履歴は無効でないべきです。");
        }

        /// <summary>
        /// Given: OnPostFinalConfirmAsync の条件を満たしている
        /// When: 通常入力で既存代理履歴あり
        /// Then: 代理履歴を無効化する
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 通常入力で既存代理履歴ありの場合は代理履歴が無効化される")]
        public async Task OnPostFinalConfirmAsync_通常入力で既存代理履歴ありの場合は代理履歴が無効化される()
        {
            var (syain, _, anken) = SeedFinalConfirmMinimumData();
            var jissekiDate = D("20250115");
            // 新規作成時の先頭ID(1)を想定して先に履歴を用意する
            db.DairiNyuryokuRirekis.Add(new DairiNyuryokuRireki
            {
                DairiNyuryokuSyainId = syain.Id,
                DairiNyuryokuTime = DateTime.Now,
                NippouId = 1,
                NippouSousa = DailyReportOperation.確定,
                Invalid = false
            });
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = 通常勤務,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0)
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = true,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 480
                    }
                }
            };

            var result = await model.OnPostFinalConfirmAsync(syain.Id, jissekiDate, false);

            AssertFinalConfirmSuccess(result);

            var savedNippou = await db.Nippous.SingleAsync(n => n.SyainId == syain.Id && n.NippouYmd == jissekiDate);
            var history = await db.DairiNyuryokuRirekis
                .SingleAsync(h => h.NippouId == savedNippou.Id && h.NippouSousa == DailyReportOperation.確定);
            Assert.IsTrue(history.Invalid, "通常入力時は既存の代理入力履歴が無効化されるべきです。");
        }

        /// <summary>
        /// Given: OnPostFinalConfirmAsync の条件を満たしている
        /// When: 保存中に例外発生
        /// Then: 内部エラーを返す
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 保存中に例外発生の場合は内部エラーが返される")]
        public async Task OnPostFinalConfirmAsync_保存中に例外発生の場合は内部エラーが返される()
        {
            var (syain, _, anken) = SeedFinalConfirmMinimumData();
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = 通常勤務,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0)
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = true,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = null,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 480
                    }
                }
            };

            var result = await model.OnPostFinalConfirmAsync(syain.Id, D("20250115"), false);

            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.エラー, GetResponseStatus(json));
            Assert.IsFalse(string.IsNullOrWhiteSpace(GetMessage(json)), "内部エラーメッセージが返るべきです。");
        }

        /// <summary>
        /// Given: 月末日の確定解除で、無効化済みの時間外労働時間制限拡張申請が当月内に存在する
        /// When: OnPostCancelConfirmAsync を実行する
        /// Then: 該当 UkagaiHeader.Invalid が false に戻る
        /// </summary>
        [TestMethod(DisplayName = "OnPostCancelConfirmAsync: 月末日の確定解除で、無効化済みの時間外労働時間制限拡張申請が" +
            "当月内に存在する場合は無効化が解除される")]
        public async Task OnPostCancelConfirmAsync_月末日の確定解除で無効化済みの時間外労働時間制限拡張申請が当月内に存在する場合は無効化が解除される()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(1);
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(1)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1)
                .WithCode("02")
                .WithName("通常勤務")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var monthEnd = D("20250131");
            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = monthEnd,
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 確定保存,
                KakuteiYmd = monthEnd
            });

            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1001)
                .WithSyainId(syain.Id)
                .WithWorkYmd(D("20250115"))
                .WithInvalid(true)
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);

            long shinseiId = 2000;
            foreach (InquiryType inquiryType in Enum.GetValues<InquiryType>())
            {
                db.UkagaiShinseis.Add(new UkagaiShinseiBuilder()
                    .WithId(shinseiId++)
                    .WithUkagaiHeaderId(ukagaiHeader.Id)
                    .WithUkagaiSyubetsu(inquiryType)
                    .Build());
            }

            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            var result = await model.OnPostCancelConfirmAsync(syain.Id, monthEnd, false, syain.SyainBaseId);

            // 検証 (Assert)
            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.正常, GetResponseStatus(json), $"message={GetMessage(json)}");

            var updatedUkagai = await db.UkagaiHeaders.FirstOrDefaultAsync(x => x.Id == ukagaiHeader.Id);
            Assert.IsNotNull(updatedUkagai);
            Assert.IsFalse(updatedUkagai.Invalid, "月末の確定解除時は、対象の時間外労働時間制限拡張申請を再有効化するべきです。");
        }

        /// <summary>
        /// Given: 確定解除処理中に例外が発生する
        /// When: OnPostCancelConfirmAsync を実行する
        /// Then: 内部エラー応答が返る
        /// </summary>
        [TestMethod(DisplayName = "OnPostCancelConfirmAsync: 確定解除処理中に例外が発生した場合は内部エラー応答が返される")]
        public async Task OnPostCancelConfirmAsync_確定解除処理中に例外が発生した場合は内部エラー応答が返される()
        {
            // 準備 (Arrange)
            var loginUser = new SyainBuilder()
                .WithId(99)
                .WithSyainBaseId(99)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .Build();
            var model = CreateModel(loginUser);

            // 日報データを null にして休暇情報生成時に例外を発生させる
            model.NippouData = null!;

            // 実行 (Act)
            var result = await model.OnPostCancelConfirmAsync(1, D("20250120"), false, 1);

            // 検証 (Assert)
            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.エラー, GetResponseStatus(json));
            StringAssert.Contains(GetMessage(json) ?? string.Empty, "サーバー内部でエラー");
        }

        #endregion

        #region OnPostCancelConfirmAsync Tests

        /// <summary>
        /// Given: 確定済みの日報データ（当日確定）
        /// When: 確定解除を実行
        /// Then: 一時保存に戻される
        /// </summary>
        [TestMethod(DisplayName = "OnPostCancelConfirmAsync: 当日確定の日報を確定解除すると一時保存に戻される")]
        public async Task OnPostCancelConfirmAsync_当日確定の日報を確定解除すると一時保存に戻される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithCode("001")
                .WithName("テスト社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(5)
                .Build();
            db.Syains.Add(syain);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250101"),
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

            var jissekiDate = D("20250115");
            var today = DateTime.Today.ToDateOnly();
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 確定保存,
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
            var updatedNippou = await db.Nippous.FindAsync(nippou.Id);
            Assert.IsNotNull(updatedNippou, "日報データが存在するべきです。");
            Assert.AreEqual(一時保存, updatedNippou.TourokuKubun, "登録区分が一時保存に戻されるべきです。");
        }

        #endregion

        #region OnPostCopyFromLastDateAsync Tests

        /// <summary>
        /// Given: 過去日報は存在するが、勤務区分(IsSyukkin=true)の日報が存在しない
        /// When: 前回コピーを実行
        /// Then: コピー対象なしとしてdata=nullが返る
        /// </summary>
        [TestMethod(DisplayName = "OnPostCopyFromLastDateAsync: 勤務区分が非勤務の過去日報がある場合、コピー対象なしとしてdata=nullが返される")]
        public async Task OnPostCopyFromLastDateAsync_勤務区分が非勤務の過去日報がある場合コピー対象なしとしてdatanullが返される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .Build();
            db.Syains.Add(syain);

            AddSyukkinKubun(欠勤, isSyukkin: false, isVacation: false);

            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = D("20250114"),
                SyukkinKubunId1 = 20,
                SyukkinKubunId2 = 20,
                TourokuKubun = 確定保存
            });

            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            var result = await model.OnPostCopyFromLastDateAsync(
                syainId: syain.Id,
                jissekiDate: D("20250115")
            );

            // 検証 (Assert)
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result);
            var dataProp = jsonResult.Value?.GetType().GetProperty("Data")
                ?? jsonResult.Value?.GetType().GetProperty("data");
            Assert.IsNotNull(dataProp, "レスポンスにDataプロパティが存在するべきです。");
            Assert.IsNull(dataProp.GetValue(jsonResult.Value), "勤務区分日報がない場合はdata=nullを返すべきです。");
            Assert.IsEmpty(model.NippouAnkenCards.NippouAnkens, "案件カードは追加されないべきです。");
        }

        /// <summary>
        /// Given: 過去日報に勤務日報があり、案件に原価凍結あり/なしが混在する
        /// When: 前回コピーを実行
        /// Then: 勤務日報のうち原価凍結でない案件のみコピーされる
        /// </summary>
        [TestMethod(DisplayName = "OnPostCopyFromLastDateAsync: 過去日報に勤務日報があり、案件に原価凍結あり/なしが混在する場合、原価凍結でない案件のみコピーされる")]
        public async Task OnPostCopyFromLastDateAsync_過去日報に勤務日報があり案件に原価凍結ありなしが混在する場合原価凍結でない案件のみコピーされる()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithCode("001")
                .WithName("テスト社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .Build();
            db.Syains.Add(syain);

            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);
            AddSyukkinKubun(欠勤, isSyukkin: false, isVacation: false);

            // 最新日報は欠勤(非勤務)にして、FirstOrDefault条件で1つ前の勤務日報が選ばれることを担保する
            var latestNippou = new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = D("20250114"),
                SyukkinKubunId1 = 20,
                TourokuKubun = 確定保存
            };
            var previousWorkNippou = new Nippou
            {
                Id = 2,
                SyainId = syain.Id,
                NippouYmd = D("20250113"),
                SyukkinKubunId1 = 2,
                TourokuKubun = 確定保存
            };
            db.Nippous.AddRange(latestNippou, previousWorkNippou);

            var nonFrozenKings = new KingsJuchuBuilder()
                .WithId(1)
                .WithProjectNo("P0001")
                .WithJuchuuNo("100")
                .WithJuchuuGyoNo(1)
                .WithChaYmd(D("20250110"))
                .WithIsGenkaToketu(false)
                .Build();
            var frozenKings = new KingsJuchuBuilder()
                .WithId(2)
                .WithProjectNo("P0002")
                .WithJuchuuNo("200")
                .WithJuchuuGyoNo(1)
                .WithChaYmd(D("20250111"))
                .WithIsGenkaToketu(true)
                .Build();
            db.KingsJuchus.AddRange(nonFrozenKings, frozenKings);

            var nonFrozenAnken = new AnkenBuilder()
                .WithId(1)
                .WithName("非凍結案件")
                .WithSearchName("非凍結案件")
                .WithKingsJuchuId(nonFrozenKings.Id)
                .Build();
            var frozenAnken = new AnkenBuilder()
                .WithId(2)
                .WithName("凍結案件")
                .WithSearchName("凍結案件")
                .WithKingsJuchuId(frozenKings.Id)
                .Build();
            db.Ankens.AddRange(nonFrozenAnken, frozenAnken);

            db.NippouAnkens.AddRange(
                new NippouAnken
                {
                    Id = 1,
                    NippouId = previousWorkNippou.Id,
                    AnkensId = nonFrozenAnken.Id,
                    AnkenName = nonFrozenAnken.Name,
                    JissekiJikan = 240,
                    KokyakuKaisyaId = 1,
                    KokyakuName = "顧客A",
                    BumonProcessId = 10,
                    IsLinked = true
                },
                new NippouAnken
                {
                    Id = 2,
                    NippouId = previousWorkNippou.Id,
                    AnkensId = frozenAnken.Id,
                    AnkenName = frozenAnken.Name,
                    JissekiJikan = 240,
                    KokyakuKaisyaId = 2,
                    KokyakuName = "顧客B",
                    BumonProcessId = 20,
                    IsLinked = false
                },
                // 最新日報側の案件はコピー対象外
                new NippouAnken
                {
                    Id = 3,
                    NippouId = latestNippou.Id,
                    AnkensId = nonFrozenAnken.Id,
                    AnkenName = "最新日報案件",
                    JissekiJikan = 60,
                    KokyakuKaisyaId = 3,
                    KokyakuName = "顧客C",
                    BumonProcessId = 30,
                    IsLinked = false
                }
            );

            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            var result = await model.OnPostCopyFromLastDateAsync(
                syainId: syain.Id,
                jissekiDate: D("20250115")
            );

            // 検証 (Assert)
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.IsNotNull(jsonResult.Value, "結果が返されるべきです。");

            Assert.HasCount(1, model.NippouAnkenCards.NippouAnkens, "原価凍結案件は除外されるべきです。");
            var copied = model.NippouAnkenCards.NippouAnkens[0];
            Assert.AreEqual(nonFrozenAnken.Id, copied.AnkensId, "非凍結案件がコピーされるべきです。");
            Assert.AreEqual(nonFrozenAnken.Name, copied.AnkenName, "案件名が一致するべきです。");
            Assert.AreEqual(nonFrozenKings.KingsJuchuNo, copied.KingsJuchuNo, "受注番号が一致するべきです。");
            Assert.AreEqual(nonFrozenKings.ChaYmd, copied.ChaYmd, "着工日が一致するべきです。");
            Assert.AreEqual(nonFrozenKings.JuchuuNo, copied.JuchuuNo, "受注Noが一致するべきです。");
            Assert.AreEqual(1L, copied.KokyakuKaisyaId, "顧客会社IDが一致するべきです。");
            Assert.AreEqual("顧客A", copied.KokyakuName, "顧客名が一致するべきです。");
            Assert.AreEqual(10L, copied.BumonProcessId, "部門プロセスIDが一致するべきです。");
            Assert.IsTrue(copied.IsLinked, "連動フラグが一致するべきです。");
            Assert.IsFalse(
                model.NippouAnkenCards.NippouAnkens.Any(x => x.AnkensId == frozenAnken.Id),
                "凍結案件はコピーされないべきです。");
            Assert.IsFalse(
                model.NippouAnkenCards.NippouAnkens.Any(x => x.AnkenName == "最新日報案件"),
                "選択されなかった日報の案件はコピーされないべきです。");
        }

        #endregion

        #region ViewModel Tests

        /// <summary>
        /// Given: NippouViewModelに出退勤時刻を設定
        /// When: TotalWorkingHoursInMinuteを取得
        /// Then: 正しい労働時間（分）が計算される
        /// </summary>
        [TestMethod(DisplayName = "NippouViewModel: 出退勤時刻を設定した場合、労働時間（分）が正しく計算される")]
        public void NippouViewModel_出退勤時刻を設定した場合労働時間分が正しく計算される()
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
            Assert.IsGreaterThan(0, totalMinutes, "労働時間(分)が正であるべきです。");
        }

        /// <summary>
        /// Given: NippouViewModel の条件を満たしている
        /// When: FurikyuYoteiDateIsSet
        /// Then: ValueIsReturned
        /// </summary>
        [TestMethod(DisplayName = "NippouViewModel: 期日を設定した場合、期日が正しく返される")]
        public void NippouViewModel_期日を設定した場合期日が正しく返される()
        {
            var vm = new IndexModel.NippouViewModel
            {
                FurikyuYoteiDate = D("20250121")
            };

            Assert.AreEqual(D("20250121"), vm.FurikyuYoteiDate);
        }

        /// <summary>
        /// Given: NippouViewModel の条件を満たしている
        /// When: TotalWorkingHoursCalculated
        /// Then: FormattedAsH mm
        /// </summary>
        [TestMethod(DisplayName = "NippouViewModel: 労働時間計算後、H:mm形式でフォーマットされる")]
        public void NippouViewModel_労働時間計算後Hmm形式でフォーマットされる()
        {
            var vm = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(10, 0)
            };

            Assert.AreEqual("1:00", vm.TotalWorkingHours);
        }

        /// <summary>
        /// Given: NippouViewModel の条件を満たしている
        /// When: TotalWorkingHoursIsZero
        /// Then: FormattedAndFurikyuYoteiDateAccessible
        /// </summary>
        [TestMethod(DisplayName = "NippouViewModel: 労働時間0の場合、H:mm形式でフォーマットされ、期日がアクセス可能")]
        public void NippouViewModel_労働時間0の場合Hmm形式でフォーマットされ期日がアクセス可能()
        {
            var vm = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = null,
                TaisyutsuHm1 = null,
                SyukkinHm2 = null,
                TaisyutsuHm2 = null,
                SyukkinHm3 = null,
                TaisyutsuHm3 = null,
                FurikyuYoteiDate = D("20250201")
            };

            Assert.AreEqual("0:00", vm.TotalWorkingHours);
            Assert.AreEqual(D("20250201"), vm.FurikyuYoteiDate);
        }

        /// <summary>
        /// Given: NippouViewModelに出勤区分コードを設定
        /// When: SyukkinKubun1プロパティを取得
        /// Then: 正しい列挙値が返される
        /// </summary>
        [TestMethod(DisplayName = "NippouViewModel: 出勤区分コードを設定した場合、出勤区分列挙値が正しく変換される")]
        public void NippouViewModel_出勤区分コードを設定した場合出勤区分列挙値が正しく変換される()
        {
            // 準備 (Arrange)
            var viewModel = new IndexModel.NippouViewModel
            {
                SyukkinKubunCodeString1 = "02"
            };

            // 実行 (Act)
            var kubun = viewModel.SyukkinKubun1;

            // 検証 (Assert)
            Assert.AreEqual(通常勤務, kubun, "出勤区分が正しく変換されるべきです。");
        }

        /// <summary>
        /// Given: NippouAnkenエンティティ
        /// When: FromEntityメソッドでViewModelに変換
        /// Then: データが正しくマッピングされる
        /// </summary>
        [TestMethod(DisplayName = "NippouAnkenViewModel: FromEntityで変換した場合、データが正しくマッピングされる")]
        public void NippouAnkenViewModel_FromEntityで変換した場合データが正しくマッピングされる()
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
                    ChaYmd = D("20250101"),
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

        /// <summary>
        /// Given: UkagaiHeadersViewModel の条件を満たしている
        /// When: UkagaiSyubetsuIsEmpty
        /// Then: ShinseiNaiyouIsEmpty
        /// </summary>
        [TestMethod(DisplayName = "UkagaiHeadersViewModel: UkagaiSyubetsuが空の場合、ShinseiNaiyouは空文字列になる")]
        public void UkagaiHeadersViewModel_UkagaiSyubetsuが空の場合ShinseiNaiyouは空文字列になる()
        {
            var viewModel = new IndexModel.UkagaiHeadersViewModel
            {
                UkagaiSyubetsu = new List<InquiryType>()
            };

            Assert.AreEqual(string.Empty, viewModel.ShinseiNaiyou);
        }

        /// <summary>
        /// Given: UkagaiHeadersViewModel の条件を満たしている
        /// When: FromEntityIsCalled
        /// Then: MappedCorrectly
        /// </summary>
        [TestMethod(DisplayName = "UkagaiHeadersViewModel: FromEntityが呼び出された場合、データが正しくマッピングされる")]
        public void UkagaiHeadersViewModel_FromEntityが呼び出された場合データが正しくマッピングされる()
        {
            var inquiryType1 = 夜間作業;
            var inquiryType2 = 早朝作業;

            var entity = new UkagaiHeader
            {
                Id = 100,
                SyainId = 200,
                ShinseiYmd = D("20250110"),
                ShoninSyainId = 300,
                ShoninYmd = D("20250111"),
                LastShoninSyainId = 301,
                LastShoninYmd = D("20250112"),
                Status = default,
                WorkYmd = D("20250115"),
                KaishiJikoku = new TimeOnly(8, 30),
                SyuryoJikoku = new TimeOnly(17, 45),
                Biko = "memo",
                Invalid = false,
                UkagaiShinseis = new List<UkagaiShinsei>
                {
                    new() { Id = 1, UkagaiHeaderId = 100, UkagaiSyubetsu = inquiryType1 },
                    new() { Id = 2, UkagaiHeaderId = 100, UkagaiSyubetsu = inquiryType2 }
                }
            };

            var viewModel = IndexModel.UkagaiHeadersViewModel.FromEntity(entity);

            Assert.AreEqual(entity.Id, viewModel.Id);
            Assert.AreEqual(entity.SyainId, viewModel.SyainId);
            Assert.AreEqual(entity.ShinseiYmd, viewModel.ShinseiYmd);
            Assert.AreEqual(entity.ShoninSyainId, viewModel.ShoninSyainId);
            Assert.AreEqual(entity.ShoninYmd, viewModel.ShoninYmd);
            Assert.AreEqual(entity.LastShoninSyainId, viewModel.LastShoninSyainId);
            Assert.AreEqual(entity.LastShoninYmd, viewModel.LastShoninYmd);
            Assert.AreEqual(entity.Status, viewModel.Status);
            Assert.AreEqual(entity.WorkYmd, viewModel.WorkYmd);
            Assert.AreEqual(entity.KaishiJikoku, viewModel.KaishiJikoku);
            Assert.AreEqual(entity.SyuryoJikoku, viewModel.SyuryoJikoku);
            Assert.AreEqual(entity.Biko, viewModel.Biko);
            Assert.AreEqual(entity.Invalid, viewModel.Invalid);

            var expectedTypes = new List<InquiryType> { inquiryType1, inquiryType2 };
            CollectionAssert.AreEqual(expectedTypes, viewModel.UkagaiSyubetsu);

            var expectedShinseiNaiyou = string.Join("・", expectedTypes.Select(x => x.GetDisplayName()));
            Assert.AreEqual(expectedShinseiNaiyou, viewModel.ShinseiNaiyou);
        }

        #endregion

        #region Boundary Tests

        /// <summary>
        /// Given: 出退勤時刻が未入力
        /// When: 一時保存を実行
        /// Then: 出退勤時刻がnullで保存される
        /// </summary>
        [TestMethod(DisplayName = "OnPostTemporarySaveAsync: 出退勤時刻が未入力の場合、出退勤時刻がnullで保存される")]
        public async Task OnPostTemporarySaveAsync_出退勤時刻が未入力の場合出退勤時刻がnullで保存される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(5)
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
                NippoStopDate = D("20250101"),
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
            model.JissekiDate = D("20250115");
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = null,
                TaisyutsuHm1 = null,
                SyukkinKubunCodeString1 = "01",
                SyukkinKubun1 = 休日
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel();

            // 実行 (Act)
            var result = await model.OnPostTemporarySaveAsync(
                syainId: syain.Id,
                jissekiDate: D("20250112"),
                isDairiInput: false
            );

            // 検証 (Assert)
            var savedNippou = await db.Nippous.FirstAsync();
            Assert.IsNull(savedNippou.SyukkinHm1, "出勤時刻1がnullであるべきです。");
            Assert.IsNull(savedNippou.TaisyutsuHm1, "退出時刻1がnullであるべきです。");
        }

        /// <summary>
        /// Given: 3回分の出退勤時刻を設定
        /// When: 一時保存を実行
        /// Then: 全ての出退勤時刻が正しく保存される
        /// </summary>
        [TestMethod(DisplayName = "OnPostTemporarySaveAsync: 3回分の出退勤時刻を設定した場合、全ての出退勤時刻が正しく保存される")]
        public async Task OnPostTemporarySaveAsync_3回分の出退勤時刻を設定した場合全ての出退勤時刻が正しく保存される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(5)
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
                NippoStopDate = D("20250101"),
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
            model.JissekiDate = D("20250115");
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(12, 0),
                SyukkinHm2 = new TimeOnly(13, 0),
                TaisyutsuHm2 = new TimeOnly(18, 0),
                SyukkinHm3 = new TimeOnly(19, 0),
                TaisyutsuHm3 = new TimeOnly(22, 0),
                SyukkinKubunCodeString1 = "02",
                SyukkinKubun1 = 通常勤務
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel();

            // 実行 (Act)
            var result = await model.OnPostTemporarySaveAsync(
                syainId: syain.Id,
                jissekiDate: D("20250115"),
                isDairiInput: false
            );

            // 検証 (Assert)
            var savedNippou = await db.Nippous.FirstAsync();
            Assert.AreEqual(new TimeOnly(9, 0), savedNippou.SyukkinHm1, "出勤時刻1が一致しません。");
            Assert.AreEqual(new TimeOnly(12, 0), savedNippou.TaisyutsuHm1, "退出時刻1が一致しません。");
            Assert.AreEqual(new TimeOnly(13, 0), savedNippou.SyukkinHm2, "出勤時刻2が一致しません。");
            Assert.AreEqual(new TimeOnly(18, 0), savedNippou.TaisyutsuHm2, "退出時刻2が一致しません。");
            Assert.AreEqual(new TimeOnly(19, 0), savedNippou.SyukkinHm3, "出勤時刻3が一致しません。");
            Assert.AreEqual(new TimeOnly(22, 0), savedNippou.TaisyutsuHm3, "退出時刻3が一致しません。");
        }

        #endregion

        #region OnGetAsync Notification & Button Tests

        /// <summary>
        /// Given: 日報停止日が実績日以前
        /// When: 初期表示
        /// Then: ConfirmButtonがFalseで停止メッセージが表示される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 日報停止日が実績日以前の場合、ConfirmButtonがFalseで停止メッセージが表示される")]
        public async Task OnGetAsync_日報停止日が実績日以前の場合ConfirmButtonがFalseで停止メッセージが表示される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            // 停止日を実績日以前に設定
            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250110"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            };
            db.ApplicationConfigs.Add(appConfig);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            await model.OnGetAsync(syainBase.Id, D("20250115"), false,
                null, null, null, null, null, null);

            // 検証 (Assert)
            Assert.IsFalse(model.ConfirmButton, "確定ボタンが非活性であるべきです。");
            Assert.IsNotNull(model.MessageString, "メッセージが設定されるべきです。");
            StringAssert.Contains(model.MessageString, "確定確定を停止", "停止メッセージを含むべきです。");
        }

        /// <summary>
        /// Given: 出勤時刻のみ設定、退出時刻なし
        /// When: 初期表示
        /// Then: 打刻漏れメッセージが表示される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 出勤時刻のみ設定、退出時刻なしの場合、打刻漏れメッセージが表示される")]
        public async Task OnGetAsync_出勤時刻のみ設定退出時刻なしの場合打刻漏れメッセージが表示される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1).WithCode("02").WithName("通常勤務").Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            // 出勤のみ設定、退出なしの日報
            var jissekiDate = D("20250115");
            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = null,
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 一時保存
            });

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20990101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            };
            db.ApplicationConfigs.Add(appConfig);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            await model.OnGetAsync(syainBase.Id, jissekiDate, false,
                null, null, null, null, null, null);

            // 検証 (Assert)
            Assert.IsFalse(model.ConfirmButton, "確定ボタンが非活性であるべきです。");
            Assert.IsNotNull(model.MessageString, "メッセージが設定されるべきです。");
            StringAssert.Contains(model.MessageString, "打刻漏れ", "打刻漏れメッセージを含むべきです。");
        }

        /// <summary>
        /// Given: 確定保存済みの日報データ
        /// When: 初期表示
        /// Then: ConfirmButtonがFalseになる
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 確定保存済みの日報データの場合、ConfirmButtonがFalseになる")]
        public async Task OnGetAsync_確定保存済みの日報データの場合ConfirmButtonがFalseになる()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1).WithCode("02").WithName("通常勤務").Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("20250115");
            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 確定保存,
                KakuteiYmd = jissekiDate
            });
            // 前日確定データ（CheckConfirmButtonAsync用）
            db.Nippous.Add(new Nippou
            {
                Id = 2,
                SyainId = syain.Id,
                NippouYmd = jissekiDate.AddDays(-1),
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 確定保存,
                KakuteiYmd = jissekiDate.AddDays(-1)
            });

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            };
            db.ApplicationConfigs.Add(appConfig);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            await model.OnGetAsync(syainBase.Id, jissekiDate, false,
                null, null, null, null, null, null);

            // 検証 (Assert)
            Assert.IsFalse(model.ConfirmButton, "確定済みなので確定ボタンは非活性であるべきです。");
        }

        /// <summary>
        /// Given: 前日の確定データが存在しない
        /// When: 初期表示
        /// Then: ConfirmButtonがFalseになる
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 前日の確定データが存在しない場合、ConfirmButtonがFalseになる")]
        public async Task OnGetAsync_前日の確定データが存在しない場合ConfirmButtonがFalseになる()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            // 前日の確定データなし
            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            };
            db.ApplicationConfigs.Add(appConfig);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            await model.OnGetAsync(syainBase.Id, D("20250115"), false,
                null, null, null, null, null, null);

            // 検証 (Assert)
            Assert.IsFalse(model.ConfirmButton, "前日確定なしなので確定ボタンは非活性であるべきです。");
        }

        /// <summary>
        /// Given: 一時保存のデータ
        /// When: 初期表示
        /// Then: UnconfirmButtonがFalseになる
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 一時保存のデータの場合、UnconfirmButtonがFalseになる")]
        public async Task OnGetAsync_一時保存のデータの場合UnconfirmButtonがFalseになる()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1).WithCode("02").WithName("通常勤務").Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("20250115");
            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 一時保存
            });

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            };
            db.ApplicationConfigs.Add(appConfig);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            await model.OnGetAsync(syainBase.Id, jissekiDate, false,
                null, null, null, null, null, null);

            // 検証 (Assert)
            Assert.IsFalse(model.UnconfirmButton, "一時保存なので確定解除ボタンは非活性であるべきです。");
        }

        /// <summary>
        /// Given: 確定保存済みのデータ
        /// When: 初期表示
        /// Then: TemporarySaveButtonがFalseになる
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 確定保存済みのデータの場合、TemporarySaveButtonがFalseになる")]
        public async Task OnGetAsync_確定保存済みのデータの場合TemporarySaveButtonがFalseになる()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1).WithCode("02").WithName("通常勤務").Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("20250115");
            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 確定保存,
                KakuteiYmd = jissekiDate
            });

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            };
            db.ApplicationConfigs.Add(appConfig);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            await model.OnGetAsync(syainBase.Id, jissekiDate, false,
                null, null, null, null, null, null);

            // 検証 (Assert)
            Assert.IsFalse(model.TemporarySaveButton, "確定済みなので一時保存ボタンは非活性であるべきです。");
        }

        /// <summary>
        /// Given: 非稼働日（土曜）で勤務時間なし、日報なし
        /// When: 初期表示
        /// Then: SyukkinKubun1が休日に設定される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 非稼働日（土曜）で勤務時間なし、日報なしの場合、勤務区分1が休日に設定される")]
        public async Task OnGetAsync_非稼働日土曜で勤務時間なし日報なしの場合勤務区分1が休日に設定される()
        {
            // 準備 (Arrange) - 2025/01/18は土曜日
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            };
            db.ApplicationConfigs.Add(appConfig);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            var saturdayDate = D("20250118"); // 土曜日

            // 実行 (Act)
            await model.OnGetAsync(syainBase.Id, saturdayDate, false,
                null, null, null, null, null, null);

            // 検証 (Assert)
            Assert.AreEqual(
                休日,
                model.NippouData.SyukkinKubun1,
                "非稼働日は休日が設定されるべきです。");
        }

        /// <summary>
        /// Given: 代理入力モード、出退勤時刻パラメータあり
        /// When: 初期表示
        /// Then: パラメータの出退勤が正しくNippouDataに反映される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 代理入力モードで出退勤時刻パラメータがある場合、NippouDataに正しく反映される")]
        public async Task OnGetAsync_代理入力モードで出退勤時刻パラメータがある場合NippouDataに正しく反映される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            };
            db.ApplicationConfigs.Add(appConfig);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            await model.OnGetAsync(syainBase.Id, D("20250115"), true,
                new TimeOnly(8, 30), new TimeOnly(17, 30),
                null, null, null, null);

            // 検証 (Assert)
            Assert.AreEqual(new TimeOnly(8, 30), model.NippouData.SyukkinHm1,
                "代理入力の出勤時刻1が反映されるべきです。");
            Assert.AreEqual(new TimeOnly(17, 30), model.NippouData.TaisyutsuHm1,
                "代理入力の退出時刻1が反映されるべきです。");
        }

        /// <summary>
        /// Given: 指示情報が存在し、本人以外が生理休暇を含む日報を表示する
        /// When: 初期表示
        /// Then: その他特別休暇判定クエリ（row.Code参照）が実行され、InMemory環境では変換例外となる
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 指示情報が存在し、本人以外が生理休暇を含む日報を表示する場合、その他特別休暇判定クエリが実行されInMemory環境では変換例外となる")]
        public async Task OnGetAsync_指示情報が存在し本人以外が生理休暇を含む日報を表示する場合その他特別休暇判定クエリが実行されInMemory環境では変換例外となる()
        {
            var targetBase = new SyainBasisBuilder().WithId(1).Build();
            var loginBase = new SyainBasisBuilder().WithId(2).Build();
            db.SyainBases.AddRange(targetBase, loginBase);

            db.KintaiZokuseis.Add(new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            });

            var target = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(targetBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(5)
                .Build();
            db.Syains.Add(target);

            var loginUser = new SyainBuilder()
                .WithId(99)
                .WithSyainBaseId(loginBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(5)
                .Build();
            db.Syains.Add(loginUser);

            AddSyukkinKubun(生理休暇, isSyukkin: false, isVacation: true);
            AddSyukkinKubun(その他特別休暇, isSyukkin: false, isVacation: true);

            var jissekiDate = D("20250115");
            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = target.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = 13,
                SyukkinKubunId2 = 13,
                TourokuKubun = 一時保存
            });

            db.UkagaiHeaders.Add(new UkagaiHeader
            {
                Id = 1,
                SyainId = target.Id,
                WorkYmd = jissekiDate,
                ShinseiYmd = jissekiDate,
                Status = 承認待,
                Invalid = false
            });

            db.ApplicationConfigs.Add(new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            });

            await db.SaveChangesAsync();

            var model = CreateModel(loginUser);

            // 実行 (Act)
            var exception = Assert.Throws<InvalidOperationException>(
                () => model.OnGetAsync(
                        syainBaseId: targetBase.Id,
                        jissekiDate: jissekiDate,
                        isDairiInput: false,
                        syukkinHm1: null,
                        taisyutsuHm1: null,
                        syukkinHm2: null,
                        taisyutsuHm2: null,
                        syukkinHm3: null,
                        taisyutsuHm3: null)
                    .GetAwaiter()
                    .GetResult());

            // 検証 (Assert)
            StringAssert.Contains(
                exception.Message,
                "could not be translated",
                "InMemory 環境では unmapped メンバー参照クエリが例外になるべきです。");
        }

        /// <summary>
        /// Given: 確定保存済み日報
        /// When: 初期表示
        /// Then: DisableAllInputがtrueに設定される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 確定保存済み日報の場合、DisableAllInputがtrueに設定される")]
        public async Task OnGetAsync_確定保存済み日報の場合DisableAllInputがtrueに設定される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1).WithCode("02").WithName("通常勤務").Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("20250115");
            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 確定保存,
                KakuteiYmd = jissekiDate
            });

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            };
            db.ApplicationConfigs.Add(appConfig);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            await model.OnGetAsync(syainBase.Id, jissekiDate, false,
                null, null, null, null, null, null);

            // 検証 (Assert)
            Assert.IsTrue((bool?)model.ViewData["DisableAllInput"],
                "確定済みの場合は入力が無効であるべきです。");
        }

        /// <summary>
        /// Given: 一時保存の日報
        /// When: 初期表示
        /// Then: DisableAllInputがfalseに設定される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 一時保存の日報の場合、DisableAllInputがfalseに設定される")]
        public async Task OnGetAsync_一時保存の日報の場合DisableAllInputがfalseに設定される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1).WithCode("02").WithName("通常勤務").Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("20250115");
            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 一時保存
            });

            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20250101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            };
            db.ApplicationConfigs.Add(appConfig);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            await model.OnGetAsync(syainBase.Id, jissekiDate, false,
                null, null, null, null, null, null);

            // 検証 (Assert)
            Assert.IsFalse((bool?)model.ViewData["DisableAllInput"],
                "一時保存の場合は入力が有効であるべきです。");
        }

        /// <summary>
        /// Given: OnGetAsync の条件を満たしている
        /// When: DifferentLoginUserAndNotProxy
        /// Then: DisableAllInputIsTrue
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: ログインユーザーと日報作成者が異なる場合、DisableAllInputがtrueに設定される")]
        public async Task OnGetAsync_ログインユーザーと日報作成者が異なる場合DisableAllInputがtrueに設定される()
        {
            var jissekiDate = D("20250115");

            var targetBase = new SyainBasisBuilder().WithId(1).Build();
            var loginBase = new SyainBasisBuilder().WithId(2).Build();
            db.SyainBases.AddRange(targetBase, loginBase);

            db.KintaiZokuseis.Add(new KintaiZokusei
            {
                Id = 4,
                Code = 管理,
                Name = "kintai"
            });

            var target = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(targetBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(4)
                .Build();
            var loginUser = new SyainBuilder()
                .WithId(99)
                .WithSyainBaseId(loginBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(4)
                .Build();
            db.Syains.AddRange(target, loginUser);

            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);
            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = target.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = 2,
                TourokuKubun = 一時保存
            });

            db.ApplicationConfigs.Add(new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20990101"),
                MsClientId = "id",
                MsClientSecret = "secret",
                MsTenantId = "tenant",
                SmtpUser = "user",
                SmtpPassword = "pass"
            });
            await db.SaveChangesAsync();

            var model = CreateModel(loginUser);

            await model.OnGetAsync(
                syainBaseId: targetBase.Id,
                jissekiDate: jissekiDate,
                isDairiInput: false,
                syukkinHm1: null,
                taisyutsuHm1: null,
                syukkinHm2: null,
                taisyutsuHm2: null,
                syukkinHm3: null,
                taisyutsuHm3: null);

            Assert.IsTrue((bool?)model.ViewData["DisableAllInput"]);
        }

        /// <summary>
        /// Given: OnGetAsync の条件を満たしている
        /// When: UkagaiAndNippouExist
        /// Then: KubunAnkenAndUkagaiAreMapped
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 有休申請と日報が存在する場合、勤務区分と案件が正しくマッピングされる")]
        public async Task OnGetAsync_有休申請と日報が存在する場合勤務区分と案件が正しくマッピングされる()
        {
            var jissekiDate = D("20250115");

            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);
            db.KintaiZokuseis.Add(new KintaiZokusei
            {
                Id = 4,
                Code = 管理,
                Name = "kintai"
            });

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(4)
                .Build();
            db.Syains.Add(syain);

            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);
            AddSyukkinKubun(半日有給, isSyukkin: false, isVacation: true);

            var nippou = new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = 2,
                SyukkinKubunId2 = 5,
                TourokuKubun = 一時保存
            };
            db.Nippous.Add(nippou);

            var ukagai = new UkagaiHeader
            {
                Id = 10,
                SyainId = syain.Id,
                ShinseiYmd = jissekiDate,
                WorkYmd = jissekiDate,
                Status = 承認待,
                Invalid = false,
                UkagaiShinseis = new List<UkagaiShinsei>
                {
                    new UkagaiShinsei { Id = 11, UkagaiHeaderId = 10, UkagaiSyubetsu = 夜間作業 }
                }
            };
            db.UkagaiHeaders.Add(ukagai);

            var kings = new KingsJuchuBuilder()
                .WithId(20)
                .WithJucKn("J00020")
                .WithJuchuuNo("00020")
                .WithChaYmd(jissekiDate)
                .WithIsGenkaToketu(false)
                .Build();
            db.KingsJuchus.Add(kings);

            var anken = new AnkenBuilder()
                .WithId(21)
                .WithName("anken")
                .WithSearchName("anken")
                .WithKingsJuchuId(kings.Id)
                .Build();
            db.Ankens.Add(anken);

            db.NippouAnkens.Add(new NippouAnken
            {
                Id = 22,
                NippouId = nippou.Id,
                AnkensId = anken.Id,
                KokyakuName = "kokyaku",
                AnkenName = "anken",
                JissekiJikan = 120,
                KokyakuKaisyaId = 1,
                BumonProcessId = null,
                IsLinked = true
            });

            db.ApplicationConfigs.Add(new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20990101"),
                MsClientId = "id",
                MsClientSecret = "secret",
                MsTenantId = "tenant",
                SmtpUser = "user",
                SmtpPassword = "pass"
            });
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            await model.OnGetAsync(
                syainBaseId: syainBase.Id,
                jissekiDate: jissekiDate,
                isDairiInput: false,
                syukkinHm1: null,
                taisyutsuHm1: null,
                syukkinHm2: null,
                taisyutsuHm2: null,
                syukkinHm3: null,
                taisyutsuHm3: null);

            Assert.AreEqual(通常勤務, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(半日有給, model.NippouData.SyukkinKubun2);
            Assert.HasCount(1, model.NippouAnkenCards.NippouAnkens);
            Assert.AreEqual(anken.Id, model.NippouAnkenCards.NippouAnkens[0].AnkensId);
            Assert.IsNotNull(model.UkagaiHeadersData);
            Assert.AreEqual(ukagai.Id, model.UkagaiHeadersData.Id);
            Assert.HasCount(1, model.UkagaiHeadersData.UkagaiSyubetsu);
        }

        /// <summary>
        /// Given: OnGetAsync の条件を満たしている
        /// When: UkagaiListIsEmptyButWorkingHourReferencesUkagai
        /// Then: UkagaiHeadersDataIsSet
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 有休申請リストが空だが、勤務時間に有休申請が関連付けられている場合、UkagaiHeadersDataが設定される")]
        public async Task OnGetAsync_有休申請リストが空だが勤務時間に有休申請が関連付けられている場合UkagaiHeadersDataが設定される()
        {
            var jissekiDate = D("20250115");

            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);
            db.KintaiZokuseis.Add(new KintaiZokusei
            {
                Id = 4,
                Code = 管理,
                Name = "kintai"
            });

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(4)
                .Build();
            db.Syains.Add(syain);

            var linkedUkagai = new UkagaiHeader
            {
                Id = 30,
                SyainId = 999,
                ShinseiYmd = jissekiDate,
                WorkYmd = jissekiDate,
                Status = 承認待,
                Invalid = false,
                UkagaiShinseis = new List<UkagaiShinsei>
                {
                    new UkagaiShinsei { Id = 31, UkagaiHeaderId = 30, UkagaiSyubetsu = 早朝作業 }
                }
            };
            db.UkagaiHeaders.Add(linkedUkagai);

            db.WorkingHours.Add(new WorkingHour
            {
                Id = 40,
                SyainId = syain.Id,
                Hiduke = jissekiDate,
                SyukkinTime = jissekiDate.ToDateTime(new TimeOnly(9, 0)),
                TaikinTime = jissekiDate.ToDateTime(new TimeOnly(18, 0)),
                UkagaiHeaderId = linkedUkagai.Id,
                Deleted = false
            });

            db.ApplicationConfigs.Add(new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20990101"),
                MsClientId = "id",
                MsClientSecret = "secret",
                MsTenantId = "tenant",
                SmtpUser = "user",
                SmtpPassword = "pass"
            });
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            await model.OnGetAsync(
                syainBaseId: syainBase.Id,
                jissekiDate: jissekiDate,
                isDairiInput: false,
                syukkinHm1: null,
                taisyutsuHm1: null,
                syukkinHm2: null,
                taisyutsuHm2: null,
                syukkinHm3: null,
                taisyutsuHm3: null);

            Assert.IsNotNull(model.UkagaiHeadersData);
            Assert.AreEqual(linkedUkagai.Id, model.UkagaiHeadersData.Id);
            Assert.AreEqual(linkedUkagai.WorkYmd, model.UkagaiHeadersData.WorkYmd);
        }

        /// <summary>
        /// Given: OnGetAsync の条件を満たしている
        /// When: OtherUserAndKubun2IsPhysiological
        /// Then: ConversionQueryForKubun2Throws
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 他のユーザーかつ勤務区分2が生理休暇の場合、勤務区分2の変換クエリが例外を投げる")]
        public async Task OnGetAsync_他のユーザーかつ勤務区分2が生理休暇の場合勤務区分2の変換クエリが例外を投げる()
        {
            var jissekiDate = D("20250115");

            var targetBase = new SyainBasisBuilder().WithId(1).Build();
            var loginBase = new SyainBasisBuilder().WithId(2).Build();
            db.SyainBases.AddRange(targetBase, loginBase);

            db.KintaiZokuseis.Add(new KintaiZokusei
            {
                Id = 4,
                Code = 管理,
                Name = "kintai"
            });

            var target = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(targetBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(4)
                .Build();
            var loginUser = new SyainBuilder()
                .WithId(99)
                .WithSyainBaseId(loginBase.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(4)
                .Build();
            db.Syains.AddRange(target, loginUser);

            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);
            AddSyukkinKubun(生理休暇, isSyukkin: false, isVacation: true);
            AddSyukkinKubun(その他特別休暇, isSyukkin: false, isVacation: true);

            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = target.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = 2,
                SyukkinKubunId2 = 13,
                TourokuKubun = 一時保存
            });

            db.UkagaiHeaders.Add(new UkagaiHeader
            {
                Id = 1,
                SyainId = target.Id,
                ShinseiYmd = jissekiDate,
                WorkYmd = jissekiDate,
                Status = 承認待,
                Invalid = false
            });

            db.ApplicationConfigs.Add(new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20990101"),
                MsClientId = "id",
                MsClientSecret = "secret",
                MsTenantId = "tenant",
                SmtpUser = "user",
                SmtpPassword = "pass"
            });
            await db.SaveChangesAsync();

            var model = CreateModel(loginUser);

            // 実行 (Act)
            var exception = Assert.Throws<InvalidOperationException>(
                () => model.OnGetAsync(
                        syainBaseId: targetBase.Id,
                        jissekiDate: jissekiDate,
                        isDairiInput: false,
                        syukkinHm1: null,
                        taisyutsuHm1: null,
                        syukkinHm2: null,
                        taisyutsuHm2: null,
                        syukkinHm3: null,
                        taisyutsuHm3: null)
                    .GetAwaiter()
                    .GetResult());

            // 検証 (Assert)
            StringAssert.Contains(exception.Message, "could not be translated");
        }

        #endregion

        #region Private Method Branch Tests

        /// <summary>
        /// Given: CheckConfirmButtonAsync の条件を満たしている
        /// When: NotProxyAndLoginBaseMismatch
        /// Then: ConfirmButtonIsFalse
        /// </summary>
        [TestMethod(DisplayName = "CheckConfirmButtonAsync: 代理入力でなく、ログインユーザーが日報作成者と異なる場合、" +
            "ConfirmButtonがfalseになる")]
        public async Task CheckConfirmButtonAsync_代理入力でなくログインユーザーが日報作成者と異なる場合ConfirmButtonがfalseになる()
        {
            var jissekiDate = D("20250120");
            var target = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .Build();
            var loginUser = new SyainBuilder()
                .WithId(99)
                .WithSyainBaseId(99)
                .Build();

            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = target.Id,
                NippouYmd = jissekiDate.AddDays(-1),
                TourokuKubun = 確定保存
            });
            await db.SaveChangesAsync();

            var model = CreateModel(loginUser);
            model.ConfirmButton = true;
            var current = new Nippou
            {
                Id = 2,
                SyainId = target.Id,
                NippouYmd = jissekiDate,
                TourokuKubun = 一時保存
            };

            await InvokePrivateAsync(
                model,
                "CheckConfirmButtonAsync",
                current,
                99L,
                target,
                jissekiDate,
                false);

            Assert.IsFalse(model.ConfirmButton);
        }

        /// <summary>
        /// Given: CheckUnconfirmButtonAsync の条件を満たしている
        /// When: NotProxyAndLoginBaseMismatch
        /// Then: UnconfirmButtonIsFalse
        /// </summary>
        [TestMethod(DisplayName = "CheckUnconfirmButtonAsync: 代理入力でなく、ログインユーザーが日報作成者と異なる場合、" +
            "UnconfirmButtonがfalseになる")]
        public async Task CheckUnconfirmButtonAsync_代理入力でなくログインユーザーが日報作成者と異なる場合UnconfirmButtonがfalseになる()
        {
            var loginUser = new SyainBuilder()
                .WithId(99)
                .WithSyainBaseId(99)
                .Build();
            var model = CreateModel(loginUser);
            model.UnconfirmButton = true;

            var nippou = new Nippou
            {
                Id = 1,
                SyainId = 1,
                NippouYmd = D("20250120"),
                TourokuKubun = 確定保存,
                KakuteiYmd = DateTime.Today.ToDateOnly()
            };

            await InvokePrivateAsync(
                model,
                "CheckUnconfirmButtonAsync",
                nippou,
                D("20250120"),
                99L,
                1L,
                false);

            Assert.IsFalse(model.UnconfirmButton);
        }

        /// <summary>
        /// Given: CheckTemporarySaveButtonAsync の条件を満たしている
        /// When: NotProxyAndLoginBaseMismatch
        /// Then: TemporarySaveButtonIsFalse
        /// </summary>
        [TestMethod(DisplayName = "CheckTemporarySaveButtonAsync: 代理入力でなく、ログインユーザーが日報作成者と異なる場合、" +
            "TemporarySaveButtonがfalseになる")]
        public async Task CheckTemporarySaveButtonAsync_代理入力でなくログインユーザーが日報作成者と異なる場合TemporarySaveButtonがfalseになる()
        {
            var loginUser = new SyainBuilder()
                .WithId(99)
                .WithSyainBaseId(99)
                .Build();
            var model = CreateModel(loginUser);
            model.TemporarySaveButton = true;

            var nippou = new Nippou
            {
                Id = 1,
                SyainId = 1,
                NippouYmd = D("20250120"),
                TourokuKubun = 一時保存
            };

            await InvokePrivateAsync(
                model,
                "CheckTemporarySaveButtonAsync",
                nippou,
                99L,
                1L,
                false);

            Assert.IsFalse(model.TemporarySaveButton);
        }

        /// <summary>
        /// Given: GetKubunDataAsync の条件を満たしている
        /// When: NoWorkAndCompensatoryIsHalfAndPaidAtLeastHalf
        /// Then: HalfSubstituteAndHalfPaid
        /// </summary>
        [TestMethod(DisplayName = "GetKubunDataAsync: 勤務なしかつ振休が半日で有給が半日以上の場合、勤務区分1が半日振休、" +
            "勤務区分2が半日有給になる")]
        public async Task GetKubunDataAsync_勤務なしかつ振休が半日で有給が半日以上の場合勤務区分1が半日振休勤務区分2が半日有給になる()
        {
            var jissekiDate = D("20250120");
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel();

            db.FurikyuuZans.Add(new FurikyuuZan
            {
                Id = 1,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = 未
            });
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 1m,
                Kurikoshi = 0m,
                Syouka = 0.5m
            });
            await db.SaveChangesAsync();

            await InvokePrivateAsync(model, "GetKubunDataAsync", true, jissekiDate, syain);

            Assert.AreEqual(半日振休, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(半日有給, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: GetKubunDataAsync の条件を満たしている
        /// When: NoWorkAndCompensatoryIsHalfAndPaidIsZero
        /// Then: HalfSubstituteAndAbsence
        /// </summary>
        [TestMethod(DisplayName = "GetKubunDataAsync: 勤務なしかつ振休が半日で有給が0の場合、勤務区分1が半日振休、勤務区分2が欠勤になる")]
        public async Task GetKubunDataAsync_勤務なしかつ振休が半日で有給が0の場合勤務区分1が半日振休勤務区分2が欠勤になる()
        {
            var jissekiDate = D("20250120");
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel();

            db.FurikyuuZans.Add(new FurikyuuZan
            {
                Id = 1,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = 未
            });
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 0m,
                Kurikoshi = 0m,
                Syouka = 0m
            });
            await db.SaveChangesAsync();

            await InvokePrivateAsync(model, "GetKubunDataAsync", true, jissekiDate, syain);

            Assert.AreEqual(半日振休, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(欠勤, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: GetKubunDataAsync の条件を満たしている
        /// When: NoWorkAndCompensatoryIsHalfAndPaidIsOther
        /// Then: NoneNone
        /// </summary>
        [TestMethod(DisplayName = "GetKubunDataAsync: 勤務なしかつ振休が半日で有給が0以外の場合、勤務区分1も勤務区分2もなしになる")]
        public async Task GetKubunDataAsync_勤務なしかつ振休が半日で有給が0以外の場合勤務区分1も勤務区分2もなしになる()
        {
            var jissekiDate = D("20250120");
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel();

            db.FurikyuuZans.Add(new FurikyuuZan
            {
                Id = 1,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = 未
            });
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 0m,
                Kurikoshi = 0m,
                Syouka = 1m
            });
            await db.SaveChangesAsync();

            await InvokePrivateAsync(model, "GetKubunDataAsync", true, jissekiDate, syain);

            Assert.AreEqual(None, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(None, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: GetKubunDataAsync の条件を満たしている
        /// When: NoWorkAndCompensatoryIsZeroAndPaidIsMoreThanHalf
        /// Then: OneDayPaid
        /// </summary>
        [TestMethod(DisplayName = "GetKubunDataAsync: 勤務なしかつ振休が0で有給が半日以上の場合、勤務区分1が年次有給休暇、勤務区分2がなしになる")]
        public async Task GetKubunDataAsync_勤務なしかつ振休が0で有給が半日以上の場合勤務区分1が年次有給休暇勤務区分2がなしになる()
        {
            var jissekiDate = D("20250120");
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel();

            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 1m,
                Kurikoshi = 0m,
                Syouka = 0m
            });
            await db.SaveChangesAsync();

            await InvokePrivateAsync(model, "GetKubunDataAsync", true, jissekiDate, syain);

            Assert.AreEqual(年次有給休暇_1日, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(None, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: GetKubunDataAsync の条件を満たしている
        /// When: NoWorkAndCompensatoryIsZeroAndPaidIsHalf
        /// Then: HalfPaidAndAbsence
        /// </summary>
        [TestMethod(DisplayName = "GetKubunDataAsync: 勤務なしかつ振休が0で有給が半日の場合、勤務区分1が半日有給、勤務区分2が欠勤になる")]
        public async Task GetKubunDataAsync_勤務なしかつ振休が0で有給が半日の場合勤務区分1が半日有給勤務区分2が欠勤になる()
        {
            var jissekiDate = D("20250120");
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel();

            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 0.5m,
                Kurikoshi = 0m,
                Syouka = 0m
            });
            await db.SaveChangesAsync();

            await InvokePrivateAsync(model, "GetKubunDataAsync", true, jissekiDate, syain);

            Assert.AreEqual(半日有給, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(欠勤, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: GetKubunDataAsync の条件を満たしている
        /// When: NoWorkAndCompensatoryIsZeroAndPaidIsZero
        /// Then: Absence
        /// </summary>
        [TestMethod(DisplayName = "GetKubunDataAsync: 勤務なしかつ振休が0で有給が0の場合、勤務区分1が欠勤、勤務区分2がなしになる")]
        public async Task GetKubunDataAsync_勤務なしかつ振休が0で有給が0の場合勤務区分1が欠勤勤務区分2がなしになる()
        {
            var jissekiDate = D("20250120");
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel();

            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 0m,
                Kurikoshi = 0m,
                Syouka = 0m
            });
            await db.SaveChangesAsync();

            await InvokePrivateAsync(model, "GetKubunDataAsync", true, jissekiDate, syain);

            Assert.AreEqual(欠勤, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(None, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: GetKubunDataAsync の条件を満たしている
        /// When: NoWorkAndCompensatoryIsZeroAndPaidIsOther
        /// Then: NoneNone
        /// </summary>
        [TestMethod(DisplayName = "GetKubunDataAsync: 勤務なしかつ振休が0で有給が0以外の場合、勤務区分1も勤務区分2もなしになる")]
        public async Task GetKubunDataAsync_勤務なしかつ振休が0で有給が0以外の場合勤務区分1も勤務区分2もなしになる()
        {
            var jissekiDate = D("20250120");
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel();

            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 0m,
                Kurikoshi = 0m,
                Syouka = 1m
            });
            await db.SaveChangesAsync();

            await InvokePrivateAsync(model, "GetKubunDataAsync", true, jissekiDate, syain);

            Assert.AreEqual(None, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(None, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: GetKubunDataAsync の条件を満たしている
        /// When: NoWorkAndCompensatoryIsOther
        /// Then: NoneNone
        /// </summary>
        [TestMethod(DisplayName = "GetKubunDataAsync: 勤務なしかつ振休が0以外の場合、勤務区分1も勤務区分2もなしになる")]
        public async Task GetKubunDataAsync_勤務なしかつ振休が0以外の場合勤務区分1も勤務区分2もなしになる()
        {
            var jissekiDate = D("20250120");
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel();

            db.FurikyuuZans.Add(new FurikyuuZan
            {
                Id = 1,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 未
            });
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 1m,
                Kurikoshi = 0m,
                Syouka = 0m
            });
            await db.SaveChangesAsync();

            await InvokePrivateAsync(model, "GetKubunDataAsync", true, jissekiDate, syain);

            Assert.AreEqual(None, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(None, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: GetKubunDataAsync の条件を満たしている
        /// When: WorkdayAndPartEmployee
        /// Then: PartWork
        /// </summary>
        [TestMethod(DisplayName = "GetKubunDataAsync: 勤務日かつパート勤務の場合、勤務区分1がパート勤務になる")]
        public async Task GetKubunDataAsync_勤務日かつパート勤務の場合勤務区分1がパート勤務になる()
        {
            var jissekiDate = D("20250120");
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(6)
                .Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(15, 0)
            };

            await InvokePrivateAsync(model, "GetKubunDataAsync", true, jissekiDate, syain);

            Assert.AreEqual(パート勤務, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(None, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: GetKubunDataAsync の条件を満たしている
        /// When: HalfDayWorkAndPaidMoreThanHalfAndHalfCountAtLeast10
        /// Then: Kubun2IsOneDayPaid
        /// </summary>
        [TestMethod(DisplayName = "GetKubunDataAsync: 半日勤務かつ有給が半日以上かつ半日数が10以上の場合、勤務区分2が1日有給になる")]
        public async Task
        GetKubunDataAsync_半日勤務かつ有給が半日以上かつ半日数が10以上の場合勤務区分2が1日有給になる()
        {
            var jissekiDate = D("20250120");
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(12, 0)
            };
            model.SyukkinKubun2List = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
            {
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = "05",
                    Text = "半日有給"
                },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = "04",
                    Text = "有給1日"
                },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = "20",
                    Text = "欠勤"
                }
            };

            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 2m,
                Kurikoshi = 0m,
                Syouka = 1m,
                HannitiKaisuu = 10
            });
            await db.SaveChangesAsync();

            await InvokePrivateAsync(model, "GetKubunDataAsync", true, jissekiDate, syain);

            Assert.AreEqual(半日勤務, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(年次有給休暇_1日, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: GetKubunDataAsync の条件を満たしている
        /// When: HalfDayWorkAndPaidMoreThanHalfAndHalfCountBelow10
        /// Then: Kubun2IsHalfPaid
        /// </summary>
        [TestMethod(DisplayName = "GetKubunDataAsync: 半日勤務かつ有給が半日以上かつ半日数が10未満の場合、勤務区分2が半日有給になる")]
        public async Task GetKubunDataAsync_半日勤務かつ有給が半日以上かつ半日数が10未満の場合勤務区分2が半日有給になる()
        {
            var jissekiDate = D("20250120");
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(12, 0)
            };
            model.SyukkinKubun2List = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
            {
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = "05",
                    Text = "半日有給"
                },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = "04",
                    Text = "有給1日"
                },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = "20",
                    Text = "欠勤"
                }
            };

            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 2m,
                Kurikoshi = 0m,
                Syouka = 1m,
                HannitiKaisuu = 9
            });
            await db.SaveChangesAsync();

            await InvokePrivateAsync(model, "GetKubunDataAsync", true, jissekiDate, syain);

            Assert.AreEqual(半日勤務, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(半日有給, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: GetKubunDataAsync の条件を満たしている
        /// When: HalfDayWorkAndPaidHalfAndHalfCountBelow10
        /// Then: Kubun2IsHalfPaid
        /// </summary>
        [TestMethod(DisplayName = "GetKubunDataAsync: 半日勤務かつ有給が半日かつ半日数が0の場合、勤務区分2が半日有給になる")]
        public async Task GetKubunDataAsync_半日勤務かつ有給が半日かつ半日数が0の場合勤務区分2が半日有給になる()
        {
            var jissekiDate = D("20250120");
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(12, 0)
            };
            model.SyukkinKubun2List = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
            {
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = "05",
                    Text = "半日有給"
                },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = "04",
                    Text = "有給1日"
                },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = "20",
                    Text = "欠勤"
                }
            };

            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 0.5m,
                Kurikoshi = 0m,
                Syouka = 0m,
                HannitiKaisuu = 0
            });
            await db.SaveChangesAsync();

            await InvokePrivateAsync(model, "GetKubunDataAsync", true, jissekiDate, syain);

            Assert.AreEqual(半日勤務, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(半日有給, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: GetKubunDataAsync の条件を満たしている
        /// When: HalfDayWorkAndPaidHalfAndHalfCountAtLeast10
        /// Then: Kubun2IsAbsence
        /// </summary>
        [TestMethod(DisplayName = "GetKubunDataAsync: 半日勤務かつ有給が半日以上かつ半日数が10以上の場合、勤務区分2が欠勤になる")]
        public async Task GetKubunDataAsync_半日勤務かつ有給が半日以上かつ半日数が10以上の場合勤務区分2が欠勤になる()
        {
            var jissekiDate = D("20250120");
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(12, 0)
            };
            model.SyukkinKubun2List = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
            {
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = "05",
                    Text = "半日有給"
                },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = "04",
                    Text = "有給1日"
                },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = "20",
                    Text = "欠勤"
                }
            };

            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 0.5m,
                Kurikoshi = 0m,
                Syouka = 0m,
                HannitiKaisuu = 10
            });
            await db.SaveChangesAsync();

            await InvokePrivateAsync(model, "GetKubunDataAsync", true, jissekiDate, syain);

            Assert.AreEqual(半日勤務, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(欠勤, model.NippouData.SyukkinKubun2);
        }

        #endregion

        #region GetKubunDataAsync Tests

        /// <summary>
        /// Given: 営業日、実働時間が4時間を超える
        /// When: GetKubunDataAsyncを実行（暗黙的にOnGetAsyncから呼ばれる）
        /// Then: 出勤区分1が「通常勤務」に設定される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 営業日かつ実働時間が4時間超えの場合、出勤区分1が通常勤務になる")]
        public async Task OnGetAsync_営業日かつ実働時間が4時間超えの場合出勤区分1が通常勤務になる()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            // 2025/01/15は水曜日（営業日）
            var appConfig = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20990101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            };
            db.ApplicationConfigs.Add(appConfig);

            // 通常勤務の区分をDBに追加
            db.SyukkinKubuns.Add(new SyukkinKubunBuilder().WithId(2).WithCode("02").WithName("通常勤務").Build());

            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            // 9:00 - 18:00 (実働8時間 > 4時間)
            await model.OnGetAsync(syainBase.Id, D("20250115"), false,
                new TimeOnly(9, 0), new TimeOnly(18, 0), null, null, null, null);

            // 検証 (Assert)
            Assert.AreEqual(通常勤務, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(None, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: 営業日、実働時間が4時間以下、振休残あり
        /// When: OnGetAsyncを実行
        /// Then: 出勤区分1が「半日勤務」、出勤区分2が「半日振休」に設定される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 営業日で実働時間が4時間以下かつ振休残ありの場合、出勤区分1が半日勤務で出勤区分2が半日振休に設定される")]
        public async Task OnGetAsync_営業日で実働時間が4時間以下かつ振休残ありの場合出勤区分1が半日勤務で出勤区分2が半日振休に設定される()
        {
            // 準備 (Arrange)
            var jissekiDate = D("20250115"); // 水曜日
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(5).Build();
            db.Syains.Add(syain);

            db.KintaiZokuseis.Add(new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            });
            db.ApplicationConfigs.Add(new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20990101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            });
            AddSyukkinKubun(半日振休, isSyukkin: false, isVacation: true);

            // 振休残を追加
            db.FurikyuuZans.Add(new FurikyuuZan
            {
                Id = 1,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = jissekiDate,
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 未
            });

            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            // 9:00 - 12:00 (実働3時間 <= 4時間)
            await model.OnGetAsync(syainBase.Id, jissekiDate, false,
                new TimeOnly(9, 0), new TimeOnly(12, 0), null, null, null, null);

            // 検証 (Assert)
            Assert.AreEqual(半日勤務, model.NippouData.SyukkinKubun1);
            Assert.AreEqual(半日振休, model.NippouData.SyukkinKubun2);
        }

        /// <summary>
        /// Given: 非稼働日、勤務時間あり
        /// When: OnGetAsyncを実行
        /// Then: 出勤区分1が「休日出勤」に設定される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 非稼働日かつ勤務時間ありの場合、出勤区分1が休日出勤になる")]
        public async Task OnGetAsync_非稼働日かつ勤務時間ありの場合出勤区分1が休日出勤になる()
        {
            // 2025/01/18は土曜日
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(5).Build();
            db.Syains.Add(syain);

            db.KintaiZokuseis.Add(new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            });
            db.ApplicationConfigs.Add(new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20990101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            });
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            await model.OnGetAsync(syainBase.Id, D("20250118"), false,
                new TimeOnly(9, 0), new TimeOnly(12, 0), null, null, null, null);

            // 検証 (Assert)
            Assert.AreEqual(AttendanceClassification.休日出勤, model.NippouData.SyukkinKubun1);
        }

        #endregion

        #region GetNippouData Tests

        /// <summary>
        /// Given: GetNippouData の条件を満たしている
        /// When: 早朝申請が承認済み
        /// Then: Some指定で補正され3件反映される
        /// </summary>
        [TestMethod(DisplayName = "GetNippouData: 早朝申請が承認済みの場合、Some指定で補正され3件反映される")]
        public void GetNippouData_早朝申請が承認済みの場合Some指定で補正され3件反映される()
        {
            EnsureKintaiZokusei(4);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.IsDairiInput = false;
            model.NippouData = new IndexModel.NippouViewModel();

            var jissekiDate = D("20250115"); // 水曜日
            DateTime DTOnDay(string hhmm) => jissekiDate.ToDateTime(TimeOnly.ParseExact(hhmm, "HHmm", null));

            var workingHours = new List<WorkingHour>
            {
                new WorkingHour { SyukkinTime = DTOnDay("0631"), TaikinTime = DTOnDay("1200") },
                new WorkingHour { SyukkinTime = DTOnDay("0835"), TaikinTime = DTOnDay("1745") },
                new WorkingHour { SyukkinTime = DTOnDay("2210"), TaikinTime = DTOnDay("2300") },
            };

            var approved = new UkagaiHeader
            {
                Id = 1,
                SyainId = syain.Id,
                ShinseiYmd = jissekiDate,
                WorkYmd = jissekiDate,
                Status = 承認,
                Invalid = false,
                KaishiJikoku = new TimeOnly(6, 30),
                UkagaiShinseis = new List<UkagaiShinsei>
                {
                    new UkagaiShinsei { Id = 1, UkagaiHeaderId = 1, UkagaiSyubetsu = 夜間作業 }, // 夜間作業
                    new UkagaiShinsei { Id = 2, UkagaiHeaderId = 1, UkagaiSyubetsu = 早朝作業 }, // 早朝作業
                    new UkagaiShinsei { Id = 3, UkagaiHeaderId = 1, UkagaiSyubetsu = 深夜作業 }, // 深夜作業
                    new UkagaiShinsei { Id = 4, UkagaiHeaderId = 1, UkagaiSyubetsu = リフレッシュデー残業 }, // リフレッシュデー残業
                }
            };
            var unapproved = new UkagaiHeader
            {
                Id = 2,
                SyainId = syain.Id,
                ShinseiYmd = jissekiDate,
                WorkYmd = jissekiDate,
                Status = 承認待,
                Invalid = false,
                KaishiJikoku = new TimeOnly(5, 0),
                UkagaiShinseis = new List<UkagaiShinsei>
                {
                    new UkagaiShinsei { Id = 5, UkagaiHeaderId = 2, UkagaiSyubetsu = 早朝作業 }
                }
            };

            InvokePrivateVoid(
                model,
                "GetNippouData",
                null,
                workingHours,
                jissekiDate,
                syain,
                new List<UkagaiHeader> { approved, unapproved },
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            var expected1 = TimeCalculator.Hosei(
                "0631",
                "1200",
                true,
                true,
                true,
                true,
                true,
                LanguageExt.Prelude.Some("0630"));
            var expected2 = TimeCalculator.Hosei(
                "0835",
                "1745",
                true,
                true,
                true,
                true,
                true,
                LanguageExt.Prelude.Some("0630"));
            var expected3 = TimeCalculator.Hosei(
                "2210",
                "2300",
                true,
                true,
                true,
                true,
                true,
                LanguageExt.Prelude.Some("0630"));

            Assert.AreEqual(ParseHHmmOrNull(expected1.Item1), model.NippouData.SyukkinHm1);
            Assert.AreEqual(ParseHHmmOrNull(expected1.Item2), model.NippouData.TaisyutsuHm1);
            Assert.AreEqual(ParseHHmmOrNull(expected2.Item1), model.NippouData.SyukkinHm2);
            Assert.AreEqual(ParseHHmmOrNull(expected2.Item2), model.NippouData.TaisyutsuHm2);
            Assert.AreEqual(ParseHHmmOrNull(expected3.Item1), model.NippouData.SyukkinHm3);
            Assert.AreEqual(ParseHHmmOrNull(expected3.Item2), model.NippouData.TaisyutsuHm3);
        }

        /// <summary>
        /// Given: GetNippouData の条件を満たしている
        /// When: フリー社員で日報なし
        /// Then: 入力時刻がそのまま設定される
        /// </summary>
        [TestMethod(DisplayName = "GetNippouData: フリー社員で日報なしの場合、入力時刻がそのまま設定される")]
        public void GetNippouData_フリー社員で日報なしの場合入力時刻がそのまま設定される()
        {
            EnsureKintaiZokusei(3);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(3)
                .Build();

            var model = CreateModel(syain);
            model.IsDairiInput = false;
            model.NippouData = new IndexModel.NippouViewModel();

            var syukkin1 = new TimeOnly(8, 45);
            var taisyutsu1 = new TimeOnly(17, 15);
            var syukkin2 = new TimeOnly(18, 0);
            var taisyutsu2 = new TimeOnly(19, 0);

            InvokePrivateVoid(
                model,
                "GetNippouData",
                null,
                new List<WorkingHour>(),
                D("20250115"),
                syain,
                new List<UkagaiHeader>(),
                null,
                syukkin1,
                taisyutsu1,
                syukkin2,
                taisyutsu2,
                null,
                null);

            Assert.AreEqual(syukkin1, model.NippouData.SyukkinHm1);
            Assert.AreEqual(taisyutsu1, model.NippouData.TaisyutsuHm1);
            Assert.AreEqual(syukkin2, model.NippouData.SyukkinHm2);
            Assert.AreEqual(taisyutsu2, model.NippouData.TaisyutsuHm2);
            Assert.IsNull(model.NippouData.SyukkinHm3);
            Assert.IsNull(model.NippouData.TaisyutsuHm3);
        }

        /// <summary>
        /// Given: GetNippouData の条件を満たしている
        /// When: 標準社員外で日報なし
        /// Then: 入力時刻がそのまま設定される
        /// </summary>
        [TestMethod(DisplayName = "GetNippouData: 標準社員外で日報なしの場合、入力時刻がそのまま設定される")]
        public void GetNippouData_標準社員外で日報なしの場合入力時刻がそのまま設定される()
        {
            EnsureKintaiZokusei(5);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(5)
                .Build();

            var model = CreateModel(syain);
            model.IsDairiInput = false;
            model.NippouData = new IndexModel.NippouViewModel();

            var syukkin1 = new TimeOnly(9, 0);
            var taisyutsu1 = new TimeOnly(18, 0);
            var syukkin2 = new TimeOnly(18, 30);
            var taisyutsu2 = new TimeOnly(19, 0);
            var syukkin3 = new TimeOnly(20, 0);
            var taisyutsu3 = new TimeOnly(21, 0);

            InvokePrivateVoid(
                model,
                "GetNippouData",
                null,
                new List<WorkingHour>(),
                D("20250115"),
                syain,
                new List<UkagaiHeader>(),
                null,
                syukkin1,
                taisyutsu1,
                syukkin2,
                taisyutsu2,
                syukkin3,
                taisyutsu3);

            Assert.AreEqual(syukkin1, model.NippouData.SyukkinHm1);
            Assert.AreEqual(taisyutsu1, model.NippouData.TaisyutsuHm1);
            Assert.AreEqual(syukkin2, model.NippouData.SyukkinHm2);
            Assert.AreEqual(taisyutsu2, model.NippouData.TaisyutsuHm2);
            Assert.AreEqual(syukkin3, model.NippouData.SyukkinHm3);
            Assert.AreEqual(taisyutsu3, model.NippouData.TaisyutsuHm3);
        }

        /// <summary>
        /// Given: GetNippouData の条件を満たしている
        /// When: パートで日報なし
        /// Then: 入力時刻がそのまま設定される
        /// </summary>
        [TestMethod(DisplayName = "GetNippouData: パートで日報なしの場合、入力時刻がそのまま設定される")]
        public void GetNippouData_パートで日報なしの場合入力時刻がそのまま設定される()
        {
            EnsureKintaiZokusei(6);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(6)
                .Build();

            var model = CreateModel(syain);
            model.IsDairiInput = false;
            model.NippouData = new IndexModel.NippouViewModel();

            var syukkin1 = new TimeOnly(10, 0);
            var taisyutsu1 = new TimeOnly(14, 0);

            InvokePrivateVoid(
                model,
                "GetNippouData",
                null,
                new List<WorkingHour>(),
                D("20250115"),
                syain,
                new List<UkagaiHeader>(),
                null,
                syukkin1,
                taisyutsu1,
                null,
                null,
                null,
                null);

            Assert.AreEqual(syukkin1, model.NippouData.SyukkinHm1);
            Assert.AreEqual(taisyutsu1, model.NippouData.TaisyutsuHm1);
            Assert.IsNull(model.NippouData.SyukkinHm2);
            Assert.IsNull(model.NippouData.TaisyutsuHm2);
            Assert.IsNull(model.NippouData.SyukkinHm3);
            Assert.IsNull(model.NippouData.TaisyutsuHm3);
        }

        /// <summary>
        /// Given: GetNippouData の条件を満たしている
        /// When: 金曜日で承認申請なし
        /// Then: RefreshDayTrueでNone指定補正が使われる
        /// </summary>
        [TestMethod(DisplayName = "GetNippouData: 金曜日で承認申請なしの場合、RefreshDayTrueでNone指定補正が使われる")]
        public void GetNippouData_金曜日で承認申請なしの場合RefreshDayTrueでNone指定補正が使われる()
        {
            EnsureKintaiZokusei(4);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.IsDairiInput = false;
            model.NippouData = new IndexModel.NippouViewModel();

            var jissekiDate = D("20250117"); // 金曜日
            DateTime DTOnDay(string hhmm) => jissekiDate.ToDateTime(TimeOnly.ParseExact(hhmm, "HHmm", null));
            var workingHours = new List<WorkingHour>
            {
                new WorkingHour { SyukkinTime = DTOnDay("1700"), TaikinTime = DTOnDay("1800") },
            };

            InvokePrivateVoid(
                model,
                "GetNippouData",
                null,
                workingHours,
                jissekiDate,
                syain,
                new List<UkagaiHeader>(),
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            var expected = TimeCalculator.Hosei(
                "1700",
                "1800",
                false,
                false,
                false,
                false,
                true,
                LanguageExt.Prelude.None);
            Assert.AreEqual(ParseHHmmOrNull(expected.Item1), model.NippouData.SyukkinHm1);
            Assert.AreEqual(ParseHHmmOrNull(expected.Item2), model.NippouData.TaisyutsuHm1);
        }

        /// <summary>
        /// Given: GetNippouData の条件を満たしている
        /// When: 水曜日で承認申請なし
        /// Then: RefreshDayTrueでNone指定補正が使われる
        /// </summary>
        [TestMethod(DisplayName = "GetNippouData: 水曜日で承認申請なしの場合、RefreshDayTrueでNone指定補正が使われる")]
        public void GetNippouData_水曜日で承認申請なしの場合RefreshDayTrueでNone指定補正が使われる()
        {
            EnsureKintaiZokusei(4);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.IsDairiInput = false;
            model.NippouData = new IndexModel.NippouViewModel();

            var jissekiDate = D("20250115"); // 水曜日
            DateTime DTOnDay(string hhmm) => jissekiDate.ToDateTime(TimeOnly.ParseExact(hhmm, "HHmm", null));
            var workingHours = new List<WorkingHour>
            {
                new WorkingHour { SyukkinTime = DTOnDay("1700"), TaikinTime = DTOnDay("1800") },
            };

            InvokePrivateVoid(
                model,
                "GetNippouData",
                null,
                workingHours,
                jissekiDate,
                syain,
                new List<UkagaiHeader>(),
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            var expected = TimeCalculator.Hosei(
                "1700",
                "1800",
                false,
                false,
                false,
                false,
                true,
                LanguageExt.Prelude.None);
            Assert.AreEqual(ParseHHmmOrNull(expected.Item1), model.NippouData.SyukkinHm1);
            Assert.AreEqual(ParseHHmmOrNull(expected.Item2), model.NippouData.TaisyutsuHm1);
        }

        /// <summary>
        /// Given: GetNippouData の条件を満たしている
        /// When: 休業日リフレッシュデーかつ早朝承認あり
        /// Then: Some指定補正が使われる
        /// </summary>
        [TestMethod(DisplayName = "GetNippouData: 休業日リフレッシュデーかつ早朝承認ありの場合、Some指定補正が使われる")]
        public void GetNippouData_休業日リフレッシュデーかつ早朝承認ありの場合Some指定補正が使われる()
        {
            EnsureKintaiZokusei(4);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.IsDairiInput = false;
            model.NippouData = new IndexModel.NippouViewModel();

            var jissekiDate = D("20250120"); // 月曜日
            DateTime DTOnDay(string hhmm) => jissekiDate.ToDateTime(TimeOnly.ParseExact(hhmm, "HHmm", null));
            var workingHours = new List<WorkingHour>
            {
                new WorkingHour { SyukkinTime = DTOnDay("0631"), TaikinTime = DTOnDay("0900") },
            };

            var approvedMorning = new UkagaiHeader
            {
                Id = 1,
                SyainId = syain.Id,
                ShinseiYmd = jissekiDate,
                WorkYmd = jissekiDate,
                Status = 承認,
                Invalid = false,
                KaishiJikoku = new TimeOnly(6, 30),
                UkagaiShinseis = new List<UkagaiShinsei>
                {
                    new UkagaiShinsei { Id = 1, UkagaiHeaderId = 1, UkagaiSyubetsu = 早朝作業 },
                }
            };

            var holiday = new Hikadoubi
            {
                Id = 1,
                Ymd = jissekiDate,
                RefreshDay = RefreshDayFlag.リフレッシュデー
            };

            InvokePrivateVoid(
                model,
                "GetNippouData",
                null,
                workingHours,
                jissekiDate,
                syain,
                new List<UkagaiHeader> { approvedMorning },
                holiday,
                null,
                null,
                null,
                null,
                null,
                null
            );

            var expected = TimeCalculator.Hosei(
                "0631",
                "0900",
                false,
                false,
                true,
                false,
                true,
                LanguageExt.Prelude.Some("0630"));
            Assert.AreEqual(ParseHHmmOrNull(expected.Item1), model.NippouData.SyukkinHm1);
            Assert.AreEqual(ParseHHmmOrNull(expected.Item2), model.NippouData.TaisyutsuHm1);
        }

        /// <summary>
        /// Given: GetNippouData の条件を満たしている
        /// When: 早朝申請が承認済みでない
        /// Then: None指定で補正され4件目は無視される
        /// </summary>
        [TestMethod(DisplayName = "GetNippouData: 早朝申請が承認済みでない場合、None指定で補正され4件目は無視される")]
        public void GetNippouData_早朝申請が承認済みでない場合None指定で補正され4件目は無視される()
        {
            EnsureKintaiZokusei(4);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            model.IsDairiInput = false;
            model.NippouData = new IndexModel.NippouViewModel();

            var jissekiDate = D("20250113"); // 月曜日
            DateTime DTOnDay(string hhmm) => jissekiDate.ToDateTime(TimeOnly.ParseExact(hhmm, "HHmm", null));

            var workingHours = new List<WorkingHour>
            {
                new WorkingHour { SyukkinTime = DTOnDay("0835"), TaikinTime = DTOnDay("1915") },
                new WorkingHour { SyukkinTime = DTOnDay("0000"), TaikinTime = DTOnDay("0420") },
                new WorkingHour { SyukkinTime = DTOnDay("0510"), TaikinTime = DTOnDay("0800") },
                new WorkingHour { SyukkinTime = DTOnDay("0900"), TaikinTime = DTOnDay("1000") }, // 4件目は無視される
            };

            var approved = new UkagaiHeader
            {
                Id = 1,
                SyainId = syain.Id,
                ShinseiYmd = jissekiDate,
                WorkYmd = jissekiDate,
                Status = 承認,
                Invalid = false,
                UkagaiShinseis = new List<UkagaiShinsei>
                {
                    new UkagaiShinsei { Id = 1, UkagaiHeaderId = 1, UkagaiSyubetsu = 夜間作業 }, // 夜間作業のみ
                }
            };
            var unapprovedMorning = new UkagaiHeader
            {
                Id = 2,
                SyainId = syain.Id,
                ShinseiYmd = jissekiDate,
                WorkYmd = jissekiDate,
                Status = 承認待,
                Invalid = false,
                UkagaiShinseis = new List<UkagaiShinsei>
                {
                    new UkagaiShinsei { Id = 2, UkagaiHeaderId = 2, UkagaiSyubetsu = 早朝作業 }, // 早朝作業(未承認)
                }
            };

            var holiday = new Hikadoubi
            {
                Id = 1,
                Ymd = jissekiDate,
                RefreshDay = (RefreshDayFlag)1
            };

            InvokePrivateVoid(
                model,
                "GetNippouData",
                null,
                workingHours,
                jissekiDate,
                syain,
                new List<UkagaiHeader> { approved, unapprovedMorning },
                holiday,
                null,
                null,
                null,
                null,
                null,
                null
            );

            var expected1 = TimeCalculator.Hosei(
                "0835",
                "1915",
                true,
                false,
                false,
                false,
                true,
                LanguageExt.Prelude.None);
            var expected2 = TimeCalculator.Hosei(
                "0000",
                "0420",
                true,
                false,
                false,
                false,
                true,
                LanguageExt.Prelude.None);
            var expected3 = TimeCalculator.Hosei(
                "0510",
                "0800",
                true,
                false,
                false,
                false,
                true,
                LanguageExt.Prelude.None);
            var expected4 = TimeCalculator.Hosei(
                "0900",
                "1000",
                true,
                false,
                false,
                false,
                true,
                LanguageExt.Prelude.None);

            Assert.AreEqual(ParseHHmmOrNull(expected1.Item1), model.NippouData.SyukkinHm1);
            Assert.AreEqual(ParseHHmmOrNull(expected1.Item2), model.NippouData.TaisyutsuHm1);
            Assert.AreEqual(ParseHHmmOrNull(expected2.Item1), model.NippouData.SyukkinHm2);
            Assert.AreEqual(ParseHHmmOrNull(expected2.Item2), model.NippouData.TaisyutsuHm2);
            Assert.AreEqual(ParseHHmmOrNull(expected3.Item1), model.NippouData.SyukkinHm3);
            Assert.AreEqual(ParseHHmmOrNull(expected3.Item2), model.NippouData.TaisyutsuHm3);
            Assert.AreNotEqual(ParseHHmmOrNull(expected4.Item1), model.NippouData.SyukkinHm3, "4件目は反映されないべきです。");
            Assert.AreNotEqual(ParseHHmmOrNull(expected4.Item2), model.NippouData.TaisyutsuHm3, "4件目は反映されないべきです。");
        }

        #endregion

        #region CheckForNotificationMessageAsync Tests

        /// <summary>
        /// Given: 休日、勤務時間あり、承認済みの休日出勤申請なし
        /// When: OnGetAsyncを実行
        /// Then: 休日出勤の警告メッセージが返される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 休日勤務時間ありかつ承認済み休日出勤申請なしの場合、休日出勤警告メッセージが返される")]
        public async Task OnGetAsync_休日勤務時間ありかつ承認済み休日出勤申請なしの場合休日出勤警告メッセージが返される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            db.KintaiZokuseis.Add(new KintaiZokusei
            {
                Id = 4,
                Code = 管理,
                Name = "管理"
            });
            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(4).Build();
            db.Syains.Add(syain);

            db.ApplicationConfigs.Add(new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20990101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            });

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            var saturdayDate = D("20250118"); // 土曜日

            // 実行 (Act)
            await model.OnGetAsync(syainBase.Id, saturdayDate, true,
                new TimeOnly(9, 0), new TimeOnly(12, 0), null, null, null, null);

            // 検証 (Assert)
            Assert.IsNotNull(model.MessageString);
            StringAssert.Contains(model.MessageString, "休日出勤の指示がでていない");
            Assert.IsFalse(model.ConfirmButton);
        }

        /// <summary>
        /// Given: 夜間作業があるが承認済みの申請なし
        /// When: OnGetAsyncを実行
        /// Then: 夜間作業の警告メッセージが返される
        /// </summary>
        [TestMethod(DisplayName = "OnGetAsync: 夜間作業があるが承認済みの申請なしの場合、夜間作業警告メッセージが返される")]
        public async Task OnGetAsync_夜間作業があるが承認済みの申請なしの場合夜間作業警告メッセージが返される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            db.KintaiZokuseis.Add(new KintaiZokusei
            {
                Id = 4,
                Code = 管理,
                Name = "管理"
            });
            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(4).Build();
            db.Syains.Add(syain);

            db.ApplicationConfigs.Add(new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = D("20990101"),
                MsClientId = "id",
                MsClientSecret = "s",
                MsTenantId = "t",
                SmtpUser = "u",
                SmtpPassword = "p"
            });

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            var jissekiDate = D("20250115"); // 水曜日

            // 実行 (Act)
            // 0:00 - 4:00 は夜間作業(0:00-5:00)に含まれる
            await model.OnGetAsync(syainBase.Id, jissekiDate, true,
                new TimeOnly(0, 0), new TimeOnly(4, 0), null, null, null, null);

            // 検証 (Assert)
            Assert.IsNotNull(model.MessageString);
            StringAssert.Contains(model.MessageString, "夜間作業の指示がでていない");
            Assert.IsFalse(model.ConfirmButton);
        }

        /// <summary>
        /// Given: CheckForNotificationMessageAsync の条件を満たしている
        /// When: 日報停止日到達
        /// Then: 停止メッセージが返る
        /// </summary>
        [TestMethod(DisplayName = "CheckForNotificationMessageAsync: 日報停止日到達の場合、停止メッセージが返る")]
        public async Task CheckForNotificationMessageAsync_日報停止日到達の場合停止メッセージが返る()
        {
            EnsureKintaiZokusei(4);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            var jissekiDate = D("20251015");
            var nippou = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0)
            };
            model.NippouData = nippou;
            model.SyainId = syain.Id;

            var message = await InvokePrivateWithResultAsync<string>(
                model,
                "CheckForNotificationMessageAsync",
                nippou,
                syain,
                new List<WorkingHour>(),
                true,
                new ApplicationConfig { NippoStopDate = jissekiDate },
                new List<UkagaiHeader>(),
                jissekiDate);

            Assert.IsNotNull(message);
            StringAssert.Contains(message, "確定確定を停止しています");
            Assert.IsFalse(model.ConfirmButton);
        }

        /// <summary>
        /// Given: CheckForNotificationMessageAsync の条件を満たしている
        /// When: 打刻漏れあり
        /// Then: 打刻漏れメッセージが返る
        /// </summary>
        [TestMethod(DisplayName = "CheckForNotificationMessageAsync: 打刻漏れありの場合、打刻漏れメッセージが返る")]
        public async Task CheckForNotificationMessageAsync_打刻漏れありの場合打刻漏れメッセージが返る()
        {
            EnsureKintaiZokusei(4);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            var jissekiDate = D("20250115");
            var nippou = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = null
            };
            model.NippouData = nippou;
            model.SyainId = syain.Id;

            var message = await InvokePrivateWithResultAsync<string>(
                model,
                "CheckForNotificationMessageAsync",
                nippou,
                syain,
                new List<WorkingHour>(),
                true,
                new ApplicationConfig { NippoStopDate = D("20990101") },
                new List<UkagaiHeader>(),
                jissekiDate);

            Assert.IsNotNull(message);
            StringAssert.Contains(message, "打刻漏れ");
            Assert.IsFalse(model.ConfirmButton);
        }

        /// <summary>
        /// Given: CheckForNotificationMessageAsync の条件を満たしている
        /// When: 休日夜間早朝深夜が未承認
        /// Then: 複合警告が返る
        /// </summary>
        [TestMethod(DisplayName = "CheckForNotificationMessageAsync: 休日夜間早朝深夜が未承認の場合、複合警告が返る")]
        public async Task CheckForNotificationMessageAsync_休日夜間早朝深夜が未承認の場合複合警告が返る()
        {
            EnsureKintaiZokusei(4);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            var jissekiDate = D("20250118"); // 土曜日
            var nippou = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(0, 0),
                TaisyutsuHm1 = new TimeOnly(23, 30)
            };
            model.NippouData = nippou;
            model.SyainId = syain.Id;

            var message = await InvokePrivateWithResultAsync<string>(
                model,
                "CheckForNotificationMessageAsync",
                nippou,
                syain,
                new List<WorkingHour>(),
                false,
                new ApplicationConfig { NippoStopDate = D("20990101") },
                new List<UkagaiHeader>(),
                jissekiDate);

            Assert.IsNotNull(message);
            StringAssert.Contains(message, "休日出勤");
            StringAssert.Contains(message, "夜間作業");
            StringAssert.Contains(message, "早朝作業");
            StringAssert.Contains(message, "深夜作業");
            Assert.IsFalse(model.ConfirmButton);
        }

        /// <summary>
        /// Given: CheckForNotificationMessageAsync の条件を満たしている
        /// When: フリー社員で最終承認なし
        /// Then: 状態4メッセージが返る
        /// </summary>
        [TestMethod(DisplayName = "CheckForNotificationMessageAsync: フリー社員で最終承認なしの場合、状態4メッセージが返る")]
        public async Task CheckForNotificationMessageAsync_フリー社員で最終承認なしの場合状態4メッセージが返る()
        {
            EnsureKintaiZokusei(3);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(3)
                .Build();
            var model = CreateModel(syain);
            var jissekiDate = D("20250115");
            var nippou = new IndexModel.NippouViewModel();

            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = syain.Id,
                ShinseiYmd = D("20250110"),
                WorkYmd = jissekiDate,
                LastShoninYmd = D("20250111"),
                Status = 承認待,
                Invalid = false
            };

            var message = await InvokePrivateWithResultAsync<string>(
                model,
                "CheckForNotificationMessageAsync",
                nippou,
                syain,
                new List<WorkingHour>(),
                true,
                new ApplicationConfig { NippoStopDate = D("20990101") },
                new List<UkagaiHeader> { ukagaiHeader },
                jissekiDate);

            Assert.IsNotNull(message);
            StringAssert.Contains(message, "最終承認されていません");
        }

        /// <summary>
        /// Given: CheckForNotificationMessageAsync の条件を満たしている
        /// When: フリー社員で伺いなし
        /// Then: メッセージなし
        /// </summary>
        [TestMethod(DisplayName = "CheckForNotificationMessageAsync: フリー社員で伺いなしの場合、メッセージなし")]
        public async Task CheckForNotificationMessageAsync_フリー社員で伺いなしの場合メッセージなし()
        {
            EnsureKintaiZokusei(3);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(3)
                .Build();
            var model = CreateModel(syain);
            var jissekiDate = D("20250115");
            var nippou = new IndexModel.NippouViewModel();

            var message = await InvokePrivateWithResultAsync<string>(
                model,
                "CheckForNotificationMessageAsync",
                nippou,
                syain,
                new List<WorkingHour>(),
                true,
                new ApplicationConfig { NippoStopDate = D("20990101") },
                new List<UkagaiHeader>(),
                jissekiDate);

            Assert.IsNull(message);
        }

        /// <summary>
        /// Given: CheckForNotificationMessageAsync の条件を満たしている
        /// When: 打刻時間修正未承認かつEditedあり
        /// Then: 修正メッセージが返る
        /// </summary>
        [TestMethod(DisplayName = "CheckForNotificationMessageAsync: 打刻時間修正未承認かつEditedありの場合、修正メッセージが返る")]
        public async Task CheckForNotificationMessageAsync_打刻時間修正未承認かつEditedありの場合修正メッセージが返る()
        {
            EnsureKintaiZokusei(4);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            var jissekiDate = D("20250115");
            var nippou = new IndexModel.NippouViewModel();

            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = syain.Id,
                ShinseiYmd = D("20250110"),
                WorkYmd = jissekiDate,
                LastShoninYmd = D("20250111"),
                Status = 承認待,
                Invalid = false
            };
            var workingHours = new List<WorkingHour>
            {
                new WorkingHour
                {
                    Id = 1,
                    SyainId = syain.Id,
                    Hiduke = jissekiDate,
                    Edited = true
                }
            };

            var message = await InvokePrivateWithResultAsync<string>(
                model,
                "CheckForNotificationMessageAsync",
                nippou,
                syain,
                workingHours,
                true,
                new ApplicationConfig { NippoStopDate = D("20990101") },
                new List<UkagaiHeader> { ukagaiHeader },
                jissekiDate);

            Assert.IsNotNull(message);
            StringAssert.Contains(message, "打刻時間修正");
        }

        /// <summary>
        /// Given: CheckForNotificationMessageAsync の条件を満たしている
        /// When: 月末超過かつ時間外拡張未承認
        /// Then: 残業制限メッセージが返る
        /// </summary>
        [TestMethod(DisplayName = "CheckForNotificationMessageAsync: 月末超過かつ時間外拡張未承認_Then残業制限メッセージが返る")]
        public async Task CheckForNotificationMessageAsync_月末超過かつ時間外拡張未承認の場合残業制限メッセージが返る()
        {
            var kintai = new KintaiZokusei
            {
                Id = 4,
                Code = 管理,
                Name = "管理",
                SeigenTime = 0.1m,
                MaxLimitTime = 80m,
                IsOvertimeLimit3m = false,
                IsMinashi = false
            };
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(kintai.Id)
                .Build();
            syain.KintaiZokusei = kintai;

            var model = CreateModel(syain);
            var jissekiDate = D("20250131");
            var nippou = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                HJitsudou = 600m
            };
            model.NippouData = nippou;
            model.SyainId = syain.Id;

            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = syain.Id,
                ShinseiYmd = D("20250110"),
                WorkYmd = jissekiDate,
                LastShoninYmd = D("20250111"),
                Status = 承認待,
                Invalid = false
            };

            var message = await InvokePrivateWithResultAsync<string>(
                model,
                "CheckForNotificationMessageAsync",
                nippou,
                syain,
                new List<WorkingHour>(),
                true,
                new ApplicationConfig { NippoStopDate = D("20990101") },
                new List<UkagaiHeader> { ukagaiHeader },
                jissekiDate);

            Assert.IsNotNull(message);
            StringAssert.Contains(message, "残業時間が制限時間を超えます");
        }

        /// <summary>
        /// Given: CheckForNotificationMessageAsync の条件を満たしている
        /// When: 該当条件なし
        /// Then: メッセージなし
        /// </summary>
        [TestMethod(DisplayName = "CheckForNotificationMessageAsync: 該当条件なしの場合、メッセージなし")]
        public async Task CheckForNotificationMessageAsync_該当条件なしの場合メッセージなし()
        {
            EnsureKintaiZokusei(4);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            var model = CreateModel(syain);
            var jissekiDate = D("20250115");
            var nippou = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0)
            };
            model.NippouData = nippou;
            model.SyainId = syain.Id;

            var message = await InvokePrivateWithResultAsync<string>(
                model,
                "CheckForNotificationMessageAsync",
                nippou,
                syain,
                new List<WorkingHour>(),
                true,
                new ApplicationConfig { NippoStopDate = D("20990101") },
                new List<UkagaiHeader>(),
                jissekiDate);

            Assert.IsNull(message);
        }

        #endregion

        #region OnPostTemporarySaveAsync Tests (Phase 2)

        /// <summary>
        /// Given: 社員が存在しない
        /// When: 一時保存
        /// Then: エラーが返される
        /// </summary>
        [TestMethod(DisplayName = "OnPostTemporarySaveAsync: 社員が存在しない場合、エラーが返される")]
        public async Task OnPostTemporarySaveAsync_社員が存在しない場合エラーが返される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            // 社員データを範囲外に設定（実績日に該当しない）
            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20250110"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainBaseId = syainBase.Id;
            model.JissekiDate = D("20250215");
            model.IsDairiInput = false;
            model.NippouData = new IndexModel.NippouViewModel();
            model.NippouAnkenCards = new IndexModel.CardsViewModel();

            // 実行 (Act)
            var result = await model.OnPostTemporarySaveAsync(999, D("20250215"), false);

            // 検証 (Assert)
            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.エラー, GetResponseStatus(json));
            StringAssert.Contains(GetMessage(json) ?? string.Empty, "社員が見つかりません。");
        }

        /// <summary>
        /// Given: 既存の日報がある（一時保存済み）
        /// When: 一時保存
        /// Then: 既存データが更新される
        /// </summary>
        [TestMethod(DisplayName = "OnPostTemporarySaveAsync: 既存日報がある場合、既存データが更新される")]
        public async Task OnPostTemporarySaveAsync_既存日報がある場合既存データが更新される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1).WithCode("02").WithName("通常勤務").Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("20250115");
            var existingNippou = new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(17, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 一時保存
            };
            db.Nippous.Add(existingNippou);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainBaseId = syainBase.Id;
            model.JissekiDate = jissekiDate;
            model.IsDairiInput = false;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(8, 30),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunCodeString1 = "02"
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel();

            // 実行 (Act)
            var result = await model.OnPostTemporarySaveAsync(syain.Id, jissekiDate, false);

            // 検証 (Assert)
            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.エラー, GetResponseStatus(json));
            StringAssert.Contains(GetMessage(json) ?? string.Empty, "サーバー内部でエラー");
            var updatedNippou = await db.Nippous
                .FirstOrDefaultAsync(n => n.SyainId == syain.Id && n.NippouYmd == jissekiDate);
            Assert.IsNotNull(updatedNippou, "日報が存在すべきです。");
            Assert.AreEqual(new TimeOnly(8, 30), updatedNippou.SyukkinHm1,
                "例外発生時でも同一DbContext上の追跡値は更新後時刻になるべきです。");
            Assert.AreEqual(new TimeOnly(18, 0), updatedNippou.TaisyutsuHm1,
                "例外発生時でも同一DbContext上の追跡値は更新後時刻になるべきです。");
        }

        #endregion

        /// <summary>
        /// Given: OnPostTemporarySaveAsync の条件を満たしている
        /// When: ValidationFails
        /// Then: ErrorJsonReturned
        /// </summary>
        [TestMethod(DisplayName = "OnPostTemporarySaveAsync: Validationエラーの場合、エラーメッセージが返る")]
        public async Task OnPostTemporarySaveAsync_Validationエラーの場合エラーメッセージが返る()
        {
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 1,
                Code = みなし対象者,
                Name = "標準"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithKintaiZokuseiId(kintaiZokusei.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .Build();
            db.Syains.Add(syain);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            var jissekiDate = D("20250115");
            model.SyainBaseId = syainBase.Id;
            model.JissekiDate = jissekiDate;
            model.IsDairiInput = false;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubunCodeString1 = ""
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel();

            var result = await model.OnPostTemporarySaveAsync(syain.Id, jissekiDate, false);

            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.エラー, GetResponseStatus(json));
            Assert.IsFalse(string.IsNullOrEmpty(GetMessage(json)));
        }

        /// <summary>
        /// Given: OnPostTemporarySaveAsync の条件を満たしている
        /// When: ExistingNippouWithAnkens
        /// Then: DeletesOldAndInsertsNew
        /// </summary>
        [TestMethod(DisplayName = "OnPostTemporarySaveAsync: 既存日報に案件がある場合、古いデータを削除し新しいデータを挿入する")]
        public async Task OnPostTemporarySaveAsync_既存日報に案件がある場合古いデータを削除し新しいデータを挿入する()
        {
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 1,
                Code = みなし対象者,
                Name = "標準"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithKintaiZokuseiId(kintaiZokusei.Id)
                .WithStartYmd(D("20250101"))
                .WithEndYmd(D("20251231"))
                .Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1)
                .WithCode("02")
                .WithName("通常勤務")
                .WithIsSyukkin(true)
                .WithIsVacation(false)
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("20250115");
            var existingNippou = new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 一時保存
            };
            db.Nippous.Add(existingNippou);
            db.NippouAnkens.Add(new NippouAnken
            {
                NippouId = existingNippou.Id,
                AnkensId = 10,
                KokyakuKaisyaId = 20,
                KokyakuName = "OLD",
                AnkenName = "OLD-ANKEN",
                JissekiJikan = 1,
                IsLinked = true
            });
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainBaseId = syainBase.Id;
            model.JissekiDate = jissekiDate;
            model.IsDairiInput = false;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunCodeString1 = "02"
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        AnkensId = 100,
                        KokyakuKaisyaId = 200,
                        KokyakuName = "NEW",
                        AnkenName = "NEW-ANKEN",
                        JissekiJikan = 2,
                        IsLinked = true
                    }
                }
            };

            var result = await model.OnPostTemporarySaveAsync(syain.Id, jissekiDate, false);

            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.エラー, GetResponseStatus(json));
            StringAssert.Contains(GetMessage(json) ?? string.Empty, "サーバー内部でエラー");

            var rows = await db.NippouAnkens.Where(x => x.NippouId == existingNippou.Id).ToListAsync();
            Assert.HasCount(1, rows, "On error, existing rows should remain.");
            Assert.AreEqual("OLD", rows[0].KokyakuName);
            Assert.AreEqual("OLD-ANKEN", rows[0].AnkenName);
        }

        #region OnPostCancelConfirmAsync Tests (Phase 4)

        /// <summary>
        /// Given: 日報データが存在しない
        /// When: 確定解除
        /// Then: エラーが返される
        /// </summary>
        [TestMethod(DisplayName = "OnPostCancelConfirmAsync: 日報データが存在しない場合、エラーが返される")]
        public async Task OnPostCancelConfirmAsync_日報データが存在しない場合エラーが返される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainBaseId = syainBase.Id;
            model.JissekiDate = D("20250120");
            model.IsDairiInput = false;

            // 実行 (Act)
            var result = await model.OnPostCancelConfirmAsync(syain.Id, D("20250120"), false, syain.Id);

            // 検証 (Assert)
            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.エラー, GetResponseStatus(json));
            StringAssert.Contains(GetMessage(json) ?? string.Empty, "取込データが存在しません。");
        }

        /// <summary>
        /// Given: 代理入力で確定済みの日報データ
        /// When: 確定解除
        /// Then: 確定解除され、代理入力の確定解除履歴が登録される
        /// </summary>
        [TestMethod(DisplayName = "OnPostCancelConfirmAsync: 代理入力で確定済みの日報データ、確定解除され代理入力の確定解除履歴が登録される")]
        public async Task OnPostCancelConfirmAsync_代理入力で確定済みの日報データ確定解除され代理入力の確定解除履歴が登録される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1).WithCode("02").WithName("通常勤務").Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("20250115");
            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 確定保存,
                KakuteiYmd = jissekiDate
            });
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainBaseId = syainBase.Id;
            model.JissekiDate = jissekiDate;
            model.IsDairiInput = true;

            // 実行 (Act)
            var result = await model.OnPostCancelConfirmAsync(syain.Id, jissekiDate, true, syain.Id);

            // 検証 (Assert)
            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.正常, GetResponseStatus(json), $"message={GetMessage(json)}");

            var nippou = await db.Nippous.FirstOrDefaultAsync(
                n => n.SyainId == syain.Id && n.NippouYmd == jissekiDate);
            Assert.AreEqual(一時保存,
                nippou!.TourokuKubun, "一時保存に戻されるべきです。");

            var history = await db.DairiNyuryokuRirekis
                .FirstOrDefaultAsync(h => h.NippouId == nippou.Id
                    && h.NippouSousa == DailyReportOperation.確定解除);
            Assert.IsNotNull(history, "代理入力の確定解除履歴が登録されるべきです。");
        }

        #endregion

        #region OnGetCancelConfirmValidateAsync Tests (Phase 5)

        /// <summary>
        /// Given: 日報が存在しない
        /// When: 確定解除バリデーション
        /// Then: エラーが返される
        /// </summary>
        [TestMethod(DisplayName = "OnGetCancelConfirmValidateAsync: 日報が存在しない場合、エラーが返される")]
        public async Task OnGetCancelConfirmValidateAsync_日報が存在しない場合エラーが返される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            var result = await model.OnGetCancelConfirmValidateAsync(
                syainBase.Id, D("20250120"));

            // 検証 (Assert)
            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.エラー, GetResponseStatus(json));
            StringAssert.Contains(GetMessage(json) ?? string.Empty, "この日報は解除されています。");
        }

        /// <summary>
        /// Given: 確定済みだが翌日も確定済み
        /// When: 確定解除バリデーション
        /// Then: 「以降確定あり」エラー
        /// </summary>
        [TestMethod(DisplayName = "OnGetCancelConfirmValidateAsync: 確定済みだが翌日も確定済みの場合、以降確定ありエラーが返される")]
        public async Task OnGetCancelConfirmValidateAsync_確定済みだが翌日も確定済みの場合以降確定ありエラーが返される()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1).WithCode("02").WithName("通常勤務").Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("20250115");
            // 当日確定済み
            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 確定保存,
                KakuteiYmd = jissekiDate
            });
            // 翌日も確定済み
            db.Nippous.Add(new Nippou
            {
                Id = 2,
                SyainId = syain.Id,
                NippouYmd = jissekiDate.AddDays(1),
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 確定保存,
                KakuteiYmd = jissekiDate.AddDays(1)
            });
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            var result = await model.OnGetCancelConfirmValidateAsync(
                syainBase.Id, jissekiDate);

            // 検証 (Assert)
            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.エラー, GetResponseStatus(json));
            StringAssert.Contains(GetMessage(json) ?? string.Empty, "以降の確定日があるため確定解除出来ません。最終確定日から順に解除してください。");
        }

        /// <summary>
        /// Given: 確定済み、翌日なし
        /// When: 確定解除バリデーション
        /// Then: 正常成功
        /// </summary>
        [TestMethod(DisplayName = "OnGetCancelConfirmValidateAsync: 確定済み、翌日なしの場合、正常成功")]
        public async Task OnGetCancelConfirmValidateAsync_確定済み翌日なしの場合正常成功()
        {
            // 準備 (Arrange)
            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            db.SyainBases.Add(syainBase);

            var kintaiZokusei = new KintaiZokusei
            {
                Id = 5,
                Code = 標準社員外,
                Name = "標準社員外"
            };
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syain = new SyainBuilder()
                .WithId(1).WithSyainBaseId(syainBase.Id)
                .WithStartYmd(D("20250101")).WithEndYmd(D("20251231"))
                .WithKintaiZokuseiId(kintaiZokusei.Id).Build();
            db.Syains.Add(syain);

            var syukkinKubun = new SyukkinKubunBuilder()
                .WithId(1).WithCode("02").WithName("通常勤務").Build();
            db.SyukkinKubuns.Add(syukkinKubun);

            var jissekiDate = D("20250115");
            db.Nippous.Add(new Nippou
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYmd = jissekiDate,
                SyukkinKubunId1 = syukkinKubun.Id,
                TourokuKubun = 確定保存,
                KakuteiYmd = jissekiDate
            });
            await db.SaveChangesAsync();

            var model = CreateModel(syain);

            // 実行 (Act)
            var result = await model.OnGetCancelConfirmValidateAsync(
                syainBase.Id, jissekiDate);

            // 検証 (Assert)
            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.正常, GetResponseStatus(json), $"message={GetMessage(json)}");
        }

        #endregion

        #region ViewModel Tests (Phase 7)

        /// <summary>
        /// Given: 出退勤時刻が全てnull
        /// When: TotalWorkingHoursInMinuteを参照
        /// Then: 0が返される
        /// </summary>
        [TestMethod(DisplayName = "NippouViewModel: 出退勤時刻が全てnullの場合、労働時間（分）は0を返す")]
        public void NippouViewModel_出退勤時刻が全てnullの場合労働時間分は0を返す()
        {
            // 準備 (Arrange)
            var vm = new IndexModel.NippouViewModel();

            // 実行 (Act)
            var totalMinutes = vm.TotalWorkingHoursInMinute;

            // 検証 (Assert)
            Assert.AreEqual(0, totalMinutes,
                "出退勤なしの場合は0分であるべきです。");
        }

        /// <summary>
        /// Given: SyukkinKubunCodeStringが空文字
        /// When: SyukkinKubun1を参照
        /// Then: Noneが返される
        /// </summary>
        [TestMethod(DisplayName = "NippouViewModel: SyukkinKubunCodeStringが空文字の場合、出勤区分1はNoneを返す")]
        public void NippouViewModel_SyukkinKubunCodeStringが空文字の場合出勤区分1はNoneを返す()
        {
            // 準備 (Arrange)
            var vm = new IndexModel.NippouViewModel
            {
                SyukkinKubunCodeString1 = ""
            };

            // 実行 (Act)
            var kubun = vm.SyukkinKubun1;

            // 検証 (Assert)
            Assert.AreEqual(None, kubun,
                "空文字の場合Noneであるべきです。");
        }

        /// <summary>
        /// Given: SyukkinKubunCodeStringが数値変換不可
        /// When: SyukkinKubun2を参照
        /// Then: Noneが返される
        /// </summary>
        [TestMethod(DisplayName = "NippouViewModel: SyukkinKubunCodeStringが数値変換不可の場合、出勤区分2はNoneを返す")]
        public void NippouViewModel_SyukkinKubunCodeStringが数値変換不可の場合出勤区分2はNoneを返す()
        {
            // 準備 (Arrange)
            var vm = new IndexModel.NippouViewModel
            {
                SyukkinKubunCodeString2 = "XY"
            };

            // 実行 (Act)
            var kubun = vm.SyukkinKubun2;

            // 検証 (Assert)
            Assert.AreEqual(None, kubun,
                "数値でないコードの場合Noneであるべきです。");
        }

        /// <summary>
        /// Given: 出退勤時刻が同値
        /// When: Syuttaikin1を参照
        /// Then: ダッシュ「-」が返される
        /// </summary>
        [TestMethod(DisplayName = "NippouViewModel: 出退勤時刻が同値の場合、出退勤表示1はダッシュを返す")]
        public void NippouViewModel_出退勤時刻が同値の場合出退勤表示1はダッシュを返す()
        {
            // 準備 (Arrange)
            var vm = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(9, 0)
            };

            // 実行 (Act)
            var display = vm.Syuttaikin1;

            // 検証 (Assert)
            Assert.AreEqual("-", display,
                "同値の場合はダッシュであるべきです。");
        }

        /// <summary>
        /// Given: 正常な出退勤時刻
        /// When: Syuttaikin1を参照
        /// Then: 「H:mm ~ H:mm」形式で返される
        /// </summary>
        [TestMethod(DisplayName = "NippouViewModel: 正常な出退勤時刻の場合、出退勤表示1はH:mm ~ H:mm形式で返される")]
        public void NippouViewModel_正常な出退勤時刻の場合出退勤表示1はHmm形式で返される()
        {
            // 準備 (Arrange)
            var vm = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0)
            };

            // 実行 (Act)
            var display = vm.Syuttaikin1;

            // 検証 (Assert)
            Assert.AreEqual("9:00 ~ 18:00", display,
                "時刻範囲としてフォーマットされるべきです。");
        }

        /// <summary>
        /// Given: SyukkinKubun1にenumを設定
        /// When: SyukkinKubunCodeString1を参照
        /// Then: 2桁整数文字列が返される
        /// </summary>
        [TestMethod(DisplayName = "NippouViewModel: 出勤区分1にenumを設定した場合、出勤区分コード文字列1は2桁整数文字列を返す")]
        public void NippouViewModel_出勤区分1にenumを設定した場合出勤区分コード文字列1は2桁整数文字列を返す()
        {
            // 準備 (Arrange)
            var vm = new IndexModel.NippouViewModel();

            // 実行 (Act)
            vm.SyukkinKubun1 = 通常勤務;

            // 検証 (Assert)
            Assert.AreEqual("02", vm.SyukkinKubunCodeString1,
                "通常勤務は02であるべきです。");
        }

        /// <summary>
        /// Given: 出退勤2, 3がnull
        /// When: Syuttaikin2, Syuttaikin3を参照
        /// Then: いずれもダッシュ「-」が返される
        /// </summary>
        [TestMethod(DisplayName = "NippouViewModel: 出退勤2, 3がnullの場合、出退勤表示2, 3はダッシュを返す")]
        public void NippouViewModel_出退勤23がnullの場合出退勤表示23はダッシュを返す()
        {
            // 準備 (Arrange)
            var vm = new IndexModel.NippouViewModel();

            // 実行 (Act) & 検証 (Assert)
            Assert.AreEqual("-", vm.Syuttaikin2,
                "出退勤2がnullならダッシュであるべきです。");
            Assert.AreEqual("-", vm.Syuttaikin3,
                "出退勤3がnullならダッシュであるべきです。");
        }

        #endregion

        #region Project Management AJAX Tests

        /// <summary>
        /// Given: 日報案件カードが存在する
        /// When: OnPostAddNippouAnkenAsyncを実行
        /// Then: カードが1つ追加され、JsonResultが返される
        /// </summary>
        [TestMethod(DisplayName = "OnPostAddNippouAnkenAsync: 日報案件カードが存在する場合、カードが1つ追加されJsonResultが返される")]
        public async Task OnPostAddNippouAnkenAsync_日報案件カードが存在する場合カードが1つ追加されJsonResultが返される()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder().WithId(1).Build();
            var model = CreateModel(syain);
            model.NippouAnkenCards = new IndexModel.CardsViewModel();

            // 実行 (Act)
            var result = await model.OnPostAddNippouAnkenAsync();

            // 検証 (Assert)
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            Assert.HasCount(1, model.NippouAnkenCards.NippouAnkens);
        }

        /// <summary>
        /// Given: 1つの日報案件カードが存在する
        /// When: Index 0 を指定して OnPostCopyNippouAnkenAsync を実行
        /// Then: カードがコピーされ、合計2つになる
        /// </summary>
        [TestMethod(DisplayName = "OnPostCopyNippouAnkenAsync: 1つの日報案件カードが存在する場合、カードがコピーされ合計2つになる")]
        public async Task OnPostCopyNippouAnkenAsync_1つの日報案件カードが存在する場合カードがコピーされ合計2つになる()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder().WithId(1).Build();
            var model = CreateModel(syain);
            model.NippouAnkenCards = new IndexModel.CardsViewModel();
            model.NippouAnkenCards.NippouAnkens.Add(new IndexModel.NippouAnkenViewModel
            {
                AnkenName = "コピー元案件",
                KingsJuchuNo = "J001"
            });

            // 実行 (Act)
            var result = await model.OnPostCopyNippouAnkenAsync(0);

            // 検証 (Assert)
            Assert.HasCount(2, model.NippouAnkenCards.NippouAnkens);
            Assert.AreEqual("コピー元案件", model.NippouAnkenCards.NippouAnkens[1].AnkenName);
        }

        /// <summary>
        /// Given: 1つの日報案件カードが存在する
        /// When: Index 0 を指定して OnPostDeleteNippouAnkenAsync を実行
        /// Then: カードが削除され、0になる
        /// </summary>
        [TestMethod(DisplayName = "OnPostDeleteNippouAnkenAsync: 1つの日報案件カードが存在する場合、カードが削除され0になる")]
        public async Task OnPostDeleteNippouAnkenAsync_1つの日報案件カードが存在する場合カードが削除され0になる()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder().WithId(1).Build();
            var model = CreateModel(syain);
            model.NippouAnkenCards = new IndexModel.CardsViewModel();
            model.NippouAnkenCards.NippouAnkens.Add(new IndexModel.NippouAnkenViewModel());

            // 実行 (Act)
            var result = await model.OnPostDeleteNippouAnkenAsync(0);

            // 検証 (Assert)
            Assert.IsEmpty(model.NippouAnkenCards.NippouAnkens);
        }

        #endregion

        #region GetNippouDetailWorkHoursAsync Tests

        /// <summary>
        /// Given: GetNippouDetailWorkHoursAsync の条件を満たしている
        /// When: 平日半日勤務条件で第3条件trueかつ第4条件false
        /// Then: 加算ロジックが反映される
        /// </summary>
        [TestMethod(DisplayName = "GetNippouDetailWorkHoursAsync: 平日半日勤務条件で第3条件trueかつ第4条件falseの場合、加算ロジックが反映される")]
        public async Task GetNippouDetailWorkHoursAsync_平日半日勤務条件で第3条件trueかつ第4条件falseの場合加算ロジックが反映される()
        {
            var syain = new SyainBuilder().WithId(1).Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = 半日勤務,
                SyukkinKubun2 = 年次有給休暇_1日,
                SyukkinHm1 = new TimeOnly(13, 10),
                TaisyutsuHm1 = new TimeOnly(13, 20),
                HJitsudou = 0m
            };

            // 時刻の減算は24時間環状で評価されるため、条件成立を明示
            var thirdCond =
                model.NippouData.TaisyutsuHm1 - new TimeOnly(13, 0)
                < new TimeOnly(12, 0) - model.NippouData.SyukkinHm1;
            var fourthCond =
                new TimeOnly(12, 0) - model.NippouData.TaisyutsuHm1
                < model.NippouData.SyukkinHm1 - new TimeOnly(13, 0);
            Assert.IsTrue(thirdCond, "第3条件が true になる時刻を使うべきです。");
            Assert.IsFalse(fourthCond, "第4条件は false になる時刻を使うべきです。");

            await InvokePrivateAsync(
                model,
                "GetNippouDetailWorkHoursAsync",
                true,
                D("20250115"));

            Assert.AreEqual(480m, model.NippouData.HJitsudou, "各条件の加算結果が一致するべきです。");
            Assert.AreEqual(0m, model.NippouData.HZangyo, "平日残業が一致するべきです。");
            Assert.AreEqual(0m, model.NippouData.HWarimashi, "平日割増が一致するべきです。");
            Assert.AreEqual(0m, model.NippouData.HShinyaZangyo, "平日深夜残業が一致するべきです。");
            Assert.AreEqual(0m, model.NippouData.TotalZangyo, "合計残業が一致するべきです。");
        }

        /// <summary>
        /// Given: GetNippouDetailWorkHoursAsync の条件を満たしている
        /// When: 平日パート勤務条件で第4条件true
        /// Then: 第4条件加算が反映される
        /// </summary>
        [TestMethod(DisplayName = "GetNippouDetailWorkHoursAsync: 平日パート勤務条件で第4条件trueの場合、第4条件加算が反映される")]
        public async Task GetNippouDetailWorkHoursAsync_平日パート勤務条件で第4条件trueの場合第4条件加算が反映される()
        {
            var syain = new SyainBuilder().WithId(1).Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = パート勤務,
                SyukkinKubun2 = 年次有給休暇_1日,
                SyukkinHm1 = new TimeOnly(11, 0),
                TaisyutsuHm1 = new TimeOnly(11, 30),
                HJitsudou = 0m
            };

            var fourthCond =
                new TimeOnly(12, 0) - model.NippouData.TaisyutsuHm1
                < model.NippouData.SyukkinHm1 - new TimeOnly(13, 0);
            Assert.IsTrue(fourthCond, "第4条件が true になる時刻を使うべきです。");

            await InvokePrivateAsync(
                model,
                "GetNippouDetailWorkHoursAsync",
                true,
                D("20250115"));

            Assert.AreEqual(480m, model.NippouData.HJitsudou, "第1・第4条件の加算結果が一致するべきです。");
            Assert.AreEqual(0m, model.NippouData.HZangyo, "平日残業が一致するべきです。");
            Assert.AreEqual(0m, model.NippouData.TotalZangyo, "合計残業が一致するべきです。");
        }

        /// <summary>
        /// Given: GetNippouDetailWorkHoursAsync の条件を満たしている
        /// When: 土曜日
        /// Then: 土祝祭日項目が設定される
        /// </summary>
        [TestMethod(DisplayName = "GetNippouDetailWorkHoursAsync: 土曜日の場合、土祝祭日項目が設定される")]
        public async Task GetNippouDetailWorkHoursAsync_土曜日の場合土祝祭日項目が設定される()
        {
            var syain = new SyainBuilder().WithId(1).Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(20, 0),
                TaisyutsuHm1 = new TimeOnly(23, 0)
            };
            var jissekiDate = D("20250118"); // 土曜日

            await InvokePrivateAsync(
                model,
                "GetNippouDetailWorkHoursAsync",
                false,
                jissekiDate);

            int total = model.NippouData.TotalWorkingHoursInMinute;
            int nightMinute = TimeCalculator.GetIncludeTimeWithout休憩("2000", "2300", (0, 5 * 60));
            int lateNightMinute = TimeCalculator.GetIncludeTimeWithout休憩("2000", "2300", (22 * 60, 24 * 60));
            int nightOvertime = nightMinute + lateNightMinute;

            Assert.AreEqual(total, model.NippouData.DJitsudou, "土祝祭日実働が一致するべきです。");
            Assert.AreEqual(Math.Max(0m, total - 8m * 60m), model.NippouData.DZangyo, "土祝祭日残業が一致するべきです。");
            Assert.AreEqual(
                TimeCalculator.CalculationWarimashiTime(total, nightOvertime),
                model.NippouData.DWarimashi,
                "土祝祭日割増が一致するべきです。");
            Assert.AreEqual(
                TimeCalculator.CalculationShinyaCyokinTime(total, nightOvertime),
                model.NippouData.DShinyaZangyo,
                "土祝祭日深夜残業が一致するべきです。");
            Assert.AreEqual(total - 8 * 60, model.NippouData.TotalZangyo, "合計残業が一致するべきです。");
        }

        /// <summary>
        /// Given: GetNippouDetailWorkHoursAsync の条件を満たしている
        /// When: 日曜日
        /// Then: 法定休日項目が設定される
        /// </summary>
        [TestMethod(DisplayName = "GetNippouDetailWorkHoursAsync: 日曜日の場合、法定休日項目が設定される")]
        public async Task GetNippouDetailWorkHoursAsync_日曜日の場合法定休日項目が設定される()
        {
            var syain = new SyainBuilder().WithId(1).Build();
            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(22, 0),
                TaisyutsuHm1 = new TimeOnly(23, 30)
            };
            var jissekiDate = D("20250119"); // 日曜日

            await InvokePrivateAsync(
                model,
                "GetNippouDetailWorkHoursAsync",
                false,
                jissekiDate);

            int total = model.NippouData.TotalWorkingHoursInMinute;
            int nightMinute = TimeCalculator.GetIncludeTimeWithout休憩("2200", "2330", (0, 5 * 60));
            int lateNightMinute = TimeCalculator.GetIncludeTimeWithout休憩("2200", "2330", (22 * 60, 24 * 60));
            int nightOvertime = nightMinute + lateNightMinute;

            Assert.AreEqual(total, model.NippouData.NJitsudou, "法定休日実働が一致するべきです。");
            Assert.AreEqual(nightOvertime, model.NippouData.NShinya, "法定休日深夜が一致するべきです。");
            Assert.AreEqual(total, model.NippouData.TotalZangyo, "合計残業が一致するべきです。");
        }

        #endregion

        #region OnPostFinalConfirmCheckAsync Tests

        /// <summary>
        /// Given: 勤務時間と実績時間の合計に差がある
        /// When: OnPostFinalConfirmCheckAsyncを実行
        /// Then: 時間差に関する警告メッセージが返される
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmCheckAsync: 勤務時間と実績時間の合計に差がある場合、警告メッセージが返される")]
        public async Task OnPostFinalConfirmCheckAsync_勤務時間と実績時間の合計に差がある場合警告メッセージが返される()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder().WithId(1).Build();
            db.Syains.Add(syain);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            // 勤務時間: 9:00 - 10:00 (実働1時間 = 60分)
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(10, 0)
            };
            // 実績時間: 2時間 (120分)
            model.NippouAnkenCards = new IndexModel.CardsViewModel();
            model.NippouAnkenCards.NippouAnkens.Add(new IndexModel.NippouAnkenViewModel
            {
                KingsJuchuNo = "J001",
                JissekiJikan = 120
            });
            model.NippouData.TotalJissekiJikan = 2;

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmCheckAsync(syain.Id, D("20250113"), false);

            // 検証 (Assert)
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var jsonResult = (JsonResult)result;
            // 応答オブジェクトからデータ項目を取得する
            var prop =
                jsonResult.Value?.GetType().GetProperty("Data")
                ?? jsonResult.Value?.GetType().GetProperty("data");
            var dataValue = prop?.GetValue(jsonResult.Value, null) as string;
            StringAssert.Contains(dataValue, "実績の時間合計に差があります");
        }

        /// <summary>
        /// Given: 実績時間が0、受注番号が未入力の案件がある
        /// When: OnPostFinalConfirmCheckAsyncを実行
        /// Then: 必須エラーを結合したErrorJsonが返る
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmCheckAsync: 実績時間が0、受注番号が未入力の案件がある場合、必須エラーを結合したErrorJsonが返る")]
        public async Task OnPostFinalConfirmCheckAsync_実績時間が0受注番号が未入力の案件がある場合必須エラーを結合したErrorJsonが返る()
        {
            // 準備 (Arrange)
            var model = CreateModel(new SyainBuilder().WithId(1).Build());
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        JissekiJikan = 0,
                        KingsJuchuNo = null
                    }
                }
            };

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmCheckAsync(1, D("20250115"), false);

            // 検証 (Assert)
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result);
            var message = GetMessage(jsonResult);
            Assert.IsNotNull(message);
            StringAssert.Contains(message, "実績時間は必須です");
            StringAssert.Contains(message, "受注番号は必須です");
        }

        /// <summary>
        /// Given: 原価連動対象の案件が他部署受注番号である
        /// When: OnPostFinalConfirmCheckAsyncを実行
        /// Then: 他部署受注番号の確認メッセージが返る
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmCheckAsync: 原価連動対象の案件が他部署受注番号である場合、他部署受注番号の確認メッセージが返る")]
        public async Task OnPostFinalConfirmCheckAsync_原価連動対象の案件が他部署受注番号である場合他部署受注番号の確認メッセージが返る()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder()
                .WithId(1)
                .WithBusyoId(1)
                .Build();
            db.Syains.Add(syain);

            var kings = new KingsJuchuBuilder()
                .WithId(1)
                .WithBusyoId(2) // 社員部署と不一致
                .Build();
            var anken = new AnkenBuilder()
                .WithId(1)
                .WithName("他部署案件")
                .WithSearchName("他部署案件")
                .WithKingsJuchuId(kings.Id)
                .Build();
            db.KingsJuchus.Add(kings);
            db.Ankens.Add(anken);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
                TotalJissekiJikan = 1
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = true,
                        AnkensId = anken.Id,
                        KingsJuchuNo = "P-001",
                        JissekiJikan = 60
                    }
                }
            };

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmCheckAsync(syain.Id, D("20250115"), false);

            // 検証 (Assert)
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result);
            var prop =
                jsonResult.Value?.GetType().GetProperty("Data")
                ?? jsonResult.Value?.GetType().GetProperty("data");
            var dataValue = prop?.GetValue(jsonResult.Value, null) as string;
            Assert.IsNotNull(dataValue);
            StringAssert.Contains(dataValue, "他部署の受注番号が選択されています");
        }

        /// <summary>
        /// Given: リフレッシュデー判定日で、打刻退勤が画面値より15分超過し、承認済み申請がない
        /// When: OnPostFinalConfirmCheckAsyncを実行
        /// Then: リフレッシュデー補正の確認メッセージが返る
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmCheckAsync: リフレッシュデー判定日で打刻退勤が画面値より15分超過し承認済み申請がない場合、リフレッシュデー補正確認メッセージが返る")]
        public async Task OnPostFinalConfirmCheckAsync_リフレッシュデー判定日で打刻退勤が画面値より15分超過し承認済み申請がない場合リフレッシュデー補正確認メッセージが返る()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder().WithId(1).Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250115"); // 水曜日
            db.WorkingHours.Add(new WorkingHour
            {
                Id = 1,
                SyainId = syain.Id,
                Hiduke = jissekiDate,
                SyukkinTime = jissekiDate.ToDateTime(new TimeOnly(9, 0)),
                TaikinTime = jissekiDate.ToDateTime(new TimeOnly(18, 0)),
                Deleted = false
            });
            // 未承認のリフレッシュ残業申請（承認済み判定にはならない）
            db.UkagaiHeaders.Add(new UkagaiHeader
            {
                Id = 1,
                SyainId = syain.Id,
                WorkYmd = jissekiDate,
                ShinseiYmd = jissekiDate,
                Invalid = false,
                Status = 承認待,
                UkagaiShinseis = new List<UkagaiShinsei>
                {
                    new UkagaiShinsei
                    {
                        Id = 1,
                        UkagaiHeaderId = 1,
                        UkagaiSyubetsu = リフレッシュデー残業 // リフレッシュデー残業
                    }
                }
            });
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel
            {
                TaisyutsuHm1 = new TimeOnly(17, 30), // 打刻18:00との差30分
                TotalJissekiJikan = 1
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        KingsJuchuNo = "J001",
                        JissekiJikan = 60,
                        IsLinked = false
                    }
                }
            };

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmCheckAsync(syain.Id, jissekiDate, false);

            // 検証 (Assert)
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result);
            var prop =
                jsonResult.Value?.GetType().GetProperty("Data")
                ?? jsonResult.Value?.GetType().GetProperty("data");
            var dataValue = prop?.GetValue(jsonResult.Value, null) as string;
            Assert.IsNotNull(dataValue);
            StringAssert.Contains(dataValue, "リフレッシュデーの時間外労働申請が行われていない");
        }

        /// <summary>
        /// Given: 勤務時間より実績合計時間が大きい（かつ他の警告条件に該当しない）
        /// When: OnPostFinalConfirmCheckAsyncを実行
        /// Then: 勤務時間差の確認メッセージが返る
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmCheckAsync: 勤務時間より実績合計時間が大きい場合、勤務時間差の確認メッセージが返る")]
        public async Task OnPostFinalConfirmCheckAsync_勤務時間より実績合計時間が大きい場合勤務時間差の確認メッセージが返る()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder().WithId(1).Build();
            db.Syains.Add(syain);
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(10, 0), // 60分
                TotalJissekiJikan = 2 // 120分
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        KingsJuchuNo = "J001",
                        JissekiJikan = 120,
                        IsLinked = false
                    }
                }
            };

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmCheckAsync(syain.Id, D("20250113"), false); // 月曜日

            // 検証 (Assert)
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result);
            var prop =
                jsonResult.Value?.GetType().GetProperty("Data")
                ?? jsonResult.Value?.GetType().GetProperty("data");
            var dataValue = prop?.GetValue(jsonResult.Value, null) as string;
            Assert.IsNotNull(dataValue);
            StringAssert.Contains(dataValue, "勤務時間と実績の時間合計に差があります");
        }

        #endregion

        #region Leave Management Confirmation Tests

        /// <summary>
        /// Given: 振休残データ（未取得）が存在する
        /// When: 確定処理（振替休暇を選択）を実行
        /// Then: 振休残データの取得日が更新され、ステータスが「1日」になる
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 振休残データ（未取得）が存在する場合、取得日が更新されステータスが「1日」になる")]
        public async Task OnPostFinalConfirmAsync_振休残データ未取得が存在する場合取得日が更新されステータスが1日になる()
        {
            // 準備 (Arrange)
            var (syain, _, anken) = SeedFinalConfirmMinimumData();
            AddSyukkinKubun(振替休暇, isSyukkin: false, isVacation: true);

            var jissekiDate = D("20250115");
            var furikyuu = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = jissekiDate.AddDays(-7),
                DaikyuuKigenYmd = jissekiDate.AddMonths(1),
                SyutokuState = 未,
                IsOneDay = true
            };
            db.FurikyuuZans.Add(furikyuu);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = 振替休暇,
                SyukkinKubun2 = None,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = false,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 480
                    }
                }
            };

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmAsync(syain.Id, jissekiDate, false);
            AssertFinalConfirmSuccess(result);

            // 検証 (Assert)
            var updated = await db.FurikyuuZans.FindAsync(furikyuu.Id);
            Assert.IsNotNull(updated);
            Assert.AreEqual(jissekiDate, updated!.SyutokuYmd1, "取得日が更新されるべきです。");
            Assert.AreEqual(_1日, updated!.SyutokuState, "ステータスが1日になるべきです。");
        }

        /// <summary>
        /// Given: 振休残データ（未取得）が存在する
        /// When: 確定処理（半日振休を選択）を実行
        /// Then: 振休残データの取得日が更新され、ステータスが「半日」になる
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 半日振休を選択した場合、振休残データの取得日が更新されステータスが半日になる")]
        public async Task OnPostFinalConfirmAsync_半日振休を選択した場合振休残データの取得日が更新されステータスが半日になる()
        {
            // 準備 (Arrange)
            var (syain, _, anken) = SeedFinalConfirmMinimumData();
            AddSyukkinKubun(半日振休, isSyukkin: false, isVacation: true);

            var jissekiDate = D("20250115");
            var furikyuu = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = jissekiDate.AddDays(-7),
                DaikyuuKigenYmd = jissekiDate.AddMonths(1),
                SyutokuState = 未,
                IsOneDay = true
            };
            db.FurikyuuZans.Add(furikyuu);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = 半日振休,
                SyukkinKubun2 = None,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(13, 0),
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = false,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 240
                    }
                }
            };

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmAsync(syain.Id, jissekiDate, false);
            AssertFinalConfirmSuccess(result);

            // 検証 (Assert)
            var updated = await db.FurikyuuZans.FindAsync(furikyuu.Id);
            Assert.IsNotNull(updated);
            Assert.AreEqual(jissekiDate, updated!.SyutokuYmd1, "取得日が更新されるべきです。");
            Assert.AreEqual(半日, updated!.SyutokuState, "ステータスが半日になるべきです。");
        }

        /// <summary>
        /// Given: 有給残データが存在する
        /// When: 確定処理（年次有給休暇_1日を選択）を実行
        /// Then: 有給残データの消化数が増加する
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 年次有給休暇_1日を選択した場合、有給残データの消化数が増加する")]
        public async Task OnPostFinalConfirmAsync_年次有給休暇_1日を選択した場合有給残データの消化数が増加する()
        {
            // 準備 (Arrange)
            var (syain, _, anken) = SeedFinalConfirmMinimumData();
            AddSyukkinKubun(年次有給休暇_1日, isSyukkin: false, isVacation: true);

            var yukyuu = new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 0m,
                Kurikoshi = 1m,
                KeikakuYukyuSu = 0,
                Syouka = 0
            };
            db.YuukyuuZans.Add(yukyuu);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = 年次有給休暇_1日,
                SyukkinKubun2 = None,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = false,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 480
                    }
                }
            };

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmAsync(syain.Id, D("20250115"), false);
            AssertFinalConfirmSuccess(result);

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstOrDefaultAsync(x => x.SyainBaseId == syain.SyainBaseId);
            Assert.IsNotNull(updated);
            Assert.AreEqual(1m, updated!.Syouka, "消化数が増加するべきです。");
        }

        /// <summary>
        /// Given: 有給残データが存在する
        /// When: 確定処理（計画有給休暇を選択）を実行
        /// Then: 計画有給数が増加する
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 計画有給休暇を選択した場合、計画有給数が増加する")]
        public async Task OnPostFinalConfirmAsync_計画有給休暇を選択した場合計画有給数が増加する()
        {
            // 準備 (Arrange)
            var (syain, _, anken) = SeedFinalConfirmMinimumData();
            AddSyukkinKubun(計画有給休暇, isSyukkin: false, isVacation: true);

            var yukyuu = new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 0m,
                Kurikoshi = 1m,
                KeikakuYukyuSu = 0
            };
            db.YuukyuuZans.Add(yukyuu);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = 計画有給休暇,
                SyukkinKubun2 = None,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0),
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = false,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 480
                    }
                }
            };

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmAsync(syain.Id, D("20250115"), false);
            AssertFinalConfirmSuccess(result);

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstOrDefaultAsync(x => x.SyainBaseId == syain.SyainBaseId);
            Assert.IsNotNull(updated);
            Assert.AreEqual(1m, updated!.KeikakuYukyuSu, "計画有給数が増加するべきです。");
        }

        /// <summary>
        /// Given: 有給残データが存在する
        /// When: 確定処理（半日有給を選択）を実行
        /// Then: 有給消化数(0.5)と半日回数が増加する
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 半日有給を選択した場合、有給消化数(0.5)と半日回数が増加する")]
        public async Task OnPostFinalConfirmAsync_半日有給を選択した場合有給消化数05と半日回数が増加する()
        {
            // 準備 (Arrange)
            var (syain, _, anken) = SeedFinalConfirmMinimumData();
            AddSyukkinKubun(半日有給, isSyukkin: false, isVacation: true);

            var yukyuu = new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Wariate = 0m,
                Kurikoshi = 1m,
                KeikakuYukyuSu = 0,
                Syouka = 0,
                HannitiKaisuu = 0
            };
            db.YuukyuuZans.Add(yukyuu);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = 半日有給,
                SyukkinKubun2 = None,
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(13, 0)
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = false,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 240
                    }
                }
            };

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmAsync(syain.Id, D("20250115"), false);
            AssertFinalConfirmSuccess(result);

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstOrDefaultAsync(x => x.SyainBaseId == syain.SyainBaseId);
            Assert.IsNotNull(updated);
            Assert.AreEqual(0.5m, updated!.Syouka, "消化数が0.5増加するべきです。");
            Assert.AreEqual(1, updated!.HannitiKaisuu, "半日回数が増加するべきです。");
        }

        /// <summary>
        /// Given: 有給残データが存在する
        /// When: 確定処理（計画特別休暇を選択）を実行
        /// Then: 計画特別休暇数が増加する
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 計画特別休暇を選択した場合、計画特別休暇数が増加する")]
        public async Task OnPostFinalConfirmAsync_計画特別休暇を選択した場合計画特別休暇数が増加する()
        {
            // 準備 (Arrange)
            var (syain, _, anken) = SeedFinalConfirmMinimumData();
            AddSyukkinKubun(計画特別休暇, isSyukkin: false, isVacation: true);

            var yukyuu = new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuYukyuSu = 0,
                KeikakuTokukyuSu = 0
            };
            db.YuukyuuZans.Add(yukyuu);

            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinKubun1 = 計画特別休暇,
                SyukkinKubun2 = None
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = false,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 480
                    }
                }
            };

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmAsync(syain.Id, D("20250115"), false);

            // 検証 (Assert)
            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.エラー, GetResponseStatus(json));
            StringAssert.Contains(
                GetMessage(json) ?? string.Empty,
                GetConfirmValidationConstant("InvalidAttendanceClassification"));
            var updated = await db.YuukyuuZans.FirstOrDefaultAsync(x => x.SyainBaseId == syain.SyainBaseId);
            Assert.AreEqual(0m, updated!.KeikakuTokukyuSu, "計画特別休暇数は更新されないべきです。");
        }

        /// <summary>
        /// Given: 休日（土曜祝日）に休日出勤
        /// When: 確定処理を実行
        /// Then: 新規振休残データが作成される
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 休日出勤の場合、新規振休残データが作成される")]
        public async Task OnPostFinalConfirmAsync_休日出勤の場合新規振休残データが作成される()
        {
            // 準備 (Arrange)
            var (syain, _, anken) = SeedFinalConfirmMinimumData();
            AddSyukkinKubun(AttendanceClassification.休日出勤, isSyukkin: true, isVacation: false);

            var jissekiDate = D("20250111"); // 土曜日
            db.Hikadoubis.Add(new Hikadoubi { Ymd = jissekiDate });
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.FuriYoteiDate = jissekiDate.AddDays(7);
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(18, 0), // 9時間 = 540分 (> 240分)
                SyukkinKubun1 = AttendanceClassification.休日出勤,
                SyukkinKubun2 = None
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = true,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 540
                    }
                }
            };

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmAsync(syain.Id, jissekiDate, false);
            AssertFinalConfirmSuccess(result);

            // 検証 (Assert)
            var newFurikyuu = await db.FurikyuuZans.FirstOrDefaultAsync(x => x.KyuujitsuSyukkinYmd == jissekiDate);
            Assert.IsNotNull(newFurikyuu, "新規振休残が作成されるべきです。");
            Assert.IsTrue(newFurikyuu.IsOneDay, "1日の振休として作成されるべきです。");
        }

        /// <summary>
        /// Given: 振休残が既に9.5日ある状態
        /// When: 確定処理で新たに半日振休を作成（合計10日）
        /// Then: 部門長宛の通知メッセージが作成される
        /// </summary>
        [TestMethod(DisplayName = "OnPostFinalConfirmAsync: 振休残が既に9.5日ある状態で半日振休を作成した場合、部門長宛の通知メッセージが作成される")]
        public async Task OnPostFinalConfirmAsync_振休残が既に95日ある状態で半日振休を作成した場合部門長宛の通知メッセージが作成される()
        {
            // 準備 (Arrange)
            var (syain, _, anken) = SeedFinalConfirmMinimumData(syainId: 1, syainBaseId: 1, busyoId: 10);
            AddSyukkinKubun(AttendanceClassification.休日出勤, isSyukkin: true, isVacation: false);

            var bumonchoBase = new SyainBasisBuilder().WithId(100).WithCode("B100").WithName("部門長Base").Build();
            db.SyainBases.Add(bumonchoBase);
            var bumoncho = new SyainBuilder()
                .WithId(1000)
                .WithSyainBaseId(bumonchoBase.Id)
                .WithBusyoId(10)
                .WithKintaiZokuseiId(5)
                .WithStartYmd(DateTime.Today.ToDateOnly().AddYears(-1))
                .WithEndYmd(DateTime.Today.ToDateOnly().AddYears(1))
                .WithName("部門長")
                .Build();
            bumoncho.EMail = "boss@test.com";
            db.Syains.Add(bumoncho);

            var busyoBase = new BusyoBasisBuilder().WithId(10).Build();
            busyoBase.BumoncyoId = bumonchoBase.Id;
            db.BusyoBases.Add(busyoBase);
            db.Busyos.Add(new BusyoBuilder()
                .WithId(10)
                .WithBusyoBaseId(10)
                .WithName("テスト部署")
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build());

            // 既存の振休残 9.5日分 (1日x9, 半日x1)
            for (int i = 1; i <= 9; i++)
            {
                db.FurikyuuZans.Add(new FurikyuuZan
                {
                    SyainId = syain.Id,
                    KyuujitsuSyukkinYmd = D("20250101").AddDays(-i),
                    DaikyuuKigenYmd = D("20250630"),
                    SyutokuState = 未,
                    IsOneDay = true
                });
            }
            db.FurikyuuZans.Add(new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101").AddDays(-10),
                DaikyuuKigenYmd = D("20250630"),
                SyutokuState = 未,
                IsOneDay = false
            });

            var jissekiDate = D("20250111"); // 土曜日
            db.Hikadoubis.Add(new Hikadoubi { Ymd = jissekiDate });
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            model.FuriYoteiDate = jissekiDate.AddDays(7);
            model.NippouData = new IndexModel.NippouViewModel
            {
                SyukkinHm1 = new TimeOnly(9, 0),
                TaisyutsuHm1 = new TimeOnly(12, 0), // 3時間 = 180分 (< 240分 -> 半日)
                SyukkinKubun1 = AttendanceClassification.休日出勤,
                SyukkinKubun2 = None
            };
            model.NippouAnkenCards = new IndexModel.CardsViewModel
            {
                NippouAnkens = new List<IndexModel.NippouAnkenViewModel>
                {
                    new IndexModel.NippouAnkenViewModel
                    {
                        IsLinked = true,
                        AnkensId = anken.Id,
                        KokyakuKaisyaId = 1,
                        KokyakuName = "顧客A",
                        AnkenName = "案件A",
                        JissekiJikan = 180
                    }
                }
            };

            // 実行 (Act)
            var result = await model.OnPostFinalConfirmAsync(syain.Id, jissekiDate, false);
            AssertFinalConfirmSuccess(result);

            // 検証 (Assert)
            var notification = await db.MessageContents.FirstOrDefaultAsync();
            Assert.IsNotNull(notification, "通知メッセージが作成されるべきです。");
            Assert.AreEqual(bumoncho.Id, notification.SyainId, "通知先が部門長であるべきです。");
            StringAssert.Contains(notification.Content, "振替休暇残日数が", "通知内容が正しいべきです。");
        }


        #endregion

        #region Leave Management Cancellation Tests

        /// <summary>
        /// Given: 有給取得済みの確定日報
        /// When: 確定解除を実行
        /// Then: 有給残データの消化数が減少する
        /// </summary>
        [TestMethod(DisplayName = "OnPostCancelConfirmAsync: 有給取得済みの確定日報を取消した場合、有給残データの消化数が減少する")]
        public async Task OnPostCancelConfirmAsync_有給取得済みの確定日報を取消した場合有給残データの消化数が減少する()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250120");

            var yukyuu = new YuukyuuZan
            {
                SyainBaseId = 1,
                Syouka = 5.0m
            };
            db.YuukyuuZans.Add(yukyuu);

            await db.SaveChangesAsync();

            // 実行 (Act)
            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                年次有給休暇_1日,
                None,
                isWorkDay: true);
            await leave.UpdateCancelConfirmLeaveAsync(syain.SyainBaseId, jissekiDate);

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstOrDefaultAsync(x => x.SyainBaseId == 1);
            Assert.IsNotNull(updated);
            Assert.AreEqual(4.0m, updated!.Syouka, "消化数が1減るべきです。");
        }

        /// <summary>
        /// Given: 計画有給取得済みの確定日報
        /// When: 確定解除を実行
        /// Then: 計画有給数と消化数が減少する
        /// </summary>
        [TestMethod(DisplayName = "OnPostCancelConfirmAsync: 計画有給取得済みの確定日報を取消した場合、計画有給数と消化数が減少する")]
        public async Task OnPostCancelConfirmAsync_計画有給取得済みの確定日報を取消した場合計画有給数と消化数が減少する()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250120");

            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = 1,
                Syouka = 5.0m,
                KeikakuYukyuSu = 2
            });

            await db.SaveChangesAsync();

            // 実行 (Act)
            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                計画有給休暇,
                None,
                isWorkDay: true);
            await leave.UpdateCancelConfirmLeaveAsync(syain.SyainBaseId, jissekiDate);

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstOrDefaultAsync(x => x.SyainBaseId == 1);
            Assert.IsNotNull(updated);
            Assert.AreEqual(4.0m, updated!.Syouka, "消化数が1減るべきです。");
            Assert.AreEqual(1.0m, updated!.KeikakuYukyuSu, "計画有給数が1減るべきです。");
        }

        /// <summary>
        /// Given: 振休取得済みの確定日報
        /// When: 確定解除を実行
        /// Then: 振休残データの取得日がnullになり、ステータスが「未」になる
        /// </summary>
        [TestMethod(DisplayName = "OnPostCancelConfirmAsync: 振休取得済みの確定日報を取消した場合、振休残データの取得日がnullになりステータスが未になる")]
        public async Task OnPostCancelConfirmAsync_振休取得済みの確定日報を取消した場合振休残データ取得日がnullになりステータスが未になる()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250120");

            var furikyuu = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                SyutokuYmd1 = jissekiDate,
                SyutokuState = _1日,
                IsOneDay = true
            };
            db.FurikyuuZans.Add(furikyuu);

            await db.SaveChangesAsync();

            // 実行 (Act)
            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                振替休暇,
                None,
                isWorkDay: true);
            await leave.UpdateCancelConfirmLeaveAsync(syain.SyainBaseId, jissekiDate);

            // 検証 (Assert)
            var updated = await db.FurikyuuZans.FindAsync(furikyuu.Id);
            Assert.IsNotNull(updated);
            Assert.IsNull(updated!.SyutokuYmd1, "取得日がクリアされるべきです。");
            Assert.AreEqual(未, updated!.SyutokuState, "ステータスが未になるべきです。");
        }

        /// <summary>
        /// Given: 休日出勤により振休残が作成された確定日報
        /// When: 確定解除を実行
        /// Then: 作成されていた振休残データが削除される
        /// </summary>
        [TestMethod(DisplayName = "OnPostCancelConfirmAsync: 休日出勤で作成された確定日報を取消した場合、作成されていた振休残データが削除される")]
        public async Task OnPostCancelConfirmAsync_休日出勤で作成された確定日報を取消した場合作成されていた振休残データが削除される()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250111"); // 土曜日
            db.Hikadoubis.Add(new Hikadoubi { Ymd = jissekiDate });

            var furikyuu = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = jissekiDate,
                DaikyuuKigenYmd = D("20251231"),
                SyutokuState = 未,
                IsOneDay = true
            };
            db.FurikyuuZans.Add(furikyuu);

            await db.SaveChangesAsync();

            // 実行 (Act)
            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                AttendanceClassification.休日出勤,
                None,
                isWorkDay: false);
            await leave.UpdateCancelConfirmLeaveAsync(syain.SyainBaseId, jissekiDate);

            // 検証 (Assert)
            var deleted = await db.FurikyuuZans.FirstOrDefaultAsync(x => x.KyuujitsuSyukkinYmd == jissekiDate);
            Assert.IsNull(deleted, "作成された振休残が削除されるべきです。");
        }

        #endregion

        #region CompensatoryPaidLeave Branch Tests

        /// <summary>
        /// Given: 1日振休残が半日取得済みで、別の未取得残がある
        /// When: 振替休暇の確定処理を実行
        /// Then: 1件目は1日化され、2件目で半日が取得される
        /// </summary>
        [TestMethod(DisplayName = "UpdateConfirmLeaveAsync: 1日振休残が半日取得済みで別の未取得残がある場合、" +
            "1件目は1日化され2件目で半日が取得される")]
        public async Task UpdateConfirmLeaveAsync_1日振休残が半日取得済みで別の未取得残がある場合1件目は1日化され2件目で半日が取得される()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250120");
            var first = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 半日,
                SyutokuYmd1 = D("20250110")
            };
            var second = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250102"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 未
            };
            db.FurikyuuZans.Add(first);
            db.FurikyuuZans.Add(second);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                振替休暇,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateConfirmLeaveAsync();

            // 検証 (Assert)
            var firstUpdated = await db.FurikyuuZans.FindAsync(first.Id);
            var secondUpdated = await db.FurikyuuZans.FindAsync(second.Id);
            Assert.IsNotNull(firstUpdated);
            Assert.IsNotNull(secondUpdated);
            Assert.AreEqual(jissekiDate, firstUpdated!.SyutokuYmd2, "1件目の2回目取得日が設定されるべきです。");
            Assert.AreEqual(_1日, firstUpdated!.SyutokuState, "1件目は1日取得になるべきです。");
            Assert.AreEqual(jissekiDate, secondUpdated!.SyutokuYmd1, "2件目で半日取得されるべきです。");
            Assert.AreEqual(半日, secondUpdated!.SyutokuState, "2件目は半日状態になるべきです。");
        }

        /// <summary>
        /// Given: 半日振休残(0.5日)が未取得で、別の1日残が未取得
        /// When: 振替休暇の確定処理を実行
        /// Then: 半日残は1日化され、別残で半日取得される
        /// </summary>
        [TestMethod(DisplayName = "UpdateConfirmLeaveAsync: 半日振休残が未取得で別の1日残も未取得の場合、半日残が1日化され別残で半日取得される")]
        public async Task UpdateConfirmLeaveAsync_半日振休残が未取得で別の1日残も未取得の場合半日残が1日化され別残で半日取得される()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250120");
            var first = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = 未
            };
            var second = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250102"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 未
            };
            db.FurikyuuZans.Add(first);
            db.FurikyuuZans.Add(second);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                振替休暇,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateConfirmLeaveAsync();

            // 検証 (Assert)
            var firstUpdated = await db.FurikyuuZans.FindAsync(first.Id);
            var secondUpdated = await db.FurikyuuZans.FindAsync(second.Id);
            Assert.IsNotNull(firstUpdated);
            Assert.IsNotNull(secondUpdated);
            Assert.AreEqual(jissekiDate, firstUpdated!.SyutokuYmd1, "半日残の取得日が設定されるべきです。");
            Assert.AreEqual(_1日, firstUpdated!.SyutokuState, "半日残は1日化されるべきです。");
            Assert.AreEqual(jissekiDate, secondUpdated!.SyutokuYmd1, "別残で半日取得されるべきです。");
            Assert.AreEqual(半日, secondUpdated!.SyutokuState, "別残は半日状態になるべきです。");
        }

        /// <summary>
        /// Given: 1日振休残が半日取得済み
        /// When: 半日振休の確定処理を実行
        /// Then: 2回目取得日が設定され、1日取得状態になる
        /// </summary>
        [TestMethod(DisplayName = "UpdateConfirmLeaveAsync: 半日振休残が半日取得済みの場合、2回目取得日が設定され1日取得状態になる")]
        public async Task UpdateConfirmLeaveAsync_半日振休残が半日取得済みの場合2回目取得日が設定され1日取得状態になる()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder().WithId(1).WithSyainBaseId(1).Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250120");
            var furikyuu = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 半日,
                SyutokuYmd1 = D("20250110")
            };
            db.FurikyuuZans.Add(furikyuu);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                半日振休,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateConfirmLeaveAsync();

            // 検証 (Assert)
            var updated = await db.FurikyuuZans.FindAsync(furikyuu.Id);
            Assert.IsNotNull(updated);
            Assert.AreEqual(jissekiDate, updated!.SyutokuYmd2, "2回目取得日が設定されるべきです。");
            Assert.AreEqual(_1日, updated!.SyutokuState, "1日取得状態になるべきです。");
        }

        /// <summary>
        /// Given: 半日振休残(0.5日)が未取得
        /// When: 半日振休の確定処理を実行
        /// Then: 取得日が設定され、1日取得状態になる
        /// </summary>
        [TestMethod(DisplayName = "UpdateConfirmLeaveAsync: 半日振休残(0.5日)が未取得の場合、取得日が設定され1日取得状態になる")]
        public async Task UpdateConfirmLeaveAsync_半日振休残が未取得の場合取得日が設定され1日取得状態になる()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder().WithId(1).WithSyainBaseId(1).Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250120");
            var furikyuu = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = 未
            };
            db.FurikyuuZans.Add(furikyuu);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                半日振休,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateConfirmLeaveAsync();

            // 検証 (Assert)
            var updated = await db.FurikyuuZans.FindAsync(furikyuu.Id);
            Assert.IsNotNull(updated);
            Assert.AreEqual(jissekiDate, updated!.SyutokuYmd1, "取得日が設定されるべきです。");
            Assert.AreEqual(_1日, updated!.SyutokuState, "1日取得状態になるべきです。");
        }

        /// <summary>
        /// Given: TakeFurikyuuAsync対象が「1日残・未取得」で、別の未取得残も存在する
        /// When: TakeFurikyuuAsyncを直接実行
        /// Then: 対象のみ1日取得となり、別残は未取得のまま（重複条件の分岐は実行されない）
        /// </summary>
        [TestMethod(DisplayName = "TakeFurikyuuAsync: 1日振休残が未取得で別の未取得残がある場合、対象のみ1日取得となり別残は未取得のまま")]
        public async Task TakeFurikyuuAsync_1日振休残が未取得で別の未取得残がある場合対象のみ1日取得となり別残は未取得のまま()
        {
            var syain = new SyainBuilder().WithId(1).WithSyainBaseId(1).Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250120");
            var primary = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 未
            };
            var secondary = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250102"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 未
            };
            db.FurikyuuZans.AddRange(primary, secondary);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                None,
                None,
                isWorkDay: true);

            await InvokePrivateAsync(leave, "TakeFurikyuuAsync", syain.Id, jissekiDate);

            var primaryUpdated = await db.FurikyuuZans.FindAsync(primary.Id);
            var secondaryUpdated = await db.FurikyuuZans.FindAsync(secondary.Id);

            Assert.AreEqual(jissekiDate, primaryUpdated!.SyutokuYmd1, "対象レコードは取得日1が設定されるべきです。");
            Assert.AreEqual(_1日, primaryUpdated.SyutokuState, "対象レコードは1日取得になるべきです。");

            Assert.IsNull(secondaryUpdated!.SyutokuYmd1, "重複条件分岐が実行されなければ別残は更新されないはずです。");
            Assert.AreEqual(未, secondaryUpdated.SyutokuState, "別残は未取得のままであるべきです。");
        }

        /// <summary>
        /// Given: 指定社員が存在しない
        /// When: 確定時休暇更新処理を実行
        /// Then: 何も更新されず終了する
        /// </summary>
        [TestMethod(DisplayName = "UpdateConfirmLeaveAsync: 指定社員が存在しない場合、何も更新されず終了する")]
        public async Task UpdateConfirmLeaveAsync_指定社員が存在しない場合何も更新されず終了する()
        {
            // 準備 (Arrange)
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = 1,
                Syouka = 0m
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                syainId: 9999,
                年次有給休暇_1日,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateConfirmLeaveAsync();

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstAsync(x => x.SyainBaseId == 1);
            Assert.AreEqual(0m, updated.Syouka, "社員が存在しない場合は更新されないべきです。");
        }

        /// <summary>
        /// Given: 区分2に年次有給休暇（1日）を指定
        /// When: 確定時休暇更新処理を実行
        /// Then: 有給消化数が1増加する
        /// </summary>
        [TestMethod(DisplayName = "UpdateConfirmLeaveAsync: 区分2に年次有給休暇（1日）を指定した場合、有給消化数が1増加する")]
        public async Task UpdateConfirmLeaveAsync_区分2に年次有給休暇1日を指定した場合有給消化数が1増加する()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder().WithId(1).WithSyainBaseId(1).Build();
            db.Syains.Add(syain);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = 1,
                Syouka = 2m
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                syain.Id,
                通常勤務,
                年次有給休暇_1日,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateConfirmLeaveAsync();

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstAsync(x => x.SyainBaseId == 1);
            Assert.AreEqual(3m, updated.Syouka, "区分2年次有給で消化数が1増えるべきです。");
        }

        /// <summary>
        /// Given: 区分1に計画有給休暇を指定
        /// When: 確定時休暇更新処理を実行
        /// Then: 計画有給数が1増加する
        /// </summary>
        [TestMethod(DisplayName = "UpdateConfirmLeaveAsync: 区分1に計画有給休暇を指定した場合、計画有給数が1増加する")]
        public async Task UpdateConfirmLeaveAsync_区分1に計画有給休暇を指定した場合計画有給数が1増加する()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder().WithId(1).WithSyainBaseId(1).Build();
            db.Syains.Add(syain);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = 1,
                KeikakuYukyuSu = 1
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                syain.Id,
                計画有給休暇,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateConfirmLeaveAsync();

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstAsync(x => x.SyainBaseId == 1);
            Assert.AreEqual(2, updated.KeikakuYukyuSu, "計画有給数が1増えるべきです。");
        }

        /// <summary>
        /// Given: 区分2に半日有給を指定
        /// When: 確定時休暇更新処理を実行
        /// Then: 消化数が0.5、半日回数が1増加する
        /// </summary>
        [TestMethod(DisplayName = "UpdateConfirmLeaveAsync: 区分2に半日有給を指定した場合、消化数が0.5増加し半日回数が1増加する")]
        public async Task UpdateConfirmLeaveAsync_区分2に半日有給を指定した場合消化数が0_5増加し半日回数が1増加する()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder().WithId(1).WithSyainBaseId(1).Build();
            db.Syains.Add(syain);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = 1,
                Syouka = 1.0m,
                HannitiKaisuu = 2
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                syain.Id,
                通常勤務,
                半日有給,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateConfirmLeaveAsync();

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstAsync(x => x.SyainBaseId == 1);
            Assert.AreEqual(1.5m, updated.Syouka, "消化数が0.5増えるべきです。");
            Assert.AreEqual(3, updated.HannitiKaisuu, "半日回数が1増えるべきです。");
        }

        /// <summary>
        /// Given: 区分1に計画特別休暇を指定
        /// When: 確定時休暇更新処理を実行
        /// Then: 計画特別休暇数が1増加する
        /// </summary>
        [TestMethod(DisplayName = "UpdateConfirmLeaveAsync: 区分1に計画特別休暇を指定した場合、計画特別休暇数が1増加する")]
        public async Task UpdateConfirmLeaveAsync_区分1に計画特別休暇を指定した場合計画特別休暇数が1増加する()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder().WithId(1).WithSyainBaseId(1).Build();
            db.Syains.Add(syain);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = 1,
                KeikakuTokukyuSu = 1
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                syain.Id,
                計画特別休暇,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateConfirmLeaveAsync();

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstAsync(x => x.SyainBaseId == 1);
            Assert.AreEqual(2, updated.KeikakuTokukyuSu, "計画特別休暇数が1増えるべきです。");
        }

        /// <summary>
        /// Given: 土曜日に休日出勤（非稼働日）
        /// When: 確定時休暇更新処理を実行
        /// Then: 新規振休残が作成される
        /// </summary>
        [TestMethod(DisplayName = "UpdateConfirmLeaveAsync: 土曜日に休日出勤（非稼働日）の場合、新規振休残が作成される")]
        public async Task UpdateConfirmLeaveAsync_土曜日に休日出勤非稼働日の場合新規振休残が作成される()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);
            await db.SaveChangesAsync();

            var jissekiDate = D("20250111"); // 土曜日
            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                AttendanceClassification.休日出勤,
                None,
                isWorkDay: false,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0));

            // 実行 (Act)
            await leave.UpdateConfirmLeaveAsync();

            // 検証 (Assert)
            var created = await db.FurikyuuZans.FirstOrDefaultAsync(x =>
                x.SyainId == syain.Id &&
                x.KyuujitsuSyukkinYmd == jissekiDate);
            Assert.IsNotNull(created, "土曜休日出勤で振休残が作成されるべきです。");
            Assert.IsTrue(created.IsOneDay, "9時間勤務のため1日振休として作成されるべきです。");
        }

        /// <summary>
        /// Given: 日曜日に管理社員が休日出勤
        /// When: 確定処理を実行
        /// Then: 新規の振休残が作成される
        /// </summary>
        [TestMethod(DisplayName = "UpdateConfirmLeaveAsync: 日曜日に管理社員が休日出勤の場合、新規振休残が作成される")]
        public async Task UpdateConfirmLeaveAsync_日曜日に管理社員が休日出勤の場合新規振休残が作成される()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .Build();
            db.Syains.Add(syain);
            await db.SaveChangesAsync();

            var jissekiDate = D("20250111").AddDays(1); // 日曜日
            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                AttendanceClassification.休日出勤,
                None,
                isWorkDay: true,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(12, 0));

            // 実行 (Act)
            await leave.UpdateConfirmLeaveAsync();

            // 検証 (Assert)
            var created = await db.FurikyuuZans.FirstOrDefaultAsync(x =>
                x.SyainId == syain.Id && x.KyuujitsuSyukkinYmd == jissekiDate);
            Assert.IsNotNull(created, "日曜日の休日出勤で振休残が作成されるべきです。");
            Assert.IsFalse(created.IsOneDay, "3時間勤務のため半日振休として作成されるべきです。");
        }

        /// <summary>
        /// Given: 振休残作成APIを呼び出す
        /// When: 労働時間240分で新規振休残を作成
        /// Then: 半日振休残が作成され、予定日が保存される
        /// </summary>
        [TestMethod(DisplayName = "CreateNewFurikyuuAsync: 労働時間240分で新規振休残を作成した場合、半日振休残が作成され、予定日が保存される")]
        public async Task CreateNewFurikyuuAsync_労働時間240分で新規振休残を作成した場合半日振休残が作成され予定日が保存される()
        {
            // 準備 (Arrange)
            var syain = new SyainBuilder().WithId(1).Build();
            db.Syains.Add(syain);
            await db.SaveChangesAsync();

            var jissekiDate = D("20250120");
            var yoteiDate = D("20250210");
            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                None);

            // 実行 (Act)
            await leave.CreateNewFurikyuuAsync(syain.Id, jissekiDate, 240, yoteiDate);

            // 検証 (Assert)
            var created = await db.FurikyuuZans.FirstOrDefaultAsync(x =>
                x.SyainId == syain.Id && x.KyuujitsuSyukkinYmd == jissekiDate);
            Assert.IsNotNull(created, "振休残が作成されるべきです。");
            Assert.IsFalse(created.IsOneDay, "240分勤務は半日振休になるべきです。");
            Assert.AreEqual(yoteiDate, created.SyutokuYoteiYmd, "取得予定日が保存されるべきです。");
        }

        /// <summary>
        /// Given: 土曜日の休日出勤で作成済み振休残が存在する
        /// When: 取消確定時の休暇更新を実行
        /// Then: 作成済み振休残が削除される
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 土曜日の休日出勤取消の場合、作成済み振休残が削除される")]
        public async Task UpdateCancelConfirmLeaveAsync_土曜日の休日出勤取消の場合作成済み振休残が削除される()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250111"); // 土曜日
            db.FurikyuuZans.Add(new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = jissekiDate,
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 未
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                AttendanceClassification.休日出勤,
                None,
                isWorkDay: false);

            // 実行 (Act)
            await leave.UpdateCancelConfirmLeaveAsync(syain.SyainBaseId, jissekiDate);

            // 検証 (Assert)
            var deleted = await db.FurikyuuZans.FirstOrDefaultAsync(x =>
                x.SyainId == syain.Id && x.KyuujitsuSyukkinYmd == jissekiDate);
            Assert.IsNull(deleted, "土曜休日出勤取消で振休残が削除されるべきです。");
        }

        /// <summary>
        /// Given: 日曜日の休日出勤で作成済み振休残が存在し、社員は管理職
        /// When: 取消確定時の休暇更新を実行
        /// Then: 作成済み振休残が削除される
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 日曜日の休日出勤取消で作成済み振休残ありかつ管理職の場合、" +
            "作成済み振休残が削除される")]
        public async Task UpdateCancelConfirmLeaveAsync_日曜日の休日出勤取消で作成済み振休残ありかつ管理職の場合作成済み振休残が削除される()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(4);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(4)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250112"); // 日曜日
            db.FurikyuuZans.Add(new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = jissekiDate,
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = 未
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                AttendanceClassification.休日出勤,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateCancelConfirmLeaveAsync(syain.SyainBaseId, jissekiDate);

            // 検証 (Assert)
            var deleted = await db.FurikyuuZans.FirstOrDefaultAsync(x =>
                x.SyainId == syain.Id && x.KyuujitsuSyukkinYmd == jissekiDate);
            Assert.IsNull(deleted, "日曜休日出勤(管理職)取消で振休残が削除されるべきです。");
        }

        /// <summary>
        /// Given: 区分1が1日有給、区分2が未選択
        /// When: 取消確定時の休暇更新を実行
        /// Then: 有給消化数が1減算される
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 区分1が1日有給で区分2が未選択の場合、有給消化数が1減算される")]
        public async Task UpdateCancelConfirmLeaveAsync_区分1が1日有給で区分2が未選択の場合有給消化数が1減算される()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Syouka = 5m
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                syain.Id,
                年次有給休暇_1日,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateCancelConfirmLeaveAsync(syain.SyainBaseId, D("20250120"));

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstAsync(x => x.SyainBaseId == syain.SyainBaseId);
            Assert.AreEqual(4m, updated.Syouka, "区分1が1日有給取消で消化数が1減るべきです。");
        }

        /// <summary>
        /// Given: 区分2が1日有給
        /// When: 取消確定時の休暇更新を実行
        /// Then: 有給消化数が1減算される
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 区分2が1日有給の場合、有給消化数が1減算される")]
        public async Task UpdateCancelConfirmLeaveAsync_区分2が1日有給の場合有給消化数が1減算される()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Syouka = 5m
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                syain.Id,
                通常勤務,
                年次有給休暇_1日,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateCancelConfirmLeaveAsync(syain.SyainBaseId, D("20250120"));

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstAsync(x => x.SyainBaseId == syain.SyainBaseId);
            Assert.AreEqual(4m, updated.Syouka, "区分2が1日有給取消で消化数が1減るべきです。");
        }

        /// <summary>
        /// Given: 計画有給休暇の取消対象データが存在する
        /// When: 取消確定時の休暇更新を実行
        /// Then: 有給消化数と計画有給数が減算される
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 計画有給を取消した場合、有給消化数と計画有給数が減算される")]
        public async Task UpdateCancelConfirmLeaveAsync_計画有給を取消した場合有給消化数と計画有給数が減算される()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Syouka = 5m,
                KeikakuYukyuSu = 3
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                syain.Id,
                計画有給休暇,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateCancelConfirmLeaveAsync(syain.SyainBaseId, D("20250120"));

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstAsync(x => x.SyainBaseId == syain.SyainBaseId);
            Assert.AreEqual(4m, updated.Syouka, "計画有給取消で消化数が1減るべきです。");
            Assert.AreEqual(2, updated.KeikakuYukyuSu, "計画有給取消で計画有給数が1減るべきです。");
        }

        /// <summary>
        /// Given: 区分2が半日有給、区分1が半日勤務
        /// When: 取消確定時の休暇更新を実行
        /// Then: 消化数と半日回数が減算される
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 区分2が半日有給で区分1が半日勤務の場合、消化数と半日回数が減算される")]
        public async Task UpdateCancelConfirmLeaveAsync_区分2が半日有給で区分1が半日勤務の場合消化数と半日回数が減算される()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                Syouka = 1.5m,
                HannitiKaisuu = 3
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                syain.Id,
                半日勤務,
                半日有給,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateCancelConfirmLeaveAsync(syain.SyainBaseId, D("20250120"));

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstAsync(x => x.SyainBaseId == syain.SyainBaseId);
            Assert.AreEqual(1.0m, updated.Syouka, "半日有給取消で消化数が0.5減るべきです。");
            Assert.AreEqual(2, updated.HannitiKaisuu, "区分1が半日勤務のため半日回数が1減るべきです。");
        }

        /// <summary>
        /// Given: 計画特別休暇の取消対象データが存在する
        /// When: 取消確定時の休暇更新を実行
        /// Then: 計画特別休暇回数が減算される
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 計画特別休暇取消対象データが存在し取消確定時の休暇更新を実行した場合、" +
            "計画特別休暇回数が減算される")]
        public async Task UpdateCancelConfirmLeaveAsync_計画特別休暇取消対象データが存在し取消確定時の休暇更新を実行した場合計画特別休暇回数が減算される()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuTokukyuSu = 2
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                syain.Id,
                計画特別休暇,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateCancelConfirmLeaveAsync(syain.SyainBaseId, D("20250120"));

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstAsync(x => x.SyainBaseId == syain.SyainBaseId);
            Assert.AreEqual(1, updated.KeikakuTokukyuSu, "計画特別休暇取消で回数が1減るべきです。");
        }

        /// <summary>
        /// Given: 振替休暇取消の対象振休残が存在する
        /// When: 取消確定時の休暇更新を実行
        /// Then: 振休残が未取得状態へ戻る
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 振替休暇取消対象振休残が存在する場合、振休残が未取得状態へ戻る")]
        public async Task UpdateCancelConfirmLeaveAsync_振替休暇取消対象振休残が存在する場合振休残が未取得状態へ戻る()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250120");
            var furikyuu = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = _1日,
                SyutokuYmd1 = jissekiDate
            };
            db.FurikyuuZans.Add(furikyuu);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                振替休暇,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateCancelConfirmLeaveAsync(syain.SyainBaseId, jissekiDate);

            // 検証 (Assert)
            var updated = await db.FurikyuuZans.FindAsync(furikyuu.Id);
            Assert.AreEqual(未, updated!.SyutokuState, "振替休暇取消で未取得状態に戻るべきです。");
            Assert.IsNull(updated.SyutokuYmd1, "振替休暇取消で取得日1がクリアされるべきです。");
        }

        /// <summary>
        /// Given: 区分2が半日振休で取消対象振休残が存在する
        /// When: 取消確定時の休暇更新を実行
        /// Then: 振休残が未取得状態へ戻る
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 区分2が半日振休で取消対象振休残が存在する場合、振休残が未取得状態へ戻る")]
        public async Task UpdateCancelConfirmLeaveAsync_区分2が半日振休で取消対象振休残が存在する場合振休残が未取得状態へ戻る()
        {
            // 準備 (Arrange)
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithStartYmd(D("20240101"))
                .WithEndYmd(D("20261231"))
                .Build();
            db.Syains.Add(syain);

            var jissekiDate = D("20250120");
            var furikyuu = new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 半日,
                SyutokuYmd1 = jissekiDate
            };
            db.FurikyuuZans.Add(furikyuu);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                通常勤務,
                半日振休,
                isWorkDay: true);

            // 実行 (Act)
            await leave.UpdateCancelConfirmLeaveAsync(syain.SyainBaseId, jissekiDate);

            // 検証 (Assert)
            var updated = await db.FurikyuuZans.FindAsync(furikyuu.Id);
            Assert.AreEqual(未, updated!.SyutokuState, "半日振休取消で未取得状態に戻るべきです。");
            Assert.IsNull(updated.SyutokuYmd1, "半日振休取消で取得日1がクリアされるべきです。");
        }

        /// <summary>
        /// Given: 半日有給取消の対象データが存在し、区分1が半日勤務
        /// When: 取消処理を実行
        /// Then: 消化数と半日回数が減算される
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 半日有給取消対象データが存在し区分1が半日勤務の場合、" +
            "消化数と半日回数が減算される")]
        public async Task UpdateCancelConfirmLeaveAsync_半日有給取消対象データが存在し区分1が半日勤務の場合消化数と半日回数が減算される()
        {
            // 準備 (Arrange)
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = 1,
                Syouka = 1.5m,
                HannitiKaisuu = 3
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                1,
                半日勤務,
                半日有給,
                isWorkDay: true);

            // 実行 (Act)
            await InvokePrivateAsync(
                leave,
                "CancelHalfDayPaidLeaveAsync",
                1L,
                半日勤務);

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstOrDefaultAsync(x => x.SyainBaseId == 1);
            Assert.IsNotNull(updated);
            Assert.AreEqual(1.0m, updated!.Syouka, "有給消化数が0.5減るべきです。");
            Assert.AreEqual(2, updated!.HannitiKaisuu, "半日回数が1減るべきです。");
        }

        /// <summary>
        /// Given: 計画特別休暇の取消対象データが存在する
        /// When: 取消処理を実行
        /// Then: 計画特別休暇回数が減算される
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 計画特別休暇取消対象データが存在し取消処理を実行した場合、" +
            "計画特別休暇回数が減算される")]
        public async Task UpdateCancelConfirmLeaveAsync_計画特別休暇取消対象データが存在し取消処理を実行した場合計画特別休暇回数が減算される()
        {
            // 準備 (Arrange)
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = 1,
                KeikakuTokukyuSu = 2
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                1,
                計画特別休暇,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await InvokePrivateAsync(
                leave,
                "CancelPlannedSpecialDayAsync",
                1L);

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstOrDefaultAsync(x => x.SyainBaseId == 1);
            Assert.IsNotNull(updated);
            Assert.AreEqual(1m, updated!.KeikakuTokukyuSu, "計画特別休暇回数が1減るべきです。");
        }

        /// <summary>
        /// Given: 振休取消対象と同日取得の別レコード(取得日1)が存在する
        /// When: 振休取消処理を実行
        /// Then: 対象・関連レコードの双方が未取得状態に戻る
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 振休取消対象と同日取得の別レコード(取得日1)が存在する場合、" +
            "対象・関連レコード双方が未取得状態に戻る")]
        public async Task UpdateCancelConfirmLeaveAsync_振休取消対象と同日取得の別レコード取得日1が存在する場合対象関連レコード双方が未取得状態に戻る()
        {
            // 準備 (Arrange)
            var jissekiDate = D("20250120");
            var first = new FurikyuuZan
            {
                SyainId = 1,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = _1日,
                SyutokuYmd1 = jissekiDate
            };
            var second = new FurikyuuZan
            {
                SyainId = 1,
                KyuujitsuSyukkinYmd = D("20250102"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 半日,
                SyutokuYmd1 = jissekiDate
            };
            db.FurikyuuZans.Add(first);
            db.FurikyuuZans.Add(second);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                1,
                振替休暇,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await InvokePrivateAsync(
                leave,
                "CancelFurikyuuAsync",
                1L,
                jissekiDate);

            // 検証 (Assert)
            var firstUpdated = await db.FurikyuuZans.FindAsync(first.Id);
            var secondUpdated = await db.FurikyuuZans.FindAsync(second.Id);
            Assert.IsNotNull(firstUpdated);
            Assert.IsNotNull(secondUpdated);
            Assert.IsNull(firstUpdated!.SyutokuYmd1, "対象レコードの取得日1がクリアされるべきです。");
            Assert.AreEqual(未, firstUpdated!.SyutokuState, "対象レコードは未取得に戻るべきです。");
            Assert.IsNull(secondUpdated!.SyutokuYmd1, "関連レコードの取得日1がクリアされるべきです。");
            Assert.AreEqual(未, secondUpdated!.SyutokuState, "関連レコードは未取得に戻るべきです。");
        }

        /// <summary>
        /// Given: 振休取消対象と同日取得の別レコード(取得日2)が存在する
        /// When: 振休取消処理を実行
        /// Then: 関連レコードは半日状態に戻る
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 振休取消対象と同日取得の別レコード(取得日2)が存在する場合、" +
            "関連レコードは半日状態に戻る")]
        public async Task UpdateCancelConfirmLeaveAsync_振休取消対象と同日取得の別レコード取得日2が存在する場合関連レコードは半日状態に戻る()
        {
            // 準備 (Arrange)
            var jissekiDate = D("20250120");
            var first = new FurikyuuZan
            {
                SyainId = 1,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = _1日,
                SyutokuYmd1 = jissekiDate
            };
            var second = new FurikyuuZan
            {
                SyainId = 1,
                KyuujitsuSyukkinYmd = D("20250102"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = _1日,
                SyutokuYmd1 = D("20250110"),
                SyutokuYmd2 = jissekiDate
            };
            db.FurikyuuZans.Add(first);
            db.FurikyuuZans.Add(second);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                1,
                振替休暇,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await InvokePrivateAsync(
                leave,
                "CancelFurikyuuAsync",
                1L,
                jissekiDate);

            // 検証 (Assert)
            var secondUpdated = await db.FurikyuuZans.FindAsync(second.Id);
            Assert.IsNotNull(secondUpdated);
            Assert.IsNull(secondUpdated!.SyutokuYmd2, "関連レコードの取得日2がクリアされるべきです。");
            Assert.AreEqual(半日, secondUpdated!.SyutokuState, "関連レコードは半日状態に戻るべきです。");
        }

        /// <summary>
        /// Given: 半日振休取消の対象が取得日2に設定されている
        /// When: 半日振休取消を実行
        /// Then: 取得日2がクリアされ、半日状態に戻る
        /// </summary>
        [TestMethod(DisplayName = "UpdateCancelConfirmLeaveAsync: 半日振休取消対象が取得日2に設定されている場合、" +
            "取得日2がクリアされ半日状態に戻る")]
        public async Task UpdateCancelConfirmLeaveAsync_半日振休取消対象が取得日2に設定されている場合取得日2がクリアされ半日状態に戻る()
        {
            // 準備 (Arrange)
            var jissekiDate = D("20250120");
            var furikyuu = new FurikyuuZan
            {
                SyainId = 1,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = _1日,
                SyutokuYmd1 = D("20250110"),
                SyutokuYmd2 = jissekiDate
            };
            db.FurikyuuZans.Add(furikyuu);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                1,
                半日振休,
                None,
                isWorkDay: true);

            // 実行 (Act)
            await InvokePrivateAsync(
                leave,
                "CancelHalfFurikyuuAsync",
                1L,
                jissekiDate);

            // 検証 (Assert)
            var updated = await db.FurikyuuZans.FindAsync(furikyuu.Id);
            Assert.IsNotNull(updated);
            Assert.IsNull(updated!.SyutokuYmd2, "取得日2がクリアされるべきです。");
            Assert.AreEqual(半日, updated!.SyutokuState, "半日状態に戻るべきです。");
        }

        /// <summary>
        /// Given: 区分2に1日有給が設定されている
        /// When: 取消処理を実行
        /// Then: 有給消化数が1減算される
        /// </summary>
        [TestMethod(DisplayName = "CancelOneDayPaidLeaveAsync: 区分2に1日有給が設定されている場合、取消処理で有給消化数が1減算される")]
        public async Task CancelOneDayPaidLeaveAsync_区分2に1日有給が設定されている場合取消処理で有給消化数が1減算される()
        {
            // 準備 (Arrange)
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = 1,
                Syouka = 5m
            });
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                1,
                通常勤務,
                年次有給休暇_1日,
                isWorkDay: true);

            // 実行 (Act)
            await InvokePrivateAsync(
                leave,
                "CancelOneDayPaidLeaveAsync",
                1L);

            // 検証 (Assert)
            var updated = await db.YuukyuuZans.FirstOrDefaultAsync(x => x.SyainBaseId == 1);
            Assert.IsNotNull(updated);
            Assert.AreEqual(4m, updated!.Syouka, "1日有給取消で消化数が1減るべきです。");
        }

        /// <summary>
        /// Given: 振休残が閾値未満
        /// When: 振休残通知判定を実行
        /// Then: 通知メッセージは作成されない
        /// </summary>
        [TestMethod(DisplayName = "SendCompensatoryLeaveNotificationIfNeededAsync: 振休残が閾値未満の場合、振休残通知は作成されない")]
        public async Task SendCompensatoryLeaveNotificationIfNeededAsync_振休残が閾値未満の場合振休残通知は作成されない()
        {
            // 準備 (Arrange)
            var today = DateTime.Today.ToDateOnly();
            var jissekiDate = D("20250120");

            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            var bumonchoBase = new SyainBasisBuilder().WithId(100).Build();
            db.SyainBases.AddRange(syainBase, bumonchoBase);

            var busyoBase = new BusyoBasisBuilder().WithId(1).WithBumoncyoId(bumonchoBase.Id).Build();
            var busyo = new BusyoBuilder().WithId(1).WithBusyoBaseId(busyoBase.Id).Build();
            db.BusyoBases.Add(busyoBase);
            db.Busyos.Add(busyo);

            var bumoncho = new SyainBuilder()
                .WithId(101)
                .WithSyainBaseId(bumonchoBase.Id)
                .WithBusyoId(busyo.Id)
                .WithStartYmd(today.AddDays(-1))
                .WithEndYmd(today.AddDays(1))
                .WithEMail("boss@test.com")
                .Build();

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithBusyoId(busyo.Id)
                .WithName("一般社員")
                .Build();

            db.Syains.AddRange(bumoncho, syain);

            // 9.5日分(通知対象外)
            for (int i = 0; i < 9; i++)
            {
                db.FurikyuuZans.Add(new FurikyuuZan
                {
                    SyainId = syain.Id,
                    KyuujitsuSyukkinYmd = D("20250101").AddDays(-i),
                    DaikyuuKigenYmd = D("20251231"),
                    SyutokuState = 未,
                    IsOneDay = true
                });
            }
            db.FurikyuuZans.Add(new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20241220"),
                DaikyuuKigenYmd = D("20251231"),
                SyutokuState = 未,
                IsOneDay = false
            });

            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                None);

            // 実行 (Act)
            await InvokePrivateAsync(
                leave,
                "SendCompensatoryLeaveNotificationIfNeededAsync",
                syain.Id,
                jissekiDate);

            // 検証 (Assert)
            Assert.AreEqual(0, await db.MessageContents.CountAsync(), "閾値未満では通知が作成されないべきです。");
        }

        /// <summary>
        /// Given: 振休残が閾値以上で、部門長メールあり
        /// When: 振休残通知判定を実行
        /// Then: 通知メッセージが作成される
        /// </summary>
        [TestMethod(DisplayName = "SendCompensatoryLeaveNotificationIfNeededAsync: 振休残が閾値以上で部門長メールありの場合、" +
            "通知メッセージが作成される")]
        public async Task SendCompensatoryLeaveNotificationIfNeededAsync_振休残が閾値以上で部門長メールありの場合通知メッセージが作成される()
        {
            // 準備 (Arrange)
            var today = DateTime.Today.ToDateOnly();
            var jissekiDate = D("20250120");

            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            var bumonchoBase = new SyainBasisBuilder().WithId(100).Build();
            db.SyainBases.AddRange(syainBase, bumonchoBase);

            var busyoBase = new BusyoBasisBuilder().WithId(1).WithBumoncyoId(bumonchoBase.Id).Build();
            var busyo = new BusyoBuilder().WithId(1).WithBusyoBaseId(busyoBase.Id).Build();
            db.BusyoBases.Add(busyoBase);
            db.Busyos.Add(busyo);

            var bumoncho = new SyainBuilder()
                .WithId(101)
                .WithSyainBaseId(bumonchoBase.Id)
                .WithBusyoId(busyo.Id)
                .WithStartYmd(today.AddDays(-1))
                .WithEndYmd(today.AddDays(1))
                .WithEMail("boss@test.com")
                .Build();

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithBusyoId(busyo.Id)
                .WithName("一般社員")
                .Build();

            db.Syains.AddRange(bumoncho, syain);

            // 10日分(通知対象)
            for (int i = 0; i < 10; i++)
            {
                db.FurikyuuZans.Add(new FurikyuuZan
                {
                    SyainId = syain.Id,
                    KyuujitsuSyukkinYmd = D("20250101").AddDays(-i),
                    DaikyuuKigenYmd = D("20251231"),
                    SyutokuState = 未,
                    IsOneDay = true
                });
            }

            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                None);

            // 実行 (Act)
            await InvokePrivateAsync(
                leave,
                "SendCompensatoryLeaveNotificationIfNeededAsync",
                syain.Id,
                jissekiDate);

            // 検証 (Assert)
            var notification = await db.MessageContents.SingleOrDefaultAsync();
            Assert.IsNotNull(notification, "閾値以上かつ部門長メールありの場合は通知が作成されるべきです。");
            Assert.AreEqual(bumoncho.Id, notification.SyainId, "通知先は部門長であるべきです。");
            Assert.AreEqual(FunctionalClassification.有給未取得アラート, notification.FunctionType, "機能区分が正しいべきです。");
            StringAssert.Contains(notification.Content, "一般社員", "通知本文に対象社員名が含まれるべきです。");
            StringAssert.Contains(notification.Content, "10", "通知本文に合計日数が含まれるべきです。");
        }

        /// <summary>
        /// Given: 振休残が閾値以上だが、部門長メールなし
        /// When: 振休残通知判定を実行
        /// Then: 通知メッセージは作成されない
        /// </summary>
        [TestMethod(DisplayName = "SendCompensatoryLeaveNotificationIfNeededAsync: 振休残が閾値以上だが部門長メールなしの場合、" +
            "通知メッセージは作成されない")]
        public async Task SendCompensatoryLeaveNotificationIfNeededAsync_振休残が閾値以上だが部門長メールなしの場合通知メッセージは作成されない()
        {
            // 準備 (Arrange)
            var today = DateTime.Today.ToDateOnly();
            var jissekiDate = D("20250120");

            var syainBase = new SyainBasisBuilder().WithId(1).Build();
            var bumonchoBase = new SyainBasisBuilder().WithId(100).Build();
            db.SyainBases.AddRange(syainBase, bumonchoBase);

            var busyoBase = new BusyoBasisBuilder().WithId(1).WithBumoncyoId(bumonchoBase.Id).Build();
            var busyo = new BusyoBuilder().WithId(1).WithBusyoBaseId(busyoBase.Id).Build();
            db.BusyoBases.Add(busyoBase);
            db.Busyos.Add(busyo);

            var bumoncho = new SyainBuilder()
                .WithId(101)
                .WithSyainBaseId(bumonchoBase.Id)
                .WithBusyoId(busyo.Id)
                .WithStartYmd(today.AddDays(-1))
                .WithEndYmd(today.AddDays(1))
                .WithEMail(string.Empty)
                .Build();

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .WithBusyoId(busyo.Id)
                .WithName("一般社員")
                .Build();

            db.Syains.AddRange(bumoncho, syain);

            for (int i = 0; i < 10; i++)
            {
                db.FurikyuuZans.Add(new FurikyuuZan
                {
                    SyainId = syain.Id,
                    KyuujitsuSyukkinYmd = D("20250101").AddDays(-i),
                    DaikyuuKigenYmd = D("20251231"),
                    SyutokuState = 未,
                    IsOneDay = true
                });
            }

            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                syain.Id,
                None);

            // 実行 (Act)
            await InvokePrivateAsync(
                leave,
                "SendCompensatoryLeaveNotificationIfNeededAsync",
                syain.Id,
                jissekiDate);

            // 検証 (Assert)
            Assert.AreEqual(0, await db.MessageContents.CountAsync(), "部門長メールがない場合は通知が作成されないべきです。");
        }

        /// <summary>
        /// Given: SendCompensatoryLeaveNotificationIfNeededAsync の条件を満たしている
        /// When: OneDayHalfStateReachesThreshold
        /// Then: NotificationCreated
        /// </summary>
        [TestMethod(DisplayName = "SendCompensatoryLeaveNotificationIfNeededAsync: 1日半日状態が閾値に達した場合、通知が作成される")]
        public async Task SendCompensatoryLeaveNotificationIfNeededAsync_1日半日状態が閾値に達した場合通知が作成される()
        {
            var today = DateTime.Today.ToDateOnly();
            var jissekiDate = D("20250120");

            var syainBase = new SyainBasisBuilder().WithId(11).Build();
            var bumonchoBase = new SyainBasisBuilder().WithId(110).Build();
            db.SyainBases.AddRange(syainBase, bumonchoBase);

            var busyoBase = new BusyoBasisBuilder().WithId(11).WithBumoncyoId(bumonchoBase.Id).Build();
            var busyo = new BusyoBuilder().WithId(11).WithBusyoBaseId(busyoBase.Id).Build();
            db.BusyoBases.Add(busyoBase);
            db.Busyos.Add(busyo);

            var bumoncho = new SyainBuilder()
                .WithId(111)
                .WithSyainBaseId(bumonchoBase.Id)
                .WithBusyoId(busyo.Id)
                .WithStartYmd(today.AddDays(-1))
                .WithEndYmd(today.AddDays(1))
                .WithEMail("boss_halfday@test.com")
                .Build();

            var syain = new SyainBuilder()
                .WithId(11)
                .WithSyainBaseId(syainBase.Id)
                .WithBusyoId(busyo.Id)
                .WithName("HalfStateEmployee")
                .Build();
            db.Syains.AddRange(bumoncho, syain);

            // 20 * 0.5 = 10
            for (int i = 0; i < 20; i++)
            {
                db.FurikyuuZans.Add(new FurikyuuZan
                {
                    SyainId = syain.Id,
                    KyuujitsuSyukkinYmd = D("20250101").AddDays(-i),
                    DaikyuuKigenYmd = D("20251231"),
                    SyutokuState = 半日,
                    IsOneDay = true
                });
            }

            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(jissekiDate, syain.Id, None);

            await InvokePrivateAsync(
                leave,
                "SendCompensatoryLeaveNotificationIfNeededAsync",
                syain.Id,
                jissekiDate);

            var notification = await db.MessageContents.SingleOrDefaultAsync();
            Assert.IsNotNull(notification, "One-day half-state accumulation should create notification.");
            StringAssert.Contains(notification.Content, "10");
        }

        /// <summary>
        /// Given: SendCompensatoryLeaveNotificationIfNeededAsync の条件を満たしている
        /// When: HalfDayUnusedReachesThreshold
        /// Then: NotificationCreated
        /// </summary>
        [TestMethod(DisplayName = "SendCompensatoryLeaveNotificationIfNeededAsync: 半日未取得が閾値に達した場合、通知が作成される")]
        public async Task SendCompensatoryLeaveNotificationIfNeededAsync_半日未取得が閾値に達した場合通知が作成される()
        {
            var today = DateTime.Today.ToDateOnly();
            var jissekiDate = D("20250120");

            var syainBase = new SyainBasisBuilder().WithId(21).Build();
            var bumonchoBase = new SyainBasisBuilder().WithId(210).Build();
            db.SyainBases.AddRange(syainBase, bumonchoBase);

            var busyoBase = new BusyoBasisBuilder().WithId(21).WithBumoncyoId(bumonchoBase.Id).Build();
            var busyo = new BusyoBuilder().WithId(21).WithBusyoBaseId(busyoBase.Id).Build();
            db.BusyoBases.Add(busyoBase);
            db.Busyos.Add(busyo);

            var bumoncho = new SyainBuilder()
                .WithId(211)
                .WithSyainBaseId(bumonchoBase.Id)
                .WithBusyoId(busyo.Id)
                .WithStartYmd(today.AddDays(-1))
                .WithEndYmd(today.AddDays(1))
                .WithEMail("boss_half_unused@test.com")
                .Build();

            var syain = new SyainBuilder()
                .WithId(21)
                .WithSyainBaseId(syainBase.Id)
                .WithBusyoId(busyo.Id)
                .WithName("HalfUnusedEmployee")
                .Build();
            db.Syains.AddRange(bumoncho, syain);

            // 20 * 0.5 = 10
            for (int i = 0; i < 20; i++)
            {
                db.FurikyuuZans.Add(new FurikyuuZan
                {
                    SyainId = syain.Id,
                    KyuujitsuSyukkinYmd = D("20250101").AddDays(-i),
                    DaikyuuKigenYmd = D("20251231"),
                    SyutokuState = 未,
                    IsOneDay = false
                });
            }

            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(jissekiDate, syain.Id, None);

            await InvokePrivateAsync(
                leave,
                "SendCompensatoryLeaveNotificationIfNeededAsync",
                syain.Id,
                jissekiDate);

            var notification = await db.MessageContents.SingleOrDefaultAsync();
            Assert.IsNotNull(notification, "Half-day unused accumulation should create notification.");
            StringAssert.Contains(notification.Content, "10");
        }

        /// <summary>
        /// Given: SendCompensatoryLeaveNotificationIfNeededAsync の条件を満たしている
        /// When: ThresholdReachedAndNoBumoncho
        /// Then: NoNotification
        /// </summary>
        [TestMethod(DisplayName = "SendCompensatoryLeaveNotificationIfNeededAsync: 閾値に達したが部門長が未設定の場合、" +
            "通知は作成されない")]
        public async Task SendCompensatoryLeaveNotificationIfNeededAsync_閾値に達したが部門長が未設定の場合通知は作成されない()
        {
            var jissekiDate = D("20250120");

            var syainBase = new SyainBasisBuilder().WithId(31).Build();
            db.SyainBases.Add(syainBase);

            var busyoBase = new BusyoBasisBuilder().WithId(31).WithBumoncyoId(null).Build();
            var busyo = new BusyoBuilder().WithId(31).WithBusyoBaseId(busyoBase.Id).Build();
            db.BusyoBases.Add(busyoBase);
            db.Busyos.Add(busyo);

            var syain = new SyainBuilder()
                .WithId(31)
                .WithSyainBaseId(syainBase.Id)
                .WithBusyoId(busyo.Id)
                .WithName("NoBumonchoEmployee")
                .Build();
            db.Syains.Add(syain);

            for (int i = 0; i < 10; i++)
            {
                db.FurikyuuZans.Add(new FurikyuuZan
                {
                    SyainId = syain.Id,
                    KyuujitsuSyukkinYmd = D("20250101").AddDays(-i),
                    DaikyuuKigenYmd = D("20251231"),
                    SyutokuState = 未,
                    IsOneDay = true
                });
            }

            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(jissekiDate, syain.Id, None);

            await InvokePrivateAsync(
                leave,
                "SendCompensatoryLeaveNotificationIfNeededAsync",
                syain.Id,
                jissekiDate);

            Assert.AreEqual(
                0,
                await db.MessageContents.CountAsync(),
                "部門長が未設定の場合は通知を作成しないこと。");
        }

        /// <summary>
        /// Given: SendCompensatoryLeaveNotificationIfNeededAsync の条件を満たしている
        /// When: HalfDayUnusedMixedStates
        /// Then: UseExpectedTotalDays
        /// </summary>
        [TestMethod(DisplayName = "SendCompensatoryLeaveNotificationIfNeededAsync: 半日未取得で状態が混在している場合、" +
            "期待される合計日数を使用する")]
        public async Task SendCompensatoryLeaveNotificationIfNeededAsync_半日未取得で状態が混在している場合期待される合計日数を使用する()
        {
            var today = DateTime.Today.ToDateOnly();
            var jissekiDate = D("20250120");

            var syainBase = new SyainBasisBuilder().WithId(41).Build();
            var bumonchoBase = new SyainBasisBuilder().WithId(410).Build();
            db.SyainBases.AddRange(syainBase, bumonchoBase);

            var busyoBase = new BusyoBasisBuilder().WithId(41).WithBumoncyoId(bumonchoBase.Id).Build();
            var busyo = new BusyoBuilder().WithId(41).WithBusyoBaseId(busyoBase.Id).Build();
            db.BusyoBases.Add(busyoBase);
            db.Busyos.Add(busyo);

            var bumoncho = new SyainBuilder()
                .WithId(411)
                .WithSyainBaseId(bumonchoBase.Id)
                .WithBusyoId(busyo.Id)
                .WithStartYmd(today.AddDays(-1))
                .WithEndYmd(today.AddDays(1))
                .WithEMail("boss_mixed@test.com")
                .Build();

            var syain = new SyainBuilder()
                .WithId(41)
                .WithSyainBaseId(syainBase.Id)
                .WithBusyoId(busyo.Id)
                .WithName("MixedStateEmployee")
                .Build();
            db.Syains.AddRange(bumoncho, syain);

            // 20 * 0.5 = 10 (counted path)
            for (int i = 0; i < 20; i++)
            {
                db.FurikyuuZans.Add(new FurikyuuZan
                {
                    SyainId = syain.Id,
                    KyuujitsuSyukkinYmd = D("20250101").AddDays(-i),
                    DaikyuuKigenYmd = D("20251231"),
                    SyutokuState = 未,
                    IsOneDay = false
                });
            }

            // 集計対象外（0m を返す経路）
            db.FurikyuuZans.Add(new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20241201"),
                DaikyuuKigenYmd = D("20251231"),
                SyutokuState = _1日,
                IsOneDay = false
            });

            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(jissekiDate, syain.Id, None);
            await InvokePrivateAsync(leave, "SendCompensatoryLeaveNotificationIfNeededAsync", syain.Id, jissekiDate);

            var notification = await db.MessageContents.SingleOrDefaultAsync();
            Assert.IsNotNull(notification, "Threshold should be reached by counted half-day rows.");
            StringAssert.Contains(notification.Content, "10", "Ignored state must contribute 0 day to total.");
        }

        /// <summary>
        /// Given: SendCompensatoryLeaveNotificationIfNeededAsync の条件を満たしている
        /// When: BumonchoHasNoActiveSyain
        /// Then: NotificationNotCreated
        /// </summary>
        [TestMethod(DisplayName = "SendCompensatoryLeaveNotificationIfNeededAsync: 閾値に達したが部門長に有効な社員がいない場合、" +
            "通知は作成されない")]
        public async Task SendCompensatoryLeaveNotificationIfNeededAsync_閾値に達したが部門長に有効な社員がいない場合通知は作成されない()
        {
            var today = DateTime.Today.ToDateOnly();
            var jissekiDate = D("20250120");

            var syainBase = new SyainBasisBuilder().WithId(51).Build();
            var bumonchoBase = new SyainBasisBuilder().WithId(510).Build();
            db.SyainBases.AddRange(syainBase, bumonchoBase);

            var busyoBase = new BusyoBasisBuilder().WithId(51).WithBumoncyoId(bumonchoBase.Id).Build();
            var busyo = new BusyoBuilder().WithId(51).WithBusyoBaseId(busyoBase.Id).Build();
            db.BusyoBases.Add(busyoBase);
            db.Busyos.Add(busyo);

            // 退職済み部門長は filtered include で Bumoncyo.Syains から除外される
            var inactiveBumoncho = new SyainBuilder()
                .WithId(511)
                .WithSyainBaseId(bumonchoBase.Id)
                .WithBusyoId(busyo.Id)
                .WithStartYmd(today.AddDays(-30))
                .WithEndYmd(today.AddDays(-1))
                .WithEMail("inactive_boss@test.com")
                .Build();

            var syain = new SyainBuilder()
                .WithId(51)
                .WithSyainBaseId(syainBase.Id)
                .WithBusyoId(busyo.Id)
                .WithName("NoActiveBumonchoEmployee")
                .Build();
            db.Syains.AddRange(inactiveBumoncho, syain);

            for (int i = 0; i < 10; i++)
            {
                db.FurikyuuZans.Add(new FurikyuuZan
                {
                    SyainId = syain.Id,
                    KyuujitsuSyukkinYmd = D("20250101").AddDays(-i),
                    DaikyuuKigenYmd = D("20251231"),
                    SyutokuState = 未,
                    IsOneDay = true
                });
            }

            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(jissekiDate, syain.Id, None);
            await InvokePrivateAsync(leave, "SendCompensatoryLeaveNotificationIfNeededAsync", syain.Id, jissekiDate);

            Assert.AreEqual(
                0,
                await db.MessageContents.CountAsync(),
                "有効な部門長がいない場合は通知対象が存在しないこと。");
        }

        #endregion

        #region ConfirmValidation FinalConfirm Branch Tests

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 半日有給回数上限
        /// Then: 半日有給上限エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 半日有給回数上限に達した場合、半日有給上限エラー")]
        public async Task FinalConfirmValidationAsync_半日有給回数上限に達した場合半日有給上限エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(半日有給, isSyukkin: false, isVacation: true);
            db.YuukyuuZans.Add(new YuukyuuZan { SyainBaseId = syain.SyainBaseId, HannitiKaisuu = 10 });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日有給,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "AnnualHalfDayPaidLeaveLimit");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 半日有給で有給残情報なし
        /// Then: 有給残情報未登録エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 半日有給で有給残情報がない場合、有給残情報未登録エラー")]
        public async Task FinalConfirmValidationAsync_半日有給で有給残情報がない場合有給残情報未登録エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(半日有給, isSyukkin: false, isVacation: true);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日有給,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "PaidLeaveInfoNotRegistered");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 男性で生理休暇選択
        /// Then: 生理休暇不可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 男性が生理休暇を選択した場合、生理休暇不可エラー")]
        public async Task FinalConfirmValidationAsync_男性が生理休暇を選択した場合生理休暇不可エラー()
        {
            var syain = SeedConfirmValidationSyain(seibetsu: '1');
            AddSyukkinKubun(生理休暇, isSyukkin: false, isVacation: true);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                生理休暇,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "CannotTakePhysiologicalLeave");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 振替休暇残が0 5以下
        /// Then: 振替休暇不可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 振替休暇残が0または5日以下の場合、振替休暇不可エラー")]
        public async Task FinalConfirmValidationAsync_振替休暇残が0または5日以下の場合振替休暇不可エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(振替休暇, isSyukkin: false, isVacation: true);
            db.FurikyuuZans.Add(new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = 未
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                振替休暇,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "CannotTakeSubstituteHoliday");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 半日振休残がない
        /// Then: 半日振休不可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 半日振休残がない場合、半日振休不可エラー")]
        public async Task FinalConfirmValidationAsync_半日振休残がない場合半日振休不可エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(半日振休, isSyukkin: false, isVacation: true);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日振休,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "CannotTakeHalfDaySubstituteHoliday");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 半日有給で有給残0 5未満
        /// Then: 半日有給不可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 半日有給で有給残が0.5未満の場合、半日有給不可エラー")]
        public async Task FinalConfirmValidationAsync_半日有給で有給残0_5未満の場合半日有給不可エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(半日有給, isSyukkin: false, isVacation: true);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuYukyuSu = 0,
                Kurikoshi = 0m,
                Syouka = 1m
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日有給,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "CannotTakeHalfDayPaidLeave");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 半日振休と半日有給で振休残あり
        /// Then: 振休先取得エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 半日振休と半日有給で振休残がある場合、振休先取得エラー")]
        public async Task FinalConfirmValidationAsync_半日振休と半日有給で振休残がある場合振休先取得エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(半日振休, isSyukkin: false, isVacation: true);
            db.FurikyuuZans.Add(new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 未
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日振休,
                半日有給,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "TakeSubstituteHolidayFirst");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 半日有給で振休残あり
        /// Then: 振休先取得エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 半日有給で振休残がある場合、振休先取得エラー")]
        public async Task FinalConfirmValidationAsync_半日有給で振休残がある場合振休先取得エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(半日有給, isSyukkin: false, isVacation: true);
            db.FurikyuuZans.Add(new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = 未
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日有給,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "TakeSubstituteHolidayFirst");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 欠勤と振休残あり
        /// Then: 欠勤時振休可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 欠勤で振休残がある場合、欠勤時振休可エラー")]
        public async Task FinalConfirmValidationAsync_欠勤で振休残がある場合欠勤時振休可エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);
            db.FurikyuuZans.Add(new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = 未
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務,
                欠勤,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "AbsenceWithSubstituteHolidayAvailable");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 欠勤と区分1半日振休で振休残1日
        /// Then: 欠勤時振休可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 欠勤と区分1半日振休で振休残がある場合、欠勤時振休可エラー")]
        public async Task FinalConfirmValidationAsync_欠勤と区分1半日振休で振休残がある場合欠勤時振休可エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(半日振休, isSyukkin: true, isVacation: false);
            db.FurikyuuZans.Add(new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 未
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日振休,
                欠勤,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "AbsenceWithSubstituteHolidayAvailable");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 欠勤と有給残1日あり
        /// Then: 欠勤時振休可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 欠勤で有給残が1日ある場合、欠勤時振休可エラー")]
        public async Task FinalConfirmValidationAsync_欠勤で有給残が1日ある場合欠勤時振休可エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuYukyuSu = 1,
                Kurikoshi = 0m,
                Syouka = 0m
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務,
                欠勤,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "AbsenceWithSubstituteHolidayAvailable");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 欠勤と有給残0 5日あり
        /// Then: 欠勤時振休可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 欠勤で有給残が0.5日ある場合、欠勤時振休可エラー")]
        public async Task FinalConfirmValidationAsync_欠勤で有給残が0_5日ある場合欠勤時振休可エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuYukyuSu = 1,
                Kurikoshi = 0m,
                Syouka = 0.5m
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務,
                欠勤,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "AbsenceWithSubstituteHolidayAvailable");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 1日有給で振休残あり
        /// Then: 振休先取得エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 1日有給で振休残がある場合、振休先取得エラー")]
        public async Task FinalConfirmValidationAsync_1日有給で振休残がある場合振休先取得エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(年次有給休暇_1日, isSyukkin: false, isVacation: true);
            db.FurikyuuZans.Add(new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = 未
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                年次有給休暇_1日,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "TakeSubstituteHolidayFirst");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 1日有給で有給残不足
        /// Then: 1日有給不可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 1日有給で有給残不足の場合、1日有給不可エラー")]
        public async Task FinalConfirmValidationAsync_1日有給で有給残不足の場合1日有給不可エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(年次有給休暇_1日, isSyukkin: false, isVacation: true);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuYukyuSu = 0,
                Kurikoshi = 0m,
                Syouka = 1m
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                年次有給休暇_1日,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "CannotTakeAnnualPaidLeave");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 区分2が1日有給かつ区分1が半日有給
        /// Then: 出勤区分不正エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 区分2が1日有給かつ区分1が半日有給の場合、出勤区分不正エラー")]
        public async Task FinalConfirmValidationAsync_区分2が1日有給かつ区分1が半日有給の場合出勤区分不正エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(半日有給, isSyukkin: false, isVacation: true);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuYukyuSu = 1,
                Kurikoshi = 0m,
                Syouka = 0m
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日有給,
                年次有給休暇_1日,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "InvalidAttendanceClassification");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 計画有給が年上限
        /// Then: 計画有給上限エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 計画有給が年上限の場合、計画有給上限エラー")]
        public async Task FinalConfirmValidationAsync_計画有給が年上限の場合計画有給上限エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(計画有給休暇, isSyukkin: false, isVacation: true);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuYukyuSu = 5,
                Kurikoshi = 0m,
                Syouka = 0m
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                計画有給休暇,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "PlannedAnnualPaidLeaveLimit");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 計画有給で有給残不足
        /// Then: 1日有給不可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 計画有給で有給残不足の場合、1日有給不可エラー")]
        public async Task FinalConfirmValidationAsync_計画有給で有給残不足の場合1日有給不可エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(計画有給休暇, isSyukkin: false, isVacation: true);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuYukyuSu = 0,
                Kurikoshi = 0m,
                Syouka = 1m
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                計画有給休暇,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "CannotTakeAnnualPaidLeave");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 計画特別休暇が年上限
        /// Then: 計画特別休暇上限エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 計画特別休暇が年上限の場合、計画特別休暇上限エラー")]
        public async Task FinalConfirmValidationAsync_計画特別休暇が年上限の場合計画特別休暇上限エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(計画特別休暇, isSyukkin: false, isVacation: true);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuYukyuSu = 2
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                計画特別休暇,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "PlannedSpecialLeaveLimit");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 計画特別休暇で有給残情報なし
        /// Then: 有給残未登録エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 計画特別休暇で有給残情報なしの場合、有給残未登録エラー")]
        public async Task FinalConfirmValidationAsync_計画特別休暇で有給残情報なしの場合有給残未登録エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(計画特別休暇, isSyukkin: false, isVacation: true);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                計画特別休暇,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            var expected = string.Format(GetConfirmValidationConstant("PaidLeaveDataNotFoundFormat"), syain.Code);
            var errors = GetModelStateErrors(modelState);
            Assert.Contains(expected, errors, "有給残未登録エラーが設定されるべきです。");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 計画特別休暇で区分2未選択
        /// Then: 出勤区分不正エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 計画特別休暇で区分2未選択の場合、出勤区分不正エラー")]
        public async Task FinalConfirmValidationAsync_計画特別休暇で区分2未選択の場合出勤区分不正エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(計画特別休暇, isSyukkin: false, isVacation: true);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuYukyuSu = 1
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                計画特別休暇,
                None,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "InvalidAttendanceClassification");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 実績明細0件
        /// Then: 実績入力エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 実績明細が0件の場合、実績入力エラー")]
        public async Task FinalConfirmValidationAsync_実績明細が0件の場合実績入力エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(new List<IndexModel.NippouAnkenViewModel>(), 0m);

            AssertConfirmValidationError(modelState, "EnterWorkPerformance");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 工番5件超リンク
        /// Then: 最大件数エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 工番5件超リンクの場合、最大件数エラー")]
        public async Task FinalConfirmValidationAsync_工番5件超リンクの場合最大件数エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(CreateNippouAnkens(5, 5), 0m);

            AssertConfirmValidationError(modelState, "MaxFiveProjectCodes");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 1日勤務でリンク工番なし
        /// Then: 工番選択エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 1日勤務でリンク工番なしの場合、工番選択エラー")]
        public async Task FinalConfirmValidationAsync_1日勤務でリンク工番なしの場合工番選択エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(CreateNippouAnkens(1, 0), 0m);

            AssertConfirmValidationError(modelState, "SelectProjectCode");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 休日出勤短時間でリンク3件
        /// Then: 休日短時間工番上限エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 休日出勤短時間でリンク3件の場合、休日短時間工番上限エラー")]
        public async Task FinalConfirmValidationAsync_休日出勤短時間でリンク3件の場合休日短時間工番上限エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(AttendanceClassification.休日出勤, isSyukkin: true, isVacation: false);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                AttendanceClassification.休日出勤,
                isWorkDay: false,
                furiyoteiDate: D("20250127"),
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                dJitsudou: 4m,
                modelState: modelState);

            await validate.FinalConfirmValidationAsync(CreateNippouAnkens(3, 3), 0m);

            AssertConfirmValidationError(modelState, "HolidayWorkShortHoursProjectLimit");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 未来日かつ半日休暇組み合わせ
        /// Then: 未来実績不可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 未来日かつ半日休暇組み合わせの場合、未来実績不可エラー")]
        public async Task FinalConfirmValidationAsync_未来日かつ半日休暇組み合わせの場合未来実績不可エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(半日振休, isSyukkin: true, isVacation: false);
            db.FurikyuuZans.Add(new FurikyuuZan
            {
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20261231"),
                IsOneDay = true,
                SyutokuState = 未
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                DateTime.Today.ToDateOnly().AddDays(1),
                半日振休,
                半日振休,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(CreateNippouAnkens(1, 1), 0m);

            AssertConfirmValidationError(modelState, "CannotRegisterFutureWorkPerformance");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 半日勤務でリンク3件
        /// Then: 半日勤務工番上限エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 半日勤務でリンク3件の場合、半日勤務工番上限エラー")]
        public async Task FinalConfirmValidationAsync_半日勤務でリンク3件の場合半日勤務工番上限エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(半日勤務, isSyukkin: true, isVacation: false);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日勤務,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(13, 0),
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(CreateNippouAnkens(3, 3), 0m);

            AssertConfirmValidationError(modelState, "HalfDayWorkProjectLimit");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 半日勤務でリンク工番なしかつ区分マスタ未登録
        /// Then: 工番選択エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 半日勤務でリンク工番なしかつ区分マスタ未登録の場合、工番選択エラー")]
        public async Task FinalConfirmValidationAsync_半日勤務でリンク工番なしかつ区分マスタ未登録の場合工番選択エラー()
        {
            var syain = SeedConfirmValidationSyain();
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日勤務,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(13, 0),
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(CreateNippouAnkens(1, 0), 0m);

            AssertConfirmValidationError(modelState, "SelectProjectCode");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 休暇時にリンク工番あり
        /// Then: 休暇時工番選択不可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 休暇時にリンク工番ありの場合、休暇時工番選択不可エラー")]
        public async Task FinalConfirmValidationAsync_休暇時にリンク工番ありの場合休暇時工番選択不可エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(計画有給休暇, isSyukkin: false, isVacation: true);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuYukyuSu = 1,
                Kurikoshi = 0m,
                Syouka = 0m
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                計画有給休暇,
                modelState: modelState);
            await validate.FinalConfirmValidationAsync(CreateNippouAnkens(1, 1), 0m);

            AssertConfirmValidationError(modelState, "CannotSelectProjectDuringLeave");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 原価連動社員が費用種別2工番選択
        /// Then: 支援グループ工番不可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 原価連動社員が費用種別2工番選択の場合、支援グループ工番不可エラー")]
        public async Task FinalConfirmValidationAsync_原価連動社員が費用種別2工番選択の場合支援グループ工番不可エラー()
        {
            var syain = SeedConfirmValidationSyain(isGenkaRendou: true);
            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);

            var kings = new KingsJuchuBuilder().WithId(1).WithBusyoId(1).WithHiyouShubetuCd(2).Build();
            var anken = new AnkenBuilder()
                .WithId(1)
                .WithName("テスト案件")
                .WithSearchName("テスト案件")
                .WithKingsJuchuId(kings.Id)
                .Build();
            db.KingsJuchus.Add(kings);
            db.Ankens.Add(anken);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                modelState: modelState);

            var nippouAnkens = new List<IndexModel.NippouAnkenViewModel>
            {
                new IndexModel.NippouAnkenViewModel { AnkensId = anken.Id, IsLinked = true }
            };
            await validate.FinalConfirmValidationAsync(nippouAnkens, 0m);

            AssertConfirmValidationError(modelState, "CannotConfirmSupportGroupOrder");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 原価凍結工番選択
        /// Then: 工番使用不可エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 原価凍結工番選択の場合、工番使用不可エラー")]
        public async Task FinalConfirmValidationAsync_原価凍結工番選択の場合工番使用不可エラー()
        {
            var syain = SeedConfirmValidationSyain(isGenkaRendou: false);
            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);

            var kings = new KingsJuchuBuilder()
                .WithId(1)
                .WithBusyoId(1)
                .WithHiyouShubetuCd(1)
                .WithIsGenkaToketu(true)
                .Build();
            var anken = new AnkenBuilder()
                .WithId(1)
                .WithName("テスト案件")
                .WithSearchName("テスト案件")
                .WithKingsJuchuId(kings.Id)
                .Build();
            db.KingsJuchus.Add(kings);
            db.Ankens.Add(anken);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                modelState: modelState);

            var nippouAnkens = new List<IndexModel.NippouAnkenViewModel>
            {
                new IndexModel.NippouAnkenViewModel { AnkensId = anken.Id, IsLinked = true }
            };
            await validate.FinalConfirmValidationAsync(nippouAnkens, 0m);

            AssertConfirmValidationError(modelState, "SelectedProjectCodeCannotBeUsed");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 時間外伺い存在
        /// Then: 時間外上限エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 時間外伺い存在の場合、時間外上限エラー")]
        public async Task FinalConfirmValidationAsync_時間外伺い存在の場合時間外上限エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);

            var ukagai = new UkagaiHeader
            {
                Id = 1,
                SyainId = syain.Id,
                ShinseiYmd = D("20250101"),
                WorkYmd = D("20250120"),
                Invalid = false,
                Status = 承認待
            };
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = ukagai.Id,
                UkagaiSyubetsu = 時間外労働時間制限拡張
            };
            db.UkagaiHeaders.Add(ukagai);
            db.UkagaiShinseis.Add(ukagaiShinsei);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                modelState: modelState);

            await validate.FinalConfirmValidationAsync(CreateNippouAnkens(1, 1), 0m);

            AssertConfirmValidationError(modelState, "OvertimeLimitExceeded");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: 時間外伺い承認済み
        /// Then: 時間外上限未承認エラー
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 時間外伺い承認済みの場合、時間外上限未承認エラー")]
        public async Task FinalConfirmValidationAsync_時間外伺い承認済みの場合時間外上限未承認エラー()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(通常勤務, isSyukkin: true, isVacation: false);

            var ukagai = new UkagaiHeader
            {
                Id = 1,
                SyainId = syain.Id,
                ShinseiYmd = D("20250101"),
                WorkYmd = D("20250120"),
                Invalid = false,
                Status = 承認
            };
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = ukagai.Id,
                UkagaiSyubetsu = 時間外労働時間制限拡張
            };
            db.UkagaiHeaders.Add(ukagai);
            db.UkagaiShinseis.Add(ukagaiShinsei);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                modelState: modelState);

            await validate.FinalConfirmValidationAsync(CreateNippouAnkens(1, 1), 0m);

            AssertConfirmValidationError(modelState, "OvertimeLimitUnapproved");
        }

        #region Additional Branch Coverage Tests

        /// <summary>
        /// Given: CheckForNotificationMessageAsync の条件を満たしている
        /// When: NoEditedAndNoOvertime
        /// Then: ReturnNull
        /// </summary>
        [TestMethod(DisplayName = "CheckForNotificationMessageAsync: 打刻修正なしかつ時間外なしの場合、nullを返す")]
        public async Task CheckForNotificationMessageAsync_打刻修正なしかつ時間外なしの場合nullを返す()
        {
            var kintai = new KintaiZokusei
            {
                Id = 4,
                Code = 管理,
                Name = "管理",
                SeigenTime = 45m,
                MaxLimitTime = 80m,
                IsOvertimeLimit3m = false,
                IsMinashi = false
            };
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(kintai.Id)
                .Build();
            syain.KintaiZokusei = kintai;

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            var jissekiDate = D("20250115");
            var nippou = new IndexModel.NippouViewModel();
            model.NippouData = nippou;

            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = syain.Id,
                ShinseiYmd = D("20250110"),
                WorkYmd = jissekiDate,
                LastShoninYmd = D("20250111"),
                Status = 承認待,
                Invalid = false,
                UkagaiShinseis = new List<UkagaiShinsei>
                {
                    new UkagaiShinsei
                    {
                        Id = 1,
                        UkagaiHeaderId = 1,
                        UkagaiSyubetsu = 打刻時間修正
                    }
                }
            };

            var message = await InvokePrivateWithResultAsync<string>(
                model,
                "CheckForNotificationMessageAsync",
                nippou,
                syain,
                new List<WorkingHour> { new WorkingHour { Edited = false } },
                true,
                new ApplicationConfig { NippoStopDate = D("20990101") },
                new List<UkagaiHeader> { ukagaiHeader },
                jissekiDate);

            Assert.IsNull(message);
        }

        /// <summary>
        /// Given: CheckForNotificationMessageAsync の条件を満たしている
        /// When: MonthEndOvertimeApproved
        /// Then: ReturnNull
        /// </summary>
        [TestMethod(DisplayName = "CheckForNotificationMessageAsync: 月終了時間外承認済みの場合、nullを返す")]
        public async Task CheckForNotificationMessageAsync_月終了時間外承認済みの場合nullを返す()
        {
            var kintai = new KintaiZokusei
            {
                Id = 4,
                Code = 管理,
                Name = "管理",
                SeigenTime = 0.1m,
                MaxLimitTime = 80m,
                IsOvertimeLimit3m = false,
                IsMinashi = false
            };
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(kintai.Id)
                .Build();
            syain.KintaiZokusei = kintai;
            db.Syains.Add(syain);

            db.Nippous.Add(new NippouBuilder()
                .WithId(5001)
                .WithSyainId(syain.Id)
                .WithNippouYmd(D("20250110"))
                .WithTotalZangyo(1000m)
                .Build());
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;
            var jissekiDate = D("20250131");
            var nippou = new IndexModel.NippouViewModel();
            model.NippouData = nippou;

            var ukagaiHeader = new UkagaiHeader
            {
                Id = 10,
                SyainId = syain.Id,
                ShinseiYmd = D("20250105"),
                WorkYmd = jissekiDate,
                LastShoninYmd = D("20250106"),
                Status = 承認,
                Invalid = false,
                UkagaiShinseis = new List<UkagaiShinsei>
                {
                    new UkagaiShinsei
                    {
                        Id = 11,
                        UkagaiHeaderId = 10,
                        UkagaiSyubetsu = 時間外労働時間制限拡張
                    }
                }
            };

            var message = await InvokePrivateWithResultAsync<string>(
                model,
                "CheckForNotificationMessageAsync",
                nippou,
                syain,
                new List<WorkingHour>(),
                true,
                new ApplicationConfig { NippoStopDate = D("20990101") },
                new List<UkagaiHeader> { ukagaiHeader },
                jissekiDate);

            Assert.IsNull(message);
        }

        /// <summary>
        /// Given: OvertimeLimitCheckAsync の条件を満たしている
        /// When: NotApprovedAndExceedsMaxLimit
        /// Then: ReturnsTrue
        /// </summary>
        [TestMethod(DisplayName = "OvertimeLimitCheckAsync: 未承認かつ時間外上限超過の場合、trueを返す")]
        public async Task OvertimeLimitCheckAsync_未承認かつ時間外上限超過の場合trueを返す()
        {
            var syain = SeedConfirmValidationSyain();
            syain.KintaiZokusei = new KintaiZokusei
            {
                Id = syain.KintaiZokuseiId,
                SeigenTime = 45m,
                MaxLimitTime = 80m
            };

            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務);

            var ukagai = new UkagaiHeader
            {
                Status = 承認待
            };

            var result = await InvokePrivateWithResultAsync<bool>(
                validate,
                "OvertimeLimitCheckAsync",
                4801m,
                ukagai);

            Assert.IsTrue(result, "時間外上限を超えているため true を返すべきです。");
        }

        /// <summary>
        /// Given: Calculate3MonthZangyoAsync の条件を満たしている
        /// When: FinishedMonthTotalIsNegative
        /// Then: MonthTotalBecomesZero
        /// </summary>
        [TestMethod(DisplayName = "Calculate3MonthZangyoAsync: 3ヶ月分の残業時間が負数の場合、0を返す")]
        public async Task Calculate3MonthZangyoAsync_3ヶ月分の残業時間が負数の場合0を返す()
        {
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(1)
                .Build();
            db.Syains.Add(syain);

            db.Nippous.Add(new NippouBuilder()
                .WithId(6001)
                .WithSyainId(syain.Id)
                .WithNippouYmd(D("20250131"))
                .WithTotalZangyo(-120m)
                .Build());
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;

            var total = await InvokePrivateWithResultAsync<decimal>(
                model,
                "Calculate3MonthZangyoAsync",
                D("20250331"));

            Assert.AreEqual(0m, total);
        }

        /// <summary>
        /// Given: Calculate3MonthZangyoAsync の条件を満たしている
        /// When: CurrentMonthIsNotFinishedAndNegative
        /// Then: NegativeIsKept
        /// </summary>
        [TestMethod(DisplayName = "Calculate3MonthZangyoAsync: 現在月が未終了かつ負数の場合、負数を保持する")]
        public async Task Calculate3MonthZangyoAsync_現在月が未終了かつ負数の場合負数を保持する()
        {
            EnsureKintaiZokusei(1);
            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(1)
                .WithKintaiZokuseiId(1)
                .Build();
            db.Syains.Add(syain);

            db.Nippous.Add(new NippouBuilder()
                .WithId(6002)
                .WithSyainId(syain.Id)
                .WithNippouYmd(D("20250310"))
                .WithTotalZangyo(-90m)
                .Build());
            await db.SaveChangesAsync();

            var model = CreateModel(syain);
            model.SyainId = syain.Id;

            var total = await InvokePrivateWithResultAsync<decimal>(
                model,
                "Calculate3MonthZangyoAsync",
                D("20250315"));

            Assert.AreEqual(-90m, total);
        }

        /// <summary>
        /// Given: HasApprovedInquiry の条件を満たしている
        /// When: OnlyDifferentHeaderIdExists
        /// Then: ReturnsFalse
        /// </summary>
        [TestMethod(DisplayName = "HasApprovedInquiry: ヘッダーIDが異なる場合、falseを返す")]
        public void HasApprovedInquiry_ヘッダーIDが異なる場合falseを返す()
        {
            var loginUser = new SyainBuilder().WithId(1).WithSyainBaseId(1).Build();
            var model = CreateModel(loginUser);

            var ukagaiHeader = new UkagaiHeader
            {
                Id = 100,
                Status = 承認,
                UkagaiShinseis = new List<UkagaiShinsei>
                {
                    new UkagaiShinsei
                    {
                        Id = 1,
                        UkagaiHeaderId = 999,
                        UkagaiSyubetsu = 夜間作業
                    }
                }
            };

            var method = typeof(IndexModel).GetMethod(
                "HasApprovedInquiry",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);
            var result = (bool)method.Invoke(model, new object[] { 夜間作業, ukagaiHeader })!;

            Assert.IsFalse(result);
        }

        /// <summary>
        /// Given: HasApprovedInquiry の条件を満たしている
        /// When: HeaderIdMatchesAndTypeMatches
        /// Then: ReturnsTrue
        /// </summary>
        [TestMethod(DisplayName = "HasApprovedInquiry: ヘッダーIDと種類が一致する場合、trueを返す")]
        public void HasApprovedInquiry_ヘッダーIDと種類が一致する場合trueを返す()
        {
            var loginUser = new SyainBuilder().WithId(1).WithSyainBaseId(1).Build();
            var model = CreateModel(loginUser);

            var ukagaiHeader = new UkagaiHeader
            {
                Id = 200,
                Status = 承認,
                UkagaiShinseis = new List<UkagaiShinsei>
                {
                    new UkagaiShinsei
                    {
                        Id = 2,
                        UkagaiHeaderId = 200,
                        UkagaiSyubetsu = 早朝作業
                    }
                }
            };

            var method = typeof(IndexModel).GetMethod(
                "HasApprovedInquiry",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);
            var result = (bool)method.Invoke(model, new object[] { 早朝作業, ukagaiHeader })!;

            Assert.IsTrue(result);
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: HolidayOnWorkday
        /// Then: HolidayOnWeekdayError
        /// </summary>
        [TestMethod(DisplayName = "CommonValidationAsync: 休日出勤かつ平日の場合、HolidayOnWeekdayErrorを返す")]
        public async Task CommonValidationAsync_休日出勤かつ平日の場合HolidayOnWeekdayErrorを返す()
        {
            var syain = SeedConfirmValidationSyain();
            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                休日,
                isWorkDay: true,
                modelState: modelState);

            await InvokePrivateAsync(
                validate,
                "CommonValidationAsync",
                new SyukkinKubun { IsSyukkin = true, IsVacation = false });

            AssertConfirmValidationError(modelState, "HolidayOnWeekdayError");
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: HolidayWorkOnWorkday
        /// Then: HolidayWorkOnWeekdayError
        /// </summary>
        [TestMethod(DisplayName = "CommonValidationAsync: 休日出勤かつ平日の場合、HolidayWorkOnWeekdayErrorを返す")]
        public async Task CommonValidationAsync_休日出勤かつ平日の場合HolidayWorkOnWeekdayErrorを返す()
        {
            var syain = SeedConfirmValidationSyain();
            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                AttendanceClassification.休日出勤,
                isWorkDay: true,
                modelState: modelState);

            await InvokePrivateAsync(
                validate,
                "CommonValidationAsync",
                new SyukkinKubun { IsSyukkin = true, IsVacation = false });

            AssertConfirmValidationError(modelState, "HolidayWorkOnWeekdayError");
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: NonWorkdayAndKubun1IsNotHoliday
        /// Then: SelectHolidayOnWeekend
        /// </summary>
        [TestMethod(DisplayName = "CommonValidationAsync: 休日出勤かつ平日の場合、SelectHolidayOnWeekendを返す")]
        public async Task CommonValidationAsync_休日出勤かつ平日の場合SelectHolidayOnWeekendを返す()
        {
            var syain = SeedConfirmValidationSyain();
            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250118"),
                通常勤務,
                isWorkDay: false,
                modelState: modelState);

            await InvokePrivateAsync(
                validate,
                "CommonValidationAsync",
                new SyukkinKubun { IsSyukkin = true, IsVacation = false });

            AssertConfirmValidationError(modelState, "SelectHolidayOnWeekend");
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: NonWorkdayAndKubun2Exists
        /// Then: InvalidAttendanceClassification
        /// </summary>
        [TestMethod(DisplayName = "CommonValidationAsync: 休日出勤かつ平日の場合、InvalidAttendanceClassificationを返す")]
        public async Task CommonValidationAsync_休日出勤かつ平日の場合InvalidAttendanceClassificationを返す()
        {
            var syain = SeedConfirmValidationSyain();
            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250118"),
                休日,
                syukkinKubun2: 欠勤,
                isWorkDay: false,
                modelState: modelState);

            await InvokePrivateAsync(
                validate,
                "CommonValidationAsync",
                new SyukkinKubun { IsSyukkin = true, IsVacation = false });

            AssertConfirmValidationError(modelState, "InvalidAttendanceClassification");
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: NonWorkdayHolidayWorkAndNoFuriYotei
        /// Then: EnterSubstituteHolidayDate
        /// </summary>
        [TestMethod(DisplayName = "CommonValidationAsync: 休日出勤かつ平日の場合、EnterSubstituteHolidayDateを返す")]
        public async Task CommonValidationAsync_休日出勤かつ平日の場合EnterSubstituteHolidayDateを返す()
        {
            var syain = SeedConfirmValidationSyain();
            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250118"),
                AttendanceClassification.休日出勤,
                isWorkDay: false,
                furiyoteiDate: null,
                modelState: modelState);

            await InvokePrivateAsync(
                validate,
                "CommonValidationAsync",
                new SyukkinKubun { IsSyukkin = true, IsVacation = false });

            AssertConfirmValidationError(modelState, "EnterSubstituteHolidayDate");
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: HolidayWithClockIn
        /// Then: CannotSelectHolidayWithClockIn
        /// </summary>
        [TestMethod(DisplayName = "CommonValidationAsync: 休日出勤かつ平日の場合、CannotSelectHolidayWithClockInを返す")]
        public async Task CommonValidationAsync_休日出勤かつ平日の場合CannotSelectHolidayWithClockInを返す()
        {
            var syain = SeedConfirmValidationSyain();
            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                休日,
                isWorkDay: true,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                modelState: modelState);

            await InvokePrivateAsync(validate, "CommonValidationAsync", (object?)null);

            AssertConfirmValidationError(modelState, "CannotSelectHolidayWithClockIn");
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: HalfDayWorkAndKubun2Exists
        /// Then: InvalidAttendanceClassification
        /// </summary>
            [TestMethod(DisplayName = "CommonValidationAsync: 半日勤務かつKubun2が存在する場合、InvalidAttendanceClassificationを返す")]
        public async Task CommonValidationAsync_半日勤務かつKubun2が存在する場合InvalidAttendanceClassificationを返す()
        {
            var syain = SeedConfirmValidationSyain();
            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日勤務,
                syukkinKubun2: 欠勤,
                isWorkDay: true,
                modelState: modelState);

            await InvokePrivateAsync(
                validate,
                "CommonValidationAsync",
                new SyukkinKubun { IsSyukkin = true, IsVacation = false });

            AssertConfirmValidationError(modelState, "InvalidAttendanceClassification");
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: HalfDayPaidAndKubun2None
        /// Then: InvalidAttendanceClassification
        /// </summary>
        [TestMethod(DisplayName = "CommonValidationAsync: 半日有給かつKubun2がNoneの場合、InvalidAttendanceClassificationを返す")]
        public async Task CommonValidationAsync_半日有給かつKubun2がNoneの場合InvalidAttendanceClassificationを返す()
        {
            var syain = SeedConfirmValidationSyain();
            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日有給,
                syukkinKubun2: None,
                isWorkDay: true,
                modelState: modelState);

            await InvokePrivateAsync(
                validate,
                "CommonValidationAsync",
                new SyukkinKubun { IsSyukkin = true, IsVacation = false });

            AssertConfirmValidationError(modelState, "InvalidAttendanceClassification");
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: BothHalfDayPaid
        /// Then: SelectAnnualPaidLeaveOneDay
        /// </summary>
        [TestMethod(DisplayName = "CommonValidationAsync: 両日半日有給の場合、SelectAnnualPaidLeaveOneDayを返す")]
        public async Task CommonValidationAsync_両日半日有給の場合SelectAnnualPaidLeaveOneDayを返す()
        {
            var syain = SeedConfirmValidationSyain();
            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日有給,
                syukkinKubun2: 半日有給,
                isWorkDay: true,
                modelState: modelState);

            await InvokePrivateAsync(
                validate,
                "CommonValidationAsync",
                new SyukkinKubun { IsSyukkin = true, IsVacation = false });

            AssertConfirmValidationError(modelState, "SelectAnnualPaidLeaveOneDay");
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: WorkingKubunAndNonPartWithKubun2
        /// Then: InvalidAttendanceClassification
        /// </summary>
        [TestMethod(DisplayName = "CommonValidationAsync: 勤務区分が通常勤務かつ非パートでKubun2が存在する場合、" +
            "InvalidAttendanceClassificationを返す")]
        public async Task
        CommonValidationAsync_勤務区分が通常勤務かつ非パートでKubun2が存在する場合InvalidAttendanceClassificationを返す()
        {
            var syain = SeedConfirmValidationSyain();
            syain.KintaiZokusei = new KintaiZokusei
            {
                Id = syain.KintaiZokuseiId,
                Code = 管理,
                Name = "管理",
                SeigenTime = 45m,
                MaxLimitTime = 80m
            };

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務,
                syukkinKubun2: 欠勤,
                isWorkDay: true,
                modelState: modelState);

            await InvokePrivateAsync(
                validate,
                "CommonValidationAsync",
                new SyukkinKubun { IsSyukkin = true, IsVacation = false });

            AssertConfirmValidationError(modelState, "InvalidAttendanceClassification");
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: NoWorkingTimeAndNormalWork
        /// Then: NotWorkingCannotSelectFormat
        /// </summary>
        [TestMethod(DisplayName = "CommonValidationAsync: 作業時間がない通常勤務の場合、NotWorkingCannotSelectFormatを返す")]
        public async Task CommonValidationAsync_作業時間がない通常勤務の場合NotWorkingCannotSelectFormatを返す()
        {
            var syain = SeedConfirmValidationSyain();
            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務,
                isWorkDay: true,
                modelState: modelState);

            await InvokePrivateAsync(
                validate,
                "CommonValidationAsync",
                new SyukkinKubun { IsSyukkin = true, IsVacation = false });

            var errors = GetModelStateErrors(modelState);
            Assert.IsTrue(
                errors.Any(e => e.Contains("選択することはできません")),
                $"Expected not-working selection error. errors={string.Join(" | ", errors)}");
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: NormalWorkAndShortHours
        /// Then: SelectHalfDayWorkDueToShortHours
        /// </summary>
        [TestMethod(DisplayName = "CommonValidationAsync: 通常勤務かつ短時間の場合、SelectHalfDayWorkDueToShortHoursを返す")]
        public async Task CommonValidationAsync_通常勤務かつ短時間の場合SelectHalfDayWorkDueToShortHoursを返す()
        {
            var syain = SeedConfirmValidationSyain();
            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                通常勤務,
                isWorkDay: true,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(12, 0),
                modelState: modelState);

            await InvokePrivateAsync(
                validate,
                "CommonValidationAsync",
                new SyukkinKubun { IsSyukkin = true, IsVacation = false });

            AssertConfirmValidationError(modelState, "SelectHalfDayWorkDueToShortHours");
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: PartWorkButEmployeeIsNotPart
        /// Then: CannotUsePartTimeWork
        /// </summary>
        [TestMethod(DisplayName = "CommonValidationAsync: パート勤務かつ非パートの場合、CannotUsePartTimeWorkを返す")]
        public async Task CommonValidationAsync_パート勤務かつ非パートの場合CannotUsePartTimeWorkを返す()
        {
            var syain = SeedConfirmValidationSyain();
            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                パート勤務,
                isWorkDay: true,
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                modelState: modelState);

            await InvokePrivateAsync(
                validate,
                "CommonValidationAsync",
                new SyukkinKubun { IsSyukkin = true, IsVacation = false });

            AssertConfirmValidationError(modelState, "CannotUsePartTimeWork");
        }

        /// <summary>
        /// Given: CommonValidationAsync の条件を満たしている
        /// When: HalfDayWorkAndLongHours
        /// Then: SelectNormalWork
        /// </summary>
        [TestMethod(DisplayName = "CommonValidationAsync: 半日勤務かつ長時間の場合、SelectNormalWorkを返す")]
        public async Task CommonValidationAsync_半日勤務かつ長時間の場合SelectNormalWorkを返す()
        {
            var syain = SeedConfirmValidationSyain();
            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日勤務,
                isWorkDay: true,
                syukkinHm1: new TimeOnly(0, 0),
                taisyutsuHm1: new TimeOnly(23, 59),
                modelState: modelState);

            await InvokePrivateAsync(
                validate,
                "CommonValidationAsync",
                new SyukkinKubun { IsSyukkin = true, IsVacation = false });

            AssertConfirmValidationError(modelState, "SelectNormalWork");
        }

        /// <summary>
        /// Given: TemporarySaveValidationAsync の条件を満たしている
        /// When: MaleAndPhysiologicalLeave
        /// Then: CannotTakePhysiologicalLeave
        /// </summary>
        [TestMethod(DisplayName = "TemporarySaveValidationAsync: 男性かつ生理休暇の場合、CannotTakePhysiologicalLeaveを返す")]
        public async Task TemporarySaveValidationAsync_男性かつ生理休暇の場合CannotTakePhysiologicalLeaveを返す()
        {
            var syain = SeedConfirmValidationSyain(seibetsu: '1');
            AddSyukkinKubun(生理休暇, isSyukkin: false, isVacation: true);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                生理休暇,
                modelState: modelState);

            await validate.TemporarySaveValidationAsync();

            AssertConfirmValidationError(modelState, "CannotTakePhysiologicalLeave");
        }

        /// <summary>
        /// Given: TemporarySaveValidationAsync の条件を満たしている
        /// When: HalfDayPaidAndOneDayPaidAsKubun2
        /// Then: InvalidAttendanceClassification
        /// </summary>
        [TestMethod(DisplayName = "TemporarySaveValidationAsync: 半日有給かつ1日有給としてKubun2が設定されている場合、" +
            "InvalidAttendanceClassificationを返す")]
        public async Task TemporarySaveValidationAsync_半日有給かつ1日有給としてKubun2が設定されている場合InvalidAttendanceClassificationを返す()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(半日有給, isSyukkin: false, isVacation: true);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日有給,
                年次有給休暇_1日,
                modelState: modelState);

            await validate.TemporarySaveValidationAsync();

            AssertConfirmValidationError(modelState, "InvalidAttendanceClassification");
        }

        /// <summary>
        /// Given: TemporarySaveValidationAsync の条件を満たしている
        /// When: PlannedSpecialLeaveAndKubun2None
        /// Then: InvalidAttendanceClassification
        /// </summary>
        [TestMethod(DisplayName = "TemporarySaveValidationAsync: 計画特別休暇かつKubun2がNoneの場合、InvalidAttendanceClassificationを返す")]
        public async Task TemporarySaveValidationAsync_計画特別休暇かつKubun2がNoneの場合InvalidAttendanceClassificationを返す()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(計画特別休暇, isSyukkin: false, isVacation: true);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                計画特別休暇,
                None,
                modelState: modelState);

            await validate.TemporarySaveValidationAsync();

            AssertConfirmValidationError(modelState, "InvalidAttendanceClassification");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: HalfDayPaidAndLinkedProject
        /// Then: CannotSelectProjectDuringLeave
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 半日有給かつ連動案件ありの場合、CannotSelectProjectDuringLeaveを返す")]
        public async Task
        FinalConfirmValidationAsync_半日有給かつ連動案件ありの場合CannotSelectProjectDuringLeaveを返す()
        {
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(半日有給, isSyukkin: false, isVacation: true);
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuYukyuSu = 1,
                Wariate = 1m,
                Kurikoshi = 0m,
                Syouka = 0m,
                HannitiKaisuu = 0
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日有給,
                None,
                modelState: modelState);

            await validate.FinalConfirmValidationAsync(CreateNippouAnkens(1, 1), 0m);

            AssertConfirmValidationError(modelState, "CannotSelectProjectDuringLeave");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: HalfDayPaidAndKubunMasterMissing
        /// Then: CannotSelectProjectDuringLeave
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 半日有給かつ区分マスタが未登録の場合、CannotSelectProjectDuringLeaveを返す")]
        public async Task
        FinalConfirmValidationAsync_半日有給かつ区分マスタが未登録の場合CannotSelectProjectDuringLeaveを返す()
        {
            // ルール28の後半条件を確認する
            // (区分1が半日有給または半日振休) かつ 連動案件あり
            var syain = SeedConfirmValidationSyain();
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syain.SyainBaseId,
                KeikakuYukyuSu = 1,
                Kurikoshi = 0m,
                Syouka = 0m,
                HannitiKaisuu = 0
            });
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                半日有給,
                None,
                modelState: modelState);

            // 区分マスタを未登録にして kubun1Info が null の経路を通す
            await validate.FinalConfirmValidationAsync(CreateNippouAnkens(1, 1), 0m);

            AssertConfirmValidationError(modelState, "CannotSelectProjectDuringLeave");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: HolidayWorkWithoutLinkedProject
        /// Then: SelectProjectCodeErrorPrecedes
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 休日出勤かつ連動案件なしの場合、SelectProjectCodeErrorPrecedesを返す")]
        public async Task
        FinalConfirmValidationAsync_休日出勤かつ連動案件なしの場合SelectProjectCodeErrorPrecedesを返す()
        {
            // 現行実装は 25-1 を先に判定するため、25-2 には到達しない
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(AttendanceClassification.休日出勤, isSyukkin: true, isVacation: false);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                AttendanceClassification.休日出勤,
                isWorkDay: false,
                furiyoteiDate: D("20250121"),
                syukkinHm1: new TimeOnly(9, 0),
                taisyutsuHm1: new TimeOnly(18, 0),
                dJitsudou: 8m,
                modelState: modelState);

            await validate.FinalConfirmValidationAsync(CreateNippouAnkens(1, 0), 0m);

            AssertConfirmValidationError(modelState, "SelectProjectCode");

            var errors = GetModelStateErrors(modelState);
            Assert.DoesNotContain(
                GetConfirmValidationConstant("CreateProjectInfoForHolidayWork"),
                errors,
                "判定順序上、25-1 成立時は 25-2 のエラーに到達しないこと。");
        }

        /// <summary>
        /// Given: FinalConfirmValidationAsync の条件を満たしている
        /// When: PlannedAnnualLeaveAndNoPaidLeaveData
        /// Then: AnnualLeaveErrorPrecedes
        /// </summary>
        [TestMethod(DisplayName = "FinalConfirmValidationAsync: 計画有給休暇かつ有給データなしの場合、AnnualLeaveErrorPrecedesを返す")]
        public async Task
        FinalConfirmValidationAsync_計画有給休暇かつ有給データなしの場合AnnualLeaveErrorPrecedesを返す()
        {
            // 現行実装は remainingPaidLeave<1 を先に判定するため、21-3 には到達しない
            var syain = SeedConfirmValidationSyain();
            AddSyukkinKubun(計画有給休暇, isSyukkin: false, isVacation: true);
            await db.SaveChangesAsync();

            var modelState = new ModelStateDictionary();
            var validate = CreateConfirmValidation(
                syain,
                D("20250120"),
                計画有給休暇,
                modelState: modelState);

            await validate.FinalConfirmValidationAsync(CreateNippouAnkens(1, 1), 0m);

            AssertConfirmValidationError(modelState, "CannotTakeAnnualPaidLeave");

            var errors = GetModelStateErrors(modelState);
            var paidLeaveDataNotFound = string.Format(
                GetConfirmValidationConstant("PaidLeaveDataNotFoundFormat"),
                syain.Code);
            Assert.DoesNotContain(
                paidLeaveDataNotFound,
                errors,
                "判定順序上、21-1 成立時は 21-3 のエラーに到達しないこと。");
        }

        /// <summary>
        /// Given: CancelHalfDayPaidLeaveAsync の条件を満たしている
        /// When: YuukyuuNotFound
        /// Then: ReturnWithoutUpdate
        /// </summary>
        [TestMethod(DisplayName = "CancelHalfDayPaidLeaveAsync: 有給残が見つからない場合、更新せずに返す")]
        public async Task CancelHalfDayPaidLeaveAsync_有給残が見つからない場合更新せずに返す()
        {
            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                1,
                None,
                None,
                isWorkDay: true);

            await InvokePrivateAsync(
                leave,
                "CancelHalfDayPaidLeaveAsync",
                999L,
                通常勤務);

            Assert.AreEqual(0, await db.YuukyuuZans.CountAsync(), "YuukyuuZan未登録時は更新しないこと。");
        }

        /// <summary>
        /// Given: CancelPlannedSpecialDayAsync の条件を満たしている
        /// When: YuukyuuNotFound
        /// Then: ReturnWithoutUpdate
        /// </summary>
        [TestMethod(DisplayName = "CancelPlannedSpecialDayAsync: 有給残が見つからない場合、更新せずに返す")]
        public async Task CancelPlannedSpecialDayAsync_有給残が見つからない場合更新せずに返す()
        {
            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                1,
                None,
                None,
                isWorkDay: true);

            await InvokePrivateAsync(
                leave,
                "CancelPlannedSpecialDayAsync",
                999L);

            Assert.AreEqual(0, await db.YuukyuuZans.CountAsync(), "YuukyuuZan未登録時は更新しないこと。");
        }

        /// <summary>
        /// Given: CancelHalfFurikyuuAsync の条件を満たしている
        /// When: NoMatchingRecord
        /// Then: ReturnWithoutUpdate
        /// </summary>
        [TestMethod(DisplayName = "CancelHalfFurikyuuAsync: 対象レコードがない場合、更新せずに返す")]
        public async Task CancelHalfFurikyuuAsync_対象レコードがない場合更新せずに返す()
        {
            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                1,
                None,
                半日振休,
                isWorkDay: true);

            await InvokePrivateAsync(
                leave,
                "CancelHalfFurikyuuAsync",
                1L,
                D("20250120"));

            Assert.AreEqual(0, await db.FurikyuuZans.CountAsync(), "対象振休がない場合は更新しないこと。");
        }

        /// <summary>
        /// Given: CancelHalfFurikyuuAsync の条件を満たしている
        /// When: SyutokuYmd1Matches
        /// Then: ClearYmd1AndSetMi
        /// </summary>
        [TestMethod(DisplayName = "CancelHalfFurikyuuAsync: 取得日1が一致する場合、取得日1をクリアし状態を未に戻す")]
        public async Task CancelHalfFurikyuuAsync_取得日1が一致する場合取得日1をクリアし状態を未に戻す()
        {
            var jissekiDate = D("20250120");
            var furikyuu = new FurikyuuZan
            {
                SyainId = 1,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = _1日,
                SyutokuYmd1 = jissekiDate
            };
            db.FurikyuuZans.Add(furikyuu);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                1,
                None,
                半日振休,
                isWorkDay: true);

            await InvokePrivateAsync(
                leave,
                "CancelHalfFurikyuuAsync",
                1L,
                jissekiDate);

            var updated = await db.FurikyuuZans.FindAsync(furikyuu.Id);
            Assert.IsNull(updated!.SyutokuYmd1, "取得日1がクリアされること。");
            Assert.AreEqual(未, updated.SyutokuState, "状態が未に戻ること。");
        }

        /// <summary>
        /// Given: CancelFurikyuuAsync の条件を満たしている
        /// When: NoPrimaryRecord
        /// Then: ReturnWithoutUpdate
        /// </summary>
        [TestMethod(DisplayName = "CancelFurikyuuAsync: 主レコードがない場合、更新せずに返す")]
        public async Task CancelFurikyuuAsync_主レコードがない場合更新せずに返す()
        {
            var leave = CreateCompensatoryPaidLeave(
                D("20250120"),
                1,
                振替休暇,
                None,
                isWorkDay: true);

            await InvokePrivateAsync(
                leave,
                "CancelFurikyuuAsync",
                1L,
                D("20250120"));

            Assert.AreEqual(0, await db.FurikyuuZans.CountAsync(), "対象振休がない場合は更新しないこと。");
        }

        /// <summary>
        /// Given: CancelFurikyuuAsync の条件を満たしている
        /// When: 半日振替休暇が取得していない場合
        /// Then: OnlyPrimaryIsUpdated
        /// </summary>
        [TestMethod(DisplayName = "CancelFurikyuuAsync: 半日振替休暇が取得していない場合、関連レコードを更新しない")]
        public async Task CancelFurikyuuAsync_半日振替休暇が取得していない場合関連レコードを更新しない()
        {
            var jissekiDate = D("20250120");
            var first = new FurikyuuZan
            {
                SyainId = 1,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = _1日,
                SyutokuYmd1 = jissekiDate
            };
            var second = new FurikyuuZan
            {
                SyainId = 1,
                KyuujitsuSyukkinYmd = D("20250102"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = _1日,
                SyutokuYmd1 = jissekiDate
            };
            db.FurikyuuZans.AddRange(first, second);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                1,
                振替休暇,
                None,
                isWorkDay: true);

            await InvokePrivateAsync(
                leave,
                "CancelFurikyuuAsync",
                1L,
                jissekiDate);

            var firstUpdated = await db.FurikyuuZans.FindAsync(first.Id);
            var secondUpdated = await db.FurikyuuZans.FindAsync(second.Id);
            Assert.IsNull(firstUpdated!.SyutokuYmd1, "主レコードの取得日1がクリアされること。");
            Assert.AreEqual(未, firstUpdated.SyutokuState, "主レコードが未に戻ること。");
            Assert.AreEqual(jissekiDate, secondUpdated!.SyutokuYmd1, "半日振替休暇が取得していない場合、関連レコードを更新しないこと。");
        }

        /// <summary>
        /// Given: CancelFurikyuuAsync の条件を満たしている
        /// When: 半日振替休暇が取得している場合ByOneDayHalfAndSecondYmd1Match
        /// Then: SecondIsCleared
        /// </summary>
        [TestMethod(DisplayName = "CancelFurikyuuAsync: 半日振替休暇が取得して関連レコードの取得日1が一致する場合、" +
            "関連レコードの取得日1がクリアされ状態が未になる")]
        public async Task CancelFurikyuuAsync_半日振替休暇が取得して関連レコードの取得日1が一致する場合関連レコードの取得日1がクリアされ状態が未になる()
        {
            var jissekiDate = D("20250120");
            var first = new FurikyuuZan
            {
                SyainId = 1,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = 半日,
                SyutokuYmd1 = jissekiDate
            };
            var second = new FurikyuuZan
            {
                SyainId = 1,
                KyuujitsuSyukkinYmd = D("20250102"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = _1日,
                SyutokuYmd1 = jissekiDate
            };
            db.FurikyuuZans.AddRange(first, second);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                1,
                None,
                None,
                isWorkDay: true);

            await InvokePrivateAsync(
                leave,
                "CancelFurikyuuAsync",
                1L,
                jissekiDate);

            var firstUpdated = await db.FurikyuuZans.FindAsync(first.Id);
            var secondUpdated = await db.FurikyuuZans.FindAsync(second.Id);

            Assert.IsNull(firstUpdated!.SyutokuYmd1, "primary SyutokuYmd1 should be cleared.");
            Assert.AreEqual(未, firstUpdated.SyutokuState, "primary state should become 未.");
            Assert.IsNull(
                secondUpdated!.SyutokuYmd1,
                "関連レコードの取得日1が対象日一致時にクリアされること。");
            Assert.AreEqual(未, secondUpdated.SyutokuState, "second state should become 未 for SyutokuYmd1 branch.");
        }

        /// <summary>
        /// Given: CancelFurikyuuAsync の条件を満たしている
        /// When: 半日振替休暇が取得している場合AndSecondYmd2Match
        /// Then: SecondYmd2ClearedAndStateHalf
        /// </summary>
        [TestMethod(DisplayName = "CancelFurikyuuAsync: 半日振替休暇が取得して関連レコードの取得日2が一致する場合、" +
            "関連レコードの取得日2がクリアされ状態が半日になる")]
        public async Task CancelFurikyuuAsync_半日振替休暇が取得して関連レコードの取得日2が一致する場合関連レコードの取得日2がクリアされ状態が半日になる()
        {
            var jissekiDate = D("20250120");
            var first = new FurikyuuZan
            {
                SyainId = 1,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = _1日,
                SyutokuYmd1 = jissekiDate
            };
            var second = new FurikyuuZan
            {
                SyainId = 1,
                KyuujitsuSyukkinYmd = D("20250102"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = true,
                SyutokuState = _1日,
                SyutokuYmd2 = jissekiDate
            };
            db.FurikyuuZans.AddRange(first, second);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                1,
                None,
                None,
                isWorkDay: true);

            await InvokePrivateAsync(
                leave,
                "CancelFurikyuuAsync",
                1L,
                jissekiDate);

            var firstUpdated = await db.FurikyuuZans.FindAsync(first.Id);
            var secondUpdated = await db.FurikyuuZans.FindAsync(second.Id);

            Assert.IsNull(firstUpdated!.SyutokuYmd1, "primary SyutokuYmd1 should be cleared.");
            Assert.AreEqual(未, firstUpdated.SyutokuState, "primary state should become 未.");
            Assert.IsNull(
                secondUpdated!.SyutokuYmd2,
                "関連レコードの取得日2が対象日一致時にクリアされること。");
            Assert.AreEqual(半日, secondUpdated.SyutokuState, "second state should become 半日 for SyutokuYmd2 branch.");
        }

        /// <summary>
        /// Given: CancelFurikyuuAsync の条件を満たしている
        /// When: 半日振替休暇が取得している場合AndSecondNotFound
        /// Then: OnlyPrimaryUpdated
        /// </summary>
        [TestMethod(DisplayName = "CancelFurikyuuAsync: 半日振替休暇が取得して関連レコードが見つからない場合、主レコードのみ更新される")]
        public async Task CancelFurikyuuAsync_半日振替休暇が取得して関連レコードが見つからない場合主レコードのみ更新される()
        {
            var jissekiDate = D("20250120");
            var first = new FurikyuuZan
            {
                SyainId = 1,
                KyuujitsuSyukkinYmd = D("20250101"),
                DaikyuuKigenYmd = D("20251231"),
                IsOneDay = false,
                SyutokuState = _1日,
                SyutokuYmd1 = jissekiDate
            };
            db.FurikyuuZans.Add(first);
            await db.SaveChangesAsync();

            var leave = CreateCompensatoryPaidLeave(
                jissekiDate,
                1,
                振替休暇,
                None,
                isWorkDay: true);

            await InvokePrivateAsync(
                leave,
                "CancelFurikyuuAsync",
                1L,
                jissekiDate);

            var firstUpdated = await db.FurikyuuZans.FindAsync(first.Id);
            Assert.IsNull(firstUpdated!.SyutokuYmd1, "主レコードの取得日1がクリアされること。");
            Assert.AreEqual(未, firstUpdated.SyutokuState, "主レコードが未に戻ること。");
        }

        #endregion

        #endregion
    }

}
