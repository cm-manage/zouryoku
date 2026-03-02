using Microsoft.AspNetCore.Mvc;
using Model.Model;
using Zouryoku.Pages.AnkenMeiKensaku;
using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.AnkenMeiKensaku
{
    /// <summary>
    /// <see cref="IndexModel.OnGetCheckExistenceAsync"/>のテストクラス
    /// </summary>
    [TestClass]
    public class OnGetCheckExistenceTest : IndexModelTestBase
    {
        // ======================================
        // テストの初期化処理
        // ======================================

        /// <summary>
        /// IndexModelを作成する。
        /// </summary>
        [TestInitialize]
        public void TestInit()
        {
            Model = CreateModel();
        }

        // ======================================
        // テストメソッド
        // ======================================

        [TestMethod]
        public async Task OnGetCheckExistenceAsync_案件情報が存在するとき_正常()
        {
            // Arrange
            db.Add(new Anken()
            {
                Id = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                SearchName = string.Empty,
            });
            db.SaveChanges();

            // Act
            var response = await Model!.OnGetCheckExistenceAsync(1);

            // Assert
            AssertSuccess(response);
        }

        [TestMethod]
        public async Task OnGetCheckExistenceAsync_案件情報が存在しないとき_エラー()
        {
            // Arrange
            db.Add(new Anken()
            {
                Id = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                SearchName = string.Empty,
            });
            db.SaveChanges();

            // Act
            var response = await Model!.OnGetCheckExistenceAsync(2);

            // Assert
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(ErrorSelectedDataNotExists, errors[0]);
        }
    }
}
