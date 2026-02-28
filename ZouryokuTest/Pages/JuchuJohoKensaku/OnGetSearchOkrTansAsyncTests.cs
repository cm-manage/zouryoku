using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using Zouryoku.Pages.JuchuJohoKensaku;
using static Zouryoku.Pages.JuchuJohoKensaku.IndexModel.JuchuJohoSearchModel;

namespace ZouryokuTest.Pages.JuchuJohoKensaku
{
    /// <summary>
    /// <see cref="IndexModel.OnGetSearchOkrTansAsync"/>のテストクラス
    /// </summary>
    [TestClass]
    public class OnGetSearchOkrTansAsyncTests : IndexModelTestBase
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
        /// 「送り担当者」確認用データ登録
        /// </summary>
        private void AddForOkrTans()
        {
            AddKingsJuchu(id: 1, iriBusCd: "100", okrTanCd1: "25001", okrTanNm1: "送り担当者サンプル1");
            AddKingsJuchu(id: 2, iriBusCd: "101", okrTanCd1: "25002", okrTanNm1: "送り担当者サンプル2");
            AddKingsJuchu(id: 3, iriBusCd: null, iriBusCdIsSpecified: true,
                                                  okrTanCd1: "25003", okrTanNm1: "送り担当者サンプル3");
            AddKingsJuchu(id: 4, iriBusCd: "100", okrTanCd1: "25001", okrTanNm1: "送り担当者サンプル4");
            AddKingsJuchu(id: 5, iriBusCd: "100", okrTanCd1: "25005", okrTanNm1: "送り担当者サンプル1");
            AddKingsJuchu(id: 6, iriBusCd: "100", okrTanCd1: "25001", okrTanNm1: "送り担当者サンプル1");
        }

        // ======================================
        // テストメソッド
        // ======================================

        //  送り担当者情報リスト取得
        // --------------------------------------

        /// <summary>
        /// 戻り値.ステータス＝正常
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchOkrTansAsync_送り担当者情報リスト取得処理_正常終了()
        {
            // Arrange
            AddForOkrTans();
            db.SaveChanges();

            // Act
            var response = await Model!.OnGetSearchOkrTansAsync("100");

            // Assert
            // 型確認
            var json = Assert.IsInstanceOfType<JsonResult>(response);
            var value = json.Value!;
            var statusProp = value.GetType().GetProperty("status");
            var status = statusProp!.GetValue(value);

            Assert.AreEqual(ResponseStatus.正常, status);
        }

        /// <summary>
        /// データ取得
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchOkrTansAsync_データ取得_値と表示ラベルが取得されている()
        {
            // Arrange
            AddForOkrTans();
            db.SaveChanges();

            // Act
            var response = await Model!.OnGetSearchOkrTansAsync("101");

            // Assert
            var json = Assert.IsInstanceOfType<JsonResult>(response);
            var value = json.Value!;
            var dataProp = value.GetType().GetProperty("data");
            var data = dataProp!.GetValue(value) as IEnumerable<OkrTanInfo>;

            Assert.IsNotNull(data);
            OkrTanInfo okr = data.First();
            Assert.AreEqual("25002", okr.Value);
            Assert.AreEqual("送り担当者サンプル2", okr.Text);
        }

        /// <summary>
        /// 検索条件　パラメータ.送り元部署コード＝NULL
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchOkrTansAsync_検索条件引数NULL_送り元部署コードNULL以外が取得されている()
        {
            // Arrange
            AddForOkrTans();
            db.SaveChanges();

            // Act
            var response = await Model!.OnGetSearchOkrTansAsync(null);

            // Assert
            var json = Assert.IsInstanceOfType<JsonResult>(response);
            var value = json.Value!;
            var dataProp = value.GetType().GetProperty("data");
            var data = dataProp!.GetValue(value) as IEnumerable<OkrTanInfo>;

            Assert.IsNotNull(data);
            Assert.IsFalse(data.Any(o => o.Value == "25003" && o.Text == "送り担当者サンプル3"));
            Assert.IsTrue(data.Any(o => o.Value == "25001" && o.Text == "送り担当者サンプル1"));
        }

        /// <summary>
        /// 検索条件　パラメータ.送り元部署コード＝"100"
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchOkrTansAsync_検索条件引数100_送り元部署コード一致が取得されている()
        {
            // Arrange
            AddForOkrTans();
            db.SaveChanges();

            // Act
            var response = await Model!.OnGetSearchOkrTansAsync("100");

            // Assert
            var json = Assert.IsInstanceOfType<JsonResult>(response);
            var value = json.Value!;
            var dataProp = value.GetType().GetProperty("data");
            var data = dataProp!.GetValue(value) as IEnumerable<OkrTanInfo>;

            Assert.IsNotNull(data);
            Assert.IsFalse(data.Any(o => o.Value == "25003" && o.Text == "送り担当者サンプル3"));
            Assert.IsFalse(data.Any(o => o.Value == "25002" && o.Text == "送り担当者サンプル2"));
            Assert.IsTrue(data.Any(o => o.Value == "25001" && o.Text == "送り担当者サンプル1"));
        }

        /// <summary>
        /// 集約条件　送り担当者コードかつ送り担当者氏名
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task OnGetSearchOkrTansAsync_集約条件引数100_送り担当者コードかつ送り担当者氏名が集約され取得されている()
        {
            // Arrange
            AddForOkrTans();
            db.SaveChanges();

            // Act
            var response = await Model!.OnGetSearchOkrTansAsync("100");

            // Assert
            var json = Assert.IsInstanceOfType<JsonResult>(response);
            var value = json.Value!;
            var dataProp = value.GetType().GetProperty("data");
            var data = dataProp!.GetValue(value) as IEnumerable<OkrTanInfo>;

            Assert.IsNotNull(data);
            Assert.HasCount(3, data);
            Assert.IsTrue(data.Any(o => o.Value == "25001" && o.Text == "送り担当者サンプル1"));
            Assert.IsTrue(data.Any(o => o.Value == "25001" && o.Text == "送り担当者サンプル4"));
            Assert.IsTrue(data.Any(o => o.Value == "25005" && o.Text == "送り担当者サンプル1"));
        }
    }
}
