using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Zouryoku.Pages.ForTest
{
    /// <summary>
    /// オートコンプリート機能のテスト用ページモデル
    /// </summary>
    public class AutocompleteTestModel : PageModel
    {
        /// <summary>
        /// 検索対象の文字列のリスト
        /// </summary>
        public List<string> Terms = ["あ", "あい", "あいう"];

        /// <summary>
        /// Termsから検索するハンドラ
        /// </summary>
        /// <param name="term">入力値</param>
        /// <returns></returns>
        public IActionResult OnGetAutocomplete(string term)
        {
            return new JsonResult(Terms.Where(x => x.Contains(term)).Select(x => new { label = x }).ToList());
        }
    }
}
