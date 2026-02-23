namespace Zouryoku.Utils
{
    public static class Const
    {
        public const string MissingVal = "***";
        public const string OverContent = "...";

        public static readonly List<string> AcceptExt = new List<string>() { ".gif", ".jpg", ".jpeg", ".png", ".bmp", ".svg" };

        public const string PasswordRegex = "^^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-z])(?=.*[!-/:-@[-`{-~])[0-9a-zA-Z!-/:-@[-`{-~]{10,}$";

        /// <summary>ファイルを選択してください。</summary>
        public const string PleaseSelectFile = "ファイルを選択してください。";
        /// <summary>CSVの列数が正しくありません。</summary>
        public const string MissingColumnCount = "CSVの列数が正しくありません。";
        /// <summary>取込データが存在しません。</summary>
        public const string EmptyReadData = "取込データが存在しません。";

        /// <summary> {0}の{1}情報を登録してください。 </summary>
        public const string ErrorRequiredSubItem = "{0}の{1}情報を登録してください。";
        /// <summary> 選択されたデータは既に存在しません。 </summary>
        public const string ErrorSelectedDataNotExists = "選択されたデータは既に存在しません。";
        /// <summary> ユーザー情報を取得できませんでした。 </summary>
        public const string ErrorUserNotFound = "ログインユーザー情報を取得できませんでした。";

        /// <summary> {0}は必須です。 </summary>
        public const string ErrorRequired = "{0}は必須です。";
        /// <summary> {0}を選択してください。 </summary>
        public const string ErrorSelectRequired = "{0}を選択してください。";
        /// <summary> {0}は{1}桁まで入力できます。 </summary>
        public const string ErrorLength = "{0}は{1}桁まで入力できます。";
        /// <summary> {0}(値：{1})は既に登録されています。 </summary>
        public const string ErrorUnique = "{0}(値：{1})は既に登録されています。";
        /// <summary> {0}({1})は既に登録されています。 </summary>
        public const string ErrorUniqueNotValue = "{0}({1})は既に登録されています。";
        /// <summary> {0}は日付（{1}）で入力してください。 </summary>
        public const string ErrorDateTime = "{0}は日付（{1}）で入力してください。";
        /// <summary> {0}(ID：{1})は存在しません。 </summary>
        public const string ErrorNotExists = "{0}(値：{1})は存在しません。";
        /// <summary> {0}は{1}のため登録できません。 </summary>
        public const string ErrorRegister = "{0}は{1}のため登録できません。";
        /// <summary> {0}は数値で入力してください。</summary>
        public const string ErrorNumber = "{0}は数値で入力してください。";
        /// <summary> {0}を正しく入力してください。</summary>
        public const string ErrorInvalidInput = "{0}を正しく入力してください。";
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
        /// <summary> {0}を設定してください。</summary>
        public const string ErrorSet = "{0}を設定してください。";
        /// <summary> {0}を入力してください。</summary>
        public const string ErrorInputRequired = "{0}を入力してください。";
        /// <summary> {0}が反対です。</summary>
        public const string ErrorReverse = "{0}が反対です。";
        /// <summary> 日報確定済みのため編集できません。</summary>
        public const string ErrorNippouLocked = "日報確定済みのため編集できません。";
        /// <summary> 特別休暇の2日にチェックを入れてください。</summary>
        public const string ErrorThereAreNotExactly2Tokukyus = "特別休暇の2日にチェックを入れてください。";
        /// <summary> 同一日が入力されています。</summary>
        public const string ErrorYmdDuplicate = "同一日が入力されています。";
        /// <summary> 対象を選択してください。</summary>
        public const string ErrorNotChecked = "対象を選択してください。";
        /// <summary> {0}が存在しません。</summary>
        public const string ErrorNonExistance = "{0}が存在しません。";
        /// <summary> 有効な{0}を入力してください。</summary>
        public const string ErrorNonValidInput = "有効な{0}を入力してください。";
        /// <summary> {0}と[1}の時間帯が重複しています。</summary>
        public const string ErrorOverlapInputTime = "{0}と{1}の時間帯が重複しています。";

        /// <summary> 検索結果が{0}件を超えたため、{0}件まで表示しています。</summary>
        public const string WarningTooManyResults = "検索結果が{0}件を超えたため、{0}件まで表示しています。";

        /// <summary> 夜間作業指示がされているため、承認されていれば日報確定時に0:00出勤とみなします</summary>
        public const string AttendanceNightWork = "夜間作業指示がされているため、承認されていれば日報確定時に0:00出勤とみなします";
        /// <summary> 深夜作業指示がされているため、承認されていれば日報確定時に24:00退勤とみなします</summary>
        public const string AttendanceLateNightWork = "深夜作業指示がされているため、承認されていれば日報確定時に24:00退勤とみなします";
        /// <summary> 未入力 </summary>
        public const string NotEntered = "未入力";

        /// <summary>
        /// 日報未確定通知メッセージの初期表示テキスト。
        /// </summary>
        /// <remarks>
        /// <list type="table">
        ///     <item>
        ///         <term><c>{0}</c></term>    
        ///         <description>対象の月</description>
        ///     </item>
        ///     <item>
        ///         <term><c>{1}</c></term>
        ///         <description>前半／後半／空文字</description>
        ///     </item>
        ///     <item>
        ///         <term><c>{2}</c></term>
        ///         <description>対象の開始日（始まり）</description>
        ///     </item>
        ///     <item>
        ///         <term><c>{3}</c></term>
        ///         <description>対象の締め日（終わり）</description>
        ///     </item>
        /// </list>
        /// </remarks>
        public const string NippouMikakuteiTsuchiMessage = """
            お疲れ様です。
            {0}月{1}分（{0}/{2}～{3}）の実績の一部が未確定ですので
            確定処理をお願い致します。
            """;
        /// <summary>
        /// 実績未確定の社員へチャットで通知を送信します。
        /// よろしいですか？
        /// </summary>
        public const string NippouMikakuteiTsuchiConfirmation = """
            実績未確定の社員へチャットで通知を送信します。
            よろしいですか？
            """;
        /// <summary>{0} {1}が{2}名に送信</summary>
        public const string NippouMikakuteiTsuchiSendHistoryStr = "{0} {1}が{2}名に送信";

        /// <summary> 勤務時間 午前開始時刻 8:30</summary>
        public static readonly TimeOnly BusinessHoursAmStart = new TimeOnly(8, 30);
        /// <summary> 勤務時間 午前終了時刻 12:00</summary>
        public static readonly TimeOnly BusinessHoursAmEnd = new TimeOnly(12, 0);
        /// <summary> 勤務時間 午後開始時刻 13:00</summary>
        public static readonly TimeOnly BusinessHoursPmStart = new TimeOnly(13, 0);
        /// <summary> 勤務時間 午後終了時刻 17:30</summary>
        public static readonly TimeOnly BusinessHoursPmEnd = new TimeOnly(17, 30);
        /// <summary> 最小日付 2001/01/01</summary>
        public static readonly DateOnly MinDate = new DateOnly(2001, 1, 1);
        /// <summary> 最大日付 2099/12/31</summary>
        public static readonly DateOnly MaxDate = new DateOnly(2099, 12, 31);
    }
}
