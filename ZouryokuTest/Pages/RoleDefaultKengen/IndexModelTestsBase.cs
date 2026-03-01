using Model.Enums;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.RoleDefaultKengen;

namespace ZouryokuTest.Pages.RoleDefaultKengen
{
    /// <summary>
    /// RoleDefaultKengenテストの基底クラスです。
    /// </summary>
    public class IndexModelTestsBase : BaseInMemoryDbContextTest
    {
        /// <summary>ログインユーザーの社員BASE ID</summary>
        protected const long LoggedInUserId = 999L;

        private const string LoggedInUserCode = "00000";
        private const string LoggedInUserName = "社員A";

        /// <summary>
        /// テスト対象の IndexModel を生成します。
        /// </summary>
        /// <returns>生成された IndexModel</returns>
        protected IndexModel CreateModel()
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData(),
                ViewModel = new IndexViewModel()
            };

            return SetLoggedInUser(model);
        }

        /// <summary>
        /// モデルにログインユーザーを設定します。
        /// </summary>
        /// <param name="model">設定対象の IndexModel</param>
        /// <returns>ログインユーザーが設定された IndexModel</returns>
        protected IndexModel SetLoggedInUser(IndexModel model)
        {
            var syainBaseEntity = CreateSyainBasis(
                id: LoggedInUserId,
                code: LoggedInUserCode,
                name: LoggedInUserName);
            db.SyainBases.Add(syainBaseEntity);

            var syainEntity = CreateSyain(
                id: LoggedInUserId,
                code: LoggedInUserCode,
                syainBaseId: LoggedInUserId,
                name: LoggedInUserName,
                kanaName: LoggedInUserName);

            db.Syains.Add(syainEntity);

            var loginInfo = new LoginInfo { User = syainEntity };
            model.HttpContext.Session.Set(loginInfo);

            return model;
        }

        /// <summary>
        /// テストデータをDBに登録します。
        /// </summary>
        /// <param name="entities">登録するエンティティ一覧</param>
        protected void SeedEntities(params object[] entities)
        {
            foreach (var entity in entities)
            {
                db.Add(entity);
            }

            db.SaveChanges();
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
    }
}
