using Microsoft.EntityFrameworkCore;
using Model.Data;
using static System.DayOfWeek;
using static Model.Enums.RefreshDayFlag;

namespace Zouryoku.Utils
{
    public static class DateOnlyUtil
    {
        /// <summary>
        /// ウィークエンドの配列
        /// </summary>
        private static readonly DayOfWeek[] Weekend = { Saturday, Sunday };

        /// <summary>
        /// 翌営業日を取得する。
        /// </summary>
        /// <param name="db">DBコンテキスト</param>
        /// <param name="baseDate">基準日付</param>
        /// <returns><paramref name="baseDate"/>の翌営業日</returns>
        public static async Task<DateOnly> GetNextBusinessDayAsync(ZouContext db, DateOnly baseDate)
        {
            // 基準日の翌日以降の非営業日リスト
            // リフレッシュデーでない非稼働日が該当する
            var hikadoubi = (await db.Hikadoubis
                .AsNoTracking()
                .Where(h => baseDate < h.Ymd
                    && h.RefreshDay != リフレッシュデー)
                .Select(h => h.Ymd)
                .ToListAsync())
                .ToHashSet();

            // 基準日付の翌営業日
            var result = baseDate;

            // 営業日かどうか
            while (true)
            {
                // 翌日を設定
                result = result.AddDays(1);

                // 設定された日付が営業日かどうかをチェック
                // 非営業日に含まれていない & ウィークエンドではない
                if (!hikadoubi.Contains(result) &&
                    !Weekend.Contains(result.DayOfWeek))
                {
                    return result;
                }
            }
        }
    }
}
