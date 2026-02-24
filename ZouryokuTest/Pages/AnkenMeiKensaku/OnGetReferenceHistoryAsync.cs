using CommonLibrary.Extensions;
using Model.Model;
using Zouryoku.Pages.AnkenMeiKensaku;

namespace ZouryokuTest.Pages.AnkenMeiKensaku
{
    /// <summary>
    /// <see cref="IndexModel.OnGetReferenceHistoryAsync"/>のテストクラス
    /// </summary>
    [TestClass]
    public class OnGetReferenceHistoryAsyncTest : IndexModelTestBase
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
        public async Task OnGetReferenceHistoryAsync_各テーブルが外部結合されていること
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
            await Model!.OnGetReferenceHistoryAsync();

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
        public async Task OnGetReferenceHistoryAsync_データを取得していること(int startYmdOffset, int endYmdOffset, string? expectedSekininSyaName)
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
                StartYmd = fakeTimeProvider.Today().AddDays(startYmdOffset),
                EndYmd = fakeTimeProvider.Today().AddDays(endYmdOffset),
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
            await Model!.OnGetReferenceHistoryAsync();

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
        public async Task OnGetReferenceHistoryAsync_案件参照履歴の社員BaseIDがログインユーザーのもの_データを取得()
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
            await Model!.OnGetReferenceHistoryAsync();

            // Assert
            Assert.IsNotEmpty(Model.Ankens);
        }

        [TestMethod]
        public async Task OnGetReferenceHistoryAsync_案件参照履歴の社員BaseIDがログインユーザーのものでない_データを取得しない()
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
            await Model!.OnGetReferenceHistoryAsync();

            // Assert
            Assert.IsEmpty(Model.Ankens);
        }

        [TestMethod]
        public async Task OnGetReferenceHistoryAsync_取得データの並び順が参照時間の降順()
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
            await Model!.OnGetReferenceHistoryAsync();

            // Assert
            var expectedAnkenIds = new List<long>() { 3, 2, 1 };
            var actualAnkenIds = Model.Ankens.Select(a => a.AnkenId).ToList();
            CollectionAssert.AreEqual(expectedAnkenIds, actualAnkenIds);
        }

        [TestMethod]
        public async Task OnGetReferenceHistoryAsync_取得件数が最大20件()
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
            await Model!.OnGetReferenceHistoryAsync();

            // Assert
            Assert.HasCount(20, Model.Ankens);
            CollectionAssert.DoesNotContain(Model.Ankens.Select(a => a.AnkenId).ToList(), 21);
        }
    }
}
