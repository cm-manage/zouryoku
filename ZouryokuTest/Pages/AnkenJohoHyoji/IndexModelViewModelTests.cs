using Model.Model;

namespace ZouryokuTest.Pages.AnkenJohoHyoji
{
    [TestClass]
    public class IndexModelViewModelTests : IndexModelTestsBase
    {
        // -----------------------------------------------------
        // 定数
        // -----------------------------------------------------
        /// <summary>
        /// 項目非表示用クラス名
        /// </summary>
        private const string HideClass = "d-none";

        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------

        // =================================================================
        /// <summary>
        /// 初期表示: 案件情報を指定した時、関連データが正しく設定されることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        public void ViewModel_案件情報を指定_関連データ設定()
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

            // シード：責任者の社員（Syain）および SyainBase を作成
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
                Name = "既存案件",
                KokyakuKaisyaId = kokyaku.Id,
                JyutyuSyuruiId = jyutyuSyurui.Id,
                SyainBaseId = syainBase.Id,
                KingsJuchuId = kings.Id,
                Naiyou = "既存案件の内容です。",
                };

            // 関連データの設定
            ankenEntity.JyutyuSyurui = jyutyuSyurui;
            ankenEntity.KokyakuKaisya = kokyaku;
            ankenEntity.SyainBase = syainBase;
            ankenEntity.SyainBase.Syains = [syain];
            ankenEntity.KingsJuchu = kings;

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

            // ---------- Act ----------
            var viewModel = CreateViewModel(false, ankenEntity, syainLogin);

            // ---------- Assert ----------
            // 案件情報の確認
            Assert.AreEqual(ankenEntity.Id, viewModel.Id, "案件IDが一致しません。");
            Assert.AreEqual(ankenEntity.KingsJuchuId, viewModel.KingsJuchuId, "KINGS受注IDが一致しません。");
            Assert.AreEqual(ankenEntity.KokyakuKaisyaId, viewModel.KokyakuKaisyaId, "顧客会社IDが一致しません。");
            Assert.AreEqual(kings.KingsJuchuNo, viewModel.DispJuchuNo, "表示用受注Noが一致しません。");
            Assert.AreEqual(kings.Bukken, viewModel.Bukken, "件名が一致しません。");
            Assert.AreEqual(ankenEntity.Name, viewModel.AnkenName, "案件名が一致しません。");
            Assert.AreEqual(jyutyuSyurui.Name, viewModel.JyutyuSyuruiName, "受注種類名が一致しません。");
            Assert.AreEqual(syain.Name, viewModel.SyainName, "担当社員名が一致しません。");
            Assert.AreEqual(ankenEntity.Naiyou, viewModel.Naiyou, "案件内容が一致しません。");

            // リンク表示フラグの確認
            Assert.AreEqual(string.Empty, viewModel.KingsJuchuLinkClass, "KINGS受注リンクのクラスが一致しません。");
            Assert.AreEqual(string.Empty, viewModel.KokyakuKaisyaLinkClass, "顧客会社リンクのクラスが一致しません。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: KINGS受注IDがNULLの場合、KINGS受注リンク表示スタイルにd-noneが設定されていることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        public void ViewModel_KINGS受注IDがNULL_KINGS受注リンク表示スタイルにd_noneを設定()
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
                };

            // 関連データの設定
            ankenEntity.JyutyuSyurui = jyutyuSyurui;
            ankenEntity.KokyakuKaisya = kokyaku;
            ankenEntity.SyainBase = syainBase;
            ankenEntity.SyainBase.Syains = [syain];

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

            // ---------- Act ----------
            var viewModel = CreateViewModel(false, ankenEntity, syainLogin);

            // ---------- Assert ----------
            // KINGS受注表示リンクの確認
            Assert.AreEqual(HideClass, viewModel.KingsJuchuLinkClass, "KINGS受注リンクのクラスが一致しません。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: 顧客会社IDがNULLの場合、顧客会社表示リンク表示スタイルにd-noneが設定されていることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        public void ViewModel_顧客会社IDがNULL_顧客会社表示リンク表示スタイルにd_noneを設定()
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
                };

            // 関連データの設定
            ankenEntity.JyutyuSyurui = jyutyuSyurui;
            ankenEntity.SyainBase = syainBase;
            ankenEntity.SyainBase.Syains = [syain];
            ankenEntity.KingsJuchu = kings;

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

            // ---------- Act ----------
            var viewModel = CreateViewModel(false, ankenEntity, syainLogin);

            // ---------- Assert ----------
            // 関連データが設定されていないこと
            Assert.IsNull(viewModel.KokyakuName, "顧客情報はNULLのはずです。");

            // 顧客会社表示リンクの確認
            Assert.AreEqual(HideClass, viewModel.KokyakuKaisyaLinkClass, "顧客会社リンクのクラスが一致しません。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: 顧客会社の支店がNULLの場合、顧客会社名のみ設定されていることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        public void ViewModel_顧客会社の支店がNULL_顧客会社名のみ設定()
        {
            // ---------- Arrange ----------
            // シード：顧客会社（支店なし）
            var kokyaku = new KokyakuKaisha(){
                Id = 1,
                Name = "顧客A",
                Shiten = null,
                };

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new Anken(){
                Id = 100,
                Name = "案件A",
                KokyakuKaisyaId = kokyaku.Id,
                };

            // 関連データの設定
            ankenEntity.KokyakuKaisya = kokyaku;

            // シード：ログインユーザー
            var syainLogin = new Syain()
            {
                Id = 1,
                SyainBaseId = 9999,
                BusyoCode = "",
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                KingsSyozoku = "",
            };

            // ---------- Act ----------
            var viewModel = CreateViewModel(false, ankenEntity, syainLogin);

            // ---------- Assert ----------
            // 顧客会社名の確認
            Assert.AreEqual(kokyaku.Name, viewModel.KokyakuName, "顧客会社名が一致しません。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: 顧客会社の支店が設定されている場合、顧客会社名が正しく設定されていることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        public void ViewModel_顧客会社の支店が設定_顧客会社名が正しく設定()
        {
            // ---------- Arrange ----------
            // シード：顧客会社（支店あり）
            var kokyaku = new KokyakuKaisha(){
                Id = 1,
                Name = "顧客A",
                Shiten = "支店B",
                };

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new Anken(){
                Id = 100,
                Name = "案件A",
                KokyakuKaisyaId = kokyaku.Id,
                };

            // 関連データの設定
            ankenEntity.KokyakuKaisya = kokyaku;

            // シード：ログインユーザー
            var syainLogin = new Syain()
            {
                Id = 1,
                SyainBaseId = 9999,
                BusyoCode = "",
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                KingsSyozoku = "",
            };

            // ---------- Act ----------
            var viewModel = CreateViewModel(false, ankenEntity, syainLogin);

            // ---------- Assert ----------
            // 顧客会社名の確認
            var expectedKokyakuName = $"{kokyaku.Name} {kokyaku.Shiten}";
            Assert.AreEqual(expectedKokyakuName, viewModel.KokyakuName, "顧客会社名が一致しません。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: 条件を満たした場合、編集ボタンのスタイルクラスにEmptyが設定されることを確認
        /// 1．CanAdd = True
        /// 2. KINGS受注登録.施工部門コード = ログインユーザー.部署番号
        /// </summary>
        // =================================================================
        [TestMethod]
        public void ViewModel_登録可能かつ部署が一致_編集ボタンのスタイルクラスにEmptyを設定()
        {
            // ---------- Arrange ----------
            // シード：KINGS受注
            var kings = new KingsJuchu()
            {
                Id = 1,
                ProjectNo = "PRJ-001",
                JuchuuNo = "JUCHU-001",
                JuchuuGyoNo = 1,
                SekouBumonCd = "001",
                Bukken = "",
                HiyouShubetuCdName = "",
                SearchBukken = ""
            };

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new Anken(){
                Id = 100,
                Name = "案件A",
                KingsJuchuId = kings.Id,
                };

            // 関連データの設定
            ankenEntity.KingsJuchu = kings;

            // シード：ログインユーザー
            var syainLogin = new Syain()
            {
                Id = 1,
                SyainBaseId = 9999,
                BusyoCode = "001",
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                KingsSyozoku = "",
            };

            // ---------- Act ----------
            var viewModel = CreateViewModel(true, ankenEntity, syainLogin);

            // ---------- Assert ----------
            // 編集ボタンのクラスの確認
            Assert.AreEqual(string.Empty, viewModel.EditButtonClass, "編集ボタンのクラスがEmptyのはずです。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: 条件を満たしていない場合、編集ボタンのスタイルクラスにd-noneが設定されていることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        [DataRow(false, "001", "001", DisplayName = "登録不可かつ部署が一致の場合")]
        [DataRow(true, "001", "002", DisplayName = "登録可能かつ部署が不一致の場合")]
        [DataRow(false, "001", "002", DisplayName = "登録不可かつ部署が不一致の場合")]
        public void ViewModel_登録不可または部署が不一致_編集ボタンのスタイルクラスにd_noneを設定(bool canAdd, string ankenBusyoCode, string loginBusyoCode)
        {
            // ---------- Arrange ----------
            // シード：KINGS受注
            var kings = new KingsJuchu()
            {
                Id = 1,
                ProjectNo = "PRJ-001",
                JuchuuNo = "JUCHU-001",
                JuchuuGyoNo = 1,
                SekouBumonCd = ankenBusyoCode,
                Bukken = "",
                HiyouShubetuCdName = "",
                SearchBukken = ""
            };

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new Anken(){
                Id = 100,
                Name = "案件A",
                KingsJuchuId = kings.Id,
                };

            // 関連データの設定
            ankenEntity.KingsJuchu = kings;

            // シード：ログインユーザー
            var syainLogin = new Syain()
            {
                Id = 1,
                SyainBaseId = 9999,
                BusyoCode = loginBusyoCode,
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                KingsSyozoku = "",
            };

            // ---------- Act ----------
            var viewModel = CreateViewModel(canAdd, ankenEntity, syainLogin);

            // ---------- Assert ----------
            // 編集ボタンのクラスの確認
            Assert.AreEqual("d-none", viewModel.EditButtonClass, "編集ボタンのクラスがd-noneのはずです。");
        }

        // =================================================================
        /// <summary>
        /// 初期表示: KINGS受注IDがNULLの場合、編集ボタンのスタイルクラスにd-noneが設定されていることを確認
        /// </summary>
        // =================================================================
        [TestMethod]
        public void ViewModel_KINGS受注IDがNULL_編集ボタンのスタイルクラスにd_noneを設定()
        {
            // ---------- Arrange ----------
            // シード：案件エンティティ（表示対象）
            var ankenEntity = new Anken(){
                Id = 100,
                Name = "案件A",
                };

            // シード：ログインユーザー
            var syainLogin = new Syain()
            {
                Id = 1,
                SyainBaseId = 9999,
                BusyoCode = "",
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                KingsSyozoku = "",
            };

            // ---------- Act ----------
            var viewModel = CreateViewModel(true, ankenEntity, syainLogin);

            // ---------- Assert ----------
            // 編集ボタンのクラスの確認
            Assert.AreEqual("d-none", viewModel.EditButtonClass, "編集ボタンのクラスがd-noneのはずです。");
        }
    }
}
