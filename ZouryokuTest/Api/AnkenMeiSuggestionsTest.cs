using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model.Model;
using System.Text.Json;
using Zouryoku.Api;
using static Zouryoku.Utils.StringUtil;

namespace ZouryokuTest.Api
{
    /// <summary>
    /// 案件名のオートコンプリート候補取得APIのテストクラス
    /// </summary>
    [TestClass]
    public class AnkenMeiSuggestionsTest : BaseInMemoryDbContextTest
    {
        // ======================================
        // 定数
        // ======================================

        /// <summary>
        /// 検索用案件名称
        /// </summary>
        const string SearchName = "1aｱ";

        // ======================================
        // 補助メソッド
        // ======================================

        // コントローラー関連
        // --------------------------------------

        /// <summary>
        /// レスポンスから案件名のリストを取得する
        /// </summary>
        /// <param name="result"><see cref="JsonResult"/>にキャストしたレスポンス</param>
        /// <returns>案件名のリスト</returns>
        private List<string> GetAnkenNamesFromResult(JsonResult result)
        {
            var json = result.Value!.ToJson();
            var items = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json)!;
            return [.. items.Select(item => item["label"])];
        }

        /// <summary>
        /// インメモリDBを備えたコントローラーを作成する
        /// </summary>
        /// <returns>コントローラーのインスタンス</returns>
        private AnkenMeiSuggestionsController CreateController() => new(db);

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

            db.AddRange(new Anken
            {
                Id = 1,
                Name = "案件1",
                SearchName = "1Aア",
            });
            db.SaveChanges();

            // Act
            var response = await controller.GetSuggestionsAsync(term);

            // Assert
            var ankenNames = GetAnkenNamesFromResult((JsonResult)response);
            Assert.IsNotNull(ankenNames);
            Assert.IsEmpty(ankenNames);
        }

        // 検索処理
        // --------------------------------------

        /// <summary>
        /// 正常系：検索条件にヒットするデータを取得すること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetSuggestionsAsync_検索条件にヒット_データを取得()
        {
            // Arrange
            var controller = CreateController();

            db.AddRange(new Anken
            {
                Name = "案件",
                SearchName = NormalizeString(SearchName),
            });
            db.SaveChanges();

            // Act
            var response = await controller.GetSuggestionsAsync(SearchName);

            // Assert
            var ankenNames = GetAnkenNamesFromResult((JsonResult)response);
            var ankenName = ankenNames.Single();
            Assert.AreEqual("案件", ankenName);
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

            db.AddRange(new Anken
            {
                Name = "案件",
                SearchName = NormalizeString(SearchName),
            });
            db.SaveChanges();
            // テストデータ内の検索用カラムに存在しないワードを作成
            var term = SearchName + "0";

            // Act
            var response = await controller.GetSuggestionsAsync(term);

            // Assert
            var ankenNames = GetAnkenNamesFromResult((JsonResult)response);
            var isExist = ankenNames.Any();
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
            db.AddRange(new Anken
            {
                Id = 1,
                Name = "案件1",
                SearchName = "案件",
            },
            new Anken
            {
                Id = 2,
                Name = "案件2",
                SearchName = "案件",
            },
            new Anken
            {
                Id = 3,
                Name = "案件3",
                SearchName = "案件",
            },
            new Anken
            {
                Id = 4,
                Name = "案件4",
                SearchName = "案件",
            },
            new Anken
            {
                Id = 5,
                Name = "案件5",
                SearchName = "案件",
            },
            new Anken
            {
                Id = 6,
                Name = "案件6",
                SearchName = "案件",
            });
            db.SaveChanges();

            // Act
            // テストデータ内のすべてのデータに共通する案件名称を設定
            var response = await controller.GetSuggestionsAsync("案件");

            // Assert
            var ankenNames = GetAnkenNamesFromResult((JsonResult)response);
            Assert.HasCount(5, ankenNames);
        }

        /// <summary>
        /// 正常系：案件名称の昇順で取得していること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSuggestions_案件名称の昇順で取得()
        {
            // Arrange
            var controller = CreateController();

            db.AddRange(new Anken
            {
                Id = 1,
                Name = "2",
                SearchName = "案件",
            },
            new Anken
            {
                Id = 2,
                Name = "3",
                SearchName = "案件",
            },
            new Anken
            {
                Id = 3,
                Name = "1",
                SearchName = "案件",
            });
            db.SaveChanges();

            // Act
            // テストデータ内のすべてのデータに共通する案件名称を設定
            var response = await controller.GetSuggestionsAsync("案件");

            // Assert
            var actualAnkenNames = GetAnkenNamesFromResult((JsonResult)response);
            // テストデータの作り方から期待される案件名称の並び順を作成
            List<string> expectedAnkenNames = ["1", "2", "3"];
            CollectionAssert.AreEqual(expectedAnkenNames, actualAnkenNames);
        }

        /// <summary>
        /// 正常系：同一案件名称を集約すること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSuggestions_同一案件名を集約()
        {
            // Arrange
            var controller = CreateController();

            db.AddRange(new Anken
            {
                Id = 1,
                Name = "案件",
                SearchName = "案件1",
            },
            new Anken
            {
                Id = 2,
                Name = "案件",
                SearchName = "案件2",
            });
            db.SaveChanges();

            // Act
            // テストデータ内のすべてのデータに共通する案件名称を設定
            var response = await controller.GetSuggestionsAsync("案件");

            // Assert
            var ankenNames = GetAnkenNamesFromResult((JsonResult)response);
            Assert.HasCount(1, ankenNames);
        }
    }
}