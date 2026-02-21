using CommonLibrary.Extensions;
using Model.Model;
using Zouryoku.Pages.AnkenMeiKensaku;

namespace ZouryokuTest.Pages.AnkenMeiKensaku
{
    /// <summary>
    /// <see cref="IndexModel.OnGetAsync"/>のテストクラス
    /// </summary>
    [TestClass]
    public class OnGetAsyncTest : IndexModelTestBase
    {
        // ======================================
        // テストの初期化処理
        // ======================================

        /// <summary>
        /// IndexModelを作成する。
        /// </summary>
        [TestInitialize]
        public void TestInit()
        {
            Model = CreateModel();
        }

        // ======================================
        // テストメソッド
        // ======================================

        // パラメータ処理
        // --------------------------------------

        [TestMethod]
        [DataRow(true, DisplayName = "TRUEのとき")]
        [DataRow(false, DisplayName = "FALSEのとき")]
        public async Task OnGetAsync_新規作成可能フラグが指定されているとき_指定された値になる(bool canAdd)
        {
            // Act
            await Model!.OnGetAsync(false, canAdd);

            // Assert
            Assert.AreEqual(canAdd, Model.CanAdd);
        }

        [TestMethod]
        [DataRow(true, DisplayName = "TRUEのとき")]
        [DataRow(false, DisplayName = "FALSEのとき")]
        public async Task OnGetAsync_表示状態フラグが指定されているとき_指定された値になる(bool canCardClick)
        {
            // Act
            await Model!.OnGetAsync(canCardClick, false);

            // Assert
            Assert.AreEqual(canCardClick, Model.CanCardClick);
        }

        [TestMethod]
        public async Task OnGetAsync_顧客会社IDがNULLのとき_検索モデルの顧客名がNULL()
        {
            // Act
            await Model!.OnGetAsync(false, false);

            // Assert
            Assert.IsNull(Model.SearchConditions.KokyakuName);
        }

        [TestMethod]

        public async Task OnGetAsync_顧客会社IDに対応する顧客会社が有効なとき_検索モデルの顧客名が設定される()
        {
            // Arrange
            string expectedKokyakuName = "顧客名称";

            var syainBase = new SyainBasis()
            {
                Id = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = string.Empty,
            };
            var syain = new Syain()
            {
                Id = 1,
                EndYmd = DateOnly.MaxValue,
                SyainBaseId = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = string.Empty,
                Name = string.Empty,
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 0,
                Retired = false,
                BusyoId = 0,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var kokyaku = new KokyakuKaisha()
            {
                Id = 1,
                Name = expectedKokyakuName,
                EigyoBaseSyainId = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = 0,
                NameKana = string.Empty,
                Ryakusyou = string.Empty,
                SearchName = string.Empty,
                SearchNameKana = string.Empty,
            };

            db.Add(syainBase);
            db.Add(syain);
            db.Add(kokyaku);
            db.SaveChanges();

            // Act
            await Model!.OnGetAsync(false, false, 1);

            // Assert
            var actualKokyakuName = Model.SearchConditions.KokyakuName;
            Assert.IsNotNull(actualKokyakuName);
            Assert.AreEqual(expectedKokyakuName, actualKokyakuName);
        }

        [TestMethod]
        [DataRow(-1, 1L, 1L, 1L, DisplayName = "営業担当の有効終了日が9999/12/31でないとき")]
        [DataRow(0, 2L, 1L, 1L, DisplayName = "営業担当の社員BASEマスタに対応する社員マスタデータが存在しないとき")]
        [DataRow(0, 1L, null, 1L, DisplayName = "営業担当の社員BaseIDがNULLのとき")]
        [DataRow(0, 1L, 1L, 2L, DisplayName = "顧客会社IDが一致しないとき")]

        public async Task OnGetAsync_検索条件を満たさない_顧客名を取得しない(
            int endYmdOffsetDays, long syainBaseId, long? eigyoSyainBaseId, long kokyakuId)
        {
            var syainBase = new SyainBasis()
            {
                Id = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = string.Empty,
            };
            var syain = new Syain()
            {
                Id = 1,
                EndYmd = DateOnly.MaxValue.AddDays(endYmdOffsetDays),
                SyainBaseId = syainBaseId,
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = string.Empty,
                Name = string.Empty,
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 0,
                Retired = false,
                BusyoId = 0,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var kokyaku = new KokyakuKaisha()
            {
                Id = 1,
                EigyoBaseSyainId = eigyoSyainBaseId,
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = 0,
                Name = string.Empty,
                NameKana = string.Empty,
                Ryakusyou = string.Empty,
                SearchName = string.Empty,
                SearchNameKana = string.Empty,
            };

            db.Add(syainBase);
            db.Add(syain);
            db.Add(kokyaku);
            db.SaveChanges();

            // Act
            await Model!.OnGetAsync(false, false, kokyakuId);

            // Assert
            var actualKokyakuName = Model.SearchConditions.KokyakuName;
            Assert.IsNull(actualKokyakuName);
        }

        // 検索モデルの初期化
        // --------------------------------------

        [TestMethod]
        public async Task OnGetAsync_検索条件が初期化されていること()
        {
            // Act
            await Model!.OnGetAsync(false, false);

            // Assert
            Assert.AreEqual(new DateOnly(DateTime.Now.AddYears(-2).Year, 1, 1), Model.SearchConditions.ChaYmd.From);
            Assert.AreEqual(DateTime.Now.ToDateOnly().GetEndOfMonth(), Model.SearchConditions.ChaYmd.To);
            Assert.IsTrue(Model.SearchConditions.IsOwnBusyoOnly);
            Assert.IsFalse(Model.SearchConditions.ShowGenkaToketu);
            Assert.AreEqual(IndexModel.AnkenSearchModel.SortKeyList.顧客名, Model.SearchConditions.SortKey);
        }

        // 参照履歴の検索
        // --------------------------------------

        [TestMethod]
        [DataRow(2, 1, 1, 1, DisplayName = "案件-顧客会社")]
        [DataRow(1, 2, 1, 1, DisplayName = "案件-社員BASEマスタ")]
        [DataRow(1, 1, 2, 1, DisplayName = "社員BASEマスタ-社員マスタ")]
        [DataRow(1, 1, 1, 2, DisplayName = "案件-KINGS受注登録")]

        public async Task OnGetAsync_各テーブルが外部結合されていること
            (long kokyakuKaisyaId, long ankenSyainBaseId, long syainBaseId, long kingsJuchuId)
        {
            // Arrange
            var ankenSansyouRireki = new AnkenSansyouRireki()
            {
                AnkenId = 1,
                SyainBaseId = LoginUserSyainBaseId,
                // 不要なNOT NULLカラムに適当に値を詰める
                SansyouTime = DateTime.MinValue,
            };
            var anken = new Anken()
            {
                Id = 1,
                KokyakuKaisyaId = kokyakuKaisyaId,
                SyainBaseId = ankenSyainBaseId,
                KingsJuchuId = kingsJuchuId,
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                SearchName = string.Empty,
            };
            var syainBase = new SyainBasis()
            {
                Id = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = string.Empty,
            };
            var syain = new Syain()
            {
                EndYmd = DateOnly.MaxValue,
                SyainBaseId = syainBaseId,
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = string.Empty,
                Name = string.Empty,
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 0,
                Retired = false,
                BusyoId = 0,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var kingsJuchu = new KingsJuchu()
            {
                Id = 1,
                JucYmd = DateOnly.MinValue,
                EntYmd = DateOnly.MinValue,
                Bukken = string.Empty,
                JucKin = 0,
                ChaYmd = DateOnly.MinValue,
                ProjectNo = string.Empty,
                SekouBumonCd = string.Empty,
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = string.Empty,
                IsGenkaToketu = false,
                Nendo = 0,
                BusyoId = 0,
                SearchBukken = string.Empty,
            };
            var kokyaku = new KokyakuKaisha()
            {
                Id = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = 0,
                Name = string.Empty,
                NameKana = string.Empty,
                Ryakusyou = string.Empty,
                EigyoBaseSyainId = 0,
                SearchName = string.Empty,
                SearchNameKana = string.Empty,
            };

            db.Add(ankenSansyouRireki);
            db.Add(anken);
            db.Add(syainBase);
            db.Add(syain);
            db.Add(kingsJuchu);
            db.Add(kokyaku);
            db.SaveChanges();

            // Act
            await Model!.OnGetAsync(false, false);

            // Assert
            Assert.IsNotEmpty(Model.Ankens);
        }

        [TestMethod]
        [DataRow(-1, 30, "社員氏名", DisplayName = "責任者を取得する（境界値：社員マスタ.有効開始日）")]
        [DataRow(-30, 30, "社員氏名", DisplayName = "責任者を取得する（代表値：社員マスタ.有効開始日と社員マスタ.有効終了日）")]
        [DataRow(-30, 1, "社員氏名", DisplayName = "責任者を取得する（境界値：社員マスタ.有効終了日）")]
        [DataRow(-30, -1, null, DisplayName = "責任者を取得しない（境界値：社員マスタ.有効終了日）")]
        [DataRow(-30, -15, null, DisplayName = "責任者を取得しない（代表値：社員マスタ.有効終了日）")]
        [DataRow(1, 30, null, DisplayName = "責任者を取得しない（境界値：社員マスタ.有効開始日）")]
        [DataRow(15, 30, null, DisplayName = "責任者を取得しない（代表値：社員マスタ.有効開始日）")]
        public async Task OnGetAsync_データを取得していること(int startYmdOffset, int endYmdOffset, string? expectedSekininSyaName)
        {
            // Arrange
            var expectedAnkenId = 1;
            var expectedAnkenName = "案件名称";
            var expectedKokyakuName = "顧客名称";
            var expectedKokyakuId = 1;
            var expectedShouhinName = "商品名";
            var projectNo = "プロジェクト番号";
            var juchuNo = "受注番号";
            short juchuGyoNo = 0;
            var expectedJuchuNo = $"{projectNo}-{juchuNo}-{juchuGyoNo}";
            var jucKin = 1000000;
            var expectedJucKin = "1,000,000";
            var chaYmd = new DateOnly(2025, 1, 1);
            var expectedChaYmd = "2025/01/01";
            var nsyYmd = new DateOnly(2026, 1, 1);
            var expectedNsyYmd = "2026/01/01";

            var ankenSansyouRireki = new AnkenSansyouRireki()
            {
                AnkenId = expectedAnkenId,
                SyainBaseId = LoginUserSyainBaseId,
                // 不要なNOT NULLカラムに適当に値を詰める
                SansyouTime = DateTime.MinValue,
            };
            var anken = new Anken()
            {
                Id = expectedAnkenId,
                KokyakuKaisyaId = expectedKokyakuId,
                SyainBaseId = 1,
                KingsJuchuId = 1,
                Name = expectedAnkenName,
                // 不要なNOT NULLカラムに適当に値を詰める
                SearchName = string.Empty,
            };
            var syainBase = new SyainBasis()
            {
                Id = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = string.Empty,
            };
            var syain = new Syain()
            {
                SyainBaseId = 1,
                Name = expectedSekininSyaName ?? "取得されないデータ",
                StartYmd = DateTime.Now.AddDays(startYmdOffset).ToDateOnly(),
                EndYmd = DateTime.Now.AddDays(endYmdOffset).ToDateOnly(),
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = string.Empty,
                KanaName = string.Empty,
                Seibetsu = char.MinValue,
                BusyoCode = string.Empty,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = DateOnly.MinValue,
                Kyusyoku = 0,
                SyucyoSyokui = 0,
                KingsSyozoku = string.Empty,
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Kengen = 0,
                Jyunjyo = 0,
                Retired = false,
                BusyoId = 0,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };
            var kingsJuchu = new KingsJuchu()
            {
                Id = 1,
                ShouhinName = expectedShouhinName,
                ProjectNo = projectNo,
                JuchuuNo = juchuNo,
                JuchuuGyoNo = juchuGyoNo,
                JucKin = jucKin,
                ChaYmd = chaYmd,
                NsyYmd = nsyYmd,
                // 不要なNOT NULLカラムに適当に値を詰める
                JucYmd = DateOnly.MinValue,
                EntYmd = DateOnly.MinValue,
                Bukken = string.Empty,
                SekouBumonCd = string.Empty,
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = string.Empty,
                IsGenkaToketu = false,
                Nendo = 0,
                BusyoId = 0,
                SearchBukken = string.Empty,
            };
            var kokyaku = new KokyakuKaisha()
            {
                Id = expectedKokyakuId,
                Name = expectedKokyakuName,
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = 0,
                NameKana = string.Empty,
                Ryakusyou = string.Empty,
                EigyoBaseSyainId = 0,
                SearchName = string.Empty,
                SearchNameKana = string.Empty,
            };

            db.Add(ankenSansyouRireki);
            db.Add(anken);
            db.Add(syainBase);
            db.Add(syain);
            db.Add(kingsJuchu);
            db.Add(kokyaku);
            db.SaveChanges();

            // Act
            await Model!.OnGetAsync(false, false);

            // Assert
            var actualAnken = Model.Ankens.Single();
            Assert.AreEqual(expectedAnkenId, actualAnken.AnkenId);
            Assert.AreEqual(expectedAnkenName, actualAnken.AnkenName);
            Assert.AreEqual(expectedKokyakuName, actualAnken.KokyakuName);
            Assert.AreEqual(expectedKokyakuId, actualAnken.KokyakuId);
            Assert.AreEqual(expectedSekininSyaName, actualAnken.SyainName);
            Assert.AreEqual(expectedShouhinName, actualAnken.ShouhinName);
            Assert.AreEqual(expectedJuchuNo, actualAnken.JuchuuNo);
            Assert.AreEqual(expectedJucKin, actualAnken.JucKin);
            Assert.AreEqual(expectedChaYmd, actualAnken.ChaYmd);
            Assert.AreEqual(expectedNsyYmd, actualAnken.NsyYmd);
        }

        [TestMethod]
        public async Task OnGetAsync_案件参照履歴の社員BaseIDがログインユーザーのもの_データを取得()
        {
            // Arrange
            var ankenSansyouRireki = new AnkenSansyouRireki()
            {
                AnkenId = 1,
                SyainBaseId = LoginUserSyainBaseId,
                // 不要なNOT NULLカラムに適当に値を詰める
                SansyouTime = DateTime.MinValue,
            };
            var anken = new Anken()
            {
                Id = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                KokyakuKaisyaId = 1,
                SyainBaseId = 1,
                KingsJuchuId = 1,
                Name = string.Empty,
                SearchName = string.Empty,
            };

            db.Add(anken);
            db.Add(ankenSansyouRireki);
            db.SaveChanges();

            // Act
            await Model!.OnGetAsync(false, false);

            // Assert
            Assert.IsNotEmpty(Model.Ankens);
        }

        [TestMethod]
        public async Task OnGetAsync_案件参照履歴の社員BaseIDがログインユーザーのものでない_データを取得しない()
        {
            // Arrange
            var ankenSansyouRireki = new AnkenSansyouRireki()
            {
                AnkenId = 1,
                SyainBaseId = LoginUserSyainBaseId + 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                SansyouTime = DateTime.MinValue,
            };
            var anken = new Anken()
            {
                Id = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                KokyakuKaisyaId = 1,
                SyainBaseId = 1,
                KingsJuchuId = 1,
                Name = string.Empty,
                SearchName = string.Empty,
            };

            db.Add(anken);
            db.Add(ankenSansyouRireki);
            db.SaveChanges();

            // Act
            await Model!.OnGetAsync(false, false);

            // Assert
            Assert.IsEmpty(Model.Ankens);
        }

        [TestMethod]
        public async Task OnGetAsync_取得データの並び順が参照時間の降順()
        {
            // Arrange
            var ankens = new List<Anken>();
            var rirekis = new List<AnkenSansyouRireki>();
            for (int i = 1; i <= 3; i++)
            {
                ankens.Add(new Anken()
                {
                    Id = i,
                    // 不要なNOT NULLカラムに適当に値を詰める
                    KokyakuKaisyaId = 1,
                    SyainBaseId = 1,
                    KingsJuchuId = 1,
                    Name = string.Empty,
                    SearchName = string.Empty,
                });
                rirekis.Add(new AnkenSansyouRireki()
                {
                    Id = i,
                    AnkenId = i,
                    SyainBaseId = LoginUserSyainBaseId,
                    // 参照時間の降順がIDの降順となるように設定
                    SansyouTime = DateTime.MinValue.AddDays(i),
                });
            }

            db.AddRange(ankens);
            db.AddRange(rirekis);
            db.SaveChanges();

            // Act
            await Model!.OnGetAsync(false, false);

            // Assert
            var expectedAnkenIds = new List<long>() { 3, 2, 1 };
            var actualAnkenIds = Model.Ankens.Select(a => a.AnkenId).ToList();
            CollectionAssert.AreEqual(expectedAnkenIds, actualAnkenIds);
        }

        [TestMethod]
        public async Task OnGetAsync_取得件数が最大20件()
        {
            // Arrange
            var ankens = new List<Anken>();
            var rirekis = new List<AnkenSansyouRireki>();
            for (int i = 1; i <= 21; i++)
            {
                ankens.Add(new Anken()
                {
                    Id = i,
                    // 不要なNOT NULLカラムに適当に値を詰める
                    KokyakuKaisyaId = 1,
                    SyainBaseId = 1,
                    KingsJuchuId = 1,
                    Name = string.Empty,
                    SearchName = string.Empty,
                });
                rirekis.Add(new AnkenSansyouRireki()
                {
                    Id = i,
                    AnkenId = i,
                    SyainBaseId = LoginUserSyainBaseId,
                    // 参照時間の降順がIDの降順となるように設定
                    SansyouTime = DateTime.MinValue.AddDays(i),
                });
            }

            db.AddRange(ankens);
            db.AddRange(rirekis);
            db.SaveChanges();

            // Act
            await Model!.OnGetAsync(false, false);

            // Assert
            Assert.HasCount(20, Model.Ankens);
            CollectionAssert.DoesNotContain(Model.Ankens.Select(a => a.AnkenId).ToList(), 21);
        }
    }
}
