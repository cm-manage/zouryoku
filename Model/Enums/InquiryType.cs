using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 伺い種別
    /// </summary>
    public enum InquiryType : short
    {
        // =1は未設定
        [Display(Name = "リフレッシュデー残業")]
        リフレッシュデー残業 = 2,
        [Display(Name = "休日出勤")]
        休日出勤 = 3,
        [Display(Name = "時間外労働時間制限拡張")]
        時間外労働時間制限拡張 = 4,
        [Display(Name = "夜間作業")]
        夜間作業 = 5,
        [Display(Name = "早朝作業")]
        早朝作業 = 6,
        [Display(Name = "深夜作業")]
        深夜作業 = 7,
        // =8は未設定
        [Display(Name = "休暇申請")]
        休暇申請 = 9,
        [Display(Name = "テレワーク")]
        テレワーク = 10,
        [Display(Name = "打刻時間修正")]
        打刻時間修正 = 11,
    }
}
