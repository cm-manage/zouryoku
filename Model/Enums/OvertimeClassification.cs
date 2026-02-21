using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 残業時間アラート：区分
    /// </summary>
    public enum OvertimeClassification : int
    {
        [Display(Name = "半月40時間")]
        半月40時間 = 1,
        [Display(Name = "半月45時間")]
        半月45時間 = 2,
        // =3は未設定
        // =4は未設定
        [Display(Name = "月50時間")]
        月50時間 = 5,
        [Display(Name = "月60時間")]
        月60時間 = 6,
        [Display(Name = "月70時間")]
        月70時間 = 7,
        [Display(Name = "月80時間")]
        月80時間 = 8,
    }
}
