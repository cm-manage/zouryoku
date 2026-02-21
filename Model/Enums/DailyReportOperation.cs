using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 日報実績操作
    /// </summary>
    public enum DailyReportOperation : short
    {
        [Display(Name = "確定")]
        確定 = 0,
        [Display(Name = "確定解除")]
        確定解除 = 1,
    }
}
