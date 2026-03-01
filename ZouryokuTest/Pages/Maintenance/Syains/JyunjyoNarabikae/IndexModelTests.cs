using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using Model.Model;
using Zouryoku.Pages.Maintenance.Syains.JyunjyoNarabikae;
using static Model.Enums.ResponseStatus;

namespace ZouryokuTest.Pages.Maintenance.Syains.JyunjyoNarabikae
{
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {
        private long syainIdSeed = 1;
        private long syainBaseIdSeed = 1;

        /// <summary>
        /// テスト対象の IndexModel を生成します。
        /// </summary>
        /// <returns>生成された IndexModel</returns>
        private IndexModel CreateModel() => new(
          db,
          GetLogger<IndexModel>(),
          options,
          viewEngine,
          fakeTimeProvider)
        {
            PageContext = GetPageContext(),
            TempData = GetTempData(),
            Condition = new SearchCondition()
        };

        /// <summary>
        /// ①初期表示: 有効期間内かつ未退職の社員のみが取得されることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "初期表示 目的：有効期間内かつ未退職の社員のみ取得")]
        public async Task OnGetSyainListAsync_有効期間内かつ未退職の社員のみ取得()
        {
            // Arrange
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            const long busyoId = 1;

            var syainA = AddNewSyain(today: today, "社員A", busyoId: busyoId,
                jyunjyo: 10, startYmd: today.AddDays(-1),
                endYmd: today.AddDays(1), retired: false);

            AddNewSyain(today: today, "社員B", busyoId: 2,
                jyunjyo: 9, startYmd: today.AddDays(-1),
                endYmd: today.AddDays(1), retired: false);

            AddNewSyain(today: today, "社員C", busyoId: busyoId,
                jyunjyo: 8, startYmd: today.AddDays(1),
                endYmd: today.AddDays(10), retired: false);

            AddNewSyain(today: today, "社員D", busyoId: busyoId,
                jyunjyo: 7, startYmd: today.AddDays(-10),
                endYmd: today.AddDays(-1), retired: false);

            AddNewSyain(today: today, "社員E", busyoId: busyoId,
                jyunjyo: 6, startYmd: today.AddDays(-1),
                endYmd: today.AddDays(1), retired: true);

            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.BusyoId = busyoId;

            // Act
            var result = await model.OnGetSyainListAsync();

            // Assert
            IEnumerable<int> syainCount = new List<int> { model.Syains.Count };
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(正常, GetResponseStatus(jsonResult));
            Assert.HasCount(1, syainCount);
            Assert.AreEqual("社員A", model.Syains[0].Name);
            Assert.AreEqual(syainA.Id, model.Syains[0].Id);
            Assert.AreEqual((short)10, model.Syains[0].Jyunjyo);
        }

        /// <summary>
        /// ②初期表示: 取得された社員が並び順（Jyunjyo）の降順でソートされていることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "初期表示 目的：社員がJyunjyoの降順で並ぶことを確認")]
        public async Task OnGetSyainListAsync_Jyunjyoの降順で並ぶ()
        {
            // Arrange
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            const long busyoId = 1;
            var syainA = AddNewSyain(today: today, "社員A", busyoId: busyoId, jyunjyo: 2);
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.BusyoId = busyoId;

            // Act
            var result = await model.OnGetSyainListAsync();

            // Assert
            var syainCount = new List<int> { model.Syains.Count };
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(正常, GetResponseStatus(jsonResult));
            Assert.HasCount(1, syainCount);
            Assert.AreEqual("社員A", model.Syains[0].Name);
            Assert.AreEqual(syainA.Id, model.Syains[0].Id);
            Assert.AreEqual((short)2, model.Syains[0].Jyunjyo);
        }

        /// <summary>
        /// ③初期表示: 条件に合致する社員が存在しない場合、正常な応答で空の一覧が返されることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "初期表示 目的：対象部署の社員が0件でも正常応答で空一覧を返す")]
        public async Task OnGetSyainListAsync_対象部署の社員が0件でも空一覧を返す()
        {
            // Arrange
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            AddNewSyain(today: today, "社員A", busyoId: 2, jyunjyo: 1);
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.BusyoId = 1;

            // Act
            var result = await model.OnGetSyainListAsync();

            // Assert
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(正常, GetResponseStatus(jsonResult));
            Assert.IsEmpty(model.Syains);
        }

        /// <summary>
        /// ④登録: 指定した社員の並び順（Jyunjyo）が正しく更新されることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "登録 目的：指定した社員のJyunjyoを更新する")]
        public async Task OnPostRegisterAsync_指定した社員のJyunjyoを更新する()
        {
            // Arrange
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var syain1 = AddNewSyain(today: today, "社員1", jyunjyo: 1);
            var syain2 = AddNewSyain(today: today, "社員2", jyunjyo: 2);
            await db.SaveChangesAsync();

            var model = CreateModel();
            var request = new List<SyainOrderModel>
            {
                new() { Id = syain1.Id, Jyunjyo = 10 },
                new() { Id = syain2.Id, Jyunjyo = 20 }
            };

            // Act
            var result = await model.OnPostRegisterAsync(request);

            // Assert
            AssertSuccess(result);
            Assert.AreEqual((short)10, db.Syains.Find(syain1.Id)!.Jyunjyo);
            Assert.AreEqual((short)20, db.Syains.Find(syain2.Id)!.Jyunjyo);
        }

        /// <summary>
        /// 並び順保存処理 目的：更新対象の社員が存在しない場合　前提：社員A(Id=1,Jyunjyo=1)のみ存在
        /// </summary>
        [TestMethod(DisplayName = "並び順保存処理 目的：更新対象の社員が存在しない場合　前提：社員A(Id=1,Jyunjyo=1)のみ存在" +
            "社員1(Id=1, Jyunjyo=1)のみ存在")]
        public async Task OnPostRegisterAsync_更新対象の社員が存在しない場合()
        {
            // Arrange
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var syain1 = AddNewSyain(today: today, "社員1", jyunjyo: 1);
            await db.SaveChangesAsync();

            var model = CreateModel();
            var request = new List<SyainOrderModel>
            {
                new() { Id = 2, Jyunjyo = 1 }
            };

            // Act
            var result = await model.OnPostRegisterAsync(request);

            // Assert
            AssertError(result, IndexModel.ErrorConflictSyain);
        }

        /// <summary>
        /// 並び順保存処理 目的：同時実行制御が発動した場合 前提：社員A(Id=1,Jyunjyo=1)
        /// </summary>
        [TestMethod(DisplayName = "並び順保存処理 目的：同時実行制御が発動した場合 前提：社員1(Id=1,Jyunjyo=1)")]
        public async Task OnPostRegisterAsync_同時実行制御が発動した場合()
        {
            // Arrange
            var today = new DateOnly(2026, 7, 15);
            fakeTimeProvider.SetLocalNow(today.ToDateTime());
            var syain1 = AddNewSyain(today: today, "社員1", jyunjyo: 1);
            await db.SaveChangesAsync();

            var model = CreateModel();
            var request = new List<SyainOrderModel>
            {
                new() { Id = syain1.Id, Jyunjyo = 10, Version = syain1.Version + 1 }
            };

            // Act
            var result = await model.OnPostRegisterAsync(request);

            // Assert
            AssertError(result, IndexModel.ErrorConflictSyain);
        }

        /// <summary>
        /// 新しい社員データを追加し、DBに登録します。
        /// </summary>
        /// <param name="name">社員名</param>
        /// <param name="busyoId">部署ID</param>
        /// <param name="jyunjyo">並び順</param>
        /// <param name="startYmd">有効開始日</param>
        /// <param name="endYmd">有効終了日</param>
        /// <param name="retired">退職フラグ</param>
        /// <returns>登録された社員エンティティ</returns>
        private Syain AddNewSyain(
            DateOnly today,
            string name = "", long busyoId = 1,
            short jyunjyo = 0, DateOnly? startYmd = null,
            DateOnly? endYmd = null, bool retired = false)
        {
            EnsureBusyoExists(busyoId);

            var syainBaseId = syainBaseIdSeed++;
            var syainBasis = CreateSyainBasis(
                id: syainBaseId,
                name: name,
                code: $"{syainBaseId:00000}");
            db.SyainBases.Add(syainBasis);

            var syain = CreateSyain(
                id: syainIdSeed++,
                syainBaseId: syainBasis.Id,
                code: syainBasis.Code,
                name: name,
                busyoCode: busyoId.ToString("000"),
                busyoId: busyoId,
                jyunjyo: jyunjyo,
                startYmd: startYmd ?? today.AddMonths(-1),
                endYmd: endYmd ?? today.AddMonths(1),
                retired: retired,
                userRoleId: 1,
                kintaiZokuseiId: 1);

            db.Syains.Add(syain);
            return syain;
        }

        /// <summary>
        /// 指定した部署IDの部署が存在することを保証します（存在しない場合は生成します）。
        /// </summary>
        /// <param name="busyoId">部署ID</param>
        private void EnsureBusyoExists(long busyoId)
        {
            if (!db.BusyoBases.Local.Any(x => x.Id == busyoId) && !db.BusyoBases.Any(x => x.Id == busyoId))
            {
                var busyoBasis = CreateBusyoBasis(
                    id: busyoId,
                    name: $"部署{busyoId}");
                db.BusyoBases.Add(busyoBasis);
            }

            if (!db.Busyos.Local.Any(x => x.Id == busyoId) && !db.Busyos.Any(x => x.Id == busyoId))
            {
                var busyo = CreateBusyo(
                    id: busyoId,
                    busyoBaseId: busyoId,
                    code: busyoId.ToString("000"),
                    name: $"部署{busyoId}",
                    jyunjyo: (short)busyoId,
                    isActive: true);
                db.Busyos.Add(busyo);
            }
        }

        /// <summary>
        /// 各テスト実行前の共通初期化処理です。
        /// </summary>
        [TestInitialize]
        public void SeedCommonData()
        {
            if (!db.UserRoles.Any())
            {
                db.UserRoles.Add(new UserRole { Id = 1, Code = 1, Name = "一般", Jyunjo = 1 });
            }

            if (!db.KintaiZokuseis.Any())
            {
                var kintaiZokusei = CreateKintaiZokusei(
                    id: 1,
                    name: "通常");
                db.KintaiZokuseis.Add(kintaiZokusei);
            }

            db.SaveChanges();
        }

        private static BusyoBasis CreateBusyoBasis(
            long? id = 1,
            string? name = null,
            long? bumoncyoId = 0)
        {
            return new BusyoBasis
            {
                Id = id ?? 1,
                Name = name?.Trim() ?? $"部署{id}",
                BumoncyoId = bumoncyoId
            };
        }

        private static Busyo CreateBusyo(
            long? id = 1,
            string? code = null,
            string? name = null,
            string? kanaName = null,
            string? oyaCode = null,
            DateOnly? startYmd = null,
            DateOnly? endYmd = null,
            short? jyunjyo = 1,
            string? kasyoCode = null,
            string? kaikeiCode = null,
            string? keiriCode = null,
            bool? isActive = true,
            string? ryakusyou = null,
            long? busyoBaseId = 1,
            long? oyaId = 0,
            long? shoninBusyoId = 0)
        {
            var result = new Busyo()
            {
                Code = code?.Trim() ?? $"B{id:D4}",
                Name = name?.Trim() ?? $"部署{id}",
                KanaName = kanaName?.Trim() ?? $"ブショ{id}",
                OyaCode = oyaCode?.Trim() ?? $"OB{id:D4}",
                StartYmd = startYmd ?? DateOnly.MinValue,
                EndYmd = endYmd ?? DateOnly.MaxValue,
                Jyunjyo = jyunjyo ?? 1,
                KasyoCode = kasyoCode?.Trim() ?? $"KAS{id:D4}",
                KaikeiCode = kaikeiCode?.Trim() ?? $"KK{id:D4}",
                KeiriCode = keiriCode?.Trim() ?? $"KR{id:D4}",
                IsActive = isActive ?? true,
                Ryakusyou = ryakusyou?.Trim() ?? $"R{id}",
                BusyoBaseId = busyoBaseId ?? 1,
                OyaId = oyaId ?? 0,
                ShoninBusyoId = shoninBusyoId ?? 0
            };

            if (id.HasValue)
            {
                result.Id = id.Value;
            }

            return result;
        }

        private static SyainBasis CreateSyainBasis(
            long? id = 1,
            string? name = null,
            string? code = null)
        {
            var result = new SyainBasis
            {
                Name = name?.Trim() ?? $"社員{id}",
                Code = code?.Trim() ?? $"S{id:D4}"
            };

            if (id.HasValue)
            {
                result.Id = id.Value;
            }

            return result;
        }

        private static Syain CreateSyain(
            long? id = 1,
            string? code = null,
            string? name = null,
            string? kanaName = null,
            char? seibetsu = null,
            string? busyoCode = null,
            int? syokusyuCode = null,
            int? syokusyuBunruiCode = null,
            DateOnly? nyushaYmd = null,
            DateOnly? startYmd = null,
            DateOnly? endYmd = null,
            short? kyusyoku = 0,
            BusinessTripRole? syucyoSyokui = BusinessTripRole._2_6級,
            string? kingsSyozoku = null,
            short? kaisyaCode = 0,
            bool? isGenkaRendou = false,
            string? eMail = null,
            string? keitaiMail = null,
            EmployeeAuthority? kengen = EmployeeAuthority.None,
            short? jyunjyo = 0,
            bool? retired = false,
            long? gyoumuTypeId = 1,
            string? phoneNumber = null,
            long? syainBaseId = 1,
            long? busyoId = 1,
            long? kintaiZokuseiId = 1,
            long? userRoleId = 1)
        {
            var result = new Syain
            {
                Code = code?.Trim() ?? $"S{id:D4}",
                Name = name?.Trim() ?? $"社員{id}",
                KanaName = kanaName?.Trim() ?? $"シャイン{id}",
                Seibetsu = seibetsu ?? '1',
                BusyoCode = busyoCode?.Trim() ?? $"B{id:D4}",
                SyokusyuCode = syokusyuCode ?? 0,
                SyokusyuBunruiCode = syokusyuBunruiCode ?? 0,
                NyuusyaYmd = nyushaYmd ?? new DateOnly(2020, 1, 1),
                StartYmd = startYmd ?? DateOnly.MinValue,
                EndYmd = endYmd ?? DateOnly.MaxValue,
                Kyusyoku = kyusyoku ?? 0,
                SyucyoSyokui = syucyoSyokui ?? BusinessTripRole._2_6級,
                KingsSyozoku = kingsSyozoku?.Trim() ?? $"K{id:D4}",
                KaisyaCode = kaisyaCode ?? 0,
                IsGenkaRendou = isGenkaRendou ?? false,
                EMail = eMail?.Trim() ?? $"syain{id}@example.com",
                KeitaiMail = keitaiMail?.Trim() ?? $"keitai{id}@example.com",
                Kengen = kengen ?? EmployeeAuthority.None,
                Jyunjyo = jyunjyo ?? 0,
                Retired = retired ?? false,
                GyoumuTypeId = gyoumuTypeId,
                PhoneNumber = phoneNumber,
                SyainBaseId = syainBaseId ?? 1,
                BusyoId = busyoId ?? 1,
                KintaiZokuseiId = kintaiZokuseiId ?? 1,
                UserRoleId = userRoleId ?? 1,
            };

            if (id.HasValue)
            {
                result.Id = id.Value;
            }

            return result;
        }

        private static KintaiZokusei CreateKintaiZokusei(
            long? id = 1,
            string? name = null,
            decimal? seigenTime = 0.00m,
            bool? isMinashi = false,
            decimal? maxLimitTime = null,
            bool? isOvertimeLimit3m = false,
            EmployeeWorkType? code = EmployeeWorkType.月45時間)
        {
            var result = new KintaiZokusei
            {
                Name = name?.Trim() ?? "標準",
                SeigenTime = seigenTime ?? 45.00m,
                IsMinashi = isMinashi ?? false,
                MaxLimitTime = maxLimitTime ?? 0m,
                IsOvertimeLimit3m = isOvertimeLimit3m ?? false,
                Code = code ?? EmployeeWorkType.月45時間
            };

            if (id.HasValue)
            {
                result.Id = id.Value;
            }

            return result;
        }
    }
}
