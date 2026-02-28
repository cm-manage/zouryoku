using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model.Model;
using Zouryoku.Pages.Kinmuhyo;
using static Model.Enums.ResponseStatus;

namespace ZouryokuTest.Pages.Kinmuhyo
{
    [TestClass]
    public class IndexModelPlannedUpdatesTests : BaseInMemoryDbContextTest
    {
        private IndexModel CreateModel()
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine, fakeTimeProvider)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };
            return model;
        }

        [TestMethod(DisplayName = "Given: 対象の予定データが存在しない When: 予定勤務更新要求 Then: エラーを返す")]
        public async Task OnPostUpdatePlannedWorkAsync_対象の予定データが存在しない_エラーを返す()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var model = CreateModel();

            model.SyainId = 999;
            model.YoteiYmd = dateYmd;

            // Act
            var result = await model.OnPostUpdatePlannedWorkAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var json = (JsonResult)result;
            var status = GetResponseStatus(json);
            var message = GetMessage(json);
            Assert.AreEqual(エラー, status);
            Assert.AreEqual("取込データが存在しません。", message);
        }

        [TestMethod(DisplayName = "Given: 対象の予定データが存在する When: 予定勤務更新要求 Then: Worked を更新する")]
        public async Task OnPostUpdatePlannedWorkAsync_対象の予定データが存在する_Workedを更新する()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            var syain = new Syain
            {
                Id = 1,
                Name = "T",
                StartYmd = dateYmd.AddYears(-1),
                EndYmd = dateYmd.AddYears(1),
                Code = "S0001",
                BusyoCode = "B001",
                KanaName = "ティー",
                KingsSyozoku = "K0001"
            };
            db.Syains.Add(syain);

            var ymd = dateYmd;
            var yotei = new NippouYotei
            {
                Id = 1,
                SyainId = syain.Id,
                NippouYoteiYmd = ymd,
                Worked = false,
                ZangyouJikan = 0
            };
            db.NippouYoteis.Add(yotei);
            await db.SaveChangesAsync();

            var model = CreateModel();

            model.SyainId = syain.Id;
            model.YoteiYmd = ymd;
            model.ShukkinFlg = true;

            // Act
            var result = await model.OnPostUpdatePlannedWorkAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var json = (JsonResult)result;
            var status = GetResponseStatus(json);
            Assert.AreEqual(正常, status);

            var updated = await db.NippouYoteis.FindAsync(yotei.Id);
            Assert.IsNotNull(updated);
            Assert.IsTrue(updated!.Worked);
        }

        [TestMethod(DisplayName = "Given: 対象の予定データが存在しない When: 予定残業更新要求 Then: エラーを返す")]
        public async Task OnPostUpdatePlannedOvertimeAsync_対象の予定データが存在しない_エラーを返す()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());

            var model = CreateModel();
            model.SyainId = 999;
            model.YoteiYmd = dateYmd;
            model.ZangyouJikan = 5;

            // Act
            var result = await model.OnPostUpdatePlannedOvertimeAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var json = (JsonResult)result;
            var status = GetResponseStatus(json);
            var message = GetMessage(json);
            Assert.AreEqual(エラー, status);
            Assert.AreEqual("取込データが存在しません。", message);
        }

        [TestMethod(DisplayName = "Given: 対象の予定データが存在する When: 予定残業更新要求 Then: ZangyouJikan を更新する")]
        public async Task OnPostUpdatePlannedOvertimeAsync_対象の予定データが存在する_ZangyouJikanを更新する()
        {
            // Arrange
            var dateYmd = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());

            var syain = new Syain
            {
                Id = 2,
                Name = "U",
                StartYmd = dateYmd.AddYears(-1),
                EndYmd = dateYmd.AddYears(1),
                Code = "S0002",
                BusyoCode = "B001",
                KanaName = "ユー",
                KingsSyozoku = "K0002"
            };
            db.Syains.Add(syain);

            var ymd = dateYmd;
            var yotei = new NippouYotei
            {
                Id = 2,
                SyainId = syain.Id,
                NippouYoteiYmd = ymd,
                Worked = true,
                ZangyouJikan = 0
            };
            db.NippouYoteis.Add(yotei);
            await db.SaveChangesAsync();

            var model = CreateModel();

            model.SyainId = syain.Id;
            model.YoteiYmd = ymd;
            model.ZangyouJikan = 123;

            // Act
            var result = await model.OnPostUpdatePlannedOvertimeAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var json = (JsonResult)result;
            var status = GetResponseStatus(json);
            Assert.AreEqual(正常, status);

            var updated = await db.NippouYoteis.FindAsync(yotei.Id);
            Assert.IsNotNull(updated);
            Assert.AreEqual((short)123, updated!.ZangyouJikan);
        }
    }
}