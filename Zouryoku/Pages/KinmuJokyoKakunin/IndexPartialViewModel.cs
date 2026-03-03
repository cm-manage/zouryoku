using System.ComponentModel.DataAnnotations;

namespace Zouryoku.Pages.KinmuJokyoKakunin
{
    /// <summary>
    /// 検索条件
    /// </summary>
    public class StatusSearchViewModel
    {
        /// <summary>
        /// 部署モードリスト
        /// </summary>
        public enum BusyoModeList
        {
            [Display(Name = "全社")]
            全社 = 1,
            [Display(Name = "部署選択")]
            部署選択 = 2,
        }

        public DateOnly DisplayYearMonthDate { get; set; }

        /// <summary>
        /// 期間(From)
        /// </summary>
        [Display(Name = "期間(From)")]
        public string From { get; set; } = string.Empty;

        /// <summary>
        /// 期間(To)
        /// </summary>
        [Display(Name = "期間(To)")]
        public string To { get; set; } = string.Empty;

        /// <summary>
        /// 警告レベル
        /// </summary>
        public WarnLevel WarnLevel { get; set; } = WarnLevel.All;

        /// <summary>
        /// 部署モード
        /// </summary>
        public BusyoModeList BusyoMode { get; set; } = BusyoModeList.全社;

        /// <summary>
        /// 部署IDリスト
        /// </summary>
        [Display(Name = "部署")]
        public string Busyo { get; set; } = "[]";
    }

    /// <summary>
    /// 定数値
    /// </summary>
    public static class KinmuJokyoConstants
    {
        public const int ZangyoMonth = 7;       // 残業 集計年度初月
        public const int YukyuMonth = 4;        // 有給 集計年度初月
    }

    /// <summary>
    /// 一覧
    /// </summary>
    public class TableViewModel
    {
        /// <summary>残業情報</summary>
        public List<WorkRowViewModel> WorkList { get; set; } = [];

        /// <summary>休暇情報</summary>
        public List<HolidayRowViewModel> HolidayList { get; set; } = [];

        /// <summary>
        /// 指定した警告レベルでフィルタリングされた行を返します
        /// </summary>
        public (List<WorkRowViewModel>, List<HolidayRowViewModel>) FilterByWarnLevel(WarnLevel warnLevel)
        {
            if (warnLevel == WarnLevel.All)
                return (WorkList, HolidayList);

            var workList = WorkList.Where(r =>
                warnLevel == WarnLevel.Warn ? r.IsWarn : r.IsNotice).ToList();
            var holidayList = HolidayList.Where(r =>
                warnLevel == WarnLevel.Warn ? r.IsWarn : r.IsNotice).ToList();

            return (workList, holidayList);
        }
    }

    /// <summary>
    /// 残業情報・休暇情報 共通
    /// </summary>
    public class CommonRowViewModel
    {
        /// <summary>部署</summary>
        public string BusyoName { get; set; } = string.Empty;

        /// <summary>社員名</summary>
        public string SyainName { get; set; } = string.Empty;

        /// <summary>勤怠属性</summary>
        public string ZokuseiName { get; set; } = string.Empty;

        /// <summary>年月</summary>
        public string YearMonth { get; set; } = string.Empty;

        /// <summary>実働</summary>
        public decimal Jitsudo { get; set; }
    }

    /// <summary>
    /// 残業情報
    /// </summary>
    public class WorkRowViewModel : CommonRowViewModel
    {
        /// <summary>残業(法休除く)</summary>
        public decimal ZangyoExceptHoliday { get; set; }

        /// <summary>残業</summary>
        public decimal Zangyo { get; set; }

        /// <summary>残業 警告レベル</summary>
        public string ZangyoWarnLevel { get; set; } = string.Empty;

        /// <summary>平均最大</summary>
        public decimal AverageMax { get; set; }

        /// <summary>平均最大 警告レベル</summary>
        public string AverageMaxWarnLevel { get; set; } = string.Empty;

        /// <summary>年間累計</summary>
        public decimal YearTotal { get; set; }

        /// <summary>年間累計 警告レベル</summary>
        public string YearTotalWarnLevel { get; set; } = string.Empty;

        /// <summary>制限超過回数</summary>
        public decimal? OverLimitCount { get; set; }

        /// <summary>制限超過回数 警告レベル</summary>
        public string OverLimitCountWarnLevel { get; set; } = string.Empty;

        /// <summary>最大連勤日数</summary>
        public string? MaxConsecutiveWorkingDays { get; set; }

        /// <summary>最大連勤日数 警告レベル</summary>
        public string MaxConsecutiveWorkingDaysWarnLevel { get; set; } = string.Empty;

        /// <summary>
        /// 警告レベルが設定されているかを確認します
        /// </summary>
        public bool HasWarning => !string.IsNullOrEmpty(AverageMaxWarnLevel) ||
                                   !string.IsNullOrEmpty(YearTotalWarnLevel) ||
                                   !string.IsNullOrEmpty(OverLimitCountWarnLevel) ||
                                   !string.IsNullOrEmpty(MaxConsecutiveWorkingDaysWarnLevel);

        /// <summary>
        /// 警告レベルが警告以上かを確認します
        /// </summary>
        public bool IsWarn => AverageMaxWarnLevel == "warn-level" ||
                              YearTotalWarnLevel == "warn-level" ||
                              OverLimitCountWarnLevel == "warn-level" ||
                              MaxConsecutiveWorkingDaysWarnLevel == "warn-level";

        /// <summary>
        /// 警告レベルが通知以上かを確認します
        /// </summary>
        public bool IsNotice => IsWarn ||
                                AverageMaxWarnLevel == "notice-level" ||
                                YearTotalWarnLevel == "notice-level" ||
                                OverLimitCountWarnLevel == "notice-level" ||
                                MaxConsecutiveWorkingDaysWarnLevel == "notice-level";

        /// <summary>
        /// ワーニング情報を設定します（表示用CSSクラスをViewModel側で決定）
        /// </summary>
        public void ApplyWarningLevels(AppSettings settings, decimal avgMax, decimal yearTotal, decimal? overLimitCount, decimal? maxConsecutiveNum)
        {
            AverageMaxWarnLevel = GetWarnLevelCssByZangyoValue(avgMax, settings.AvgMaxWarn, settings.AvgMaxNotice);
            YearTotalWarnLevel = GetWarnLevelCssByZangyoValue(yearTotal, settings.YearTotalZangyoExceptHolidayWarn, settings.YearTotalZangyoExceptHolidayNotice);
            OverLimitCountWarnLevel = GetWarnLevelCssByZangyoValue(overLimitCount, settings.OverLimitCountWarn, settings.OverLimitCountNotice);
            MaxConsecutiveWorkingDaysWarnLevel = GetWarnLevelCssByZangyoValue(maxConsecutiveNum, settings.MaxConsecutiveWorkingDaysWarn, settings.MaxConsecutiveWorkingDaysNotice);
        }

        private static string GetWarnLevelCssByZangyoValue(decimal? value, decimal warn, decimal notice)
        {
            WarnLevel level = WarnLevel.All;
            if (!value.HasValue)
            {
                level = WarnLevel.All;
            }
            else if (warn <= value)
            {
                level = WarnLevel.Warn;
            }
            else if (notice <= value)
            {
                level = WarnLevel.Notice;
            }

            return WarnLevelExtensions.ToCssClass(level);
        }
    }

    /// <summary>
    /// 休暇情報
    /// </summary>
    public class HolidayRowViewModel : CommonRowViewModel
    {
        /// <summary>有給休暇_年間累計</summary>
        public decimal PaidYearTotal { get; set; }

        /// <summary>年間累計 警告レベル</summary>
        public string PaidYearTotalWarnLevel { get; set; } = string.Empty;

        /// <summary>有給休暇_残日数</summary>
        public decimal PaidRemain { get; set; }

        /// <summary>有給休暇_うち半日残</summary>
        public decimal PaidHalfRemain { get; set; }

        /// <summary>特別休暇_取得</summary>
        public decimal? SpecialUsed { get; set; }

        /// <summary>振替休暇_残日数</summary>
        public decimal TransferRemain { get; set; }

        /// <summary>振替休暇_3ヶ月期限</summary>
        public decimal Transfer3Month { get; set; }

        /// <summary>振替休暇_失効日数</summary>
        public decimal TransferExpired { get; set; }

        /// <summary>
        /// 警告レベルが設定されているかを確認します
        /// </summary>
        public bool HasWarning => !string.IsNullOrEmpty(PaidYearTotalWarnLevel);

        /// <summary>
        /// 警告レベルが警告以上かを確認します
        /// </summary>
        public bool IsWarn => PaidYearTotalWarnLevel == "warn-level";

        /// <summary>
        /// 警告レベルが通知以上かを確認します
        /// </summary>
        public bool IsNotice => IsWarn || PaidYearTotalWarnLevel == "notice-level";

        /// <summary>
        /// 有給関連の警告レベルを設定します（表示用CSSクラスをViewModel側で決定）
        /// </summary>
        public void ApplyWarningLevel(AppSettings settings, decimal paidYearTotal, int month)
        {
            PaidYearTotalWarnLevel = GetWarnLevelCssByYukyuValue(paidYearTotal, month, settings);
        }

        private static string GetWarnLevelCssByYukyuValue(decimal value, int month, AppSettings settings)
        {
            bool firstTerm = month <= 1 || 12 <= month;
            bool secondTerm = 2 <= month && month <= 3;

            WarnLevel level = WarnLevel.All;
            if (firstTerm && value <= settings.PaidYearTotalWarn12To1
                || secondTerm && value <= settings.PaidYearTotalWarn2To3)
            {
                level = WarnLevel.Warn;
            }
            else if (firstTerm && value <= settings.PaidYearTotalNotice12To1
                || secondTerm && value <= settings.PaidYearTotalNotice2To3)
            {
                level = WarnLevel.Notice;
            }

            return WarnLevelExtensions.ToCssClass(level);
        }
    }

    internal static class WarnLevelExtensions
    {
        public static string ToCssClass(WarnLevel level)
        {
            return level switch
            {
                WarnLevel.Warn => "warn-level",
                WarnLevel.Notice => "notice-level",
                _ => ""
            };
        }
    }
}
