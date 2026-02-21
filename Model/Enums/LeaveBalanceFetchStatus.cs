using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 振替休暇残：取得状況
    /// </summary>
    public enum LeaveBalanceFetchStatus : short
    {
        [Display(Name = "未")]
        未 = 0,
        [Display(Name = "半日")]
        半日 = 1,
        [Display(Name = "1日")]
        _1日 = 2,
    }
}
