using System.ComponentModel.DataAnnotations;

namespace Zouryoku.Enums
{
    /// <summary>
    /// デバイス種別
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// PC端末
        /// </summary>
        [Display(Name = "PC")]
        PC,

        /// <summary>
        /// モバイル端末
        /// </summary>
        [Display(Name = "モバイル")]
        MOBILE,
    }
}
