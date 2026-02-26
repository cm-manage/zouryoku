using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Zouryoku.Utils;
using ZouryokuTest.Builder;
using ZouryokuTest.Pages.Builder;
using static Model.Enums.ContractClassification;

namespace ZouryokuTest.Pages.JuchuJohoHyoji
{
    /// <summary>
    /// JuchuJohoHyoji/Index（受注表示ページ）のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetAsyncTests : IndexModelTestBase
    {
        // =============================================
        // 正常系テストケース
        // =============================================

        // ===========================================================
        // 初期表示
        //      パラメータ.受注IDと一致する受注情報が存在する場合
        // ===========================================================
        /// <summary>
        /// PageResultが返却されることを確認
        /// </summary>
        [TestMethod(DisplayName = "パラメータ.受注IDと一致する受注情報が存在する → PageResultが返却される")]
        public async Task OnGetAsync_パラメータの受注IDと一致する受注情報が存在する_PageResultを返却()
        {
            // ================ Arrange ================ //
            // 受注情報の作成
            var juchu = CreateKingsJuchu(1);

            // 部署情報の作成
            var busyo = CreateBusyo(1);

            // データの登録
            SeedEntities(juchu, busyo);

            var model = CreateModel();

            // ================ Act ================ //
            var result = await model.OnGetAsync(1);

            // ================ Assert ================ //
            Assert.IsInstanceOfType(
                result,
                typeof(PageResult),
                "レコードが存在する場合は PageResult が返るべきです。"
                );

            Assert.IsNotNull(model.JuchuView, "受注情報は取得されているべきです。");
        }

        // ==========================================================
        // 初期処理 受注情報取得
        //      パラメータ.受注IDのデータがDBに存在する場合
        // ==========================================================
        /// <summary>
        /// 受注情報が返却されることを確認
        /// </summary>
        [TestMethod(DisplayName = "パラメータの受注IDと一致する受注情報が存在する → 受注情報が返却される")]
        public async Task OnGetAsync_パラメータの受注IDと一致する受注情報が存在する_受注情報を返却()
        {
            // ================ Arrange ================ //
            var now = new DateTime(2026, 2, 1, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);

            // 受注情報の作成
            var juchu = new KingsJuchuBuilder()
                .WithProjectNo("00000")
                .WithJuchuuNo("11111")
                .WithJuchuuGyoNo(1)
                .WithKeiyakuJoutaiKbnName("契約区分")
                .WithJucYmd(DateOnly.Parse("2025/01/01"))
                .WithKeiNm("契約名")
                .WithJucNm("受注名")
                .WithBukken("物件")
                .WithShouhinName("商品名")
                .WithHiyouShubetuCdName("費用種別名")
                .WithJucKin(10000)
                .WithOkrTanNm1("送担当者名")
                .WithUkeTanNm1("受担当者名")
                .WithChaYmd(DateOnly.Parse("2025/01/02"))
                .WithNsyYmd(DateOnly.Parse("2025/01/03"))
                .WithKurYmd(DateOnly.Parse("2025/01/04"))
                .WithKnyYmd(DateOnly.Parse("2025/01/05"))
                .WithIsGenkaToketu(false)
                .WithToketuYmd(DateOnly.Parse("2025/01/06"))
                .WithBiko("備考")
                .WithKeiyakuJoutaiKbn(受注_共同)
                .WithBusyoId(1)
                .Build();

            // 部署情報の作成
            var today = fakeTimeProvider.Today();

            var busyo = new BusyoBuilder()
                .WithName("部署A")
                .WithStartYmd(today.AddDays(-10))
                .WithEndYmd(today.AddDays(10))
                .WithIsActive(true)
                .Build();

            // データの登録
            SeedEntities(juchu, busyo);

            var model = CreateModel();

            // ================ Act ================ //
            await model.OnGetAsync(1);

            // ================ Assert ================ //
            // 受注情報
            Assert.IsNotNull(model.JuchuView, "受注情報は取得されているべきです。");
            Assert.AreEqual("00000", model.JuchuView.ProjectNo, "ProjectNo が 00000 と一致しません。");
            Assert.AreEqual("11111", model.JuchuView.JuchuNo, "JuchuNo が 11111 と一致しません。");
            Assert.AreEqual("1", model.JuchuView.JuchuGyoBangou, "JuchuGyoBangou が 1 と一致しません。");
            Assert.AreEqual("契約区分", model.JuchuView.KeiyakuJotai, "KeiyakuJotai が 契約区分 と一致しません。");
            Assert.AreEqual("2025/01/01", model.JuchuView.JuchuYmd, "JuchuYmd が 2025/01/01 と一致しません。");
            Assert.AreEqual("契約名", model.JuchuView.KeiNm, "KeiNm が 契約名 と一致しません。");
            Assert.AreEqual("受注名", model.JuchuView.JuchuNm, "JuchuNm が 受注名 と一致しません。");
            Assert.AreEqual("物件", model.JuchuView.Bukken, "Bukken が 物件 と一致しません。");
            Assert.AreEqual("商品名", model.JuchuView.ShohinName, "ShohinName が 商品名 と一致しません。");
            Assert.AreEqual("費用種別名", model.JuchuView.HiyoShubetuName, "HiyoShubetuName が 費用種別名 と一致しません。");
            Assert.AreEqual("10,000", model.JuchuView.JuchuKin, "JuchuKin が 10,000 と一致しません。");
            Assert.AreEqual("送担当者名", model.JuchuView.OkrTanName, "OkrTanName が 送担当者名 と一致しません。");
            Assert.AreEqual("受担当者名", model.JuchuView.UkTanName, "UkTanName が 受担当者名 と一致しません。");
            Assert.AreEqual("2025/01/02", model.JuchuView.ChaYmd, "ChaYmd が 2025/01/02 と一致しません。");
            Assert.AreEqual("2025/01/03", model.JuchuView.NsyYmd, "NsyYmd が 2025/01/03 と一致しません。");
            Assert.AreEqual("2025/01/04", model.JuchuView.KurYmd, "KurYmd が 2025/01/04 と一致しません。");
            Assert.AreEqual("2025/01/05", model.JuchuView.KnyYmd, "KnyYmd が 2025/01/05 と一致しません。");
            Assert.AreEqual("未", model.JuchuView.GenkaToketu, "GenkaToketu が 未 と一致しません。");
            Assert.AreEqual("2025/01/06", model.JuchuView.ToketuYmd, "ToketuYmd が 2025/01/06 と一致しません。");
            Assert.AreEqual("備考", model.JuchuView.Biko, "Biko が 備考 と一致しません。");
            Assert.AreEqual(受注_共同, model.JuchuView.KeiyakuJoutaiKbn, "KeiyakuJoutaiKbn が 受注_共同 と一致しません。");

            // 部署情報
            Assert.AreEqual("部署A", model.JuchuView.SekoBumon, "SekoBumon が 部署A と一致しません。");
        }

        // ==========================================================
        // 条件
        // ①: パラメータ.受注ID = 受注参照履歴.受注ID
        // ②: ログインユーザー.社員BaseID = 受注参照履歴.社員BaseID
        // ======================================================
        // ①かつ②の条件を満たす受注参照履歴が存在する場合
        // ======================================================
        /// <summary>
        /// 受注参照履歴.参照時間が現在日時に更新されることを確認
        /// </summary>
        [TestMethod(DisplayName = "表示されている受注情報のID、ログインユーザーの社員BaseIDの受注参照履歴が存在する → 受注参照履歴の参照時間が現在日時に更新される")]
        public async Task OnGetAsync_対象の受注参照履歴が存在する_参照時間を現在日時に更新()
        {
            // ================ Arrange ================ //
            var now = new DateTime(2026, 2, 1, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);

            // 受注情報の作成
            var juchu = CreateKingsJuchu(1);

            // 部署情報の作成
            var busyo = CreateBusyo(1);

            // 受注参照履歴の作成
            var juchuSansyouRireki = new KingsJuchuSansyouRirekiBuilder()
                .WithId(1)
                .WithKingsJuchuId(juchu.Id)
                .WithSyainBaseId(LoggedInUserId)
                .WithSansyouTime(new DateTime(2026, 1, 1, 19, 00, 00))
                .Build();

            var juchuOtherSansyouRireki = new KingsJuchuSansyouRirekiBuilder()
                .WithId(2)
                .WithKingsJuchuId(juchu.Id)
                .WithSyainBaseId(LoggedInUserId)
                .WithSansyouTime(new DateTime(2026, 1, 1, 10, 00, 00))
                .Build();

            // データ登録
            SeedEntities(juchu, busyo, juchuSansyouRireki, juchuOtherSansyouRireki);

            var model = CreateModel();

            // ================ Act ================ //
            // 処理実行前の受注参照履歴の件数を取得
            var allBeforeCount = await db.KingsJuchuSansyouRirekis.CountAsync();

            await model.OnGetAsync(juchu.Id);

            // ================ Assert ================ //
            // 確認対象の受注参照履歴を取得
            var targetRireki = await db.KingsJuchuSansyouRirekis.FirstOrDefaultAsync(x => x.Id == juchu.Id);
            Assert.IsNotNull(targetRireki, "テストデータが作成されていません。");
            AssertSansyouTime(targetRireki, now);

            // 件数が増加していないことを確認
            var allAfterCount = await db.KingsJuchuSansyouRirekis.CountAsync();
            Assert.AreEqual(allBeforeCount, allAfterCount, "受注参照履歴が追加されています。");

            // 他の参照履歴に対して更新処理が実行されていないか確認
            var others = await db.KingsJuchuSansyouRirekis.Where(x => x.Id != juchu.Id).AsNoTracking().ToListAsync();
            AssertOtherRirekiNotUpdated(others, now);
        }

        // =============================================
        // 異常系テストケース
        // =============================================

        // =============================================================
        // 初期表示
        //      パラメータ.受注IDと一致する受注情報が存在しない場合
        // =============================================================
        /// <summary>
        /// パラメータ.受注IDと一致する受注情報が存在しない場合、RedirectToPageResultが返却されることを確認
        /// </summary>
        [TestMethod(DisplayName = "パラメータ.受注IDと一致する受注情報が存在しない → エラーページに遷移する")]
        public async Task OnGetAsync_受注情報が取得されなかった場合_エラーページに遷移()
        {
            // ================ Arrange ================ //
            // 受注情報の作成
            var juchu = CreateKingsJuchu(1);

            // 部署情報の作成
            var busyo = CreateBusyo(1);

            // データの登録
            SeedEntities(juchu, busyo);

            var model = CreateModel();

            // ================ Act ================ //
            var result = await model.OnGetAsync(2);

            // ================ Assert ================ //
            var redirect = result as RedirectToPageResult;

            Assert.IsNotNull(redirect);
            Assert.AreEqual("/ErrorMessage", redirect.PageName);
            Assert.AreEqual(Const.ErrorSelectedDataNotExists, redirect.RouteValues?["errorMessage"]);
        }
    }
}
