using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 登録状況区分
    /// </summary>
    public enum DailyReportStatusClassification : short
    {
        [Display(Name = "一時保存")]
        一時保存 = 0,
        [Display(Name = "確定保存")]
        確定保存 = 1,
    }
}
