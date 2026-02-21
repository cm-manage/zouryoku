using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 出張職位
    /// </summary>
    public enum BusinessTripRole : short
    {
        [Display(Name = "2～6級")]
        _2_6級 = 6,
        [Display(Name = "7～8級")]
        _7_8級 = 17,
        [Display(Name = "執行役員")]
        執行役員 = 82,
    }
}
