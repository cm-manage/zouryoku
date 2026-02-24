using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zouryoku.Utils;

namespace Zouryoku.Pages
{
    public class ErrorMessage : PageModel
    {
        public async Task<IActionResult> OnGetAsync(string? errorMessage)
        {
            // パラメータが取得できなかった場合
            if (errorMessage == null)
            {
                ModelState.AddModelError(string.Empty, Const.Error);
                return Page();
            }

            ModelState.AddModelError(string.Empty, errorMessage);
            return Page();
        }
    }
}
