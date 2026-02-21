using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using ZouryokuCommonLibrary;

namespace Zouryoku.Pages.Explanation
{
    [FunctionAuthorizationAttribute]
    public class DescriptionModel : BasePageModel<DescriptionModel>
    {
        private readonly ZouContext _context;

        public DescriptionModel(ZouContext context, ILogger<DescriptionModel> logger, IOptions<AppConfig> options)
            : base(context, logger, options)
            => _context = context;

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
