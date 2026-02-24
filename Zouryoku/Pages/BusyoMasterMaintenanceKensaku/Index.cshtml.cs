using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.BusyoMasterMaintenanceKensaku
{
    /// <summary>
    /// 部署マスタメンテナンス検索ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class IndexModel : BasePageModel<IndexModel>
    {
        public IndexModel(ZouContext db, ILogger<IndexModel> logger,
            IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine, TimeProvider? timeProvider = null)
            : base(db, logger, optionsAccessor, viewEngine, timeProvider) { }

        public override bool UseInputAssets => true;

        /// <summary>
        /// 検索条件
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public BusyoSearchConditionModel Condition { get; set; } = new BusyoSearchConditionModel();

        /// <summary>
        /// 検索結果部署リスト
        /// </summary>
        public List<BusyoViewModel> Results { get; set; } = [];

        /// <summary>
        /// 画面初期表示
        /// </summary>
        /// <returns>部署マスタメンテナンスページ</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            Results = await GetBusyoListAsync();
            return Page();
        }

        /// <summary>
        /// 検索ボタン押下
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetSearchAsync()
        {
            Results = await GetBusyoListAsync();
            var data = await PartialToJsonAsync("_BusyoSearchResults", this);
            return SuccessJson(data: data);
        }

        /// <summary>
        /// 部署検索処理
        /// </summary>
        /// <returns></returns>
        private async Task<List<BusyoViewModel>> GetBusyoListAsync()
        {
            var today = timeProvider.Today();

            var query = db.Busyos
                .Include(b => b.BusyoBase)              // 部署 → 部署BASE → 部門長（社員）
                    .ThenInclude(bb => bb.Bumoncyo)
                .Include(b => b.ShoninBusyo)            // 部署 → 承認部署（自己参照）
                .Include(b => b.Oya)                    // 部署 → 親部署（自己参照）
                .AsSplitQuery()
                .AsNoTracking();

            // アクティブ条件（OFFの場合のみ）
            if (!Condition.IncludeInactive)
            {
                query = query.Where(b =>
                    b.IsActive &&
                    b.StartYmd <= today &&
                    today <= b.EndYmd);
            }

            var busyoList = await query.ToListAsync();

            // 部署名条件（入力がある場合のみ）
            if (!string.IsNullOrEmpty(Condition.BusyoName))
            {
                query = query.Where(b =>
                    b.Name.Contains(Condition.BusyoName));
            }

            // 部署名検索の条件に一致する部署
            var searchNameBusyoList = await query.ToListAsync();

            // 検索結果のIDをHashSet化
            var busyoIds = new HashSet<long>(searchNameBusyoList.Select(b => b.Id));

            // キーが親ID、値がその親に紐づく子部署群となるルックアップ作成
            var lookup = busyoList.ToLookup(b => b.OyaId);

            var allRoots = busyoList
                .Where(b => b.OyaId == null || !lookup.Contains(b.OyaId))
                .OrderBy(b => b.Jyunjyo);

            var flattenedBusyoList = FlattenBusyoViewModel(allRoots, 0, lookup);

            // 検索結果に含まれるIDのみ残す
            return flattenedBusyoList
                .Where(b => busyoIds.Contains(b.BusyoId))
                .ToList();
        }

        /// <summary>
        /// 各部署の階層設定、階層ごとの並び替え
        /// </summary>
        /// <param name="parents">親部署</param>
        /// <param name="busyoLookup">部署のルックアップ</param>
        /// <param name="depth">階層</param>
        /// <returns></returns>
        private static List<BusyoViewModel> FlattenBusyoViewModel(IEnumerable<Busyo> parents, int depth, ILookup<long?, Busyo> busyoLookup)
        {
            if (parents == null) return [];

            return parents
                .OrderBy(p => p.Jyunjyo)
                .SelectMany(busyo =>
                    new[] { new BusyoViewModel(busyo) { Depth = depth } }
                        .Concat(FlattenBusyoViewModel(busyoLookup[busyo.Id].OrderBy(b => b.Jyunjyo), depth + 1, busyoLookup))
                )
                .ToList();
        }
    }

    /// <summary>
    /// 検索条件
    /// </summary>
    public class BusyoSearchConditionModel
    {
        /// <summary>部署名称</summary>
        [Display(Name = "部署名")]
        public string? BusyoName { get; set; }

        /// <summary>現在無効な部署</summary>
        [Display(Name = "現在無効な部署を含む")]
        public bool IncludeInactive { get; set; }
    }

    /// <summary>
    /// 部署情報のViewモデル
    /// </summary>
    public class BusyoViewModel
    {
        // 部署エンティティ
        private readonly Busyo _busyo;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="busyo">部署エンティティ </param>
        public BusyoViewModel(Busyo busyo)
        {
            _busyo = busyo;
        }

        /// <summary>部署ID</summary>
        public long BusyoId => _busyo.Id;

        /// <summary>部署番号</summary>
        [Display(Name = "部署番号")]
        public string BusyoCode => _busyo.Code;

        /// <summary>部署名</summary>
        [Display(Name = "部署名")]
        public string BusyoName
            => _busyo.Oya != null ? $"{_busyo.Oya.Name}　{_busyo.Name}" : _busyo.Name;

        /// <summary>部門長</summary>
        [Display(Name = "部門長")]
        public string BumoncyoName
            => _busyo.BusyoBase.Bumoncyo?.Name ?? string.Empty;

        /// <summary>承認部署</summary>
        [Display(Name = "承認部署")]
        public string ShoninBusyoName
            => _busyo.ShoninBusyo?.Name ?? string.Empty;

        /// <summary>無効</summary>
        [Display(Name = "無効")]
        public string IsActiveDisplay
            => _busyo.IsActive ? string.Empty : "無効";

        /// <summary>階層</summary>
        public int Depth { get; set; }
    }
}