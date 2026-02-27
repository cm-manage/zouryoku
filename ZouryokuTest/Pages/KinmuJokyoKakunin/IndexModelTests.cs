using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Model.Enums;
using Model.Model;
using System.Text.Json;
using Zouryoku;
using Zouryoku.Pages.KinmuJokyoKakunin;
using ZouryokuTest.Builder;
using ZouryokuTest.Pages.Builder;
using static Zouryoku.Pages.KinmuJokyoKakunin.WarnLevel;
using static Model.Enums.ResponseStatus;
using static Model.Enums.LeaveBalanceFetchStatus;

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
        /// 前提: From が To より後の年月（無効な日付範囲）が指定されている
        /// 操作: 検索処理（OnGetSearchAsync）を実行する
        /// 結果: エラーステータスとエラーメッセージが返却される
        /// </summary>
        [TestMethod]
        public async Task OnGetSearchAsync_無効な日付範囲の戻りエラー()
        {
            // Arrange
            IndexModel model = CreateModel();
            StatusSearchViewModel search = new StatusSearchViewModel
            {
                From = "2026-03",
                To = "2026-02",
                BusyoMode = "all",
                Busyo = "[]",
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
            Busyo busyo = new BusyoBuilder()
                .WithId(1)
                .Build();
            db.Busyos.Add(busyo);

            KintaiZokusei kintaiZokusei = new KintaiZokuseiBuilder()
                .WithId(1)
                .Build();
            db.KintaiZokuseis.Add(kintaiZokusei);

            SyainBasis syainBasis = new SyainBasisBuilder()
                .WithId(1)
                .Build();
            db.SyainBases.Add(syainBasis);

            Syain syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBasis.Id)
                .Build();
            db.Syains.Add(syain);

            SyainBasis syainBasis2 = new SyainBasisBuilder()
                .WithId(2)
                .Build();
            db.SyainBases.Add(syainBasis2);

            Syain syain2 = new SyainBuilder()
                .WithId(2)
                .WithSyainBaseId(syainBasis2.Id)
                .Build();
            db.Syains.Add(syain2);

            SyainBasis syainBasis3 = new SyainBasisBuilder()
                .WithId(3)
                .Build();
            db.SyainBases.Add(syainBasis3);

            Syain syain3 = new SyainBuilder()
                .WithId(3)
                .WithSyainBaseId(syainBasis3.Id)
                .Build();
            db.Syains.Add(syain3);

            var syukkinKubun1 = new SyukkinKubunBuilder()
                .WithId(2)
                .WithCode("04")
                .WithName("年次有給休暇_1日")
                .WithNameRyaku("年次有給休暇_1日")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun1);

            var syukkinKubun2 = new SyukkinKubunBuilder()
                .WithId(0)
                .WithCode("00")
                .WithName("未設定")
                .WithNameRyaku("-")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun2);

            var syukkinKubun3 = new SyukkinKubunBuilder()
                .WithId(3)
                .WithCode("02")
                .WithName("通常勤務")
                .WithNameRyaku("通常勤務")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun3);

            var syukkinKubun4 = new SyukkinKubunBuilder()
                .WithId(1)
                .WithCode("01")
                .WithName("休日")
                .WithNameRyaku("休日")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun3);

            var syukkinKubun5 = new SyukkinKubunBuilder()
                .WithId(5)
                .WithCode("05")
                .WithName("半日有給")
                .WithNameRyaku("半日有給")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun5);

            Nippou nippou = new NippouBuilder()
                .WithId(1)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/2/5"))
                .WithSyukkinKubunId1(syukkinKubun1.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippou);

            Nippou nippou2 = new NippouBuilder()
                .WithId(2)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/3/5"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .WithHZangyo(670)
                .Build();
            db.Nippous.Add(nippou2);

            UkagaiHeader ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(syain.Id)
                .WithWorkYmd(DateOnly.Parse("2024/2/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);

            UkagaiShinsei ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(ukagaiHeader.Id)
                .WithUkagaiSyubetsu(InquiryType.時間外労働時間制限拡張)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            Nippou nippou3 = new NippouBuilder()
                .WithId(3)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2025/3/5"))
                .WithSyukkinKubunId1(syukkinKubun5.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippou3);

            Nippou nippou4 = new NippouBuilder()
                .WithId(4)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2025/10/10"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippou3);

            Nippou nippou5 = new NippouBuilder()
                .WithId(100)
                .WithSyainId(syain3.Id)
                .WithNippouYmd(DateOnly.Parse("2025/10/12"))
                .WithSyukkinKubunId1(syukkinKubun4.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippou3);

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

            Nippou nippou1Syain2 = new NippouBuilder()
                .WithId(40)
                .WithSyainId(syain2.Id)
                .WithNippouYmd(DateOnly.Parse("2025/10/5"))
                .WithSyukkinKubunId1(syukkinKubun1.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippou1Syain2);

            Nippou nippou2Syain2 = new NippouBuilder()
                .WithId(41)
                .WithSyainId(syain2.Id)
                .WithNippouYmd(DateOnly.Parse("2025/11/6"))
                .WithSyukkinKubunId1(syukkinKubun5.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippou2Syain2);

            Nippou nippou3Syain2 = new NippouBuilder()
                .WithId(42)
                .WithSyainId(syain2.Id)
                .WithNippouYmd(DateOnly.Parse("2024/3/3"))
                .WithSyukkinKubunId1(syukkinKubun1.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippou3Syain2);


            Nippou nippou4Syain2 = new NippouBuilder()
                .WithId(43)
                .WithSyainId(syain2.Id)
                .WithNippouYmd(DateOnly.Parse("2024/2/5"))
                .WithSyukkinKubunId1(syukkinKubun1.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippou4Syain2);

            Nippou nippou5Syain2 = new NippouBuilder()
                .WithId(44)
                .WithSyainId(syain2.Id)
                .WithNippouYmd(DateOnly.Parse("2024/2/6"))
                .WithSyukkinKubunId1(syukkinKubun5.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippou5Syain2);

            Nippou nippouConsecutive1 = new NippouBuilder()
                .WithId(6)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/10/30"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutive1);

            Nippou nippouConsecutive2 = new NippouBuilder()
                .WithId(7)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/10/31"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutive2);

            Nippou nippouConsecutive3 = new NippouBuilder()
                .WithId(8)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/11/1"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutive3);

            Nippou nippouConsecutive4 = new NippouBuilder()
                .WithId(9)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/11/2"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutive4);

            Nippou nippouConsecutive5 = new NippouBuilder()
                .WithId(10)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/11/3"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutive5);

            Nippou nippouConsecutive6 = new NippouBuilder()
                .WithId(11)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/11/4"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutive6);

            Nippou nippouConsecutive7 = new NippouBuilder()
                .WithId(12)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/11/5"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutive7);

            Nippou nippouConsecutive8 = new NippouBuilder()
                .WithId(13)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/11/6"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutive8);

            Nippou nippouConsecutiveB1 = new NippouBuilder()
                .WithId(21)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/12/1"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutiveB1);

            Nippou nippouConsecutiveB2 = new NippouBuilder()
                .WithId(22)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/12/2"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutiveB2);

            Nippou nippouConsecutiveB3 = new NippouBuilder()
                .WithId(23)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/12/3"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutiveB3);

            Nippou nippouConsecutiveB4 = new NippouBuilder()
                .WithId(24)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/12/4"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutiveB4);

            Nippou nippouConsecutiveB5 = new NippouBuilder()
                .WithId(25)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/12/5"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutiveB5);

            Nippou nippouConsecutiveB6 = new NippouBuilder()
                .WithId(26)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/12/6"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
            db.Nippous.Add(nippouConsecutiveB6);

            Nippou nippouConsecutiveB7 = new NippouBuilder()
                .WithId(27)
                .WithSyainId(syain.Id)
                .WithNippouYmd(DateOnly.Parse("2024/12/7"))
                .WithSyukkinKubunId1(syukkinKubun3.Id)
                .WithSyukkinKubunId2(syukkinKubun2.Id)
                .Build();
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
    }
}