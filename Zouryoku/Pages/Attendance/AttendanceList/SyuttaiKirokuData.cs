namespace Zouryoku.Pages.Attendance.AttendanceList
{
    // 出退記録表示用クラス
    public class SyuttaiKirokuData
    {
        /// <summary>
        /// 出勤時間１
        /// </summary>
        public string SyukkinJikan1 { get; set; } = string.Empty;

        /// <summary>
        /// 出勤時間２
        /// </summary>
        public string SyukkinJikan2 { get; set; } = string.Empty;

        /// <summary>
        /// 出勤時間３
        /// </summary>
        public string SyukkinJikan3 { get; set; } = string.Empty;

        /// <summary>
        /// 退勤時間１
        /// </summary>
        public string TaikinJikan1 { get; set; } = string.Empty;

        /// <summary>
        /// 退勤時間２
        /// </summary>
        public string TaikinJikan2 { get; set; } = string.Empty;

        /// <summary>
        /// 退勤時間３
        /// </summary>
        public string TaikinJikan3 { get; set; } = string.Empty;

        /// <summary>
        /// 出勤位置１
        /// </summary>
        public string SyukkinPos1 { get; set; } = string.Empty;

        /// <summary>
        /// 出勤位置２
        /// </summary>
        public string SyukkinPos2 { get; set; } = string.Empty;

        /// <summary>
        /// 出勤位置３
        /// </summary>
        public string SyukkinPos3 { get; set; } = string.Empty;

        /// <summary>
        /// 退勤位置１
        /// </summary>
        public string TaikinPos1 { get; set; } = string.Empty;

        /// <summary>
        /// 退勤位置２
        /// </summary>
        public string TaikinPos2 { get; set; } = string.Empty;

        /// <summary>
        /// 退勤位置３
        /// </summary>
        public string TaikinPos3 { get; set; } = string.Empty;

        /// <summary>
        /// 日またぎ出勤１
        /// </summary>
        public bool IsHimatagiSyukkin1 { get; set; } = false;

        /// <summary>
        /// 日またぎ退勤１
        /// </summary>
        public bool IsHimatagiTaikin1 { get; set; } = false;

        /// <summary>
        /// 日またぎ退勤２
        /// </summary>
        public bool IsHimatagiTaikin2 { get; set; } = false;

        /// <summary>
        /// 日またぎ退勤３
        /// </summary>
        public bool IsHimatagiTaikin3 { get; set; } = false;
    }
}
