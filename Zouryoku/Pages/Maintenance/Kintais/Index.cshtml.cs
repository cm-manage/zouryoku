using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Model;
using System.Globalization;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.Maintenance.Kintais
{
    /// <summary>
    /// 勤怠メンテナンスページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class IndexModel(ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine) : BasePageModel<IndexModel>(db, logger, optionsAccessor, viewEngine)
    {
        /// <summary>
        /// 入力画面用共通CSS/JSをレイアウトで読み込むかどうかのフラグ
        /// </summary>
        public override bool UseInputAssets { get; } = true;

        [BindProperty]
        public SearchCondition Condition { get; set; } = new();

        /// <summary>
        /// 社員選択用セレクトリスト
        /// </summary>
        public List<SelectListItem> SyainSelectList { get; set; } = new();

        /// <summary>
        /// 勤怠一覧ビュー
        /// </summary>
        public ViewModel KintaiView { get; set; } = new();

        /// <summary>
        /// 画面初期表示
        /// </summary>
        public async Task OnGetAsync()
        {
            var today = timeProvider.Today();
            Condition.TargetYearMonth ??= today;
            var allBusyos = await db.Busyos
                .AsNoTracking()
                .Where(b =>
                    b.IsActive &&
                    b.StartYmd <= today &&
                    b.EndYmd >= today)
                .OrderBy(b => b.Jyunjyo)
                .Select(b => b.Id)
                .ToListAsync();

            // 全社員を1回のクエリで取得
            SyainSelectList = await db.Syains
                .AsNoTracking()
                .Where(s => allBusyos.Contains(s.BusyoId) && !s.Retired && s.NyuusyaYmd <= today && s.StartYmd <= today && s.EndYmd >= today)
                .Select(x => new SelectListItem{
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToListAsync();

            KintaiView = await BuildKintaiViewAsync();
        }

        /// <summary>
        /// 検索ボタン
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostSearchAsync()
        {
            KintaiView = await BuildKintaiViewAsync();

            var data = await PartialToJsonAsync("_IndexPartial", KintaiView);
            return SuccessJson(null, data);
        }

        private async Task<ViewModel> BuildKintaiViewAsync()
        {
            if (Condition.TargetYearMonth == null || Condition.SyainId == null)
            {
                return new ViewModel();
            }

            var target = Condition.TargetYearMonth.Value;
            var start = target.GetStartOfMonth();
            var end = target.GetEndOfMonth();

            var nippous = await db.Nippous.AsNoTracking()
                .AsSplitQuery()
                .Where(n => n.SyainId == Condition.SyainId && start <= n.NippouYmd && n.NippouYmd <= end)
                .Include(n => n.SyukkinKubunId1Navigation)
                .Include(n => n.SyukkinKubunId2Navigation)
                .Include(n => n.NippouAnkens)
                    .ThenInclude(na => na.Ankens)
                    .ThenInclude(a => a.KingsJuchu)
                .OrderBy(n => n.NippouYmd)
                .ToListAsync();

            var rows = nippous.Select(nippou => new RowModel
            {
                Date = nippou.NippouYmd,
                DateText = FormatDateText(nippou.NippouYmd),
                SyukkinKubun = BuildSyukkinKubunText(nippou),
                JuchuNumbers = BuildJuchuNumbers(nippou),
                SyukkinTimes = BuildTimes(nippou.SyukkinHm1, nippou.SyukkinHm2, nippou.SyukkinHm3),
                TaisyutsuTimes = BuildTimes(nippou.TaisyutsuHm1, nippou.TaisyutsuHm2, nippou.TaisyutsuHm3),
                HZangyo = nippou.HZangyo,
                HWarimashi = nippou.HWarimashi,
                HShinyaZangyo = nippou.HShinyaZangyo,
                DZangyo = nippou.DZangyo,
                DWarimashi = nippou.DWarimashi,
                DShinyaZangyo = nippou.DShinyaZangyo,
                NJitsudou = nippou.NJitsudou,
                NShinya = nippou.NShinya,
            }).ToList();

            return new ViewModel
            {
                Rows = rows,
            };
        }

        private static string FormatDateText(DateOnly date)
            => date.ToDateTime().ToString("dd(ddd)", new CultureInfo("ja-JP"));

        private static string BuildSyukkinKubunText(Nippou nippou)
        {
            var names = new[]
            {
                nippou.SyukkinKubunId1Navigation?.Name,
                nippou.SyukkinKubunId2Navigation?.Name,
            };

            return string.Join(" + ", names.Where(name => !string.IsNullOrWhiteSpace(name)));
        }

        private static IReadOnlyList<string> BuildJuchuNumbers(Nippou nippou)
            => nippou.NippouAnkens
                .Select(x => x.Ankens?.KingsJuchu?.JuchuuNo)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct()
                .ToList();

        private static IReadOnlyList<string> BuildTimes(params TimeOnly?[] times)
            => times
                .Where(time => time.HasValue)
                .Select(time => time!.Value.ToString("HH:mm"))
                .ToList();

        public class SearchCondition
        {
            public long? SyainId { get; set; }

            public DateOnly? TargetYearMonth { get; set; }

            public ServiceStatus? Status { get; set; }
        }

        /// <summary>
        /// 勤怠一覧ビュー
        /// </summary>
        public class ViewModel
        {
            public IList<RowModel> Rows { get; set; } = [];
        }

        /// <summary>
        /// 勤怠一覧行
        /// </summary>
        public class RowModel
        {
            public DateOnly Date { get; set; }

            public string DateText { get; set; } = string.Empty;

            public string SyukkinKubun { get; set; } = string.Empty;

            public IReadOnlyList<string> JuchuNumbers { get; set; } = [];

            public IReadOnlyList<string> SyukkinTimes { get; set; } = [];

            public IReadOnlyList<string> TaisyutsuTimes { get; set; } = [];

            public decimal? HZangyo { get; set; }

            public decimal? HWarimashi { get; set; }

            public decimal? HShinyaZangyo { get; set; }

            public decimal? DZangyo { get; set; }

            public decimal? DWarimashi { get; set; }

            public decimal? DShinyaZangyo { get; set; }

            public decimal? NJitsudou { get; set; }

            public decimal? NShinya { get; set; }
        }
    }
}