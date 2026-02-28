using Model.Enums;
using Zouryoku.Pages.JuchuJohoKensaku;
using static Zouryoku.Pages.JuchuJohoKensaku.IndexModel.JuchuJohoSearchModel;

namespace ZouryokuTest.Pages.JuchuJohoKensaku
{
    /// <summary>
    /// <see cref="IndexModel.OnGetMovePageAsync"/> のテストクラス
    /// </summary>
    [TestClass]
    public class OnGetMovePageAsyncTests : IndexModelTestBase
    {
        // ======================================
        // ヘルパーメソッド
        // ======================================

        /// <summary>
        /// 全データを取得した際に想定されるKINGS受注IDのリストを作成する
        /// NがpageSize以下（2ページ目が不正なページ番号）の場合は1ページ目を取得した場合の想定リストを作成する
        /// NがpageSize超過（2ページ目がvalidなページ番号）の場合は2ページ目を取得した場合の想定リストを作成する
        /// </summary>
        /// <param name="dataCount">テスト内で作成したデータの個数</param>
        /// <param name="pageSize">テスト対象ページの最大表示データ数／ページ</param>
        /// <returns></returns>
        private List<long> GetExpectedKingsIdList(int dataCount, int pageSize)
        {
            // 2ページ目に最初に現れるデータのインデックス
            var pageStartIndex = pageSize + 1;
            // 2ページ目の最後に現れるデータのインデックス
            var pageEndIndex = pageSize * 2;

            // データ総数に応じて取得想定のデータの範囲を計算する
            // 最初
            var startKingsId = dataCount < pageStartIndex ? 1 : pageStartIndex;
            // 最後
            var endKingsId = pageEndIndex < dataCount ? pageEndIndex : dataCount;
            // 個数
            var count = endKingsId - startKingsId + 1;

            return [.. Enumerable.Range(startKingsId, count).Select(x => (long)x)];
        }

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

        // パラメーター.参照履歴フラグ＝TRUEのとき、 KINGS受注参照履歴検索処理
        // --------------------------------------

        /// <summary>
        /// 表示データ取得
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_受注取消がFALSE_正しく表示される()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            // Act
            await Model!.OnGetMovePageAsync(0, 0, true);

            // Assert
            Assert.HasCount(1, Model.Juchus);
            var j = Model.Juchus.First();
            Assert.AreEqual("13125-500701-J13025000111-11", j.JuchuuNoForDisplay);
            Assert.AreEqual("受注先サンプル", j.JuchuuKokyakuName);
            Assert.AreEqual("契約先サンプル", j.KeiyakuKokyakuName);
            Assert.AreEqual("件名サンプル", j.Bukken);
            Assert.AreEqual("商品名サンプル", j.ShouhinName);
            Assert.AreEqual("1,000,000", j.JucKin);
            Assert.AreEqual("2025/01/01", j.ChaYmd);
            Assert.AreEqual("2026/01/01", j.JucYmd);
            Assert.AreEqual("", j.Deleted);
            Assert.AreEqual(1, j.JuchuId);
        }

        /// <summary>
        /// 表示データ取得（受注取消）
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_受注取消がTRUE_正しく表示される()
        {
            // Arrange
            AddKingsJuchu(isGenkaToketu: true);
            db.SaveChanges();

            // Act
            await Model!.OnGetMovePageAsync(0, 0, true);

            // Assert
            Assert.HasCount(1, Model.Juchus);
            var j = Model.Juchus.First();
            Assert.AreEqual("◎", j.Deleted);
        }

        /// <summary>
        /// 検索条件
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_検索条件_社員BaseIDで絞り込んでいる()
        {
            // Arrange
            AddKingsJuchu();
            AddKingsJuchu(id: 2, syainBaseId: 888);
            db.SaveChanges();

            // Act
            await Model!.OnGetMovePageAsync(0, 0, true);

            // Assert
            Assert.HasCount(1, Model.Juchus);
            var j = Model.Juchus.First();
            Assert.AreEqual(1, j.JuchuId);
        }

        /// <summary>
        /// 並び順
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_並び順_参照時間降順で並んでいる()
        {
            // Arrange
            AddKingsJuchu(id: 1, sansyouTime: new(2026, 1, 1, 0, 0, 03));
            AddKingsJuchu(id: 2, sansyouTime: new(2026, 1, 1, 0, 0, 01));
            AddKingsJuchu(id: 3, sansyouTime: new(2026, 1, 1, 0, 0, 02));
            db.SaveChanges();

            // Act
            await Model!.OnGetMovePageAsync(0, 0, true);

            // Assert
            Assert.HasCount(3, Model.Juchus);
            Assert.AreEqual(1, Model.Juchus[0].JuchuId);
            Assert.AreEqual(3, Model.Juchus[1].JuchuId);
            Assert.AreEqual(2, Model.Juchus[2].JuchuId);
        }

        /// <summary>
        /// KINGS受注参照履歴検索処理の取得範囲
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(10, DisplayName = "取得開始行数の代表値（取得範囲外）")]
        [DataRow(20, DisplayName = "取得開始行数の境界値（取得範囲外）")]
        [DataRow(21, DisplayName = "取得開始行数の境界値（取得範囲内）")]
        [DataRow(30, DisplayName = "取得行数の代表値（取得範囲内）")]
        [DataRow(40, DisplayName = "取得終了行数の境界値（取得範囲内）")]
        [DataRow(41, DisplayName = "取得終了行数の境界値（取得範囲外）")]
        [DataRow(50, DisplayName = "取得終了行数の代表値（取得範囲外）")]
        public async Task OnGetMovePageAsync_KINGS受注参照履歴検索時に取得データが該当ページのもの(int count)
        {
            // Arrange
            AddKingsJuchus(count);
            db.SaveChanges();

            // Act
            // 2ページ目の案件情報を取得する（ページ番号 = 0、オフセット = 1で設定）
            await Model!.OnGetMovePageAsync(0, 1, true);

            // Assert
            var expectedKingsIds = GetExpectedKingsIdList(count, Model.Pager.PageSize);
            var actualKingsIds = Model.Juchus
                .Select(x => x.JuchuId)
                .ToList();
            CollectionAssert.AreEqual(expectedKingsIds, actualKingsIds);
        }

        // パラメーター.参照履歴フラグ＝FALSEのとき、 KINGS受注登録検索処理
        // --------------------------------------

        /// <summary>
        /// プロジェクト番号、受注番号、受注行番号、着工日From、着工日To、施工部署、
        /// 件名、送り元部署、送り担当者コード、受注金額、顧客名
        /// 入力なし
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_全項目入力なし_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// プロジェクト番号（入力ありの場合、前方一致）前方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_プロジェクト番号前方一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JuchuuNo.ProjectNo = "13125";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// プロジェクト番号（入力ありの場合、前方一致）後方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_プロジェクト番号後方一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JuchuuNo.ProjectNo = "500701";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 受注番号（入力ありの場合、前方一致）前方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_受注番号前方一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JuchuuNo.JuchuuNo = "J13";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 受注番号（入力ありの場合、前方一致）後方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_受注番号後方一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JuchuuNo.JuchuuNo = "111";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 受注行番号（入力ありの場合、完全一致）完全一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_受注行番号完全一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JuchuuNo.JuchuuGyoNo = 11;

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 受注行番号（入力ありの場合、完全一致）前方後方部分一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_受注行番号前方後方部分一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JuchuuNo.JuchuuGyoNo = 1;

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 着工日From（入力ありの場合、着工日以前）着工日より前
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_着工日From着工日より前_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.ChaYmd.From = new(2024, 12, 31);

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 着工日From（入力ありの場合、着工日以前）着工日同日
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_着工日From着工日同日_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.ChaYmd.From = new(2025, 1, 1);

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 着工日From（入力ありの場合、着工日以前）着工日より後
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_着工日From着工日より後_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.ChaYmd.From = new(2025, 1, 2);

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 着工日To（入力ありの場合、着工日以降）着工日より前
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_着工日To着工日より前_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.ChaYmd.To = new(2024, 12, 31);

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 着工日To（入力ありの場合、着工日以降）着工日同日
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_着工日To着工日同日_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.ChaYmd.To = new(2025, 1, 1);

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 着工日To（入力ありの場合、着工日以降）着工日より後
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_着工日To着工日より後_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.ChaYmd.To = new(2025, 1, 2);

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 施工部署（入力ありの場合、完全一致）完全一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_施工部署完全一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.SekouBusyoCd = "131";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 施工部署（入力ありの場合、完全一致）前方後方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_施工部署前方後方一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.SekouBusyoCd = "121";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 件名（入力ありの場合、部分一致）正規化
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_件名正規化で検索用件名と一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.Bukken = "５ｅｵ";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 件名（入力ありの場合、部分一致）部分一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_件名検索用件名と部分一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.Bukken = "E";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 件名（入力ありの場合、部分一致）非検索用カラム
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_件名非検索用カラムと一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.Bukken = "サンプル";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 送り元部署（入力ありの場合、完全一致）完全一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_送り元部署完全一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.IriBusCd = "100";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 送り元部署（入力ありの場合、完全一致）前方後方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_送り元部署前方後方一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.IriBusCd = "110";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 送り担当者コード（入力ありの場合、完全一致）完全一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_送り担当者コード完全一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.OkrTanCd1 = "25000";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 送り担当者コード（入力ありの場合、完全一致）前方後方一致
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_送り担当者コード前方後方一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.OkrTanCd1 = "24000";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 受注金額（入力ありの場合、以上）超過、カンマあり
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_受注金額超過_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JucKin = "999,999";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 受注金額（入力ありの場合、以上）同額、カンマあり
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_受注金額同額_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JucKin = "1,000,000";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 受注金額（入力ありの場合、以上）未満、カンマあり
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_受注金額未満_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.JucKin = "1,000,001";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）正規化、検索用契約先
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_顧客名正規化で検索用契約先と一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "１ａｱ";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）正規化、検索用契約先カナ
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_顧客名正規化で検索用契約先カナと一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "２ｂｲ";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）正規化、検索用受注先
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_顧客名正規化で検索用受注先と一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "３ｃｳ";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）正規化、検索用受注先カナ
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_顧客名正規化で検索用受注先カナと一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "４ｄｴ";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）部分一致、検索用契約先
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_顧客名検索用契約先と部分一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "A";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）部分一致、検索用契約先カナ
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_顧客名検索用契約先カナと部分一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "B";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）部分一致、検索用受注先
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_顧客名検索用受注先と部分一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "C";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）部分一致、検索用受注先カナ
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_顧客名検索用受注先カナと部分一致_取得している()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "D";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(1, Model.Juchus);
        }

        /// <summary>
        /// 顧客名（入力ありの場合、部分一致）非検索用カラム
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_顧客名非検索用カラムと一致_取得していない()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            Model!.SearchConditions.KokyakuName = "サンプル";

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(0, Model.Juchus);
        }

        /// <summary>
        /// プロジェクト番号、受注番号、受注行番号、着工日From、着工日To、施工部署、
        /// 件名、送り元部署、送り担当者コード、受注金額、顧客名
        /// 入力なし
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetMovePageAsync_全項目入力なし_年度条件で絞り込みしている()
        {
            // Arrange
            AddKingsJuchu();
            AddKingsJuchu(id: 2, nendo: 1);
            db.SaveChanges();

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

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
        public async Task OnGetMovePageAsync_契約状態すべて_全件取得している()
        {
            // Arrange
            AddForKeiyaku();
            db.SaveChanges();

            Model!.SearchConditions.Keiyaku = KeiyakuJoutai.すべて;

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

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
        public async Task OnGetMovePageAsync_契約状態自営_経費と自営を取得している()
        {
            // Arrange
            AddForKeiyaku();
            db.SaveChanges();

            Model!.SearchConditions.Keiyaku = KeiyakuJoutai.自営;

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

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
        public async Task OnGetMovePageAsync_契約状態協同受け_協同を取得している()
        {
            // Arrange
            AddForKeiyaku();
            db.SaveChanges();

            Model!.SearchConditions.Keiyaku = KeiyakuJoutai.協同受け;

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

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
        public async Task OnGetMovePageAsync_契約状態依頼受け_社内取引を取得している()
        {
            // Arrange
            AddForKeiyaku();
            db.SaveChanges();

            Model!.SearchConditions.Keiyaku = KeiyakuJoutai.依頼受け;

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

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
        public async Task OnGetMovePageAsync_並び順受注先顧客_受注先カナ昇順で並んでいる()
        {
            // Arrange
            AddKingsJuchu(id: 1, jucKn: "ｼﾞｭﾁｭｳｻｷｻﾝﾌﾟﾙ3");
            AddKingsJuchu(id: 2, jucKn: "ｼﾞｭﾁｭｳｻｷｻﾝﾌﾟﾙ1");
            AddKingsJuchu(id: 3, jucKn: "ｼﾞｭﾁｭｳｻｷｻﾝﾌﾟﾙ2");
            db.SaveChanges();

            Model!.SearchConditions.SortKey = SortKeyList.受注先顧客;

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

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
        public async Task OnGetMovePageAsync_並び順契約先顧客_契約先カナ昇順で並んでいる()
        {
            // Arrange
            AddKingsJuchu(id: 1, keiKn: "ｹｲﾔｸｻｷｻﾝﾌﾟﾙ3");
            AddKingsJuchu(id: 2, keiKn: "ｹｲﾔｸｻｷｻﾝﾌﾟﾙ1");
            AddKingsJuchu(id: 3, keiKn: "ｹｲﾔｸｻｷｻﾝﾌﾟﾙ2");
            db.SaveChanges();

            Model!.SearchConditions.SortKey = SortKeyList.契約先顧客;

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

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
        public async Task OnGetMovePageAsync_並び順受注件名_件名昇順で並んでいる()
        {
            // Arrange
            AddKingsJuchu(id: 1, bukken: "件名サンプル3");
            AddKingsJuchu(id: 2, bukken: "件名サンプル1");
            AddKingsJuchu(id: 3, bukken: "件名サンプル2");
            db.SaveChanges();

            Model!.SearchConditions.SortKey = SortKeyList.受注件名;

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

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
        public async Task OnGetMovePageAsync_並び順着工日_着工日降順で並んでいる()
        {
            // Arrange
            AddKingsJuchu(id: 1, chaYmd: new DateOnly(2025, 1, 3));
            AddKingsJuchu(id: 2, chaYmd: new DateOnly(2025, 1, 1));
            AddKingsJuchu(id: 3, chaYmd: new DateOnly(2025, 1, 2));
            db.SaveChanges();

            Model!.SearchConditions.SortKey = SortKeyList.着工日;

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

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
        public async Task OnGetMovePageAsync_並び順受注日_受注日降順で並んでいる()
        {
            // Arrange
            AddKingsJuchu(id: 1, jucYmd: new DateOnly(2025, 1, 3));
            AddKingsJuchu(id: 2, jucYmd: new DateOnly(2025, 1, 1));
            AddKingsJuchu(id: 3, jucYmd: new DateOnly(2025, 1, 2));
            db.SaveChanges();

            Model!.SearchConditions.SortKey = SortKeyList.受注日;

            // Act
            await Model!.OnGetMovePageAsync(0, 0, false);

            // Assert
            Assert.HasCount(3, Model.Juchus);
            Assert.AreEqual(1, Model.Juchus[0].JuchuId);
            Assert.AreEqual(3, Model.Juchus[1].JuchuId);
            Assert.AreEqual(2, Model.Juchus[2].JuchuId);
        }

        /// <summary>
        /// KINGS受注登録検索処理の取得範囲
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(10, DisplayName = "取得開始行数の代表値（取得範囲外）")]
        [DataRow(20, DisplayName = "取得開始行数の境界値（取得範囲外）")]
        [DataRow(21, DisplayName = "取得開始行数の境界値（取得範囲内）")]
        [DataRow(30, DisplayName = "取得行数の代表値（取得範囲内）")]
        [DataRow(40, DisplayName = "取得終了行数の境界値（取得範囲内）")]
        [DataRow(41, DisplayName = "取得終了行数の境界値（取得範囲外）")]
        [DataRow(50, DisplayName = "取得終了行数の代表値（取得範囲外）")]
        public async Task OnGetMovePageAsync_KINGS受注登録検索時に取得データが該当ページのもの(int count)
        {
            // Arrange
            AddKingsJuchus(count);
            db.SaveChanges();

            // Act
            // 2ページ目の案件情報を取得する（ページ番号 = 0、オフセット = 1で設定）
            await Model!.OnGetMovePageAsync(0, 1, false);

            // Assert
            var expectedKingsIds = GetExpectedKingsIdList(count, Model.Pager.PageSize);
            var actualKingsIds = Model.Juchus
                .Select(x => x.JuchuId)
                .ToList();
            CollectionAssert.AreEqual(expectedKingsIds, actualKingsIds);
        }
    }
}