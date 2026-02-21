using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Pages.BusyoMasterMaintenanceTouroku;
using Zouryoku.Utils;
using static Zouryoku.Pages.BusyoMasterMaintenanceTouroku.InputModel;

namespace ZouryokuTest.Pages.BusyoMasterMaintenanceTouroku
{
    /// <summary>
    /// 部署マスタメンテナンス登録画面のユニットテスト
    /// </summary>
    [TestClass]
    public class InputModelTests : BaseInMemoryDbContextTest
    {
        /// <summary>
        /// システム日付
        /// </summary>
        private static readonly DateOnly Today = DateTime.Now.ToDateOnly();

        /// <summary>
        /// 9999/12/31
        /// </summary>
        private static readonly DateOnly MaxEndYmd = new(9999, 12, 31);

        /// <summary>
        /// 共通シードで追加する部署Baseマスタ件数
        /// </summary>
        private static readonly int BusyoBaseCount = 3;

        /// <summary>
        /// 共通シードで追加する部署マスタ件数
        /// </summary>
        private static readonly int BusyoCount = 4;

        /// <summary>
        /// 共通シードで追加する社員マスタ件数
        /// </summary>
        private static readonly int SyainCount = 4;

        private InputModel CreateModel()
        {
            var model = new InputModel(db, GetLogger<InputModel>(), options)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };
            return model;
        }

        /// <summary>
        /// 共通シード
        /// - BusyoBases: 30 / 40 / 99
        /// - Busyos: 1（本体）, 2（親）, 3（承認）, 4（NULL部署）
        /// - Syains: 1～4
        /// </summary>
        private void SeedEntities()
        {
            // SyainBasis（部門長BASE）
            var bumoncyoSyainBase = new SyainBasis
            {
                Id = 200,
                Name = "部門長テスト",
                Code = "",
            };

            // BusyoBasis（3件：30 / 40 / 99）
            var busyoBase_既存 = new BusyoBasis
            {
                Id = 30,
                Name = "既存部署BASE名称",
                Bumoncyo = bumoncyoSyainBase
            };
            var busyoBase_NULL = new BusyoBasis
            {
                Id = 40,
                Name = "既存部署NULLBASE名称",
                BumoncyoId = null
            };
            var busyoBase_更新対象外 = new BusyoBasis
            {
                Id = 99,
                Name = "更新対象でない部署BASE",
                BumoncyoId = 999
            };

            // 親部署（BusyoBase=99）
            var oyaBusyo = new Busyo
            {
                Id = 2,
                Code = "222",
                Name = "既存親部署名称",
                KanaName = "ｷｿﾞﾝｵﾔﾌﾞｼｮｶﾅ",
                OyaCode = "333",
                StartYmd = Today.AddDays(-1),
                EndYmd = Today.AddDays(1),
                Jyunjyo = 1,
                KasyoCode = "12",
                KaikeiCode = "234",
                KeiriCode = "45",
                IsActive = true,
                Ryakusyou = "既存親部署略称",
                BusyoBase = busyoBase_更新対象外,
                Oya = null,
                ShoninBusyo = null
            };

            // 承認部署（BusyoBase=99）
            var shoninBusyo = new Busyo
            {
                Id = 3,
                Code = "",
                Name = "承認部署テスト",
                KanaName = "",
                OyaCode = "",
                StartYmd = Today.AddDays(-1),
                EndYmd = Today.AddDays(1),
                Jyunjyo = 1,
                KasyoCode = "",
                KaikeiCode = "",
                KeiriCode = "",
                IsActive = true,
                Ryakusyou = "",
                BusyoBase = busyoBase_更新対象外,
                Oya = null,
                ShoninBusyo = null
            };

            // 本体（表示対象）部署（BusyoBase=30, 親=2, 承認=3）
            var busyo = new Busyo
            {
                Id = 1,
                Code = "111",
                Name = "既存部署名称",
                KanaName = "ｷｿﾞﾝﾌﾞｼｮｶﾀｶﾅﾒｲ",
                OyaCode = "222",
                StartYmd = Today.AddDays(-1),
                EndYmd = Today.AddDays(1),
                Jyunjyo = 1,
                KasyoCode = "44",
                KaikeiCode = "555",
                KeiriCode = "66",
                IsActive = true,
                Ryakusyou = "既存部署略称ﾘｬｸｼｮｳ",
                BusyoBase = busyoBase_既存,
                Oya = oyaBusyo,
                ShoninBusyo = shoninBusyo
            };

            // NULL可項目は全てNULLの部署（BusyoBase=40）
            var nullBusyo = new Busyo
            {
                Id = 4,
                Code = "666",
                Name = "既存部署NULL名称",
                KanaName = "ｷｿﾞﾝﾌﾞｼｮﾇﾙｶﾀｶﾅﾒｲ",
                OyaCode = "777",
                StartYmd = Today.AddDays(-1),
                EndYmd = Today.AddDays(1),
                Jyunjyo = 8,
                KasyoCode = "99",
                KaikeiCode = "999",
                KeiriCode = null,
                IsActive = true,
                Ryakusyou = null,
                BusyoBase = busyoBase_NULL,
                Oya = null,
                ShoninBusyo = null
            };

            // 更新対象社員
            var syainWithRireki = new Syain
            {
                Id = 1,
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = '1',
                BusyoCode = "",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = Today.AddDays(-1),
                StartYmd = Today.AddDays(-1),
                EndYmd = Today.AddDays(1),
                Kyusyoku = 1,
                SyucyoSyokui = Model.Enums.BusinessTripRole._7_8級,
                KingsSyozoku = "",
                KaisyaCode = 1,
                IsGenkaRendou = true,
                Kengen = Model.Enums.EmployeeAuthority.None,
                Jyunjyo = 1,
                Retired = false,
                SyainBase = bumoncyoSyainBase,
                Busyo = busyo,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

            // 更新対象外 社員(部署が異なる)
            var syainNotBusyo = new Syain
            {
                Id = 2,
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = '1',
                BusyoCode = "",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = Today.AddDays(-1),
                StartYmd = Today.AddDays(-1),
                EndYmd = Today.AddDays(1),
                Kyusyoku = 1,
                SyucyoSyokui = Model.Enums.BusinessTripRole._7_8級,
                KingsSyozoku = "",
                KaisyaCode = 1,
                IsGenkaRendou = true,
                Kengen = Model.Enums.EmployeeAuthority.None,
                Jyunjyo = 1,
                Retired = false,
                SyainBase = bumoncyoSyainBase,
                Busyo = oyaBusyo,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

            // 更新対象外 社員(開始日が範囲外)
            var syainBeforeStart = new Syain
            {
                Id = 3,
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = '1',
                BusyoCode = "",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = Today.AddDays(-1),
                StartYmd = Today.AddDays(1),
                EndYmd = Today.AddDays(1),
                Kyusyoku = 1,
                SyucyoSyokui = Model.Enums.BusinessTripRole._7_8級,
                KingsSyozoku = "",
                KaisyaCode = 1,
                IsGenkaRendou = true,
                Kengen = Model.Enums.EmployeeAuthority.None,
                Jyunjyo = 1,
                Retired = false,
                SyainBase = bumoncyoSyainBase,
                Busyo = busyo,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

            // 更新対象外 社員(終了日が範囲外)
            var syainAfterEnd = new Syain
            {
                Id = 4,
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = '1',
                BusyoCode = "",
                SyokusyuCode = 1,
                SyokusyuBunruiCode = 1,
                NyuusyaYmd = Today.AddDays(-1),
                StartYmd = Today.AddDays(-1),
                EndYmd = Today.AddDays(-1),
                Kyusyoku = 1,
                SyucyoSyokui = Model.Enums.BusinessTripRole._7_8級,
                KingsSyozoku = "",
                KaisyaCode = 1,
                IsGenkaRendou = true,
                Kengen = Model.Enums.EmployeeAuthority.None,
                Jyunjyo = 1,
                Retired = false,
                SyainBase = bumoncyoSyainBase,
                Busyo = busyo,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            };

            // 部署
            db.AddRange(busyo, nullBusyo);
            // 社員
            db.AddRange(syainWithRireki, syainNotBusyo, syainBeforeStart, syainAfterEnd);
        }

        /// <summary>
        /// 編集モードの ViewModel を DB から作って上書きオプションを与える共通ヘルパ
        /// </summary>
        /// <returns></returns>
        private BusyoViewModel BuildEditInput(
            int targetBusyoId,
            DateOnly? applyDate = null,
            bool nullifyOptionals = false,
            string? overrideName = null,
            Busyo? overrideOya = null,
            bool? overrideIsActive = null)
        {
            // 既存データ取得（OnGet 代替で必要な情報だけ取得）
            var existing = db.Busyos
                .Include(x => x.BusyoBase)
                .Include(x => x.Oya)
                .First(x => x.Id == targetBusyoId);

            var vm = new BusyoViewModel
            {
                // モード／主キー／バージョン
                IsCreate = false,
                BusyoId = existing.Id,
                BusyoBaseId = existing.BusyoBaseId,
                BusyoVersion = existing.Version,
                BusyoBaseVersion = existing.BusyoBase?.Version ?? 0u,

                // 入力
                BusyoCode = "112",                                      // 既存: "111" → 変更
                BusyoName = overrideName ?? existing.Name,              // 名称変更あり／なし
                BusyoKanaName = "ｺｳｼﾝﾌﾞｼｮｶﾅ",
                BusyoRyakusyou = nullifyOptionals ? null : "更新部署略称",
                OyaId = overrideOya?.Id ?? (nullifyOptionals ? null : existing.OyaId),
                OyaCode = overrideOya?.Code ?? existing.OyaCode,
                OyaName = nullifyOptionals ? null : (overrideOya?.Name ?? existing.Oya?.Name),
                ApplyDate = applyDate ?? Today,
                StartYmd = Today,
                EndYmd = MaxEndYmd,
                KasyoCode = "88",
                KaikeiCode = "99",
                KeiriCode = nullifyOptionals ? null : "00",
                IsActive = overrideIsActive ?? existing.IsActive,

                // 部門長・承認部署（任意）
                BumoncyoId = nullifyOptionals ? (int?)null : 12,
                BumoncyoName = nullifyOptionals ? null : "更新三郎",
                ShoninBusyoId = nullifyOptionals ? (int?)null : 34,
                ShoninBusyoName = nullifyOptionals ? null : "更新営業部",
            };

            return vm;
        }

        /// <summary>
        /// 新規作成モード：初期表示＆画面入力
        /// </summary>
        private InputModel CreateScreenConditions()
        {
            var model = CreateModel();

            BusyoViewModel input = new()
            {
                IsCreate = true,
                BusyoCode = "111",
                BusyoName = "新規部署名称",
                BusyoKanaName = "ｼﾝｷﾌﾞｼｮｶﾅ",
                BusyoRyakusyou = "新規部署略称",
                OyaId = 2,
                OyaCode = "222",
                OyaName = "既存親部署名称",
                ApplyDate = Today,
                StartYmd = Today,
                EndYmd = MaxEndYmd,
                KasyoCode = "44",
                KaikeiCode = "555",
                KeiriCode = "66",
                IsActive = true,
                BumoncyoId = 0,
                BumoncyoName = "田中次郎",
                ShoninBusyoId = 3,
                ShoninBusyoName = "営業部",
            };

            model.Input = input;
            return model;
        }

        /// <summary>
        /// 初期表示（新規作成モード）
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_新規作成モード_初期値が正しく表示される()
        {
            // Arrange
            var model = CreateModel();

            // Act
            await model.OnGetAsync(null);

            // Assert
            Assert.AreEqual(Today, model.Input.ApplyDate, "適用開始日の初期値が違います。");
            Assert.AreEqual(Today, model.Input.StartYmd, "有効開始日の初期値が違います。");
            Assert.AreEqual(MaxEndYmd, model.Input.EndYmd, "有効終了日の初期値が違います。");
        }

        /// <summary>
        /// 初期表示（編集モード）：NULL項目なし
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_編集モード_NULL項目なし_初期値が正しく表示される()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            await model.OnGetAsync(1);

            // Assert
            Assert.AreEqual(1, model.Input.BusyoId, "IDの初期値が違います。");
            Assert.AreEqual(30, model.Input.BusyoBaseId, "部署BaseIDの初期値が違います。");
            Assert.AreEqual("111", model.Input.BusyoCode, "部署番号の初期値が違います。");
            Assert.AreEqual("既存部署名称", model.Input.BusyoName, "部署名称の初期値が違います。");
            Assert.AreEqual("ｷｿﾞﾝﾌﾞｼｮｶﾀｶﾅﾒｲ", model.Input.BusyoKanaName, "部署名称カナの初期値が違います。");
            Assert.AreEqual("既存部署略称ﾘｬｸｼｮｳ", model.Input.BusyoRyakusyou, "部署略称の初期値が違います。");
            Assert.AreEqual("既存親部署名称", model.Input.OyaName, "親部署名称の初期値が違います。");
            Assert.AreEqual(2, model.Input.OyaId, "親IDの初期値が違います。");
            Assert.AreEqual("222", model.Input.OyaCode, "親部署番号の初期値が違います。");
            Assert.AreEqual(Today, model.Input.ApplyDate, "適用開始日の初期値が違います。");
            Assert.AreEqual(Today.AddDays(-1), model.Input.StartYmd, "有効開始日の初期値が違います。");
            Assert.AreEqual(Today.AddDays(1), model.Input.EndYmd, "有効終了日の初期値が違います。");
            Assert.AreEqual("44", model.Input.KasyoCode, "箇所コードの初期値が違います。");
            Assert.AreEqual("555", model.Input.KaikeiCode, "会計コードの初期値が違います。");
            Assert.AreEqual("66", model.Input.KeiriCode, "経理コードの初期値が違います。");
            Assert.IsTrue(model.Input.IsActive, "アクティブフラグの初期値が違います。");
            Assert.AreEqual("部門長テスト", model.Input.BumoncyoName, "部門長名称の初期値が違います。");
            Assert.AreEqual(200, model.Input.BumoncyoId, "部門長IDの初期値が違います。");
            Assert.AreEqual("承認部署テスト", model.Input.ShoninBusyoName, "承認部署名称の初期値が違います。");
            Assert.AreEqual(3, model.Input.ShoninBusyoId, "承認部署IDの初期値が違います。");
        }

        /// <summary>
        /// 初期表示（編集モード）：NULL可項目は全てNULL
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_編集モード_NULL項目あり_初期値が正しく表示される()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            await model.OnGetAsync(4);

            // Assert
            Assert.AreEqual(4, model.Input.BusyoId, "IDの初期値が違います。");
            Assert.AreEqual(40, model.Input.BusyoBaseId, "部署BaseIDの初期値が違います。");
            Assert.AreEqual("666", model.Input.BusyoCode, "部署番号の初期値が違います。");
            Assert.AreEqual("既存部署NULL名称", model.Input.BusyoName, "部署名称の初期値が違います。");
            Assert.AreEqual("ｷｿﾞﾝﾌﾞｼｮﾇﾙｶﾀｶﾅﾒｲ", model.Input.BusyoKanaName, "部署名称カナの初期値が違います。");
            Assert.IsNull(model.Input.BusyoRyakusyou, "部署略称の初期値が違います。");
            Assert.IsNull(model.Input.OyaName, "親部署名称の初期値が違います。");
            Assert.IsNull(model.Input.OyaId, "親IDの初期値が違います。");
            Assert.AreEqual("777", model.Input.OyaCode, "親部署番号の初期値が違います。");
            Assert.AreEqual(Today, model.Input.ApplyDate, "適用開始日の初期値が違います。");
            Assert.AreEqual(Today.AddDays(-1), model.Input.StartYmd, "有効開始日の初期値が違います。");
            Assert.AreEqual(Today.AddDays(1), model.Input.EndYmd, "有効終了日の初期値が違います。");
            Assert.AreEqual("99", model.Input.KasyoCode, "箇所コードの初期値が違います。");
            Assert.AreEqual("999", model.Input.KaikeiCode, "会計コードの初期値が違います。");
            Assert.IsNull(model.Input.KeiriCode, "経理コードの初期値が違います。");
            Assert.IsTrue(model.Input.IsActive, "アクティブフラグの初期値が違います。");
            Assert.IsNull(model.Input.BumoncyoName, "部門長名称の初期値が違います。");
            Assert.IsNull(model.Input.BumoncyoId, "部門長IDの初期値が違います。");
            Assert.IsNull(model.Input.ShoninBusyoName, "承認部署名称の初期値が違います。");
            Assert.IsNull(model.Input.ShoninBusyoId, "承認部署IDの初期値が違います。");
        }

        /// <summary>
        /// 登録（バリデーション）：必須エラー
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_必須項目未入力_必須エラー()
        {
            // Arrange
            var model = CreateModel();

            // Act
            var (isValid, results) = ValidateModel(model.Input);

            // Assert
            Assert.IsFalse(isValid, "バリデーションは失敗するはずです。");

            // 各種必須エラーが含まれていることを確認
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("BusyoCode")), "部署番号のエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("BusyoName")), "部署名称のエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("BusyoKanaName")), "部署名称カナのエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("OyaCode")), "親部署番号のエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("KasyoCode")), "箇所コードのエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("KaikeiCode")), "会計コードのエラーが含まれているはずです。");

            // 必須エラーメッセージの内容を確認
            Assert.AreEqual(string.Format(Const.ErrorRequired, "部署番号"),
                results.First(r => r.MemberNames.Contains("BusyoCode")).ErrorMessage,
                "部署番号のエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorRequired, "部署名称"),
                results.First(r => r.MemberNames.Contains("BusyoName")).ErrorMessage,
                "部署名称のエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorRequired, "部署名称カナ"),
                results.First(r => r.MemberNames.Contains("BusyoKanaName")).ErrorMessage,
                "部署名称カナのエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorRequired, "親部署"),
                results.First(r => r.MemberNames.Contains("OyaCode")).ErrorMessage,
                "親部署のエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorRequired, "箇所コード"),
                results.First(r => r.MemberNames.Contains("KasyoCode")).ErrorMessage,
                "箇所コードのエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorRequired, "会計コード"),
                results.First(r => r.MemberNames.Contains("KaikeiCode")).ErrorMessage,
                "会計コードのエラーメッセージが一致しません。");

            // 任意項目のエラーが含まれていないことを確認
            Assert.IsFalse(results.Any(r => r.MemberNames.Contains("BusyoRyakusyou")), "部署略称のエラーが含まれていないはずです。");
            Assert.IsFalse(results.Any(r => r.MemberNames.Contains("KeiriCode")), "経理コードのエラーが含まれていないはずです。");
            Assert.IsFalse(results.Any(r => r.MemberNames.Contains("BumoncyoSyainId")), "部門長のエラーが含まれていないはずです。");
            Assert.IsFalse(results.Any(r => r.MemberNames.Contains("ShoninBusyoId")), "承認部署のエラーが含まれていないはずです。");
        }

        /// <summary>
        /// 登録（バリデーション）：桁数エラー
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_桁数制限項目桁溢れ_桁数制限項目に桁数エラー()
        {
            // Arrange
            var model = CreateModel();

            // 手入力を再現
            model.Input.BusyoCode = "1234";
            model.Input.BusyoName = "123456789012345678901234567890123";
            model.Input.BusyoKanaName = "12345678901234567890123456789012345678901234567890123456789012345";
            model.Input.BusyoRyakusyou = "12345678901";
            model.Input.OyaCode = "1234";
            model.Input.KasyoCode = "123";
            model.Input.KaikeiCode = "1234";
            model.Input.KeiriCode = "123";

            // Act
            var (isValid, results) = ValidateModel(model.Input);

            // Assert
            Assert.IsFalse(isValid, "バリデーションは失敗するはずです。");

            // 各種必須エラーが含まれていることを確認
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("BusyoCode")), "部署番号のエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("BusyoName")), "部署名称のエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("BusyoKanaName")), "部署名称カナのエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("BusyoRyakusyou")), "部署略称のエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("OyaCode")), "親部署番号のエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("KasyoCode")), "箇所コードのエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("KaikeiCode")), "会計コードのエラーが含まれているはずです。");
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("KeiriCode")), "経理コードのエラーが含まれているはずです。");

            // 必須エラーメッセージの内容を確認
            Assert.AreEqual(string.Format(Const.ErrorLength, "部署番号", 3),
                results.First(r => r.MemberNames.Contains("BusyoCode")).ErrorMessage,
                "部署番号のエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorLength, "部署名称", 32),
                results.First(r => r.MemberNames.Contains("BusyoName")).ErrorMessage,
                "部署名称のエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorLength, "部署名称カナ", 64),
                results.First(r => r.MemberNames.Contains("BusyoKanaName")).ErrorMessage,
                "部署名称カナのエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorLength, "部署略称", 10),
                results.First(r => r.MemberNames.Contains("BusyoRyakusyou")).ErrorMessage,
                "部署略称のエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorLength, "親部署", 3),
                results.First(r => r.MemberNames.Contains("OyaCode")).ErrorMessage,
                "親部署のエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorLength, "箇所コード", 2),
                results.First(r => r.MemberNames.Contains("KasyoCode")).ErrorMessage,
                "箇所コードのエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorLength, "会計コード", 3),
                results.First(r => r.MemberNames.Contains("KaikeiCode")).ErrorMessage,
                "会計コードのエラーメッセージが一致しません。");
            Assert.AreEqual(string.Format(Const.ErrorLength, "経理コード", 2),
                results.First(r => r.MemberNames.Contains("KeiriCode")).ErrorMessage,
                "経理コードのエラーメッセージが一致しません。");
        }

        /// <summary>
        /// 登録（バリデーション）：エラー返却
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_バリデーションエラーあり_JsonResultが返却される()
        {
            // Arrange
            var model = CreateModel();

            // 手動でバリデーションエラーを発生
            var key = "Input.BusyoCode";
            var errorMessage = string.Format(Const.ErrorRequired, "部署番号");
            model.ModelState.AddModelError(key, errorMessage);

            // Act
            var result = await model.OnPostRegisterAsync();

            // Assert
            var json = Assert.IsInstanceOfType<JsonResult>(result);

            // JsonResult にエラーメッセージが含まれていることを確認
            var errorMessageList = GetErrors(json, key);
            Assert.IsNotNull(errorMessageList);

            // 手動で発生させたエラーメッセージが含まれていることを確認
            Assert.Contains(errorMessage, errorMessageList, "部署番号 のエラーメッセージが存在するはずです。");
        }

        /// <summary>
        /// 登録（新規作成モード）：部署番号の重複
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_新規作成モード_部署番号重複_エラー()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateScreenConditions();

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsNotNull(model.ModelState[nameof(model.Input.BusyoCode)], "ModelStateに部署番号のエラーが存在するはずです。");

            var errors = model.ModelState[nameof(model.Input.BusyoCode)]!.Errors;
            Assert.HasCount(1, errors, "ModelStateにはエラーが1件設定されているはずです。");

            Assert.AreEqual(string.Format(Const.ErrorUnique, "部署番号", "111"),
                errors[0].ErrorMessage, "エラーメッセージが一致しません。");
        }

        /// <summary>
        /// 登録（新規作成モード）：NULL項目なし→部署BASEマスタ・部署マスタへINSERT
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_新規作成モード_NULL項目なし_部署BASEマスタと部署マスタへINSERT()
        {
            // Arrange
            var model = CreateScreenConditions();

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            // 部署BASEマスタが登録できていること
            var baseCount = await db.BusyoBases.CountAsync();
            Assert.AreEqual(1, baseCount, "部署BASEが1件追加されているはずです。");

            // 部署BASEマスタ登録内容の確認
            var savedBase = await db.BusyoBases.FirstAsync();
            Assert.AreEqual(model.Input.BusyoName, savedBase.Name, "保存された名前が一致しません。");
            Assert.AreEqual(model.Input.BumoncyoId, savedBase.BumoncyoId, "保存された部門長IDが一致しません。");

            // 部署BASEマスタへINSERT時に自動採番されたIDを取得
            var busyoBaseId = savedBase.Id;

            // 部署マスタが登録できていること
            var busyoCount = await db.Busyos.CountAsync();
            Assert.AreEqual(1, busyoCount, "部署が1件追加されているはずです。");

            // 部署マスタ登録内容の確認
            var saved = await db.Busyos.FirstAsync();
            Assert.AreEqual(model.Input.BusyoCode, saved.Code, "保存された部署番号が一致しません。");
            Assert.AreEqual(model.Input.BusyoName, saved.Name, "保存された部署名称が一致しません。");
            Assert.AreEqual(model.Input.BusyoKanaName, saved.KanaName, "保存された部署名称カナが一致しません。");
            Assert.AreEqual(model.Input.OyaCode, saved.OyaCode, "保存された親部署番号が一致しません。");
            Assert.AreEqual(model.Input.ApplyDate, saved.StartYmd, "保存された有効開始日が一致しません。");
            Assert.AreEqual(MaxEndYmd, saved.EndYmd, "保存された有効終了日が一致しません。");
            Assert.AreEqual(0, saved.Jyunjyo, "保存された順序が一致しません。");
            Assert.AreEqual(model.Input.KasyoCode, saved.KasyoCode, "保存された箇所コードが一致しません。");
            Assert.AreEqual(model.Input.KaikeiCode, saved.KaikeiCode, "保存された会計コードが一致しません。");
            Assert.AreEqual(model.Input.KeiriCode, saved.KeiriCode, "保存された経理コードが一致しません。");
            Assert.AreEqual(model.Input.IsActive, saved.IsActive, "保存されたアクティブフラグが一致しません。");
            Assert.AreEqual(model.Input.BusyoRyakusyou, saved.Ryakusyou, "保存された略称が一致しません。");
            Assert.AreEqual(busyoBaseId, saved.BusyoBaseId, "保存された部署BaseIDが一致しません。");
            Assert.AreEqual(model.Input.OyaId, saved.OyaId, "保存された親IDが一致しません。");
            Assert.AreEqual(model.Input.ShoninBusyoId, saved.ShoninBusyoId, "保存された承認部署IDが一致しません。");
        }

        /// <summary>
        /// 登録（新規作成モード）：NULL項目あり→部署BASEマスタ・部署マスタへINSERT
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_新規作成モード_NULL項目あり_部署BASEマスタと部署マスタへINSERT()
        {
            // Arrange
            var model = CreateModel();

            BusyoViewModel input = new()
            {
                IsCreate = true,
                BusyoCode = "111",
                BusyoName = "新規部署名称",
                BusyoKanaName = "ｼﾝｷﾌﾞｼｮｶﾅ",
                BusyoRyakusyou = null,
                OyaId = null,
                OyaCode = "222",
                OyaName = null,
                ApplyDate = Today,
                StartYmd = Today,
                EndYmd = MaxEndYmd,
                KasyoCode = "44",
                KaikeiCode = "555",
                KeiriCode = null,
                IsActive = true,
                BumoncyoId = null,
                BumoncyoName = null,
                ShoninBusyoId = null,
                ShoninBusyoName = null,
            };
            model.Input = input;

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            // 部署BASEマスタが登録できていること
            var baseCount = await db.BusyoBases.CountAsync();
            Assert.AreEqual(1, baseCount, "部署BASEが1件追加されているはずです。");

            // 部署BASEマスタ登録内容の確認
            var savedBase = await db.BusyoBases.FirstAsync();
            Assert.AreEqual(model.Input.BusyoName, savedBase.Name, "保存された名前が一致しません。");
            Assert.IsNull(savedBase.BumoncyoId, "保存された部門長IDが一致しません。");

            // 部署BASEマスタへINSERT時に自動採番されたIDを取得
            var busyoBaseId = savedBase.Id;

            // 部署マスタが登録できていること
            var busyoCount = await db.Busyos.CountAsync();
            Assert.AreEqual(1, busyoCount, "部署が1件追加されているはずです。");

            // 部署マスタ登録内容の確認
            var saved = await db.Busyos.FirstAsync();
            Assert.AreEqual(model.Input.BusyoCode, saved.Code, "保存された部署番号が一致しません。");
            Assert.AreEqual(model.Input.BusyoName, saved.Name, "保存された部署名称が一致しません。");
            Assert.AreEqual(model.Input.BusyoKanaName, saved.KanaName, "保存された部署名称カナが一致しません。");
            Assert.AreEqual(model.Input.OyaCode, saved.OyaCode, "保存された親部署番号が一致しません。");
            Assert.AreEqual(model.Input.ApplyDate, saved.StartYmd, "保存された有効開始日が一致しません。");
            Assert.AreEqual(MaxEndYmd, saved.EndYmd, "保存された有効終了日が一致しません。");
            Assert.AreEqual(0, saved.Jyunjyo, "保存された順序が一致しません。");
            Assert.AreEqual(model.Input.KasyoCode, saved.KasyoCode, "保存された箇所コードが一致しません。");
            Assert.AreEqual(model.Input.KaikeiCode, saved.KaikeiCode, "保存された会計コードが一致しません。");
            Assert.IsNull(saved.KeiriCode, "保存された経理コードが一致しません。");
            Assert.AreEqual(model.Input.IsActive, saved.IsActive, "保存されたアクティブフラグが一致しません。");
            Assert.IsNull(saved.Ryakusyou, "保存された略称が一致しません。");
            Assert.AreEqual(busyoBaseId, saved.BusyoBaseId, "保存された部署BaseIDが一致しません。");
            Assert.IsNull(saved.OyaId, "保存された親IDが一致しません。");
            Assert.IsNull(saved.ShoninBusyoId, "保存された承認部署IDが一致しません。");
        }

        /// <summary>
        /// 登録（編集モード）：履歴対象項目変更なし、NULL項目なし→部署BASEマスタ・部署マスタへ単純更新UPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_履歴対象項目変更なし_NULL項目なし_部署BASEマスタと部署マスタへ単純更新UPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 単純更新NULL項目なし(model);
        }

        /// <summary>
        /// 登録（編集モード）：履歴対象項目変更なし、NULL項目あり→部署BASEマスタ・部署マスタへ単純更新UPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_履歴対象項目変更なし_NULL項目あり_部署BASEマスタと部署マスタへ単純更新UPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today,
                nullifyOptionals: true
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 単純更新NULL項目あり(model);
        }

        /// <summary>
        /// 登録（編集モード）：部署名称変更あり、適用開始日＝有効開始日、NULL項目なし→部署BASEマスタ・部署マスタへ単純更新UPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_部署名称変更あり_適用開始日有効開始日が同日_NULL項目なし_部署BASEマスタと部署マスタへ単純更新UPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today,
                nullifyOptionals: false,
                overrideName: "更新後部署名称"
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 単純更新NULL項目なし(model);
        }

        /// <summary>
        /// 登録（編集モード）：部署名称変更あり、適用開始日＝有効開始日、NULL項目あり→部署BASEマスタ・部署マスタへ単純更新UPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_部署名称変更あり_適用開始日有効開始日が同日_NULL項目あり_部署BASEマスタと部署マスタへ単純更新UPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today,
                nullifyOptionals: true,
                overrideName: "更新後部署名称"
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 単純更新NULL項目あり(model);
        }

        /// <summary>
        /// 登録（編集モード）：部署名称変更あり、適用開始日＜有効開始日→エラー
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_部署名称変更あり_適用開始日より有効開始日が後ろ_エラー()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today.AddDays(-1),
                nullifyOptionals: false,
                overrideName: "更新後部署名称"
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsNotNull(model.ModelState[nameof(model.Input.ApplyDate)], "ModelStateに適用開始日のエラーが存在するはずです。");

            var errors = model.ModelState[nameof(model.Input.ApplyDate)]!.Errors;
            Assert.HasCount(1, errors, "ModelStateにはエラーが1件設定されているはずです。");

            Assert.AreEqual(string.Format(Const.ErrorMoreThanDateTime, "適用開始日", "有効開始日"),
                errors[0].ErrorMessage, "エラーメッセージが一致しません。");
        }

        /// <summary>
        /// 登録（編集モード）：部署名称変更あり、有効開始日＜適用開始日、NULL項目なし→履歴ありUPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_部署名称変更あり_有効開始日より適用開始日が後ろ_NULL項目なし_履歴ありUPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today.AddDays(1),
                nullifyOptionals: false,
                overrideName: "更新後部署名称"
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 履歴あり更新NULL項目なし(model);
        }

        /// <summary>
        /// 登録（編集モード）：部署名称変更あり、有効開始日＜適用開始日、NULL項目あり→履歴ありUPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_部署名称変更あり_有効開始日より適用開始日が後ろ_NULL項目あり_履歴ありUPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today.AddDays(1),
                nullifyOptionals: true,
                overrideName: "更新後部署名称"
             );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 履歴あり更新NULL項目あり(model);
        }

        /// <summary>
        /// 登録（編集モード）：親部署変更あり、適用開始日＝有効開始日、NULL項目なし→部署BASEマスタ・部署マスタへ単純更新UPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_親部署変更あり_適用開始日有効開始日が同日_NULL項目なし_部署BASEマスタと部署マスタへ単純更新UPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            var oya = new Busyo
            {
                Id = 4,
                Code = "444",
                Name = "親部署更新テスト",
            };

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today,
                nullifyOptionals: false,
                overrideOya: oya
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 単純更新NULL項目なし(model);
        }

        /// <summary>
        /// 登録（編集モード）：親部署変更あり、適用開始日＝有効開始日、NULL項目あり→部署BASEマスタ・部署マスタへ単純更新UPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_親部署変更あり_適用開始日有効開始日が同日_NULL項目あり_部署BASEマスタと部署マスタへ単純更新UPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            var oya = new Busyo
            {
                Id = 4,
                Code = "444",
                Name = "親部署更新テスト",
            };

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today,
                nullifyOptionals: true,
                overrideOya: oya
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 単純更新NULL項目あり(model);
        }

        /// <summary>
        /// 登録（編集モード）：親部署変更あり、適用開始日＜有効開始日→エラー
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_親部署変更あり_適用開始日より有効開始日が後ろ_エラー()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            var oya = new Busyo
            {
                Id = 4,
                Code = "444",
                Name = "親部署更新テスト",
            };

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today.AddDays(-1),
                nullifyOptionals: true,
                overrideOya: oya
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsNotNull(model.ModelState[nameof(model.Input.ApplyDate)], "ModelStateに適用開始日のエラーが存在するはずです。");

            var errors = model.ModelState[nameof(model.Input.ApplyDate)]!.Errors;
            Assert.HasCount(1, errors, "ModelStateにはエラーが1件設定されているはずです。");

            Assert.AreEqual(string.Format(Const.ErrorMoreThanDateTime, "適用開始日", "有効開始日"),
                errors[0].ErrorMessage, "エラーメッセージが一致しません。");
        }

        /// <summary>
        /// 登録（編集モード）：親部署変更あり、有効開始日＜適用開始日、NULL項目なし→履歴ありUPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_親部署変更あり_有効開始日より適用開始日が後ろ_NULL項目なし_履歴ありUPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            var oya = new Busyo
            {
                Id = 4,
                Code = "444",
                Name = "親部署更新テスト",
            };

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today.AddDays(1),
                nullifyOptionals: false,
                overrideOya: oya
             );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 履歴あり更新NULL項目なし(model);
        }

        /// <summary>
        /// 登録（編集モード）：親部署変更あり、有効開始日＜適用開始日、NULL項目あり→履歴ありUPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_親部署変更あり_有効開始日より適用開始日が後ろ_NULL項目あり_履歴ありUPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            var oya = new Busyo
            {
                Id = 4,
                Code = "444",
                Name = "親部署更新テスト",
            };

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today.AddDays(1),
                nullifyOptionals: true,
                overrideOya: oya
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 履歴あり更新NULL項目あり(model);
        }

        /// <summary>
        /// 登録（編集モード）：アクティブフラグ変更あり、適用開始日＝有効開始日、NULL項目なし→部署BASEマスタ・部署マスタへ単純更新UPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_アクティブフラグ変更あり_適用開始日有効開始日が同日_NULL項目なし_部署BASEマスタと部署マスタへ単純更新UPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today,
                nullifyOptionals: false,
                overrideIsActive: false
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 単純更新NULL項目なし(model);
        }

        /// <summary>
        /// 登録（編集モード）：アクティブフラグ変更あり、適用開始日＝有効開始日、NULL項目あり→部署BASEマスタ・部署マスタへ単純更新UPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_アクティブフラグ変更あり_適用開始日有効開始日が同日_NULL項目あり_部署BASEマスタと部署マスタへ単純更新UPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today,
                nullifyOptionals: true,
                overrideIsActive: false
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 単純更新NULL項目あり(model);
        }

        /// <summary>
        /// 登録（編集モード）：アクティブフラグ変更あり、適用開始日＜有効開始日→エラー
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_アクティブフラグ変更あり_適用開始日より有効開始日が後ろ_エラー()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today.AddDays(-1),
                nullifyOptionals: false,
                overrideIsActive: false
             );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsNotNull(model.ModelState[nameof(model.Input.ApplyDate)], "ModelStateに適用開始日のエラーが存在するはずです。");

            var errors = model.ModelState[nameof(model.Input.ApplyDate)]!.Errors;
            Assert.HasCount(1, errors, "ModelStateにはエラーが1件設定されているはずです。");

            Assert.AreEqual(string.Format(Const.ErrorMoreThanDateTime, "適用開始日", "有効開始日"),
                errors[0].ErrorMessage, "エラーメッセージが一致しません。");
        }

        /// <summary>
        /// 登録（編集モード）：アクティブフラグ変更あり、有効開始日＜適用開始日、NULL項目なし→履歴ありUPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_アクティブフラグ変更あり_有効開始日より適用開始日が後ろ_NULL項目なし_履歴ありUPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today.AddDays(1),
                nullifyOptionals: false,
                overrideIsActive: false
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 履歴あり更新NULL項目なし(model);
        }

        /// <summary>
        /// 登録（編集モード）：アクティブフラグ変更あり、有効開始日＜適用開始日、NULL項目あり→履歴ありUPDATE
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_編集モード_アクティブフラグ変更あり_有効開始日より適用開始日が後ろ_NULL項目あり_履歴ありUPDATE()
        {
            // Arrange
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // ViewModel 構築
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today.AddDays(1),
                nullifyOptionals: true,
                overrideIsActive: false
            );

            // Act
            await model.OnPostRegisterAsync();

            // Assert
            await 履歴あり更新NULL項目あり(model);
        }

        // 登録（編集モード）：楽観的同時実行制御エラーが発生した場合、更新されないことを確認
        [TestMethod]
        public async Task OnPostRegisterAsync_楽観的同時実行制御エラーが発生した場合_更新されない()
        {
            // Arrange
            // 初期データ投入
            SeedEntities();
            await db.SaveChangesAsync();

            var model = CreateModel();

            // 画面入力（編集モード）を作成
            model.Input = BuildEditInput(
                targetBusyoId: 1,
                applyDate: Today,
                nullifyOptionals: false
            );

            // 意図的に「異なる Version」をセットして競合を誘発させる
            model.Input.BusyoVersion = 9999u;

            // Act
            var result = await model.OnPostRegisterAsync();

            // Assert
            Assert.IsInstanceOfType<ObjectResult>(result);

            // ObjectResult にエラーメッセージが含まれていることを確認
            var json = result as ObjectResult;
            Assert.IsNotNull(json);
            var message = GetMessage(json);
            Assert.IsNotNull(message);

            // 楽観的同時実行制御エラーが含まれていることを確認
            Assert.Contains(string.Format(Const.ErrorConflictReload, "部署"),
                message, "楽観的同時実行制御エラーメッセージが含まれているはずです。");

            // レコードが更新されていないことを確認
            var busyoCount = await db.Busyos.CountAsync();
            Assert.AreEqual(BusyoCount, busyoCount, "競合時は部署の件数が増減しないはずです。");
            var after = await db.Busyos.AsNoTracking().FirstAsync(x => x.Id == model.Input.BusyoId);
            Assert.AreEqual("111", after.Code, "競合時に部署が更新されてはいけません。");
        }

        /// <summary>
        /// NULLへの更新項目がない場合の、単純更新。
        /// 部署BASEマスタへのUPDATE、部署マスタへのUPDATE　を確認する
        /// </summary>
        /// <param name="model">ページモデル</param>
        /// <returns></returns>
        public async Task 単純更新NULL項目なし(InputModel model)
        {
            // Assert
            // 部署BASEマスタの件数が変わっていないこと
            var baseCount = await db.BusyoBases.CountAsync();
            Assert.AreEqual(BusyoBaseCount, baseCount, "部署BASEが件数増減していないはずです。");

            // 更新内容の確認
            var updateBase = await db.BusyoBases.FirstAsync(x => x.Id == model.Input.BusyoBaseId);
            Assert.AreEqual("既存部署BASE名称", updateBase.Name, "更新対象でない部署BASE.名前が変更されています。");
            Assert.AreEqual(model.Input.BumoncyoId, updateBase.BumoncyoId, "更新された部門長IDが一致しません。");

            // 他のレコードの確認
            var noTouchBase = await db.BusyoBases.FirstAsync(x => x.Id == 99);
            Assert.AreEqual("更新対象でない部署BASE", noTouchBase.Name, "更新対象でないレコードの部署BASE.名前が変更されています。");
            Assert.AreEqual(999, noTouchBase.BumoncyoId, "更新対象でないレコードの部署BASE.部門長IDが変更されています。");

            // 部署マスタの件数が変わっていないこと
            var busyoCount = await db.Busyos.CountAsync();
            Assert.AreEqual(BusyoCount, busyoCount, "部署が件数増減していないはずです。");

            // 更新内容の確認
            var updateBusyo = await db.Busyos.FirstAsync(x => x.Id == model.Input.BusyoId);
            Assert.AreEqual(model.Input.BusyoCode, updateBusyo.Code, "更新された部署番号が一致しません。");
            Assert.AreEqual(model.Input.BusyoName, updateBusyo.Name, "更新された部署名称が一致しません。");
            Assert.AreEqual(model.Input.BusyoKanaName, updateBusyo.KanaName, "更新された部署名称カナが一致しません。");
            Assert.AreEqual(model.Input.OyaCode, updateBusyo.OyaCode, "更新された親部署番号が一致しません。");
            Assert.AreEqual(model.Input.StartYmd, updateBusyo.StartYmd, "更新された有効開始日が一致しません。");
            Assert.AreEqual(model.Input.EndYmd, updateBusyo.EndYmd, "更新された有効終了日が一致しません。");
            Assert.AreEqual(1, updateBusyo.Jyunjyo, "更新対象でない部署.並び順序が変更されています。");
            Assert.AreEqual(model.Input.KasyoCode, updateBusyo.KasyoCode, "更新された箇所コードが一致しません。");
            Assert.AreEqual(model.Input.KaikeiCode, updateBusyo.KaikeiCode, "更新された会計コードが一致しません。");
            Assert.AreEqual(model.Input.KeiriCode, updateBusyo.KeiriCode, "更新された経理コードが一致しません。");
            Assert.AreEqual(model.Input.IsActive, updateBusyo.IsActive, "更新されたアクティブフラグが一致しません。");
            Assert.AreEqual(model.Input.BusyoRyakusyou, updateBusyo.Ryakusyou, "更新された部署略称が一致しません。");
            Assert.AreEqual(30, updateBusyo.BusyoBaseId, "更新対象でない部署.部署BaseIDが変更されています。");
            Assert.AreEqual(model.Input.OyaId, updateBusyo.OyaId, "更新された親IDが一致しません。");
            Assert.AreEqual(model.Input.ShoninBusyoId, updateBusyo.ShoninBusyoId, "更新された承認部署IDが一致しません。");

            // 他のレコードの確認
            var oyaBusyo = await db.Busyos.FirstAsync(x => x.Id == 2);
            Assert.AreEqual("222", oyaBusyo.Code, "更新対象でないレコードの部署.部署番号が変更されています。");
            Assert.AreEqual("既存親部署名称", oyaBusyo.Name, "更新対象でないレコードの部署.名前が変更されています。");
            Assert.AreEqual("ｷｿﾞﾝｵﾔﾌﾞｼｮｶﾅ", oyaBusyo.KanaName, "更新対象でないレコードの部署.部署名称カナが変更されています。");
            Assert.AreEqual("333", oyaBusyo.OyaCode, "更新対象でないレコードの部署.親部署番号が変更されています。");
            Assert.AreEqual(Today.AddDays(-1), oyaBusyo.StartYmd, "更新対象でないレコードの部署.有効開始日が変更されています。");
            Assert.AreEqual(Today.AddDays(1), oyaBusyo.EndYmd, "更新対象でないレコードの部署.有効終了日が変更されています。");
            Assert.AreEqual("12", oyaBusyo.KasyoCode, "更新対象でないレコードの部署.箇所コードが変更されています。");
            Assert.AreEqual("234", oyaBusyo.KaikeiCode, "更新対象でないレコードの部署.会計コードが変更されています。");
            Assert.AreEqual("45", oyaBusyo.KeiriCode, "更新対象でないレコードの部署.経理コードが変更されています。");
            Assert.IsTrue(oyaBusyo.IsActive, "更新対象でないレコードの部署.アクティブフラグが変更されています。");
            Assert.AreEqual("既存親部署略称", oyaBusyo.Ryakusyou, "更新対象でないレコードの部署.略称が変更されています。");
            Assert.IsNull(oyaBusyo.OyaId, "更新対象でないレコードの部署.親IDが変更されています。");
            Assert.IsNull(oyaBusyo.ShoninBusyoId, "更新対象でないレコードの部署.承認部署IDが変更されています。");
        }

        /// <summary>
        /// NULLへの更新項目がある場合の、単純更新。
        /// 部署BASEマスタへのUPDATE、部署マスタへのUPDATE　を確認する
        /// </summary>
        /// <param name="model">ページモデル</param>
        /// <returns></returns>
        public async Task 単純更新NULL項目あり(InputModel model)
        {
            // Assert
            // 部署BASEマスタの件数が変わっていないこと
            var baseCount = await db.BusyoBases.CountAsync();
            Assert.AreEqual(BusyoBaseCount, baseCount, "部署BASEが件数増減していないはずです。");

            // 更新内容の確認
            var updateBase = await db.BusyoBases.FirstAsync(x => x.Id == model.Input.BusyoBaseId);
            Assert.AreEqual("既存部署BASE名称", updateBase.Name, "更新対象でない部署BASE.名前が変更されています。");
            Assert.IsNull(updateBase.BumoncyoId, "更新された部門長IDが一致しません。");

            // 他のレコードの確認
            var noTouchBase = await db.BusyoBases.FirstAsync(x => x.Id == 99);
            Assert.AreEqual("更新対象でない部署BASE", noTouchBase.Name, "更新対象でないレコードの部署BASE.名前が変更されています。");
            Assert.AreEqual(999, noTouchBase.BumoncyoId, "更新対象でないレコードの部署BASE.部門長IDが変更されています。");

            // 部署マスタの件数が変わっていないこと
            var busyoCount = await db.Busyos.CountAsync();
            Assert.AreEqual(BusyoCount, busyoCount, "部署が件数増減していないはずです。");

            // 更新内容の確認
            var updateBusyo = await db.Busyos.FirstAsync(x => x.Id == model.Input.BusyoId);
            Assert.AreEqual(model.Input.BusyoCode, updateBusyo.Code, "更新された部署番号が一致しません。");
            Assert.AreEqual(model.Input.BusyoName, updateBusyo.Name, "更新された部署名称が一致しません。");
            Assert.AreEqual(model.Input.BusyoKanaName, updateBusyo.KanaName, "更新された部署名称カナが一致しません。");
            Assert.AreEqual(model.Input.OyaCode, updateBusyo.OyaCode, "更新された親部署番号が一致しません。");
            Assert.AreEqual(model.Input.StartYmd, updateBusyo.StartYmd, "更新された有効開始日が一致しません。");
            Assert.AreEqual(model.Input.EndYmd, updateBusyo.EndYmd, "更新された有効終了日が一致しません。");
            Assert.AreEqual(1, updateBusyo.Jyunjyo, "更新対象でない部署.並び順序が変更されています。");
            Assert.AreEqual(model.Input.KasyoCode, updateBusyo.KasyoCode, "更新された箇所コードが一致しません。");
            Assert.AreEqual(model.Input.KaikeiCode, updateBusyo.KaikeiCode, "更新された会計コードが一致しません。");
            Assert.IsNull(updateBusyo.KeiriCode, "更新された経理コードが一致しません。");
            Assert.AreEqual(model.Input.IsActive, updateBusyo.IsActive, "更新されたアクティブフラグが一致しません。");
            Assert.AreEqual(model.Input.BusyoRyakusyou, updateBusyo.Ryakusyou, "更新された部署略称が一致しません。");
            Assert.AreEqual(30, updateBusyo.BusyoBaseId, "更新対象でない部署.部署BaseIDが変更されています。");
            Assert.AreEqual(model.Input.OyaId, updateBusyo.OyaId, "更新された親IDが一致しません。");
            Assert.IsNull(updateBusyo.ShoninBusyoId, "更新された承認部署IDが一致しません。");

            // 他のレコードの確認
            var oyaBusyo = await db.Busyos.FirstAsync(x => x.Id == 2);
            Assert.AreEqual("222", oyaBusyo.Code, "更新対象でないレコードの部署.部署番号が変更されています。");
            Assert.AreEqual("既存親部署名称", oyaBusyo.Name, "更新対象でないレコードの部署.名前が変更されています。");
            Assert.AreEqual("ｷｿﾞﾝｵﾔﾌﾞｼｮｶﾅ", oyaBusyo.KanaName, "更新対象でないレコードの部署.部署名称カナが変更されています。");
            Assert.AreEqual("333", oyaBusyo.OyaCode, "更新対象でないレコードの部署.親部署番号が変更されています。");
            Assert.AreEqual(Today.AddDays(-1), oyaBusyo.StartYmd, "更新対象でないレコードの部署.有効開始日が変更されています。");
            Assert.AreEqual(Today.AddDays(1), oyaBusyo.EndYmd, "更新対象でないレコードの部署.有効終了日が変更されています。");
            Assert.AreEqual("12", oyaBusyo.KasyoCode, "更新対象でないレコードの部署.箇所コードが変更されています。");
            Assert.AreEqual("234", oyaBusyo.KaikeiCode, "更新対象でないレコードの部署.会計コードが変更されています。");
            Assert.AreEqual("45", oyaBusyo.KeiriCode, "更新対象でないレコードの部署.経理コードが変更されています。");
            Assert.IsTrue(oyaBusyo.IsActive, "更新対象でないレコードの部署.アクティブフラグが変更されています。");
            Assert.AreEqual("既存親部署略称", oyaBusyo.Ryakusyou, "更新対象でないレコードの部署.略称が変更されています。");
            Assert.IsNull(oyaBusyo.OyaId, "更新対象でないレコードの部署.親IDが変更されています。");
            Assert.IsNull(oyaBusyo.ShoninBusyoId, "更新対象でないレコードの部署.承認部署IDが変更されています。");
        }

        /// <summary>
        /// NULLへの更新項目がない場合の、履歴あり更新。
        /// 部署BASEマスタへのUPDATE、部署マスタへのUPDATE(無効化)、部署マスタへのINSERT、
        /// 社員マスタへのUPDATE(無効化)、社員マスタへのINSERT　を確認する
        /// </summary>
        /// <param name="model">ページモデル</param>
        /// <returns></returns>
        public async Task 履歴あり更新NULL項目なし(InputModel model)
        {
            // Assert
            // ■部署BASEマスタ
            // 件数が変わっていないこと
            var baseCount = await db.BusyoBases.CountAsync();
            Assert.AreEqual(BusyoBaseCount, baseCount, "部署BASEが件数増減していないはずです。");

            // 更新内容の確認
            var updateBase = await db.BusyoBases.FirstAsync(x => x.Id == model.Input.BusyoBaseId);
            Assert.AreEqual("既存部署BASE名称", updateBase.Name, "更新対象でない部署BASE.名前が変更されています。");
            Assert.AreEqual(model.Input.BumoncyoId, updateBase.BumoncyoId, "更新された部門長IDが一致しません。");

            // 他のレコードの確認
            var noTouchBase = await db.BusyoBases.FirstAsync(x => x.Id == 99);
            Assert.AreEqual("更新対象でない部署BASE", noTouchBase.Name, "更新対象でないレコードの部署BASE.名前が変更されています。");
            Assert.AreEqual(999, noTouchBase.BumoncyoId, "更新対象でないレコードの部署BASE.部門長IDが変更されています。");

            // ■部署マスタ
            // 件数が1件追加されていること
            var busyoCount = await db.Busyos.CountAsync();
            Assert.AreEqual(BusyoCount + 1, busyoCount, "部署が1件追加されているはずです。");

            // 更新内容の確認
            var updateBusyo = await db.Busyos.FirstAsync(x => x.Id == model.Input.BusyoId);
            Assert.AreEqual(model.Input.ApplyDate.AddDays(-1), updateBusyo.EndYmd, "有効終了日が画面.適用開始日の前日に更新されていません。");
            Assert.IsFalse(updateBusyo.IsActive, "アクティブフラグがFALSEに更新されていません。");

            // 他のレコードの確認
            var oyaBusyo = await db.Busyos.FirstAsync(x => x.Id == 2);
            Assert.AreEqual(Today.AddDays(1), oyaBusyo.EndYmd, "更新対象でないレコードの部署.有効終了日が変更されています。");
            Assert.IsTrue(oyaBusyo.IsActive, "更新対象でないレコードの部署.アクティブフラグが変更されています。");

            // 登録内容の確認
            var savedBusyo = await db.Busyos.OrderByDescending(x => x.Id).FirstAsync();
            Assert.AreEqual(model.Input.BusyoCode, savedBusyo.Code, "保存された部署番号が一致しません。");
            Assert.AreEqual(model.Input.BusyoName, savedBusyo.Name, "保存された部署名称が一致しません。");
            Assert.AreEqual(model.Input.BusyoKanaName, savedBusyo.KanaName, "保存された部署名称カナが一致しません。");
            Assert.AreEqual(model.Input.OyaCode, savedBusyo.OyaCode, "保存された親部署番号が一致しません。");
            Assert.AreEqual(model.Input.ApplyDate, savedBusyo.StartYmd, "保存された有効開始日が一致しません。");
            Assert.AreEqual(MaxEndYmd, savedBusyo.EndYmd, "保存された有効終了日が一致しません。");
            Assert.AreEqual(0, savedBusyo.Jyunjyo, "保存された順序が一致しません。");
            Assert.AreEqual(model.Input.KasyoCode, savedBusyo.KasyoCode, "保存された箇所コードが一致しません。");
            Assert.AreEqual(model.Input.KaikeiCode, savedBusyo.KaikeiCode, "保存された会計コードが一致しません。");
            Assert.AreEqual(model.Input.KeiriCode, savedBusyo.KeiriCode, "保存された経理コードが一致しません。");
            Assert.AreEqual(model.Input.IsActive, savedBusyo.IsActive, "保存されたアクティブフラグが一致しません。");
            Assert.AreEqual(model.Input.BusyoRyakusyou, savedBusyo.Ryakusyou, "保存された略称が一致しません。");
            Assert.AreEqual(model.Input.BusyoBaseId, savedBusyo.BusyoBaseId, "保存された部署BaseIDが一致しません。");
            Assert.AreEqual(model.Input.OyaId, savedBusyo.OyaId, "保存された親IDが一致しません。");
            Assert.AreEqual(model.Input.ShoninBusyoId, savedBusyo.ShoninBusyoId, "保存された承認部署IDが一致しません。");

            // ■社員マスタ
            // 件数が1件追加されていること
            var syainCount = await db.Syains.CountAsync();
            Assert.AreEqual(SyainCount + 1, syainCount, "社員が1件追加されているはずです。");

            // 更新内容の確認
            var updateSyain = await db.Syains.FirstAsync(x => x.Id == 1);
            Assert.AreEqual(model.Input.ApplyDate.AddDays(-1), updateSyain.EndYmd, "有効終了日が画面.適用開始日の前日に更新されていません。");

            // 他のレコードの確認
            var syainNotBusyo = await db.Syains.FirstAsync(x => x.Id == 2);
            Assert.AreEqual(Today.AddDays(1), syainNotBusyo.EndYmd, "更新対象でないレコードの社員マスタ.有効終了日が変更されています。");
            var syainBeforeStart = await db.Syains.FirstAsync(x => x.Id == 3);
            Assert.AreEqual(Today.AddDays(1), syainBeforeStart.EndYmd, "更新対象でないレコードの社員マスタ.有効終了日が変更されています。");
            var syainAfterEnd = await db.Syains.FirstAsync(x => x.Id == 4);
            Assert.AreEqual(Today.AddDays(-1), syainAfterEnd.EndYmd, "更新対象でないレコードの社員マスタ.有効終了日が変更されています。");

            // 登録内容の確認
            var insertSyain = await db.Syains.OrderByDescending(x => x.Id).FirstAsync();
            Assert.AreEqual(updateSyain.Code, insertSyain.Code, "保存された社員番号が一致しません。");
            Assert.AreEqual(updateSyain.Name, insertSyain.Name, "保存された社員氏名が一致しません。");
            Assert.AreEqual(updateSyain.KanaName, insertSyain.KanaName, "保存された社員氏名カナが一致しません。");
            Assert.AreEqual(updateSyain.Seibetsu, insertSyain.Seibetsu, "保存された性別が一致しません。");
            Assert.AreEqual(updateSyain.BusyoCode, insertSyain.BusyoCode, "保存された部署コードが一致しません。");
            Assert.AreEqual(updateSyain.SyokusyuCode, insertSyain.SyokusyuCode, "保存された職種コードが一致しません。");
            Assert.AreEqual(updateSyain.SyokusyuBunruiCode, insertSyain.SyokusyuBunruiCode, "保存された職種分類コードが一致しません。");
            Assert.AreEqual(updateSyain.NyuusyaYmd, insertSyain.NyuusyaYmd, "保存された入社年月日が一致しません。");
            Assert.AreEqual(model.Input.ApplyDate, insertSyain.StartYmd, "保存された有効開始日が一致しません。");
            Assert.AreEqual(MaxEndYmd, insertSyain.EndYmd, "保存された有効終了日が一致しません。");
            Assert.AreEqual(updateSyain.Kyusyoku, insertSyain.Kyusyoku, "保存された級職が一致しません。");
            Assert.AreEqual(updateSyain.SyucyoSyokui, insertSyain.SyucyoSyokui, "保存された出張職位が一致しません。");
            Assert.AreEqual(updateSyain.KingsSyozoku, insertSyain.KingsSyozoku, "保存されたKINGS所属が一致しません。");
            Assert.AreEqual(updateSyain.KaisyaCode, insertSyain.KaisyaCode, "保存された会社コードが一致しません。");
            Assert.AreEqual(updateSyain.IsGenkaRendou, insertSyain.IsGenkaRendou, "保存された原価連動フラグが一致しません。");
            Assert.AreEqual(updateSyain.EMail, insertSyain.EMail, "保存されたE-Mailアドレスが一致しません。");
            Assert.AreEqual(updateSyain.KeitaiMail, insertSyain.KeitaiMail, "保存された携帯Mailアドレスが一致しません。");
            Assert.AreEqual(updateSyain.Kengen, insertSyain.Kengen, "保存された社員権限が一致しません。");
            Assert.AreEqual(updateSyain.Jyunjyo, insertSyain.Jyunjyo, "保存された並び順序が一致しません。");
            Assert.AreEqual(updateSyain.Retired, insertSyain.Retired, "保存された退職フラグが一致しません。");
            Assert.AreEqual(updateSyain.GyoumuTypeId, insertSyain.GyoumuTypeId, "保存された業務タイプIDが一致しません。");
            Assert.AreEqual(updateSyain.PhoneNumber, insertSyain.PhoneNumber, "保存された電話番号が一致しません。");
            Assert.AreEqual(updateSyain.SyainBaseId, insertSyain.SyainBaseId, "保存された社員BaseIDが一致しません。");
            Assert.AreEqual(savedBusyo.Id, insertSyain.BusyoId, "保存された部署IDが一致しません。");
            Assert.AreEqual(updateSyain.KintaiZokuseiId, insertSyain.KintaiZokuseiId, "保存された勤怠属性IDが一致しません。");
            Assert.AreEqual(updateSyain.UserRoleId, insertSyain.UserRoleId, "保存されたユーザーロールIDが一致しません。");
        }

        /// <summary>
        /// NULLへの更新項目がある場合の、履歴あり更新。
        /// 部署BASEマスタへのUPDATE、部署マスタへのUPDATE(無効化)、部署マスタへのINSERT、
        /// 社員マスタへのUPDATE(無効化)、社員マスタへのINSERT　を確認する
        /// </summary>
        /// <param name="model">ページモデル</param>
        /// <returns></returns>
        public async Task 履歴あり更新NULL項目あり(InputModel model)
        {
            // Assert
            // ■部署BASEマスタ
            // 件数が変わっていないこと
            var baseCount = await db.BusyoBases.CountAsync();
            Assert.AreEqual(BusyoBaseCount, baseCount, "部署BASEが件数増減していないはずです。");

            // 更新内容の確認
            var updateBase = await db.BusyoBases.FirstAsync(x => x.Id == model.Input.BusyoBaseId);
            Assert.AreEqual("既存部署BASE名称", updateBase.Name, "更新対象でない部署BASE.名前が変更されています。");
            Assert.IsNull(updateBase.BumoncyoId, "更新された部門長IDが一致しません。");

            // 他のレコードの確認
            var noTouchBase = await db.BusyoBases.FirstAsync(x => x.Id == 99);
            Assert.AreEqual("更新対象でない部署BASE", noTouchBase.Name, "更新対象でないレコードの部署BASE.名前が変更されています。");
            Assert.AreEqual(999, noTouchBase.BumoncyoId, "更新対象でないレコードの部署BASE.部門長IDが変更されています。");

            // ■部署マスタ
            // 件数が1件追加されていること
            var busyoCount = await db.Busyos.CountAsync();
            Assert.AreEqual(BusyoCount + 1, busyoCount, "部署が1件追加されているはずです。");

            // 更新内容の確認
            var updateBusyo = await db.Busyos.FirstAsync(x => x.Id == model.Input.BusyoId);
            Assert.AreEqual(model.Input.ApplyDate.AddDays(-1), updateBusyo.EndYmd, "有効終了日が画面.適用開始日の前日に更新されていません。");
            Assert.IsFalse(updateBusyo.IsActive, "アクティブフラグがFALSEに更新されていません。");

            // 他のレコードの確認
            var oyaBusyo = await db.Busyos.FirstAsync(x => x.Id == 2);
            Assert.AreEqual(Today.AddDays(1), oyaBusyo.EndYmd, "更新対象でないレコードの部署.有効終了日が変更されています。");
            Assert.IsTrue(oyaBusyo.IsActive, "更新対象でないレコードの部署.アクティブフラグが変更されています。");

            // 登録内容の確認
            var savedBusyo = await db.Busyos.OrderByDescending(x => x.Id).FirstAsync();
            Assert.AreEqual(model.Input.BusyoCode, savedBusyo.Code, "保存された部署番号が一致しません。");
            Assert.AreEqual(model.Input.BusyoName, savedBusyo.Name, "保存された部署名称が一致しません。");
            Assert.AreEqual(model.Input.BusyoKanaName, savedBusyo.KanaName, "保存された部署名称カナが一致しません。");
            Assert.AreEqual(model.Input.OyaCode, savedBusyo.OyaCode, "保存された親部署番号が一致しません。");
            Assert.AreEqual(model.Input.ApplyDate, savedBusyo.StartYmd, "保存された有効開始日が一致しません。");
            Assert.AreEqual(MaxEndYmd, savedBusyo.EndYmd, "保存された有効終了日が一致しません。");
            Assert.AreEqual(0, savedBusyo.Jyunjyo, "保存された順序が一致しません。");
            Assert.AreEqual(model.Input.KasyoCode, savedBusyo.KasyoCode, "保存された箇所コードが一致しません。");
            Assert.AreEqual(model.Input.KaikeiCode, savedBusyo.KaikeiCode, "保存された会計コードが一致しません。");
            Assert.IsNull(savedBusyo.KeiriCode, "保存された経理コードが一致しません。");
            Assert.AreEqual(model.Input.IsActive, savedBusyo.IsActive, "保存されたアクティブフラグが一致しません。");
            Assert.IsNull(savedBusyo.Ryakusyou, "保存された略称が一致しません。");
            Assert.AreEqual(model.Input.BusyoBaseId, savedBusyo.BusyoBaseId, "保存された部署BaseIDが一致しません。");
            Assert.AreEqual(model.Input.OyaId, savedBusyo.OyaId, "保存された親IDが一致しません。");
            Assert.IsNull(savedBusyo.ShoninBusyoId, "保存された承認部署IDが一致しません。");

            // ■社員マスタ
            // 件数が1件追加されていること
            var syainCount = await db.Syains.CountAsync();
            Assert.AreEqual(SyainCount + 1, syainCount, "社員が1件追加されているはずです。");

            // 更新内容の確認
            var updateSyain = await db.Syains.FirstAsync(x => x.Id == 1);
            Assert.AreEqual(model.Input.ApplyDate.AddDays(-1), updateSyain.EndYmd, "有効終了日が画面.適用開始日の前日に更新されていません。");

            // 他のレコードの確認
            var syainNotBusyo = await db.Syains.FirstAsync(x => x.Id == 2);
            Assert.AreEqual(Today.AddDays(1), syainNotBusyo.EndYmd, "更新対象でないレコードの社員マスタ.有効終了日が変更されています。");
            var syainBeforeStart = await db.Syains.FirstAsync(x => x.Id == 3);
            Assert.AreEqual(Today.AddDays(1), syainBeforeStart.EndYmd, "更新対象でないレコードの社員マスタ.有効終了日が変更されています。");
            var syainAfterEnd = await db.Syains.FirstAsync(x => x.Id == 4);
            Assert.AreEqual(Today.AddDays(-1), syainAfterEnd.EndYmd, "更新対象でないレコードの社員マスタ.有効終了日が変更されています。");

            // 登録内容の確認
            var insertSyain = await db.Syains.OrderByDescending(x => x.Id).FirstAsync();
            Assert.AreEqual(updateSyain.Code, insertSyain.Code, "保存された社員番号が一致しません。");
            Assert.AreEqual(updateSyain.Name, insertSyain.Name, "保存された社員氏名が一致しません。");
            Assert.AreEqual(updateSyain.KanaName, insertSyain.KanaName, "保存された社員氏名カナが一致しません。");
            Assert.AreEqual(updateSyain.Seibetsu, insertSyain.Seibetsu, "保存された性別が一致しません。");
            Assert.AreEqual(updateSyain.BusyoCode, insertSyain.BusyoCode, "保存された部署コードが一致しません。");
            Assert.AreEqual(updateSyain.SyokusyuCode, insertSyain.SyokusyuCode, "保存された職種コードが一致しません。");
            Assert.AreEqual(updateSyain.SyokusyuBunruiCode, insertSyain.SyokusyuBunruiCode, "保存された職種分類コードが一致しません。");
            Assert.AreEqual(updateSyain.NyuusyaYmd, insertSyain.NyuusyaYmd, "保存された入社年月日が一致しません。");
            Assert.AreEqual(model.Input.ApplyDate, insertSyain.StartYmd, "保存された有効開始日が一致しません。");
            Assert.AreEqual(MaxEndYmd, insertSyain.EndYmd, "保存された有効終了日が一致しません。");
            Assert.AreEqual(updateSyain.Kyusyoku, insertSyain.Kyusyoku, "保存された級職が一致しません。");
            Assert.AreEqual(updateSyain.SyucyoSyokui, insertSyain.SyucyoSyokui, "保存された出張職位が一致しません。");
            Assert.AreEqual(updateSyain.KingsSyozoku, insertSyain.KingsSyozoku, "保存されたKINGS所属が一致しません。");
            Assert.AreEqual(updateSyain.KaisyaCode, insertSyain.KaisyaCode, "保存された会社コードが一致しません。");
            Assert.AreEqual(updateSyain.IsGenkaRendou, insertSyain.IsGenkaRendou, "保存された原価連動フラグが一致しません。");
            Assert.AreEqual(updateSyain.EMail, insertSyain.EMail, "保存されたE-Mailアドレスが一致しません。");
            Assert.AreEqual(updateSyain.KeitaiMail, insertSyain.KeitaiMail, "保存された携帯Mailアドレスが一致しません。");
            Assert.AreEqual(updateSyain.Kengen, insertSyain.Kengen, "保存された社員権限が一致しません。");
            Assert.AreEqual(updateSyain.Jyunjyo, insertSyain.Jyunjyo, "保存された並び順序が一致しません。");
            Assert.AreEqual(updateSyain.Retired, insertSyain.Retired, "保存された退職フラグが一致しません。");
            Assert.AreEqual(updateSyain.GyoumuTypeId, insertSyain.GyoumuTypeId, "保存された業務タイプIDが一致しません。");
            Assert.AreEqual(updateSyain.PhoneNumber, insertSyain.PhoneNumber, "保存された電話番号が一致しません。");
            Assert.AreEqual(updateSyain.SyainBaseId, insertSyain.SyainBaseId, "保存された社員BaseIDが一致しません。");
            Assert.AreEqual(savedBusyo.Id, insertSyain.BusyoId, "保存された部署IDが一致しません。");
            Assert.AreEqual(updateSyain.KintaiZokuseiId, insertSyain.KintaiZokuseiId, "保存された勤怠属性IDが一致しません。");
            Assert.AreEqual(updateSyain.UserRoleId, insertSyain.UserRoleId, "保存されたユーザーロールIDが一致しません。");
        }
    }
}