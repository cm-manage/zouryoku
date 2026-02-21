using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CommonLibrary.Utils
{
    public class EntityUtil
    {
        /// <summary>
        /// クラスにあるプロパティーのStringLength値を取得
        /// </summary>
        /// <typeparam name="A">対象クラス</typeparam>
        /// <param name="prop">対象プロパティ</param>
        /// <returns></returns>
        public static int? GetStringLength<A>(string prop) where A : class
        {
            var peroperty = typeof(A).GetProperty(prop);
            return peroperty == null
                ? null
                : GetStringLength<A>(peroperty);
        }

        /// <summary>
        /// クラスにあるプロパティーのStringLength値を取得
        /// </summary>
        /// <typeparam name="A">対象クラス</typeparam>
        /// <param name="prop">対象プロパティ</param>
        /// <returns></returns>
        public static int? GetStringLength<A>(PropertyInfo prop) where A : class
        {
            var stringLengthAttribute = Attribute.GetCustomAttribute(prop, typeof(StringLengthAttribute)) as StringLengthAttribute;
            return stringLengthAttribute?.MaximumLength;
        }
    }
}
