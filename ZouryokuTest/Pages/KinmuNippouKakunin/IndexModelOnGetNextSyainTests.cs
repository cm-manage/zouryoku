using Zouryoku.Utils;

namespace ZouryokuTest.Pages.KinmuNippouKakunin
{
    /// <summary>
    /// 勤務日報確認画面のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetNextSyainTests : IndexModelTestsBase
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 異常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 異常: #17 人送りボタン(右) 検索条件あり「対象社員ID：存在しない社員ID」
        /// </summary>
        [TestMethod(DisplayName = "#17 人送りボタン(右) 検索条件あり「対象社員ID：存在しない社員ID」")]
        public async Task OnGetNextSyainAsync_対象社員が存在しない場合_エラー()
        {
            // Arrange
            var loginUserSyain = CreateSyainWithBusyo();
            db.Add(loginUserSyain);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            // Act
            // ログインユーザーの社員IDに1を加算した値を対象社員IDとすることで、存在しない社員IDを指定する
            var result = await model.OnGetNextSyainAsync(CreateDaysQuery(loginUserSyain.Id + 1));

            AssertErrorJson(result, Const.ErrorSelectedDataNotExists); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 正常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 正常:
        /// #18 人送りボタン(右)
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
            #18 人送りボタン(右)
            検索条件あり
            「対象社員ID：1」

            社員マスタデータあり
            「ID：1、部署コード：1、並び順序：1、社員番号：3」
            「ID：2、部署コード：1、並び順序：2、社員番号：2」
            「ID：3、部署コード：1、並び順序：2、社員番号：1」
            「ID：4、部署コード：2」
            """)]
        public async Task OnGetNextSyainAsync_対象社員ID1_3が取得される()
        {
            // Arrange
            var loginUserSyain = await Seed4SyainsForOrderAsync();
            var model = CreateModel(loginUserSyain);

            var result = await model.OnGetNextSyainAsync(CreateDaysQuery(1)); // Act
            AssertSuccessJson(3, result); // Assert
        }

        /// <summary>
        /// 正常:
        /// #19 人送りボタン(右)
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
            #19 人送りボタン(右)
            検索条件あり
            「対象社員ID：2」

            社員マスタデータあり
            「ID：1、部署コード：1、並び順序：1、社員番号：3」
            「ID：2、部署コード：1、並び順序：2、社員番号：2」
            「ID：3、部署コード：1、並び順序：2、社員番号：1」
            「ID：4、部署コード：2」
            """)]
        public async Task OnGetNextSyainAsync_対象社員ID2_1が取得される()
        {
            // Arrange
            var loginUserSyain = await Seed4SyainsForOrderAsync();
            var model = CreateModel(loginUserSyain);

            var result = await model.OnGetNextSyainAsync(CreateDaysQuery(2)); // Act
            AssertSuccessJson(1, result); // Assert
        }

        /// <summary>
        /// 正常:
        /// #20 人送りボタン(右)
        /// 検索条件あり
        /// 「対象社員ID：3」
        ///
        /// 社員マスタデータあり「ID：1、部署コード：1、並び順序：1、社員番号：3」
        /// 「ID：2、部署コード：1、並び順序：2、社員番号：2」
        /// 「ID：3、部署コード：1、並び順序：2、社員番号：1」
        /// 「ID：4、部署コード：2」
        /// </summary>
        [TestMethod(DisplayName = """
            #20 人送りボタン(右)
            検索条件あり
            「対象社員ID：3」

            社員マスタデータあり「ID：1、部署コード：1、並び順序：1、社員番号：3」
            「ID：2、部署コード：1、並び順序：2、社員番号：2」
            「ID：3、部署コード：1、並び順序：2、社員番号：1」
            「ID：4、部署コード：2」
            """)]
        public async Task OnGetNextSyainAsync_対象社員ID3_2が取得される()
        {
            // Arrange
            var loginUserSyain = await Seed4SyainsForOrderAsync();
            var model = CreateModel(loginUserSyain);

            var result = await model.OnGetNextSyainAsync(CreateDaysQuery(3)); // Act
            AssertSuccessJson(2, result); // Assert
        }

        /// <summary>
        /// 正常:
        /// #21 人送りボタン(右)
        /// 検索条件あり
        /// 「対象社員ID：1」
        ///
        /// 社員マスタデータあり
        /// 「ID：1、部署コード：1」
        /// 「ID：2、部署コード：2」
        /// </summary>
        [TestMethod(DisplayName = """
            #21 人送りボタン(右)
            検索条件あり
            「対象社員ID：1」

            社員マスタデータあり
            「ID：1、部署コード：1」
            「ID：2、部署コード：2」
            """)]
        public async Task OnGetNextSyainAsync_同一部署コード社員が一件のみの場合_1が取得される()
        {
            // Arrange
            var loginUserSyain = await Seed2SyainsForCirculateAsync();
            var model = CreateModel(loginUserSyain);

            var result = await model.OnGetNextSyainAsync(CreateDaysQuery(1)); // Act
            AssertSuccessJson(1, result); // Assert
        }
    }
}
