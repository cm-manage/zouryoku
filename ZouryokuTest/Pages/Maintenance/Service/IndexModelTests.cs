using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Model.Enums;
using Model.Model;
using Zouryoku.Pages.Maintenance.Service;

namespace ZouryokuTest.Pages.Maintenance.Service
{
    /// <summary>
    /// IndexModel (サービス稼働状況) のユニットテスト
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
        /// 初期表示: Type=1と2のレコードが存在する場合、checkboxに対応する値が設定されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_Loads_ServiceExecute_By_Type()
        {
            // Arrange
            var kingsRecord = new ServiceExecute
            {
                Id = 1,
                Used = true,
                Type = ServiceClassification.連携プログラム稼働,
            };

            var fatigueRecord = new ServiceExecute
            {
                Id = 2,
                Used = false,
                Type = ServiceClassification.過労運転防止,
            };

            var restRecord = new ServiceExecute
            {
                Id = 3,
                Used = true,
                Type = ServiceClassification.有給未取得アラート
            };

            var chatRecord = new ServiceExecute
            {
                Id = 4,
                Used = false,
                Type = ServiceClassification.チャット連携,
            };

            db.ServiceExecutes.Add(kingsRecord);
            db.ServiceExecutes.Add(fatigueRecord);
            db.ServiceExecutes.Add(restRecord);
            db.ServiceExecutes.Add(chatRecord);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsTrue(model.ServiceExecuteData.KingsIntegrationProgram);
            Assert.IsFalse(model.ServiceExecuteData.DriverFatiguePrevention);
            Assert.IsTrue(model.ServiceExecuteData.RestPrevention);
            Assert.IsFalse(model.ServiceExecuteData.ChatPrevention);
        }

        /// <summary>
        /// 初期表示: レコードが存在しない場合、デフォルト値が設定されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_When_No_ServiceExecute_Exists()
        {
            // Arrange
            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsFalse(model.ServiceExecuteData.KingsIntegrationProgram);
            Assert.IsFalse(model.ServiceExecuteData.DriverFatiguePrevention);
            Assert.IsFalse(model.ServiceExecuteData.RestPrevention);
            Assert.IsFalse(model.ServiceExecuteData.ChatPrevention);
        }

        /// <summary>
        /// 更新: 4つのレコード（全サービスタイプ）のUsedフラグが更新されること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_Updates_All_Service_Records_Used_Flag()
        {
            // Arrange
            var kingsRecord = new ServiceExecute
            {
                Id = 1,
                Used = false,
                Type = ServiceClassification.連携プログラム稼働,
            };

            var fatigueRecord = new ServiceExecute
            {
                Id = 2,
                Used = false,
                Type = ServiceClassification.過労運転防止,
            };

            var restRecord = new ServiceExecute
            {
                Id = 3,
                Used = false,
                Type = ServiceClassification.有給未取得アラート,
            };

            var chatRecord = new ServiceExecute
            {
                Id = 4,
                Used = false,
                Type = ServiceClassification.チャット連携,
            };

            db.ServiceExecutes.Add(kingsRecord);
            db.ServiceExecutes.Add(fatigueRecord);
            db.ServiceExecutes.Add(restRecord);
            db.ServiceExecutes.Add(chatRecord);
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.ServiceExecuteData = new IndexModel.ServiceExecuteModel
            {
                KingsIntegrationProgram = true,
                DriverFatiguePrevention = true,
                RestPrevention = true,
                ChatPrevention = true
            };

            // Act
            var result = await model.OnPostRegisterAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(200, objectResult.StatusCode);

            var savedKings = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.連携プログラム稼働)
                .FirstAsync();
            Assert.IsTrue(savedKings.Used);

            var savedFatigue = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.過労運転防止)
                .FirstAsync();
            Assert.IsTrue(savedFatigue.Used);

            var savedRest = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.有給未取得アラート)
                .FirstAsync();
            Assert.IsTrue(savedRest.Used);

            var savedChat = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.チャット連携)
                .FirstAsync();
            Assert.IsTrue(savedChat.Used);
        }

        /// <summary>
        /// 更新: 混合パターンで各レコードのUsedフラグが正しく更新されること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_Updates_Mixed_Used_Flags()
        {
            // Arrange
            var kingsRecord = new ServiceExecute
            {
                Id = 1,
                Used = true,
                Type = ServiceClassification.連携プログラム稼働,
            };

            var fatigueRecord = new ServiceExecute
            {
                Id = 2,
                Used = true,
                Type = ServiceClassification.過労運転防止,
            };

            var restRecord = new ServiceExecute
            {
                Id = 3,
                Used = true,
                Type = ServiceClassification.有給未取得アラート, 
            };

            var chatRecord = new ServiceExecute
            {
                Id = 4,
                Used = true,
                Type = ServiceClassification.チャット連携, 
            };

            db.ServiceExecutes.Add(kingsRecord);
            db.ServiceExecutes.Add(fatigueRecord);
            db.ServiceExecutes.Add(restRecord);
            db.ServiceExecutes.Add(chatRecord);
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.ServiceExecuteData = new IndexModel.ServiceExecuteModel
            {
                KingsIntegrationProgram = true,
                DriverFatiguePrevention = false,
                RestPrevention = true,
                ChatPrevention = false
            };

            // Act
            var result = await model.OnPostRegisterAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(200, objectResult.StatusCode);

            var savedKings = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.連携プログラム稼働)
                .FirstAsync();
            Assert.IsTrue(savedKings.Used);

            var savedFatigue = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.過労運転防止)
                .FirstAsync();
            Assert.IsFalse(savedFatigue.Used);

            var savedRest = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.有給未取得アラート)
                .FirstAsync();
            Assert.IsTrue(savedRest.Used);

            var savedChat = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.チャット連携)
                .FirstAsync();
            Assert.IsFalse(savedChat.Used);
        }

        /// <summary>
        /// 更新: すべてのcheckboxがfalseに変更された場合、すべてのレコードが更新されること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_Updates_All_Records_To_False()
        {
            // Arrange
            var kingsRecord = new ServiceExecute
            {
                Id = 1,
                Used = true,
                Type = ServiceClassification.連携プログラム稼働,
            };

            var fatigueRecord = new ServiceExecute
            {
                Id = 2,
                Used = true,
                Type = ServiceClassification.過労運転防止,
            };

            var restRecord = new ServiceExecute
            {
                Id = 3,
                Used = true,
                Type = ServiceClassification.有給未取得アラート,
            };

            var chatRecord = new ServiceExecute
            {
                Id = 4,
                Used = true,
                Type = ServiceClassification.チャット連携,
            };

            db.ServiceExecutes.Add(kingsRecord);
            db.ServiceExecutes.Add(fatigueRecord);
            db.ServiceExecutes.Add(restRecord);
            db.ServiceExecutes.Add(chatRecord);
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.ServiceExecuteData = new IndexModel.ServiceExecuteModel
            {
                KingsIntegrationProgram = false,
                DriverFatiguePrevention = false,
                RestPrevention = false,
                ChatPrevention = false
            };

            // Act
            var result = await model.OnPostRegisterAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(200, objectResult.StatusCode);

            var savedKings = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.連携プログラム稼働)
                .FirstAsync();
            Assert.IsFalse(savedKings.Used);

            var savedFatigue = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.過労運転防止)
                .FirstAsync();
            Assert.IsFalse(savedFatigue.Used);

            var savedRest = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.有給未取得アラート)
                .FirstAsync();
            Assert.IsFalse(savedRest.Used);

            var savedChat = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.チャット連携)
                .FirstAsync();
            Assert.IsFalse(savedChat.Used);
        }




        /// <summary>
        /// 新規作成: レコードが存在しない場合、新規にレコードが作成されること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_Creates_New_Records_When_It_Does_Not_Exist()
        {
            // Arrange
            var model = CreateModel();
            model.ServiceExecuteData = new IndexModel.ServiceExecuteModel
            {
                KingsIntegrationProgram = true,
                DriverFatiguePrevention = true,
                RestPrevention = true,
                ChatPrevention = true
            };

            // Act
            var result = await model.OnPostRegisterAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(200, objectResult.StatusCode);

            var savedKings = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.連携プログラム稼働)
                .FirstAsync();
            Assert.IsTrue(savedKings.Used);

            var savedFatigue = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.過労運転防止)
                .FirstAsync();
            Assert.IsTrue(savedFatigue.Used);

            var savedRest = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.有給未取得アラート)
                .FirstAsync();
            Assert.IsTrue(savedRest.Used);

            var savedChat = await db.ServiceExecutes
                .Where(s => s.Type == ServiceClassification.チャット連携)
                .FirstAsync();
            Assert.IsTrue(savedChat.Used);
        }

        /// <summary>
        /// 初期表示: 一部のレコードのみ存在する場合、存在するレコードは読み込まれ、存在しないレコードはデフォルト値が使用されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_Loads_Partial_Records_With_Defaults()
        {
            // Arrange
            // Type = 1と3のレコードのみ追加
            var kingsRecord = new ServiceExecute
            {
                Id = 1,
                Used = true,
                Type = ServiceClassification.連携プログラム稼働,
            };

            var restRecord = new ServiceExecute
            {
                Id = 3,
                Used = true,
                Type = ServiceClassification.有給未取得アラート,
            };

            db.ServiceExecutes.Add(kingsRecord);
            db.ServiceExecutes.Add(restRecord);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            // 存在するレコード
            Assert.IsTrue(model.ServiceExecuteData.KingsIntegrationProgram);
            Assert.IsTrue(model.ServiceExecuteData.RestPrevention);
            // 存在しないレコード（デフォルト値）
            Assert.IsFalse(model.ServiceExecuteData.DriverFatiguePrevention);
            Assert.IsFalse(model.ServiceExecuteData.ChatPrevention);
        }

        /// <summary>
        /// 初期表示: UseInputAssetsフラグがtrueであること
        /// </summary>
        [TestMethod]
        public void OnGet_UseInputAssets_Should_Be_True()
        {
            // Arrange
            var model = CreateModel();

            // Act & Assert
            Assert.IsTrue(model.UseInputAssets, "UseInputAssetsはtrueである必要があります");
        }

        /// <summary>
        /// 初期表示: ServiceExecuteDataが初期化されていること
        /// </summary>
        [TestMethod]
        public void OnGet_ServiceExecuteData_Should_Be_Initialized()
        {
            // Arrange
            var model = CreateModel();

            // Act & Assert
            Assert.IsNotNull(model.ServiceExecuteData);
            Assert.IsFalse(model.ServiceExecuteData.KingsIntegrationProgram);
            Assert.IsFalse(model.ServiceExecuteData.DriverFatiguePrevention);
            Assert.IsFalse(model.ServiceExecuteData.RestPrevention);
            Assert.IsFalse(model.ServiceExecuteData.ChatPrevention);
        }
    }
}