namespace Zouryoku.Pages.Attendance.AttendanceList
{
    // PCログ表示用クラス
    public class PcLogData
    {
        /// <summary>
        /// PC名
        /// </summary>
        public string PcName { get; set; } = string.Empty;

        /// <summary>
        /// 開始時刻
        /// </summary>
        public DateTime? LogonTime { get; set; }

        /// <summary>
        /// 終了時刻
        /// </summary>
        public DateTime? LogoffTime { get; set; }

        /// <summary>
        /// 開始時刻（文字列）
        /// </summary>
        public string StartTime => LogonTime != null ? LogonTime.Value.ToString("HH:mm") : "";

        /// <summary>
        /// 終了時刻（文字列）
        /// </summary>
        public string EndTime => LogoffTime != null ? LogoffTime.Value.ToString("HH:mm") : "";

    }
}
