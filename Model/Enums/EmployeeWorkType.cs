using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 勤怠属性
    /// </summary>
    public enum EmployeeWorkType : short
    {
        [Display(Name = "みなし対象者")]
        みなし対象者 = 1,
        [Display(Name = "3か月60時間")]
        _3か月60時間 = 2,
        [Display(Name = "フリー")]
        フリー = 3,
        [Display(Name = "管理")]
        管理 = 4,
        [Display(Name = "標準社員外")]
        標準社員外 = 5,
        [Display(Name = "パート")]
        パート = 6,
        [Display(Name = "月45時間")]
        月45時間 = 7,
    }
}
