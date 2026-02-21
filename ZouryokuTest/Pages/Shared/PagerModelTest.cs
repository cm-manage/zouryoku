using Zouryoku.Pages.Shared;

namespace ZouryokuTest.Pages.Shared
{

    [TestClass]
    public class PagerModelTest
    {
        /// <summary>
        /// 項目数=0のときページ総数が1となる
        /// </summary>
        [TestMethod]
        public void WhenTotalIsZero_PagesNumIsOne()
        {
            // Arrange
            var model = new PagerModel
            {
                PageIndex = 0,
                Total = 0,
                PageSize = 20,
            };

            // Assert
            Assert.AreEqual(1, model.PagesNum);
        }

        /// <summary>
        /// 項目数>0の場合ページ総数が正しく算出される
        /// </summary>
        /// <param name="total"></param>
        /// <param name="pagesNum"></param>
        [TestMethod]
        [DataRow(19, 1, DisplayName = "項目数 < ページ内項目数")]
        [DataRow(20, 1, DisplayName = "項目数 = ページ内項目数")]
        [DataRow(21, 2, DisplayName = "項目数 = ページ内項目数 + 1")]
        [DataRow(30, 2, DisplayName = "ページ内項目数 + 1 < 項目数 < ページ内項目数 * 2")]
        public void WhenTotalIsLargerThanZero_PagesNumIs(int total, int pagesNum)
        {
            // Arrange
            var model = new PagerModel
            {
                PageIndex = 0,
                PageSize = 20,
                Total = total
            };

            // Assert
            Assert.AreEqual(pagesNum, model.PagesNum);
        }
    }
}