using Model.Model;
using Zouryoku.Pages.BusyoMasterMaintenanceJyunjyoNarabikae;

namespace ZouryokuTest.Pages.BusyoMasterMaintenanceJyunjyoNarabikae
{
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {
        /// <summary>
        /// テストデータのNOT NULL制約を満たす以外に意味を持たない文字列です。
        /// </summary>
        /// <remarks>
        /// この定数は、事業部や社員エンティティのテストデータ作成時に、
        /// テストの本質的な検証に関係しない必須項目を埋める目的でのみ使用します。
        /// テストがこの値の内容に依存しないよう、業務的な意味を持つ値に変更してはなりません。
        /// また、関連するエンティティのカラムに最大長さ制約が設定されていることを想定し、
        /// その制約に抵触しないよう「N/A」という短めの文字列を設定しています。
        /// </remarks>
        private const string NotNullConstraintPlaceholder = "N/A";

        private IndexModel CreateModel() => new(db, GetLogger<IndexModel>(), options)
        {
            PageContext = GetPageContext(),
            TempData = GetTempData()
        };

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // OnGetAsync: 正常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// 初期表示 目的：有効部署のみ取得 前提：部署A(IsActive=true)、部署B(IsActive=false)
        /// </summary>
        [TestMethod(DisplayName = "初期表示 目的：有効部署のみ取得 前提：部署A(IsActive=true)、部署B(IsActive=false)")]
        public async Task OnGetAsync_有効部署のみ取得()
        {
            // Arrange
            AddNewBusyo("部署A", isActive: true);
            AddNewBusyo("部署B", isActive: false);
            await db.SaveChangesAsync();
            var model = CreateModel();

            await model.OnGetAsync(); // Act

            // Assert
            var expectedBusyoOrders = new[]
            {
                new IndexModel.BusyoOrder { Id = 1, Name = "部署A" }
            };
            AssertAreEqual(expectedBusyoOrders, model.RootBusyoOrders);
        }

        /// <summary>
        /// 初期表示 目的：部署がJyunjyoの昇順で並ぶ 前提：部署A(Jyunjyo=2,IsActive=true)、部署B(Jyunjyo=1,IsActive=true)
        /// </summary>
        [TestMethod(DisplayName = """
            初期表示 目的：部署がJyunjyoの昇順で並ぶ 前提：部署A(Jyunjyo=2,IsActive=true)、部署B(Jyunjyo=1,IsActive=true)
            """)]
        public async Task OnGetAsync_部署がJyunjyoの昇順で並ぶ()
        {
            // Arrange
            AddNewBusyo("部署A", 2);
            AddNewBusyo("部署B", 1);
            await db.SaveChangesAsync();
            var model = CreateModel();

            await model.OnGetAsync(); // Act

            // Assert
            var expectedBusyoOrders = new[]
            {
                new IndexModel.BusyoOrder { Id = 2, Name = "部署B" },
                new IndexModel.BusyoOrder { Id = 1, Name = "部署A" }
            };
            AssertAreEqual(expectedBusyoOrders, model.RootBusyoOrders);
        }

        /// <summary>
        /// 初期表示
        /// 目的：親部署が正しく判定されること 前提：部署A(Id=1,IsActive=true)、部署B(OyaId=1,IsActive=true)
        /// </summary>
        [TestMethod(DisplayName = """
            初期表示
            目的：親部署が正しく判定されること 前提：部署A(Id=1,IsActive=true)、部署B(OyaId=1,IsActive=true)
            """)]
        public async Task OnGetAsync_親部署が正しく判定されること()
        {
            // Arrange
            var busyoA = AddNewBusyo("部署A");
            AddNewBusyo("部署B", oya: busyoA);
            await db.SaveChangesAsync();
            var model = CreateModel();

            await model.OnGetAsync(); // Act

            // Assert
            var expectedBusyoOrders = new[]
            {
                new IndexModel.BusyoOrder { Id = 1, Name = "部署A", Children = [new IndexModel.BusyoOrder { Id = 2, Name ="部署B" }] }
            };
            AssertAreEqual(expectedBusyoOrders, model.RootBusyoOrders);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // OnPostRegisterAsync: 異常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// 並び順保存処理 目的：更新対象の部署が存在しない場合　前提：部署A(Id=1, IsActive=true)のみ存在
        /// </summary>
        [TestMethod(DisplayName = "並び順保存処理 目的：更新対象の部署が存在しない場合　前提：部署A(Id=1, IsActive=true)のみ存在")]
        public async Task OnPostRegisterAsync_更新対象の部署が存在しない場合()
        {
            // Arrange
            AddNewBusyo("部署A");
            await db.SaveChangesAsync();
            var model = CreateModel();
            var request = new[]
            {
                new IndexModel.BusyoOrder { Id = 2, Jyunjyo = 1 } // 処理時にId=2,Jyunjyo=1を渡す
            };

            var result = await model.OnPostRegisterAsync(request); // Act
            AssertError(result, IndexModel.ErrorConflictBusyo); // Assert
        }

        /// <summary>
        /// 並び順保存処理 目的：同時実行制御が発動した場合 前提：部署A(Id=1,IsActive=true,Jyunjyo=1)
        /// </summary>
        [TestMethod(DisplayName = "並び順保存処理 目的：同時実行制御が発動した場合 前提：部署A(Id=1,IsActive=true,Jyunjyo=1)")]
        public async Task OnPostRegisterAsync_同時実行制御が発動した場合()
        {
            // Arrange
            var busyoA = AddNewBusyo("部署A", 1);
            await db.SaveChangesAsync();
            var model = CreateModel();
            var request = new[]
            {
                // 処理時にId=1,Jyunjyo=2と同時実行制御が発動するVersionを渡す
                new IndexModel.BusyoOrder { Id = 1, Jyunjyo = 2, Version = busyoA.Version + 1 }
            };

            var result = await model.OnPostRegisterAsync(request); // Act
            AssertError(result, IndexModel.ErrorConflictBusyo); // Assert
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // OnPostRegisterAsync: 正常系テストメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// 並び順保存処理 目的：部署のJyunjyoの更新 前提：部署A(Id=1,IsActive=true,Jyunjyo=1)
        /// </summary>
        [TestMethod(DisplayName = "並び順保存処理 目的：部署のJyunjyoの更新 前提：部署A(Id=1,IsActive=true,Jyunjyo=1)")]
        public async Task OnPostRegisterAsync_部署のJyunjyoの更新()
        {
            // Arrange
            var busyoA = AddNewBusyo("部署A", 1);
            await db.SaveChangesAsync();
            var model = CreateModel();
            var request = new[]
            {
                new IndexModel.BusyoOrder { Id = 1, Jyunjyo = 2 } // 処理時にId=1,Jyunjyo=2を渡す
            };

            var result = await model.OnPostRegisterAsync(request); // Act

            // Assert
            AssertSuccess(result);
            AssertBusyoJyunjyoEqual(2, busyoA);
        }

        /// <summary>
        /// 並び順保存処理
        /// 目的：子部署も再帰的に更新される　前提：部署A(Id=1, IsActive=true, Jyunjyo=1)、
        /// 部署B(Id= 2, IsActive= true, Jyunjyo= 2, OyaId= 1)、部署C(Id= 3, IsActive= true, Jyunjyo= 3, OyaId= 2)
        /// </summary>
        [TestMethod(DisplayName = """
            並び順保存処理
            目的：子部署も再帰的に更新される　前提：部署A(Id=1, IsActive=true, Jyunjyo=1)、
            部署B(Id= 2, IsActive= true, Jyunjyo= 2, OyaId= 1)、部署C(Id= 3, IsActive= true, Jyunjyo= 3, OyaId= 2)
            """)]
        public async Task OnPostRegisterAsync_子部署も再帰的に更新される()
        {
            // Arrange
            var busyoA = AddNewBusyo("部署A", 1);
            var busyoB = AddNewBusyo("部署B", 2, oya: busyoA);
            var busyoC = AddNewBusyo("部署C", 3, oya: busyoB);
            await db.SaveChangesAsync();
            var model = CreateModel();
            var request = new[]
            {
                // 処理時に(Id=1,Jyunjyo=4),(Id=2,Jyunjyo=5),(Id=3,Jyunjyo=6)を渡す
                new IndexModel.BusyoOrder { Id = 1, Jyunjyo = 4 },
                new IndexModel.BusyoOrder { Id = 2, Jyunjyo = 5 },
                new IndexModel.BusyoOrder { Id = 3, Jyunjyo = 6 },
            };

            var result = await model.OnPostRegisterAsync(request); // Act

            // Assert
            AssertSuccess(result);
            AssertBusyoJyunjyoEqual(4, busyoA);
            AssertBusyoJyunjyoEqual(5, busyoB);
            AssertBusyoJyunjyoEqual(6, busyoC);
        }

        /// <summary>
        /// 並び順保存処理 目的：変更箇所がない場合 前提：部署A(Id=1, Jyunjyo=1, IsActive=true)
        /// </summary>
        [TestMethod(DisplayName = "並び順保存処理 目的：変更箇所がない場合 前提：部署A(Id=1, Jyunjyo=1, IsActive=true)")]
        public async Task OnPostRegisterAsync_変更箇所がない場合()
        {
            // Arrange
            var busyoA = AddNewBusyo("部署A", 1);
            await db.SaveChangesAsync();
            var model = CreateModel();
            var request = new[]
            {
                new IndexModel.BusyoOrder { Id = 1, Jyunjyo = null } // 処理時にId=1,Jyunjyo=nullを渡す
            };

            var result = await model.OnPostRegisterAsync(request); // Act

            // Assert
            AssertSuccess(result);
            AssertBusyoJyunjyoEqual(1, busyoA);
        }

        /// <summary>
        /// 並び順保存処理 目的：リクエストが空の場合 前提：部署A(Id=1, Jyunjyo=1, IsActive=true)
        /// </summary>
        [TestMethod(DisplayName = "並び順保存処理 目的：リクエストが空の場合 前提：部署A(Id=1, Jyunjyo=1, IsActive=true)")]
        public async Task OnPostRegisterAsync_リクエストが空の場合()
        {
            // Arrange
            var busyoA = AddNewBusyo("部署A", 1);
            await db.SaveChangesAsync();
            var model = CreateModel();
            var request = Array.Empty<IndexModel.BusyoOrder>(); // 処理時に空を渡す

            var result = await model.OnPostRegisterAsync(request); // Act

            // Assert
            AssertSuccess(result);
            AssertBusyoJyunjyoEqual(1, busyoA);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // データシード用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// テスト用の部署基礎情報（<see cref="BusyoBase"/>）と部署（<see cref="Busyo"/>）を作成し、
        /// インメモリ DB コンテキストに追加したうえで作成した部署エンティティを返します。
        /// </summary>
        /// <param name="name">作成する部署の名称。</param>
        /// <param name="jyunjyo">作成する部署の表示順序（未指定時は 0）。</param>
        /// <param name="isActive">作成する部署を有効とするかどうかを示すフラグ。</param>
        /// <param name="oya">親部署として関連付ける部署。親部署がない場合は <c>null</c>。</param>
        /// <returns>作成され、コンテキストに追加された <see cref="Busyo"/> エンティティ。</returns>
        private Busyo AddNewBusyo(string name, short jyunjyo = 0, bool isActive = true, Busyo? oya = null)
        {
            var busyoBase = new BusyoBasis
            {
                Id = default,
                Name = NotNullConstraintPlaceholder,
                BumoncyoId = default
            };
            db.BusyoBases.Add(busyoBase);

            var busyo = new Busyo
            {
                Id = default,
                Code = NotNullConstraintPlaceholder,
                Name = name,
                KanaName = NotNullConstraintPlaceholder,
                OyaCode = NotNullConstraintPlaceholder,
                StartYmd = default,
                EndYmd = default,
                Jyunjyo = jyunjyo,
                KasyoCode = NotNullConstraintPlaceholder,
                KaikeiCode = NotNullConstraintPlaceholder,
                KeiriCode = default,
                IsActive = isActive,
                Ryakusyou = default,
                BusyoBaseId = default,
                OyaId = default,
                ShoninBusyoId = default,
                BusyoBase = busyoBase,
                Oya = oya
            };
            db.Busyos.Add(busyo);
            return busyo;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Assert用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void AssertAreEqual(
            IList<IndexModel.BusyoOrder> expectedBusyoOrders, IList<IndexModel.BusyoOrder>? actualBusyoOrders)
        {
            Assert.IsNotNull(actualBusyoOrders, "BusyoOrder 一覧が設定されていません。");
            Assert.HasCount(expectedBusyoOrders.Count, actualBusyoOrders, "BusyoOrder 一覧の件数が一致しません。");

            for (int i = 0; i < expectedBusyoOrders.Count; i++)
            {
                var expected = expectedBusyoOrders[i];
                var actual = actualBusyoOrders[i];
                Assert.AreEqual(expected.Id, actual.Id, $"{i}件目の Id が一致しません。");
                Assert.AreEqual(expected.Name, actual.Name, $"{i}件目の Name が一致しません。");
                Assert.AreEqual(expected.Jyunjyo, actual.Jyunjyo, $"{i}件目の Jyunjyo が一致しません。");

                var expectedBusyoOrderChildren = expected.Children;
                if (expectedBusyoOrderChildren is not null)
                {
                    AssertAreEqual(expectedBusyoOrderChildren, actual.Children);
                }
            }
        }

        private static void AssertBusyoJyunjyoEqual(short expectedBusyoJyunjyo, Busyo actualBusyo)
        {
            Assert.AreEqual(expectedBusyoJyunjyo, actualBusyo.Jyunjyo, $"Jyunjyo が一致しません。");
        }
    }
}
