using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Model.Enums;
using Model.Model;
using Zouryoku.Pages.Maintenance.Syains.Touroku;
using ZouryokuTest.Builder;
using ZouryokuTest.Pages.Builder;

namespace ZouryokuTest.Pages.Maintenance.Syains.Touroku
{
    /// <summary>
    /// 社員マスタ登録 画面モデルテスト
    /// </summary>
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {
        private static readonly DateOnly MaxEndYmd = new(9999, 12, 31);
        private static readonly DateOnly FixedToday = new(2025, 7, 1);

        private IndexModel CreateModel()
        {
            return new IndexModel(
              db,
              GetLogger<IndexModel>(),
              options,
              viewEngine,
              fakeTimeProvider)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };
        }

        private static object? GetData(JsonResult result)
        {
            var value = result.Value ?? throw new ArgumentException("JsonResult.Value が null です。");
            var prop = value.GetType().GetProperty("Data") ?? value.GetType().GetProperty("data");
            return prop?.GetValue(value);
        }

        private void SeedMasters()
        {
            db.AddRange(
                new BusyoBuilder().WithId(1).WithCode("101").WithName("BUSYO-1").WithBusyoBaseId(1).Build(),
                new BusyoBuilder().WithId(2).WithCode("102").WithName("BUSYO-2").WithBusyoBaseId(1).Build(),
                new KintaiZokuseiBuilder().WithId(1).WithName("KINTAI-1").Build(),
                new KintaiZokuseiBuilder().WithId(2).WithName("KINTAI-2").Build(),
                new UserRoleBuilder()
                    .WithId(1)
                    .WithCode(1)
                    .WithName("ROLE-1")
                    .WithJunjo(1)
                    .WithKengen((EmployeeAuthority)((1 << 0) | (1 << 14)))
                    .Build(),
                new UserRoleBuilder()
                    .WithId(2)
                    .WithCode(2)
                    .WithName("ROLE-2")
                    .WithJunjo(2)
                    .WithKengen((EmployeeAuthority)(1 << 4))
                    .Build(),
                new GyoumuType { Id = 1, Name = "TYPE-1", Jyunjyo = 1, Deleted = false },
                new GyoumuType { Id = 2, Name = "TYPE-2", Jyunjyo = 2, Deleted = false }
            );
        }

        private SyainBasis AddSyainBase(long id, string code, string name)
        {
            var syainBase = new SyainBasis
            {
                Id = id,
                Code = code,
                Name = name
            };
            db.SyainBases.Add(syainBase);
            return syainBase;
        }

        private Syain AddCurrentSyain(
            long id,
            long syainBaseId,
            string code,
            string name = "NAME-1",
            long busyoId = 1,
            DateOnly? startYmd = null)
        {
            var busyoCode = busyoId == 2 ? "102" : "101";
            var syain = new Syain
            {
                Id = id,
                Code = code,
                Name = name,
                KanaName = "KANA",
                Seibetsu = '1',
                BusyoCode = busyoCode,
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                StartYmd = startYmd ?? new DateOnly(2025, 1, 1),
                EndYmd = MaxEndYmd,
                Kyusyoku = 1,
                SyucyoSyokui = (BusinessTripRole)6,
                KingsSyozoku = "10000",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                EMail = "before@example.com",
                KeitaiMail = "before-mobile@example.com",
                Kengen = (EmployeeAuthority)((1 << 0) | (1 << 2)),
                Jyunjyo = 1,
                Retired = false,
                GyoumuTypeId = 1,
                PhoneNumber = "090-1111-2222",
                SyainBaseId = syainBaseId,
                BusyoId = busyoId,
                KintaiZokuseiId = 1,
                UserRoleId = 1
            };
            db.Syains.Add(syain);
            return syain;
        }

        private IndexModel.SyainInputModel CreateValidInput(
            bool isCreate,
            long id = 0,
            long syainBaseId = 0,
            string code = "10001",
            string name = "NAME-1",
            long busyoId = 1,
            string busyoCode = "101",
            DateOnly? startDate = null,
            DateOnly? startYmd = null,
            DateOnly? endYmd = null)
        {
            return new IndexModel.SyainInputModel
            {
                IsCreate = isCreate,
                Id = id,
                SyainBaseId = syainBaseId,
                Code = code,
                Name = name,
                KanaName = "KANA",
                NyuusyaYmd = new DateOnly(2020, 1, 1),
                Seibetsu = '1',
                BusyoId = busyoId,
                BusyoName = $"BUSYO-{busyoId}",
                BusyoCode = busyoCode,
                GyoumuTypeId = 1,
                StartDate = startDate ?? FixedToday,
                StartYmd = startYmd ?? new DateOnly(2025, 1, 1),
                EndYmd = endYmd ?? MaxEndYmd,
                Kyusyoku = 1,
                SyucyoSyokui = (BusinessTripRole)6,
                KingsSyozoku = "10000",
                KintaiZokuseiId = 1,
                IsGenkaRendou = false,
                KaisyaCode = 1,
                EMail = "after@example.com",
                KeitaiMail = "after-mobile@example.com",
                PhoneNumber = "090-1234-5678",
                Retired = false,
                Wariate = 10m,
                Kurikoshi = 2m,
                Syouka = 1m,
                HannitiKaisuu = 0,
                IsOvertimeExcessLimitStart = false,
                OvertimeExcessLimitYm = null,
                UserRoleId = 1,
                Perm1Checked = true,
                Perm3Checked = true,
                Perm15Checked = true,
            };
        }

        [TestMethod(DisplayName = "初期表示 目的：id未指定時に新規作成初期値と選択肢を設定すること 前提：" +
            "部署Id=1,2・勤怠属性Id=1,2・ロールId=1,2・業務種別Id=1,2が登録済み")]
        public async Task OnGetAsync_id未指定時に新規初期値が設定されること()
        {
            SeedMasters();
            await db.SaveChangesAsync();

            var model = CreateModel();
            var result = await model.OnGetAsync(null);

            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsTrue(model.Input.IsCreate);
            Assert.AreEqual(FixedToday, model.Input.StartDate);
            Assert.AreEqual(FixedToday, model.Input.StartYmd);
            Assert.AreEqual(MaxEndYmd, model.Input.EndYmd);
            Assert.IsTrue(model.GyoumuTypeOptions.Any());
            Assert.IsTrue(model.KintaiZokuseiOptions.Any());
            Assert.IsTrue(model.RoleOptions.Any());
            Assert.IsTrue(model.CompanyOptions.Any());
            Assert.IsTrue(model.SyucyoSyokuiOptions.Any());
        }

        [TestMethod(DisplayName = "初期表示 目的：社員BaseId指定時に現行社員と有給残を読込むこと 前提：" +
            "社員BaseId=10,社員Id=100,社員番号=10001,有給残Id=1が登録済み")]
        public async Task OnGetAsync_id指定時に既存社員情報を読込むこと()
        {
            var dateYmd = new DateOnly(2026, 1, 1);
            // fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            SeedMasters();
            var syainBase = AddSyainBase(10, "10001", "NAME-1");
            AddCurrentSyain(100, syainBase.Id, "10001");
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                Id = 1,
                SyainBaseId = syainBase.Id,
                Wariate = 15.5m,
                Kurikoshi = 1.5m,
                Syouka = 2.0m,
                HannitiKaisuu = 5,
                KeikakuYukyuSu = 0,
                KeikakuTokukyuSu = 0
            });
            db.OvertimeExcessLimits.Add(new OvertimeExcessLimit
            {
                Id = 1,
                SyainBaseId = syainBase.Id,
                DisabledYm = dateYmd
            });
            await db.SaveChangesAsync();

            var model = CreateModel();
            var result = await model.OnGetAsync(syainBase.Id);

            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsFalse(model.Input.IsCreate);
            Assert.AreEqual(100L, model.Input.Id);
            Assert.AreEqual(syainBase.Id, model.Input.SyainBaseId);
            Assert.AreEqual("10001", model.Input.Code);
            Assert.AreEqual(15.5m, model.Input.Wariate);
            Assert.AreEqual(1.5m, model.Input.Kurikoshi);
            Assert.AreEqual(2.0m, model.Input.Syouka);
            Assert.AreEqual((short)5, model.Input.HannitiKaisuu);
            Assert.IsTrue(model.Input.IsOvertimeExcessLimitStart);
            Assert.AreEqual("2026/01", model.Input.OvertimeExcessLimitYm);
            Assert.AreEqual(FixedToday, model.Input.StartDate);
            Assert.IsTrue(model.Input.Perm1Checked);
            Assert.IsTrue(model.Input.Perm3Checked);
            Assert.IsFalse(model.Input.Perm2Checked);
        }

        [TestMethod(DisplayName = "初期表示 目的：存在しない社員BaseId指定時にモデルエラーを設定すること 前提：" +
            "社員BaseId=999に対応する社員データが存在しない")]
        public async Task OnGetAsync_存在しない社員BaseId指定時にモデルエラーとなること()
        {
            SeedMasters();
            await db.SaveChangesAsync();

            var model = CreateModel();
            var result = await model.OnGetAsync(999);

            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsFalse(model.ModelState.IsValid);
            Assert.IsTrue(model.ModelState[string.Empty]!.Errors.Any());
            Assert.IsTrue(model.Input.IsCreate);
        }

        [TestMethod(DisplayName = "登録処理 目的：ModelState不正時に検証エラーJsonを返すこと 前提：" +
            "Input.Codeに手動エラーinvalidを追加した状態で新規入力を設定")]
        public async Task OnPostRegisterAsync_ModelState不正時にエラーJsonを返すこと()
        {
            var model = CreateModel();
            model.Input = CreateValidInput(isCreate: true);
            model.ModelState.AddModelError("Input.Code", "invalid");

            var result = await model.OnPostRegisterAsync();

            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.IsNotNull(GetErrors(json, "Input.Code"));
        }

        [TestMethod(DisplayName = "登録処理 目的：新規登録時に部署未存在ならエラー応答を返すこと 前提：" +
            "入力のBusyoId=999,BusyoCode=999で部署Id=999は未登録")]
        public async Task OnPostRegisterAsync_新規登録時に部署未存在ならエラーとなること()
        {
            SeedMasters();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Input = CreateValidInput(isCreate: true, busyoId: 999, busyoCode: "999");

            var result = await model.OnPostRegisterAsync();

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual(0, await db.SyainBases.CountAsync());
            Assert.AreEqual(0, await db.Syains.CountAsync());
        }

        [TestMethod(DisplayName = "登録処理 目的：新規登録時に社員番号重複ならエラー応答を返すこと 前提：" +
            "社員BaseId=10に社員Id=100,社員番号=10001が登録済み")]
        public async Task OnPostRegisterAsync_新規登録時に社員番号重複ならエラーとなること()
        {
            SeedMasters();
            var syainBase = AddSyainBase(10, "10001", "EXIST");
            AddCurrentSyain(100, syainBase.Id, "10001", "EXIST");
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Input = CreateValidInput(isCreate: true, code: "10001", name: "NEW-NAME");

            var result = await model.OnPostRegisterAsync();

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual(1, await db.Syains.CountAsync());
        }

        [TestMethod(DisplayName = "登録処理 目的：新規登録成功時に社員Base・社員・有給残を1件ずつ追加すること 前提：" +
            "社員番号=10001が未登録で参照マスタId=1系は有効")]
        public async Task OnPostRegisterAsync_新規登録成功時に社員関連データを追加すること()
        {
            var dateYmd = new DateOnly(2026, 1, 1);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            SeedMasters();
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Input = CreateValidInput(isCreate: true, code: "10001");
            model.Input.IsOvertimeExcessLimitStart = true;
            model.Input.OvertimeExcessLimitYm = "2026/01";

            var result = await model.OnPostRegisterAsync();

            AssertSuccess(result);
            Assert.AreEqual(1, await db.SyainBases.CountAsync());
            Assert.AreEqual(1, await db.Syains.CountAsync());
            Assert.AreEqual(1, await db.YuukyuuZans.CountAsync());
            Assert.AreEqual(1, await db.OvertimeExcessLimits.CountAsync());

            var inserted = await db.Syains.SingleAsync();
            var expected = (EmployeeAuthority)((1 << 0) | (1 << 2) | (1 << 14));
            Assert.AreEqual(expected, inserted.Kengen);
            Assert.AreEqual(1L, inserted.BusyoId);
            Assert.AreEqual((short)1, inserted.KaisyaCode);
            Assert.AreEqual(dateYmd, (await db.OvertimeExcessLimits.SingleAsync()).DisabledYm);
        }

        [TestMethod(DisplayName = "登録処理 目的：更新時に社員未存在ならエラー応答を返すこと 前提：" +
            "社員BaseId=10は登録済みで更新対象社員Id=999は未登録")]
        public async Task OnPostRegisterAsync_更新時に社員未存在ならエラーとなること()
        {
            SeedMasters();
            AddSyainBase(10, "10001", "BASE");
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Input = CreateValidInput(isCreate: false, id: 999, syainBaseId: 10);

            var result = await model.OnPostRegisterAsync();

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
        }

        [TestMethod(DisplayName = "登録処理 目的：更新時に社員Base未存在ならエラー応答を返すこと 前提：" +
            "社員Id=100は登録済みで更新対象社員BaseId=999は未登録")]
        public async Task OnPostRegisterAsync_更新時に社員Base未存在ならエラーとなること()
        {
            SeedMasters();
            AddCurrentSyain(100, 10, "10001");
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Input = CreateValidInput(isCreate: false, id: 100, syainBaseId: 999);

            var result = await model.OnPostRegisterAsync();

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
        }

        [TestMethod(DisplayName = "登録処理 目的：更新時に他社員と社員番号重複ならエラー応答を返すこと 前提：" +
            "社員BaseId=10の社員Id=100は社員番号10001,社員BaseId=20の社員Id=200は社員番号20000で登録済み")]
        public async Task OnPostRegisterAsync_更新時に他社員と社員番号重複ならエラーとなること()
        {
            SeedMasters();
            var base1 = AddSyainBase(10, "10001", "BASE-1");
            var base2 = AddSyainBase(20, "20000", "BASE-2");
            AddCurrentSyain(100, base1.Id, "10001", "TARGET");
            AddCurrentSyain(200, base2.Id, "20000", "OTHER");
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Input = CreateValidInput(isCreate: false, id: 100, syainBaseId: 10, code: "20000");

            var result = await model.OnPostRegisterAsync();

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var target = await db.Syains.SingleAsync(x => x.Id == 100);
            Assert.AreEqual("10001", target.Code);
        }

        [TestMethod(DisplayName = "登録処理 目的：履歴対象変更かつ適用開始日が有効開始日より前ならエラー応答を" +
            "返すこと 前提：社員Id=100の有効開始日=2025-02-01で入力の適用開始日=2025-01-01かつ氏名変更あり")]
        public async Task OnPostRegisterAsync_履歴変更かつ適用開始日が有効開始日より前ならエラーとなること()
        {
            var dateYmd = new DateOnly(2025, 2, 1);
            // fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            SeedMasters();
            var syainBase = AddSyainBase(10, "10001", "BASE");
            AddCurrentSyain(100, syainBase.Id, "10001", startYmd: dateYmd);
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Input = CreateValidInput(
                isCreate: false,
                id: 100,
                syainBaseId: 10,
                name: "CHANGED-NAME",
                startDate: dateYmd.AddMonths(-1),
                startYmd: dateYmd);

            var result = await model.OnPostRegisterAsync();

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual(1, await db.Syains.CountAsync());
            var current = await db.Syains.SingleAsync(x => x.Id == 100);
            Assert.AreEqual(MaxEndYmd, current.EndYmd);
        }

        [TestMethod(DisplayName = "登録処理 目的：履歴対象変更なしの更新で現行社員と有給残を更新すること 前提：" +
            "社員BaseId=10の社員Id=100と有給残Id=1が登録済みで入力の履歴対象項目は既存値と同一")]
        public async Task OnPostRegisterAsync_履歴変更なし更新で現行社員と有給残を更新すること()
        {
            var dateYmd = new DateOnly(2025, 1, 1);
            // fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            SeedMasters();
            var syainBase = AddSyainBase(10, "10001", "BASE");
            AddCurrentSyain(100, syainBase.Id, "10001");
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                Id = 1,
                SyainBaseId = syainBase.Id,
                Wariate = 1m,
                Kurikoshi = 1m,
                Syouka = 1m,
                HannitiKaisuu = 0,
                KeikakuYukyuSu = 0,
                KeikakuTokukyuSu = 0
            });
            db.OvertimeExcessLimits.Add(new OvertimeExcessLimit
            {
                Id = 1,
                SyainBaseId = syainBase.Id,
                DisabledYm = dateYmd.AddMonths(11)
            });
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Input = CreateValidInput(
                isCreate: false,
                id: 100,
                syainBaseId: 10,
                code: "10001",
                name: "NAME-1",
                startDate: dateYmd.AddMonths(2),
                startYmd: dateYmd,
                endYmd: MaxEndYmd);
            model.Input.EMail = "updated@example.com";
            model.Input.Wariate = 9m;
            model.Input.Kurikoshi = 4m;
            model.Input.Syouka = 2m;
            model.Input.IsOvertimeExcessLimitStart = true;
            model.Input.OvertimeExcessLimitYm = "2026/02";

            var result = await model.OnPostRegisterAsync();

            AssertSuccess(result);
            Assert.AreEqual(1, await db.Syains.CountAsync());

            var syain = await db.Syains.SingleAsync(x => x.Id == 100);
            Assert.AreEqual("updated@example.com", syain.EMail);
            Assert.AreEqual(dateYmd, syain.StartYmd);
            Assert.AreEqual(MaxEndYmd, syain.EndYmd);

            var leave = await db.YuukyuuZans.SingleAsync(x => x.SyainBaseId == 10);
            Assert.AreEqual(9m, leave.Wariate);
            Assert.AreEqual(4m, leave.Kurikoshi);
            Assert.AreEqual(2m, leave.Syouka);

            var overtimeLimit = await db.OvertimeExcessLimits.SingleAsync(x => x.SyainBaseId == 10);
            Assert.AreEqual(dateYmd.AddMonths(13), overtimeLimit.DisabledYm);
        }

        [TestMethod(DisplayName = "登録処理 目的：残業超過制限開始フラグがOFFなら対象社員の残業超過制限設定を" +
            "削除すること 前提：社員BaseId=10の残業超過制限が1件存在する")]
        public async Task OnPostRegisterAsync_残業超過制限開始フラグオフで残業超過制限を削除すること()
        {
            var dateYmd = new DateOnly(2025, 12, 1);
            fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            SeedMasters();
            var syainBase = AddSyainBase(10, "10001", "BASE");
            AddCurrentSyain(100, syainBase.Id, "10001");
            db.OvertimeExcessLimits.Add(new OvertimeExcessLimit
            {
                Id = 1,
                SyainBaseId = syainBase.Id,
                DisabledYm = dateYmd
            });
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Input = CreateValidInput(isCreate: false, id: 100, syainBaseId: 10);
            model.Input.IsOvertimeExcessLimitStart = false;
            model.Input.OvertimeExcessLimitYm = null;

            var result = await model.OnPostRegisterAsync();

            AssertSuccess(result);
            Assert.AreEqual(0, await db.OvertimeExcessLimits.CountAsync());
        }

        [TestMethod(DisplayName = "登録処理 目的：履歴対象変更ありの更新で既存終了日を調整し新規履歴を" +
            "追加すること 前提：社員Id=100の有効開始日=2025-01-01,適用開始日=2025-02-10,部署をId=1からId=2へ変更")]
        public async Task OnPostRegisterAsync_履歴変更あり更新で履歴分割して新規社員を追加すること()
        {
            var dateYmd = new DateOnly(2025, 1, 1);
            // fakeTimeProvider.SetLocalNow(dateYmd.ToDateTime());
            SeedMasters();
            var syainBase = AddSyainBase(10, "10001", "BASE");
            AddCurrentSyain(100, syainBase.Id, "10001", busyoId: 1, startYmd: dateYmd);
            await db.SaveChangesAsync();

            var applyDate = new DateOnly(2025, 2, 10);
            var model = CreateModel();
            model.Input = CreateValidInput(
                isCreate: false,
                id: 100,
                syainBaseId: 10,
                code: "10001",
                name: "NAME-1",
                busyoId: 2,
                busyoCode: "102",
                startDate: applyDate,
                startYmd: dateYmd,
                endYmd: MaxEndYmd);

            var result = await model.OnPostRegisterAsync();

            AssertSuccess(result);
            Assert.AreEqual(2, await db.Syains.CountAsync());

            var oldSyain = await db.Syains.SingleAsync(x => x.Id == 100);
            Assert.AreEqual(applyDate.AddDays(-1), oldSyain.EndYmd);

            var newSyain = await db.Syains.SingleAsync(x => x.Id != 100);
            Assert.AreEqual(applyDate, newSyain.StartYmd);
            Assert.AreEqual(MaxEndYmd, newSyain.EndYmd);
            Assert.AreEqual(2L, newSyain.BusyoId);
            Assert.AreEqual(1, await db.YuukyuuZans.CountAsync());
        }

        [TestMethod(DisplayName = "ロール既定権限取得 目的：ロール未存在時にエラーJsonを返すこと 前提：" +
            "検索ロールId=999が未登録")]
        public async Task OnGetRoleDefaultsAsync_ロール未存在ならエラーJsonを返すこと()
        {
            var model = CreateModel();

            var result = await model.OnGetRoleDefaultsAsync(999);

            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(3, (int)GetResponseStatus(json));
            var message = GetMessage(json);
            Assert.IsFalse(string.IsNullOrWhiteSpace(message));
        }

        [TestMethod(DisplayName = "ロール既定権限取得 目的：ロール存在時に権限ビットを真偽値へ展開して返すこと 前提：" +
            "ロールId=50の権限はbit0,bit4,bit14を保持")]
        public async Task OnGetRoleDefaultsAsync_ロール存在時に権限ビットを返すこと()
        {
            db.UserRoles.Add(new UserRole
            {
                Id = 50,
                Code = 50,
                Name = "ROLE-50",
                Jyunjo = 1,
                Kengen = (EmployeeAuthority)((1 << 0) | (1 << 4) | (1 << 14))
            });
            await db.SaveChangesAsync();

            var model = CreateModel();
            var result = await model.OnGetRoleDefaultsAsync(50);

            var json = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(ResponseStatus.正常, GetResponseStatus(json));

            var html = GetData(json);
            Assert.IsNotNull(html);

            Assert.IsTrue(model.Input.Perm1Checked);
            Assert.IsTrue(model.Input.Perm5Checked);
            Assert.IsTrue(model.Input.Perm15Checked);
            Assert.IsFalse(model.Input.Perm2Checked);
            Assert.IsFalse(model.Input.Perm14Checked);
        }
    }
}
