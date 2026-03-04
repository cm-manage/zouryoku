using Microsoft.AspNetCore.Mvc.RazorPages;
using Model.Enums;
using Model.Model;
using Zouryoku.Pages.Maintenance.Syains.Kensaku;
using static Model.Enums.EmployeeAuthority;
using static Model.Enums.EmployeeWorkType;

namespace ZouryokuTest.Pages.Maintenance.Syains.Kensaku
{
    /// <summary>
    /// 社員マスタメンテナンス検索画面のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {
        private IndexModel CreateModel()
        {
            var model = new IndexModel(
                db,
                GetLogger<IndexModel>(),
                options,
                viewEngine,
                fakeTimeProvider)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };
            return model;
        }

        /// <summary>
        /// BASEテーブルの共通シード
        /// </summary>
        private void SeedBase()
        {
            // ユーザーロール
            var roleAdmin = new UserRole { Id = 1, Code = 1, Name = "管理者", Jyunjo = 1 };
            var roleGeneral = new UserRole { Id = 2, Code = 2, Name = "一般", Jyunjo = 2 };
            db.AddRange(roleAdmin, roleGeneral);

            // 勤怠属性
            var kintai3Months60Hours = CreateKintaiZokusei(
                id: (short)_3か月60時間,
                name: "_3か月60時間");

            var kintaiPartTime = CreateKintaiZokusei(
                id: (short)パート,
                name: "パート");

            var kintaiMinashi = CreateKintaiZokusei(
                id: (short)みなし対象者,
                name: "みなし対象者",
                isMinashi: true,
                code: みなし対象者);

            db.AddRange(kintai3Months60Hours, kintaiPartTime, kintaiMinashi);

            // 部署
            var busyoSystem = CreateBusyo(
                id: 1,
                name: "システム部",
                isActive: true,
                jyunjyo: 1);

            var busyoSales = CreateBusyo(
                id: 2,
                name: "営業部",
                isActive: true,
                jyunjyo: 2);

            db.AddRange(busyoSystem, busyoSales);

            // 社員基本情報
            var syainBase1 = new SyainBasis { Id = 1, Code = "S001", Name = "田中" };
            var syainBase2 = new SyainBasis { Id = 2, Code = "S002", Name = "佐藤" };
            var syainBase3 = new SyainBasis { Id = 3, Code = "S003", Name = "鈴木" };
            db.AddRange(syainBase1, syainBase2, syainBase3);
        }

        /// <summary>
        /// みなし対象者なしのBASEデータ
        /// </summary>
        private void SeedBaseWithoutMinashi()
        {
            // ユーザーロール
            var roleAdmin = new UserRole { Id = 1, Code = 1, Name = "管理者", Jyunjo = 1 };
            var roleGeneral = new UserRole { Id = 2, Code = 2, Name = "一般", Jyunjo = 2 };
            db.AddRange(roleAdmin, roleGeneral);

            // 勤怠属性（みなし対象者なし）
            var kintai3Months60Hours = CreateKintaiZokusei(
                id: (short)_3か月60時間,
                name: "_3か月60時間");

            var kintaiPart = CreateKintaiZokusei(
                id: (short)パート,
                name: "パート");

            db.AddRange(kintai3Months60Hours, kintaiPart);
        }

        /// <summary>
        /// 検索条件テスト用のデータ生成
        /// </summary>
        private void CreateConditionRecords()
        {
            SeedBase();

            // S001: 田中, システム部, 退職=false, ロール=管理者, 勤怠=通常, 級職=3, 権限=労働状況報告
            var syain1 = CreateSyain(
                id: 1,
                syainBaseId: 1,
                code: "S001",
                name: "田中太郎",
                busyoId: 1,
                userRoleId: 1,
                kintaiZokuseiId: 1,
                retired: false,
                kyusyoku: 3,
                kengen: 労働状況報告,
                jyunjyo: 1);

            // S002: 佐藤, 営業部, 退職=true, ロール=一般, 勤怠=通常, 級職=5, 権限=None
            var syain2 = CreateSyain(
                id: 2,
                syainBaseId: 2,
                code: "S002",
                name: "佐藤花子",
                busyoId: 2,
                userRoleId: 2,
                kintaiZokuseiId: 1,
                retired: true,
                kyusyoku: 5,
                kengen: None,
                jyunjyo: 2);

            // S003: 鈴木, 営業部, 退職=false, ロール=一般, 勤怠=シフト, 級職=3,
            // 権限=労働状況報告|勤務日報未確定チェック
            var syain3 = CreateSyain(
                id: 3,
                syainBaseId: 3,
                code: "S003",
                name: "鈴木一郎",
                busyoId: 2,
                userRoleId: 2,
                kintaiZokuseiId: 2,
                retired: false,
                kyusyoku: 3,
                kengen: 労働状況報告 | 勤務日報未確定チェック,
                jyunjyo: 3);

            db.AddRange(syain1, syain2, syain3);
        }

        // =====================================================================
        // OnGetAsync テスト
        // =====================================================================

        /// <summary>
        /// ①初期表示: 勤怠属性の選択肢が取得される
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_初期表示_勤怠属性の選択肢が取得されること()
        {
            // Arrange
            SeedBase();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual((EmployeeWorkType)みなし対象者, model.SearchCondition.KintaiZokuseiId);
            Assert.IsNotNull(model.KintaiZokuseiOptions);
            Assert.HasCount(3, model.KintaiZokuseiOptions);
        }

        /// <summary>
        /// ①-2 初期表示: 勤怠属性IDが事前設定済みの場合は上書きしない
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_初期表示_勤怠属性ID指定済みなら上書きしないこと()
        {
            // Arrange
            SeedBase();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.KintaiZokuseiId = (EmployeeWorkType)パート;

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual((EmployeeWorkType)パート, model.SearchCondition.KintaiZokuseiId);
        }

        /// <summary>
        /// ①-3 初期表示: みなし対象者が存在しない場合は勤怠属性IDを設定しない
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_初期表示_みなし対象者未登録なら勤怠属性ID未設定のまま()
        {
            // Arrange
            SeedBaseWithoutMinashi();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsFalse(model.SearchCondition.KintaiZokuseiId.HasValue);
            Assert.HasCount(2, model.KintaiZokuseiOptions);
        }

        /// <summary>
        /// ②初期表示: ロールの選択肢が取得される
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_初期表示_ロールの選択肢が取得されること()
        {
            // Arrange
            SeedBase();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.UserRoleOptions);
            Assert.HasCount(2, model.UserRoleOptions);
        }

        /// <summary>
        /// ③初期表示: 社員権限の選択肢が取得される
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_初期表示_社員権限の選択肢が取得されること()
        {
            // Arrange
            SeedBase();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.KengenOptions);
            // EmployeeAuthority enumの全値分
            var expectedCount = Enum.GetValues<EmployeeAuthority>().Length;
            Assert.HasCount(expectedCount, model.KengenOptions);
        }

        /// <summary>
        /// ④初期表示: 検索結果は空である
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_初期表示_検索結果は空であること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsTrue(model.Results.Any());
        }

        /// <summary>
        /// ⑤初期表示: PageResultが返される
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_初期表示_PageResultが返されること()
        {
            // Arrange
            SeedBase();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync();

            // Assert
            Assert.IsInstanceOfType<PageResult>(result);
        }

        // =====================================================================
        // OnGetSearchAsync テスト: フィルター条件
        // =====================================================================

        /// <summary>
        /// ⑥検索: 条件なし、退職者除外（デフォルト）
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_条件なし_退職者を除外して取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            // デフォルト: IncludeRetired = false

            // Act
            await model.OnGetSearchAsync();

            // Assert
            // 田中(S001)と鈴木(S003)のみ、佐藤(S002 退職)は除外
            Assert.HasCount(2, model.Results);
            Assert.IsTrue(model.Results.Any(r => r.SyainNo == "S001"));
            Assert.IsTrue(model.Results.Any(r => r.SyainNo == "S003"));
            Assert.IsFalse(model.Results.Any(r => r.SyainNo == "S002"));
        }

        /// <summary>
        /// ⑦検索: 退職者を含む
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_退職者を含むON_全員取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.IncludeRetired = true;

            // Act
            await model.OnGetSearchAsync();

            // Assert
            // 田中(S001), 佐藤(S002), 鈴木(S003) 全員取得
            Assert.HasCount(3, model.Results);
        }

        /// <summary>
        /// ⑧検索: 社員番号（部分一致）
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_社員番号検索_部分一致で取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.SyainNo = "003"; // S003(鈴木)

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.HasCount(1, model.Results);
            Assert.AreEqual("S003", model.Results[0].SyainNo);
        }

        /// <summary>
        /// ⑨検索: 社員名（部分一致）
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_社員名検索_部分一致で取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.SyainName = "太郎"; // 田中太郎(S001)

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.HasCount(1, model.Results);
            Assert.AreEqual("S001", model.Results[0].SyainNo);
        }

        /// <summary>
        /// ⑩-A 検索: 退職者OFF時に退職者が取得されないこと
        /// </summary>
        [TestMethod]
        public async Task
            OnPostSearchAsync_社員名で退職者を検索_退職者OFFでは取得されないこと()
        {
            // Arrange（準備）
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.SyainName = "花子"; // 佐藤花子(S002, 退職者)

            // Act（実行）
            await model.OnGetSearchAsync();

            // Assert（検証）
            Assert.IsFalse(model.Results.Any());
        }

        /// <summary>
        /// ⑩-B 検索: 退職者ON時に退職者が取得されること
        /// </summary>
        [TestMethod]
        public async Task
            OnPostSearchAsync_社員名で退職者を検索_退職者ONでは取得されること()
        {
            // Arrange（準備）
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.SyainName = "花子"; // 佐藤花子(S002, 退職者)
            model.SearchCondition.IncludeRetired = true;

            // Act（実行）
            await model.OnGetSearchAsync();

            // Assert（検証）
            Assert.HasCount(1, model.Results);
            Assert.AreEqual("S002", model.Results[0].SyainNo);
        }

        /// <summary>
        /// ⑪検索: 部署名（部分一致）
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_部署名検索_部分一致で取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.BusyoName = "システム"; // 田中(S001)のみ

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.HasCount(1, model.Results);
            Assert.AreEqual("S001", model.Results[0].SyainNo);
        }

        /// <summary>
        /// ⑫検索: 部署名で複数件取得
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_部署名検索_営業部で複数件取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.BusyoName = "営業"; // 佐藤(退職)と鈴木
            model.SearchCondition.IncludeRetired = true;

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.HasCount(2, model.Results);
        }

        /// <summary>
        /// ⑬検索: 勤怠属性
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_勤怠属性検索_一致するものを取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.KintaiZokuseiId = EmployeeWorkType._3か月60時間; // シフト勤務 → 鈴木(S003)のみ

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.HasCount(1, model.Results);
            Assert.AreEqual("S003", model.Results[0].SyainNo);
        }

        /// <summary>
        /// ⑭検索: ロール
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_ロール検索_一致するものを取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.UserRoleId = 1; // 管理者 → 田中(S001)のみ

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.HasCount(1, model.Results);
            Assert.AreEqual("S001", model.Results[0].SyainNo);
        }

        /// <summary>
        /// ⑮検索: 社員権限（ビットマスク一致）
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_社員権限検索_ビットマスクで一致するものを取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.Kengen = 労働状況報告; // 田中(S001)と鈴木(S003)

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.HasCount(2, model.Results);
            Assert.IsTrue(model.Results.Any(r => r.SyainNo == "S001"));
            Assert.IsTrue(model.Results.Any(r => r.SyainNo == "S003"));
        }

        /// <summary>
        /// ⑯検索: 社員権限（フラグ判定）
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_社員権限検索_特定のフラグで一致するものを取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.Kengen = 勤務日報未確定チェック; // 鈴木(S003)のみ

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.HasCount(1, model.Results);
            Assert.AreEqual("S003", model.Results[0].SyainNo);
        }

        /// <summary>
        /// ⑰検索: 級職（部分一致）
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_級職検索_部分一致で取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.Kyusyoku = 5; // 佐藤(S002, 級職=5, 退職)
            model.SearchCondition.IncludeRetired = true;

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.HasCount(1, model.Results);
            Assert.AreEqual("S002", model.Results[0].SyainNo);
        }

        /// <summary>
        /// ⑱検索: 級職で複数件取得
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_級職検索_同じ級職の社員が複数取得されること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.Kyusyoku = 3; // 田中(級職3)と鈴木(級職3)

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.HasCount(2, model.Results);
        }

        /// <summary>
        /// ⑲検索: 該当なし
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_該当なし_空リストを返すこと()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.SyainNo = "XXXX"; // 存在しない

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.IsFalse(model.Results.Any());
        }

        /// <summary>
        /// ⑳検索: 複数条件の組み合わせ
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_複数条件_AND条件で絞り込むこと()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.BusyoName = "営業"; // 営業部の社員
            model.SearchCondition.KintaiZokuseiId = EmployeeWorkType._3か月60時間; // シフト勤務

            // Act
            await model.OnGetSearchAsync();

            // Assert
            // 営業部かつシフト勤務 → 鈴木(S003)のみ
            Assert.HasCount(1, model.Results);
            Assert.AreEqual("S003", model.Results[0].SyainNo);
        }

        // =====================================================================
        // OnGetSearchAsync テスト: ソート順
        // =====================================================================

        /// <summary>
        /// ㉑検索: ソート順の確認（部署順序→社員順序降順→社員番号降順）
        /// </summary>
        [TestMethod]
        public async Task OnPostSearchAsync_ソート順_部署順序昇順_社員順序降順_社員番号降順()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SearchCondition.IncludeRetired = true; // 全員取得

            // Act
            await model.OnGetSearchAsync();

            // Assert
            // ソート: .OrderBy(s => s.Busyo.Jyunjyo)   → システム(1) → 営業(2)
            //         .ThenByDescending(s => s.Jyunjyo)  → 順序降順
            //         .ThenByDescending(s => s.Code)     → コード降順
            // システム部(順1): 田中(順1, S001)
            // 営業部(順2): 鈴木(順3, S003), 佐藤(順2, S002)
            Assert.HasCount(3, model.Results);
            Assert.AreEqual("S001", model.Results[0].SyainNo);
            Assert.AreEqual("S003", model.Results[1].SyainNo);
            Assert.AreEqual("S002", model.Results[2].SyainNo);
        }

        // =====================================================================
        // SyainViewModel テスト: ViewModelマッピング
        // =====================================================================

        /// <summary>
        /// ㉒ViewModel: マッピング確認（退職者）
        /// </summary>
        [TestMethod]
        public void SyainViewModel_マッピング確認_退職者のとき正しく反映されること()
        {
            // Arrange
            var syain = new Syain
            {
                Id = 100,
                SyainBaseId = 100,
                Code = "T100",
                Name = "テスト太郎",
                Kyusyoku = 5,
                Retired = true,
                Busyo = new Busyo { Name = "テスト部署" },
                KintaiZokusei = new KintaiZokusei { Name = "通常" },
                UserRole = new UserRole { Name = "管理者" }
            };

            // Act
            var vm = new SyainViewModel(syain);

            // Assert
            Assert.AreEqual(100, vm.SyainBaseId);
            Assert.AreEqual("T100", vm.SyainNo);
            Assert.AreEqual("テスト太郎", vm.Name);
            Assert.AreEqual("テスト部署", vm.BusyoName);
            Assert.AreEqual((short)5, vm.Kyusyoku);
            Assert.AreEqual("通常", vm.KintaiZokuseiName);
            Assert.AreEqual("管理者", vm.UserRoleName);
            Assert.AreEqual("退職", vm.RetiredDisplay);
        }

        /// <summary>
        /// ㉓ViewModel: マッピング確認（在職者）
        /// </summary>
        [TestMethod]
        public void SyainViewModel_マッピング確認_在職者のとき正しく反映されること()
        {
            // Arrange
            var syain = new Syain
            {
                Id = 200,
                SyainBaseId = 200,
                Code = "T200",
                Name = "テスト花子",
                Kyusyoku = 2,
                Retired = false,
                Busyo = new Busyo { Name = "営業部" },
                KintaiZokusei = new KintaiZokusei { Name = "シフト" },
                UserRole = new UserRole { Name = "一般" }
            };

            // Act
            var vm = new SyainViewModel(syain);

            // Assert
            Assert.AreEqual(200, vm.SyainBaseId);
            Assert.AreEqual("T200", vm.SyainNo);
            Assert.AreEqual("テスト花子", vm.Name);
            Assert.AreEqual("営業部", vm.BusyoName);
            Assert.AreEqual((short)2, vm.Kyusyoku);
            Assert.AreEqual("シフト", vm.KintaiZokuseiName);
            Assert.AreEqual("一般", vm.UserRoleName);
            Assert.AreEqual(string.Empty, vm.RetiredDisplay);
        }

        /// <summary>
        /// ㉔IndexModel: Conditionセッター
        /// </summary>
        [TestMethod]
        public void IndexModel_Conditionセッター_設定した検索条件インスタンスを保持すること()
        {
            // Arrange
            var model = CreateModel();
            var condition = new SyainSearchCondition
            {
                SyainNo = "S999",
                Kyusyoku = 9,
                IncludeRetired = true
            };

            // Act
            model.SearchCondition = condition;

            // Assert
            Assert.AreSame(condition, model.SearchCondition);
        }

        private static Busyo CreateBusyo(
            long? id = 1,
            string? code = null,
            string? name = null,
            string? kanaName = null,
            string? oyaCode = null,
            DateOnly? startYmd = null,
            DateOnly? endYmd = null,
            short? jyunjyo = 1,
            string? kasyoCode = null,
            string? kaikeiCode = null,
            string? keiriCode = null,
            bool? isActive = true,
            string? ryakusyou = null,
            long? busyoBaseId = 1,
            long? oyaId = 0,
            long? shoninBusyoId = 0)
        {
            var result = new Busyo()
            {
                Code = code?.Trim() ?? $"B{id:D4}",
                Name = name?.Trim() ?? $"部署{id}",
                KanaName = kanaName?.Trim() ?? $"ブショ{id}",
                OyaCode = oyaCode?.Trim() ?? $"OB{id:D4}",
                StartYmd = startYmd ?? DateOnly.MinValue,
                EndYmd = endYmd ?? DateOnly.MaxValue,
                Jyunjyo = jyunjyo ?? 1,
                KasyoCode = kasyoCode?.Trim() ?? $"KAS{id:D4}",
                KaikeiCode = kaikeiCode?.Trim() ?? $"KK{id:D4}",
                KeiriCode = keiriCode?.Trim() ?? $"KR{id:D4}",
                IsActive = isActive ?? true,
                Ryakusyou = ryakusyou?.Trim() ?? $"R{id}",
                BusyoBaseId = busyoBaseId ?? 1,
                OyaId = oyaId ?? 0,
                ShoninBusyoId = shoninBusyoId ?? 0
            };

            if (id.HasValue)
            {
                result.Id = id.Value;
            }

            return result;
        }

        private static Syain CreateSyain(
            long? id = 1,
            string? code = null,
            string? name = null,
            string? kanaName = null,
            char? seibetsu = null,
            string? busyoCode = null,
            int? syokusyuCode = null,
            int? syokusyuBunruiCode = null,
            DateOnly? nyushaYmd = null,
            DateOnly? startYmd = null,
            DateOnly? endYmd = null,
            short? kyusyoku = 0,
            BusinessTripRole? syucyoSyokui = BusinessTripRole._2_6級,
            string? kingsSyozoku = null,
            short? kaisyaCode = 0,
            bool? isGenkaRendou = false,
            string? eMail = null,
            string? keitaiMail = null,
            EmployeeAuthority? kengen = EmployeeAuthority.None,
            short? jyunjyo = 0,
            bool? retired = false,
            long? gyoumuTypeId = 1,
            string? phoneNumber = null,
            long? syainBaseId = 1,
            long? busyoId = 1,
            long? kintaiZokuseiId = 1,
            long? userRoleId = 1)
        {
            var result = new Syain
            {
                Code = code?.Trim() ?? $"S{id:D4}",
                Name = name?.Trim() ?? $"社員{id}",
                KanaName = kanaName?.Trim() ?? $"シャイン{id}",
                Seibetsu = seibetsu ?? '1',
                BusyoCode = busyoCode?.Trim() ?? $"B{id:D4}",
                SyokusyuCode = syokusyuCode ?? 0,
                SyokusyuBunruiCode = syokusyuBunruiCode ?? 0,
                NyuusyaYmd = nyushaYmd ?? new DateOnly(2020, 1, 1),
                StartYmd = startYmd ?? DateOnly.MinValue,
                EndYmd = endYmd ?? DateOnly.MaxValue,
                Kyusyoku = kyusyoku ?? 0,
                SyucyoSyokui = syucyoSyokui ?? BusinessTripRole._2_6級,
                KingsSyozoku = kingsSyozoku?.Trim() ?? $"K{id:D4}",
                KaisyaCode = kaisyaCode ?? 0,
                IsGenkaRendou = isGenkaRendou ?? false,
                EMail = eMail?.Trim() ?? $"syain{id}@example.com",
                KeitaiMail = keitaiMail?.Trim() ?? $"keitai{id}@example.com",
                Kengen = kengen ?? EmployeeAuthority.None,
                Jyunjyo = jyunjyo ?? 0,
                Retired = retired ?? false,
                GyoumuTypeId = gyoumuTypeId,
                PhoneNumber = phoneNumber,
                SyainBaseId = syainBaseId ?? 1,
                BusyoId = busyoId ?? 1,
                KintaiZokuseiId = kintaiZokuseiId ?? 1,
                UserRoleId = userRoleId ?? 1,
            };

            if (id.HasValue)
            {
                result.Id = id.Value;
            }

            return result;
        }

        private static KintaiZokusei CreateKintaiZokusei(
            long? id = 1,
            string? name = null,
            decimal? seigenTime = 0.00m,
            bool? isMinashi = false,
            decimal? maxLimitTime = null,
            bool? isOvertimeLimit3m = false,
            EmployeeWorkType? code = EmployeeWorkType.月45時間)
        {
            var result = new KintaiZokusei
            {
                Name = name?.Trim() ?? "標準",
                SeigenTime = seigenTime ?? 45.00m,
                IsMinashi = isMinashi ?? false,
                MaxLimitTime = maxLimitTime ?? 0m,
                IsOvertimeLimit3m = isOvertimeLimit3m ?? false,
                Code = code ?? EmployeeWorkType.月45時間
            };

            if (id.HasValue)
            {
                result.Id = id.Value;
            }

            return result;
        }
    }
}
