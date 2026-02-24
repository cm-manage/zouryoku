using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Model;
using System.Linq.Expressions;

namespace Zouryoku.Utils
{
    /// <summary>
    /// 案件参照履歴共通処理
    /// </summary>
    public static class AnkenSansyouRirekisUtil
    {
        /// <summary>
        /// 案件情報参照履歴の最大登録件数
        /// </summary>
        public const int MaxAnkenHistoryCount = 50;

        /// <summary>
        /// 参照履歴の登録
        /// 
        /// 注意事項
        ///     この処理内ではコンテキストの保存を行わない。呼び出し元で保存処理を実行すること。
        /// 処理概要
        ///     呼出画面に案件情報を渡したとき、案件情報を表示した時、案件情報を編集した時に呼び出す。
        ///     案件情報を初めて参照した場合は新規登録、既に参照履歴が存在する場合は参照日時を更新する。
        ///     また、ログインユーザーの参照履歴が最大件数を超過した場合は参照日時が古い履歴を削除する。
        /// </summary>
        /// <param name="db">DbContext</param>
        /// <param name="anken">参照した案件情報</param>
        /// <param name="syainBaseId">ログインユーザーの社員BaseID</param>
        /// <param name="now">現在日時</param>
        public static async Task MaintainAnkenSansyouRirekiAsync(ZouContext db, Anken anken, long syainBaseId, DateTime now)
        {
            // 既存の案件参照履歴を検索
            AnkenSansyouRireki? existingRireki = await db.AnkenSansyouRirekis
                .FirstOrDefaultAsync(x => x.AnkenId == anken.Id && x.SyainBaseId == syainBaseId);

            // 削除対象検索クエリ
            Expression<Func<AnkenSansyouRireki, bool>> searchQuery;

            if (existingRireki is not null)
            {
                // 案件参照履歴を更新
                existingRireki.SansyouTime = now;

                // 削除対象検索クエリ（更新データは検索しない）
                searchQuery = x => (x.SyainBaseId == syainBaseId && x.Id != existingRireki.Id);
            }
            else
            {
                // 案件参照履歴を登録
                AnkenSansyouRireki newEntity = new AnkenSansyouRireki
                {
                    Anken = anken,
                    SyainBaseId = syainBaseId,
                    SansyouTime = now
                };
                db.AnkenSansyouRirekis.Add(newEntity);

                // 削除対象検索クエリ
                searchQuery = x => (x.SyainBaseId == syainBaseId);
            }

            // 参照履歴超過件数分の削除（登録・更新データ含めて最大件数分まで参照履歴に残す）
            List<AnkenSansyouRireki> deleteRirekis = await db.AnkenSansyouRirekis
                .Where(searchQuery)
                .OrderByDescending(x => x.SansyouTime)
                .Skip(MaxAnkenHistoryCount - 1)
                .ToListAsync();

            if (0 < deleteRirekis.Count)
            {
                db.AnkenSansyouRirekis.RemoveRange(deleteRirekis);
            }
        }
    }
}
