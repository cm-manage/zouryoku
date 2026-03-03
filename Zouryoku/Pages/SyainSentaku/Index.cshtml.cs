using CommonLibrary.Extensions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;

namespace Zouryoku.Pages.SyainSentaku
{
    [FunctionAuthorizationAttribute]
    public class IndexModel : BasePageModel<IndexModel>
    {
        // ---------------------------------------------
        // 定数
        // ---------------------------------------------

        /// <summary>
        /// 複数選択時のPartialファイル名
        /// </summary>
        private const string PartialMultiple = "_SyainSentakuMultiplePartial";

        /// <summary>
        /// 単数選択時のPartialファイル名
        /// </summary>
        private const string PartialSingle = "_SyainSentakuSinglePartial";

        /// <summary>
        /// 増減量
        /// </summary>
        private const int Step = 1;

        /// <summary>
        /// 保存するセッション名
        /// </summary>
        private const string SaveSessionName = "selectedBusyoId";

        /// <summary>
        /// 本日の日付
        /// </summary>
        private DateOnly Today;

        // ---------------------------------------------
        // DI（サービス、DB、ロガーなど）
        // ---------------------------------------------

        public IndexModel(ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine, TimeProvider? timeProvider = null)
           : base(db, logger, optionsAccessor, viewEngine, timeProvider) { }

        /// <summary>
        /// 複数選択フラグ
        /// </summary>
        public bool IsMultipleSelection { get; set; } = default;

        /// <summary>
        /// 入力画面用共通CSS/JSをレイアウトで読み込むかのフラグ
        /// </summary>
        public override bool UseInputAssets => true;

        /// <summary>
        /// 文字列型の前回選択した社員情報
        /// </summary>
        public Dictionary<long, SyainViewModel> PreSelectedSyain { get; set; } = [];

        /// <summary>
        /// 前回選択されていた部署のカウント
        /// </summary>
        public Dictionary<long, int> PreSelectedBusyoCounts { get; set; } = [];

        /// <summary>
        /// 検索ワード（社員名）
        /// </summary>
        [BindProperty(SupportsGet = true)]
        [Display(Name = "社員名")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public string? SyainName { get; set; }

        /// <summary>
        /// 選択部署ID
        /// </summary>
        public long SelectionBusyoId { get; set; }

        /// <summary>
        /// ユーザー所属部署ID
        /// </summary>
        public long UserBusyoId { get; set; }

        /// <summary>
        /// Partialページに渡すmodel
        /// </summary>
        public PartialModel SyainListPage { get; set; } = new PartialModel();

        /// <summary>
        /// 検索対象部署ID
        /// </summary>
        public List<long> TargetBusyoIds { get; set; } = [];

        /// <summary>
        /// 検索対象社員
        /// </summary>
        public List<SyainViewModel> SyainList { get; set; } = [];

        public class ValidateSelectionRequest
        {
            [Display(Name = "社員")]
            public int SelectCounts { get; set; }

            public long BusyoId { get; set; }
        }

        // ---------------------------------------------
        // OnGet
        // ---------------------------------------------

        /// <summary>
        /// 初期表示に選択状態にする部署ID、最終選択社員情報取得
        /// </summary>
        /// <param name="isMultipleSelection">複数選択フラグ</param>
        /// <param name="id">最終選択社員BaseId文字列</param>
        /// <returns>社員選択ページ</returns>
        public async Task<IActionResult> OnGetAsync(bool isMultipleSelection, string? ids)
        {
            // 複数選択フラグからモード設定
            IsMultipleSelection = isMultipleSelection;

            // 検索ワードのエラー表示を防ぐ
            ModelState.Clear();

            // sessionから最終選択部署IDを取得
            var busyoID = HttpContext.Session.Get<long>(SaveSessionName);

            // sessionに最終選択部署IDがある場合、最終担部署IDを初期選択
            SelectionBusyoId = !busyoID.IsNone ? busyoID.Value() : LoginInfo.User.BusyoId;

            // ログインユーザー所属部署ボタン設定のため、所属部署IDを取得
            UserBusyoId = LoginInfo.User.BusyoId;

            // 最終選択社員が存在するかつ複数選択状態の場合、最終選択社員IDから社員情報取得
            if (!string.IsNullOrEmpty(ids) && IsMultipleSelection)
            {
                // ids を long のリストに変換
                List<long> selectedSyainBaseIds = ids
                    .Split('_', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => id.Trim())
                    .Select(id =>
                    {
                        bool parsed = long.TryParse(id, out long value);
                        return (parsed, value);
                    })
                    .Where(result => result.parsed)
                    .Select(result => result.value)
                    .ToList();

                // 現在日付取得
                Today = timeProvider.Today();

                // 最終選択社員を設定
                PreSelectedSyain = await GetPreselectedSyainAsync(selectedSyainBaseIds);

                // 部署ごとのカウント設定
                PreSelectedBusyoCounts = PreSelectedSyain.Values
                    .GroupBy(syain => syain.BusyoId)
                    .ToDictionary(group => group.Key, group => group.Count());
            }
            return Page();
        }

        /// <summary>
        /// 部署ツリービュー生成
        /// </summary>
        /// <returns>ツリービュー形式部署リスト</returns>
        public async Task<IActionResult> OnGetTreeAsync()
        {
            // 現在日付取得
            Today = timeProvider.Today();
            var busyo = await GetBusyoAsync();
            var busyoList = BuildTree(busyo);
            return new JsonResult(busyoList);
        }

        /// <summary>
        /// 部署IDから社員検索
        /// </summary>
        /// <param name="busyoId">選択部署Id</param>
        /// <param name="isMultipleFlg">複数選択フラグ</param>
        /// <param name="selectedSyainIds">最終選択社員(退職者含む)</param>
        /// <returns>検索結果表示PartialPage</returns>
        public async Task<IActionResult> OnGetSyainAsync(int busyoId, bool isMultipleFlg, string selectedSyainIds)
        {
            // 選択済み社員IDリスト
            List<long> selectedIds = [];
            if (!string.IsNullOrWhiteSpace(selectedSyainIds))
            {
                selectedIds = JsonConvert.DeserializeObject<List<long>>(selectedSyainIds) ?? [];
            }

            // 社員表示Partialファイル名
            var partialName = GetPartialName(isMultipleFlg);

            // 現在日付取得
            Today = timeProvider.Today();

            // 部署一覧を取得
            var busyoList = await GetBusyoAsync();

            // 親子関係を構築してDepthを付与
            var orderedBusyoList = BuildParentRelation(busyoList);

            // 指定部署ID、指定部署ID配下取得
            TargetBusyoIds = GetChildrenBusyoIds(busyoId, orderedBusyoList);

            // 社員を取得
            SyainList = await GetSyainFromBusyoAsync(TargetBusyoIds, selectedIds);

            // 取得した社員を部署に割り当てる
            AssignSyainToBusyo(orderedBusyoList, SyainList);

            SyainListPage = new PartialModel
            {
                BusyoList = orderedBusyoList
            };

            var data = await PartialToJsonAsync(partialName, SyainListPage);
            return SuccessJson(null, data);
        }

        /// <summary>
        /// 社員名検索
        /// </summary>
        /// <param name="isMultipleFlg"></param>
        /// <returns>検索結果表示PartialPage</returns>
        public async Task<IActionResult> OnGetSearchAsync(bool isMultipleFlg)
        {
            // 入力値チェック
            var errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            // 社員表示Partialファイル名
            var partialName = GetPartialName(isMultipleFlg);

            // 現在日付取得
            Today = timeProvider.Today();

            // 部署一覧を取得
            var busyoList = await GetBusyoAsync();

            // 部署IDのMap作成
            var busyoMap = busyoList.ToDictionary(busyo => busyo.Id);

            // 取得した部署Listから親がない部署除外
            var filteredBusyoList = busyoList
                                    .Where(busyo => HasValidParentChain(busyo, busyoMap))
                                    .ToList();

            // 親子関係を構築して Depth を付与
            var orderedBusyoList = BuildParentRelation(filteredBusyoList);

            // 検索対象部署ID
            TargetBusyoIds = filteredBusyoList.Select(busyo => busyo.Id).ToList();

            // 社員名検索
            SyainList = await GetSyainFromNameAsync(SyainName!, TargetBusyoIds);

            // 取得した社員を部署に割り当て
            AssignSyainToBusyo(orderedBusyoList, SyainList);

            // Partialに渡すモデル
            SyainListPage = new PartialModel
            {
                BusyoList = orderedBusyoList
            };

            var data = await PartialToJsonAsync(partialName, SyainListPage);
            return SuccessJson(null, data);
        }

        /// <summary>
        /// 社員名検索のオートコンプリート
        /// </summary>
        /// <param name="term">入力値</param>
        /// <returns>入力値の類似候補</returns>
        public async Task<IActionResult> OnGetSyainNameAutoCompAsync(string term)
        {
            // 現在日付取得
            Today = timeProvider.Today();

            var busyoList = await GetBusyoAsync();

            // 部署IDのMap作成
            var busyoMap = busyoList.ToDictionary(busyo => busyo.Id);

            // 取得した部署Listから親がない部署除外
            var filteredBusyoList = busyoList
                                    .Where(busyo => HasValidParentChain(busyo, busyoMap))
                                    .ToList();

            // 検索対象部署ID
            var targetBusyoIds = filteredBusyoList.Select(busyo => busyo.Id).ToList();

            return new JsonResult(await GetSyainAutoComp(term, targetBusyoIds));
        }

        // ---------------------------------------------
        // OnPost
        // ---------------------------------------------

        /// <summary>
        /// バリデーションチェック、セッション保存
        /// </summary>
        /// <param name="data">検証リクエスト</param>
        /// <returns>選択検証結果JSONデータ</returns>
        public async Task<IActionResult> OnPostValidateSelectionAsync(ValidateSelectionRequest data)
        {
            // 未使用パラメータ警告抑制
            _ = data;

            ModelState.Remove(nameof(SyainName));

            // 選択数が0の場合、アラート表示
            if (data.SelectCounts <= 0)
            {
                return ErrorJson(string.Format(Const.ErrorSelectRequired, "社員"));
            }

            // セッションに部署IDを保存
            HttpContext.Session.Set(data.BusyoId, SaveSessionName);

            return SuccessJson();
        }

        // ---------------------------------------------
        // private メソッド
        // ---------------------------------------------

        /// <summary>
        /// 最終選択社員IDから社員情報取得
        /// </summary>
        /// <param name="preSelectedSyainBaseIds">最終選択社員BaseID</param>
        /// <returns>社員情報</returns>
        private async Task<Dictionary<long, SyainViewModel>> GetPreselectedSyainAsync(List<long> preSelectedSyainBaseIds)
        {
            return await db.Syains
                .AsNoTracking()
                .AsSplitQuery()
                .Where(syain =>
                    preSelectedSyainBaseIds.Contains(syain.SyainBaseId) &&
                    syain.StartYmd <= Today &&
                    Today <= syain.EndYmd &&
                    syain.Busyo.IsActive &&
                    syain.Busyo.StartYmd <= Today &&
                    Today <= syain.Busyo.EndYmd
                )
                .Include(syain => syain.Busyo)
                .Include(syain => syain.SyainBase)
                .OrderBy(syain => syain.Busyo.Jyunjyo)
                .ThenBy(syain => syain.Jyunjyo)
                .ThenByDescending(syain => syain.SyainBase.Code)
                .ToDictionaryAsync(
                    syain => syain.SyainBaseId,
                    syain => new SyainViewModel(syain)
                );
        }

        /// <summary>
        /// 部署検索
        /// </summary>
        /// <returns>検索結果</returns>
        private async Task<List<BusyoViewModel>> GetBusyoAsync()
        {
            // 部署検索結果
            return await db.Busyos
                .AsNoTracking()
                .Where(busyo =>
                    busyo.IsActive &&
                    busyo.StartYmd <= Today &&
                    Today <= busyo.EndYmd
                )
                .OrderBy(busyo => busyo.Jyunjyo)
                .Include(busyo => busyo.Syains)
                .Select(busyo => new BusyoViewModel(busyo))
                .ToListAsync();
        }

        /// <summary>
        /// 取得部署をツリー構造に変換
        /// </summary>
        /// <param name="source">部署リスト</param>
        /// <param name="parentId">親Id</param>
        /// <param name="depth">階層</param>
        /// <returns>ツリー構造変換後リスト</returns>
        private static List<BusyoViewModel> BuildTree(List<BusyoViewModel> source, long? parentId = null, int depth = 0)
        {
            return source
               .Where(busyo => busyo.OyaId == parentId)
               .OrderBy(busyo => busyo.Jyunjyo)
               .Select(busyo =>
               {
                   busyo.Children = BuildTree(source, busyo.Id, depth + Step);
                   return busyo;
               })
               .ToList();
        }

        /// <summary>
        /// 表示する部分ビューを取得
        /// </summary>
        /// <param name="isMultipleFlg">複数選択フラグ</param>
        /// <returns>部分ビューのファイル名</returns>
        public string GetPartialName(bool isMultipleFlg) => isMultipleFlg ? PartialMultiple : PartialSingle;

        /// <summary>
        /// 親子関係を構築
        /// </summary>
        /// <param name="busyoList">部署リスト</param>
        /// <returns>ルート部署から順に親子関係を平坦化したリスト</returns>
        private static List<BusyoViewModel> BuildParentRelation(List<BusyoViewModel> busyoList)
        {
            if (busyoList.Count == 0) return [];

            // キーが親ID、値がその親に紐づく子部署群となるルックアップ作成
            var lookup = busyoList.ToLookup(busyo => busyo.OyaId);

            return lookup[null]
                .OrderBy(busyo => busyo.Jyunjyo)
                .SelectMany(root => TraverseNode(root, 0, lookup))
                .ToList();
        }

        /// <summary>
        /// 各部署の階層設定、階層ごとの並び替え
        /// </summary>
        /// <param name="node">親部署</param>
        /// <param name="depth">階層</param>
        /// <param name="lookup">部署のルックアップ</param>
        /// <returns>階層ごとに並び替えられた部署</returns>
        private static IEnumerable<BusyoViewModel> TraverseNode(BusyoViewModel node, int depth, ILookup<long?, BusyoViewModel> lookup)
        {
            node.Depth = depth;
            node.Children = lookup[node.Id].OrderBy(busyo => busyo.Jyunjyo).ToList();
            var rest = node.Children.SelectMany(child => TraverseNode(child, depth + Step, lookup));
            return rest.Prepend(node);
        }

        /// <summary>
        /// 指定部署IDの配下、指定部署ID列挙
        /// </summary>
        /// <param name="rootId">指定部署ID</param>
        /// <param name="busyoList">部署リスト</param>
        /// <returns>列挙結果List</returns>
        private static List<long> GetChildrenBusyoIds(long rootId, List<BusyoViewModel> busyoList)
        {
            var dict = busyoList.ToDictionary(b => b.Id);

            // 指定部署が見つからない場合、空リスト返却
            if (!dict.TryGetValue(rootId, out var root))
                return [];

            var result = new List<long>();
            SetParentBusyoIds(root, result);
            return result;
        }

        /// <summary>
        /// 指定ノードID、子ノードをcollector追加
        /// </summary>
        /// <param name="node">ノード部署Id</param>
        /// <param name="collector">部署IdList</param>
        private static void SetParentBusyoIds(BusyoViewModel node, List<long> collector)
        {
            if (node == null || collector == null) return;

            IEnumerable<long> DescendantIds(BusyoViewModel n) =>
                new[] { n.Id }.Concat((n.Children ?? Enumerable.Empty<BusyoViewModel>())
                    .SelectMany(child => DescendantIds(child)));

            collector.AddRange(DescendantIds(node));
        }

        /// <summary>
        /// 所属部署から社員情報取得
        /// </summary>
        /// <param name="busyoIds">部署IDリスト</param>
        /// <param name="selectedIds">選択社員IDリスト</param>
        /// <returns>検索結果</returns>
        private async Task<List<SyainViewModel>> GetSyainFromBusyoAsync(List<long> busyoIds, List<long> selectedIds)
        {
            return await db.Syains
                .AsNoTracking()
                .AsSplitQuery()
                .Where(syain =>
                    busyoIds.Contains(syain.BusyoId) &&
                    syain.StartYmd <= Today &&
                    Today <= syain.EndYmd &&
                    (
                        // 未退職者
                        !syain.Retired
                        ||
                        // 選択済み社員（退職者含む）
                        selectedIds.Contains(syain.SyainBaseId)
                    )
                )
                .Include(syain => syain.Busyo)
                .Include(syain => syain.SyainBase)
                .OrderBy(syain => syain.Jyunjyo)
                .ThenByDescending(syain => syain.SyainBase.Code)
                .Select(syain => new SyainViewModel(syain))
                .ToListAsync();
        }

        /// <summary>
        /// 指定した部署リストに社員リストを割り当て
        /// </summary>
        /// <param name="busyoList">部署リスト</param>
        /// <param name="syainList">社員リスト</param>
        private static void AssignSyainToBusyo(List<BusyoViewModel> busyoList, List<SyainViewModel> syainList)
        {
            busyoList
                .Select(busyo =>
                {
                    var assigned = syainList.Where(syain => syain.BusyoId == busyo.Id).ToList();
                    busyo.Syains = assigned.Count == 0 ? null : assigned;

                    // 子部署にも再帰的に割り当てる
                    if (busyo.Children != null && busyo.Children.Count > 0)
                    {
                        AssignSyainToBusyo(busyo.Children, syainList);
                    }
                    return busyo;
                })
                .ToList();
        }

        /// <summary>
        /// 指定した部署から親をたどり、親部署が有効でルートに到達できるかを再帰的に判定
        /// </summary>
        /// <param name="busyo">部署Model</param>
        /// <param name="busyoMap">部署IDのMap</param>
        /// <returns>親部署存在フラグ</returns>
        private static bool HasValidParentChain(BusyoViewModel busyo, Dictionary<long, BusyoViewModel> busyoMap)
        {
            // ルートに到達（親IDが null）
            if (!busyo.OyaId.HasValue)
                return true;

            // 親が存在しない
            if (!busyoMap.TryGetValue(busyo.OyaId.Value, out var parent))
                return false;

            // 親について再帰的にチェック
            return HasValidParentChain(parent, busyoMap);
        }

        /// <summary>
        /// 入力された値と部分一致する社員名取得
        /// </summary>
        /// <param name="syainName">社員名</param>
        /// <param name="busyoIds">検索対象とする部署IDの一覧</param>
        /// <returns>検索結果</returns>
        private async Task<List<SyainViewModel>> GetSyainFromNameAsync(string syainName, List<long> busyoIds)
        {
            return await db.Syains
                .AsNoTracking()
                .AsSplitQuery()
                .Where(s =>
                    s.Name.Contains(syainName) &&
                    s.StartYmd <= Today &&
                    Today <= s.EndYmd &&
                    busyoIds.Contains(s.BusyoId)
                )
                .Include(syain => syain.Busyo)
                .Include(syain => syain.SyainBase)
                .OrderBy(syain => syain.Jyunjyo)
                .ThenByDescending(syain => syain.SyainBase.Code)
                .Select(syain => new SyainViewModel(syain))
                .ToListAsync();
        }

        /// <summary>
        /// オートコンプリートの候補検索
        /// </summary>
        /// <param name="syainName">社員名</param>
        /// <param name="busyoIds">検索対象とする部署IDの一覧</param>
        /// <returns>検索結果</returns>
        private async Task<List<string>> GetSyainAutoComp(string syainName, List<long> busyoIds)
        {
            return await db.Syains
               .AsNoTracking()
               .AsSplitQuery()
               .Where(syain =>
                   syain.Name.Contains(syainName) &&
                   syain.StartYmd <= Today &&
                   Today <= syain.EndYmd &&
                   busyoIds.Contains(syain.BusyoId)
               )
               .Include(syain => syain.Busyo)
               .Include(syain => syain.SyainBase)
               .OrderBy(syain => syain.Jyunjyo)
               .ThenByDescending(syain => syain.SyainBase.Code)
               .Select(syain => syain.Name)
               .Take(5)
               .ToListAsync();
        }
    }

    /// <summary>
    /// 部署ビューモデル
    /// </summary>
    public class BusyoViewModel
    {
        private readonly Busyo _busyo;

        public BusyoViewModel(Busyo busyo)
            => _busyo = busyo;

        [Display(Name = "ID")]
        public long Id => _busyo.Id;

        [Display(Name = "部署名称")]
        [JsonPropertyName("title")]
        public string? Name => _busyo.Name;

        [Display(Name = "並び順序")]
        public short Jyunjyo => _busyo.Jyunjyo;

        [Display(Name = "親ID")]
        public long? OyaId => _busyo.OyaId;

        public List<BusyoViewModel> Children { get; set; } = [];

        public List<SyainViewModel>? Syains { get; set; } = [];

        public int Depth { get; set; }
    }

    /// <summary>
    /// 社員ビューモデル
    /// </summary>
    public class SyainViewModel
    {
        private readonly Syain _syain;

        public SyainViewModel(Syain syain)
            => _syain = syain;

        [Display(Name = "社員ID")]
        public long Id => _syain.Id;

        [Display(Name = "社員名")]
        public string Name => _syain.Name;

        [Display(Name = "部署ID")]
        public long BusyoId => _syain.BusyoId;

        [Display(Name = "社員BaseID")]
        public long SyainBaseId => _syain.SyainBaseId;

        [Display(Name = "社員番号")]
        public string Code => _syain.Code;

        [Display(Name = "退職フラグ")]
        public bool Retired => _syain.Retired;
    }

    /// <summary>
    /// Partialページで使用Model
    /// </summary>
    public class PartialModel
    {
        public List<BusyoViewModel> BusyoList { get; set; } = [];
    }
}
