using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Utils;
using ZouryokuTest.Builder;
using ZouryokuTest.Pages.Builder;
using static Zouryoku.Pages.KokyakuJohoHyoji.IndexModel;

namespace ZouryokuTest.Pages.KokyakuJohoHyoji
{
    /// <summary>
    /// 顧客情報表示の初期表示のテスト
    /// </summary>
    [TestClass]
    public class IndexModelOnGetTests : IndexModelTestBase
    {
        // =============================================
        // 定数
        // =============================================
        /// <summary>
        /// 参照履歴テーブルに保持する最大件数
        /// </summary>
        private const int MaxHistoryCount = 50;

        // =============================================
        // 正常系テストケース
        // =============================================

        // ===========================================================
        // 初期表示
        //      パラメータ.顧客会社IDと一致する顧客情報が存在する場合
        // ===========================================================
        /// <summary>
        /// パラメータ.顧客会社IDと一致する顧客情報が取得され、PageResultが返却されることを確認
        /// </summary>
        [TestMethod(DisplayName = "パラメータ.顧客会社IDと一致する顧客情報が存在する → PageResultが返却される")]
        public async Task OnGetAsync_パラメータの顧客会社IDと一致する顧客情報が存在_PageResultを返却()
        {
            // ================ Arrange ================ //
            // 顧客会社情報の作成
            var kokyaku = CreateKokyakuKaisha(1);

            // データの登録
            SeedEntities(kokyaku);

            var model = CreateModel();

            // ================ Act ================ //
            var result = await model.OnGetAsync(1);

            // ================ Assert ================ //
            Assert.IsInstanceOfType(
                result,
                typeof(PageResult),
                "レコードが存在する場合は PageResult が返るべきです。"
                );

            Assert.IsNotNull(model.KokyakuView, "顧客情報は取得されているべきです。");
        }

        // =============================================================
        // 顧客情報取得
        //      パラメータ.顧客会社IDのデータがDBに存在する場合
        // =============================================================
        /// <summary>
        /// 登録されている顧客会社とそれに紐づく全ての情報が取得されることを確認
        /// </summary>
        [TestMethod(DisplayName = "パラメータ.顧客会社IDのデータがDBに存在する → 対象の顧客情報を全て取得")]
        public async Task OnGetAsync_パラメータの顧客会社IDと一致する顧客情報が存在_対象の顧客情報を返却()
        {
            // ================ Arrange ================ //
            // 顧客会社情報の作成
            var kokyaku = new KokyakuKaishaBuilder().WithName("AA会社")
                                    .WithNameKana("エーエーカイシャ")
                                    .WithRyakusyou("AA")
                                    .WithYuubinBangou("000-0000")
                                    .WithJyuusyo1("大阪府大阪市")
                                    .WithJyuusyo2("○○区")
                                    .WithTel("00-0000-0000")
                                    .WithFax("00-0000-0000")
                                    .WithMemo("sample")
                                    .WithUrl("test")
                                    .WithGyousyuId(1)
                                    .WithEigyoBaseSyainId(1)
                                    .Build();

            // 業種情報の作成
            var gyousyu = CreateGyousyu(1);

            // 社員Base情報の作成
            var syainBase = CreateSyainBase(1);

            // 社員情報の作成
            List<Syain> syains = new List<Syain>();
            var today = DateTime.Now.ToDateOnly();

            syains.Add(new SyainBuilder().WithId(1)
                .WithSyainBaseId(1)
                .WithName("現役社員")
                .WithBusyoId(5)
                .WithStartYmd(today.AddDays(-10))
                .WithEndYmd(today.AddDays(10))
                .Build());

            syains.Add(new SyainBuilder().WithId(2)
                .WithSyainBaseId(1)
                .WithName("過去社員")
                .WithBusyoId(5)
                .WithStartYmd(today.AddDays(-20))
                .WithEndYmd(today.AddDays(-10))
                .Build());

            syains.Add(new SyainBuilder().WithId(3)
                .WithSyainBaseId(1)
                .WithName("未来社員")
                .WithBusyoId(5)
                .WithStartYmd(today.AddDays(10))
                .WithEndYmd(today.AddDays(20))
                .Build());

            // 部署情報の作成
            List<Busyo> busyos = new List<Busyo>();

            busyos.Add(new BusyoBuilder().Build());

            busyos.Add(new BusyoBuilder().WithId(2)
                .WithName("部署B")
                .WithOyaId(1)
                .Build());

            busyos.Add(new BusyoBuilder().WithId(3)
                .WithName("部署C")
                .WithOyaId(1)
                .Build());

            busyos.Add(new BusyoBuilder().WithId(4)
                .WithName("部署D")
                .WithOyaId(3)
                .Build());

            busyos.Add(new BusyoBuilder().WithId(5)
                .WithName("部署E")
                .WithOyaId(3)
                .Build());

            // データの登録
            SeedEntities(kokyaku, gyousyu, syainBase, syains, busyos);

            var model = CreateModel();

            // ================ Act ================ //
            await model.OnGetAsync(kokyaku.Id);

            // ================ Assert ================ //
            // 顧客情報
            Assert.IsNotNull(model.KokyakuView, "顧客情報が取得できていません。");
            Assert.AreEqual(1, model.KokyakuView.Id, "ID が 1 と一致しません。");
            Assert.AreEqual("AA会社", model.KokyakuView.Name, "Name が AA会社 と一致しません。");
            Assert.AreEqual("エーエーカイシャ", model.KokyakuView.NameKana, "NameKana が エーエーカイシャ と一致しません。");
            Assert.AreEqual("AA", model.KokyakuView.Ryakusyou, "Ryakusyou が AA と一致しません。");
            Assert.AreEqual("000-0000", model.KokyakuView.YuubinnBangou, "YuubinBangou が 000-0000 と一致しません。");
            Assert.AreEqual("大阪府大阪市", model.KokyakuView.Jyuusyo1, "Jyuusyo1 が 大阪府大阪市 と一致しません。");
            Assert.AreEqual("○○区", model.KokyakuView.Jyuusyo2, "Jyuusyo2 が ○○区 と一致しません。");
            Assert.AreEqual("00-0000-0000", model.KokyakuView.Tel, "Tel が 00-0000-0000 と一致しません。");
            Assert.AreEqual("00-0000-0000", model.KokyakuView.Fax, "Fax が 00-0000-0000 と一致しません。");
            Assert.AreEqual("sample", model.KokyakuView.Memo, "Memo が sample と一致しません。");
            Assert.AreEqual("test", model.KokyakuView.Url, "Url が test と一致しません。");

            // 業種情報
            Assert.AreEqual("サンプル", model.KokyakuView.GyousyuName, "GyousyuName が サンプル と一致しません。");

            // 社員情報
            Assert.AreEqual("現役社員", model.KokyakuView.EigyouSyainName, "EigyouSyainName が 現役社員 と一致しません。");

            // 部署情報
            Assert.AreEqual(
                "部署A > 部署C > 部署E : ",
                model.EigyouSyainBusyoName,
                "EigyouSyainBusyoName が 部署A > 部署C > 部署E : と一致しません。"
                );
        }

        /// <summary>
        /// 営業担当者の有効開始日がシステム日付の当日だった場合、営業担当者名が取得されることを確認
        /// 営業担当者の有効終了日がシステム日付の当日だった場合、営業担当者名が取得されることを確認
        /// </summary>
        /// <param name="startInt">有効開始日に加える数値</param>
        /// <param name="endInt">有効終了日に加える数値</param>
        [DataRow(0, 5, DisplayName = "営業担当者.有効開始日がシステム日付の当日 → 営業担当者名が取得される")]
        [DataRow(-5, 0, DisplayName = "営業担当者.有効終了日がシステム日付の当日 → 営業担当者名が取得される")]
        [TestMethod]
        public async Task OnGetAsync_社員情報の有効日がシステム日付の当日_社員情報を返却(
            int startInt,
            int endInt)
        {
            // ================ Arrange ================ //
            // 現在の日付を取得
            var today = DateTime.Now.ToDateOnly();

            // 顧客会社情報の作成
            var kokyaku = new KokyakuKaishaBuilder().WithName("AA会社")
                                    .WithNameKana("エーエーカイシャ")
                                    .WithRyakusyou("AA")
                                    .WithYuubinBangou("000-0000")
                                    .WithJyuusyo1("大阪府大阪市")
                                    .WithJyuusyo2("○○区")
                                    .WithTel("00-0000-0000")
                                    .WithFax("00-0000-0000")
                                    .WithMemo("sample")
                                    .WithUrl("test")
                                    .WithGyousyuId(1)
                                    .WithEigyoBaseSyainId(1)
                                    .Build();

            // 社員Base情報の作成
            var syainBase = CreateSyainBase(1);

            // 社員情報の作成
            var syain = new SyainBuilder().WithId(1)
                .WithName("社員A")
                .WithStartYmd(today.AddDays(startInt))
                .WithEndYmd(today.AddDays(endInt))
                .Build();

            // データの登録
            SeedEntities(kokyaku, syainBase, syain);

            var model = CreateModel();

            // ================ Act ================ //
            await model.OnGetAsync(kokyaku.Id);

            // ================ Arrange ================ //
            Assert.AreEqual(
                "社員A",
                model.KokyakuView.EigyouSyainName,
                "EigyouSyainName が 社員A と一致しません。"
                );
        }

        // =============================================================================
        // 部署名取得
        //      パラメータ.部署ID = 部署マスタ.IDを満たす部署情報がDBに存在する場合
        // =============================================================================
        /// <summary>
        /// 親IDがNullの場合、「(部署名) : 」が返却されることを確認
        /// </summary>
        [TestMethod(DisplayName = "取得された部署の親IDがNull → 取得された部署名のみ返却")]
        public async Task OnGetAsync_パラメータの部署IDと一致する部署情報が存在_部署名を返却()
        {
            // ================ Arrange ================ //
            // 現在の日付を取得
            var today = DateTime.Now.ToDateOnly();

            // 顧客会社情報の作成
            var kokyaku = new KokyakuKaishaBuilder().WithId(1)
                .WithEigyoBaseSyainId(1)
                .Build();

            // 部署情報の作成
            var busyo = new BusyoBuilder().WithId(1)
                .WithName("部署A")
                .WithStartYmd(today.AddDays(-10))
                .WithEndYmd(today.AddDays(10))
                .WithOyaId(null)
                .WithIsActive(true)
                .Build();

            // 社員Base情報の作成
            var syainBase = CreateSyainBase(1);

            // 社員情報の作成
            var syain = new SyainBuilder().WithId(1)
                .WithName("社員A")
                .WithStartYmd(DateOnly.Parse("2025/04/01"))
                .WithEndYmd(DateOnly.Parse("9999/12/31"))
                .WithBusyoId(1)
                .WithSyainBaseId(1)
                .Build();

            // データの登録
            SeedEntities(kokyaku, busyo, syainBase, syain);

            var model = CreateModel();

            // ================ Act ================ //
            await model.OnGetAsync(1);

            // ================ Arrange ================ //
            Assert.AreEqual(
                "部署A : ",
                model.EigyouSyainBusyoName,
                "EigyouSyainBusyoName が 部署A : と一致しません。"
                );
        }

        // ==============================================
        // 登録済みの顧客会社参照履歴が存在する場合
        // ==============================================
        /// <summary>
        /// 更新対象のレコードが存在する場合、参照履歴の参照時間を更新する
        ///         参照履歴を更新しても全ての履歴を保持する。
        /// </summary>
        [TestMethod(DisplayName = "更新対象のレコードが存在する → 対象の参照時間を現在日時に更新")]
        public async Task OnGetAsync_履歴が存在する_参照時間を更新()
        {
            // ================ Arrange ================ //
            // 社員情報の作成
            var syainBase = CreateSyainBase(1);
            var busyo = CreateBusyo(1);

            // ログインユーザーの顧客会社参照履歴の作成（複数件）

            var idCount = MaxHistoryCount - 5;
            var userRirekis = CreateKokyakuKaisyaSansyouRireki(LoggedInUserId, idCount);

            // 顧客会社情報の作成
            var kokyaku = CreateKokyakuKaisha(idCount);

            // 別ユーザーの顧客会社参照履歴の作成（複数件）
            var otherUserRirekis = CreateOtherKokyakuKaisyaSansyouRireki(0, idCount + 1);

            // データ登録
            SeedEntities(syainBase, kokyaku, busyo, userRirekis, otherUserRirekis);

            var model = CreateModel();

            // ================ Act ================ //
            // 処理実行前の現在日時の取得
            var beforeActTime = DateTime.Now;

            await model.OnGetAsync(kokyaku.Id);

            // 処理実行後の現在日時の取得
            var afterActTime = DateTime.Now;

            // ================ Assert ================ //
            // 確認対象の顧客会社参照履歴を取得
            var targetRireki = await db.KokyakuKaisyaSansyouRirekis
                .SingleOrDefaultAsync(x => x.Id == idCount);

            Assert.IsNotNull(targetRireki, "テストデータが作成されていません。");

            AssertSansyouTime(targetRireki, beforeActTime, afterActTime);

            // 件数が増加していないことを確認
            var allBeforeCount = idCount + otherUserRirekis.Count;
            var allAfterCount = await db.KokyakuKaisyaSansyouRirekis.CountAsync();
            Assert.AreEqual(allBeforeCount, allAfterCount, "顧客参照履歴が追加されています。");

            // 他の参照履歴に対して更新処理が実行されていないか確認
            var others = await db.KokyakuKaisyaSansyouRirekis
                .Where(x => x.Id != idCount)
                .AsNoTracking()
                .ToListAsync();

            AssertOtherRirekiNotUpdated(others, beforeActTime, afterActTime);
        }

        // =============================================
        // 異常系テストケース
        // =============================================

        // ===========================================================================================
        // 初期表示
        //      パラメータ.部署ID = 部署マスタ.IDを満たす部署情報がDBに存在しない場合
        // ===========================================================================================
        /// <summary>
        /// パラメータ.顧客会社IDと一致する顧客情報が存在しない場合、NotFoundが返却されることを確認
        /// </summary>
        /// <param name="kokyakuId">顧客会社ID</param>
        [TestMethod(DisplayName = "パラメータの顧客会社IDと一致する顧客情報が存在しない → エラーメッセージ が返却される")]
        public async Task OnGetAsync_パラメータの顧客会社IDと一致する顧客情報が存在しない_エラーメッセージを返却()
        {
            // ================ Arrange ================ //
            // 顧客会社情報の作成
            var kokyaku = CreateKokyakuKaisha(5);

            // データの登録
            SeedEntities(kokyaku);

            var model = CreateModel();

            // ================ Act ================ //
            var result = await model.OnGetAsync(1);

            // ================ Assert ================ //
            // ModelStateにエラーが設定されていること
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsNotNull(model.ModelState[string.Empty], "ModelStateにキーがemptyのエラーが存在するはずです。");

            // エラーメッセージの確認
            var messages = model.ModelState[string.Empty]!.Errors.Select(e => e.ErrorMessage).ToList();
            Assert.HasCount(1, messages, "ModelStateにはエラーが1件設定されているはずです。");
            Assert.AreEqual(Const.ErrorSelectedDataNotExists, messages[0], "エラーメッセージが一致しません。");
        }

        // =================================================================
        // 顧客情報取得
        //      パラメータ.顧客会社IDのデータがDBに存在する場合
        // =================================================================
        /// <summary>
        /// 営業担当者の有効開始日がシステム日付より未来だった場合、営業担当者名が空文字で取得されることを確認
        /// 営業担当者の有効終了日がシステム日付より過去だった場合、営業担当者名が空文字で取得されることを確認
        /// </summary>
        /// <param name="startInt">有効開始日に加える数値</param>
        /// <param name="endInt">有効終了日に加える数値</param>
        [DataRow(1, 2, DisplayName = "営業担当者の有効開始日がシステム日付より未来だった場合、営業担当者名が空文字で取得される")]
        [DataRow(-2, -1, DisplayName = "営業担当者の有効終了日がシステム日付より過去だった場合、営業担当者名が空文字で取得される")]
        [TestMethod]
        public async Task OnGetAsync_営業担当社員の社員情報の有効期限が不正_営業担当者名が空文字で返却(
            int startInt,
            int endInt)
        {
            // ================ Arrange ================ //
            // 現在の日付を取得
            var today = DateTime.Now.ToDateOnly();

            // 顧客会社情報の作成
            var kokyaku = new KokyakuKaishaBuilder().WithName("AA会社")
                                    .WithNameKana("エーエーカイシャ")
                                    .WithRyakusyou("AA")
                                    .WithYuubinBangou("000-0000")
                                    .WithJyuusyo1("大阪府大阪市")
                                    .WithJyuusyo2("○○区")
                                    .WithTel("00-0000-0000")
                                    .WithFax("00-0000-0000")
                                    .WithMemo("sample")
                                    .WithUrl("test")
                                    .WithGyousyuId(1)
                                    .WithEigyoBaseSyainId(1)
                                    .Build();

            // 社員Base情報の作成
            var syainBase = CreateSyainBase(1);

            // 社員情報の作成
            var syain = new SyainBuilder().WithId(1)
                .WithName("社員A")
                .WithStartYmd(today.AddDays(startInt))
                .WithEndYmd(today.AddDays(endInt))
                .Build();

            // データの登録
            SeedEntities(kokyaku, syainBase, syain);

            var model = CreateModel();

            // ================ Act ================ //
            await model.OnGetAsync(kokyaku.Id);

            // ================ Arrange ================ //
            Assert.IsNull(model.KokyakuView.EigyouSyainName, "EigyouSyainName が取得されています。");
        }

        /// <summary>
        /// 業種IDがNullだった場合、業種名称が空文字で取得されることを確認
        /// </summary>
        [TestMethod(DisplayName = "業種IDがNullだった場合、業種名称が空文字で取得される")]
        public async Task OnGetAsync_取得した顧客情報に業種IDが登録されていない_業種名が空文字で返却()
        {
            // ================ Arrange ================ //
            // 顧客会社情報の作成
            var kokyaku = new KokyakuKaishaBuilder().WithName("AA会社")
                                    .WithNameKana("エーエーカイシャ")
                                    .WithRyakusyou("AA")
                                    .WithYuubinBangou("000-0000")
                                    .WithJyuusyo1("大阪府大阪市")
                                    .WithJyuusyo2("○○区")
                                    .WithTel("00-0000-0000")
                                    .WithFax("00-0000-0000")
                                    .WithMemo("sample")
                                    .WithUrl("test")
                                    .WithGyousyuId(null)
                                    .WithEigyoBaseSyainId(1)
                                    .Build();

            // 業種情報の作成
            var gyousyu = CreateGyousyu(1);

            // データの登録
            SeedEntities(kokyaku, gyousyu);

            var model = CreateModel();

            // ================ Act ================ //
            await model.OnGetAsync(kokyaku.Id);

            // ================ Assert ================ //
            Assert.IsNull(model.KokyakuView.GyousyuName, "GyousyuName が取得されています。");
        }

        /// <summary>
        /// 営業社員BaseIDがNullだった場合、営業社員情報が取得されないことを確認
        /// </summary>
        [TestMethod(DisplayName = "営業社員BaseIDがNullだった場合、営業社員情報が取得されない")]
        public async Task OnGetAsync_取得した顧客情報に営業社員BaseIDが登録されていない_営業社員情報が取得されない()
        {
            // ================ Arrange ================ //
            // 顧客会社情報の作成
            var kokyaku = new KokyakuKaishaBuilder().WithName("AA会社")
                                    .WithNameKana("エーエーカイシャ")
                                    .WithRyakusyou("AA")
                                    .WithYuubinBangou("000-0000")
                                    .WithJyuusyo1("大阪府大阪市")
                                    .WithJyuusyo2("○○区")
                                    .WithTel("00-0000-0000")
                                    .WithFax("00-0000-0000")
                                    .WithMemo("sample")
                                    .WithUrl("test")
                                    .WithGyousyuId(1)
                                    .WithEigyoBaseSyainId(null)
                                    .Build();

            // 営業社員情報の作成
            var syainBase = CreateSyainBase(1);
            var syain = CreateSyain(1);

            // データの登録
            SeedEntities(kokyaku, syainBase, syain);

            var model = CreateModel();

            // ================ Act ================ //
            await model.OnGetAsync(kokyaku.Id);

            // ================ Assert ================ //
            Assert.IsNull(model.KokyakuView.EigyouSyainBusyoId, "EigyouSyainBusyoId がNullと一致しません。");
            Assert.IsNull(model.KokyakuView.EigyouSyainName, "EigyouSyainName が取得されています。");
            Assert.IsNotNull(model.EigyouSyainBusyoName);
            Assert.IsEmpty(model.EigyouSyainBusyoName, "EigyouSyainBusyoName が取得されています。");
        }

        // =============================================================
        // 顧客情報取得
        //      パラメータ.顧客会社IDのデータがDBに存在しない場合
        // =============================================================
        /// <summary>
        /// 顧客情報が取得されないことを確認
        /// </summary>
        [TestMethod(DisplayName = "パラメータ.顧客会社IDのデータがDBに存在しない → 顧客情報が取得されない")]
        public async Task OnGetAsync_パラメータの顧客会社IDと一致する顧客情報が存在しない_顧客情報が取得されない()
        {
            // ================ Arrange ================ //
            var model = CreateModel();

            // ================ Act ================ //
            await model.OnGetAsync(1);

            // ================ Assert ================ //
            Assert.IsNotNull(model.KokyakuView);
            Assert.IsNull(model.KokyakuView.Id);
            Assert.IsNull(model.KokyakuView.Code);
            Assert.IsNull(model.KokyakuView.Name);
            Assert.IsNull(model.KokyakuView.NameKana);
            Assert.IsNull(model.KokyakuView.Ryakusyou);
            Assert.IsNull(model.KokyakuView.Shiten);
            Assert.IsNull(model.KokyakuView.YuubinnBangou);
            Assert.IsNull(model.KokyakuView.Jyuusyo1);
            Assert.IsNull(model.KokyakuView.Jyuusyo2);
            Assert.IsNull(model.KokyakuView.Tel);
            Assert.IsNull(model.KokyakuView.Fax);
            Assert.IsNull(model.KokyakuView.Memo);
            Assert.IsNull(model.KokyakuView.Url);
            Assert.IsNull(model.KokyakuView.GyousyuName);
            Assert.IsNull(model.KokyakuView.EigyouSyainName);
            Assert.IsNull(model.KokyakuView.EigyouSyainBusyoId);
        }
    }
}
