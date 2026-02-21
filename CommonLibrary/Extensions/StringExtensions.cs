using System;
using System.Globalization;
using System.Text.RegularExpressions;
using LanguageExt;
using static LanguageExt.Prelude;
using static CommonLibrary.Extensions.OptionExtensions;
using System.Linq;

namespace CommonLibrary.Extensions
{
    /// <summary>
    /// string型の拡張メソッドを管理するクラス
    /// </summary>
    public static class StringExtensions
    {
        public static string Format(this string str, params object[] args)
            => string.Format(str, args);

        public static string TexOverflow(this string str, int length, string text)
            => !string.IsNullOrEmpty(str) && (str.Length >= length)
                    ? str.Substring(0, 50) + text
                    : str;

        /// <summary>
        /// Option型の戻り値にNullを許容した取得ができる
        /// </summary>
        /// <param name="str"></param>
        /// <param name="val">設定されている場合、Null時に設定値をそうでない場合Nullを返す</param>
        /// <returns></returns>
        public static string? OptionGet(this string str, string? val = null)
            => OptionalT(str).IfNoneUnsafe(() => val);

        /// <summary>
        /// 長さが足りている場合は、カットして取得、足りていない場合はそのまま返す(使い勝手を考慮して、Substringsと引数順序が逆です。)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SubStringEx(this string str, int length, int startIndex = 0)
            => (str.Length >= (length + startIndex)) ? str.Substring(startIndex, length) : str;

        public static Option<DateTime> ToDateOption(this string str)
        {
            return parseDateTime(str);
        }

        public static Option<DateTime> ToDateOption(this string str, string format = "yyyyMMdd")
        {
            var result = new Option<DateTime>();
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (DateTime.TryParseExact(str, format, null, DateTimeStyles.None, out var resultDt))
                {
                    result = resultDt;
                }
            }
            return result;
        }

        public static DateTime ToDate(this string str)
        {
            return str.ToDateOption().First();
        }

        public static DateTime ToDate(this string str, string format = "yyyyMMdd")
        {
            return DateTime.ParseExact(str, format, null, DateTimeStyles.None);
        }

        public static DateOnly ToDateOnly(this string str)
        {
            return DateOnly.ParseExact(str, "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        public static DateTime ToDateTimeFromISO8601(this string isoString)
        {
            string format = "yyyy-MM-ddTHH:mm:sszzz";
            DateTimeOffset dto = DateTimeOffset.ParseExact(isoString, format, null);
            return dto.DateTime;
        }

        public static string PadLeft(this string str, int totalWidth, char paddingChar = ' ', bool isAllNull = false)
        {
            if (string.IsNullOrEmpty(str))
            {
                return isAllNull ? new string('\0', totalWidth) : new string(paddingChar, totalWidth);
            }

            return str.PadLeft(totalWidth, paddingChar);
        }

        public static string SubstringByte(this string str, int index, int count)
        {
            var sjis = System.Text.Encoding.GetEncoding("shift_jis");
            var bytes = sjis.GetBytes(str);
            return sjis.GetString(bytes, index, count);
        }

        public static string RemoveWhiteSpace(this string str)
            => Regex.Replace(str, @"\s", string.Empty);

        public static Option<int> ExtractNumericOpt(this string str)
            => parseInt(Regex.Replace(str, @"[^0-9]", ""))
                .IfNone(() => 0);

        public static int? ExtractNumeric(this string str)
            => str.ExtractNumericOpt()
            .Map(x => (int?)x)
            .IfNoneUnsafe(() => null);

        /// <summary>
        /// 先頭のみ小文字にする
        /// （例）ABCD => aBCD
        /// </summary>
        public static string ToTopLower(this string str)
            => string.IsNullOrEmpty(str)
                ? str
                : char.ToLower(str[0]) + str.Substring(1);

        /// <summary>
        /// 先頭のみ大文字にする
        /// （例）abcd => Abcd
        /// </summary>
        public static string ToTopUpper(this string str)
            => string.IsNullOrEmpty(str)
                ? str
                : char.ToUpper(str[0]) + str.Substring(1);

        /// <summary>
        /// Encoding
        /// </summary>
        public static string ToEncode(this string str, System.Text.Encoding fromEnc, System.Text.Encoding toEnc)
        {
            var fromBytes = fromEnc.GetBytes(str);
            var toBytes = System.Text.Encoding.Convert(fromEnc, toEnc, fromBytes);
            return toEnc.GetString(toBytes);
        }

        public static int? ToInt(this string str)
            => str.ToIntUnsafe();

        public static int? ToIntUnsafe(this string str)
            => parseInt(str).Map(x => (int?)x).IfNoneUnsafe(() => null);

        public static long? ToLong(this string str)
            => str.ToLongUnsafe();

        public static long? ToLongUnsafe(this string str)
            => parseLong(str).Map(x => (long?)x).IfNoneUnsafe(() => null);

        public static double? ToDouble(this string str)
            => str.ToDoubleUnsafe();

        public static double? ToDoubleUnsafe(this string str)
            => parseDouble(str).Map(x => (double?)x).IfNoneUnsafe(() => null);
    }
}
