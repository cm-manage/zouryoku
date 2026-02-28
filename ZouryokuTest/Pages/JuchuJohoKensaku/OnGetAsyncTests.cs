using Zouryoku.Pages.JuchuJohoKensaku;
using static Zouryoku.Pages.JuchuJohoKensaku.IndexModel.JuchuJohoSearchModel;

namespace ZouryokuTest.Pages.JuchuJohoKensaku
{
    /// <summary>
    /// <see cref="IndexModel.OnGetAsync"/>のテストクラス
    /// </summary>
    [TestClass]
    public class OnGetAsyncTests : IndexModelTestBase
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

        // 検索条件初期化
        // --------------------------------------

        /// <summary>
        /// 検索条件初期化
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_検索条件初期化_初期値が正しく設定される()
        {
            // Arrange
            // Act
            await Model!.OnGetAsync();

            // Assert
            Assert.AreEqual("131", Model.SearchConditions.SekouBusyoCd);
            Assert.AreEqual(KeiyakuJoutai.すべて, Model.SearchConditions.Keiyaku);
            Assert.AreEqual(SortKeyList.受注先顧客, Model.SearchConditions.SortKey);
            Assert.IsTrue(Model.IsReferHistory);
        }

        // KINGS受注参照履歴検索処理
        // --------------------------------------

        /// <summary>
        /// 表示データ取得
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetAsync_受注取消がFALSE_カード内各項目が仕様通りに表示される()
        {
            // Arrange
            AddKingsJuchu();
            db.SaveChanges();

            // Act
            await Model!.OnGetAsync();

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
        public async Task OnGetAsync_受注取消がTRUE_記号が表示される()
        {
            // Arrange
            AddKingsJuchu(isGenkaToketu: true);
            db.SaveChanges();

            // Act
            await Model!.OnGetAsync();

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
        public async Task OnGetAsync_検索条件_社員BaseIDで絞り込んでいる()
        {
            // Arrange
            AddKingsJuchu();
            AddKingsJuchu(id: 2, syainBaseId: 888);
            db.SaveChanges();

            // Act
            await Model!.OnGetAsync();

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
        public async Task OnGetAsync_並び順_参照時間降順で並んでいる()
        {
            // Arrange
            AddKingsJuchu(id: 1, sansyouTime: new(2026, 1, 1, 0, 0, 03));
            AddKingsJuchu(id: 2, sansyouTime: new(2026, 1, 1, 0, 0, 01));
            AddKingsJuchu(id: 3, sansyouTime: new(2026, 1, 1, 0, 0, 02));
            db.SaveChanges();

            // Act
            await Model!.OnGetAsync();

            // Assert
            Assert.HasCount(3, Model.Juchus);
            Assert.AreEqual(1, Model.Juchus[0].JuchuId);
            Assert.AreEqual(3, Model.Juchus[1].JuchuId);
            Assert.AreEqual(2, Model.Juchus[2].JuchuId);
        }
    }
}