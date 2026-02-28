using Model.Enums;
using System;

namespace Zouryoku
{
    /// <summary>
    /// Zouryokuプロジェクト固有のappsettings.json設定バインドクラス
    /// </summary>
    /// <remarks>
    /// ZouryokuCommonLibrary.AppConfigを継承していませんが、
    /// 同名のAppSettingsプロパティを持つことで、IOptions&lt;AppConfig&gt;として
    /// 共通ライブラリと実行プロジェクトの両方で使用可能になります。
    /// </remarks>
    public class AppConfig
    {
        // UnitTestでMoqがoverrideするためにvirtualが必要
        public virtual required AppSettings AppSettings { get; set; }
    }

    /// <summary>
    /// Zouryokuプロジェクト固有のアプリケーション設定
    /// </summary>
    /// <remarks>
    /// ZouryokuCommonLibrary.AppSettingsを継承し、共通設定に加えて
    /// Zouryokuプロジェクト固有の設定項目を追加しています。
    /// appsettings.jsonの設定値は、基底クラスと派生クラスの両方のプロパティにバインドされます。
    /// </remarks>
    public class AppSettings : ZouryokuCommonLibrary.AppSettings
    {
        /// <summary>Hogeしきい値（Zouryoku固有設定の例）</summary>
        public double HogeThreshold { get; set; }

        /// <summary>テンプレートフォルダのパス</summary>
        public string TemplatesFolderPath { get; set; } = string.Empty;

        /// <summary>勤務状況確認画面　出力ファイル名</summary>
        public string KinmuJokyoFileName { get; set; } = string.Empty;

        /// <summary>平均最大 警告の下限</summary>
        public decimal AvgMaxWarn { get; set; }

        /// <summary>平均最大 通知の下限</summary>
        public decimal AvgMaxNotice { get; set; }

        /// <summary>年間累計 警告の下限</summary>
        public decimal YearTotalZangyoExceptHolidayWarn { get; set; }

        /// <summary>年間累計 通知の下限</summary>
        public decimal YearTotalZangyoExceptHolidayNotice { get; set; }

        /// <summary>制限超過回数 警告の下限</summary>
        public decimal OverLimitCountWarn { get; set; }

        /// <summary>制限超過回数 通知の下限</summary>
        public decimal OverLimitCountNotice { get; set; }

        /// <summary>最大連勤日数 警告の下限</summary>
        public decimal MaxConsecutiveWorkingDaysWarn { get; set; }

        /// <summary>最大連勤日数 通知の下限</summary>
        public decimal MaxConsecutiveWorkingDaysNotice { get; set; }

        /// <summary>有給休暇年間累計 警告(12月～1月)の上限</summary>
        public decimal PaidYearTotalWarn12To1 { get; set; }

        /// <summary>有給休暇年間累計 通知(12月～1月)の上限</summary>
        public decimal PaidYearTotalNotice12To1 { get; set; }

        /// <summary>有給休暇年間累計 警告(2月～3月)の上限</summary>
        public decimal PaidYearTotalWarn2To3 { get; set; }

        /// <summary>有給休暇年間累計 通知(2月～3月)の上限</summary>
        public decimal PaidYearTotalNotice2To3 { get; set; }

        /// <summary>振替休暇残日数増加アラートの閾値となる日数</summary>
        public decimal FurikyuAlertThresholdDays { get; set; }

        /// <summary>計画有給休暇の取得上限日数</summary>
        public short MaxKeikakuYuukyuuDays { get; set; }

        /// <summary>計画特別休暇の取得上限数</summary>
        public short MaxKeikakuTokukyuuDays { get; set; }

        /// <summary>半日有給休暇の取得上限数</summary>
        public short MaxHannichiYuukyuuDays { get; set; }
    }
}
