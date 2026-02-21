using Zouryoku.Pages.KinmuNippouKakunin;
using Zouryoku.Utils;

namespace ZouryokuTest.Pages.KinmuNippouKakunin
{
    /// <summary>
    /// 勤務日報確認画面のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetSearchTests : IndexModelCreateViewModelTestsBase
    {
        /// <summary>
        /// テスト用の任意の年月日。2025年4月1日とする。
        /// </summary>
        private static DateOnly FirstDay => new DateOnly(2025, 4, 1);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 異常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 異常: #06 初期表示 検索条件**あり**「対象社員ID：存在しない社員ID」
        /// </summary>
        [TestMethod(DisplayName = "#06 初期表示 検索条件**あり**「対象社員ID：存在しない社員ID」")]
        public async Task OnGetSearchAsync_対象社員が存在しない場合_エラー()
        {
            // Arrange
            var loginUserSyain = CreateSyainWithBusyo();
            db.Add(loginUserSyain);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            // Act
            // ログインユーザーの社員IDに1を加算した値を対象社員IDとすることで、存在しない社員IDを指定する
            var result = await model.OnGetSearchAsync(CreateDaysQuery(loginUserSyain.Id + 1));

            AssertError(result, Const.ErrorSelectedDataNotExists); // Assert
        }

        /// <summary>
        /// 異常:
        /// #07 初期表示
        /// 検索条件**あり**
        /// 「対象年月：2025/04、対象社員ID：1」
        ///
        /// 日報実績データあり
        /// 「社員ID：1、実績年月日：2025/04/01」
        /// 「社員ID：1、実績年月日：2025/04/01」
        /// </summary>
        [TestMethod(DisplayName = """
            #07 初期表示
            検索条件**あり**
            「対象年月：2025/04、対象社員ID：1」

            日報実績データあり
            「社員ID：1、実績年月日：2025/04/01」
            「社員ID：1、実績年月日：2025/04/01」
            """)]
        public async Task OnGetSearchAsync_実績年月日が重複した場合_エラー()
        {
            // Arrange
            var loginUserSyain = CreateSyainWithBusyo();
            var syukkinKubun = CreateSyukkinKubun();
            var nippou1 = CreateNippou(loginUserSyain, FirstDay, syukkinKubun);
            var nippou2 = CreateNippou(loginUserSyain, FirstDay, syukkinKubun);
            db.AddRange(loginUserSyain, syukkinKubun, nippou1, nippou2);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            var result = await model.OnGetSearchAsync(CreateDaysQuery(loginUserSyain.Id, FirstDay)); // Act
            AssertError(result, string.Format(Const.ErrorRead, "日報実績")); // Assert
        }

        /// <summary>
        /// 異常:
        /// #08 初期表示
        /// 検索条件**あり**
        /// 「対象年月：2025/04、対象社員ID：1」
        ///
        /// 社員マスタデータあり
        /// 「ID：1、部署ID：1」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：2」
        /// 「ID：2、親ID：1」
        /// </summary>
        [TestMethod(DisplayName = """
            #08 初期表示
            検索条件**あり**
            「対象年月：2025/04、対象社員ID：1」

            社員マスタデータあり
            「ID：1、部署ID：1」

            部署マスタデータあり
            「ID：1、親ID：2」
            「ID：2、親ID：1」
            """)]
        public async Task OnGetSearchAsync_親子部署が循環した場合_エラー()
        {
            // Arrange
            var busyo1 = CreateBusyo();
            var busyo2 = CreateBusyo();
            busyo1.Oya = busyo2;
            busyo2.Oya = busyo1;
            var loginUserSyain = CreateSyainWithBusyo(busyo1);
            db.AddRange(busyo1, busyo2, loginUserSyain);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            var result = await model.OnGetSearchAsync(CreateDaysQuery(loginUserSyain.Id, FirstDay)); // Act
            AssertError(result, string.Format(Const.ErrorRead, "部署マスタ")); // Assert
        }

        /// <summary>
        /// 異常:
        /// #09 初期表示
        /// 検索条件**あり**
        /// 「対象年月：2025/04、対象社員ID：1」
        ///
        /// 社員マスタデータあり
        /// 「ID：1、部署ID：1」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：2」
        /// </summary>
        [TestMethod(DisplayName = """
            #09 初期表示
            検索条件**あり**
            「対象年月：2025/04、対象社員ID：1」

            社員マスタデータあり
            「ID：1、部署ID：1」

            部署マスタデータあり
            「ID：1、親ID：2」
            """)]
        public async Task OnGetSearchAsync_親部署が存在しない場合_エラー()
        {
            // Arrange
            var busyo1 = CreateBusyo();
            busyo1.OyaId = 2;
            var loginUserSyain = CreateSyainWithBusyo(busyo1);
            db.AddRange(busyo1, loginUserSyain);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            var result = await model.OnGetSearchAsync(CreateDaysQuery(loginUserSyain.Id, FirstDay)); // Act
            AssertError(result, string.Format(Const.ErrorRead, "部署マスタ")); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 正常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 正常:
        /// #10 初期表示
        /// 検索条件**あり**
        /// 「対象年月：2025/04、対象社員ID：1」
        ///
        /// 社員マスタデータあり
        /// 「ID：1、部署ID：3」
        ///
        /// 日報実績データ**あり**
        /// ①「社員ID：1、実績年月日：2025/03/31」
        /// ②「社員ID：1、実績年月日：2025/04/01」
        /// ③「社員ID：1、実績年月日：2025/04/30」
        /// ④「社員ID：1、実績年月日：2025/05/01」
        /// ⑤「社員ID：2、実績年月日：2025/03/31」
        /// ⑥「社員ID：2、実績年月日：2025/04/01」
        /// ⑦「社員ID：2、実績年月日：2025/04/30」
        /// ⑧「社員ID：2、実績年月日：2025/05/01」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// ├「ID：2、親ID：1、部署Baseの部門長ID：空」
        /// └「ID：3、親ID：1、部署Baseの部門長ID：3」
        /// 　├「ID：4、親ID：3、部署Baseの部門長ID：4」
        /// 　└「ID：5、親ID：3、部署Baseの部門長ID：空」
        /// </summary>
        [TestMethod(DisplayName = """
            #10 初期表示
            検索条件**あり**
            「対象年月：2025/04、対象社員ID：1」

            社員マスタデータあり
            「ID：1、部署ID：3」

            日報実績データ**あり**
            ①「社員ID：1、実績年月日：2025/03/31」
            ②「社員ID：1、実績年月日：2025/04/01」
            ③「社員ID：1、実績年月日：2025/04/30」
            ④「社員ID：1、実績年月日：2025/05/01」
            ⑤「社員ID：2、実績年月日：2025/03/31」
            ⑥「社員ID：2、実績年月日：2025/04/01」
            ⑦「社員ID：2、実績年月日：2025/04/30」
            ⑧「社員ID：2、実績年月日：2025/05/01」

            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            ├「ID：2、親ID：1、部署Baseの部門長ID：空」
            └「ID：3、親ID：1、部署Baseの部門長ID：3」
            　├「ID：4、親ID：3、部署Baseの部門長ID：4」
            　└「ID：5、親ID：3、部署Baseの部門長ID：空」
            """)]
        public async Task OnGetSearchAsync_日報実績データあり_日報実績データが取得される()
        {
            // Arrange
            var busyos = CreateBusyoHierarchy();
            var loginUserSyain = CreateSyainWithBusyo(busyos.Busyo3);
            var otherSyain = CreateSyainWithBusyo();
            var nippous = NippouSet.Create(loginUserSyain, otherSyain, FirstDay);
            var hikadoubi = CreateHikadoubi(nippous.Nippou2.NippouYmd);
            db.AddRange(busyos);
            db.AddRange(loginUserSyain, otherSyain, hikadoubi);
            db.AddRange(nippous);
            await db.SaveChangesAsync();
            IndexModel.DaysViewModel? viewModel = null;
            var model = CreateModel(loginUserSyain, vm => viewModel = vm);

            await model.OnGetSearchAsync(CreateDaysQuery(loginUserSyain.Id, FirstDay)); // Act
            AssertPopulatesList(loginUserSyain, busyos, FirstDay, viewModel); // Assert
        }

        /// <summary>
        /// 正常:
        /// #11 初期表示
        /// 検索条件**あり**
        /// 「対象年月：2025/04、対象社員ID：1」
        ///
        /// 社員マスタデータあり
        /// 「ID：1、部署ID：3」
        ///
        /// 日報実績データ**なし**
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// ├「ID：2、親ID：1、部署Baseの部門長ID：空」
        /// └「ID：3、親ID：1、部署Baseの部門長ID：3」
        /// 　├「ID：4、親ID：3、部署Baseの部門長ID：4」
        /// 　└「ID：5、親ID：3、部署Baseの部門長ID：空」
        /// </summary>
        [TestMethod(DisplayName = """
            #11 初期表示
            検索条件**あり**
            「対象年月：2025/04、対象社員ID：1」

            社員マスタデータあり
            「ID：1、部署ID：3」

            日報実績データ**なし**

            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            ├「ID：2、親ID：1、部署Baseの部門長ID：空」
            └「ID：3、親ID：1、部署Baseの部門長ID：3」
            　├「ID：4、親ID：3、部署Baseの部門長ID：4」
            　└「ID：5、親ID：3、部署Baseの部門長ID：空」
            """)]
        public async Task OnGetSearchAsync_日報実績データなし_日報実績データが取得されない()
        {
            // Arrange
            var busyos = CreateBusyoHierarchy();
            var loginUserSyain = CreateSyainWithBusyo(busyos.Busyo3);
            db.AddRange(busyos);
            db.Add(loginUserSyain);
            await db.SaveChangesAsync();
            IndexModel.DaysViewModel? viewModel = null;
            var model = CreateModel(loginUserSyain, vm => viewModel = vm);

            await model.OnGetSearchAsync(CreateDaysQuery(loginUserSyain.Id, FirstDay)); // Act
            AssertSetsEmptyList(loginUserSyain, busyos, FirstDay, viewModel); // Assert
        }
    }
}
