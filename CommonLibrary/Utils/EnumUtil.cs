using System;
using System.Collections.Generic;
using System.Linq;
using CommonLibrary.Extensions;
using LanguageExt;

namespace CommonLibrary.Utils
{
    public static partial class EnumUtil
    {
        public static List<A> List<A>() where A : Enum
        {
            return Enum.GetValues(typeof(A)).ToList<A>();
        }

        public static A? Parse<A>(string? name) where A : Enum
        {
            return List<A>().Find(a => a.ToString() == name);
        }

        public static Option<A> Cast<A>(int val) where A : Enum
        {
            return List<A>().FirstOption(a => Convert.ToInt32(a) == val);
        }

        public static Option<A> Cast<A>(string val) where A : Enum
        {
            return List<A>().FirstOption(a => (Convert.ToInt32(a)).ToString() == val);
        }
    }
}
