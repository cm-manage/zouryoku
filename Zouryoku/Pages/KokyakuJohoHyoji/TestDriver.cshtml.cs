using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.KokyakuJohoHyoji
{
    [FunctionAuthorizationAttribute]
    public class TestDriverModel : BasePageModel<TestDriverModel>
    {
        public TestDriverModel(
            ZouContext db,
            ILogger<TestDriverModel> logger,
            IOptions<AppConfig> options)
            : base(db, logger, options)
        {
        }

        public void OnGet()
        {
        }
    }
}
