using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CommonLibrary.Extensions
{
    public static class IHtmlHelperExtensions
    {
        /// <summary>
        /// HtmlHelper標準のGetEnumSelectListメソッドでは、指定した列挙体の列挙子を全てSelectListeItemのリストにして返してくれるが、
        /// 使用する列挙子を限定できないため、絞り込みに対応したGetEnumSelectListを用意
        /// </summary>
        public static IEnumerable<SelectListItem> GetEnumSelectList<TEnum>(this IHtmlHelper htmlHelper, Func<TEnum, bool> predocate) where TEnum : Enum
        {
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

            foreach (var value in values.Where(predocate))
            {
                // Display属性が指定されていれば、そのNameをTextに使用
                var display = value.GetType()
                    .GetField(value.ToString() ?? string.Empty)?
                    .GetCustomAttributes(typeof(DisplayAttribute), false)
                    .FirstOrDefault() as DisplayAttribute;

                yield return new SelectListItem
                {
                    Text = display?.GetName() ?? value.ToString(),
                    Value = Convert.ToInt32(value).ToString(),
                };
            }
            ;
        }
    }
}
