using System.ComponentModel.DataAnnotations;

namespace Zouryoku.Pages.Example
{
    public partial class ModalSubSearchModel
    {
        public class ViewModel
        {
            [Display(Name = "名前")]
            public string Name { get; init; } = "";

            [Display(Name = "住所")]
            public string Address { get; init; } = "";

            [Display(Name = "電話番号")]
            public string Tel { get; init; } = "";

            [Display(Name = "メモ")]
            public string Memo { get; init; } = "";
        }
    }
}
