using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model.Model;
using System.Text.Json;
using Zouryoku.Api;
using static Zouryoku.Utils.StringUtil;

namespace ZouryokuTest.Api
{
    /// <summary>
    /// 受注件名のオートコンプリート候補取得APIのテストクラス
    /// </summary>
    [TestClass]
    public class JuchuBukkenMeiSuggestionsTest : BaseInMemoryDbContextTest
    {
        // ======================================
        // 定数
        // ======================================

        /// <summary>
        /// 件名
        /// </summary>
        const string Bukken = "件名サンプル";

        /// <summary>
        /// 検索用件名
        /// </summary>
        const string SearchBukken = "１ａｱ";

        // ======================================
        // 補助メソッド
        // ======================================

        // コントローラー関連
        // --------------------------------------

        /// <summary>
        /// レスポンスから受注件名のリストを取得する
        /// </summary>
        /// <param name="result">（<see cref="JsonResult"/>にキャストしたレスポンス</param>
        /// <returns>受注件名のリスト</returns>
        private List<string> GetBukkenNamesFromResult(JsonResult result)
        {
            var json = ObjectExtensions.ToJson(result.Value!);
            var items = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json)!;
            return [.. items.Select(item => item["label"])];
        }

        /// <summary>
        /// インメモリDBを備えたコントローラーを作成する
        /// </summary>
        /// <returns>コントローラーのインスタンス</returns>
        private JuchuBukkenMeiSuggestionsController CreateController() => new(db);

        // テストデータ作成関連
        // --------------------------------------

        /// <summary>
        /// 検索条件の確認用テストデータをインメモリDBに作成する
        /// </summary>
        private void CreateDataForSearchTest()
        {
            db.Add(new KingsJuchu
            {
                Id = 1,
                JucYmd = new(2026, 2, 13),
                EntYmd = new(2026, 2, 14),
                Bukken = Bukken,
                JucKin = 0,
                ChaYmd = new(2026, 2, 15),
                ProjectNo = "",
                SekouBumonCd = "",
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = "",
                IsGenkaToketu = false,
                Nendo = 2025,
                BusyoId = 0,
                SearchBukken = NormalizeString(SearchBukken),
            });
        }

        /// <summary>
        /// 集約条件の確認用のテストデータを作成する
        /// </summary>
        private void CreateDataForDistinctTest()
        {
            db.AddRange(Enumerable.Range(1, 3).Select(
                i => new KingsJuchu
                {
                    Id = i,
                    JucYmd = new(2026, 2, 13),
                    EntYmd = new(2026, 2, 14),
                    Bukken = Bukken,
                    JucKin = 0,
                    ChaYmd = new(2026, 2, 15),
                    ProjectNo = "",
                    SekouBumonCd = "",
                    HiyouShubetuCd = 0,
                    HiyouShubetuCdName = "",
                    IsGenkaToketu = false,
                    Nendo = 2025,
                    BusyoId = 0,
                    SearchBukken = GetBukkenNameWithNumber(i), // 検索にヒットさせるため共通部分を持つように設定
                }
            ).ToList());
        }

        /// <summary>
        /// 取得件数確認のデータを作成する
        /// </summary>
        /// <param name="count">テストデータの件数</param>
        private void CreateDataForCountTest(int count)
        {
            db.AddRange(Enumerable.Range(1, count).Select(
                i => new KingsJuchu
                {
                    Id = i,
                    JucYmd = new(2026, 2, 13),
                    EntYmd = new(2026, 2, 14),
                    Bukken = GetBukkenNameWithNumber(i), // 重複排除を防ぐためにサフィックスを付加
                    JucKin = 0,
                    ChaYmd = new(2026, 2, 15),
                    ProjectNo = "",
                    SekouBumonCd = "",
                    HiyouShubetuCd = 0,
                    HiyouShubetuCdName = "",
                    IsGenkaToketu = false,
                    Nendo = 2025,
                    BusyoId = 0,
                    SearchBukken = NormalizeString(SearchBukken),
                }
            ).ToList());
        }

        /// <summary>
        /// 並び順確認用のデータを作成する
        /// </summary>
        private void CreateDataForSortTest()
        {
            db.Add(new KingsJuchu
            {
                Id = 1,
                JucYmd = new(2026, 2, 13),
                EntYmd = new(2026, 2, 14),
                Bukken = "い",
                JucKin = 0,
                ChaYmd = new(2026, 2, 15),
                ProjectNo = "",
                SekouBumonCd = "",
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = "",
                IsGenkaToketu = false,
                Nendo = 2025,
                BusyoId = 0,
                SearchBukken = NormalizeString(SearchBukken),
            });
            db.Add(new KingsJuchu
            {
                Id = 2,
                JucYmd = new(2026, 2, 13),
                EntYmd = new(2026, 2, 14),
                Bukken = "あ",
                JucKin = 0,
                ChaYmd = new(2026, 2, 15),
                ProjectNo = "",
                SekouBumonCd = "",
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = "",
                IsGenkaToketu = false,
                Nendo = 2025,
                BusyoId = 0,
                SearchBukken = NormalizeString(SearchBukken),
            });
            db.Add(new KingsJuchu
            {
                Id = 3,
                JucYmd = new(2026, 2, 13),
                EntYmd = new(2026, 2, 14),
                Bukken = "う",
                JucKin = 0,
                ChaYmd = new(2026, 2, 15),
                ProjectNo = "",
                SekouBumonCd = "",
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = "",
                IsGenkaToketu = false,
                Nendo = 2025,
                BusyoId = 0,
                SearchBukken = NormalizeString(SearchBukken),
            });
        }

        /// <summary>
        /// 番号のサフィックスを付加した件名を取得する
        /// </summary>
        /// <param name="number">番号</param>
        /// <returns></returns>
        private string GetBukkenNameWithNumber(long number) => Bukken + number.ToString("D2");

        // ======================================
        // テストメソッド
        // ======================================

        // パラメータチェック
        // --------------------------------------

        /// <summary>
        /// 異常系：検索ワードが不正な場合に空リストを返却すること
        /// </summary>
        /// <param name="term">入力値</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(null, DisplayName = "NULLの場合")]
        [DataRow("", DisplayName = "空文字の場合")]
        public async Task GetSuggestionsAsync_入力値が不正_空リストを返却(string? term)
        {
            // Arrange
            var controller = CreateController();
            CreateDataForSearchTest();
            db.SaveChanges();

            // Act
            var response = await controller.GetSuggestionsAsync(term);

            // Assert
            var bukkenNames = GetBukkenNamesFromResult((JsonResult)response);
            Assert.IsNotNull(bukkenNames);
            Assert.IsEmpty(bukkenNames);
        }

        // 検索処理
        // --------------------------------------

        /// <summary>
        /// 正常系：検索条件にヒットするデータを取得すること
        /// </summary>
        /// <param name="term">検索ワード</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(SearchBukken, DisplayName = "正規化で一致")]
        [DataRow("ａ", DisplayName = "正規化で部分一致")]
        public async Task GetSuggestionsAsync_検索条件にヒット_データを取得(string term)
        {
            // Arrange
            var controller = CreateController();
            CreateDataForSearchTest();
            db.SaveChanges();

            // Act
            var response = await controller.GetSuggestionsAsync(term);

            // Assert
            var bukkenNames = GetBukkenNamesFromResult((JsonResult)response);
            var bukkenName = bukkenNames.Single();
            Assert.AreEqual(Bukken, bukkenName);
        }

        /// <summary>
        /// 正常系：検索条件にヒットしないデータを取得しないこと
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetSuggestionsAsync_検索条件にヒットしない_データを取得しない()
        {
            // Arrange
            var controller = CreateController();
            CreateDataForSearchTest();
            db.SaveChanges();
            // テストデータ内の検索用カラムに存在しないワードを作成
            var term = SearchBukken + "0";

            // Act
            var response = await controller.GetSuggestionsAsync(term);

            // Assert
            var bukkenNames = GetBukkenNamesFromResult((JsonResult)response);
            var isExist = bukkenNames.Any();
            Assert.IsFalse(isExist);
        }

        // 集約
        // --------------------------------------

        /// <summary>
        /// 正常系：同一件名を集約すること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSuggestions_同一件名を集約()
        {
            // Arrange
            var controller = CreateController();
            CreateDataForDistinctTest();
            db.SaveChanges();

            // Act
            // テストデータ内のすべてのデータに共通する件名を設定
            var response = await controller.GetSuggestionsAsync(Bukken);

            // Assert
            var bukkenNames = GetBukkenNamesFromResult((JsonResult)response);
            Assert.HasCount(1, bukkenNames);
        }

        // 取得データ
        // --------------------------------------

        /// <summary>
        /// 正常系：取得件数が最大5件であること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSuggestions_最大5件を取得()
        {
            // Arrange
            var controller = CreateController();
            // 最大取得件数を超過するテストデータを作成
            CreateDataForCountTest(6);
            db.SaveChanges();

            // Act
            // テストデータ内のすべてのデータに共通する件名を設定
            var response = await controller.GetSuggestionsAsync(SearchBukken);

            // Assert
            var bukkenNames = GetBukkenNamesFromResult((JsonResult)response);
            Assert.HasCount(5, bukkenNames);
        }

        /// <summary>
        /// 正常系：受注件名の昇順で取得していること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSuggestions_受注件名の昇順で取得()
        {
            // Arrange
            var controller = CreateController();
            CreateDataForSortTest();
            db.SaveChanges();

            // Act
            // テストデータ内のすべてのデータに共通する件名を設定
            var response = await controller.GetSuggestionsAsync(SearchBukken);

            // Assert
            var actualBukkenNames = GetBukkenNamesFromResult((JsonResult)response);
            // テストデータの作り方から期待される件名の並び順を作成
            List<string> expectedBukkenNames = ["あ", "い", "う"];
            CollectionAssert.AreEqual(expectedBukkenNames, actualBukkenNames);
        }
    }
}
