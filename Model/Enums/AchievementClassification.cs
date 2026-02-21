using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 実績確定締め日：区分
    /// </summary>
    public enum AchievementClassification : short
    {
        [Display(Name = "中締め")]
        中締め = 1,
        [Display(Name = "月末締め")]
        月末締め = 2,
        [Display(Name = "一か月締め")]
        一か月締め = 3,
    }
}
