using ZouryokuTest.Builder;

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
            var jyutyuSyurui = CreateJyutyuSyurui(1);

            // シード：顧客会社
            var kokyaku = CreateKokyakuKaisha(1);

            // シード：責任者の社員（Syain）および SyainBase を作成
            var syainBase = CreateSyainBasis(1);
            var syain = CreateSyain(1, syainBase.Id);

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

            // 関連データの設定
            ankenEntity.JyutyuSyurui = jyutyuSyurui;
            ankenEntity.KokyakuKaisya = kokyaku;
            ankenEntity.SyainBase = syainBase;
            ankenEntity.SyainBase.Syains = [syain];
            ankenEntity.KingsJuchu = kings;

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(4);

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

            // 関連データの設定
            ankenEntity.JyutyuSyurui = jyutyuSyurui;
            ankenEntity.KokyakuKaisya = kokyaku;
            ankenEntity.SyainBase = syainBase;
            ankenEntity.SyainBase.Syains = [syain];

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(2);

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

            // 関連データの設定
            ankenEntity.JyutyuSyurui = jyutyuSyurui;
            ankenEntity.SyainBase = syainBase;
            ankenEntity.SyainBase.Syains = [syain];
            ankenEntity.KingsJuchu = kings;

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(2);

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
            var kokyaku = new KokyakuKaishaBuilder()
                .WithId(1)
                .WithName("顧客A")
                .WithShiten(null)
                .Build();

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("案件A")
                .WithKokyakuKaisyaId(kokyaku.Id)
                .Build();

            // 関連データの設定
            ankenEntity.KokyakuKaisya = kokyaku;

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(1);

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
            var kokyaku = new KokyakuKaishaBuilder()
                .WithId(1)
                .WithName("顧客A")
                .WithShiten("支店B")
                .Build();

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("案件A")
                .WithKokyakuKaisyaId(kokyaku.Id)
                .Build();

            // 関連データの設定
            ankenEntity.KokyakuKaisya = kokyaku;

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(1);

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
            var kings = CreateKingsJuchu(1, "001");

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("案件A")
                .WithKingsJuchuId(kings.Id)
                .Build();

            // 関連データの設定
            ankenEntity.KingsJuchu = kings;

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(1, "001");

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
        public void ViewModel_登録不可または部署が不一致_編集ボタンのスタイルクラスにd_noneを設定(bool canAdd, string? ankenBusyoCode, string loginBusyoCode)
        {
            // ---------- Arrange ----------
            // シード：KINGS受注
            var kings = CreateKingsJuchu(1, ankenBusyoCode);

            // シード：案件エンティティ（表示対象）
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("案件A")
                .WithKingsJuchuId(kings.Id)
                .Build();

            // 関連データの設定
            ankenEntity.KingsJuchu = kings;

            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(1, loginBusyoCode);

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
            var ankenEntity = new AnkenBuilder()
                .WithId(100)
                .WithName("案件A")
                .Build();
            
            // シード：ログインユーザー
            var syainLogin = CreateSyainLogin(1);

            // ---------- Act ----------
            var viewModel = CreateViewModel(true, ankenEntity, syainLogin);

            // ---------- Assert ----------
            // 編集ボタンのクラスの確認
            Assert.AreEqual("d-none", viewModel.EditButtonClass, "編集ボタンのクラスがd-noneのはずです。");
        }
    }
}
