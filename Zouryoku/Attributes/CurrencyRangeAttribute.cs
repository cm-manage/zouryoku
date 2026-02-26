using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Zouryoku.Attributes
{
    /// <summary>
    /// 金額の範囲を検証する属性。
    /// カンマ区切りの数値形式を許可するため、文字列として扱い検証を行う。
    /// カンマなしの数値も許可する。
    /// 例: "1,234", "1234", "12,345,678"
    /// 内部的にはlong型で範囲チェックを行う。
    /// よって、long型の範囲外の数値はエラーとなる。
    /// 使用例:
    /// [CurrencyRange(1000, 1000000, ErrorMessage = "金額は1,000～1,000,000の範囲で入力してください。")]
    /// </summary>
    public class CurrencyRangeAttribute : ValidationAttribute, IClientModelValidator
    {
        // 最小値
        private readonly long _minimum;

        // 最大値
        private readonly long _maximum;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="minimum">最小値</param>
        /// <param name="maximum">最大値</param>
        public CurrencyRangeAttribute(long minimum, long maximum)
        {
            _minimum = minimum;
            _maximum = maximum;
        }

        /// <summary>
        /// <see cref="ValidationAttribute.IsValid(object?, ValidationContext)"/>
        /// 金額の範囲を検証する。
        /// カンマ区切りの数値形式を許可する。
        /// カンマなしの数値も許可。
        /// 例: "1,234", "1234", "12,345,678"
        /// </summary>
        /// <param name="value">検証対象の値</param>
        /// <param name="validationContext">検証コンテキスト</param>
        /// <returns>
        /// 検証結果。成功した場合は<see cref="ValidationResult.Success" />、失敗した場合はエラー情報を含む<see cref="ValidationResult"/>オブジェクトを返す。
        /// </returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // nullまたは空文字列は検証しない（他の属性で必須チェックを行う）
            if (value is not string s || string.IsNullOrEmpty(s))
                return ValidationResult.Success;

            // カンマ区切りの数値形式かどうかをチェック
            // カンマなしの数値も許可
            // 例: "1,234", "1234", "12,345,678"
            if (!Regex.IsMatch(s, @"^\d{1,3}(,\d{3})*$|^\d+$"))
                return CreateErrorResult(validationContext);

            // カンマを除去
            var normalized = s.Replace(",", "");

            // 数値に変換できなければエラー
            if (!long.TryParse(normalized, out var amount))
                return CreateErrorResult(validationContext);

            // 範囲内かどうかをチェック
            if (_minimum <= amount && amount <= _maximum)
                return ValidationResult.Success;

            // 範囲外の場合はエラー
            return CreateErrorResult(validationContext);
        }

        /// <summary>
        /// <see cref="ValidationAttribute.FormatErrorMessage(string)"/>
        /// エラーメッセージをフォーマットする。
        /// </summary>
        public override string FormatErrorMessage(string name)
        {
            // カスタムメッセージが設定されていればそれを使用、なければデフォルトメッセージを使用
            var message = ErrorMessage ?? "{0}は{1}～{2}の範囲で入力してください。";
            return string.Format(message, name, _minimum.ToString("N0"), _maximum.ToString("N0"));
        }

        /// <summary>
        /// <see cref="IClientModelValidator.AddValidation(ClientModelValidationContext)"/>
        /// クライアント側の検証ルールを追加する。
        /// クライアント側での検証を有効にする場合は、JavaScriptで対応する検証ロジックを実装する必要がある。
        /// </summary>
        /// <param name="context"></param>
        public void AddValidation(ClientModelValidationContext context)
        {
            // data-val="true"
            context.Attributes["data-val"] = "true";

            // data-val-currencyrange="エラーメッセージ"
            context.Attributes["data-val-currencyrange"] =
                FormatErrorMessage(context.ModelMetadata.GetDisplayName());

            // data-val-currencyrange-min="1000"
            context.Attributes["data-val-currencyrange-min"] = _minimum.ToString();

            // data-val-currencyrange-max="1000000"
            context.Attributes["data-val-currencyrange-max"] = _maximum.ToString();
        }

        // エラー結果を作成するヘルパーメソッド
        private ValidationResult CreateErrorResult(ValidationContext validationContext)
        {
            return new ValidationResult(
                    FormatErrorMessage(validationContext.DisplayName),
                    new[] { validationContext.MemberName ?? validationContext.DisplayName }
                );
        }

    }
}
