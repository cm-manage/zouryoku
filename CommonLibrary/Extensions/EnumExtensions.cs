using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using CommonLibrary.Utils;

namespace CommonLibrary.Extensions
{
    public static class EnumExtensions
    {
        public static bool IsInclude<A>(this A kbn, A val) where A : Enum
        {
            return kbn.HasFlag(val);
        }

        public static string? GetDisplayName(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);

            var displayAttribute = type.GetField(name ?? "")?.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute == null ? name : displayAttribute?.Name;
        }
    }
}
