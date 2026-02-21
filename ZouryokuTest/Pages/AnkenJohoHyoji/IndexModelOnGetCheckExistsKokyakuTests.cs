using Zouryoku.Utils;
using ZouryokuTest.Builder;

namespace ZouryokuTest.Pages.AnkenJohoHyoji
{
    [TestClass]
    public class IndexModelOnGetCheckExistsKokyakuTests : IndexModelTestsBase
    {
        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------
        // =================================================================
        /// <summary>
        /// 顧客情報リンク押下: 存在する顧客IDを指定した場合、正常レスポンスを返却すること
        /// </summary>
        // ================================================================
        [TestMethod]
        public async Task OnGetCheckExistsKokyakuAsync_存在する顧客ID_正常レスポンスを返却()
        {
            // ---------- Arrange ----------
            // シード：顧客会社データ
            var kokyaku = new KokyakuKaishaBuilder()
                .WithId(1)
                .Build();

            // 必要データ登録
            SeedEntities(kokyaku);

            var model = CreateModel();

            // ---------- Act ----------
            var result = await model.OnGetCheckExistsKokyakuAsync(kokyaku.Id);

            // ---------- Assert ----------
            // 正常ステータスが返却されていること
            AssertSuccess(result);
        }

        // -----------------------------------------------------
        // 異常系テストケース
        // -----------------------------------------------------
        // =================================================================
        /// <summary>
        /// 初期表示: 存在しない顧客IDを指定した場合、エラーレスポンスを返却すること
        /// </summary>
        // ================================================================
        [TestMethod]
        public async Task OnGetCheckExistsKokyakuAsync_存在しない顧客ID_エラーレスポンスを返却()
        {
            // ---------- Arrange ----------
            // シード：顧客会社データ
            var kokyaku = new KokyakuKaishaBuilder()
                .WithId(1)
                .Build();

            // 必要データ登録
            SeedEntities(kokyaku);

            var model = CreateModel();

            // ---------- Act ----------
            var result = await model.OnGetCheckExistsKokyakuAsync(9999); // 存在しないID

            // ---------- Assert ----------
            AssertError(result, Const.ErrorSelectedDataNotExists);
        }
    }
}
