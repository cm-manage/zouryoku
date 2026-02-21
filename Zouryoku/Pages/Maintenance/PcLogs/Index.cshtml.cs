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
using ZouryokuCommonLibrary;
using Zouryoku.Attributes; // Ensure AppConfig refers to common library type

namespace Zouryoku.Pages.Maintenance.PcLogs
{
    /// <summary>
    /// PCログ一覧ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class IndexModel : BasePageModel<IndexModel>
    {
        private readonly Model.Data.ZouContext _context;

        public IndexModel(ZouContext context, ILogger<IndexModel> logger, IOptions<AppConfig> options)
            : base(context, logger, options)
            => _context = context;

        public IList<PcLog> PcLog { get;set; } = default!;

        public async Task OnGetAsync()
        {
            PcLog = await _context.PcLogs
                .Include(p => p.Syain)
                .OrderBy(p => p.Datetime)
                .ToListAsync();
        }
    }
}
