namespace Zouryoku.Pages.Shared.Components
{
    /// <summary>
    /// <para><see cref="Selector"/> で指定した入力欄にカレンダーボタンを追加する（jQueryカレンダー版）</para>
    /// <para>ボタンを追加する対象の &lt;input&gt; 要素は type="text" にしてください。</para>
    /// </summary>
    public class DatepickerModel
    {
        /// <summary>
        /// カレンダ―ボタンを追加する要素のidを指定してください。 (例: "start-ymd")
        /// </summary>
        public required string Id { get; init; }

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
        /// 土日祝日の選択を許可するかどうかを指定します。
        /// </summary>
        public bool AllowSelectWeekendAndHoliday { get; init; } = true;

        /// <summary>
        /// 展開直後の月を、現在の月からどれだけオフセットするかを指定します。 (例: 0 → 現在の月、-1 → 前の月、1 → 次の月)
        /// </summary>
        public int DefaultMonthOffset { get; init; }
    }
}
