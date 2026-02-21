using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Model;
using System.Linq.Expressions;

namespace Zouryoku.Utils
{
    /// <summary>
    /// 顧客会社参照履歴共通処理
    /// </summary>
    public static class KokyakuKaisyaSansyouRirekisUtil
    {
        /// <summary>
        /// 参照履歴テーブルに保持する最大件数
        /// </summary>
        public const int MaxHistoryCount = 50;

        /// <summary>
        /// 参照履歴の登録
        /// 
        /// 注意事項
        ///     この処理内ではコンテキストの保存を行わない。呼び出し元で保存処理を実行すること。
        ///     
        /// 処理概要
        ///     顧客情報を表示した時に呼び出す。
        ///     顧客情報を初めて参照した場合は新規登録、既に参照履歴が存在する場合は参照日時を更新する。
        ///     また、ログインユーザーの参照履歴が最大件数を超過した場合は参照日時が古い履歴を削除する。
        /// </summary>
        /// <param name="db">DbContext</param>
        /// <param name="kokyakuId">参照する顧客情報のID</param>
        /// <param name="syainBaseId">ログインユーザーの社員BaseID</param>
        public static async Task MaintainKokyakuKaisyaSansyouRirekiAsync(ZouContext db, long kokyakuId, long syainBaseId)
        {
            // 既存の顧客会社参照履歴を取得
            KokyakuKaisyaSansyouRireki? existingRireki = await db.KokyakuKaisyaSansyouRirekis
                .FirstOrDefaultAsync(x => x.KokyakuKaisyaId == kokyakuId && x.SyainBaseId == syainBaseId);

            // 削除対象検索クエリ
            Expression<Func<KokyakuKaisyaSansyouRireki, bool>> searchQuery;

            if (existingRireki is not null)
            {
                // 顧客会社参照履歴を更新
                existingRireki.SansyouTime = DateTime.Now;

                // 削除対象検索クエリ（更新データは検索しない）
                searchQuery = x => (x.SyainBaseId == syainBaseId && x.Id != existingRireki.Id);
            }
            else
            {
                // 顧客会社参照履歴を登録
                KokyakuKaisyaSansyouRireki newEntity = new()
                {
                    KokyakuKaisyaId = kokyakuId,
                    SyainBaseId = syainBaseId,
                    SansyouTime = DateTime.Now
                };
                db.KokyakuKaisyaSansyouRirekis.Add(newEntity);

                // 削除対象検索クエリ
                searchQuery = x => (x.SyainBaseId == syainBaseId);
            }

            // 参照履歴超過件数分の削除（登録・更新データ含めて最大件数分まで参照履歴に残す）
            List<KokyakuKaisyaSansyouRireki> deleteRirekis = await db.KokyakuKaisyaSansyouRirekis
                .Where(searchQuery)
                .OrderByDescending(x => x.SansyouTime)
                .Skip(MaxHistoryCount - 1)
                .ToListAsync();

            if (0 < deleteRirekis.Count)
            {
                db.KokyakuKaisyaSansyouRirekis.RemoveRange(deleteRirekis);
            }
        }

    }
}
