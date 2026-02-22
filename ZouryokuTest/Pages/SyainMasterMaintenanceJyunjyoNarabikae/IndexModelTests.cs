using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using Model.Model;
using Zouryoku.Pages.SyainMasterMaintenanceJyunjyoNarabikae;
using ZouryokuTest.Builder;
using ZouryokuTest.Pages.Builder;
using static Model.Enums.ResponseStatus;

namespace ZouryokuTest.Pages.SyainMasterMaintenanceJyunjyoNarabikae
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
        private IndexModel CreateModel() => new(db, GetLogger<IndexModel>(), options, viewEngine)
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
            var today = DateTime.Today.ToDateOnly();
            const long busyoId = 1;

            var syainA = AddNewSyain("社員A", busyoId: busyoId, 
                jyunjyo: 10, startYmd: today.AddDays(-1), 
                endYmd: today.AddDays(1), retired: false);

            AddNewSyain("社員B", busyoId: 2, 
                jyunjyo: 9, startYmd: today.AddDays(-1), 
                endYmd: today.AddDays(1), retired: false);

            AddNewSyain("社員C", busyoId: busyoId, 
                jyunjyo: 8, startYmd: today.AddDays(1), 
                endYmd: today.AddDays(10), retired: false);

            AddNewSyain("社員D", busyoId: busyoId, 
                jyunjyo: 7, startYmd: today.AddDays(-10), 
                endYmd: today.AddDays(-1), retired: false);

            AddNewSyain("社員E", busyoId: busyoId, 
                jyunjyo: 6, startYmd: today.AddDays(-1), 
                endYmd: today.AddDays(1), retired: true);

            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.BusyoId = busyoId;

            // Act
            var result = await model.OnGetSyainListAsync();

            // Assert
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(正常, GetResponseStatus(jsonResult));
            Assert.AreEqual(1, model.Syains.Count);
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
            const long busyoId = 1;
            var syainA = AddNewSyain("社員A", busyoId: busyoId, jyunjyo: 2);
            var syainB = AddNewSyain("社員B", busyoId: busyoId, jyunjyo: 1);
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.BusyoId = busyoId;

            // Act
            var result = await model.OnGetSyainListAsync();

            // Assert
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(正常, GetResponseStatus(jsonResult));
            Assert.AreEqual(2, model.Syains.Count);
            Assert.AreEqual("社員A", model.Syains[0].Name);
            Assert.AreEqual("社員B", model.Syains[1].Name);
            Assert.AreEqual(syainA.Id, model.Syains[0].Id);
            Assert.AreEqual((short)2, model.Syains[0].Jyunjyo);
            Assert.AreEqual(syainB.Id, model.Syains[1].Id);
            Assert.AreEqual((short)1, model.Syains[1].Jyunjyo);
        }

        /// <summary>
        /// ③初期表示: 条件に合致する社員が存在しない場合、正常な応答で空の一覧が返されることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "初期表示 目的：対象部署の社員が0件でも正常応答で空一覧を返す")]
        public async Task OnGetSyainListAsync_対象部署の社員が0件でも空一覧を返す()
        {
            // Arrange
            AddNewSyain("社員A", busyoId: 2, jyunjyo: 1);
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition.BusyoId = 1;

            // Act
            var result = await model.OnGetSyainListAsync();

            // Assert
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result);
            Assert.AreEqual(正常, GetResponseStatus(jsonResult));
            Assert.AreEqual(0, model.Syains.Count);
        }

        /// <summary>
        /// ④登録: 指定した社員の並び順（Jyunjyo）が正しく更新されることを確認します。
        /// </summary>
        [TestMethod(DisplayName = "登録 目的：指定した社員のJyunjyoを更新する")]
        public async Task OnPostRegisterAsync_指定した社員のJyunjyoを更新する()
        {
            // Arrange
            var syain1 = AddNewSyain("社員1", jyunjyo: 1);
            var syain2 = AddNewSyain("社員2", jyunjyo: 2);
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
        /// 新しい社員データを追加し、DBに登録します。
        /// </summary>
        /// <param name="name">社員名</param>
        /// <param name="busyoId">部署ID</param>
        /// <param name="jyunjyo">並び順</param>
        /// <param name="startYmd">有効開始日</param>
        /// <param name="endYmd">有効終了日</param>
        /// <param name="retired">退職フラグ</param>
        /// <returns>登録された社員エンティティ</returns>
        private Syain AddNewSyain(string name, long busyoId = 1, 
            short jyunjyo = 0, DateOnly? startYmd = null, 
            DateOnly? endYmd = null, bool retired = false)
        {
            EnsureBusyoExists(busyoId);

            var syainBaseId = syainBaseIdSeed++;
            var syainBasis = new SyainBasisBuilder()
                .WithId(syainBaseId)
                .WithName(name)
                .WithCode($"{syainBaseId:00000}")
                .Build();
            db.SyainBases.Add(syainBasis);

            var syain = new SyainBuilder()
                .WithId(syainIdSeed++)
                .WithSyainBaseId(syainBasis.Id)
                .WithCode(syainBasis.Code)
                .WithName(name)
                .WithBusyoCode(busyoId.ToString("000"))
                .WithBusyoId(busyoId)
                .WithJyunjyo(jyunjyo)
                .WithStartYmd(startYmd ?? DateTime.Today.AddMonths(-1).ToDateOnly())
                .WithEndYmd(endYmd ?? DateTime.Today.AddMonths(1).ToDateOnly())
                .WithRetired(retired)
                .WithUserRoleId(1)
                .WithKintaiZokuseiId(1)
                .Build();

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
                db.BusyoBases.Add(new BusyoBasisBuilder()
                    .WithId(busyoId)
                    .WithName($"部署{busyoId}")
                    .Build());
            }

            if (!db.Busyos.Local.Any(x => x.Id == busyoId) && !db.Busyos.Any(x => x.Id == busyoId))
            {
                db.Busyos.Add(new BusyoBuilder()
                    .WithId(busyoId)
                    .WithCode(busyoId.ToString("000"))
                    .WithName($"部署{busyoId}")
                    .WithBusyoBaseId(busyoId)
                    .WithJyunjyo((short)busyoId)
                    .WithIsActive(true)
                    .Build());
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
                db.KintaiZokuseis.Add(new KintaiZokuseiBuilder().WithId(1).WithName("通常").Build());
            }

            db.SaveChanges();
        }
    }
}

