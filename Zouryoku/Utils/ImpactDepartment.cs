using CommonLibrary.Extensions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Model;

namespace Zouryoku.Utils
{
    public class ImpactDepartment
    {
        /// <summary>
        /// 影響部署取得
        /// </summary>
        /// <param name="db">DBコンテキスト</param>
        /// <param name="busyoId">対象部署ID</param>
        /// <param name="today">基準日（本日）</param>
        /// <returns>影響部署</returns>
        public static async Task <List<Busyo>> GetImpactDepartmentAsync(ZouContext db, long busyoId, DateOnly today)
        {
            var tempData = new List<Busyo>();
            var busyos = await db.Busyos.AsNoTracking()
                                        .Where(x => x.IsActive && x.StartYmd <= today && today <= x.EndYmd)
                                        .Include(x =>x.BusyoBase)
                                        .ToListAsync();
            var parentId = GetParentId(busyos, busyoId);
            var oyaData = busyos.FirstOrDefault(x => x.Id == parentId);

            if (oyaData != null)
            {
                tempData.Add(oyaData);
                tempData.AddRange(GetChildId(busyos, oyaData.Id));                
            }

            // 承認部署追加
            var syoninFrom = busyos.FirstOrDefault(x => busyos.Any(y => y.Id == busyoId && y.ShoninBusyoId == x.Id));
            if (syoninFrom != null)
            {
                tempData.Add(syoninFrom);
            }
            var syoninTo = busyos.Where(x => x.ShoninBusyoId == busyoId).ToList();
            if (syoninTo.Any())
            {
                tempData.AddRange(syoninTo);
            }

            return tempData.Distinct().OrderBy(x => x.Jyunjyo).ToList();
        }

        /// <summary>
        /// 親部署取得
        /// </summary>
        /// <param name="busyos">有効な部署一覧</param>
        /// <param name="busyoId">対象の部署ID</param>
        /// <returns>親部署ID</returns>
        private static long? GetParentId(List<Busyo> busyos, long? busyoId)
        {
            var data = busyos.FirstOrDefault(x => x.Id == busyoId);

            if (data == null) return null;
            if (data.BusyoBase == null || data.BusyoBase.BumoncyoId == null)
            {
                var ret = GetParentId(busyos, data.OyaId);
                return ret == null ? data.Id : ret;
            }
            else
            {
                return data.Id;
            }
        }

        /// <summary>
        /// 子部署検索
        /// </summary>
        /// <param name="busyos">有効な部署一覧</param>
        /// <param name="id">対象部署ID</param>
        /// <returns>子部署リスト</returns>
        private static List<Busyo> GetChildId(List<Busyo> busyos, long id)
        {
            return busyos.Where(x => x.OyaId == id && (x.BusyoBase == null || x.BusyoBase.BumoncyoId == null))
                         .SelectMany(x =>
                         {
                              var temp = new List<Busyo>() { x };
                              temp.AddRange(GetChildId(busyos, x.Id));
                              return temp;
                         }).ToList();
        }
    }
}
