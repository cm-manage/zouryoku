using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    public enum WorkTime : short
    {
        [Display(Name = "1日時間数")]
        HoursPerDay = 24,
        [Display(Name = "規定労働時間数")]
        RegularWorkHoursPerDay = 8,
    }
}
