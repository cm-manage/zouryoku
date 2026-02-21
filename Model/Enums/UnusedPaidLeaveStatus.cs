using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 有給未取得アラート：状態
    /// </summary>
    public enum UnusedPaidLeaveStatus : int
    {
        [Display(Name = "通知")]
        通知 = 0,
        [Display(Name = "警告")]
        警告 = 1,
    }
}
