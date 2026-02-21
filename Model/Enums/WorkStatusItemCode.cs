using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 労働状況集計：項目コード
    /// </summary>
    public enum WorkStatusItemCode : short
    {
        [Display(Name = "振替休暇期限切れ")]
        振替休暇期限切れ = 1,
        [Display(Name = "時間外労働(45H-79H)")]
        時間外労働_45H_79H = 2,
        [Display(Name = "時間外労働(80H-)")]
        時間外労働_80H = 3,
        [Display(Name = "連続勤務労働(13日以上)")]
        連続勤務労働_13日以上 = 4,
    }
}
