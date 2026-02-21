using Microsoft.AspNetCore.Mvc;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Pages.SyainSentaku;
using Zouryoku.Utils;
using static Model.Enums.ResponseStatus;

namespace ZouryokuTest.Pages.SyainSentaku
{
    /// <summary>
    /// 社員選択時のテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnPostValidateSelectionTests : IndexModelTestsBase
    {
        /// <summary>
        /// 正常系：社員選択した場合、セッションに最終選択部署IDが格納されている
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostValidateSelectionAsync_社員選択_最終選択部署IDがセッションに保存される()
        {
            // Arrange
            var model = CreateModel();
            var input = new IndexModel.ValidateSelectionRequest
            {
                SelectCounts = 1,
                BusyoId = 1
            };

            // Act
            await model.OnPostValidateSelectionAsync(input);
            var busyoId = model.HttpContext.Session.Get<long>(SaveSessionName);

            // Assert
            Assert.AreEqual(1, busyoId);
        }

        /// <summary>
        /// 正常系：社員選択した場合、成功ステータスのJSONを返す
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostValidateSelectionAsync_社員選択_successJsonが返される()
        {
            // Arrange
            var model = CreateModel();
            var input = new IndexModel.ValidateSelectionRequest
            {
                SelectCounts = 1,
                BusyoId = 1
            };

            // Act
            var response = await model.OnPostValidateSelectionAsync(input);
            var result = (JsonResult)response;

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(ResponseJson));
            var value = (ResponseJson)result.Value;
            Assert.AreEqual(正常, value.Status);
        }

        /// <summary>
        /// 異常系：社員未選択である場合、エラーを返す
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnPostValidateSelectionAsync_未選択のまま確定ボタン押下_エラーが返される()
        {
            // Arrange
            var model = CreateModel();
            model.SyainName = "社員名";
            var input = new IndexModel.ValidateSelectionRequest
            {
                SelectCounts = 0
            };
            model.ModelState.AddModelError(nameof(input), Const.ErrorSelectRequired);
            // Act
            var response = await model.OnPostValidateSelectionAsync(input);

            // Assert
            var result = (ObjectResult)response;
            Assert.AreEqual(エラー, GetResponseStatus(result));
        }

    }
}
