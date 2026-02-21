using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Utils;

namespace Zouryoku.Pages.Shared.Components
{
    /// <summary>
    /// <para>
    /// <see cref="SelectSelector"/>で指定したドロップダウンを変更時、
    /// 期間（<see cref="FromSelector"/>～<see cref="ToSelector"/>）を自動入力する機能を追加します。
    /// </para>
    /// <para>バリデーションは <code>$('#対象FromID')[0].datepickerRangeIsValid()</code> で行います。</para>
    /// </summary>
    public class DatepickerRangeModel
    {
        /// <summary>
        /// 開始日付欄のidを指定してください。 (例: "start-ymd") その項目の値がnullのとき期間ドロップダウンの初期値で初期化します。
        /// </summary>
        public required string FromId { get; init; }

        /// <summary>
        /// 終了日付欄のidを指定してください。 (例: "end-ymd") その項目の値がnullのとき期間ドロップダウンの初期値で初期化します。
        /// </summary>
        public required string ToId { get; init; }

        /// <summary>
        /// 期間ドロップダウンのidを指定してください。 (例: "range-ymd")
        /// </summary>
        public required string SelectId { get; init; }

        /// <summary>
        /// ピッカーの選択可能範囲（始端）です。
        /// </summary>
        public DateOnly? MinDate { get; init; }

        /// <summary>
        /// ピッカーの選択可能範囲（終端）です。
        /// </summary>
        public DateOnly? MaxDate { get; init; }

        /// <summary>
        /// 表示するカレンダーの数を変更します。（横の数）
        /// </summary>
        public int NumberOfMonthsWidth { get; init; } = 1;

        /// <summary>
        /// 表示するカレンダーの数を変更します。（縦の数）
        /// </summary>
        public int NumberOfMonthsHeight { get; init; } = 1;

        /// <summary>
        /// 期間ドロップダウンの全項目リストを取得します。
        /// </summary>
        public static IReadOnlyList<Option> AllOptions =>
        [
            Option.Yesterday,
            Option.Today,
            Option.ThisMonth,
            Option.Past2Months,
            Option.Past3Months,
            Option.PastHalfYear,
            Option.ThisFiscalYear
        ];

        private const string yesterdayName = "昨日";
        private const string todayName = "今日";
        private const string thisMonthName = "今月";
        private const string past2MonthsName = "過去2ヶ月";
        private const string past3MonthsName = "過去3ヶ月";
        private const string pastHalfYearName = "過去半年";
        private const string thisFiscalYearName = "今年度";

        public static IEnumerable<SelectListItem> ItemsFrom(IEnumerable<Option> options) => options.Select(o => o switch
        {
            Option.Yesterday => new SelectListItem(yesterdayName, o.ToString()),
            Option.Today => new SelectListItem(todayName, o.ToString()),
            Option.ThisMonth => new SelectListItem(thisMonthName, o.ToString()),
            Option.Past2Months => new SelectListItem(past2MonthsName, o.ToString()),
            Option.Past3Months => new SelectListItem(past3MonthsName, o.ToString()),
            Option.PastHalfYear => new SelectListItem(pastHalfYearName, o.ToString()),
            Option.ThisFiscalYear => new SelectListItem(thisFiscalYearName, o.ToString()),
            _ => new SelectListItem(),
        });

        public enum Option
        {
            /// <summary>(空白)</summary>
            [Display(Name = "")]
            None,

            /// <summary>昨日</summary>
            [Display(Name = yesterdayName)]
            Yesterday,

            /// <summary>今日</summary>
            [Display(Name = todayName)]
            Today,

            /// <summary>今月</summary>
            [Display(Name = thisMonthName)]
            ThisMonth,

            /// <summary>過去2ヶ月</summary>
            [Display(Name = past2MonthsName)]
            Past2Months,

            /// <summary>過去3ヶ月</summary>
            [Display(Name = past3MonthsName)]
            Past3Months,

            /// <summary>過去半年</summary>
            [Display(Name = pastHalfYearName)]
            PastHalfYear,

            /// <summary>今年度</summary>
            [Display(Name = thisFiscalYearName)]
            ThisFiscalYear
        }

        public class Values
        {
            [Display(Name = "開始日付")]
            public DateOnly From { get; init; } = Const.MinDate;

            [Display(Name = "終了日付")]
            public DateOnly To { get; init; } = Const.MaxDate;

            [Display(Name = "期間")]
            public Option SelectedOption { get; init; }
        }
    }
}
