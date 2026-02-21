using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.BusyoSentaku
{
    /// <summary>
    /// 部署選択ドライバページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class TestDriverModel : BasePageModel<TestDriverModel>
    {
        public TestDriverModel(ZouContext db, ILogger<TestDriverModel> logger, IOptions<AppConfig> options)
            : base(db, logger, options) { }

        public override bool UseInputAssets => true;

        public void OnGet()
        {
        }
    }
}
