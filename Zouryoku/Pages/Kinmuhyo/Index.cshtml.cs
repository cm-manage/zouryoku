using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Model.Enums.InquiryType;
using static Model.Enums.LeaveBalanceFetchStatus;
using Common = ZouryokuCommonLibrary.Utils;

namespace Zouryoku.Pages.Kinmuhyo
{
    /// <summary>
    /// 勤務ページモデル
    /// </summary>
    [FunctionAuthorization]
    public class IndexModel : BasePageModel<IndexModel>
    {
        public IndexModel(ZouContext context,
                          ILogger<IndexModel> logger,
                          IOptions<Zouryoku.AppConfig> options,
                          ICompositeViewEngine viewEngine,
                          TimeProvider? timeProvider = null)
            : base(context, logger, options, viewEngine, timeProvider)
        {
        }

        // ---------------------------------------------
        // 通常プロパティ（画面表示用）
        // ---------------------------------------------
        /// <summary>
        /// 勤務画面表示用 ViewModel
        /// </summary>
        public IndexViewModel ViewModel { get; } = new IndexViewModel();

        // ============================================================
        // プロパティ
        // ==========================================================
        
        /// <summary>
        /// 社員ID
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public long SyainId { get; set; }

        /// <summary>
        /// 日報年月日（表示年月指定用）
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public DateOnly? NippouYmd { get; set; }

        /// <summary>
        /// 予定年月日（更新用）
        /// </summary>
        [BindProperty]
        public DateOnly YoteiYmd { get; set; }

        /// <summary>
        /// 出勤フラグ（予定勤務更新用）
        /// </summary>
        [BindProperty]
        public bool ShukkinFlg { get; set; }

        /// <summary>
        /// 残業時間（予定残業更新用）
        /// </summary>
        [BindProperty]
        public short ZangyouJikan { get; set; }


        /// <summary>
        /// 初期表示処理
        /// </summary>
        /// <returns>ページ遷移結果</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            var syain = await InitializePageDataAsync(SyainId, NippouYmd);
            if (syain == null)
            {
                return NotFound();
            }

            return Page();
        }

        /// <summary>
        /// 勤務表更新処理 (年月移動)
        /// </summary>
        /// <returns>勤務表部分のPartialView</returns>
        public async Task<IActionResult> OnPostRefreshCalendarAsync()
        {
            var syain = await InitializePageDataAsync(SyainId, NippouYmd);
            if (syain == null)
            {
                return ErrorJson(string.Format(Const.EmptyReadData));
            }

            var kinmuJokyoHtml = await PartialToJsonAsync(
                "_KinmuJokyoPartial", ViewModel.KinmuJokyoRows);
            var karendaHyojiHtml = await PartialToJsonAsync(
                "_KarendaHyojiPartial", ViewModel.KarendaHyojiRows);

            return SuccessJson(data: new
            {
                kinmuJokyo = kinmuJokyoHtml,
                karendaHyoji = karendaHyojiHtml,
                selectedUserId = syain.Id,
                selectedUserName = syain.Name,
                breakTimeHours = ViewModel.BreakTimeHours,
                departmentEmployees = ViewModel.DepartmentEmployees
            });
        }

        /// <summary>
        /// 画面表示に必要な共通データを初期化します
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="nippouYmd">表示対象日（任意）</param>
        /// <returns>初期化に成功した場合は社員情報、失敗した場合はnull</returns>
        private async Task<Syain?> InitializePageDataAsync(long syainId, DateOnly? nippouYmd)
        {
            ViewModel.CurrentTime = timeProvider.Now();
            var today = ViewModel.CurrentTime.ToDateOnly();
            var systemYearMonth = today.GetStartOfMonth();

            var displayStart = nippouYmd?.GetStartOfMonth() ?? systemYearMonth;

            var displayEnd = displayStart.GetEndOfMonth();
            ViewModel.DisplayYearMonthDate = displayStart;

            // 社員情報取得
            var syain = await GetSyainDataAsync(syainId, today);

            if (syain == null)
            {
                return null;
            }

            ViewModel.SelectedEmployee = syain;

            var syains = new List<Syain>();
            if (syain.BusyoId != 0)
            {
                syains = await GetSyainListByDepartmentAsync(syain, today);
            }

            ViewModel.DepartmentEmployeesRaw = syains;

            // 通知メッセージ情報作成
            ViewModel.DeadlineSimebi = await GetJissekiKakuteiSimebiDataAsync(systemYearMonth);

            // 予定情報取得
            var yukyuKeikakuMeisais = await GetYukyuKeikakuMeisaiListAsync(syain, displayStart, displayEnd);

            var nippouYoteis = await GetNippouYoteiListAsync(syain, displayStart, displayEnd);

            // 振替休暇残情報取得（取得予定日）
            var furikyuuZans = await GetFurikyuuZanListAsync(syain, displayStart, displayEnd);

            // 予定情報作成・登録（取得結果0件なら）
            if (!nippouYoteis.Any())
            {
                nippouYoteis = await InitializePlannedWorkAsync(syain, yukyuKeikakuMeisais, furikyuuZans, displayStart, displayEnd);
            }

            // 申請情報取得（未来月なら取得しない）
            var ukagaiHeaders = await GetUkagaiHeaderListAsync(syain, displayStart, displayEnd, systemYearMonth);

            // 勤務時間(実績)情報取得
            var nippous = await GetNippouListAsync(syain, displayStart, displayEnd);

            // 勤務時間(打刻)情報取得（システム年月または前月のみ実行）
            var workingHours = await GetWorkingHourListAsync(syain, displayStart, displayEnd, systemYearMonth);

            // 非稼働日取得
            var hikadoubis = await GetHikadoubiListAsync(displayStart, displayEnd);

            // 勤務状況情報取得（アラート作成）
            var systemNippous = await GetNippouListAsync(syain, systemYearMonth, systemYearMonth.GetEndOfMonth());

            var systemWorkingHours = await GetWorkingHourListAsync(syain, 
                systemYearMonth, 
                systemYearMonth.GetEndOfMonth(), 
                systemYearMonth);

            var workStatus = await GenerateWorkStatusInfoAsync(syain, nippouYoteis, systemNippous, systemWorkingHours, today);

            ViewModel.KinmuJokyoRows = ViewModel.BuildKinmuJokyoRows(workStatus, syain, systemYearMonth);

            ViewModel.KarendaHyojiRows = ViewModel.BuildKarendaHyojiRows(
                nippouYoteis, 
                nippous, 
                workingHours, 
                ukagaiHeaders, 
                yukyuKeikakuMeisais, 
                furikyuuZans, 
                hikadoubis, 
                today, 
                syain);

            return syain;
        }



        /// <summary>
        /// 予定勤務（出/休）更新ハンドラ
        /// </summary>
        /// <returns>更新結果 (JSON)</returns>
        public async Task<IActionResult> OnPostUpdatePlannedWorkAsync()
        {
            return await UpdateNippouYoteiAsync(SyainId, YoteiYmd, yotei =>
            {
                yotei.Worked = ShukkinFlg;
            });
        }

        /// <summary>
        /// 予定残業時間更新ハンドラ
        /// </summary>
        /// <returns>更新結果 (JSON)</returns>
        public async Task<IActionResult> OnPostUpdatePlannedOvertimeAsync()
        {
            return await UpdateNippouYoteiAsync(SyainId, YoteiYmd, yotei =>
            {
                yotei.ZangyouJikan = ZangyouJikan;
            });
        }

        /// <summary>
        /// 日報予定の共通更新処理
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="yoteiYmd">予定年月日</param>
        /// <param name="updateAction">更新内容の処理</param>
        /// <returns>アクション結果</returns>
        private async Task<IActionResult> UpdateNippouYoteiAsync(long syainId, 
            DateOnly yoteiYmd, 
            Action<NippouYotei> updateAction)
        {
            var nippouYotei = await db.NippouYoteis
                .FirstOrDefaultAsync(x => x.SyainId == syainId && x.NippouYoteiYmd == yoteiYmd);

            if (nippouYotei == null)
            {
                return ErrorJson(Const.EmptyReadData);
            }

            updateAction(nippouYotei);
            await db.SaveChangesAsync();

            return SuccessJson(data: nippouYotei);
        }

        // ---------------------------------------------
        // private メソッド
        // ---------------------------------------------

        /// <summary>
        /// 予定情報（勤務/休日）の初期作成・登録
        /// 非稼働日、休暇管理情報、振替休暇残情報、計画有給明細情報をもとに予定を作成します。
        /// </summary>
        /// <param name="employee">対象社員</param>
        /// <param name="yukyuKeikakuMeisais">計画有給明細リスト</param>
        /// <param name="furikyuuZans">振替休暇残リスト</param>
        /// <param name="displayStart">表示開始日</param>
        /// <param name="displayEnd">表示終了日</param>
        /// <returns>作成・登録後の予定リスト</returns>
        private async Task<List<NippouYotei>> InitializePlannedWorkAsync(Syain employee, 
            List<YukyuKeikakuMeisai> yukyuKeikakuMeisais, 
            List<FurikyuuZan> furikyuuZans, 
            DateOnly displayStart, 
            DateOnly displayEnd)
        {
            // 非稼働日取得
            var hikadoubis = await GetHikadoubiListAsync(displayStart, displayEnd);

            // 休暇管理情報取得
            var kyuukaKanris = await GetKyuukaKanriListAsync(employee, displayStart, displayEnd);

            // 振替休日出勤（休日出勤年月日）
            var furikyuuSyukkinDates = await db.FurikyuuZans.AsNoTracking()
                .Where(f => f.SyainId == employee.Id && displayStart <= f.KyuujitsuSyukkinYmd && f.KyuujitsuSyukkinYmd <= displayEnd)
                .Select(f => f.KyuujitsuSyukkinYmd)
                .ToListAsync();

            // 既存の予定日を取得
            var existingDates = await db.NippouYoteis.AsNoTracking()
                .Where(n => n.SyainId == employee.Id && displayStart <= n.NippouYoteiYmd && n.NippouYoteiYmd <= displayEnd)
                .Select(n => n.NippouYoteiYmd)
                .ToListAsync();

            var nonWorkingSet = hikadoubis.Select(x => x.Ymd).ToHashSet();
            var leaveManagementSet = kyuukaKanris.Select(x => x.TaisyouYmd).ToHashSet();
            var furikyuuYoteiSet = furikyuuZans
                .Where(x => x.SyutokuYoteiYmd.HasValue)
                .Select(x => x.SyutokuYoteiYmd!.Value)
                .ToHashSet();

            var plannedPaidLeaveSet = yukyuKeikakuMeisais.Select(x => x.Ymd).ToHashSet();

            var createdNippouYoteis = displayStart.DateList(displayEnd)
                        .Select(d => new
                        {
                            ExistingDate = existingDates.Contains(d),
                            Date = d,
                        })
                .Where(x => !x.ExistingDate)
                .Select(x =>
                {
                    // 優先順位に従って判定
                    bool worked = true;
                    // 非稼働日、休暇管理、振替休暇取得予定は「休」
                    if (nonWorkingSet.Contains(x.Date) || leaveManagementSet.Contains(x.Date) || furikyuuYoteiSet.Contains(x.Date))
                    {
                        worked = false;
                    }
                    else if (furikyuuSyukkinDates.Contains(x.Date))
                    {
                        worked = true; // 振替休日出勤 → 出
                    }
                    else if (plannedPaidLeaveSet.Contains(x.Date))
                    {
                        worked = false; // 計画有給休暇 → 休
                    }
                    else
                    {
                        worked = true; // その他 → 出
                    }

                    return new NippouYotei
                    {
                        SyainId = employee.Id,
                        NippouYoteiYmd = x.Date,
                        Worked = worked,
                        ZangyouJikan = 0
                    };
                }).ToList();

            if (createdNippouYoteis.Any())
            {
                db.NippouYoteis.AddRange(createdNippouYoteis);
                await db.SaveChangesAsync();
            }

            // 作成後に再取得
            return await GetNippouYoteiListAsync(employee, displayStart, displayEnd);
        }

        /// <summary>
        /// 申請情報取得
        /// 申請情報は未来月なら取得しません。
        /// </summary>
        /// <param name="employee">対象社員</param>
        /// <param name="displayStart">表示開始日</param>
        /// <param name="displayEnd">表示終了日</param>
        /// <param name="systemYearMonth">システムの当月1日</param>
        /// <returns>申請リスト</returns>
        private async Task<List<UkagaiHeader>> GetUkagaiHeaderListAsync(Syain employee, 
            DateOnly displayStart, 
            DateOnly displayEnd, 
            DateOnly systemYearMonth)
        {
            // 未来月なら取得しない
            if (displayStart > systemYearMonth)
            {
                return new List<UkagaiHeader>();
            }

            return await db.UkagaiHeaders.AsNoTracking()
                .Include(u => u.UkagaiShinseis)
                .Where(u => u.SyainId == employee.Id &&
                            displayStart <= u.WorkYmd &&
                            u.WorkYmd <= displayEnd)
                .ToListAsync();
        }

        /// <summary>
        /// 勤務時間(実績)情報取得
        /// </summary>
        /// <param name="employee">対象社員</param>
        /// <param name="displayStart">表示開始日</param>
        /// <param name="displayEnd">表示終了日</param>
        /// <returns>実績日報リスト</returns>
        private async Task<List<Nippou>> GetNippouListAsync(Syain employee, DateOnly displayStart, DateOnly displayEnd)
        {
            return await db.Nippous.AsNoTracking()
                .Include(n => n.DairiNyuryokuRirekis)
                .Where(n => n.SyainId == employee.Id && displayStart <= n.NippouYmd && n.NippouYmd <= displayEnd)
                .OrderBy(n => n.NippouYmd)
                .ToListAsync();
        }

        /// <summary>
        /// 勤務時間(打刻)情報取得
        /// システム年月または前月のみ実行可能です。
        /// </summary>
        /// <param name="employee">対象社員</param>
        /// <param name="displayStart">表示開始日</param>
        /// <param name="displayEnd">表示終了日</param>
        /// <param name="systemYearMonth">システムの当月1日</param>
        /// <returns>打刻情報リスト</returns>
        private async Task<List<WorkingHour>> GetWorkingHourListAsync(Syain employee, 
            DateOnly displayStart, 
            DateOnly displayEnd, 
            DateOnly systemYearMonth)
        {
            // システム年月または前月のみ実行
            if (displayStart != systemYearMonth && displayStart != systemYearMonth.GetStartOfLastMonth())
            {
                return new List<WorkingHour>();
            }

            return await db.WorkingHours.AsNoTracking()
                .Where(w => w.SyainId == employee.Id && displayStart <= w.Hiduke && w.Hiduke <= displayEnd)
                .OrderBy(w => w.Hiduke)
                .ToListAsync();
        }

        /// <summary>
        /// 指定月の実績日報リストを取得します
        /// </summary>
        /// <param name="employee">対象社員</param>
        /// <param name="displayYearMonth">基準年月</param>
        /// <param name="monthOffset">月オフセット（前月なら-1、翌月なら1）</param>
        /// <returns>実績日報リスト</returns>
        private async Task<List<Nippou>> GetNippouListByMonthOffsetAsync(Syain employee, 
            DateOnly displayYearMonth, 
            int monthOffset)
        {
            var targetMonthStart = displayYearMonth.GetStartOfMonth().AddMonths(monthOffset);
            var targetMonthEnd = targetMonthStart.GetEndOfMonth();
            return await GetNippouListAsync(employee, targetMonthStart, targetMonthEnd);
        }

        /// <summary>
        /// 指定月の予定リストを取得します
        /// </summary>
        /// <param name="employee">対象社員</param>
        /// <param name="displayYearMonth">基準年月</param>
        /// <param name="monthOffset">月オフセット（前月なら-1、翌月なら1）</param>
        /// <returns>予定リスト</returns>
        private async Task<List<NippouYotei>> GetNippouYoteiListByMonthOffsetAsync(Syain employee, 
            DateOnly displayYearMonth, 
            int monthOffset)
        {
            var targetMonthStart = displayYearMonth.GetStartOfMonth().AddMonths(monthOffset);
            var targetMonthEnd = targetMonthStart.GetEndOfMonth();
            return await GetNippouYoteiListAsync(employee, targetMonthStart, targetMonthEnd);
        }

        /// <summary>
        /// 勤務状況情報を生成します
        /// </summary>
        /// <param name="employee">対象社員</param>
        /// <param name="nippouYoteis">予定リスト</param>
        /// <param name="nippous">実績日報リスト</param>
        /// <param name="workingHours">打刻情報リスト</param>
        /// <param name="today">今日の日付</param>
        /// <returns>勤務状況情報</returns>
        private async Task<WorkStatusInfo> GenerateWorkStatusInfoAsync(Syain employee, 
            List<NippouYotei> nippouYoteis, 
            List<Nippou> nippous, 
            List<WorkingHour> workingHours, 
            DateOnly today)
        {
            var info = new WorkStatusInfo();
            var systemYearMonth = today.GetStartOfMonth();

            // 連続勤務日数の計算（実績）- 前月・当月・翌月を含む
            var nippousPrevMonth = await GetNippouListByMonthOffsetAsync(employee, systemYearMonth, -1);
            var nippousNextMonth = await GetNippouListByMonthOffsetAsync(employee, systemYearMonth, 1);
            var allNippous = nippousPrevMonth.Concat(nippous).Concat(nippousNextMonth).ToList();
            info.ContinuousWorkDaysActual = CountContinuousWorkDaysActual(allNippous, today);

            // 連続勤務日数の計算（予定）- 前月・当月・翌月を含む
            var nippouYoteisPrevMonth = await GetNippouYoteiListByMonthOffsetAsync(employee, systemYearMonth, -1);
            var nippouYoteisNextMonth = await GetNippouYoteiListByMonthOffsetAsync(employee, systemYearMonth, 1);

            var nippouYoteisCurrent = await GetNippouYoteiListAsync(employee, 
                systemYearMonth, 
                systemYearMonth.GetEndOfMonth());

            info.ContinuousWorkDaysPlanned = CountContinuousWorkDaysPlanned(nippouYoteisCurrent, 
                nippouYoteisPrevMonth, 
                nippouYoteisNextMonth, 
                allNippous, 
                today);

            // 残業時間の計算
            info.TotalOvertimeHours = CalculateTotalOvertime(nippous, workingHours, true);

            // 残業拡張状態の判定
            var hasOvertimeExtension = await CheckOvertimeExtensionAsync(employee, systemYearMonth);
            info.IsOvertimeLimitUnlimited = employee.KintaiZokusei.SeigenTime == 0m;

            if (info.IsOvertimeLimitUnlimited)
            {
                info.OvertimeExtensionStatus = "制限解除";
            }
            else if (hasOvertimeExtension)
            {
                info.OvertimeExtensionStatus = "拡張あり";
            }
            else
            {
                info.OvertimeExtensionStatus = "拡張なし";
            }

            // 有給残日数の取得
            var yuukyuuZan = await db.YuukyuuZans.AsNoTracking()
                .Where(y => y.SyainBaseId == employee.SyainBaseId)
                .OrderByDescending(y => y.Id)
                .FirstOrDefaultAsync();

            if (yuukyuuZan != null)
            {
                info.PaidLeaveRemaining = yuukyuuZan.Wariate + yuukyuuZan.Kurikoshi - yuukyuuZan.Syouka;
                info.PaidLeaveHalfDay = yuukyuuZan.HannitiKaisuu / 2m; // 半日有給は2回で1日
            }

            // 振替休暇（失効間近）
            var furikyuuZans = await db.FurikyuuZans.AsNoTracking()
                .Where(fz => fz.SyainId == employee.Id && fz.DaikyuuKigenYmd > systemYearMonth.GetStartOfMonth())
                .ToListAsync();

            var unusedFurikyuuZans = furikyuuZans
                .Where(fz => fz.SyutokuState == 未 || (fz.IsOneDay && fz.SyutokuYmd1.HasValue && !fz.SyutokuYmd2.HasValue))
                .ToList();

            info.CompensatoryLeaveRemaining = unusedFurikyuuZans.Sum(fz => fz.SyutokuState == 未 && fz.IsOneDay ? 1m : 0.5m);
            info.CompensatoryLeaveNearest3MonthExpiry = unusedFurikyuuZans
                .Select(fz => (DateOnly?)fz.KyuujitsuSyukkinYmd.AddMonths(3))
                .OrderBy(d => d)
                .FirstOrDefault();

            info.CompensatoryLeaveExpiringThisMonth = unusedFurikyuuZans
                .Count(fz => fz.DaikyuuKigenYmd.Year == systemYearMonth.Year && fz.DaikyuuKigenYmd.Month == systemYearMonth.Month);

            info.CompensatoryLeaveExpired3Month = unusedFurikyuuZans
                .Count(fz => fz.KyuujitsuSyukkinYmd.AddMonths(3) < today && fz.DaikyuuKigenYmd > systemYearMonth.GetEndOfMonth());

            // 過去2-6か月の平均残業時間を計算
            info.AverageOvertime = await CalculateAverageOvertime2To6MonthsAsync(employee, systemYearMonth);

            return info;
        }

        /// <summary>
        /// 連続勤務日数（実績）を計算します
        /// </summary>
        /// <param name="nippous">全ての実績日報リスト</param>
        /// <param name="systemDate">基準日</param>
        /// <returns>最大連続勤務日数</returns>
        private int CountContinuousWorkDaysActual(List<Nippou> nippous, DateOnly systemDate)
        {
            // システム日付までに出勤した日付を取得（実績がある日のみ）
            var workedDates = nippous
               .Where(n => n.NippouYmd <= systemDate)
               .Select(n => n.NippouYmd)
               .Distinct()
               .OrderBy(d => d.DayNumber)
               .ToList();

            if (!workedDates.Any())
            {
                return 0;
            }

            return workedDates
                .Select((d, i) => d.DayNumber - i)
                .GroupBy(diff => diff)
                .Select(g => g.Count())
                .Max();
        }

        /// <summary>
        /// 連続勤務日数（予定を含む）を表示用にカウントします
        /// </summary>
        /// <param name="nippouYoteisCurrent">当月の予定リスト</param>
        /// <param name="nippouYoteisPrevMonth">前月の予定リスト</param>
        /// <param name="nippouYoteisNextMonth">翌月の予定リスト</param>
        /// <param name="nippous">全ての実績日報リスト</param>
        /// <param name="systemDate">基準日</param>
        /// <returns>実績連続日数と予定連続日数のタプル</returns>
        private (int actualContinuousCount, int plannedContinuousCount) CountContinuousWorkDaysPlanned(
            List<NippouYotei> nippouYoteisCurrent,
            List<NippouYotei> nippouYoteisPrevMonth,
            List<NippouYotei> nippouYoteisNextMonth,
            List<Nippou> nippous,
            DateOnly systemDate)
        {
            // 前月・当月・翌月の予定を結合（出勤予定のみ）
            var plannedDates = nippouYoteisPrevMonth
                .Concat(nippouYoteisCurrent)
                .Concat(nippouYoteisNextMonth)
                .Where(x => x.Worked)
                .Select(x => x.NippouYoteiYmd)
                .Distinct()
                .OrderBy(d => d.DayNumber)
                .ToList();

            // 実績出勤日のセット
            var actualWorkedDateSet = new HashSet<DateOnly>(
                nippous
                    .Where(n =>
                        n.SyukkinHm1.HasValue ||
                        n.HJitsudou.HasValue ||
                        n.DJitsudou.HasValue ||
                        n.NJitsudou.HasValue)
                    .Select(n => n.NippouYmd)
            );

            // 連続した予定勤務グループを作成し評価（予定 > 実績のみ対象）
            var targetGroup = plannedDates
                .Select((d, i) => new { Date = d, GroupKey = d.DayNumber - i })
                .GroupBy(x => x.GroupKey)
                .Select(g => new
                {
                    PlannedCount = g.Count(),
                    ActualCount = g.Count(x => actualWorkedDateSet.Contains(x.Date))
                })
                .Where(x => x.PlannedCount > x.ActualCount)
                .OrderByDescending(x => x.PlannedCount)
                .FirstOrDefault();

            // 対象なし
            if (targetGroup == null)
                return (0, 0);

            return (targetGroup.ActualCount, targetGroup.PlannedCount);
        }

        /// <summary>
        /// 社員情報を取得します
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiYmd">実績年月日</param>
        /// <returns>社員情報、存在しない場合はnull</returns>
        private async Task<Syain?> GetSyainDataAsync(long syainId, DateOnly jissekiYmd)
        {
            return await db.Syains.AsNoTracking()
                .Include(s => s.Busyo)
                .Include(s => s.KintaiZokusei)
                .Where(s => s.Id == syainId && s.StartYmd <= jissekiYmd && jissekiYmd <= s.EndYmd)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 同一部署の社員一覧を取得します
        /// </summary>
        /// <param name="syain">基準社員</param>
        /// <param name="jissekiYmd">基準日</param>
        /// <returns>社員リスト</returns>
        private async Task<List<Syain>> GetSyainListByDepartmentAsync(Syain syain, DateOnly jissekiYmd)
        {
            return await db.Syains.AsNoTracking()
                .Where(s => s.BusyoId == syain.BusyoId &&
                            s.StartYmd <= jissekiYmd &&
                            jissekiYmd <= s.EndYmd)
                .OrderBy(s => s.Jyunjyo)
                .ToListAsync();
        }

        /// <summary>
        /// 実績確定締め日を取得します
        /// </summary>
        /// <param name="displayYearMonth">基準年月</param>
        /// <returns>実績確定締め日、存在しない場合はnull</returns>
        private async Task<JissekiKakuteiKigenInfo?> GetJissekiKakuteiSimebiDataAsync(DateOnly systemDate)
        {
            var jissekikakuteishimebi = await JissekiKakuteiSimeUtil.GetKakuteiShimeKigenAsync(db, systemDate);
            return jissekikakuteishimebi.FirstOrDefault();
        }

        /// <summary>
        /// 計画有給休暇明細情報を取得します
        /// </summary>
        /// <param name="employee">対象社員</param>
        /// <param name="displayStart">開始日</param>
        /// <param name="displayEnd">終了日</param>
        /// <returns>計画有給明細リスト</returns>
        private async Task<List<YukyuKeikakuMeisai>> GetYukyuKeikakuMeisaiListAsync(Syain employee, DateOnly displayStart, DateOnly displayEnd)
        {
            return await db.YukyuKeikakuMeisais.AsNoTracking()
                .Include(y => y.YukyuKeikaku)
                    .ThenInclude(k => k.YukyuNendo)
                .Where(y => y.YukyuKeikaku.SyainBaseId == employee.SyainBaseId &&
                            y.YukyuKeikaku.YukyuNendo.IsThisYear &&
                            displayStart <= y.Ymd &&
                            y.Ymd <= displayEnd)
                .OrderBy(y => y.Ymd)
                .ToListAsync();
        }

        /// <summary>
        /// 予定情報を取得します
        /// </summary>
        /// <param name="employee">対象社員</param>
        /// <param name="displayStart">開始日</param>
        /// <param name="displayEnd">終了日</param>
        /// <returns>予定リスト</returns>
        private async Task<List<NippouYotei>> GetNippouYoteiListAsync(Syain employee, DateOnly displayStart, DateOnly displayEnd)
        {
            return await db.NippouYoteis.AsNoTracking()
                .Where(n => n.SyainId == employee.Id &&
                            displayStart <= n.NippouYoteiYmd &&
                            n.NippouYoteiYmd <= displayEnd)
                .OrderBy(n => n.NippouYoteiYmd)
                .ToListAsync();
        }

        /// <summary>
        /// 非稼働日を取得します
        /// </summary>
        /// <param name="displayStart">開始日</param>
        /// <param name="displayEnd">終了日</param>
        /// <returns>非稼働日リスト</returns>
        private async Task<List<Hikadoubi>> GetHikadoubiListAsync(DateOnly displayStart, DateOnly displayEnd)
        {
            return await db.Hikadoubis.AsNoTracking()
                .Where(h => displayStart <= h.Ymd && h.Ymd <= displayEnd)
                .OrderBy(h => h.Ymd)
                .ToListAsync();
        }

        /// <summary>
        /// 休暇管理情報を取得します
        /// </summary>
        /// <param name="employee">対象社員</param>
        /// <param name="displayStart">開始日</param>
        /// <param name="displayEnd">終了日</param>
        /// <returns>休暇管理情報リスト</returns>
        private async Task<List<KyuukaKanri>> GetKyuukaKanriListAsync(Syain employee, DateOnly displayStart, DateOnly displayEnd)
        {
            return await db.KyuukaKanris.AsNoTracking()
                .Where(k => k.SyainBaseId == employee.SyainBaseId &&
                            displayStart <= k.TaisyouYmd &&
                            k.TaisyouYmd <= displayEnd)
                .OrderBy(k => k.TaisyouYmd)
                .ToListAsync();
        }

        /// <summary>
        /// 振替休暇残情報を取得します
        /// </summary>
        /// <param name="employee">対象社員</param>
        /// <param name="displayStart">開始日</param>
        /// <param name="displayEnd">終了日</param>
        /// <returns>振替休暇残リスト</returns>
        private async Task<List<FurikyuuZan>> GetFurikyuuZanListAsync(Syain employee, DateOnly displayStart, DateOnly displayEnd)
        {
            return await db.FurikyuuZans.AsNoTracking()
                .Where(f => f.SyainId == employee.Id &&
                            f.SyutokuYoteiYmd.HasValue &&
                            displayStart <= f.SyutokuYoteiYmd.Value &&
                            f.SyutokuYoteiYmd.Value <= displayEnd)
                .OrderBy(f => f.SyutokuYoteiYmd)
                .ToListAsync();
        }

        /// <summary>
        /// 合計残業時間を計算します
        /// </summary>
        /// <param name="nippous">実績日報リスト</param>
        /// <param name="workingHours">打刻情報リスト</param>
        /// <param name="isSystemMonth">システム年月フラグ</param>
        /// <returns>合計残業時間</returns>
        private TimeSpan CalculateTotalOvertime(List<Nippou> nippous, List<WorkingHour> workingHours, bool isSystemMonth)
        {
            var totalHours = nippous.Sum(r =>
                (r.HZangyo ?? 0m) +
                (r.HShinyaZangyo ?? 0m) +
                (r.DJitsudou ?? 0m) +
                (r.DZangyo ?? 0m) +
                (r.DShinyaZangyo ?? 0m) +
                (r.NJitsudou ?? 0m) +
                (r.NShinya ?? 0m));

            if (isSystemMonth && workingHours != null)
            {
                totalHours += workingHours
                    .Where(p => p.SyukkinTime.HasValue && p.TaikinTime.HasValue)
                    .Select(p =>
                    {
                        var workedMinutes = Common.TimeCalculator.CalcJitsudouTimes(p.SyukkinTime!.Value.ToString("HHmm"), 
                            p.TaikinTime!.Value.ToString("HHmm"));
                        return Math.Max(0, workedMinutes - Common.Time.kitei) / 60m;
                    })
                    .Sum();
            }

            return TimeSpan.FromHours((double)totalHours);
        }

        /// <summary>
        /// 残業拡張申請の承認状態を確認します
        /// </summary>
        /// <param name="employee">対象社員</param>
        /// <param name="systemYearMonth">システムの当月1日</param>
        /// <returns>拡張ありの場合はtrue</returns>
        private async Task<bool> CheckOvertimeExtensionAsync(Syain employee, DateOnly systemYearMonth)
        {
            var ukagaiHeader = await db.UkagaiHeaders.AsNoTracking()
                .Include(u => u.UkagaiShinseis)
                .Where(u => u.SyainId == employee.Id &&
                           u.WorkYmd.Year == systemYearMonth.Year &&
                           u.WorkYmd.Month == systemYearMonth.Month &&
                           u.LastShoninYmd.HasValue && // 最終承認済み
                           u.UkagaiShinseis.Any(s => s.UkagaiSyubetsu == 時間外労働時間制限拡張))
                .FirstOrDefaultAsync();

            return ukagaiHeader != null;
        }

        /// <summary>
        /// 2-6か月平均残業時間を計算します
        /// </summary>
        /// <param name="employee">対象社員</param>
        /// <param name="systemYearMonth">システムの当月1日</param>
        /// <returns>平均残業時間</returns>
        private async Task<TimeSpan> CalculateAverageOvertime2To6MonthsAsync(Syain employee, DateOnly systemYearMonth)
        {
            var startDate = systemYearMonth.AddMonths(-6).GetStartOfMonth();
            var endDate = systemYearMonth.GetEndOfLastMonth();

            var nippous = await GetNippouListAsync(employee, startDate, endDate);

            // 月ごとに残業時間を集計
            var monthlyOvertime = nippous
                .GroupBy(r => r.NippouYmd.GetStartOfMonth())
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r =>
                        (r.HZangyo ?? 0m) +
                        (r.HShinyaZangyo ?? 0m) +
                        (r.DJitsudou ?? 0m) +
                        (r.DZangyo ?? 0m) +
                        (r.DShinyaZangyo ?? 0m) +
                        (r.NJitsudou ?? 0m) +
                        (r.NShinya ?? 0m))
                );

            if (monthlyOvertime.Count < 2)
            {
                return TimeSpan.FromHours(0);
            }

            // 過去2-6か月の平均を計算
            var recentMonthsValues = monthlyOvertime.OrderByDescending(k => k.Key)
                .Select(k => k.Value)
                .ToList();

            var averages = Enumerable.Range(
                    2,
                    Math.Min(6, recentMonthsValues.Count) - 1)
                .Select(months => recentMonthsValues.Take(months).Average())
                .ToList();

            var maxAverageHours = averages.Any() ? (double)averages.Max() : 0.0;
            return TimeSpan.FromHours(maxAverageHours);
        }
    }
}
