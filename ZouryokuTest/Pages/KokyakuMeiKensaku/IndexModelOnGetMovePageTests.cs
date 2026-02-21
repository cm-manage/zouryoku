using ZouryokuTest.Builder;

namespace ZouryokuTest.Pages.KokyakuMeiKensaku
{
    /// <summary>
    /// ページ移動機能のテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetMovePageTests : IndexModelTestBase
    {

        /// <summary>
        /// 全データを取得した際に想定される顧客会社IDのリストを作成する
        /// NがpageSize以下（2ページ目が不正なページ番号）の場合は1ページ目を取得した場合の想定リストを作成する
        /// NがpageSize超過（2ページ目がvalidなページ番号）の場合は2ページ目を取得した場合の想定リストを作成する
        /// </summary>
        /// <param name="dataCount">テスト内で作成したデータの個数</param>
        /// <param name="pageSize">テスト対象ページの最大表示データ数／ページ</param>
        /// <returns></returns>
        private List<long> GetExpectedCustomerIdList(int dataCount, int pageSize)
        {
            // 2ページ目に最初に現れるデータのインデックス
            var pageStartIndex = pageSize + 1;
            // 2ページ目の最後に現れるデータのインデックス
            var pageEndIndex = pageSize * 2;

            // データ総数に応じて取得想定のデータの範囲を計算する
            // 最初
            var startCustomerId = dataCount < pageStartIndex ? 1 : pageStartIndex;
            // 最後
            var endCustomerId = pageEndIndex < dataCount ? pageEndIndex : dataCount;
            // 個数
            var count = endCustomerId - startCustomerId + 1;

            return [.. Enumerable.Range(startCustomerId, count).Select(x => (long)x)];
        }

        // ======================================
        // 履歴参照時
        // ======================================

        /// <summary>
        /// 正常系：営業社員を含めてデータを取得すること
        /// </summary>
        /// <param name="customerId">対応するデータの顧客会社ID</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(1, DisplayName = "営業社員の有効開始日の境界値")]
        [DataRow(2, DisplayName = "営業社員の有効期限の代表値")]
        [DataRow(3, DisplayName = "営業社員の有効終了日の境界値")]
        public async Task OnGetMovePage_履歴参照時かつ営業社員が有効_データを取得(int customerId)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();

            // Act
            await model.OnGetMovePageAsync(0, 0, true);

            // Assert
            var target = model.Customers
                .SingleOrDefault(x => x.KokyakuKaishaId == customerId);
            Assert.IsNotNull(target);
            AssertCustomerVM(target, customerId);
        }

        /// <summary>
        /// 正常系：社員BASE IDに対応する営業社員の有効期限が不正でもデータを取得すること
        /// </summary>
        /// <param name="customerId">対象データの顧客会社ID</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(4, DisplayName = "社員有効開始日に関する代表値")]
        [DataRow(5, DisplayName = "社員有効開始日に関する境界値")]
        [DataRow(6, DisplayName = "社員有効終了日に関する境界値")]
        [DataRow(7, DisplayName = "社員有効終了日に関する代表値")]
        public async Task OnGetMovePage_履歴参照時かつ営業社員の有効期限が不正_データを取得(int customerId)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();

            // Act
            await model.OnGetMovePageAsync(0, 0, true);

            // Assert
            var target = model.Customers
                .SingleOrDefault(x => x.KokyakuKaishaId == customerId);
            Assert.IsNotNull(target);
            AssertCustomerVM(target, customerId, true);
        }

        /// <summary>
        /// 正常系：社員BASE IDに対応する営業社員が存在しなくてもデータを取得すること
        /// </summary>
        /// <param name="customerId">対応するデータの顧客会社ID</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(8, DisplayName = "営業社員BASE IDがNULLの場合")]
        [DataRow(9, DisplayName = "営業社員BASE IDに対応する社員BASEのデータが存在しないとき")]
        [DataRow(10, DisplayName = "営業社員BASE IDに対応する社員マスタのデータが存在しないとき")]
        public async Task OnGetMovePage_履歴参照時かつ営業社員BaseIDに対応する社員が存在しない_データを取得する(int customerId)
        {
            //Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();

            // Act
            await model.OnGetMovePageAsync(0, 0, true);

            // Assert
            var target = model.Customers
                .SingleOrDefault(x => x.KokyakuKaishaId == customerId);
            Assert.IsNotNull(target);
            AssertCustomerVM(target, customerId, true);
        }

        /// <summary>
        /// 正常系：参照履歴に対応する顧客会社が存在しないデータは取得しないこと
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePage_履歴参照時かつ対応する顧客会社が存在しない_データを取得しない()
        {
            //Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();

            // Act
            await model.OnGetMovePageAsync(0, 0, true);

            // Assert
            var isExist = model.Customers
                .Any(x => x.KokyakuKaishaId == 100);
            Assert.IsFalse(isExist);
        }

        /// <summary>
        /// 正常系：参照履歴の社員BASE IDがログインユーザーのものと一致しないデータは取得しないこと
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePage_履歴参照時かつログインユーザーのものでない_データを取得しない()
        {
            //Arrange
            var model = CreateModel();
            db.Add(new KokyakuKaisyaSansyouRirekiBuilder()
                .WithId(12)
                .WithKokyakuKaisyaId(1)
                .WithSyainBaseId(1)
                .WithSansyouTime(DateTime.Now.AddDays(-1))
                .Build());
            db.SaveChanges();

            // Act
            await model.OnGetMovePageAsync(0, 0, true);

            // Assert
            Assert.IsEmpty(model.Customers);
        }

        /// <summary>
        /// 正常系：取得データが参照時間の降順であること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePage_履歴参照時_データは参照時間の降順()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSort();
            db.SaveChanges();

            // Act
            await model.OnGetMovePageAsync(0, 0, true);

            // Assert
            List<long> expectedCustomerIds = [3, 2, 1];
            var actualCustomerIds = model.Customers
                .Select(x => x.KokyakuKaishaId)
                .ToList();
            CollectionAssert.AreEqual(expectedCustomerIds, actualCustomerIds);
        }

        /// <summary>
        /// 正常系：取得データが該当ページのものであること
        /// </summary>
        /// <param name="count">サンプルデータ数</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(10, DisplayName = "取得開始行数の代表値（取得範囲外）")]
        [DataRow(20, DisplayName = "取得開始行数の境界値（取得範囲外）")]
        [DataRow(21, DisplayName = "取得開始行数の境界値（取得範囲内）")]
        [DataRow(30, DisplayName = "取得行数の代表値（取得範囲内）")]
        [DataRow(40, DisplayName = "取得終了行数の境界値（取得範囲内）")]
        [DataRow(41, DisplayName = "取得終了行数の境界値（取得範囲外）")]
        [DataRow(50, DisplayName = "取得終了行数の代表値（取得範囲外）")]
        public async Task OnGetMovePage_履歴参照時_取得データが該当ページのもの(int count)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForAcquire(count);
            db.SaveChanges();

            // Act
            // 2ページ目の顧客情報を取得する（ページ番号 = 0、オフセット = 1で設定）
            await model.OnGetMovePageAsync(0, 1, true);

            // Assert
            var expectedCustomerIds = GetExpectedCustomerIdList(count, model.Pager.PageSize);
            var actualCustomerIds = model.Customers
                .Select(x => x.KokyakuKaishaId)
                .ToList();
            CollectionAssert.AreEqual(expectedCustomerIds, actualCustomerIds);
        }

        // ======================================
        // 顧客名検索時
        // ======================================

        /// <summary>
        /// 正常系：検索条件にヒットするデータを営業社員名含めて取得すること
        /// </summary>
        /// <param name="customerId">対応するデータの顧客会社ID</param>
        /// <param name="input">検索ワード</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(1, "株式", DisplayName = "営業社員の有効開始日の境界値（顧客名称検索）")]
        [DataRow(2, "株式", DisplayName = "営業社員の有効期限の代表値（顧客名称検索）")]
        [DataRow(3, "株式", DisplayName = "営業社員の有効終了日の境界値（顧客名称検索）")]
        [DataRow(1, "ｶﾌﾞｼｷ", DisplayName = "営業社員の有効開始日の境界値（顧客名称カナ検索）")]
        [DataRow(2, "ｶﾌﾞｼｷ", DisplayName = "営業社員の有効期限の代表値（顧客名称カナ検索）")]
        [DataRow(3, "ｶﾌﾞｼｷ", DisplayName = "営業社員の有効終了日の境界値（顧客名称カナ検索）")]
        public async Task OnGetMovePage_検索時かつ営業社員が有効_データを取得(int customerId, string input)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();
            model.CustomerName = input;

            // Act
            await model.OnGetMovePageAsync(0, 0, false);

            // Assert
            var target = model.Customers
                .FirstOrDefault(x => x.KokyakuKaishaId == customerId);
            Assert.IsNotNull(target);
            AssertCustomerVM(target, customerId);
        }

        /// <summary>
        /// 正常系：検索条件にヒットするが、営業社員の有効期限が不正なデータも取得すること
        /// </summary>
        /// <param name="customerId">対応するデータの顧客会社ID</param>
        /// <param name="input">検索ワード</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(4, "株式", DisplayName = "社員有効開始日に関する代表値（顧客名検索）")]
        [DataRow(5, "株式", DisplayName = "社員有効開始日に関する境界値（顧客名検索）")]
        [DataRow(6, "株式", DisplayName = "社員有効終了日に関する境界値（顧客名検索）")]
        [DataRow(7, "株式", DisplayName = "社員有効終了日に関する代表値（顧客名検索）")]
        [DataRow(4, "ｶﾌﾞｼｷ", DisplayName = "社員有効開始日に関する代表値（顧客名カナ検索）")]
        [DataRow(5, "ｶﾌﾞｼｷ", DisplayName = "社員有効開始日に関する境界値（顧客名カナ検索）")]
        [DataRow(6, "ｶﾌﾞｼｷ", DisplayName = "社員有効終了日に関する境界値（顧客名カナ検索）")]
        [DataRow(7, "ｶﾌﾞｼｷ", DisplayName = "社員有効終了日に関する代表値（顧客名カナ検索）")]
        public async Task OnGetMovePage_検索時かつ営業社員の有効期限が不正_データを取得(int customerId, string input)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();
            model.CustomerName = input;

            // Act
            await model.OnGetMovePageAsync(0, 0, false);

            // Assert
            var target = model.Customers
                .FirstOrDefault(x => x.KokyakuKaishaId == customerId);
            Assert.IsNotNull(target);
            AssertCustomerVM(target, customerId, true);
        }

        /// <summary>
        /// 正常系：検索条件にヒットするが、対応する営業社員が存在しないデータを取得すること
        /// </summary>
        /// <param name="customerId">対応するデータの顧客会社ID</param>
        /// <param name="input">検索ワード</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(8, "株式", DisplayName = "営業社員BASE IDがNULLのとき（顧客名検索）")]
        [DataRow(9, "株式", DisplayName = "営業社員BASE IDに対応する社員BASEが存在しないとき（顧客名検索）")]
        [DataRow(10, "株式", DisplayName = "営業社員BASE IDに対応する社員が存在しないとき（顧客名検索）")]
        [DataRow(8, "ｶﾌﾞｼｷ", DisplayName = "営業社員BASE IDがNULLのとき（顧客名カナ検索）")]
        [DataRow(9, "ｶﾌﾞｼｷ", DisplayName = "営業社員BASE IDに対応する社員BASEが存在しないとき（顧客名カナ検索）")]
        [DataRow(10, "ｶﾌﾞｼｷ", DisplayName = "営業社員BASE IDに対応する社員が存在しないとき（顧客名カナ検索）")]
        public async Task OnGetMovePage_検索時かつ営業社員BaseIDに対応する社員が存在しない_データを取得(int customerId, string input)
        {
            //Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();
            model.CustomerName = input;

            // Act
            await model.OnGetMovePageAsync(0, 0, false);

            // Assert
            var target = model.Customers
                .FirstOrDefault(x => x.KokyakuKaishaId == customerId);
            Assert.IsNotNull(target);
            AssertCustomerVM(target, customerId, true);
        }

        /// <summary>
        /// 正常系：検索条件にヒットしないデータを取得しないこと
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePage_検索時かつ検索条件にヒットしない_データを取得しない()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();
            model.CustomerName = "アイティー";

            // Act
            await model.OnGetMovePageAsync(0, 0, false);

            // Assert
            var isExist = model.Customers
                .Any(x => x.KokyakuKaishaId == 1);
            Assert.IsFalse(isExist);
        }

        /// <summary>
        /// 正常系：取得データが顧客名カナの昇順で並んでいること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePage_顧客名検索時_データは顧客名の昇順()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSort();
            db.SaveChanges();
            model.CustomerName = "株式";

            // Act
            await model.OnGetMovePageAsync(0, 0, false);

            // Assert
            List<long> expectedCustomerIds = [3, 1, 2];
            var actualCustomerIds = model.Customers
                .Select(x => x.KokyakuKaishaId)
                .ToList();
            CollectionAssert.AreEqual(expectedCustomerIds, actualCustomerIds);
        }

        /// <summary>
        /// 正常系：取得データ範囲がページ番号に対応したものであること
        /// </summary>
        /// <param name="count">サンプルデータ数</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(10, DisplayName = "取得開始行数の代表値（取得範囲外）")]
        [DataRow(20, DisplayName = "取得開始行数の境界値（取得範囲外）")]
        [DataRow(21, DisplayName = "取得開始行数の境界値（取得範囲内）")]
        [DataRow(30, DisplayName = "取得行数の代表値（取得範囲内）")]
        [DataRow(40, DisplayName = "取得終了行数の境界値（取得範囲内）")]
        [DataRow(41, DisplayName = "取得終了行数の境界値（取得範囲外）")]
        [DataRow(50, DisplayName = "取得終了行数の代表値（取得範囲外）")]
        public async Task OnGetMovePage_検索時_取得データが該当ページのもの(int count)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForAcquire(count);
            db.SaveChanges();
            model.CustomerName = "株式";

            // Act
            // 2ページ目の顧客情報を取得する（ページ番号 = 0、オフセット = 1で設定）
            await model.OnGetMovePageAsync(0, 1, false);

            // Assert
            var expectedCustomerIds = GetExpectedCustomerIdList(count, model.Pager.PageSize);
            var actualCustomerIds = model.Customers
                .Select(x => x.KokyakuKaishaId)
                .ToList();
            CollectionAssert.AreEqual(expectedCustomerIds, actualCustomerIds);
        }
    }
}