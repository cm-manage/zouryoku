using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Zouryoku.Pages.Kokyaku.KokyakuNameSearch
{
    public class KokyakuModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public bool IsNormal { get; set; } = true;

        public void OnGet()
        {
        }

        /// <summary>
        /// 仮の顧客情報のリスト
        /// </summary>
        public List<KokyakuMock> Kokyakus { get; set; } = [
            new KokyakuMock(1, "小島 悠靖"),
            new KokyakuMock(2, "前川 嶺"),
            new KokyakuMock(3, "成 将寿")
            ];

        /// <summary>
        /// 顧客情報
        /// </summary>
        public class KokyakuMock
        {
            public long Id { get; set; }
            public string Name { get; set; }

            public KokyakuMock(long id, string name)
            {
                Id = id;
                Name = name;
            }
        }
    }
}
