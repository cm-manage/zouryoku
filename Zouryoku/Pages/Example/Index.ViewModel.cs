using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Model.Enums;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Pages.Shared.Components;
using ZouryokuCommonLibrary.Utils;

namespace Zouryoku.Pages.Example
{
    public partial class IndexModel
    {
        private const string DateFormat = "{0:MM/dd (ddd)}";
        private const string TimeFormat = "{0:hh:mm}";

        public record ViewModel
        {
            [Display(Name = "テキストボックス")]
            public string? TextBox { get; init; }

            [Display(Name = "ドロップダウン（asp-items版）")]
            public string DropdownList1 { get; init; } = "";

            public IReadOnlyList<SelectListItem> DropdownList1Items =>
            [
                new SelectListItem("〇〇　一郎", "0"),
                new SelectListItem("〇〇　二郎", "1"),
                new SelectListItem("〇〇　三郎", "2"),
            ];

            [Display(Name = "ドロップダウン（<option>版）")]
            public string DropdownList2 { get; init; } = "";


            [Display(Name = "ラジオボタン")]
            public string? RadioButton { get; init; }

            [Display(Name = "チェック1")]
            public bool CheckBox1 { get; init; }

            [Display(Name = "チェック2")]
            public bool CheckBox2 { get; init; }

            [Display(Name = "チェック3")]
            public bool CheckBox3 { get; init; }

            [Display(Name = "トグルスイッチ")]
            public bool ToggleSwitch { get; init; }

            [Display(Name = "日付入力")]
            public DateOnly Datepicker { get; init; } = DateTime.Today.ToDateOnly();

            [Display(Name = "日付入力（範囲）")]
            public DatepickerRangeModel.Values DatepickerRange { get; init; } = new() { SelectedOption = DatepickerRangeModel.Option.ThisMonth };

            public IReadOnlyList<DatepickerRangeModel.Option> DatepickerRangeOptions => DatepickerRangeModel.AllOptions;

            public IReadOnlyList<Day> Days { get; init; } = [];

            public ModalRegisterModel.ViewModel ModalRegister { get; init; } = new();

            public ModalSearchModel.ViewModel ModalSearch { get; init; } = new();

            [Display(Name = "部署")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public string Busyo { get; init; } = "サンプル部署";
        }

        /// <summary>
        /// 日報実績
        /// </summary>
        public class Day
        {
            [Display(Name = "担当者")]
            public required string Name { get; init; }

            [Display(Name = "日付")]
            [DisplayFormat(DataFormatString = DateFormat)]
            public required DateOnly Date { get; init; }

            public required string SyukkinKubun1 { get; init; }

            public required bool SyukkinKubun1IsHoliday { get; init; }

            public required IReadOnlyList<DayWorkingHour> WorkingHours { get; init; }

            public required IReadOnlyList<DayNippou> Nippous { get; init; }

            public required IReadOnlyList<DayPcLog> PcLogs { get; init; }

            public required InquiryType? UkagaiSyubetsu { get; init; }

            /// <summary>
            /// 土日祝で行全体の背景色を変更
            /// </summary>
            public string RowColorClass => Date.DayOfWeek switch
            {
                DayOfWeek.Sunday => "back-color-sunday",
                DayOfWeek.Saturday => "back-color-saturday",
                _ => SyukkinKubun1IsHoliday == true ? "back-color-holiday" : ""
            };
        }

        /// <summary>
        /// 勤怠打刻
        /// </summary>
        public class DayWorkingHour
        {
            [DisplayFormat(DataFormatString = TimeFormat)]
            public required DateTime? SyukkinTime { get; init; }

            [DisplayFormat(DataFormatString = TimeFormat)]
            public required DateTime? TaikinTime { get; init; }

            public required bool SyukkinHasLocation { get; init; }

            public required bool TaikinHasLocation { get; init; }

            public string SyukkinHref => SyukkinHasLocation ? "#" : "";

            public string TaikinHref => TaikinHasLocation ? "#" : "";
        }

        /// <summary>
        /// 日報実績
        /// </summary>
        public class DayNippou
        {
            [DisplayFormat(DataFormatString = TimeFormat)]
            public required TimeOnly? SyukkinHm { get; init; }

            [DisplayFormat(DataFormatString = TimeFormat)]
            public required TimeOnly? TaisyutsuHm { get; init; }

            public required bool SyukkinHasLocation { get; init; }

            public required bool TaisyutsuHasLocation { get; init; }

            public string SyukkinHref => SyukkinHasLocation ? "#" : "";

            public string TaisyutsuHref => TaisyutsuHasLocation ? "#" : "";
        }

        /// <summary>
        /// PCログ
        /// </summary>
        public class DayPcLog
        {
            public required string PcName { get; init; }

            [DisplayFormat(DataFormatString = TimeFormat)]
            public required DateTime? LogOnDateTime { get; init; }

            [DisplayFormat(DataFormatString = TimeFormat)]
            public required DateTime? LogOffDateTime { get; init; }
        }
    }
}
