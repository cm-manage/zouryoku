using Microsoft.AspNetCore.Http;
using static CommonLibrary.Extensions.OptionExtensions;

namespace CommonLibrary.Extensions
{
    public class PageParameter
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int Start { get; set; }
        public string SortField { get; set; } = "";
        public AscOrDesc SortOrder { get; set; }
    }

    public enum AscOrDesc
    {
        Asc,
        Desc,
    }

    public static class HttpRequestExtensions
    {
        public static string StringParam(this HttpRequest request, string key)
        {
            if (request.Method == "GET")
            {
                return OptionalT(request.Query[key])
                      .GetOrThrowException($"QueryParam[{key}]が取得できません。");
            }
            else
            {
                return OptionalT(request.Form[key])
                      .GetOrThrowException($"QueryParam[{key}]が取得できません。");
            }
        }

        public static int IntParam(this HttpRequest request, string key)
            => int.Parse(request.StringParam(key));

        /// <summary>
        /// jsGridから渡されたQueryParamから
        /// ページ番号、１ページ辺りの表示数、データ取得開始位置、ソート対象カラム、ソート順序を返します。
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static PageParameter GridParameter(this HttpRequest request)
        {
            // ページ番号
            var index = request.IntParam("pageIndex");
            // 1ページ表示上限
            var size = request.IntParam("pageSize");
            // データ取得開始位置
            var start = (index - 1) * size;
            // ソート対象カラム
            var sortField = request.StringParam("sortField");
            // ソート順序
            var sortOrder = request.StringParam("sortOrder");

            return new PageParameter
            {
                PageIndex = index,
                PageSize = size,
                Start = start,
                SortField = sortField,
                SortOrder = sortOrder == "desc" ? AscOrDesc.Desc : AscOrDesc.Asc,
            };
        }
    }
}
