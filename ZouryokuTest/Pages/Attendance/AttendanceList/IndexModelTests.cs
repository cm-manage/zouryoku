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
using ZouryokuTest.Extensions;
using static Model.Enums.BusinessTripRole;
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
        /// <summary>
        /// 出退勤一覧用のindexModelを生成し、テスト実行に必要なコンテキスト情報を設定します。
        /// </summary>
        /// <param name="loginUser">セッションに設定するログインユーザー（社員）情報</param>
        /// <returns>ページコンテキスト</returns>
        private IndexModel CreateModel(Syain loginUser)
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine, fakeTimeProvider)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };

            fakeTimeProvider.SetLocalNow(new(2026, 2, 10));
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
            var busyoBase1 = new BusyoBasis
            {
                Id = 1,
                Name = "部署1",
                BumoncyoId = 9999,
            };
            db.BusyoBases.Add(busyoBase1);

            var busyo1 = new Busyo
            {
                Id = 1,
                Code = "001",
                Name = "部署1",
                Jyunjyo = 5,
                IsActive = true,
                BusyoBaseId = busyoBase1.Id,
                KanaName = "ブショイチ",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                KasyoCode = "1",
                KaikeiCode = "1",
            };
            db.Busyos.Add(busyo1);

            // 部署1-1
            var busyoBase1_1 = new BusyoBasis
            {
                Id = 2,
                Name = "部署1-1",
            };
            db.BusyoBases.Add(busyoBase1_1);

            var busyo1_1 = new Busyo
            {
                Id = 2,
                Code = "002",
                Name = "部署1-1",
                Jyunjyo = 4,
                IsActive = true,
                OyaCode = "001",
                OyaId = 1,
                BusyoBaseId = busyoBase1_1.Id,
                KanaName = "ブショイチ",
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                KasyoCode = "1",
                KaikeiCode = "1",
            };
            db.Busyos.Add(busyo1_1);

            // 部署1-2
            var busyoBase1_2 = new BusyoBasis
            {
                Id = 3,
                Name = "部署1-2",
            };
            db.BusyoBases.Add(busyoBase1_2);

            var busyo1_2 = new Busyo
            {
                Id = 3,
                Code = "003",
                Name = "部署1-2",
                Jyunjyo = 4,
                IsActive = true,
                OyaCode = "001",
                OyaId = 1,
                BusyoBaseId = busyoBase1_2.Id,
                KanaName = "ブショイチ",
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                KasyoCode = "1",
                KaikeiCode = "1",
            };
            db.Busyos.Add(busyo1_2);

            // 社員BASE & 社員 データ登録
            // 社員A
            var syainBaseA = new SyainBasis
            {
                Id = 11,
                Code = "1001",
                Name = "社員A",
            };
            db.SyainBases.Add(syainBaseA);

            var syainA = new Syain
            {
                Id = 1,
                Code = "1001",
                Name = "社員A",
                SyainBaseId = syainBaseA.Id,
                BusyoId = busyo1.Id,
                Jyunjyo = 6,
                StartYmd = DateOnly.Parse("2025/4/1"),
                EndYmd = DateOnly.Parse("2026/4/1"),
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = busyo1.Code,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
            db.Syains.Add(syainA);

            // 社員B
            var syainBaseB = new SyainBasis
            {
                Id = 12,
                Code = "1002",
                Name = "社員B",
            };
            db.SyainBases.Add(syainBaseB);

            var syainB = new Syain
            {
                Id = 2,
                Code = "1002",
                Name = "社員B",
                SyainBaseId = syainBaseB.Id,
                BusyoId = busyo1_1.Id,
                Jyunjyo = 5,
                StartYmd = DateOnly.Parse("2025/4/1"),
                EndYmd = DateOnly.Parse("2026/4/1"),
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = busyo1.Code,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
            db.Syains.Add(syainB);

            // 社員C
            var syainBaseC = new SyainBasis
            {
                Id = 13,
                Code = "1003",
                Name = "社員C",
            };
            db.SyainBases.Add(syainBaseC);

            var syainC = new Syain
            {
                Id = 3,
                Code = "1003",
                Name = "社員C",
                SyainBaseId = syainBaseC.Id,
                BusyoId = busyo1_2.Id,
                Jyunjyo = 4,
                StartYmd = DateOnly.Parse("2025/4/1"),
                EndYmd = DateOnly.Parse("2026/4/1"),
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = busyo1_2.Code,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
            db.Syains.Add(syainC);

            // 社員D
            var syainBaseD = new SyainBasis
            {
                Id = 14,
                Code = "1004",
                Name = "社員D",
            };
            db.SyainBases.Add(syainBaseD);

            var syainD = new Syain
            {
                Id = 4,
                Code = "1004",
                Name = "社員D",
                SyainBaseId = syainBaseD.Id,
                BusyoId = busyo1.Id,
                Jyunjyo = 1,
                StartYmd = DateOnly.Parse("2025/4/1"),
                EndYmd = DateOnly.Parse("2026/4/1"),
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = busyo1.Code,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
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
            var busyoBase1 = new BusyoBasis
            {
                Id = 1,
                Name = "部署1",
                BumoncyoId = 9999,
            };
            db.BusyoBases.Add(busyoBase1);

            var busyo1 = new Busyo
            {
                Id = 1,
                Code = "001",
                Name = "部署1",
                Jyunjyo = 5,
                IsActive = true,
                BusyoBaseId = busyoBase1.Id,
                KanaName = "ブショイチ",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                KasyoCode = "1",
                KaikeiCode = "1",
            };
            db.Busyos.Add(busyo1);

            // 社員BASE & 社員 データ登録
            // 社員A
            var syainBaseA = new SyainBasis
            {
                Id = 11,
                Code = "1001",
                Name = "社員A",
            };
            db.SyainBases.Add(syainBaseA);

            var syainA = new Syain
            {
                Id = 1,
                Code = "1001",
                Name = "社員A",
                SyainBaseId = syainBaseA.Id,
                BusyoId = busyo1.Id,
                Jyunjyo = 6,
                StartYmd = DateOnly.Parse("2010/4/1"),
                EndYmd = DateOnly.Parse("2050/4/1"),
                Kengen = kengen,
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = busyo1.Code,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
            db.Syains.Add(syainA);

            // 出勤区分
            var syukkinKubun1 = new SyukkinKubun
            {
                Id = 1,
                CodeString = "01",
                Name = "休日",
                NameRyaku = "休日",
            };
            db.SyukkinKubuns.Add(syukkinKubun1);
            var syukkinKubun2 = new SyukkinKubun
            {
                Id = 2,
                CodeString = "02",
                Name = "通常勤務",
                NameRyaku = "通常",
            };
            db.SyukkinKubuns.Add(syukkinKubun2);
            var syukkinKubun3 = new SyukkinKubun
            {
                Id = 3,
                CodeString = "03",
                Name = "休日出勤",
                NameRyaku = "休出",
            };
            db.SyukkinKubuns.Add(syukkinKubun3);
            var syukkinKubun4 = new SyukkinKubun
            {
                Id = 4,
                CodeString = "13",
                Name = "生理休暇",
                NameRyaku = "生理休暇",
            };
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
            var busyoBase2 = new BusyoBasis
            {
                Id = 4,
                Name = "部署2",
                BumoncyoId = 9999,
            };
            db.BusyoBases.Add(busyoBase2);

            var busyo2 = new Busyo
            {
                Id = 4,
                Code = "004",
                Name = "部署2",
                Jyunjyo = 2,
                IsActive = true,
                BusyoBaseId = busyoBase2.Id,
                KanaName = "ブショニ",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                KasyoCode = "1",
                KaikeiCode = "1",
            };
            db.Busyos.Add(busyo2);

            // 部署2-1
            var busyoBase2_1 = new BusyoBasis
            {
                Id = 5,
                Name = "部署2-1",
            };
            db.BusyoBases.Add(busyoBase2_1);

            var busyo2_1 = new Busyo
            {
                Id = 5,
                Code = "005",
                Name = "部署2-1",
                Jyunjyo = 1,
                IsActive = true,
                OyaCode = "004",
                OyaId = 4,
                BusyoBaseId = busyoBase2_1.Id,
                KanaName = "ブショニノイチ",
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                KasyoCode = "1",
                KaikeiCode = "1",
            };
            db.Busyos.Add(busyo2_1);

            // 社員E
            var syainBaseE = new SyainBasis
            {
                Id = 15,
                Code = "1005",
                Name = "社員E",
            };
            db.SyainBases.Add(syainBaseE);

            var syainE = new Syain
            {
                Id = 5,
                Code = "1005",
                Name = "社員E",
                SyainBaseId = syainBaseE.Id,
                BusyoId = busyo2.Id,
                Jyunjyo = 3,
                StartYmd = DateOnly.Parse("2025/4/1"),
                EndYmd = DateOnly.Parse("2026/4/1"),
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = busyo2.Code,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
            db.Syains.Add(syainE);

            // 社員F
            var syainBaseF = new SyainBasis
            {
                Id = 16,
                Code = "1006",
                Name = "社員F",
            };
            db.SyainBases.Add(syainBaseF);

            var syainF = new Syain
            {
                Id = 6,
                Code = "1006",
                Name = "社員F",
                SyainBaseId = syainBaseF.Id,
                BusyoId = busyo2_1.Id,
                Jyunjyo = 2,
                StartYmd = DateOnly.Parse("2025/4/1"),
                EndYmd = DateOnly.Parse("2026/4/1"),
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = busyo2_1.Code,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
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
            var busyoBase2 = new BusyoBasis
            {
                Id = 4,
                Name = "部署2",
                BumoncyoId = 9999,
            };
            db.BusyoBases.Add(busyoBase2);

            var busyo2 = new Busyo
            {
                Id = 4,
                Code = "004",
                Name = "部署2",
                Jyunjyo = 2,
                IsActive = true,
                BusyoBaseId = busyoBase2.Id,
                KanaName = "ブショニ",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                KasyoCode = "1",
                KaikeiCode = "1",
            };
            db.Busyos.Add(busyo2);

            // 部署2-1
            var busyoBase2_1 = new BusyoBasis
            {
                Id = 5,
                Name = "部署2-1",
            };
            db.BusyoBases.Add(busyoBase2_1);

            var busyo2_1 = new Busyo
            {
                Id = 5,
                Code = "005",
                Name = "部署2-1",
                Jyunjyo = 1,
                IsActive = true,
                OyaCode = "004",
                OyaId = 4,
                BusyoBaseId = busyoBase2_1.Id,
                KanaName = "ブショニノイチ",
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                KasyoCode = "1",
                KaikeiCode = "1",
            };
            db.Busyos.Add(busyo2_1);

            // 社員E
            var syainBaseE = new SyainBasis
            {
                Id = 15,
                Code = "1005",
                Name = "社員E",
            };
            db.SyainBases.Add(syainBaseE);

            var syainE = new Syain
            {
                Id = 5,
                Code = "1005",
                Name = "社員E",
                SyainBaseId = syainBaseE.Id,
                BusyoId = busyo2.Id,
                Jyunjyo = 3,
                StartYmd = DateOnly.Parse("2025/4/1"),
                EndYmd = DateOnly.Parse("2026/4/1"),
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = busyo2.Code,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
            db.Syains.Add(syainE);

            // 社員F
            var syainBaseF = new SyainBasis
            {
                Id = 16,
                Code = "1006",
                Name = "社員F",
            };
            db.SyainBases.Add(syainBaseF);

            var syainF = new Syain
            {
                Id = 6,
                Code = "1006",
                Name = "社員F",
                SyainBaseId = syainBaseF.Id,
                BusyoId = busyo2_1.Id,
                Jyunjyo = 2,
                StartYmd = DateOnly.Parse("2025/4/1"),
                EndYmd = DateOnly.Parse("2026/4/1"),
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = busyo2_1.Code,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
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
            var syukujitsu = new Hikadoubi
            {
                Id = 1,
                Ymd = DateOnly.Parse("2026/1/12"),
                SyukusaijitsuFlag = 祝祭日,
                RefreshDay = RefreshDayFlag.それ以外,
            };
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
            var syukujitsu = new Hikadoubi
            {
                Id = 1,
                Ymd = DateOnly.Parse("2026/1/17"),
                SyukusaijitsuFlag = 祝祭日,
                RefreshDay = RefreshDayFlag.それ以外,
            };
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
            var syukujitsu = new Hikadoubi
            {
                Id = 1,
                Ymd = DateOnly.Parse("2026/1/18"),
                SyukusaijitsuFlag = 祝祭日,
                RefreshDay = RefreshDayFlag.それ以外,
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 09:00"),
                TaikinTime = DateTime.Parse("2026/1/5 18:00"),
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 09:00"),
                TaikinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);

            // 打刻2
            var workingHour2 = new WorkingHour
            {
                Id = 2,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 13:00"),
                TaikinTime = DateTime.Parse("2026/1/5 18:00"),
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 09:00"),
                TaikinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);

            // 打刻2
            var workingHour2 = new WorkingHour
            {
                Id = 2,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 13:00"),
                TaikinTime = DateTime.Parse("2026/1/5 15:30"),
            };
            db.WorkingHours.Add(workingHour2);

            // 打刻3
            var workingHour3 = new WorkingHour
            {
                Id = 3,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 16:30"),
                TaikinTime = DateTime.Parse("2026/1/5 18:00"),
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 09:00"),
                TaikinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);

            // 打刻2
            var workingHour2 = new WorkingHour
            {
                Id = 2,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 19:00"),
                TaikinTime = DateTime.Parse("2026/1/5 20:00"),
            };
            db.WorkingHours.Add(workingHour2);

            // 打刻3
            var workingHour3 = new WorkingHour
            {
                Id = 3,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 16:30"),
                TaikinTime = DateTime.Parse("2026/1/5 18:00"),
            };
            db.WorkingHours.Add(workingHour3);

            // 打刻4
            var workingHour4 = new WorkingHour
            {
                Id = 4,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 13:00"),
                TaikinTime = DateTime.Parse("2026/1/5 15:30"),
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 09:00"),
                TaikinTime = DateTime.Parse("2026/1/5 12:00"),
                SyukkinLatitude = 35.6895M,
                SyukkinLongitude = 139.6917M,
                TaikinLatitude = 36.6895M,
                TaikinLongitude = 140.6917M,
            };
            db.WorkingHours.Add(workingHour1);

            // 打刻2
            var workingHour2 = new WorkingHour
            {
                Id = 2,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 13:00"),
                TaikinTime = DateTime.Parse("2026/1/5 15:30"),
                SyukkinLatitude = 35.6896M,
                SyukkinLongitude = 139.6918M,
                TaikinLatitude = 36.6896M,
                TaikinLongitude = 140.6918M,
            };
            db.WorkingHours.Add(workingHour2);

            // 打刻3
            var workingHour3 = new WorkingHour
            {
                Id = 3,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 16:30"),
                TaikinTime = DateTime.Parse("2026/1/5 18:00"),
                SyukkinLatitude = 35.6897M,
                SyukkinLongitude = 139.6919M,
                TaikinLatitude = 36.6897M,
                TaikinLongitude = 140.6919M,
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 09:00"),
                TaikinTime = DateTime.Parse("2026/1/5 12:00"),
                SyukkinLatitude = 35.6895M,
                SyukkinLongitude = 139.6917M,
                TaikinLatitude = 36.6895M,
                TaikinLongitude = 140.6917M,
            };
            db.WorkingHours.Add(workingHour1);

            // 打刻2
            var workingHour2 = new WorkingHour
            {
                Id = 2,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 13:00"),
                TaikinTime = DateTime.Parse("2026/1/5 15:30"),
                SyukkinLatitude = 35.6896M,
                SyukkinLongitude = 139.6918M,
                TaikinLatitude = 36.6896M,
                TaikinLongitude = 140.6918M,
            };
            db.WorkingHours.Add(workingHour2);

            // 打刻3
            var workingHour3 = new WorkingHour
            {
                Id = 3,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 16:30"),
                TaikinTime = DateTime.Parse("2026/1/5 18:00"),
                SyukkinLatitude = 35.6897M,
                SyukkinLongitude = 139.6919M,
                TaikinLatitude = 36.6897M,
                TaikinLongitude = 140.6919M,
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 09:00"),
                TaikinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);

            // 打刻2
            var workingHour2 = new WorkingHour
            {
                Id = 2,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 13:00"),
                TaikinTime = DateTime.Parse("2026/1/5 15:30"),
            };
            db.WorkingHours.Add(workingHour2);

            // 打刻3
            var workingHour3 = new WorkingHour
            {
                Id = 3,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 16:30"),
                TaikinTime = DateTime.Parse("2026/1/5 18:00"),
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                TaikinTime = DateTime.Parse("2026/1/5 12:00"),
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                TaikinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);

            // 伺い申請登録（深夜作業）
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 深夜作業,
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                TaikinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);

            // 伺い申請登録（夜間作業）
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 夜間作業,
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 12:00"),
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);

            // 伺い申請登録（深夜作業）
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 深夜作業,
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);

            // 伺い申請登録（夜間作業）
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 夜間作業,
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);
            // 打刻2
            var workingHour2 = new WorkingHour
            {
                Id = 2,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 8:30"),
                TaikinTime = DateTime.Parse("2026/1/5 11:00"),
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);
            // 打刻2
            var workingHour2 = new WorkingHour
            {
                Id = 2,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 8:30"),
                TaikinTime = DateTime.Parse("2026/1/5 11:00"),
            };
            db.WorkingHours.Add(workingHour2);

            // 伺い申請登録（深夜作業）
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 深夜作業,
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);
            // 打刻2
            var workingHour2 = new WorkingHour
            {
                Id = 2,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 8:30"),
                TaikinTime = DateTime.Parse("2026/1/5 11:00"),
            };
            db.WorkingHours.Add(workingHour2);

            // 伺い申請登録（夜間作業）
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 夜間作業,
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);
            // 打刻2
            var workingHour2 = new WorkingHour
            {
                Id = 2,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 8:30"),
                TaikinTime = DateTime.Parse("2026/1/5 11:00"),
            };
            db.WorkingHours.Add(workingHour2);
            // 打刻3
            var workingHour3 = new WorkingHour
            {
                Id = 3,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 6:00"),
                TaikinTime = DateTime.Parse("2026/1/5 7:30"),
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);
            // 打刻2
            var workingHour2 = new WorkingHour
            {
                Id = 2,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 8:30"),
                TaikinTime = DateTime.Parse("2026/1/5 11:00"),
            };
            db.WorkingHours.Add(workingHour2);
            // 打刻3
            var workingHour3 = new WorkingHour
            {
                Id = 3,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 6:00"),
                TaikinTime = DateTime.Parse("2026/1/5 7:30"),
            };
            db.WorkingHours.Add(workingHour3);

            // 伺い申請登録（深夜作業）
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 深夜作業,
            };
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
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 12:00"),
            };
            db.WorkingHours.Add(workingHour1);
            // 打刻2
            var workingHour2 = new WorkingHour
            {
                Id = 2,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 8:30"),
                TaikinTime = DateTime.Parse("2026/1/5 11:00"),
            };
            db.WorkingHours.Add(workingHour2);
            // 打刻3
            var workingHour3 = new WorkingHour
            {
                Id = 3,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 6:00"),
                TaikinTime = DateTime.Parse("2026/1/5 7:30"),
            };
            db.WorkingHours.Add(workingHour3);

            // 伺い申請登録（夜間作業）
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 夜間作業,
            };
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
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = 1,
                SyukkinHm1 = TimeOnly.Parse("08:30"),
                TaisyutsuHm1 = TimeOnly.Parse("12:00"),
                NippouYmd = DateOnly.Parse("2026/1/5"),
                TourokuKubun = 確定保存,
                SyukkinKubunId1 = 2,
            };
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
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = 1,
                SyukkinHm1 = TimeOnly.Parse("08:30"),
                TaisyutsuHm1 = TimeOnly.Parse("12:00"),
                SyukkinHm2 = TimeOnly.Parse("13:00"),
                TaisyutsuHm2 = TimeOnly.Parse("17:30"),
                NippouYmd = DateOnly.Parse("2026/1/5"),
                TourokuKubun = 確定保存,
                SyukkinKubunId1 = 2,
            };
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
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = 1,
                SyukkinHm1 = TimeOnly.Parse("08:30"),
                TaisyutsuHm1 = TimeOnly.Parse("12:00"),
                SyukkinHm2 = TimeOnly.Parse("13:00"),
                TaisyutsuHm2 = TimeOnly.Parse("17:30"),
                SyukkinHm3 = TimeOnly.Parse("19:00"),
                TaisyutsuHm3 = TimeOnly.Parse("22:00"),
                NippouYmd = DateOnly.Parse("2026/1/5"),
                TourokuKubun = 確定保存,
                SyukkinKubunId1 = 2,
            };
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
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = 1,
                NippouYmd = DateOnly.Parse("2026/1/5"),
                TourokuKubun = 確定保存,
                SyukkinKubunId1 = 4,
            };
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
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = 1,
                NippouYmd = DateOnly.Parse("2026/1/5"),
                TourokuKubun = 確定保存,
                SyukkinKubunId1 = 4,
            };
            db.Nippous.Add(nippou);

            // 社員BASE & 社員 データ登録
            // ログイン用ユーザー
            var syainBaseA = new SyainBasis
            {
                Id = 12,
                Code = "1002",
                Name = "ユーザー",
            };
            db.SyainBases.Add(syainBaseA);

            var syainB = new Syain
            {
                Id = 2,
                Code = "1002",
                Name = "ユーザー",
                SyainBaseId = syainBaseA.Id,
                BusyoId = 1,
                Jyunjyo = 6,
                StartYmd = DateOnly.Parse("2025/4/1"),
                EndYmd = DateOnly.Parse("2026/4/1"),
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = "0001",
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
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
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = 1,
                NippouYmd = DateOnly.Parse("2026/1/5"),
                TourokuKubun = 確定保存,
                SyukkinKubunId1 = 2,
                SyukkinKubunId2 = 3,
            };
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
            var pclog1 = new PcLog
            {
                Id = 1,
                SyainId = 1,
                Datetime = DateTime.Parse("2026/1/5 08:30"),
                PcName = "PC-0001",
                Operation = ログオン,
            };
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
            var pclog1 = new PcLog
            {
                Id = 1,
                SyainId = 1,
                Datetime = DateTime.Parse("2026/1/5 08:30"),
                PcName = "PC-0001",
                Operation = ログオン,
            };
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
            var pclog1 = new PcLog
            {
                Id = 1,
                SyainId = 1,
                Datetime = DateTime.Parse("2026/1/5 08:30"),
                PcName = "PC-0001",
                Operation = ログオフ,
            };
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
            var pclog1 = new PcLog
            {
                Id = 1,
                SyainId = 1,
                Datetime = DateTime.Parse("2026/1/5 08:30"),
                PcName = "PC-0001",
                Operation = ログオン,
            };
            db.PcLogs.Add(pclog1);
            var pclog2 = new PcLog
            {
                Id = 2,
                SyainId = 1,
                Datetime = DateTime.Parse("2026/1/5 17:30"),
                PcName = "PC-0001",
                Operation = ログオフ,
            };
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
            var pclog1 = new PcLog
            {
                Id = 1,
                SyainId = 1,
                Datetime = DateTime.Parse("2026/1/5 08:30"),
                PcName = "PC-0001",
                Operation = ログオン,
            };
            db.PcLogs.Add(pclog1);
            var pclog2 = new PcLog
            {
                Id = 2,
                SyainId = 1,
                Datetime = DateTime.Parse("2026/1/5 17:30"),
                PcName = "PC-0001",
                Operation = ログオフ,
            };
            db.PcLogs.Add(pclog2);
            var pclog3 = new PcLog
            {
                Id = 3,
                SyainId = 1,
                Datetime = DateTime.Parse("2026/1/5 12:00"),
                PcName = "PC-0002",
                Operation = ログオン,
            };
            db.PcLogs.Add(pclog3);
            var pclog4 = new PcLog
            {
                Id = 4,
                SyainId = 1,
                Datetime = DateTime.Parse("2026/1/5 14:00"),
                PcName = "PC-0002",
                Operation = ログオフ,
            };
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
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 深夜作業,
            };
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
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
                KaishiJikoku = TimeOnly.Parse("08:30"),
                SyuryoJikoku = TimeOnly.Parse("12:00"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 休暇申請,
            };
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
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
                KaishiJikoku = TimeOnly.Parse("13:00"),
                SyuryoJikoku = TimeOnly.Parse("17:30"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 休暇申請,
            };
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
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
                KaishiJikoku = TimeOnly.Parse("08:30"),
                SyuryoJikoku = TimeOnly.Parse("17:30"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 休暇申請,
            };
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
            var ukagaiHeader1 = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader1);
            var ukagaiShinsei1 = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 早朝作業,
            };
            db.UkagaiShinseis.Add(ukagaiShinsei1);
            var ukagaiShinsei2 = new UkagaiShinsei
            {
                Id = 2,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = テレワーク,
            };
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
            var ukagaiHeader1 = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader1);
            var ukagaiHeader2 = new UkagaiHeader
            {
                Id = 2,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader2);
            var ukagaiShinsei1 = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 早朝作業,
            };
            db.UkagaiShinseis.Add(ukagaiShinsei1);
            var ukagaiShinsei2 = new UkagaiShinsei
            {
                Id = 2,
                UkagaiHeaderId = 2,
                UkagaiSyubetsu = テレワーク,
            };
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
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 打刻時間修正,
            };
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
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 時間外労働時間制限拡張,
            };
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
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = 1,
                NippouYmd = DateOnly.Parse("2026/1/5"),
                TourokuKubun = 確定保存,
                SyukkinKubunId1 = 4,
            };
            db.Nippous.Add(nippou);

            // 代理入力履歴データ登録
            var dairi = new DairiNyuryokuRireki
            {
                Id = 1,
                NippouId = 1,
            };
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
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = 1,
                NippouYmd = DateOnly.Parse("2026/1/5"),
                TourokuKubun = 確定保存,
                SyukkinKubunId1 = 4,
            };
            db.Nippous.Add(nippou);

            // 代理入力履歴データ登録
            var dairi = new DairiNyuryokuRireki
            {
                Id = 1,
                NippouId = 1,
            };
            db.DairiNyuryokuRirekis.Add(dairi);

            // 伺い申請登録
            var ukagaiHeader = new UkagaiHeader
            {
                Id = 1,
                SyainId = 1,
                WorkYmd = DateOnly.Parse("2026/1/5"),
            };
            db.UkagaiHeaders.Add(ukagaiHeader);
            var ukagaiShinsei = new UkagaiShinsei
            {
                Id = 1,
                UkagaiHeaderId = 1,
                UkagaiSyubetsu = 夜間作業,
            };
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
            var syainBaseB = new SyainBasis
            {
                Id = 12,
                Code = "1002",
                Name = "社員B",
            };
            db.SyainBases.Add(syainBaseB);

            var syainB = new Syain
            {
                Id = 2,
                Code = "1002",
                Name = "社員B",
                SyainBaseId = syainBaseB.Id,
                BusyoId = 1,
                Jyunjyo = 10,
                StartYmd = DateOnly.Parse("2025/4/1"),
                EndYmd = DateOnly.Parse("2026/4/1"),
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = "0001",
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
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
            var syainBaseB = new SyainBasis
            {
                Id = 12,
                Code = "1002",
                Name = "社員B",
            };
            db.SyainBases.Add(syainBaseB);

            var syainB = new Syain
            {
                Id = 2,
                Code = "1002",
                Name = "社員B",
                SyainBaseId = syainBaseB.Id,
                BusyoId = 1,
                Jyunjyo = 10,
                StartYmd = DateOnly.Parse("2025/4/1"),
                EndYmd = DateOnly.Parse("2026/4/1"),
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = "0001",
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
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
            var syainBaseB = new SyainBasis
            {
                Id = 12,
                Code = "1002",
                Name = "社員B",
            };
            db.SyainBases.Add(syainBaseB);

            var syainB = new Syain
            {
                Id = 2,
                Code = "1002",
                Name = "社員B",
                SyainBaseId = syainBaseB.Id,
                BusyoId = 1,
                Jyunjyo = 10,
                StartYmd = DateOnly.Parse("2025/4/1"),
                EndYmd = DateOnly.Parse("2026/4/1"),
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = "0001",
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
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
            var syainBaseB = new SyainBasis
            {
                Id = 12,
                Code = "1002",
                Name = "社員B",
            };
            db.SyainBases.Add(syainBaseB);

            var syainB = new Syain
            {
                Id = 2,
                Code = "1002",
                Name = "社員B",
                SyainBaseId = syainBaseB.Id,
                BusyoId = 1,
                Jyunjyo = 10,
                StartYmd = DateOnly.Parse("2025/4/1"),
                EndYmd = DateOnly.Parse("2026/4/1"),
                KanaName = "サンプルタロウ",
                Seibetsu = '1',
                BusyoCode = "0001",
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Kyusyoku = 0,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "00000",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Retired = false,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
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

        /// <summary>
        /// 出退勤検索
        ///     テストケース  出退勤検索-53：勤怠打刻・退勤時間が翌日00:00の場合に24:00を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_勤怠打刻退勤時間翌日0時_24時を返す()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 打刻データ登録
            // 打刻1
            var workingHour1 = new WorkingHour
            {
                Id = 1,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 09:00"),
                TaikinTime = DateTime.Parse("2026/1/6 00:00"),
            };
            db.WorkingHours.Add(workingHour1);

            // 打刻2
            var workingHour2 = new WorkingHour
            {
                Id = 2,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 13:00"),
                TaikinTime = DateTime.Parse("2026/1/6 00:00"),
            };
            db.WorkingHours.Add(workingHour2);

            // 打刻3
            var workingHour3 = new WorkingHour
            {
                Id = 3,
                SyainId = 1,
                Hiduke = DateOnly.Parse("2026/1/5"),
                SyukkinTime = DateTime.Parse("2026/1/5 16:30"),
                TaikinTime = DateTime.Parse("2026/1/6 00:00"),
            };
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
                TaikinJikan1 = "24:00",
                TaikinJikan2 = "24:00",
                TaikinJikan3 = "24:00",
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
        ///     テストケース  出退勤検索-54：日報実績・退出時間が00:00の場合に24:00を返す
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_日報実績退出時間0時_24時を返す()
        {
            // Arrange
            var loginSyain = InitializeTestData_OnPostSearchAsync();

            // 日報データ登録
            var nippou = new Nippou
            {
                Id = 1,
                SyainId = 1,
                SyukkinHm1 = TimeOnly.Parse("08:30"),
                TaisyutsuHm1 = TimeOnly.Parse("00:00"),
                SyukkinHm2 = TimeOnly.Parse("13:00"),
                TaisyutsuHm2 = TimeOnly.Parse("00:00"),
                SyukkinHm3 = TimeOnly.Parse("19:00"),
                TaisyutsuHm3 = TimeOnly.Parse("00:00"),
                NippouYmd = DateOnly.Parse("2026/1/5"),
                TourokuKubun = 確定保存,
                SyukkinKubunId1 = 2,
            };
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
                Taisyutsu1 = "24:00",
                Taisyutsu2 = "24:00",
                Taisyutsu3 = "24:00",
                SyukkinKubunList = new List<string> { "通常勤務" },
            };
            AssertKinmuDataNippou(nippouClass, kinmuList[0].GetNippouData(loginUser));
        }
    }
}
