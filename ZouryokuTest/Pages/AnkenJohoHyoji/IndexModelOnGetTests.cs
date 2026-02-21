using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Utils;
using ZouryokuTest.Builder;

namespace ZouryokuTest.Pages.AnkenJohoHyoji
{
    [TestClass]
    public class IndexModelOnGetTests : IndexModelTestsBase
    {
        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------

        // =================================================================
        /// <summary>
        /// 初期表示: ID指定の場合、関連データが設定されることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        [DataRow(true, DisplayName = "新規作成フラグがtrueの場合")]
        [DataRow(false, DisplayName = "新規作成フラグがfalseの場合")]
        public async Task OnGetAsync_存在する案件ID指定_関連データを返却(bool canAdd)
        {
            // ---------- Arrange ----------
            // シード：受注種類
            var jyutyuSyurui = CreateJyutyuSyurui(1);

            // シード：顧客会社
            KokyakuKaisha kokyaku = CreateKokyakuKaisha(1);

            // シード：責任者の社員（Syain）および SyainBase を作成（Start/End を現在に合わせる）
            SyainBasis syainBase = CreateSyainBasis(1);

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
            var kings = CreateKingsJuchu(1);

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("既存案件")
                .WithKokyakuKaisyaId(kokyaku.Id)
                .WithJyutyuSyuruiId(jyutyuSyurui.Id)
                .WithSyainBaseId(syainBase.Id)
                .WithKingsJuchuId(kings.Id)
                .WithNaiyou("既存案件の内容です。")
                .Build();

            // シード：表示対象外の案件エンティティ
            var otherAnken = new AnkenBuilder()
                .WithId(101)
                .WithName("他の案件")
                .Build();

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(4);

            // 必要データ登録
            SeedEntities(jyutyuSyurui, kokyaku, syainBase, syainNow, syainPast, syainFuture,
                kings, ankenEntity, otherAnken, syainLogin);

            var model = CreateModel(syainLogin);

            // ---------- Act ----------
            await model.OnGetAsync(ankenEntity.Id, canAdd);

            // ---------- Assert ----------
            var viewModel = model.IndexViewModel;
            Assert.IsNotNull(viewModel);

            // 案件内容の確認
            Assert.AreEqual(ankenEntity.Id, viewModel.Id, "案件IDが一致しません。");
            Assert.AreEqual(jyutyuSyurui.Name, viewModel.JyutyuSyuruiName, "受注種類名称が一致しません。");
            Assert.AreEqual(kokyaku.Id, viewModel.KokyakuKaisyaId, "顧客会社IDが一致しません。");
            Assert.AreEqual(kings.Id, viewModel.KingsJuchuId, "KINGS受注IDが一致しません。");
            Assert.AreEqual(syainNow.Name, viewModel.SyainName, "責任者名が一致しません。");

            // 新規作成フラグが反映されているかの確認
            Assert.AreEqual(canAdd, viewModel.CanAdd, "新規作成フラグが一致しません。");

            // 案件参照履歴が1件登録されていること
            var count = await db.AnkenSansyouRirekis.CountAsync();
            Assert.AreEqual(1, count, "案件参照履歴が1件登録されていること");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: 有効期間内の責任者の場合、責任者名が設定されることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        [DataRow(0, 10, DisplayName = "責任者の有効開始日がシステム日付の当日の場合")]
        [DataRow(-10, 0, DisplayName = "責任者の有効終了日がシステム日付の当日")]
        public async Task OnGetAsync_有効期間内の責任者_責任者名を設定(int startOffset, int endOffset)
        {
            // ---------- Arrange ----------
            // シード：責任者の社員（Syain）および SyainBase を作成（Start/End を現在に合わせる）
            SyainBasis syainBase = CreateSyainBasis(1);

            var today = DateTime.Today.ToDateOnly();
            var syainToday = new SyainBuilder()
                .WithId(4)
                .WithCode("TOD01")
                .WithName("当日社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(today.AddDays(startOffset))
                .WithEndYmd(today.AddDays(endOffset))
                .Build();

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("既存案件")
                .WithSyainBaseId(syainBase.Id)
                .Build();

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(2);

            // 必要データ登録
            SeedEntities(syainBase, syainToday, ankenEntity, syainLogin);

            var model = CreateModel(syainLogin);

            // ---------- Act ----------
            await model.OnGetAsync(ankenEntity.Id, false);

            // ---------- Assert ----------
            var viewModel = model.IndexViewModel;
            Assert.IsNotNull(viewModel);

            // 責任者名が設定されていること
            Assert.AreEqual(syainToday.Name, viewModel.SyainName, "責任者名が一致しません。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: 有効期間外の責任者の場合、責任者名が設定されないことを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        [DataRow(1, 10, DisplayName = "責任者の有効開始日がシステム日付より未来の場合")]
        [DataRow(-10, -1, DisplayName = "責任者の有効終了日がシステム日付より過去の場合")]
        public async Task OnGetAsync_有効期間外の責任者_責任者名は設定されない(int startOffset, int endOffset)
        {
            // ---------- Arrange ----------
            // シード：責任者の社員（Syain）および SyainBase を作成（Start/End を現在に合わせる）
            SyainBasis syainBase = CreateSyainBasis(1);

            var today = DateTime.Today.ToDateOnly();
            var syainToday = new SyainBuilder()
                .WithId(4)
                .WithCode("TOD01")
                .WithName("当日社員")
                .WithSyainBaseId(syainBase.Id)
                .WithStartYmd(today.AddDays(startOffset))
                .WithEndYmd(today.AddDays(endOffset))
                .Build();

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("既存案件")
                .WithSyainBaseId(syainBase.Id)
                .Build();

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(2);

            // 必要データ登録
            SeedEntities(syainBase, syainToday, ankenEntity, syainLogin);

            var model = CreateModel(syainLogin);

            // ---------- Act ----------
            await model.OnGetAsync(ankenEntity.Id, false);

            // ---------- Assert ----------
            var viewModel = model.IndexViewModel;

            // 案件情報自体は取得できていること
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(ankenEntity.Id, viewModel.Id, "案件IDが一致しません。");

            // 責任者名が設定されていないこと
            Assert.IsNull(viewModel.SyainName, "責任者名は設定されていないはずです。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: KINGS受注IDが設定されていない場合、関連データが設定されないことを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        public async Task OnGetAsync_KINGS受注IDがNULL_受注情報が設定されない()
        {
            // ---------- Arrange ----------
            // シード：受注種類
            var jyutyuSyurui = CreateJyutyuSyurui(1);

            // シード：顧客会社
            var kokyaku = CreateKokyakuKaisha(1);

            // シード：責任者の社員
            var syainBase = CreateSyainBasis(1);
            var syain = CreateSyain(1, syainBase.Id);

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("関連データなし案件")
                .WithKokyakuKaisyaId(kokyaku.Id)
                .WithJyutyuSyuruiId(jyutyuSyurui.Id)
                .WithSyainBaseId(syainBase.Id)
                .Build();

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(2);

            // 必要データ登録
            SeedEntities(jyutyuSyurui, kokyaku, syainBase, syain, ankenEntity, syainLogin);

            var model = CreateModel(syainLogin);

            // ---------- Act ----------
            await model.OnGetAsync(ankenEntity.Id, false);

            // ---------- Assert ----------
            var viewModel = model.IndexViewModel;

            // 案件情報自体は取得できていること
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(ankenEntity.Id, viewModel.Id, "案件IDが一致しません。");

            // 関連データが設定されていないこと
            Assert.IsNull(viewModel.DispJuchuNo, "受注工番はNULLのはずです。");
            Assert.IsNull(viewModel.KingsJuchuId, "KINGS受注IDはNULLのはずです。");
            Assert.IsNull(viewModel.Bukken, "件名はNULLのはずです。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: 受注種類IDが設定されていない場合、関連データが設定されないことを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        public async Task OnGetAsync_受注種類IDがNULL_受注種類名が設定されない()
        {
            // ---------- Arrange ----------
            // シード：顧客会社
            var kokyaku = CreateKokyakuKaisha(1);

            // シード：責任者の社員
            var syainBase = CreateSyainBasis(1);
            var syain = CreateSyain(1, syainBase.Id);

            // シード：KINGS受注
            var kings = CreateKingsJuchu(1);

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("関連データなし案件")
                .WithKokyakuKaisyaId(kokyaku.Id)
                .WithSyainBaseId(syainBase.Id)
                .WithKingsJuchuId(kings.Id)
                .Build();

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(2);

            // 必要データ登録
            SeedEntities(kokyaku, syainBase, syain, kings, ankenEntity, syainLogin);

            var model = CreateModel(syainLogin);

            // ---------- Act ----------
            await model.OnGetAsync(ankenEntity.Id, false);

            // ---------- Assert ----------
            var viewModel = model.IndexViewModel;

            // 案件情報自体は取得できていること
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(ankenEntity.Id, viewModel.Id, "案件IDが一致しません。");

            // 関連データが設定されていないこと
            Assert.IsNull(viewModel.JyutyuSyuruiName, "受注種類名称はNULLのはずです。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: 顧客会社IDが設定されていない場合、関連データが設定されないことを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        public async Task OnGetAsync_顧客会社IDがNULL_顧客会社情報が設定されない()
        {
            // ---------- Arrange ----------
            // シード：受注種類
            var jyutyuSyurui = CreateJyutyuSyurui(1);

            // シード：責任者の社員
            var syainBase = CreateSyainBasis(1);
            var syain = CreateSyain(1, syainBase.Id);

            // シード：KINGS受注
            var kings = CreateKingsJuchu(1);

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("関連データなし案件")
                .WithJyutyuSyuruiId(jyutyuSyurui.Id)
                .WithSyainBaseId(syainBase.Id)
                .WithKingsJuchuId(kings.Id)
                .Build();

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(2);

            // 必要データ登録
            SeedEntities(jyutyuSyurui, syainBase, syain, ankenEntity, syainLogin);

            var model = CreateModel(syainLogin);

            // ---------- Act ----------
            await model.OnGetAsync(ankenEntity.Id, false);

            // ---------- Assert ----------
            var viewModel = model.IndexViewModel;

            // 案件情報自体は取得できていること
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(ankenEntity.Id, viewModel.Id, "案件IDが一致しません。");

            // 関連データが設定されていないこと
            Assert.IsNull(viewModel.KokyakuName, "顧客情報はNULLのはずです。");
            Assert.IsNull(viewModel.KokyakuKaisyaId, "顧客会社IDはNULLのはずです。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: 社員BaseIDが設定されていない場合、関連データが設定されないことを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        public async Task OnGetAsync_社員BaseIDがNULL_責任者情報が設定されない()
        {
            // ---------- Arrange ----------
            // シード：受注種類
            var jyutyuSyurui = CreateJyutyuSyurui(1);

            // シード：顧客会社
            var kokyaku = CreateKokyakuKaisha(1);

            // シード：KINGS受注
            var kings = CreateKingsJuchu(1);

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("関連データなし案件")
                .WithKokyakuKaisyaId(kokyaku.Id)
                .WithJyutyuSyuruiId(jyutyuSyurui.Id)
                .WithKingsJuchuId(kings.Id)
                .Build();

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(2);

            // 必要データ登録
            SeedEntities(jyutyuSyurui, kokyaku, kings, ankenEntity, syainLogin);

            var model = CreateModel(syainLogin);

            // ---------- Act ----------
            await model.OnGetAsync(ankenEntity.Id, false);

            // ---------- Assert ----------
            var viewModel = model.IndexViewModel;

            // 案件情報自体は取得できていること
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(ankenEntity.Id, viewModel.Id, "案件IDが一致しません。");

            // 関連データが設定されていないこと
            Assert.IsNull(viewModel.SyainName, "責任者名はNULLのはずです。");
        }

        // -----------------------------------------------------
        // 異常系テストケース
        // -----------------------------------------------------

        // =================================================================
        /// <summary>
        /// 初期表示: 存在しないID指定の場合、Pageを返し、ModelStateにエラーが設定されることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        public async Task OnGetAsync_存在しない案件ID指定_ModelStateにエラー設定()
        {
            // ---------- Arrange ----------
            // シード：案件エンティティ（表示対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(1)
                .Build();

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(2);

            // 必要データ登録
            SeedEntities(ankenEntity, syainLogin);

            var model = CreateModel(syainLogin);

            // ---------- Act ----------
            var nonexistentId = ankenEntity.Id + 1;
            var result = await model.OnGetAsync(nonexistentId, false);

            // ---------- Assert ----------
            Assert.IsInstanceOfType<PageResult>(result);

            // ModelStateにエラーが設定されていること
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsNotNull(model.ModelState[string.Empty], "ModelStateにキーがemptyのエラーが存在するはずです。");

            // エラーメッセージの確認
            var messages = model.ModelState[string.Empty]!.Errors.Select(e => e.ErrorMessage).ToList();
            Assert.HasCount(1, messages, "ModelStateにはエラーが1件設定されているはずです。");
            Assert.AreEqual(Const.ErrorSelectedDataNotExists, messages[0], "エラーメッセージが一致しません。");

            var viewModel = model.IndexViewModel;

            // ViewModelの設定内容の確認
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.CanAdd, "Falseのはずです。");
            Assert.AreEqual(syainLogin.Id, viewModel.LoginInfo.User.Id, "ログイン情報が一致しません。");
            Assert.IsNull(viewModel.Id, "案件IDはNULLのはずです。");

            // 案件参照履歴が登録されていないこと
            var count = await db.AnkenSansyouRirekis.CountAsync();
            Assert.AreEqual(0, count, "案件参照履歴が登録されていないこと");
        }
    }
}
