using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Model.Enums;
using Model.Model;
using System.Text.Json;
using Zouryoku;
using Zouryoku.Pages.KinmuJokyoKakunin;
using static Model.Enums.LeaveBalanceFetchStatus;
using static Model.Enums.ResponseStatus;
using static Zouryoku.Pages.KinmuJokyoKakunin.WarnLevel;

namespace ZouryokuTest.Pages.KinmuJokyoKakunin
{
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {

        private IOptions<AppConfig> CreateOptions(Action<AppSettings>? configure = null)
        {
            var settings = new AppSettings
            {
                WebApplication = new ZouryokuCommonLibrary.WebApplication(),
                MailPath = new ZouryokuCommonLibrary.MailPath()
                {
                    Host = "smtp.example.com",
                    Port = 587,
                    FromMail = "test@example.com",
                    RequestHost = "http://localhost",
                },
                TemplatesFolderPath = "/Templates",
                KinmuJokyoFileName = "KinmuJokyo.xlsx",
                AvgMaxWarn = 70,
                AvgMaxNotice = 60,
                YearTotalZangyoExceptHolidayWarn = 700,
                YearTotalZangyoExceptHolidayNotice = 660,
                OverLimitCountWarn = 6,
                OverLimitCountNotice = 4,
                MaxConsecutiveWorkingDaysWarn = 10,
                MaxConsecutiveWorkingDaysNotice = 7,
                PaidYearTotalWarn12To1 = 1,
                PaidYearTotalNotice12To1 = 2,
                PaidYearTotalWarn2To3 = 2,
                PaidYearTotalNotice2To3 = 3,
            };

            configure?.Invoke(settings);

            var appConfig = new AppConfig
            {
                AppSettings = settings
            };

            return Options.Create(appConfig);
        }

        private IndexModel CreateModel()
        {
            IndexModel model = new IndexModel(
                db, GetLogger<IndexModel>(),
                options = CreateOptions(),
                viewEngine,
                fakeTimeProvider)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData(),
            };
            return model;
        }

        #region OnGet
        /// <summary>
        /// 前提: 画面初期表示（OnGet）が呼び出される
        /// 操作: OnGet() を実行する
        /// 結果: 
        /// ・SearchIndex が初期化されていること
        /// ・From / To が当月（yyyy-MM）で設定されていること
        /// ・WarnLevel が All に設定されていること
        /// </summary>
        [TestMethod]
        public void OnGet_SetsDefaultSearchIndex()
        {
            // Arrange
            IndexModel model = CreateModel();
            var today = new DateOnly(2026, 2, 26);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            string expectedPrefix = today.ToString("yyyy-MM");

            // Act
            model.OnGet();

            // Assert
            Assert.IsNotNull(model.SearchIndex);
            Assert.StartsWith(expectedPrefix, model.SearchIndex.From);
            Assert.StartsWith(expectedPrefix, model.SearchIndex.To);
            Assert.AreEqual(All, model.SearchIndex.WarnLevel);
            Assert.AreEqual("all", model.SearchIndex.BusyoMode);
        }

        #endregion



        #region OnGetSearchAsync

        /// <summary>
        /// 前提: 部署モードが「all」以外で、部署が未指定の検索条件が設定されている
        /// 操作: 検索処理（OnGetSearchAsync）を実行する
        /// 結果: エラーステータスとエラーメッセージが返却される
        /// </summary>
        [TestMethod]
        public async Task OnGetSearchAsync_部署モードがall以外_部署未指定の場合はエラー()
        {
            // Arrange
            IndexModel model = CreateModel();
            StatusSearchViewModel search = new StatusSearchViewModel
            {
                From = "2026-02",
                To = "2026-02",
                BusyoMode = "select",
                Busyo = "",
                WarnLevel = All
            };

            // Act
            IActionResult result = await model.OnGetSearchAsync(search);

            // Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            JsonResult json = (JsonResult)result;
            ResponseStatus status = GetResponseStatus(json);
            string? message = GetMessage(json);
            Assert.AreEqual(エラー, status);
            Assert.IsNotNull(message);
        }


        /// <summary>
        /// 前提: 正常に機能する
        /// 操作: 索処理（OnGetSearchAsync）を実行し、HTML データを取得する
        /// 結果: 正常ステータスが返却され、HTML データが存在する。また、セッションに TableViewModel が格納されている
        /// </summary>
        [TestMethod]
        public async Task OnGetSearchAsync_Success_ReturnsHtmlData()
        {
            //Arrange
            IndexModel model = CreateModel();
            StatusSearchViewModel search = new StatusSearchViewModel
            {
                From = "2024-01",
                To = "2027-02",
                BusyoMode = "select",
                Busyo = "[1, 2, 5]",
                WarnLevel = All
            };
            var busyo = CreateBusyo(id: 1);
            db.Busyos.Add(busyo);

            var kintaiZokusei = CreateKintaiZokusei(id: 1);
            db.KintaiZokuseis.Add(kintaiZokusei);

            var syainBasis = CreateSyainBasis(id: 1);
            db.SyainBases.Add(syainBasis);

            var syain = CreateSyain(id: 1, syainBaseId: syainBasis.Id);
            db.Syains.Add(syain);

            var syainBasis2 = CreateSyainBasis(id: 2);
            db.SyainBases.Add(syainBasis2);

            var syain2 = CreateSyain(id: 2, syainBaseId: syainBasis2.Id);
            db.Syains.Add(syain2);

            var syainBasis3 = CreateSyainBasis(id: 3);
            db.SyainBases.Add(syainBasis3);

            var syain3 = CreateSyain(id: 3, syainBaseId: syainBasis3.Id);
            db.Syains.Add(syain3);

            var syukkinKubun1 = CreateSyukkinKubun(
                id: 1,
                name: "年次有給休暇_1日",
                nameRyaku: "年次有給休暇_1日");
            db.SyukkinKubuns.Add(syukkinKubun1);

            var syukkinKubun2 = CreateSyukkinKubun(
                id: 2,
                name: "未設定",
                nameRyaku: "-");
            db.SyukkinKubuns.Add(syukkinKubun2);

            var syukkinKubun3 = CreateSyukkinKubun(
                id: 3,
                name: "通常勤務",
                nameRyaku: "通常勤務");
            db.SyukkinKubuns.Add(syukkinKubun3);

            var syukkinKubun4 = CreateSyukkinKubun(
                id: 4,
                name: "休日",
                nameRyaku: "休日");
            db.SyukkinKubuns.Add(syukkinKubun4);

            var syukkinKubun5 = CreateSyukkinKubun(
                id: 5,
                name: "半日有給",
                nameRyaku: "半日有給");
            db.SyukkinKubuns.Add(syukkinKubun5);

            var nippou = CreateNippou(
                id: 1,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/2/5"),
                syukkinKubunId1: syukkinKubun1.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippou);

            var nippou2 = CreateNippou(
                id: 2,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/3/5"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id,
                hZangyo: 670);
            db.Nippous.Add(nippou2);

            var ukagaiHeader = CreateUkagaiHeader(
                id: 1,
                syainId: syain.Id,
                workYmd: DateOnly.Parse("2024/2/5"));
            db.UkagaiHeaders.Add(ukagaiHeader);

            var ukagaiShinsei = CreateUkagaiShinsei(
                id: 1,
                ukagaiHeaderId: ukagaiHeader.Id,
                ukagaiSyubetsu: InquiryType.時間外労働時間制限拡張);
            db.UkagaiShinseis.Add(ukagaiShinsei);

            var nippou3 = CreateNippou(
                id: 3,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2025/3/5"),
                syukkinKubunId1: syukkinKubun5.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippou3);

            var nippou4 = CreateNippou(
                id: 4,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2025/10/10"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippou4);

            var nippou5 = CreateNippou(
                id: 100,
                syainId: syain3.Id,
                nippouYmd: DateOnly.Parse("2025/10/12"),
                syukkinKubunId1: syukkinKubun4.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippou5);

            FurikyuuZan furikyuu = new FurikyuuZan
            {
                Id = 1,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = DateOnly.Parse("2024/2/11"),
                DaikyuuKigenYmd = DateOnly.Parse("2027/12/31"),
                IsOneDay = true,
                SyutokuState = 未
            };
            db.FurikyuuZans.Add(furikyuu);

            FurikyuuZan furikyuu2 = new FurikyuuZan
            {
                Id = 2,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = DateOnly.Parse("2024/2/11"),
                DaikyuuKigenYmd = DateOnly.Parse("2027/12/31"),
                IsOneDay = true,
                SyutokuState = 半日
            };
            db.FurikyuuZans.Add(furikyuu2);

            FurikyuuZan furikyuu3 = new FurikyuuZan
            {
                Id = 3,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = DateOnly.Parse("2024/2/11"),
                DaikyuuKigenYmd = DateOnly.Parse("2027/12/31"),
                IsOneDay = false,
                SyutokuState = 未
            };
            db.FurikyuuZans.Add(furikyuu3);

            FurikyuuZan furikyuu4 = new FurikyuuZan
            {
                Id = 4,
                SyainId = syain.Id,
                KyuujitsuSyukkinYmd = DateOnly.Parse("2024/2/11"),
                DaikyuuKigenYmd = DateOnly.Parse("2027/12/31"),
                IsOneDay = false,
                SyutokuState = _1日
            };
            db.FurikyuuZans.Add(furikyuu4);

            YuukyuuZan yukyuu = new YuukyuuZan
            {
                Id = 1,
                SyainBaseId = syainBasis.Id,
                Wariate = 10,
                Kurikoshi = 5,
                Syouka = 2.5m,
                HannitiKaisuu = 2,
                KeikakuYukyuSu = 15,
                KeikakuTokukyuSu = 5
            };
            db.YuukyuuZans.Add(yukyuu);

            YuukyuuZan yukyuu2 = new YuukyuuZan
            {
                Id = 2,
                SyainBaseId = syainBasis2.Id,
                Wariate = 10,
                Kurikoshi = 5,
                Syouka = 4,
                HannitiKaisuu = 2,
                KeikakuYukyuSu = 15,
                KeikakuTokukyuSu = 5
            };
            db.YuukyuuZans.Add(yukyuu);

            YukyuRireki yukyuRereki1 = new YukyuRireki()
            {
                Id = 1,
                YukyuNendoId = 1,
                SyainBaseId = 1,
                Wariate = 20,
                Kurikoshi = 5,
                Syouka = 4,
                HannitiKaisuu = 2,
            };
            db.YukyuRirekis.Add(yukyuRereki1);

            YukyuNendo yukyuNendo1 = new YukyuNendo()
            {
                Id = 1,
                Nendo = 2024,
                StartDate = DateOnly.Parse("2024/4/1"),
                EndDate = DateOnly.Parse("2025/3/31"),
                IsThisYear = false
            };
            db.YukyuNendos.Add(yukyuNendo1);

            YukyuNendo yukyuNendo2 = new YukyuNendo()
            {
                Id = 2,
                Nendo = 2025,
                StartDate = DateOnly.Parse("2025/4/1"),
                EndDate = DateOnly.Parse("2026/3/31"),
                IsThisYear = true,
            };
            db.YukyuNendos.Add(yukyuNendo2);

            var nippou1Syain2 = CreateNippou(
                id: 40,
                syainId: syain2.Id,
                nippouYmd: DateOnly.Parse("2025/10/5"),
                syukkinKubunId1: syukkinKubun1.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippou1Syain2);

            var nippou2Syain2 = CreateNippou(
                id: 41,
                syainId: syain2.Id,
                nippouYmd: DateOnly.Parse("2025/11/6"),
                syukkinKubunId1: syukkinKubun5.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippou2Syain2);

            var nippou3Syain2 = CreateNippou(
                id: 42,
                syainId: syain2.Id,
                nippouYmd: DateOnly.Parse("2024/3/3"),
                syukkinKubunId1: syukkinKubun1.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippou3Syain2);

            var nippou4Syain2 = CreateNippou(
                id: 43,
                syainId: syain2.Id,
                nippouYmd: DateOnly.Parse("2024/2/5"),
                syukkinKubunId1: syukkinKubun1.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippou4Syain2);

            var nippou5Syain2 = CreateNippou(
                id: 44,
                syainId: syain2.Id,
                nippouYmd: DateOnly.Parse("2024/2/6"),
                syukkinKubunId1: syukkinKubun5.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippou5Syain2);

            var nippouConsecutive1 = CreateNippou(
                id: 6,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/10/30"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutive1);

            var nippouConsecutive2 = CreateNippou(
                id: 7,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/10/31"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutive2);

            var nippouConsecutive3 = CreateNippou(
                id: 8,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/11/1"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutive3);

            var nippouConsecutive4 = CreateNippou(
                id: 9,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/11/2"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutive4);

            var nippouConsecutive5 = CreateNippou(
                id: 10,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/11/3"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutive5);

            var nippouConsecutive6 = CreateNippou(
                id: 11,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/11/4"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutive6);

            var nippouConsecutive7 = CreateNippou(
                id: 12,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/11/5"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutive7);

            var nippouConsecutive8 = CreateNippou(
                id: 13,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/11/6"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutive8);

            var nippouConsecutiveB1 = CreateNippou(
                id: 21,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/12/1"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutiveB1);

            var nippouConsecutiveB2 = CreateNippou(
                id: 22,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/12/2"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutiveB2);

            var nippouConsecutiveB3 = CreateNippou(
                id: 23,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/12/3"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutiveB3);

            var nippouConsecutiveB4 = CreateNippou(
                id: 24,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/12/4"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutiveB4);

            var nippouConsecutiveB5 = CreateNippou(
                id: 25,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/12/5"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutiveB5);

            var nippouConsecutiveB6 = CreateNippou(
                id: 26,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/12/6"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutiveB6);

            var nippouConsecutiveB7 = CreateNippou(
                id: 27,
                syainId: syain.Id,
                nippouYmd: DateOnly.Parse("2024/12/7"),
                syukkinKubunId1: syukkinKubun3.Id,
                syukkinKubunId2: syukkinKubun2.Id);
            db.Nippous.Add(nippouConsecutiveB7);

            await db.SaveChangesAsync();

            //Act
            IActionResult result = await model.OnGetSearchAsync(search);

            //Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            JsonResult json = (JsonResult)result;
            ResponseStatus status = GetResponseStatus(json);
            Assert.AreEqual(正常, status);

            object val = json.Value ?? throw new AssertFailedException("JSON の値が null です");

            System.Reflection.PropertyInfo? dataProp = val.GetType().GetProperty("data");
            dataProp ??= val.GetType().GetProperty("Data");
            Assert.IsNotNull(dataProp, "JSON に data または Data プロパティが存在する必要があります。");

            object? data = dataProp!.GetValue(val);
            Assert.IsNotNull(data);

            string? html = data as string;
            Assert.IsNotNull(html, "返却された HTML は文字列である必要があります。");


            byte[]? stored;
            bool has = model.PageContext.HttpContext.Session
                .TryGetValue("StatusView_TableViewModel", out stored);

            Assert.IsTrue(has, "検索後、セッションに StatusView_TableViewModel が保存されている必要があります。");
            Assert.IsNotNull(stored);
        }

        #endregion


        #region OnGetExportExcelAsync
        /// <summary>
        /// 前提: Export 用の検索結果が Session に存在しない、または不正な状態である
        /// 操作: OnGetExportExcelAsync を呼び出す
        /// 結果: 
        /// ・Session にキーが存在しない場合は BadRequest（検索結果が存在しません）
        /// ・Session に "null" が格納されている場合は BadRequest（検索結果の取得に失敗しました）
        /// </summary>
        [TestMethod]
        public async Task OnGetExportExcelAsync_SessionErrors()
        {
            // Arrange
            IndexModel model = CreateModel();

            // Case A: 「検索結果が存在しません」というメッセージ付きの BadRequest が返ること
            // Act
            IActionResult resultA = await model.OnGetExportExcelAsync();

            // Assert
            Assert.IsInstanceOfType(resultA, typeof(BadRequestObjectResult));
            var badA = (BadRequestObjectResult)resultA;
            Assert.IsTrue(badA.Value is string && ((string)badA.Value).Contains("検索結果が存在しません"));

            // Case B: 「検索結果の取得に失敗しました」というメッセージ付きの BadRequest が返ること
            // Act
            string sessionKey = "StatusView_TableViewModel";
            model.HttpContext.Session.SetString(sessionKey, "null");
            IActionResult resultB = await model.OnGetExportExcelAsync();

            // Assert
            Assert.IsInstanceOfType(resultB, typeof(BadRequestObjectResult));
            var badB = (BadRequestObjectResult)resultB;
            Assert.IsTrue(badB.Value is string && ((string)badB.Value).Contains("検索結果の取得に失敗しました"));
        }


        /// <summary>
        /// 前提:
        /// ・Export 用の検索結果（TableViewModel）が Session に正しく保存されている
        /// ・Excel テンプレートファイルのパスが正しく設定されている
        /// 操作:
        /// ・OnGetExportExcelAsync を呼び出す
        /// 結果:
        /// ・FileContentResult が返却される
        /// ・ダウンロードファイル名が「勤務状況.xlsx」である
        /// </summary>
        [TestMethod]
        public async Task OnGetExportExcelAsync_success()
        {
            // Arrange
            var model = CreateModel();
            var testDir = Directory.GetCurrentDirectory();

            var projectDir = Path.GetFullPath(
                Path.Combine(testDir, @"..\..\..\..\Zouryoku")
            );

            model.Dir = projectDir;

            // Prepare session data
            var vm = new TableViewModel
            {
                WorkList = new List<WorkRowViewModel>
                {
                    new WorkRowViewModel
                    {
                        BusyoName = "営業部",
                        SyainName = "山田太郎",
                        ZokuseiName = "正社員",
                        YearMonth = "2026-02",
                        Jitsudo = 8,
                        ZangyoExceptHoliday = 2,
                        Zangyo = 3,
                        AverageMax = 4,
                        YearTotal = 100,
                        OverLimitCount = 1,
                        MaxConsecutiveWorkingDays = "10"
                    },
                    new WorkRowViewModel
                    {
                        BusyoName = "開発部",
                        SyainName = "佐藤花子",
                        ZokuseiName = "契約社員",
                        YearMonth = "2026-02",
                        Jitsudo = 7,
                        ZangyoExceptHoliday = 1,
                        Zangyo = 2,
                        AverageMax = 3,
                        YearTotal = 90,
                        OverLimitCount = 0,
                        MaxConsecutiveWorkingDays = "8"
                    }
                },

                HolidayList = new List<HolidayRowViewModel>
                {
                    new HolidayRowViewModel
                    {
                        PaidYearTotal = 10,
                        PaidRemain = 5,
                        PaidHalfRemain = 2,
                        SpecialUsed = 1,
                        TransferRemain = 3,
                        Transfer3Month = 1,
                        TransferExpired = 0
                    },
                    new HolidayRowViewModel
                    {
                        PaidYearTotal = 8,
                        PaidRemain = 4,
                        PaidHalfRemain = 1,
                        SpecialUsed = 0,
                        TransferRemain = 2,
                        Transfer3Month = 1,
                        TransferExpired = 0
                    }
                }
            };
            string sessionKey = "StatusView_TableViewModel";
            model.HttpContext.Session.SetString(sessionKey, JsonSerializer.Serialize(vm));

            // Act
            var result = await model.OnGetExportExcelAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(FileContentResult));
            var fileResult = (FileContentResult)result;
            Assert.AreEqual("勤務状況.xlsx", fileResult.FileDownloadName);
        }
        #endregion

        private static SyukkinKubun CreateSyukkinKubun(
            long? id = 1,
            string? name = null,
            string? nameRyaku = null,
            bool? isSyukkin = false,
            bool? isVacation = false,
            bool? isHoliday = false,
            bool? isNeedKubun1 = false,
            bool? isNeedKubun2 = false)
        {
            return new SyukkinKubun
            {
                Id = id ?? 1,
                Name = name?.Trim() ?? "出勤",
                NameRyaku = nameRyaku?.Trim() ?? "出勤",
                IsSyukkin = isSyukkin ?? false,
                IsVacation = isVacation ?? false,
                IsHoliday = isHoliday ?? false,
                IsNeedKubun1 = isNeedKubun1 ?? false,
                IsNeedKubun2 = isNeedKubun2 ?? false
            };
        }

        private static Nippou CreateNippou(
            long? id = 1,
            long? syainId = 1,
            DateOnly? nippouYmd = null,
            short? youbi = 0,
            TimeOnly? syukkinHm1 = null,
            TimeOnly? taisyutsuHm1 = null,
            TimeOnly? syukkinHm2 = null,
            TimeOnly? taisyutsuHm2 = null,
            TimeOnly? syukkinHm3 = null,
            TimeOnly? taisyutsuHm3 = null,
            decimal? hJitsudou = 0,
            decimal? hZangyo = 0,
            decimal? hWarimashi = 0,
            decimal? hShinyaZangyo = 0,
            decimal? dJitsudou = 0,
            decimal? dZangyo = 0,
            decimal? dWarimashi = 0,
            decimal? dShinyaZangyo = 0,
            decimal? nJitsudou = 0,
            decimal? nShinya = 0,
            decimal? totalZangyo = 0,
            NippousCompanyCode? kaisyaCode = NippousCompanyCode.協和,
            bool? isRendouZumi = false,
            DateOnly? rendouYmd = null,
            DailyReportStatusClassification? tourokuKubun = DailyReportStatusClassification.一時保存,
            DateOnly? kakuteiYmd = null,
            long? syukkinKubunId1 = 0,
            long? syukkinKubunId2 = 0)
        {
            return new Nippou
            {
                Id = id ?? 1,
                SyainId = syainId ?? 1,
                NippouYmd = nippouYmd ?? new DateOnly(2026, 1, 1),
                Youbi = youbi ?? 0,
                SyukkinHm1 = syukkinHm1,
                TaisyutsuHm1 = taisyutsuHm1,
                SyukkinHm2 = syukkinHm2,
                TaisyutsuHm2 = taisyutsuHm2,
                SyukkinHm3 = syukkinHm3,
                TaisyutsuHm3 = taisyutsuHm3,
                HJitsudou = hJitsudou,
                HZangyo = hZangyo,
                HWarimashi = hWarimashi,
                HShinyaZangyo = hShinyaZangyo,
                DJitsudou = dJitsudou,
                DZangyo = dZangyo,
                DWarimashi = dWarimashi,
                DShinyaZangyo = dShinyaZangyo,
                NJitsudou = nJitsudou,
                NShinya = nShinya,
                TotalZangyo = totalZangyo,
                KaisyaCode = kaisyaCode ?? NippousCompanyCode.協和,
                IsRendouZumi = isRendouZumi ?? false,
                RendouYmd = rendouYmd,
                TourokuKubun = tourokuKubun ?? DailyReportStatusClassification.一時保存,
                KakuteiYmd = kakuteiYmd,
                SyukkinKubunId1 = syukkinKubunId1 ?? 0,
                SyukkinKubunId2 = syukkinKubunId2 ?? 0
            };
        }

        private static UkagaiHeader CreateUkagaiHeader(
            long? id = 1,
            long? syainId = 1,
            DateOnly? shinseiYmd = null,
            long? shoninSyainId = 1,
            DateOnly? shoninYmd = null,
            long? lastShoninSyainId = 1,
            ApprovalStatus? status = ApprovalStatus.承認,
            DateOnly? lastShoninYmd = null,
            DateOnly? workYmd = null,
            TimeOnly? kaishiJikoku = null,
            TimeOnly? syuryoJikoku = null,
            string? biko = null,
            bool? invalid = false)
        {
            return new UkagaiHeader
            {
                Id = id ?? 1,
                SyainId = syainId ?? 1,
                ShinseiYmd = shinseiYmd ?? new DateOnly(2026, 1, 1),
                ShoninSyainId = shoninSyainId,
                ShoninYmd = shoninYmd,
                LastShoninSyainId = lastShoninSyainId,
                Status = status ?? ApprovalStatus.承認待,
                LastShoninYmd = lastShoninYmd,
                WorkYmd = workYmd ?? new DateOnly(2026, 1, 1),
                KaishiJikoku = kaishiJikoku,
                SyuryoJikoku = syuryoJikoku,
                Biko = biko,
                Invalid = invalid ?? false,
            };
        }

        private static UkagaiShinsei CreateUkagaiShinsei(
            long? id = 1,
            long? ukagaiHeaderId = 1,
            InquiryType? ukagaiSyubetsu = InquiryType.テレワーク)
        {
            return new UkagaiShinsei()
            {
                Id = id ?? 1,
                UkagaiHeaderId = ukagaiHeaderId ?? 1,
                UkagaiSyubetsu = ukagaiSyubetsu ?? InquiryType.テレワーク,
            };
        }

        private static Busyo CreateBusyo(
            long? id = 1,
            string? code = null,
            string? name = null,
            string? kanaName = null,
            string? oyaCode = null,
            DateOnly? startYmd = null,
            DateOnly? endYmd = null,
            short? jyunjyo = 1,
            string? kasyoCode = null,
            string? kaikeiCode = null,
            string? keiriCode = null,
            bool? isActive = true,
            string? ryakusyou = null,
            long? busyoBaseId = 1,
            long? oyaId = 0,
            long? shoninBusyoId = 0)
        {
            var result = new Busyo()
            {
                Code = code?.Trim() ?? $"B{id:D4}",
                Name = name?.Trim() ?? $"部署{id}",
                KanaName = kanaName?.Trim() ?? $"ブショ{id}",
                OyaCode = oyaCode?.Trim() ?? $"OB{id:D4}",
                StartYmd = startYmd ?? DateOnly.MinValue,
                EndYmd = endYmd ?? DateOnly.MaxValue,
                Jyunjyo = jyunjyo ?? 1,
                KasyoCode = kasyoCode?.Trim() ?? $"KAS{id:D4}",
                KaikeiCode = kaikeiCode?.Trim() ?? $"KK{id:D4}",
                KeiriCode = keiriCode?.Trim() ?? $"KR{id:D4}",
                IsActive = isActive ?? true,
                Ryakusyou = ryakusyou?.Trim() ?? $"R{id}",
                BusyoBaseId = busyoBaseId ?? 1,
                OyaId = oyaId ?? 0,
                ShoninBusyoId = shoninBusyoId ?? 0
            };

            if (id.HasValue)
            {
                result.Id = id.Value;
            }

            return result;
        }

        private static KintaiZokusei CreateKintaiZokusei(
            long? id = 1,
            string? name = null,
            decimal? seigenTime = 0.00m,
            bool? isMinashi = false,
            decimal? maxLimitTime = null,
            bool? isOvertimeLimit3m = false,
            EmployeeWorkType? code = EmployeeWorkType.月45時間)
        {
            var result = new KintaiZokusei
            {
                Name = name?.Trim() ?? "標準",
                SeigenTime = seigenTime ?? 45.00m,
                IsMinashi = isMinashi ?? false,
                MaxLimitTime = maxLimitTime ?? 0m,
                IsOvertimeLimit3m = isOvertimeLimit3m ?? false,
                Code = code ?? EmployeeWorkType.月45時間
            };

            if (id.HasValue)
            {
                result.Id = id.Value;
            }

            return result;
        }

        private static SyainBasis CreateSyainBasis(
            long? id = 1,
            string? name = null,
            string? code = null)
        {
            var result = new SyainBasis
            {
                Name = name?.Trim() ?? $"社員{id}",
                Code = code?.Trim() ?? $"S{id:D4}"
            };

            if (id.HasValue)
            {
                result.Id = id.Value;
            }

            return result;
        }

        private static Syain CreateSyain(
            long? id = 1,
            string? code = null,
            string? name = null,
            string? kanaName = null,
            char? seibetsu = null,
            string? busyoCode = null,
            int? syokusyuCode = null,
            int? syokusyuBunruiCode = null,
            DateOnly? nyushaYmd = null,
            DateOnly? startYmd = null,
            DateOnly? endYmd = null,
            short? kyusyoku = 0,
            BusinessTripRole? syucyoSyokui = BusinessTripRole._2_6級,
            string? kingsSyozoku = null,
            short? kaisyaCode = 0,
            bool? isGenkaRendou = false,
            string? eMail = null,
            string? keitaiMail = null,
            EmployeeAuthority? kengen = EmployeeAuthority.None,
            short? jyunjyo = 0,
            bool? retired = false,
            long? gyoumuTypeId = 1,
            string? phoneNumber = null,
            long? syainBaseId = 1,
            long? busyoId = 1,
            long? kintaiZokuseiId = 1,
            long? userRoleId = 1)
        {
            var result = new Syain
            {
                Code = code?.Trim() ?? $"S{id:D4}",
                Name = name?.Trim() ?? $"社員{id}",
                KanaName = kanaName?.Trim() ?? $"シャイン{id}",
                Seibetsu = seibetsu ?? '1',
                BusyoCode = busyoCode?.Trim() ?? $"B{id:D4}",
                SyokusyuCode = syokusyuCode ?? 0,
                SyokusyuBunruiCode = syokusyuBunruiCode ?? 0,
                NyuusyaYmd = nyushaYmd ?? new DateOnly(2020, 1, 1),
                StartYmd = startYmd ?? DateOnly.MinValue,
                EndYmd = endYmd ?? DateOnly.MaxValue,
                Kyusyoku = kyusyoku ?? 0,
                SyucyoSyokui = syucyoSyokui ?? BusinessTripRole._2_6級,
                KingsSyozoku = kingsSyozoku?.Trim() ?? $"K{id:D4}",
                KaisyaCode = kaisyaCode ?? 0,
                IsGenkaRendou = isGenkaRendou ?? false,
                EMail = eMail?.Trim() ?? $"syain{id}@example.com",
                KeitaiMail = keitaiMail?.Trim() ?? $"keitai{id}@example.com",
                Kengen = kengen ?? EmployeeAuthority.None,
                Jyunjyo = jyunjyo ?? 0,
                Retired = retired ?? false,
                GyoumuTypeId = gyoumuTypeId,
                PhoneNumber = phoneNumber,
                SyainBaseId = syainBaseId ?? 1,
                BusyoId = busyoId ?? 1,
                KintaiZokuseiId = kintaiZokuseiId ?? 1,
                UserRoleId = userRoleId ?? 1,
            };

            if (id.HasValue)
            {
                result.Id = id.Value;
            }

            return result;
        }
    }
}