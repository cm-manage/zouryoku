using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using Model.Model;
using Zouryoku.Api;
using Zouryoku.Data;
using Zouryoku.Extensions;

using static Model.Enums.ApprovalStatus;
using static Model.Enums.EmployeeAuthority;

namespace ZouryokuTest.Api
{
    [TestClass]
    public class SinseiKensuBadgeTest : BaseInMemoryDbContextTest
    {
        // ======================================
        // private プロパティ
        // ======================================
        /// <summary>
        /// 当日日付
        /// </summary>
        private static readonly DateOnly Today = DateTime.Today.ToDateOnly();

        /// <summary>
        /// 有効開始日
        /// </summary>
        private static readonly DateOnly ConstStartYmd = Today.AddDays(-10);

        /// <summary>
        /// 有効終了日
        /// </summary>
        private static readonly DateOnly ConstEndYmd = Today.AddDays(10);


        // ======================================
        // 補助メソッド
        // ======================================

        // コントローラー関連
        // --------------------------------------
        /// <summary>
        /// インメモリDB、ログインユーザー情報を備えたコントローラーを作成する
        /// </summary>
        /// <returns>コントローラーのインスタンス</returns>
        private SinseiKensuBadgeController CreateController(Syain loginUser)
        {
            SinseiKensuBadgeController controller = new(db);

            // HttpContextの取得
            var pageContext = GetPageContext();
            var httpContext = pageContext.HttpContext;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            httpContext.Session.Set(new LoginInfo { User = loginUser });
            return controller;
        }

        // テストデータ作成関連
        // --------------------------------------
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

        // ======================================
        // テストメソッド
        // ======================================
        // ---------------------------------------------------------------------
        // GetCountAsync Tests
        // ---------------------------------------------------------------------

        // =================================================================
        // ログインユーザーに権限がない場合
        // =================================================================

        // =================================================================
        /// <summary>
        /// 権限無し: 処理を行わず０件が返却されること
        /// </summary>
        // ================================================================
        [TestMethod]
        public async Task GetCountAsync_権限を持たないユーザー_常に0件()
        {
            // ---------- Arrange ----------
            // シード：自部署
            var ownBusyo = new Busyo
            {
                Id = 1,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 2,
                BusyoId = ownBusyo.Id,
                Kengen = None, // 権限無し
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：申請ユーザー
            var sinseiUser = new Syain
            {
                Id = 3,
                BusyoId = loginUser.BusyoId, // ログインユーザーと同じ部署
                Kengen = None,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：申請情報
            // 一次承認待ち
            var firstApprovalPendingHeader = new UkagaiHeader
            {
                SyainId = sinseiUser.Id,
                ShoninSyainId = null,
                LastShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            // 最終承認待ち
            var lastApprovalPendingHeader = new UkagaiHeader
            {
                SyainId = sinseiUser.Id,
                ShoninSyainId = 99,
                LastShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            // データ登録
            SeedEntities(ownBusyo, loginUser, sinseiUser, firstApprovalPendingHeader, lastApprovalPendingHeader);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(0, result.Value);
        }

        // =================================================================
        // ログインユーザーが指示承認者の場合
        // =================================================================

        // =================================================================
        /// <summary>
        /// 指示承認者: 自部署かつ一次承認待ち申請のデータをカウントすること
        /// </summary>
        // ================================================================
        [TestMethod]
        public async Task GetCountAsync_指示承認者_自部署の一次承認待ち申請をカウント()
        {
            // ---------- Arrange ----------
            // シード：自部署
            var ownBusyo = new Busyo
            {
                Id = 1,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // シード：ログインユーザー（指示承認者）
            var loginUser = new Syain
            {
                Id = 3,
                BusyoId = ownBusyo.Id,
                Kengen = 指示承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：申請ユーザー
            var sinseiUser = new Syain
            {
                Id = 4,
                BusyoId = ownBusyo.Id,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：一次承認待ち伺い申請ヘッダ
            var firstApprovalPendingHeader = new UkagaiHeader
            {
                SyainId = sinseiUser.Id,
                ShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            // データ登録
            SeedEntities(ownBusyo, loginUser, sinseiUser, firstApprovalPendingHeader);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(1, result.Value);

        }

        // =================================================================
        /// <summary>
        /// 指示承認者: 一次承認待ち申請以外の申請をカウントしないこと
        /// </summary>
        // ================================================================
        [TestMethod]
        [DataRow(承認, false, null, DisplayName = "ステータスが承認の場合")]
        [DataRow(差戻, false, null, DisplayName = "ステータスが差戻の場合")]
        [DataRow(承認待, true, null, DisplayName = "無効フラグがtrueの場合")]
        [DataRow(承認待, false, 99L, DisplayName = "一次承認社員が入力されている場合")]
        public async Task GetCountAsync_指示承認者_条件を満たさない申請をカウントしない(
            ApprovalStatus status,
            bool invalid,
            long? shoninSyainId)
        {
            // ---------- Arrange ----------
            // シード：自部署
            var ownBusyo = new Busyo
            {
                Id = 1,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 2,
                BusyoId = ownBusyo.Id,
                Kengen = 指示承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：申請ユーザー
            var sinseiUser = new Syain
            {
                Id = 3,
                BusyoId = loginUser.BusyoId,
                Kengen = None,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：カウント対象外伺い申請ヘッダ
            var nonFirstApprovalPendingHeader = new UkagaiHeader
            {
                SyainId = sinseiUser.Id,
                ShoninSyainId = shoninSyainId,
                LastShoninSyainId = null,
                Status = status,
                Invalid = invalid
            };

            // データ登録
            SeedEntities(ownBusyo, loginUser, sinseiUser, nonFirstApprovalPendingHeader);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(0, result.Value);
        }

        // =================================================================
        /// <summary>
        /// 指示承認者: 子部署、孫部署、・・・かつ一次承認待ちの申請をカウントすること
        /// </summary>
        // ================================================================
        [TestMethod]
        public async Task GetCountAsync_指示承認者_子部署以降の申請をカウント()
        {
            // ---------- Arrange ----------
            // シード：部署
            // 自部署
            var ownBusyo = new Busyo
            {
                Id = 1,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // 子部署 3件
            var childBusyos = Enumerable.Range(2, 3)
                .Select(i => new Busyo
                {
                    Id = i,
                    IsActive = true,
                    StartYmd = ConstStartYmd,
                    EndYmd = ConstEndYmd,
                    OyaId = ownBusyo.Id,
                    ShoninBusyoId = null,
                    // テストに不要だが、NotNullAbleな項目を設定
                    Code = "",
                    Name = "",
                    KanaName = "",
                    OyaCode = "",
                    KasyoCode = "",
                    KaikeiCode = "",
                })
                .ToList();


            // 孫部署 3件
            var grandChildBusyo = Enumerable.Range(5, 3)
                .Select(i => new Busyo
                {
                    Id = i,
                    IsActive = true,
                    StartYmd = ConstStartYmd,
                    EndYmd = ConstEndYmd,
                    OyaId = childBusyos[0].Id,
                    ShoninBusyoId = null,
                    // テストに不要だが、NotNullAbleな項目を設定
                    Code = "",
                    Name = "",
                    KanaName = "",
                    OyaCode = "",
                    KasyoCode = "",
                    KaikeiCode = "",
                })
                .ToList();

            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 8,
                BusyoId = ownBusyo.Id,
                Kengen = 指示承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：申請ユーザー
            var sinseiBusyos = childBusyos.Concat(grandChildBusyo).ToList();
            // 子部署ユーザー 3件 孫部署ユーザー 3件
            var sinseiUsers = Enumerable.Range(0, sinseiBusyos.Count)
                .Select(i => new Syain
                {
                    Id = 9 + i,
                    BusyoId = sinseiBusyos[i].Id,
                    Kengen = None,
                    StartYmd = ConstStartYmd,
                    EndYmd = ConstEndYmd,
                    // テストに不要だが、NotNullAbleな項目を設定
                    Code = "",
                    Name = "",
                    KanaName = "",
                    Seibetsu = ' ',
                    BusyoCode = "",
                    KingsSyozoku = "",
                })
                .ToList();

            // シード：一次承認待ち伺い申請ヘッダ
            // 子部署 3件 孫部署 3件
            var ukagaiHeaders = Enumerable.Range(0, sinseiBusyos.Count)
                .Select(i => new UkagaiHeader
                {
                    SyainId = sinseiUsers[i].Id,
                    ShoninSyainId = null,
                    Status = 承認待,
                    Invalid = false
                })
                .ToList();

            // データ登録
            SeedEntities(ownBusyo, sinseiBusyos, loginUser, sinseiUsers, ukagaiHeaders);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(ukagaiHeaders.Count, result.Value);
        }

        // =================================================================
        /// <summary>
        /// 指示承認者: 子部署取得中に無限再起が発生しないこと
        /// </summary>
        // ================================================================
        [TestMethod]
        public async Task GetCountAsync_指示承認者_無限再起が発生しないこと()
        {
            // ---------- Arrange ----------
            // シード：部署
            // 自部署
            var ownBusyo = new Busyo
            {
                Id = 1,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // 子部署
            var childBusyo = new Busyo
                {
                    Id = 2,
                    IsActive = true,
                    StartYmd = ConstStartYmd,
                    EndYmd = ConstEndYmd,
                    OyaId = ownBusyo.Id,
                    ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // 孫部署
            var grandChildBusyo = new Busyo
                {
                    Id = 3,
                    IsActive = true,
                    StartYmd = ConstStartYmd,
                    EndYmd = ConstEndYmd,
                    OyaId = childBusyo.Id,
                    ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 4,
                BusyoId = ownBusyo.Id,
                Kengen = 指示承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：申請ユーザー
            // 子部署ユーザー
            var childSinseiUser = new Syain
                {
                    Id = 5,
                    BusyoId = childBusyo.Id,
                    Kengen = None,
                    StartYmd = ConstStartYmd,
                    EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // 孫部署ユーザー
            var grandChildSinseiUser = new Syain
            {
                Id = 6,
                BusyoId = childBusyo.Id,
                Kengen = None,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：一次承認待ち伺い申請ヘッダ
            // 子部署
            var childUkagaiHeader = new UkagaiHeader
                {
                    SyainId = childSinseiUser.Id,
                    ShoninSyainId = null,
                    Status = 承認待,
                    Invalid = false
                };

            // 孫部署
            var grandChildUkagaiHeader = new UkagaiHeader
            {
                SyainId = grandChildSinseiUser.Id,
                ShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            // 循環参照発生
            ownBusyo.OyaId = grandChildBusyo.Id;

            // データ登録
            SeedEntities(ownBusyo, childBusyo, grandChildBusyo, loginUser,
                childSinseiUser, grandChildSinseiUser,
                childUkagaiHeader, grandChildUkagaiHeader);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(2, result.Value);
        }

        // =================================================================
        /// <summary>
        /// 指示承認者: 有効な部署情報を取得していること
        /// </summary>
        // ================================================================
        [TestMethod]
        [DataRow(0, 1, DisplayName = "部署の有効開始日が当日の場合")]
        [DataRow(-1, 0, DisplayName = "部署の有効終了日が当日の場合")]
        public async Task GetCountAsync_指示承認者_有効な子部署以降の申請をカウント(
            int startOffset,
            int endOffset)
        {
            // シード：部署
            // 自部署
            var ownBusyo = new Busyo
            {
                Id = 1,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // 子部署
            var childBusyo = new Busyo
            {
                Id = 2,
                IsActive = true,
                StartYmd = Today.AddDays(startOffset),
                EndYmd = Today.AddDays(endOffset),
                OyaId = ownBusyo.Id,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 3,
                BusyoId = ownBusyo.Id,
                Kengen = 指示承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：子部署申請者ユーザー
            var sinseiUser = new Syain
            {
                Id = 4,
                BusyoId = childBusyo.Id,
                Kengen = None,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：一次承認待ち伺い申請ヘッダ
            var ukgaggaiHeader = new UkagaiHeader
            {
                SyainId = sinseiUser.Id,
                ShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            // データ登録
            SeedEntities(ownBusyo, childBusyo, loginUser, sinseiUser, ukgaggaiHeader);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(1, result.Value);
        }

        // =================================================================
        /// <summary>
        /// 指示承認者: 無効な部署情報を取得していないこと
        /// </summary>
        // ================================================================
        [TestMethod]
        [DataRow(1, 2, true, DisplayName = "部署の有効開始日が未来日の場合")]
        [DataRow(-2, -1, true, DisplayName = "部署の有効終了日が過去日の場合")]
        [DataRow(-1, 1, false, DisplayName = "アクティブフラグがfalseの場合")]
        public async Task GetCountAsync_指示承認者_無効な子部署以降の申請をカウントしない(
            int startOffset,
            int endOffset,
            bool isActive)
        {
            // シード：部署
            // 自部署
            var ownBusyo = new Busyo
            {
                Id = 1,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // 子部署
            var childBusyo = new Busyo
            {
                Id = 2,
                IsActive = isActive,
                StartYmd = Today.AddDays(startOffset),
                EndYmd = Today.AddDays(endOffset),
                OyaId = ownBusyo.Id,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 3,
                BusyoId = ownBusyo.Id,
                Kengen = 指示承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：子部署申請者ユーザー
            var sinseiUser = new Syain
            {
                Id = 4,
                BusyoId = childBusyo.Id,
                Kengen = None,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：一次承認待ち伺い申請ヘッダ
            var firstApprovalPendingHeader = new UkagaiHeader
            {
                SyainId = sinseiUser.Id,
                ShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            // データ登録
            SeedEntities(ownBusyo, childBusyo, loginUser, sinseiUser, firstApprovalPendingHeader);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(0, result.Value);
        }

        // =================================================================
        /// <summary>
        /// 指示承認者: 子部署に他の指示承認者がいる場合、それ以降の部署を取得しないこと
        /// </summary>
        // ================================================================
        [TestMethod]
        public async Task GetCountAsync_指示承認者_指示承認者がいる子部署以降の申請をカウントしない()
        {
            // シード：部署
            // 自部署
            var ownBusyo = new Busyo
            {
                Id = 1,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // 子部署
            var childBusyo = new Busyo
            {
                Id = 2,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = ownBusyo.Id,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // 孫部署
            var grandChildBusyo = new Busyo
            {
                Id = 3,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = childBusyo.Id,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 4,
                BusyoId = ownBusyo.Id,
                Kengen = 指示承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：子部署指示承認者
            var childBusyoApprover = new Syain
            {
                Id = 5,
                BusyoId = childBusyo.Id,
                Kengen = 指示承認者,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                SyokusyuCode = 0,
                SyokusyuBunruiCode = 0,
                NyuusyaYmd = new(),
                Kyusyoku = 0,
                KingsSyozoku = "",
                KaisyaCode = 0,
                IsGenkaRendou = false,
                Jyunjyo = 0,
                Retired = false,
                SyainBaseId = 0,
                KintaiZokuseiId = 0,
                UserRoleId = 0,
            };

            // シード：申請者ユーザー
            // 子部署
            var childSinseiUser = new Syain
            {
                Id = 6,
                BusyoId = childBusyo.Id,
                Kengen = None,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // 孫部署
            var grandChildSinseiUser = new Syain
            {
                Id = 7,
                BusyoId = grandChildBusyo.Id,
                Kengen = None,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：一次承認待ち伺い申請ヘッダ
            var childUkagaiHeader = new UkagaiHeader
            {
                SyainId = childSinseiUser.Id,
                ShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            var grandChildUkagaiHeader = new UkagaiHeader
            {
                SyainId = grandChildSinseiUser.Id,
                ShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            // データ登録
            SeedEntities(ownBusyo, childBusyo, grandChildBusyo, loginUser,
                childBusyoApprover, childSinseiUser, grandChildSinseiUser,
                childUkagaiHeader, grandChildUkagaiHeader);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(0, result.Value);
        }

        // =================================================================
        /// <summary>
        /// 指示承認者: 有効な指示承認者以降の子部署がカウントされないこと
        /// </summary>
        // ================================================================
        [TestMethod]
        [DataRow(0, 1, DisplayName = "指示承認者の有効開始日が当日の場合")]
        [DataRow(-1, 0, DisplayName = "指示承認者の有効終了日が当日の場合")]
        public async Task GetCountAsync_指示承認者_有効な指示承認者がいる子部署以降の申請をカウントしない(
            int startOffset,
            int endOffset)
        {
            // シード：部署
            // 自部署
            var ownBusyo = new Busyo
            {
                Id = 1,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // 子部署
            var childBusyo = new Busyo
            {
                Id = 2,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = ownBusyo.Id,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // 孫部署
            var grandChildBusyo = new Busyo
            {
                Id = 3,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = childBusyo.Id,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 4,
                BusyoId = ownBusyo.Id,
                Kengen = 指示承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：子部署指示承認者
            var childBusyoApprover = new Syain
            {
                Id = 5,
                BusyoId = childBusyo.Id,
                Kengen = 指示承認者,
                StartYmd = Today.AddDays(startOffset),
                EndYmd = Today.AddDays(endOffset),
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：申請者ユーザー
            // 子部署
            var childSinseiUser = new Syain
            {
                Id = 6,
                BusyoId = childBusyo.Id,
                Kengen = None,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // 孫部署
            var grandChildSinseiUser = new Syain
            {
                Id = 7,
                BusyoId = grandChildBusyo.Id,
                Kengen = None,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：一次承認待ち伺い申請ヘッダ
            var childUkagaiHeader = new UkagaiHeader
            {
                SyainId = childSinseiUser.Id,
                ShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            var grandChildUkagaiHeader = new UkagaiHeader
            {
                SyainId = grandChildSinseiUser.Id,
                ShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            // データ登録
            SeedEntities(ownBusyo, childBusyo, grandChildBusyo, loginUser,
                childBusyoApprover, childSinseiUser, grandChildSinseiUser,
                childUkagaiHeader, grandChildUkagaiHeader);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(0, result.Value);
        }

        // =================================================================
        /// <summary>
        /// 指示承認者: 無効な指示承認者以降の子部署の申請はカウントできること
        /// </summary>
        // ================================================================
        [TestMethod]
        [DataRow(1, 2, DisplayName = "指示承認者の有効開始日が未来日の場合")]
        [DataRow(-2, -1, DisplayName = "指示承認者の有効終了日が過去日の場合")]
        public async Task GetCountAsync_指示承認者_無効な指示承認者がいる子部署以降の申請をカウント(
            int startOffset,
            int endOffset)
        {
            // シード：部署
            // 自部署
            var ownBusyo = new Busyo
            {
                Id = 1,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // 子部署
            var childBusyo = new Busyo
            {
                Id = 2,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = ownBusyo.Id,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // 孫部署
            var grandChildBusyo = new Busyo
            {
                Id = 3,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = childBusyo.Id,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 4,
                BusyoId = ownBusyo.Id,
                Kengen = 指示承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：子部署指示承認者
            var childBusyoApprover = new Syain
            {
                Id = 5,
                BusyoId = childBusyo.Id,
                Kengen = 指示承認者,
                StartYmd = Today.AddDays(startOffset),
                EndYmd = Today.AddDays(endOffset),
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：申請者ユーザー
            // 子部署
            var childSinseiUser = new Syain
            {
                Id = 6,
                BusyoId = childBusyo.Id,
                Kengen = None,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // 孫部署
            var grandChildSinseiUser = new Syain
            {
                Id = 7,
                BusyoId = grandChildBusyo.Id,
                Kengen = None,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：一次承認待ち伺い申請ヘッダ
            var childUkagaiHeader = new UkagaiHeader
            {
                SyainId = childSinseiUser.Id,
                ShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            var grandChildUkagaiHeader = new UkagaiHeader
            {
                SyainId = grandChildSinseiUser.Id,
                ShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            // データ登録
            SeedEntities(ownBusyo, childBusyo, grandChildBusyo, loginUser,
                childBusyoApprover, childSinseiUser, grandChildSinseiUser,
                childUkagaiHeader, grandChildUkagaiHeader);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(2, result.Value);
        }

        // =================================================================
        /// <summary>
        /// 指示承認者: 自部署が承認部署として設定されている部署の申請をカウントすること
        /// </summary>
        // ================================================================
        [TestMethod]
        public async Task GetCountAsync_指示承認者_承認部署として設定されている部署の申請をカウント()
        {

            // シード：部署
            // 自部署
            var ownBusyo = new Busyo
            {
                Id = 1,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // 承認対象部署
            var targetBusyo = new Busyo
            {
                Id = 2,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = ownBusyo.Id,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 3,
                BusyoId = ownBusyo.Id,
                Kengen = 指示承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：申請者ユーザー
            // 承認対象部署
            var targetSinseiUser = new Syain
            {
                Id = 4,
                BusyoId = targetBusyo.Id,
                Kengen = None,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：一次承認待ち伺い申請ヘッダ
            var targetUkagaiHeader = new UkagaiHeader
            {
                SyainId = targetSinseiUser.Id,
                ShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            // データ登録
            SeedEntities(ownBusyo, targetBusyo, loginUser, targetSinseiUser, targetUkagaiHeader);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(1, result.Value);
        }

        // =================================================================
        /// <summary>
        /// 指示承認者: 無関係な部署の申請をカウントしないこと
        /// </summary>
        // ================================================================
        [TestMethod]
        public async Task GetCountAsync_指示承認者_無関係な部署の申請をカウントしない()
        {
            // シード：部署
            // 自部署
            var ownBusyo = new Busyo
            {
                Id = 1,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // 無関係な部署
            var nonTargetBusyo = new Busyo
            {
                Id = 2,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = 999,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 3,
                BusyoId = ownBusyo.Id,
                Kengen = 指示承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：申請者ユーザー
            // 承認対象部署
            var targetSinseiUser = new Syain
            {
                Id = 4,
                BusyoId = nonTargetBusyo.Id,
                Kengen = None,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：一次承認待ち伺い申請ヘッダ
            var targetUkagaiHeader = new UkagaiHeader
            {
                SyainId = targetSinseiUser.Id,
                ShoninSyainId = null,
                Status = 承認待,
                Invalid = false
            };

            // データ登録
            SeedEntities(ownBusyo, nonTargetBusyo, loginUser, targetSinseiUser, targetUkagaiHeader);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(0, result.Value);
        }

        // =================================================================
        /// <summary>
        /// 最終指示承認者: 最終承認待ち申請をすべてカウントすること
        /// </summary>
        // ================================================================
        [TestMethod]
        public async Task GetCountAsync_指示最終承認者_最終承認待ち申請をカウント()
        {
            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 1,
                BusyoId = 999,
                Kengen = 指示最終承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：最終承認待ち伺い申請ヘッダ
            var lastApprovalPendingHeaders = Enumerable.Range(2, 10)
                .Select(i => new UkagaiHeader
                {
                    SyainId = i,
                    ShoninSyainId = 99L,
                    LastShoninSyainId = null,
                    Status = 承認待,
                    Invalid = false
                })
                .ToList();

            // データ登録
            SeedEntities(loginUser, lastApprovalPendingHeaders);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(lastApprovalPendingHeaders.Count, result.Value);
        }

        // =================================================================
        /// <summary>
        /// 最終指示承認者: 最終承認待ち以外の申請をカウントしないこと
        /// </summary>
        // ================================================================
        [TestMethod]
        [DataRow(承認, false, 99L, null, DisplayName = "ステータスが承認の場合")]
        [DataRow(差戻, false, 99L, null, DisplayName = "ステータスが差戻の場合")]
        [DataRow(承認待, true, 99L, null, DisplayName = "無効フラグがtrueの場合")]
        [DataRow(承認待, false, null, null, DisplayName = "一次承認社員が入力されていない場合")]
        [DataRow(承認待, false, 99L, 99L, DisplayName = "一次承認社員が入力されている場合")]
        public async Task GetCountAsync_指示最終承認者_条件を満たさない申請をカウントしない(
            ApprovalStatus status,
            bool invalid,
            long? shoninSyainId,
            long? lastShoninSyainId)
        {
            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 1,
                BusyoId = 999,
                Kengen = 指示最終承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：カウント対象外伺い申請ヘッダ
            var nonLastApprovalPendingHeaders = Enumerable.Range(2, 10)
                .Select(i => new UkagaiHeader
                {
                    SyainId = i,
                    ShoninSyainId = shoninSyainId,
                    LastShoninSyainId = lastShoninSyainId,
                    Status = status,
                    Invalid = invalid
                })
                .ToList();

            // データ登録
            SeedEntities(loginUser, nonLastApprovalPendingHeaders);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(0, result.Value);
        }

        // =================================================================
        /// <summary>
        /// 指示承認者＋最終指示承認者: 一次承認待ち申請と最終承認待ち申請の両方をカウントすること
        /// </summary>
        // ================================================================
        [TestMethod]
        public async Task GetCountAsync_指示承認者かつ指示最終承認者_件数を合算する()
        {
            // シード：部署
            // 自部署
            var ownBusyo = new Busyo
            {
                Id = 1,
                IsActive = true,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                OyaId = null,
                ShoninBusyoId = null,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                OyaCode = "",
                KasyoCode = "",
                KaikeiCode = "",
            };

            // シード：ログインユーザー
            var loginUser = new Syain
            {
                Id = 2,
                BusyoId = ownBusyo.Id,
                Kengen = 指示承認者 | 指示最終承認者,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：申請ユーザー
            var sinseiUser = new Syain
            {
                Id = 3,
                BusyoId = ownBusyo.Id,
                StartYmd = ConstStartYmd,
                EndYmd = ConstEndYmd,
                // テストに不要だが、NotNullAbleな項目を設定
                Code = "",
                Name = "",
                KanaName = "",
                Seibetsu = ' ',
                BusyoCode = "",
                KingsSyozoku = "",
            };

            // シード：一次承認待ち伺い申請ヘッダ
            var firstApprovalPendingHeaders = Enumerable.Range(0, 5)
                .Select(i => new UkagaiHeader
                {
                    SyainId = sinseiUser.Id,
                    ShoninSyainId = null,
                    Status = 承認待,
                    Invalid = false
                })
                .ToList();

            // シード：最終承認待ち伺い申請ヘッダ
            var lastApprovalPendingHeaders = Enumerable.Range(3, 10)
                .Select(i => new UkagaiHeader
                {
                    SyainId = i,
                    ShoninSyainId = 1000,
                    LastShoninSyainId = null,
                    Status = 承認待,
                    Invalid = false
                })
                .ToList();

            // データ登録
            SeedEntities(ownBusyo, loginUser, sinseiUser, firstApprovalPendingHeaders, lastApprovalPendingHeaders);

            var controller = CreateController(loginUser);

            // ---------- Act ----------
            var result = await controller.GetCountAsync();

            // ---------- Assert ----------
            Assert.AreEqual(firstApprovalPendingHeaders.Count + lastApprovalPendingHeaders.Count, result.Value);
        }
    }
}
