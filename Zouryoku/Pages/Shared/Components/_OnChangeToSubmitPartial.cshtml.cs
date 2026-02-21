namespace Zouryoku.Pages.Shared.Components
{
    /// <summary>
    /// <see cref="Selector"/>の値を変更後フォーカス解除したか、ピッカーで選択した際に'change2Submit'イベントを発行する。
    /// </summary>
    public class OnChangeToSubmitPartialModel(string selector) : PartialComponent
    {
        public string Selector { get; init; } = selector;
    }
}
