using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using ZouryokuCommonLibrary;

namespace Zouryoku.Pages.SyainMastaMaintenance
{
    /// <summary>
    /// 社員検索ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class SearchModel : BasePageModel<SearchModel>
    {
        private readonly ZouContext context;

        public SearchModel(ZouContext context, ILogger<SearchModel> logger, IOptions<AppConfig> options)
            : base(context, logger, options)
        {
            this.context = context;
        }

        // 検索条件モデル
        [BindProperty(SupportsGet = true)]
        public SyainSearchCondition Condition { get; set; } = new SyainSearchCondition();

        // 検索結果リスト
        public List<SyainSearchResultModel> Results { get; set; } = new();

        /// <summary>
        /// 勤怠属性の選択肢
        /// </summary>
        public SelectList KintaiZokuseiOptions { get; set; } = default!;

        /// <summary>
        /// ロールの選択肢
        /// </summary>
        public SelectList UserRoleOptions { get; set; } = default!;

        /// <summary>
        /// 社員権限の選択肢
        /// </summary>
        public SelectList KengenOptions { get; set; } = default!;

        /// <summary>
        /// 初期表示（一覧は空）
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // 勤怠属性一覧
            var kintaiList = await context.KintaiZokuseis
                .AsNoTracking()
                .Select(k => new { k.Id, k.Name })
                .ToListAsync();
            KintaiZokuseiOptions = new SelectList(kintaiList, "Id", "Name");

            // ロール一覧
            var roles = await context.UserRoles
                .AsNoTracking()
                .Select(r => new { r.Id, r.Name })
                .ToListAsync();
            UserRoleOptions = new SelectList(roles, "Id", "Name");

            var kengenList = Enum.GetValues(typeof(EmployeeAuthority))
                .Cast<EmployeeAuthority>()
                .Select(e => new { Id = (int)e, Name = e.ToString() })
                .ToList();
            KengenOptions = new SelectList(kengenList, "Id", "Name");


            Condition = new SyainSearchCondition();
            return Page();
        }

        /// <summary>
        /// 検索処理
        /// </summary>
        public async Task<IActionResult> OnPostSearchAsync()
        {
            Results = await GetSyainListAsync(Condition);

            // 勤怠属性一覧
            var kintaiList = await context.KintaiZokuseis
                .AsNoTracking()
                .Select(k => new { k.Id, k.Name })
                .ToListAsync();
            KintaiZokuseiOptions = new SelectList(kintaiList, "Id", "Name");

            // ロール一覧
            var roles = await context.UserRoles
                .AsNoTracking()
                .Select(r => new { r.Id, r.Name })
                .ToListAsync();
            UserRoleOptions = new SelectList(roles, "Id", "Name");

            var kengenList = Enum.GetValues(typeof(EmployeeAuthority))
                .Cast<EmployeeAuthority>()
                .Select(e => new { Id = (int)e, Name = e.ToString() })
                .ToList();
            KengenOptions = new SelectList(kengenList, "Id", "Name");

            return Page();
        }

        /// <summary>
        /// 社員一覧取得
        /// </summary>
        private async Task<List<SyainSearchResultModel>> GetSyainListAsync(SyainSearchCondition cond)
        {
            var query = context.Syains
                .Include(s => s.Busyo)
                .Include(s => s.KintaiZokusei)
                .Include(s => s.UserRole)
                .AsNoTracking()
                .AsQueryable();

            // 社員番号
            if (!string.IsNullOrEmpty(cond.SyainNo))
            {
                query = query.Where(s => s.Code.Contains(cond.SyainNo));
            }

            // 社員名
            if (!string.IsNullOrEmpty(cond.SyainName))
            {
                query = query.Where(s => s.Name.Contains(cond.SyainName));
            }

            // 部署
            if (!string.IsNullOrEmpty(cond.BusyoName))
            {
                query = query.Where(s => s.Busyo.Name.Contains(cond.BusyoName));
            }

            // 退職者を含む
            if (!cond.IncludeRetired)
            {
                query = query.Where(s => s.Retired == false);
            }

            // 勤怠属性
            if (cond.KintaiZokuseiId.HasValue)
            {
                query = query.Where(s => s.KintaiZokuseiId == cond.KintaiZokuseiId.Value);
            }

            // ロール
            if (cond.UserRoleId.HasValue)
            {
                query = query.Where(s => s.UserRoleId == cond.UserRoleId.Value);
            }

            // 社員権限
            if (cond.Kengen.HasValue)
            {
                query = query.Where(s => (s.Kengen & cond.Kengen.Value) != 0);
            }

            query = query.OrderBy(s => s.Jyunjyo);

            var list = await query.ToListAsync();

            return list.Select(s => new SyainSearchResultModel
            {
                Id = s.Id,
                SyainNo = s.Code,
                Name = s.Name,
                BusyoName = s.Busyo?.Name ?? string.Empty,
                Grade = s.Kyusyoku.ToString() ?? string.Empty,
                KintaiZokuseiName = s.KintaiZokusei?.Name ?? string.Empty,
                UserRoleName = s.UserRole?.Name ?? string.Empty,
                RetiredDisplay = s.Retired == true ? "退職" : string.Empty
            }).ToList();
        }



        // Search.cshtml.cs のクラス内に追加または置き換え
        public async Task<IActionResult> OnGetSearchAsync()
        {
            Results = await GetSyainListAsync(Condition);
            return await RespondPageAsync();
        }

        // 部分ページを返すユーティリティ
        private async Task<IActionResult> RespondPageAsync()
        {
            // PartialToJsonAsync が BasePageModel にある前提
            var data = await PartialToJsonAsync("_SyainSearchResults", this);
            return SuccessJson(data: data);
        }

    }

    /// <summary>
    /// 検索結果モデル
    /// </summary>
    public class SyainSearchResultModel
    {
        [Display(Name = "ID")]
        public long Id { get; set; }
        public string SyainNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string BusyoName { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string KintaiZokuseiName { get; set; } = string.Empty;
        public string UserRoleName { get; set; } = string.Empty;
        public string RetiredDisplay { get; set; } = string.Empty;
    }

    /// <summary>
    /// 検索条件モデル
    /// </summary>
    public class SyainSearchCondition
    {
        public string? SyainNo { get; set; }
        public string? SyainName { get; set; }
        public string? BusyoName { get; set; }
        public string? Grade { get; set; }
        public bool IncludeRetired { get; set; }
        public int? KintaiZokuseiId { get; set; }
        public int? UserRoleId { get; set; }
        public EmployeeAuthority? Kengen { get; set; }
    }
}

