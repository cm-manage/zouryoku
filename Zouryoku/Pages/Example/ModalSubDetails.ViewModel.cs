using Model.Model;
using System.ComponentModel.DataAnnotations;

namespace Zouryoku.Pages.Example
{
    public partial class ModalSubDetailsModel
    {
        public class ViewModel
        {
            /// <summary>
            /// <see cref="Anken.KingsJuchu"/> の <see cref="KingsJuchu.ProjectNo"/>
            /// </summary>
            [Display(Name = "受注工番")]
            public required string JuchuuNo { get; init; }

            /// <summary>
            /// <see cref="Anken.KingsJuchu"/> の <see cref="KingsJuchu.Bukken"/>
            /// </summary>
            [Display(Name = "受注件名")]
            public required string JuchuuName { get; init; }

            /// <summary>
            /// <see cref="Anken.Name"/>
            /// </summary>
            [Display(Name = "案件名")]
            public required string AnkenName { get; init; }

            /// <summary>
            /// <see cref="Anken.JyutyuSyurui"/> の <see cref="JyutyuSyurui.Name"/>
            /// </summary>
            [Display(Name = "受注種類")]
            public required string JyutyuSyurui { get; init; }

            /// <summary>
            /// <see cref="Anken.KokyakuKaisya"/> の <see cref="KokyakuKaisha.Name"/>
            /// </summary>
            [Display(Name = "顧客情報")]
            public required string KokyakuKaisyaName { get; init; }

            /// <summary>
            /// <see cref="Anken.SyainBase"/> の <see cref="SyainBasis.Name"/>
            /// </summary>
            [Display(Name = "弊社責任者")]
            public required string SekininsyaName { get; init; }

            /// <summary>
            /// <see cref="Anken.Naiyou"/>
            /// </summary>
            [Display(Name = "案件内容")]
            public required string AnkenNaiyou { get; init; }
        }
    }
}
