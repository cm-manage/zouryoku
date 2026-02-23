using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Model.Enums;
using Model.Model;

namespace ZouryokuTest.Pages.DakokuJikanSyusei
{
    [TestClass]
    public class IndexModelOnGetAsyncTests : IndexModelTestBase
    {
        // =============================================
        // 正常系テストケース
        // =============================================

        // ==================================================================================
        // 初期表示
        //      パラメータ.社員ID、パラメータ.実績年月日と一致する勤怠打刻情報が存在する場合
        // ==================================================================================
        [TestMethod(DisplayName = "パラメータ.社員ID、パラメータ.実績年月日と一致する勤怠打刻情報が存在する場合 → PageResultと勤怠打刻情報が返却される")]
        public async Task OnGetAsync_パラメータの社員IDとパラメータの実績年月日と一致する勤怠打刻情報が存在する_PageResultと勤怠打刻情報を返却()
        {
            // ================ Arrange ================ //
            var baseDate = new DateOnly(2025, 04, 01);

            // 勤怠打刻情報の登録
            var kintai = new WorkingHour
            {
                SyainId = 1,
                Hiduke = baseDate,
                SyukkinLatitude = 0,
                SyukkinLongitude = 0,
                TaikinLatitude = 0,
                TaikinLongitude = 0,
                SyukkinTime = new DateTime(2025, 4, 1, 9, 0, 0),
                TaikinTime = new DateTime(2025, 4, 1, 18, 0, 0),
                Edited = false,
                Deleted = false,
            };

            // 社員情報の登録
            var syain = new Syain()
            {
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
                KanaName = LoggedInUserName,
                Seibetsu = (char)1,
                BusyoCode = "100",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = true,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = LoggedInUserId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

            // データの登録
            SeedEntities(kintai, syain);

            var model = CreateModel();

            // ================ Act ================ //
            var result = await model.OnGetAsync(kintai.SyainId, baseDate);

            // ================ Assert ================ //
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.AreEqual(9, model.ViewModel.TimeSets[0].Start.Hour);
            Assert.AreEqual(0, model.ViewModel.TimeSets[0].Start.Minute);
            Assert.AreEqual(18, model.ViewModel.TimeSets[0].End.Hour);
            Assert.AreEqual(0, model.ViewModel.TimeSets[0].End.Minute);
        }

        // ==================================================================================
        // 初期表示
        //      パラメータ.社員ID、パラメータ.実績年月日と一致する勤怠打刻情報が存在しない場合
        // ==================================================================================
        [TestMethod(DisplayName = "パラメータ.社員ID、パラメータ.実績年月日と一致する勤怠打刻情報が存在しない場合 → PageResultが返却される")]
        public async Task OnGetAsync_パラメータの社員IDとパラメータの実績年月日と一致する勤怠打刻情報が存在しない_PageResultを返却()
        {
            // ================ Arrange ================ //
            var baseDate = new DateOnly(2025, 04, 01);

            var model = CreateModel();

            // ================ Act ================ //
            var result = await model.OnGetAsync(1, baseDate);

            // ================ Assert ================ //
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsNull(model.ViewModel.TimeSets[0].Start.Hour);
            Assert.IsNull(model.ViewModel.TimeSets[1].Start.Hour);
            Assert.IsNull(model.ViewModel.TimeSets[2].Start.Hour);
            Assert.IsNull(model.ViewModel.TimeSets[0].End.Hour);
            Assert.IsNull(model.ViewModel.TimeSets[1].End.Hour);
            Assert.IsNull(model.ViewModel.TimeSets[2].End.Hour);
            Assert.IsNull(model.ViewModel.TimeSets[0].Start.Minute);
            Assert.IsNull(model.ViewModel.TimeSets[1].Start.Minute);
            Assert.IsNull(model.ViewModel.TimeSets[2].Start.Minute);
            Assert.IsNull(model.ViewModel.TimeSets[0].End.Minute);
            Assert.IsNull(model.ViewModel.TimeSets[1].End.Minute);
            Assert.IsNull(model.ViewModel.TimeSets[2].End.Minute);
        }

        // ==================================================================================
        // 勤怠打刻情報を取得
        //      パラメータ.社員ID、パラメータ.実績年月日と一致する勤怠打刻情報が存在する場合
        // ==================================================================================
        [DataRow(1, DisplayName = "Deletedがfalseの勤怠打刻情報が1件 → 勤怠打刻情報が取得される")]
        [DataRow(2, DisplayName = "Deletedがfalseの勤怠打刻情報が2件 → 勤怠打刻情報が取得される")]
        [DataRow(3, DisplayName = "Deletedがfalseの勤怠打刻情報が3件 → 勤怠打刻情報が取得される")]
        [TestMethod]
        public async Task OnGetAsync_パラメータと一致する勤怠打刻情報が存在する_勤怠打刻情報を取得(
            int notDeletedCount)
        { 
            // ================ Arrange ================ //
            var baseDate = new DateOnly(2025, 4, 1);
            var syainId = 1;

            var baseExpectedSyukkinTime = new DateTime(2025, 4, 1, 9, 0, 0);
            var expectedSyukkinTimes = new List<DateTime>();

            var baseExpectedTaikinTime = new DateTime(2025, 4, 1, 9, 30, 0);
            var expectedTaikinTimes = new List<DateTime>();

            // 社員情報の登録
            var syain = new Syain()
            {
                Id = syainId,
                Code = "100",
                Name = "社員A",
                KanaName = "シャインエー",
                Seibetsu = (char)1,
                BusyoCode = "100",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = BusinessTripRole._2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = true,
                Kengen = EmployeeAuthority.None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = 1,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

            // 日報実績の登録
            var nippou = new Nippou()
            {
                SyainId = syainId,
                NippouYmd = baseDate,
                Youbi = 1,
                KaisyaCode = NippousCompanyCode.協和,
                IsRendouZumi = true,
                TourokuKubun = DailyReportStatusClassification.一時保存,
                SyukkinKubunId1 = 1,
            };

            // 未削除の勤怠打刻情報を登録
            var notDeletedKintais = new List<WorkingHour>();

            for (int i = 0; i < notDeletedCount; i++)
            {
                expectedSyukkinTimes.Add(baseExpectedSyukkinTime.AddHours(i));
                expectedTaikinTimes.Add(baseExpectedTaikinTime.AddHours(i + 1));
                notDeletedKintais.Add(new WorkingHour()
                {
                    SyainId = syainId,
                    Hiduke = baseDate,
                    SyukkinLatitude = 0,
                    SyukkinLongitude = 0,
                    TaikinLatitude = 0,
                    TaikinLongitude = 0,
                    SyukkinTime = expectedSyukkinTimes[i],
                    TaikinTime = expectedTaikinTimes[i],
                    Edited = false,
                    Deleted = false,
                });
            }

            SeedEntities(syain, nippou, notDeletedKintais);

            var model = CreateModel();

            // ================ Act ================ //
            await model.OnGetAsync(syainId, baseDate);

            // ================ Assert ================ //
            Assert.AreEqual(syainId, model.ViewModel.SyainId);
            Assert.AreEqual(baseDate, model.ViewModel.JissekiDate);
            Assert.IsNull(model.ViewModel.UkagaiHeaderId);
            for (int i = 0; i < notDeletedCount; i++)
            {
                Assert.AreEqual(expectedSyukkinTimes[i].ToTimeOnly(), model.ViewModel.TimeSets[i].Start.AsTimeOnly);
                Assert.AreEqual(expectedTaikinTimes[i].ToTimeOnly(), model.ViewModel.TimeSets[i].End.AsTimeOnly);
            }
            Assert.IsEmpty(model.ViewModel.DeletedTimeSets);
            Assert.AreEqual(string.Empty, model.ViewModel.SyuseiReason);
            Assert.IsNull(model.ViewModel.UkagaiHeaderVersion);
            Assert.IsEmpty(model.ViewModel.UkagaiShinseiVersions);
            Assert.IsFalse(model.ViewModel.IsKakutei);
            Assert.AreEqual(DailyReportStatusClassification.一時保存, model.ViewModel.TorokuKubun);
            Assert.IsNotEmpty(model.ViewModel.TimeSets);
        }

        [DataRow(1, 1, DisplayName = "Deletedがfalseの勤怠打刻情報が1件、Deletedがtrueの勤怠打刻情報が1件 → 勤怠打刻情報が取得される")]
        [DataRow(1, 2, DisplayName = "Deletedがfalseの勤怠打刻情報が1件、Deletedがtrueの勤怠打刻情報が2件 → 勤怠打刻情報が取得される")]
        [DataRow(1, 3, DisplayName = "Deletedがfalseの勤怠打刻情報が1件、Deletedがtrueの勤怠打刻情報が3件 → 勤怠打刻情報が取得される")]
        [DataRow(2, 1, DisplayName = "Deletedがfalseの勤怠打刻情報が2件、Deletedがtrueの勤怠打刻情報が1件 → 勤怠打刻情報が取得される")]
        [DataRow(2, 2, DisplayName = "Deletedがfalseの勤怠打刻情報が2件、Deletedがtrueの勤怠打刻情報が2件 → 勤怠打刻情報が取得される")]
        [DataRow(2, 3, DisplayName = "Deletedがfalseの勤怠打刻情報が2件、Deletedがtrueの勤怠打刻情報が3件 → 勤怠打刻情報が取得される")]
        [DataRow(3, 1, DisplayName = "Deletedがfalseの勤怠打刻情報が3件、Deletedがtrueの勤怠打刻情報が1件 → 勤怠打刻情報が取得される")]
        [DataRow(3, 2, DisplayName = "Deletedがfalseの勤怠打刻情報が3件、Deletedがtrueの勤怠打刻情報が2件 → 勤怠打刻情報が取得される")]
        [DataRow(3, 3, DisplayName = "Deletedがfalseの勤怠打刻情報が3件、Deletedがtrueの勤怠打刻情報が3件 → 勤怠打刻情報が取得される")]
        [TestMethod]
        public async Task OnGetAsync_パラメータと一致する勤怠打刻情報が複数件存在する_勤怠打刻情報を取得(
            int notDeletedCount,
            int deletedCount)
        {
            // ================ Arrange ================ //
            var baseDate = new DateOnly(2025, 4, 1);
            var syainId = 1;

            var baseExpectedSyukkinTime = new DateTime(2025, 4, 1, 9, 0, 0);
            var expectedSyukkinTimes = new List<DateTime>();

            var baseExpectedTaikinTime = new DateTime(2025, 4, 1, 9, 30, 0);
            var expectedTaikinTimes = new List<DateTime>();

            // 社員情報の登録
            var syain = new Syain()
            {
                Id = syainId,
                Code = "100",
                Name = "社員A",
                KanaName = "シャインエー",
                Seibetsu = (char)1,
                BusyoCode = "100",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = BusinessTripRole._2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = true,
                Kengen = EmployeeAuthority.None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = 1,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

            // 日報実績の登録
            var nippou = new Nippou()
            {
                SyainId = syainId,
                NippouYmd = baseDate,
                Youbi = 1,
                KaisyaCode = NippousCompanyCode.協和,
                IsRendouZumi = true,
                TourokuKubun = DailyReportStatusClassification.一時保存,
                SyukkinKubunId1 = 1,
            };

            // 伺い入力ヘッダを登録
            var ukagaiHeader = new UkagaiHeader()
            {
                Id = 1,
                SyainId = syainId,
                ShinseiYmd = fakeTimeProvider.Now().ToDateOnly(),
                Status = 0,
                WorkYmd = baseDate,
                Biko = "備考",
                Invalid = false,
            };

            // 伺い申請を登録
            var ukagaiShinsei = new UkagaiShinsei()
            {
                UkagaiHeader = ukagaiHeader,
                UkagaiSyubetsu = InquiryType.打刻時間修正,
            };

            // 未削除の勤怠打刻情報を登録
            var notDeletedKintais = new List<WorkingHour>();

            for (int i = 0; i < notDeletedCount; i++)
            {
                expectedSyukkinTimes.Add(baseExpectedSyukkinTime.AddHours(i));
                expectedTaikinTimes.Add(baseExpectedTaikinTime.AddHours(i + 1));
                notDeletedKintais.Add(new WorkingHour()
                {
                    SyainId = syainId,
                    Hiduke = baseDate,
                    SyukkinLatitude = 0,
                    SyukkinLongitude = 0,
                    TaikinLatitude = 0,
                    TaikinLongitude = 0,
                    SyukkinTime = expectedSyukkinTimes[i],
                    TaikinTime = expectedTaikinTimes[i],
                    Edited = false,
                    Deleted = false,
                    UkagaiHeader = ukagaiHeader,
                });
            }

            // 削除済みの勤怠打刻情報を登録
            var deletedKintais = new List<WorkingHour>();

            for (int i = 0; i < deletedCount; i++)
            {
                expectedSyukkinTimes.Add(baseExpectedSyukkinTime.AddHours(i));
                expectedTaikinTimes.Add(baseExpectedTaikinTime.AddHours(i + 1));
                deletedKintais.Add(new WorkingHour()
                {
                    SyainId = syainId,
                    Hiduke = baseDate,
                    SyukkinLatitude = 0,
                    SyukkinLongitude = 0,
                    TaikinLatitude = 0,
                    TaikinLongitude = 0,
                    SyukkinTime = expectedSyukkinTimes[i],
                    TaikinTime = expectedTaikinTimes[i],
                    Edited = false,
                    Deleted = true,
                    UkagaiHeader = ukagaiHeader,
                });
            }

            SeedEntities(syain, nippou, ukagaiHeader, ukagaiShinsei, notDeletedKintais, deletedKintais);

            var model = CreateModel();

            // ================ Act ================ //
            await model.OnGetAsync(syainId, baseDate);

            // ================ Assert ================ //
            Assert.AreEqual(syainId, model.ViewModel.SyainId);
            Assert.AreEqual(baseDate, model.ViewModel.JissekiDate);
            Assert.AreEqual(1, model.ViewModel.UkagaiHeaderId);
            for (int i = 0; i < notDeletedCount; i++)
            {
                Assert.AreEqual(expectedSyukkinTimes[i].ToTimeOnly(), model.ViewModel.TimeSets[i].Start.AsTimeOnly);
                Assert.AreEqual(expectedTaikinTimes[i].ToTimeOnly(), model.ViewModel.TimeSets[i].End.AsTimeOnly);
            }
            Assert.HasCount(deletedCount, model.ViewModel.DeletedTimeSets);
            Assert.AreEqual("備考", model.ViewModel.SyuseiReason);
            var ukagaiHead = await db.UkagaiHeaders.FirstOrDefaultAsync();
            Assert.IsNotNull(ukagaiHead);
            Assert.AreEqual(ukagaiHead.Version, model.ViewModel.UkagaiHeaderVersion);
            Assert.IsNotEmpty(model.ViewModel.UkagaiShinseiVersions);
            Assert.IsFalse(model.ViewModel.IsKakutei);
            Assert.AreEqual(DailyReportStatusClassification.一時保存, model.ViewModel.TorokuKubun);
            Assert.IsNotEmpty(model.ViewModel.TimeSets);
        }

        // =============================================
        // 異常系テストケース
        // =============================================

        // ==================================================================================
        // 勤怠打刻情報を取得
        // ==================================================================================
        [DataRow(2, 0, DisplayName = "パラメータの社員IDと一致する勤怠打刻情報が存在しない → 勤怠打刻情報を取得しない")]
        [DataRow(1, 1, DisplayName = "パラメータの実績日付と一致する勤怠打刻情報が存在しない → 勤怠打刻情報を取得しない")]
        [TestMethod]
        public async Task OnGetAsync_パラメータと一致する勤怠打刻情報が存在しない_勤怠打刻情報を取得しない(
            long syainId,
            int addJissekiDate)
        {
            // ================ Arrange ================ //
            var baseDate = new DateOnly(2025, 4, 1);

            // 社員情報の登録
            var syain = new Syain()
            {
                Id = syainId,
                Code = "100",
                Name = "社員A",
                KanaName = "シャインエー",
                Seibetsu = (char)1,
                BusyoCode = "100",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = BusinessTripRole._2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = true,
                Kengen = EmployeeAuthority.None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = 1,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

            // 日報実績の登録
            var nippou = new Nippou()
            {
                SyainId = syainId,
                NippouYmd = baseDate,
                Youbi = 1,
                KaisyaCode = NippousCompanyCode.協和,
                IsRendouZumi = true,
                TourokuKubun = DailyReportStatusClassification.一時保存,
                SyukkinKubunId1 = 1,
            };

            // 伺い入力ヘッダを登録
            var ukagaiHeader = new UkagaiHeader()
            {
                Id = 1,
                SyainId = syainId,
                ShinseiYmd = fakeTimeProvider.Now().ToDateOnly(),
                Status = 0,
                WorkYmd = baseDate,
                Biko = "備考",
                Invalid = false,
            };

            // 伺い申請を登録
            var ukagaiShinsei = new UkagaiShinsei()
            {
                UkagaiHeader = ukagaiHeader,
                UkagaiSyubetsu = InquiryType.打刻時間修正,
            };

            // 未削除の勤怠打刻情報を登録
            var notDeletedKintai = new WorkingHour()
            {
                SyainId = 1,
                Hiduke = baseDate.AddDays(addJissekiDate),
                SyukkinLatitude = 0,
                SyukkinLongitude = 0,
                TaikinLatitude = 0,
                TaikinLongitude = 0,
                SyukkinTime = new DateTime(2025, 4, 1, 9, 0, 0),
                TaikinTime = new DateTime(2025, 4, 1, 18, 0, 0),
                Edited = false,
                Deleted = false,
                UkagaiHeader = ukagaiHeader,
            };

            SeedEntities(syain, nippou, ukagaiHeader, ukagaiShinsei, notDeletedKintai);

            var model = CreateModel();

            // ================ Act ================ //
            var result = await model.OnGetAsync(syainId, baseDate);

            // ================ Assert ================ //
            Assert.AreEqual(syainId, model.ViewModel.SyainId);
            Assert.AreEqual(baseDate, model.ViewModel.JissekiDate);
            Assert.IsNull(model.ViewModel.UkagaiHeaderId);
            Assert.IsEmpty(model.ViewModel.DeletedTimeSets);
            Assert.IsEmpty(model.ViewModel.SyuseiReason);
            Assert.IsEmpty(model.ViewModel.UkagaiShinseiVersions);
            Assert.IsNull(model.ViewModel.UkagaiHeaderVersion);
            Assert.IsFalse(model.ViewModel.IsKakutei);
            Assert.IsNull(model.ViewModel.TorokuKubun);
        }

        [TestMethod(DisplayName = "勤怠打刻情報の紐づいた伺い入力ヘッダ伺い申請が存在しない → 伺い入力ヘッダと伺い申請情報が取得されない")]
        public async Task OnGetAsync_勤怠打刻情報の紐づいた伺い入力ヘッダ伺い申請が存在しない_伺い入力ヘッダと伺い申請情報が取得されない()
        {
            // ================ Arrange ================ //
            var baseDate = new DateOnly(2025, 4, 1);
            var syainId = 1;
            
            // 社員情報の登録
            var syain = new Syain()
            {
                Id = syainId,
                Code = "100",
                Name = "社員A",
                KanaName = "シャインエー",
                Seibetsu = (char)1,
                BusyoCode = "100",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = BusinessTripRole._2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = true,
                Kengen = EmployeeAuthority.None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = 1,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

            // 日報実績の登録
            var nippou = new Nippou()
            {
                SyainId = syainId,
                NippouYmd = baseDate,
                Youbi = 1,
                KaisyaCode = NippousCompanyCode.協和,
                IsRendouZumi = true,
                TourokuKubun = DailyReportStatusClassification.一時保存,
                SyukkinKubunId1 = 1,
            };

            // 未削除の勤怠打刻情報を登録
            var notDeletedKintai = new WorkingHour()
            {
                SyainId = syainId,
                Hiduke = baseDate,
                SyukkinLatitude = 0,
                SyukkinLongitude = 0,
                TaikinLatitude = 0,
                TaikinLongitude = 0,
                SyukkinTime = new DateTime(2025, 4, 1, 9, 0, 0),
                TaikinTime = new DateTime(2025, 4, 1, 18, 0, 0),
                Edited = false,
                Deleted = false,
            };

            SeedEntities(syain, nippou, notDeletedKintai);

            var model = CreateModel();

            // ================ Act ================ //
            var result = await model.OnGetAsync(syainId, baseDate);

            // ================ Assert ================ //
            Assert.IsNull(model.ViewModel.UkagaiHeaderId);
            Assert.IsEmpty(model.ViewModel.SyuseiReason);
            Assert.IsEmpty(model.ViewModel.UkagaiShinseiVersions);
            Assert.IsNull(model.ViewModel.UkagaiHeaderVersion);
            Assert.IsFalse(model.ViewModel.IsKakutei);
            Assert.AreEqual(DailyReportStatusClassification.一時保存, model.ViewModel.TorokuKubun);
        }

        [TestMethod(DisplayName = "勤怠打刻情報に紐づく日報実績情報が存在しない → 日報実績情報を取得しない")]
        public async Task OnGetAsync_勤怠打刻情報に紐づく日報実績情報が存在しない_日報実績情報を取得しない()
        {
            // ================ Arrange ================ //
            var baseDate = new DateOnly(2025, 4, 1);
            var syainId = 1;

            // 社員情報の登録
            var syain = new Syain()
            {
                Id = syainId,
                Code = "100",
                Name = "社員A",
                KanaName = "シャインエー",
                Seibetsu = (char)1,
                BusyoCode = "100",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = BusinessTripRole._2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = true,
                Kengen = EmployeeAuthority.None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = 1,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

            // 伺い入力ヘッダを登録
            var ukagaiHeader = new UkagaiHeader()
            {
                Id = 1,
                SyainId = syainId,
                ShinseiYmd = fakeTimeProvider.Now().ToDateOnly(),
                Status = 0,
                WorkYmd = baseDate,
                Biko = "備考",
                Invalid = false,
            };

            // 伺い申請を登録
            var ukagaiShinsei = new UkagaiShinsei()
            {
                UkagaiHeader = ukagaiHeader,
                UkagaiSyubetsu = InquiryType.打刻時間修正,
            };

            // 未削除の勤怠打刻情報を登録
            var notDeletedKintai = new WorkingHour()
            {
                SyainId = 1,
                Hiduke = baseDate,
                SyukkinLatitude = 0,
                SyukkinLongitude = 0,
                TaikinLatitude = 0,
                TaikinLongitude = 0,
                SyukkinTime = new DateTime(2025, 4, 1, 9, 0, 0),
                TaikinTime = new DateTime(2025, 4, 1, 18, 0, 0),
                Edited = false,
                Deleted = false,
                UkagaiHeader = ukagaiHeader,
            };

            SeedEntities(syain, ukagaiHeader, ukagaiShinsei, notDeletedKintai);

            await db.SaveChangesAsync();

            var model = CreateModel();

            // ================ Act ================ //
            var result = await model.OnGetAsync(syainId, baseDate);

            // ================ Assert ================ //
            Assert.AreEqual(syainId, model.ViewModel.SyainId);
            Assert.AreEqual(baseDate, model.ViewModel.JissekiDate);
            Assert.AreEqual(1, model.ViewModel.UkagaiHeaderId);
            Assert.IsNotEmpty(model.ViewModel.TimeSets);
            Assert.IsEmpty(model.ViewModel.DeletedTimeSets);
            Assert.AreEqual("備考", model.ViewModel.SyuseiReason);
            Assert.IsNotEmpty(model.ViewModel.UkagaiShinseiVersions);
            Assert.IsNotNull(model.ViewModel.UkagaiHeaderVersion);
            Assert.IsFalse(model.ViewModel.IsKakutei);
            Assert.IsNull(model.ViewModel.TorokuKubun);
        }
    }
}
