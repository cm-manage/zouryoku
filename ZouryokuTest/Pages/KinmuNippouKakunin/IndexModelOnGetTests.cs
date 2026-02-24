using CommonLibrary.Extensions;
using Zouryoku.Utils;

namespace ZouryokuTest.Pages.KinmuNippouKakunin
{
    /// <summary>
    /// 勤務日報確認画面のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetTests : IndexModelCreateViewModelTestsBase
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 異常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 異常:
        /// #01 初期表示
        /// 検索条件**なし**
        /// 
        /// ログインユーザー社員マスタ
        /// 「ID：1」
        /// 
        /// 日報実績データあり
        /// 「社員ID：1、実績年月日：システム月の1日」
        /// 「社員ID：1、実績年月日：システム月の1日」
        /// </summary>
        [TestMethod(DisplayName = """
            #01 初期表示
            検索条件**なし**

            ログインユーザー社員マスタ
            「ID：1」

            日報実績データあり
            「社員ID：1、実績年月日：システム月の1日」
            「社員ID：1、実績年月日：システム月の1日」
            """)]
        public async Task OnGetAsync_実績年月日が重複した場合_エラー()
        {
            // Arrange
            var loginUserSyain = CreateSyainWithBusyo();
            var syukkinKubun = CreateSyukkinKubun();
            var nippou1 = CreateNippou(loginUserSyain, FirstDayOfMonth, syukkinKubun);
            var nippou2 = CreateNippou(loginUserSyain, FirstDayOfMonth, syukkinKubun);
            db.AddRange(loginUserSyain, syukkinKubun, nippou1, nippou2);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            await model.OnGetAsync(); // Act
            AssertModelStateErrors(string.Format(Const.ErrorRead, "日報実績"), model.ModelState); // Assert
        }

        /// <summary>
        /// 異常:
        /// #02 初期表示
        /// 検索条件**なし**
        ///
        /// ログインユーザー社員マスタ
        /// 「ID：1、部署ID：1」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：2」
        /// 「ID：2、親ID：1」
        /// </summary>
        [TestMethod(DisplayName = """
            #02 初期表示
            検索条件なし

            ログインユーザー社員マスタ
            「ID：1、部署ID：1」

            部署マスタデータあり
            「ID：1、親ID：2」
            「ID：2、親ID：1」
            """)]
        public async Task OnGetAsync_親子部署が循環した場合_エラー()
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

            await model.OnGetAsync(); // Act
            AssertModelStateErrors(string.Format(Const.ErrorRead, "部署マスタ"), model.ModelState); // Assert
        }

        /// <summary>
        /// 異常:
        /// #03 初期表示
        /// 検索条件**なし**
        ///
        /// ログインユーザー社員マスタ
        /// 「ID：1、部署ID：1」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：2」
        /// </summary>
        [TestMethod(DisplayName = """
            #03 初期表示
            検索条件なし

            ログインユーザー社員マスタ
            「ID：1、部署ID：1」

            部署マスタデータあり
            「ID：1、親ID：2」
            """)]
        public async Task OnGetAsync_親部署が存在しない場合_エラー()
        {
            // Arrange
            var busyo1 = CreateBusyo();
            busyo1.OyaId = 2;
            var loginUserSyain = CreateSyainWithBusyo(busyo1);
            db.AddRange(busyo1, loginUserSyain);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            await model.OnGetAsync(); // Act
            AssertModelStateErrors(string.Format(Const.ErrorRead, "部署マスタ"), model.ModelState); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 正常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 正常:
        /// #04 初期表示
        /// 検索条件**なし**
        ///
        /// ログインユーザー社員マスタ
        /// 「ID：1、部署ID：3」
        ///
        /// 日報実績データ**あり**
        /// ①「社員ID：1、実績年月日：システム前月の最終日」
        /// ②「社員ID：1、実績年月日：システム月の1日」
        /// ③「社員ID：1、実績年月日：システム月の最終日」
        /// ④「社員ID：1、実績年月日：システム次月の1日」
        /// ⑤「社員ID：2、実績年月日：システム前月の最終日」
        /// ⑥「社員ID：2、実績年月日：システム月の1日」
        /// ⑦「社員ID：2、実績年月日：システム月の最終日」
        /// ⑧「社員ID：2、実績年月日：システム次月の1日」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// ├「ID：2、親ID：1、部署Baseの部門長ID：空」
        /// └「ID：3、親ID：1、部署Baseの部門長ID：3」
        /// 　├「ID：4、親ID：3、部署Baseの部門長ID：4」
        /// 　└「ID：5、親ID：3、部署Baseの部門長ID：空」
        /// </summary>
        [TestMethod(DisplayName = """
            #04 初期表示
            検索条件**なし**

            ログインユーザー社員マスタ
            「ID：1、部署ID：3」

            日報実績データ**あり**
            ①「社員ID：1、実績年月日：システム前月の最終日」
            ②「社員ID：1、実績年月日：システム月の1日」
            ③「社員ID：1、実績年月日：システム月の最終日」
            ④「社員ID：1、実績年月日：システム次月の1日」
            ⑤「社員ID：2、実績年月日：システム前月の最終日」
            ⑥「社員ID：2、実績年月日：システム月の1日」
            ⑦「社員ID：2、実績年月日：システム月の最終日」
            ⑧「社員ID：2、実績年月日：システム次月の1日」

            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            ├「ID：2、親ID：1、部署Baseの部門長ID：空」
            └「ID：3、親ID：1、部署Baseの部門長ID：3」
            　├「ID：4、親ID：3、部署Baseの部門長ID：4」
            　└「ID：5、親ID：3、部署Baseの部門長ID：空」
            """)]
        public async Task OnGetAsync_日報実績データあり_日報実績データが取得される()
        {
            // Arrange
            var today = Today.ToDateOnly();
            var busyos = CreateBusyoHierarchy();
            var loginUserSyain = CreateSyainWithBusyo(busyos.Busyo3);
            var otherSyain = CreateSyainWithBusyo();
            var nippous = NippouSet.Create(loginUserSyain, otherSyain, today);
            var hikadoubi = CreateHikadoubi(nippous.Nippou2.NippouYmd);
            db.AddRange(busyos);
            db.AddRange(loginUserSyain, otherSyain, hikadoubi);
            db.AddRange(nippous);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            await model.OnGetAsync(); // Act
            AssertPopulatesList(loginUserSyain, busyos, today, model.TargetDaysViewModel); // Assert
        }

        /// <summary>
        /// 正常:
        /// #05 初期表示
        /// 検索条件**なし**
        ///
        /// ログインユーザー社員マスタ
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
            #05 初期表示
            検索条件**なし**

            ログインユーザー社員マスタ
            「ID：1、部署ID：3」

            日報実績データ**なし**

            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            ├「ID：2、親ID：1、部署Baseの部門長ID：空」
            └「ID：3、親ID：1、部署Baseの部門長ID：3」
            　├「ID：4、親ID：3、部署Baseの部門長ID：4」
            　└「ID：5、親ID：3、部署Baseの部門長ID：空」
            """)]
        public async Task OnGetAsync_日報実績データなし_日報実績データが取得されない()
        {
            // Arrange
            var today = Today.ToDateOnly();
            var busyos = CreateBusyoHierarchy();
            var loginUserSyain = CreateSyainWithBusyo(busyos.Busyo3);
            db.AddRange(busyos);
            db.Add(loginUserSyain);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            await model.OnGetAsync(); // Act
            AssertSetsEmptyList(loginUserSyain, busyos, today, model.TargetDaysViewModel); // Assert
        }
    }
}
