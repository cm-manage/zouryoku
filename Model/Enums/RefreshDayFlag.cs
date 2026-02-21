using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// リフレッシュデーフラグ
    /// </summary>
    public enum RefreshDayFlag : int
    {
        [Display(Name = "それ以外")]
        それ以外 = 0,
        [Display(Name = "リフレッシュデー")]
        リフレッシュデー = 1
    }
}
