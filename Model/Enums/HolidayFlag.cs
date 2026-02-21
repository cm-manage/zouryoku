using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 祝祭日フラグ
    /// </summary>
    public enum HolidayFlag : int
    {
        [Display(Name = "それ以外")]
        それ以外 = 0,
        [Display(Name = "祝祭日")]
        祝祭日 = 1
    }
}
