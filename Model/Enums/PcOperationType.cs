using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    public enum PcOperationType
    {
        [Display(Name = "電源オン")]
        電源オン = 0,
        [Display(Name = "ログオン")]
        ログオン = 1,
        [Display(Name = "ログオフ")]
        ログオフ = 2,
        [Display(Name = "電源オフ")]
        電源オフ = 4
    }
}
