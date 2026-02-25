using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Data;
using Zouryoku.Pages.Shared;
using static CommonLibrary.Extensions.DateOnlyExtensions;
using static Model.Enums.AchievementClassification;
using static Model.Enums.DailyReportStatusClassification;
using static Model.Enums.EmployeeWorkType;
using static Zouryoku.Pages.KinmuNippouMiKakuteiCheck.IndexModel.BusyoRange;
using static ZouryokuCommonLibrary.Utils.DateOnlyUtil;
using static Zouryoku.Utils.JissekiKakuteiSimeUtil;

namespace Zouryoku.Pages.KinmuNippouMiKakuteiCheck
{
    /// <summary>
    /// 日報未確定通知画面のモデル。
    /// </summary>
    [FunctionAuthorization]
    public partial class IndexModel : BasePageModel<IndexModel>
    {
        // ======================================
        // フィールド・列挙体
        // ======================================

        /// <summary>
        /// 不正な日報データを持つ社員に付与するサフィックス。
        /// </summary>
        /// <value>全角スペース*2 + "（データ不正あり）"</value>
        private const string BadNippouSuffix = "　　（データ不正あり）";


        /// <summary>
        /// 検索時の対象部署範囲を指定する列挙体。
        /// </summary>
        public enum BusyoRange
        {
            /// <summary>
            /// 全社
            /// </summary>
            [Display(Name = "全社")]
            全社,
            /// <summary>
            /// 単一部署
            /// </summary>
            [Display(Name = "部署")]
            部署,
        }

        // ======================================
        // DI
        // ======================================

        public IndexModel(ZouContext db, ILogger<IndexModel> logger,
            IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine, TimeProvider timeProvider)
            : base(db, logger, optionsAccessor, viewEngine, timeProvider)
        {
            Today = timeProvider.Today();
        }

        // ======================================
        // プロパティ
        // ======================================

        /// <summary>
        /// システム日付。
        /// </summary>
        /// <remarks>クライアント側と参照する日時を同期するためのプロパティ。</remarks>
        public DateOnly Today { get; init; }

        /// <summary>
        /// 検索条件のバインドプロパティ。
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public NippouSearchViewModel SearchConditions { get; set; } = new();

        /// <summary>
        /// 通知可能かどうかのフラグ。
        /// </summary>
        /// <remarks><see cref="OnGetAsync"/>で設定され、初期遷移時に使用される。</remarks>
        public bool CanNotify { get; set; }

        /// <summary>
        /// 検索結果に使用する未確定社員のリスト。
        /// </summary>
        public List<MikakuteiSyainViewModel> MikakuteiSyains { get; set; } = [];

        public override bool UseInputAssets => true;

        // ======================================
        // ハンドラー
        // ======================================

        /// <summary>
        /// 初期遷移時に画面項目初期化を行うハンドラー。
        /// </summary>
        public async Task OnGetAsync()
        {
            // 検索条件の初期化
            // ----------------------------------

            // 通知対象の実績期間
            var jissekiSpan = await GetCanNotifyJissekiSpanAsync(db, Today);

            // 検索日付の初期条件
            var simebi = jissekiSpan.JissekiSimebiYmd;
            // 通知対象の実績期間の確定期限の翌営業日の翌日以降は、次の実績締め日を取得する
            if (await GetNextBusinessDayAsync(db, jissekiSpan.JissekiKakuteiKigenInfo.KakuteiKigenYmd) < Today)
            {
                simebi = await GetNextJissekiSimebiAsync(simebi);
            }

            SearchConditions.Busyo.Id = LoginInfo.User.BusyoId;
            SearchConditions.Busyo.Name = LoginInfo.User.Busyo.Name;
            SearchConditions.Busyo.Range = 部署;
            SearchConditions.Date = simebi;

            // 通知可能かどうかのチェック
            // ----------------------------------

            CanNotify = LoginInfo.User.IsCheckPendingReports
                && await IsInNotificationPeriodAsync(db, Today, jissekiSpan.JissekiSimebiYmd, 
                    jissekiSpan.JissekiKakuteiKigenInfo.KakuteiKigenYmd);
        }

        /// <summary>
        /// 未確定の日報の検索を行うハンドラー。
        /// </summary>
        public async Task<IActionResult> OnGetSearchNippousAsync()
        {
            // 入力日付
            var inputDate = SearchConditions.Date;

            // 検索条件の取得
            // ----------------------------------

            // 部署ID or null
            // NOTE: nullで全社検索を行うようにメソッドを作成している
            var busyoId = SearchConditions.Busyo.Range != 全社 ? SearchConditions.Busyo.Id : null;

            // 未確定者リストの取得
            // ----------------------------------

            // 未確定者リスト
            MikakuteiSyains = await GetMikakuteiSyainsAsync(inputDate, Today, busyoId);

            // 不正データを持つ社員の取得
            // ----------------------------------
            // NOTE: 不正データ = 指定日付から過去一か月間内の、確定状態でない日報

            // 指定日付から過去一か月間の確定日報をIncludeした社員のリスト
            var syainsWithKakutei = await GetSyainsWithKakuteiNippousAsync(inputDate.AddMonths(-1), inputDate, Today, busyoId);

            // 未確定者のIDリスト
            var mikakuteiSyainBaseIds = MikakuteiSyains
                .Select(s => s.SyainBaseId)
                .ToList();

            foreach (var syain in syainsWithKakutei)
            {
                // 既に未確定者リストにある社員はスキップ
                if (mikakuteiSyainBaseIds.Contains(syain.SyainBaseId))
                {
                    continue;
                }

                // 過去一か月間の確定日報の件数
                var count = syain.Nippous
                    .Count(n => n.NippouYmd <= inputDate);
                // 検索期間の日数
                var span = GetDayCount(inputDate.AddMonths(-1), inputDate);

                // 確定件数が検索期間の日数と一致しない社員をビューに格納する
                if (count != span)
                {
                    // 社員氏名にサフィックスを付与する
                    syain.Name = $"{syain.Name}{BadNippouSuffix}";
                    MikakuteiSyains.Add(new MikakuteiSyainViewModel(syain));
                }
            }

            // 検索結果の返却
            // ----------------------------------

            var data = await PartialToJsonAsync("_Nippous", MikakuteiSyains);
            return SuccessJson(null, data);
        }

        // ================================================
        // メソッド
        // ================================================

        /// <summary>
        /// 最終確定日が指定日付より前の日報をもつ、<paramref name="baseDate"/>時点で有効な社員（標準社員外を除く）を取得する。
        /// <paramref name="busyoId"/>を指定したときは、部署IDによる絞り込みも行う。
        /// </summary>
        /// <param name="date">日付</param>
        /// <param name="baseDate">有効かどうかを判定する日付</param>
        /// <param name="busyoId">部署ID</param>
        /// <returns>最終確定日が<paramref name="date"/>より前の社員のビューモデル</returns>
        private async Task<List<MikakuteiSyainViewModel>> GetMikakuteiSyainsAsync(DateOnly date, DateOnly baseDate, long? busyoId = null)
        {
            // SQLクエリ
            var query = CreateQueryForFetchValidStandardSyain(baseDate).AsNoTracking()
                .Include(s => s.Nippous)
                .Where(s =>
                    !s.Nippous.Any(n => n.TourokuKubun == 確定保存)
                    || s.Nippous
                    .Where(n => n.TourokuKubun == 確定保存)
                    .Max(n => n.NippouYmd) < date)
                .AsQueryable();

            // 部署IDを指定されているなら部署IDで絞り込む
            if (busyoId.HasValue)
            {
                query = query
                    .Where(s => s.BusyoId == busyoId.Value);
            }

            return await query
                .Select(s => new MikakuteiSyainViewModel(s))
                .ToListAsync();
        }

        /// <summary>
        /// <paramref name="baseDate"/>時点で有効な社員を、指定期間内の確定済み日報とともに取得する。
        /// </summary>
        /// <param name="startYmd">指定期間の開始年月日</param>
        /// <param name="endYmd">指定期間の終了年月日</param>
        /// <param name="baseDate">基準日付</param>
        /// <param name="busyoId">検索対象の部署のID（nullのときは全社検索）</param>
        /// <returns>確定済み日報のデータを含んだ有効社員のリスト</returns>
        private async Task<List<Syain>> GetSyainsWithKakuteiNippousAsync(DateOnly startYmd, DateOnly endYmd, DateOnly baseDate, long? busyoId = null)
        {
            var query = CreateQueryForFetchValidStandardSyain(baseDate).AsNoTracking()
                .Include(s => s.Nippous
                    .Where(n => n.TourokuKubun == 確定保存)
                    .Where(n => startYmd <= n.NippouYmd))
                .AsQueryable();

            // 部署IDを指定されているなら部署IDで絞り込む
            if (busyoId.HasValue)
            {
                query = query
                    .Where(s => s.BusyoId == busyoId.Value);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// 有効な部署情報をIncludeした有効な（標準社員外でない）社員を取得するクエリを生成する。
        /// </summary>
        /// <param name="baseDate">有効かどうかを判断する基準日付</param>
        /// <returns>基準日付時点で有効な部署情報をIncludeした、基準日付時点で有効な社員を取得するクエリ</returns>
        private IQueryable<Syain> CreateQueryForFetchValidStandardSyain(DateOnly baseDate)
            => db.Syains
                .AsSplitQuery()
                .Include(s => s.SyainBase)
                .Include(s => s.Busyo)
                .Where(s =>
                    s.KintaiZokusei.Code != 標準社員外
                    && !s.Retired
                    && s.StartYmd <= baseDate && baseDate <= s.EndYmd
                    && s.Busyo.StartYmd <= baseDate && baseDate <= s.Busyo.EndYmd)
                .OrderBy(s => s.Busyo.Code)
                .ThenBy(s => s.Code)
                .AsQueryable();

        /// <summary>
        /// 次回の実績締め日を取得する。
        /// </summary>
        /// <param name="simebi">実績締め日</param>
        /// <returns><paramref name="simebi"/>の次の締め日</returns>
        private async Task<DateOnly> GetNextJissekiSimebiAsync(DateOnly simebi)
        {
            // 15日以前 ⇔ 中締めのときは月末を返却
            if (simebi.Day <= NakajimeDay)
            {
                return simebi.GetEndOfMonth();
            }

            // 次回締め日の年月
            var nextSimebiYmd = simebi.GetEndOfMonth().AddMonths(1);

            var kakuteiKigenInfos = await GetKakuteiShimeKigenAsync(db, nextSimebiYmd);

            return new DateOnly(
                nextSimebiYmd.Year,
                nextSimebiYmd.Month,
                kakuteiKigenInfos.Any(k => k.Kubun == 中締め) ? NakajimeDay : nextSimebiYmd.Day);
        }
    }
}
