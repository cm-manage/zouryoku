using CommonLibrary.Extensions;
using Model.Model;
using Zouryoku.Pages.AnkenMeiKensaku;
using static Zouryoku.Pages.AnkenMeiKensaku.IndexModel.AnkenSearchModel;
using static Zouryoku.Pages.AnkenMeiKensaku.IndexModel.AnkenSearchModel.SortKeyList;

namespace ZouryokuTest.Pages.AnkenMeiKensaku
{
    /// <summary>
    /// <see cref="IndexModel.OnGetSearchAnkensAsync"/>のテストクラス
    /// </summary>
    [TestClass]
    public class OnGetSearchAnkensTest : IndexModelTestBase
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

        [TestMethod]
        [DataRow(2, 1, 1, 1, DisplayName = "案件-顧客会社")]
        [DataRow(1, 2, 1, 1, DisplayName = "案件-社員BASEマスタ")]
        [DataRow(1, 1, 2, 1, DisplayName = "社員BASEマスタ-社員マスタ")]
        [DataRow(1, 1, 1, 2, DisplayName = "案件-KINGS受注登録")]

        public async Task OnGetSearchAnkensAsync_各テーブルが外部結合されていること
            (long kokyakuKaisyaId, long ankenSyainBaseId, long syainBaseId, long kingsJuchuId)
        {
            // Arrange
            fakeTimeProvider.SetLocalNow(new(2026, 2, 15));
            var anken = new Anken()
            {
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
                Nendo = 2026,
                // 不要なNOT NULLカラムに適当に値を詰める
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

            db.Add(anken);
            db.Add(syainBase);
            db.Add(syain);
            db.Add(kingsJuchu);
            db.Add(kokyaku);
            db.SaveChanges();

            // 検索条件を設定
            Model!.SearchConditions = new()
            {
                ChaYmd = new(),
                JuchuuNo = new(),
                IsOwnBusyoOnly = false,
                ShowGenkaToketu = true,
            };

            // Act
            await Model.OnGetSearchAnkensAsync();

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
        public async Task OnGetSearchAnkensAsync_データを取得していること(int startYmdOffset, int endYmdOffset, string? expectedSekininSyaName)
        {
            // Arrange
            var now = new DateTime(2026, 2, 15);
            fakeTimeProvider.SetLocalNow(now);

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
                StartYmd = now.ToDateOnly().AddDays(startYmdOffset),
                EndYmd = now.ToDateOnly().AddDays(endYmdOffset),
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
                Nendo = 2026,
                // 不要なNOT NULLカラムに適当に値を詰める
                JucYmd = DateOnly.MinValue,
                EntYmd = DateOnly.MinValue,
                Bukken = string.Empty,
                SekouBumonCd = string.Empty,
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = string.Empty,
                IsGenkaToketu = false,
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

            db.Add(anken);
            db.Add(syainBase);
            db.Add(syain);
            db.Add(kingsJuchu);
            db.Add(kokyaku);
            db.SaveChanges();

            // 検索条件を設定
            Model!.SearchConditions = new()
            {
                ChaYmd = new(),
                JuchuuNo = new(),
                IsOwnBusyoOnly = false,
                ShowGenkaToketu = true,
            };

            // Act
            await Model!.OnGetSearchAnkensAsync();

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
        [DataRow("プロ", null, null, null, null, null, null, null, false, true, DisplayName = "プロジェクト番号")]
        [DataRow(null, "受注", null, null, null, null, null, null, false, true, DisplayName = "受注番号")]
        [DataRow(null, null, (short)1, null, null, null, null, null, false, true, DisplayName = "受注行番号")]
        [DataRow(null, null, null, -1, null, null, null, null, false, true, DisplayName = "着工日From（境界値）")]
        [DataRow(null, null, null, -30, null, null, null, null, false, true, DisplayName = "着工日From（代表値）")]
        [DataRow(null, null, null, null, 1, null, null, null, false, true, DisplayName = "着工日To（境界値）")]
        [DataRow(null, null, null, null, 30, null, null, null, false, true, DisplayName = "着工日To（代表値）")]
        [DataRow(null, null, null, null, null, "ｻｸ", null, null, false, true, DisplayName = "顧客名（検索用顧客名称）")]
        [DataRow(null, null, null, null, null, "ﾖｳｶ", null, null, false, true, DisplayName = "顧客名（検索用顧客名称カナ）")]
        [DataRow(null, null, null, null, null, null, "ｹﾝ", null, false, true, DisplayName = "案件名")]
        [DataRow(null, null, null, null, null, null, null, 1L, false, true, DisplayName = "責任者ID")]
        [DataRow(null, null, null, null, null, null, null, null, true, true, DisplayName = "自部署の案件のみ")]
        [DataRow(null, null, null, null, null, null, null, null, false, false, DisplayName = "凍結案件を表示しない")]
        public async Task OnGetSearchAnkensAsync_検索条件が空でないとき_マッチするデータを取得すること
            (string? projectNo, string? juchuNo, short? juchuGyoNo, int? fromOffset, int? toOffset,
            string? kokyakuName, string? ankenName, long? sekininSyaId, bool isOwnBusyoOnly, bool showGentaToketsu)
        {
            // Arrange
            fakeTimeProvider.SetLocalNow(new(2026, 2, 15));

            var chaYmd = new DateOnly(2025, 1, 1);
            var anken = new Anken()
            {
                Id = 1,
                KokyakuKaisyaId = 1,
                SyainBaseId = 1,
                KingsJuchuId = 1,
                SearchName = "アンケン",
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
            };
            var syainBase = new SyainBasis()
            {
                Id = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = string.Empty,
            };
            var syain = new Syain()
            {
                Id = 1,
                SyainBaseId = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
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
                ProjectNo = "プロジェクト番号",
                JuchuuNo = "受注番号",
                JuchuuGyoNo = 1,
                ChaYmd = new DateOnly(2025, 1, 1),
                Nendo = 2026,
                IsGenkaToketu = false,
                SekouBumonCd = LoginUserBusyoCode,
                // 不要なNOT NULLカラムに適当に値を詰める
                JucKin = 0,
                ShouhinName = string.Empty,
                JucYmd = DateOnly.MinValue,
                EntYmd = DateOnly.MinValue,
                Bukken = string.Empty,
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = string.Empty,
                BusyoId = 0,
                SearchBukken = string.Empty,
            };
            var kokyaku = new KokyakuKaisha()
            {
                Id = 1,
                SearchName = "ケンサクヨウ",
                SearchNameKana = "ケンサクヨウカナ",
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                Code = 0,
                NameKana = string.Empty,
                Ryakusyou = string.Empty,
                EigyoBaseSyainId = 0,
            };

            db.Add(anken);
            db.Add(syainBase);
            db.Add(syain);
            db.Add(kingsJuchu);
            db.Add(kokyaku);
            db.SaveChanges();

            // 検索条件を設定
            Model!.SearchConditions = new()
            {
                ChaYmd = new()
                {
                    From = fromOffset is null ? null : chaYmd.AddDays(fromOffset.Value),
                    To = toOffset is null ? null : chaYmd.AddDays(toOffset.Value),
                },
                JuchuuNo = new()
                {
                    ProjectNo = projectNo,
                    JuchuuNo = juchuNo,
                    JuchuuGyoNo = juchuGyoNo,
                },
                AnkenName = ankenName,
                KokyakuName = kokyakuName,
                SekininSyaBaseId = sekininSyaId,
                IsOwnBusyoOnly = isOwnBusyoOnly,
                ShowGenkaToketu = showGentaToketsu,
            };

            // Act
            await Model!.OnGetSearchAnkensAsync();

            // Assert
            var actualAnkens = Model.Ankens;
            Assert.IsNotEmpty(actualAnkens);
        }

        [TestMethod]
        [DataRow("ロジェ", "受注", (short)1, -30, 30, "ｻｸ", "ｹﾝ", 1L, LoginUserBusyoCode, false, DisplayName = "プロジェクト番号")]
        [DataRow("プロ", "注番", (short)1, -30, 30, "ｻｸ", "ｹﾝ", 1L, LoginUserBusyoCode, false, DisplayName = "受注番号")]
        [DataRow("プロ", "受注", (short)2, -30, 30, "ｻｸ", "ｹﾝ", 1L, LoginUserBusyoCode, false, DisplayName = "受注行番号")]
        [DataRow("プロ", "受注", (short)1, 1, 30, "ｻｸ", "ｹﾝ", 1L, LoginUserBusyoCode, false, DisplayName = "着工日From（境界値）")]
        [DataRow("プロ", "受注", (short)1, 15, 30, "ｻｸ", "ｹﾝ", 1L, LoginUserBusyoCode, false, DisplayName = "着工日From（代表値）")]
        [DataRow("プロ", "受注", (short)1, -30, -1, "ｻｸ", "ｹﾝ", 1L, LoginUserBusyoCode, false, DisplayName = "着工日To（境界値）")]
        [DataRow("プロ", "受注", (short)1, -30, -15, "ｻｸ", "ｹﾝ", 1L, LoginUserBusyoCode, false, DisplayName = "着工日To（代表値）")]
        [DataRow("プロ", "受注", (short)1, -30, 30, "検索", "ｹﾝ", 1L, LoginUserBusyoCode, false, DisplayName = "顧客名（検索用顧客名称、検索用顧客名称カナ）")]
        [DataRow("プロ", "受注", (short)1, -30, 30, "ｻｸ", "検索", 1L, LoginUserBusyoCode, false, DisplayName = "案件名")]
        [DataRow("プロ", "受注", (short)1, -30, 30, "ｻｸ", "ｹﾝ", 2L, LoginUserBusyoCode, false, DisplayName = "責任者ID")]
        [DataRow("プロ", "受注", (short)1, -30, 30, "ｻｸ", "ｹﾝ", 1L, "", false, DisplayName = "自部署の案件のみ")]
        [DataRow("プロ", "受注", (short)1, -30, 30, "ｻｸ", "ｹﾝ", 1L, LoginUserBusyoCode, true, DisplayName = "凍結案件を表示")]
        public async Task OnGetSearchAnkensAsync_検索条件が空でないとき_マッチしないデータを取得しないこと
            (string? projectNo, string? juchuNo, short? juchuGyoNo, int? fromOffset, int? toOffset,
            string? kokyakuName, string? ankenName, long? sekininSyaId, string sekouBumonCd, bool isGentaToketsu)
        {
            // Arrange
            fakeTimeProvider.SetLocalNow(new(2026, 2, 15));

            var chaYmd = new DateOnly(2025, 1, 1);
            var anken = new Anken()
            {
                Id = 1,
                KokyakuKaisyaId = 1,
                SyainBaseId = 1,
                KingsJuchuId = 1,
                SearchName = "アンケン",
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
            };
            var syainBase = new SyainBasis()
            {
                Id = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Code = string.Empty,
            };
            var syain = new Syain()
            {
                Id = 1,
                SyainBaseId = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
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
                ProjectNo = "プロジェクト番号",
                JuchuuNo = "受注番号",
                JuchuuGyoNo = 1,
                ChaYmd = chaYmd,
                Nendo = 2026,
                SekouBumonCd = sekouBumonCd,
                IsGenkaToketu = isGentaToketsu,
                // 不要なNOT NULLカラムに適当に値を詰める
                JucKin = 0,
                ShouhinName = string.Empty,
                JucYmd = DateOnly.MinValue,
                EntYmd = DateOnly.MinValue,
                Bukken = string.Empty,
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = string.Empty,
                BusyoId = 0,
                SearchBukken = string.Empty,
            };
            var kokyaku = new KokyakuKaisha()
            {
                Id = 1,
                SearchName = "ケンサクヨウ",
                SearchNameKana = "ケンサクヨウカナ",
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                Code = 0,
                NameKana = string.Empty,
                Ryakusyou = string.Empty,
                EigyoBaseSyainId = 0,
            };

            db.Add(anken);
            db.Add(syainBase);
            db.Add(syain);
            db.Add(kingsJuchu);
            db.Add(kokyaku);
            db.SaveChanges();

            // 検索条件を設定
            Model!.SearchConditions = new()
            {
                ChaYmd = new()
                {
                    From = fromOffset is null ? null : chaYmd.AddDays(fromOffset.Value),
                    To = toOffset is null ? null : chaYmd.AddDays(toOffset.Value),
                },
                JuchuuNo = new()
                {
                    ProjectNo = projectNo,
                    JuchuuNo = juchuNo,
                    JuchuuGyoNo = juchuGyoNo,
                },
                AnkenName = ankenName,
                KokyakuName = kokyakuName,
                SekininSyaBaseId = sekininSyaId,
                IsOwnBusyoOnly = true,
                ShowGenkaToketu = false,
            };

            // Act
            await Model!.OnGetSearchAnkensAsync();

            // Assert
            var actualAnkens = Model.Ankens;
            Assert.IsEmpty(actualAnkens);
        }

        [TestMethod]
        public async Task OnGetSearchAnkensAsync_検索条件が空のとき_年度がシステム日付のデータを取得する()
        {
            // Arrange
            fakeTimeProvider.SetLocalNow(new(2026, 2, 15));
            var anken = new Anken()
            {
                KingsJuchuId = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                SearchName = string.Empty,
            };
            var kingsJuchu = new KingsJuchu()
            {
                Id = 1,
                Nendo = 2026,
                IsGenkaToketu = false,
                // 不要なNOT NULLカラムに適当に値を詰める
                SekouBumonCd = string.Empty,
                ProjectNo = string.Empty,
                ChaYmd = new DateOnly(2025, 1, 1),
                JucKin = 0,
                ShouhinName = string.Empty,
                JucYmd = DateOnly.MinValue,
                EntYmd = DateOnly.MinValue,
                Bukken = string.Empty,
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = string.Empty,
                BusyoId = 0,
                SearchBukken = string.Empty,
            };

            db.Add(anken);
            db.Add(kingsJuchu);
            db.SaveChanges();

            // 検索条件を設定
            Model!.SearchConditions = new()
            {
                ChaYmd = new(),
                JuchuuNo = new(),
                IsOwnBusyoOnly = false,
                ShowGenkaToketu = true,
            };

            // Act
            await Model!.OnGetSearchAnkensAsync();

            // Assert
            Assert.IsNotEmpty(Model.Ankens);
        }

        [TestMethod]
        public async Task OnGetSearchAnkensAsync_検索条件が空のとき_年度がシステム日付でないデータを取得しない()
        {
            // Arrange
            fakeTimeProvider.SetLocalNow(new(2026, 2, 15));

            var anken = new Anken()
            {
                KingsJuchuId = 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                Name = string.Empty,
                SearchName = string.Empty,
            };
            var kingsJuchu = new KingsJuchu()
            {
                Id = 1,
                Nendo = 2025,
                IsGenkaToketu = false,
                // 不要なNOT NULLカラムに適当に値を詰める
                SekouBumonCd = string.Empty,
                ProjectNo = string.Empty,
                ChaYmd = new DateOnly(2025, 1, 1),
                JucKin = 0,
                ShouhinName = string.Empty,
                JucYmd = DateOnly.MinValue,
                EntYmd = DateOnly.MinValue,
                Bukken = string.Empty,
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = string.Empty,
                BusyoId = 0,
                SearchBukken = string.Empty,
            };

            db.Add(anken);
            db.Add(kingsJuchu);
            db.SaveChanges();

            // 検索条件を設定
            Model!.SearchConditions = new()
            {
                ChaYmd = new(),
                JuchuuNo = new(),
                IsOwnBusyoOnly = false,
                ShowGenkaToketu = true,
            };

            // Act
            await Model!.OnGetSearchAnkensAsync();

            // Assert
            Assert.IsEmpty(Model.Ankens);
        }

        [TestMethod]
        [DataRow(顧客名, DisplayName = "顧客名の昇順")]
        [DataRow(着工日, DisplayName = "着工日の降順")]
        public async Task OnGetSearchAnkensAsync_取得データの並び順が検索条件のもの(SortKeyList sortKey)
        {
            // Arrange
            fakeTimeProvider.SetLocalNow(new(2026, 2, 15));
            var ankens = new List<Anken>();
            var kokyakus = new List<KokyakuKaisha>();
            var kingsJuchus = new List<KingsJuchu>();
            for (int i = 1; i <= 3; i++)
            {
                ankens.Add(new Anken()
                {
                    Id = i,
                    KokyakuKaisyaId = i,
                    KingsJuchuId = i,
                    // 不要なNOT NULLカラムに適当に値を詰める
                    Name = string.Empty,
                    SearchName = string.Empty,
                });
                kokyakus.Add(new KokyakuKaisha()
                {
                    Id = i,
                    // NOTE: (案件ID, 顧客名称) = (1, 2), (2, 0), (3, 1)となるので、
                    //       顧客名の昇順で案件IDが[2, 3, 1]と並ぶ
                    Name = ((i + 1) % 3).ToString(),
                    // 不要なNOT NULLカラムに適当に値を詰める
                    Code = 0,
                    NameKana = string.Empty,
                    Ryakusyou = string.Empty,
                    EigyoBaseSyainId = 0,
                    SearchName = string.Empty,
                    SearchNameKana = string.Empty,
                });
                kingsJuchus.Add(new KingsJuchu()
                {
                    Id = i,
                    Nendo = 2026,
                    // NOTE: (案件ID, 着工日) = (1, 2025/1/1), (2, 2025/1/2), (3, 2025/1/3)となるので、
                    //       着工日の降順で案件IDが[3, 2, 1]と並ぶ
                    ChaYmd = new DateOnly(2025, 1, 1).AddDays((i + 2) % 3),
                    // 不要なNOT NULLカラムに適当に値を詰める
                    SekouBumonCd = string.Empty,
                    ProjectNo = string.Empty,
                    JucKin = 0,
                    ShouhinName = string.Empty,
                    JucYmd = DateOnly.MinValue,
                    EntYmd = DateOnly.MinValue,
                    Bukken = string.Empty,
                    HiyouShubetuCd = 0,
                    HiyouShubetuCdName = string.Empty,
                    BusyoId = 0,
                    IsGenkaToketu = false,
                    SearchBukken = string.Empty,
                });
            }

            db.AddRange(ankens);
            db.AddRange(kokyakus);
            db.AddRange(kingsJuchus);
            db.SaveChanges();

            // 検索条件を設定
            Model!.SearchConditions = new()
            {
                ChaYmd = new(),
                JuchuuNo = new(),
                IsOwnBusyoOnly = false,
                ShowGenkaToketu = true,
                SortKey = sortKey,
            };

            // Act
            await Model!.OnGetSearchAnkensAsync();

            // Assert
            List<long> expectedAnkenIds = sortKey switch
            {
                顧客名 => [2, 3, 1],
                着工日 => [3, 2, 1],
                _ => []
            };
            var actualAnkenIds = Model.Ankens.Select(a => a.AnkenId).ToList();
            CollectionAssert.AreEqual(expectedAnkenIds, actualAnkenIds);
        }

        [TestMethod]
        public async Task OnGetSearchAnkensAsync_取得件数が最大20件()
        {
            // Arrange
            var ankens = new List<Anken>();
            for (int i = 1; i <= 21; i++)
            {
                ankens.Add(new Anken()
                {
                    Id = i,
                    // 不要なNOT NULLカラムに適当に値を詰める
                    Name = string.Empty,
                    SearchName = string.Empty,
                });
            }

            db.AddRange(ankens);
            db.SaveChanges();

            // 検索条件を設定
            Model!.SearchConditions = new()
            {
                ChaYmd = new(),
                JuchuuNo = new(),
                IsOwnBusyoOnly = false,
                ShowGenkaToketu = true,
            };

            // Act
            await Model!.OnGetSearchAnkensAsync();

            // Assert
            Assert.HasCount(20, Model.Ankens);
            CollectionAssert.DoesNotContain(Model.Ankens.Select(a => a.AnkenId).ToList(), 21);
        }
    }
}
