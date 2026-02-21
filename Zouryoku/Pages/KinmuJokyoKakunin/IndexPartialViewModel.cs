using System.ComponentModel.DataAnnotations;
using Zouryoku.Utils;

namespace Zouryoku.Pages.KinmuJokyoKakunin
{
    /// <summary>
    /// 検索条件
    /// </summary>
    public class StatusSearchViewModel
    {
        /// <summary>
        /// 期間(From)
        /// </summary>
        [Display(Name = "期間(From)")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public string From { get; set; } = DateTime.Now.ToString("yyyy-MM");

        /// <summary>
        /// 期間(To)
        /// </summary>
        [Display(Name = "期間(To)")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public string To { get; set; } = DateTime.Now.ToString("yyyy-MM");

        /// <summary>
        /// 警告レベル
        /// </summary>
        public WarnLevel WarnLevel { get; set; } = WarnLevel.All;

        /// <summary>
        /// 部署プルダウン
        /// </summary>
        public string BusyoMode { get; set; } = "all";

        /// <summary>
        /// 部署IDリスト
        /// </summary>
        [Display(Name = "部署")]
        public string Busyo { get; set; } = "[]";
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
    }
}
