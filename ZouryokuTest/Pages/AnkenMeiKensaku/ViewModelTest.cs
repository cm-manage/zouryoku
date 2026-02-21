using Model.Model;
using static Zouryoku.Pages.AnkenMeiKensaku.IndexModel;

namespace ZouryokuTest.Pages.AnkenMeiKensaku
{
    /// <summary>
    /// <see cref="AnkenViewModel"/>のテストクラス
    /// </summary>
    [TestClass]
    public class ViewModelTest : IndexModelTestBase
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

        // 引数が案件エンティティの場合
        // --------------------------------------

        [TestMethod]
        [DataRow(1L, DisplayName = "顧客会社ID != NULLのとき")]
        [DataRow(null, DisplayName = "顧客会社ID == NULLのとき")]
        public void AnkenViewModel_引数が案件エンティティのとき_案件エンティティのデータを取得している(long? expectedKokyakuId)
        {
            // Arrange
            var expectedAnkenId = 1;
            var expectedAnkenName = "案件名称";
            var anken = new Anken()
            {
                Id = 1,
                KokyakuKaisyaId = expectedKokyakuId,
                Name = expectedAnkenName,
                // 不要なNOT NULLカラムに適当に値を詰める
                SearchName = string.Empty,
            };

            // Act
            var viewModel = new AnkenViewModel(anken);

            // Assert
            Assert.AreEqual(expectedAnkenId, viewModel.AnkenId);
            Assert.AreEqual(expectedAnkenName, viewModel.AnkenName);
            Assert.AreEqual(expectedKokyakuId, viewModel.KokyakuId);
        }

        [TestMethod]
        [DataRow(false, DisplayName = "顧客会社 != NULLのとき")]
        [DataRow(true, DisplayName = "顧客会社 == NULLのとき")]
        public void AnkenViewModel_引数が案件エンティティのとき_顧客会社エンティティのデータを取得している(bool isNull)
        {
            // Arrange
            var expectedKokyakuName = isNull ? null : "顧客名称";
            var anken = new Anken()
            {
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                SearchName = string.Empty,
            };
            if (!isNull)
            {
                anken.KokyakuKaisya = new KokyakuKaisha()
                {
                    Name = expectedKokyakuName!,
                    // 不要なNOT NULLカラムに適当に値を詰める
                    Code = 0,
                    NameKana = string.Empty,
                    Ryakusyou = string.Empty,
                    EigyoBaseSyainId = 0,
                    SearchName = string.Empty,
                    SearchNameKana = string.Empty,
                };
            }

            // Act
            var viewModel = new AnkenViewModel(anken);

            // Assert
            Assert.AreEqual(expectedKokyakuName, viewModel.KokyakuName);
        }

        [TestMethod]
        [DataRow(false, false, DisplayName = "社員BASEマスタ != NULLかつ社員マスタ != NULLのとき")]
        [DataRow(false, true, DisplayName = "社員BASEマスタ != NULLかつ社員マスタ == NULLのとき")]
        [DataRow(true, true, DisplayName = "社員BASEマスタ == NULLかつ社員マスタ == NULLのとき")]
        public void AnkenViewModel_引数が案件エンティティのとき_社員マスタエンティティのデータを取得している(bool isNullSyainBase, bool isNullSyain)
        {
            // Arrange
            var expectedSekininSyaName = isNullSyain ? null : "社員氏名";
            var anken = new Anken()
            {
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                SearchName = string.Empty,
            };

            if (!isNullSyainBase)
            {
                anken.SyainBase = new SyainBasis()
                {
                    // 不要なNOT NULLカラムに適当に値を詰める
                    Code = string.Empty,
                };

                if (!isNullSyain)
                {
                    anken.SyainBase.Syains.Add(new Syain()
                    {
                        Name = expectedSekininSyaName!,
                        // 不要なNOT NULLカラムに適当に値を詰める
                        EndYmd = DateOnly.MaxValue,
                        SyainBaseId = 1,
                        Code = string.Empty,
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
                    });
                }
            }

            // Act
            var viewModel = new AnkenViewModel(anken);

            // Assert
            Assert.AreEqual(expectedSekininSyaName, viewModel.SyainName);
        }

        [TestMethod]
        [DataRow(false, "商品名", "受注番号", (short)0, false, DisplayName = "すべてNULLでないとき")]
        [DataRow(false, "商品名", "受注番号", null, false, DisplayName = "受注番号：受注行番号がNULLのとき")]
        [DataRow(false, "商品名", null, (short)0, false, DisplayName = "受注番号：受注番号がNULLのとき")]
        [DataRow(false, null, null, null, true, DisplayName = "すべてNULLのとき")]
        [DataRow(true, null, null, null, true, DisplayName = "KINGS受注登録エンティティがNULLのとき")]
        public void AnkenViewModel_引数が案件エンティティのとき_KINGS受注登録エンティティのデータを取得している
            (bool isNullKingsJuchu, string? expectedShouhinName, string? juchuNo, short? juchuGyoNo, bool isNullNsyYmd)
        {
            // Arrange
            var projectNo = "プロジェクト番号";
            var jucKin = 1000000;
            var chaYmd = new DateOnly(2025, 1, 1);
            DateOnly? nsyYmd = isNullNsyYmd ? null : new(2026, 1, 1);

            var anken = new Anken()
            {
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                SearchName = string.Empty,
            };

            // KINGS受注登録がNULLでない場合
            if (!isNullKingsJuchu)
            {
                anken.KingsJuchu = new KingsJuchu()
                {
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
            }

            // Act
            var viewModel = new AnkenViewModel(anken);

            // Assert
            var expectedJuchuNo = $"{projectNo}-{juchuNo}-{juchuGyoNo}";
            var expectedJucKin = "1,000,000";
            var expectedChaYmd = "2025/01/01";
            var expectedNsyYmd = isNullNsyYmd ? null : "2026/01/01";

            // KINGS受注登録エンティティがNULLのときはすべてNULLが期待される
            if (isNullKingsJuchu)
            {
                expectedJuchuNo = null;
                expectedJucKin = null;
                expectedChaYmd = null;
                expectedNsyYmd = null;
            }

            Assert.AreEqual(expectedShouhinName, viewModel.ShouhinName);
            Assert.AreEqual(expectedJuchuNo, viewModel.JuchuuNo);
            Assert.AreEqual(expectedJucKin, viewModel.JucKin);
            Assert.AreEqual(expectedChaYmd, viewModel.ChaYmd);
            Assert.AreEqual(expectedNsyYmd, viewModel.NsyYmd);
        }

        [TestMethod]
        public void AnkenViewModel_引数が案件エンティティのとき_バージョンがNULLで取得されている()
        {
            // Arrange
            var anken = new Anken()
            {
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                SearchName = string.Empty,
            };

            // Act
            var viewModel = new AnkenViewModel(anken);

            // Assert
            Assert.IsNull(viewModel.Version);
        }

        // 引数が案件参照履歴エンティティの場合
        // --------------------------------------

        [TestMethod]
        [DataRow(1L, DisplayName = "顧客会社ID != NULLのとき")]
        [DataRow(null, DisplayName = "顧客会社ID == NULLのとき")]
        public void AnkenViewModel_引数が案件参照履歴エンティティのとき_案件エンティティのデータを取得している(long? expectedKokyakuId)
        {
            // Arrange
            var expectedAnkenId = 1;
            var expectedAnkenName = "案件名称";

            var anken = new Anken()
            {
                Id = 1,
                KokyakuKaisyaId = expectedKokyakuId,
                Name = expectedAnkenName,
                // 不要なNOT NULLカラムに適当に値を詰める
                SearchName = string.Empty,
            };
            var rireki = new AnkenSansyouRireki()
            {
                Anken = anken,
                // 不要なNOT NULLカラムに適当に値を詰める
                SyainBaseId = 0,
                SansyouTime = DateTime.MinValue
            };

            // Act
            var viewModel = new AnkenViewModel(rireki);

            // Assert
            Assert.AreEqual(expectedAnkenId, viewModel.AnkenId);
            Assert.AreEqual(expectedAnkenName, viewModel.AnkenName);
            Assert.AreEqual(expectedKokyakuId, viewModel.KokyakuId);
        }

        [TestMethod]
        [DataRow(false, DisplayName = "顧客会社 != NULLのとき")]
        [DataRow(true, DisplayName = "顧客会社 == NULLのとき")]
        public void AnkenViewModel_引数が案件参照履歴エンティティのとき_顧客会社エンティティのデータを取得している(bool isNull)
        {
            // Arrange
            var expectedKokyakuName = isNull ? null : "顧客名称";
            var anken = new Anken()
            {
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                SearchName = string.Empty,
            };
            if (!isNull)
            {
                anken.KokyakuKaisya = new KokyakuKaisha()
                {
                    Name = expectedKokyakuName!,
                    // 不要なNOT NULLカラムに適当に値を詰める
                    Code = 0,
                    NameKana = string.Empty,
                    Ryakusyou = string.Empty,
                    EigyoBaseSyainId = 0,
                    SearchName = string.Empty,
                    SearchNameKana = string.Empty,
                };
            }
            var rireki = new AnkenSansyouRireki()
            {
                Anken = anken,
                // 不要なNOT NULLカラムに適当に値を詰める
                SyainBaseId = 0,
                SansyouTime = DateTime.MinValue
            };

            // Act
            var viewModel = new AnkenViewModel(rireki);

            // Assert
            Assert.AreEqual(expectedKokyakuName, viewModel.KokyakuName);
        }

        [TestMethod]
        [DataRow(false, false, DisplayName = "社員BASEマスタ != NULLかつ社員マスタ != NULLのとき")]
        [DataRow(false, true, DisplayName = "社員BASEマスタ != NULLかつ社員マスタ == NULLのとき")]
        [DataRow(true, true, DisplayName = "社員BASEマスタ == NULLかつ社員マスタ == NULLのとき")]
        public void AnkenViewModel_引数が案件参照履歴エンティティのとき_社員マスタエンティティのデータを取得している(bool isNullSyainBase, bool isNullSyain)
        {
            // Arrange
            var expectedSekininSyaName = isNullSyain ? null : "社員氏名";
            var anken = new Anken()
            {
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                SearchName = string.Empty,
            };

            if (!isNullSyainBase)
            {
                anken.SyainBase = new SyainBasis()
                {
                    // 不要なNOT NULLカラムに適当に値を詰める
                    Code = string.Empty,
                };

                if (!isNullSyain)
                {
                    anken.SyainBase.Syains.Add(new Syain()
                    {
                        Name = expectedSekininSyaName!,
                        // 不要なNOT NULLカラムに適当に値を詰める
                        EndYmd = DateOnly.MaxValue,
                        SyainBaseId = 1,
                        Code = string.Empty,
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
                    });
                }
            }
            var rireki = new AnkenSansyouRireki()
            {
                Anken = anken,
                // 不要なNOT NULLカラムに適当に値を詰める
                SyainBaseId = 0,
                SansyouTime = DateTime.MinValue
            };

            // Act
            var viewModel = new AnkenViewModel(rireki);

            // Assert
            Assert.AreEqual(expectedSekininSyaName, viewModel.SyainName);
        }

        [TestMethod]
        [DataRow(false, "商品名", "受注番号", (short)0, false, DisplayName = "すべてNULLでないとき")]
        [DataRow(false, "商品名", "受注番号", null, false, DisplayName = "受注番号：受注行番号がNULLのとき")]
        [DataRow(false, "商品名", null, (short)0, false, DisplayName = "受注番号：受注番号がNULLのとき")]
        [DataRow(false, null, null, null, true, DisplayName = "すべてNULLのとき")]
        [DataRow(true, null, null, null, true, DisplayName = "KINGS受注登録エンティティがNULLのとき")]
        public void AnkenViewModel_引数が案件参照履歴エンティティのとき_KINGS受注登録エンティティのデータを取得している
            (bool isNullKingsJuchu, string? expectedShouhinName, string? juchuNo, short? juchuGyoNo, bool isNullNsyYmd)
        {
            // Arrange
            var projectNo = "プロジェクト番号";
            var jucKin = 1000000;
            var chaYmd = new DateOnly(2025, 1, 1);
            DateOnly? nsyYmd = isNullNsyYmd ? null : new(2026, 1, 1);

            var anken = new Anken()
            {
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                SearchName = string.Empty,
            };

            // KINGS受注登録がNULLでない場合
            if (!isNullKingsJuchu)
            {
                anken.KingsJuchu = new KingsJuchu()
                {
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
            }

            var rireki = new AnkenSansyouRireki()
            {
                Anken = anken,
                // 不要なNOT NULLカラムに適当に値を詰める
                SyainBaseId = 0,
                SansyouTime = DateTime.MinValue
            };

            // Act
            var viewModel = new AnkenViewModel(rireki);

            // Assert
            var expectedJuchuNo = $"{projectNo}-{juchuNo}-{juchuGyoNo}";
            var expectedJucKin = "1,000,000";
            var expectedChaYmd = "2025/01/01";
            var expectedNsyYmd = isNullNsyYmd ? null : "2026/01/01";

            // KINGS受注登録エンティティがNULLのときはすべてNULLが期待される
            if (isNullKingsJuchu)
            {
                expectedJuchuNo = null;
                expectedJucKin = null;
                expectedChaYmd = null;
                expectedNsyYmd = null;
            }

            Assert.AreEqual(expectedShouhinName, viewModel.ShouhinName);
            Assert.AreEqual(expectedJuchuNo, viewModel.JuchuuNo);
            Assert.AreEqual(expectedJucKin, viewModel.JucKin);
            Assert.AreEqual(expectedChaYmd, viewModel.ChaYmd);
            Assert.AreEqual(expectedNsyYmd, viewModel.NsyYmd);
        }

        [TestMethod]
        public void AnkenViewModel_引数が案件参照履歴エンティティのとき_案件参照履歴のバージョンを取得している()
        {
            // Arrange
            uint expectedVersion = 1u;
            var anken = new Anken()
            {
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                SearchName = string.Empty,
            };
            var rireki = new AnkenSansyouRireki()
            {
                Anken = anken,
                Version = expectedVersion,
                // 不要なNOT NULLカラムに適当に値を詰める
                SyainBaseId = 0,
                SansyouTime = DateTime.MinValue
            };

            // Act
            var viewModel = new AnkenViewModel(rireki);

            // Assert
            Assert.AreEqual(expectedVersion, viewModel.Version);
        }

        // 例外処理
        // --------------------------------------

        [TestMethod]
        public void AnkenViewModel_引数が案件エンティティを含まない案件参照履歴エンティティのとき_InvalidOperation()
        {
            // Arrange
            var rireki = new AnkenSansyouRireki()
            {
                // 不要なNOT NULLカラムに適当に値を詰める
                SyainBaseId = 0,
                SansyouTime = DateTime.MinValue
            };

            // Act
            var viewModel = new AnkenViewModel(rireki);
            var ex = Assert.Throws<InvalidOperationException>(() => viewModel.AnkenName);

            // Assert
            Assert.AreEqual("案件情報と案件参照履歴の情報の両方が設定されていません。", ex.Message);
        }
    }
}
