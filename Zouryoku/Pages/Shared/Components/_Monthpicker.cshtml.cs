namespace Zouryoku.Pages.Shared.Components
{
    /// <summary>
    /// <para><see cref="Selector"/> で指定した入力欄にカレンダーボタンを追加する（jQueryカレンダー版）</para>
    /// <para>ボタンを追加する対象の &lt;input&gt; 要素は type="text" にしてください。</para>
    /// </summary>
    public class MonthPickerModel
    {
        /// <summary>
        /// カレンダ―ボタンを追加する要素のidを指定してください。 (例: "start-ymd")
        /// </summary>
        public required string Id { get; init; }
    }
}
