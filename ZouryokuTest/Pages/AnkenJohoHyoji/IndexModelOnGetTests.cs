using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Utils;

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
            var jyutyuSyurui = new JyutyuSyurui()
            {
                Id = 1,
                Name = "受注種類A",
                Code = "",
            };

            // シード：顧客会社
            KokyakuKaisha kokyaku = new KokyakuKaisha()
            {
                Id = 1,
                Name = "顧客会社A",
                Shiten = "本店",
                NameKana = "",
                Ryakusyou = "",
                SearchName = "",
                SearchNameKana = "",
            };

            // シード：責任者の社員（Syain）および SyainBase を作成（Start/End を現在に合わせる）
            SyainBasis syainBase = new SyainBasis()
            {
                Id = 1,
                Code = "",
            };

            var syainNow = new Syain(){
                Id = 1,
                Name = "現役社員",
                SyainBaseId = syainBase.Id,
                StartYmd = today.AddDays(-10),
                EndYmd = today.AddDays(10),
                Code = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            var syainPast = new Syain(){
                Id = 2,
                Name = "過去社員",
                SyainBaseId = syainBase.Id,
                StartYmd = today.AddDays(-20),
                EndYmd = today.AddDays(-10),
                Code = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            var syainFuture = new Syain(){
                Id = 3,
                Name = "未来社員",
                SyainBaseId = syainBase.Id,
                StartYmd = today.AddDays(10),
                EndYmd = today.AddDays(20),
                Code = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：KINGS受注
            var kings = new KingsJuchu()
            {
                Id = 1,
                ProjectNo = "PRJ-001",
                JuchuuNo = "JUCHU-001",
                JuchuuGyoNo = 1,
                SekouBumonCd = "",
                Bukken = "",
                HiyouShubetuCdName = "",
                SearchBukken = ""
            };

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new Anken(){
                Id = 100,
                Name = "既存案件",
                KokyakuKaisyaId = kokyaku.Id,
                JyutyuSyuruiId = jyutyuSyurui.Id,
                SyainBaseId = syainBase.Id,
                KingsJuchuId = kings.Id,
                Naiyou = "既存案件の内容です。",
                SearchName = "既存案件",
            };

            // シード：表示対象外の案件エンティティ
            var otherAnken = new Anken(){
                Id = 101,
                Name = "他の案件",
                SearchName = "他の案件",
            };

            // シード：ログインユーザー
            var syainLogin = new Syain()
            {
                Id = 4,
                SyainBaseId = 9999,
                BusyoCode = "",
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                KingsSyozoku = "",
            };

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
            SyainBasis syainBase = new SyainBasis()
            {
                Id = 1,
                Code = "",
            };

            var syainToday = new Syain(){
                Id = 4,
                Name = "当日社員",
                SyainBaseId = syainBase.Id,
                StartYmd = today.AddDays(startOffset),
                EndYmd = today.AddDays(endOffset),
                Code = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new Anken(){
                Id = 100,
                Name = "既存案件",
                SyainBaseId = syainBase.Id,
                SearchName = "既存案件",
            };

            // シード：ログインユーザー
            var syainLogin = new Syain()
            {
                Id = 2,
                SyainBaseId = 9999,
                BusyoCode = "",
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                KingsSyozoku = "",
            };

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
            SyainBasis syainBase = new SyainBasis()
            {
                Id = 1,
                Code = "",
            };

            var syainToday = new Syain(){
                Id = 4,
                Name = "当日社員",
                SyainBaseId = syainBase.Id,
                StartYmd = today.AddDays(startOffset),
                EndYmd = today.AddDays(endOffset),
                Code = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new Anken(){
                Id = 100,
                Name = "既存案件",
                SyainBaseId = syainBase.Id,
                SearchName = "既存案件",
                };

            // シード：ログインユーザー
            var syainLogin = new Syain()
            {
                Id = 2,
                SyainBaseId = 9999,
                BusyoCode = "",
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                KingsSyozoku = "",
            };

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
            var jyutyuSyurui = new JyutyuSyurui()
            {
                Id = 1,
                Name = "受注種類A",
                Code = "",
            };

            // シード：顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = 1,
                Name = "顧客会社A",
                Shiten = "本店",
                NameKana = "",
                Ryakusyou = "",
                SearchName = "",
                SearchNameKana = "",
            };

            // シード：責任者の社員
            var syainBase = new SyainBasis()
            {
                Id = 1,
                Code = "",
            };

            var syain = new Syain()
            {
                Id = 1,
                SyainBaseId = syainBase.Id,
                Name = "社員A",
                Code = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new Anken(){
                Id = 100,
                Name = "関連データなし案件",
                KokyakuKaisyaId = kokyaku.Id,
                JyutyuSyuruiId = jyutyuSyurui.Id,
                SyainBaseId = syainBase.Id,
                SearchName = "関連データなし案件",
            };

            // シード：ログインユーザー
            var syainLogin = new Syain()
            {
                Id = 2,
                SyainBaseId = 9999,
                BusyoCode = "",
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                KingsSyozoku = "",
            };

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
            var kokyaku = new KokyakuKaisha()
            {
                Id = 1,
                Name = "顧客会社A",
                Shiten = "本店",
                NameKana = "",
                Ryakusyou = "",
                SearchName = "",
                SearchNameKana = "",
            };

            // シード：責任者の社員
            var syainBase = new SyainBasis()
            {
                Id = 1,
                Code = "",
            };

            var syain = new Syain()
            {
                Id = 1,
                SyainBaseId = syainBase.Id,
                Name = "社員A",
                Code = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：KINGS受注
            var kings = new KingsJuchu()
            {
                Id = 1,
                ProjectNo = "PRJ-001",
                JuchuuNo = "JUCHU-001",
                JuchuuGyoNo = 1,
                SekouBumonCd = "",
                Bukken = "",
                HiyouShubetuCdName = "",
                SearchBukken = ""
            };

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new Anken(){
                Id = 100,
                Name = "関連データなし案件",
                KokyakuKaisyaId = kokyaku.Id,
                SyainBaseId = syainBase.Id,
                KingsJuchuId = kings.Id,
                SearchName = "関連データなし案件",
            };

            // シード：ログインユーザー
            var syainLogin = new Syain()
            {
                Id = 2,
                SyainBaseId = 9999,
                BusyoCode = "",
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                KingsSyozoku = "",
            };

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
            var jyutyuSyurui = new JyutyuSyurui()
            {
                Id = 1,
                Name = "受注種類A",
                Code = "",
            };

            // シード：責任者の社員
            var syainBase = new SyainBasis()
            {
                Id = 1,
                Code = "",
            };

            var syain = new Syain()
            {
                Id = 1,
                SyainBaseId = syainBase.Id,
                Name = "社員A",
                Code = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：KINGS受注
            var kings = new KingsJuchu()
            {
                Id = 1,
                ProjectNo = "PRJ-001",
                JuchuuNo = "JUCHU-001",
                JuchuuGyoNo = 1,
                SekouBumonCd = "",
                Bukken = "",
                HiyouShubetuCdName = "",
                SearchBukken = ""
            };

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new Anken(){
                Id = 100,
                Name = "関連データなし案件",
                JyutyuSyuruiId = jyutyuSyurui.Id,
                SyainBaseId = syainBase.Id,
                KingsJuchuId = kings.Id,
                SearchName = "関連データなし案件",
            };

            // シード：ログインユーザー
            var syainLogin = new Syain()
            {
                Id = 2,
                SyainBaseId = 9999,
                BusyoCode = "",
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                KingsSyozoku = "",
            };

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
            var jyutyuSyurui = new JyutyuSyurui()
            {
                Id = 1,
                Name = "受注種類A",
                Code = "",
            };

            // シード：顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = 1,
                Name = "顧客会社A",
                Shiten = "本店",
                NameKana = "",
                Ryakusyou = "",
                SearchName = "",
                SearchNameKana = "",
            };

            // シード：KINGS受注
            var kings = new KingsJuchu()
            {
                Id = 1,
                ProjectNo = "PRJ-001",
                JuchuuNo = "JUCHU-001",
                JuchuuGyoNo = 1,
                SekouBumonCd = "",
                Bukken = "",
                HiyouShubetuCdName = "",
                SearchBukken = ""
            };

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new Anken(){
                Id = 100,
                Name = "関連データなし案件",
                KokyakuKaisyaId = kokyaku.Id,
                JyutyuSyuruiId = jyutyuSyurui.Id,
                KingsJuchuId = kings.Id,
                SearchName = "関連データなし案件",
            };

            // シード：ログインユーザー
            var syainLogin = new Syain()
            {
                Id = 2,
                SyainBaseId = 9999,
                BusyoCode = "",
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                KingsSyozoku = "",
            };

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
        /// 初期表示: 存在しないID指定の場合、RedirectToPageResultが返却されることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        public async Task OnGetAsync_存在しない案件ID指定_エラーページに遷移()
        {
            // ---------- Arrange ----------
            // シード：案件エンティティ
            var ankenEntity = new Anken(){
                Id = 1,
                Name = "表示対象外案件",
                SearchName = "表示対象外案件",
                };

            // シード：ログインユーザー
            var syainLogin = new Syain()
            {
                Id = 2,
                SyainBaseId = 9999,
                BusyoCode = "",
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                KingsSyozoku = "",
            };

            // 必要データ登録
            SeedEntities(ankenEntity, syainLogin);

            var model = CreateModel(syainLogin);

            // ---------- Act ----------
            var nonexistentId = ankenEntity.Id + 1;
            var result = await model.OnGetAsync(nonexistentId, false);

            // ---------- Assert ----------
            var redirect = result as RedirectToPageResult;

            Assert.IsNotNull(redirect);
            Assert.AreEqual("/ErrorMessage", redirect.PageName);
            Assert.AreEqual(Const.ErrorSelectedDataNotExists, redirect.RouteValues?["errorMessage"]);

            var viewModel = model.IndexViewModel;

            // ViewModelの設定内容の確認
            Assert.IsNull(viewModel);

            // 案件参照履歴が登録されていないこと
            var count = await db.AnkenSansyouRirekis.CountAsync();
            Assert.AreEqual(0, count, "案件参照履歴が登録されていないこと");
        }
    }
}
