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
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine, fakeTimeProvider)
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
            var kintai3Months60Hours = KintaiZokuseiEntity.CreateKintaiZokusei(
                id: (short)_3か月60時間,
                name: "_3か月60時間");

            var kintaiPartTime = KintaiZokuseiEntity.CreateKintaiZokusei(
                id: (short)パート,
                name: "パート");

            var kintaiMinashi = KintaiZokuseiEntity.CreateKintaiZokusei(
                id: (short)みなし対象者,
                name: "みなし対象者",
                isMinashi: true,
                code: みなし対象者);

            db.AddRange(kintai3Months60Hours, kintaiPartTime, kintaiMinashi);

            // 部署
            var busyoSystem = BusyoEntity.CreateBusyo(
                id: 1,
                name: "システム部",
                isActive: true,
                jyunjyo: 1);

            var busyoSales = BusyoEntity.CreateBusyo(
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
            var kintai3Months60Hours = KintaiZokuseiEntity.CreateKintaiZokusei(
                id: (short)_3か月60時間,
                name: "_3か月60時間");

            var kintaiPart = KintaiZokuseiEntity.CreateKintaiZokusei(
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
            var syain1 = SyainEntity.CreateSyain(
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
            var syain2 = SyainEntity.CreateSyain(
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
            var syain3 = SyainEntity.CreateSyain(
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
        [TestMethod(DisplayName = "初期表示したとき、勤怠属性の選択肢が取得されること")]
        public async Task OnGetAsync_初期表示_勤怠属性の選択肢が取得されること()
        {
            // Arrange
            SeedBase();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual((long)みなし対象者, model.Condition.KintaiZokuseiId, "初期表示時の勤怠属性IDは" +
                "3であること。");
            Assert.IsNotNull(model.Condition.KintaiZokuseiOptions, "勤怠属性の選択肢が取得されていること");
            Assert.AreEqual(3, model.Condition.KintaiZokuseiOptions.Count(), "勤怠属性の選択肢が" +
                "2件取得されていること");
        }

        /// <summary>
        /// ①-2 初期表示: 勤怠属性IDが事前設定済みの場合は上書きしない
        /// </summary>
        [TestMethod(DisplayName = "初期表示時に勤怠属性IDが指定済みなら上書きされないこと")]
        public async Task OnGetAsync_初期表示_勤怠属性ID指定済みなら上書きしないこと()
        {
            // Arrange
            SeedBase();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.KintaiZokuseiId = (long)パート;

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.AreEqual((long)パート, model.Condition.KintaiZokuseiId, "事前設定した勤怠属性IDが保持されること");
        }

        /// <summary>
        /// ①-3 初期表示: みなし対象者が存在しない場合は勤怠属性IDを設定しない
        /// </summary>
        [TestMethod(DisplayName = "初期表示時にみなし対象者が未登録なら勤怠属性IDは未設定のままであること")]
        public async Task OnGetAsync_初期表示_みなし対象者未登録なら勤怠属性ID未設定のまま()
        {
            // Arrange
            SeedBaseWithoutMinashi();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsFalse(model.Condition.KintaiZokuseiId.HasValue, "みなし対象者が存在しない場合は勤怠属性IDが" +
                "設定されないこと");
            Assert.AreEqual(2, model.Condition.KintaiZokuseiOptions.Count(), "みなし対象者なしの2件が" +
                "選択肢になること");
        }

        /// <summary>
        /// ②初期表示: ロールの選択肢が取得される
        /// </summary>
        [TestMethod(DisplayName = "初期表示したとき、ロールの選択肢が取得されること")]
        public async Task OnGetAsync_初期表示_ロールの選択肢が取得されること()
        {
            // Arrange
            SeedBase();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.Condition.UserRoleOptions, "ロールの選択肢が取得されていること");
            Assert.AreEqual(2, model.Condition.UserRoleOptions.Count(), "ロールの選択肢が2件取得されていること");
        }

        /// <summary>
        /// ③初期表示: 社員権限の選択肢が取得される
        /// </summary>
        [TestMethod(DisplayName = "初期表示したとき、社員権限の選択肢が取得されること")]
        public async Task OnGetAsync_初期表示_社員権限の選択肢が取得されること()
        {
            // Arrange
            SeedBase();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsNotNull(model.Condition.KengenOptions, "社員権限の選択肢が取得されていること");
            // EmployeeAuthority enumの全値分
            var expectedCount = Enum.GetValues(typeof(EmployeeAuthority)).Length;
            Assert.AreEqual(expectedCount, model.Condition.KengenOptions.Count(), "全権限の選択肢が" +
                "取得されていること");
        }

        /// <summary>
        /// ④初期表示: 検索結果は空である
        /// </summary>
        [TestMethod(DisplayName = "初期表示したとき、検索結果はあること")]
        public async Task OnGetAsync_初期表示_検索結果はあること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            await model.OnGetAsync();

            // Assert
            Assert.IsTrue(model.Results.Any(), "初期表示時は検索結果が空であること");
        }

        /// <summary>
        /// ⑤初期表示: PageResultが返される
        /// </summary>
        [TestMethod(DisplayName = "初期表示したとき、PageResultが返されること")]
        public async Task OnGetAsync_初期表示_PageResultが返されること()
        {
            // Arrange
            SeedBase();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult), "PageResultが" +
                "返されること");
        }

        // =====================================================================
        // OnGetSearchAsync テスト: フィルター条件
        // =====================================================================

        /// <summary>
        /// ⑥検索: 条件なし、退職者除外（デフォルト）
        /// </summary>
        [TestMethod(DisplayName = "条件なしで検索したとき、退職者を除外して取得すること")]
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
            Assert.AreEqual(2, model.Results.Count(), "退職者以外の2件が取得されること");
            Assert.IsTrue(model.Results.Any(r => r.SyainNo == "S001"), "田中が取得されていること");
            Assert.IsTrue(model.Results.Any(r => r.SyainNo == "S003"), "鈴木が取得されていること");
            Assert.IsFalse(model.Results.Any(r => r.SyainNo == "S002"), "退職者の佐藤は取得されないこと");
        }

        /// <summary>
        /// ⑦検索: 退職者を含む
        /// </summary>
        [TestMethod(DisplayName = "退職者を含む条件で検索したとき、退職者を含めて全員取得すること")]
        public async Task OnPostSearchAsync_退職者を含むON_全員取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.IncludeRetired = true;

            // Act
            await model.OnGetSearchAsync();

            // Assert
            // 田中(S001), 佐藤(S002), 鈴木(S003) 全員取得
            Assert.AreEqual(3, model.Results.Count(), "退職者を含む全3件が取得されること");
        }

        /// <summary>
        /// ⑧検索: 社員番号（部分一致）
        /// </summary>
        [TestMethod(DisplayName = "社員番号で検索したとき、部分一致で取得すること")]
        public async Task OnPostSearchAsync_社員番号検索_部分一致で取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.SyainNo = "003"; // S003(鈴木)

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.AreEqual(1, model.Results.Count(), "1件取得されること");
            Assert.AreEqual("S003", model.Results[0].SyainNo, "社員番号S003の社員が取得されていること");
        }

        /// <summary>
        /// ⑨検索: 社員名（部分一致）
        /// </summary>
        [TestMethod(DisplayName = "社員名で検索したとき、部分一致で取得すること")]
        public async Task OnPostSearchAsync_社員名検索_部分一致で取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.SyainName = "太郎"; // 田中太郎(S001)

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.AreEqual(1, model.Results.Count(), "1件取得されること");
            Assert.AreEqual("S001", model.Results[0].SyainNo, "社員名に'太郎'を含む社員が取得されていること");
        }

        /// <summary>
        /// ⑩-A 検索: 退職者OFF時に退職者が取得されないこと
        /// </summary>
        [TestMethod(DisplayName =
            "社員名で退職者を検索したとき、退職者OFFでは取得されないこと")]
        public async Task
            OnPostSearchAsync_社員名で退職者を検索_退職者OFFでは取得されないこと()
        {
            // Arrange（準備）
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.SyainName = "花子"; // 佐藤花子(S002, 退職者)

            // Act（実行）
            await model.OnGetSearchAsync();

            // Assert（検証）
            Assert.IsFalse(
                model.Results.Any(),
                "退職者OFFのときは取得されないこと");
        }

        /// <summary>
        /// ⑩-B 検索: 退職者ON時に退職者が取得されること
        /// </summary>
        [TestMethod(DisplayName =
            "社員名で退職者を検索したとき、退職者ONでは取得されること")]
        public async Task
            OnPostSearchAsync_社員名で退職者を検索_退職者ONでは取得されること()
        {
            // Arrange（準備）
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.SyainName = "花子"; // 佐藤花子(S002, 退職者)
            model.Condition.IncludeRetired = true;

            // Act（実行）
            await model.OnGetSearchAsync();

            // Assert（検証）
            Assert.AreEqual(
                1,
                model.Results.Count(),
                "退職者ONのときは1件取得されること");
            Assert.AreEqual(
                "S002",
                model.Results[0].SyainNo,
                "退職者の佐藤が取得されていること");
        }

        /// <summary>
        /// ⑪検索: 部署名（部分一致）
        /// </summary>
        [TestMethod(DisplayName = "部署名で検索したとき、部分一致で取得すること")]
        public async Task OnPostSearchAsync_部署名検索_部分一致で取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.BusyoName = "システム"; // 田中(S001)のみ

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.AreEqual(1, model.Results.Count(), "1件取得されること");
            Assert.AreEqual("S001", model.Results[0].SyainNo, "部署名に'システム'を含む社員が取得されていること");
        }

        /// <summary>
        /// ⑫検索: 部署名で複数件取得
        /// </summary>
        [TestMethod(DisplayName = "部署名で検索したとき、複数件取得できること")]
        public async Task OnPostSearchAsync_部署名検索_営業部で複数件取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.BusyoName = "営業"; // 佐藤(退職)と鈴木
            model.Condition.IncludeRetired = true;

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.AreEqual(2, model.Results.Count(), "営業部の社員が2件取得されること");
        }

        /// <summary>
        /// ⑬検索: 勤怠属性
        /// </summary>
        [TestMethod(DisplayName = "勤怠属性を指定して検索したとき、該当する社員のみ取得すること")]
        public async Task OnPostSearchAsync_勤怠属性検索_一致するものを取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.KintaiZokuseiId = 2; // シフト勤務 → 鈴木(S003)のみ

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.AreEqual(1, model.Results.Count(), "該当する1件のみ取得されること");
            Assert.AreEqual("S003", model.Results[0].SyainNo, "勤怠属性が'シフト'の鈴木が取得されていること");
        }

        /// <summary>
        /// ⑭検索: ロール
        /// </summary>
        [TestMethod(DisplayName = "ロールを指定して検索したとき、該当する社員のみ取得すること")]
        public async Task OnPostSearchAsync_ロール検索_一致するものを取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.UserRoleId = 1; // 管理者 → 田中(S001)のみ

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.AreEqual(1, model.Results.Count(), "該当する1件のみ取得されること");
            Assert.AreEqual("S001", model.Results[0].SyainNo, "ロールが'管理者'の田中が取得されていること");
        }

        /// <summary>
        /// ⑮検索: 社員権限（ビットマスク一致）
        /// </summary>
        [TestMethod(DisplayName = "社員権限を指定して検索したとき、ビットマスクで一致する社員を取得すること")]
        public async Task OnPostSearchAsync_社員権限検索_ビットマスクで一致するものを取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.Kengen = 労働状況報告; // 田中(S001)と鈴木(S003)

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.AreEqual(2, model.Results.Count(), "該当する2件が取得されること");
            Assert.IsTrue(model.Results.Any(r => r.SyainNo == "S001"), "田中が取得されていること");
            Assert.IsTrue(model.Results.Any(r => r.SyainNo == "S003"), "鈴木が取得されていること");
        }

        /// <summary>
        /// ⑯検索: 社員権限（フラグ判定）
        /// </summary>
        [TestMethod(DisplayName = "特定の社員権限フラグで検索したとき、一致する社員を取得すること")]
        public async Task OnPostSearchAsync_社員権限検索_特定のフラグで一致するものを取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.Kengen = 勤務日報未確定チェック; // 鈴木(S003)のみ

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.AreEqual(1, model.Results.Count(), "該当する1件のみ取得されること");
            Assert.AreEqual("S003", model.Results[0].SyainNo, "該当する権限を持つ鈴木が取得されていること");
        }

        /// <summary>
        /// ⑰検索: 級職（部分一致）
        /// </summary>
        [TestMethod(DisplayName = "級職で検索したとき、部分一致で取得すること")]
        public async Task OnPostSearchAsync_級職検索_部分一致で取得すること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.Grade = 5; // 佐藤(S002, 級職=5, 退職)
            model.Condition.IncludeRetired = true;

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.AreEqual(1, model.Results.Count(), "該当する1件のみ取得されること");
            Assert.AreEqual("S002", model.Results[0].SyainNo, "級職'5'の佐藤が取得されていること");
        }

        /// <summary>
        /// ⑱検索: 級職で複数件取得
        /// </summary>
        [TestMethod(DisplayName = "級職で検索したとき、同じ級職の社員が複数取得されること")]
        public async Task OnPostSearchAsync_級職検索_同じ級職の社員が複数取得されること()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.Grade = 3; // 田中(級職3)と鈴木(級職3)

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.AreEqual(2, model.Results.Count(), "同じ級職の社員が2件取得されること");
        }

        /// <summary>
        /// ⑲検索: 該当なし
        /// </summary>
        [TestMethod(DisplayName = "該当する社員がいないとき、空の結果を返すこと")]
        public async Task OnPostSearchAsync_該当なし_空リストを返すこと()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.SyainNo = "XXXX"; // 存在しない

            // Act
            await model.OnGetSearchAsync();

            // Assert
            Assert.IsFalse(model.Results.Any(), "該当なしのときは空リストであること");
        }

        /// <summary>
        /// ⑳検索: 複数条件の組み合わせ
        /// </summary>
        [TestMethod(DisplayName = "複数の条件を組み合わせたとき、AND条件で絞り込まれること")]
        public async Task OnPostSearchAsync_複数条件_AND条件で絞り込むこと()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.BusyoName = "営業"; // 営業部の社員
            model.Condition.KintaiZokuseiId = 2; // シフト勤務

            // Act
            await model.OnGetSearchAsync();

            // Assert
            // 営業部かつシフト勤務 → 鈴木(S003)のみ
            Assert.AreEqual(1, model.Results.Count(), "絞り込まれた1件のみ取得されること");
            Assert.AreEqual("S003", model.Results[0].SyainNo, "条件に一致する鈴木が取得されていること");
        }

        // =====================================================================
        // OnGetSearchAsync テスト: ソート順
        // =====================================================================

        /// <summary>
        /// ㉑検索: ソート順の確認（部署順序→社員順序降順→社員番号降順）
        /// </summary>
        [TestMethod(DisplayName = "検索したとき、部署順序昇順、社員順序降順、社員番号降順でソートされていること")]
        public async Task OnPostSearchAsync_ソート順_部署順序昇順_社員順序降順_社員番号降順()
        {
            // Arrange
            CreateConditionRecords();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.IncludeRetired = true; // 全員取得

            // Act
            await model.OnGetSearchAsync();

            // Assert
            // ソート: .OrderBy(s => s.Busyo.Jyunjyo)   → システム(1) → 営業(2)
            //         .ThenByDescending(s => s.Jyunjyo)  → 順序降順
            //         .ThenByDescending(s => s.Code)     → コード降順
            // システム部(順1): 田中(順1, S001)
            // 営業部(順2): 鈴木(順3, S003), 佐藤(順2, S002)
            Assert.AreEqual(3, model.Results.Count(), "全員取得されていること");
            Assert.AreEqual("S001", model.Results[0].SyainNo, "1番目は田中（システム部 順1）");
            Assert.AreEqual("S003", model.Results[1].SyainNo, "2番目は鈴木（営業部 順3）");
            Assert.AreEqual("S002", model.Results[2].SyainNo, "3番目は佐藤（営業部 順2）");
        }

        // =====================================================================
        // SyainViewModel テスト: ViewModelマッピング
        // =====================================================================

        /// <summary>
        /// ㉒ViewModel: マッピング確認（退職者）
        /// </summary>
        [TestMethod(DisplayName = "退職者の社員情報をマッピングしたとき、ViewModelに正しく反映されること")]
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
            Assert.AreEqual(100, vm.SyainBaseId, "SyainBaseIdが一致すること");
            Assert.AreEqual("T100", vm.SyainNo, "SyainNoが一致すること");
            Assert.AreEqual("テスト太郎", vm.Name, "Nameが一致すること");
            Assert.AreEqual("テスト部署", vm.BusyoName, "BusyoNameが一致すること");
            Assert.AreEqual((short)5, vm.Grade, "Gradeが一致すること");
            Assert.AreEqual("通常", vm.KintaiZokuseiName, "KintaiZokuseiNameが一致すること");
            Assert.AreEqual("管理者", vm.UserRoleName, "UserRoleNameが一致すること");
            Assert.AreEqual("退職", vm.RetiredDisplay, "退職フラグが'退職'と表示されること");
        }

        /// <summary>
        /// ㉓ViewModel: マッピング確認（在職者）
        /// </summary>
        [TestMethod(DisplayName = "在職者の社員情報をマッピングしたとき、ViewModelに正しく反映されること")]
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
            Assert.AreEqual(200, vm.SyainBaseId, "SyainBaseIdが一致すること");
            Assert.AreEqual("T200", vm.SyainNo, "SyainNoが一致すること");
            Assert.AreEqual("テスト花子", vm.Name, "Nameが一致すること");
            Assert.AreEqual("営業部", vm.BusyoName, "BusyoNameが一致すること");
            Assert.AreEqual((short)2, vm.Grade, "Gradeが一致すること");
            Assert.AreEqual("シフト", vm.KintaiZokuseiName, "KintaiZokuseiNameが一致すること");
            Assert.AreEqual("一般", vm.UserRoleName, "UserRoleNameが一致すること");
            Assert.AreEqual(string.Empty, vm.RetiredDisplay, "在職の場合は空文字になること");
        }

        /// <summary>
        /// ㉔IndexModel: Conditionセッター
        /// </summary>
        [TestMethod(DisplayName = "Conditionに検索条件を設定したとき、設定したインスタンスが保持されること")]
        public void IndexModel_Conditionセッター_設定した検索条件インスタンスを保持すること()
        {
            // Arrange
            var model = CreateModel();
            var condition = new SyainSearchCondition
            {
                SyainNo = "S999",
                Grade = 9,
                IncludeRetired = true
            };

            // Act
            model.Condition = condition;

            // Assert
            Assert.AreSame(condition, model.Condition, "Conditionのsetter/getterで同一インスタンスが保持されること");
        }
    }
}
