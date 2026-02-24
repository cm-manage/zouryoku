using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Zouryoku.Attributes;
using Zouryoku.Models;
using Zouryoku.Pages.Shared;
using static LanguageExt.Prelude;

namespace Zouryoku.Pages.Maintenance.ServiceHistory
{
    /// <summary>
    /// サービス稼働履歴一覧モデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class IndexModel : BasePageModel<IndexModel>
    {
        private readonly ZouContext _context;

        public IndexModel(ZouContext context, ILogger<IndexModel> logger, IOptions<AppConfig> options, TimeProvider? timeProvider = null)
            : base(context, logger, options, timeProvider)
            => _context = context;
        
        [BindProperty]
        /// <summary>
        /// サービス稼働履歴一覧の検索条件
        /// </summary>
        public SearchCondition Condition { get; set; } = new();

        /// <summary>
        /// 検索処理
        /// </summary>
        public async Task<JsonResult> OnPostSearchAsync()
        {
            return null;
            // TOOD Model変更に伴いコメントアウト、要対応
            //var q = _context.ServiceExecuteHistories
            //    .Include(s => s.ServiceExecute)
            //    .AsNoTracking();

            //// サービス名で絞り込み
            //if (Condition.ServiceType.HasValue)
            //{
            //    q = q.Where(x => x.ServiceExecute.Type == Condition.ServiceType.Value);
            //}

            //// 依頼日時（From）で絞り込み
            //Optional(Condition.RequestDateFrom)
            //    .IfSome(val => q = q.Where(x => x.RequestDatetime >= val));

            //// 完了日時（To）で絞り込み
            //Optional(Condition.CompletedDateTo)
            //    .IfSome(val => q = q.Where(x => x.CompletedDatetime <= val));

            //// ステータスで絞り込み
            //if (Condition.Status.HasValue)
            //{
            //    q = q.Where(x => x.Status == Condition.Status.Value);
            //}

            //var list = q
            //    .Select(x => new SearchGridModel
            //    {
            //        Id = x.Id,
            //        ServiceType = x.ServiceExecute.Type.ToString(),
            //        RequestDatetime = x.RequestDatetime,
            //        CompletedDatetime = x.CompletedDatetime,
            //        ExecuteDatetime = x.ExecuteDatetime,
            //        Status = x.Status.ToString(),
            //        Content = x.Content,
            //    });

            //// ソート（完了日時の降順で固定）
            //var result = await list
            //    .OrderByDescending(x => x.CompletedDatetime)
            //    .ThenByDescending(x => x.Id)
            //    .ToListAsync();

            //return new JsonResult(new GridJson<SearchGridModel>
            //{
            //    Data = result,
            //    ItemsCount = result.Count,
            //});
        }
    }

    /// <summary>
    /// グリッド表示用のモデル
    /// </summary>
    public class SearchGridModel
    {
        public long Id { get; set; }
        public required string ServiceType { get; set; }
        public required DateTime RequestDatetime { get; set; }
        public string DisplayRequestDate => RequestDatetime.ToString("yyyy/MM/dd HH:mm");
        public required DateTime ExecuteDatetime { get; set; }
        public string DisplayExecuteDate => ExecuteDatetime.ToString("yyyy/MM/dd HH:mm");
        public required DateTime CompletedDatetime { get; set; }
        public string DisplayCompletedDate => CompletedDatetime.ToString("yyyy/MM/dd HH:mm");
        public required string Status { get; set; }
        public string? Content { get; set; }
    }

    /// <summary>
    /// 検索条件
    /// </summary>
    public class SearchCondition
    {
        /// <summary>
        /// サービス名（サービス区分）
        /// </summary>
        public ServiceClassification? ServiceType { get; set; }

        /// <summary>
        /// 請求日期（From）
        /// </summary>
        public DateTime? RequestDateFrom { get; set; }

        /// <summary>
        /// 完成日期（To）
        /// </summary>
        public DateTime? CompletedDateTo { get; set; }

        /// <summary>
        /// ステータス
        /// </summary>
        public ServiceStatus? Status { get; set; }
    }
}