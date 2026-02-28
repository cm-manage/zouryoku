using Model.Enums;

namespace Model.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 勤務日報時間入力での入力が必要な勤怠属性かどうか
        /// </summary>
        /// <param name="workType">勤怠属性</param>
        /// <returns>勤務日報時間入力での入力が必要な勤怠属性の場合はtrue、そうでない場合はfalse</returns>
        public static bool IsNippouTimeInput(this EmployeeWorkType workType)
        {
            return workType == EmployeeWorkType.フリー
                || workType == EmployeeWorkType.パート
                || workType == EmployeeWorkType.標準社員外;
        }

        /// <summary>
        /// 日報確定で指示なしで入力可能な勤怠属性かどうか
        /// </summary>
        /// <param name="workType">勤怠属性</param>
        /// <returns>日報確定で指示なしで入力可能な勤怠属性の場合はtrue、そうでない場合はfalse</returns>
        public static bool IsNippouInputUnlimited(this EmployeeWorkType workType)
        {
            return workType == EmployeeWorkType.フリー
                || workType == EmployeeWorkType.標準社員外;
        }
    }
}
