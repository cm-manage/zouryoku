using LanguageExt;
using System;
using System.Collections.Generic;
using static LanguageExt.Prelude;

namespace CommonLibrary.Extensions
{
    public static class IDictionaryExtensions
    {
        public static Option<B> Get<A, B>(this IDictionary<A, B> dic, A key)
        {
            if (dic.ContainsKey(key))
            {
                return Some(dic[key]);
            }
            return None;
        }
        public static B GetOrElse<A, B>(this IDictionary<A, B> dic, A key, B alternative) where B : notnull
        {
            return dic.Get(key).IfNone(alternative);
        }

        public static B GetOrElse<A, B>(this IDictionary<A, B> dic, A key, Func<B> alternative) where B : notnull
        {
            return dic.Get(key).IfNone(alternative);
        }

        public static B? GetOrNull<A, B>(this IDictionary<A, B> dic, A key) where B : class
        {
            if (dic.TryGetValue(key, out B? res))
            {
                return res;
            }
            return null;
        }

        public static B? GetOrNullReference<A, B>(this IDictionary<A, B> dic, A key) where B : struct
        {
            if (dic.TryGetValue(key, out B res))
            {
                return res;
            }
            return null;
        }

        public static B? GetOrNullReference<A, B>(this IDictionary<A, B?> dic, A key) where B : struct
        {
            if (dic.TryGetValue(key, out B? res))
            {
                return res;
            }
            return null;
        }
    }
}
