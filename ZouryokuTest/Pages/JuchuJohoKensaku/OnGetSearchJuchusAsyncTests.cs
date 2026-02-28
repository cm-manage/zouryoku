using Model.Enums;
using Zouryoku.Pages.JuchuJohoKensaku;
using static Zouryoku.Pages.JuchuJohoKensaku.IndexModel.JuchuJohoSearchModel;

namespace ZouryokuTest.Pages.JuchuJohoKensaku
{
    /// <summary>
    /// <see cref="IndexModel.OnGetSearchJuchusAsync"/>のテストクラス
    /// </summary>
    [TestClass]
    public class OnGetSearchJuchusAsyncTests : IndexModelTestBase
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
        // データ登録
        // ======================================

        /// <summary>
        /// 「契約状態区分」確認用データ登録
        /// </summary>
        private void AddForKeiyaku()
        {
            AddKingsJuchu(id: 1, keiyakuJoutaiKbn: ContractClassification.経費);
            AddKingsJuchu(id: 2, keiyakuJoutaiKbn: ContractClassification.受注_自営);
            AddKingsJuchu(id: 3, keiyakuJoutaiKbn: ContractClassification.仮受注_自営);
            AddKingsJuchu(id: 4, keiyakuJoutaiKbn: ContractClassification.受注_共同);
            AddKingsJuchu(id: 5, keiyakuJoutaiKbn: ContractClassification.仮受注_共同);
            AddKingsJuchu(id: 6, keiyakuJoutaiKbn: ContractClassification.受注_社内取引);
        }

        // ======================================
        // テストメソッド
        // ======================================

        // KINGS受注登録検索処理
        // --------------------------------------

        /// <summary>
        /// プロジェクト番号、受注番号、受注行番号、着工日From、着工日To、
        /// 施工部署、件名、送り元部署、送り担当者コード、受注金額、顧客名
        /// 入力なし
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_全項目入力なし_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// プロジェクト番号（入力ありの場合、前方一致）前方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_プロジェクト番号前方一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JuchuuNo.ProjectNo = "13125";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// プロジェクト番号（入力ありの場合、前方一致）後方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_プロジェクト番号後方一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JuchuuNo.ProjectNo = "500701";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 受注番号（入力ありの場合、前方一致）前方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_受注番号前方一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JuchuuNo.JuchuuNo = "J13";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 受注番号（入力ありの場合、前方一致）後方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_受注番号後方一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JuchuuNo.JuchuuNo = "111";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 受注行番号（入力ありの場合、完全一致）完全一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_受注行番号完全一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JuchuuNo.JuchuuGyoNo = 11;

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 受注行番号（入力ありの場合、完全一致）前方後方部分一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_受注行番号前方後方部分一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JuchuuNo.JuchuuGyoNo = 1;

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 着工日From（入力ありの場合、着工日以前）着工日より前
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_着工日From着工日より前_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.ChaYmd.From = new(2024, 12, 31);

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 着工日From（入力ありの場合、着工日以前）着工日同日
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_着工日From着工日同日_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.ChaYmd.From = new(2025, 1, 1);

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 着工日From（入力ありの場合、着工日以前）着工日より後
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_着工日From着工日より後_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.ChaYmd.From = new(2025, 1, 2);

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 着工日To（入力ありの場合、着工日以降）着工日より前
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_着工日To着工日より前_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.ChaYmd.To = new(2024, 12, 31);

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 着工日To（入力ありの場合、着工日以降）着工日同日
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_着工日To着工日同日_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.ChaYmd.To = new(2025, 1, 1);

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 着工日To（入力ありの場合、着工日以降）着工日より後
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_着工日To着工日より後_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.ChaYmd.To = new(2025, 1, 2);

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 施工部署（入力ありの場合、完全一致）完全一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_施工部署完全一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.SekouBusyoCd = "131";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 施工部署（入力ありの場合、完全一致）前方後方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_施工部署前方後方一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.SekouBusyoCd = "121";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 件名（入力ありの場合、部分一致）正規化
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_件名正規化で検索用件名と一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.Bukken = "５ｅｵ";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 件名（入力ありの場合、部分一致）部分一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_件名検索用件名と部分一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.Bukken = "E";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 件名（入力ありの場合、部分一致）非検索用カラム
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_件名非検索用カラムと一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.Bukken = "サンプル";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 送り元部署（入力ありの場合、完全一致）完全一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_送り元部署完全一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.IriBusCd = "100";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 送り元部署（入力ありの場合、完全一致）前方後方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_送り元部署前方後方一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.IriBusCd = "110";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 送り担当者コード（入力ありの場合、完全一致）完全一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_送り担当者コード完全一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.OkrTanCd1 = "25000";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 送り担当者コード（入力ありの場合、完全一致）前方後方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_送り担当者コード前方後方一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.OkrTanCd1 = "24000";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 受注金額（入力ありの場合、以上）超過、カンマあり
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_受注金額超過_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JucKin = "999,999";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 受注金額（入力ありの場合、以上）同額、カンマあり
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_受注金額同額_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JucKin = "1,000,000";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 受注金額（入力ありの場合、以上）未満、カンマあり
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_受注金額未満_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JucKin = "1,000,001";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）正規化、検索用契約先
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_顧客名正規化で検索用契約先と一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "１ａｱ";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）正規化、検索用契約先カナ
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_顧客名正規化で検索用契約先カナと一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "２ｂｲ";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）正規化、検索用受注先
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_顧客名正規化で検索用受注先と一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "３ｃｳ";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）正規化、検索用受注先カナ
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_顧客名正規化で検索用受注先カナと一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "４ｄｴ";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）部分一致、検索用契約先
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_顧客名検索用契約先と部分一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "A";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）部分一致、検索用契約先カナ
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_顧客名検索用契約先カナと部分一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "B";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）部分一致、検索用受注先
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_顧客名検索用受注先と部分一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "C";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）部分一致、検索用受注先カナ
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_顧客名検索用受注先カナと部分一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "D";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）非検索用カラム
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_顧客名非検索用カラムと一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "サンプル";

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// プロジェクト番号、受注番号、受注行番号、着工日From、着工日To、施工部署、
        /// 件名、送り元部署、送り担当者コード、受注金額、顧客名
        /// 入力なし → 年度条件
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_全項目入力なし_年度条件で絞り込みしている()
        {
            // Arrange
            AddKingsJuchu();
            AddKingsJuchu(id: 2, nendo: 1);
            db.SaveChanges();

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(1, Model.Juchus);
            var j = Model.Juchus.First();
            Assert.AreEqual(1, j.JuchuId);
        }

        /// <summary>
        /// 契約状態＝「すべて」
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_契約状態すべて_全件取得している()
        {
            // Arrange
            AddForKeiyaku();
            db.SaveChanges();

            Model!.SearchConditions.Keiyaku = KeiyakuJoutai.すべて;

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.IsTrue(Model.Juchus.Any(j => j.JuchuId == 1));
            Assert.IsTrue(Model.Juchus.Any(j => j.JuchuId == 2));
            Assert.IsTrue(Model.Juchus.Any(j => j.JuchuId == 3));
            Assert.IsTrue(Model.Juchus.Any(j => j.JuchuId == 4));
            Assert.IsTrue(Model.Juchus.Any(j => j.JuchuId == 5));
            Assert.IsTrue(Model.Juchus.Any(j => j.JuchuId == 6));
        }

        /// <summary>
        /// 契約状態＝「自営」
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_契約状態自営_経費と自営を取得している()
        {
            // Arrange
            AddForKeiyaku();
            db.SaveChanges();

            Model!.SearchConditions.Keiyaku = KeiyakuJoutai.自営;

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.IsTrue(Model.Juchus.Any(j => j.JuchuId == 1));
            Assert.IsTrue(Model.Juchus.Any(j => j.JuchuId == 2));
            Assert.IsTrue(Model.Juchus.Any(j => j.JuchuId == 3));
            Assert.IsFalse(Model.Juchus.Any(j => j.JuchuId == 4));
            Assert.IsFalse(Model.Juchus.Any(j => j.JuchuId == 5));
            Assert.IsFalse(Model.Juchus.Any(j => j.JuchuId == 6));
        }

        /// <summary>
        /// 契約状態＝「協同受け」
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_契約状態協同受け_協同を取得している()
        {
            // Arrange
            AddForKeiyaku();
            db.SaveChanges();

            Model!.SearchConditions.Keiyaku = KeiyakuJoutai.協同受け;

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.IsFalse(Model.Juchus.Any(j => j.JuchuId == 1));
            Assert.IsFalse(Model.Juchus.Any(j => j.JuchuId == 2));
            Assert.IsFalse(Model.Juchus.Any(j => j.JuchuId == 3));
            Assert.IsTrue(Model.Juchus.Any(j => j.JuchuId == 4));
            Assert.IsTrue(Model.Juchus.Any(j => j.JuchuId == 5));
            Assert.IsFalse(Model.Juchus.Any(j => j.JuchuId == 6));
        }

        /// <summary>
        /// 契約状態＝「依頼受け」
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_契約状態依頼受け_社内取引を取得している()
        {
            // Arrange
            AddForKeiyaku();
            db.SaveChanges();

            Model!.SearchConditions.Keiyaku = KeiyakuJoutai.依頼受け;

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.IsFalse(Model.Juchus.Any(j => j.JuchuId == 1));
            Assert.IsFalse(Model.Juchus.Any(j => j.JuchuId == 2));
            Assert.IsFalse(Model.Juchus.Any(j => j.JuchuId == 3));
            Assert.IsFalse(Model.Juchus.Any(j => j.JuchuId == 4));
            Assert.IsFalse(Model.Juchus.Any(j => j.JuchuId == 5));
            Assert.IsTrue(Model.Juchus.Any(j => j.JuchuId == 6));
        }

        /// <summary>
        /// 並び順＝「受注先顧客」
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_並び順受注先顧客_受注先カナ昇順で並んでいる()
        {
            // Arrange
            AddKingsJuchu(id: 1, jucKn: "ｼﾞｭﾁｭｳｻｷｻﾝﾌﾟﾙ3");
            AddKingsJuchu(id: 2, jucKn: "ｼﾞｭﾁｭｳｻｷｻﾝﾌﾟﾙ1");
            AddKingsJuchu(id: 3, jucKn: "ｼﾞｭﾁｭｳｻｷｻﾝﾌﾟﾙ2");
            db.SaveChanges();

            Model!.SearchConditions.SortKey = SortKeyList.受注先顧客;

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(3, Model.Juchus);
            Assert.AreEqual(2, Model.Juchus[0].JuchuId);
            Assert.AreEqual(3, Model.Juchus[1].JuchuId);
            Assert.AreEqual(1, Model.Juchus[2].JuchuId);
        }

        /// <summary>
        /// 並び順＝「契約先顧客」
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_並び順契約先顧客_契約先カナ昇順で並んでいる()
        {
            // Arrange
            AddKingsJuchu(id: 1, keiKn: "ｹｲﾔｸｻｷｻﾝﾌﾟﾙ3");
            AddKingsJuchu(id: 2, keiKn: "ｹｲﾔｸｻｷｻﾝﾌﾟﾙ1");
            AddKingsJuchu(id: 3, keiKn: "ｹｲﾔｸｻｷｻﾝﾌﾟﾙ2");
            db.SaveChanges();

            Model!.SearchConditions.SortKey = SortKeyList.契約先顧客;

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(3, Model.Juchus);
            Assert.AreEqual(2, Model.Juchus[0].JuchuId);
            Assert.AreEqual(3, Model.Juchus[1].JuchuId);
            Assert.AreEqual(1, Model.Juchus[2].JuchuId);
        }

        /// <summary>
        /// 並び順＝「受注件名」
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_並び順受注件名_件名昇順で並んでいる()
        {
            // Arrange
            AddKingsJuchu(id: 1, bukken: "件名サンプル3");
            AddKingsJuchu(id: 2, bukken: "件名サンプル1");
            AddKingsJuchu(id: 3, bukken: "件名サンプル2");
            db.SaveChanges();

            Model!.SearchConditions.SortKey = SortKeyList.受注件名;

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(3, Model.Juchus);
            Assert.AreEqual(2, Model.Juchus[0].JuchuId);
            Assert.AreEqual(3, Model.Juchus[1].JuchuId);
            Assert.AreEqual(1, Model.Juchus[2].JuchuId);
        }

        /// <summary>
        /// 並び順＝「着工日」
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_並び順着工日_着工日降順で並んでいる()
        {
            // Arrange
            AddKingsJuchu(id: 1, chaYmd: new DateOnly(2025, 1, 3));
            AddKingsJuchu(id: 2, chaYmd: new DateOnly(2025, 1, 1));
            AddKingsJuchu(id: 3, chaYmd: new DateOnly(2025, 1, 2));
            db.SaveChanges();

            Model!.SearchConditions.SortKey = SortKeyList.着工日;

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(3, Model.Juchus);
            Assert.AreEqual(1, Model.Juchus[0].JuchuId);
            Assert.AreEqual(3, Model.Juchus[1].JuchuId);
            Assert.AreEqual(2, Model.Juchus[2].JuchuId);
        }

        /// <summary>
        /// 並び順＝「受注日」
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchJuchusAsync_並び順受注日_受注日降順で並んでいる()
        {
            // Arrange
            AddKingsJuchu(id: 1, jucYmd: new DateOnly(2025, 1, 3));
            AddKingsJuchu(id: 2, jucYmd: new DateOnly(2025, 1, 1));
            AddKingsJuchu(id: 3, jucYmd: new DateOnly(2025, 1, 2));
            db.SaveChanges();

            Model!.SearchConditions.SortKey = SortKeyList.受注日;

            // Act
            await Model!.OnGetSearchJuchusAsync();

            // Assert
            Assert.HasCount(3, Model.Juchus);
            Assert.AreEqual(1, Model.Juchus[0].JuchuId);
            Assert.AreEqual(3, Model.Juchus[1].JuchuId);
            Assert.AreEqual(2, Model.Juchus[2].JuchuId);
        }
    }
}
