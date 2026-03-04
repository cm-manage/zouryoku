namespace Zouryoku.Utils
{
    public static class Const
    {
        public const string MissingVal = "***";
        public const string OverContent = "...";

        public static readonly List<string> AcceptExt = new List<string>() { ".gif", ".jpg", ".jpeg", ".png", ".bmp", ".svg" };

        public const string PasswordRegex = "^^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-z])(?=.*[!-/:-@[-`{-~])[0-9a-zA-Z!-/:-@[-`{-~]{10,}$";

        /// <summary>エラーが発生しました。</summary>
        public const string Error = "エラーが発生しました。";

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
        /// <summary> {0}は{1}～{2}の範囲で入力してください。</summary>
        public const string ErrorRange = "{0}は{1}～{2}の範囲で入力してください。";
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

        /// <summary> 社員ID該当なし </summary>
        public const string ErrorSyainNonExistance = "{0}:{1} の社員が見つかりません。";


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

        // 実績入力画面：入力チェックメッセージ
        public const string SelectAttendanceClassification = "出勤区分を選択してください。";
        public const string HolidayOnWeekdayError = "平日に『休日』を選択する事は出来ません。";
        public const string HolidayWorkOnWeekdayError = "平日に『休日出勤』を選択する事は出来ません。";
        public const string SelectHolidayOnWeekend = "休日は『休日』もしくは『休日出勤』を選択してください。";
        public const string InvalidAttendanceClassification = "出勤区分が不正です。";
        public const string EnterSubstituteHolidayDate = "振替休暇予定日を入力してください。";
        public const string CannotSelectHolidayWithClockIn = "出退勤の打刻があるため、休日を選択する事は出来ません。";
        public const string SelectAnnualPaidLeaveOneDay = "「年次有給休暇(1日)」を選択してください。";
        public const string SelectHalfDayWorkDueToShortHours = "実働時間が4時間以下ですので『半日勤務』を選択してください。";
        public const string CannotUsePartTimeWork = "出勤区分『パート勤務』を使用する事は出来ません。";
        public const string SelectNormalWork = "『通常勤務』を選択してください。";
        public const string CannotTakePhysiologicalLeave = "生理休暇を取得することは出来ません。";
        public const string AnnualHalfDayPaidLeaveLimit = "半日有給休暇を取得できるのは年間{0}回までです。";
        public const string PaidLeaveInfoNotRegistered = "有給情報が登録されていません。";
        public const string CannotTakeSubstituteHoliday = "振替休暇を取得する事は出来ません。";
        public const string CannotTakeHalfDaySubstituteHoliday = "半日振休を取得する事は出来ません。";
        public const string TakeSubstituteHolidayFirst = "振替休暇から先に取得してください。";
        public const string CannotTakeHalfDayPaidLeave = "半日有給休暇を取得する事が出来ません。";
        public const string AbsenceWithSubstituteHolidayAvailable = "『欠勤』が選択されていますが{0}が取得可能です。";
        public const string CannotTakeAnnualPaidLeave = "有給休暇を取得する事は出来ません。";
        public const string PlannedAnnualPaidLeaveLimit = "計画有給休暇の年間取得回数は{0}回までです。";
        public const string PlannedSpecialLeaveLimit = "計画特別休暇の年間取得回数は{0}回までです。";
        public const string EnterWorkPerformance = "実績を入力してください。";
        public const string MaxFiveProjectCodes = "工番を5つ以上選択する事は出来ません。";
        public const string SelectProjectCode = "工番を選択してください。";
        public const string CreateProjectInfoForHolidayWork = "休日出勤の場合は工番情報を作成してください。";
        public const string CreateProjectInfoForNormalWork = "勤務の場合は工番情報を作成してください。";
        public const string HolidayWorkShortHoursProjectLimit = "休日出勤4時間以下の場合は工番を2つ以上選択する事はできません。";
        public const string CannotRegisterFutureWorkPerformance = "本日より未来の勤務日報を登録する事は出来ません。";
        public const string HalfDayWorkProjectLimit = "半日勤務時に工番を3つ以上選択する事は出来ません。";
        public const string CannotSelectProjectDuringLeave = "休暇時に工番を選択する事は出来ません。";
        public const string CannotConfirmSupportGroupOrder = "支援グループの受注番号では確定処理出来ません。";
        public const string SelectedProjectCodeCannotBeUsed = "選択された工番は使用出来ません。";
        public const string OvertimeLimitExceeded = "時間外労働時間の制限をこれ以上超過することは出来ません。";
        public const string OvertimeLimitUnapproved = "時間外労働時間の上限を超えており、指示入力が提出されていない、または、認められていません。";
        public const string NotWorkingCannotSelectFormat = "出勤していないため、{0}を選択することはできません。";
        public const string PaidLeaveDataNotFoundFormat = "有給情報が登録されていません。";
        public const string ErrorNippouAlreadyUnconfirmed = "この日報は解除されています。";
        public const string ErrorNippouKeiriRendouzumi = "この日報は経理連動が完了しているので、確定解除出来ません。";
        public const string ErrorCannotUnconfirmDueToLaterConfirmeData = "以降の確定日があるため確定解除出来ません。最終確定日から順に解除してください。";

        // 実績入力画面：確定確認メッセージ
        public const string ConfirmFixNippou = "確定してよろしいですか？";
        public const string ConfirmFixCautionNippou = "（本日を過ぎると確定を解除出来なくなります）";
        public const string ErrorOtherBusyoOrdeSelected = "他部署の受注番号が選択されています。";
        public const string JikanHoseiForRefreshDay = "リフレッシュデーの時間外労働申請が行われていないため、勤務時間を補正します。";
        public const string JitsudouJissekiMismatch = "勤務時間と実績の時間合計に差があります。";
    }
}
