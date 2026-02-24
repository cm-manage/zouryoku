using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zouryoku.Pages.Maintenance.PhotoList;
using ZouryokuTest;
using Model.Model;
using System.Reflection;

namespace ZouryokuTest.Pages.Maintenance.PhotoList
{
    /// <summary>
    /// IndexModel (顔写真一覧ページ) のユニットテスト
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

        /// <summary>
        /// GetBusyoHierarchyメソッドを呼び出すためのヘルパーメソッド
        /// プライベートメソッドをリフレクションで呼び出す
        /// </summary>
        private List<Busyo> InvokeGetBusyoHierarchy(IndexModel model, long parentBusyoId, Dictionary<long, List<Busyo>> busyosByParent, int depth = 0)
        {
            var method = typeof(IndexModel).GetMethod("GetBusyoHierarchy", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "GetBusyoHierarchy メソッドが見つかりません");
            
            var result = method.Invoke(model, new object[] { parentBusyoId, busyosByParent, depth });
            return (List<Busyo>)result!;
        }

        #region GetBusyoHierarchy Tests

        /// <summary>
        /// 正常: 単一階層の部署が正しく取得されること
        /// </summary>
        [TestMethod]
        public void GetBusyoHierarchy_Returns_Single_Level_Hierarchy()
        {
            // Arrange
            var model = CreateModel();
            var busyo1 = new Busyo { Id = 1, Name = "部署1", OyaId = 0 };
            var busyo2 = new Busyo { Id = 2, Name = "部署2", OyaId = 0 };
            
            var busyosByParent = new Dictionary<long, List<Busyo>>
            {
                { 0, new List<Busyo> { busyo1, busyo2 } }
            };

            // Act
            var result = InvokeGetBusyoHierarchy(model, 0, busyosByParent);

            // Assert
            Assert.HasCount(2, result, "部署の数が一致しません");
            Assert.AreEqual("部署1", result[0].Name);
            Assert.AreEqual("部署2", result[1].Name);
        }

        /// <summary>
        /// 正常: 2階層の部署階層が正しく取得されること
        /// </summary>
        [TestMethod]
        public void GetBusyoHierarchy_Returns_Two_Level_Hierarchy()
        {
            // Arrange
            var model = CreateModel();
            var busyo1 = new Busyo { Id = 1, Name = "親部署", OyaId = 0 };
            var busyo2 = new Busyo { Id = 2, Name = "子部署1", OyaId = 1 };
            var busyo3 = new Busyo { Id = 3, Name = "子部署2", OyaId = 1 };
            
            var busyosByParent = new Dictionary<long, List<Busyo>>
            {
                { 0, new List<Busyo> { busyo1 } },
                { 1, new List<Busyo> { busyo2, busyo3 } }
            };

            // Act
            var result = InvokeGetBusyoHierarchy(model, 0, busyosByParent);

            // Assert
            Assert.HasCount(3, result, "部署の数が一致しません");
            Assert.AreEqual("親部署", result[0].Name, "親部署が最初に来るべきです");
            Assert.AreEqual("子部署1", result[1].Name, "子部署1が2番目に来るべきです");
            Assert.AreEqual("子部署2", result[2].Name, "子部署2が3番目に来るべきです");
        }

        /// <summary>
        /// 正常: 3階層の部署階層が正しく取得されること
        /// </summary>
        [TestMethod]
        public void GetBusyoHierarchy_Returns_Three_Level_Hierarchy()
        {
            // Arrange
            var model = CreateModel();
            var busyo1 = new Busyo { Id = 1, Name = "本部", OyaId = 0 };
            var busyo2 = new Busyo { Id = 2, Name = "部", OyaId = 1 };
            var busyo3 = new Busyo { Id = 3, Name = "課", OyaId = 2 };
            
            var busyosByParent = new Dictionary<long, List<Busyo>>
            {
                { 0, new List<Busyo> { busyo1 } },
                { 1, new List<Busyo> { busyo2 } },
                { 2, new List<Busyo> { busyo3 } }
            };

            // Act
            var result = InvokeGetBusyoHierarchy(model, 0, busyosByParent);

            // Assert
            Assert.HasCount(3, result, "3階層の部署が全て取得されるべきです");
            Assert.AreEqual("本部", result[0].Name);
            Assert.AreEqual("部", result[1].Name);
            Assert.AreEqual("課", result[2].Name);
        }

        /// <summary>
        /// 正常: 複雑な階層構造が正しく取得されること（枝分かれあり）
        /// </summary>
        [TestMethod]
        public void GetBusyoHierarchy_Returns_Complex_Hierarchy()
        {
            // Arrange
            var model = CreateModel();
            var busyo1 = new Busyo { Id = 1, Name = "本部", OyaId = 0 };
            var busyo2 = new Busyo { Id = 2, Name = "営業部", OyaId = 1 };
            var busyo3 = new Busyo { Id = 3, Name = "技術部", OyaId = 1 };
            var busyo4 = new Busyo { Id = 4, Name = "営業1課", OyaId = 2 };
            var busyo5 = new Busyo { Id = 5, Name = "営業2課", OyaId = 2 };
            var busyo6 = new Busyo { Id = 6, Name = "開発課", OyaId = 3 };
            
            var busyosByParent = new Dictionary<long, List<Busyo>>
            {
                { 0, new List<Busyo> { busyo1 } },
                { 1, new List<Busyo> { busyo2, busyo3 } },
                { 2, new List<Busyo> { busyo4, busyo5 } },
                { 3, new List<Busyo> { busyo6 } }
            };

            // Act
            var result = InvokeGetBusyoHierarchy(model, 0, busyosByParent);

            // Assert
            Assert.HasCount(6, result, "全ての部署が取得されるべきです");
            Assert.AreEqual("本部", result[0].Name);
            Assert.AreEqual("営業部", result[1].Name);
            Assert.AreEqual("営業1課", result[2].Name);
            Assert.AreEqual("営業2課", result[3].Name);
            Assert.AreEqual("技術部", result[4].Name);
            Assert.AreEqual("開発課", result[5].Name);
        }

        /// <summary>
        /// エッジケース: 空の辞書が渡された場合、空のリストが返されること
        /// </summary>
        [TestMethod]
        public void GetBusyoHierarchy_Returns_Empty_List_When_Dictionary_Is_Empty()
        {
            // Arrange
            var model = CreateModel();
            var busyosByParent = new Dictionary<long, List<Busyo>>();

            // Act
            var result = InvokeGetBusyoHierarchy(model, 0, busyosByParent);

            // Assert
            Assert.IsNotNull(result, "結果はnullではなく空のリストであるべきです");
            Assert.HasCount(0, result, "空の辞書の場合、空のリストが返されるべきです");
        }

        /// <summary>
        /// エッジケース: 存在しない親IDを指定した場合、空のリストが返されること
        /// </summary>
        [TestMethod]
        public void GetBusyoHierarchy_Returns_Empty_List_When_Parent_Id_Not_Exists()
        {
            // Arrange
            var model = CreateModel();
            var busyo1 = new Busyo { Id = 1, Name = "部署1", OyaId = 0 };
            
            var busyosByParent = new Dictionary<long, List<Busyo>>
            {
                { 0, new List<Busyo> { busyo1 } }
            };

            // Act
            var result = InvokeGetBusyoHierarchy(model, 999, busyosByParent);

            // Assert
            Assert.IsNotNull(result, "結果はnullではなく空のリストであるべきです");
            Assert.HasCount(0, result, "存在しない親IDの場合、空のリストが返されるべきです");
        }

        /// <summary>
        /// エッジケース: 親部署のリストが空の場合、空のリストが返されること
        /// </summary>
        [TestMethod]
        public void GetBusyoHierarchy_Returns_Empty_List_When_Parent_Has_No_Children()
        {
            // Arrange
            var model = CreateModel();
            var busyosByParent = new Dictionary<long, List<Busyo>>
            {
                { 0, new List<Busyo>() }
            };

            // Act
            var result = InvokeGetBusyoHierarchy(model, 0, busyosByParent);

            // Assert
            Assert.IsNotNull(result, "結果はnullではなく空のリストであるべきです");
            Assert.HasCount(0, result, "子部署がない場合、空のリストが返されるべきです");
        }

        /// <summary>
        /// 深さ制限: maxDepth (10) を超えた場合、それ以上深い階層は取得されないこと
        /// </summary>
        [TestMethod]
        public void GetBusyoHierarchy_Stops_At_MaxDepth()
        {
            // Arrange
            var model = CreateModel();
            const int maxDepth = 10;
            const int testDepthLevels = 12; // maxDepthを超える階層を作成
            var busyosByParent = new Dictionary<long, List<Busyo>>();
            
            // testDepthLevels階層の部署を作成
            for (long i = 0; i <= testDepthLevels; i++)
            {
                var busyo = new Busyo { Id = i + 1, Name = $"部署{i + 1}", OyaId = i };
                busyosByParent[i] = new List<Busyo> { busyo };
            }

            // Act
            var result = InvokeGetBusyoHierarchy(model, 0, busyosByParent);

            // Assert
            // maxDepth=10なので、深さ0から10までが取得される
            var expectedCount = maxDepth + 1; // 深さ0から10まで = 11個
            Assert.HasCount(expectedCount, result, $"maxDepth={maxDepth}の制限により{expectedCount}個（深さ0-{maxDepth}）の部署が取得されるべきです");
            Assert.AreEqual("部署1", result[0].Name);
            Assert.AreEqual($"部署{expectedCount}", result[expectedCount - 1].Name);
        }

        /// <summary>
        /// 深さ制限: 初期depth値が指定された場合でも正しく動作すること
        /// </summary>
        [TestMethod]
        public void GetBusyoHierarchy_Respects_Initial_Depth_Parameter()
        {
            // Arrange
            var model = CreateModel();
            var busyosByParent = new Dictionary<long, List<Busyo>>();
            
            // 5階層の部署を作成
            for (long i = 0; i <= 5; i++)
            {
                var busyo = new Busyo { Id = i + 1, Name = $"部署{i + 1}", OyaId = i };
                busyosByParent[i] = new List<Busyo> { busyo };
            }

            // Act - depth=9から開始（maxDepth=10なので2階層のみ取得できる）
            var result = InvokeGetBusyoHierarchy(model, 0, busyosByParent, 9);

            // Assert
            Assert.HasCount(2, result, "depth=9から開始なので、深さ9と10の2個のみ取得されるべきです");
            Assert.AreEqual("部署1", result[0].Name);
            Assert.AreEqual("部署2", result[1].Name);
        }

        /// <summary>
        /// 深さ制限: depth > maxDepth の場合、空のリストが返されること
        /// </summary>
        [TestMethod]
        public void GetBusyoHierarchy_Returns_Empty_When_Depth_Exceeds_MaxDepth()
        {
            // Arrange
            var model = CreateModel();
            var busyo1 = new Busyo { Id = 1, Name = "部署1", OyaId = 0 };
            
            var busyosByParent = new Dictionary<long, List<Busyo>>
            {
                { 0, new List<Busyo> { busyo1 } }
            };

            // Act - depth=11（maxDepth=10を超える）
            var result = InvokeGetBusyoHierarchy(model, 0, busyosByParent, 11);

            // Assert
            Assert.IsNotNull(result, "結果はnullではなく空のリストであるべきです");
            Assert.HasCount(0, result, "depth > maxDepthの場合、空のリストが返されるべきです");
        }

        /// <summary>
        /// 循環参照: 辞書構造により循環参照は発生しないが、同じ親IDが複数回参照されても正しく処理されること
        /// </summary>
        [TestMethod]
        public void GetBusyoHierarchy_Handles_Multiple_Children_Correctly()
        {
            // Arrange
            var model = CreateModel();
            var busyo1 = new Busyo { Id = 1, Name = "親部署", OyaId = 0 };
            var busyo2 = new Busyo { Id = 2, Name = "子部署1", OyaId = 1 };
            var busyo3 = new Busyo { Id = 3, Name = "子部署2", OyaId = 1 };
            var busyo4 = new Busyo { Id = 4, Name = "子部署3", OyaId = 1 };
            
            var busyosByParent = new Dictionary<long, List<Busyo>>
            {
                { 0, new List<Busyo> { busyo1 } },
                { 1, new List<Busyo> { busyo2, busyo3, busyo4 } }
            };

            // Act
            var result = InvokeGetBusyoHierarchy(model, 0, busyosByParent);

            // Assert
            Assert.HasCount(4, result, "親1つと子3つの合計4つが取得されるべきです");
            Assert.AreEqual("親部署", result[0].Name);
            Assert.AreEqual("子部署1", result[1].Name);
            Assert.AreEqual("子部署2", result[2].Name);
            Assert.AreEqual("子部署3", result[3].Name);
        }

        /// <summary>
        /// エッジケース: 葉ノード（子を持たない部署）からの取得で空のリストが返されること
        /// </summary>
        [TestMethod]
        public void GetBusyoHierarchy_Returns_Empty_For_Leaf_Node()
        {
            // Arrange
            var model = CreateModel();
            var busyo1 = new Busyo { Id = 1, Name = "親部署", OyaId = 0 };
            var busyo2 = new Busyo { Id = 2, Name = "子部署（葉）", OyaId = 1 };
            
            var busyosByParent = new Dictionary<long, List<Busyo>>
            {
                { 0, new List<Busyo> { busyo1 } },
                { 1, new List<Busyo> { busyo2 } }
                // busyo2 (Id=2) には子がない
            };

            // Act - 葉ノードから取得
            var result = InvokeGetBusyoHierarchy(model, 2, busyosByParent);

            // Assert
            Assert.IsNotNull(result, "結果はnullではなく空のリストであるべきです");
            Assert.HasCount(0, result, "葉ノードには子がないので空のリストが返されるべきです");
        }

        #endregion
    }
}
