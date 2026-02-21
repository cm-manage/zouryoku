using LanguageExt;

namespace ZouryokuTest.Pages.SyainSentaku
{
    /// <summary>
    /// 社員検索のテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetSyainTests : IndexModelTestsBase
    {
        /// <summary>
        /// 異常系：選択した部署が存在しない場合、表示されない
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSyainAsync_部署が存在しない_社員一覧に表示されない(bool isMultiple)
        {
            // Arrange
            var syain1 = AddSyain(10, "社員1", "01", null, false, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, isMultiple, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 異常系：選択した部署のアクティブフラグがFALSEの場合、表示されない
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSyainAsync_部署のアクティブフラグがFALSE_社員一覧に表示されない(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, false, null);
            var syain1 = AddSyain(10, "社員1", "01", null, false, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, isMultiple, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 異常系：選択した部署が有効期限外である場合、表示されない
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(1, null, false ,DisplayName = "単数選択 && 有効終了日がシステム日付より前の場合")]
        [DataRow(null, -1, false, DisplayName = "単数選択 && 有効開始日がシステム日付より後の場合")]
        [DataRow(1, null, true, DisplayName = "複数選択 && 有効終了日がシステム日付より前の場合")]
        [DataRow(null, -1, true, DisplayName = "複数選択 && 有効開始日がシステム日付より後の場合")]
        public async Task OnGetSyainAsync_部署日付が有効期限外_社員一覧に表示されない(int? start, int? end, bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null, start: start, end: end);
            var syain1 = AddSyain(10, "社員1", "01", null, false, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, isMultiple, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：検索対象部署選択した場合、部署情報を取得する
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSyainAsync_検索対象部署選択_部署情報を取得する(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            SeedEntities(busyo1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, isMultiple, "");

            // Assert
            Assert.HasCount(1, model.TargetBusyoIds);
        }

        /// <summary>
        /// 正常系：社員が存在しない部署選択した場合、表示されない
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSyainAsync_社員データが存在しない_社員一覧に表示されない(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            SeedEntities(busyo1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, isMultiple, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：部署選択時社員が有効期限外の場合、表示されない
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(1, null, false, DisplayName = "単数選択 && 有効終了日がシステム日付より前の場合")]
        [DataRow(null, -1, false, DisplayName = "単数選択 && 有効開始日がシステム日付より後の場合")]
        [DataRow(1, null, true, DisplayName = "複数選択 && 有効終了日がシステム日付より前の場合")]
        [DataRow(null, -1, true, DisplayName = "複数選択 && 有効開始日がシステム日付より後の場合")]
        public async Task OnGetSyainAsync_社員日付が期限外_社員一覧に表示されない(int? start, int? end, bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var syain1 = AddSyain(10, "社員1", "01", null, false, 1, 1, start: start, end: end);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, isMultiple, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：指定部署IDの全子孫部署を取得した部署IDリスト内に引数の部署IDが存在しない場合、社員が取得されない
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSyainAsync_指定部署IDの全子孫部署を取得した部署IDリスト内に引数の部署IDが存在しない_社員が取得されない(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var busyo2 = AddBusyo(2, "部署2", 2, true, 1);
            var syain1 = AddSyain(10, "社員1", "01", null, false, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, busyo2,  syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(3, isMultiple, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：退職者かつ未選択の場合、表示されない
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSyainAsync_退職者フラグがTrueかつ選択済社員BaseIDリストに含まれていない_社員一覧に表示されない(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null, start: null, end: null);
            var syain1 = AddSyain(10, "社員1", "01", null, true, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, isMultiple, "");

            // Arrange
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：退職者かつ選択済みの場合、表示される
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSyainAsync_退職者フラグがTrueかつ選択済社員BaseIDリストに含まれている_社員一覧に表示される(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var syain1 = AddSyain(10, "社員1", "01", null, true, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, isMultiple, "[1]");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            Assert.AreEqual(1, model.SyainListPage.BusyoList[0].Syains![0].SyainBaseId);
        }

        /// <summary>
        /// 正常系：未退職者かつ選択済みではない場合、表示される
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSyainAsync_退職者フラグがFalseかつ選択済社員BaseIDリストに含まれていない_社員一覧に表示される(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var syain1 = AddSyain(10, "社員1", "01", null, false, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, isMultiple, "[1]");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            Assert.AreEqual(1, model.SyainListPage.BusyoList[0].Syains![0].SyainBaseId);
        }

        /// <summary>
        /// 正常系：選択部署配下に社員がいる場合、表示される
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSyainAsync_選択部署配下の部署に社員がいる場合_社員一覧に表示される(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var busyo2 = AddBusyo(2, "部署2", 2, true, 1);
            var syain1 = AddSyain(10, "社員1", "01", null, false, 1, 2);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, busyo2, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, isMultiple, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[1].Syains);
            Assert.AreEqual(1, model.SyainListPage.BusyoList[1].Syains![0].SyainBaseId);
        }

        /// <summary>
        /// 正常系：部署に社員が複数いる場合、順序の昇順で表示される
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSyainAsync_選択部署配下の部署に複数社員が所属する場合_順序の昇順で社員が表示される(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var syain1 = AddSyain(10, "社員1", "01", 3, false, 1, 1);
            var syain2 = AddSyain(20, "社員2", "02", 1, false, 2, 1);
            var syain3 = AddSyain(30, "社員3", "03", 2, false, 3, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            var syainBase2 = AddSyainBase(2, "社員2", "02");
            var syainBase3 = AddSyainBase(3, "社員3", "03");
            SeedEntities(busyo1, syain1, syain2, syain3, syainBase1, syainBase2, syainBase3);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, isMultiple, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            CollectionAssert.AreEqual(OrderList, model.SyainListPage.BusyoList[0].Syains!.Select(syain => syain.SyainBaseId).ToArray());
        }

        /// <summary>
        /// 正常系：社員の順序が同値の場合、社員番号の降順で社員が表示される
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSyainAsync_取得社員の順序が同値_社員番号の降順で社員が表示される(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var syain1 = AddSyain(10, "社員1", "01", 1, false, 1, 1);
            var syain2 = AddSyain(20, "社員2", "03", 1, false, 2, 1);
            var syain3 = AddSyain(30, "社員3", "02", 1, false, 3, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            var syainBase2 = AddSyainBase(2, "社員2", "03");
            var syainBase3 = AddSyainBase(3, "社員3", "02");
            SeedEntities(busyo1, syain1, syain2, syain3, syainBase1, syainBase2, syainBase3);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, isMultiple, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            CollectionAssert.AreEqual(OrderList, model.SyainListPage.BusyoList[0].Syains!.Select(syain => syain.SyainBaseId).ToArray());
        }
    }
}
