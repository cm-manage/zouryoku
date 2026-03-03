using CommonLibrary.Extensions;
using CommonLibrary.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Model;
using NPOI.SS.UserModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Model.Enums.AttendanceClassification;
using static Model.Enums.InquiryType;
using static Model.Enums.LeaveBalanceFetchStatus;
using static Zouryoku.Pages.KinmuJokyoKakunin.StatusSearchViewModel.BusyoModeList;
using static Zouryoku.Pages.KinmuJokyoKakunin.WarnLevel;
using FileUtil = Zouryoku.Utils.FileUtil;


namespace Zouryoku.Pages.KinmuJokyoKakunin
{
    public partial class IndexModel : BasePageModel<IndexModel>
    {
        public IndexModel(
            ZouContext db,
            ILogger<IndexModel> logger,
            IOptions<AppConfig> optionsAccessor,
            ICompositeViewEngine viewEngine,
            TimeProvider timeProvider)
            : base(db, logger, optionsAccessor, viewEngine, timeProvider) { }

        public override bool UseInputAssets => true;

        private const string BusyoLabel = "部署";

        private const string SessionKey_StatusViewVm = "StatusView_TableViewModel";

        private const string DateMustBeBefore = "{0}は{1}より前の日付を入力してください。";

        /// <summary>
        /// 検索条件欄 初期表示
        /// </summary>
        public StatusSearchViewModel SearchIndex { get; set; } = new();

        /// <summary>
        /// 残業 集計年度初月
        /// </summary>
        public const int zangyoMonth = KinmuJokyoConstants.ZangyoMonth;

        /// <summary>
        /// 有給 集計年度初月
        /// </summary>
        public const int yukyuMonth = KinmuJokyoConstants.YukyuMonth;

        /// <summary>
        /// Excelテンプレートヘッダ行数
        /// </summary>
        public const int excelHeader = 2;

        public string Dir { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// 初期表示
        /// </summary>
        public void OnGet()
        {
            var today = timeProvider.Today();
            SearchIndex.From = today.ToString("yyyy-MM");
            SearchIndex.To = today.ToString("yyyy-MM");
            SearchIndex.WarnLevel = All;
        }

        /// <summary>
        /// 日報実績.出勤区分1または2のいずれかが、指定した出勤区分に含まれるか
        /// </summary>
        /// <param name="n">日報実績DTO</param>
        /// <param name="targets">対象の出勤区分</param>
        /// <returns></returns>
        private static bool HasAnyKubun(Nippou n, params AttendanceClassification[] targets)
        {
            var code1 = n.SyukkinKubunId1Navigation?.Code;
            var code2 = n.SyukkinKubunId2Navigation?.Code;

            return (code1.HasValue && targets.Contains(code1.Value))
                || (code2.HasValue && targets.Contains(code2.Value));
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetSearchAsync(StatusSearchViewModel Search)
        {
            DateOnly startMonth = DateOnly.ParseExact(Search.From, "yyyy-MM", CultureInfo.InvariantCulture);
            DateOnly endMonth = DateOnly.ParseExact(Search.To, "yyyy-MM", CultureInfo.InvariantCulture);
            // 入れ替える
            if (startMonth > endMonth)
            {
                (startMonth, endMonth) = (endMonth, startMonth);
            }

            // 条件付きバリデーションチェック
            if (Search.BusyoMode != 全社 && Search.Busyo == "")
            {
                ModelState.AddModelError
                        (string.Empty,
                        string.Format(Const.ErrorSelectRequired, BusyoLabel));

                return ModelState.ErrorJson()!;
            }

            // 画面表示モデル
            var vm = new TableViewModel();
            // 早いほう → 1日
            DateOnly searchFrom = startMonth.GetStartOfMonth();
            // 遅いほう → 末日
            DateOnly searchTo = endMonth.GetEndOfMonth();
            // システム日付
            var today = timeProvider.Today();

            // JSON 文字列を long[] に変換
            var selectedBusyoIds = string.IsNullOrEmpty(Search.Busyo)
                ? Array.Empty<long>()
                : Search.Busyo
                    .Trim('[', ']')          // JSONの角括弧を除去
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => long.Parse(s.Trim('"'))) // "1" の場合も対応
                    .ToArray();

            // Include を分割して AsSplitQuery() を使用（デカルト爆発対策）
            var syains = await db.Syains
                    .AsNoTracking()
                    .Where(x => selectedBusyoIds.Length == 0
                        || selectedBusyoIds.Contains(x.BusyoId))
                    .Include(s => s.Busyo)
                    .Include(s => s.KintaiZokusei)
                    .AsSplitQuery()
                    .ToListAsync();

            // 日報データを別途取得（期間条件: searchFrom <= NippouYmd <= searchTo）
            var nippous = await db.Nippous
                    .AsNoTracking()
                    .Where(n => searchFrom <= n.NippouYmd &&
                                n.NippouYmd <= searchTo &&
                                syains.Select(s => s.Id).Contains(n.SyainId))
                    .Include(n => n.SyukkinKubunId1Navigation)
                    .Include(n => n.SyukkinKubunId2Navigation)
                    .AsSplitQuery()
                    .ToListAsync();

            // 伺いデータを別途取得
            var ukagaiHeaders = await db.UkagaiHeaders
                    .AsNoTracking()
                    .Where(h => !h.Invalid && syains.Select(s => s.Id).Contains(h.SyainId))
                    .Include(h => h.UkagaiShinseis)
                    .AsSplitQuery()
                    .ToListAsync();

            // 振替休暇データを別途取得
            var furikyuuZans = await db.FurikyuuZans
                    .AsNoTracking()
                    .Where(z => syains.Select(s => s.Id).Contains(z.SyainId))
                    .ToListAsync();

            // SyainBase関連データを別途取得
            var syainBases = await db.SyainBases
                    .AsNoTracking()
                    .Where(b => syains.Select(s => s.SyainBaseId).Contains(b.Id))
                    .Include(b => b.YuukyuuZans)
                    .Include(b => b.YukyuRirekis)
                        .ThenInclude(r => r.YukyuNendo)
                    .AsSplitQuery()
                    .ToListAsync();

            var syainBaseMap = syainBases.ToDictionary(x => x.Id);

            foreach (var s in syains)
            {
                // ■月別集計（月中に社員IDが変わった場合は1行に合算）
                var monthlyList = nippous
                    .Where(n => n.SyainId == s.Id)
                    .GroupBy(n => new { n.NippouYmd.Year, n.NippouYmd.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        // ■年月
                        YearMonth = $"{g.Key.Year:0000}/{g.Key.Month:00}",
                        // ■実働
                        Jitsudo = g.Sum(n => (n.HJitsudou ?? 0) + (n.DJitsudou ?? 0) + (n.NJitsudou ?? 0)),
                        // ■残業_残業(法休除く)
                        ZangyoExceptHoliday = g.Sum(n => (n.HZangyo ?? 0) + (n.DZangyo ?? 0)),
                        // ■残業_残業
                        Zangyo = g.Sum(n => (n.HZangyo ?? 0) + (n.DZangyo ?? 0) + (n.NJitsudou ?? 0)),
                        // ■特別休暇取得
                        SpecialUsed = g.Count(n => HasAnyKubun(n, 計画特別休暇)),
                        Nippous = g.ToList()
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToList();

                // 伺い入力ヘッダと伺い申請情報から、残業年度・制限超過回数をMap化
                var ukagaiByZangyoNendo = ukagaiHeaders
                    .Where(h => !h.Invalid && h.SyainId == s.Id)
                    .GroupBy(h => h.WorkYmd.Month >= KinmuJokyoConstants.ZangyoMonth ? h.WorkYmd.Year : h.WorkYmd.Year - 1)
                    .ToDictionary(
                        g => g.Key,
                        g => g.SelectMany(h => h.UkagaiShinseis)
                              .Count(d => d.UkagaiSyubetsu == 時間外労働時間制限拡張)
                    );

                // 出勤日
                var workKubuns = new[]
                {
                    通常勤務,
                    AttendanceClassification.休日出勤,
                    年次有給休暇_1日,
                    半日有給,
                    計画有給休暇
                };

                // 出勤日リスト
                var workingDates = nippous
                    .Where(n => n.SyainId == s.Id && HasAnyKubun(n, workKubuns))
                    .Select(n => n.NippouYmd)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                // 連勤ブロック
                var streaks = BuildStreaks(workingDates);

                // 年月・■残業_残業 を Map化
                var zangyoMap = monthlyList.ToDictionary(x => (x.Year, x.Month), x => x.Zangyo);

                // 1か月ずつループ
                foreach (var m in monthlyList)
                {
                    // 集計月末日
                    var finalDay = new DateOnly(m.Year, m.Month, 1).AddMonths(1).AddDays(-1);

                    // ■残業_平均最大
                    var avgMax = GetMaxAverageZangyoForRecentMonths(m.Year, m.Month, zangyoMap);

                    // 残業年度
                    var zangyoNendo = m.Month >= KinmuJokyoConstants.ZangyoMonth ? m.Year : m.Year - 1;
                    // 残業年度初日
                    var zangyoFirstDay = new DateOnly(zangyoNendo, KinmuJokyoConstants.ZangyoMonth, 1);

                    // ■残業_年間累計
                    var yearTotalZangyoExceptHoliday = m.Nippous
                        .Where(n => zangyoFirstDay <= n.NippouYmd && n.NippouYmd <= finalDay)
                        .Sum(n => (n.HZangyo ?? 0) + (n.DZangyo ?? 0));

                    // ■残業_制限超過回数
                    int? overLimitCount = ukagaiByZangyoNendo.TryGetValue(zangyoNendo, out var c) && c > 0 ? c : null;

                    // ■最大連勤日数 ※6以上の月のみ表示
                    var (maxConsecutiveStr, maxConsecutiveNum) = CalcMaxConsecutiveForMonth(streaks, m.Year, m.Month);

                    // 残業情報 行作成
                    WorkRowViewModel zangyoRow = new()
                    {
                        BusyoName = s.Busyo?.Name ?? "",
                        SyainName = s.Name,
                        ZokuseiName = s.KintaiZokusei?.Name ?? "",
                        YearMonth = m.YearMonth,
                        Jitsudo = m.Jitsudo,

                        ZangyoExceptHoliday = m.ZangyoExceptHoliday,
                        Zangyo = m.Zangyo,
                        ZangyoWarnLevel = "",
                        AverageMax = avgMax,
                        YearTotal = yearTotalZangyoExceptHoliday,
                        OverLimitCount = overLimitCount,
                        MaxConsecutiveWorkingDays = maxConsecutiveStr,
                    };

                    // 有給年度
                    var yukyuNendo = m.Month >= KinmuJokyoConstants.YukyuMonth ? m.Year : m.Year - 1;
                    // 有給年度初日
                    var yukyuFirstDay = new DateOnly(yukyuNendo, KinmuJokyoConstants.YukyuMonth, 1);

                    // ■有給休暇_年間累計
                    decimal paidYearTotal = m.Nippous
                        .Where(n => yukyuFirstDay <= n.NippouYmd && n.NippouYmd <= finalDay)
                        .Sum(n =>
                        {
                            if (HasAnyKubun(n, 年次有給休暇_1日, 計画有給休暇))
                                return 1m;
                            if (HasAnyKubun(n, 半日有給))
                                return 0.5m;
                            return 0m;
                        });

                    // 年度初めの有給日数
                    decimal wariate = 0;

                    var currentNendo = today.Month >= KinmuJokyoConstants.YukyuMonth ? today.Year : today.Year - 1;

                    if (syainBaseMap.TryGetValue(s.SyainBaseId, out var syainBase))
                    {
                        if (yukyuNendo == currentNendo)
                        {
                            wariate = syainBase.YuukyuuZans?.SingleOrDefault()?.Wariate ?? 0;
                        }
                        else
                        {
                            wariate = syainBase.YukyuRirekis
                                .SingleOrDefault(z => z.YukyuNendo.Nendo == yukyuNendo)?.Wariate ?? 0;
                        }
                    }

                    // ■有給休暇_残日数
                    var paidRemain = wariate - paidYearTotal;

                    // 半日休_年間累計
                    var halfDayUsed = m.Nippous
                        .Where(n => yukyuFirstDay <= n.NippouYmd && n.NippouYmd <= finalDay)
                        .Count(n => HasAnyKubun(n, 半日有給));
                    var paidHalfRemain = 10 - (0.5m * halfDayUsed);

                    // ■振替休暇_残日数
                    var transferRemain = furikyuuZans
                        .Where(z => z.SyainId == s.Id &&
                                   z.KyuujitsuSyukkinYmd <= finalDay &&
                                   finalDay <= z.DaikyuuKigenYmd)
                        .Sum(CalcFurikyuuRemain);

                    // ■振替休暇_3ヶ月期限
                    var transfer3Month = furikyuuZans
                        .Where(z => z.SyainId == s.Id &&
                                   m.Month == z.KyuujitsuSyukkinYmd.AddMonths(3).AddDays(-1).Month)
                        .Sum(CalcFurikyuuRemain);

                    // ■振替休暇_失効日数
                    var transferExpired = furikyuuZans
                        .Where(z => z.SyainId == s.Id && m.Month == z.DaikyuuKigenYmd.Month)
                        .Sum(CalcFurikyuuRemain);

                    // 有給情報 行作成
                    HolidayRowViewModel yukyuRow = new()
                    {
                        BusyoName = zangyoRow.BusyoName,
                        SyainName = zangyoRow.SyainName,
                        ZokuseiName = zangyoRow.ZokuseiName,
                        YearMonth = zangyoRow.YearMonth,
                        Jitsudo = zangyoRow.Jitsudo,

                        PaidYearTotal = paidYearTotal,
                        PaidRemain = paidRemain,
                        PaidHalfRemain = paidHalfRemain,
                        SpecialUsed = m.SpecialUsed == 0 ? null : m.SpecialUsed,
                        TransferRemain = transferRemain,
                        Transfer3Month = transfer3Month,
                        TransferExpired = transferExpired
                    };

                    // 警告レベルをViewModel側で設定
                    zangyoRow.ApplyWarningLevels(appSettings, avgMax, yearTotalZangyoExceptHoliday, overLimitCount, maxConsecutiveNum);
                    yukyuRow.ApplyWarningLevel(appSettings, paidYearTotal, m.Month);

                    // 警告レベルでの絞り込み
                    if (Search.WarnLevel == WarnLevel.All ||
                        Search.WarnLevel == WarnLevel.Warn && (zangyoRow.IsWarn || yukyuRow.IsWarn) ||
                        Search.WarnLevel == WarnLevel.Notice && (zangyoRow.IsNotice || yukyuRow.IsNotice))
                    {
                        vm.WorkList.Add(zangyoRow);
                        vm.HolidayList.Add(yukyuRow);
                    }
                }
            }

            // Excel出力用にSessionへ保持
            HttpContext.Session.Set(vm, SessionKey_StatusViewVm);

            var html = await PartialToJsonAsync("_IndexPartial", vm);
            return SuccessJson(data: html);
        }

        // 警告レベル判定はViewModel側へ移譲しました

        /// <summary>
        /// 警告レベルから表示用CSSクラス名を返す
        /// </summary>
        /// <param name="level">警告レベル</param>
        /// <returns>表示用CSSクラス名</returns>
        public static string ToCssClass(WarnLevel level)
        {
            return level switch
            {
                Warn => "warn-level",
                Notice => "notice-level",
                _ => ""
            };
        }

        /// <summary>
        /// 出勤日リストから「連勤ブロック（連続出勤期間）」を生成する
        /// </summary>
        /// <param name="workingDates">出勤日リスト</param>
        /// <returns>
        /// 連勤ブロックの一覧
        /// Start : 連勤開始日
        /// End   : 連勤終了日
        /// Length: 連勤日数（Start～Endを含む）
        /// </returns>
        private static List<(DateOnly Start, DateOnly End, int Length)> BuildStreaks(List<DateOnly> workingDates)
        {
            var blocks = new List<(DateOnly, DateOnly, int)>();

            if (workingDates.Count == 0)
                return blocks;

            // 現在の連勤ブロックの開始日
            var blockStart = workingDates[0];
            // 現在の連勤ブロックの直前日
            var previousDay = workingDates[0];

            foreach (var currentDay in workingDates)
            {
                // 前日から連続していれば連勤継続
                if (currentDay == previousDay.AddDays(1))
                {
                    previousDay = currentDay;
                    continue;
                }

                // 連勤が途切れたので、ここまでをブロックとして確定
                blocks.Add((
                    blockStart,
                    previousDay,
                    previousDay.DayNumber - blockStart.DayNumber + 1
                ));

                // 新しい連勤ブロックを開始
                blockStart = currentDay;
                previousDay = currentDay;
            }

            // 最後の連勤ブロックを追加
            blocks.Add((
                blockStart,
                previousDay,
                previousDay.DayNumber - blockStart.DayNumber + 1
            ));

            return blocks;
        }

        /// <summary>
        /// 指定した年月における最大連勤日数を算出する。
        /// 月初（1日）を含む連勤が最長の場合は、前月以前を含めた通算連続日数を[]付きで返す。
        /// 6日未満の場合は表示対象外として null を返す。
        /// </summary>
        /// <param name="streaks">連勤ブロック</param>
        /// <param name="year">対象年</param>
        /// <param name="month">対象月</param>
        /// <returns>
        /// maxConsecutiveStr : 最大連勤日数(文字列)
        /// maxConsecutiveNum : 最大連勤日数(数値)
        /// </returns>
        private static (string? maxConsecutiveStr, decimal? maxConsecutiveNum) CalcMaxConsecutiveForMonth(
            List<(DateOnly Start, DateOnly End, int Length)> streaks,
            int year,
            int month)
        {
            // 対象月初日
            var monthStart = new DateOnly(year, month, 1);
            // 対象月最終日
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            int maxDaysInMonth = 0;
            int totalDaysIncludingPreviousMonths = 0;
            bool spansFromPreviousMonth = false;

            foreach (var (Start, End, Length) in streaks)
            {
                // 対象月と重ならない連勤は無視
                if (End < monthStart || monthEnd < Start)
                    continue;

                // 月内に収まる部分だけ切り出す
                var effectiveStart = Start < monthStart ? monthStart : Start;
                var effectiveEnd = End > monthEnd ? monthEnd : End;

                var daysInMonth =
                    effectiveEnd.DayNumber - effectiveStart.DayNumber + 1;

                if (daysInMonth > maxDaysInMonth)
                {
                    maxDaysInMonth = daysInMonth;
                    totalDaysIncludingPreviousMonths = Length;
                    spansFromPreviousMonth =
                        Start < monthStart && monthStart <= End;
                }
            }

            // 5連勤以下は表示しない
            if (maxDaysInMonth < 6)
                return (null, null);

            // 前月から継続している連勤の場合は [] 表示
            if (spansFromPreviousMonth)
                return ($"[{totalDaysIncludingPreviousMonths}]", totalDaysIncludingPreviousMonths);

            return (maxDaysInMonth.ToString(), maxDaysInMonth);
        }

        /// <summary>
        /// 指定した年月を起点として、「直近2ヶ月～直近6ヶ月」の各期間における月平均残業時間を算出し、
        /// その最大値を返す。
        /// ※対象期間内にデータが存在しない月は無視し、存在する月のみで平均を計算する。
        /// </summary>
        /// <param name="baseYear">集計対象の年</param>
        /// <param name="baseMonth">集計対象の月</param>
        /// <param name="monthlyZangyoMap">(年, 月) → 残業時間 の対応表</param>
        /// <returns>月平均残業時間の最大値</returns>
        private static decimal GetMaxAverageZangyoForRecentMonths(
            int baseYear,
            int baseMonth,
            IReadOnlyDictionary<(int Year, int Month), decimal> monthlyZangyoMap)
        {
            // 平均値の中での最大値
            decimal maxAverageZangyo = 0m;

            // 集計対象月の初日
            var baseMonthDate = new DateOnly(baseYear, baseMonth, 1);

            // 直近2ヶ月～6ヶ月の平均を順に計算
            for (int monthsSpan = 2; monthsSpan <= 6; monthsSpan++)
            {
                // 集計対象期間の残業合計
                decimal totalZangyoInSpan = 0m;
                // 集計対象月数
                int monthsWithDataCount = 0;

                // baseMonth から過去にさかのぼる
                for (int offset = 0; offset < monthsSpan; offset++)
                {
                    var targetMonth = baseMonthDate.AddMonths(-offset);
                    var key = (targetMonth.Year, targetMonth.Month);

                    // 対象月の残業データがあれば加算
                    if (monthlyZangyoMap.TryGetValue(key, out var zangyo))
                    {
                        totalZangyoInSpan += zangyo;
                        monthsWithDataCount++;
                    }
                }

                // 集計対象期間の平均残業時間
                var averageZangyo =
                    totalZangyoInSpan / monthsWithDataCount;

                // 最大値を更新
                if (averageZangyo > maxAverageZangyo)
                {
                    maxAverageZangyo = averageZangyo;
                }
            }
            return maxAverageZangyo;
        }

        /// <summary>
        /// セルを取得または作成する
        /// </summary>
        private static ICell GetOrCreateCell(IRow row, int cellIndex)
        {
            var cell = row.GetCell(cellIndex);
            if (cell == null)
                cell = row.CreateCell(cellIndex);
            return cell;
        }

        /// <summary>
        /// 振替休暇残テーブルの未取得日数を計算する
        /// </summary>
        /// <param name="z">振替休暇残DTO</param>
        /// <returns>残日数</returns>
        private static decimal CalcFurikyuuRemain(FurikyuuZan z)
        {
            // 1日休・未取得
            if (z.IsOneDay && z.SyutokuState == 未) return 1.0m;
            // 1日休・半日取得済
            if (z.IsOneDay && z.SyutokuState == 半日) return 0.5m;
            // 半休・未取得
            if (!z.IsOneDay && z.SyutokuState == 未) return 0.5m;
            // それ以外＝取得済み
            return 0m;
        }

        /// <summary>
        /// Excel出力
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetExportExcelAsync()
        {
            try
            {
                var vmOption = HttpContext.Session.Get<TableViewModel>(SessionKey_StatusViewVm);
                if (!vmOption.IsSome)
                    return BadRequest("検索結果が存在しません。再度検索してください。");

                var vm = vmOption.IfNone(() => null!)!;

                // vmがnullの場合
                if (vm == null)
                    return BadRequest("検索結果の取得に失敗しました。再度検索してください。");

                // テンプレートファイル（フルパス指定の場合は Dir を付けない）
                var folderPath = appSettings.TemplatesFolderPath;
                var file = appSettings.KinmuJokyoFileName;
                var template = Path.IsPathRooted(folderPath)
                    ? Path.Combine(folderPath, file)
                    : Path.Combine(Dir, folderPath, file);

                // テンプレートファイルの存在確認
                if (!System.IO.File.Exists(template))
                    return BadRequest($"テンプレートファイルが見つかりません: {template}");

                // Excel作成（NPOI）
                byte[] bytes = ExcelUtil.Write(book =>
                {
                    var sheet = book.GetSheet("勤務状況");
                    if (sheet == null)
                        throw new InvalidOperationException("テンプレートに「勤務状況」シートが見つかりません。");

                    int rowIndex = excelHeader;
                    int maxCount = Math.Max(vm.WorkList.Count, vm.HolidayList.Count);

                    // 行数分、1行目をコピーして行を追加
                    for (int i = 1; i < maxCount; i++)
                    {
                        sheet.CopyAndInsertRow(rowIndex, rowIndex + 1);
                    }

                    // WorkList と HolidayList を同じ行に並べて記入
                    for (int i = 0; i < maxCount; i++)
                    {
                        IRow? row = sheet.GetRow(rowIndex + i);
                        if (row == null)
                            row = sheet.CreateRow(rowIndex + i);

                        // WorkList のデータ（存在する場合）
                        if (i < vm.WorkList.Count)
                        {
                            var w = vm.WorkList[i];
                            // 共通
                            GetOrCreateCell(row, 0).SetCellValue(w.BusyoName);
                            GetOrCreateCell(row, 1).SetCellValue(w.SyainName);
                            GetOrCreateCell(row, 2).SetCellValue(w.ZokuseiName);
                            GetOrCreateCell(row, 3).SetCellValue(w.YearMonth);
                            GetOrCreateCell(row, 4).SetCellValue((double)w.Jitsudo);

                            // 残業
                            GetOrCreateCell(row, 5).SetCellValue((double)w.ZangyoExceptHoliday);
                            GetOrCreateCell(row, 6).SetCellValue((double)w.Zangyo);
                            GetOrCreateCell(row, 7).SetCellValue((double)w.AverageMax);
                            GetOrCreateCell(row, 8).SetCellValue((double)w.YearTotal);
                            if (w.OverLimitCount != null) GetOrCreateCell(row, 9).SetCellValue((double)w.OverLimitCount);
                            GetOrCreateCell(row, 10).SetCellValue(w.MaxConsecutiveWorkingDays);
                        }

                        // HolidayList のデータ（存在する場合）
                        if (i < vm.HolidayList.Count)
                        {
                            var h = vm.HolidayList[i];
                            // 休暇
                            GetOrCreateCell(row, 11).SetCellValue((double)h.PaidYearTotal);
                            GetOrCreateCell(row, 12).SetCellValue((double)h.PaidRemain);
                            GetOrCreateCell(row, 13).SetCellValue((double)h.PaidHalfRemain);
                            if (h.SpecialUsed != null) GetOrCreateCell(row, 14).SetCellValue((double)h.SpecialUsed);
                            GetOrCreateCell(row, 15).SetCellValue((double)h.TransferRemain);
                            GetOrCreateCell(row, 16).SetCellValue((double)h.Transfer3Month);
                            GetOrCreateCell(row, 17).SetCellValue((double)h.TransferExpired);
                        }
                    }

                    for (int i = 0; i <= 17; i++)
                        sheet.AutoSizeColumn(i);
                }
                , template
                );

                var fileName = $"勤務状況.xlsx";

                return File(
                    bytes,
                    FileUtil.XlsxContextType,
                    fileName
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Excel出力処理でエラーが発生しました");
                return BadRequest("Excel出力処理でエラーが発生しました。");
            }
        }
    }

    /// <summary> 
    /// 警告レベル定義 
    /// </summary>
    public enum WarnLevel
    {
        /// <summary>全て</summary>
        [Display(Name = "全て")]
        All = 0,

        /// <summary>通知</summary>
        [Display(Name = "通知")]
        Notice = 1,

        /// <summary>警告</summary>
        [Display(Name = "警告")]
        Warn = 2
    }
}
