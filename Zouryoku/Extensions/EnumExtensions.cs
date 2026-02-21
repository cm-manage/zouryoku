using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Zouryoku.Extensions
{
    public static class EnumExtensions
    {
        public static List<SelectListItem> SelectListItem<A>(this List<A> enums) where A : Enum
            => enums
                .Select(x => new SelectListItem()
                {
                    Value = Convert.ToInt32(x).ToString(),
                    Text = x.ToString(),
                })
                .ToList();

        public static List<SelectListItem> ToSelectListItems(this Type enumType)
        {
            var items = new List<SelectListItem>();
            foreach (var value in Enum.GetValues(enumType))
            {
                var field = enumType.GetField(value.ToString() ?? string.Empty);
                var displayAttribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
                    .FirstOrDefault() as DisplayAttribute;
                var name = displayAttribute?.Name ?? value.ToString();

                items.Add(new SelectListItem { Value = ((int)value).ToString(), Text = name });
            }
            return items;
        }
    }
}
