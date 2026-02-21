using System.ComponentModel.DataAnnotations;

namespace Zouryoku.Pages.Example
{
    public partial class ModalSearchModel
    {
        public class ViewModel
        {
            [Display(Name = "名前")]
            public string Name { get; init; } = "";

            [Display(Name = "住所")]
            public string Address { get; init; } = "";

            [Display(Name = "電話番号")]
            public string Tel { get; init; } = "";
        }
    }
}
