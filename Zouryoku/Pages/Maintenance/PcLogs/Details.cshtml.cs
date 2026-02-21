using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Model;
using Zouryoku.Pages.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zouryoku.Attributes;

namespace Zouryoku.Pages.Maintenance.PcLogs
{
    /// <summary>
    /// PCログ詳細ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class DetailsModel : BasePageModel<DetailsModel>
    {
        private readonly Model.Data.ZouContext _context;

        public DetailsModel(ZouContext context, ILogger<DetailsModel> logger, IOptions<AppConfig> options)
            : base(context, logger, options)
            => _context = context;

        public PcLog PcLog { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pclog = await _context.PcLogs.FirstOrDefaultAsync(m => m.Id == id);

            if (pclog is not null)
            {
                PcLog = pclog;

                return Page();
            }

            return NotFound();
        }
    }
}
