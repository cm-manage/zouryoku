using CommonLibrary.Extensions;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Enums;

namespace Zouryoku.Utils;

/// <summary>
/// 実績確定期限情報の取得結果
/// </summary>
/// <param name="JissekiKakuteiSimebiId">実績確定締め日ID</param>
/// <param name="KakuteiKigenYmd">確定期限</param>
/// <param name="Kubun">区分</param>
public record JissekiKakuteiKigenInfo(long JissekiKakuteiSimebiId, DateOnly KakuteiKigenYmd, AchievementClassification Kubun);

/// <summary>
/// 実績確定期限ユーティリティ
/// </summary>
public static class JissekiKakuteiSimeUtil
{
    /// <summary>
    /// 実績の中締め日。
    /// </summary>
    private const int NakajimeDay = 15;

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
}
