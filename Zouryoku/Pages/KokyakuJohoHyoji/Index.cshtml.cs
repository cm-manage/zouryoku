using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Data;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Zouryoku.Utils.KokyakuKaisyaSansyouRirekisUtil;

namespace Zouryoku.Pages.KokyakuJohoHyoji
{
    /// <summary>
    /// 顧客情報表示ページモデル
    /// </summary>
    [FunctionAuthorization]
    public class IndexModel : BasePageModel<IndexModel>
    {
        // ---------------------------------------------
        // DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        public IndexModel(
            ZouContext db,
            ILogger<IndexModel> logger,
            IOptions<AppConfig> options,
            TimeProvider? timeProvider = null)
            : base(db, logger, options, timeProvider)
        {
        }

        // ---------------------------------------------
        // プロパティ
        // ---------------------------------------------
        /// <summary>
        /// 顧客情報表示用のViewModel
        /// </summary>
        public KokyakuViewModel KokyakuView { get; set; } = new KokyakuViewModel();

        /// <summary>
        /// 顧客会社の営業社員の部署名
        /// </summary>
        [Display(Name = "所属部署")]
        public string? EigyouSyainBusyoName { get; set; } = string.Empty;

        // ---------------------------------------------
        // OnGet
        // ---------------------------------------------
        /// <summary>
        /// 初期表示
        ///     表示用顧客情報を取得
        ///     営業担当社員.部署名を取得
        ///     参照履歴の登録/更新
        ///         参照履歴が最大件数を超過した場合、削除処理を実行
        /// </summary>
        /// <param name="id">顧客会社ID</param>
        /// <returns>PageResult</returns>
        public async Task<IActionResult> OnGetAsync(long id)
        {
            // 顧客情報を取得
            var kokyakuData = await GetKokyakuKaishaAsync(id);

            // データが取得できなかった場合
            if (kokyakuData is null)
            {
                return RedirectToPage("/ErrorMessage", new { errorMessage = Const.ErrorSelectedDataNotExists });
            }

            // エンティティをViewModelに変換
            KokyakuView = new KokyakuViewModel()
            {
                Kokyaku = kokyakuData,
            };

            // 営業担当者の部署名を取得
            var today = timeProvider.Today();
            EigyouSyainBusyoName = await DepartmentHierarchy.GetDepartmentHierarchyStringAsync(db, today, KokyakuView.EigyouSyainBusyoId);

            // 条件を満たす場合、顧客会社参照履歴を登録/更新/削除処理を実行する
            var now = timeProvider.Now();
            await MaintainKokyakuKaisyaSansyouRirekiAsync(db, id, LoginInfo.User.SyainBaseId, now);

            await db.SaveChangesAsync();

            return Page();
        }

        // ---------------------------------------------
        // プライベートメソッド
        // ---------------------------------------------
        /// <summary>
        /// 顧客詳細取得
        /// 条件に合致する顧客情報が存在しない場合は <c>null</c> を返します。
        /// </summary>
        /// <param name="kokyakuId">顧客会社ID</param>
        /// <returns>顧客情報</returns>
        private async Task<KokyakuKaisha?> GetKokyakuKaishaAsync(long kokyakuId)
        {
            // 現在の日時を取得する
            var today = timeProvider.Today();

            // 引数から顧客情報を取得
            return await db.KokyakuKaishas
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Gyousyu)
                .Include(x => x.EigyoBaseSyain)
                    .ThenInclude(x => x!.Syains.Where(x => x.StartYmd <= today && today <= x.EndYmd))
                .FirstOrDefaultAsync(x => x.Id == kokyakuId);
        }

        // ---------------------------------------------
        // ViewModel
        // ---------------------------------------------
        /// <summary>
        /// 顧客情報ビュー / バインドモデル
        /// </summary>
        public class KokyakuViewModel
        {
            /// ==================================
            /// プロパティ
            /// ==================================

            /// <summary>
            /// 表示対象顧客情報（エンティティ）
            /// </summary>
            public KokyakuKaisha Kokyaku { private get; set; } = new KokyakuKaisha();

            /// <summary>ID</summary>
            [Display(Name = "ID")]
            public long Id => Kokyaku.Id;

            /// <summary>コード</summary>
            [Display(Name = "コード")]
            public string Code => Kokyaku.Code.ToString();

            /// <summary>顧客名</summary>
            [Display(Name = "顧客名")]
            public string Name => Kokyaku.Name;

            /// <summary>カナ顧客名</summary>
            [Display(Name = "顧客名カナ")]
            public string NameKana => Kokyaku.NameKana;

            /// <summary>略称</summary>
            [Display(Name = "顧客名略称")]
            public string Ryakusyou => Kokyaku.Ryakusyou;

            /// <summary>支店</summary>
            [Display(Name = "支店")]
            public string? Shiten => Kokyaku.Shiten;

            /// <summary>郵便番号</summary>
            [Display(Name = "郵便番号")]
            public string? YuubinnBangou => Kokyaku.YuubinBangou;

            /// <summary>住所１</summary>
            [Display(Name = "住所１")]
            public string? Jyuusyo1 => Kokyaku.Jyuusyo1;

            /// <summary>住所２</summary>
            [Display(Name = "住所２")]
            public string? Jyuusyo2 => Kokyaku.Jyuusyo2;

            /// <summary>電話番号</summary>
            [Display(Name = "電話番号")]
            public string? Tel => Kokyaku.Tel;

            /// <summary>Fax</summary>
            [Display(Name = "FAX番号")]
            public string? Fax => Kokyaku.Fax;

            /// <summary>メモ</summary>
            [Display(Name = "メモ")]
            public string? Memo => Kokyaku.Memo;

            /// <summary>Url</summary>
            [Display(Name = "URL")]
            public string? Url => Kokyaku.Url;

            /// <summary>業種名</summary>
            [Display(Name = "業種")]
            public string? GyousyuName => Kokyaku.Gyousyu?.Name;

            /// <summary>営業社員名</summary>
            [Display(Name = "弊社営業担当")]
            public string? EigyouSyainName => Kokyaku.EigyoBaseSyain?.Syains.FirstOrDefault()?.Name;

            /// <summary>営業社員部署ID</summary>
            public long? EigyouSyainBusyoId => Kokyaku.EigyoBaseSyain?.Syains.FirstOrDefault()?.BusyoId;
        }
    }
}
