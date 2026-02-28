using CommonLibrary.Extensions;
using Model.Model;
using Zouryoku.Extensions;

namespace ZouryokuTest.Pages.SyainSentaku
{
    /// <summary>
    /// 初期表示時のテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetTests : IndexModelTestsBase
    {
        /// <summary>
        /// 正常系：セッションに最終選択部署IDが存在判別し、選択部署IDを設定する
        /// </summary>
        /// <param name="isSetBusyoID"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(true, DisplayName = "セッションに最終選択部署IDが存在する")]
        [DataRow(false, DisplayName = "セッションに最終選択部署IDが存在しない")]
        public async Task OnGetAsync_セッションに最終選択部署IDの有無_選択部署IDが設定される(bool isSetBusyoID)
        {
            // Arrange
            var model = CreateModel();
            if (isSetBusyoID)
            {
                model.HttpContext.Session.Set(222, SaveSessionName);
            }

            // Act
            await model.OnGetAsync(false, "");

            // Assert
            if (isSetBusyoID)
            {
                Assert.AreEqual(222, model.SelectionBusyoId);
            }
            else
            {
                Assert.AreEqual(111, model.SelectionBusyoId);
            }
        }

        /// <summary>
        /// 正常系：インプットの値を設定する
        /// </summary>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(false, DisplayName = "単数選択の場合")]
        [DataRow(true, DisplayName = "複数選択の場合")]
        public async Task OnGetAsync_複数選択フラグがTrueかFalse_複数選択フラグが設定される(bool isMultiple)
        {
            // Arrange
            var model = CreateModel();

            // Act
            await model.OnGetAsync(isMultiple, "");

            // Assert
            Assert.AreEqual(model.IsMultipleSelection, isMultiple);
        }

        /// <summary>
        /// 異常系：社員BaseIDの配列をアンダーバーで連結した文字列ではない場合、最終選択社員が取得されない
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_社員BaseIDの配列をアンダーバーで連結した文字列ではない_最終選択社員が取得されない()
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
                Jyunjyo = 3,
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

            SeedEntities(busyo1, syain1, syainBase1, syain2, syainBase2, syain3, syainBase3);
            var model = CreateModel();
            // Act
            await model.OnGetAsync(true, "1,2,3");

            // Assert
            Assert.HasCount(0, model.PreSelectedSyain);
            Assert.HasCount(0, model.PreSelectedBusyoCounts);
        }

        /// <summary>
        /// 異常系：インプットが空または空文字の場合、最終選択社員が取得されない
        /// </summary>
        /// <param name="preSelectedSyains"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow("", DisplayName = "空文字である場合")]
        [DataRow(null, DisplayName = "nullである場合")]
        public async Task OnGetAsync_インプットが無効値_最終選択社員が取得されない(string? preSelectedSyains)
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
                Jyunjyo = 3,
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
            SeedEntities(busyo1, syain1, syainBase1, syain2, syainBase2, syain3, syainBase3);
            var model = CreateModel();

            // Act
            await model.OnGetAsync(true, preSelectedSyains);

            // Assert
            Assert.HasCount(0, model.PreSelectedSyain);
            Assert.HasCount(0, model.PreSelectedBusyoCounts);
        }

        /// <summary>
        /// 正常系：単数選択の場合、最終選択社員が取得されない
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_複数選択フラグがFALSE_最終選択社員が取得されない()
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
                Jyunjyo = 3,
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
            SeedEntities(busyo1, syain1, syainBase1, syain2, syainBase2, syain3, syainBase3);
            var model = CreateModel();

            // Act
            await model.OnGetAsync(false, "");

            // Assert
            Assert.HasCount(0, model.PreSelectedSyain);
            Assert.HasCount(0, model.PreSelectedBusyoCounts);
        }

        /// <summary>
        /// 異常系：最終選択社員所属部署が存在しない場合、最終選択社員が取得されない
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_最終選択社員所属部署が存在しない_最終選択社員が取得されない()
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
                Jyunjyo = 3,
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
            SeedEntities(syain1, syainBase1, syain2, syainBase2, syain3, syainBase3);
            var model = CreateModel();

            // Act
            await model.OnGetAsync(true, "1_2_3");

            // Assert
            Assert.HasCount(0, model.PreSelectedSyain);
            Assert.HasCount(0, model.PreSelectedBusyoCounts);
        }

        /// <summary>
        /// 正常系：最終選択社員所属部署のアクティブフラグがFALSEの場合、最終選択社員が取得されない
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_最終選択社員所属部署のアクティブフラグがFALSE_最終選択社員が取得されない()
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
                Jyunjyo = 3,
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
            SeedEntities(busyo1, syain1, syainBase1, syain2, syainBase2, syain3, syainBase3);
            var model = CreateModel();

            // Act
            await model.OnGetAsync(true, "1_2_3");

            // Assert
            Assert.HasCount(0, model.PreSelectedSyain);
            Assert.HasCount(0, model.PreSelectedBusyoCounts);
        }

        /// <summary>
        /// 正常系：最終選択社員が存在しない場合、最終選択社員が取得されない
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_最終選択社員が存在しない_最終選択社員が取得されない()
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
            await model.OnGetAsync(true, "1_2_3");

            // Assert
            Assert.HasCount(0, model.PreSelectedSyain);
            Assert.HasCount(0, model.PreSelectedBusyoCounts);
        }

        /// <summary>
        /// 異常系：最終選択社員所属部署が有効期限外場合、最終選択社員が取得されない
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(1, 0,DisplayName = "有効終了日がシステム日付より前の場合")]
        [DataRow(0, -1, DisplayName = "有効開始日がシステム日付より後の場合")]
        public async Task OnGetAsync_最終選択社員所属部署が有効期限外_最終選択社員が取得されない(int start, int end)
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
                Jyunjyo = 3,
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
            SeedEntities(busyo1, syain1, syainBase1, syain2, syainBase2, syain3, syainBase3);
            var model = CreateModel();

            // Act
            await model.OnGetAsync(true, "1_2_3");

            // Assert
            Assert.HasCount(0, model.PreSelectedSyain);
            Assert.HasCount(0, model.PreSelectedBusyoCounts);
        }

        /// <summary>
        /// 異常系：最終選択社員が有効期限外の場合、最終選択社員が取得されない
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(1, 0, DisplayName = "有効終了日がシステム日付より前の場合")]
        [DataRow(0, -1, DisplayName = "有効開始日がシステム日付より後の場合")]
        public async Task OnGetAsync_最終選択社員が有効期限外_最終選択社員が取得されない(int start, int end)
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
                StartYmd = today.AddDays(start),
                EndYmd = today.AddDays(end),
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
                StartYmd = today.AddDays(start),
                EndYmd = today.AddDays(end),
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
            SeedEntities(busyo1, syain1, syainBase1, syain2, syainBase2, syain3, syainBase3);
            var model = CreateModel();

            // Act
            await model.OnGetAsync(true, "1_2_3");

            // Assert
            Assert.HasCount(0, model.PreSelectedSyain);
            Assert.HasCount(0, model.PreSelectedBusyoCounts);
        }

        /// <summary>
        /// 正常系：最終選択社員が検索対象である場合、最終選択社員が取得される
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_複数選択フラグがTRUEかつ引数の最終選択社員が1_2_3の形式かつ所属部署と社員の検索条件に当てはまる_最終選択社員が取得される()
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
                Jyunjyo = 3,
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
            SeedEntities(busyo1, syain1, syainBase1, syain2, syainBase2, syain3, syainBase3);
            var model = CreateModel();

            // Act
            await model.OnGetAsync(true, "1_2_3");

            // Assert
            Assert.HasCount(3, model.PreSelectedSyain);
            Assert.HasCount(1, model.PreSelectedBusyoCounts);
        }

        /// <summary>
        /// 正常系：最終選択社員取得された場合、順序の昇順で最終選択社員が設定される
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_最終選択社員取得_順序の昇順で最終選択社員が設定される()
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
                Jyunjyo = 3,
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
            SeedEntities(busyo1, syain1, syainBase1, syain2, syainBase2, syain3, syainBase3);
            var model = CreateModel();
            
            // Act
            await model.OnGetAsync(true, "1_2_3");

            // Assert
            var list = model.PreSelectedSyain.ToList();
            Assert.AreEqual(2, list[0].Key);
            Assert.AreEqual(2, list[0].Value.SyainBaseId);
            Assert.AreEqual(3, list[1].Key);
            Assert.AreEqual(3, list[1].Value.SyainBaseId);
            Assert.AreEqual(1, list[2].Key);
            Assert.AreEqual(1, list[2].Value.SyainBaseId);
        }

        /// <summary>
        /// 正常系：最終選択社員の順序が同値である場合、社員番号の降順で最終選択社員が設定される
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_最終選択社員の順序が同値_社員番号の降順で最終選択社員が設定される()
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
            SeedEntities(busyo1, syain1, syainBase1, syain2, syainBase2, syain3, syainBase3);
            var model = CreateModel();

            // Act
            await model.OnGetAsync(true, "1_2_3");

            // Assert
            var list = model.PreSelectedSyain.ToList();
            Assert.AreEqual(2, list[0].Key);
            Assert.AreEqual(2, list[0].Value.SyainBaseId);
            Assert.AreEqual(3, list[1].Key);
            Assert.AreEqual(3, list[1].Value.SyainBaseId);
            Assert.AreEqual(1, list[2].Key);
            Assert.AreEqual(1, list[2].Value.SyainBaseId);
        }

        /// <summary>
        /// 正常系：最終選択社員が検索対象である場合、最終選択社員が設定される
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_複数選択フラグがTRUEかつ引数の最終選択社員が1_2_3の形式かつ所属部署と社員の検索条件に当てはまる_最終選択社員が設定される()
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
                Jyunjyo = 3,
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
            SeedEntities(busyo1, syain1, syainBase1, syain2, syainBase2, syain3, syainBase3);
            var model = CreateModel();
            var preselectedSyain = CreatePreSelectedSyain();
            var preSelectedBusyoCounts = CreatePreSelectedBusyoCounts();

            // Act
            await model.OnGetAsync(true, "1_2_3");

            // Assert
            Assert.AreEqual(preSelectedBusyoCounts[1], model.PreSelectedBusyoCounts[1]);
            Assert.HasCount(3, model.PreSelectedSyain);

            Assert.IsTrue(model.PreSelectedSyain.TryGetValue(1, out var assertSyain1));
            Assert.AreEqual(preselectedSyain[1].Id, assertSyain1.Id);
            Assert.AreEqual(preselectedSyain[1].Name, assertSyain1.Name);
            Assert.AreEqual(preselectedSyain[1].BusyoId, assertSyain1.BusyoId);
            Assert.AreEqual(preselectedSyain[1].SyainBaseId, assertSyain1.SyainBaseId);
            Assert.AreEqual(preselectedSyain[1].Code, assertSyain1.Code);
            Assert.AreEqual(preselectedSyain[1].Retired, assertSyain1.Retired);

            Assert.IsTrue(model.PreSelectedSyain.TryGetValue(2, out var assertSyain2));
            Assert.AreEqual(preselectedSyain[2].Id, assertSyain2.Id);
            Assert.AreEqual(preselectedSyain[2].Name, assertSyain2.Name);
            Assert.AreEqual(preselectedSyain[2].BusyoId, assertSyain2.BusyoId);
            Assert.AreEqual(preselectedSyain[2].SyainBaseId, assertSyain2.SyainBaseId);
            Assert.AreEqual(preselectedSyain[2].Code, assertSyain2.Code);
            Assert.AreEqual(preselectedSyain[2].Retired, assertSyain2.Retired);

            Assert.IsTrue(model.PreSelectedSyain.TryGetValue(3, out var assertSyain3));
            Assert.AreEqual(preselectedSyain[3].Id, assertSyain3.Id);
            Assert.AreEqual(preselectedSyain[3].Name, assertSyain3.Name);
            Assert.AreEqual(preselectedSyain[3].BusyoId, assertSyain3.BusyoId);
            Assert.AreEqual(preselectedSyain[3].SyainBaseId, assertSyain3.SyainBaseId);
            Assert.AreEqual(preselectedSyain[3].Code, assertSyain3.Code);
            Assert.AreEqual(preselectedSyain[3].Retired, assertSyain3.Retired);
        }
    }
}
