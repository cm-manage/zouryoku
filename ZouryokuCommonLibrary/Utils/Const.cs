using Model.Enums;
using System;
using System.Collections.Generic;

namespace ZouryokuCommonLibrary.Utils
{
    public static class Const
    {
        public const string CancelSelectedKeyId = "cancel-selected-key";
        public const string IdListKeyId = "id-list-key";

        public static readonly List<string> AcceptExt = new List<string>() { ".gif", ".jpg", ".jpeg", ".png", ".bmp", ".svg" };

        // リダイレクトURL
        public const string UrlPage404 = "/page404";

        /// <summary> 日付の最小値（1900/01/01）</summary>
        public static readonly DateTime MinDate = new DateTime(1900, 1, 1);
        /// <summary> 日付の最大値（2100/01/01）</summary>
        public static readonly DateTime MaxDate = new DateTime(2100, 1, 1);

        /// <summary> 該当データがありません。 </summary>
        public const string NotFountData = "該当データがありません。";
        /// <summary> 再度ログインしてください。 </summary>
        public const string ReLogin = "再度ログインしてください。";
        /// <summary> 管理者に問い合わせてください。</summary>
        public const string ReportAdmin = "管理者に問い合わせてください。";
        /// <summary> 登録処理に失敗しました。 管理者に問い合わせてください。</summary>
        public const string RegisterMiss = "登録処理に失敗しました。" + ReportAdmin;
        /// <summary> 正常終了しました。</summary>
        public const string ProcessComplete = "正常終了しました。";

        /// <summary> {0}は必須です。 </summary>
        public const string ErrorRequired = "{0}は必須です。";
        /// <summary> {0}は{1}文字まで入力できます。 </summary>
        public const string ErrorMaxlength = "{0}は{1}文字まで入力できます。";
        /// <summary> {0}は{1}桁まで入力できます。 </summary>
        public const string ErrorMaxDigits = "{0}は{1}桁まで入力できます。";
        /// <summary> {0}は{1}桁まで入力できます。 </summary>
        public const string ErrorMinDigits = "{0}は{1}桁以上入力してください。";
        /// <summary> {0}(値：{1})は既に登録されています。 </summary>
        public const string ErrorUnique = "{0}(値：{1})は既に登録されています。";
        /// <summary> {0}(値：{1})にて{2}(値：{3})は既に登録されています。 </summary>
        public const string TargetErrorUnique = "{0}(値：{1})にて{2}(値：{3})は既に登録されています。";
        /// <summary> {0}({1})は既に登録されています。 </summary>
        public const string ErrorUniqueNotValue = "{0}({1})は既に登録されています。";
        /// <summary> {0}は日付（{1}）で入力してください。 </summary>
        public const string ErrorDateTime = "{0}は日付（{1}）で入力してください。";
        /// <summary> {0}(ID：{1})は存在しません。 </summary>
        public const string ErrorNotExists = "{0}(値：{1})は存在しません。";
        /// <summary> {0}は{1}のため登録できません。 </summary>
        public const string ErrorRegister = "{0}は{1}のため登録できません。";
        /// <summary> {0}は無効なアドレスのため登録できません。</summary>
        public const string ErrorInvalidAddress = "{0}は無効なアドレスのため登録できません。";
        /// <summary> {0}は数値で入力してください。</summary>
        public const string ErrorNumber = "{0}は数値で入力してください。";
        /// <summary> {0}は{1}以上の数値で入力してください。 </summary>
        public const string ErrorNumberRangeMoreThanEqual = "{0}は{1}以上の数値で入力してください。";
        /// <summary> {0}は{1}より大きい数値で入力してください。 </summary>
        public const string ErrorNumberRangeMoreThan = "{0}は{1}より大きい数値で入力してください。";
        /// <summary> {0}は{1}以下の数値で入力してください。 </summary>
        public const string ErrorNumberRangeLessThanEqual = "{0}は{1}以下の数値で入力してください。";
        /// <summary> {0}は{1}より小さい数値で入力してください。 </summary>
        public const string ErrorNumberRangeLessThan = "{0}は{1}より小さい数値で入力してください。";
        /// <summary> {0}は{1}よりも未来の日付を入力してください。</summary>
        public const string ErrorMoreThanDateTime = "{0}は{1}よりも未来の日付を入力してください。";
        /// <summary> {0}は{1}よりも過去の日付を入力してください。</summary>
        public const string ErrorLessThanDateTime = "{0}は{1}よりも過去の日付を入力してください。";
        /// <summary> {0}は{1}よりも過去の日付を入力してください。</summary>
        public const string ErrorOutOfRangeDateTime = "{0}は{1}から{2}までの間の日付を入力してください。";
        /// <summary> {0}は{1}よりも過去の時刻を入力してください。</summary>
        public const string ErrorOutOfRangeTime = "{0}は{1}から{2}までの間の時刻を入力してください。";
        /// <summary> {0}は{1}よりも過去の時刻を入力してください。</summary>
        public const string ErrorLessThanTime = "{0}は{1}よりも過去の時刻を入力してください。";
        /// <summary> {0}(ID：{1})は削除済みです。</summary>
        public const string ErrorNotFound = "{0}(値：{1})は削除済みです。";
        /// <summary> {0}は紐付く{1}が存在するため削除できません。</summary>
        public const string ErrorLinked = "{0}は紐付く{1}が存在するため削除できません。";
        /// <summary> 一覧を再検索してください。</summary>
        public const string Research = "一覧を再検索してください。";
        /// <summary> 再度開きなおしてください。</summary>
        public const string Reload = "再度開きなおしてください。";
        /// <summary> {0}は既に更新済みです。</summary>
        public const string ErrorConflict = "{0}は既に更新済みです。";
        /// <summary> {0}は既に更新済みです。一覧を再検索してください。</summary>
        public const string ErrorConflictResearch = ErrorConflict + Research;
        /// <summary> {0}は既に更新済みです。再度開きなおしてください。</summary>
        public const string ErrorConflictReload = ErrorConflict + Reload;
        /// <summary> {0}は正しく読み込めませんでした。</summary>
        public const string ErrorRead = "{0}は正しく読み込めませんでした。";
        /// <summary> {0}は正しく読み込めませんでした。一覧を再検索してください</summary>
        public const string ErrorReadResearch = ErrorRead + Research;
        /// <summary> {0}は正しく読み込めませんでした。再度開きなおしてください。</summary>
        public const string ErrorReadReload = ErrorRead + Reload;

    }
}
