using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Pages.Shared.Components;
using Zouryoku.Utils;
using static Model.Enums.DailyReportStatusClassification;
using static Model.Enums.PcOperationType;
using static Zouryoku.Pages.Attendance.AttendanceList.SortOrderEnum;
using static Zouryoku.Pages.Attendance.AttendanceList.SortSelectedEnum;
using static Zouryoku.Utils.Const;

namespace Zouryoku.Pages.Attendance.AttendanceList
{
    /// <summary>
    /// 出退勤一覧ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class IndexModel : BasePageModel<IndexModel>
    {
        // ============================================================
        // 定数
        // ============================================================

        // 検索結果最大件数
        public const int SearchResultMaxCount = 4000;

        // クッキー有効期限(日)
        private const int CookieExpiresDays = 30;

        // クッキー保存用モデル
        public class AttendanceListCookie
        {
            public string? SelectedKikanOption { get; set; }
            public long? SelectedSyainBaseId { get; set; }
        }

        public IndexModel(ZouContext context,
                          ILogger<IndexModel> logger,
                          IOptions<Zouryoku.AppConfig> options,
                          ICompositeViewEngine viewEngine,
                          TimeProvider? timeProvider = null)
      : base(context, logger, options, viewEngine, timeProvider)
        { }

        // ============================================================
        // プロパティ
        // ==========================================================

        // 入力アセット使用
        public override bool UseInputAssets => true;

        // 検索モデル / バインドモデル
        [BindProperty]
        public SearchModel Search { get; set; } = new SearchModel();

        // 社員ドロップダウンビュー / バインドモデル
        [BindProperty]
        public SyainViewModel SyainView { get; set; } = new SyainViewModel();

        // 出退勤一覧ビュー / バインドモデル
        public ViewModel AttendanceView { get; set; } = new ViewModel { };

        // ============================================================
        // ハンドラーメソッド
        // ============================================================

        /// <summary>
        /// ページ初期表示
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync()
        {
            var today = timeProvider.Today();

            // クッキーから選択値を復元
            var cookie = HttpContext.GetCookieOrDefault<AttendanceListCookie>();

            // 社員ドロップダウン初期化
            await InitializeSyainView(LoginInfo.User, cookie.SelectedSyainBaseId, today);

            // 検索モデル初期化
            InitializeSearchModel(LoginInfo.User, cookie.SelectedKikanOption);

            return Page();
        }

        /// <summary>
        /// 部署変更
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostBusyoAsync()
        {
            var today = timeProvider.Today();
            var items = await ViewImpactDepartmentAsync(Search.BusyoId, SyainView.SelectedId, today);

            return SuccessJson(null, items);
        }

        /// <summary>
        /// 検索ボタン
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostSearchAsync()
        {
            var today = timeProvider.Today();

            // 期間取得
            var (kikanStart, kikanEnd) = Search.GetNormalizeKikan();

            // 日付リスト作成
            var dayList = kikanStart.DateList(kikanEnd);

            // 対象社員決定
            var syainBaseIds = await ResolveTargetSyainBaseIdsAsync(Search.BusyoId, today);

            // ----- 各種データ取得 -----
            // 社員情報検索
            var syains = await LoadSyainsAsync(syainBaseIds);

            // 勤怠打刻検索
            var workings = await LoadWorkingHoursAsync(syainBaseIds, kikanStart, kikanEnd);

            // 日報実績検索
            var nippous = await LoadNippousAsync(syainBaseIds, kikanStart, kikanEnd);

            // 伺い情報検索
            var ukagaiHeaders = await LoadUkagaiHeadersAsync(syainBaseIds, kikanStart, kikanEnd);

            // PCログ検索
            var pcLogs = await LoadPcLogsAsync(syainBaseIds, kikanStart, kikanEnd);

            // 非稼働日検索
            var hikadoubis = await LoadHikadoubisAsync(kikanStart, kikanEnd);

            // データ整形用ルックアップ辞書作成
            // Whereで線形探索するのを避けるため
            // 社員の期間別ルックアップ：社員ID→その期間レコード一覧
            var syainById = syains.GroupBy(s => s.SyainBaseId)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.StartYmd).ToList());

            // 勤怠打刻： (baseid, 日付) → List<WorkingHour>
            var workingByKey = workings.GroupBy(w => (w.Syain.SyainBaseId, w.Hiduke))
                .ToDictionary(g => g.Key, g => g.ToList());

            // 日報： (baseid, 日付) → Nippou?
            var nippouByKey = nippous.GroupBy(n => (n.Syain.SyainBaseId, n.NippouYmd))
                .ToDictionary(g => g.Key, g => g.FirstOrDefault());

            // 伺い： (baseid, 日付) → List<UkagaiShinsei>
            var ukagaiByKey = ukagaiHeaders
                .SelectMany(h => h.UkagaiShinseis.Select(s => new { h.Syain.SyainBaseId, h.WorkYmd, Shinsei = s }))
                .GroupBy(x => (x.SyainBaseId, x.WorkYmd))
                .ToDictionary(g => g.Key, g => g.Select(x => x.Shinsei).ToList());

            // PCログ： (baseid, 日付) → List<PcLog>
            var pcLogByKey = pcLogs.GroupBy(p => (p.Syain!.SyainBaseId, p.Datetime.ToDateOnly()))
                .ToDictionary(g => g.Key, g => g.ToList());

            // 非稼働日：日付 → Hikadoubi?
            var hikadoubiByDate = hikadoubis.ToDictionary(h => h.Ymd, h => h);

            // 勤務表作成
            var kinmuList = BuildKinmuDataList
            (
                syainBaseIds,
                dayList,
                syainById,
                workingByKey,
                nippouByKey,
                ukagaiByKey,
                pcLogByKey,
                hikadoubiByDate
            );

            string message = string.Empty;

            if (kinmuList.Count > SearchResultMaxCount)
            {
                kinmuList= kinmuList.Take(SearchResultMaxCount).ToList(); // 超過分は切り捨て
                message = string.Format(WarningTooManyResults, SearchResultMaxCount);
            }

            // ソートしてviewModelにセット
            var sorted = SortKinmuList(kinmuList);

            // 検索条件をクッキー保存
            SaveSearchCookies();

            // ViewModelセット
            AttendanceView = new ViewModel
            {
                KinmuDataList = sorted.ToList(),
                LoginUser = LoginInfo.User,
                Message = message,
            };

            var data = await PartialToJsonAsync("_IndexPartial", AttendanceView);
            return SuccessJson(null, data);
        }

        /// <summary>
        /// 社員ドロップダウン初期化
        /// </summary>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <param name="selectedSyainBaseId">選択済み社員BASE ID</param>
        /// <param name="today">本日</param>
        private async Task InitializeSyainView(Syain loginUser,long? selectedSyainBaseId, DateOnly today)
        {
            var syainBaseId = selectedSyainBaseId?.ToString() ?? "";

            // 社員ドロップダウン初期化
            SyainView = new SyainViewModel
            {
                IsSelectDepartment = loginUser.IsSelectDepartment,
                Items = await ViewImpactDepartmentAsync(loginUser.BusyoId, syainBaseId, today)
            };

            // 選択済み社員BASE IDがリストにあればセットする
            if (SyainView.Items.Any(x => x.Value == syainBaseId))
            {
                SyainView.SelectedId = syainBaseId ?? "";
            }
        }

        /// <summary>
        /// 検索モデル初期化
        /// </summary>
        /// <param name="loginUser">ログインユーザーの社員情報</param>
        /// <param name="selectedKikan">選択済み期間</param>
        private void InitializeSearchModel(Syain loginUser, string? selectedKikan)
        {
            Search = new SearchModel
            {
                BusyoId = loginUser.BusyoId,
                BusyoName = loginUser.Busyo?.Name ?? "",
                Kikan = new DatepickerRangeModel.Values
                {
                    SelectedOption =
                        (Enum.TryParse<DatepickerRangeModel.Option>(selectedKikan, out var result))
                            ? result
                            : DatepickerRangeModel.Option.ThisMonth,
                },
            };
        }

        /// <summary>
        /// 対象社員ID群解決
        /// </summary>
        /// <param name="loginUser">ログインユーザーの社員情報</param>
        /// <param name="today">本日</param>
        /// <returns></returns>
        private async Task<long[]> ResolveTargetSyainBaseIdsAsync(long busyoId, DateOnly today)
        {
            if (!string.IsNullOrEmpty(SyainView.SelectedId))
                return new[] { long.Parse(SyainView.SelectedId) };

            var items = await ViewImpactDepartmentAsync(busyoId, "", today);
            return items.Skip(1).Select(x => long.Parse(x.Value)).ToArray();
        }

        /// <summary>
        /// 検索条件をクッキー保存
        /// </summary>
        private void SaveSearchCookies()
        {
            HttpContext.SetCookie(
                new AttendanceListCookie
                {
                    SelectedKikanOption = Search.Kikan.SelectedOption.ToString(),
                    SelectedSyainBaseId = string.IsNullOrEmpty(SyainView.SelectedId) ? null : long.Parse(SyainView.SelectedId),
                },
            null,
            CookieExpiresDays);
        }

        // ============================================================
        // DB ロード
        // ============================================================

        // 社員情報取得
        private async Task<List<Syain>> LoadSyainsAsync(long[] ids)
            => await db.Syains.AsNoTracking()
                              .Where(x => ids.Contains(x.SyainBaseId) && !x.Retired)
                              .ToListAsync();

        // 勤怠打刻取得
        private async Task<List<WorkingHour>> LoadWorkingHoursAsync(long[] ids, DateOnly from, DateOnly to)
            => await db.WorkingHours.AsNoTracking()
                                    .Where(x => ids.Contains(x.Syain.SyainBaseId) &&
                                                !x.Deleted &&
                                                from <= x.Hiduke &&
                                                x.Hiduke <= to)
                                    .Include(x => x.Syain)
                                    .ToListAsync();

        // 日報実績取得
        private async Task<List<Nippou>> LoadNippousAsync(long[] ids, DateOnly from, DateOnly to)
            => await db.Nippous.AsNoTracking()
                               .AsSplitQuery()
                               .Where(x => ids.Contains(x.Syain.SyainBaseId) &&
                                           x.TourokuKubun == 確定保存 &&
                                           from <= x.NippouYmd &&
                                           x.NippouYmd <= to)
                               .Include(x => x.Syain)
                               .Include(x => x.SyukkinKubunId1Navigation)
                               .Include(x => x.SyukkinKubunId2Navigation)
                               .Include(x => x.DairiNyuryokuRirekis)
                               .ToListAsync();

        // 伺い情報取得
        private async Task<List<UkagaiHeader>> LoadUkagaiHeadersAsync(long[] ids, DateOnly from, DateOnly to)
            => await db.UkagaiHeaders.AsNoTracking()
                                     .AsSplitQuery()
                                     .Where(x => ids.Contains(x.Syain.SyainBaseId) &&
                                                 from <= x.WorkYmd &&
                                                 x.WorkYmd <= to)
                                     .Include(x => x.Syain)
                                     .Include(x => x.UkagaiShinseis)
                                     .ToListAsync();

        // PCログ取得
        private async Task<List<PcLog>> LoadPcLogsAsync(long[] ids, DateOnly from, DateOnly to)
        {
            var targetOps = new[] { ログオン, ログオフ };

            return await db.PcLogs.AsNoTracking()
                                  .Where(x => ids.Contains(x.Syain!.SyainBaseId) &&
                                              from.ToDateTime() <= x.Datetime &&
                                              x.Datetime < to.AddDays(1).ToDateTime() &&
                                              targetOps.Contains(x.Operation))
                                  .Include(x => x.Syain)
                                  .ToListAsync();
        }

        // 非稼働日取得
        private async Task<List<Hikadoubi>> LoadHikadoubisAsync(DateOnly from, DateOnly to)
            => await db.Hikadoubis.AsNoTracking()
                                  .Where(x => from <= x.Ymd && x.Ymd <= to)
                                  .ToListAsync();

        // ============================================================
        // 勤務データ作成
        // ============================================================
        private static List<KinmuData> BuildKinmuDataList(
            long[] syainBaseIds,
            List<DateOnly> dayList,
            Dictionary<long, List<Syain>> syainById,
            Dictionary<(long, DateOnly), List<WorkingHour>> workingByKey,
            Dictionary<(long, DateOnly), Nippou?> nippouByKey,
            Dictionary<(long, DateOnly), List<UkagaiShinsei>> ukagaiByKey,
            Dictionary<(long, DateOnly), List<PcLog>> pcLogByKey,
            Dictionary<DateOnly, Hikadoubi> hikadoubiByDate)
        {
            return syainBaseIds.SelectMany(s =>
            {
                syainById.TryGetValue(s, out var syainPeriods);

                return dayList.Select(d => new
                {
                    Period = syainPeriods?.FirstOrDefault(x => x.StartYmd <= d && d <= x.EndYmd),
                    Date = d,
                })
                .Where(x => x.Period != null)
                .Select(x =>
                {
                    hikadoubiByDate.TryGetValue(x.Date, out var hd);
                    workingByKey.TryGetValue((s, x.Date), out var wh);
                    nippouByKey.TryGetValue((s, x.Date), out var np);
                    ukagaiByKey.TryGetValue((s, x.Date), out var us);
                    pcLogByKey.TryGetValue((s, x.Date), out var pl);
                    return new KinmuData
                    {
                        SyainData = x.Period!,
                        Hiduke = x.Date,
                        HikadoubiData = hd,
                        WorkingHours = wh ?? new List<WorkingHour>(),
                        NippouData = np,
                        UkagaiShinseis = us ?? new List<UkagaiShinsei>(),
                        PcLogs = pl ?? new List<PcLog>(),
                    };
                });
            })
            .Take(SearchResultMaxCount + 1) // 最大件数制限+1件（超過判定用）
            .ToList();
        }

        // ============================================================
        // ソート
        // ============================================================
        private IEnumerable<KinmuData> SortKinmuList(List<KinmuData> kinmuList)
        {
            // ソートしてviewModelにセット
            bool ascending = (Search.SortOrderType == 昇順);
            IOrderedEnumerable<KinmuData> sorted;

            // ソートキー関数の宣言
            static int sortKeyJyunjyo(KinmuData x) => x.SyainData.Jyunjyo;
            static string sortKeyCode(KinmuData x) => x.SyainData.Code;
            static DateOnly sortKeyHiduke(KinmuData x) => x.Hiduke;

            if (Search.SortSelected == 担当者順)
            {
                // 一次キー：社員順（昇降を切替）
                sorted = ascending
                    ? kinmuList.OrderBy(sortKeyJyunjyo)
                    : kinmuList.OrderByDescending(sortKeyJyunjyo);

                // 二次・三次キー：常に昇順
                sorted = sorted
                    .ThenBy(sortKeyCode)
                    .ThenBy(sortKeyHiduke);
            }
            else
            {
                // 一次キー：日付順（昇降を切替）
                sorted = ascending
                    ? kinmuList.OrderBy(sortKeyHiduke)
                    : kinmuList.OrderByDescending(sortKeyHiduke);

                // 二次・三次キー：常に昇順
                sorted = sorted
                    .ThenBy(sortKeyJyunjyo)
                    .ThenBy(sortKeyCode);
            }

            return sorted;
        }

        // ============================================================
        // 影響部門
        // ============================================================

        /// <summary>
        /// 影響部門表示
        /// </summary>
        /// <param name="busyoId">部署ID</param>
        /// <param name="today">本日</param>
        /// <returns></returns>
        private async Task<List<SelectListItem>> ViewImpactDepartmentAsync(long busyoId, string selectedSyainBaseId, DateOnly today)
        {
            // 影響部門範囲（ログインユーザの部署で検索）
            var busyos = await ImpactDepartment.GetImpactDepartmentAsync(db, busyoId, today);

            // 影響部門社員情報取得
            var syainItems = await GetSyainItemsAsync(busyos, selectedSyainBaseId);
            syainItems.Insert(0, new SelectListItem { Value = "", Text = "" });
            return syainItems;
        }

        /// <summary>
        /// 影響部門社員取得
        /// </summary>
        /// <param name="busyos">影響部門</param>
        /// <returns>社員</returns>
        private async Task<List<SelectListItem>> GetSyainItemsAsync(List<Busyo> busyos, string selectedSyainBaseId)
        {
            var busyoIds = busyos.Select(x => x.Id);
            var today = timeProvider.Today();
            return await db.Syains.Where(x => (busyoIds.Contains(x.BusyoId)) &&
                                         !x.Retired && x.StartYmd <= today &&
                                         today <= x.EndYmd)
                                  .Include(x => x.Busyo)
                                  .OrderBy(x => x.Busyo.Jyunjyo)
                                  .ThenBy(x => x.Jyunjyo)
                                  .Select(x => new SelectListItem
                                  {
                                      Value = x.SyainBaseId.ToString(),
                                      Text = x.Name,
                                      Selected = x.SyainBaseId.ToString() == selectedSyainBaseId,
                                  })
                                  .ToListAsync();
        }

    }

    // ============================================================
    // 列挙型
    // ============================================================

    /// <summary>
    /// ソート種類
    /// </summary>
    public enum SortSelectedEnum : int
    {
        [Display(Name = "担当者順")]
        担当者順 = 0,
        [Display(Name = "日付順")]
        日付順 = 1
    }

    /// <summary>
    /// ソート順
    /// </summary>
    public enum SortOrderEnum : int
    {
        [Display(Name = "昇順")]
        昇順 = 0,
        [Display(Name = "降順")]
        降順 = 1
    }

    // ============================================================
    // ビューモデル / バインドモデル
    // ============================================================

    /// <summary>
    /// 社員ドロップダウンリスト用ビュー / バインドモデル / 権限
    /// </summary>
    public class SyainViewModel
    {
        /// <summary>対象社員</summary>
        [Display(Name = "対象社員")]
        [DataType(DataType.Text)]
        public string SelectedId { get; set; } = string.Empty;

        public List<SelectListItem> Items { get; set; } = [];

        /// <summary>部署選択可能</summary>
        public bool IsSelectDepartment { get; set; }
    }

    /// <summary>
    /// 検索用ビュー / バインドモデル他
    /// 入力検証属性を付与
    /// </summary>
    public class SearchModel
    {
        /// <summary>期間選択/// </summary>
        [Display(Name = "期間")]
        public DatepickerRangeModel.Values Kikan { get; init; } =
            new() { SelectedOption = DatepickerRangeModel.Option.ThisMonth };
        public IReadOnlyList<DatepickerRangeModel.Option> KikanRangeOptions => new[]
        {
            DatepickerRangeModel.Option.Yesterday,
            DatepickerRangeModel.Option.Today,
            DatepickerRangeModel.Option.ThisMonth,
            DatepickerRangeModel.Option.Past2Months,
            DatepickerRangeModel.Option.Past3Months,
            DatepickerRangeModel.Option.PastHalfYear,
            DatepickerRangeModel.Option.ThisFiscalYear,
        };
        /// <summary>部署ID/// </summary>
        [Display(Name = "部署ID")]
        public long BusyoId { get; set; }
        /// <summary>部署/// </summary>
        [Display(Name = "部署")]
        public string BusyoName { get; set; } = string.Empty;
        /// <summary>ソート選択/// </summary>
        [Display(Name = "ソート選択")]
        [DataType(DataType.Text)]
        public SortSelectedEnum SortSelected { get; set; } = SortSelectedEnum.担当者順;
        public SelectListItem[] SortSelectItems => Enum.GetValues<SortSelectedEnum>()
            .Select(e => new SelectListItem
            {
                Value = e.ToString(),
                Text = e.ToString()
            })
            .ToArray();
        /// <summary>ソート順/// </summary>
        [Display(Name = "ソート順")]
        [DataType(DataType.Text)]
        public SortOrderEnum SortOrderType { get; set; } = SortOrderEnum.昇順;
        public SelectListItem[] SortOrderItems => Enum.GetValues<SortOrderEnum>()
            .Select(e => new SelectListItem
            {
                Value = e.ToString(),
                Text = e.ToString()
            })
            .ToArray();

        /// <summary>
        /// 期間正規化
        /// </summary>
        /// <returns>期間from～to</returns>
        public (DateOnly From, DateOnly To) GetNormalizeKikan()
        {
            var from = Kikan.From;
            var to = Kikan.To;

            // from と to の大小関係を正規化
            if (to < from)
                (from, to) = (to, from);

            return (from, to);
        }
    }

    /// <summary>
    /// パーシャルクラス_indexPartial.cshtml用
    /// </summary>
    public class ViewModel
    {
        /// <summary>
        /// グーグルマップ
        /// </summary>
        public readonly string googlemap = "https://www.google.com/maps?q=";

        /// <summary>
        /// 勤務表
        /// </summary>
        public IList<KinmuData> KinmuDataList { get; set; } = [];

        /// <summary>
        /// ログインユーザー
        /// </summary>
        public Syain LoginUser { get; set; } = new Syain();

        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 曜日背景色・文字色用のクラス
    /// </summary>
    public static class StyleYoubiColorClasses
    {
        // 祝祭日
        public const string Holiday = "app-line--holiday";
        // 日曜日
        public const string Sunday = "app-line--sunday";
        // 土曜日
        public const string Saturday = "app-line--saturday";
        // それ以外
        public const string Weekday = "app-line--weekday";
    }
}
