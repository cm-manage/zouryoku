using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 有休計画：ステータス
    /// </summary>
    public enum LeavePlanStatus : short
    {
        // =10は未設定
        [Display(Name = "未申請")]
        未申請 = 20,
        [Display(Name = "事業部承認待ち")]
        事業部承認待ち = 30,
        [Display(Name = "人財承認待ち")]
        人財承認待ち = 40,
        [Display(Name = "承認済")]
        承認済 = 50,
    }
}
