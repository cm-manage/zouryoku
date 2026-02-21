using System;
using System.Collections.Generic;
using System.Linq;
using CommonLibrary.Extensions;
using LanguageExt;

namespace CommonLibrary.Utils
{
    public static class BitFlagUtil
    {
        /// <summary>
        /// ビットフラグ管理してる列挙型を分解して配列で返します
        /// </summary>
        /// <typeparam name="A">列挙型</typeparam>
        /// <param name="val">分解したい列挙型</param>
        /// <returns></returns>
        public static List<A> Enums<A>(A val) where A : Enum
            => EnumUtil.List<A>().Where(x => val.HasFlag(x)).ToList();
    }
}
