namespace Zouryoku.Pages.Attendance.AttendanceList
{
    // 日報表示用クラス
    public class NippouData
    {
        /// <summary>
        /// 出勤区分
        /// </summary>
        public List<string> SyukkinKubunList { get; set; } = [];

        /// <summary>
        /// 出勤時間１
        /// </summary>
        public string Syukkin1 { get; set; } = string.Empty;

        /// <summary>
        /// 出勤時間２
        /// </summary>
        public string Syukkin2 { get; set; } = string.Empty;

        /// <summary>
        /// 出勤時間３
        /// </summary>
        public string Syukkin3 { get; set; } = string.Empty;

        /// <summary>
        /// 退出時間１
        /// </summary>
        public string Taisyutsu1 { get; set; } = string.Empty;

        /// <summary>
        /// 退出時間２
        /// </summary>
        public string Taisyutsu2 { get; set; } = string.Empty;

        /// <summary>
        /// 退出時間３
        /// </summary>
        public string Taisyutsu3 { get; set; } = string.Empty;
    }
}
