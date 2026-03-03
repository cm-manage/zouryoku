using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using Model.Model;
using System.Text.Json;
using Zouryoku.Pages.BusyoSentaku;
using Zouryoku.Utils;

namespace ZouryokuTest.Pages.BusyoSentaku
{
    /// <summary>
    /// IndexModel (部署選択) のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {
        private IndexModel CreateModel()
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, fakeTimeProvider);
            model.PageContext = GetPageContext();
            model.TempData = GetTempData();
            return model;
        }

        // FancyTreeノードのデシリアライズヘルパーメソッド
        private static List<FancyNode> DeserializeNodes(JsonResult json)
            => JsonSerializer.Deserialize<List<FancyNode>>(JsonSerializer.Serialize(json.Value))!;

        // 部署データを追加するヘルパーメソッド
        private Busyo AddBusyo(int id, string code, string name, short jyunjyo, bool active, int? parentId = null, int? start = null, int? end = null)
        {
            var today = fakeTimeProvider.Today();
            DateOnly startYmd = start == null ? DateOnly.MinValue : today.AddDays(start.Value);
            DateOnly endYmd = end == null ? DateOnly.MaxValue : today.AddDays(end.Value);

            var busyo = new Busyo()
            {
                Id = id,
                Code = code,
                Name = name,
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = startYmd,
                EndYmd = endYmd,
                Jyunjyo = jyunjyo,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = active,
                BusyoBaseId = 1,
                OyaId = parentId,
            };

            db.Busyos.Add(busyo);
            return busyo;
        }

        // 応答のアサーション
        private static JsonResult AssertJson(IActionResult result)
        {
            Assert.IsInstanceOfType<JsonResult>(result);
            return (JsonResult)result;
        }

        /// <summary>
        /// ①正常: アクティブフラグ＝FALSE のレコードが部署一覧に取得されないこと
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_アクティブフラグがFALSE_部署一覧に取得されない()
        {
            // Arrange
            AddBusyo(1, "002", "部署A", 2, false);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetTreeAsync();

            // Assert
            var json = AssertJson(result);

            var nodes = DeserializeNodes(json);

            Assert.HasCount(0, nodes, "FancyTree 一覧の件数が一致しません。");
        }

        /// <summary>
        /// ②正常: システム日付＜部署マスタ.有効開始日 のレコードが部署一覧に取得されないこと
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_有効開始日がシステム日付より後_部署一覧に取得されない()
        {
            // Arrange
            AddBusyo(1, "002", "部署A", 2, true, parentId: null, start: 1);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetTreeAsync();

            // Assert
            var json = AssertJson(result);

            var nodes = DeserializeNodes(json);

            Assert.HasCount(0, nodes, "FancyTree 一覧の件数が一致しません。");
        }

        /// <summary>
        /// ③正常: 部署マスタ.有効終了日＜システム日付 のレコードが部署一覧に取得されないこと
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_有効終了日がシステム日付より前_部署一覧に取得されない()
        {
            // Arrange
            AddBusyo(1, "002", "部署A", 2, true, parentId: null, start: null, end: -1);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetTreeAsync();

            // Assert
            var json = AssertJson(result);

            var nodes = DeserializeNodes(json);

            Assert.HasCount(0, nodes, "FancyTree 一覧の件数が一致しません。");
        }

        /// <summary>
        /// ④正常: アクティブフラグ＝TRUE、部署マスタ.有効開始日＜＝システム日付、システム日付＜＝部署マスタ.有効終了日 のレコードの
        /// 「ID、部署番号、部署名称、並び順序、親ID」が部署一覧に取得されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_アクティブフラグがTRUEかつ有効開始日がシステム日付以前かつ有効終了日がシステム日付以後_部署一覧に取得される()
        {
            // Arrange
            // 境界値：部署マスタ.有効開始日＝システム日付、システム日付＝部署マスタ.有効終了日
            AddBusyo(1, "002", "部署A", 2, true, parentId: null, start: 0, end: 0);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetTreeAsync();

            // Assert
            var json = AssertJson(result);

            var nodes = DeserializeNodes(json);

            Assert.HasCount(1, nodes, "FancyTree 一覧の件数が一致しません。");

            var node = nodes[0];
            Assert.AreEqual(1, node.Key, "1件目の ID(Id) が一致しません。");
            Assert.AreEqual("002", node.Data.Code, "1件目の 部署番号(Code) が一致しません。");
            Assert.AreEqual("部署A", node.Title, "1件目の 部署名称(Name) が一致しません。");
            Assert.AreEqual(2, node.Data.Jyunjyo, "1件目の 並び順序(Jyunjyo) が一致しません。");
            Assert.IsNull(node.Data.OyaId, "1件目の 親ID(OyaId) が一致しません。");
        }

        /// <summary>
        /// ⑤正常: アクティブフラグ＝TRUEだが親項目のアクティブフラグ＝FALSEのレコードが部署一覧に取得されないこと
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_アクティブフラグがTRUEかつ親項目のアクティブフラグがFALSE_部署一覧に取得されない()
        {
            // Arrange
            // 子
            AddBusyo(2, "004", "子部署", 2, true, parentId: 1);
            // 親
            AddBusyo(1, "003", "親部署", 1, false);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetTreeAsync();

            // Assert
            var json = AssertJson(result);

            var nodes = DeserializeNodes(json);

            Assert.HasCount(0, nodes, "FancyTree 一覧の件数が一致しません。");
        }

        /// <summary>
        /// ⑥正常: アクティブフラグ＝TRUEで親項目のアクティブフラグ＝TRUEのレコードが部署一覧に取得されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_アクティブフラグがTRUEかつ親項目のアクティブフラグがTRUE_部署一覧に取得される()
        {
            // Arrange
            // 子
            AddBusyo(2, "004", "子部署", 2, true, parentId: 1);
            // 親
            AddBusyo(1, "001", "親部署", 1, true);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetTreeAsync();

            // Assert
            var json = AssertJson(result);

            var nodes = DeserializeNodes(json);

            Assert.HasCount(1, nodes, "FancyTree 親項目の件数が一致しません。");
            Assert.HasCount(1, nodes[0].Children, "FancyTree 子項目の件数が一致しません。");
        }

        /// <summary>
        /// ⑦入力チェックエラー: 複数選択モードで 未選択→確定の場合 JsonResult（errorsあり）が返却されること
        /// </summary>
        [TestMethod]
        public void OnPostValidateSelection_複数選択モードで未選択のまま確定ボタン押下_エラー()
        {
            // Arrange
            var model = CreateModel();

            model.MultiFlag = true;
            var input = new IndexModel.ValidateSelectionRequest
            {
                SelectedIds = [] // 未選択
            };

            // Act
            var result = model.OnPostValidateSelection(input);

            // Assert
            AssertErrorJson(result, string.Format(Const.ErrorSelectRequired, "部署"));
        }

        /// <summary>
        /// ⑧入力チェック合格: 複数選択モードで 選択→確定の場合
        /// JsonResult（errorsなし）が返却されること
        /// </summary>
        [TestMethod]
        public void OnPostValidateSelection_複数選択モードで１つ以上選択し確定ボタン押下_合格()
        {
            // Arrange
            var model = CreateModel();

            model.MultiFlag = true;
            var input = new IndexModel.ValidateSelectionRequest
            {
                SelectedIds = [1, 2] // 選択あり
            };

            // Act
            var result = model.OnPostValidateSelection(input);

            // Assert
            var json = AssertJson(result);

            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                JsonSerializer.Serialize(json.Value))!;

            // Status の確認
            var status = (ResponseStatus)dict["Status"].GetInt32();
            Assert.AreEqual(ResponseStatus.正常, status, "StatusがResponseStatus.正常であるべきです。");
        }
    }
}

