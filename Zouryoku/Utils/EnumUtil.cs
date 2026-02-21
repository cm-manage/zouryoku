using CommonLibrary.Extensions;
using LanguageExt;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Zouryoku.Utils
{
    public static partial class EnumUtil
    {
        public static List<SelectListItem> SelectListItems<A>(bool isDisplayName = false) where A : Enum
            => CommonLibrary.Utils.EnumUtil.List<A>().SelectListItems(isDisplayName);

        public static List<SelectListItem> SelectListItems<A>(this IEnumerable<A> entities, bool isDisplayName = false) where A : Enum
            => entities
            .Select(e => new SelectListItem
            {
                Value = (Convert.ToInt32(e)).ToString(),
                Text = isDisplayName ? e.GetDisplayName() : e.ToString(),
            })
            .ToList();

        public static string BitString(List<SelectListItem> enumBitList, int bitValue)
        {
            var bitString = new List<string>();
            enumBitList.OrderByDescending(x => int.Parse(x.Value))
                .ForEach(y =>
                {
                    if (bitValue >= int.Parse(y.Value))
                    {
                        bitValue -= int.Parse(y.Value);
                        bitString.Insert(0, y.Text);
                    }
                });
            return bitString.Aggregate((a, b) => a + "," + b);
        }
    }
}
