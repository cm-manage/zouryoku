using CommonLibrary.Extensions;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Model.Enums;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.Attendance.AttendanceList;
using Zouryoku.Pages.Shared;
using Zouryoku.Pages.Shared.Components;
using ZouryokuCommonLibrary.Utils;
using ZouryokuTest.Builder;
using ZouryokuTest.Extensions;
using ZouryokuTest.Pages.Builder;
using static Model.Enums.DailyReportStatusClassification;
using static Model.Enums.EmployeeAuthority;
using static Model.Enums.HolidayFlag;
using static Model.Enums.InquiryType;
using static Model.Enums.PcOperationType;
using static Zouryoku.Pages.Attendance.AttendanceList.IndexModel;
using static Zouryoku.Pages.Attendance.AttendanceList.SortOrderEnum;
using static Zouryoku.Pages.Attendance.AttendanceList.SortSelectedEnum;
using static Zouryoku.Pages.Shared.Components.DatepickerRangeModel.Option;
using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.Attendance.AttendanceList
{
    /// <summary>
    /// IndexModel 出退勤一覧 のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {
        // 検索結果最大件数
        private const int SearchResultMaxCount = 4000;

        /// <summary>
        /// 出退勤一覧用のindexModelを生成し、テスト実行に必要なコンテキスト情報を設定します。
        /// </summary>
        /// <param name="loginUser">セッションに設定するログインユーザー（社員）情報</param>
        /// <returns>ページコンテキスト</returns>
        private IndexModel CreateModel(Syain loginUser)
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };
            // セッション保存
            model.HttpContext.Session.Set(new LoginInfo { User = loginUser });
            // クッキークリア
            model.HttpContext.RemoveTestCookie<AttendanceListCookie>();
            // 部署セット
            model.Search.BusyoId = loginUser.BusyoId;
            return model;
        }

        /// <summary>
        /// 初期表示、部署変更 共通のテストデータ登録
        /// </summary>
        /// <param name="loginUserId">ログイン用社員ID</param>
        /// <returns>ログイン用社員</returns>
        private Syain InitializeTestData_OnGetAsync_OnPostBusyoAsync(long loginUserId)
        {
            // Arrange
            // 部署BASE & 部署 データ登録
            // 部署1
            var busyoBase1 = new BusyoBasisBuilder()
                .WithId(1)
                .WithName("部署1")
                .WithBumoncyoId(9999)
                .Build();
            db.BusyoBases.Add(busyoBase1);

            var busyo1 = new BusyoBuilder()
                .WithId(1)
                .WithCode("001")
                .WithName("部署1")
                .WithJyunjyo(5)
                .WithIsActive(true)
                .WithBusyoBaseId(busyoBase1.Id)
                .Build();
            db.Busyos.Add(busyo1);

            // 部署1-1
            var busyoBase1_1 = new BusyoBasisBuilder()
                .WithId(2)
                .WithName("部署1-1")
                .Build();
            db.BusyoBases.Add(busyoBase1_1);

            var busyo1_1 = new BusyoBuilder()
                .WithId(2)
                .WithCode("002")
                .WithName("部署1-1")
                .WithJyunjyo(4)
                .WithIsActive(true)
                .WithOyaCode("001")
                .WithOyaId(1)
                .WithBusyoBaseId(busyoBase1_1.Id)
                .Build();
            db.Busyos.Add(busyo1_1);

            // 部署1-2
            var busyoBase1_2 = new BusyoBasisBuilder()
                .WithId(3)
                .WithName("部署1-2")
                .Build();
            db.BusyoBases.Add(busyoBase1_2);

            var busyo1_2 = new BusyoBuilder()
                .WithId(3)
                .WithCode("003")
                .WithName("部署1-2")
                .WithJyunjyo(4)
                .WithIsActive(true)
                .WithOyaCode("001")
                .WithOyaId(1)
                .WithBusyoBaseId(busyoBase1_2.Id)
                .Build();
            db.Busyos.Add(busyo1_2);

            // 社員BASE & 社員 データ登録
            // 社員A
            var syainBaseA = new SyainBasisBuilder()
                .WithId(11)
                .Build();
            db.SyainBases.Add(syainBaseA);

            var syainA = new SyainBuilder()
                .WithId(1)
                .WithCode("1001")
                .WithName("社員A")
                .WithSyainBaseId(syainBaseA.Id)
                .WithBusyoId(busyo1.Id)
                .WithJyunjyo(6)
                .WithStartYmd(DateOnly.Parse("2025/4/1"))
                .WithEndYmd(DateOnly.Parse("2026/4/1"))
                .Build();
            db.Syains.Add(syainA);

            // 社員B
            var syainBaseB = new SyainBasisBuilder()
                .WithId(12)
                .Build();
            db.SyainBases.Add(syainBaseB);

            var syainB = new SyainBuilder()
                .WithId(2)
                .WithCode("1002")
                .WithName("社員B")
                .WithSyainBaseId(syainBaseB.Id)
                .WithBusyoId(busyo1_1.Id)
                .WithJyunjyo(5)
                .WithStartYmd(DateOnly.Parse("2025/4/1"))
                .WithEndYmd(DateOnly.Parse("2026/4/1"))
                .Build();
            db.Syains.Add(syainB);

            // 社員C
            var syainBaseC = new SyainBasisBuilder()
                .WithId(13)
                .Build();
            db.SyainBases.Add(syainBaseC);

            var syainC = new SyainBuilder()
                .WithId(3)
                .WithCode("1003")
                .WithName("社員C")
                .WithSyainBaseId(syainBaseC.Id)
                .WithBusyoId(busyo1_2.Id)
                .WithJyunjyo(4)
                .WithStartYmd(DateOnly.Parse("2025/4/1"))
                .WithEndYmd(DateOnly.Parse("2026/4/1"))
                .Build();
            db.Syains.Add(syainC);

            // 社員D
            var syainBaseD = new SyainBasisBuilder()
                .WithId(14)
                .Build();
            db.SyainBases.Add(syainBaseD);

            var syainD = new SyainBuilder()
                .WithId(4)
                .WithCode("1004")
                .WithName("社員D")
                .WithSyainBaseId(syainBaseD.Id)
                .WithBusyoId(busyo1.Id)
                .WithJyunjyo(1)
                .WithStartYmd(DateOnly.Parse("2025/4/1"))
                .WithEndYmd(DateOnly.Parse("2026/4/1"))
                .Build();
            db.Syains.Add(syainD);

            if (syainA.Id == loginUserId)
            {
                return syainA;
            }
            else if (syainB.Id == loginUserId)
            {
                return syainB;
            }
            else if (syainC.Id == loginUserId)
            {
                return syainC;
            }
            else
            {
                return syainD;
            }
        }

        /// <summary>
        /// 勤務表検索 共通のテストデータ登録
        /// </summary>
        /// <param name="kengen">付与権限（初期値権限なし）</param>
        /// <returns>ログイン用社員</returns>
        private Syain InitializeTestData_OnPostSearchAsync(EmployeeAuthority kengen = EmployeeAuthority.None)
        {
            // Arrange
            // 部署BASE & 部署 データ登録
            // 部署1
            var busyoBase1 = new BusyoBasisBuilder()
                .WithId(1)
                .WithName("部署1")
                .WithBumoncyoId(9999)
                .Build();
            db.BusyoBases.Add(busyoBase1);

            var busyo1 = new BusyoBuilder()
                .WithId(1)
                .WithCode("001")
                .WithName("部署1")
                .WithJyunjyo(5)
                .WithIsActive(true)
                .WithBusyoBaseId(busyoBase1.Id)
                .Build();
            db.Busyos.Add(busyo1);

            // 社員BASE & 社員 データ登録
            // 社員A
            var syainBaseA = new SyainBasisBuilder()
                .WithId(11)
                .Build();
            db.SyainBases.Add(syainBaseA);

            var syainA = new SyainBuilder()
                .WithId(1)
                .WithCode("1001")
                .WithName("社員A")
                .WithSyainBaseId(syainBaseA.Id)
                .WithBusyoId(busyo1.Id)
                .WithJyunjyo(6)
                .WithStartYmd(DateOnly.Parse("2010/4/1"))
                .WithEndYmd(DateOnly.Parse("2050/4/1"))
                .WithKengen(kengen)
                .Build();
            db.Syains.Add(syainA);

            // 出勤区分
            var syukkinKubun1 = new SyukkinKubunBuilder()
                .WithId(1)
                .WithCode("01")
                .WithName("休日")
                .WithNameRyaku("休日")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun1);
            var syukkinKubun2 = new SyukkinKubunBuilder()
                .WithId(2)
                .WithCode("02")
                .WithName("通常勤務")
                .WithNameRyaku("通常")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun2);
            var syukkinKubun3 = new SyukkinKubunBuilder()
                .WithId(3)
                .WithCode("03")
                .WithName("休日出勤")
                .WithNameRyaku("休出")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun3);
            var syukkinKubun4 = new SyukkinKubunBuilder()
                .WithId(4)
                .WithCode("13")
                .WithName("生理休暇")
                .WithNameRyaku("生理休暇")
                .Build();
            db.SyukkinKubuns.Add(syukkinKubun4);

            return syainA;
        }

        // 勤務データ　社員・日付データ確認
        private void AssertKinmuDataSyain(long? expectedSyainId, string expectedSyainCode, string expectedSyainName,
                                         string expectedHiduke, string expectedYoubisyoku, KinmuData actualKinmuData)
        {
            Assert.AreEqual(expectedSyainId, actualKinmuData.SyainData.Id, "SyainId が一致しません。");
            Assert.AreEqual(expectedSyainCode, actualKinmuData.SyainData.Code, "SyainCode が一致しません。");
            Assert.AreEqual(expectedSyainName, actualKinmuData.SyainData.Name, "SyainName が一致しません。");
            Assert.AreEqual(expectedHiduke, actualKinmuData.GetHiduke, "GetHiduke が一致しません。");
            var actualYoubisyoku = actualKinmuData.GetYoubisyoku();
            Assert.AreEqual(expectedYoubisyoku, actualKinmuData.GetYoubisyoku(), "GetYoubisyoku が一致しません。");
        }

        // 勤務データ　出退勤データ確認
        private void AssertKinmuDataSyuttaikin(SyuttaiKirokuData expectedSyuttaikiroku, SyuttaiKirokuData actualSyuttaikiroku)
        {
            Assert.AreEqual(expectedSyuttaikiroku.SyukkinJikan1, actualSyuttaikiroku.SyukkinJikan1, "SyukkinJikan1 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.SyukkinJikan2, actualSyuttaikiroku.SyukkinJikan2, "SyukkinJikan2 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.SyukkinJikan3, actualSyuttaikiroku.SyukkinJikan3, "SyukkinJikan3 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.TaikinJikan1, actualSyuttaikiroku.TaikinJikan1, "TaikinJikan1 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.TaikinJikan2, actualSyuttaikiroku.TaikinJikan2, "TaikinJikan2 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.TaikinJikan3, actualSyuttaikiroku.TaikinJikan3, "TaikinJikan3 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.SyukkinPos1, actualSyuttaikiroku.SyukkinPos1, "SyukkinPos1 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.SyukkinPos2, actualSyuttaikiroku.SyukkinPos2, "SyukkinPos2 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.SyukkinPos3, actualSyuttaikiroku.SyukkinPos3, "SyukkinPos3 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.TaikinPos1, actualSyuttaikiroku.TaikinPos1, "TaikinPos1 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.TaikinPos2, actualSyuttaikiroku.TaikinPos2, "TaikinPos2 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.TaikinPos3, actualSyuttaikiroku.TaikinPos3, "TaikinPos3 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.IsHimatagiSyukkin1, actualSyuttaikiroku.IsHimatagiSyukkin1, "IsHimatagiSyukkin1 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.IsHimatagiTaikin1, actualSyuttaikiroku.IsHimatagiTaikin1, "IsHimatagiTaikin1 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.IsHimatagiTaikin2, actualSyuttaikiroku.IsHimatagiTaikin2, "IsHimatagiTaikin2 が一致しません。");
            Assert.AreEqual(expectedSyuttaikiroku.IsHimatagiTaikin3, actualSyuttaikiroku.IsHimatagiTaikin3, "IsHimatagiTaikin3 が一致しません。");
        }

        // 勤務データ　日報データ確認
        private void AssertKinmuDataNippou(NippouData expectedNippouClass, NippouData actualNippouClass)
        {
            Assert.AreEqual(expectedNippouClass.Syukkin1, actualNippouClass.Syukkin1, "Syukkin1 が一致しません。");
            Assert.AreEqual(expectedNippouClass.Syukkin2, actualNippouClass.Syukkin2, "Syukkin2 が一致しません。");
            Assert.AreEqual(expectedNippouClass.Syukkin3, actualNippouClass.Syukkin3, "Syukkin3 が一致しません。");
            Assert.AreEqual(expectedNippouClass.Taisyutsu1, actualNippouClass.Taisyutsu1, "Taisyutsu1 が一致しません。");
            Assert.AreEqual(expectedNippouClass.Taisyutsu2, actualNippouClass.Taisyutsu2, "Taisyutsu2 が一致しません。");
            Assert.AreEqual(expectedNippouClass.Taisyutsu3, actualNippouClass.Taisyutsu3, "Taisyutsu3 が一致しません。");
            Assert.HasCount(expectedNippouClass.SyukkinKubunList.Count, actualNippouClass.SyukkinKubunList, "SyukkinKubunList の件数が一致しません。");

            for (int i = 0; i < expectedNippouClass.SyukkinKubunList.Count; i++)
            {
                Assert.AreEqual(expectedNippouClass.SyukkinKubunList[i], actualNippouClass.SyukkinKubunList[i], $"SyukkinKubunList の {i} 件目が一致しません。");
            }
        }

        // 勤務データ　PCログデータ確認
        private void AssertKinmuDataPcLog(List<(string PcName, string StartTime, string EndTime)> expectedPcLog, List<PcLogData> actualPcLog)
        {
            Assert.HasCount(expectedPcLog.Count, actualPcLog, "pcLog の件数が一致しません。");

            for (int i = 0; i < expectedPcLog.Count; i++)
            {
                Assert.AreEqual(expectedPcLog[i].PcName, actualPcLog[i].PcName, $"pcLog の PcName {i} 件目が一致しません。");
                Assert.AreEqual(expectedPcLog[i].StartTime, actualPcLog[i].StartTime, $"pcLog の StartTime {i} 件目が一致しません。");
                Assert.AreEqual(expectedPcLog[i].EndTime, actualPcLog[i].EndTime, $"pcLog の EndTime {i} 件目が一致しません。");
            }
        }

        // 勤務データ　伺い申請データ確認
        private void AssertKinmuDataUkagaiShinsei(List<string> expectedUkagaiShinsei, List<string> actualUkagaiShinsei)
        {
            Assert.HasCount(expectedUkagaiShinsei.Count, actualUkagaiShinsei, "ukagaiShinsei の件数が一致しません。");

            for (int i = 0; i < expectedUkagaiShinsei.Count; i++)
            {
                Assert.AreEqual(expectedUkagaiShinsei[i], actualUkagaiShinsei[i], $"ukagaiShinsei の {i} 件目が一致しません。");
            }
        }

        /// <summary>
        /// 初期表示
        ///     テストケース  初期-1：初期表示、影響部署の社員リストを返す
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_WhenSyokihyouji_ReturnsEikyoubumonSyain()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnGetAsync_OnPostBusyoAsync(1);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.SyainView, "SyainViewが設定されていません。");
            Assert.AreEqual(5, model.SyainView.Items.Count(), "SyainView.Itemsの件数が一致しません。");
            Assert.AreEqual("", model.SyainView.Items[0].Value, "1件目の Value が一致しません。");
            Assert.AreEqual("", model.SyainView.Items[0].Text, "1件目の Text が一致しません。");
            Assert.AreEqual("13", model.SyainView.Items[1].Value, "2件目の Value が一致しません。");
            Assert.AreEqual("社員C", model.SyainView.Items[1].Text, "2件目の Text が一致しません。");
            Assert.AreEqual("12", model.SyainView.Items[2].Value, "3件目の Value が一致しません。");
            Assert.AreEqual("社員B", model.SyainView.Items[2].Text, "3件目の Text が一致しません。");
            Assert.AreEqual("14", model.SyainView.Items[3].Value, "4件目の Value が一致しません。");
            Assert.AreEqual("社員D", model.SyainView.Items[3].Text, "4件目の Text が一致しません。");
            Assert.AreEqual("11", model.SyainView.Items[4].Value, "5件目の Value が一致しません。");
            Assert.AreEqual("社員A", model.SyainView.Items[4].Text, "5件目の Text が一致しません。");
            Assert.AreEqual("", model.SyainView.SelectedId, "SelectedId が一致しません。");
            Assert.IsFalse(model.SyainView.IsSelectDepartment, "IsSelectDepartment が一致しません。");
            Assert.AreEqual(1, model.Search.BusyoId, "BusyoId が一致しません。");
            Assert.AreEqual("部署1", model.Search.BusyoName, "BusyoName が一致しません。");
        }

        /// <summary>
        /// 初期表示
        ///     テストケース  初期-2：初期表示、影響部署以外の社員は返さない
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_WhenSyokihyouji_ReturnsEikyoubumongaiNotSyain()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnGetAsync_OnPostBusyoAsync(1);

            // 部署2
            var busyoBase2 = new BusyoBasisBuilder()
                .WithId(4)
                .WithName("部署2")
                .WithBumoncyoId(9999)
                .Build();
            db.BusyoBases.Add(busyoBase2);

            var busyo2 = new BusyoBuilder()
                .WithId(4)
                .WithCode("004")
                .WithName("部署2")
                .WithJyunjyo(2)
                .WithIsActive(true)
                .WithBusyoBaseId(busyoBase2.Id)
                .Build();
            db.Busyos.Add(busyo2);

            // 部署2-1
            var busyoBase2_1 = new BusyoBasisBuilder()
                .WithId(5)
                .WithName("部署2-1")
                .Build();
            db.BusyoBases.Add(busyoBase2_1);

            var busyo2_1 = new BusyoBuilder()
                .WithId(5)
                .WithCode("005")
                .WithName("部署2-1")
                .WithJyunjyo(1)
                .WithIsActive(true)
                .WithOyaCode("004")
                .WithOyaId(4)
                .WithBusyoBaseId(busyoBase2_1.Id)
                .Build();
            db.Busyos.Add(busyo2_1);

            // 社員E
            var syainBaseE = new SyainBasisBuilder()
                .WithId(15)
                .Build();
            db.SyainBases.Add(syainBaseE);

            var syainE = new SyainBuilder()
                .WithId(5)
                .WithCode("1005")
                .WithName("社員E")
                .WithSyainBaseId(syainBaseE.Id)
                .WithBusyoId(busyo2.Id)
                .WithJyunjyo(3)
                .WithStartYmd(DateOnly.Parse("2025/4/1"))
                .WithEndYmd(DateOnly.Parse("2026/4/1"))
                .Build();
            db.Syains.Add(syainE);

            // 社員F
            var syainBaseF = new SyainBasisBuilder()
                .WithId(16)
                .Build();
            db.SyainBases.Add(syainBaseF);

            var syainF = new SyainBuilder()
                .WithId(6)
                .WithCode("1006")
                .WithName("社員F")
                .WithSyainBaseId(syainBaseF.Id)
                .WithBusyoId(busyo2_1.Id)
                .WithJyunjyo(2)
                .WithStartYmd(DateOnly.Parse("2025/4/1"))
                .WithEndYmd(DateOnly.Parse("2026/4/1"))
                .Build();
            db.Syains.Add(syainF);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.DoesNotContain(syainBaseE.Id.ToString(), model.SyainView.Items.Select(x => x.Value), "影響部署外の社員Eが含まれています。");
            Assert.DoesNotContain(syainBaseF.Id.ToString(), model.SyainView.Items.Select(x => x.Value), "影響部署外の社員Fが含まれています。");
            Assert.DoesNotContain(syainE.Name, model.SyainView.Items.Select(x => x.Text), "影響部署外の社員Eが含まれています。");
            Assert.DoesNotContain(syainF.Name, model.SyainView.Items.Select(x => x.Text), "影響部署外の社員Fが含まれています。");
        }

        /// <summary>
        /// 初期表示
        ///     テストケース  初期-3：クッキーに前回履歴あり、社員と期間がクッキーから引き継がれた選択状態となる
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_WhenZenkairirekiari_ReturnsRirekiSentaku()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnGetAsync_OnPostBusyoAsync(2);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            // クッキー保存（期間：今年度、社員BASE ID：11）
            model.HttpContext.AddTestCookie(new AttendanceListCookie
            {
                SelectedKikanOption = ThisFiscalYear.ToString(),
                SelectedSyainBaseId = 11,
            });

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual("11", model.SyainView.SelectedId, "SelectedId が一致しません。");
            Assert.AreEqual(ThisFiscalYear, model.Search.Kikan.SelectedOption, "SelectedOption が一致しません。");
        }

        /// <summary>
        /// 初期表示
        ///     テストケース  初期-4：クッキーに前回履歴あるが選択不可、初期状態となる
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_WhenZenkairirekisiyoufuka_ReturnsSyokiSentaku()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnGetAsync_OnPostBusyoAsync(2);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            // クッキー保存（期間：今年度、社員BASE ID：99）
            model.HttpContext.AddTestCookie(new AttendanceListCookie 
            {
                SelectedKikanOption = "ABCDEFG",
                SelectedSyainBaseId = 99,
            });

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual("", model.SyainView.SelectedId, "SelectedId が一致しません。");
            Assert.AreEqual(ThisMonth, model.Search.Kikan.SelectedOption, "SelectedOption が一致しません。");
        }

        /// <summary>
        /// 初期表示
        ///     テストケース  初期-5：クッキーに前回履歴ない、初期状態となる
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_WhenZenkairirekisiyounashi_ReturnsSyokiSentaku()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnGetAsync_OnPostBusyoAsync(2);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual("", model.SyainView.SelectedId, "SelectedId が一致しません。");
            Assert.AreEqual(ThisMonth, model.Search.Kikan.SelectedOption, "SelectedOption が一致しません。");
        }

        /// <summary>
        /// 初期表示
        ///     テストケース  初期-6：ログインユーザー部署選択権限なし、権限なしとなる
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_WhenBusyosentakukengennashi_ReturnsKengennashi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnGetAsync_OnPostBusyoAsync(2);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsFalse(model.SyainView.IsSelectDepartment, "IsSelectDepartment が一致しません。");
        }

        /// <summary>
        /// 初期表示
        ///     テストケース  初期-7：ログインユーザー部署選択権限あり、権限ありとなる
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_WhenBusyosentakukengenari_ReturnsKengenari()
        {
            // Arrange
            InitializeTestData_OnGetAsync_OnPostBusyoAsync(1);

            await db.SaveChangesAsync();

            // 部署選択権限割り当て
            var syain1 = await db.Syains.FirstAsync(s => s.Id == 1);
            syain1.SetKengen(出退勤一覧画面の部署選択);
            await db.SaveChangesAsync();

            var model = CreateModel(syain1);

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsTrue(model.SyainView.IsSelectDepartment, "IsSelectDepartment が一致しません。");
        }

        /// <summary>
        /// 部署変更
        ///     テストケース  部署選択-1：部署選択、選択した部署の影響部署の社員リスト
        /// </summary>
        [TestMethod]
        public async Task OnPostBusyoAsync_WhenBusyosentaku_ReturnsEikyoubumonSyain()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnGetAsync_OnPostBusyoAsync(1);

            // 部署2
            var busyoBase2 = new BusyoBasisBuilder()
                .WithId(4)
                .WithName("部署2")
                .WithBumoncyoId(9999)
                .Build();
            db.BusyoBases.Add(busyoBase2);

            var busyo2 = new BusyoBuilder()
                .WithId(4)
                .WithCode("004")
                .WithName("部署2")
                .WithJyunjyo(2)
                .WithIsActive(true)
                .WithBusyoBaseId(busyoBase2.Id)
                .Build();
            db.Busyos.Add(busyo2);

            // 部署2-1
            var busyoBase2_1 = new BusyoBasisBuilder()
                .WithId(5)
                .WithName("部署2-1")
                .Build();
            db.BusyoBases.Add(busyoBase2_1);

            var busyo2_1 = new BusyoBuilder()
                .WithId(5)
                .WithCode("005")
                .WithName("部署2-1")
                .WithJyunjyo(1)
                .WithIsActive(true)
                .WithOyaCode("004")
                .WithOyaId(4)
                .WithBusyoBaseId(busyoBase2_1.Id)
                .Build();
            db.Busyos.Add(busyo2_1);

            // 社員E
            var syainBaseE = new SyainBasisBuilder()
                .WithId(15)
                .Build();
            db.SyainBases.Add(syainBaseE);

            var syainE = new SyainBuilder()
                .WithId(5)
                .WithCode("1005")
                .WithName("社員E")
                .WithSyainBaseId(syainBaseE.Id)
                .WithBusyoId(busyo2.Id)
                .WithJyunjyo(3)
                .WithStartYmd(DateOnly.Parse("2025/4/1"))
                .WithEndYmd(DateOnly.Parse("2026/4/1"))
                .Build();
            db.Syains.Add(syainE);

            // 社員F
            var syainBaseF = new SyainBasisBuilder()
                .WithId(16)
                .Build();
            db.SyainBases.Add(syainBaseF);

            var syainF = new SyainBuilder()
                .WithId(6)
                .WithCode("1006")
                .WithName("社員F")
                .WithSyainBaseId(syainBaseF.Id)
                .WithBusyoId(busyo2_1.Id)
                .WithJyunjyo(2)
                .WithStartYmd(DateOnly.Parse("2025/4/1"))
                .WithEndYmd(DateOnly.Parse("2026/4/1"))
                .Build();
            db.Syains.Add(syainF);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search.BusyoId = 5; // 部署ID:5 を選択

            // Act
            var data = await model.OnPostBusyoAsync();

            // Assert
            var jsondata = (JsonResult)data;
            Assert.IsNotNull(jsondata.Value, "戻り値がありません。");
            var resultdata = (ResponseJson)jsondata.Value;
            Assert.IsNotNull(resultdata.Data, "戻り値がありません。");
            var items = (List<SelectListItem>)resultdata.Data;

            Assert.AreEqual(3, items.Count(), "Itemsの件数が一致しません。");
            Assert.AreEqual("", items[0].Value, "1件目の Value が一致しません。");
            Assert.AreEqual("", items[0].Text, "1件目の Text が一致しません。");
            Assert.AreEqual("16", items[1].Value, "2件目の Value が一致しません。");
            Assert.AreEqual("社員F", items[1].Text, "2件目の Text が一致しません。");
            Assert.AreEqual("15", items[2].Value, "3件目の Value が一致しません。");
            Assert.AreEqual("社員E", items[2].Text, "3件目の Text が一致しません。");
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-1：データなし、社員・日付のみ返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenDataNashi_ReturnNamaeHidukeNomi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert
            Assert.AreEqual(1, kinmuList.Count(), "kinmuListの件数が一致しません。");
            Assert.AreEqual(1, model.AttendanceView.LoginUser.Id, "LoginUser.Id が一致しません。");

            // 社員・日付データ確認
            AssertKinmuDataSyain(1,"1001", "社員A", "01/05(月)", "app-line--weekday", kinmuList[0]);

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "",
                SyukkinJikan2 = "",
                SyukkinJikan3 = "",
                TaikinJikan1 = "",
                TaikinJikan2 = "",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));

            // 日報データ確認
            var nippouClass = new NippouData
            {
                Syukkin1 = "",
                Syukkin2 = "",
                Syukkin3 = "",
                Taisyutsu1 = "",
                Taisyutsu2 = "",
                Taisyutsu3 = "",
                SyukkinKubunList = new List<string>(),
            };
            AssertKinmuDataNippou(nippouClass, kinmuList[0].GetNippouData(loginUser));

            // PCログデータ確認
            AssertKinmuDataPcLog(new List<(string PcName, string StartTime, string EndTime)>(), kinmuList[0].GetPcLogDataList(loginUser));

            // 伺い申請データ確認
            AssertKinmuDataUkagaiShinsei(new List<string>(), kinmuList[0].GetUkagaiShinseiList());
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-2：土曜日、土曜日の曜日色を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenDoyoubi_ReturnDoyoubiYoubisyoku()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/10").ToDateOnly(),
                    To = DateTime.Parse("2026/1/10").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 社員・日付データ確認
            AssertKinmuDataSyain(1, "1001", "社員A", "01/10(土)", "app-line--saturday", kinmuList[0]);
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-3：日曜日、日曜日の曜日色を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenNichiyoubi_ReturnNichiyoubiYoubisyoku()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/11").ToDateOnly(),
                    To = DateTime.Parse("2026/1/11").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 社員・日付データ確認
            AssertKinmuDataSyain(1, "1001", "社員A", "01/11(日)", "app-line--sunday", kinmuList[0]);
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-4：平日に祝日設定、祝日の曜日色を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenHeijitsuSyukujitsu_ReturnSyukujitsuYoubisyoku()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 祝日設定登録
            var syukujitsu = new HikadoubiBuilder()
                .WithId(1)
                .WithYmd(DateOnly.Parse("2026/1/12"))
                .WithSyukusaijitsuFlag(祝祭日)
                .WithRefreshDay(RefreshDayFlag.それ以外)
                .Build();
            db.Hikadoubis.Add(syukujitsu);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/12").ToDateOnly(),
                    To = DateTime.Parse("2026/1/12").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 社員・日付データ確認
            AssertKinmuDataSyain(1, "1001", "社員A", "01/12(月)", "app-line--holiday", kinmuList[0]);
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-5：土曜日に祝日設定、祝日の曜日色を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenDoyoubiSyukujitsu_ReturnSyukujitsuYoubisyoku()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 祝日設定登録
            var syukujitsu = new HikadoubiBuilder()
                .WithId(1)
                .WithYmd(DateOnly.Parse("2026/1/17"))
                .WithSyukusaijitsuFlag(祝祭日)
                .WithRefreshDay(RefreshDayFlag.それ以外)
                .Build();
            db.Hikadoubis.Add(syukujitsu);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/17").ToDateOnly(),
                    To = DateTime.Parse("2026/1/17").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 社員・日付データ確認
            AssertKinmuDataSyain(1, "1001", "社員A", "01/17(土)", "app-line--holiday", kinmuList[0]);
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-6：日曜日に祝日設定、祝日の曜日色を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenNichiyoubiSyukujitsu_ReturnSyukujitsuYoubisyoku()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 祝日設定登録
            var syukujitsu = new HikadoubiBuilder()
                .WithId(1)
                .WithYmd(DateOnly.Parse("2026/1/18"))
                .WithSyukusaijitsuFlag(祝祭日)
                .WithRefreshDay(RefreshDayFlag.それ以外)
                .Build();
            db.Hikadoubis.Add(syukujitsu);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/18").ToDateOnly(),
                    To = DateTime.Parse("2026/1/18").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 社員・日付データ確認
            AssertKinmuDataSyain(1, "1001", "社員A", "01/18(日)", "app-line--holiday", kinmuList[0]);
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-7：打刻データ1件、出退勤時間１を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenDakoku1ken_ReturnSyuttaikin1Ken()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 09:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 18:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert
            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "09:00",
                SyukkinJikan2 = "",
                SyukkinJikan3 = "",
                TaikinJikan1 = "18:00",
                TaikinJikan2 = "",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-8：打刻データ2件、出退勤時間１と２を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenDakoku2ken_ReturnSyuttaikin2Ken()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 09:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);

            // 打刻2
            var workingHour2 = new WorkingHoursBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 13:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 18:00"))
                .Build();
            db.WorkingHours.Add(workingHour2);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert
            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "09:00",
                SyukkinJikan2 = "13:00",
                SyukkinJikan3 = "",
                TaikinJikan1 = "12:00",
                TaikinJikan2 = "18:00",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-9：打刻データ3件、出退勤時間１～３を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenDakoku3ken_ReturnSyuttaikin3Ken()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 09:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);

            // 打刻2
            var workingHour2 = new WorkingHoursBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 13:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 15:30"))
                .Build();
            db.WorkingHours.Add(workingHour2);

            // 打刻3
            var workingHour3 = new WorkingHoursBuilder()
                .WithId(3)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 16:30"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 18:00"))
                .Build();
            db.WorkingHours.Add(workingHour3);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert
            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "09:00",
                SyukkinJikan2 = "13:00",
                SyukkinJikan3 = "16:30",
                TaikinJikan1 = "12:00",
                TaikinJikan2 = "15:30",
                TaikinJikan3 = "18:00",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-10：打刻データ4件、出勤時間の昇順で出退勤時間１～３を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenDakoku4ken_ReturnSyuttaikinTop3Ken()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 09:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);

            // 打刻2
            var workingHour2 = new WorkingHoursBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 19:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 20:00"))
                .Build();
            db.WorkingHours.Add(workingHour2);

            // 打刻3
            var workingHour3 = new WorkingHoursBuilder()
                .WithId(3)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 16:30"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 18:00"))
                .Build();
            db.WorkingHours.Add(workingHour3);

            // 打刻4
            var workingHour4 = new WorkingHoursBuilder()
                .WithId(4)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 13:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 15:30"))
                .Build();
            db.WorkingHours.Add(workingHour4);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert
            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "09:00",
                SyukkinJikan2 = "13:00",
                SyukkinJikan3 = "16:30",
                TaikinJikan1 = "12:00",
                TaikinJikan2 = "15:30",
                TaikinJikan3 = "18:00",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-11：打刻位置情報あり位置確認権限なし、打刻位置を返さない
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenDakokuIchiKengenNashi_ReturnIchinashi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 09:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 12:00"))
                .WithSyukkinLatitude(35.6895M)
                .WithSyukkinLongitude(139.6917M)
                .WithTaikinLatitude(36.6895M)
                .WithTaikinLongitude(140.6917M)
                .Build();
            db.WorkingHours.Add(workingHour1);

            // 打刻2
            var workingHour2 = new WorkingHoursBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 13:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 15:30"))
                .WithSyukkinLatitude(35.6896M)
                .WithSyukkinLongitude(139.6918M)
                .WithTaikinLatitude(36.6896M)
                .WithTaikinLongitude(140.6918M)
                .Build();
            db.WorkingHours.Add(workingHour2);

            // 打刻3
            var workingHour3 = new WorkingHoursBuilder()
                .WithId(3)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 16:30"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 18:00"))
                .WithSyukkinLatitude(35.6897M)
                .WithSyukkinLongitude(139.6919M)
                .WithTaikinLatitude(36.6897M)
                .WithTaikinLongitude(140.6919M)
                .Build();
            db.WorkingHours.Add(workingHour3);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "09:00",
                SyukkinJikan2 = "13:00",
                SyukkinJikan3 = "16:30",
                TaikinJikan1 = "12:00",
                TaikinJikan2 = "15:30",
                TaikinJikan3 = "18:00",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-12：打刻位置情報あり位置確認権限あり、打刻位置を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenDakokuIchiKengenAri_ReturnIchiari()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync(出退勤一覧の打刻位置確認);

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 09:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 12:00"))
                .WithSyukkinLatitude(35.6895M)
                .WithSyukkinLongitude(139.6917M)
                .WithTaikinLatitude(36.6895M)
                .WithTaikinLongitude(140.6917M)
                .Build();
            db.WorkingHours.Add(workingHour1);

            // 打刻2
            var workingHour2 = new WorkingHoursBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 13:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 15:30"))
                .WithSyukkinLatitude(35.6896M)
                .WithSyukkinLongitude(139.6918M)
                .WithTaikinLatitude(36.6896M)
                .WithTaikinLongitude(140.6918M)
                .Build();
            db.WorkingHours.Add(workingHour2);

            // 打刻3
            var workingHour3 = new WorkingHoursBuilder()
                .WithId(3)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 16:30"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 18:00"))
                .WithSyukkinLatitude(35.6897M)
                .WithSyukkinLongitude(139.6919M)
                .WithTaikinLatitude(36.6897M)
                .WithTaikinLongitude(140.6919M)
                .Build();
            db.WorkingHours.Add(workingHour3);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert
            Assert.AreEqual("https://www.google.com/maps?q=", model.AttendanceView.googlemap, "googlemap が一致しません。");

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "09:00",
                SyukkinJikan2 = "13:00",
                SyukkinJikan3 = "16:30",
                TaikinJikan1 = "12:00",
                TaikinJikan2 = "15:30",
                TaikinJikan3 = "18:00",
                SyukkinPos1 = "35.6895,139.6917",
                SyukkinPos2 = "35.6896,139.6918",
                SyukkinPos3 = "35.6897,139.6919",
                TaikinPos1 = "36.6895,140.6917",
                TaikinPos2 = "36.6896,140.6918",
                TaikinPos3 = "36.6897,140.6919",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-13：打刻位置情報なし位置確認権限あり、打刻位置を返さない
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenDakokuIchiAriKengenAri_ReturnIchinashi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync(出退勤一覧の打刻位置確認);

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 09:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);

            // 打刻2
            var workingHour2 = new WorkingHoursBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 13:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 15:30"))
                .Build();
            db.WorkingHours.Add(workingHour2);

            // 打刻3
            var workingHour3 = new WorkingHoursBuilder()
                .WithId(3)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 16:30"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 18:00"))
                .Build();
            db.WorkingHours.Add(workingHour3);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "09:00",
                SyukkinJikan2 = "13:00",
                SyukkinJikan3 = "16:30",
                TaikinJikan1 = "12:00",
                TaikinJikan2 = "15:30",
                TaikinJikan3 = "18:00",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-14：出勤時間１空欄・伺い申請なし、日またぎにならない
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenSyukkin1KaraShinseiNashi_ReturnNotHimatagi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "",
                SyukkinJikan2 = "",
                SyukkinJikan3 = "",
                TaikinJikan1 = "12:00",
                TaikinJikan2 = "",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-15：出勤時間１空欄・深夜作業申請、日またぎにならない
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenSyukkin1KaraShinyaShinsei_ReturnNotHimatagi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);

            // 伺い申請登録（深夜作業）
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(深夜作業)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "",
                SyukkinJikan2 = "",
                SyukkinJikan3 = "",
                TaikinJikan1 = "12:00",
                TaikinJikan2 = "",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-16：出勤時間１空欄・夜間作業申請、日またぎになる
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenSyukkin1KaraYakanShinsei_ReturnHimatagi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);

            // 伺い申請登録（夜間作業）
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(夜間作業)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "",
                SyukkinJikan2 = "",
                SyukkinJikan3 = "",
                TaikinJikan1 = "12:00",
                TaikinJikan2 = "",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = true,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-17：退勤時間１空欄・伺い申請なし、日またぎにならない
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenTaikin1KaraShinseiNashi_ReturnNotHimatagi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "12:00",
                SyukkinJikan2 = "",
                SyukkinJikan3 = "",
                TaikinJikan1 = "",
                TaikinJikan2 = "",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-18：退勤時間１空欄・深夜作業申請、日またぎになる
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenTaikin1KaraShinyaShinsei_ReturnHimatagi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);

            // 伺い申請登録（深夜作業）
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(深夜作業)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "12:00",
                SyukkinJikan2 = "",
                SyukkinJikan3 = "",
                TaikinJikan1 = "",
                TaikinJikan2 = "",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = true,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-19：退勤時間１空欄・夜間作業申請、日またぎにならない
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenTaikin1KaraYakanShinsei_ReturnNotHimatagi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);

            // 伺い申請登録（夜間作業）
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(夜間作業)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "12:00",
                SyukkinJikan2 = "",
                SyukkinJikan3 = "",
                TaikinJikan1 = "",
                TaikinJikan2 = "",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-20：退勤時間２空欄・伺い申請なし、日またぎにならない
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenTaikin2KaraShinseiNashi_ReturnNotHimatagi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);
            // 打刻2
            var workingHour2 = new WorkingHoursBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 8:30"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 11:00"))
                .Build();
            db.WorkingHours.Add(workingHour2);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "08:30",
                SyukkinJikan2 = "12:00",
                SyukkinJikan3 = "",
                TaikinJikan1 = "11:00",
                TaikinJikan2 = "",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-21：退勤時間２空欄・深夜作業申請、日またぎになる
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenTaikin2KaraShinyaShinsei_ReturnHimatagi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);
            // 打刻2
            var workingHour2 = new WorkingHoursBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 8:30"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 11:00"))
                .Build();
            db.WorkingHours.Add(workingHour2);

            // 伺い申請登録（深夜作業）
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(深夜作業)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "08:30",
                SyukkinJikan2 = "12:00",
                SyukkinJikan3 = "",
                TaikinJikan1 = "11:00",
                TaikinJikan2 = "",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = true,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-22：退勤時間２空欄・夜間作業申請、日またぎにならない
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenTaikin2KaraYakanShinsei_ReturnNotHimatagi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);
            // 打刻2
            var workingHour2 = new WorkingHoursBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 8:30"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 11:00"))
                .Build();
            db.WorkingHours.Add(workingHour2);

            // 伺い申請登録（夜間作業）
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(夜間作業)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "08:30",
                SyukkinJikan2 = "12:00",
                SyukkinJikan3 = "",
                TaikinJikan1 = "11:00",
                TaikinJikan2 = "",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-23：退勤時間３空欄・伺い申請なし、日またぎにならない
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenTaikin3KaraShinseiNashi_ReturnNotHimatagi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);
            // 打刻2
            var workingHour2 = new WorkingHoursBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 8:30"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 11:00"))
                .Build();
            db.WorkingHours.Add(workingHour2);
            // 打刻3
            var workingHour3 = new WorkingHoursBuilder()
                .WithId(3)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 6:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 7:30"))
                .Build();
            db.WorkingHours.Add(workingHour3);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "06:00",
                SyukkinJikan2 = "08:30",
                SyukkinJikan3 = "12:00",
                TaikinJikan1 = "07:30",
                TaikinJikan2 = "11:00",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-24：退勤時間３空欄・深夜作業申請、日またぎになる
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenTaikin3KaraShinyaShinsei_ReturnHimatagi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);
            // 打刻2
            var workingHour2 = new WorkingHoursBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 8:30"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 11:00"))
                .Build();
            db.WorkingHours.Add(workingHour2);
            // 打刻3
            var workingHour3 = new WorkingHoursBuilder()
                .WithId(3)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 6:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 7:30"))
                .Build();
            db.WorkingHours.Add(workingHour3);

            // 伺い申請登録（深夜作業）
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(深夜作業)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "06:00",
                SyukkinJikan2 = "08:30",
                SyukkinJikan3 = "12:00",
                TaikinJikan1 = "07:30",
                TaikinJikan2 = "11:00",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = true,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-25：退勤時間３空欄・夜間作業申請、日またぎにならない
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenTaikin3KaraYakanShinsei_ReturnNotHimatagi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHoursBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 12:00"))
                .Build();
            db.WorkingHours.Add(workingHour1);
            // 打刻2
            var workingHour2 = new WorkingHoursBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 8:30"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 11:00"))
                .Build();
            db.WorkingHours.Add(workingHour2);
            // 打刻3
            var workingHour3 = new WorkingHoursBuilder()
                .WithId(3)
                .WithSyainId(1)
                .WithHiduke(DateOnly.Parse("2026/1/5"))
                .WithSyukkinTime(DateTime.Parse("2026/1/5 6:00"))
                .WithTaikinTime(DateTime.Parse("2026/1/5 7:30"))
                .Build();
            db.WorkingHours.Add(workingHour3);

            // 伺い申請登録（夜間作業）
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(夜間作業)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 勤怠打刻データ確認
            var syuttaikiroku = new Zouryoku.Pages.Attendance.AttendanceList.SyuttaiKirokuData
            {
                SyukkinJikan1 = "06:00",
                SyukkinJikan2 = "08:30",
                SyukkinJikan3 = "12:00",
                TaikinJikan1 = "07:30",
                TaikinJikan2 = "11:00",
                TaikinJikan3 = "",
                SyukkinPos1 = "",
                SyukkinPos2 = "",
                SyukkinPos3 = "",
                TaikinPos1 = "",
                TaikinPos2 = "",
                TaikinPos3 = "",
                IsHimatagiSyukkin1 = false,
                IsHimatagiTaikin1 = false,
                IsHimatagiTaikin2 = false,
                IsHimatagiTaikin3 = false,
            };
            AssertKinmuDataSyuttaikin(syuttaikiroku, kinmuList[0].GetSyuttaiKirokuData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-26：日報（出退出時間１）あり、出退出時間１を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenNippouSyutaisyutsu1_ReturnSyuttaisyutsu1()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 日報データ登録
            var nippou = new NippouBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithSyukkinHm1(TimeOnly.Parse("08:30"))
                .WithTaisyutsuHm1(TimeOnly.Parse("12:00"))
                .WithNippouYmd(DateOnly.Parse("2026/1/5"))
                .WithTourokuKbn(確定保存)
                .WithSyukkinKubunId1(2)
                .Build();
            db.Nippous.Add(nippou);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 日報データ確認
            var nippouClass = new NippouData
            {
                Syukkin1 = "08:30",
                Syukkin2 = "",
                Syukkin3 = "",
                Taisyutsu1 = "12:00",
                Taisyutsu2 = "",
                Taisyutsu3 = "",
                SyukkinKubunList = new List<string>{"通常勤務"},
            };
            AssertKinmuDataNippou(nippouClass, kinmuList[0].GetNippouData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-27：日葡（出退出時間１・２）あり、出退出時間１・２を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenNippouSyutaisyutsu2_ReturnSyuttaisyutsu2()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 日報データ登録
            var nippou = new NippouBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithSyukkinHm1(TimeOnly.Parse("08:30"))
                .WithTaisyutsuHm1(TimeOnly.Parse("12:00"))
                .WithSyukkinHm2(TimeOnly.Parse("13:00"))
                .WithTaisyutsuHm2(TimeOnly.Parse("17:30"))
                .WithNippouYmd(DateOnly.Parse("2026/1/5"))
                .WithTourokuKbn(確定保存)
                .WithSyukkinKubunId1(2)
                .Build();
            db.Nippous.Add(nippou);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 日報データ確認
            var nippouClass = new NippouData
            {
                Syukkin1 = "08:30",
                Syukkin2 = "13:00",
                Syukkin3 = "",
                Taisyutsu1 = "12:00",
                Taisyutsu2 = "17:30",
                Taisyutsu3 = "",
                SyukkinKubunList = new List<string> { "通常勤務" },
            };
            AssertKinmuDataNippou(nippouClass, kinmuList[0].GetNippouData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-28：日葡（出退出時間１・２・３）あり、出退出時間１・２・３を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenNippouSyutaisyutsu3_ReturnSyuttaisyutsu3()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 日報データ登録
            var nippou = new NippouBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithSyukkinHm1(TimeOnly.Parse("08:30"))
                .WithTaisyutsuHm1(TimeOnly.Parse("12:00"))
                .WithSyukkinHm2(TimeOnly.Parse("13:00"))
                .WithTaisyutsuHm2(TimeOnly.Parse("17:30"))
                .WithSyukkinHm3(TimeOnly.Parse("19:00"))
                .WithTaisyutsuHm3(TimeOnly.Parse("22:00"))
                .WithNippouYmd(DateOnly.Parse("2026/1/5"))
                .WithTourokuKbn(確定保存)
                .WithSyukkinKubunId1(2)
                .Build();
            db.Nippous.Add(nippou);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);


            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 日報データ確認
            var nippouClass = new NippouData
            {
                Syukkin1 = "08:30",
                Syukkin2 = "13:00",
                Syukkin3 = "19:00",
                Taisyutsu1 = "12:00",
                Taisyutsu2 = "17:30",
                Taisyutsu3 = "22:00",
                SyukkinKubunList = new List<string> { "通常勤務" },
            };
            AssertKinmuDataNippou(nippouClass, kinmuList[0].GetNippouData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-29：日葡（出勤区分：生理休暇）あり、出勤区分表示変換なし
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenNippouSeirikyuuka_ReturnKubunhenkanNashi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 日報データ登録
            var nippou = new NippouBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithNippouYmd(DateOnly.Parse("2026/1/5"))
                .WithTourokuKbn(確定保存)
                .WithSyukkinKubunId1(4)
                .Build();
            db.Nippous.Add(nippou);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 日報データ確認
            var nippouClass = new NippouData
            {
                Syukkin1 = "",
                Syukkin2 = "",
                Syukkin3 = "",
                Taisyutsu1 = "",
                Taisyutsu2 = "",
                Taisyutsu3 = "",
                SyukkinKubunList = new List<string> { "生理休暇" },
            };
            AssertKinmuDataNippou(nippouClass, kinmuList[0].GetNippouData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-30：他者の日葡（出勤区分：生理休暇）あり、出勤区分表示変換あり
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenOtherNippouSeirikyuuka_ReturnKubunhenkanAri()
        {
            // Arrange
            InitializeTestData_OnPostSearchAsync();

            // 日報データ登録
            var nippou = new NippouBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithNippouYmd(DateOnly.Parse("2026/1/5"))
                .WithTourokuKbn(確定保存)
                .WithSyukkinKubunId1(4)
                .Build();
            db.Nippous.Add(nippou);

            // 社員BASE & 社員 データ登録
            // ログイン用ユーザー
            var syainBaseA = new SyainBasisBuilder()
                .WithId(12)
                .Build();
            db.SyainBases.Add(syainBaseA);

            var syainB = new SyainBuilder()
                .WithId(2)
                .WithCode("1002")
                .WithName("ユーザー")
                .WithSyainBaseId(syainBaseA.Id)
                .WithBusyoId(1)
                .WithJyunjyo(6)
                .WithStartYmd(DateOnly.Parse("2025/4/1"))
                .WithEndYmd(DateOnly.Parse("2026/4/1"))
                .Build();
            db.Syains.Add(syainB);

            await db.SaveChangesAsync();

            var model = CreateModel(syainB);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 日報データ確認
            var nippouClass = new NippouData
            {
                Syukkin1 = "",
                Syukkin2 = "",
                Syukkin3 = "",
                Taisyutsu1 = "",
                Taisyutsu2 = "",
                Taisyutsu3 = "",
                SyukkinKubunList = new List<string> { "その他特別休暇" },
            };
            AssertKinmuDataNippou(nippouClass, kinmuList[0].GetNippouData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-31：日報（出勤区分２）あり、出勤区分複数
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenNippouSyukkinKubun2Ari_ReturnSyukinKubunFukusuu()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 日報データ登録
            var nippou = new NippouBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithNippouYmd(DateOnly.Parse("2026/1/5"))
                .WithTourokuKbn(確定保存)
                .WithSyukkinKubunId1(2)
                .WithSyukkinKubunId2(3)
                .Build();
            db.Nippous.Add(nippou);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // 日報データ確認
            var nippouClass = new NippouData
            {
                Syukkin1 = "",
                Syukkin2 = "",
                Syukkin3 = "",
                Taisyutsu1 = "",
                Taisyutsu2 = "",
                Taisyutsu3 = "",
                SyukkinKubunList = new List<string> { "通常勤務", "休日出勤" },
            };
            AssertKinmuDataNippou(nippouClass, kinmuList[0].GetNippouData(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-32：PCログ（ログオン）あり・ログ表示権限なし、ログ表示なし
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenPcLogOnKengenNashi_ReturnLogHyoujiNashi()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // PCログデータ登録
            var pclog1 = new PcLogBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithDatetime(DateTime.Parse("2026/1/5 08:30"))
                .WithPcName("PC-0001")
                .WithOperation(ログオン)
                .Build();
            db.PcLogs.Add(pclog1);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // PCログデータ確認
            AssertKinmuDataPcLog(new List<(string PcName, string StartTime, string EndTime)>(), kinmuList[0].GetPcLogDataList(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-33：PCログ（ログオン）あり・表示権限あり、ログ表示（終了時間空欄）あり
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenPcLogOnKengenAri_ReturnLogHyoujiSyuryouNull()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync(PCログ出力);

            // PCログデータ登録
            var pclog1 = new PcLogBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithDatetime(DateTime.Parse("2026/1/5 08:30"))
                .WithPcName("PC-0001")
                .WithOperation(ログオン)
                .Build();
            db.PcLogs.Add(pclog1);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // PCログデータ確認
            var pcLogs = new List<(string PcName, string StartTime, string EndTime)>
            {
                (
                    PcName: "PC-0001",
                    StartTime: "08:30",
                    EndTime: ""
                ),
            };
            AssertKinmuDataPcLog(pcLogs, kinmuList[0].GetPcLogDataList(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-34：PCログ（ログオフ）あり・表示権限あり、ログ表示（開始時間空欄）あり
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenPcLogOffKengenAri_ReturnLogHyoujiKaishiNull()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync(PCログ出力);

            // PCログデータ登録
            var pclog1 = new PcLogBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithDatetime(DateTime.Parse("2026/1/5 08:30"))
                .WithPcName("PC-0001")
                .WithOperation(ログオフ)
                .Build();
            db.PcLogs.Add(pclog1);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // PCログデータ確認
            var pcLogs = new List<(string PcName, string StartTime, string EndTime)>
            {
                (
                    PcName: "PC-0001",
                    StartTime: "",
                    EndTime: "08:30"
                ),
            };
            AssertKinmuDataPcLog(pcLogs, kinmuList[0].GetPcLogDataList(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-35：PCログ（ログオン・ログオフ）あり・表示権限あり、ログ表示（開始終了セット）あり
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenPcLogOnOffKengenAri_ReturnLogHyoujiKaishiSyuuryouAri()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync(PCログ出力);

            // PCログデータ登録
            var pclog1 = new PcLogBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithDatetime(DateTime.Parse("2026/1/5 08:30"))
                .WithPcName("PC-0001")
                .WithOperation(ログオン)
                .Build();
            db.PcLogs.Add(pclog1);
            var pclog2 = new PcLogBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithDatetime(DateTime.Parse("2026/1/5 17:30"))
                .WithPcName("PC-0001")
                .WithOperation(ログオフ)
                .Build();
            db.PcLogs.Add(pclog2);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // PCログデータ確認
            var pcLogs = new List<(string PcName, string StartTime, string EndTime)>
            {
                (
                    PcName: "PC-0001",
                    StartTime: "08:30",
                    EndTime: "17:30"
                ),
            };
            AssertKinmuDataPcLog(pcLogs, kinmuList[0].GetPcLogDataList(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-36：PCログ（複数）あり・表示権限あり、ログ表示（複数）あり
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenPcLogFukusuuKengenAri_ReturnLogHyoujiFukusuu()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync(PCログ出力);

            // PCログデータ登録
            var pclog1 = new PcLogBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithDatetime(DateTime.Parse("2026/1/5 08:30"))
                .WithPcName("PC-0001")
                .WithOperation(ログオン)
                .Build();
            db.PcLogs.Add(pclog1);
            var pclog2 = new PcLogBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithDatetime(DateTime.Parse("2026/1/5 17:30"))
                .WithPcName("PC-0001")
                .WithOperation(ログオフ)
                .Build();
            db.PcLogs.Add(pclog2);
            var pclog3 = new PcLogBuilder()
                .WithId(3)
                .WithSyainId(1)
                .WithDatetime(DateTime.Parse("2026/1/5 12:00"))
                .WithPcName("PC-0002")
                .WithOperation(ログオン)
                .Build();
            db.PcLogs.Add(pclog3);
            var pclog4 = new PcLogBuilder()
                .WithId(4)
                .WithSyainId(1)
                .WithDatetime(DateTime.Parse("2026/1/5 14:00"))
                .WithPcName("PC-0002")
                .WithOperation(ログオフ)
                .Build();
            db.PcLogs.Add(pclog4);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var loginUser = model.AttendanceView.LoginUser;

            // Assert

            // PCログデータ確認
            var pcLogs = new List<(string PcName, string StartTime, string EndTime)>
            {
                (
                    PcName: "PC-0001",
                    StartTime: "08:30",
                    EndTime: "17:30"
                ),
                (
                    PcName: "PC-0002",
                    StartTime: "12:00",
                    EndTime: "14:00"
                ),
            };
            AssertKinmuDataPcLog(pcLogs, kinmuList[0].GetPcLogDataList(loginUser));
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-37：伺い申請あり、伺い申請表示
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenUkagaishinseiAri_ReturnUkagaishinseiHyouji()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 伺い申請登録（深夜作業）
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(深夜作業)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 伺い申請 データ確認
            var pcLogs = new List<string>
            {
                "深夜作業",
            };
            AssertKinmuDataUkagaiShinsei(pcLogs, kinmuList[0].GetUkagaiShinseiList());
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-38：休暇申請あり、午前表示
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenKyuukashinseiAri_ReturnGozenHyouji()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 伺い申請登録
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .WithKaishiJikoku(TimeOnly.Parse("08:30"))
                .WithSyuryoJikoku(TimeOnly.Parse("12:00"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(休暇申請)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 伺い申請 データ確認
            var pcLogs = new List<string>
            {
                "休暇申請（午前）",
            };
            AssertKinmuDataUkagaiShinsei(pcLogs, kinmuList[0].GetUkagaiShinseiList());
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-39：休暇申請あり、午後表示
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenKyuukashinseiAri_ReturnGogoHyouji()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 伺い申請登録
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .WithKaishiJikoku(TimeOnly.Parse("13:00"))
                .WithSyuryoJikoku(TimeOnly.Parse("17:30"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(休暇申請)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 伺い申請 データ確認
            var pcLogs = new List<string>
            {
                "休暇申請（午後）",
            };
            AssertKinmuDataUkagaiShinsei(pcLogs, kinmuList[0].GetUkagaiShinseiList());
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-40：休暇申請あり、終日表示
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenKyuukashinseiAri_ReturnSyuujitsuHyouji()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 伺い申請登録
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .WithKaishiJikoku(TimeOnly.Parse("08:30"))
                .WithSyuryoJikoku(TimeOnly.Parse("17:30"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(休暇申請)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 伺い申請 データ確認
            var pcLogs = new List<string>
            {
                "休暇申請（終日）",
            };
            AssertKinmuDataUkagaiShinsei(pcLogs, kinmuList[0].GetUkagaiShinseiList());
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-41：伺い申請複数あり（1ヘッダー2明細）、複数表示
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenUkagaishinseiFukusuuAri_ReturnFukusuuHyouji()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 伺い申請登録
            var ukagaiHeader1 = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader1);
            var ukagaiShinsei1 = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(早朝作業)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei1);
            var ukagaiShinsei2 = new UkagaiShinseiBuilder()
                .WithId(2)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(テレワーク)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei2);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 伺い申請 データ確認
            var pcLogs = new List<string>
            {
                "早朝作業",
                "テレワーク",
            };
            AssertKinmuDataUkagaiShinsei(pcLogs, kinmuList[0].GetUkagaiShinseiList());
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-42：伺い申請複数あり（2ヘッダー各1明細）、複数表示
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenUkagaishinseiFukusuuHeaderAri_ReturnFukusuuHyouji()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 伺い申請登録
            var ukagaiHeader1 = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader1);
            var ukagaiHeader2 = new UkagaiHeaderBuilder()
                .WithId(2)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader2);
            var ukagaiShinsei1 = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(早朝作業)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei1);
            var ukagaiShinsei2 = new UkagaiShinseiBuilder()
                .WithId(2)
                .WithUkagaiHeaderId(2)
                .WithUkagaiSyubetsu(テレワーク)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei2);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 伺い申請 データ確認
            var pcLogs = new List<string>
            {
                "早朝作業",
                "テレワーク",
            };
            AssertKinmuDataUkagaiShinsei(pcLogs, kinmuList[0].GetUkagaiShinseiList());
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-43：打刻時間修正申請あり、表示しない
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenDakokujikanSyuuseishinseiAri_ReturnNotHyouji()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 伺い申請登録
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(打刻時間修正)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 伺い申請 データ確認
            var pcLogs = new List<string>();
            AssertKinmuDataUkagaiShinsei(pcLogs, kinmuList[0].GetUkagaiShinseiList());
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-44：時間外労働時間制限拡張申請あり、表示しない
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenJikangaikakutyoushinseiAri_ReturnNotHyouji()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 伺い申請登録
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(時間外労働時間制限拡張)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 伺い申請 データ確認
            var pcLogs = new List<string>();
            AssertKinmuDataUkagaiShinsei(pcLogs, kinmuList[0].GetUkagaiShinseiList());
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-45：代理入力履歴あり、代理入力表示
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenDairinyuuryokuRirekiAri_ReturnDairinyuuryokuHyouji()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 日報データ登録
            var nippou = new NippouBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithNippouYmd(DateOnly.Parse("2026/1/5"))
                .WithTourokuKbn(確定保存)
                .WithSyukkinKubunId1(4)
                .Build();
            db.Nippous.Add(nippou);

            // 代理入力履歴データ登録
            var dairi = new DairiNyuryokuRirekiBuilder()
                .WithId(1)
                .WithNippouId(1)
                .Build();
            db.DairiNyuryokuRirekis.Add(dairi);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 伺い申請 データ確認
            var pcLogs = new List<string>
            {
                "代理入力",
            };
            AssertKinmuDataUkagaiShinsei(pcLogs, kinmuList[0].GetUkagaiShinseiList());
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-46：伺い申請・代理入力履歴あり、伺い申請・代理入力表示
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenUkagaiDairinyuuryokuRirekiAri_ReturnUkagaiDairinyuuryokuHyouji()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 日報データ登録
            var nippou = new NippouBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithNippouYmd(DateOnly.Parse("2026/1/5"))
                .WithTourokuKbn(確定保存)
                .WithSyukkinKubunId1(4)
                .Build();
            db.Nippous.Add(nippou);

            // 代理入力履歴データ登録
            var dairi = new DairiNyuryokuRirekiBuilder()
                .WithId(1)
                .WithNippouId(1)
                .Build();
            db.DairiNyuryokuRirekis.Add(dairi);

            // 伺い申請登録
            var ukagaiHeader = new UkagaiHeaderBuilder()
                .WithId(1)
                .WithSyainId(1)
                .WithWorkYmd(DateOnly.Parse("2026/1/5"))
                .Build();
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinseiBuilder()
                .WithId(1)
                .WithUkagaiHeaderId(1)
                .WithUkagaiSyubetsu(夜間作業)
                .Build();
            db.UkagaiShinseis.Add(ukagaiShinsei);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/5").ToDateOnly(),
                }
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert

            // 伺い申請 データ確認
            var pcLogs = new List<string>
            {
                "夜間作業",
                "代理入力",
            };
            AssertKinmuDataUkagaiShinsei(pcLogs, kinmuList[0].GetUkagaiShinseiList());
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-47：担当者(順序)昇順、担当者昇順・日付昇順ソートされること
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenTantousyaSyoujyun_ReturnTantousyaSyoujyunHidukeKoujyun()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 社員B
            var syainBaseB = new SyainBasisBuilder()
                .WithId(12)
                .Build();
            db.SyainBases.Add(syainBaseB);

            var syainB = new SyainBuilder()
                .WithId(2)
                .WithCode("1002")
                .WithName("社員B")
                .WithSyainBaseId(syainBaseB.Id)
                .WithBusyoId(1)
                .WithJyunjyo(10)
                .WithStartYmd(DateOnly.Parse("2025/4/1"))
                .WithEndYmd(DateOnly.Parse("2026/4/1"))
                .Build();
            db.Syains.Add(syainB);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/6").ToDateOnly(),
                },
                SortSelected = 担当者順,
                SortOrderType = 昇順,
                BusyoId = loginSyain.BusyoId,
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert
            Assert.AreEqual(4, kinmuList.Count(), "kinmuListの件数が一致しません。");

            // 社員・日付データ確認
            AssertKinmuDataSyain(1, "1001", "社員A", "01/05(月)", "app-line--weekday", kinmuList[0]);
            AssertKinmuDataSyain(1, "1001", "社員A", "01/06(火)", "app-line--weekday", kinmuList[1]);
            AssertKinmuDataSyain(2, "1002", "社員B", "01/05(月)", "app-line--weekday", kinmuList[2]);
            AssertKinmuDataSyain(2, "1002", "社員B", "01/06(火)", "app-line--weekday", kinmuList[3]);
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-48：担当者(順序)降順、担当者降順・日付昇順ソートされること
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenTantousyaKoujyun_ReturnTantousyaKoujyunHidukeKoujyun()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 社員B
            var syainBaseB = new SyainBasisBuilder()
                .WithId(12)
                .Build();
            db.SyainBases.Add(syainBaseB);

            var syainB = new SyainBuilder()
                .WithId(2)
                .WithCode("1002")
                .WithName("社員B")
                .WithSyainBaseId(syainBaseB.Id)
                .WithBusyoId(1)
                .WithJyunjyo(10)
                .WithStartYmd(DateOnly.Parse("2025/4/1"))
                .WithEndYmd(DateOnly.Parse("2026/4/1"))
                .Build();
            db.Syains.Add(syainB);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/6").ToDateOnly(),
                },
                SortSelected = 担当者順,
                SortOrderType = 降順,
                BusyoId = loginSyain.BusyoId,
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert
            Assert.AreEqual(4, kinmuList.Count(), "kinmuListの件数が一致しません。");

            // 社員・日付データ確認
            AssertKinmuDataSyain(2, "1002", "社員B", "01/05(月)", "app-line--weekday", kinmuList[0]);
            AssertKinmuDataSyain(2, "1002", "社員B", "01/06(火)", "app-line--weekday", kinmuList[1]);
            AssertKinmuDataSyain(1, "1001", "社員A", "01/05(月)", "app-line--weekday", kinmuList[2]);
            AssertKinmuDataSyain(1, "1001", "社員A", "01/06(火)", "app-line--weekday", kinmuList[3]);
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-49：日付昇順、日付昇順・担当者昇順ソートされること
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenHidukeSyoujyun_ReturnHidukeSyoujyunTantousyaKoujyun()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 社員B
            var syainBaseB = new SyainBasisBuilder()
                .WithId(12)
                .Build();
            db.SyainBases.Add(syainBaseB);

            var syainB = new SyainBuilder()
                .WithId(2)
                .WithCode("1002")
                .WithName("社員B")
                .WithSyainBaseId(syainBaseB.Id)
                .WithBusyoId(1)
                .WithJyunjyo(10)
                .WithStartYmd(DateOnly.Parse("2025/4/1"))
                .WithEndYmd(DateOnly.Parse("2026/4/1"))
                .Build();
            db.Syains.Add(syainB);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/6").ToDateOnly(),
                },
                SortSelected = 日付順,
                SortOrderType = 昇順,
                BusyoId=loginSyain.BusyoId,
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "";

            // Act
            await model.OnPostSearchAsync();

            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert
            Assert.AreEqual(4, kinmuList.Count(), "kinmuListの件数が一致しません。");

            // 社員・日付データ確認
            AssertKinmuDataSyain(1, "1001", "社員A", "01/05(月)", "app-line--weekday", kinmuList[0]);
            AssertKinmuDataSyain(2, "1002", "社員B", "01/05(月)", "app-line--weekday", kinmuList[1]);
            AssertKinmuDataSyain(1, "1001", "社員A", "01/06(火)", "app-line--weekday", kinmuList[2]);
            AssertKinmuDataSyain(2, "1002", "社員B", "01/06(火)", "app-line--weekday", kinmuList[3]);
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-50：日付降順、日付降順・担当者昇順ソートされること
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_WhenHidukeKoujyun_ReturnHidukeKoujyunTantousyaKoujyun()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 社員B
            var syainBaseB = new SyainBasisBuilder()
                .WithId(12)
                .Build();
            db.SyainBases.Add(syainBaseB);

            var syainB = new SyainBuilder()
                .WithId(2)
                .WithCode("1002")
                .WithName("社員B")
                .WithSyainBaseId(syainBaseB.Id)
                .WithBusyoId(1)
                .WithJyunjyo(10)
                .WithStartYmd(DateOnly.Parse("2025/4/1"))
                .WithEndYmd(DateOnly.Parse("2026/4/1"))
                .Build();
            db.Syains.Add(syainB);

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = DateTime.Parse("2026/1/5").ToDateOnly(),
                    To = DateTime.Parse("2026/1/6").ToDateOnly(),
                },
                SortSelected = 日付順,
                SortOrderType = 降順,
                BusyoId = loginSyain.BusyoId,
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert
            Assert.AreEqual(4, kinmuList.Count(), "kinmuListの件数が一致しません。");

            // 社員・日付データ確認
            AssertKinmuDataSyain(1, "1001", "社員A", "01/06(火)", "app-line--weekday", kinmuList[0]);
            AssertKinmuDataSyain(2, "1002", "社員B", "01/06(火)", "app-line--weekday", kinmuList[1]);
            AssertKinmuDataSyain(1, "1001", "社員A", "01/05(月)", "app-line--weekday", kinmuList[2]);
            AssertKinmuDataSyain(2, "1002", "社員B", "01/05(月)", "app-line--weekday", kinmuList[3]);
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-51：検索結果が検索結果最大件数のときデータが返されること
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_検索結果検索結果最大件数_データ取得成功()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            var fromDay = DateTime.Parse("2020/1/1").ToDateOnly();

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = fromDay,
                    To = fromDay.AddDays(SearchResultMaxCount - 1), //日付範囲で件数を増やす
                },
                SortSelected = 日付順,
                SortOrderType = 降順,
                BusyoId = loginSyain.BusyoId,
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;

            // Assert
            Assert.AreEqual(SearchResultMaxCount, kinmuList.Count(), "kinmuListの件数が一致しません。");
        }

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-52：検索結果が検索結果最大件数を超えるときメッセージを返し件数超過分はカットされること
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_検索結果検索結果最大件数超過_件数超過分カット()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            await db.SaveChangesAsync();

            var model = CreateModel(loginSyain);

            var fromDay = DateTime.Parse("2020/1/1").ToDateOnly();

            model.Search = new SearchModel()
            {
                Kikan = new DatepickerRangeModel.Values()
                {
                    From = fromDay,
                    To = fromDay.AddDays(SearchResultMaxCount), //日付範囲で件数を増やす
                },
                SortSelected = 日付順,
                SortOrderType = 降順,
                BusyoId = loginSyain.BusyoId,
            };
            model.SyainView = new SyainViewModel();
            model.SyainView.SelectedId = "11";

            // Act
            await model.OnPostSearchAsync();
            var kinmuList = model.AttendanceView.KinmuDataList;
            var message = model.AttendanceView.Message;

            // Assert
            Assert.IsNotEmpty(message, "メッセージがありません。");
            Assert.AreEqual(string.Format(WarningTooManyResults, SearchResultMaxCount), message, "メッセージが一致しません。");
            Assert.AreEqual(SearchResultMaxCount, kinmuList.Count(), "kinmuListの件数が一致しません。");
        }
    }
}
