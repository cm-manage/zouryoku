using Model.Enums;
using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Model
{
    public partial class Busyo
    {
        /// <summary>
        /// Version
        /// </summary>
        [Timestamp]
        public uint Version { get; set; }
    }

    public partial class BusyoBasis
    {
        /// <summary>
        /// Version
        /// </summary>
        [Timestamp]
        public uint Version { get; set; }
    }

    public partial class PcLog
    {
        /// <summary>
        /// 操作種別
        /// </summary>
        [Column("operation")]
        public PcOperationType Operation { get; set; }
    }

    public partial class SyukkinKubun
    {
        /// <summary>
        /// 出勤区分コード
        /// Enum値変換用で、2桁整数の文字列（1桁数字は前0データで規定外データは扱わない前提です）
        /// </summary>
        [Column("code")]
        public string CodeString { get; set; } = "00";

        // C# 側で使う enum プロパティ（DBには保存されない）
        [NotMapped]
        public AttendanceClassification Code => (AttendanceClassification)int.Parse(CodeString);
    }

    public partial class Syain
    {
        /// <summary>
        /// 社員権限
        /// </summary>
        [Column("kengen")]
        public EmployeeAuthority Kengen { get; set; }

        /// <summary>
        /// PCログ出力
        /// </summary>
        public bool IsPcLogOutput => Kengen.HasFlag(EmployeeAuthority.PCログ出力);

        /// <summary>
        /// 出退勤一覧の打刻位置確認権限所持
        /// </summary>
        public bool IsCheckStampPosition => Kengen.HasFlag(EmployeeAuthority.出退勤一覧の打刻位置確認);

        /// <summary>
        /// 出退勤一覧の打刻時間修正権限所持
        /// </summary>
        public bool IsCorrectingTimeStamps => Kengen.HasFlag(EmployeeAuthority.出退勤一覧の打刻時間修正);

        /// <summary>
        /// 出退勤一覧画面の部署選択権限所持
        /// </summary>
        public bool IsSelectDepartment => Kengen.HasFlag(EmployeeAuthority.出退勤一覧画面の部署選択);

        /// <summary>
        /// 労働最終警告メール送信対象者権限所持
        /// </summary>
        public bool IsFinalLaborWarningEmailRecipients => Kengen.HasFlag(EmployeeAuthority.労働最終警告メール送信対象者);

        /// <summary>
        /// 労働状況報告権限所持
        /// </summary>
        public bool IsLaborStatusReport => Kengen.HasFlag(EmployeeAuthority.労働状況報告);

        /// <summary>
        /// 勤務日報未確定チェック権限所持
        /// </summary>
        public bool IsCheckPendingReports => Kengen.HasFlag(EmployeeAuthority.勤務日報未確定チェック);

        /// <summary>
        /// 勤務日報未確定者への通知権限所持
        /// </summary>
        public bool IsNotificationReportUnconfirmed => Kengen.HasFlag(EmployeeAuthority.勤務日報未確定者への通知);

        /// <summary>
        /// 勤怠データ出力権限所持
        /// </summary>
        public bool IsAttendanceDataOutput => Kengen.HasFlag(EmployeeAuthority.勤怠データ出力);

        /// <summary>
        /// 指示承認者権限所持
        /// </summary>
        public bool IsInstructionApprover => Kengen.HasFlag(EmployeeAuthority.指示承認者);

        /// <summary>
        /// 指示最終承認者権限所持
        /// </summary>
        public bool IsFinalInstructionApprover => Kengen.HasFlag(EmployeeAuthority.指示最終承認者);

        /// <summary>
        /// 管理機能利用_その他権限所持
        /// </summary>
        public bool IsManagementFunctionsOther => Kengen.HasFlag(EmployeeAuthority.管理機能利用_その他);

        /// <summary>
        /// 管理機能利用_人財向け権限所持
        /// </summary>
        public bool IsManagementFunctionsHumanResources => Kengen.HasFlag(EmployeeAuthority.管理機能利用_人財向け);

        /// <summary>
        /// 計画休暇承認権限所持
        /// </summary>
        public bool IsPlannedLeaveApproval => Kengen.HasFlag(EmployeeAuthority.計画休暇承認);

        /// <summary>
        /// 部門プロセス設定権限所持
        /// </summary>
        public bool IsDepartmentProcessSettings => Kengen.HasFlag(EmployeeAuthority.部門プロセス設定);

        /// <summary>
        /// 労働状況報告の部署選択権限所持
        /// </summary>
        public bool IsDepartmentSelectForLaborReport => Kengen.HasFlag(EmployeeAuthority.労働状況報告の部署選択);

        /// <summary>
        /// 有給・振替管理権限所持
        /// </summary>
        public bool IsPayrollAndTransferManagement => Kengen.HasFlag(EmployeeAuthority.有給_振替管理);

        /// <summary>
        /// 残業超過制限無効権限所持
        /// </summary>
        public bool IsOverTimeUnrestricted => Kengen.HasFlag(EmployeeAuthority.残業超過制限無効);

        /// <summary>
        /// 権限設定
        /// </summary>
        /// <param name="authority">設定する権限</param>
        public void SetKengen(EmployeeAuthority authority) => Kengen |= authority;

        /// <summary>
        /// 権限削除
        /// </summary>
        /// <param name="authority">削除する権限</param>
        public void DeleteKengen(EmployeeAuthority authority) => Kengen &= ~authority;

        /// <summary>
        /// 出張職位
        /// </summary>
        [Column("syucyo_syokui")]
        public BusinessTripRole SyucyoSyokui { get; set; }
    }

    public partial class UkagaiShinsei
    {
        /// <summary>
        /// 伺い種別
        /// </summary>
        [Column("ukagai_syubetsu")]
        public InquiryType UkagaiSyubetsu { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        [Timestamp]
        public uint Version { get; set; }
    }

    public partial class Nippou
    {
        /// <summary>
        /// 登録状況区分
        /// </summary>
        [Column("touroku_kubun")]
        public DailyReportStatusClassification TourokuKubun { get; set; }

        /// <summary>
        /// 会社コード
        /// </summary>
        [Column("kaisya_code")]
        public NippousCompanyCode KaisyaCode { get; set; }
    }

    public partial class WorkingHour
    {
        /// <summary>
        /// 出勤位置取得
        /// </summary>
        public string GetSyukkinPosition => GetPosition(SyukkinLatitude, SyukkinLongitude);

        /// <summary>
        /// 退勤位置取得
        /// </summary>
        public string GetTaikinPosition => GetPosition(TaikinLatitude, TaikinLongitude);

        /// <summary>
        /// 位置取得
        /// </summary>
        /// <param name="latitude">緯度</param>
        /// <param name="longitude">経度</param>
        private string GetPosition(decimal latitude, decimal longitude) =>
            (latitude != 0 && longitude != 0) ?
            $"{latitude:0.######},{longitude:0.######}" : "";
    }

    public partial class WorkStatusSummary
    {
        /// <summary>
        /// 項目コード
        /// </summary>
        [Column("item_code")]
        public WorkStatusItemCode ItemCode { get; set; }
    }

    public partial class DairiNyuryokuRireki
    {
        /// <summary>
        /// 日報実績操作
        /// </summary>
        [Column("nippou_sousa")]
        public DailyReportOperation NippouSousa { get; set; }
    }

    public partial class UkagaiHeader
    {
        /// <summary>
        /// ステータス
        /// </summary>
        [Column("status")]
        public ApprovalStatus Status { get; set; }
    }

    public partial class FurikyuuZan
    {
        /// <summary>
        /// 取得状況
        /// </summary>
        [Column("syutoku_state")]
        public LeaveBalanceFetchStatus SyutokuState { get; set; }
    }

    public partial class UnpaidLeaveAlert
    {
        /// <summary>
        /// 状態
        /// </summary>
        [Column("type")]
        public UnusedPaidLeaveStatus Type { get; set; }
    }

    public partial class OvertimeAlert
    {
        /// <summary>
        /// 区分
        /// </summary>
        [Column("kubun")]
        public OvertimeClassification Kubun { get; set; }
    }

    public partial class YukyuKeikaku
    {
        /// <summary>
        /// Version
        /// </summary>
        [Timestamp]
        public uint Version { get; set; }

        /// <summary>
        /// ステータス
        /// </summary>
        [Column("status")]
        public LeavePlanStatus Status { get; set; }
    }

    public partial class YukyuKeikakuMeisai
    {
        /// <summary>
        /// Version
        /// </summary>
        [Timestamp]
        public uint Version { get; set; }
    }

    public partial class ServiceExecuteHistory
    {
        /// <summary>
        /// 区分
        /// </summary>
        [Column("type")]
        public ServiceClassification Type { get; set; }

        /// <summary>
        /// ステータス
        /// </summary>
        [Column("status")]
        public ServiceStatus Status { get; set; }
    }

    public partial class ServiceExecute
    {
        /// <summary>
        /// 区分
        /// </summary>
        [Column("type")]
        public ServiceClassification Type { get; set; }
    }

    public partial class UserRole
    {
        /// <summary>
        /// 社員権限
        /// </summary>
        [Column("kengen")]
        public EmployeeAuthority Kengen { get; set; }
    }

    public partial class KintaiZokusei
    {
        /// <summary>
        /// コード
        /// </summary>
        [Column("code")]
        public EmployeeWorkType Code { get; set; }
    }

    public partial class ContinuousWork
    {
        /// <summary>
        /// 状態
        /// </summary>
        [Column("type")]
        public WorkStreakStatus Type { get; set; }
    }

    public partial class Hikadoubi
    {
        /// <summary>
        /// 祝祭日
        /// </summary>
        [Column("syukusaijitsu_flag")]
        public HolidayFlag SyukusaijitsuFlag { get; set; }

        /// <summary>
        /// リフレッシュデー
        /// </summary>
        [Column("refresh_day")]
        public RefreshDayFlag RefreshDay { get; set; }
    }

    public partial class Anken
    {
        /// <summary>
        /// Version
        /// </summary>
        [Timestamp]
        public uint Version { get; set; }
    }

    public partial class KingsJuchu
    {
        /// <summary>
        /// 受注工番
        /// </summary>
        public string KingsJuchuNo
            => string.Join(
                "-",
                ProjectNo,
                JuchuuNo ?? string.Empty,
                JuchuuGyoNo?.ToString() ?? string.Empty
                );

        /// <summary>
        /// 契約状態区分
        /// </summary>
        [Column("keiyaku_joutai_kbn")]
        public ContractClassification? KeiyakuJoutaiKbn { get; set; }
    }

    public partial class AnkenSansyouRireki
    {
        /// <summary>
        /// Version
        /// </summary>
        [Timestamp]
        public uint Version { get; set; }
    }

    public partial class KokyakuKaisyaSansyouRireki
    {
        /// <summary>
        /// Version
        /// </summary>
        [Timestamp]
        public uint Version { get; set; }
    }

    public partial class WorkingHour
    {
        /// <summary>
        /// Version
        /// </summary>
        [Timestamp]
        public uint Version { get; set; }
    }

    public partial class UkagaiHeader
    {
        /// <summary>
        /// Version
        /// </summary>
        [Timestamp]
        public uint Version { get; set; }
    }

    public partial class KingsJuchuSansyouRireki
    {
        /// <summary>
        /// Version
        /// </summary>
        [Timestamp]
        public uint Version { get; set; }
    }

    public partial class MessageContent
    {
        /// <summary>
        /// 機能区分
        /// </summary>
        [Column("function_type")]
        public FunctionalClassification FunctionType { get; set; }
    }
}
