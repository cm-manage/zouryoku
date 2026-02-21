using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 連続勤務：ステータス
    /// </summary>
    public enum WorkStreakStatus : int
    {
        [Display(Name = "通知")]
        通知 = 1,
        [Display(Name = "警告")]
        警告 = 2,
    }
}
