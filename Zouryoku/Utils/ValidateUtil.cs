using System.Globalization;
using System.Text;
using CommonLibrary.Extensions;
using static LanguageExt.Prelude;

namespace Zouryoku.Utils
{
    public static class ValidateUtil
    {
        /// <summary>
        /// 必須かつ桁数チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="length">許容桁数</param>
        /// <returns></returns>
        public static string RequiredAndLength(string value, string name, int length, string? message = null)
        {
            var result = Required(value, name, message);
            if (string.IsNullOrWhiteSpace(result))
            {
                result = Length(value, name, length, message);
            }
            return result;
        }

        /// <summary>
        /// 必須かつバイト数チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="unicode">ユニコード</param>
        /// <param name="length">許容バイト￥数</param>
        /// <returns></returns>
        public static string RequiredAndByteLength(string value, string name, int length, string unicode = "Shift_JIS", string? message = null)
        {
            var result = Required(value, name, message);
            if (string.IsNullOrWhiteSpace(result))
            {
                result = ByteLength(value, name, length, unicode, message);
            }
            return result;
        }

        /// <summary>
        /// 必須かつ日付チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string RequiredAndToDate(string value, string name, string format = "yyyyMMdd", string? message = null)
        {
            var result = Required(value, name, message);
            if (string.IsNullOrWhiteSpace(result))
            {
                result = ToDate(value, name, format, message);
            }
            return result;
        }

        /// <summary>
        /// 必須かつ日付チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string RequiredAndToDateAndMinMaxRange(string value, string name, string format = "yyyyMMdd", string? message = null)
        {
            var result = Required(value, name, message);
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }

            result = ToDateAndMinMaxRange(value, name, format, message);
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }
            return result;
        }

        /// <summary>
        /// 日付チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string ToDateAndMinMaxRange(string value, string name, string format = "yyyyMMdd", string? message = null)
        {
            var result = string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                return result;
            }

            result = ToDate(value, name, format, message);
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }
            var date = value.ToDate();
            result = MoreThanEqual(date, ZouryokuCommonLibrary.Utils.Const.MinDate, name, ZouryokuCommonLibrary.Utils.Const.MinDate.ToString("yyyy/MM/dd"), message);
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }

            result = LessThanEqual(date, ZouryokuCommonLibrary.Utils.Const.MaxDate, name, ZouryokuCommonLibrary.Utils.Const.MaxDate.ToString("yyyy/MM/dd"), message);
            return result;
        }

        /// <summary>
        /// 必須かつ数値チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string RequiredAndToInt(string value, string name, string? message = null)
        {
            var result = Required(value, name, message);
            if (string.IsNullOrWhiteSpace(result))
            {
                result = ToInt(value, name, message);
            }
            return result;
        }

        /// <summary>
        /// 必須かつ数値チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string RequiredAndToLong(string value, string name, string? message = null)
        {
            var result = Required(value, name, message);
            if (string.IsNullOrWhiteSpace(result))
            {
                result = ToLong(value, name, message);
            }
            return result;
        }

        /// <summary>
        /// 必須かつ数値チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string RequiredAndToEnum<A>(string value, string name, string? message = null)
        {
            var result = Required(value, name, message);
            if (string.IsNullOrWhiteSpace(result))
            {
                result = ToEnum<A>(value, name, message);
            }
            return result;
        }


        /// <summary>
        /// 必須チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <returns></returns>
        public static bool IsRequired(string value)
            => !string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// 必須チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string Required(string value, string name, string? message = null)
            => IsRequired(value)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorRequired.Format(name) : message;


        /// <summary>
        /// 必須チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <returns></returns>
        public static bool IsRequired(DateTime? value)
            => value.HasValue && value.Value != DateTime.MinValue;

        /// <summary>
        /// 必須チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string Required(DateTime? value, string name, string? message = null)
            => IsRequired(value)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorRequired.Format(name) : message;

        /// <summary>
        /// 必須チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <returns></returns>
        public static bool IsRequired(int? value)
            => value.HasValue;

        /// <summary>
        /// 必須チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string Required(int? value, string name, string? message = null)
            => IsRequired(value)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorRequired.Format(name) : message;

        /// <summary>
        /// 必須チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <returns></returns>
        public static bool IsRequired(long? value)
            => value.HasValue;

        /// <summary>
        /// 必須チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string Required(long? value, string name, string? message = null)
            => IsRequired(value)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorRequired.Format(name) : message;

        /// <summary>
        /// 桁数チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="length">許容桁数</param>
        /// <returns></returns>
        public static bool IsLength(string value, int length)
            => !string.IsNullOrWhiteSpace(value) && value.Length <= length;

        /// <summary>
        /// 桁数チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="length">許容桁数</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string Length(string value, string name, int length, string? message = null)
            => IsLength(value, length)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorLength.Format(name, length) : message;

        /// <summary>
        /// 桁数チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="length">許容桁数</param>
        /// <returns></returns>
        public static bool IsLengthEqual(string value, int length)
            => !string.IsNullOrWhiteSpace(value) && value.Length == length;

        /// <summary>
        /// 桁数チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="length">許容桁数</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string LengthEqual(string value, string name, int length, string? message = null)
            => IsLengthEqual(value, length)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorLength.Format(name, length) : message;

        /// <summary>
        /// バイト数チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="length">許容バイト数</param>
        /// <param name="unicode">ユニコード</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static bool IsByteLength(string value, int length, string unicode = "Shift_JIS")
            => !string.IsNullOrWhiteSpace(value) && Encoding.GetEncoding(unicode).GetByteCount(value) <= length;

        /// <summary>
        /// バイト数チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="length">許容バイト数</param>
        /// <param name="unicode">ユニコード</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string ByteLength(string value, string name, int length, string unicode = "Shift_JIS", string? message = null)
            => IsByteLength(value, length, unicode)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorLength.Format(name, length) : message;

        /// <summary>
        /// バイト数チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="length">許容バイト数</param>
        /// <param name="unicode">ユニコード</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static bool IsByteLengthEqual(string value, int length, string unicode = "Shift_JIS")
            => !string.IsNullOrWhiteSpace(value) && Encoding.GetEncoding(unicode).GetByteCount(value) == length;

        /// <summary>
        /// バイト数チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="length">許容バイト数</param>
        /// <param name="unicode">ユニコード</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string ByteLengthEqual(string value, string name, int length, string unicode = "Shift_JIS", string? message = null)
            => IsByteLengthEqual(value, length, unicode)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorLength.Format(name, length) : message;

        /// <summary>
        /// 必須かつ日付チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string RequiredAndRangeMinMax(DateTime? value, string name, string? message = null)
        {
            var result = Required(value, name, message);
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }

            result = MoreThanEqual(value, ZouryokuCommonLibrary.Utils.Const.MinDate, name, ZouryokuCommonLibrary.Utils.Const.MinDate.ToString("yyyy/MM/dd"), message);
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }

            result = LessThanEqual(value, ZouryokuCommonLibrary.Utils.Const.MaxDate, name, ZouryokuCommonLibrary.Utils.Const.MaxDate.ToString("yyyy/MM/dd"), message);
            return result;
        }

        /// <summary>
        /// 日付用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="start">開始日付</param>
        /// <param name="end">終了日付</param>
        /// <remarks>「end <= start」はエラー</remarks>
        public static bool IsLessThan(DateTime start, DateTime end)
            => end > start;

        /// <summary>
        /// 日付用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="start">開始日付</param>
        /// <param name="end">終了日付</param>
        /// <param name="startName">開始日付名</param>
        /// <param name="endName">終了日付名</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「end <= start」はエラー</remarks>
        public static string LessThan(DateTime start, DateTime end, string startName, string endName, string? message = null)
            => IsLessThan(start, end)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? $"{startName}は{endName}より前の日付を入力してください。" : message;

        /// <summary>
        /// 日付用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="start">開始日付</param>
        /// <param name="end">終了日付</param>
        /// <remarks>「end < start」はエラー</remarks>
        public static bool IsLessThanEqual(DateTime? start, DateTime? end)
            => end >= start;

        /// <summary>
        /// 日付用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="start">開始日付</param>
        /// <param name="end">終了日付</param>
        /// <param name="startName">開始日付名</param>
        /// <param name="endName">終了日付名</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「end < start」はエラー</remarks>
        public static string LessThanEqual(DateTime? start, DateTime? end, string startName, string endName, string? message = null)
            => IsLessThanEqual(start, end)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? $"{startName}は{endName}以前の日付を入力してください。" : message;

        /// <summary>
        /// 日付用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="start">開始日付</param>
        /// <param name="end">終了日付</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「end >= start」はエラー</remarks>
        public static bool IsMoreThan(DateTime start, DateTime end)
            => end < start;

        /// <summary>
        /// 日付用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="start">開始日付</param>
        /// <param name="end">終了日付</param>
        /// <param name="startName">開始日付名</param>
        /// <param name="endName">終了日付名</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「end >= start」はエラー</remarks>
        public static string MoreThan(DateTime start, DateTime end, string startName, string endName, string? message = null)
            => IsMoreThan(start, end)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? $"{startName}は{endName}より後の日付を入力してください。" : message;

        /// <summary>
        /// 日付用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="start">開始日付</param>
        /// <param name="end">終了日付</param>
        /// <remarks>「end > start」はエラー</remarks>
        public static bool IsMoreThanEqual(DateTime? start, DateTime? end)
            => end <= start;

        /// <summary>
        /// 日付用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="start">開始日付</param>
        /// <param name="end">終了日付</param>
        /// <param name="startName">開始日付名</param>
        /// <param name="endName">終了日付名</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「end > start」はエラー</remarks>
        public static string MoreThanEqual(DateTime? start, DateTime? end, string startName, string endName, string? message = null)
            => IsMoreThanEqual(start, end)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? $"{startName}は{endName}以降の日付を入力してください。" : message;

        /// <summary>
        /// 数値用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <remarks>「range <= target」はエラー</remarks>
        public static bool IsLessThan(int target, int range)
            => range > target;

        /// <summary>
        /// 数値用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「range <= target」はエラー</remarks>
        public static string LessThan(int target, int range, string name, string? message = null)
            => IsLessThan(target, range)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumberRangeLessThan.Format(name, range) : message;

        /// <summary>
        /// 数値用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <remarks>「range < target」はエラー</remarks>
        public static bool IsLessThanEqual(int target, int range)
            => range >= target;

        /// <summary>
        /// 数値用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「range < target」はエラー</remarks>
        public static string LessThanEqual(int target, int range, string name, string? message = null)
            => IsLessThanEqual(target, range)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumberRangeLessThanEqual.Format(name, range) : message;

        /// <summary>
        /// 数値用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <remarks>「range >= target」はエラー</remarks>
        public static bool IsMoreThan(int target, int range)
            => range < target;

        /// <summary>
        /// 数値用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「range >= target」はエラー</remarks>
        public static string MoreThan(int target, int range, string name, string? message = null)
            => IsMoreThan(target, range)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumberRangeMoreThan.Format(name, range) : message;

        /// <summary>
        /// 数値用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <remarks>「range > target」はエラー</remarks>
        public static bool IsMoreThanEqual(int target, int range)
            => range <= target;

        /// <summary>
        /// 数値用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「range > target」はエラー</remarks>
        public static string MoreThanEqual(int target, int range, string name, string? message = null)
            => IsMoreThanEqual(target, range)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumberRangeMoreThanEqual.Format(name, range) : message;

        /// <summary>
        /// 数値用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <remarks>「range <= target」はエラー</remarks>
        public static bool IsLessThan(long target, long range)
            => range > target;

        /// <summary>
        /// 数値用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「range <= target」はエラー</remarks>
        public static string LessThan(long target, long range, string name, string? message = null)
            => IsLessThan(target, range)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumberRangeLessThan.Format(name, range) : message;

        /// <summary>
        /// 数値用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <remarks>「range < target」はエラー</remarks>
        public static bool IsLessThanEqual(long target, long range)
            => range >= target;

        /// <summary>
        /// 数値用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「range < target」はエラー</remarks>
        public static string LessThanEqual(long target, long range, string name, string? message = null)
            => IsLessThanEqual(target, range)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumberRangeLessThanEqual.Format(name, range) : message;

        /// <summary>
        /// 数値用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <remarks>「range >= target」はエラー</remarks>
        public static bool IsMoreThan(long target, long range)
            => range < target;

        /// <summary>
        /// 数値用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「range >= target」はエラー</remarks>
        public static string MoreThan(long target, long range, string name, string? message = null)
            => IsMoreThan(target, range)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumberRangeMoreThan.Format(name, range) : message;

        /// <summary>
        /// 数値用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「range > target」はエラー</remarks>
        public static bool IsMoreThanEqual(long target, long range)
            => range <= target;

        /// <summary>
        /// 数値用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「range > target」はエラー</remarks>
        public static string MoreThanEqual(long target, long range, string name, string? message = null)
            => IsMoreThanEqual(target, range)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumberRangeMoreThanEqual.Format(name, range) : message;

        /// <summary>
        /// 数値用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <remarks>「range <= target」はエラー</remarks>
        public static bool IsLessThan(decimal target, decimal range)
            => range > target;

        /// <summary>
        /// 数値用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「range <= target」はエラー</remarks>
        public static string LessThan(decimal target, decimal range, string name, string? message = null)
            => IsLessThan(target, range)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumberRangeLessThan.Format(name, range) : message;

        /// <summary>
        /// 数値用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <remarks>「range < target」はエラー</remarks>
        public static bool IsLessThanEqual(decimal target, decimal range)
            => range >= target;

        /// <summary>
        /// 数値用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「range < target」はエラー</remarks>
        public static string LessThanEqual(decimal target, decimal range, string name, string? message = null)
            => IsLessThanEqual(target, range)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumberRangeLessThanEqual.Format(name, range) : message;

        /// <summary>
        /// 数値用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <remarks>「range >= target」はエラー</remarks>
        public static bool IsMoreThan(decimal target, decimal range)
            => range < target;

        /// <summary>
        /// 数値用比較チェック（イコールはエラー）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「range >= target」はエラー</remarks>
        public static string MoreThan(decimal target, decimal range, string name, string? message = null)
            => IsMoreThan(target, range)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumberRangeMoreThan.Format(name, range) : message;

        /// <summary>
        /// 数値用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <remarks>「range > target」はエラー</remarks>
        public static bool IsMoreThanEqual(decimal target, decimal range)
            => range <= target;

        /// <summary>
        /// 数値用比較チェック（イコールは正常）
        /// </summary>
        /// <param name="target">対象</param>
        /// <param name="range">しきい値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <remarks>「range > target」はエラー</remarks>
        public static string MoreThanEqual(decimal target, decimal range, string name, string? message = null)
            => IsMoreThanEqual(target, range)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumberRangeMoreThanEqual.Format(name, range) : message;

        /// <summary>
        /// 日付変更チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <returns></returns>
        public static bool IsToDate(string value)
            => parseDateTime(value).IsSome;

        /// <summary>
        /// 日付変更チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <returns></returns>
        public static bool IsToDate(string value, string format = "yyyyMMdd")
            => !string.IsNullOrWhiteSpace(value) && DateTime.TryParseExact(value, format, null, DateTimeStyles.None, out var resultDt);

        /// <summary>
        /// 日付変更チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string ToDate(string value, string name, string format = "yyyyMMdd", string? message = null)
            => IsToDate(value, format)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorDateTime.Format(name, format) : message;

        /// <summary>
        /// 数値変更チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <returns></returns>
        public static bool IsToInt(string value)
            => parseInt(value).IsSome;

        /// <summary>
        /// 数値変更チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string ToInt(string value, string name, string? message = null)
            => IsToInt(value)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumber.Format(name) : message;

        /// <summary>
        /// 数値変更チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <returns></returns>
        public static bool IsToLong(string value)
            => parseLong(value).IsSome;

        /// <summary>
        /// 数値変更チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string ToLong(string value, string name, string? message = null)
            => IsToLong(value)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumber.Format(name) : message;

        /// <summary>
        /// 数値変更チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <returns></returns>
        public static bool IsToDecimal(string value)
            => parseDecimal(value).IsSome;

        /// <summary>
        /// 数値変更チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string ToDecimal(string value, string name, string? message = null)
            => IsToDecimal(value)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumber.Format(name) : message;

        /// <summary>
        /// 数値変更チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <returns></returns>
        public static bool IsToDouble(string value)
            => parseDouble(value).IsSome;

        /// <summary>
        /// 数値変更チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string ToDouble(string value, string name, string? message = null)
            => IsToDouble(value)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumber.Format(name) : message;

        /// <summary>
        /// 列挙型変更チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <returns></returns>
        public static bool IsToEnum<A>(string value)
            => Enum.TryParse(typeof(A), value, out var wd) && Enum.IsDefined(typeof(A), wd);

        /// <summary>
        /// 列挙型変更チェック
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="name">名前</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns></returns>
        public static string ToEnum<A>(string value, string name, string? message = null)
            => IsToEnum<A>(value)
                ? string.Empty
                : string.IsNullOrEmpty(message) ? Const.ErrorNumber.Format(name) : message;
    }
}
