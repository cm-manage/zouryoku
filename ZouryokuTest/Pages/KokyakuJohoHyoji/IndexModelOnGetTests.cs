using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using System.Globalization;
using Zouryoku.Utils;
using static Model.Enums.BusinessTripRole;
using static Model.Enums.EmployeeAuthority;

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
            var kokyaku = new KokyakuKaisha()
            {
                Id = 1,
                Code = 100,
                Name = "A会社",
                NameKana = "エーカイシャ",
                Ryakusyou = "A",
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

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
            var kokyaku = new KokyakuKaisha()
            {
                Code = 100,
                Name = "AA会社",
                NameKana = "エーエーカイシャ",
                Ryakusyou = "AA",
                YuubinBangou = "000-0000",
                Jyuusyo1 = "大阪府大阪市",
                Jyuusyo2 = "○○区",
                Tel = "00-0000-0000",
                Fax = "00-0000-0000",
                Memo = "sample",
                Url = "test",
                GyousyuId = 1,
                EigyoBaseSyainId = 1,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種情報の作成
            var gyousyu = new Gyousyu()
            {
                Id = 1,
                Code = "100",
                Name = "業種A",
            };

            // 社員Base情報の作成
            var syainBase = new SyainBasis()
            {
                Id = 1,
                Name = "社員A",
                Code = "100",
            };

            // 社員情報の作成
            List<Syain> syains = new List<Syain>();
            var now = new DateTime(2026, 7, 1, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);
            var today = now.ToDateOnly();

            syains.Add(new Syain()
            {
                Id = 1,
                Code = "100",
                Name = "現役社員",
                KanaName = "ゲンエキシャイン",
                Seibetsu = '1',
                BusyoCode = "500",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = today.AddDays(-10),
                EndYmd = today.AddDays(10),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = 1,
                BusyoId = 5,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            syains.Add(new Syain()
            {
                Id = 2,
                Code = "100",
                Name = "過去社員",
                KanaName = "カコシャイン",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = today.AddDays(-20),
                EndYmd = today.AddDays(-10),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = 1,
                BusyoId = 5,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            syains.Add(new Syain()
            {
                Id = 3,
                Code = "100",
                Name = "未来社員",
                KanaName = "ミライシャイン",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = today.AddDays(10),
                EndYmd = today.AddDays(20),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = 1,
                BusyoId = 5,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            // 部署情報の作成
            List<Busyo> busyos =
            [
                new Busyo()
                {
                    Id = 1,
                    Code = "100",
                    Name = "部署A",
                    KanaName = "ブショエー",
                    OyaCode = "0",
                    StartYmd = new DateOnly(2010, 4, 1),
                    EndYmd = new DateOnly(9999, 12, 31),
                    Jyunjyo = 1,
                    KasyoCode = "1",
                    KaikeiCode = "1",
                    IsActive = true,
                    BusyoBaseId = 1,
                    OyaId = null,
                },

                new Busyo()
                {
                    Id = 2,
                    Code = "200",
                    Name = "部署B",
                    KanaName = "ブショビー",
                    OyaCode = "100",
                    StartYmd = new DateOnly(2010, 4, 1),
                    EndYmd = new DateOnly(9999, 12, 31),
                    Jyunjyo = 1,
                    KasyoCode = "1",
                    KaikeiCode = "1",
                    IsActive = true,
                    BusyoBaseId = 2,
                    OyaId = 1,
                },

                new Busyo()
                {
                    Id = 3,
                    Code = "300",
                    Name = "部署C",
                    KanaName = "ブショシー",
                    OyaCode = "100",
                    StartYmd = new DateOnly(2010, 4, 1),
                    EndYmd = new DateOnly(9999, 12, 31),
                    Jyunjyo = 1,
                    KasyoCode = "1",
                    KaikeiCode = "1",
                    IsActive = true,
                    BusyoBaseId = 3,
                    OyaId = 1,
                },

                new Busyo()
                {
                    Id = 4,
                    Code = "400",
                    Name = "部署D",
                    KanaName = "ブショディー",
                    OyaCode = "300",
                    StartYmd = new DateOnly(2010, 4, 1),
                    EndYmd = new DateOnly(9999, 12, 31),
                    Jyunjyo = 1,
                    KasyoCode = "1",
                    KaikeiCode = "1",
                    IsActive = true,
                    BusyoBaseId = 4,
                    OyaId = 3,
                },

                new Busyo()
                {
                    Id = 5,
                    Code = "500",
                    Name = "部署E",
                    KanaName = "ブショイー",
                    OyaCode = "500",
                    StartYmd = new DateOnly(2010, 4, 1),
                    EndYmd = new DateOnly(9999, 12, 31),
                    Jyunjyo = 1,
                    KasyoCode = "1",
                    KaikeiCode = "1",
                    IsActive = true,
                    BusyoBaseId = 5,
                    OyaId = 3,
                },
            ];

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
            Assert.AreEqual("業種A", model.KokyakuView.GyousyuName, "GyousyuName が サンプル と一致しません。");

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
            var now = new DateTime(2026, 7, 1, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);
            var today = now.ToDateOnly();

            // 顧客会社情報の作成
            var kokyaku = new KokyakuKaisha()
            {
                Code = 100,
                Name = "AA会社",
                NameKana = "エーエーカイシャ",
                Ryakusyou = "AA",
                YuubinBangou = "000-0000",
                Jyuusyo1 = "大阪府大阪市",
                Jyuusyo2 = "○○区",
                Tel = "00-0000-0000",
                Fax = "00-0000-0000",
                Memo = "sample",
                Url = "test",
                GyousyuId = 1,
                EigyoBaseSyainId = 1,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 社員Base情報の作成
            var syainBase = new SyainBasis()
            {
                Id = 1,
                Name = "社員A",
                Code = "100",
            };

            // 社員情報の作成
            var syain = new Syain()
            {
                Id = 1,
                Code = "100",
                Name = "社員A",
                KanaName = "シャインエー",
                Seibetsu = '1',
                BusyoCode = "500",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = today.AddDays(startInt),
                EndYmd = today.AddDays(endInt),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = 1,
                BusyoId = 5,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

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
            var now = new DateTime(2026, 7, 1, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);
            var today = now.ToDateOnly();

            // 顧客会社情報の作成
            var kokyaku = new KokyakuKaisha()
            {
                Code = 100,
                Name = "AA会社",
                NameKana = "エーエーカイシャ",
                Ryakusyou = "AA",
                YuubinBangou = "000-0000",
                Jyuusyo1 = "大阪府大阪市",
                Jyuusyo2 = "○○区",
                Tel = "00-0000-0000",
                Fax = "00-0000-0000",
                Memo = "sample",
                Url = "test",
                GyousyuId = 1,
                EigyoBaseSyainId = 1,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 部署情報の作成
            var busyo = new Busyo()
            {
                Id = 1,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = "0",
                StartYmd = today.AddDays(-10),
                EndYmd = today.AddDays(10),
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
                OyaId = null,
            };

            // 社員Base情報の作成
            var syainBase = new SyainBasis()
            {
                Id = 1,
                Name = "社員A",
                Code = "100",
            };

            // 社員情報の作成
            var syain = new Syain()
            {
                Id = 1,
                Code = "100",
                Name = "社員A",
                KanaName = "シャインエー",
                Seibetsu = '1',
                BusyoCode = "500",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = today.AddDays(-10),
                EndYmd = today.AddDays(10),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = 1,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

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
            var now = new DateTime(2026, 7, 1, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);
            var today = now.ToDateOnly();

            // 社員情報の作成
            // 社員Base情報の作成
            var syainBase = new SyainBasis()
            {
                Id = 1,
                Name = "社員A",
                Code = "100",
            };

            // 部署情報の作成
            var busyo = new Busyo()
            {
                Id = 1,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = "0",
                StartYmd = today.AddDays(-10),
                EndYmd = today.AddDays(10),
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
                OyaId = null,
            };

            // ログインユーザーの顧客会社参照履歴の作成（複数件）

            var idCount = MaxHistoryCount - 5;

            var baseTime = DateTime.ParseExact(
                "2025/04/01 09:00",
                "yyyy/MM/dd HH:mm",
                CultureInfo.InvariantCulture
                );

            var userRirekis = Enumerable.Range(1, idCount)
                .Select(i => new KokyakuKaisyaSansyouRireki()
                {
                    Id = i,
                    KokyakuKaisyaId = i,
                    SyainBaseId = LoggedInUserId,
                    SansyouTime = baseTime.AddMinutes(i - 1),
                }
                    )
                .ToList();

            var otherUserRirekis = Enumerable.Range(idCount + 1, 3)
                .Select(i => new KokyakuKaisyaSansyouRireki()
                {
                    Id = i,
                    KokyakuKaisyaId = 1,
                    SyainBaseId = 0, // ダミー
                    SansyouTime = baseTime.AddMinutes(i),
                })
                .ToList();

            // 顧客会社情報の作成
            var kokyaku = new KokyakuKaisha()
            {
                Id = idCount,
                Name = "AA会社",
                NameKana = "エーエーカイシャ",
                Ryakusyou = "AA",
                YuubinBangou = "000-0000",
                Jyuusyo1 = "大阪府大阪市",
                Jyuusyo2 = "○○区",
                Tel = "00-0000-0000",
                Fax = "00-0000-0000",
                Memo = "sample",
                Url = "test",
                GyousyuId = 1,
                EigyoBaseSyainId = 1,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // データ登録
            SeedEntities(syainBase, kokyaku, busyo, userRirekis, otherUserRirekis);

            var model = CreateModel();

            // ================ Act ================ //
            await model.OnGetAsync(kokyaku.Id);

            // ================ Assert ================ //
            // 確認対象の顧客会社参照履歴を取得
            var targetRireki = await db.KokyakuKaisyaSansyouRirekis
                .SingleOrDefaultAsync(x => x.Id == idCount);

            Assert.IsNotNull(targetRireki, "テストデータが作成されていません。");

            AssertSansyouTime(targetRireki, now);

            // 件数が増加していないことを確認
            var allBeforeCount = idCount + otherUserRirekis.Count;
            var allAfterCount = await db.KokyakuKaisyaSansyouRirekis.CountAsync();
            Assert.AreEqual(allBeforeCount, allAfterCount, "顧客参照履歴が追加されています。");

            // 他の参照履歴に対して更新処理が実行されていないか確認
            var others = await db.KokyakuKaisyaSansyouRirekis
                .Where(x => x.Id != idCount)
                .AsNoTracking()
                .ToListAsync();

            AssertOtherRirekiNotUpdated(others, now);
        }

        // =============================================
        // 異常系テストケース
        // =============================================

        // ===========================================================================================
        // 初期表示
        //      パラメータ.部署ID = 部署マスタ.IDを満たす部署情報がDBに存在しない場合
        // ===========================================================================================
        /// <summary>
        /// パラメータ.顧客会社IDと一致する顧客情報が存在しない場合、RedirectToPageResultが返却されることを確認
        /// </summary>
        /// <param name="kokyakuId">顧客会社ID</param>
        [TestMethod(DisplayName = "パラメータの顧客会社IDと一致する顧客情報が存在しない → エラーページに遷移する")]
        public async Task OnGetAsync_パラメータの顧客会社IDと一致する顧客情報が存在しない_エラーページに遷移()
        {
            // ================ Arrange ================ //
            // 顧客会社情報の作成
            var kokyaku = new KokyakuKaisha()
            {
                Id = 5,
                Name = "AA会社",
                NameKana = "エーエーカイシャ",
                Ryakusyou = "AA",
                YuubinBangou = "000-0000",
                Jyuusyo1 = "大阪府大阪市",
                Jyuusyo2 = "○○区",
                Tel = "00-0000-0000",
                Fax = "00-0000-0000",
                Memo = "sample",
                Url = "test",
                GyousyuId = 1,
                EigyoBaseSyainId = 1,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // データの登録
            SeedEntities(kokyaku);

            var model = CreateModel();

            // ================ Act ================ //
            var result = await model.OnGetAsync(1);

            // ================ Assert ================ //
            var redirect = result as RedirectToPageResult;

            Assert.IsNotNull(redirect);
            Assert.AreEqual("/ErrorMessage", redirect.PageName);
            Assert.AreEqual(Const.ErrorSelectedDataNotExists, redirect.RouteValues?["errorMessage"]);
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
            var now = new DateTime(2026, 7, 1, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);
            var today = fakeTimeProvider.Today();

            // 顧客会社情報の作成
            var kokyaku = new KokyakuKaisha()
            {
                Code = 100,
                Name = "AA会社",
                NameKana = "エーエーカイシャ",
                Ryakusyou = "AA",
                YuubinBangou = "000-0000",
                Jyuusyo1 = "大阪府大阪市",
                Jyuusyo2 = "○○区",
                Tel = "00-0000-0000",
                Fax = "00-0000-0000",
                Memo = "sample",
                Url = "test",
                GyousyuId = 1,
                EigyoBaseSyainId = 1,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 部署情報の作成
            var busyo = new Busyo()
            {
                Id = 1,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = "0",
                StartYmd = new DateOnly(2010, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
                OyaId = null,
            };

            // 社員Base情報の作成
            var syainBase = new SyainBasis()
            {
                Id = 1,
                Name = "社員A",
                Code = "100",
            };

            // 社員情報の作成
            var syain = new Syain()
            {
                Id = 1,
                Code = "100",
                Name = "社員A",
                KanaName = "シャインエー",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = today.AddDays(startInt),
                EndYmd = today.AddDays(endInt),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = 1,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

            // データの登録
            SeedEntities(kokyaku, busyo, syainBase, syain);

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
            var kokyaku = new KokyakuKaisha()
            {
                Code = 100,
                Name = "AA会社",
                NameKana = "エーエーカイシャ",
                Ryakusyou = "AA",
                YuubinBangou = "000-0000",
                Jyuusyo1 = "大阪府大阪市",
                Jyuusyo2 = "○○区",
                Tel = "00-0000-0000",
                Fax = "00-0000-0000",
                Memo = "sample",
                Url = "test",
                GyousyuId = null,
                EigyoBaseSyainId = 1,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種情報の作成
            var gyousyu = new Gyousyu()
            {
                Id = 1,
                Code = "100",
                Name = "業種A",
            };

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
            // 現在の日付を取得
            var now = new DateTime(2026, 7, 1, 18, 0, 0);
            fakeTimeProvider.SetLocalNow(now);
            var today = fakeTimeProvider.Today();

            // 顧客会社情報の作成
            var kokyaku = new KokyakuKaisha()
            {
                Code = 100,
                Name = "AA会社",
                NameKana = "エーエーカイシャ",
                Ryakusyou = "AA",
                YuubinBangou = "000-0000",
                Jyuusyo1 = "大阪府大阪市",
                Jyuusyo2 = "○○区",
                Tel = "00-0000-0000",
                Fax = "00-0000-0000",
                Memo = "sample",
                Url = "test",
                GyousyuId = 1,
                EigyoBaseSyainId = null,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 営業社員情報の作成
            var syainBase = new SyainBasis()
            {
                Id = 1,
                Name = "社員A",
                Code = "100",
            };

            var syain = new Syain()
            {
                Id = 1,
                Code = "100",
                Name = "社員A",
                KanaName = "シャインエー",
                Seibetsu = '1',
                BusyoCode = "500",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = today.AddDays(-10),
                EndYmd = today.AddDays(10),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = 1,
                BusyoId = 5,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

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
    }
}
