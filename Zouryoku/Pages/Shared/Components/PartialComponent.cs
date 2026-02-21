namespace Zouryoku.Pages.Shared.Components
{
    /// <summary>
    /// （コンポーネントにDataAnnotationsを追加することはできません）
    /// </summary>
    public abstract class PartialComponent
    {
        public string NewId()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", "").Replace("+", "").Replace("/", "")[..8];
        }
    }
}
