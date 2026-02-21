using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.ForTest
{
    public class PagerTestModel : PageModel
    {
        public PagerModel? Pager { get; set; }

        /// <summary>
        /// パラメーターからページャーを表示
        /// </summary>
        /// <param name="pagesNum">ページ総数</param>
        /// <param name="pageIndex">ページ番号</param>
        public void OnGet(int pagesNum, int pageIndex)
        {
            Pager = new()
            {
                PageIndex = pageIndex,
                Total = pagesNum * 20, // 項目数をページ番号から算出
                PageSize = 20
            };
        }
    }
}
