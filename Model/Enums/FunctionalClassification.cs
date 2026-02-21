using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 機能区分
    /// </summary>
    public enum FunctionalClassification : int
    {
        [Display(Name = "過労運転防止")]
        過労運転防止 = 1,
        [Display(Name = "有給未取得アラート")]
        有給未取得アラート = 2,
        [Display(Name = "連続勤務アラート")]
        連続勤務アラート = 3,
        [Display(Name = "未確定通知")]
        未確定通知 = 4,
    }
}
