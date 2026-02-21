using Microsoft.EntityFrameworkCore;
using Model.Model;
using System;
using System.Linq;

namespace Model.Extensions
{
    public static class ModelExtensions
    {
        /// <summary>
        /// 社員BaseIDで絞り込み、対象日付が有効期間内の社員情報を抽出
        /// </summary>
        /// <param name="syainBaseId">社員BaseID</param>
        /// <param name="targetDate">対象日付</param>
        public static IQueryable<Syain> FilterBySyainBaseIdAndDate(this IQueryable<Syain> query, long syainBaseId, DateOnly targetDate)
        {
            return query.Where(s =>
                s.SyainBaseId == syainBaseId &&
                s.StartYmd <= targetDate &&
                targetDate <= s.EndYmd);
        }
    }
}
