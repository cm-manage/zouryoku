using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using ZouryokuCommonLibrary;
using static Model.Enums.ResponseStatus;

namespace Zouryoku.Pages.SyainSort
{
    /// <summary>
    /// 部署並び替えページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class SortModel : BasePageModel<SortModel>
    {
        private readonly ZouContext context;

        public SortModel(ZouContext context, ILogger<SortModel> logger, IOptions<AppConfig> options, ICompositeViewEngine viewEngine)
            : base(context, logger, options, viewEngine)
        {
            this.context = context;
        }

        public override bool UseInputAssets => true;

        /// <summary>
        /// 部署ID（選択された部署）
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public long? BusyoId { get; set; }

        public string? BusyoName { get; set; }

        /// <summary>
        /// 表示用社員リスト
        /// </summary>
        public List<SyainModel> Syains { get; set; } = new();

        /// <summary>
        /// 初期表示（部署選択後に社員一覧を取得）
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            if (BusyoId.HasValue)
            {
                var busyo = await context.Busyos.AsNoTracking().FirstOrDefaultAsync(b => b.Id == BusyoId.Value);
                if (busyo != null)
                {
                    BusyoName = busyo.Name;
                    var syains = await context.Syains
                        .Where(s => s.BusyoId == busyo.Id && s.Retired == false)
                        .OrderBy(s => s.Jyunjyo)
                        .AsNoTracking()
                        .ToListAsync();
                    Syains = syains.Select(SyainModel.FromEntity).ToList();
                }
            }
            return Page();
        }


        /// <summary>
        /// 並び順保存
        /// </summary>
        public async Task<IActionResult> OnPostRegisterAsync(List<SyainOrderModel> Syains)
        {
            foreach (var dto in Syains)
            {
                var syain = await context.Syains.FirstOrDefaultAsync(s => s.Id == dto.Id);
                if (syain != null)
                {
                    syain.Jyunjyo = dto.Jyunjyo;
                }
            }

            await context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

        /// <summary>
        /// 社員一覧取得API（部署ID指定）
        /// </summary>
        public async Task<IActionResult> OnGetGetSyainListAsync(long busyoId)
        {
            var busyo = await context.Busyos.AsNoTracking().FirstOrDefaultAsync(b => b.Id == busyoId);
            if (busyo == null) return new JsonResult(new { success = false, message = "部署が見つかりません" });

            var syains = await context.Syains
                .Where(s => s.BusyoId == busyo.Id && s.Retired == false)
                .OrderBy(s => s.Jyunjyo)
                .AsNoTracking()
                .ToListAsync();

            var model = new SyainListPageModel
            {
                BusyoName = busyo.Name,
                Syains = syains.Select(SyainModel.FromEntity).ToList()
            };

            var html = await PartialToJsonAsync("SyainListPagePartial", model);
            return SuccessJson(data: html);
        }
    }

    /// <summary>
    /// 表示用社員モデル
    /// </summary>
    public class SyainModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public static SyainModel FromEntity(Model.Model.Syain entity)
        {
            return new SyainModel
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }
    }

    /// <summary>
    /// 並び順保存用社員モデル
    /// </summary>
    public class SyainOrderModel
    {
        public long Id { get; set; }
        public short Jyunjyo { get; set; }
    }

    public class SyainListPageModel
    {
        public string? BusyoName { get; set; }
        public List<SyainModel> Syains { get; set; } = new();
    }
 }
