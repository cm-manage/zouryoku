using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.Ankens;
using Zouryoku.Utils;
using ZouryokuTest.Builder;

namespace ZouryokuTest.Pages.Ankens
{
    [TestClass]
    public class InputModelTests : BaseInMemoryDbContextTest
    {
        private InputModel CreateModel(Syain? loginUser = null)
        {
            var model = new InputModel(db, GetLogger<InputModel>(), options, fakeTimeProvider);
            model.PageContext = GetPageContext();
            model.TempData = GetTempData();
            if (loginUser != null)
            {
                // ログイン情報の設定
                model.HttpContext.Session.Set(new LoginInfo { User = loginUser });
            }
            return model;
        }

        // ---------------------------------------------------------------------
        // OnGetAsync Tests
        // ---------------------------------------------------------------------

        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------

        // =================================================================
        /// <summary>
        /// 初期表示: ID未指定の場合、Pageを返すことを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#01 ID未指定 → 新規登録モードのPageを返却")]
        public async Task OnGetAsync_WhenNoId_ThenReturnsPage()
        {
            // ---------- Arrange ----------
            var model = CreateModel();

            // ---------- Act ----------
            // IDを指定せずに初期表示
            var result = await model.OnGetAsync(null);

            // ---------- Assert ----------
            Assert.IsInstanceOfType<PageResult>(result);

            // 新規登録モードであること
            Assert.IsFalse(model.IsEdit, "新規登録モードでは IsEdit は False であるべきです。");

            // 案件内容の確認
            Assert.IsNull(model.Anken.ProjectNo, "初期のプロジェクト番号はNullであるべきです。");
            Assert.IsNull(model.Anken.JuchuuNo, "初期の受注番号はNullであるべきです。");
            Assert.IsNull(model.Anken.JuchuuGyoNo, "初期の受注行番号はNullであるべきです。");
            Assert.IsNull(model.Anken.Bukken, "初期の件名はNullであるべきです。");
            Assert.IsNull(model.Anken.KingsJuchuId, "初期のKINGS受注IDはNullであるべきです。");
            Assert.IsEmpty(model.Anken.AnkenName, "初期の案件名は空白であるべきです。");
            Assert.IsNull(model.Anken.JyutyuSyuruiId, "初期の受注種類IDはNullであるべきです。");
            Assert.IsNull(model.Anken.KokyakuName, "初期の顧客名はNullであるべきです。");
            Assert.IsNull(model.Anken.KokyakuKaisyaId, "初期の顧客会社IDはNullであるべきです。");
            Assert.IsNull(model.Anken.Naiyou, "初期の案件内容はNullであるべきです。");
            Assert.IsNull(model.Anken.SyainBaseId, "初期の責任者BaseIDはNullであるべきです。");
            Assert.IsNull(model.Anken.SyainName, "初期の弊社責任者はNullであるべきです。");

            // ドロップダウンはロードされている（受注種類が無くても null ではない）
            Assert.IsNotNull(model.JyutyuSyuruiOptions, "受注種類ドロップダウンがロードされていません。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: ID指定の場合、Pageを返し、関連データが設定されることを確認
        /// （受注種類、顧客名、責任者名が設定されることを検証）
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#02 ID指定 → 編集モードPageを返却")]
        public async Task OnGetAsync_WhenIdExists_ThenReturnsPageWithEntity()
        {
            // ---------- Arrange ----------
            // シード：受注種類
            var jyutyuSyurui = new JyutyuSyuruiBuilder()
                .WithId(1)
                .Build();

            // シード：顧客会社
            KokyakuKaisha kokyaku = new KokyakuKaishaBuilder()
                .WithId(1)
                .Build();

            // シード：責任者の社員（Syain）および SyainBase を作成（Start/End を現在に合わせる）
            SyainBasis syainBase = new SyainBasisBuilder()
                .WithId(1)
                .Build();

            var today = DateTime.Today.ToDateOnly();
            var syainNow = new SyainBuilder()
                .WithId(1)
                .WithCode("NOW01")
                .WithName("現役社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(today.AddDays(-10))
                .WithEndYmd(today.AddDays(10))
                .Build();

            var syainPast = new SyainBuilder()
                .WithId(2)
                .WithCode("PAST01")
                .WithName("過去社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(today.AddDays(-20))
                .WithEndYmd(today.AddDays(-10))
                .Build();

            var syainFuture = new SyainBuilder()
                .WithId(3)
                .WithCode("FUT01")
                .WithName("未来社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(today.AddDays(10))
                .WithEndYmd(today.AddDays(20))
                .Build();

            // シード：KINGS受注
            var kings = new KingsJuchuBuilder()
                .WithId(1)
                .WithJuchuuNo("JUCHU-001")
                .WithJuchuuGyoNo(1)
                .Build();

            // シード：案件エンティティ（編集対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("既存案件")
                .WithKokyakuKaisyaId(kokyaku.Id)
                .WithJyutyuSyuruiId(jyutyuSyurui.Id)
                .WithSyainBaseId(syainBase.Id)
                .WithKingsJuchuId(kings.Id)
                .WithNaiyou("既存案件の内容です。")
                .WithVersion(0u)
                .Build();

            // シード：表示対象外の案件エンティティ
            var otherAnken = new AnkenBuilder()
                .WithId(101)
                .WithName("他の案件")
                .WithVersion(0u)
                .Build();

            // 必要データ登録
            SeedEntities(jyutyuSyurui, kokyaku, syainBase, syainNow, syainPast, syainFuture, kings, ankenEntity, otherAnken);

            var model = CreateModel();

            // ---------- Act ----------
            // IDを指定して初期表示
            var result = await model.OnGetAsync(ankenEntity.Id);

            // ---------- Assert ----------
            Assert.IsInstanceOfType<PageResult>(result);

            // 編集モードであること
            Assert.IsTrue(model.IsEdit, "編集モードでは IsEdit は True であるべきです。");

            // 案件内容の確認
            Assert.AreEqual(ankenEntity.Id, model.Anken.Id, "案件IDが一致しません。");
            Assert.AreEqual(kings.ProjectNo, model.Anken.ProjectNo, "プロジェクト番号が一致しません。");
            Assert.AreEqual(kings.JuchuuNo, model.Anken.JuchuuNo, "受注番号が一致しません。");
            Assert.AreEqual(kings.JuchuuGyoNo.ToString(), model.Anken.JuchuuGyoNo, "受注行番号が一致しません。");
            Assert.AreEqual(kings.Bukken, model.Anken.Bukken, "件名が一致しません。");
            Assert.AreEqual(ankenEntity.Name, model.Anken.AnkenName, "案件名が一致しません。");
            Assert.AreEqual(kokyaku.Name, model.Anken.KokyakuName, "顧客名が一致しません。");
            Assert.AreEqual(kokyaku.Id, model.Anken.KokyakuKaisyaId, "顧客会社IDが一致しません。");
            Assert.AreEqual(jyutyuSyurui.Id, model.Anken.JyutyuSyuruiId, "受注種類IDが一致しません。");
            Assert.AreEqual(ankenEntity.Naiyou, model.Anken.Naiyou, "案件内容が一致しません。");

            // 受注種類ドロップダウンの値と表示名が取得されること
            Assert.AreEqual(model.JyutyuSyuruiOptions[0].Value, jyutyuSyurui.Id.ToString(), "受注種類ドロップダウンリストの値が一致しません。");
            Assert.AreEqual(model.JyutyuSyuruiOptions[0].Text,  jyutyuSyurui.Name, "受注種類ドロップダウンリストの表示名が一致しません。");

            // SyainBase に紐づく社員が複数存在した場合、AnkenInputModel.SyainName に有効な名前が設定されていること
            Assert.IsNotNull(model.Anken.SyainName, "責任者名が設定されていません。");
            Assert.AreEqual(syainNow.Name,          model.Anken.SyainName,      "責任者名が現在の有効社員名と一致しません。");
            Assert.AreEqual(syainNow.SyainBaseId,   model.Anken.SyainBaseId,    "責任者BaseIDが一致しません。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: 責任者の有効期間によって、責任者名が設定されることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        [DataRow(0, 10, true, DisplayName = "#03 責任者の有効開始日がシステム日付の当日 → 責任者名が取得できる")]
        [DataRow(-10, 0, true, DisplayName = "#04 責任者の有効終了日がシステム日付の当日 → 責任者名が取得できる")]
        [DataRow(1, 10, false, DisplayName = "#05 責任者の有効開始日がシステム日付より未来 → 責任者名が取得できない")]
        [DataRow(-10, -1, false, DisplayName = "#06 責任者の有効終了日がシステム日付より過去 → 責任者名が取得できない")]
        public async Task OnGetAsync_SyainDataRanges_ThenSyainNameSet(int startOffset, int endOffset, bool shouldBeSet)
        {
            // ---------- Arrange ----------
            // シード：責任者の社員（Syain）および SyainBase を作成（Start/End を現在に合わせる）
            SyainBasis syainBase = new SyainBasisBuilder()
                .WithId(1)
                .Build();

            var today = DateTime.Today.ToDateOnly();
            var syainToday = new SyainBuilder()
                .WithId(4)
                .WithCode("TOD01")
                .WithName("当日社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(today.AddDays(startOffset))
                .WithEndYmd(today.AddDays(endOffset))
                .Build();

            // シード：案件エンティティ（編集対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("既存案件")
                .WithSyainBaseId(syainBase.Id)
                .WithVersion(0u)
                .Build();

            // 必要データ登録
            SeedEntities(syainBase, syainToday, ankenEntity);

            var model = CreateModel();

            // ---------- Act ----------
            var result = await model.OnGetAsync(ankenEntity.Id);

            // ---------- Assert ----------
            Assert.IsInstanceOfType<PageResult>(result);

            if (shouldBeSet)
            {
                // 責任者名が設定されていること
                Assert.IsNotNull(model.Anken.SyainName, "責任者名が設定されているはずです。");
                Assert.AreEqual(syainToday.Name, model.Anken.SyainName, "責任者名が一致しません。");
                return;
            }

            // 責任者名が設定されていないこと
            Assert.IsNull(model.Anken.SyainName, "責任者名は設定されていないはずです。");
        }

        // -----------------------------------------------------
        // 異常系テストケース
        // -----------------------------------------------------

        // =================================================================
        /// <summary>
        /// 初期表示: ID指定が存在しない場合、RedirectToPageResultが返却されることを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#07 ID指定が存在しない → エラーページに遷移する")]
        public async Task OnGetAsync_WhenIdNotExists_ThenReturnsRedirectToPageResult()
        {
            // ---------- Arrange ----------
            var model = CreateModel();

            // ---------- Act ----------
            // 無効なIDを指定して初期表示
            var result = await model.OnGetAsync(9999);

            // ---------- Assert ----------
            var redirect = result as RedirectToPageResult;

            Assert.IsNotNull(redirect);
            Assert.AreEqual("/ErrorMessage", redirect.PageName);
            Assert.AreEqual(Const.ErrorSelectedDataNotExists, redirect.RouteValues?["errorMessage"]);
        }

        // ---------------------------------------------------------------------
        // OnPostRegisterAsync Tests
        // ---------------------------------------------------------------------

        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------

        // =================================================================
        /// <summary>
        /// 登録処理: 有効な入力の場合、新規レコードが追加されていること
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#20 登録モードで有効な入力 → 新規レコード追加")]
        public async Task OnPostRegisterAsync_WhenValid_ThenAdded()
        {
            // ---------- Arrange ----------
            // シードデータ作成
            var testSeeds = new TestSeedEntitiesSet();

            // 必要データ登録
            SeedEntities(testSeeds.ToArray());

            // セッションにログイン情報を設定
            var model = CreateModel(testSeeds.SyainLogin);

            // 案件モデルに有効なデータを設定
            model.Anken = new AnkenInputModel
            {
                AnkenName = "新規案件",
                ProjectNo = "PJ-2024-001",
                KokyakuName = testSeeds.Kokyaku.Name,
                KingsJuchuId = testSeeds.KingsJuchu.Id,
                JyutyuSyuruiId = testSeeds.JyutyuSyurui.Id,
                KokyakuKaisyaId = testSeeds.Kokyaku.Id,
                SyainBaseId = testSeeds.SyainBaseForAnken.Id,
                Naiyou = "新規案件の内容です。",
                Version = 0u
            };

            // ---------- Act ----------
            var result = await model.OnPostRegisterAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<JsonResult>(result);

            // 案件が登録できていること
            var count = await db.Ankens.CountAsync();
            Assert.AreEqual(1, count, "案件が1件追加されているはずです。");

            var saved = await db.Ankens.FirstAsync();

            // 案件内容の確認
            Assert.AreEqual(1, saved.Id, "保存された案件IDが1であるべきです。");
            Assert.AreEqual("新規案件", saved.Name, "保存された案件名が一致しません。");
            Assert.AreEqual(testSeeds.Kokyaku.Id, saved.KokyakuKaisyaId, "保存された顧客IDが一致しません。");
            Assert.AreEqual(testSeeds.JyutyuSyurui.Id, saved.JyutyuSyuruiId, "保存された受注種類IDが一致しません。");
            Assert.AreEqual(testSeeds.KingsJuchu.Id, saved.KingsJuchuId, "保存されたKINGS受注IDが一致しません。");
            Assert.AreEqual(testSeeds.SyainBaseForAnken.Id, saved.SyainBaseId, "保存された責任者BaseIDが一致しません。");
            Assert.AreEqual("新規案件の内容です。", saved.Naiyou, "保存された案件内容が一致しません。");
            Assert.AreEqual("新規案件", saved.SearchName, "保存された検索用案件名が一致しません。");
        }

        // =================================================================
        /// <summary>
        /// 登録処理: 任意項目が未入力の場合でも正常に登録されることを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#23 登録モードで任意項目のみ未入力 → 新規レコード追加")]
        public async Task OnPostRegisterAsync_WhenOptionalEmpty_ThenAdded()
        {
            // ---------- Arrange ----------
            // シードデータ作成
            var testSeeds = new TestSeedEntitiesSet();

            // 必要データ登録
            SeedEntities(testSeeds.ToArray());

            // セッションにログイン情報を設定
            var model = CreateModel(testSeeds.SyainLogin);

            // 案件モデルに任意項目を未入力で設定
            model.Anken = new AnkenInputModel
            {
                AnkenName = "新規案件",
                ProjectNo = "PJ-2024-001",
                KokyakuName = testSeeds.Kokyaku.Name,
                KingsJuchuId = testSeeds.KingsJuchu.Id,
                KokyakuKaisyaId = testSeeds.Kokyaku.Id,
                Version = 0u
            };

            // ---------- Act ----------
            var result = await model.OnPostRegisterAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<JsonResult>(result);

            // 案件が登録できていること
            var count = await db.Ankens.CountAsync();
            Assert.AreEqual(1, count, "案件が1件追加されているはずです。");

            var saved = await db.Ankens.FirstAsync();

            // 必須項目が保存されていることを確認
            Assert.AreEqual("新規案件", saved.Name, "保存された案件名が一致しません。");
            Assert.AreEqual(testSeeds.Kokyaku.Id, saved.KokyakuKaisyaId, "保存された顧客IDが一致しません。");
            Assert.AreEqual(testSeeds.KingsJuchu.Id, saved.KingsJuchuId, "保存されたKINGS受注IDが一致しません。");
            Assert.AreEqual("新規案件", saved.SearchName, "保存された検索用案件名が一致しません。");

            // 任意項目がNullで保存されていることを確認
            Assert.IsNull(saved.JyutyuSyuruiId, "受注種類IDはNullで保存されているはずです。");
            Assert.IsNull(saved.SyainBaseId, "責任者BaseIDはNullで保存されているはずです。");
            Assert.IsNull(saved.Naiyou, "案件内容はNullで保存されているはずです。");
        }

        // =================================================================
        /// <summary>
        /// 登録処理: 文字数制限オーバーにならない入力の場合、新規レコードが追加されることを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#10 登録モードで最大文字数まで入力 → 新規レコード追加")]
        public async Task OnPostRegisterAsync_WhenMaxLengthValid_ThenAdded()
        {
            // ---------- Arrange ----------
            // シードデータ作成
            var testSeeds = new TestSeedEntitiesSet();

            // 必要データ登録
            SeedEntities(testSeeds.ToArray());

            // セッションにログイン情報を設定
            var model = CreateModel(testSeeds.SyainLogin);

            // 案件モデルに最大文字数のデータを設定
            model.Anken = new AnkenInputModel
            {
                AnkenName = new string('A', 128), // 案件名（最大128文字）
                ProjectNo = "PJ-2024-001",
                KokyakuName = testSeeds.Kokyaku.Name,
                KingsJuchuId = testSeeds.KingsJuchu.Id,
                JyutyuSyuruiId = testSeeds.JyutyuSyurui.Id,
                KokyakuKaisyaId = testSeeds.Kokyaku.Id,
                SyainBaseId = testSeeds.SyainBaseForAnken.Id,
                Naiyou = new string('F', 2000),   // 案件内容（最大2000文字）
                Version = 0u
            };

            // ---------- Act ----------
            var result = await model.OnPostRegisterAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<JsonResult>(result);

            // 案件が登録できていること
            var count = await db.Ankens.CountAsync();
            Assert.AreEqual(1, count, "案件が1件追加されているはずです。");

            var saved = await db.Ankens.FirstAsync();

            // 案件内容の確認
            Assert.AreEqual(new string('A', 128), saved.Name, "保存された案件名が一致しません。");
            Assert.AreEqual(new string('F', 2000), saved.Naiyou, "保存された案件内容が一致しません。");
            Assert.AreEqual(new string('A', 128), saved.SearchName, "保存された検索用案件名が一致しません。");
        }

        // =================================================================
        /// <summary>
        /// 登録処理: 加工が必要な入力の場合、正しく加工されて登録されることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        [DataRow("　 　新規案件 　 ", "新規案件", "新規案件", DisplayName = "#21 登録モードで案件名の前後に全角・半角空白 → 前後の空白文字を削除")]
        [DataRow("Ｎｅｗ　ﾌﾟﾛｼﾞｪｸﾄ", "Ｎｅｗ　ﾌﾟﾛｼﾞｪｸﾄ", "NEW プロジェクト",
            DisplayName = "#22 登録モードで案件名が全角英字 + 半角カナ + 合成文字 → 半角大文字英字 + 全角かな")]
        public async Task OnPostRegisterAsync_WhenAnkenNameNormalized_ThenAdded(
            string inputName,
            string expectedName,
            string expectedSearchName)
        {
            // ---------- Arrange ----------
            // シードデータ作成
            var testSeeds = new TestSeedEntitiesSet();

            // 必要データ登録
            SeedEntities(testSeeds.ToArray());

            // セッションにログイン情報を設定
            var model = CreateModel(testSeeds.SyainLogin);

            // 案件モデルに案件名を設定
            model.Anken = new AnkenInputModel
            {
                AnkenName = inputName,
                ProjectNo = "PJ-2024-001",
                KokyakuName = testSeeds.Kokyaku.Name,
                KingsJuchuId = testSeeds.KingsJuchu.Id,
                KokyakuKaisyaId = testSeeds.Kokyaku.Id,
                Version = 0u
            };

            // ---------- Act ----------
            var result = await model.OnPostRegisterAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<JsonResult>(result);

            // 案件が登録できていること
            var count = await db.Ankens.CountAsync();
            Assert.AreEqual(1, count, "案件が1件追加されているはずです。");

            var saved = await db.Ankens.FirstAsync();

            // 案件内容の確認
            Assert.AreEqual(expectedName, saved.Name, "保存された案件名が正しく加工されていません。");
            Assert.AreEqual(expectedSearchName, saved.SearchName, "保存された検索用案件名が正しく加工されていません。");
        }

        // =================================================================
        /// <summary>
        /// 登録処理: 有効な入力の場合、既存レコードが更新されることを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#24 編集モードで有効な入力 → 既存レコード更新")]
        public async Task OnPostRegisterAsync_WhenValid_ThenUpdated()
        {
            // ---------- Arrange ----------
            // シードデータ作成
            var testSeeds = new TestSeedEntitiesSet();

            // シード：案件エンティティ（編集対象）
            var ankenEntity = new AnkenBuilder()
                .WithName("既存案件名")
                .WithKokyakuKaisyaId(testSeeds.Kokyaku.Id + 1)
                .WithJyutyuSyuruiId(testSeeds.JyutyuSyurui.Id + 1)
                .WithSyainBaseId(testSeeds.SyainBaseForAnken.Id + 1)
                .WithKingsJuchuId(testSeeds.KingsJuchu.Id + 1)
                .WithNaiyou("既存案件内容")
                .WithSearchName("既存案件名")
                .WithVersion(0u)
                .Build();

            // 必要データ登録
            SeedEntities(testSeeds.ToArray(), ankenEntity);

            // セッションにログイン情報を設定
            var model = CreateModel(testSeeds.SyainLogin);

            // 案件モデルに有効なデータを設定
            model.Anken = new AnkenInputModel
            {
                Id = ankenEntity.Id,
                ProjectNo = "PJ-2024-002",
                AnkenName = "更新後案件名",
                KokyakuName = testSeeds.Kokyaku.Name,
                KingsJuchuId = testSeeds.KingsJuchu.Id,
                JyutyuSyuruiId = testSeeds.JyutyuSyurui.Id,
                KokyakuKaisyaId = testSeeds.Kokyaku.Id,
                SyainBaseId = testSeeds.SyainBaseForAnken.Id,
                Naiyou = "更新後案件内容",
                Version = ankenEntity.Version
            };

            // ---------- Act ----------
            var result = await model.OnPostRegisterAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<JsonResult>(result);

            // 案件件数は増えていないことを確認
            var count = await db.Ankens.CountAsync();
            Assert.AreEqual(1, count, "案件件数は増えていないはずです。");

            var updated = await db.Ankens.FirstAsync(a => a.Id == ankenEntity.Id);

            // レコードが更新されていることを確認
            Assert.AreEqual("更新後案件名", updated.Name, "案件名が更新されているはずです。");
            Assert.AreEqual(testSeeds.Kokyaku.Id, updated.KokyakuKaisyaId, "顧客会社IDが更新されているはずです。");
            Assert.AreEqual(testSeeds.JyutyuSyurui.Id, updated.JyutyuSyuruiId, "受注種類IDが更新されているはずです。");
            Assert.AreEqual(testSeeds.KingsJuchu.Id, updated.KingsJuchuId, "KINGS受注IDが更新されているはずです。");
            Assert.AreEqual(testSeeds.SyainBaseForAnken.Id, updated.SyainBaseId, "責任者BaseIDが更新されているはずです。");
            Assert.AreEqual("更新後案件内容", updated.Naiyou, "案件内容が更新されているはずです。");
            Assert.AreEqual("更新後案件名", updated.SearchName, "検索用案件名が更新されているはずです。");
        }

        // =================================================================
        /// <summary>
        /// 登録処理: 任意項目が未入力の場合でも正常に更新されることを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#27 編集モードで任意項目のみ未入力 → 既存レコード更新")]
        public async Task OnPostRegisterAsync_WhenOptionalEmpty_ThenUpdated()
        {
            // ---------- Arrange ----------
            // シードデータ作成
            var testSeeds = new TestSeedEntitiesSet();

            // シード：案件エンティティ（編集対象）
            var ankenEntity = new AnkenBuilder()
                .WithName("既存案件名")
                .WithKokyakuKaisyaId(testSeeds.Kokyaku.Id)
                .WithJyutyuSyuruiId(testSeeds.JyutyuSyurui.Id)
                .WithSyainBaseId(testSeeds.SyainBaseForAnken.Id)
                .WithKingsJuchuId(testSeeds.KingsJuchu.Id)
                .WithNaiyou("既存案件内容")
                .WithVersion(0u)
                .Build();

            // 必要データ登録
            SeedEntities(testSeeds.ToArray(), ankenEntity);

            // セッションにログイン情報を設定
            var model = CreateModel(testSeeds.SyainLogin);

            // 案件モデルに任意項目を未入力で設定
            model.Anken = new AnkenInputModel
            {
                Id = ankenEntity.Id,
                ProjectNo = "PJ-2024-002",
                AnkenName = "更新後案件名",
                KokyakuName = testSeeds.Kokyaku.Name,
                KingsJuchuId = testSeeds.KingsJuchu.Id,
                KokyakuKaisyaId = testSeeds.Kokyaku.Id,
                Version = ankenEntity.Version
            };

            // ---------- Act ----------
            var result = await model.OnPostRegisterAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<JsonResult>(result);

            // 案件件数は増えていないことを確認
            var count = await db.Ankens.CountAsync();
            Assert.AreEqual(1, count, "案件件数は増えていないはずです。");

            var updated = await db.Ankens.FirstAsync(a => a.Id == ankenEntity.Id);

            // レコードが更新されていることを確認
            Assert.AreEqual("更新後案件名", updated.Name, "案件名が更新されているはずです。");
            Assert.AreEqual(testSeeds.Kokyaku.Id, updated.KokyakuKaisyaId, "顧客会社IDが更新されているはずです。");
            Assert.IsNull(updated.JyutyuSyuruiId, "受注種類IDはNullで更新されているはずです。");
            Assert.IsNull(updated.SyainBaseId, "責任者BaseIDはNullで更新されているはずです。");
            Assert.AreEqual(testSeeds.KingsJuchu.Id, updated.KingsJuchuId, "KINGS受注IDが更新されているはずです。");
            Assert.IsNull(updated.Naiyou, "案件内容はNullで更新されているはずです。");
        }

        // =================================================================
        /// <summary>
        /// 登録処理: 加工が必要な入力の場合、正しく加工されて更新されることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        [DataRow("　 　更新後案件名 　 ", "更新後案件名", "更新後案件名", DisplayName = "#25 編集モードで案件名の前後に全角・半角空白 → 前後の空白文字を削除")]
        [DataRow("Ｎｅｗ　ﾌﾟﾛｼﾞｪｸﾄ", "Ｎｅｗ　ﾌﾟﾛｼﾞｪｸﾄ", "NEW プロジェクト",
            DisplayName = "#26 編集モードで案件名が全角英字 + 半角カナ + 合成文字 → 半角大文字英字 + 全角かな")]
        public async Task OnPostRegisterAsync_WhenAnkenNameNormalized_ThenUpdated(
            string inputName,
            string expectedName,
            string expectedSearchName)
        {
            // ---------- Arrange ----------
            // シードデータ作成
            var testSeeds = new TestSeedEntitiesSet();

            // シード：案件エンティティ（編集対象）
            var ankenEntity = new AnkenBuilder()
                .WithName("既存案件名")
                .WithKokyakuKaisyaId(testSeeds.Kokyaku.Id)
                .WithJyutyuSyuruiId(testSeeds.JyutyuSyurui.Id)
                .WithSyainBaseId(testSeeds.SyainBaseForAnken.Id)
                .WithKingsJuchuId(testSeeds.KingsJuchu.Id)
                .WithSearchName("既存案件名")
                .WithVersion(0u)
                .Build();

            // 必要データ登録
            SeedEntities(testSeeds.ToArray(), ankenEntity);

            // セッションにログイン情報を設定
            var model = CreateModel(testSeeds.SyainLogin);

            // 案件モデルに案件名を設定
            model.Anken = new AnkenInputModel
            {
                Id = ankenEntity.Id,
                AnkenName = inputName,
                ProjectNo = "PJ-2024-002",
                KokyakuName = testSeeds.Kokyaku.Name,
                KingsJuchuId = testSeeds.KingsJuchu.Id,
                KokyakuKaisyaId = testSeeds.Kokyaku.Id,
                Version = ankenEntity.Version
            };

            // ---------- Act ----------
            var result = await model.OnPostRegisterAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<JsonResult>(result);

            // 案件件数は増えていないことを確認
            var count = await db.Ankens.CountAsync();
            Assert.AreEqual(1, count, "案件件数は増えていないはずです。");

            // レコードが更新されていることを確認
            var updated = await db.Ankens.FirstAsync(a => a.Id == ankenEntity.Id);
            Assert.AreEqual(expectedName, updated.Name, "案件名が正しく加工されているはずです。");
            Assert.AreEqual(expectedSearchName, updated.SearchName, "検索用案件名が正しく加工されているはずです。");
        }

        // -----------------------------------------------------
        // 異常系テストケース
        // -----------------------------------------------------

        // =================================================================
        /// <summary>
        /// 登録処理: 必須項目が未入力の場合、アノテーションによるバリデーションエラーが発生することを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#08 必須項目が未入力 → バリデーションエラー")]
        public async Task OnPostRegisterAsync_WhenRequiredEmpty_ThenInvalid()
        {
            // ---------- Arrange ----------
            var model = CreateModel();

            // 案件モデルに必須項目を設定しない（バリデーションエラーを誘発）
            model.Anken = new AnkenInputModel
            {
                AnkenName = "",             // 案件名
                ProjectNo = "",
                JuchuuNo = "",
                JuchuuGyoNo = null,         // プロジェクト番号
                Bukken = "",                // 件名
                KokyakuName = "",           // 顧客名
                Naiyou = "",                // 案件内容(任意のため 空白 可)
                SyainName = "",             // 責任者名(任意のため 空白 可)
                KingsJuchuId = null,        // KINGS受注ID（アノテーション検証ではない）
                JyutyuSyuruiId = null,      // 受注種類ID（任意のため null 可）
                KokyakuKaisyaId = null,     // 顧客会社ID（アノテーション検証ではない）
                SyainBaseId = null,         // 責任者BaseID（任意のため null 可）
                Version = 0u
            };

            // ---------- Act ----------
            var (isValid, results) = ValidateModel(model.Anken);

            // ---------- Assert ----------
            Assert.IsFalse(isValid, "バリデーションは失敗するはずです。");

            // 各種必須エラーが含まれていることを確認
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("AnkenName")), "案件名のエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("ProjectNo")), "受注工番のいずれかのエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Bukken")), "受注件名のいずれかのエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("KokyakuName")), "顧客名のエラーが含まれているはずです。");

            // 必須エラーメッセージの内容を確認
            Assert.AreEqual(string.Format(Const.ErrorRequired, "案件名"),
                results.First(r => r.MemberNames.Contains("AnkenName")).ErrorMessage,
                "案件名のエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorRequired, "受注工番"),
                results.First(r => r.MemberNames.Contains("ProjectNo")).ErrorMessage,
                "受注工番のエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorRequired, "受注件名"),
                results.First(r => r.MemberNames.Contains("Bukken")).ErrorMessage,
                "受注件名のエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorRequired, "顧客情報"),
                results.First(r => r.MemberNames.Contains("KokyakuName")).ErrorMessage,
                "顧客名のエラーメッセージが一致しません。");

            // 任意項目のエラーが含まれていないことを確認
            Assert.IsFalse(results.Any(r => r.MemberNames.Contains("Naiyou")), "案件内容のエラーは含まれていないはずです。");
            Assert.IsFalse(results.Any(r => r.MemberNames.Contains("SyainName")), "責任者名のエラーは含まれていないはずです。");
            Assert.IsFalse(results.Any(r => r.MemberNames.Contains("JyutyuSyuruiId")), "受注種類IDのエラーは含まれていないはずです。");
            Assert.IsFalse(results.Any(r => r.MemberNames.Contains("SyainBaseId")), "責任者BaseIDのエラーは含まれていないはずです。");
            Assert.IsFalse(results.Any(r => r.MemberNames.Contains("KingsJuchuId")), "KINGS受注IDのエラーは含まれていないはずです。");
            Assert.IsFalse(results.Any(r => r.MemberNames.Contains("KokyakuKaisyaId")), "顧客会社IDのエラーは含まれていないはずです。");
        }

        // =================================================================
        /// <summary>
        /// 登録処理: 文字数制限がオーバーした場合、アノテーションによるバリデーションエラーが発生することを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#11 文字数制限超過 → バリデーションエラー")]
        public async Task OnPostRegisterAsync_WhenMaxLengthExceeded_ThenInvalid()
        {
            // ---------- Arrange ----------
            var model = CreateModel();

            // 案件モデルに文字数制限オーバーのデータを設定（バリデーションエラーを誘発）
            model.Anken = new AnkenInputModel
            {
                AnkenName = new string('A', 129), // 案件名（最大128文字）
                JuchuuGyoNo = "1",
                Bukken = "テスト受注",
                KokyakuName = "テスト顧客",
                Naiyou = new string('F', 2001),   // 案件内容（最大2000文字）
                KingsJuchuId = 1,
                JyutyuSyuruiId = 1,
                KokyakuKaisyaId = 1,
                SyainBaseId = 1,
                Version = 0u
            };

            // ---------- Act ----------
            var (isValid, results) = ValidateModel(model.Anken);

            // ---------- Assert ----------
            Assert.IsFalse(isValid, "バリデーションは失敗するはずです。");

            // 各種文字数制限エラーが含まれていることを確認
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("AnkenName")), "案件名のエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Naiyou")), "案件内容のエラーが含まれているはずです。");

            // 文字数制限エラーメッセージの内容を確認
            Assert.AreEqual(string.Format(Const.ErrorLength, "案件名", "128"),
                results.First(r => r.MemberNames.Contains("AnkenName")).ErrorMessage,
                "案件名のエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorLength, "案件内容", "2000"),
                results.First(r => r.MemberNames.Contains("Naiyou")).ErrorMessage,
                "案件内容のエラーメッセージが一致しません。");
        }

        // =================================================================
        /// <summary>
        /// 登録処理: 案件名が半角空白・全角空白のみの場合、アノテーションによるバリデーションエラーが発生することを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#09 案件名が半角・全角空白文字のみ → バリデーションエラー")]
        public async Task OnPostRegisterAsync_WhenAnkenNameBlank_ThenInvalid()
        {
            // ---------- Arrange ----------
            var model = CreateModel();

            // 案件モデルに案件名を空白のみで設定（バリデーションエラーを誘発）
            model.Anken = new AnkenInputModel
            {
                AnkenName = "　 　", // 全角空白 + 半角空白 + 全角空白
                ProjectNo = "PJ-2024-001",
                Bukken = "テスト受注",
                KokyakuName = "テスト顧客",
                Naiyou = "新規案件の内容です。",
                KingsJuchuId = 1,
                JyutyuSyuruiId = 1,
                KokyakuKaisyaId = 1,
                SyainBaseId = 1,
                Version = 0u
            };

            // ---------- Act ----------
            var (isValid, results) = ValidateModel(model.Anken);

            // ---------- Assert ----------
            Assert.IsFalse(isValid, "バリデーションは失敗するはずです。");

            // 案件名のエラーが含まれていることを確認
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("AnkenName")), "案件名のエラーが含まれているはずです。");

            // 案件名の必須エラーメッセージの内容を確認
            Assert.AreEqual(string.Format(Const.ErrorRequired, "案件名"),
                results.First(r => r.MemberNames.Contains("AnkenName")).ErrorMessage,
                "案件名のエラーメッセージが一致しません。");
        }

        // =================================================================
        /// <summary>
        /// 登録処理: バリデーションエラー発生時に、JsonResultを返すことを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#12 バリデーションエラー発生 → エラーメッセージを含むJsonResult返却")]
        public async Task OnPostRegisterAsync_WhenInvalid_ThenReturnsJsonResult()
        {
            // ---------- Arrange ----------
            var model = CreateModel();

            // 手動でバリデーションエラーを発生
            var key = "Anken.Name";
            var errorMessage = string.Format(Const.ErrorRequired, "案件名");
            model.ModelState.AddModelError(key, errorMessage);

            // ---------- Act ----------
            var result = await model.OnPostRegisterAsync();

            // ---------- Assert ----------
            var json = Assert.IsInstanceOfType<JsonResult>(result);

            // JsonResult にエラーメッセージが含まれていることを確認
            var errorMessageList = GetErrors(json, key);
            Assert.IsNotNull(errorMessageList);

            // 手動で発生させたエラーメッセージが含まれていることを確認
            Assert.Contains(errorMessage, errorMessageList, "案件名 のエラーメッセージが存在するはずです。");
        }

        // =================================================================
        /// <summary>
        /// 登録処理: 存在しない各種IDを指定した場合、登録されていないことを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        [DataRow("KingsJuchuId", DisplayName = "#14 存在しないKINGS受注IDを指定 → エラーメッセージ返却")]
        [DataRow("JyutyuSyuruiId", DisplayName = "#15 存在しない受注種類IDを指定 → エラーメッセージ返却")]
        [DataRow("KokyakuKaisyaId", DisplayName = "#16 存在しない顧客会社IDを指定 → エラーメッセージ返却")]
        [DataRow("SyainBaseId", DisplayName = "#17 存在しない社員BaseIDを指定 → エラーメッセージ返却")]
        public async Task OnPostRegisterAsync_WhenReferenceNotFound_ThenError(string invalidField)
        {
            // ---------- Arrange ----------
            // シードデータ作成
            var testSeeds = new TestSeedEntitiesSet();

            // 必要データ登録
            SeedEntities(testSeeds.ToArray());

            // セッションにログイン情報を設定
            var model = CreateModel(testSeeds.SyainLogin);

            // ベースの有効な入力
            var input = new AnkenInputModel
            {
                AnkenName = "新規案件",
                ProjectNo = "PJ-2024-001",
                KokyakuName = "顧客A",
                KingsJuchuId = testSeeds.KingsJuchu.Id,
                JyutyuSyuruiId = testSeeds.JyutyuSyurui.Id,
                KokyakuKaisyaId = testSeeds.Kokyaku.Id,
                SyainBaseId = testSeeds.SyainBaseForAnken.Id,
                Naiyou = "新規案件の内容です。",
                Version = 0u
            };

            // 指定フィールドだけ不正値
            switch (invalidField)
            {
                case "KingsJuchuId":
                    input.KingsJuchuId = 9999;
                    break;
                case "JyutyuSyuruiId":
                    input.JyutyuSyuruiId = 9999;
                    break;
                case "KokyakuKaisyaId":
                    input.KokyakuKaisyaId = 9999;
                    input.KokyakuName = "存在しない顧客";
                    break;
                case "SyainBaseId":
                    input.SyainBaseId = 9999;
                    break;
            }

            model.Anken = input;

            var beforeCount = await db.Ankens.CountAsync();

            // ---------- Act ----------
            var result = await model.OnPostRegisterAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<ObjectResult>(result);

            // ObjectResult にエラーメッセージが含まれていることを確認
            var json = result as ObjectResult;
            Assert.IsNotNull(json);
            var message = GetMessage(json);
            Assert.IsNotNull(message);

            // 存在チェックエラーが含まれていることを確認
            Assert.Contains(Const.ErrorSelectedDataNotExists, message, "存在チェックのエラーが含まれているはずです。");

            var afterCount = await db.Ankens.CountAsync();
            Assert.AreEqual(beforeCount, afterCount, "案件は追加されていないはずです。");
        }

        // =================================================================
        /// <summary>
        /// 登録処理: KINGS受注の施工部門コードがログインユーザーの所属部門と異なる場合、登録されていないことを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#18 KINGS受注の施工部門コードとログインユーザーの所属部門が不一致 → エラーメッセージ返却")]
        public async Task OnPostRegisterAsync_WhenDeptMismatch_ThenError()
        {
            // ---------- Arrange ----------
            // シードデータ作成
            var testSeeds = new TestSeedEntitiesSet();

            // ログインユーザーの所属部門コードと異なるようにKINGS受注の施工部門コードを設定
            var errorKingsJuchu = new KingsJuchuBuilder()
                .WithId(testSeeds.KingsJuchu.Id + 1)
                .WithSekouBumonCd("ERR01") // ログインユーザーと異なる部門コード
                .Build();

            // 必要データ登録
            SeedEntities(testSeeds.ToArray(), errorKingsJuchu);

            // セッションにログイン情報を設定
            var model = CreateModel(testSeeds.SyainLogin);

            // 案件モデルに有効なデータを設定
            model.Anken = new AnkenInputModel
            {
                AnkenName = "新規案件",
                ProjectNo = "PJ-2024-001",
                KokyakuName = testSeeds.Kokyaku.Name,
                KingsJuchuId = errorKingsJuchu.Id, // 不一致のKINGS受注IDを指定
                JyutyuSyuruiId = testSeeds.JyutyuSyurui.Id,
                KokyakuKaisyaId = testSeeds.Kokyaku.Id,
                SyainBaseId = testSeeds.SyainBaseForAnken.Id,
                Naiyou = "新規案件の内容です。",
                Version = 0u
            };

            // ---------- Act ----------
            var result = await model.OnPostRegisterAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<ObjectResult>(result);

            // ObjectResult にエラーメッセージが含まれていることを確認
            var json = result as ObjectResult;
            Assert.IsNotNull(json);
            var message = GetMessage(json);
            Assert.IsNotNull(message);

            // KINGS受注の施工部門コード不一致エラーが含まれていることを確認
            Assert.Contains(string.Format(Const.ErrorRequiredSubItem, "自部署", "受注"),
                message, "KINGS受注IDのエラーが含まれているはずです。");

            // 案件が追加されていないことを確認
            var count = await db.Ankens.CountAsync();
            Assert.AreEqual(0, count, "案件は追加されていないはずです。");
        }

        // =================================================================
        /// <summary>
        /// 登録処理: 指定されたIDが存在しない場合、エラーメッセージが返却されることを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#13 存在しないID指定 → エラーメッセージ返却")]
        public async Task OnPostRegisterAsync_WhenIdNotFound_ThenReturnsErrorMessage()
        {
            // ---------- Arrange ----------
            // シードデータ作成
            var testSeeds = new TestSeedEntitiesSet();

            // 必要データ登録
            SeedEntities(testSeeds.ToArray());

            // セッションにログイン情報を設定
            var model = CreateModel(testSeeds.SyainLogin);

            // 案件モデルに存在しない案件IDを設定
            model.Anken = new AnkenInputModel
            {
                Id = 9999, // 存在しない案件ID
                AnkenName = "更新後案件名",
                ProjectNo = "PJ-2024-002",
                KokyakuName = testSeeds.Kokyaku.Name,
                KingsJuchuId = testSeeds.KingsJuchu.Id,
                JyutyuSyuruiId = testSeeds.JyutyuSyurui.Id,
                KokyakuKaisyaId = testSeeds.Kokyaku.Id,
                SyainBaseId = testSeeds.SyainBaseForAnken.Id,
                Naiyou = "更新後案件内容",
                Version = 0u
            };

            // ---------- Act ----------
            var result = await model.OnPostRegisterAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<ObjectResult>(result);

            // ObjectResult にエラーメッセージが含まれていることを確認
            var json = result as ObjectResult;
            Assert.IsNotNull(json);
            var message = GetMessage(json);
            Assert.IsNotNull(message);

            // 指定IDが存在しない旨のエラーメッセージを確認
            Assert.Contains(string.Format(Const.ErrorNotFound, "案件情報", model.Anken.Id), message,
                "指定IDが存在しない旨のエラーメッセージが含まれているはずです。");
        }

        // =================================================================
        /// <summary>
        /// 登録処理: 楽観的同時実行制御エラーが発生した場合、データが更新されていないことを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#19 楽観的同時実行制御エラー発生 → エラーメッセージ返却")]
        public async Task OnPostRegisterAsync_WhenConcurrencyConflict_ThenError()
        {
            // ---------- Arrange ----------
            // シードデータ作成
            var testSeeds = new TestSeedEntitiesSet();

            // シード：案件エンティティ（編集対象）
            var ankenEntity = new AnkenBuilder()
                .WithName("既存案件名")
                .WithKokyakuKaisyaId(testSeeds.Kokyaku.Id)
                .WithJyutyuSyuruiId(testSeeds.JyutyuSyurui.Id)
                .WithSyainBaseId(testSeeds.SyainBaseForAnken.Id)
                .WithKingsJuchuId(testSeeds.KingsJuchu.Id)
                .WithNaiyou("既存案件内容")
                .WithVersion(1u) // バージョンを1に設定
                .Build();

            // 必要データ登録
            SeedEntities(testSeeds.ToArray(), ankenEntity);

            // セッションにログイン情報を設定
            var model = CreateModel(testSeeds.SyainLogin);

            // 案件モデルに有効なデータを設定（古いバージョンを指定してコンフリクトを誘発）
            model.Anken = new AnkenInputModel
            {
                Id = ankenEntity.Id,
                AnkenName = "更新後案件名",
                ProjectNo = "PJ-2024-002",
                KokyakuName = testSeeds.Kokyaku.Name,
                KingsJuchuId = testSeeds.KingsJuchu.Id,
                JyutyuSyuruiId = testSeeds.JyutyuSyurui.Id,
                KokyakuKaisyaId = testSeeds.Kokyaku.Id,
                SyainBaseId = testSeeds.SyainBaseForAnken.Id,
                Naiyou = "更新後案件内容",
                Version = 0u // 古いバージョン
            };

            // ---------- Act ----------
            var result = await model.OnPostRegisterAsync();

            //Assert
            Assert.IsInstanceOfType<ObjectResult>(result);

            // ObjectResult にエラーメッセージが含まれていることを確認
            var json = result as ObjectResult;
            Assert.IsNotNull(json);
            var message = GetMessage(json);
            Assert.IsNotNull(message);

            // 楽観的同時実行制御エラーが含まれていることを確認（コンテキスト内のレコードは変更されたままのため内容確認は行えない）
            Assert.Contains(string.Format(Const.ErrorConflictReload, "案件情報"),
                message, "楽観的同時実行制御エラーメッセージが含まれているはずです。");
        }

        // ---------------------------------------------------------------------
        // OnPostDeleteAsync Tests
        // ---------------------------------------------------------------------

        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------

        // =================================================================
        /// <summary>
        /// 削除処理: 有効なIDの場合、レコードが削除されることを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#32 削除可能な案件のID指定 → 既存レコード削除")]
        public async Task OnPostDeleteAsync_WhenValid_ThenRemovesRecord()
        {
            // ---------- Arrange ----------
            // SyainBase + Syain（ログインユーザー用）
            var syainBase = new SyainBasisBuilder()
                .WithId(1)
                .Build();

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .Build();

            // シード：案件エンティティ（削除対象）
            var anken = new AnkenBuilder()
                .WithId(500)
                .WithName("削除対象案件")
                .WithVersion(0u)
                .Build();

            // シード：案件情報参照履歴エンティティ（削除対象）
            var rirekis = new AnkenSansyouRirekiBuilder()
                .WithAnkenId(anken.Id)
                .BuildMany(1, 3, data =>
                    {
                        data.SyainBaseId = syainBase.Id + data.Id; // 異なるSyainBaseIdで複数作成
                    });

            // 必要データ登録
            SeedEntities(syainBase, syain, anken, rirekis);

            var model = CreateModel();

            // 編集モード判定のため Id をセット (IsEdit == true)
            model.Anken = new AnkenInputModel { Id = anken.Id, Version = anken.Version };

            // セッションにログイン情報を設定
            model.PageContext.HttpContext.Session.Set(syain);

            // ---------- Act ----------
            var result = await model.OnPostDeleteAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<ObjectResult>(result);

            // レコードが削除されていることを確認
            var exists = await db.Ankens.AnyAsync(x => x.Id == anken.Id);
            Assert.IsFalse(exists, "案件が削除されているはずです。");

            // 紐づく参照履歴も削除されていることを確認
            var rirekiExists = await db.AnkenSansyouRirekis
                .AnyAsync(x => x.AnkenId == anken.Id);
            Assert.IsFalse(rirekiExists, "紐づく参照履歴も削除されているはずです。");
        }

        // -----------------------------------------------------
        // 異常系テストケース
        // -----------------------------------------------------

        // =================================================================
        /// <summary>
        /// 削除処理: 新規登録フラグがTrue（編集モードでない想定）の場合、BadRequestを返すことを確認
        /// （CanAdd = true かつ Anken.Id が新規値 = 0 の場合 IsEdit == false のため BadRequest）
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#30 新規登録モードで削除 → BadRequest返却")]
        public async Task OnPostDeleteAsync_WhenNewCreateFlagTrue_ThenReturnsBadRequest()
        {
            // ---------- Arrange ----------
            var model = CreateModel();

            // 新規作成状態
            model.Anken = new AnkenInputModel { Id = 0 };

            // ---------- Act ----------
            var result = await model.OnPostDeleteAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<BadRequestResult>(result);
        }

        // =================================================================
        /// <summary>
        /// 削除処理: 指定されたIDが存在しない場合、エラーメッセージが返却されることを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#31 存在しないID指定 → エラーメッセージ返却")]
        public async Task OnPostDeleteAsync_WhenIdNotFound_ThenReturnsErrorMessage()
        {
            // ---------- Arrange ----------
            var model = CreateModel();

            // 存在しないIDを指定（IsEdit = true）
            model.Anken = new AnkenInputModel { Id = 99999, Version = 0u };

            // ---------- Act ----------
            var result = await model.OnPostDeleteAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<ObjectResult>(result);

            // ObjectResult にエラーメッセージが含まれていることを確認
            var json = result as ObjectResult;
            Assert.IsNotNull(json);
            var message = GetMessage(json);
            Assert.IsNotNull(message);

            // 指定IDが存在しない旨のエラーメッセージを確認
            Assert.Contains(string.Format(Const.ErrorNotFound, "案件情報", model.Anken.Id), message,
                "指定IDが存在しない旨のエラーメッセージが含まれているはずです。");
        }

        // =================================================================
        /// <summary>
        /// 削除処理: 日報実績に紐づく案件を削除しようとした場合、削除されないことを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#28 日報実績に紐づく案件のID指定 → エラーメッセージ返却")]
        public async Task OnPostDeleteAsync_WhenLinkedToJisseki_ThenReturnsErrorMessage()
        {
            // ---------- Arrange ----------
            // SyainBase + Syain（ログインユーザー用）
            var syainBase = new SyainBasisBuilder()
                .WithId(1)
                .Build();

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .Build();

            // シード：案件エンティティ（削除対象）
            var anken = new AnkenBuilder()
                .WithId(400)
                .WithName("削除対象案件")
                .WithVersion(0u)
                .Build();

            // シード：日報実績エンティティ（案件に紐づける）
            var jisseki = new NippouAnkenBuilder()
                .WithId(1)
                .WithAnkensId(anken.Id)
                .Build();

            // 必要データ登録
            SeedEntities(syainBase, syain, anken, jisseki);

            var model = CreateModel();

            // 編集モードにする（Id > 0）
            model.Anken = new AnkenInputModel { Id = anken.Id, Version = anken.Version };

            // セッションにログイン情報を設定
            model.PageContext.HttpContext.Session.Set(syain);

            // ---------- Act ----------
            var result = await model.OnPostDeleteAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<ObjectResult>(result);

            // ObjectResult にエラーメッセージが含まれていることを確認
            var json = result as ObjectResult;
            Assert.IsNotNull(json);
            var message = GetMessage(json);
            Assert.IsNotNull(message);

            // 紐づく実績が存在する時のエラーメッセージを確認
            Assert.Contains(string.Format(Const.ErrorLinked, "案件情報", "実績"),
                message,
                "紐づく実績が存在するため削除できない旨のエラーが含まれているはずです。");

            // レコードは削除されていないことを確認
            anken = await db.Ankens.FirstOrDefaultAsync(x => x.Id == anken.Id);
            Assert.IsNotNull(anken, "紐づく実績が存在するため案件は削除されていないはずです。");
        }

        // =================================================================
        /// <summary>
        /// 削除処理: 楽観的同時実行制御エラーが発生した場合、削除されないことを確認
        /// </summary>
        // =================================================================
        [TestMethod(DisplayName = "#29 楽観的同時実行制御エラー発生 → エラーメッセージ返却")]
        public async Task OnPostDeleteAsync_WhenConcurrencyConflict_ThenReturnsErrorMessage()
        {
            // ---------- Arrange ----------
            // SyainBase + Syain（ログインユーザー用）
            var syainBase = new SyainBasisBuilder()
                .WithId(1)
                .Build();

            var syain = new SyainBuilder()
                .WithId(1)
                .WithSyainBaseId(syainBase.Id)
                .Build();

            // シード：案件エンティティ（削除対象）
            var anken = new AnkenBuilder()
                .WithId(600)
                .WithName("排他対象案件")
                .WithVersion(9999u)
                .Build();

            // 必要データ登録
            SeedEntities(syainBase, syain, anken);

            var model = CreateModel();

            // 編集モードにする（Id > 0）
            // 敢えて異なる Version をセットして楽観的同時実行制御を誘発させる（SetOriginalValue でモデルの Version を使うため）
            model.Anken = new AnkenInputModel { Id = anken.Id, Version = 1u };

            // ---------- Act ----------
            var result = await model.OnPostDeleteAsync();

            // ---------- Assert ----------
            Assert.IsInstanceOfType<ObjectResult>(result);

            // ObjectResult にエラーメッセージが含まれていることを確認
            var json = result as ObjectResult;
            Assert.IsNotNull(json);
            var message = GetMessage(json);
            Assert.IsNotNull(message);

            // 楽観的同時実行制御エラーが含まれていることを確認
            Assert.Contains(string.Format(Const.ErrorConflictReload, "案件情報"),
                message, "楽観的同時実行制御エラーメッセージが含まれているはずです。");

            // レコードは削除されていないことを確認
            var exists = await db.Ankens.AnyAsync(x => x.Id == anken.Id);
            Assert.IsTrue(exists, "楽観的同時実行制御発生時はレコードは削除されていないはずです。");
        }

        // ---------------------------------------------------------------------
        // Helper Methods
        // ---------------------------------------------------------------------

        /// <summary>
        /// シード処理
        /// </summary>
        private void SeedEntities(params object[] entities)
        {
            foreach (var e in entities)
            {
                if (e is IEnumerable<object> list)
                {
                    db.AddRange(list);
                }
                else
                {
                    db.Add(e);
                }
            }
            db.SaveChanges();
        }

        /// <summary>
        /// 登録可能な状態のシードデータを作成するクラス
        /// </summary>
        private sealed class TestSeedEntitiesSet
        {
            /// <summary>
            /// 顧客会社
            /// </summary>
            public KokyakuKaisha Kokyaku { get; init; }

            /// <summary>
            /// 受注種類
            /// </summary>
            public JyutyuSyurui JyutyuSyurui { get; init; }

            /// <summary>
            /// KINGS受注
            /// </summary>
            public KingsJuchu KingsJuchu { get; init; }

            /// <summary>
            /// 案件用社員Base
            /// </summary>
            public SyainBasis SyainBaseForAnken { get; init; }

            /// <summary>
            /// 案件用社員
            /// </summary>
            public Syain SyainForAnken { get; init; }

            /// <summary>
            /// ログインユーザー用社員Base
            /// </summary>
            public SyainBasis SyainLoginBase { get; init; }

            /// <summary>
            /// ログインユーザー用社員
            /// </summary>
            public Syain SyainLogin { get; init; }

            public TestSeedEntitiesSet()
            {
                // シード：顧客会社
                Kokyaku = new KokyakuKaishaBuilder()
                    .WithId(1)
                    .Build();

                // シード：受注種類
                JyutyuSyurui = new JyutyuSyuruiBuilder()
                    .WithId(1)
                    .Build();

                // シード：受注情報
                KingsJuchu = new KingsJuchuBuilder()
                    .WithId(1)
                    .WithSekouBumonCd("B002")
                    .Build();

                // シード：弊社責任者
                SyainBaseForAnken = new SyainBasisBuilder()
                    .WithId(1)
                    .Build();

                SyainForAnken = new SyainBuilder()
                    .WithId(1)
                    .WithName("責任者社員")
                    .WithSyainBaseId(1)
                    .Build();

                // ログイン対象の社員（Syain）および SyainBase を作成（KINGS受注の施工部門に合わせる）
                SyainLoginBase = new SyainBasisBuilder()
                    .WithId(2)
                    .Build();

                var today = DateTime.Today.ToDateOnly();
                SyainLogin = new SyainBuilder()
                    .WithId(2)
                    .WithCode("NOW01")
                    .WithSyainBaseId(2)
                    .WithBusyoCode("B002")
                    .WithStartYmd(today.AddDays(-10))
                    .WithEndYmd(today.AddDays(10))
                    .Build();
            }

            /// <summary>
            /// 各要素の配列化
            /// </summary>
            public object[] ToArray()
                => [
                    Kokyaku,
                    JyutyuSyurui,
                    KingsJuchu,
                    SyainBaseForAnken,
                    SyainForAnken,
                    SyainLoginBase,
                    SyainLogin
                ];
        }
    }
}