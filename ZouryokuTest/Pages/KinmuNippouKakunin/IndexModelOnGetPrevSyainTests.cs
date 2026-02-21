using Zouryoku.Utils;

namespace ZouryokuTest.Pages.KinmuNippouKakunin
{
    /// <summary>
    /// 勤務日報確認画面のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetPrevSyainTests : IndexModelTestsBase
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 異常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 異常: #12 人送りボタン(左) 検索条件あり「対象社員ID：存在しない社員ID」
        /// </summary>
        [TestMethod(DisplayName = "#12 人送りボタン(左) 検索条件あり「対象社員ID：存在しない社員ID」")]
        public async Task OnGetPrevSyainAsync_対象社員が存在しない場合_エラー()
        {
            // Arrange
            var loginUserSyain = CreateSyainWithBusyo();
            db.Add(loginUserSyain);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            // Act
            // ログインユーザーの社員IDに1を加算した値を対象社員IDとすることで、存在しない社員IDを指定する
            var result = await model.OnGetPrevSyainAsync(CreateDaysQuery(loginUserSyain.Id + 1));

            AssertError(result, Const.ErrorSelectedDataNotExists); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 正常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 正常:
        /// #13 人送りボタン(左)
        /// 検索条件あり
        /// 「対象社員ID：1」
        ///
        /// 社員マスタデータあり
        /// 「ID：1、部署コード：1、並び順序：1、社員番号：3」
        /// 「ID：2、部署コード：1、並び順序：2、社員番号：2」
        /// 「ID：3、部署コード：1、並び順序：2、社員番号：1」
        /// 「ID：4、部署コード：2」
        /// </summary>
        [TestMethod(DisplayName = """
            #13 人送りボタン(左)
            検索条件あり
            「対象社員ID：1」

            社員マスタデータあり
            「ID：1、部署コード：1、並び順序：1、社員番号：3」
            「ID：2、部署コード：1、並び順序：2、社員番号：2」
            「ID：3、部署コード：1、並び順序：2、社員番号：1」
            「ID：4、部署コード：2」
            """)]
        public async Task OnGetPrevSyainAsync_対象社員ID1_2が取得される()
        {
            // Arrange
            var loginUserSyain = await Seed4SyainsForOrderAsync();
            var model = CreateModel(loginUserSyain);

            var result = await model.OnGetPrevSyainAsync(CreateDaysQuery(1)); // Act
            AssertSuccessJson(2, result); // Assert
        }

        /// <summary>
        /// 正常:
        /// #14 人送りボタン(左)
        /// 検索条件あり
        /// 「対象社員ID：2」
        ///
        /// 社員マスタデータあり
        /// 「ID：1、部署コード：1、並び順序：1、社員番号：3」
        /// 「ID：2、部署コード：1、並び順序：2、社員番号：2」
        /// 「ID：3、部署コード：1、並び順序：2、社員番号：1」
        /// 「ID：4、部署コード：2」
        /// </summary>
        [TestMethod(DisplayName = """
            #14 人送りボタン(左)
            検索条件あり
            「対象社員ID：2」

            社員マスタデータあり
            「ID：1、部署コード：1、並び順序：1、社員番号：3」
            「ID：2、部署コード：1、並び順序：2、社員番号：2」
            「ID：3、部署コード：1、並び順序：2、社員番号：1」
            「ID：4、部署コード：2」
            """)]
        public async Task OnGetPrevSyainAsync_対象社員ID2_3が取得される()
        {
            // Arrange
            var loginUserSyain = await Seed4SyainsForOrderAsync();
            var model = CreateModel(loginUserSyain);

            var result = await model.OnGetPrevSyainAsync(CreateDaysQuery(2)); // Act
            AssertSuccessJson(3, result); // Assert
        }

        /// <summary>
        /// 正常:
        /// #15 人送りボタン(左)
        /// 検索条件あり
        /// 「対象社員ID：3」
        ///
        /// 社員マスタデータあり「ID：1、部署コード：1、並び順序：1、社員番号：3」
        /// 「ID：2、部署コード：1、並び順序：2、社員番号：2」
        /// 「ID：3、部署コード：1、並び順序：2、社員番号：1」
        /// 「ID：4、部署コード：2」
        /// </summary>
        [TestMethod(DisplayName = """
            #15 人送りボタン(左)
            検索条件あり
            「対象社員ID：3」

            社員マスタデータあり「ID：1、部署コード：1、並び順序：1、社員番号：3」
            「ID：2、部署コード：1、並び順序：2、社員番号：2」
            「ID：3、部署コード：1、並び順序：2、社員番号：1」
            「ID：4、部署コード：2」
            """)]
        public async Task OnGetPrevSyainAsync_対象社員ID3_1が取得される()
        {
            // Arrange
            var loginUserSyain = await Seed4SyainsForOrderAsync();
            var model = CreateModel(loginUserSyain);

            var result = await model.OnGetPrevSyainAsync(CreateDaysQuery(3)); // Act
            AssertSuccessJson(1, result); // Assert
        }

        /// <summary>
        /// 正常:
        /// #16 人送りボタン(左)
        /// 検索条件あり
        /// 「対象社員ID：1」
        ///
        /// 社員マスタデータあり
        /// 「ID：1、部署コード：1」
        /// 「ID：2、部署コード：2」
        /// </summary>
        [TestMethod(DisplayName = """
            #16 人送りボタン(左)
            検索条件あり
            「対象社員ID：1」

            社員マスタデータあり
            「ID：1、部署コード：1」
            「ID：2、部署コード：2」
            """)]
        public async Task OnGetPrevSyainAsync_同一部署コード社員が一件のみの場合_1が取得される()
        {
            // Arrange
            var loginUserSyain = await Seed2SyainsForCirculateAsync();
            var model = CreateModel(loginUserSyain);

            var result = await model.OnGetPrevSyainAsync(CreateDaysQuery(1)); // Act
            AssertSuccessJson(1, result); // Assert
        }
    }
}
