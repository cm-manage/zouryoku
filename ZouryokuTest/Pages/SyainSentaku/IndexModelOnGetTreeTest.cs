namespace ZouryokuTest.Pages.SyainSentaku
{
    /// <summary>
    /// 部署検索のテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetTreeTest : IndexModelTestsBase
    {
        /// <summary>
        /// 正常系: アクティブフラグ＝FALSEの場合、レコードが部署一覧に取得されないこと
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_アクティブフラグがFALSE_部署一覧に取得されない()
        {
            // Arrange
            var busyo = AddBusyo(1, "部署A" , 2 , false);
            var model = CreateModel();
            SeedEntities(busyo);

            // Act
            var result = await model.OnGetTreeAsync();

            // Assert
            var json = AssertJson(result);

            var nodes = DeserializeNodes(json);

            Assert.HasCount(0, nodes);
        }

        /// <summary>
        /// 正常系: システム日付＜部署マスタ.有効開始日の場合、レコードが部署一覧に取得されないこと
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_有効開始日がシステム日付より後_部署一覧に取得されない()
        {
            // Arrange
            var busyo = AddBusyo(1, "部署A", 2, true, null , start: 1);
            var model = CreateModel();
            SeedEntities(busyo);

            // Act
            var result = await model.OnGetTreeAsync();

            // Assert
            var json = AssertJson(result);

            var nodes = DeserializeNodes(json);

            Assert.HasCount(0, nodes);
        }

        /// <summary>
        /// 正常系: 部署マスタ.有効終了日＜システム日付の場合、レコードが部署一覧に取得されないこと
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_有効終了日がシステム日付より前_部署一覧に取得されない()
        {
            // Arrange
            var busyo =AddBusyo(1, "部署A", 2, true, null, start: null, end: -1);
            var model = CreateModel();
            SeedEntities(busyo);

            // Act
            var result = await model.OnGetTreeAsync();

            // Assert
            var json = AssertJson(result);

            var nodes = DeserializeNodes(json);

            Assert.HasCount(0, nodes);
        }

        /// <summary>
        /// 正常系: アクティブフラグ＝TRUE、部署マスタ.有効開始日＜＝システム日付、システム日付＜＝部署マスタ.有効終了日の場合、部署一覧に取得されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_アクティブフラグがTRUEかつ有効開始日がシステム日付以前かつ有効終了日がシステム日付以後_部署一覧に取得される()
        {
            // Arrange
            // 境界値：部署マスタ.有効開始日＝システム日付、システム日付＝部署マスタ.有効終了日
            var busyo = AddBusyo(1, "部署A", 2, true, null, start: 0, end: 0);
            var model = CreateModel();
            SeedEntities(busyo);

            // Act
            var result = await model.OnGetTreeAsync();

            // Assert
            var json = AssertJson(result);
            var nodes = DeserializeNodes(json);
            var node = nodes[0];

            Assert.HasCount(1, nodes);
            Assert.AreEqual(1, node.Id);
            Assert.AreEqual("部署A", node.Name);
            Assert.AreEqual(2, node.Jyunjyo);
            Assert.IsNull(node.OyaId);
            Assert.IsEmpty(node.Children);
        }

        /// <summary>
        /// 正常系: アクティブフラグ＝TRUEだが親項目のアクティブフラグ＝FALSEの場合、レコードが部署一覧に取得されないこと
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_アクティブフラグがTRUEかつ親項目のアクティブフラグがFALSE_部署一覧に取得されない()
        {
            // Arrange
            // 子
            var busyo1 = AddBusyo(2, "子部署", 2, true, 1);
            // 親
            var busyo2 = AddBusyo(1, "親部署", 1, false);
            var model = CreateModel();
            SeedEntities(busyo1, busyo2);

            // Act
            var result = await model.OnGetTreeAsync();

            // Assert
            var json = AssertJson(result);

            var nodes = DeserializeNodes(json);

            Assert.HasCount(0, nodes);
        }

        /// <summary>
        /// 正常系: アクティブフラグ＝TRUEかつ親項目のアクティブフラグ＝TRUEの場合、レコードが部署一覧に取得されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_アクティブフラグがTRUEかつ親項目のアクティブフラグがTRUE_部署一覧に取得される()
        {
            // Arrange
            // 子
            var busyo1 = AddBusyo(2, "子部署", 2, true, 1);
            // 親
            var busyo2 = AddBusyo(1, "親部署", 1, true);
            var model = CreateModel();
            SeedEntities(busyo1, busyo2);

            // Act
            var result = await model.OnGetTreeAsync();

            // Assert
            var json = AssertJson(result);

            var nodes = DeserializeNodes(json);

            Assert.HasCount(1, nodes);
            Assert.HasCount(1, nodes[0].Children);

            var node = nodes[0];
            var childNode = node.Children[0];

            Assert.AreEqual(1, node.Id);
            Assert.AreEqual("親部署", node.Name);
            Assert.AreEqual(1, node.Jyunjyo);
            Assert.IsNull(node.OyaId);

            Assert.AreEqual(2, childNode.Id);
            Assert.AreEqual("子部署", childNode.Name);
            Assert.AreEqual(2, childNode.Jyunjyo);
            Assert.AreEqual(1, childNode.OyaId);
            Assert.IsEmpty(childNode.Children);
        }

        /// <summary>
        /// 正常系: 部署の階層が同値の場合、順序の昇順で部署が取得される
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_部署の階層が同値_順序の昇順で部署が取得される()
        {
            // Arrange
            var busyo1 = AddBusyo(1, "部署1", 2, true);
            var busyo2 = AddBusyo(2, "部署2", 1, true);
            var model = CreateModel();
            SeedEntities(busyo1, busyo2);

            // Act
            var result = await model.OnGetTreeAsync();

            // Assert
            var json = AssertJson(result);

            var nodes = DeserializeNodes(json);

            Assert.HasCount(2, nodes);

            var node1 = nodes[0];
            var node2 = nodes[1];

            Assert.AreEqual(2, node1.Id);
            Assert.AreEqual("部署2", node1.Name);
            Assert.AreEqual(1, node1.Jyunjyo);
            Assert.IsNull(node1.OyaId);

            Assert.AreEqual(1, node2.Id);
            Assert.AreEqual("部署1", node2.Name);
            Assert.AreEqual(2, node2.Jyunjyo);
            Assert.IsNull(node2.OyaId);
        }
    }
}
