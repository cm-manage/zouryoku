using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace ZouryokuTest.Pages.KokyakuMeiKensaku
{
    /// <summary>
    /// 顧客名検索機能のテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetSearchTests : IndexModelTestBase
    {
        /// <summary>
        /// 異常系：顧客名が不正な場合にエラーを返却すること
        /// </summary>
        /// <param name="input">パラメータ（検索ワード）</param>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchCustomers_入力値が不正_エラー()
        {
            // Arrange
            var model = CreateModel();
            // 顧客名のフィールドにダミーのバリデーションエラーを発生させる
            var message = "エラーメッセージ";
            model.ModelState.AddModelError(nameof(model.CustomerName), message);

            // Act
            var response = await model.OnGetSearchCustomersAsync();

            // Assert
            var result = (JsonResult)response;
            var messages = GetErrors(result, nameof(model.CustomerName));
            Assert.IsNotNull(messages);
            Assert.HasCount(1, messages);
            var actualMessage = messages[0];
            Assert.AreEqual(message, actualMessage);
        }

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
        public async Task OnGetSearchCustomers_検索にヒットかつ営業社員が有効_データを取得(int customerId, string input)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();
            model.CustomerName = input;

            // Act
            await model.OnGetSearchCustomersAsync();

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
        [DataRow(4, "株式", DisplayName = "営業社員の有効開始日の代表値（顧客名称検索）")]
        [DataRow(5, "株式", DisplayName = "営業社員の有効開始日の境界値（顧客名称検索）")]
        [DataRow(6, "株式", DisplayName = "営業社員の有効終了日の境界値（顧客名称検索）")]
        [DataRow(7, "株式", DisplayName = "営業社員の有効終了日の代表値（顧客名称検索）")]
        [DataRow(4, "ｶﾌﾞｼｷ", DisplayName = "営業社員の有効開始日の代表値（顧客名称検索）")]
        [DataRow(5, "ｶﾌﾞｼｷ", DisplayName = "営業社員の有効開始日の境界値（顧客名称検索）")]
        [DataRow(6, "ｶﾌﾞｼｷ", DisplayName = "営業社員の有効終了日の境界値（顧客名称検索）")]
        [DataRow(7, "ｶﾌﾞｼｷ", DisplayName = "営業社員の有効終了日の代表値（顧客名称検索）")]
        public async Task OnGetSearchCustomers_検索にヒットかつ営業社員が無効_データを取得(int customerId, string input)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();
            model.CustomerName = input;

            // Act
            await model.OnGetSearchCustomersAsync();

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
        public async Task OnGetSearchCustomers_検索にヒットかつ営業社員が存在しない_データを取得(int customerId, string input)
        {
            //Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();
            model.CustomerName = input;

            // Act
            await model.OnGetSearchCustomersAsync();

            // Assert
            var target = model.Customers
                .FirstOrDefault(x => x.KokyakuKaishaId == customerId);
            Assert.IsNotNull(target);
            AssertCustomerVM(target, customerId, true);
        }

        /// <summary>
        /// 異常系：検索条件にヒットしないデータを取得しないこと
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchCustomers_検索にヒットしない_データを取得しない()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();
            model.CustomerName = "アイティー";

            // Act
            await model.OnGetSearchCustomersAsync();

            // Assert
            var isExist = model.Customers
                .Any(x => x.KokyakuKaishaId == 1);
            Assert.IsFalse(isExist);
        }

        /// <summary>
        /// 正常系：取得データが顧客名カナの昇順であること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchCustomers_取得データが顧客名カナの昇順()
        {
            //Arrange
            var model = CreateModel();
            CreateDataForAcquire(50);
            db.SaveChanges();
            model.CustomerName = "株式";

            // Act
            await model.OnGetSearchCustomersAsync();

            // Assert
            Assert.HasCount(20, model.Customers);
            CollectionAssert.AreEqual(
                Enumerable
                    .Range(1, 20)
                    .Select(x => (long)x)
                    .ToList(),
                model.Customers
                    .Select(x => x.KokyakuKaishaId).ToList()
                );
        }

        /// <summary>
        /// 正常系：取得データが上から20件であること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchCustomers_取得データが20件()
        {
            //Arrange
            var model = CreateModel();
            CreateDataForAcquire(21);
            db.SaveChanges();
            model.CustomerName = "株式";

            // Act
            await model.OnGetSearchCustomersAsync();

            // Assert
            Assert.HasCount(20, model.Customers);
            // 21件のテストデータを上から20件取得するのでIDが21のデータが存在しなければOK
            var isExist = model.Customers.Any(x => x.KokyakuKaishaId == 21);
            Assert.IsFalse(isExist);
        }
    }
}