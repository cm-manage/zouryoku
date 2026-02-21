using Model.Model;
using Zouryoku.Utils;

namespace ZouryokuTest.Utils
{
    [TestClass]
    public class DepartmentHierarchyTests : BaseInMemoryDbContextTest
    {
        // テストで使用する基準日付
        protected DateOnly referenceDate = new DateOnly(2026, 02, 13);

        // ---------------------------------------------------------------------
        // GetDepartmentHierarchyStringAsync Tests
        // ---------------------------------------------------------------------
        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------
        [DataRow(1, "部署A", 0, 0, true, DisplayName = "起点部署のみ取得できた場合 → 部署名にEndPointがついた文字列が返却される")]
        [DataRow(1, "部署A", 1, 0, true, DisplayName = "当日が起点部署のStartYmdと等しい場合 → 部署名にEndPointがついた文字列が返却される")]
        [DataRow(1, "部署A", 0, -1, true, DisplayName = "当日が起点部署のEndYmdと等しい場合 → 部署名にEndPointがついた文字列が返却される")]
        [TestMethod]
        public async Task GetDepartmentHierarchyStringAsync_起点部署を取得_整形された部署名を返却(
            long busyoId,
            string busyoName,
            int addStartYmd,
            int addEndYmd,
            bool isActive)
        {
            // ---------- Arrange ----------
            // シード: 部署
            var busyo = new Busyo
            {
                Id = busyoId,
                Code = "100",
                Name = busyoName,
                KanaName = "ブショエー",
                OyaCode = "200",
                StartYmd = referenceDate.AddDays(-1).AddDays(addStartYmd),
                EndYmd = referenceDate.AddDays(1).AddDays(addEndYmd),
                Jyunjyo = 1,
                KasyoCode = "100",
                KaikeiCode = "100",
                IsActive = isActive,
                BusyoBaseId = 1,
            };

            SeedEntities(busyo);

            // ---------- Act ----------
            var result = await DepartmentHierarchy.GetDepartmentHierarchyStringAsync(db, referenceDate, busyoId);

            // ---------- Assert ----------
            Assert.AreEqual(busyoName + DepartmentHierarchy.EndPoint, result);
        }

        [TestMethod(DisplayName = "部署を2階層取得 → 整形された部署名を返却")]
        public async Task GetDepartmentHierarchyStringAsync_部署を2階層取得_整形された部署名を返却()
        {
            // ---------- Arrange ----------
            // シード: 部署
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = "200",
                StartYmd = referenceDate.AddDays(-1),
                EndYmd = referenceDate.AddDays(1),
                Jyunjyo = 1,
                KasyoCode = "100",
                KaikeiCode = "100",
                IsActive = true,
                BusyoBaseId = 1,
            };

            var busyo2 = new Busyo
            {
                Id = 2,
                Code = "200",
                Name = "部署B",
                KanaName = "ブショビー",
                OyaCode = "300",
                StartYmd = referenceDate.AddDays(-1),
                EndYmd = referenceDate.AddDays(1),
                Jyunjyo = 2,
                KasyoCode = "100",
                KaikeiCode = "100",
                IsActive = true,
                BusyoBaseId = 2,
                OyaId = 1,
            };

            SeedEntities(busyo1, busyo2);

            // ---------- Act ----------
            var result = await DepartmentHierarchy.GetDepartmentHierarchyStringAsync(db, referenceDate, busyo2.Id);

            // ---------- Assert ----------
            Assert.AreEqual(busyo1.Name + DepartmentHierarchy.Delimiter + busyo2.Name + DepartmentHierarchy.EndPoint , result);
        }

        [TestMethod(DisplayName = "部署を3階層取得 → 整形された部署名を返却")]
        public async Task GetDepartmentHierarchyStringAsync_部署を3階層取得_整形された部署名を返却()
        {
            // ---------- Arrange ----------
            // シード: 部署
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = "200",
                StartYmd = referenceDate.AddDays(-1),
                EndYmd = referenceDate.AddDays(1),
                Jyunjyo = 1,
                KasyoCode = "100",
                KaikeiCode = "100",
                IsActive = true,
                BusyoBaseId = 1,
            };

            var busyo2 = new Busyo
            {
                Id = 2,
                Code = "200",
                Name = "部署B",
                KanaName = "ブショビー",
                OyaCode = "300",
                StartYmd = referenceDate.AddDays(-1),
                EndYmd = referenceDate.AddDays(1),
                Jyunjyo = 2,
                KasyoCode = "100",
                KaikeiCode = "100",
                IsActive = true,
                BusyoBaseId = 2,
                OyaId = 1,
            };

            var busyo3 = new Busyo
            {
                Id = 3,
                Code = "300",
                Name = "部署C",
                KanaName = "ブショシー",
                OyaCode = "400",
                StartYmd = referenceDate.AddDays(-1),
                EndYmd = referenceDate.AddDays(1),
                Jyunjyo = 3,
                KasyoCode = "100",
                KaikeiCode = "100",
                IsActive = true,
                BusyoBaseId = 3,
                OyaId = 2,
            };

            SeedEntities(busyo1, busyo2, busyo3);

            // ---------- Act ----------
            var result = await DepartmentHierarchy.GetDepartmentHierarchyStringAsync(db, referenceDate, busyo3.Id);

            // ---------- Assert ----------
            Assert.AreEqual(
                busyo1.Name + DepartmentHierarchy.Delimiter + busyo2.Name + DepartmentHierarchy.Delimiter + busyo3.Name + DepartmentHierarchy.EndPoint,
                result
                );
        }

        // -----------------------------------------------------
        // 異常系テストケース
        // -----------------------------------------------------
        // =================================================================
        // 起点部署が取得できない場合
        // =================================================================
        [DataRow(null, "部署A", 0, 0, true, DisplayName = "部署IDがnullの場合 → 空文字が返却される")]
        [DataRow(1, "部署A", 0, 0, false, DisplayName = "IsActiveがfalseの場合 → 空文字が返却される")]
        [DataRow(1, " ", 0, 0, true, DisplayName = "取得された部署名が空文字の場合 → 空文字が返却される")]
        [DataRow(1, "部署A", 5, 0, true, DisplayName = "取得された部署の有効開始日が当日より未来だった場合 → 空文字が返却される")]
        [DataRow(1, "部署A", 0, -5, true, DisplayName = "取得された部署の有効終了日が当日より過去だった場合 → 空文字が返却される")]
        [TestMethod]
        public async Task GetDepartmentHierarchyStringAsync_起点部署が取得できない_空文字を返却(
            int? busyoId,
            string busyoName,
            int addStartYmd,
            int addEndYmd,
            bool isActive)
        {
            // ---------- Arrange ----------
            // シード: 部署
            var busyo = new Busyo
            {
                Id = 1,
                Code = "100",
                Name = busyoName,
                KanaName = "ブショエー",
                OyaCode = "200",
                StartYmd = referenceDate.AddDays(-1).AddDays(addStartYmd),
                EndYmd = referenceDate.AddDays(1).AddDays(addEndYmd),
                Jyunjyo = 1,
                KasyoCode = "100",
                KaikeiCode = "100",
                IsActive = isActive,
                BusyoBaseId = 1,
            };

            SeedEntities(busyo);

            // ---------- Act ----------
            var longBusyoId = (long?)busyoId;
            var result = await DepartmentHierarchy.GetDepartmentHierarchyStringAsync(db, referenceDate, longBusyoId);

            // ---------- Assert ----------
            Assert.IsEmpty(result);
        }
        
        [DataRow(" ", 0, 0, true, DisplayName = "2階層目の取得された部署の名前が空文字の場合 → それまでに取得された部署名が返却される")]
        [DataRow("部署B", 0, 0, false, DisplayName = "2階層目の部署情報のIsActiveがfalseだった場合 → それまでに取得された部署名が返却される")]
        [DataRow("部署B", 5, 0, true, DisplayName = "2階層目の取得された部署の有効開始日が当日より未来だった場合 → それまでに取得された部署名が返却される")]
        [DataRow("部署B", 0, -5, true, DisplayName = "2階層目の取得された部署の有効終了日が当日より過去だった場合 → それまでに取得された部署名が返却される")]
        [TestMethod]
        public async Task GetDepartmentHierarchyStringAsync_2階層目で部署名を取得できない_それまでに取得された部署名を返却(
            string busyoName,
            int addStartYmd,
            int addEndYmd,
            bool isActive)
        {
            // ---------- Arrange ----------
            // シード: 部署
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = "200",
                StartYmd = referenceDate.AddDays(-1),
                EndYmd = referenceDate.AddDays(1),
                Jyunjyo = 1,
                KasyoCode = "100",
                KaikeiCode = "100",
                IsActive = true,
                BusyoBaseId = 1,
            };

            var busyo2 = new Busyo
            {
                Id = 2,
                Code = "200",
                Name = busyoName,
                KanaName = "ブショビー",
                OyaCode = "300",
                StartYmd = referenceDate.AddDays(-1).AddDays(addStartYmd),
                EndYmd = referenceDate.AddDays(1).AddDays(addEndYmd),
                Jyunjyo = 2,
                KasyoCode = "100",
                KaikeiCode = "100",
                IsActive = isActive,
                BusyoBaseId = 2,
                OyaId = 1,
            };

            var busyo3 = new Busyo
            {
                Id = 3,
                Code = "300",
                Name = "部署C",
                KanaName = "ブショシー",
                OyaCode = "400",
                StartYmd = referenceDate.AddDays(-1),
                EndYmd = referenceDate.AddDays(1),
                Jyunjyo = 3,
                KasyoCode = "100",
                KaikeiCode = "100",
                IsActive = true,
                BusyoBaseId = 3,
                OyaId = 2,
            };

            SeedEntities(busyo1, busyo2, busyo3);

            // ---------- Act ----------
            var result = await DepartmentHierarchy.GetDepartmentHierarchyStringAsync(db, referenceDate, busyo3.Id);

            // ---------- Assert ----------
            Assert.AreEqual(
                busyo3.Name + DepartmentHierarchy.EndPoint,
                result
                );
        }
        
        [TestMethod(DisplayName = "循環参照の場合 → それまでに取得された部署名を返却")]
        public async Task GetDepartmentHierarchyStringAsync_循環参照が発生_それまでに取得された部署名を返却()
        {
            // ---------- Arrange ----------
            // シード: 部署
            var busyo1 = new Busyo
            {
                Id = 1,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = "200",
                StartYmd = referenceDate.AddDays(-1),
                EndYmd = referenceDate.AddDays(1),
                Jyunjyo = 1,
                KasyoCode = "100",
                KaikeiCode = "100",
                IsActive = true,
                BusyoBaseId = 1,
                OyaId = null,
            };

            var busyo2 = new Busyo
            {
                Id = 2,
                Code = "200",
                Name = "部署B",
                KanaName = "ブショビー",
                OyaCode = "300",
                StartYmd = referenceDate.AddDays(-1),
                EndYmd = referenceDate.AddDays(1),
                Jyunjyo = 2,
                KasyoCode = "100",
                KaikeiCode = "100",
                IsActive = true,
                BusyoBaseId = 2,
                OyaId = 3,
            };

            var busyo3 = new Busyo
            {
                Id = 3,
                Code = "300",
                Name = "部署C",
                KanaName = "ブショシー",
                OyaCode = "400",
                StartYmd = referenceDate.AddDays(-1),
                EndYmd = referenceDate.AddDays(1),
                Jyunjyo = 3,
                KasyoCode = "100",
                KaikeiCode = "100",
                IsActive = true,
                BusyoBaseId = 3,
                OyaId = 2,
            };

            SeedEntities(busyo1, busyo2, busyo3);

            // ---------- Act ----------
            var result = await DepartmentHierarchy.GetDepartmentHierarchyStringAsync(db, referenceDate, busyo3.Id);

            // ---------- Assert ----------
            Assert.AreEqual(
                busyo2.Name + DepartmentHierarchy.Delimiter + busyo3.Name + DepartmentHierarchy.EndPoint,
                result
                );
        }

        // ---------------------------------------------------------------------
        // Helper Methods
        // ---------------------------------------------------------------------

        /// <summary>
        /// シード処理
        /// </summary>
        private void SeedEntities(params object[] entities)
        {
            foreach (var e in entities)
            {
                if (e is IEnumerable<object> list)
                {
                    db.AddRange(list);
                }
                else
                {
                    db.Add(e);
                }
            }
            db.SaveChanges();
        }
    }
}
