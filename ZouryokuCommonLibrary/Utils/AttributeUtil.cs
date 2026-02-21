using System.ComponentModel.DataAnnotations.Schema;

namespace ZouryokuCommonLibrary.Utils
{
    public class AttributeUtil
    {
        /// <summary>
        /// TableAttribute の Name を取得
        /// </summary>
        /// <typeparam name="T">クラス</typeparam>
        public static string TableName<T>()
        {
            var attr = ((TableAttribute[])typeof(T).GetCustomAttributes(typeof(TableAttribute), false))[0];
            return attr.Name;
        }
    }
}
