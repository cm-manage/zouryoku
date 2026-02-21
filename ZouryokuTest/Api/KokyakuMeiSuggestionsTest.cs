using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Zouryoku.Api;
using ZouryokuTest.Builder;
using static Zouryoku.Utils.StringUtil;

namespace ZouryokuTest.Api
{
    /// <summary>
    /// 顧客名のオートコンプリート候補取得APIのテストクラス
    /// </summary>
    [TestClass]
    public class KokyakuMeiSuggestionsTest : BaseInMemoryDbContextTest
    {
        // ======================================
        // 定数
        // ======================================

        /// <summary>
        /// 検索用顧客名称
        /// </summary>
        const string SearchName = "１ａｱ";
        /// <summary>
        /// 検索用顧客名称カナ
        /// </summary>
        const string SearchNameKana = "２ｂｲ";

        // ======================================
        // 補助メソッド
        // ======================================

        // コントローラー関連
        // --------------------------------------

        /// <summary>
        /// レスポンスから顧客名のリストを取得する
        /// </summary>
        /// <param name="result">（<see cref="JsonResult"/>にキャストしたレスポンス</param>
        /// <returns>顧客名のリスト</returns>
        private List<string> GetCustomerNamesFromResult(JsonResult result)
        {
            var json = result.Value!.ToJson();
            var items = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json)!;
            return [.. items.Select(item => item["label"])];
        }

        /// <summary>
        /// インメモリDBを備えたコントローラーを作成する
        /// </summary>
        /// <returns>コントローラーのインスタンス</returns>
        private KokyakuMeiSuggestionsController CreateController() => new(db);

        // テストデータ作成関連
        // --------------------------------------

        /// <summary>
        /// 検索条件の確認用テストデータをインメモリDBに作成する
        /// </summary>
        private void CreateDataForSearchTest()
        {
            // 顧客会社マスタ
            // 正規化されてるかの確認を行うために、正規化済みのカラムを設定
            db.Add(new KokyakuKaishaBuilder()
                .WithSearchName(NormalizeString(SearchName))
                .WithSearchNameKana(NormalizeString(SearchNameKana))
                .Build());
        }

        /// <summary>
        /// 取得件数確認用のデータを作成する
        /// </summary>
        /// <param name="count">テストデータの件数</param>
        private void CreateDataForCountTest(int count)
        {
            // 顧客会社マスタ
            db.AddRange(Enumerable.Range(1, count).Select(
                i => new KokyakuKaishaBuilder()
                    .WithId(i)
                    .WithName(GetKokyakuNameWithNumber(i))  // 取得時に重複削除が働かないように一意の顧客名称を設定
                    .Build()
            ).ToList());
        }

        /// <summary>
        /// 並び順の確認用のテストデータを作成する
        /// 顧客名称カナの昇順でのみ、顧客名称が2, 3, 1と並ぶ
        /// </summary>
        private void CreateDataForSortTest()
        {
            // 顧客会社マスタ
            db.AddRange(Enumerable.Range(1, 3).Select(
                i => new KokyakuKaishaBuilder()
                    .WithId(i)
                    .WithName(i.ToString())
                    .WithNameKana(((i + 1) % 3).ToString())      // 他のカラムの並び順と異なるように顧客名カナを設定
                    .WithSearchName(GetKokyakuNameWithNumber(i)) // 検索にヒットさせるため共通部分を持つように設定
                    .WithSearchNameKana(i.ToString())
                    .Build()
            ).ToList());
        }

        /// <summary>
        /// 集約条件の確認用のテストデータを作成する
        /// </summary>
        private void CreateDataForDistinctTest()
        {
            // 顧客会社マスタ
            // 顧客名称はビルダーのデフォルト値を使用する
            // 他の名称関係のカラムは相異なるように設定
            db.AddRange(Enumerable.Range(1, 3).Select(
                i => new KokyakuKaishaBuilder()
                    .WithId(i)
                    .WithNameKana(i.ToString())
                    .WithSearchName(GetKokyakuNameWithNumber(i)) // 検索にヒットさせるため共通部分を持つように設定
                    .WithSearchNameKana(i.ToString())
                    .Build()
            ).ToList());
        }

        /// <summary>
        /// 番号のサフィックスを付加した顧客名称を取得する
        /// </summary>
        /// <param name="number">番号</param>
        /// <returns></returns>
        private string GetKokyakuNameWithNumber(long number) => "株式会社サンプル" + number.ToString("D2");

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
            var kokyakuNames = GetCustomerNamesFromResult((JsonResult)response);
            Assert.IsNotNull(kokyakuNames);
            Assert.IsEmpty(kokyakuNames);
        }

        // 検索処理
        // --------------------------------------

        /// <summary>
        /// 正常系：検索条件にヒットするデータを取得すること
        /// </summary>
        /// <param name="term">検索ワード</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(SearchName, DisplayName = "検索用顧客名称にヒット")]
        [DataRow(SearchNameKana, DisplayName = "検索用顧客名称カナにヒット")]
        public async Task GetSuggestionsAsync_検索条件にヒット_データを取得(string term)
        {
            // Arrange
            var controller = CreateController();
            CreateDataForSearchTest();
            db.SaveChanges();

            // Act
            var response = await controller.GetSuggestionsAsync(term);

            // Assert
            var kokyakuNames = GetCustomerNamesFromResult((JsonResult)response);
            var kokyakuName = kokyakuNames.Single();
            Assert.AreEqual("株式会社サンプル", kokyakuName);
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
            var term = SearchName + "0";

            // Act
            var response = await controller.GetSuggestionsAsync(term);

            // Assert
            var kokyakuNames = GetCustomerNamesFromResult((JsonResult)response);
            var isExist = kokyakuNames.Any();
            Assert.IsFalse(isExist);
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
            // テストデータ内のすべてのデータに共通する顧客名称を設定
            var response = await controller.GetSuggestionsAsync("株式");

            // Assert
            var kokyakuNames = GetCustomerNamesFromResult((JsonResult)response);
            Assert.HasCount(5, kokyakuNames);
        }

        /// <summary>
        /// 正常系：顧客名称カナの昇順で取得していること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSuggestions_顧客名称カナの昇順で取得()
        {
            // Arrange
            var controller = CreateController();
            CreateDataForSortTest();
            db.SaveChanges();

            // Act
            // テストデータ内のすべてのデータに共通する顧客名称を設定
            var response = await controller.GetSuggestionsAsync("株式");

            // Assert
            var actualKokyakuNames = GetCustomerNamesFromResult((JsonResult)response);
            // テストデータの作り方から期待される顧客名称の並び順を作成
            List<string> expectedKokyakuNames = ["2", "3", "1"];
            CollectionAssert.AreEqual(expectedKokyakuNames, actualKokyakuNames);
        }

        /// <summary>
        /// 正常系：同一顧客名称を集約すること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSuggestions_同一顧客名を集約()
        {
            // Arrange
            var controller = CreateController();
            CreateDataForDistinctTest();
            db.SaveChanges();

            // Act
            // テストデータ内のすべてのデータに共通する顧客名称を設定
            var response = await controller.GetSuggestionsAsync("株式");

            // Assert
            var kokyakuNames = GetCustomerNamesFromResult((JsonResult)response);
            Assert.HasCount(1, kokyakuNames);
        }
    }
}
