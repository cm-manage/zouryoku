using Azure;
using CommonLibrary.Extensions;
using LanguageExt.Pipes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Utils;
using static Model.Enums.ApprovalStatus;
using static Model.Enums.DailyReportStatusClassification;
using static Model.Enums.EmployeeWorkType;
using static Model.Enums.InquiryType;
using static Model.Enums.NippousCompanyCode;

namespace ZouryokuTest.Pages.DakokuJikanSyusei
{
    [TestClass]
    public class IndexModelOnPostRegisterAsyncTests : IndexModelTestBase
    {
        // =============================================
        // 正常系テストケース
        // =============================================

        // =============================================
        // 入力チェック
        // =============================================
        [DataRow(9, 0, 18, 0, DisplayName = "出退勤1の入力値が正常 → IsValidがtrueで返却される")]
        [DataRow(null, null, 18, 0, DisplayName = "出勤時間1が全て未入力の場合 → IsValidがtrueで返却される")]
        [DataRow(9, 0, null, null, DisplayName = "退勤時間1が全て未入力の場合 → IsValidがtrueで返却される")]
        [DataRow(0, 0, 18, 0, DisplayName = "出勤時間1が00:00で入力された場合 → IsValidがtrueで返却される")]
        [DataRow(9, 0, 0, 0, DisplayName = "退勤時間1が00:00で入力された場合 → IsValidがtrueで返却される")]
        [TestMethod]
        public async Task OnPostRegisterAsync_出退勤1の入力値が正常_IsValidがtrueで返却(
            int? syukkinHour,
            int? syukkinMinute,
            int? taikinHour,
            int? taikinMinute)
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            model.ViewModel.TimeSets[0].Start.Hour = syukkinHour;
            model.ViewModel.TimeSets[0].Start.Minute = syukkinMinute;
            model.ViewModel.TimeSets[0].End.Hour = taikinHour;
            model.ViewModel.TimeSets[0].End.Minute = taikinMinute;

            await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsTrue(model.ModelState.IsValid);
        }

        [DataRow(12, 0, 16, 0, DisplayName = "出退勤2の入力値が正常 → IsValidがtrueで返却される")]
        [DataRow(12, 0, null, null, DisplayName = "退勤時間2が全て未入力の場合 → IsValidがtrueで返却される")]
        [DataRow(12, 0, 0, 0, DisplayName = "退勤時間2が00:00で入力された場合 → IsValidがtrueで返却される")]
        [TestMethod]
        public async Task OnPostRegisterAsync_出退勤2の入力値が正常_IsValidがtrueで返却(
            int? syukkinHour,
            int? syukkinMinute,
            int? taikinHour,
            int? taikinMinute)
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            model.ViewModel.JissekiDate = new DateOnly(2025, 4, 1);
            // 出退勤1
            model.ViewModel.TimeSets[0].Start.Hour = 9;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = 11;
            model.ViewModel.TimeSets[0].End.Minute = 0;

            // 出退勤2
            model.ViewModel.TimeSets[1].Start.Hour = syukkinHour;
            model.ViewModel.TimeSets[1].Start.Minute = syukkinMinute;
            model.ViewModel.TimeSets[1].End.Hour = taikinHour;
            model.ViewModel.TimeSets[1].End.Minute = taikinMinute;

            await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsTrue(model.ModelState.IsValid);
        }

        [DataRow(17, 0, 21, 0, DisplayName = "出退勤3の入力値が正常 → IsValidがtrueで返却される")]
        [DataRow(17, 0, null, null, DisplayName = "退勤時間3が全て未入力の場合 → IsValidがtrueで返却される")]
        [DataRow(17, 0, 0, 0, DisplayName = "退勤時間3が00:00で入力された場合 → IsValidがtrueで返却される")]
        [TestMethod]
        public async Task OnPostRegisterAsync_出退勤3の入力値が正常_IsValidがtrueで返却(
            int? syukkinHour,
            int? syukkinMinute,
            int? taikinHour,
            int? taikinMinute)
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            // 出退勤1
            model.ViewModel.TimeSets[0].Start.Hour = 9;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = 11;
            model.ViewModel.TimeSets[0].End.Minute = 0;

            // 出退勤2
            model.ViewModel.TimeSets[1].Start.Hour = 12;
            model.ViewModel.TimeSets[1].Start.Minute = 0;
            model.ViewModel.TimeSets[1].End.Hour = 16;
            model.ViewModel.TimeSets[1].End.Minute = 0;

            // 出退勤3
            model.ViewModel.TimeSets[2].Start.Hour = syukkinHour;
            model.ViewModel.TimeSets[2].Start.Minute = syukkinMinute;
            model.ViewModel.TimeSets[2].End.Hour = taikinHour;
            model.ViewModel.TimeSets[2].End.Minute = taikinMinute;

            await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsTrue(model.ModelState.IsValid);
        }

        // =============================================
        // DB更新処理
        // =============================================

        [DataRow(1, DisplayName = "打刻漏れ修正時に１件入力 → 入力値が登録される")]
        [DataRow(2, DisplayName = "打刻漏れ修正時に２件入力 → 入力値が登録される")]
        [DataRow(3, DisplayName = "打刻漏れ修正時に３件入力 → 入力値が登録される")]
        [TestMethod]
        public async Task OnPostRegisterAsync_打刻漏れ修正時の入力値が正常_勤怠打刻情報を登録(
            int kintaiInputCount)
        {
            // ================ Arrange ================ //
            var baseDate = new DateOnly(2026, 7, 1);
            var now = new DateTime(2026, 7, 2, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);

            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = パート,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            // 日報実績の登録
            var nippou = new Nippou()
            {
                Syain = syain,
                NippouYmd = baseDate,
                Youbi = 1,
                KaisyaCode = 協和,
                IsRendouZumi = true,
                TourokuKubun = 一時保存,
                SyukkinKubunId1 = 1,
            };

            SeedEntities(syainBase, syain, nippou, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            await model.OnGetAsync(syain.Id, baseDate);

            // 入力値の設定
            List<DateTime> syukkinTimeList = new List<DateTime>();
            List<DateTime> taikinTimeList = new List<DateTime>();

            model.ViewModel.TimeSets[0].Start.Hour = 8;
            model.ViewModel.TimeSets[0].Start.Minute = 30;
            model.ViewModel.TimeSets[0].End.Hour = 10;
            model.ViewModel.TimeSets[0].End.Minute = 00;

            syukkinTimeList.Add(baseDate.ToDateTime(new TimeOnly(8, 30, 0)));

            taikinTimeList.Add(baseDate.ToDateTime(new TimeOnly(10, 0, 0)));

            if (1 < kintaiInputCount)
            {
                model.ViewModel.TimeSets[1].Start.Hour = 11;
                model.ViewModel.TimeSets[1].Start.Minute = 0;
                model.ViewModel.TimeSets[1].End.Hour = 12;
                model.ViewModel.TimeSets[1].End.Minute = 0;

                syukkinTimeList.Add(baseDate.ToDateTime(new TimeOnly(11, 0, 0)));

                taikinTimeList.Add(baseDate.ToDateTime(new TimeOnly(12, 0, 0)));
            }

            if (2 < kintaiInputCount)
            {
                model.ViewModel.TimeSets[2].Start.Hour = 13;
                model.ViewModel.TimeSets[2].Start.Minute = 0;
                model.ViewModel.TimeSets[2].End.Hour = 21;
                model.ViewModel.TimeSets[2].End.Minute = 0;

                syukkinTimeList.Add(baseDate.ToDateTime(new TimeOnly(13, 0, 0)));

                taikinTimeList.Add(baseDate.ToDateTime(new TimeOnly(21, 0, 0)));
            }

            await model.OnPostRegisterAsync();

            // 更新後の情報を取得
            var targetDeletedWorkingHour = await db.WorkingHours
                .Where(x => x.Deleted)
                .OrderBy(x => x.SyukkinTime)
                .ToListAsync();

            var targetNotDeletedWorkingHour = await db.WorkingHours
                .Where(x => !x.Deleted)
                .OrderBy(x => x.SyukkinTime)
                .ToListAsync();

            var targetUkagaiHeader = await db.UkagaiHeaders
                .FirstOrDefaultAsync();

            var targetUkagaiShinsei = await db.UkagaiShinseis
                .ToListAsync();

            // ================ Assert ================ //
            Assert.IsEmpty(targetDeletedWorkingHour);
            Assert.IsNotEmpty(targetNotDeletedWorkingHour);
            Assert.IsNotNull(targetUkagaiHeader);
            Assert.IsNotEmpty(targetUkagaiShinsei);
            Assert.AreEqual(syain.Id, targetUkagaiHeader.SyainId);
            Assert.AreEqual(now.ToDateOnly(), targetUkagaiHeader.ShinseiYmd);
            Assert.AreEqual(承認, targetUkagaiHeader.Status);
            Assert.AreEqual(baseDate, targetUkagaiHeader.WorkYmd);
            Assert.AreEqual(model.ViewModel.SyuseiReason, targetUkagaiHeader.Biko);
            Assert.IsFalse(targetUkagaiHeader.Invalid);
            Assert.HasCount(1, targetUkagaiShinsei);
            Assert.AreEqual(targetUkagaiHeader.Id, targetUkagaiShinsei[0].UkagaiHeaderId);
            Assert.AreEqual(打刻時間修正, targetUkagaiShinsei[0].UkagaiSyubetsu);
            Assert.HasCount(kintaiInputCount, targetNotDeletedWorkingHour);
            for (int i = 0; i < kintaiInputCount; i++)
            {
                Assert.AreEqual(model.ViewModel.SyainId, targetNotDeletedWorkingHour[i].SyainId);
                Assert.AreEqual(model.ViewModel.JissekiDate, targetNotDeletedWorkingHour[i].Hiduke);
                Assert.AreEqual(0, targetNotDeletedWorkingHour[i].SyukkinLatitude);
                Assert.AreEqual(0, targetNotDeletedWorkingHour[i].SyukkinLongitude);
                Assert.AreEqual(0, targetNotDeletedWorkingHour[i].TaikinLatitude);
                Assert.AreEqual(0, targetNotDeletedWorkingHour[i].TaikinLongitude);
                Assert.AreEqual(syukkinTimeList[i], targetNotDeletedWorkingHour[i].SyukkinTime);
                Assert.AreEqual(taikinTimeList[i], targetNotDeletedWorkingHour[i].TaikinTime);
                Assert.AreEqual(targetUkagaiHeader.Id, targetNotDeletedWorkingHour[i].UkagaiHeaderId);
                Assert.IsFalse(targetNotDeletedWorkingHour[i].Deleted);
                Assert.IsTrue(targetNotDeletedWorkingHour[i].Edited);
            }
        }

        [DataRow(1, 2, DisplayName = "初回修正時（件数増加） → 入力値が登録される")]
        [DataRow(3, 1, DisplayName = "初回修正時（件数減少） → 入力値が登録される")]
        [TestMethod]
        public async Task OnPostRegisterAsync_修正時の入力値が正常_勤怠打刻情報を登録(
            int notDeletedKintaiCount,
            int kintaiInputCount)
        {
            // ================ Arrange ================ //
            var baseDate = new DateOnly(2026, 7, 1);
            var now = new DateTime(2026, 7, 2, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);

            var baseExpectedSyukkinTime = new DateTime(2026, 7, 1, 9, 0, 0);
            var expectedSyukkinTimes = new List<DateTime>();

            var baseExpectedTaikinTime = new DateTime(2026, 7, 1, 9, 30, 0);
            var expectedTaikinTimes = new List<DateTime>();

            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            // 日報実績の登録
            var nippou = new Nippou()
            {
                Syain = syain,
                NippouYmd = baseDate,
                Youbi = 1,
                KaisyaCode = 協和,
                IsRendouZumi = true,
                TourokuKubun = 一時保存,
                SyukkinKubunId1 = 1,
            };

            SeedEntities(syainBase, syain, nippou, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // 未削除の勤怠打刻情報の登録
            var notDeletedKintais = new List<WorkingHour>();

            for (int i = 0; i < notDeletedKintaiCount; i++)
            {
                expectedSyukkinTimes.Add(baseExpectedSyukkinTime.AddHours(i));
                expectedTaikinTimes.Add(baseExpectedTaikinTime.AddHours(i + 1));
                notDeletedKintais.Add(new WorkingHour()
                {
                    SyainId = syain.Id,
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

            SeedEntities(notDeletedKintais);

            // ================ Act ================ //
            await model.OnGetAsync(syain.Id, baseDate);

            // 入力値の設定
            // 出退勤時間の設定
            List<DateTime> syukkinTimeList = new List<DateTime>();
            List<DateTime> taikinTimeList = new List<DateTime>();

            // 出退勤１
            model.ViewModel.TimeSets[0].Start.Hour = 8;
            model.ViewModel.TimeSets[0].Start.Minute = 30;
            model.ViewModel.TimeSets[0].End.Hour = 10;
            model.ViewModel.TimeSets[0].End.Minute = 00;
            syukkinTimeList.Add(baseDate.ToDateTime(new TimeOnly(8, 30, 0)));
            taikinTimeList.Add(baseDate.ToDateTime(new TimeOnly(10, 0, 0)));

            // 出退勤２
            if (1 < kintaiInputCount)
            {
                model.ViewModel.TimeSets[1].Start.Hour = 11;
                model.ViewModel.TimeSets[1].Start.Minute = 0;
                model.ViewModel.TimeSets[1].End.Hour = 12;
                model.ViewModel.TimeSets[1].End.Minute = 0;
                syukkinTimeList.Add(baseDate.ToDateTime(new TimeOnly(11, 0, 0)));
                taikinTimeList.Add(baseDate.ToDateTime(new TimeOnly(12, 0, 0)));
            }
            // 未入力の場合nullを設定
            else
            {
                model.ViewModel.TimeSets[1].Start.Hour = null;
                model.ViewModel.TimeSets[1].Start.Minute = null;
                model.ViewModel.TimeSets[1].End.Hour = null;
                model.ViewModel.TimeSets[1].End.Minute = null;
            }

            // 出退勤３
            //      未入力であるためnullを設定
            model.ViewModel.TimeSets[2].Start.Hour = null;
            model.ViewModel.TimeSets[2].Start.Minute = null;
            model.ViewModel.TimeSets[2].End.Hour = null;
            model.ViewModel.TimeSets[2].End.Minute = null;

            // 修正理由の設定
            model.ViewModel.SyuseiReason = "修正理由";

            await model.OnPostRegisterAsync();

            // 更新後の情報を取得
            var targetDeletedWorkingHour = await db.WorkingHours
                .Where(x => x.Deleted)
                .OrderBy(x => x.SyukkinTime)
                .ToListAsync();

            var targetNotDeletedWorkingHour = await db.WorkingHours
                .Where(x => !x.Deleted)
                .OrderBy(x => x.SyukkinTime)
                .ToListAsync();

            var targetUkagaiHeader = await db.UkagaiHeaders
                .FirstOrDefaultAsync();

            var targetUkagaiShinsei = await db.UkagaiShinseis
                .ToListAsync();

            // ================ Assert ================ //
            Assert.IsNotEmpty(targetDeletedWorkingHour);
            Assert.IsNotEmpty(targetNotDeletedWorkingHour);
            Assert.IsNotNull(targetUkagaiHeader);
            Assert.IsNotEmpty(targetUkagaiShinsei);
            Assert.HasCount(notDeletedKintaiCount, targetDeletedWorkingHour);
            for (int i = 0; i < notDeletedKintaiCount; i++)
            {
                Assert.IsTrue(targetDeletedWorkingHour[i].Deleted);
                Assert.AreEqual(targetUkagaiHeader.Id, targetDeletedWorkingHour[i].UkagaiHeaderId);
            }
            Assert.HasCount(kintaiInputCount, targetNotDeletedWorkingHour);
            for (int i = 0; i < kintaiInputCount; i++)
            {
                Assert.AreEqual(model.ViewModel.SyainId, targetNotDeletedWorkingHour[i].SyainId);
                Assert.AreEqual(model.ViewModel.JissekiDate, targetNotDeletedWorkingHour[i].Hiduke);
                Assert.AreEqual(0, targetNotDeletedWorkingHour[i].SyukkinLatitude);
                Assert.AreEqual(0, targetNotDeletedWorkingHour[i].SyukkinLongitude);
                Assert.AreEqual(0, targetNotDeletedWorkingHour[i].TaikinLatitude);
                Assert.AreEqual(0, targetNotDeletedWorkingHour[i].TaikinLongitude);
                Assert.AreEqual(syukkinTimeList[i], targetNotDeletedWorkingHour[i].SyukkinTime);
                Assert.AreEqual(taikinTimeList[i], targetNotDeletedWorkingHour[i].TaikinTime);
                Assert.AreEqual(targetUkagaiHeader.Id, targetNotDeletedWorkingHour[i].UkagaiHeaderId);
                Assert.IsFalse(targetNotDeletedWorkingHour[i].Deleted);
                Assert.IsTrue(targetNotDeletedWorkingHour[i].Edited);
            }
            Assert.AreEqual(now.ToDateOnly(), targetUkagaiHeader.ShinseiYmd);
            Assert.AreEqual(承認待, targetUkagaiHeader.Status);
            Assert.AreEqual(baseDate, targetUkagaiHeader.WorkYmd);
            Assert.AreEqual(model.ViewModel.SyuseiReason, targetUkagaiHeader.Biko);
            Assert.IsFalse(targetUkagaiHeader.Invalid);
            Assert.HasCount(1, targetUkagaiShinsei);
            Assert.AreEqual(targetUkagaiHeader.Id, targetUkagaiShinsei[0].UkagaiHeaderId);
            Assert.AreEqual(打刻時間修正, targetUkagaiShinsei[0].UkagaiSyubetsu);
        }

        [TestMethod(DisplayName = "２回目以降の修正時 → 入力値が登録される")]
        public async Task OnPostRegisterAsync_２回目以降の修正時_入力値を登録()
        {
            // ================ Arrange ================ //
            var baseDate = new DateOnly(2026, 7, 1);
            var now = new DateTime(2026, 7, 2, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);

            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            // 日報実績の登録
            var nippou = new Nippou()
            {
                Syain = syain,
                NippouYmd = baseDate,
                Youbi = 1,
                KaisyaCode = 協和,
                IsRendouZumi = true,
                TourokuKubun = 一時保存,
                SyukkinKubunId1 = 1,
            };

            SeedEntities(syainBase, syain, nippou, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // 伺い入力ヘッダ情報の登録
            var ukagaiHeader = new UkagaiHeader()
            {
                SyainId = syain.Id,
                ShinseiYmd = now.ToDateOnly(),
                Status = 0,
                WorkYmd = baseDate,
                Biko = "備考",
                Invalid = false,
                Version = 1, // ダミー
            };

            // 伺い申請の登録
            var ukagaiShinsei = new UkagaiShinsei()
            {
                UkagaiHeader = ukagaiHeader,
                UkagaiSyubetsu = 打刻時間修正,
                Version = 1, // ダミー
            };

            // 未削除の勤怠打刻情報の登録
            var notDeletedKintai = new WorkingHour()
            {
                SyainId = syain.Id,
                Hiduke = baseDate,
                SyukkinLatitude = 0,
                SyukkinLongitude = 0,
                TaikinLatitude = 0,
                TaikinLongitude = 0,
                SyukkinTime = baseDate.ToDateTime(new TimeOnly(9, 0, 0)),
                TaikinTime = baseDate.ToDateTime(new TimeOnly(18, 0, 0)),
                Edited = true,
                Deleted = false,
                UkagaiHeader = ukagaiHeader,
                Version = 1, // ダミー
            };

            // 削除済みの勤怠打刻情報の登録
            var deletedKintai = new WorkingHour()
            {
                SyainId = syain.Id,
                Hiduke = baseDate,
                SyukkinLatitude = 0,
                SyukkinLongitude = 0,
                TaikinLatitude = 0,
                TaikinLongitude = 0,
                SyukkinTime = baseDate.ToDateTime(new TimeOnly(15, 0, 0)),
                TaikinTime = baseDate.ToDateTime(new TimeOnly(21, 0, 0)),
                Edited = false,
                Deleted = true,
                UkagaiHeader = ukagaiHeader,
                Version = 1, // ダミー
            };

            SeedEntities(ukagaiHeader, ukagaiShinsei, notDeletedKintai, deletedKintai);

            // ================ Act ================ //
            await model.OnGetAsync(syain.Id, baseDate);

            // 入力値の設定
            // 出退勤時間の設定
            model.ViewModel.TimeSets[0].Start.Hour = 8;
            model.ViewModel.TimeSets[0].Start.Minute = 30;
            model.ViewModel.TimeSets[0].End.Hour = 10;
            model.ViewModel.TimeSets[0].End.Minute = 00;
            var syukkinTime = baseDate.ToDateTime(new TimeOnly(8, 30, 0));
            var taikinTime = baseDate.ToDateTime(new TimeOnly(10, 0, 0));

            // 修正理由の設定
            model.ViewModel.SyuseiReason = "修正理由";

            await model.OnPostRegisterAsync();

            // 更新後の情報を取得
            var afterDeletedWorkinHour = await db.WorkingHours
                .Where(x => x.Deleted)
                .OrderBy(x => x.SyukkinTime)
                .ToListAsync();

            var afterNotDeletedWorkinHour = await db.WorkingHours
                .Where(x => !x.Deleted)
                .OrderBy(x => x.SyukkinTime)
                .ToListAsync();

            var afterUkagaiHeader = await db.UkagaiHeaders
                .FirstOrDefaultAsync();

            var afterUkagaiShinsei = await db.UkagaiShinseis
                .ToListAsync();

            // ================ Assert ================ //
            Assert.IsNotEmpty(afterDeletedWorkinHour);
            Assert.IsNotEmpty(afterNotDeletedWorkinHour);
            Assert.IsNotNull(afterUkagaiHeader);
            Assert.IsNotEmpty(afterUkagaiShinsei);
            Assert.HasCount(1, afterDeletedWorkinHour);
            Assert.AreEqual(model.ViewModel.SyainId, afterDeletedWorkinHour[0].SyainId);
            Assert.AreEqual(model.ViewModel.JissekiDate, afterDeletedWorkinHour[0].Hiduke);
            Assert.AreEqual(0, afterDeletedWorkinHour[0].SyukkinLatitude);
            Assert.AreEqual(0, afterDeletedWorkinHour[0].SyukkinLongitude);
            Assert.AreEqual(0, afterDeletedWorkinHour[0].TaikinLatitude);
            Assert.AreEqual(0, afterDeletedWorkinHour[0].TaikinLongitude);
            Assert.AreEqual(new DateTime(2026, 7, 1, 15, 0, 0), afterDeletedWorkinHour[0].SyukkinTime);
            Assert.AreEqual(new DateTime(2026, 7, 1, 21, 0, 0), afterDeletedWorkinHour[0].TaikinTime);
            Assert.AreEqual(afterUkagaiHeader.Id, afterDeletedWorkinHour[0].UkagaiHeaderId);
            Assert.IsTrue(afterDeletedWorkinHour[0].Deleted);
            Assert.HasCount(1, afterNotDeletedWorkinHour);
            Assert.AreEqual(model.ViewModel.SyainId, afterNotDeletedWorkinHour[0].SyainId);
            Assert.AreEqual(model.ViewModel.JissekiDate, afterNotDeletedWorkinHour[0].Hiduke);
            Assert.AreEqual(0, afterNotDeletedWorkinHour[0].SyukkinLatitude);
            Assert.AreEqual(0, afterNotDeletedWorkinHour[0].SyukkinLongitude);
            Assert.AreEqual(0, afterNotDeletedWorkinHour[0].TaikinLatitude);
            Assert.AreEqual(0, afterNotDeletedWorkinHour[0].TaikinLongitude);
            Assert.AreEqual(syukkinTime, afterNotDeletedWorkinHour[0].SyukkinTime);
            Assert.AreEqual(taikinTime, afterNotDeletedWorkinHour[0].TaikinTime);
            Assert.AreEqual(afterUkagaiHeader.Id, afterNotDeletedWorkinHour[0].UkagaiHeaderId);
            Assert.IsFalse(afterNotDeletedWorkinHour[0].Deleted);
            Assert.AreEqual(now.ToDateOnly(), afterUkagaiHeader.ShinseiYmd);
            Assert.AreEqual(承認待, afterUkagaiHeader.Status);
            Assert.AreEqual(baseDate, afterUkagaiHeader.WorkYmd);
            Assert.AreEqual(model.ViewModel.SyuseiReason, afterUkagaiHeader.Biko);
            Assert.IsFalse(afterUkagaiHeader.Invalid);
            Assert.HasCount(1, afterUkagaiShinsei);
            Assert.AreEqual(afterUkagaiHeader.Id, afterUkagaiShinsei[0].UkagaiHeaderId);
            Assert.AreEqual(打刻時間修正, afterUkagaiShinsei[0].UkagaiSyubetsu);
        }

        [DataRow(null, null, 18, 0, DisplayName = "出勤時間が未入力 → 出勤時間がnullで登録される")]
        [DataRow(9, 0, null, null, DisplayName = "退勤時間が未入力 → 退勤時間がnullで登録される")]
        [TestMethod]
        public async Task OnPostRegisterAsync_入力値がnull_正常に登録(
            int? syukkinHour,
            int? syukkinMinute,
            int? taikinHour,
            int? taikinMinute)
        {
            // ================ Arrange ================ //
            var baseDate = new DateOnly(2026, 7, 1);
            var now = new DateTime(2026, 7, 2, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);

            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = パート,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            // 日報実績の登録
            var nippou = new Nippou()
            {
                Syain = syain,
                NippouYmd = baseDate,
                Youbi = 1,
                KaisyaCode = 協和,
                IsRendouZumi = true,
                TourokuKubun = 一時保存,
                SyukkinKubunId1 = 1,
            };

            SeedEntities(syainBase, syain, nippou, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            await model.OnGetAsync(syain.Id, baseDate);

            // 入力値の設定
            model.ViewModel.TimeSets[0].Start.Hour = syukkinHour;
            model.ViewModel.TimeSets[0].Start.Minute = syukkinMinute;
            model.ViewModel.TimeSets[0].End.Hour = taikinHour;
            model.ViewModel.TimeSets[0].End.Minute = taikinMinute;

            DateTime? syukkinTime = new DateTime();
            if (syukkinHour is not null && syukkinMinute is not null)
            {
                syukkinTime = baseDate.ToDateTime(new TimeOnly(syukkinHour.Value, syukkinMinute.Value, 0));
            }
            else
            {
                syukkinTime = null;
            }

            DateTime? taikinTime = new DateTime();
            if (taikinHour is not null && taikinMinute is not null)
            {
                taikinTime = baseDate.ToDateTime(new TimeOnly(taikinHour.Value, taikinMinute.Value, 0));
            }
            else
            {
                taikinTime = null;
            }

            await model.OnPostRegisterAsync();

            // 更新後の情報を取得
            var targetDeletedWorkingHour = await db.WorkingHours
                .Where(x => x.Deleted)
                .OrderBy(x => x.SyukkinTime)
                .ToListAsync();

            var targetNotDeletedWorkingHour = await db.WorkingHours
                .Where(x => !x.Deleted)
                .OrderBy(x => x.SyukkinTime)
                .ToListAsync();

            var targetUkagaiHeader = await db.UkagaiHeaders
                .FirstOrDefaultAsync();

            var targetUkagaiShinsei = await db.UkagaiShinseis
                .ToListAsync();

            // ================ Assert ================ //
            Assert.IsEmpty(targetDeletedWorkingHour);
            Assert.IsNotEmpty(targetNotDeletedWorkingHour);
            Assert.IsNotNull(targetUkagaiHeader);
            Assert.IsNotEmpty(targetUkagaiShinsei);
            Assert.AreEqual(syain.Id, targetUkagaiHeader.SyainId);
            Assert.AreEqual(now.ToDateOnly(), targetUkagaiHeader.ShinseiYmd);
            Assert.AreEqual(承認, targetUkagaiHeader.Status);
            Assert.AreEqual(baseDate, targetUkagaiHeader.WorkYmd);
            Assert.AreEqual(model.ViewModel.SyuseiReason, targetUkagaiHeader.Biko);
            Assert.IsFalse(targetUkagaiHeader.Invalid);
            Assert.HasCount(1, targetUkagaiShinsei);
            Assert.AreEqual(targetUkagaiHeader.Id, targetUkagaiShinsei[0].UkagaiHeaderId);
            Assert.AreEqual(打刻時間修正, targetUkagaiShinsei[0].UkagaiSyubetsu);
            Assert.HasCount(1, targetUkagaiShinsei);
            Assert.AreEqual(model.ViewModel.SyainId, targetNotDeletedWorkingHour[0].SyainId);
            Assert.AreEqual(model.ViewModel.JissekiDate, targetNotDeletedWorkingHour[0].Hiduke);
            Assert.AreEqual(0, targetNotDeletedWorkingHour[0].SyukkinLatitude);
            Assert.AreEqual(0, targetNotDeletedWorkingHour[0].SyukkinLongitude);
            Assert.AreEqual(0, targetNotDeletedWorkingHour[0].TaikinLatitude);
            Assert.AreEqual(0, targetNotDeletedWorkingHour[0].TaikinLongitude);
            Assert.AreEqual(syukkinTime, targetNotDeletedWorkingHour[0].SyukkinTime);
            Assert.AreEqual(taikinTime, targetNotDeletedWorkingHour[0].TaikinTime);
            Assert.AreEqual(targetUkagaiHeader.Id, targetNotDeletedWorkingHour[0].UkagaiHeaderId);
            Assert.IsFalse(targetNotDeletedWorkingHour[0].Deleted);
            Assert.IsTrue(targetNotDeletedWorkingHour[0].Edited);
        }

        [DataRow(0, 0, 18, 0, DisplayName = "出勤時間が00:00 → 出勤時間が00:00で登録される")]
        [DataRow(9, 0, 0, 0, DisplayName = "退勤時間が00:00 → 退勤時間が次の日の00:00で登録される")]
        [DataRow(0, 0, 0, 0, DisplayName = "出勤時間、退勤時間が00:00 → 出勤時間が00:00、退勤時間が次の日の00:00で登録される")]
        [TestMethod]
        public async Task OnPostRegisterAsync_入力値が0_正常に登録(
            int syukkinHour,
            int syukkinMinute,
            int taikinHour,
            int taikinMinute)
        {
            // ================ Arrange ================ //
            var baseDate = new DateOnly(2026, 7, 1);
            var now = new DateTime(2026, 7, 2, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);

            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = パート,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            // 日報実績の登録
            var nippou = new Nippou()
            {
                Syain = syain,
                NippouYmd = baseDate,
                Youbi = 1,
                KaisyaCode = 協和,
                IsRendouZumi = true,
                TourokuKubun = 一時保存,
                SyukkinKubunId1 = 1,
            };

            SeedEntities(syainBase, syain, nippou, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            await model.OnGetAsync(syain.Id, baseDate);

            // 入力値の設定
            model.ViewModel.TimeSets[0].Start.Hour = syukkinHour;
            model.ViewModel.TimeSets[0].Start.Minute = syukkinMinute;
            model.ViewModel.TimeSets[0].End.Hour = taikinHour;
            model.ViewModel.TimeSets[0].End.Minute = taikinMinute;

            DateTime syukkinTime = new DateTime();
            syukkinTime = baseDate.ToDateTime(new TimeOnly(syukkinHour, syukkinMinute, 0));
            

            DateTime taikinTime = new DateTime();
            if (taikinHour == 0 && taikinMinute == 0)
            {
                taikinTime = new DateTime(baseDate.Year, baseDate.Month, baseDate.AddDays(1).Day, taikinHour, taikinMinute, 0);
            }
            else
            {
                taikinTime = baseDate.ToDateTime(new TimeOnly(taikinHour, taikinMinute, 0));
            }

            await model.OnPostRegisterAsync();

            // 更新後の情報を取得
            var targetDeletedWorkingHour = await db.WorkingHours
                .Where(x => x.Deleted)
                .OrderBy(x => x.SyukkinTime)
                .ToListAsync();

            var targetNotDeletedWorkingHour = await db.WorkingHours
                .Where(x => !x.Deleted)
                .OrderBy(x => x.SyukkinTime)
                .ToListAsync();

            var targetUkagaiHeader = await db.UkagaiHeaders
                .FirstOrDefaultAsync();

            var targetUkagaiShinsei = await db.UkagaiShinseis
                .ToListAsync();

            // ================ Assert ================ //
            Assert.IsEmpty(targetDeletedWorkingHour);
            Assert.IsNotEmpty(targetNotDeletedWorkingHour);
            Assert.IsNotNull(targetUkagaiHeader);
            Assert.IsNotEmpty(targetUkagaiShinsei);
            Assert.AreEqual(syain.Id, targetUkagaiHeader.SyainId);
            Assert.AreEqual(now.ToDateOnly(), targetUkagaiHeader.ShinseiYmd);
            Assert.AreEqual(承認, targetUkagaiHeader.Status);
            Assert.AreEqual(baseDate, targetUkagaiHeader.WorkYmd);
            Assert.AreEqual(model.ViewModel.SyuseiReason, targetUkagaiHeader.Biko);
            Assert.IsFalse(targetUkagaiHeader.Invalid);
            Assert.HasCount(1, targetUkagaiShinsei);
            Assert.AreEqual(targetUkagaiHeader.Id, targetUkagaiShinsei[0].UkagaiHeaderId);
            Assert.AreEqual(打刻時間修正, targetUkagaiShinsei[0].UkagaiSyubetsu);
            Assert.HasCount(1, targetUkagaiShinsei);
            Assert.AreEqual(model.ViewModel.SyainId, targetNotDeletedWorkingHour[0].SyainId);
            Assert.AreEqual(model.ViewModel.JissekiDate, targetNotDeletedWorkingHour[0].Hiduke);
            Assert.AreEqual(0, targetNotDeletedWorkingHour[0].SyukkinLatitude);
            Assert.AreEqual(0, targetNotDeletedWorkingHour[0].SyukkinLongitude);
            Assert.AreEqual(0, targetNotDeletedWorkingHour[0].TaikinLatitude);
            Assert.AreEqual(0, targetNotDeletedWorkingHour[0].TaikinLongitude);
            Assert.AreEqual(syukkinTime, targetNotDeletedWorkingHour[0].SyukkinTime);
            Assert.AreEqual(taikinTime, targetNotDeletedWorkingHour[0].TaikinTime);
            Assert.AreEqual(targetUkagaiHeader.Id, targetNotDeletedWorkingHour[0].UkagaiHeaderId);
            Assert.IsFalse(targetNotDeletedWorkingHour[0].Deleted);
            Assert.IsTrue(targetNotDeletedWorkingHour[0].Edited);
        }

        // =============================================
        // 異常系テストケース
        // =============================================

        // =============================================
        // 入力チェック
        // =============================================
        [DataRow(null, 0, 18, 0, DisplayName = "出勤時間1の時が未入力 → IsValidがfalseで返却される")]
        [DataRow(9, null, 18, 0, DisplayName = "出勤時間1の分が未入力 → IsValidがfalseで返却される")]
        [DataRow(9, 0, null, 0, DisplayName = "退勤時間1の時が未入力 → IsValidがfalseで返却される")]
        [DataRow(9, 0, 0, null, DisplayName = "退勤時間1の分が未入力 → IsValidがfalseで返却される")]
        [TestMethod]
        public async Task OnPostRegisterAsync_出退勤１の時または分が未入力_IsValidがfalseで返却(
            int? syukkinHour,
            int? syukkinMinute,
            int? taikinHour,
            int? taikinMinute)
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            model.ViewModel.TimeSets[0].Start.Hour = syukkinHour;
            model.ViewModel.TimeSets[0].Start.Minute = syukkinMinute;
            model.ViewModel.TimeSets[0].End.Hour = taikinHour;
            model.ViewModel.TimeSets[0].End.Minute = taikinMinute;

            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(string.Format(Const.ErrorSet, "出退勤1" + "、時間と分の両方"), errors[0]);
        }

        [TestMethod(DisplayName = "出勤時間１と退勤時間１の入力値が逆転 → IsValidがfalseで返却される")]
        public async Task OnPostRegisterAsync_出勤時間１と退勤時間１が逆転_IsValidがfalseで返却()
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            model.ViewModel.TimeSets[0].Start.Hour = 18;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = 9;
            model.ViewModel.TimeSets[0].End.Minute = 0;

            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(string.Format(Const.ErrorReverse, "出退勤1、出退勤時間"), errors[0]);
        }

        [DataRow(null, 0, 16, 0, DisplayName = "出勤時間２の時が未入力 → IsValidがfalseで返却される")]
        [DataRow(12, null, 16, 0, DisplayName = "出勤時間２の分が未入力 → IsValidがfalseで返却される")]
        [DataRow(12, 0, null, 0, DisplayName = "退勤時間２の時が未入力 → IsValidがfalseで返却される")]
        [DataRow(12, 0, 16, null, DisplayName = "退勤時間２の分が未入力 → IsValidがfalseで返却される")]
        [TestMethod]
        public async Task OnPostRegisterAsync_出退勤２の時または分が未入力_IsValidがfalseで返却(
            int? syukkinHour,
            int? syukkinMinute,
            int? taikinHour,
            int? taikinMinute)
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            // 出退勤1
            model.ViewModel.TimeSets[0].Start.Hour = 9;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = 11;
            model.ViewModel.TimeSets[0].End.Minute = 0;

            // 出退勤2
            model.ViewModel.TimeSets[1].Start.Hour = syukkinHour;
            model.ViewModel.TimeSets[1].Start.Minute = syukkinMinute;
            model.ViewModel.TimeSets[1].End.Hour = taikinHour;
            model.ViewModel.TimeSets[1].End.Minute = taikinMinute;

            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(string.Format(Const.ErrorSet, "出退勤2" + "、時間と分の両方"), errors[0]);

        }

        [TestMethod(DisplayName = "出勤時間２と退勤時間２の入力値が逆転 → IsValidがfalseで返却される")]
        public async Task OnPostRegisterAsync_出勤時間２と退勤時間２が逆転_IsValidがfalseで返却()
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            // 出退勤1
            model.ViewModel.TimeSets[0].Start.Hour = 9;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = 11;
            model.ViewModel.TimeSets[0].End.Minute = 0;

            // 出退勤2
            model.ViewModel.TimeSets[1].Start.Hour = 16;
            model.ViewModel.TimeSets[1].Start.Minute = 0;
            model.ViewModel.TimeSets[1].End.Hour = 12;
            model.ViewModel.TimeSets[1].End.Minute = 0;

            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(string.Format(Const.ErrorReverse, "出退勤2、出退勤時間"), errors[0]);
        }

        [DataRow(null, 0, 21, 0, DisplayName = "出勤時間３の時が未入力 → IsValidがfalseで返却される")]
        [DataRow(17, null, 21, 0, DisplayName = "出勤時間３の分が未入力 → IsValidがfalseで返却される")]
        [DataRow(17, 0, null, 0, DisplayName = "退勤時間３の時が未入力 → IsValidがfalseで返却される")]
        [DataRow(17, 0, 21, null, DisplayName = "退勤時間３の分が未入力 → IsValidがfalseで返却される")]
        [TestMethod]
        public async Task OnPostRegisterAsync_出退勤３の入力値が異常_IsValidがfalseで返却(
            int? syukkinHour,
            int? syukkinMinute,
            int? taikinHour,
            int? taikinMinute)
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            // 出退勤1
            model.ViewModel.TimeSets[0].Start.Hour = 9;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = 11;
            model.ViewModel.TimeSets[0].End.Minute = 0;

            // 出退勤2
            model.ViewModel.TimeSets[1].Start.Hour = 12;
            model.ViewModel.TimeSets[1].Start.Minute = 0;
            model.ViewModel.TimeSets[1].End.Hour = 17;
            model.ViewModel.TimeSets[1].End.Minute = 0;

            // 出退勤3
            model.ViewModel.TimeSets[2].Start.Hour = syukkinHour;
            model.ViewModel.TimeSets[2].Start.Minute = syukkinMinute;
            model.ViewModel.TimeSets[2].End.Hour = taikinHour;
            model.ViewModel.TimeSets[2].End.Minute = taikinMinute;

            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(string.Format(Const.ErrorSet, "出退勤3" + "、時間と分の両方"), errors[0]);
        }

        [TestMethod(DisplayName = "出退勤３の入力値が逆転 → IsValidがfalseで返却される")]
        public async Task OnPostRegisterAsync_出勤時間３と退勤時間３が逆転_IsValidがfalseで返却()
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            // 出退勤1
            model.ViewModel.TimeSets[0].Start.Hour = 9;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = 11;
            model.ViewModel.TimeSets[0].End.Minute = 0;

            // 出退勤2
            model.ViewModel.TimeSets[1].Start.Hour = 12;
            model.ViewModel.TimeSets[1].Start.Minute = 0;
            model.ViewModel.TimeSets[1].End.Hour = 17;
            model.ViewModel.TimeSets[1].End.Minute = 0;

            // 出退勤3
            model.ViewModel.TimeSets[2].Start.Hour = 21;
            model.ViewModel.TimeSets[2].Start.Minute = 0;
            model.ViewModel.TimeSets[2].End.Hour = 17;
            model.ViewModel.TimeSets[2].End.Minute = 0;

            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(string.Format(Const.ErrorReverse, "出退勤3、出退勤時間"), errors[0]);
        }

        [TestMethod(DisplayName = "退勤時間１と出勤時間２が逆転 → IsValidがfalseで返却される")]
        public async Task OnPostRegisterAsync_退勤時間１と出勤時間２が逆転_IsValidがfalseで返却()
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            // 出退勤1
            model.ViewModel.TimeSets[0].Start.Hour = 9;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = 11;
            model.ViewModel.TimeSets[0].End.Minute = 0;

            // 出退勤2
            model.ViewModel.TimeSets[1].Start.Hour = 7;
            model.ViewModel.TimeSets[1].Start.Minute = 0;
            model.ViewModel.TimeSets[1].End.Hour = 8;
            model.ViewModel.TimeSets[1].End.Minute = 0;

            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(string.Format(Const.ErrorReverse, "出退勤1と出退勤2"), errors[0]);
        }

        [TestMethod(DisplayName = "退勤時間２と出勤時間３が逆転 → IsValidがfalseで返却される")]
        public async Task OnPostRegisterAsync_退勤時間２と出勤時間３が逆転_IsValidがfalseで返却()
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            // 出退勤1
            model.ViewModel.TimeSets[0].Start.Hour = 9;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = 11;
            model.ViewModel.TimeSets[0].End.Minute = 0;

            // 出退勤2
            model.ViewModel.TimeSets[1].Start.Hour = 18;
            model.ViewModel.TimeSets[1].Start.Minute = 0;
            model.ViewModel.TimeSets[1].End.Hour = 21;
            model.ViewModel.TimeSets[1].End.Minute = 0;

            // 出退勤3
            model.ViewModel.TimeSets[2].Start.Hour = 12;
            model.ViewModel.TimeSets[2].Start.Minute = 0;
            model.ViewModel.TimeSets[2].End.Hour = 17;
            model.ViewModel.TimeSets[2].End.Minute = 0;

            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(string.Format(Const.ErrorReverse, "出退勤2と出退勤3"), errors[0]);
        }

        [TestMethod(DisplayName = "出退勤時間全てが未入力 → IsValidがfalseで返却される")]
        public async Task OnPostRegisterAsync_出退勤時間全てが未入力_IsValidがfalseで返却()
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            // 出退勤1
            model.ViewModel.TimeSets[0].Start.Hour = null;
            model.ViewModel.TimeSets[0].Start.Minute = null;
            model.ViewModel.TimeSets[0].End.Hour = null;
            model.ViewModel.TimeSets[0].End.Minute = null;

            // 出退勤2
            model.ViewModel.TimeSets[1].Start.Hour = null;
            model.ViewModel.TimeSets[1].Start.Minute = null;
            model.ViewModel.TimeSets[1].End.Hour = null;
            model.ViewModel.TimeSets[1].End.Minute = null;

            // 出退勤3
            model.ViewModel.TimeSets[2].Start.Hour = null;
            model.ViewModel.TimeSets[2].Start.Minute = null;
            model.ViewModel.TimeSets[2].End.Hour = null;
            model.ViewModel.TimeSets[2].End.Minute = null;

            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(string.Format(Const.ErrorInputRequired, "出退勤時間"), errors[0]);
        }

        [DataRow(9, 11, 10, 15, 16, 18, "出退勤1", "出退勤2", DisplayName = "出退勤１と出退勤２が重複 → IsValidがfalseで返却される")]
        [DataRow(9, 11, 12, 15, 14, 18, "出退勤2", "出退勤3", DisplayName = "出退勤２と出退勤３が重複 → IsValidがfalseで返却される")]
        [DataRow(9, 11, 12, 15, 8, 10, "出退勤3", "出退勤1", DisplayName = "出退勤１と出退勤３が重複 → IsValidがfalseで返却される")]
        [TestMethod]
        public async Task OnPostRegisterAsync_出退勤時間が重複_IsValidがfalseで返却(
            int syukkinHour1,
            int taikinHour1,
            int syukkinHour2,
            int taikinHour2,
            int syukkinHour3,
            int taikinHour3,
            string inputLabel1,
            string inputLabel2)
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            // 出退勤1
            model.ViewModel.TimeSets[0].Start.Hour = syukkinHour1;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = taikinHour1;
            model.ViewModel.TimeSets[0].End.Minute = 0;

            // 出退勤2
            model.ViewModel.TimeSets[1].Start.Hour = syukkinHour2;
            model.ViewModel.TimeSets[1].Start.Minute = 0;
            model.ViewModel.TimeSets[1].End.Hour = taikinHour2;
            model.ViewModel.TimeSets[1].End.Minute = 0;

            // 出退勤3
            model.ViewModel.TimeSets[2].Start.Hour = syukkinHour3;
            model.ViewModel.TimeSets[2].Start.Minute = 0;
            model.ViewModel.TimeSets[2].End.Hour = taikinHour3;
            model.ViewModel.TimeSets[2].End.Minute = 0;

            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(string.Format(Const.ErrorOverlapInputTime, inputLabel1, inputLabel2), errors[0]);
        }

        [TestMethod(DisplayName = "入力したユーザ != 修正したユーザ かつ 社員権限が代理入力者に該当せず、修正理由が未入力 → IsValidがfalseで返却される")]
        public async Task OnPostRegisterAsync_代理入力者に該当せず修正理由が未入力_IsValidがfalseで返却()
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
                Kengen = 0,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // ================ Act ================ //
            // 入力値の設定
            model.ViewModel.SyainId = syain.Id;

            // 出退勤1
            model.ViewModel.TimeSets[0].Start.Hour = 9;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = 18;
            model.ViewModel.TimeSets[0].End.Minute = 0;

            // 修正理由
            model.ViewModel.SyuseiReason = "";
            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(string.Format(Const.ErrorInputRequired, "修正理由"), errors[0]);
        }

        [TestMethod(DisplayName = "パラメータの実績日の日報実績が確定 → IsValidがfalseで返却される")]
        public async Task OnPostRegisterAsync_パラメータの実績日の日報実績が確定_IsValidがfalseで返却()
        {
            // ================ Arrange ================ //
            var baseDate = new DateOnly(2025, 04, 01);

            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = 管理,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

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

            // 日報実績の登録
            var nippou = new Nippou()
            {
                Syain = syain,
                NippouYmd = baseDate,
                Youbi = 1,
                KaisyaCode = 協和,
                IsRendouZumi = true,
                TourokuKubun = 確定保存,
                SyukkinKubunId1 = 1,
            };

            SeedEntities(kintai, nippou);

            // ================ Act ================ //
            await model.OnGetAsync(syain.Id, baseDate);

            // 入力値を設定
            model.ViewModel.TimeSets[0].Start.Hour = 10;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = 21;
            model.ViewModel.TimeSets[0].End.Minute = 00;

            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(Const.ErrorNippouLocked, errors[0]);
        }

        [DataRow(true, false, DisplayName = "未削除の勤怠打刻情報の件数が異常 → IsValidがfalseで返却される")]
        [DataRow(false, true, DisplayName = "削除済みの勤怠打刻情報の件数が異常 → IsValidがfalseで返却される")]
        [TestMethod]
        public async Task OnPostRegisterAsync_勤怠打刻情報の件数が異常_IsValidがfalseで返却(
            bool isAddNotDeletedKintai,
            bool isAddDeletedKintai)
        {
            // ================ Arrange ================ //
            var syainId = 1;
            var baseDate = new DateOnly(2025, 04, 01);

            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = パート,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // 未削除の勤怠打刻情報の登録
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
                Edited = true,
                Deleted = false,
                Version = 0, // ダミー
            };

            // 削除済み勤怠打刻情報の登録
            var deletedKintai = new WorkingHour()
            {
                SyainId = syainId,
                Hiduke = baseDate,
                SyukkinLatitude = 0,
                SyukkinLongitude = 0,
                TaikinLatitude = 0,
                TaikinLongitude = 0,
                SyukkinTime = new DateTime(2025, 4, 1, 9, 0, 0),
                TaikinTime = new DateTime(2025, 4, 1, 19, 0, 0),
                Edited = false,
                Deleted = true,
                Version = 0, // ダミー
            };

            // 日報実績の登録
            var nippou = new Nippou()
            {
                SyainId = syain.Id,
                NippouYmd = baseDate,
                Youbi = 1,
                KaisyaCode = 協和,
                IsRendouZumi = true,
                TourokuKubun = 一時保存,
                SyukkinKubunId1 = 1,
            };

            SeedEntities(notDeletedKintai, deletedKintai, nippou);

            await model.OnGetAsync(syainId, baseDate);

            // ================ Act ================ //
            // 入力値を設定
            model.ViewModel.TimeSets[0].Start.Hour = 10;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = 21;
            model.ViewModel.TimeSets[0].End.Minute = 00;

            // OnPost前に勤怠打刻情報の件数が変化
            if (isAddNotDeletedKintai)
            {
                var addNotDeletedKintai = new WorkingHour()
                {
                    SyainId = syainId,
                    Hiduke = baseDate,
                    SyukkinLatitude = 0,
                    SyukkinLongitude = 0,
                    TaikinLatitude = 0,
                    TaikinLongitude = 0,
                    SyukkinTime = new DateTime(2025, 4, 1, 10, 0, 0),
                    TaikinTime = new DateTime(2025, 4, 1, 18, 0, 0),
                    Edited = true,
                    Deleted = false,
                };
                db.Add(addNotDeletedKintai);
            }

            if (isAddDeletedKintai)
            {
                var addDeletedKintai = new WorkingHour()
                {
                    SyainId = syainId,
                    Hiduke = baseDate,
                    SyukkinLatitude = 0,
                    SyukkinLongitude = 0,
                    TaikinLatitude = 0,
                    TaikinLongitude = 0,
                    SyukkinTime = new DateTime(2025, 4, 1, 10, 0, 0),
                    TaikinTime = new DateTime(2025, 4, 1, 21, 0, 0),
                    Edited = false,
                    Deleted = true,
                };
                db.Add(addDeletedKintai);
            }

            await db.SaveChangesAsync();

            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(string.Format(Const.ErrorConflictReload, "打刻情報"), errors[0]);
        }

        [TestMethod(DisplayName = "伺い申請情報の件数が異常 → IsValidがfalseで返却される")]
        public async Task OnPostRegisterAsync_伺い申請情報の件数が異常_IsValidがfalseで返却()
        {
            // ================ Arrange ================ //
            var syainId = 1;
            var baseDate = new DateOnly(2026, 07, 01);
            var now = new DateTime(2026, 7, 2, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);

            var model = CreateModel();

            // 勤怠属性の登録
            var kintaiZokusei = new KintaiZokusei()
            {
                Name = "test",
                Code = パート,
                SeigenTime = 0,
                IsMinashi = false,
                MaxLimitTime = 0,
            };

            // ログインユーザーの社員情報を追加
            var syainBase = new SyainBasis()
            {
                Id = LoggedInUserId,
                Code = LoggedInUserCode,
                Name = LoggedInUserName,
            };

            var syain = new Syain
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
                SyainBase = syainBase,
                BusyoId = 1,
                KintaiZokusei = kintaiZokusei,
                UserRoleId = 1,
            };

            SeedEntities(syainBase, syain, kintaiZokusei);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = syain };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            // 伺い入力ヘッダ情報の登録
            var ukagaiHeader = new UkagaiHeader()
            {
                SyainId = syainId,
                ShinseiYmd = now.ToDateOnly(),
                Status = 0,
                WorkYmd = baseDate,
                Biko = "備考",
                Invalid = false,
            };

            // 伺い申請の登録
            var ukagaiShinsei = new UkagaiShinsei()
            {
                UkagaiHeader = ukagaiHeader,
                UkagaiSyubetsu = 打刻時間修正,
                Version = 0, // ダミー
            };

            // 未削除の勤怠打刻情報の登録
            var notDeletedKintai = new WorkingHour()
            {
                SyainId = syainId,
                Hiduke = baseDate,
                SyukkinLatitude = 0,
                SyukkinLongitude = 0,
                TaikinLatitude = 0,
                TaikinLongitude = 0,
                SyukkinTime = new DateTime(2025, 7, 1, 9, 0, 0),
                TaikinTime = new DateTime(2025, 7, 1, 18, 0, 0),
                Edited = true,
                Deleted = false,
                UkagaiHeader = ukagaiHeader,
            };

            // 削除済み勤怠打刻情報の登録
            var deletedKintai = new WorkingHour()
            {
                SyainId = syainId,
                Hiduke = baseDate,
                SyukkinLatitude = 0,
                SyukkinLongitude = 0,
                TaikinLatitude = 0,
                TaikinLongitude = 0,
                SyukkinTime = new DateTime(2025, 7, 1, 9, 0, 0),
                TaikinTime = new DateTime(2025, 7, 1, 19, 0, 0),
                Edited = false,
                Deleted = true,
                UkagaiHeader = ukagaiHeader,
            };

            // 日報実績の登録
            var nippou = new Nippou()
            {
                Syain = syain,
                NippouYmd = baseDate,
                Youbi = 1,
                KaisyaCode = 協和,
                IsRendouZumi = true,
                TourokuKubun = 一時保存,
                SyukkinKubunId1 = 1,
            };

            SeedEntities(notDeletedKintai, deletedKintai, ukagaiHeader, ukagaiShinsei, nippou);

            await model.OnGetAsync(syainId, baseDate);

            // ================ Act ================ //
            model.ViewModel.TimeSets[0].Start.Hour = 10;
            model.ViewModel.TimeSets[0].Start.Minute = 0;
            model.ViewModel.TimeSets[0].End.Hour = 21;
            model.ViewModel.TimeSets[0].End.Minute = 00;

            // OnPost前に伺い申請情報の件数が変化
            db.Add(new UkagaiShinsei()
            {
                UkagaiHeader = ukagaiHeader,
                UkagaiSyubetsu = 打刻時間修正,
            });

            await db.SaveChangesAsync();

            var response = await model.OnPostRegisterAsync();

            // ================ Assert ================ //
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(string.Format(Const.ErrorConflictReload, "打刻情報"), errors[0]);
        }
    }
}
