using CommonLibrary.Extensions;
using LanguageExt;
using Model.Model;

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
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_部署が存在しない_社員一覧に表示されない()
        {
            // Arrange
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            SeedEntities(syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, true, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 異常系：選択した部署のアクティブフラグがFALSEの場合、表示されない
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_部署のアクティブフラグがFALSE_社員一覧に表示されない()
        {
            // Arrange
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = false,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, true, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 異常系：選択した部署が有効期限外である場合、表示されない
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(1, 0, DisplayName = "有効終了日がシステム日付より前の場合")]
        [DataRow(0, -1, DisplayName = "有効開始日がシステム日付より後の場合")]
        public async Task OnGetSyainAsync_部署日付が有効期限外_社員一覧に表示されない(int start, int end)
        {
            // Arrange
            fakeTimeProvider.SetLocalNow(TestDate);
            var today = fakeTimeProvider.Today();
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = today.AddDays(start),
                EndYmd = today.AddDays(end),
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, true, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：検索対象部署選択した場合、部署情報を取得する
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_検索対象部署選択_部署情報を取得する()
        {
            // Arrange
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            SeedEntities(busyo1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, true, "");

            // Assert
            Assert.HasCount(1, model.TargetBusyoIds);
        }

        /// <summary>
        /// 正常系：社員が存在しない部署選択した場合、表示されない
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_社員データが存在しない_社員一覧に表示されない()
        {
            // Arrange
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            SeedEntities(busyo1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, true, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：部署選択時社員が有効期限外の場合、表示されない
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(1, 0, DisplayName = "有効終了日がシステム日付より前の場合")]
        [DataRow(0, -1, DisplayName = "有効開始日がシステム日付より後の場合")]
        public async Task OnGetSyainAsync_社員日付が期限外_社員一覧に表示されない(int start, int end)
        {
            // Arrange
            fakeTimeProvider.SetLocalNow(TestDate);
            var today = fakeTimeProvider.Today();
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = today.AddDays(start),
                EndYmd = today.AddDays(end),
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, true, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：指定部署IDの全子孫部署を取得した部署IDリスト(1, 2)内に引数の部署ID = 3である場合、表示されない
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_指定部署IDの全子孫部署を取得した部署IDリスト内に引数の部署IDが存在しない_社員一覧に表示されない()
        {
            // Arrange
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            var busyo2 = new Busyo
            {
                Id = 2,
                Code = string.Empty,
                Name = "部署2",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 2,
                OyaId = 1,
                ShoninBusyoId = null
            };
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            SeedEntities(busyo1, busyo2, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(3, true, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：指定部署IDの全子孫部署を取得した部署IDリスト(1, 2)内に引数の部署ID = 1である場合、表示される
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_指定部署IDの全子孫部署を取得した部署IDリスト内に引数の部署IDが存在する_社員一覧に表示される()
        {
            // Arrange
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            var busyo2 = new Busyo
            {
                Id = 2,
                Code = string.Empty,
                Name = "部署2",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 2,
                OyaId = 1,
                ShoninBusyoId = null
            };
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            SeedEntities(busyo1, busyo2, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, true, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            Assert.AreEqual(1, model.SyainListPage.BusyoList[0].Syains![0].SyainBaseId);
        }

        /// <summary>
        /// 正常系：退職者かつ未選択の場合、表示されない
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_退職者フラグがTrueかつ選択済社員BaseIDリストに含まれていない_社員一覧に表示されない()
        {
            // Arrange
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = true,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, true, "");

            // Arrange
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsTrue(model.SyainListPage.BusyoList.All(busyo => busyo.Syains == null));
        }

        /// <summary>
        /// 正常系：退職者かつ選択済みの場合、表示される
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_退職者フラグがTrueかつ選択済社員BaseIDリストに含まれている_社員一覧に表示される()
        {
            // Arrange
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = true,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, true, "[1]");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            Assert.AreEqual(1, model.SyainListPage.BusyoList[0].Syains![0].SyainBaseId);
        }

        /// <summary>
        /// 正常系：未退職者かつ選択済みではない場合、表示される
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_退職者フラグがFalseかつ選択済社員BaseIDリストに含まれていない_社員一覧に表示される()
        {
            // Arrange
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, true, "[1]");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            Assert.AreEqual(1, model.SyainListPage.BusyoList[0].Syains![0].SyainBaseId);
        }

        /// <summary>
        /// 正常系：未退職者かつ選択済みの場合、表示される
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_退職者フラグがFalseかつ選択済社員BaseIDリストに含まれている_社員一覧に表示される()
        {
            // Arrange
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            SeedEntities(busyo1, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, true, "[1]");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            Assert.AreEqual(1, model.SyainListPage.BusyoList[0].Syains![0].SyainBaseId);
        }

        /// <summary>
        /// 正常系：選択部署配下に社員がいる場合、表示される
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_選択部署配下の部署に社員がいる場合_社員一覧に表示される()
        {
            // Arrange
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            var busyo2 = new Busyo
            {
                Id = 2,
                Code = string.Empty,
                Name = "部署2",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 2,
                OyaId = 1,
                ShoninBusyoId = null
            };
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 2,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            SeedEntities(busyo1, busyo2, syain1, syainBase1);
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, true, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[1].Syains);
            Assert.AreEqual(1, model.SyainListPage.BusyoList[1].Syains![0].SyainBaseId);
        }

        /// <summary>
        /// 正常系：部署と社員が検索対象である場合、部署階層の昇順で社員一覧に表示される
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_部署と社員が検索対象である場合_部署階層の昇順で社員一覧に表示される()
        {
            // Arrange
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "親部署",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            var busyo2 = new Busyo
            {
                Id = 2,
                Code = string.Empty,
                Name = "子部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 2,
                OyaId = 1,
                ShoninBusyoId = null
            };
            var busyo3 = new Busyo
            {
                Id = 3,
                Code = string.Empty,
                Name = "子部署2",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 2,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 3,
                OyaId = 1,
                ShoninBusyoId = null
            };
            var busyo4 = new Busyo
            {
                Id = 4,
                Code = string.Empty,
                Name = "孫部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 4,
                OyaId = 2,
                ShoninBusyoId = null
            };
            var busyo5 = new Busyo
            {
                Id = 5,
                Code = string.Empty,
                Name = "孫部署2",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 2,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 5,
                OyaId = 3,
                ShoninBusyoId = null
            };
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syain2 = new Syain()
            {
                Id = 20,
                SyainBaseId = 2,
                Code = "02",
                Name = "社員2",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 2,
                Retired = false,
                BusyoId = 2,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syain3 = new Syain()
            {
                Id = 30,
                SyainBaseId = 3,
                Code = "03",
                Name = "社員3",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 3,
                Retired = false,
                BusyoId = 3,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syain4 = new Syain()
            {
                Id = 40,
                SyainBaseId = 4,
                Code = "04",
                Name = "社員4",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 4,
                Retired = false,
                BusyoId = 4,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syain5 = new Syain()
            {
                Id = 50,
                SyainBaseId = 5,
                Code = "05",
                Name = "社員5",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 5,
                Retired = false,
                BusyoId = 5,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            var syainBase2 = new SyainBasis
            {
                Id = 2,
                Name = "社員2",
                Code = "02",
            };
            var syainBase3 = new SyainBasis
            {
                Id = 3,
                Name = "社員3",
                Code = "03",
            };
            var syainBase4 = new SyainBasis
            {
                Id = 4,
                Name = "社員1",
                Code = "01",
            };
            var syainBase5 = new SyainBasis
            {
                Id = 5,
                Name = "社員2",
                Code = "02",
            };
            SeedEntities(busyo1, busyo2, busyo3, busyo4, busyo5,
                syain1, syain2, syain3, syain4, syain5, 
                syainBase1, syainBase2, syainBase3, syainBase4, syainBase5
            );
            var model = CreateModel();

            // Act
            await model.OnGetSyainAsync(1, true, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            // 部署リストの1番目に"親部署"が入っている、その中に"社員1"が入っているか確認
            Assert.AreEqual(1, model.SyainListPage.BusyoList[0].Id);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            Assert.AreEqual(1, model.SyainListPage.BusyoList[0].Syains![0].SyainBaseId);
            // 部署リストの2番目に"子部署1"が入っている、その中に"社員2"が入っているか確認
            Assert.AreEqual(2, model.SyainListPage.BusyoList[1].Id);
            Assert.IsNotNull(model.SyainListPage.BusyoList[1].Syains);
            Assert.AreEqual(2, model.SyainListPage.BusyoList[1].Syains![0].SyainBaseId);
            // 部署リストの3番目に"孫部署1"が入っている、その中に"社員4"が入っているか確認
            Assert.AreEqual(4, model.SyainListPage.BusyoList[2].Id);
            Assert.IsNotNull(model.SyainListPage.BusyoList[2].Syains);
            Assert.AreEqual(4, model.SyainListPage.BusyoList[2].Syains![0].SyainBaseId);
            // 部署リストの4番目に"子部署2"が入っている、その中に"社員3"が入っているか確認
            Assert.AreEqual(3, model.SyainListPage.BusyoList[3].Id);
            Assert.IsNotNull(model.SyainListPage.BusyoList[3].Syains);
            Assert.AreEqual(3, model.SyainListPage.BusyoList[3].Syains![0].SyainBaseId);
            // 部署リストの5番目に"孫部署2"が入っている、その中に"社員5"が入っているか確認
            Assert.AreEqual(5, model.SyainListPage.BusyoList[4].Id);
            Assert.IsNotNull(model.SyainListPage.BusyoList[4].Syains);
            Assert.AreEqual(5, model.SyainListPage.BusyoList[4].Syains![0].SyainBaseId);
        }

        /// <summary>
        /// 正常系：部署に社員が複数いる場合、順序の昇順で表示される
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_選択部署配下の部署に複数社員が所属する場合_順序の昇順で社員が表示される()
        {
            // Arrange
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 3,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syain2 = new Syain()
            {
                Id = 20,
                SyainBaseId = 2,
                Code = "02",
                Name = "社員2",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syain3 = new Syain()
            {
                Id = 30,
                SyainBaseId = 3,
                Code = "03",
                Name = "社員3",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 2,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            var syainBase2 = new SyainBasis
            {
                Id = 2,
                Name = "社員2",
                Code = "02",
            };
            var syainBase3 = new SyainBasis
            {
                Id = 3,
                Name = "社員3",
                Code = "03",
            };
            SeedEntities(busyo1, syain1, syain2, syain3, syainBase1, syainBase2, syainBase3);
            var model = CreateModel();
            
            // Act
            await model.OnGetSyainAsync(1, true, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            CollectionAssert.AreEqual(OrderList, model.SyainListPage.BusyoList[0].Syains!.Select(syain => syain.SyainBaseId).ToArray());
        }

        /// <summary>
        /// 正常系：社員の順序が同値の場合、社員番号の降順で社員が表示される
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSyainAsync_取得社員の順序が同値_社員番号の降順で社員が表示される()
        {
            // Arrange
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = string.Empty,
                Name = "部署1",
                KanaName = string.Empty,
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = string.Empty,
                KaikeiCode = string.Empty,
                KeiriCode = string.Empty,
                IsActive = true,
                Ryakusyou = string.Empty,
                BusyoBaseId = 1,
                OyaId = null,
                ShoninBusyoId = null
            };
            var syain1 = new Syain()
            {
                Id = 10,
                SyainBaseId = 1,
                Code = "01",
                Name = "社員1",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syain2 = new Syain()
            {
                Id = 20,
                SyainBaseId = 2,
                Code = "03",
                Name = "社員2",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syain3 = new Syain()
            {
                Id = 30,
                SyainBaseId = 3,
                Code = "02",
                Name = "社員3",
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 1,
                Retired = false,
                BusyoId = 1,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var syainBase1 = new SyainBasis
            {
                Id = 1,
                Name = "社員1",
                Code = "01",
            };
            var syainBase2 = new SyainBasis
            {
                Id = 2,
                Name = "社員2",
                Code = "03",
            };
            var syainBase3 = new SyainBasis
            {
                Id = 3,
                Name = "社員3",
                Code = "02",
            };
            SeedEntities(busyo1, syain1, syain2, syain3, syainBase1, syainBase2, syainBase3);
            var model = CreateModel();

            // Act
            var ss = await model.OnGetSyainAsync(1, true, "");

            // Assert
            Assert.IsNotNull(model.SyainListPage);
            Assert.IsNotNull(model.SyainListPage.BusyoList);
            Assert.IsNotNull(model.SyainListPage.BusyoList[0].Syains);
            CollectionAssert.AreEqual(OrderList, model.SyainListPage.BusyoList[0].Syains!.Select(syain => syain.SyainBaseId).ToArray());
        }
    }
}
