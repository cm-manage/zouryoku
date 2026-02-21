using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using ZouryokuCommonLibrary;

namespace Zouryoku.Pages.Explanation
{
    [FunctionAuthorizationAttribute]
    public class LawsModel : BasePageModel<LawsModel>
    {

        public LawsModel(ZouContext context, ILogger<LawsModel> logger, IOptions<AppConfig> options)
            : base(context, logger, options)
        {
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
