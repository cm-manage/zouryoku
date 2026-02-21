using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Model;

namespace Zouryoku.Utils
{
    public class DepartmentHierarchy
    {
        /// <summary>
        /// 部署名表示の部署同士の区切り文字
        /// </summary>
        public const string Delimiter = " > ";

        /// <summary>
        /// 部署名表示の末尾に付ける文字
        /// </summary>
        public const string EndPoint = " : ";

        /// <summary>
        /// 指定した部署IDから親部署を辿り、有効期限内かつアクティブな部署のみを対象に、
        /// 「親 → 子」の順で連結した階層文字列を生成して返します。
        /// 
        /// 起点部署から親部署へと辿った名称を「親 → 子」の順に並べ、区切り文字（<c>Delimiter</c>）で連結し、
        ///     末尾には <c>EndPoint</c> を付与します。
        ///     
        /// 起点部署が取得できない、または部署名が空白の場合は string.Empty を返します。
        /// 
        /// 親の欠落・名称不正・循環参照を検出した場合は、その時点までに収集できた名称で連結した階層文字列を返します。
        /// </summary>
        /// <param name="db">DBコンテキスト</param>
        /// <param name="referenceDate">取得対象を絞り込む基準日付。</param>
        /// <param name="busyoId">階層生成の起点となる部署ID。<c>null</c> の場合は空文字列を返します。</param>
        /// <returns>部署名</returns>
        public static async Task<string> GetDepartmentHierarchyStringAsync(ZouContext db, DateOnly referenceDate, long? busyoId)
        {
            // 引数がnullだった場合、空文字をreturn
            if (busyoId == null)
                return string.Empty;

            // 有効期限内かつアクティブな部署情報を全て取得
            var busyoList = await db.Busyos
                .Where(x => x.StartYmd <= referenceDate && referenceDate <= x.EndYmd && x.IsActive)
                .AsNoTracking()
                .ToListAsync();

            // 取得した部署情報をIDをキーとしたDictionaryに変換
            var map = busyoList.ToDictionary(x => x.Id);

            // 起点部署が取得できない場合 or 部署名が空白の場合、空文字をreturn
            if (!map.TryGetValue(busyoId.Value, out var current) ||
                string.IsNullOrWhiteSpace(current.Name))
            {
                return string.Empty;
            }

            // 部署階層名称リストを構築
            var names = BuildHierarchyNames(current, map);

            // 文字列化
            return string.Concat(string.Join(Delimiter, names), EndPoint);
        }

        /// <summary>
        /// 部署階層名称リストを構築します。
        /// </summary>
        /// <param name="start">起点部署</param>
        /// <param name="map">部署IDをキー、部署情報を値としたマップ</param>
        /// <returns>部署階層名称リスト</returns>
        private static List<string> BuildHierarchyNames(Busyo start, Dictionary<long, Busyo> map)
        {
            // 部署名称を格納するスタック（親→子の順に格納するため）
            var names = new Stack<string>();

            // 訪問済み部署IDの集合（循環参照検出用）
            var visited = new HashSet<long>();

            // 現在の部署
            var current = start;

            // 親部署を辿るループ
            while (true)
            {
                // 名称不正
                if (string.IsNullOrWhiteSpace(current.Name))
                    break;

                // 循環参照
                if (!visited.Add(current.Id))
                    break;

                // 名称をスタックに格納
                names.Push(current.Name);

                // 親が無い
                if (current.OyaId is null)
                    break;

                // 親が取得できない
                if (!map.TryGetValue(current.OyaId.Value, out var parent))
                    break;

                // 親へ移動
                current = parent;
            }

            // リスト化して返却
            return names.ToList();
        }
    }
}
