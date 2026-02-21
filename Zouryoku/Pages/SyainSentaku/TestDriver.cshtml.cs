using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.SyainSentaku
{
    [FunctionAuthorizationAttribute]
    public class TestDriverModel : BasePageModel<TestDriverModel>
    {
        public TestDriverModel(ZouContext db, ILogger<TestDriverModel> logger, IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine)
           : base(db, logger, optionsAccessor, viewEngine) { }

        /// <summary>
        /// 入力画面用共通CSS/JSをレイアウトで読み込むかのフラグ
        /// </summary>
        public override bool UseInputAssets => true;

        public void OnGet()
        {
        }
    }
}
