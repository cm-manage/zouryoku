using CommonLibrary.Extensions;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Enums;
using static Model.Enums.AchievementClassification;
using static ZouryokuCommonLibrary.Utils.DateOnlyUtil;

namespace Zouryoku.Utils;

/// <summary>
/// 実績確定期限情報の取得結果
/// </summary>
/// <param name="JissekiKakuteiSimebiId">実績確定締め日ID</param>
/// <param name="KakuteiKigenYmd">確定期限</param>
/// <param name="Kubun">区分</param>
public record JissekiKakuteiKigenInfo(long JissekiKakuteiSimebiId, DateOnly KakuteiKigenYmd, AchievementClassification Kubun);

/// <summary>
/// 実績期間と確定期限情報をラッパーしたレコード。
/// </summary>
public record JissekiSpan(DateOnly JissekiStartYmd, DateOnly JissekiSimebiYmd, JissekiKakuteiKigenInfo JissekiKakuteiKigenInfo);

/// <summary>
/// 実績確定期限ユーティリティ
/// </summary>
public static class JissekiKakuteiSimeUtil
{
    /// <summary>
    /// 実績の中締め日。
    /// </summary>
    public const int NakajimeDay = 15;

    /// <summary>
    /// 実績確定期限情報を取得します。
    /// 対象とする実績年月の16日から翌月15日までの期間に確定期限を持つ実績確定締め日を検索し、
    /// 対象月の月末日を基準に中締め・月末締め・一か月締めの区分を判定して返却します。
    /// </summary>
    /// <param name="context">DBコンテキスト</param>
    /// <param name="targetMonth">
    /// 実績の対象年月を表す日付。
    /// 年月部分のみを使用し、「対象年月の16日〜翌月15日」の期間で確定期限を検索します。
    /// </param>
    /// <returns>
    /// 対象年月の16日から翌月15日までの確定期限を持つ実績確定締め日について、
    /// 対象月の月末日以下のものを「中締め」、それ以外を中締めの有無に応じて「月末締め」または「一か月締め」と判定した
    /// 実績確定期限情報リスト。
    /// </returns>
    public static async Task<List<JissekiKakuteiKigenInfo>> GetKakuteiShimeKigenAsync(ZouContext context, DateOnly targetMonth)
    {
        // 検索条件の設定
        // 検索開始日：年月：インプット.実績年月 日：16日
        var startDate = targetMonth.GetDateDesignatedDay(NakajimeDay + 1);

        // 検索終了日：年月：インプット.実績年月 + 1 日：15日
        var endDate = targetMonth.AddMonths(1).GetDateDesignatedDay(NakajimeDay);

        // 実績確定期限情報の取得
        var jissekiKakuteiSimebis = await context.JissekiKakuteiSimebis
            .AsNoTracking()
            .Where(s => startDate <= s.KakuteiKigenYmd && s.KakuteiKigenYmd <= endDate)
            .ToListAsync();

        // 区分判定用の月末日
        var endOfMonth = targetMonth.GetEndOfMonth();

        // 中締め（確定期限 ≦ インプット.実績年月の月末）が存在するか
        var hasNakajime = jissekiKakuteiSimebis.Any(s => s.KakuteiKigenYmd <= endOfMonth);

        return jissekiKakuteiSimebis.Select(s =>
        {
            AchievementClassification kubun;
            if (s.KakuteiKigenYmd <= endOfMonth)
            {
                // 確定期限 ≦ インプット.実績年月の月末の場合：中締め
                kubun = AchievementClassification.中締め;
            }
            else
            {
                if (hasNakajime)
                {
                    // 確定期限 > インプット.実績年月の月末 かつ 中締め有り：月末締め
                    kubun = AchievementClassification.月末締め;
                }
                else
                {
                    // 確定期限 > インプット.実績年月の月末 かつ 中締め無し：一か月締め
                    kubun = AchievementClassification.一か月締め;
                }
            }

            return new JissekiKakuteiKigenInfo(s.Id, s.KakuteiKigenYmd, kubun);
        }).ToList();
    }

    /// <summary>
    /// 基準日付が対象実績期間の通知可能期間に含まれるかどうかを判定する。
    /// </summary>
    /// <param name="db">DBコンテキスト</param>
    /// <param name="baseDate">基準日付</param>
    /// <param name="simebiYmd">対象実績期間の締め日</param>
    /// <param name="kakuteiKigenYmd">対象実績期間の確定期限</param>
    /// <returns><paramref name="baseDate"/>が通知可能期間に含まれていれば<c>true</c></returns>
    public static async Task<bool> IsInNotificationPeriodAsync(ZouContext db, DateOnly baseDate, DateOnly simebiYmd, DateOnly kakuteiKigenYmd)
    {
        // 実績締め日の翌日（通知可能期間の開始日）
        var startYmd = simebiYmd.AddDays(1);
        // 確定期限の翌営業日（通知可能期間の終了日）
        var endYmd = await GetNextBusinessDayAsync(db, kakuteiKigenYmd);

        return (startYmd <= baseDate && baseDate <= endYmd);
    }

    /// <summary>
    /// 基準日付から見て通知対象となる実績期間を取得する。
    /// </summary>
    /// <param name="db">DBコンテキスト</param>
    /// <param name="baseDate">基準日付</param>
    /// <returns>通知対象の実績期間情報</returns>
    /// <exception cref="InvalidOperationException">DB内の実績確定締め日データが不正な場合</exception>
    public static async Task<JissekiSpan> GetCanNotifyJissekiSpanAsync(ZouContext db, DateOnly baseDate)
    {
        try
        {
            // 基準月の中締め情報
            // nullなら通知対象の実績期間は月末締めor一か月締めになる
            var nakajimeInfo = (await GetKakuteiShimeKigenAsync(db, baseDate))
                .SingleOrDefault(k => k.Kubun == 中締め);
            DateOnly jissekiSimebi;

            // 基準月に中締めが存在して、基準日付が中締め日以前でないなら、基準月の中締めで確定する
            if (nakajimeInfo is not null && NakajimeDay < baseDate.Day)
            {
                jissekiSimebi = baseDate.GetDateDesignatedDay(NakajimeDay);
                return new JissekiSpan(jissekiSimebi.GetStartOfMonth(), jissekiSimebi, nakajimeInfo);
            }

            // 通知対象となる実績期間の締め日
            // 基準月の先月の月末
            jissekiSimebi = baseDate.AddMonths(-1).GetEndOfMonth();
            // 基準月の先月の実績確定期限情報
            // 月末締めか一か月締めの情報を取得する
            var jissekiKakuteiKigenInfo = (await GetKakuteiShimeKigenAsync(db, jissekiSimebi))
                .SingleOrDefault(k => k.Kubun == 月末締め || k.Kubun == 一か月締め);

            if (jissekiKakuteiKigenInfo is null)
            {
                throw new InvalidOperationException("実績確定締め日データが存在しません。");
            }

            return new JissekiSpan(
                jissekiSimebi.GetDateDesignatedDay(jissekiKakuteiKigenInfo.Kubun == 一か月締め ? 1 : NakajimeDay + 1),
                jissekiSimebi,
                jissekiKakuteiKigenInfo);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException("実績確定締め日データに不整合があります。", ex);
        }
    }
}