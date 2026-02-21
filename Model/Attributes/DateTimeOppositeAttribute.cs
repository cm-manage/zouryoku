using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Model.Attributes
{
    public class DateTimeOppositeAttribute : ValidationAttribute
    {
        public string StartDatePropertyName { get; set; }
        public DateTimeOppositeAttribute(string startDatePropertyName)
        {
            StartDatePropertyName = startDatePropertyName;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // 開始日プロパティ取得
            var startDateProperty = validationContext.ObjectInstance.GetType().GetProperty(StartDatePropertyName);
            if (startDateProperty == null)
            {
                throw new ArgumentException(string.Format("対象パラメータ名が見つかりません Name:{0}", StartDatePropertyName));
            }

            // 開始日プロパティの値取得
            var startDateValue = startDateProperty.GetValue(validationContext.ObjectInstance, null);
            if (startDateValue == null)
            {
                // 開始日が指定されていない場合、検証不可のため検証を終える
                // 開始日、終了日の必須検証はそれぞれのRequiredで行うためSuccessを返す
                return ValidationResult.Success;
            }

            var begin = startDateValue as DateTime?;
            if (begin == null)
            {
                throw new ArgumentException(string.Format("日付型のプロパティを指定してください Type:{0}", startDateValue.GetType().FullName));
            }

            // 終了日
            if (value == null)
            {
                // 終了日の必須検証はRequiredで行うためSuccessを返す
                return ValidationResult.Success;
            }

            var end = value as DateTime?;
            if (end == null)
            {
                throw new ArgumentException(string.Format("日付型のプロパティを指定してください Type:{0}", value == null ? "NULL" : value.GetType().FullName));
            }
            if (!end.HasValue)
            {
                // 終了日の必須検証はRequiredで行うためSuccessを返す
                return ValidationResult.Success;
            }

            if (begin > end)
            {
                return new ValidationResult($"{validationContext.DisplayName}は{startDateProperty.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().Single().DisplayName}より未来の日付を設定してください。");
            }

            return ValidationResult.Success;
        }
    }
}
