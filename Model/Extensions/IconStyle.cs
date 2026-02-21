using System.ComponentModel.DataAnnotations;

namespace Model.Expand
{
    public enum IconStyle
    {
        [Display(Name = "fa-solid")]
        Solid,
        [Display(Name = "fa-regular")]
        Regular,
        [Display(Name = "fa-light")]
        Light,
        [Display(Name = "fa-thin")]
        Thin,
    }
}
