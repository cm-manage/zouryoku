using System;

namespace CommonLibrary.Extensions
{
    public static class IntExtensions
    {
        /// <summary>
        /// 年月または年月日のintから年を抽出します。
        /// yyyyMMまたはyyyyMMdd形式の整数を想定しています。それ以外の場合はnullを返します。
        /// </summary>
        public static int? ToYear(this int yearMonth)
        {
            var digits = (int)Math.Log10(yearMonth) + 1;
            return digits switch
            {
                6 or 8 => yearMonth / (int)Math.Pow(10, digits - 4),
                _ => null
            };
        }
    }
}