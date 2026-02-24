using CommonLibrary.Extensions;
using Microsoft.EntityFrameworkCore;
using Model.Model;

namespace ZouryokuTest.Utils
{
    /// <summary>
    /// ImpactDepartment 影響部署取得 のユニットテスト
    /// </summary>
    [TestClass]
    public class ImpactDepartmentTests : BaseInMemoryDbContextTest
    {
        /// <summary>
        /// 影響範囲部署取得
        ///     テストケース1：子部署在籍（部署1-1）
        ///                     親部署とその子部署（部門長IDがないもの）すべてを取得し、影響範囲外の部署は取得しないこと
        /// </summary>
        [TestMethod]
        public async Task GetImpactDepartmentAsync_WhenChildDepartmentSelected_ReturnsParentAndChildrenWithoutManager()
        {
            // Arrange
            db.BusyoBases.Add(new BusyoBasis { Id = 1, Name = "部署1", BumoncyoId = 9999 });
            db.BusyoBases.Add(new BusyoBasis { Id = 2, Name = "部署1-1", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 3, Name = "部署1-2", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 4, Name = "部署2", BumoncyoId = 8888 });
            db.BusyoBases.Add(new BusyoBasis { Id = 5, Name = "部署2-1", BumoncyoId = null });

            db.Busyos.Add(new Busyo { Id = 1, Code = "001", Name = "部署1", IsActive = true, Jyunjyo = 5, OyaCode = " ", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2010/4/1"), EndYmd = DateOnly.Parse("2030/4/1"), OyaId = null, BusyoBaseId = 1, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 2, Code = "002", Name = "部署1-1", IsActive = true, Jyunjyo = 4, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2011/4/1"), EndYmd = DateOnly.Parse("2031/4/1"), OyaId = 1, BusyoBaseId = 2, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 3, Code = "003", Name = "部署1-2", IsActive = true, Jyunjyo = 3, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2012/4/1"), EndYmd = DateOnly.Parse("2032/4/1"), OyaId = 1, BusyoBaseId = 3, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 4, Code = "004", Name = "部署2", IsActive = true, Jyunjyo = 2, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2013/4/1"), EndYmd = DateOnly.Parse("2033/4/1"), OyaId = 1, BusyoBaseId = 4, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 5, Code = "005", Name = "部署2-1", IsActive = true, Jyunjyo = 1, OyaCode = "004", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2014/4/1"), EndYmd = DateOnly.Parse("2034/8/1"), OyaId = 4, BusyoBaseId = 5, ShoninBusyoId = null });

            await db.SaveChangesAsync();

            // Act
            var data = await Zouryoku.Utils.ImpactDepartment.GetImpactDepartmentAsync(db, 2, fakeTimeProvider.Today());

            // Assert
            Assert.HasCount(3, data, "戻り値の件数が一致しません。");

            Assert.AreEqual(3, data[0].Id, "1件目の Id が一致しません。");
            Assert.AreEqual("003", data[0].Code, "1件目の Code が一致しません。");
            Assert.AreEqual("部署1-2", data[0].Name, "1件目の Name が一致しません。");
            Assert.IsTrue(data[0].IsActive, "1件目の IsActive が一致しません。");
            Assert.AreEqual((short)3, data[0].Jyunjyo, "1件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2012/4/1"), data[0].StartYmd, "1件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2032/4/1"), data[0].EndYmd, "1件目の EndYmd が一致しません。");
            Assert.AreEqual(1, data[0].OyaId, "1件目の OyaId が一致しません。");
            Assert.IsNull(data[0].BusyoBase.BumoncyoId, "1件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[0].ShoninBusyoId, "1件目の ShoninBusyoId が一致しません。");

            Assert.AreEqual(2, data[1].Id, "2件目の Id が一致しません。");
            Assert.AreEqual("002", data[1].Code, "2件目の Code が一致しません。");
            Assert.AreEqual("部署1-1", data[1].Name, "2件目の Name が一致しません。");
            Assert.IsTrue(data[1].IsActive, "2件目の IsActive が一致しません。");
            Assert.AreEqual((short)4, data[1].Jyunjyo, "2件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2011/4/1"), data[1].StartYmd, "2件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2031/4/1"), data[1].EndYmd, "2件目の EndYmd が一致しません。");
            Assert.AreEqual(1, data[1].OyaId, "2件目の OyaId が一致しません。");
            Assert.IsNull(data[1].BusyoBase.BumoncyoId, "2件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[1].ShoninBusyoId, "2件目の ShoninBusyoId が一致しません。");

            Assert.AreEqual(1, data[2].Id, "3件目の Id が一致しません。");
            Assert.AreEqual("001", data[2].Code, "3件目の Code が一致しません。");
            Assert.AreEqual("部署1", data[2].Name, "3件目の Name が一致しません。");
            Assert.IsTrue(data[2].IsActive, "3件目の IsActive が一致しません。");
            Assert.AreEqual((short)5, data[2].Jyunjyo, "3件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2010/4/1"), data[2].StartYmd, "3件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2030/4/1"), data[2].EndYmd, "3件目の EndYmd が一致しません。");
            Assert.IsNull(data[2].OyaId, "3件目の OyaId が一致しません。");
            Assert.AreEqual(9999, data[2].BusyoBase.BumoncyoId, "3件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[2].ShoninBusyoId, "3件目の ShoninBusyoId が一致しません。");
        }

        /// <summary>
        /// 影響範囲部署取得
        ///     テストケース2：親部署在籍（部署1）
        ///                     その子部署（部門長IDがないもの）すべてを取得し、影響範囲外の部署は取得しないこと
        /// </summary>
        [TestMethod]
        public async Task GetImpactDepartmentAsync_WhenParentDepartmentSelected_ReturnsAllChildrenWithoutManager()
        {
            // Arrange
            db.BusyoBases.Add(new BusyoBasis { Id = 1, Name = "部署1", BumoncyoId = 9999 });
            db.BusyoBases.Add(new BusyoBasis { Id = 2, Name = "部署1-1", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 3, Name = "部署1-2", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 4, Name = "部署2", BumoncyoId = 8888 });
            db.BusyoBases.Add(new BusyoBasis { Id = 5, Name = "部署2-1", BumoncyoId = null });

            db.Busyos.Add(new Busyo { Id = 1, Code = "001", Name = "部署1", IsActive = true, Jyunjyo = 5, OyaCode = " ", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2010/4/1"), EndYmd = DateOnly.Parse("2030/4/1"), OyaId = null, BusyoBaseId = 1, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 2, Code = "002", Name = "部署1-1", IsActive = true, Jyunjyo = 4, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2011/4/1"), EndYmd = DateOnly.Parse("2031/4/1"), OyaId = 1, BusyoBaseId = 2, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 3, Code = "003", Name = "部署1-2", IsActive = true, Jyunjyo = 3, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2012/4/1"), EndYmd = DateOnly.Parse("2032/4/1"), OyaId = 1, BusyoBaseId = 3, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 4, Code = "004", Name = "部署2", IsActive = true, Jyunjyo = 2, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2013/4/1"), EndYmd = DateOnly.Parse("2033/4/1"), OyaId = 1, BusyoBaseId = 4, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 5, Code = "005", Name = "部署2-1", IsActive = true, Jyunjyo = 1, OyaCode = "004", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2014/4/1"), EndYmd = DateOnly.Parse("2034/8/1"), OyaId = 4, BusyoBaseId = 5, ShoninBusyoId = null });

            await db.SaveChangesAsync();

            // Act
            var data = await Zouryoku.Utils.ImpactDepartment.GetImpactDepartmentAsync(db, 1, fakeTimeProvider.Today());

            // Assert
            Assert.HasCount(3, data, "戻り値の件数が一致しません。");

            Assert.AreEqual(3, data[0].Id, "1件目の Id が一致しません。");
            Assert.AreEqual("003", data[0].Code, "1件目の Code が一致しません。");
            Assert.AreEqual("部署1-2", data[0].Name, "1件目の Name が一致しません。");
            Assert.IsTrue(data[0].IsActive, "1件目の IsActive が一致しません。");
            Assert.AreEqual((short)3, data[0].Jyunjyo, "1件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2012/4/1"), data[0].StartYmd, "1件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2032/4/1"), data[0].EndYmd, "1件目の EndYmd が一致しません。");
            Assert.AreEqual(1, data[0].OyaId, "1件目の OyaId が一致しません。");
            Assert.IsNull(data[0].BusyoBase.BumoncyoId, "1件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[0].ShoninBusyoId, "1件目の ShoninBusyoId が一致しません。");

            Assert.AreEqual(2, data[1].Id, "2件目の Id が一致しません。");
            Assert.AreEqual("002", data[1].Code, "2件目の Code が一致しません。");
            Assert.AreEqual("部署1-1", data[1].Name, "2件目の Name が一致しません。");
            Assert.IsTrue(data[1].IsActive, "2件目の IsActive が一致しません。");
            Assert.AreEqual((short)4, data[1].Jyunjyo, "2件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2011/4/1"), data[1].StartYmd, "2件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2031/4/1"), data[1].EndYmd, "2件目の EndYmd が一致しません。");
            Assert.AreEqual(1, data[1].OyaId, "2件目の OyaId が一致しません。");
            Assert.IsNull(data[1].BusyoBase.BumoncyoId, "2件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[1].ShoninBusyoId, "2件目の ShoninBusyoId が一致しません。");

            Assert.AreEqual(1, data[2].Id, "3件目の Id が一致しません。");
            Assert.AreEqual("001", data[2].Code, "3件目の Code が一致しません。");
            Assert.AreEqual("部署1", data[2].Name, "3件目の Name が一致しません。");
            Assert.IsTrue(data[2].IsActive, "3件目の IsActive が一致しません。");
            Assert.AreEqual((short)5, data[2].Jyunjyo, "3件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2010/4/1"), data[2].StartYmd, "3件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2030/4/1"), data[2].EndYmd, "3件目の EndYmd が一致しません。");
            Assert.IsNull(data[2].OyaId, "3件目の OyaId が一致しません。");
            Assert.AreEqual(9999, data[2].BusyoBase.BumoncyoId, "3件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[2].ShoninBusyoId, "3件目の ShoninBusyoId が一致しません。");
        }


        /// <summary>
        /// 影響範囲部署取得
        ///     テストケース3：親部署IDが設定されていない（部署1-2）
        ///                     自部署のみ取得すること
        /// </summary>
        [TestMethod]
        public async Task GetImpactDepartmentAsync_WhenNoParentIdExists_ReturnsOnlySelf()
        {
            // Arrange
            db.BusyoBases.Add(new BusyoBasis { Id = 1, Name = "部署1", BumoncyoId = 9999 });
            db.BusyoBases.Add(new BusyoBasis { Id = 2, Name = "部署1-1", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 3, Name = "部署1-2", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 4, Name = "部署2", BumoncyoId = 8888 });
            db.BusyoBases.Add(new BusyoBasis { Id = 5, Name = "部署2-1", BumoncyoId = null });

            db.Busyos.Add(new Busyo { Id = 1, Code = "001", Name = "部署1", IsActive = true, Jyunjyo = 5, OyaCode = " ", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2010/4/1"), EndYmd = DateOnly.Parse("2030/4/1"), OyaId = null, BusyoBaseId = 1, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 2, Code = "002", Name = "部署1-1", IsActive = true, Jyunjyo = 4, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2011/4/1"), EndYmd = DateOnly.Parse("2031/4/1"), OyaId = 1, BusyoBaseId = 2, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 3, Code = "003", Name = "部署1-2", IsActive = true, Jyunjyo = 3, OyaCode = " ", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2012/4/1"), EndYmd = DateOnly.Parse("2032/4/1"), OyaId = null, BusyoBaseId = 3, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 4, Code = "004", Name = "部署2", IsActive = true, Jyunjyo = 2, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2013/4/1"), EndYmd = DateOnly.Parse("2033/4/1"), OyaId = 1, BusyoBaseId = 4, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 5, Code = "005", Name = "部署2-1", IsActive = true, Jyunjyo = 1, OyaCode = "004", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2014/4/1"), EndYmd = DateOnly.Parse("2034/8/1"), OyaId = 4, BusyoBaseId = 5, ShoninBusyoId = null });

            await db.SaveChangesAsync();

            // Act
            var data = await Zouryoku.Utils.ImpactDepartment.GetImpactDepartmentAsync(db, 3, fakeTimeProvider.Today());

            // Assert
            Assert.HasCount(1, data, "戻り値の件数が一致しません。");

            Assert.AreEqual(3, data[0].Id, "1件目の Id が一致しません。");
            Assert.AreEqual("003", data[0].Code, "1件目の Code が一致しません。");
            Assert.AreEqual("部署1-2", data[0].Name, "1件目の Name が一致しません。");
            Assert.IsTrue(data[0].IsActive, "1件目の IsActive が一致しません。");
            Assert.AreEqual((short)3, data[0].Jyunjyo, "1件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2012/4/1"), data[0].StartYmd, "1件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2032/4/1"), data[0].EndYmd, "1件目の EndYmd が一致しません。");
            Assert.IsNull(data[0].OyaId, "1件目の OyaId が一致しません。");
            Assert.IsNull(data[0].BusyoBase.BumoncyoId, "1件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[0].ShoninBusyoId, "1件目の ShoninBusyoId が一致しません。");
        }

        /// <summary>
        /// 影響範囲部署取得
        ///     テストケース4：自部署にも親部署にも部門長IDが設定されていない（部署1-2）
        ///                     親IDでたどり着ける親部署とその子部署（部門長IDがないもの）すべてを取得し、影響範囲外の部署は取得しないこと
        /// </summary>
        [TestMethod]
        public async Task GetImpactDepartmentAsync_WhenNoManagerIdsExistInHierarchy_ReturnsParentAndChildren()
        {
            // Arrange
            db.BusyoBases.Add(new BusyoBasis { Id = 1, Name = "部署1", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 2, Name = "部署1-1", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 3, Name = "部署1-2", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 4, Name = "部署2", BumoncyoId = 8888 });
            db.BusyoBases.Add(new BusyoBasis { Id = 5, Name = "部署2-1", BumoncyoId = null });

            db.Busyos.Add(new Busyo { Id = 1, Code = "001", Name = "部署1", IsActive = true, Jyunjyo = 5, OyaCode = " ", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2010/4/1"), EndYmd = DateOnly.Parse("2030/4/1"), OyaId = null, BusyoBaseId = 1, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 2, Code = "002", Name = "部署1-1", IsActive = true, Jyunjyo = 4, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2011/4/1"), EndYmd = DateOnly.Parse("2031/4/1"), OyaId = 1, BusyoBaseId = 2, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 3, Code = "003", Name = "部署1-2", IsActive = true, Jyunjyo = 3, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2012/4/1"), EndYmd = DateOnly.Parse("2032/4/1"), OyaId = 2, BusyoBaseId = 3, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 4, Code = "004", Name = "部署2", IsActive = true, Jyunjyo = 2, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2013/4/1"), EndYmd = DateOnly.Parse("2033/4/1"), OyaId = 1, BusyoBaseId = 4, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 5, Code = "005", Name = "部署2-1", IsActive = true, Jyunjyo = 1, OyaCode = "004", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2014/4/1"), EndYmd = DateOnly.Parse("2034/8/1"), OyaId = 4, BusyoBaseId = 5, ShoninBusyoId = null });

            await db.SaveChangesAsync();

            // Act
            var data = await Zouryoku.Utils.ImpactDepartment.GetImpactDepartmentAsync(db, 3, fakeTimeProvider.Today());

            // Assert
            Assert.HasCount(3, data, "戻り値の件数が一致しません。");

            Assert.AreEqual(3, data[0].Id, "1件目の Id が一致しません。");
            Assert.AreEqual("003", data[0].Code, "1件目の Code が一致しません。");
            Assert.AreEqual("部署1-2", data[0].Name, "1件目の Name が一致しません。");
            Assert.IsTrue(data[0].IsActive, "1件目の IsActive が一致しません。");
            Assert.AreEqual((short)3, data[0].Jyunjyo, "1件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2012/4/1"), data[0].StartYmd, "1件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2032/4/1"), data[0].EndYmd, "1件目の EndYmd が一致しません。");
            Assert.AreEqual(2, data[0].OyaId, "1件目の OyaId が一致しません。");
            Assert.IsNull(data[0].BusyoBase.BumoncyoId, "1件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[0].ShoninBusyoId, "1件目の ShoninBusyoId が一致しません。");

            Assert.AreEqual(2, data[1].Id, "2件目の Id が一致しません。");
            Assert.AreEqual("002", data[1].Code, "2件目の Code が一致しません。");
            Assert.AreEqual("部署1-1", data[1].Name, "2件目の Name が一致しません。");
            Assert.IsTrue(data[1].IsActive, "2件目の IsActive が一致しません。");
            Assert.AreEqual((short)4, data[1].Jyunjyo, "2件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2011/4/1"), data[1].StartYmd, "2件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2031/4/1"), data[1].EndYmd, "2件目の EndYmd が一致しません。");
            Assert.AreEqual(1, data[1].OyaId, "2件目の OyaId が一致しません。");
            Assert.IsNull(data[1].BusyoBase.BumoncyoId, "2件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[1].ShoninBusyoId, "2件目の ShoninBusyoId が一致しません。");

            Assert.AreEqual(1, data[2].Id, "3件目の Id が一致しません。");
            Assert.AreEqual("001", data[2].Code, "3件目の Code が一致しません。");
            Assert.AreEqual("部署1", data[2].Name, "3件目の Name が一致しません。");
            Assert.IsTrue(data[2].IsActive, "3件目の IsActive が一致しません。");
            Assert.AreEqual((short)5, data[2].Jyunjyo, "3件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2010/4/1"), data[2].StartYmd, "3件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2030/4/1"), data[2].EndYmd, "3件目の EndYmd が一致しません。");
            Assert.IsNull(data[2].OyaId, "3件目の OyaId が一致しません。");
            Assert.IsNull(data[2].BusyoBase.BumoncyoId, "3件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[2].ShoninBusyoId, "3件目の ShoninBusyoId が一致しません。");
        }

        /// <summary>
        /// 影響範囲部署取得
        ///     テストケース5：親部署は設定されていないが、承認部署が設定されている（部署1-2）
        ///                     自部署と承認部署のみ取得すること
        /// </summary>
        [TestMethod]
        public async Task GetImpactDepartmentAsync_WhenApprovalDepartmentIsSet_ReturnsSelfAndApprovalDepartment()
        {
            // Arrange
            db.BusyoBases.Add(new BusyoBasis { Id = 1, Name = "部署1", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 2, Name = "部署1-1", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 3, Name = "部署1-2", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 4, Name = "部署2", BumoncyoId = 8888 });
            db.BusyoBases.Add(new BusyoBasis { Id = 5, Name = "部署2-1", BumoncyoId = null });

            db.Busyos.Add(new Busyo { Id = 1, Code = "001", Name = "部署1", IsActive = true, Jyunjyo = 5, OyaCode = " ", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2010/4/1"), EndYmd = DateOnly.Parse("2030/4/1"), OyaId = null, BusyoBaseId = 1, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 2, Code = "002", Name = "部署1-1", IsActive = true, Jyunjyo = 4, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2011/4/1"), EndYmd = DateOnly.Parse("2031/4/1"), OyaId = 1, BusyoBaseId = 2, ShoninBusyoId = null});
            db.Busyos.Add(new Busyo { Id = 3, Code = "003", Name = "部署1-2", IsActive = true, Jyunjyo = 3, OyaCode = " ", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2012/4/1"), EndYmd = DateOnly.Parse("2032/4/1"), OyaId = null, BusyoBaseId = 3, ShoninBusyoId = 4 });
            db.Busyos.Add(new Busyo { Id = 4, Code = "004", Name = "部署2", IsActive = true, Jyunjyo = 2, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2013/4/1"), EndYmd = DateOnly.Parse("2033/4/1"), OyaId = 1, BusyoBaseId = 4, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 5, Code = "005", Name = "部署2-1", IsActive = true, Jyunjyo = 1, OyaCode = "004", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2014/4/1"), EndYmd = DateOnly.Parse("2034/8/1"), OyaId = 4, BusyoBaseId = 5, ShoninBusyoId = null });

            await db.SaveChangesAsync();

            // Act
            var data = await Zouryoku.Utils.ImpactDepartment.GetImpactDepartmentAsync(db, 3, fakeTimeProvider.Today());

            // Assert
            Assert.HasCount(2, data, "戻り値の件数が一致しません。");

            Assert.AreEqual(4, data[0].Id, "1件目の Id が一致しません。");
            Assert.AreEqual("004", data[0].Code, "1件目の Code が一致しません。");
            Assert.AreEqual("部署2", data[0].Name, "1件目の Name が一致しません。");
            Assert.IsTrue(data[0].IsActive, "1件目の IsActive が一致しません。");
            Assert.AreEqual((short)2, data[0].Jyunjyo, "1件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2013/4/1"), data[0].StartYmd, "1件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2033/4/1"), data[0].EndYmd, "1件目の EndYmd が一致しません。");
            Assert.AreEqual(1, data[0].OyaId, "1件目の OyaId が一致しません。");
            Assert.AreEqual(8888, data[0].BusyoBase.BumoncyoId, "1件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[0].ShoninBusyoId, "1件目の ShoninBusyoId が一致しません。");

            Assert.AreEqual(3, data[1].Id, "2件目の Id が一致しません。");
            Assert.AreEqual("003", data[1].Code, "2件目の Code が一致しません。");
            Assert.AreEqual("部署1-2", data[1].Name, "2件目の Name が一致しません。");
            Assert.IsTrue(data[1].IsActive, "2件目の IsActive が一致しません。");
            Assert.AreEqual((short)3, data[1].Jyunjyo, "2件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2012/4/1"), data[1].StartYmd, "2件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2032/4/1"), data[1].EndYmd, "2件目の EndYmd が一致しません。");
            Assert.IsNull(data[1].OyaId, "2件目の OyaId が一致しません。");
            Assert.IsNull(data[1].BusyoBase.BumoncyoId, "2件目の BumoncyoId が一致しません。");
            Assert.AreEqual(4, data[1].ShoninBusyoId, "2件目の ShoninBusyoId が一致しません。");
        }

        /// <summary>
        /// 影響範囲部署取得
        ///     テストケース6：親部署は設定されていないが、他部署から承認部署として設定されている（部署1-2）
        ///                     自部署と被承認部署のみ取得すること
        /// </summary>
        [TestMethod]
        public async Task GetImpactDepartmentAsync_WhenUsedAsApprovalDepartment_ReturnsSelfAndApprovedByDepartments()
        {
            // Arrange
            db.BusyoBases.Add(new BusyoBasis { Id = 1, Name = "部署1", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 2, Name = "部署1-1", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 3, Name = "部署1-2", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 4, Name = "部署2", BumoncyoId = 8888 });
            db.BusyoBases.Add(new BusyoBasis { Id = 5, Name = "部署2-1", BumoncyoId = null });

            db.Busyos.Add(new Busyo { Id = 1, Code = "001", Name = "部署1", IsActive = true, Jyunjyo = 5, OyaCode = " ", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2010/4/1"), EndYmd = DateOnly.Parse("2030/4/1"), OyaId = null, BusyoBaseId = 1, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 2, Code = "002", Name = "部署1-1", IsActive = true, Jyunjyo = 4, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2011/4/1"), EndYmd = DateOnly.Parse("2031/4/1"), OyaId = 1, BusyoBaseId = 2, ShoninBusyoId = 3 });
            db.Busyos.Add(new Busyo { Id = 3, Code = "003", Name = "部署1-2", IsActive = true, Jyunjyo = 3, OyaCode = " ", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2012/4/1"), EndYmd = DateOnly.Parse("2032/4/1"), OyaId = null, BusyoBaseId = 3, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 4, Code = "004", Name = "部署2", IsActive = true, Jyunjyo = 2, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2013/4/1"), EndYmd = DateOnly.Parse("2033/4/1"), OyaId = 1, BusyoBaseId = 4, ShoninBusyoId = 3 });
            db.Busyos.Add(new Busyo { Id = 5, Code = "005", Name = "部署2-1", IsActive = true, Jyunjyo = 1, OyaCode = "004", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2014/4/1"), EndYmd = DateOnly.Parse("2034/8/1"), OyaId = 4, BusyoBaseId = 5, ShoninBusyoId = null });

            await db.SaveChangesAsync();

            // Act
            var data = await Zouryoku.Utils.ImpactDepartment.GetImpactDepartmentAsync(db, 3, fakeTimeProvider.Today());

            // Assert
            Assert.HasCount(3, data, "戻り値の件数が一致しません。");

            Assert.AreEqual(4, data[0].Id, "1件目の Id が一致しません。");
            Assert.AreEqual("004", data[0].Code, "1件目の Code が一致しません。");
            Assert.AreEqual("部署2", data[0].Name, "1件目の Name が一致しません。");
            Assert.IsTrue(data[0].IsActive, "1件目の IsActive が一致しません。");
            Assert.AreEqual((short)2, data[0].Jyunjyo, "1件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2013/4/1"), data[0].StartYmd, "1件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2033/4/1"), data[0].EndYmd, "1件目の EndYmd が一致しません。");
            Assert.AreEqual(1, data[0].OyaId, "1件目の OyaId が一致しません。");
            Assert.AreEqual(8888, data[0].BusyoBase.BumoncyoId, "1件目の BumoncyoId が一致しません。");
            Assert.AreEqual(3, data[0].ShoninBusyoId, "1件目の ShoninBusyoId が一致しません。");

            Assert.AreEqual(3, data[1].Id, "2件目の Id が一致しません。");
            Assert.AreEqual("003", data[1].Code, "2件目の Code が一致しません。");
            Assert.AreEqual("部署1-2", data[1].Name, "2件目の Name が一致しません。");
            Assert.IsTrue(data[1].IsActive, "2件目の IsActive が一致しません。");
            Assert.AreEqual((short)3, data[1].Jyunjyo, "2件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2012/4/1"), data[1].StartYmd, "2件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2032/4/1"), data[1].EndYmd, "2件目の EndYmd が一致しません。");
            Assert.IsNull(data[1].OyaId, "2件目の OyaId が一致しません。");
            Assert.IsNull(data[1].BusyoBase.BumoncyoId, "2件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[1].ShoninBusyoId, "2件目の ShoninBusyoId が一致しません。");

            Assert.AreEqual(2, data[2].Id, "3件目の Id が一致しません。");
            Assert.AreEqual("002", data[2].Code, "3件目の Code が一致しません。");
            Assert.AreEqual("部署1-1", data[2].Name, "3件目の Name が一致しません。");
            Assert.IsTrue(data[2].IsActive, "3件目の IsActive が一致しません。");
            Assert.AreEqual((short)4, data[2].Jyunjyo, "3件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2011/4/1"), data[2].StartYmd, "3件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2031/4/1"), data[2].EndYmd, "3件目の EndYmd が一致しません。");
            Assert.AreEqual(1, data[2].OyaId, "3件目の OyaId が一致しません。");
            Assert.IsNull(data[2].BusyoBase.BumoncyoId, "3件目の BumoncyoId が一致しません。");
            Assert.AreEqual(3, data[2].ShoninBusyoId, "3件目の ShoninBusyoId が一致しません。");
        }

        /// <summary>
        /// 影響範囲部署取得
        ///     テストケース7：部署重複（部署1-1）
        ///                     親部署とその子部署で検索した部署と承認部署の設定で部署1-2が重複するが二重に出力しないこと
        /// </summary>
        [TestMethod]
        public async Task GetImpactDepartmentAsync_WhenDuplicateDepartmentsFound_DoesNotReturnDuplicates()
        {
            // Arrange
            db.BusyoBases.Add(new BusyoBasis { Id = 1, Name = "部署1", BumoncyoId = 9999 });
            db.BusyoBases.Add(new BusyoBasis { Id = 2, Name = "部署1-1", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 3, Name = "部署1-2", BumoncyoId = null });
            db.BusyoBases.Add(new BusyoBasis { Id = 4, Name = "部署2", BumoncyoId = 8888 });
            db.BusyoBases.Add(new BusyoBasis { Id = 5, Name = "部署2-1", BumoncyoId = null });

            db.Busyos.Add(new Busyo { Id = 1, Code = "001", Name = "部署1", IsActive = true, Jyunjyo = 5, OyaCode = " ", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2010/4/1"), EndYmd = DateOnly.Parse("2030/4/1"), OyaId = null, BusyoBaseId = 1, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 2, Code = "002", Name = "部署1-1", IsActive = true, Jyunjyo = 4, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2011/4/1"), EndYmd = DateOnly.Parse("2031/4/1"), OyaId = 1, BusyoBaseId = 2, ShoninBusyoId = 3 });
            db.Busyos.Add(new Busyo { Id = 3, Code = "003", Name = "部署1-2", IsActive = true, Jyunjyo = 3, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2012/4/1"), EndYmd = DateOnly.Parse("2032/4/1"), OyaId = 1, BusyoBaseId = 3, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 4, Code = "004", Name = "部署2", IsActive = true, Jyunjyo = 2, OyaCode = "001", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2013/4/1"), EndYmd = DateOnly.Parse("2033/4/1"), OyaId = 1, BusyoBaseId = 4, ShoninBusyoId = null });
            db.Busyos.Add(new Busyo { Id = 5, Code = "005", Name = "部署2-1", IsActive = true, Jyunjyo = 1, OyaCode = "004", KasyoCode = " ", KaikeiCode = " ", KanaName = " ", StartYmd = DateOnly.Parse("2014/4/1"), EndYmd = DateOnly.Parse("2034/8/1"), OyaId = 4, BusyoBaseId = 5, ShoninBusyoId = null });

            await db.SaveChangesAsync();

            // Act
            var data = await Zouryoku.Utils.ImpactDepartment.GetImpactDepartmentAsync(db, 1, fakeTimeProvider.Today());

            // Assert
            Assert.HasCount(3, data, "戻り値の件数が一致しません。");

            Assert.AreEqual(3, data[0].Id, "1件目の Id が一致しません。");
            Assert.AreEqual("003", data[0].Code, "1件目の Code が一致しません。");
            Assert.AreEqual("部署1-2", data[0].Name, "1件目の Name が一致しません。");
            Assert.IsTrue(data[0].IsActive, "1件目の IsActive が一致しません。");
            Assert.AreEqual((short)3, data[0].Jyunjyo, "1件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2012/4/1"), data[0].StartYmd, "1件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2032/4/1"), data[0].EndYmd, "1件目の EndYmd が一致しません。");
            Assert.AreEqual(1, data[0].OyaId, "1件目の OyaId が一致しません。");
            Assert.IsNull(data[0].BusyoBase.BumoncyoId, "1件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[0].ShoninBusyoId, "1件目の ShoninBusyoId が一致しません。");

            Assert.AreEqual(2, data[1].Id, "2件目の Id が一致しません。");
            Assert.AreEqual("002", data[1].Code, "2件目の Code が一致しません。");
            Assert.AreEqual("部署1-1", data[1].Name, "2件目の Name が一致しません。");
            Assert.IsTrue(data[1].IsActive, "2件目の IsActive が一致しません。");
            Assert.AreEqual((short)4, data[1].Jyunjyo, "2件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2011/4/1"), data[1].StartYmd, "2件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2031/4/1"), data[1].EndYmd, "2件目の EndYmd が一致しません。");
            Assert.AreEqual(1, data[1].OyaId, "2件目の OyaId が一致しません。");
            Assert.IsNull(data[1].BusyoBase.BumoncyoId, "2件目の BumoncyoId が一致しません。");
            Assert.AreEqual(3, data[1].ShoninBusyoId, "2件目の ShoninBusyoId が一致しません。");

            Assert.AreEqual(1, data[2].Id, "3件目の Id が一致しません。");
            Assert.AreEqual("001", data[2].Code, "3件目の Code が一致しません。");
            Assert.AreEqual("部署1", data[2].Name, "3件目の Name が一致しません。");
            Assert.IsTrue(data[2].IsActive, "3件目の IsActive が一致しません。");
            Assert.AreEqual((short)5, data[2].Jyunjyo, "3件目の Jyunjyo が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2010/4/1"), data[2].StartYmd, "3件目の StartYmd が一致しません。");
            Assert.AreEqual(DateOnly.Parse("2030/4/1"), data[2].EndYmd, "3件目の EndYmd が一致しません。");
            Assert.IsNull(data[2].OyaId, "3件目の OyaId が一致しません。");
            Assert.AreEqual(9999, data[2].BusyoBase.BumoncyoId, "3件目の BumoncyoId が一致しません。");
            Assert.IsNull(data[2].ShoninBusyoId, "3件目の ShoninBusyoId が一致しません。");
        }
    }
}
