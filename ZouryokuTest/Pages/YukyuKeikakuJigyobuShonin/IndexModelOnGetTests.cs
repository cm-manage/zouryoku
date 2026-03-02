using Zouryoku.Utils;
using static Model.Enums.EmployeeAuthority;
using static Model.Enums.LeavePlanStatus;

namespace ZouryokuTest.Pages.YukyuKeikakuJigyobuShonin
{
    /// <summary>
    /// 計画有給休暇事業部承認画面のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetTests : IndexModelOnGetTestsBase
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 異常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 異常:
        /// #01 初期表示
        /// ログインユーザー社員マスタ
        /// 「部署ID：1」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：2」
        /// 「ID：2、親ID：1」
        /// </summary>
        [TestMethod(DisplayName = """
            #01 初期表示
            ログインユーザー社員マスタ
            「部署ID：1」

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

            var result = await model.OnGetAsync(); // Act
            AssertRedirectError(string.Format(Const.ErrorRead, "部署マスタ"), result); // Assert
        }

        /// <summary>
        /// 異常:
        /// #02 初期表示
        /// ログインユーザー社員マスタ
        /// 「部署ID：1」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：2」
        /// </summary>
        [TestMethod(DisplayName = """
            #02 初期表示
            ログインユーザー社員マスタ
            「部署ID：1」

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

            var result = await model.OnGetAsync(); // Act
            AssertRedirectError(string.Format(Const.ErrorRead, "部署マスタ"), result); // Assert
        }

        /// <summary>
        /// 異常:
        /// #03 初期表示
        /// ログインユーザー社員マスタ
        /// 「部署ID：1」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// </summary>
        [TestMethod(DisplayName = """
            #03 初期表示
            ログインユーザー社員マスタ
            「部署ID：1」

            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            """)]
        public async Task OnGetAsync_部門長が存在しない場合_エラー()
        {
            // Arrange
            var loginUserSyain = CreateSyainWithBusyo();
            db.AddRange(loginUserSyain);
            await db.SaveChangesAsync();
            var model = CreateModel(loginUserSyain);

            var result = await model.OnGetAsync(); // Act
            AssertRedirectError(string.Format(Const.ErrorRead, "部署マスタ"), result); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 正常系テストメソッド（一般ユーザ）
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 正常:
        /// #04 初期表示
        /// ログインユーザー社員マスタ
        /// 「ID：5、部署ID：5、社員権限：空」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// ├「ID：2、親ID：1、部署Baseの部門長ID：空」
        /// └「ID：3、親ID：1、部署Baseの部門長ID：3」
        /// 　├「ID：4、親ID：3、部署Baseの部門長ID：4」
        /// 　└「ID：5、親ID：3、部署Baseの部門長ID：空」
        /// 　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
        /// 　　└「ID：7、親ID：5、部署Baseの部門長ID：空」
        ///
        /// 計画有給休暇データあり
        /// ①「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：1」のBaseID」
        /// ②「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：2」のBaseID」
        /// ③「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：3」のBaseID」
        /// ④「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：4」のBaseID」
        /// ⑤「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：5」のBaseID」
        /// ⑥「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：6」のBaseID」
        /// ⑦「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：7」のBaseID」
        /// </summary>
        [TestMethod(DisplayName = """
            #04 初期表示
            ログインユーザー社員マスタ
            「ID：5、部署ID：5、社員権限：空」

            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            ├「ID：2、親ID：1、部署Baseの部門長ID：空」
            └「ID：3、親ID：1、部署Baseの部門長ID：3」
            　├「ID：4、親ID：3、部署Baseの部門長ID：4」
            　└「ID：5、親ID：3、部署Baseの部門長ID：空」
            　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
            　　└「ID：7、親ID：5、部署Baseの部門長ID：空」

            計画有給休暇データあり
            ①「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：1」のBaseID」
            ②「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：2」のBaseID」
            ③「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：3」のBaseID」
            ④「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：4」のBaseID」
            ⑤「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：5」のBaseID」
            ⑥「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：6」のBaseID」
            ⑦「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：7」のBaseID」
            """)]
        public async Task OnGetAsync_一般ユーザ_計画有給休暇データが取得される()
        {
            // Arrange
            var yukyuNendoOfThisYear = CreateYukyuNendo(true);
            var syainSet = CreateSyainSet();
            var yukyuKeikakuSet = InitializeSyainYukyuKeikakus(syainSet, yukyuNendoOfThisYear);
            db.Add(yukyuNendoOfThisYear);
            db.AddRange(syainSet);
            await db.SaveChangesAsync();
            var model = CreateModel(syainSet.Syain5);

            await model.OnGetAsync(); // Act
            AssertPopulatesList(
                yukyuKeikakuSet.YukyuKeikaku3.Id, yukyuKeikakuSet.YukyuKeikaku5.Id,
                yukyuKeikakuSet.YukyuKeikaku7A.Id, yukyuKeikakuSet.YukyuKeikaku7B.Id,
                false, model.LoginUserJigyoubuShoninViewModel); // Assert
        }

        /// <summary>
        /// 正常:
        /// #05 初期表示
        /// ログインユーザー社員マスタ
        /// 「ID：5、部署ID：5、社員権限：空」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// ├「ID：2、親ID：1、部署Baseの部門長ID：空」
        /// └「ID：3、親ID：1、部署Baseの部門長ID：3」
        /// 　├「ID：4、親ID：3、部署Baseの部門長ID：4」
        /// 　└「ID：5、親ID：3、部署Baseの部門長ID：空」
        /// 　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
        /// 　　└「ID：7、親ID：5、部署Baseの部門長ID：空」
        ///
        /// 計画有給休暇データあり
        /// ①「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：1」のBaseID」
        /// ②「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：2」のBaseID」
        /// ③「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：3」のBaseID」
        /// ④「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：4」のBaseID」
        /// ⑤「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：5」のBaseID」
        /// ⑥「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：6」のBaseID」
        /// ⑦「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：7」のBaseID」
        /// </summary>
        [TestMethod(DisplayName = """
            #05 初期表示
            ログインユーザー社員マスタ
            「ID：5、部署ID：5、社員権限：空」

            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            ├「ID：2、親ID：1、部署Baseの部門長ID：空」
            └「ID：3、親ID：1、部署Baseの部門長ID：3」
            　├「ID：4、親ID：3、部署Baseの部門長ID：4」
            　└「ID：5、親ID：3、部署Baseの部門長ID：空」
            　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
            　　└「ID：7、親ID：5、部署Baseの部門長ID：空」

            計画有給休暇データあり
            ①「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：1」のBaseID」
            ②「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：2」のBaseID」
            ③「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：3」のBaseID」
            ④「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：4」のBaseID」
            ⑤「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：5」のBaseID」
            ⑥「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：6」のBaseID」
            ⑦「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：7」のBaseID」
            """)]
        public async Task OnGetAsync_一般ユーザ_今年度フラグFALSEの計画有給休暇データが取得されない()
        {
            // Arrange
            var yukyuNendoOfNotThisYear = CreateYukyuNendo(false);
            var syainSet = CreateSyainSet();
            InitializeSyainYukyuKeikakus(syainSet, yukyuNendoOfNotThisYear);
            db.Add(yukyuNendoOfNotThisYear);
            db.AddRange(syainSet);
            await db.SaveChangesAsync();
            var model = CreateModel(syainSet.Syain5);

            await model.OnGetAsync(); // Act
            AssertSetsEmptyList(false, model.LoginUserJigyoubuShoninViewModel); // Assert
        }

        /// <summary>
        /// 正常:
        /// #06 初期表示
        /// ログインユーザー社員マスタ
        /// 「ID：5、部署ID：5、社員権限：空」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// ├「ID：2、親ID：1、部署Baseの部門長ID：空」
        /// └「ID：3、親ID：1、部署Baseの部門長ID：3」
        /// 　├「ID：4、親ID：3、部署Baseの部門長ID：4」
        /// 　└「ID：5、親ID：3、部署Baseの部門長ID：空」
        /// 　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
        /// 　　└「ID：7、親ID：5、部署Baseの部門長ID：空」
        ///
        /// 計画有給休暇データなし
        /// ①
        /// ②
        /// ③
        /// ④
        /// ⑤
        /// ⑥
        /// ⑦
        /// </summary>
        [TestMethod(DisplayName = """
            #06 初期表示
            ログインユーザー社員マスタ
            「ID：5、部署ID：5、社員権限：空」

            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            ├「ID：2、親ID：1、部署Baseの部門長ID：空」
            └「ID：3、親ID：1、部署Baseの部門長ID：3」
            　├「ID：4、親ID：3、部署Baseの部門長ID：4」
            　└「ID：5、親ID：3、部署Baseの部門長ID：空」
            　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
            　　└「ID：7、親ID：5、部署Baseの部門長ID：空」

            計画有給休暇データなし
            ①
            ②
            ③
            ④
            ⑤
            ⑥
            ⑦
            """)]
        public async Task OnGetAsync_一般ユーザ_存在しない計画有給休暇データが取得されない()
        {
            // Arrange
            var syainSet = CreateSyainSet();
            db.AddRange(syainSet);
            await db.SaveChangesAsync();
            var model = CreateModel(syainSet.Syain5);

            await model.OnGetAsync(); // Act
            AssertSetsEmptyList(false, model.LoginUserJigyoubuShoninViewModel); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 正常系テストメソッド（部門長）
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 正常:
        /// #07 初期表示
        /// ログインユーザー社員マスタ
        /// 「ID：3、部署ID：3、社員権限：計画休暇承認(32768)」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// ├「ID：2、親ID：1、部署Baseの部門長ID：空」
        /// └「ID：3、親ID：1、部署Baseの部門長ID：3」
        /// 　├「ID：4、親ID：3、部署Baseの部門長ID：4」
        /// 　└「ID：5、親ID：3、部署Baseの部門長ID：空」
        /// 　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
        /// 　　└「ID：7、親ID：5、部署Baseの部門長ID：空」
        ///
        /// 計画有給休暇データあり
        /// ①「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：1」のBaseID」
        /// ②「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：2」のBaseID」
        /// ③「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：3」のBaseID」
        /// ④「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：4」のBaseID」
        /// ⑤「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：5」のBaseID」
        /// ⑥「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：6」のBaseID」
        /// ⑦「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：7」のBaseID」
        /// </summary>
        [TestMethod(DisplayName = """
            #07 初期表示
            ログインユーザー社員マスタ
            「ID：3、部署ID：3、社員権限：計画休暇承認(32768)」

            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            ├「ID：2、親ID：1、部署Baseの部門長ID：空」
            └「ID：3、親ID：1、部署Baseの部門長ID：3」
            　├「ID：4、親ID：3、部署Baseの部門長ID：4」
            　└「ID：5、親ID：3、部署Baseの部門長ID：空」
            　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
            　　└「ID：7、親ID：5、部署Baseの部門長ID：空」

            計画有給休暇データあり
            ①「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：1」のBaseID」
            ②「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：2」のBaseID」
            ③「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：3」のBaseID」
            ④「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：4」のBaseID」
            ⑤「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：5」のBaseID」
            ⑥「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：6」のBaseID」
            ⑦「有給年度ID：有給年度「今年度フラグ：TRUE」のID、社員BaseID：社員マスタ「部署ID：7」のBaseID」
            """)]
        public async Task OnGetAsync_部門長_計画有給休暇データが取得される()
        {
            // Arrange
            var yukyuNendoOfThisYear = CreateYukyuNendo(true);
            var syainSet = CreateSyainSet();
            syainSet.Syain3.SetKengen(計画休暇承認);
            var yukyuKeikakuSet = InitializeSyainYukyuKeikakus(syainSet, yukyuNendoOfThisYear);
            db.Add(yukyuNendoOfThisYear);
            db.AddRange(syainSet);
            await db.SaveChangesAsync();
            var model = CreateModel(syainSet.Syain3);

            await model.OnGetAsync(); // Act
            AssertPopulatesList(
                yukyuKeikakuSet.YukyuKeikaku3.Id, yukyuKeikakuSet.YukyuKeikaku5.Id,
                yukyuKeikakuSet.YukyuKeikaku7A.Id, yukyuKeikakuSet.YukyuKeikaku7B.Id,
                true, model.LoginUserJigyoubuShoninViewModel); // Assert
        }

        /// <summary>
        /// 正常:
        /// #08 初期表示
        /// ログインユーザー社員マスタ
        /// 「ID：3、部署ID：3、社員権限：計画休暇承認(32768)」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// ├「ID：2、親ID：1、部署Baseの部門長ID：空」
        /// └「ID：3、親ID：1、部署Baseの部門長ID：3」
        /// 　├「ID：4、親ID：3、部署Baseの部門長ID：4」
        /// 　└「ID：5、親ID：3、部署Baseの部門長ID：空」
        /// 　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
        /// 　　└「ID：7、親ID：5、部署Baseの部門長ID：空」
        ///
        /// 計画有給休暇データあり
        /// ①「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：1」のBaseID」
        /// ②「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：2」のBaseID」
        /// ③「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：3」のBaseID」
        /// ④「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：4」のBaseID」
        /// ⑤「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：5」のBaseID」
        /// ⑥「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：6」のBaseID」
        /// ⑦「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：7」のBaseID」
        /// </summary>
        [TestMethod(DisplayName = """
            #08 初期表示
            ログインユーザー社員マスタ
            「ID：3、部署ID：3、社員権限：計画休暇承認(32768)」

            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            ├「ID：2、親ID：1、部署Baseの部門長ID：空」
            └「ID：3、親ID：1、部署Baseの部門長ID：3」
            　├「ID：4、親ID：3、部署Baseの部門長ID：4」
            　└「ID：5、親ID：3、部署Baseの部門長ID：空」
            　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
            　　└「ID：7、親ID：5、部署Baseの部門長ID：空」

            計画有給休暇データあり
            ①「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：1」のBaseID」
            ②「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：2」のBaseID」
            ③「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：3」のBaseID」
            ④「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：4」のBaseID」
            ⑤「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：5」のBaseID」
            ⑥「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：6」のBaseID」
            ⑦「有給年度ID：有給年度「今年度フラグ：FALSE」のID、社員BaseID：社員マスタ「部署ID：7」のBaseID」
            """)]
        public async Task OnGetAsync_部門長_今年度フラグFALSEの計画有給休暇データが取得されない()
        {
            // Arrange
            var yukyuNendoOfNotThisYear = CreateYukyuNendo(false);
            var syainSet = CreateSyainSet();
            syainSet.Syain3.SetKengen(計画休暇承認);
            InitializeSyainYukyuKeikakus(syainSet, yukyuNendoOfNotThisYear);
            db.Add(yukyuNendoOfNotThisYear);
            db.AddRange(syainSet);
            await db.SaveChangesAsync();
            var model = CreateModel(syainSet.Syain3);

            await model.OnGetAsync(false); // Act
            AssertSetsEmptyList(true, model.LoginUserJigyoubuShoninViewModel); // Assert
        }

        /// <summary>
        /// 正常:
        /// #09 初期表示
        /// ログインユーザー社員マスタ
        /// 「ID：3、部署ID：3、社員権限：計画休暇承認(32768)」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// ├「ID：2、親ID：1、部署Baseの部門長ID：空」
        /// └「ID：3、親ID：1、部署Baseの部門長ID：3」
        /// 　├「ID：4、親ID：3、部署Baseの部門長ID：4」
        /// 　└「ID：5、親ID：3、部署Baseの部門長ID：空」
        /// 　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
        /// 　　└「ID：7、親ID：5、部署Baseの部門長ID：空」
        ///
        /// 計画有給休暇データなし
        /// ①
        /// ②
        /// ③
        /// ④
        /// ⑤
        /// ⑥
        /// ⑦
        /// </summary>
        [TestMethod(DisplayName = """
            #09 初期表示
            ログインユーザー社員マスタ
            「ID：3、部署ID：3、社員権限：計画休暇承認(32768)」

            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            ├「ID：2、親ID：1、部署Baseの部門長ID：空」
            └「ID：3、親ID：1、部署Baseの部門長ID：3」
            　├「ID：4、親ID：3、部署Baseの部門長ID：4」
            　└「ID：5、親ID：3、部署Baseの部門長ID：空」
            　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
            　　└「ID：7、親ID：5、部署Baseの部門長ID：空」

            計画有給休暇データなし
            ①
            ②
            ③
            ④
            ⑤
            ⑥
            ⑦
            """)]
        public async Task OnGetAsync_部門長_存在しない計画有給休暇データが取得されない()
        {
            // Arrange
            var syainSet = CreateSyainSet();
            syainSet.Syain3.SetKengen(計画休暇承認);
            db.AddRange(syainSet);
            await db.SaveChangesAsync();
            var model = CreateModel(syainSet.Syain3);

            await model.OnGetAsync(); // Act
            AssertSetsEmptyList(true, model.LoginUserJigyoubuShoninViewModel); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 正常系テストメソッド（人財）
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 正常:
        /// #10 初期表示
        /// ログインユーザー社員マスタ
        /// 「ID：5、部署ID：5、社員権限：指示最終承認者(8192)」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// ├「ID：2、親ID：1、部署Baseの部門長ID：空」
        /// └「ID：3、親ID：1、部署Baseの部門長ID：3」
        /// 　├「ID：4、親ID：3、部署Baseの部門長ID：4」
        /// 　└「ID：5、親ID：3、部署Baseの部門長ID：空」
        /// 　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
        /// 　　└「ID：7、親ID：5、部署Baseの部門長ID：空」
        ///
        /// 計画有給休暇データあり
        /// 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：人財承認待ち」
        /// 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：事業部承認待ち」
        /// 「有給年度ID：有給年度「今年度フラグ：FALSE」のID、ステータス：人財承認待ち」
        /// 「有給年度ID：有給年度「今年度フラグ：FALSE」のID、ステータス：事業部承認待ち」
        /// </summary>
        [TestMethod(DisplayName = """
            #10 初期表示
            ログインユーザー社員マスタ
            「ID：5、部署ID：5、社員権限：指示最終承認者(8192)」

            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            ├「ID：2、親ID：1、部署Baseの部門長ID：空」
            └「ID：3、親ID：1、部署Baseの部門長ID：3」
            　├「ID：4、親ID：3、部署Baseの部門長ID：4」
            　└「ID：5、親ID：3、部署Baseの部門長ID：空」
            　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
            　　└「ID：7、親ID：5、部署Baseの部門長ID：空」

            計画有給休暇データあり
            「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：人財承認待ち」
            「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：事業部承認待ち」
            「有給年度ID：有給年度「今年度フラグ：FALSE」のID、ステータス：人財承認待ち」
            「有給年度ID：有給年度「今年度フラグ：FALSE」のID、ステータス：事業部承認待ち」
            """)]
        public async Task OnGetAsync_人財_今年度フラグTRUEの計画有給休暇データのみ取得される()
        {
            // Arrange
            var yukyuNendoOfThisYear = CreateYukyuNendo(true);
            var yukyuNendoOfNotThisYear = CreateYukyuNendo(false);
            var syainSet = CreateSyainSet();
            syainSet.Syain5.SetKengen(指示最終承認者);
            var yukyuKeikakus = InitializeSyainYukyuKeikakus(
                // 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：人財承認待ち」
                (syainSet.Syain7A, yukyuNendoOfThisYear, 人財承認待ち, SyainSetConst.Syain7AMonth),
                // 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：人財承認待ち」の順序テスト用1
                (syainSet.Syain7B, yukyuNendoOfThisYear, 人財承認待ち, SyainSetConst.Syain7BMonth),
                // 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：人財承認待ち」の順序テスト用2
                (syainSet.Syain6, yukyuNendoOfThisYear, 人財承認待ち, SyainSetConst.Syain6Month),
                // 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：事業部承認待ち」
                (syainSet.Syain1, yukyuNendoOfThisYear, 事業部承認待ち, SyainSetConst.Syain1Month),
                // 「有給年度ID：有給年度「今年度フラグ：FALSE」のID、ステータス：人財承認待ち」
                (syainSet.Syain2, yukyuNendoOfNotThisYear, 人財承認待ち, SyainSetConst.Syain2Month),
                // 「有給年度ID：有給年度「今年度フラグ：FALSE」のID、ステータス：事業部承認待ち」
                (syainSet.Syain3, yukyuNendoOfNotThisYear, 事業部承認待ち, SyainSetConst.Syain3Month));
            db.AddRange(yukyuNendoOfThisYear, yukyuNendoOfNotThisYear);
            db.AddRange(syainSet);
            db.AddRange(yukyuKeikakus);
            await db.SaveChangesAsync();
            var model = CreateModel(syainSet.Syain5);

            await model.OnGetAsync(true); // Act

            // Assert
            AssertPopulatesListWhenJinzai(
                yukyuKeikakus[2].Id, yukyuKeikakus[0].Id, yukyuKeikakus[1].Id, model.LoginUserJigyoubuShoninViewModel);
        }

        /// <summary>
        /// 正常:
        /// #11 初期表示
        /// ログインユーザー社員マスタ
        /// 「ID：3、部署ID：3、社員権限：指示最終承認者(8192)」
        ///
        /// 部署マスタデータあり
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// ├「ID：2、親ID：1、部署Baseの部門長ID：空」
        /// └「ID：3、親ID：1、部署Baseの部門長ID：3」
        /// 　├「ID：4、親ID：3、部署Baseの部門長ID：4」
        /// 　└「ID：5、親ID：3、部署Baseの部門長ID：空」
        /// 　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
        /// 　　└「ID：7、親ID：5、部署Baseの部門長ID：空」
        ///
        /// 計画有給休暇データあり
        /// 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：人財承認待ち」
        /// 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：事業部承認待ち」
        /// 「有給年度ID：有給年度「今年度フラグ：FALSE」のID、ステータス：人財承認待ち」
        /// 「有給年度ID：有給年度「今年度フラグ：FALSE」のID、ステータス：事業部承認待ち」
        /// </summary>
        [TestMethod(DisplayName = """
            #11 初期表示
            ログインユーザー社員マスタ
            「ID：3、部署ID：3、社員権限：指示最終承認者(8192)」

            部署マスタデータあり
            「ID：1、親ID：空、部署Baseの部門長ID：空」
            ├「ID：2、親ID：1、部署Baseの部門長ID：空」
            └「ID：3、親ID：1、部署Baseの部門長ID：3」
            　├「ID：4、親ID：3、部署Baseの部門長ID：4」
            　└「ID：5、親ID：3、部署Baseの部門長ID：空」
            　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
            　　└「ID：7、親ID：5、部署Baseの部門長ID：空」

            計画有給休暇データあり
            「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：人財承認待ち」
            「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：事業部承認待ち」
            「有給年度ID：有給年度「今年度フラグ：FALSE」のID、ステータス：人財承認待ち」
            「有給年度ID：有給年度「今年度フラグ：FALSE」のID、ステータス：事業部承認待ち」
            """)]
        public async Task OnGetAsync_部門長かつ人財_今年度フラグTRUEの計画有給休暇データのみ取得される()
        {
            // Arrange
            var yukyuNendoOfThisYear = CreateYukyuNendo(true);
            var yukyuNendoOfNotThisYear = CreateYukyuNendo(false);
            var syainSet = CreateSyainSet();
            syainSet.Syain3.SetKengen(指示最終承認者);
            var yukyuKeikakus = InitializeSyainYukyuKeikakus(
                // 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：人財承認待ち」
                (syainSet.Syain7A, yukyuNendoOfThisYear, 人財承認待ち, SyainSetConst.Syain7AMonth),
                // 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：人財承認待ち」の順序テスト用1
                (syainSet.Syain7B, yukyuNendoOfThisYear, 人財承認待ち, SyainSetConst.Syain7BMonth),
                // 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：人財承認待ち」の順序テスト用2
                (syainSet.Syain6, yukyuNendoOfThisYear, 人財承認待ち, SyainSetConst.Syain6Month),
                // 「有給年度ID：有給年度「今年度フラグ：TRUE」のID、ステータス：事業部承認待ち」
                (syainSet.Syain1, yukyuNendoOfThisYear, 事業部承認待ち, SyainSetConst.Syain1Month),
                // 「有給年度ID：有給年度「今年度フラグ：FALSE」のID、ステータス：人財承認待ち」
                (syainSet.Syain2, yukyuNendoOfNotThisYear, 人財承認待ち, SyainSetConst.Syain2Month),
                // 「有給年度ID：有給年度「今年度フラグ：FALSE」のID、ステータス：事業部承認待ち」
                (syainSet.Syain3, yukyuNendoOfNotThisYear, 事業部承認待ち, SyainSetConst.Syain3Month));
            db.AddRange(yukyuNendoOfThisYear, yukyuNendoOfNotThisYear);
            db.AddRange(syainSet);
            db.AddRange(yukyuKeikakus);
            await db.SaveChangesAsync();
            var model = CreateModel(syainSet.Syain3);

            await model.OnGetAsync(true); // Act

            // Assert
            AssertPopulatesListWhenJinzai(
                yukyuKeikakus[2].Id, yukyuKeikakus[0].Id, yukyuKeikakus[1].Id, model.LoginUserJigyoubuShoninViewModel);
        }
    }
}
