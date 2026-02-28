using Model.Model;
using Zouryoku.Utils;

namespace ZouryokuTest.Pages.AnkenJohoHyoji
{
    [TestClass]
    public class IndexModelOnGetCheckExistsJuchuTests : IndexModelTestsBase
    {
        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------
        // =================================================================
        /// <summary>
        /// 受注件名リンク押下: 存在するKINGS受注IDを指定した場合、正常レスポンスを返却すること
        /// </summary>
        // ================================================================
        [TestMethod]
        public async Task OnGetCheckExistsJuchuAsync_存在するKINGS受注ID_正常レスポンスを返却()
        {
            // ---------- Arrange ----------
            // シード：KINGS受注データ
            var kingsJuchu = new KingsJuchu(){
                Id = 1,
                Bukken = "",
                ProjectNo = "",
                SekouBumonCd = "",
                HiyouShubetuCdName = "",
                SearchBukken = ""
            };

            // 必要データ登録
            SeedEntities(kingsJuchu);

            var model = CreateModel();

            // ---------- Act ----------
            var result = await model.OnGetCheckExistsJuchuAsync(kingsJuchu.Id);

            // ---------- Assert ----------
            // 正常ステータスが返却されていること
            AssertSuccess(result);
        }

        // -----------------------------------------------------
        // 異常系テストケース
        // -----------------------------------------------------
        // =================================================================
        /// <summary>
        /// 受注件名リンク押下: 存在しないKINGS受注IDを指定した場合、エラーレスポンスを返却すること
        /// </summary>
        // ================================================================
        [TestMethod]
        public async Task OnGetCheckExistsJuchuAsync_存在しないKINGS受注ID_エラーレスポンスを返却()
        {
            // ---------- Arrange ----------
            // シード：KINGS受注データ
            var kingsJuchu = new KingsJuchu(){
                Id = 1,
                Bukken = "",
                ProjectNo = "",
                SekouBumonCd = "",
                HiyouShubetuCdName = "",
                SearchBukken = ""
            };

            // 必要データ登録
            SeedEntities(kingsJuchu);

            var model = CreateModel();

            // ---------- Act ----------
            var result = await model.OnGetCheckExistsJuchuAsync(9999); // 存在しないID

            // ---------- Assert ----------
            AssertError(result, Const.ErrorSelectedDataNotExists);
        }
    }
}
