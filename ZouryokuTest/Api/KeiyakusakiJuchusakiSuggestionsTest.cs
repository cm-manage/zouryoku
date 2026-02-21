using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model.Model;
using System.Text.Json;
using Zouryoku.Api;
using static Zouryoku.Utils.StringUtil;

namespace ZouryokuTest.Api
{
    /// <summary>
    /// 契約先・受注先のオートコンプリート候補取得APIのテストクラス
    /// </summary>
    [TestClass]
    public class KeiyakusakiJuchusakiSuggestionsTest : BaseInMemoryDbContextTest
    {
        // ======================================
        // 定数
        // ======================================

        const string KeiNm = "契約先サンプル";
        const string JucNm = "受注先サンプル";

        const string SearchKei = "１ａｱ";
        const string SearchKeiKana = "２ｂｲ";
        const string SearchJuc = "３ｃｳ";
        const string SearchJucKana = "４ｄｴ";

        // ======================================
        // 補助メソッド
        // ======================================

        // コントローラー関連
        // --------------------------------------

        /// <summary>
        /// レスポンスから顧客名のリストを取得する
        /// </summary>
        /// <param name="result">（<see cref="JsonResult"/>にキャストしたレスポンス</param>
        /// <returns>顧客名のリスト</returns>
        private List<string> GetCustomerNamesFromResult(JsonResult result)
        {
            var json = ObjectExtensions.ToJson(result.Value!);
            var items = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json)!;
            return [.. items.Select(item => item["label"])];
        }

        /// <summary>
        /// インメモリDBを備えたコントローラーを作成する
        /// </summary>
        /// <returns>コントローラーのインスタンス</returns>
        private KeiyakusakiJuchusakiSuggestionsController CreateController() => new(db);

        // ======================================
        // テストデータ作成
        // ======================================

        /// <summary>
        /// 検索条件の確認用テストデータをインメモリDBに作成する
        /// </summary>
        private void CreateDataForSearchTest()
        {
            db.Add(new KingsJuchu
            {
                Id = 1,
                JucYmd = new(2026, 2, 13),
                EntYmd = new(2026, 2, 14),
                KeiNm = KeiNm,
                KeiKn = "ケイヤクサキサンプル",
                JucNm = JucNm,
                JucKn = "ジュチュウサキサンプル",
                Bukken = "",
                JucKin = 0,
                ChaYmd = new(2026, 2, 15),
                ProjectNo = "",
                SekouBumonCd = "",
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = "",
                IsGenkaToketu = false,
                Nendo = 2025,
                BusyoId = 0,
                SearchKeiNm = NormalizeString(SearchKei),
                SearchKeiKn = NormalizeString(SearchKeiKana),
                SearchJucNm = NormalizeString(SearchJuc),
                SearchJucKn = NormalizeString(SearchJucKana),
                SearchBukken = "",
            });

            db.Add(new KingsJuchu
            {
                Id = 2,
                JucYmd = new(2026, 2, 13),
                EntYmd = new(2026, 2, 14),
                KeiNm = null,
                KeiKn = null,
                JucNm = null,
                JucKn = null,
                Bukken = "",
                JucKin = 0,
                ChaYmd = new(2026, 2, 15),
                ProjectNo = "",
                SekouBumonCd = "",
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = "",
                IsGenkaToketu = false,
                Nendo = 2025,
                BusyoId = 0,
                SearchKeiNm = KeiNm,
                SearchKeiKn = KeiNm,
                SearchJucNm = KeiNm,
                SearchJucKn = KeiNm,
                SearchBukken = "",
            });
        }

        /// <summary>
        /// UNIONの確認用のテストデータを作成する
        /// </summary>
        private void CreateDataForUnionTest()
        {
            db.AddRange(
                new KingsJuchu
                {
                    Id = 1,
                    JucYmd = new(2026, 2, 13),
                    EntYmd = new(2026, 2, 14),
                    KeiNm = KeiNm,
                    KeiKn = "カ",
                    JucNm = "",
                    JucKn = "",
                    Bukken = "",
                    JucKin = 0,
                    ChaYmd = new(2026, 2, 15),
                    ProjectNo = "",
                    SekouBumonCd = "",
                    HiyouShubetuCd = 0,
                    HiyouShubetuCdName = "",
                    IsGenkaToketu = false,
                    Nendo = 2025,
                    BusyoId = 0,
                    SearchKeiNm = NormalizeString(SearchKei),
                    SearchKeiKn = "",
                    SearchJucNm = "",
                    SearchJucKn = "",
                    SearchBukken = "",
                },
                new KingsJuchu
                {
                    Id = 2,
                    JucYmd = new(2026, 2, 13),
                    EntYmd = new(2026, 2, 14),
                    KeiNm = "",
                    KeiKn = "",
                    JucNm = JucNm,
                    JucKn = "ア",
                    Bukken = "",
                    JucKin = 0,
                    ChaYmd = new(2026, 2, 15),
                    ProjectNo = "",
                    SekouBumonCd = "",
                    HiyouShubetuCd = 0,
                    HiyouShubetuCdName = "",
                    IsGenkaToketu = false,
                    Nendo = 2025,
                    BusyoId = 0,
                    SearchKeiNm = "",
                    SearchKeiKn = "",
                    SearchJucNm = NormalizeString(SearchKei),
                    SearchJucKn = "",
                    SearchBukken = "",
                }
            );
        }

        /// <summary>
        /// 集約条件の確認用のテストデータを作成する
        /// 同一 契約先,受注先 を複数レコードでヒットさせる
        /// </summary>
        private void CreateDataForDistinctTest()
        {
            db.AddRange(
                new KingsJuchu
                {
                    Id = 1,
                    JucYmd = new(2026, 2, 13),
                    EntYmd = new(2026, 2, 14),
                    KeiNm = KeiNm,
                    KeiKn = "イ",
                    JucNm = "",
                    JucKn = "",
                    Bukken = "",
                    JucKin = 0,
                    ChaYmd = new(2026, 2, 15),
                    ProjectNo = "",
                    SekouBumonCd = "",
                    HiyouShubetuCd = 0,
                    HiyouShubetuCdName = "",
                    IsGenkaToketu = false,
                    Nendo = 2025,
                    BusyoId = 0,
                    SearchKeiNm = NormalizeString(SearchKei),
                    SearchKeiKn = "",
                    SearchJucNm = "",
                    SearchJucKn = "",
                    SearchBukken = "",
                },
                new KingsJuchu
                {
                    Id = 2,
                    JucYmd = new(2026, 2, 13),
                    EntYmd = new(2026, 2, 14),
                    KeiNm = KeiNm,
                    KeiKn = "イ",
                    JucNm = "",
                    JucKn = "",
                    Bukken = "",
                    JucKin = 0,
                    ChaYmd = new(2026, 2, 15),
                    ProjectNo = "",
                    SekouBumonCd = "",
                    HiyouShubetuCd = 0,
                    HiyouShubetuCdName = "",
                    IsGenkaToketu = false,
                    Nendo = 2025,
                    BusyoId = 0,
                    SearchKeiNm = NormalizeString(SearchKei),
                    SearchKeiKn = "",
                    SearchJucNm = "",
                    SearchJucKn = "",
                    SearchBukken = "",
                },
                new KingsJuchu
                {
                    Id = 3,
                    JucYmd = new(2026, 2, 13),
                    EntYmd = new(2026, 2, 14),
                    KeiNm = "",
                    KeiKn = "",
                    JucNm = KeiNm,
                    JucKn = "ア",
                    Bukken = "",
                    JucKin = 0,
                    ChaYmd = new(2026, 2, 15),
                    ProjectNo = "",
                    SekouBumonCd = "",
                    HiyouShubetuCd = 0,
                    HiyouShubetuCdName = "",
                    IsGenkaToketu = false,
                    Nendo = 2025,
                    BusyoId = 0,
                    SearchKeiNm = "",
                    SearchKeiKn = "",
                    SearchJucNm = NormalizeString(SearchKei),
                    SearchJucKn = "",
                    SearchBukken = "",
                }
            );
        }

        /// <summary>
        /// 取得件数確認用のデータを作成する
        /// （重複が働かないよう Name はユニークに）
        /// </summary>
        /// <param name="count">テストデータの件数</param>
        private void CreateDataForCountTest(int count)
        {
            db.AddRange(Enumerable.Range(1, count).Select(i =>
                new KingsJuchu
                {
                    Id = i,
                    JucYmd = new(2026, 2, 13),
                    EntYmd = new(2026, 2, 14),
                    KeiNm = GetKokyakuNameWithNumber(i),
                    KeiKn = "ケイヤクサキサンプル",
                    JucNm = JucNm,
                    JucKn = "ジュチュウサキサンプル",
                    Bukken = "",
                    JucKin = 0,
                    ChaYmd = new(2026, 2, 15),
                    ProjectNo = "",
                    SekouBumonCd = "",
                    HiyouShubetuCd = 0,
                    HiyouShubetuCdName = "",
                    IsGenkaToketu = false,
                    Nendo = 2025,
                    BusyoId = 0,
                    SearchKeiNm = NormalizeString(SearchKei),
                    SearchKeiKn = NormalizeString(SearchKeiKana),
                    SearchJucNm = NormalizeString(SearchJuc),
                    SearchJucKn = NormalizeString(SearchJucKana),
                    SearchBukken = "",
                }
            ));
        }

        /// <summary>
        /// 並び順の確認用のテストデータを作成する
        /// </summary>
        private void CreateDataForSortTest()
        {
            db.AddRange(
                new KingsJuchu
                {
                    Id = 1,
                    JucYmd = new(2026, 2, 13),
                    EntYmd = new(2026, 2, 14),
                    KeiNm = "",
                    KeiKn = "",
                    JucNm = "いち",
                    JucKn = "ア",
                    Bukken = "",
                    JucKin = 0,
                    ChaYmd = new(2026, 2, 15),
                    ProjectNo = "",
                    SekouBumonCd = "",
                    HiyouShubetuCd = 0,
                    HiyouShubetuCdName = "",
                    IsGenkaToketu = false,
                    Nendo = 2025,
                    BusyoId = 0,
                    SearchKeiNm = "",
                    SearchKeiKn = "",
                    SearchJucNm = NormalizeString(SearchKei),
                    SearchJucKn = "",
                    SearchBukken = "",
                },
                new KingsJuchu
                {
                    Id = 2,
                    JucYmd = new(2026, 2, 13),
                    EntYmd = new(2026, 2, 14),
                    KeiNm = "さん",
                    KeiKn = "サ",
                    JucNm = "",
                    JucKn = "",
                    Bukken = "",
                    JucKin = 0,
                    ChaYmd = new(2026, 2, 15),
                    ProjectNo = "",
                    SekouBumonCd = "",
                    HiyouShubetuCd = 0,
                    HiyouShubetuCdName = "",
                    IsGenkaToketu = false,
                    Nendo = 2025,
                    BusyoId = 0,
                    SearchKeiNm = NormalizeString(SearchKei),
                    SearchKeiKn = "",
                    SearchJucNm = "",
                    SearchJucKn = "",
                    SearchBukken = "",
                },
                new KingsJuchu
                {
                    Id = 3,
                    JucYmd = new(2026, 2, 13),
                    EntYmd = new(2026, 2, 14),
                    KeiNm = "に",
                    KeiKn = "カ",
                    JucNm = "",
                    JucKn = "",
                    Bukken = "",
                    JucKin = 0,
                    ChaYmd = new(2026, 2, 15),
                    ProjectNo = "",
                    SekouBumonCd = "",
                    HiyouShubetuCd = 0,
                    HiyouShubetuCdName = "",
                    IsGenkaToketu = false,
                    Nendo = 2025,
                    BusyoId = 0,
                    SearchKeiNm = NormalizeString(SearchKei),
                    SearchKeiKn = "",
                    SearchJucNm = "",
                    SearchJucKn = "",
                    SearchBukken = "",
                }
            );
        }

        /// <summary>
        /// 番号のサフィックスを付加した契約先名称を取得する
        /// </summary>
        /// <param name="number">番号</param>
        /// <returns></returns>
        private string GetKokyakuNameWithNumber(long number) => KeiNm + number.ToString("D2");

        // ======================================
        // テストメソッド
        // ======================================

        // パラメータチェック
        // --------------------------------------

        /// <summary>
        /// 異常系：検索ワードが不正な場合に空リストを返却すること
        /// </summary>
        /// <param name="term">入力値</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(null, DisplayName = "NULLの場合")]
        [DataRow("", DisplayName = "空文字の場合")]
        public async Task GetSuggestionsAsync_入力値が不正_空リストを返却(string? term)
        {
            // Arrange
            var controller = CreateController();
            CreateDataForSearchTest();
            db.SaveChanges();

            // Act
            var response = await controller.GetSuggestionsAsync(term);

            // Assert
            var kokyakuNames = GetCustomerNamesFromResult((JsonResult)response);
            Assert.IsNotNull(kokyakuNames);
            Assert.IsEmpty(kokyakuNames);
        }

        // 検索条件（契約先）
        // --------------------------------------

        /// <summary>
        /// 正常系：検索条件（契約先）にヒットするデータを取得すること
        /// </summary>
        /// <param name="term">検索ワード</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(SearchKei, DisplayName = "検索用契約先名称にヒット")]
        [DataRow(SearchKeiKana, DisplayName = "検索用契約先名称カナにヒット")]
        public async Task GetSuggestionsAsync_検索条件契約先にヒット_データを取得(string term)
        {
            // Arrange
            var controller = CreateController();
            CreateDataForSearchTest();
            db.SaveChanges();

            // Act
            var response = await controller.GetSuggestionsAsync(term);

            // Assert
            var kokyakuNames = GetCustomerNamesFromResult((JsonResult)response);
            var kokyakuName = kokyakuNames.Single();
            Assert.AreEqual(KeiNm, kokyakuName);
        }

        /// <summary>
        /// 正常系：検索条件（契約先）にヒットしないデータを取得しないこと
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetSuggestionsAsync_検索条件契約先にヒットしない_データを取得しない()
        {
            // Arrange
            var controller = CreateController();
            CreateDataForSearchTest();
            db.SaveChanges();
            // テストデータ内の検索用カラムに存在しないワードを作成
            var term = SearchKei + "0";

            // Act
            var response = await controller.GetSuggestionsAsync(term);

            // Assert
            var kokyakuNames = GetCustomerNamesFromResult((JsonResult)response);
            var isExist = kokyakuNames.Any();
            Assert.IsFalse(isExist);
        }

        // 検索条件（受注先）
        // --------------------------------------

        /// <summary>
        /// 正常系：検索条件（受注先）にヒットするデータを取得すること
        /// </summary>
        /// <param name="term">検索ワード</param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(SearchJuc, DisplayName = "検索用受注先名称にヒット")]
        [DataRow(SearchJucKana, DisplayName = "検索用受注先名称カナにヒット")]
        public async Task GetSuggestionsAsync_検索条件受注先にヒット_データを取得(string term)
        {
            // Arrange
            var controller = CreateController();
            CreateDataForSearchTest();
            db.SaveChanges();

            // Act
            var response = await controller.GetSuggestionsAsync(term);

            // Assert
            var kokyakuNames = GetCustomerNamesFromResult((JsonResult)response);
            var kokyakuName = kokyakuNames.Single();
            Assert.AreEqual(JucNm, kokyakuName);
        }

        /// <summary>
        /// 正常系：検索条件（受注先）にヒットしないデータを取得しないこと
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetSuggestionsAsync_検索条件受注先にヒットしない_データを取得しない()
        {
            // Arrange
            var controller = CreateController();
            CreateDataForSearchTest();
            db.SaveChanges();
            // テストデータ内の検索用カラムに存在しないワードを作成
            var term = SearchJuc + "0";

            // Act
            var response = await controller.GetSuggestionsAsync(term);

            // Assert
            var kokyakuNames = GetCustomerNamesFromResult((JsonResult)response);
            var isExist = kokyakuNames.Any();
            Assert.IsFalse(isExist);
        }

        /// <summary>
        /// 正常系：名称がNULLのデータを取得しないこと
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetSuggestionsAsync_名称がNULL_データを取得しない()
        {
            // Arrange
            var controller = CreateController();
            CreateDataForSearchTest();
            db.SaveChanges();

            // Act
            var response = await controller.GetSuggestionsAsync(KeiNm);

            // Assert
            var kokyakuNames = GetCustomerNamesFromResult((JsonResult)response);
            var isExist = kokyakuNames.Any();
            Assert.IsFalse(isExist);
        }

        // UNION
        // --------------------------------------

        /// <summary>
        /// 正常系：検索条件（契約先・受注先）両方にヒットするデータがあり、名称が重複していない場合、両方の名称を取得すること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetSuggestionsAsync_契約先受注先両方ヒット_両方の名称を取得する()
        {
            var controller = CreateController();
            CreateDataForUnionTest();
            db.SaveChanges();

            var response = await controller.GetSuggestionsAsync(SearchKei);
            var labels = GetCustomerNamesFromResult((JsonResult)response);

            Assert.HasCount(2, labels);
            Assert.Contains(KeiNm, labels);
            Assert.Contains(JucNm, labels);
        }

        // 集約
        // --------------------------------------

        /// <summary>
        /// 正常系：検索条件（契約先・受注先）両方にヒットするデータがあり、名称が重複している場合、集約すること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetSuggestionsAsync_契約先受注先両方ヒット_同一名称は集約()
        {
            var controller = CreateController();
            CreateDataForDistinctTest();
            db.SaveChanges();

            var response = await controller.GetSuggestionsAsync(SearchKei);
            var labels = GetCustomerNamesFromResult((JsonResult)response);

            Assert.HasCount(1, labels);
            Assert.AreEqual(KeiNm, labels.Single());
        }

        // 取得件数 / 並び順
        // --------------------------------------

        /// <summary>
        /// 正常系：取得件数が最大5件であること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetSuggestionsAsync_最大5件を取得()
        {
            var controller = CreateController();
            CreateDataForCountTest(6);
            db.SaveChanges();

            var response = await controller.GetSuggestionsAsync(SearchKei);
            var labels = GetCustomerNamesFromResult((JsonResult)response);

            Assert.HasCount(5, labels);
        }

        /// <summary>
        /// 正常系：契約先カナ・受注先カナの昇順で取得していること
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetSuggestionsAsync_契約先受注先混在_カナ昇順で取得()
        {
            var controller = CreateController();
            CreateDataForSortTest();
            db.SaveChanges();

            var response = await controller.GetSuggestionsAsync(SearchKei);
            var labels = GetCustomerNamesFromResult((JsonResult)response);

            CollectionAssert.AreEqual(new List<string> { "いち", "に", "さん" }, labels);
        }
    }
}
