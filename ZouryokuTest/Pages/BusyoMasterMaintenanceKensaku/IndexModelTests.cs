using CommonLibrary.Extensions;
using Model.Model;
using Zouryoku.Pages.BusyoMasterMaintenanceKensaku;

namespace ZouryokuTest.Pages.BusyoMasterMaintenanceKensaku
{
    /// <summary>
    /// 部署マスタメンテナンス検索画面のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {
        private IndexModel CreateModel()
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine, fakeTimeProvider)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };
            return model;
        }

        /// <summary>
        /// BASEテーブルの共通シード
        /// </summary>
        private void SeedBase()
        {
            // 部門長（SyainBasis）
            var bunmoncho = new SyainBasis()
            {
                Id = 10,
                Name = "田中部長",
                Code = "S001"
            };

            // BusyoBase（部門長あり）
            var bumonchoAri = new BusyoBasis()
            {
                Id = 1,
                Name = "基底1",
                Bumoncyo = bunmoncho,
            };

            // BusyoBase（部門長なし）
            var bumonchoNashi = new BusyoBasis()
            {
                Id = 2,
                Name = "基底2",
                BumoncyoId = null,
            };

            db.AddRange(bunmoncho, bumonchoAri, bumonchoNashi);
        }

        /// <summary>
        /// 画面条件検索用のデータ生成
        /// </summary>
        private void CreateConditionRecords()
        {
            SeedBase();

            db.Add(new Busyo()
            {
                Id = 1,
                Code = "100",
                Name = "システム部",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
            });

            db.Add(new Busyo()
            {
                Id = 2,
                Code = "100",
                Name = "第一営業部",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 2,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = false,
                BusyoBaseId = 1,
            });

            db.Add(new Busyo()
            {
                Id = 3,
                Code = "100",
                Name = "第二営業部",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 3,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
            });
        }

        /// <summary>
        /// ①初期表示: アクティブフラグFALSEは取得されない
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_アクティブフラグがFALSE_取得されない()
        {
            SeedBase();

            var today = fakeTimeProvider.Today();

            db.Add(new Busyo()
            {
                Id = 1,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = today,
                EndYmd = today,
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = false,
                BusyoBaseId = 1,
            });

            await db.SaveChangesAsync();

            var model = CreateModel();
            await model.OnGetAsync();

            Assert.IsEmpty(model.Results);
        }

        /// <summary>
        /// ②初期表示: システム日付＜有効開始日は取得されない
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_有効開始日がシステム日付より後_取得されない()
        {
            SeedBase();

            db.Add(new Busyo()
            {
                Id = 1,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = fakeTimeProvider.Now().AddDays(1).ToDateOnly(),
                EndYmd = fakeTimeProvider.Now().AddDays(2).ToDateOnly(),
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
            });

            await db.SaveChangesAsync();

            var model = CreateModel();
            await model.OnGetAsync();

            Assert.IsEmpty(model.Results);
        }

        /// <summary>
        /// ③初期表示: 有効終了日＜システム日付は取得されない
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_有効終了日がシステム日付より前_取得されない()
        {
            SeedBase();

            db.Add(new Busyo()
            {
                Id = 1,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = fakeTimeProvider.Now().AddDays(-2).ToDateOnly(),
                EndYmd = fakeTimeProvider.Now().AddDays(-1).ToDateOnly(),
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
            });

            await db.SaveChangesAsync();

            var model = CreateModel();
            await model.OnGetAsync();

            Assert.IsEmpty(model.Results);
        }

        /// <summary>
        /// ④初期表示: アクティブフラグ＝TRUE、有効開始日＜＝システム日付、システム日付＜＝有効終了日 は取得される
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_アクティブフラグがTRUEかつ有効開始日がシステム日付以前かつ有効終了日がシステム日付以後_取得される()
        {
            SeedBase();

            db.Add(new Busyo()
            {
                Id = 1,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = fakeTimeProvider.Now().AddDays(-1).ToDateOnly(),
                EndYmd = fakeTimeProvider.Now().AddDays(1).ToDateOnly(),
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
            });

            await db.SaveChangesAsync();

            var model = CreateModel();
            await model.OnGetAsync();

            Assert.HasCount(1, model.Results);
        }

        /// <summary>
        /// ⑤初期表示: 親部署、部門長、承認部署なし
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_親部署と部門長と承認部署なし_IDと部署番号と部署名は空白を取得する()
        {
            SeedBase();

            var busyo = new Busyo()
            {
                Id = 2,
                Code = "A001",
                Name = "営業部",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 2, // 部門長なし
                OyaId = null,
                ShoninBusyoId = null,
            };

            db.Add(busyo);
            await db.SaveChangesAsync();

            var vm = new BusyoViewModel(busyo);

            Assert.AreEqual(2, vm.BusyoId);
            Assert.AreEqual("A001", vm.BusyoCode);
            Assert.AreEqual("営業部", vm.BusyoName);
            Assert.AreEqual("", vm.BumoncyoName);
            Assert.AreEqual("", vm.ShoninBusyoName);
            Assert.AreEqual("", vm.IsActiveDisplay);
        }

        /// <summary>
        /// ⑥初期表示: 親部署、部門長、承認部署あり
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_親部署と部門長と承認部署あり_親部署名と部門長名と承認部署名を取得する()
        {
            SeedBase();

            // 親部署
            db.Add(new Busyo()
            {
                Id = 88,
                Code = "100",
                Name = "第一本部",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
            });

            // 承認部署
            db.Add(new Busyo()
            {
                Id = 99,
                Code = "S001",
                Name = "承認部",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1, // 部門長あり
            });

            db.Add(new Busyo()
            {
                Id = 1,
                Code = "A001",
                Name = "営業部",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
                OyaId = 88,
                ShoninBusyoId = 99,
            });

            await db.SaveChangesAsync();

            var model = CreateModel();
            await model.OnGetAsync();

            Assert.AreEqual(1, model.Results[1].BusyoId);
            Assert.AreEqual("A001", model.Results[1].BusyoCode);
            Assert.AreEqual("第一本部　営業部", model.Results[1].BusyoName);
            Assert.AreEqual("田中部長", model.Results[1].BumoncyoName);
            Assert.AreEqual("承認部", model.Results[1].ShoninBusyoName);
            Assert.AreEqual("", model.Results[1].IsActiveDisplay);
        }

        /// <summary>
        /// ⑦初期表示: ソートの確認
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_部署検索_階層順で並んでいる()
        {
            SeedBase();

            db.Add(new Busyo()
            {
                Id = 1,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 2,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
                OyaId = null
            });

            db.Add(new Busyo()
            {
                Id = 2,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
                OyaId = 1,
            });

            db.Add(new Busyo()
            {
                Id = 3,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
                OyaId = null
            });

            await db.SaveChangesAsync();

            var model = CreateModel();
            await model.OnGetAsync();

            Assert.AreEqual(3, model.Results[0].BusyoId);
            Assert.AreEqual(1, model.Results[1].BusyoId);
            Assert.AreEqual(2, model.Results[2].BusyoId);
            Assert.AreEqual(0, model.Results[0].Depth); // ID:3, トップレベル
            Assert.AreEqual(0, model.Results[1].Depth); // ID:1, トップレベル
            Assert.AreEqual(1, model.Results[2].Depth); // ID:2, ID:1の子
        }

        /// <summary>
        /// ⑧初期表示: ソートの確認
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_同階層_並び順序の昇順に並んでいる()
        {
            SeedBase();

            db.Add(new Busyo()
            {
                Id = 1,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 2,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
                OyaId = null,
            });

            db.Add(new Busyo()
            {
                Id = 2,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 1,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
                OyaId = null,
            });

            db.Add(new Busyo()
            {
                Id = 3,
                Code = "100",
                Name = "部署A",
                KanaName = "ブショエー",
                OyaCode = string.Empty,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Jyunjyo = 3,
                KasyoCode = "1",
                KaikeiCode = "1",
                IsActive = true,
                BusyoBaseId = 1,
                OyaId = null,
            });

            await db.SaveChangesAsync();

            var model = CreateModel();
            await model.OnGetAsync();

            Assert.AreEqual(2, model.Results[0].BusyoId);
            Assert.AreEqual(1, model.Results[1].BusyoId);
            Assert.AreEqual(3, model.Results[2].BusyoId);
        }

        /// <summary>
        /// ⑨部署検索: 部署名なし、無効部署OFF
        /// </summary>
        [TestMethod]
        public async Task OnGetSearch_部署名なし無効部署OFF_アクティブTRUEを取得する()
        {
            CreateConditionRecords();

            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition = new BusyoSearchConditionModel { BusyoName = null, IncludeInactive = false };

            await model.OnGetSearchAsync();

            Assert.HasCount(2, model.Results);
            Assert.AreEqual(1, model.Results[0].BusyoId);
            Assert.AreEqual(3, model.Results[1].BusyoId);
        }

        /// <summary>
        /// ⑩部署検索: 部署名なし、無効部署ON
        /// </summary>
        [TestMethod]
        public async Task OnGetSearch_部署名なし無効部署ON_全て取得する()
        {
            CreateConditionRecords();

            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition = new BusyoSearchConditionModel { BusyoName = null, IncludeInactive = true };

            await model.OnGetSearchAsync();

            Assert.HasCount(3, model.Results);
            Assert.AreEqual(1, model.Results[0].BusyoId);
            Assert.AreEqual(2, model.Results[1].BusyoId);
            Assert.AreEqual(3, model.Results[2].BusyoId);
        }

        /// <summary>
        /// ⑪部署検索: 部署名"営業"、無効部署OFF
        /// </summary>
        [TestMethod]
        public async Task OnGetSearch_部署名あり無効部署OFF_部署名部分一致かつアクティブTRUEを取得する()
        {
            CreateConditionRecords();

            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition = new BusyoSearchConditionModel { BusyoName = "営業", IncludeInactive = false };

            await model.OnGetSearchAsync();

            Assert.HasCount(1, model.Results);
            Assert.AreEqual(3, model.Results[0].BusyoId);
        }

        /// <summary>
        /// ⑫部署検索: 部署名"営業"、無効部署ON
        /// </summary>
        [TestMethod]
        public async Task OnGetSearch_部署名あり無効部署ON_部署名部分一致を全て取得する()
        {
            CreateConditionRecords();

            await db.SaveChangesAsync();

            var model = CreateModel();
            model.Condition = new BusyoSearchConditionModel { BusyoName = "営業", IncludeInactive = true };

            await model.OnGetSearchAsync();

            Assert.HasCount(2, model.Results);
            Assert.AreEqual(2, model.Results[0].BusyoId);
            Assert.AreEqual(3, model.Results[1].BusyoId);
        }
    }
}
