using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Pages.Maintenance.AppSettings;
using System.Collections;

namespace ZouryokuTest.Pages.Maintenance.AppSettings
{
    /// <summary>
    /// IndexModel (アプリケーション設定ページ) のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {
        private IndexModel CreateModel()
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, fakeTimeProvider);
            model.PageContext = GetPageContext();
            model.TempData = GetTempData();
            return model;
        }

        /// <summary>
        /// 初期表示: レコードが存在する場合、AppSettingプロパティに値が設定されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_Loads_Existing_Config()
        {
            // Arrange
            var existing = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = new DateOnly(2025, 12, 31),
                MsTenantId = "test-tenant-id",
                MsClientId = "test-client-id",
                MsClientSecret = "test-secret",
                SmtpUser = "test@example.com",
                SmtpPassword = "password123"
            };
            db.ApplicationConfigs.Add(existing);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync();

            // Assert
            Assert.IsInstanceOfType<PageResult>(result);
            Assert.AreEqual(new DateOnly(2025, 12, 31), model.AppSetting.NippoStopDate);
            Assert.AreEqual("test-tenant-id", model.AppSetting.MsTenantId);
            Assert.AreEqual("test-client-id", model.AppSetting.MsClientId);
            Assert.AreEqual("test-secret", model.AppSetting.MsClientSecret);
            Assert.AreEqual("test@example.com", model.AppSetting.SmtpUser);
            Assert.AreEqual("password123", model.AppSetting.SmtpPassword);
        }

       
        /// <summary>
        /// 更新: 既存レコードを編集し OnPostRegisterAsyncで変更が反映されること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_Updates_Existing_Config()
        {
            // Arrange
            var existing = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = new DateOnly(2025, 06, 30),
                MsTenantId = "old-tenant-id",
                MsClientId = "old-client-id",
                MsClientSecret = "old-secret",
                SmtpUser = "olduser@example.com",
                SmtpPassword = "oldpassword"
            };
            db.ApplicationConfigs.Add(existing);
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.AppSetting = new IndexModel.AppSettingModel
            {
                NippoStopDate = new DateOnly(2025, 12, 31),
                MsTenantId = "updated-tenant-id",
                MsClientId = "updated-client-id",
                MsClientSecret = "updated-secret",
                SmtpUser = "updateduser@example.com",
                SmtpPassword = "updatedpassword"
            };

            // Act
            var result = await model.OnPostRegisterAsync();

            // Assert
            var configs = await db.ApplicationConfigs.ToListAsync();
            Assert.HasCount(1, configs);
            var saved = configs.First();
            Assert.AreEqual(new DateOnly(2025, 12, 31), saved.NippoStopDate);
            Assert.AreEqual("updated-tenant-id", saved.MsTenantId);
            Assert.AreEqual("updated-client-id", saved.MsClientId);
            Assert.AreEqual("updated-secret", saved.MsClientSecret);
            Assert.AreEqual("updateduser@example.com", saved.SmtpUser);
            Assert.AreEqual("updatedpassword", saved.SmtpPassword);
        }

        /// <summary>
        /// 入力チェックエラー: NippoStopDateがMinValueの場合、エラーが返却されること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_Returns_ErrorJson_When_NippoStopDate_Missing()
        {
            // Arrange
            var existing = new ApplicationConfig
            {
                Id = 1,
                NippoStopDate = new DateOnly(2025, 12, 31),
                MsTenantId = "tenant-id",
                MsClientId = "client-id",
                MsClientSecret = "secret",
                SmtpUser = "user@example.com",
                SmtpPassword = "password"
            };
            db.ApplicationConfigs.Add(existing);
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.AppSetting = new IndexModel.AppSettingModel
            {
                NippoStopDate = DateOnly.MinValue,
                MsTenantId = "tenant-id",
                MsClientId = "client-id",
                MsClientSecret = "secret",
                SmtpUser = "user@example.com",
                SmtpPassword = "password"
            };

            // Act
            var result = await model.OnPostRegisterAsync();

            // Assert
            Assert.IsInstanceOfType<JsonResult>(result);
            var json = (JsonResult)result;
            Assert.IsNotNull(json.Value, "JsonResult.Value が null です");
            
            var errorsProp = json.Value.GetType().GetProperty("Errors");
            Assert.IsNotNull(errorsProp, "Errors プロパティが見つかりません");
            
            var errorsDict = errorsProp.GetValue(json.Value) as IDictionary;
            Assert.IsNotNull(errorsDict, "Errors の値が IDictionary ではありません");
            Assert.Contains("AppSetting.NippoStopDate", errorsDict.Keys, "NippoStopDate のエラーが含まれていません");

            // データが保存されていないことを確認
            var config = await db.ApplicationConfigs.FirstAsync();
            Assert.AreEqual(new DateOnly(2025, 12, 31), config.NippoStopDate, "データが誤って更新されています");
        }
    }
}