using CommonLibrary.Extensions;
using Model.Model;
using ZouryokuTest.Builder;

namespace ZouryokuTest.Pages.KokyakuMeiKensaku
{
    /// <summary>
    /// OnGetAsyncのテストクラス
    /// </summary>
    [TestClass]
    public class IndexModelOnGetTests : IndexModelTestBase
    {
        /// <summary>
        /// 正常系：パラメータが指定されているときその値がモデルに格納されていること
        /// </summary>
        /// <param name="canCardClick"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(true, DisplayName = "trueの場合")]
        [DataRow(false, DisplayName = "falseの場合")]
        public async Task OnGetAsync_GETパラメータが指定されている_CanCardClickが指定された値となる(bool canCardClick)
        {
            // Arrange
            var model = CreateModel();

            // Act
            await model.OnGetAsync(canCardClick);

            // Assert
            Assert.AreEqual(canCardClick, model.CanCardClick);
        }

        /// <summary>
        /// 正常系：パラメータが指定されていないときモデルにfalseが格納されていること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_GETパラメータが指定されていない_CanCardClickがfalseとなる()
        {
            // Arrange
            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsFalse(model.CanCardClick);
        }

        /// <summary>
        /// 正常系：営業社員含めてデータを取得すること
        /// </summary>
        /// <param name="customerId">対象データの顧客会社ID</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(1, DisplayName = "社員有効開始日の境界値")]
        [DataRow(2, DisplayName = "社員有効期限の代表値")]
        [DataRow(3, DisplayName = "社員有効終了日の境界値")]
        public async Task OnGetAsync_営業社員が有効_データを取得(int customerId)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();

            // Act
            await model.OnGetAsync();

            // Assert
            var target = model.Customers
                .FirstOrDefault(x => x.KokyakuKaishaId == customerId);
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
        public async Task OnGetAsync_営業社員が有効でない_データを取得(int customerId)
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();

            // Act
            await model.OnGetAsync();

            // Assert
            var target = model.Customers
                .FirstOrDefault(x => x.KokyakuKaishaId == customerId);
            Assert.IsNotNull(target);
            AssertCustomerVM(target, customerId, true);
        }

        /// <summary>
        /// 正常系：社員BASE IDに対応する営業社員が存在しなくてもデータを取得すること
        /// </summary>
        /// <param name="customerId">対象データの顧客会社ID</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(8, DisplayName = "営業社員BASE IDがNULLの場合")]
        [DataRow(9, DisplayName = "営業社員BASE IDに対応する社員BASEのデータが存在しないとき")]
        [DataRow(10, DisplayName = "営業社員BASE IDに対応する社員マスタのデータが存在しないとき")]
        public async Task OnGetAsync_営業社員BaseIDに対応する社員が存在しない_データを取得(int customerId)
        {
            //Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();

            // Act
            await model.OnGetAsync();

            // Assert
            var target = model.Customers
                .FirstOrDefault(x => x.KokyakuKaishaId == customerId);
            Assert.IsNotNull(target);
            AssertCustomerVM(target, customerId, true);
        }

        /// <summary>
        /// 正常系：参照履歴に対応する顧客会社が存在しないデータは取得しないこと
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_対応する顧客が存在しない_データを取得しない()
        {
            //Arrange
            var model = CreateModel();
            CreateDataForSearch();
            db.SaveChanges();

            // Act
            await model.OnGetAsync();

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
        public async Task OnGetAsync_ログインユーザーのものでない_データを取得しない()
        {
            //Arrange
            var model = CreateModel();
            db.Add(new KokyakuKaisyaSansyouRireki(){
                Id = 12,
                KokyakuKaisyaId = 1,
                SyainBaseId = 1,
                SansyouTime = fakeTimeProvider.Now().AddDays(-1),
                });
            db.SaveChanges();

            // Act
            await model.OnGetAsync();

            // Assert
            var isExist = model.Customers.Any();
            Assert.IsFalse(isExist);
        }

        /// <summary>
        /// 正常系：取得データが参照時間の降順であること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_取得データが参照時間の降順()
        {
            // Arrange
            var model = CreateModel();
            CreateDataForSort();
            db.SaveChanges();

            // Act
            await model.OnGetAsync();

            // Assert
            List<long> expectedCustomerId = [3, 2, 1];
            var actualCustomerId = model.Customers
                .Select(x => (long)x.KokyakuKaishaId)
                .ToList();
            CollectionAssert.AreEqual(expectedCustomerId, actualCustomerId);
        }

        /// <summary>
        /// 正常系：取得データが上から20件であること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_取得データが20件()
        {
            //Arrange
            var model = CreateModel();
            CreateDataForAcquire(21);
            db.SaveChanges();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.HasCount(20, model.Customers);
            // 21件のテストデータを上から20件取得するのでIDが21のデータが存在しなければOK
            var isExist = model.Customers.Any(x => x.KokyakuKaishaId == 21);
            Assert.IsFalse(isExist);
        }
    }
}
