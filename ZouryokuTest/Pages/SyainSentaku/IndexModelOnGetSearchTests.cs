using Microsoft.AspNetCore.Mvc;

namespace ZouryokuTest.Pages.SyainSentaku
{
    /// <summary>
    /// 社員名検索のテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetSearchTests : IndexModelTestsBase
    {
        /// <summary>
        /// 異常系：検索ワードが空の場合、エラーを返す
        /// </summary>
        /// <param name="syainName"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow("", DisplayName = "空文字である場合")]
        [DataRow(null, DisplayName = "nullである場合")]
        public async Task OnGetSearchAsync_検索ワードが空_errorJsonが返される(string? syainName)
        {
            // Arrange
            var model = CreateModel();
            var message = "エラーメッセージ";
            model.ModelState.AddModelError(nameof(model.SyainName), message);
            model.SyainName = syainName;

            // Act
            var response = await model.OnGetSearchAsync(false);

            // Assert
            var result = (JsonResult)response;
            var errorMessage = GetErrors(result, nameof(model.SyainName));
            Assert.IsNotNull(errorMessage);
            Assert.HasCount(1, errorMessage);
            var actualMessage = errorMessage[0];
            Assert.AreEqual(message, actualMessage);
        }

        /// <summary>
        /// 正常系：部署が存在しない場合、表示されない
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSearchAsync_部署が存在しない_社員一覧に表示されない(bool isMultiple)
        {
            // Arrange
            var syain1 = AddSyain(10, "社員1", "01", null, false, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(syain1, syainBase1);
            var model = CreateModel();
            model.SyainName = "社員1";

            // Act
            await model.OnGetSearchAsync(isMultiple);

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：部署のアクティブフラグがFalseの場合、表示されない
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSearchAsync_部署のアクティブフラグがFALSE_社員一覧に表示されない(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, false, null);
            var syain1 = AddSyain(10, "社員1", "01", null, false, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();
            model.SyainName = "社員1";

            // Act
            await model.OnGetSearchAsync(isMultiple);

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：部署が有効期限外の場合、表示されない
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
        public async Task OnGetSearchAsync_部署日付が有効期限外_社員一覧に表示されない(int? start, int? end, bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null, start: start, end: end);
            var syain1 = AddSyain(10, "社員1", "01", null, false, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();
            model.SyainName = "社員1";

            // Act
            await model.OnGetSearchAsync(isMultiple);

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：検索対象部署存在する場合、部署情報を取得する
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSyainAsync_検索対象部署が存在する_部署情報を取得する(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var syain1 = AddSyain(10, "社員1", "01", null, false, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();
            model.SyainName = "社員1";

            // Act
            await model.OnGetSearchAsync(isMultiple);

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.HasCount(1, model.TargetBusyoIds);
        }

        /// <summary>
        /// 正常系：社員が存在しない場合、表示されない
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSearchAsync_社員データが存在しない_社員一覧に表示されない(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            SeedEntities(busyo1);
            var model = CreateModel();
            model.SyainName = "社員1";

            // Act
            await model.OnGetSearchAsync(isMultiple);

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：社員が有効期限外の場合、表示されない
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
        public async Task OnGetSearchAsync_社員日付が期限外_社員一覧に表示されない(int? start, int? end, bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var syain1 = AddSyain(10, "社員1", "01", null, false, 1, 1, start: start, end: end);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();
            model.SyainName = "社員1";

            // Act
            await model.OnGetSearchAsync(isMultiple);

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：部署IDリスト内に検索対象社員所属部署が存在しない場合、社員が取得されない
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択")]
        [DataRow(true, DisplayName = "複数選択")]
        public async Task OnGetSyainAsync_部署IDリスト内に検索対象社員所属部署が存在しない場合_社員が取得されない(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var busyo2 = AddBusyo(2, "部署2", 2, false, 1);
            var syain1 = AddSyain(10, "社員1", "01", null, false, 1, 2);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, busyo2, syain1, syainBase1);
            var model = CreateModel();
            model.SyainName = "社員1";

            // Act
            await model.OnGetSearchAsync(isMultiple);

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：存在する社員名に対して入力した検索文字列が一致しない場合、表示されない
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択の場合")]
        [DataRow(true, DisplayName = "複数選択の場合")]
        public async Task OnGetSearchAsync_社員名検索ワードが一致しない_社員一覧に表示されない(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var syain1 = AddSyain(10, "社員1", "01", null, true, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();
            model.SyainName = "aaa";

            // Act
            await model.OnGetSearchAsync(isMultiple);

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNull(model.SyainListPage.BusyoList[0].Syains);
        }

        /// <summary>
        /// 正常系：存在する社員名に対して入力した検索文字列が前方一致一致する場合、表示される
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択の場合")]
        [DataRow(true, DisplayName = "複数選択の場合")]
        public async Task OnGetSearchAsync_社員名検索ワードが前方一致_社員一覧に表示される(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var syain1 = AddSyain(10, "社員1", "01", null, true, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();
            model.SyainName = "社";

            // Act
            await model.OnGetSearchAsync(isMultiple);

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            Assert.AreEqual(1, model.SyainListPage.BusyoList[0].Syains![0].SyainBaseId);
        }

        /// <summary>
        /// 正常系：存在する社員名に対して入力した検索文字列が後方一致する場合、表示される
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択の場合")]
        [DataRow(true, DisplayName = "複数選択の場合")]
        public async Task OnGetSearchAsync_社員名検索ワードが後方一致_社員一覧に表示される(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var syain1 = AddSyain(10, "社員1", "01", null, true, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();
            model.SyainName = "1";

            // Act
            await model.OnGetSearchAsync(isMultiple);

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            Assert.AreEqual(1, model.SyainListPage.BusyoList[0].Syains![0].SyainBaseId);
        }

        /// <summary>
        /// 正常系：社員名が部分一致する場合、表示される
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択の場合")]
        [DataRow(true, DisplayName = "複数選択の場合")]
        public async Task OnGetSearchAsync_社員名検索ワードが部分一致_社員一覧に表示される(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var syain1 = AddSyain(10, "社員1", "01", null, true, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();
            model.SyainName = "員";

            // Act
            await model.OnGetSearchAsync(isMultiple);

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            Assert.AreEqual(1, model.SyainListPage.BusyoList[0].Syains![0].SyainBaseId);
        }

        /// <summary>
        /// 正常系：社員名が完全一致する場合、表示される
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択の場合")]
        [DataRow(true, DisplayName = "複数選択の場合")]
        public async Task OnGetSearchAsync_社員名検索ワードが完全一致_社員一覧に表示される(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var syain1 = AddSyain(10, "社員1", "01", null, true, 1, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();
            model.SyainName = "社員1";

            // Act
            await model.OnGetSearchAsync(isMultiple);

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            Assert.AreEqual(1, model.SyainListPage.BusyoList[0].Syains![0].SyainBaseId);
        }

        /// <summary>
        /// 正常系：社員名が複数人一致する場合、昇順で社員が表示される
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択の場合")]
        [DataRow(true, DisplayName = "複数選択の場合")]
        public async Task OnGetSearchAsync_同部署に所属し検索ワードにかかる社員が複数人所属_順序の昇順で社員が表示される(bool isMultiple)
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 1, true, null);
            var syain1 = AddSyain(10, "社員1", "01", 3, false, 1, 1);
            var syain2 = AddSyain(20, "社員2", "02", 1, false, 2, 1);
            var syain3 = AddSyain(30, "社員3", "02", 2, false, 3, 1);
            var syainBase1 = AddSyainBase(1, "社員1", "01");
            var syainBase2 = AddSyainBase(2, "社員2", "02");
            var syainBase3 = AddSyainBase(3, "社員3", "03");
            SeedEntities(busyo1, syain1, syain2, syain3, syainBase1, syainBase2, syainBase3);
            var model = CreateModel();
            model.SyainName = "社員";

            // Act
            await model.OnGetSearchAsync(isMultiple);

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
            model.SyainName = "社員";

            // Act
            await model.OnGetSearchAsync(isMultiple);

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            CollectionAssert.AreEqual(OrderList, model.SyainListPage.BusyoList[0].Syains!.Select(syain => syain.SyainBaseId).ToArray());
        }
    }
}
