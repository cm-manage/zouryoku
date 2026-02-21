using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zouryoku.Pages.Maintenance.PcLogs;
using Model.Model;
using ZouryokuTest;
using Microsoft.AspNetCore.Mvc;

namespace ZouryokuTest.Pages.Maintenance.PcLogs
{
    /// <summary>
    /// InputModel (PCログ入力ページ) のユニットテスト
    /// </summary>
    [TestClass]
    public class InputModelTests : BaseInMemoryDbContextTest
    {
        private InputModel CreateModel()
        {
            var model = new InputModel(db, GetLogger<InputModel>(), options);
            model.PageContext = GetPageContext();
            model.TempData = GetTempData();
            return model;
        }

        /// <summary>
        /// 新規登録: OnPostRegisterAsyncでレコードが追加されること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_Adds_New_Record()
        {
            // Arrange
            var model = CreateModel();
            model.PcLog = new PcLogModel
            {
                Datetime = DateTime.UtcNow,
                PcName = "PC-NEW",
                UserName = "tester",
                SyainId = null,
                Operation = Model.Enums.PcOperationType.ログオン
            };
            var beforeCount = await db.PcLogs.CountAsync();

            // Act
            var result = await model.OnPostRegisterAsync();

            // Assert
            var afterCount = await db.PcLogs.CountAsync();
            Assert.AreEqual(beforeCount +1, afterCount);
            var saved = await db.PcLogs.FirstAsync();
            Assert.AreEqual("PC-NEW", saved.PcName);
            Assert.AreEqual("tester", saved.UserName);
        }

        /// <summary>
        /// 更新:既存レコードを編集し OnPostRegisterAsyncで変更が反映されること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_Updates_Existing_Record()
        {
            // Arrange
            var existing = new PcLog
            {
                Datetime = DateTime.UtcNow.AddMinutes(-10),
                PcName = "PC-OLD",
                UserName = "olduser",
                Operation = Model.Enums.PcOperationType.電源オン
            };
            db.PcLogs.Add(existing);
            await db.SaveChangesAsync();

            var model = CreateModel();
            var toUpdateEntity = await db.PcLogs.FirstAsync();
            var toUpdate = PcLogModel.FromEntity(toUpdateEntity);
            toUpdate.PcName = "PC-UPDATED";
            toUpdate.UserName = "newuser";
            toUpdate.Operation = Model.Enums.PcOperationType.ログオフ;
            model.PcLog = toUpdate;

            // Act
            var result = await model.OnPostRegisterAsync();

            // Assert
            var saved = await db.PcLogs.FirstAsync();
            Assert.AreEqual("PC-UPDATED", saved.PcName);
            Assert.AreEqual("newuser", saved.UserName);
            Assert.AreEqual(Model.Enums.PcOperationType.ログオフ, saved.Operation);
            Assert.AreEqual(1, await db.PcLogs.CountAsync());
        }

        /// <summary>
        /// 入力チェックエラー: PcName が必須エラーの場合 JsonResult が返却されること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_Returns_ErrorJson_When_PcName_Missing()
        {
            // Arrange
            var model = CreateModel();
            model.PcLog = new PcLogModel
            {
                Datetime = DateTime.UtcNow,
                UserName = "invalid-user",
                Operation = Model.Enums.PcOperationType.ログオン
            };
            model.ModelState.AddModelError("PcLog.PcName", "PcNameは必須です。");
            var beforeCount = await db.PcLogs.CountAsync();

            // Act
            var result = await model.OnPostRegisterAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var json = (JsonResult)result;
            var errorsProp = json.Value?.GetType().GetProperty("Errors");
            Assert.IsNotNull(errorsProp);
            var errorsDict = errorsProp!.GetValue(json.Value) as System.Collections.IDictionary;
            Assert.IsNotNull(errorsDict);
            Assert.Contains("PcLog.PcName", errorsDict.Keys);

            var afterCount = await db.PcLogs.CountAsync();
            Assert.AreEqual(beforeCount, afterCount);
        }

        /// <summary>
        /// 日時未入力（MinValue）時に必須エラーが返ること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_Returns_ErrorJson_When_Datetime_Missing()
        {
            // Arrange
            var model = CreateModel();
            model.PcLog = new PcLogModel
            {
                Datetime = DateTime.MinValue, // 必須違反
                PcName = "PC-NO-DATE",
                UserName = "user",
                Operation = Model.Enums.PcOperationType.ログオン
            };
            var beforeCount = await db.PcLogs.CountAsync();

            // 手動でなくサーバ側必須チェックでも拾えるが、キー名を確認するため追加
            model.ModelState.AddModelError("PcLog.Datetime", "日時は必須です。");

            // Act
            var result = await model.OnPostRegisterAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var json = (JsonResult)result;
            var errorsProp = json.Value?.GetType().GetProperty("Errors");
            Assert.IsNotNull(errorsProp);
            var errorsDict = errorsProp!.GetValue(json.Value) as System.Collections.IDictionary;
            Assert.IsNotNull(errorsDict);
            Assert.Contains("PcLog.Datetime", errorsDict.Keys);

            var afterCount = await db.PcLogs.CountAsync();
            Assert.AreEqual(beforeCount, afterCount);
        }

        /// <summary>
        /// 削除: OnPostDeleteAsyncで既存レコードが削除されること
        /// </summary>
        [TestMethod]
        public async Task OnPostDeleteAsync_Removes_Record()
        {
            // Arrange
            var existing = new PcLog
            {
                Datetime = DateTime.UtcNow,
                PcName = "PC-DEL",
                UserName = "deluser",
                Operation = Model.Enums.PcOperationType.電源オン
            };
            db.PcLogs.Add(existing);
            await db.SaveChangesAsync();
            var id = existing.Id;

            var model = CreateModel();
            model.PcLog = new PcLogModel { Id = id };

            // Act
            var result = await model.OnPostDeleteAsync();

            // Assert
            var count = await db.PcLogs.CountAsync();
            Assert.AreEqual(0, count);
            var isExists = await db.PcLogs.AnyAsync(x => x.Id == id);
            Assert.IsFalse(isExists);
        }

        /// <summary>
        /// 削除: 編集モードではない場合 BadRequest が返ること
        /// </summary>
        [TestMethod]
        public async Task OnPostDeleteAsync_Returns_BadRequest_When_Not_Edit()
        {
            // Arrange
            var model = CreateModel();
            model.PcLog = new PcLogModel { Id =0 };

            // Act
            var result = await model.OnPostDeleteAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }
    }
}