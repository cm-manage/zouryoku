using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Web.HttpUtility;

namespace Zouryoku.Pages.Utils
{
    [Authorize]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class ShowErrorAndCloseIziModalModel : PageModel
    {
        public required string Title { get; set; }
        public required string Message { get; set; }

        public void OnGet(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public static string URL(string title, string message)
            => "~/Utils/ShowErrorAndCloseIziModal"
                + "?title=" + UrlEncode(title)
                + "&message=" + UrlEncode(message);

        public static string AlredyDeletedURL => URL("既に削除されています。", "処理対象が削除されています、画面を閉じます。");

        public static string AlredyUpdatedURL => URL("既に更新済みです。", "他ユーザによって更新済みです、画面を閉じます。");
    }
}
