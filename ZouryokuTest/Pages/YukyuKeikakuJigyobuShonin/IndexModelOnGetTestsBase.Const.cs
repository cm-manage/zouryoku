using Model.Enums;
using static Model.Enums.LeavePlanStatus;

namespace ZouryokuTest.Pages.YukyuKeikakuJigyobuShonin
{
    public partial class IndexModelOnGetTestsBase
    {
        /// <summary>
        /// SyainSet の各 Syain についての定数
        /// </summary>
        protected static class SyainSetConst
        {
            /// <summary>社員1の基準月を表します。</summary>
            public const int Syain1Month = 4;
            /// <summary>社員2の基準月を表します。</summary>
            public const int Syain2Month = 5;
            /// <summary>社員3の基準月を表します。</summary>
            public const int Syain3Month = 6;
            /// <summary>社員4の基準月を表します。</summary>
            public const int Syain4Month = 7;
            /// <summary>社員5の基準月を表します。</summary>
            public const int Syain5Month = 8;
            /// <summary>社員6の基準月を表します。</summary>
            public const int Syain6Month = 9;
            /// <summary>社員7（通常データ）の基準月を表します。</summary>
            public const int Syain7AMonth = 10;
            /// <summary>社員7（順序テスト用データ）の基準月を表します。</summary>
            public const int Syain7BMonth = 11;

            /// <summary>社員7（通常データ）の表示順序を表します。</summary>
            public const int Syain7AJyunjyo = 1;
            /// <summary>社員7（順序テスト用データ）の表示順序を表します。</summary>
            public const int Syain7BJyunjyo = 2;

            /// <summary>社員1のステータスを表します。</summary>
            public const LeavePlanStatus Syain1Status = 未申請;
            /// <summary>社員2のステータスを表します。</summary>
            public const LeavePlanStatus Syain2Status = 事業部承認待ち;
            /// <summary>社員3のステータスを表します。</summary>
            public const LeavePlanStatus Syain3Status = 未申請;
            /// <summary>社員4のステータスを表します。</summary>
            public const LeavePlanStatus Syain4Status = 人財承認待ち;
            /// <summary>社員5のステータスを表します。</summary>
            public const LeavePlanStatus Syain5Status = 事業部承認待ち;
            /// <summary>社員6のステータスを表します。</summary>
            public const LeavePlanStatus Syain6Status = 未申請;
            /// <summary>社員7（通常データ）のステータスを表します。</summary>
            public const LeavePlanStatus Syain7AStatus = 人財承認待ち;
            /// <summary>社員7（順序テスト用データ）のステータスを表します。</summary>
            public const LeavePlanStatus Syain7BStatus = 承認済;
        }
    }
}
