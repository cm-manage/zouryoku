using CommonLibrary.Extensions;
using Model.Enums;
using Model.Model;
using Zouryoku.Utils;
using ZouryokuCommonLibrary.Utils;
using static Model.Enums.AchievementClassification;
using static Model.Enums.DailyReportStatusClassification;
using static Model.Enums.EmployeeWorkType;
using static Model.Enums.HolidayFlag;
using static Model.Enums.InquiryType;

namespace Zouryoku.Pages.Kinmuhyo
{
    /// <summary>
    /// 勤務状況情報（アラート・サマリー用）
    /// </summary>
    public sealed class WorkStatusInfo
    {
        public int ContinuousWorkDaysActual { get; set; }
        public (int, int) ContinuousWorkDaysPlanned { get; set; }
        public TimeSpan TotalOvertimeHours { get; set; }
        public string OvertimeExtensionStatus { get; set; } = "拡張なし";
        public decimal PaidLeaveRemaining { get; set; }
        public decimal PaidLeaveHalfDay { get; set; }
        public decimal CompensatoryLeaveRemaining { get; set; }
        public DateOnly? CompensatoryLeaveNearest3MonthExpiry { get; set; }
        public int CompensatoryLeaveExpiringThisMonth { get; set; }
        public int CompensatoryLeaveExpired3Month { get; set; }
        public bool IsOvertimeLimitUnlimited { get; set; }
        public TimeSpan AverageOvertime { get; set; }
    }

    /// <summary>
    /// 勤務画面表示用 ViewModel
    /// </summary>
    public class IndexViewModel
    {
        // 定数
        public const int LevelDanger = 1;
        public const int LevelWarn = 2;
        public const int LevelPrimary = 3;

        private const string Limit100 = "100:00";
        private const string Limit80 = "80:00";
        private const string Limit45 = "45:00";

        // 残業アラート閾値
        private const double OvertimeRedThreshold100 = 95;
        private const double OvertimeYellowThreshold100 = 85;
        private const double OvertimeRedThreshold80 = 70;
        private const double OvertimeYellowThreshold80 = 60;
        private const double OvertimeRedThreshold45 = 40;
        private const double OvertimeYellowThreshold45 = 30;

        // 2-6か月平均残業閾値
        private const double AverageOvertimeRedThreshold = 70;
        private const double AverageOvertimeYellowThreshold = 60;

        // 連勤アラート閾値
        private const int ContinuousWorkRedThreshold = 10;
        private const int ContinuousWorkYellowThreshold = 7;
        private const int ContinuousWorkPlannedRedThreshold = 11;

        // 申請期限日数
        private const int ApplicationCutoffDays = -3;

        // 表示用ラベル
        private const string OvertimeLabel = "残業";
        private const string PaidLeaveLabel = "有給残日数 (半日有給)";
        private const string CompensatoryLeaveLabel = "振替休暇残日数";
        private const string ContinuousWorkLabel = "連勤日数";
        private const string ContinuousWorkPlannedLabel = "連勤日数 (予定)";
        private const string CompensatoryExpiringThisMonthLabel = "振休 (当月失効)";
        private const string CompensatoryExpired3MonthLabel = "振休 (3か月超過)";
        private const string ApplicationStatusLabel = "申請";

        // 単位・フォーマット
        private const string DayUnit = "日";
        private const string DayFormat = "{0}日";
        private const string PaidLeaveHalfFormat = "({0:F1}日)";
        private const string ZeroDayValue = "0日";
        private const string DashValue = "-";
        
        // メッセージフォーマット
        private const string DeadlineFirstHalfMessage =
            "日報締切　{0}月前半の日報は、{1}までに確定処理をお願いします。";
        private const string DeadlineSecondHalfMessage =
            "日報締切　{0}月後半の日報は、{1}までに確定処理をお願いします。";
        private const string DeadlineFullMonthMessage =
            "日報締切　{0}月 前半＆後半 の日報は、{1}までに確定処理をお願いします。"
            + "（今回、1ヶ月分となります。ご注意ください）";
        private const string DeadlineTodayMessage =
            "本日、実績の締切日です。";
        private const string OvertimeLimitMessage =
            "残業上限に近づいています。残業をおさえるよう業務を調整してください"
            + "（残業可能時間は{0}[{1}]）。";
        private const string OvertimeScheduledMessage =
            "予定を含めた残業時間は{0}です[{1}]。";
        private const string CompensatoryNearestExpiryMessage =
            "直近の3か月期限は{0}です。";
        private const string ContinuousWorkActualActionMessage =
            "連続勤務{0}日間経過しています。休暇を取得してください。";
        private const string ContinuousWorkActualAdjustmentMessage =
            "連続勤務{0}日間経過しています。勤務予定を調整してください。";
        private const string ContinuousWorkPlannedAdjustmentMessage =
            "連続勤務{0}日間の予定になっています。勤務予定を調整してください。";
        private const string CompensatoryExpiringThisMonthMessage =
            "今月失効する振替休日が{0}日あります。必ず取得してください。";
        private const string CompensatoryExpired3MonthMessage = 
            "3か月期限以内に取得していない振替休日が{0}日あります。必ず取得してください。";

        // 内部データ
        public DateTime CurrentTime { get; set; }
        public Syain? SelectedEmployee { get; set; }
        public List<Syain> DepartmentEmployeesRaw { get; set; } = new();
        public JissekiKakuteiKigenInfo? DeadlineSimebi { get; set; }
        public DateOnly DisplayYearMonthDate { get; set; }

        // 算出プロパティ
        public string DepartmentName => SelectedEmployee?.Busyo?.Name ?? string.Empty;
        public long SelectedUserId => SelectedEmployee?.Id ?? 0;
        public string SelectedUserName => SelectedEmployee?.Name ?? string.Empty;
        public string DisplayYearMonth => DisplayYearMonthDate.YMSlash();

        /// <summary>
        /// 編集可能月かどうかの判定 (当月以降か)
        /// </summary>
        /// <returns>編集可能な場合はtrue</returns>
        public bool IsEditableMonth()
            => DisplayYearMonthDate >= CurrentTime.ToDateOnly().GetStartOfMonth();

        /// <summary>
        /// アラートバナーに表示するメッセージの取得
        /// </summary>
        /// <returns>アラートメッセージ</returns>
        public string AlertBanner()
        {
            if (DeadlineSimebi == null)
                return string.Empty;

            var month = DeadlineSimebi.KakuteiKigenYmd.Month;
            var deadlineDate = DeadlineSimebi.KakuteiKigenYmd.MDJp();

            return DeadlineSimebi.Kubun switch
            {
                中締め => string.Format(DeadlineFirstHalfMessage, month, deadlineDate),
                月末締め => string.Format(DeadlineSecondHalfMessage, month, deadlineDate),
                一か月締め => string.Format(DeadlineFullMonthMessage, month, deadlineDate),
                _ => DeadlineTodayMessage
            };
        }

        /// <summary>
        /// 勤務状況アラートサマリー行の構築
        /// </summary>
        /// <param name="workStatus">勤務状況情報</param>
        /// <param name="syain">対象社員</param>
        /// <param name="systemYearMonth">基準年月</param>
        /// <returns>勤務状況行リスト</returns>
        public List<KinmuJokyoRowViewModel> BuildKinmuJokyoRows(
            WorkStatusInfo workStatus, 
            Syain syain, 
            DateOnly systemYearMonth)
        {
            var rows = new List<KinmuJokyoRowViewModel>();

            // 1. 残業時間行
            var totalHours = workStatus.TotalOvertimeHours.TotalHours;
            string overTimeValue = $"{(int)totalHours:D2}:{workStatus.TotalOvertimeHours.Minutes:D2}";
            var averagetotalHours = workStatus.AverageOvertime.TotalHours;
            string averageOverTimeValue = $"{(int)averagetotalHours:D2}:{workStatus.AverageOvertime.Minutes:D2}";
            string? message = null;
            int titleLevel = LevelPrimary;
            int messageLevel = 0;

            bool isExempt = syain.KintaiZokusei.Code is フリー or 標準社員外;

            // 当月の残業アラート
            AddOvertimeAlert(
                ref message, ref titleLevel,ref messageLevel, totalHours,
                OvertimeRedThreshold100, OvertimeYellowThreshold100, Limit100,
                workStatus.OvertimeExtensionStatus, 
                isExempt);

            if (!workStatus.IsOvertimeLimitUnlimited)
            {
                AddOvertimeAlert(
                    ref message, ref titleLevel, ref messageLevel, totalHours,
                    OvertimeRedThreshold80, OvertimeYellowThreshold80, 
                    Limit80, workStatus.OvertimeExtensionStatus, 
                    isExempt);

                AddOvertimeAlert(
                    ref message, ref titleLevel, ref messageLevel, totalHours,
                    OvertimeRedThreshold45, OvertimeYellowThreshold45, 
                    Limit45, workStatus.OvertimeExtensionStatus, 
                    isExempt);
            }

            if (message == null)
            {
                message = isExempt
                    ? "-"
                    : string.Format(
                        OvertimeScheduledMessage, 
                        overTimeValue, 
                        workStatus.OvertimeExtensionStatus);
            }


            // 2-6か月平均による残業アラート
            if (workStatus.AverageOvertime.TotalHours >= AverageOvertimeYellowThreshold)
            {
                message = string.Format(
                    OvertimeLimitMessage, 
                    Limit80, 
                    workStatus.OvertimeExtensionStatus);
                messageLevel = workStatus.AverageOvertime.TotalHours 
                    >= AverageOvertimeRedThreshold 
                    ? LevelDanger 
                    : LevelWarn;
                titleLevel = LevelPrimary;
            }

            rows.Add(
                new KinmuJokyoRowViewModel(
                    Label: OvertimeLabel,
                    Value: averageOverTimeValue,
                    Description: message ?? DashValue,
                    TitleLevel: titleLevel,
                    MessageLevel: messageLevel));

            // 2. 有給残日数行
            var paidLeaveValue = workStatus.PaidLeaveRemaining > 0m
                ? $"{workStatus.PaidLeaveRemaining:F1}{DayUnit}"
                : ZeroDayValue;
            var paidLeaveHalf = workStatus.PaidLeaveHalfDay > 0m
                ? string.Format(PaidLeaveHalfFormat, workStatus.PaidLeaveHalfDay)
                : "";

            rows.Add(
                new KinmuJokyoRowViewModel(
                    Label: PaidLeaveLabel,
                    Value: $"{paidLeaveValue} {paidLeaveHalf}",
                    Description: "-",
                    TitleLevel: LevelPrimary,
                    MessageLevel: 0));

            // 3. 振替休暇残日数行
            var compLeaveValue = workStatus.CompensatoryLeaveRemaining > 0
                ? $"{workStatus.CompensatoryLeaveRemaining:F1}日"
                : "0日";
            var compLeaveDesc = workStatus.CompensatoryLeaveNearest3MonthExpiry.HasValue
                ? string.Format(
                    CompensatoryNearestExpiryMessage,
                    workStatus.CompensatoryLeaveNearest3MonthExpiry.Value.YMDJp())
                : "-";

            rows.Add(
                new KinmuJokyoRowViewModel(
                    Label: CompensatoryLeaveLabel,
                    Value: compLeaveValue,
                    Description: compLeaveDesc,
                    TitleLevel: LevelPrimary,
                    MessageLevel: 0));

            // 4. 連続勤務アラート（実績）
            AddAlertRow(
                rows,
                ContinuousWorkLabel,
                string.Format(DayFormat, workStatus.ContinuousWorkDaysActual),
                workStatus.ContinuousWorkDaysActual,
                ContinuousWorkRedThreshold,
                ContinuousWorkYellowThreshold,
                string.Format(
                    ContinuousWorkActualActionMessage,
                    workStatus.ContinuousWorkDaysActual),
                string.Format(
                    ContinuousWorkActualAdjustmentMessage,
                    workStatus.ContinuousWorkDaysActual));

            // 5. 連続勤務アラート（予定）
            AddAlertRow(
                rows,
                ContinuousWorkPlannedLabel,
                string.Format(DayFormat, workStatus.ContinuousWorkDaysPlanned.Item2),
                workStatus.ContinuousWorkDaysPlanned.Item2,
                ContinuousWorkPlannedRedThreshold,
                ContinuousWorkYellowThreshold,
                string.Format(
                    ContinuousWorkPlannedAdjustmentMessage, 
                    workStatus.ContinuousWorkDaysPlanned.Item2),
                string.Format(
                    ContinuousWorkPlannedAdjustmentMessage, 
                    workStatus.ContinuousWorkDaysPlanned.Item2));

            // 6. 振替休暇アラート（当月失効）
            if (workStatus.CompensatoryLeaveExpiringThisMonth > 0)
            {
                rows.Add(
                    new KinmuJokyoRowViewModel(
                        Label: CompensatoryExpiringThisMonthLabel,
                        Value: string.Format(DayFormat, workStatus.CompensatoryLeaveExpiringThisMonth),
                        Description: string.Format(
                            CompensatoryExpiringThisMonthMessage,
                            workStatus.CompensatoryLeaveExpiringThisMonth),
                        TitleLevel: LevelDanger,
                        MessageLevel: LevelDanger));
            }

            // 7. 振替休暇アラート（3か月超過）
            if (workStatus.CompensatoryLeaveExpired3Month > 0)
            {
                rows.Add(
                    new KinmuJokyoRowViewModel(
                        Label: CompensatoryExpired3MonthLabel,
                        Value: string.Format(DayFormat, workStatus.CompensatoryLeaveExpired3Month),
                        Description: string.Format(
                            CompensatoryExpired3MonthMessage,
                            workStatus.CompensatoryLeaveExpired3Month),
                        TitleLevel: LevelWarn,
                        MessageLevel: LevelWarn));
            }

            return rows;
        }

        /// <summary>
        /// 条件に応じてアラート行を追加します
        /// </summary>
        private void AddAlertRow(
            List<KinmuJokyoRowViewModel> rows, 
            string label, 
            string value, 
            decimal currentVal, 
            decimal redThreshold, 
            decimal yellowThreshold, 
            string redDesc, 
            string yellowDesc)
        {
                if (currentVal >= redThreshold)
            {
                rows.Add(new KinmuJokyoRowViewModel(label, value, redDesc, LevelDanger, LevelDanger));
            }
            else if (currentVal >= yellowThreshold)
            {
                rows.Add(new KinmuJokyoRowViewModel(label, value, yellowDesc, LevelWarn, LevelWarn));
            }
        }

        /// <summary>
        /// 残業アラートの判定とメッセージ設定
        /// </summary>
        private void AddOvertimeAlert(
            ref string? message, 
            ref int titleLevel,
            ref int messageLevel,
            double totalHours, 
            double redThreshold, 
            double yellowThreshold, 
            string? limitLabel, 
            string? extensionStatus, 
            bool isExempt)
        {
            if (totalHours >= redThreshold)
            {
                message = isExempt ? "-" : string.Format(OvertimeLimitMessage, limitLabel, extensionStatus);
                messageLevel = LevelDanger;
                titleLevel = LevelPrimary;

            }
            else if (totalHours >= yellowThreshold)
            {
                message = isExempt ? "-" : string.Format(OvertimeLimitMessage, limitLabel, extensionStatus);
                messageLevel = LevelWarn;
                titleLevel = LevelPrimary;
            }
        }

        /// <summary>
        /// 勤務表表示行の構築
        /// </summary>
        /// <param name="nippouYoteis">日報予定リスト</param>
        /// <param name="nippous">日報実績リスト</param>
        /// <param name="workingHours">打刻情報リスト</param>
        /// <param name="applications">申請リスト</param>
        /// <param name="plannedPaidLeaves">有給計画明細リスト</param>
        /// <param name="furikyuuZans">振休残リスト</param>
        /// <param name="hikadoubis">非稼働日リスト</param>
        /// <param name="today">当日日付</param>
        /// <param name="syain">対象社員</param>
        /// <returns>勤務表表示行リスト</returns>
        public List<KarendaHyojiRowViewModel> BuildKarendaHyojiRows(
            List<NippouYotei> nippouYoteis,
            List<Nippou> nippous,
            List<WorkingHour> workingHours,
            List<UkagaiHeader> applications,
            List<YukyuKeikakuMeisai> plannedPaidLeaves,
            List<FurikyuuZan> furikyuuZans,
            List<Hikadoubi> hikadoubis,
            DateOnly today,
            Syain syain)
        {
            var hikadoubiDict = hikadoubis.ToDictionary(h => h.Ymd, h => h);

            return nippouYoteis.Select(nippouYotei =>
            {
                var (nippou, workingHour) = FindDailyRecords(nippouYotei.NippouYoteiYmd, nippous, workingHours);
                var workTimeInfo = GetWorkTimeDisplay(nippou, workingHour);
                var reportStatusInfo = GetReportStatusDisplay(nippou, workingHour);
                var applicationStatus = GetApplicationStatus(
                    nippouYotei.NippouYoteiYmd,
                    applications,
                    syain.IsInstructionApprover || syain.IsFinalInstructionApprover,
                    today);
                var plannedLabel = GetPlannedLabel(nippouYotei.NippouYoteiYmd, plannedPaidLeaves, furikyuuZans);

                hikadoubiDict.TryGetValue(nippouYotei.NippouYoteiYmd, out var hikadoubi);
                var lineClass = GetStyles(nippouYotei.NippouYoteiYmd, nippouYotei.Worked, hikadoubi);

                return new KarendaHyojiRowViewModel(
                    PlannedWork: nippouYotei.Worked,
                    PlannedOvertimeRaw: nippouYotei.ZangyouJikan,
                    PlannedLeave: plannedLabel,
                    ApplicationLabel: applicationStatus.Label,
                    ApplicationStatus: applicationStatus.Status,
                    SyukkinTime: workTimeInfo.SyukkinJikan,
                    ApplicationType: applicationStatus.InquiryType ?? string.Empty,
                    ShowApplyButton: applicationStatus.ShowApplyButton,
                    ShowTypeButton: applicationStatus.ShowTypeButton,
                    WorkTime: workTimeInfo.DisplayValue ?? string.Empty,
                    ActualWorked: workTimeInfo.JitsuDoJikan ?? string.Empty,
                    ReportStatus: reportStatusInfo.DisplayText ?? string.Empty,
                    LineClass: lineClass,
                    ReportCss: reportStatusInfo.StatusType ?? string.Empty,
                    IsLink: IsEditableMonth() && nippouYotei.NippouYoteiYmd >= today,
                    SyainId: syain.Id,
                    SyainBaseId: syain.SyainBaseId,
                    Date: nippouYotei.NippouYoteiYmd,
                    IsAfterSystemDay: nippouYotei.NippouYoteiYmd > today,
                    IsWorkTimeConfirmed: workTimeInfo.IsConfirmed,
                    HasProxyInput: reportStatusInfo.HasProxyInput,
                    ReportStatusType: reportStatusInfo.StatusType ?? string.Empty
                );
            }).ToList();
        }

        /// <summary>
        /// 日付別の表示スタイルを取得
        /// </summary>
        /// <param name="d">対象日付</param>
        /// <param name="plannedWork">出勤予定フラグ</param>
        /// <param name="hd">非稼働日情報</param>
        /// <param name="today">当日日付</param>
        /// <returns>行クラスと日付クラスのタプル</returns>
        private string GetStyles(
            DateOnly date, 
            bool plannedWork, 
            Hikadoubi? hikadoubi)
        {
            // 祝祭日判定を最優先
            if ((hikadoubi?.SyukusaijitsuFlag ?? それ以外) == 祝祭日)
            {
                return StyleLineClasses.Holiday;
            }

            // 背景色判定
            if (!plannedWork)
            {
                return StyleLineClasses.Sunday;
            }

            return date.DayOfWeek switch
            {
                DayOfWeek.Saturday => StyleLineClasses.Saturday,
                DayOfWeek.Sunday => StyleLineClasses.Sunday,
                _ => StyleLineClasses.Weekday,
            };
        }

        /// <summary>
        /// 勤務時間表示情報の取得
        /// </summary>
        /// <param name="nippou">日報実績情報</param>
        /// <param name="workingHour">打刻情報</param>
        /// <returns>勤務時間表示情報</returns>
        private WorkTimeDisplayInfo GetWorkTimeDisplay(Nippou? nippou, WorkingHour? workingHour)
        {
            bool isConfirmed = nippou != null && nippou.TourokuKubun == 確定保存;

            if (isConfirmed)
            {
                // 登録確定した日報実績情報が存在する日は黒字でラベル表示
                var timeRanges = new[] {
                    FormatRange(nippou!.SyukkinHm1, nippou.TaisyutsuHm1),
                    FormatRange(nippou.SyukkinHm2, nippou.TaisyutsuHm2),
                    FormatRange(nippou.SyukkinHm3, nippou.TaisyutsuHm3)
                }.Where(r => r != null).ToList();

                var displayValue = timeRanges.Any() ? string.Join("\r\n", timeRanges) : "―";
                var jitsuDoJikan = GetActualWorkedDisplay(nippou);
                return new WorkTimeDisplayInfo(displayValue, true, nippou.SyukkinHm1, jitsuDoJikan);
            }
            else if (workingHour != null)
            {
                var displayValue = FormatRange(
                    workingHour.SyukkinTime?.ToTimeOnly(),
                    workingHour.TaikinTime?.ToTimeOnly(),
                    false) ?? "―";
                return new WorkTimeDisplayInfo(displayValue, false, null, null);
            }
            else
            {
                return new WorkTimeDisplayInfo(null, false, null, null);
            }
        }

        /// <summary>
        /// 時間範囲を文字列形式(HH:mm～HH:mm)に整形します
        /// </summary>
        private string? FormatRange(TimeOnly? start, TimeOnly? end, bool requireBoth = true)
        {
            if (start.HasValue && end.HasValue)
                return $"{start.ToStrByHHmmOrEmpty()}～{end.ToStrByHHmmOrEmpty()}";
            if (!requireBoth && start.HasValue)
                return $"{start.ToStrByHHmmOrEmpty()}～";
            return null;
        }

        /// <summary>
        /// 実働時間表示文字列の取得
        /// </summary>
        /// <param name="nippou">日報実績情報</param>
        /// <returns>実働時間 (hh:mm 形式)</returns>
        private string GetActualWorkedDisplay(Nippou? nippou)
        {
            if (nippou == null)
                return string.Empty;

            // 実働時間を取得（平日、土祝祭日、法定休日の合計）
            var totalJitsudou = 0m;

            if (nippou.HJitsudou.HasValue)
            {
                totalJitsudou += nippou.HJitsudou.Value;
            }
            if (nippou.DJitsudou.HasValue)
            {
                totalJitsudou += nippou.DJitsudou.Value;
            }
            if (nippou.NJitsudou.HasValue)
            {
                totalJitsudou += nippou.NJitsudou.Value;
            }

            if (totalJitsudou > 0m)
            {
                int hours = (int)totalJitsudou;
                int minutes = (int)((totalJitsudou - hours) * 60);
                var time = new TimeSpan(hours, minutes, 0);
                return time.HHmmColon();
            }

            return string.Empty;
        }

        /// <summary>
        /// 報告状況表示情報の取得
        /// </summary>
        /// <param name="nippou">日報実績情報</param>
        /// <param name="workingHour">打刻情報</param>
        /// <returns>報告状況表示情報</returns>
        private ReportStatusDisplayInfo GetReportStatusDisplay(Nippou? nippou, WorkingHour? workingHour)
        {

            if (nippou == null)
            {
                if (workingHour != null)
                {
                    // 日報実績が未入力の場合、赤字の「未」をリンク表示
                    return new ReportStatusDisplayInfo("未", true, false, "未入力");
                }

                return new ReportStatusDisplayInfo(null, false, false, "");
            }

            // 代理入力チェック（無効フラグがfalseのレコードが存在するか）
            bool hasProxyInput = nippou.DairiNyuryokuRirekis != null &&
                                nippou.DairiNyuryokuRirekis.Count > 0;

            // 確定済みチェック
            if (nippou?.TourokuKubun == 確定保存)
            {
                // 表示する勤務表のユーザーが日報実績を入力して登録確定されていた場合、「済」のリンク表示
                return new ReportStatusDisplayInfo("済", true, hasProxyInput, "確定済");
            }
            else
            {
                return new ReportStatusDisplayInfo("一時", true, false, "一時保存");
            }
        }

        /// <summary>
        /// 指定日の日報実績と打刻情報を取得します
        /// </summary>
        private (Nippou? nippou, WorkingHour? workingHour) FindDailyRecords(
            DateOnly date, 
            List<Nippou> nippous, 
            List<WorkingHour> workingHours)
        {
            var nippou = nippous.FirstOrDefault(n => n.NippouYmd == date);
            var workingHour = workingHours.FirstOrDefault(w => w.Hiduke == date);
            return (nippou, workingHour);
        }

        /// <summary>
        /// 申請状況情報の取得
        /// </summary>
        /// <param name="date">対象日付</param>
        /// <param name="ukagaiHeaders">申請リスト</param>
        /// <param name="canApplyAfterCutoff">締め日後申請可能フラグ</param>
        /// <returns>申請状況情報</returns>
        private ApplicationStatusInfo GetApplicationStatus(
            DateOnly date,
            List<UkagaiHeader> ukagaiHeaders,
            bool canApplyAfterCutoff,
            DateOnly today)
        {
            var cutoff = today.AddDays(ApplicationCutoffDays);

            bool isAfterCutoff = canApplyAfterCutoff || date >= cutoff;

            // ===== 対象日の申請 =====
            var dayUkagaiHeaders = ukagaiHeaders?
                .Where(a => a.WorkYmd == date)
                .ToList();

            string? type = null;
            ApprovalStatus? status = null;

            if (dayUkagaiHeaders != null && dayUkagaiHeaders.Any())
            {
                // 承認済み
                var approvedUkagaiHeader = dayUkagaiHeaders.FirstOrDefault(a => a.LastShoninYmd.HasValue);
                if (approvedUkagaiHeader != null)
                {
                    type = MapInquiryTypeToLabel(approvedUkagaiHeader.UkagaiShinseis?.FirstOrDefault()?.UkagaiSyubetsu);
                    status = approvedUkagaiHeader.Status;
                }
                else
                {
                    // 承認待ち or 下書き
                    var ukagaiHeader = dayUkagaiHeaders.First();
                    type = MapInquiryTypeToLabel(ukagaiHeader.UkagaiShinseis?.FirstOrDefault()?.UkagaiSyubetsu);
                    status = ukagaiHeader.Status;
                }
            }

            bool showApplyButton = false;
            bool showTypeButton = false;

            if (!isAfterCutoff)
            {
                showApplyButton = false;
                showTypeButton = !string.IsNullOrEmpty(type);
            }
            else
            {
                showApplyButton = true;
                showTypeButton = !string.IsNullOrEmpty(type);
            }

            return new ApplicationStatusInfo(
                Label: ApplicationStatusLabel,
                Status: status,
                InquiryType: type,
                ShowApplyButton: showApplyButton,
                ShowTypeButton: showTypeButton
            );
        }


        // 休暇予定
        /// <summary>
        /// 計画休暇ラベルの取得
        /// </summary>
        /// <param name="nippouYmd">対象日付</param>
        /// <param name="yukyuKeikakuMeisais">有給計画明細リスト</param>
        /// <param name="furikyuuZans">振休残リスト</param>
        /// <returns>休暇ラベル文字列</returns>
        private string GetPlannedLabel(
            DateOnly nippouYmd, 
            List<YukyuKeikakuMeisai> yukyuKeikakuMeisais, 
            List<FurikyuuZan> furikyuuZans)
        {
            var furiKyuuYoteiSet = furikyuuZans
                .Where(furikyuuZan => furikyuuZan.SyutokuYoteiYmd.HasValue)
                .Select(furikyuuZan => furikyuuZan.SyutokuYoteiYmd!.Value)
                .ToHashSet();

            var plannedPaidLeaveSet = yukyuKeikakuMeisais
                .Where(yukyuKeikakuMeisai => yukyuKeikakuMeisai.Ymd == nippouYmd)
                .Select(yukyuKeikakuMeisai => yukyuKeikakuMeisai.IsTokukyu)
                .ToHashSet();


            if (furikyuuZans != null && furiKyuuYoteiSet.Contains(nippouYmd))
            {
                return "振休"; // 振替休暇予定
            }

            if (plannedPaidLeaveSet != null && plannedPaidLeaveSet.Contains(false))
            {
                return "有給"; // 計画有給休暇
            }

            if (plannedPaidLeaveSet != null && plannedPaidLeaveSet.Contains(true))
            {
                return "特休"; // 計画特別休暇
            }

            return string.Empty;
        }

        /// <summary>
        /// 伺い種別ラベルのマッピング
        /// </summary>
        /// <param name="t">伺い種別</param>
        /// <returns>表示ラベル文字列</returns>
        private string MapInquiryTypeToLabel(InquiryType? inquiryType)
        {
            if (!inquiryType.HasValue)
                return string.Empty;
            return inquiryType.Value switch
            {
                夜間作業 => "夜間",
                早朝作業 => "早朝",
                深夜作業 => "深夜",
                リフレッシュデー残業 => "リフ",
                休日出勤 => "休出",
                テレワーク => "テレ",
                打刻時間修正 => "打刻",
                _ => inquiryType.Value.ToString(),
            };
        }

        public sealed record WorkTimeDisplayInfo(
            string? DisplayValue, 
            bool IsConfirmed, 
            TimeOnly? SyukkinJikan, 
            string? JitsuDoJikan);

        public sealed record ReportStatusDisplayInfo(
            string? DisplayText, 
            bool IsLink, 
            bool HasProxyInput, 
            string? StatusType);

        public sealed record ApplicationStatusInfo(
            string Label,
            ApprovalStatus? Status,
            string? InquiryType,
            bool ShowApplyButton,
            bool ShowTypeButton
        );

        public List<EmployeeViewModel> DepartmentEmployees => 
            DepartmentEmployeesRaw.Select(d => new EmployeeViewModel(
            d.Id,
            d.Name,
            d.BusyoCode,
            d.SyainBaseId,
            d.Code)).ToList();

        public int TotalBreakTime => Time.休憩時間List.Sum(b => b.Item2 - b.Item1);

        /// <summary>ブレイク（休憩）時間（時間単位） - フロントで残業上限計算に使用</summary>
        public decimal BreakTimeHours => TotalBreakTime / 60m;
        /// <summary>勤務状況（残業・有給残・振休・連勤等）</summary>
        public List<KinmuJokyoRowViewModel> KinmuJokyoRows { get; set; } = new();
        /// <summary>勤務表表示（日付別 予定・申請・勤務時間・日報）</summary>
        public List<KarendaHyojiRowViewModel> KarendaHyojiRows { get; set; } = new();
    }

    /// <summary>
    /// 勤務状況 1 行 ViewModel（残業・有給残・振休・連勤等）
    /// </summary>
    public sealed record KinmuJokyoRowViewModel(
        string? Label, 
        string? Value, 
        string Description, 
        int TitleLevel,
        int MessageLevel);

    /// <summary>
    /// 勤務表表示 1 行 ViewModel（日付別 予定・申請・勤務時間・日報）
    /// </summary>
    public sealed record KarendaHyojiRowViewModel(
        bool PlannedWork,
        int PlannedOvertimeRaw,
        string? PlannedLeave,
        string ApplicationLabel,
        ApprovalStatus? ApplicationStatus,
        TimeOnly? SyukkinTime,
        string ApplicationType,
        bool ShowApplyButton,
        bool ShowTypeButton,
        string WorkTime,
        string ActualWorked,
        string ReportStatus,
        string LineClass = "",
        string ReportCss = "",
        bool IsLink = false,
        long SyainId = 0,
        long SyainBaseId = 0,
        DateOnly Date = default,
        bool IsAfterSystemDay = false,
        bool IsWorkTimeConfirmed = false,
        bool HasProxyInput = false,
        string ReportStatusType = "")
    {
        // Viewでの表示・判定用の短縮プロパティ
        public string DateIso => Date.ToString("yyyy-MM-dd");
        public int DayOfWeekValue => (int)Date.DayOfWeek;
        public string DateLabel => $"{Date.Day}({Date.DayOfWeek.ToJpShortString()})";
    }

    /// <summary>
    /// 勤務表の行スタイル（背景色・文字色）用クラス
    /// </summary>
    public static class StyleLineClasses
    {
        // 祝祭日
        public const string Holiday = "app-line--holiday";
        // 日曜日
        public const string Sunday = "app-line--sunday";
        // 土曜日
        public const string Saturday = "app-line--saturday";
        // 平日
        public const string Weekday = "app-line--weekday";
    }

    /// <summary>
    /// 部署社員 1 件 ViewModel
    /// </summary>
    public sealed record EmployeeViewModel(
        long Id, 
        string Name, 
        string BusyoCode, 
        long SyainBaseId, 
        string Code);
}
