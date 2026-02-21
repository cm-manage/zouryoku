using Microsoft.AspNetCore.Mvc.Rendering;

namespace Zouryoku.Utils
{
    /// <summary>
    /// アイコン付きセレクトボックス用SelectListItem
    /// </summary>
    public class SelectListItemWithIcon : SelectListItem
    {
        public string? Icon { get; set; }

        public SelectListItemWithIcon() : base() { }

        public SelectListItemWithIcon(string text, string value, string iconUnicode) : base()
        {
            Text = text;
            Value = value;
            Icon = char.ConvertFromUtf32(Convert.ToInt32(iconUnicode, 16));
        }
    }
}
