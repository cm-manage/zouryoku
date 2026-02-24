using CommonLibrary.Extensions;
using Model.Data;
using static Model.Enums.AchievementClassification;
using static ZouryokuCommonLibrary.Utils.DateOnlyUtil;
using static Zouryoku.Utils.JissekiKakuteiSimeUtil;

namespace Zouryoku.Utils
{
    /// <summary>
    /// 未確定通知・チェック画面で使用するUtil。
    /// </summary>
    public static class MikakuteiTsuchiUtil
    {
        /// <summary>
        /// 実績期間と確定期限情報のラッパークラス。
        /// </summary>
        public class JissekiSpan
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            /// <remarks>
            /// 月をまたぐような実績期間を登録する場合は<see cref="JissekiSpan.JissekiSpan(DateOnly, DateOnly, JissekiKakuteiKigenInfo)"/>を使用すること。
            /// </remarks>
            /// <param name="year">実績期間の年</param>
            /// <param name="month">実績期間の月</param>
            /// <param name="startDay">実績期間開始日</param>
            /// <param name="endDay">実績締め日</param>
            /// <param name="jissekiKakuteiKigenInfo">確定期限・区分情報</param>
            public JissekiSpan(int year, int month, int startDay, int endDay, JissekiKakuteiKigenInfo jissekiKakuteiKigenInfo)
            {
                JissekiStartYmd = new DateOnly(year, month, startDay);
                JissekiSimebiYmd = new DateOnly(year, month, endDay);
                JissekiKakuteiKigenInfo = jissekiKakuteiKigenInfo;
            }

            /// <summary>
            /// コンストラクタ。
            /// </summary>
            /// <param name="jissekiStartYmd">実績開始年月日</param>
            /// <param name="jissekiSimebiYmd">実績締め年月日</param>
            /// <param name="jissekiKakuteiKigenInfo">確定期限・区分情報</param>
            public JissekiSpan(DateOnly jissekiStartYmd, DateOnly jissekiSimebiYmd, JissekiKakuteiKigenInfo jissekiKakuteiKigenInfo)
            {
                JissekiStartYmd = jissekiStartYmd;
                JissekiSimebiYmd = jissekiSimebiYmd;
                JissekiKakuteiKigenInfo = jissekiKakuteiKigenInfo;
            }

            /// <summary>
            /// 実績開始日の年月日。
            /// </summary>
            public DateOnly JissekiStartYmd { get; init; }
            /// <summary>
            /// 実績締め日の年月日
            /// </summary>
            public DateOnly JissekiSimebiYmd { get; init; }
            /// <summary>
            /// 実績確定期限情報。
            /// </summary>
            /// <value>確定期日と締め日区分</value>
            public JissekiKakuteiKigenInfo JissekiKakuteiKigenInfo { get; init; }
        };

        /// <summary>
        /// 基準日付が対象実績期間の通知可能期間に含まれるかどうかを判定する。
        /// </summary>
        /// <param name="db">DBコンテキスト</param>
        /// <param name="baseDate">基準日付</param>
        /// <param name="jissekiSpan">通知対象の実績期間</param>
        /// <returns><paramref name="baseDate"/>が通知可能期間に含まれていれば<c>true</c></returns>
        public static async Task<bool> IsInNotificationPeriodAsync(ZouContext db, DateOnly baseDate, JissekiSpan jissekiSpan)
        {
            // 実績締め日の翌日（通知可能期間の開始日）
            var startYmd = jissekiSpan.JissekiSimebiYmd.AddDays(1);
            // 確定期限の翌営業日（通知可能期間の終了日）
            var endYmd = await GetNextBusinessDayAsync(db, jissekiSpan.JissekiKakuteiKigenInfo.KakuteiKigenYmd);

            return (startYmd <= baseDate && baseDate <= endYmd);
        }

        /// <summary>
        /// 基準日付から見て通知対象となる実績期間を取得する。
        /// </summary>
        /// <param name="db">DBコンテキスト</param>
        /// <param name="baseDate">基準日付</param>
        /// <returns>通知対象の実績期間情報</returns>
        public static async Task<JissekiSpan> GetJissekiSpanAsync(ZouContext db, DateOnly baseDate)
        {
            // 基準月の中締め情報
            // nullなら通知対象の実績期間は月末締めor一か月締めになる
            var nakajimeInfo = (await GetKakuteiShimeKigenAsync(db, baseDate))
                .SingleOrDefault(k => k.Kubun == 中締め);

            // 基準月に中締めが存在して、基準日付が中締め日以前でないなら、基準月の中締めで確定する
            if (nakajimeInfo is not null && NakajimeDay < baseDate.Day)
            {
                return new JissekiSpan(
                    baseDate.Year,
                    baseDate.Month,
                    1,
                    NakajimeDay,
                    nakajimeInfo);
            }

            // 通知対象となる実績期間の締め日
            // 基準月の先月の月末
            var jissekiSimebi = baseDate.AddMonths(-1).GetEndOfMonth();
            // 基準月の先月の実績確定期限情報
            // 月末締めか一か月締めの情報を取得する
            var jissekiKakuteiKigenInfo = (await GetKakuteiShimeKigenAsync(db, jissekiSimebi))
                .Single(k => k.Kubun == 月末締め || k.Kubun == 一か月締め);

            return new JissekiSpan(
                jissekiSimebi.Year,
                jissekiSimebi.Month,
                jissekiKakuteiKigenInfo.Kubun == 一か月締め ? 1 : NakajimeDay + 1,
                jissekiSimebi.Day,
                jissekiKakuteiKigenInfo);
        }
    }
}
