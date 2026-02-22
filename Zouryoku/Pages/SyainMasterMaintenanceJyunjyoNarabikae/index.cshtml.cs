using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.SyainMasterMaintenanceJyunjyoNarabikae
{
    /// <summary>
    /// 驛ｨ鄂ｲ荳ｦ縺ｳ譖ｿ縺医・繝ｼ繧ｸ繝｢繝・Ν
    /// </summary>
    [FunctionAuthorization]
    public class IndexModel : BasePageModel<IndexModel>
    {
        public IndexModel(
            ZouContext db,
            ILogger<IndexModel> logger,
            IOptions<AppConfig> options,
            ICompositeViewEngine viewEngine)
            : base(db, logger, options, viewEngine)
        {
        }

        // ---------------------------------------------
        // 騾壼ｸｸ縺ｮ繝励Ο繝代ユ繧｣・育判髱｢陦ｨ遉ｺ逕ｨ・・
        // ---------------------------------------------
        public override bool UseInputAssets => true;

        /// <summary>
        /// 驛ｨ鄂ｲID・磯∈謚槭＆繧後◆驛ｨ鄂ｲ・・
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public SearchCondition Condition { get; set; }

        /// <summary>
        /// 陦ｨ遉ｺ逕ｨ遉ｾ蜩｡繝ｪ繧ｹ繝・
        /// </summary>
        public IList<SyainViewModel> Syains { get; set; } = [];

        /// <summary>
        /// 蛻晄悄陦ｨ遉ｺ
        /// </summary>
        /// <returns>繝壹・繧ｸ繝ｪ繧ｶ繝ｫ繝・/returns>
        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }


        /// <summary>
        /// 荳ｦ縺ｳ鬆・ｿ晏ｭ・
        /// </summary>
        /// <param name="syains">荳ｦ縺ｳ鬆・､画峩蟇ｾ雎｡縺ｮ遉ｾ蜩｡繝ｪ繧ｹ繝・/param>
        /// <returns>螳溯｡檎ｵ先棡・・SON・・/returns>
        public async Task<IActionResult> OnPostRegisterAsync(List<SyainOrderModel> syains)
        {
            // 譖ｴ譁ｰ蟇ｾ雎｡ID縺ｮ縺ｿ謚ｽ蜃ｺ
            var updateIds = syains.Select(s => s.Id).ToHashSet();

            // 譖ｴ譁ｰ蟇ｾ雎｡繧剃ｸ蠎ｦ縺ｫ蜿門ｾ・
            var targetSyains = await db.Syains
                .Where(s => updateIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id);

            foreach (var dto in syains)
            {
                if (targetSyains.TryGetValue(dto.Id, out var syain))
                {
                    syain.Jyunjyo = dto.Jyunjyo;
                }
            }

            await db.SaveChangesAsync();
            return Success();
        }

        /// <summary>
        /// 遉ｾ蜩｡荳隕ｧ蜿門ｾ輸PI・磯Κ鄂ｲID謖・ｮ夲ｼ・
        /// </summary>
        /// <returns>遉ｾ蜩｡繝ｪ繧ｹ繝茨ｼ・SON・・/returns>
        public async Task<IActionResult> OnGetSyainListAsync()
        {
            var today = DateTime.Today.ToDateOnly();

            var syains = await db.Syains
                .Include(s => s.Busyo)
                .Where(s => s.BusyoId == Condition.BusyoId
                            && s.StartYmd <= today
                            && today <= s.EndYmd
                            && s.Retired == false)
                .OrderByDescending(s => s.Jyunjyo)
                .AsNoTracking()
                .ToListAsync();

            Syains = syains.Select(SyainViewModel.FromEntity).ToList();

            var html = await PartialToJsonAsync("_SyainListPartial", this);
            return SuccessJson(data: html); 
        }
    }

    /// <summary>
    /// 陦ｨ遉ｺ逕ｨ遉ｾ蜩｡繝｢繝・Ν
    /// </summary>
    public class SyainViewModel
    {
        /// <summary>
        /// 遉ｾ蜩｡ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 遉ｾ蜩｡蜷・
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 荳ｦ縺ｳ鬆・
        /// </summary>
        public short Jyunjyo { get; set; }

        /// <summary>
        /// 繧ｨ繝ｳ繝・ぅ繝・ぅ縺九ｉ陦ｨ遉ｺ逕ｨ繝｢繝・Ν繧剃ｽ懈・縺励∪縺吶・
        /// </summary>
        /// <param name="syain">螟画鋤蜈・・遉ｾ蜩｡繧ｨ繝ｳ繝・ぅ繝・ぅ</param>
        /// <returns>螟画鋤蠕後・SyainViewModel</returns>
        public static SyainViewModel FromEntity(Syain syain)
        {

            return new SyainViewModel
            {
                Id = syain.Id,
                Name = syain.Name,
                Jyunjyo = syain.Jyunjyo
            };
        }
    }

    /// <summary>
    /// 讀懃ｴ｢譚｡莉ｶ繝｢繝・Ν
    /// </summary>
    public class SearchCondition
    {
        /// <summary>
        /// 驕ｸ謚槭＆繧後◆驛ｨ鄂ｲID・・ULL險ｱ螳ｹ・・
        /// </summary>
        public long? BusyoId { get; set; }
    }

    /// <summary>
    /// 荳ｦ縺ｳ鬆・ｿ晏ｭ倡畑遉ｾ蜩｡繝｢繝・Ν
    /// </summary>
    public class SyainOrderModel
    {
        /// <summary>
        /// 遉ｾ蜩｡ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 險ｭ螳壹☆繧倶ｸｦ縺ｳ鬆・
        /// </summary>
        public short Jyunjyo { get; set; }
    }
 }

