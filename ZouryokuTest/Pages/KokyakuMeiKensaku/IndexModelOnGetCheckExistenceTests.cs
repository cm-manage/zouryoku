using Microsoft.AspNetCore.Mvc;
using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.KokyakuMeiKensaku
{
    /// <summary>
    /// 顧客存在チェック機能のテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetCheckExistenceTests : IndexModelTestBase
    {
        /// <summary>
        /// 正常系：データが存在するとき<see cref="Model.Enums.ResponseStatus"/>の正常を返却する
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetCheckExistence_顧客情報が存在するとき_正常()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();

            // Act
            var response = await model.OnGetCheckExistenceAsync(1);

            // Assert
            AssertSuccess(response);
        }

        /// <summary>
        /// 異常系：データが存在しないときエラーを返却する
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetCheckExistence_顧客情報が存在しないとき_エラー()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();

            // Act
            var response = await model.OnGetCheckExistenceAsync(100);

            // Assert
            AssertErrorJson(response, ErrorSelectedDataNotExists);
        }
    }
}