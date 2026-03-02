using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Extensions;
using Model.Model;
using Zouryoku.Attributes;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Model.Enums.ResponseStatus;
using static Zouryoku.Pages.AnkenMeiKensaku.IndexModel.AnkenSearchModel.SortKeyList;
using static Zouryoku.Utils.AnkenSansyouRirekisUtil;
using static Zouryoku.Utils.Const;

namespace Zouryoku.Pages.AnkenMeiKensaku
{
    [FunctionAuthorization]
    public partial class IndexModel : BasePageModel<IndexModel>
    {
        // ======================================
        // 定数
        // ======================================

        /// <summary>
        /// 1ページあたりの項目数
        /// </summary>
        private const int PageSize = 20;

        // ======================================
        // DI
        // ======================================

        public IndexModel(ZouContext db, ILogger<IndexModel> logger,
            IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine, TimeProvider timeProvider)
            : base(db, logger, optionsAccessor, viewEngine, timeProvider) { }

        // ======================================
        // フィールド
        // ======================================

        /// <summary>
        /// 部分ページに引き渡すモデル
        /// </summary>
        public PartialPageModel PartialPage => new()
        {
            IsReferHistory = this.IsReferHistory,
            Ankens = this.Ankens,
            Pager = this.Pager
        };

        /// <summary>
        /// 履歴検索フラグ
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public bool IsReferHistory { get; set; } = true;

        /// <summary>
        /// 検索結果のリスト
        /// </summary>
        public List<AnkenViewModel> Ankens { get; set; } = [];

        /// <summary>
        /// ページャー
        /// </summary>
        public PagerModel Pager => new()
        {
            PageIndex = this.PageIndex,
            Total = this.SearchResultCount,
            PageSize = PageSize
        };

        /// <summary>
        /// ページのインデックス（zero-based）
        /// </summary>
        public int PageIndex { get; set; } = 0;

        /// <summary>
        /// 検索結果の総数
        /// </summary>
        public int SearchResultCount { get; set; } = 0;

        /// <summary>
        /// 参照モード
        /// trueのときはカードクリック時に情報を渡す処理が有効になる
        /// </summary>
        public bool CanCardClick { get; set; } = false;

        /// <summary>
        /// 案件追加が可能かどうか
        /// </summary>
        public bool CanAdd { get; set; } = false;

        // iziModal, SweetAlert2用アセットを有効化
        public override bool UseInputAssets => true;

        /// <summary>
        /// 検索条件を格納するモデル
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public required AnkenSearchModel SearchConditions { get; set; }

        // ======================================
        // リクエスト処理
        // ======================================

        // -- GET -------------------------------

        /// <summary>
        /// 初期表示用に各プロパティの初期値を設定するハンドラー。
        /// </summary>
        /// <param name="canCardClick">カードクリック可能かどうか</param>
        /// <param name="canAdd">案件追加が可能かどうか</param>
        /// <param name="kokyakuId">初期検索条件に指定する顧客会社ID</param>
        public async Task<IActionResult> OnGetAsync(bool canCardClick, bool canAdd, long? kokyakuId = null)
        {
            var today = timeProvider.Today();

            // 参照履歴を取得
            (SearchResultCount, Ankens) = await GetReferenceHistoriesAsync(LoginInfo.User.SyainBaseId, 0, today);

            // プロパティの設定
            CanCardClick = canCardClick;
            CanAdd = canAdd;
            IsReferHistory = true;

            // 検索条件の初期化
            var kokyakuName = kokyakuId is not null ? await GetKokyakuNameAsync(kokyakuId.Value) : null;
            SearchConditions = new()
            {
                JuchuuNo = new(),
                ChaYmd = new()
                {
                    // 現在日付の2年前の1/1
                    From = new DateOnly(today.AddYears(-2).Year, 1, 1),
                    // 現在日付の月末
                    To = today.GetEndOfMonth()
                },
                IsOwnBusyoOnly = true,
                ShowGenkaToketu = false,
                SortKey = 顧客名,
                KokyakuName = kokyakuName
            };

            return Page();
        }

        /// <summary>
        /// 参照履歴の最初のページを表示するハンドラー。
        /// </summary>
        public async Task<IActionResult> OnGetReferenceHistoryAsync()
        {
            // 履歴検索フラグをtrueに
            IsReferHistory = true;

            // 参照履歴を取得
            (SearchResultCount, Ankens) = await GetReferenceHistoriesAsync(LoginInfo.User.SyainBaseId, 0, timeProvider.Today());

            // 部分ページを返答
            return await RespondPageAsync(PartialPage);
        }

        /// <summary>
        /// 検索処理を実行する
        /// </summary>
        /// <returns>検索結果</returns>
        public async Task<IActionResult> OnGetSearchAnkensAsync()
        {
            // 入力値チェック
            var errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            // 履歴検索フラグをfalseにする
            IsReferHistory = false;

            (SearchResultCount, Ankens) = await SearchAnkensAsync(SearchConditions, 0);

            return await RespondPageAsync(PartialPage);
        }

        /// <summary>
        /// ページを移動する
        /// </summary>
        /// <param name="pageOffset">ページのオフセット</param>
        /// <param name="pageIndex">ページのインデックス</param>
        /// <returns>移動先の表示データ</returns>
        public async Task<IActionResult> OnGetMovePageAsync(int pageOffset, int pageIndex)
        {
            // 遷移先のページ番号が負の場合は0に補正
            PageIndex = Math.Max(0, pageIndex + pageOffset);

            (SearchResultCount, Ankens) = await GetAnkensAsync(IsReferHistory, SearchConditions);

            // 表示するページ番号がページ総数より大きい場合は最後のページへ移動し再取得
            // NOTE: PageIndexは0-base、ページ総数は1-baseなので、baseを合わせるために+1する
            if (Pager.PagesNum < PageIndex + 1)
            {
                PageIndex = Pager.PagesNum - 1;
                (SearchResultCount, Ankens) = await GetAnkensAsync(IsReferHistory, SearchConditions);
            }

            return await RespondPageAsync(PartialPage);

            // 顧客リストを取得するためのローカル関数
            // 履歴参照時は参照履歴を、検索時は検索結果を取得する
            async Task<(int total, List<AnkenViewModel> ankens)> GetAnkensAsync(bool isReferHistory, AnkenSearchModel model)
            {
                // 履歴参照の場合
                if (isReferHistory)
                {
                    return await GetReferenceHistoriesAsync(LoginInfo.User.SyainBaseId, PageIndex, timeProvider.Today());
                }
                // 顧客名検索の場合
                else
                {
                    return await SearchAnkensAsync(model, PageIndex);
                }
            }
        }

        /// <summary>
        /// 案件情報が存在するかどうかの確認
        /// </summary>
        /// <param name="ankenId">案件ID</param>
        /// <returns>存在すれば正常、しなければエラーとメッセージ</returns>
        public async Task<IActionResult> OnGetCheckExistenceAsync(long ankenId)
        {
            var isExist = await IsExistAnkenAsync(ankenId);
            if (isExist)
            {
                return Success();
            }

            ModelState.AddModelError(string.Empty, ErrorSelectedDataNotExists);
            return ModelState.ErrorJson()!;
        }

        // -- POST ------------------------------

        /// <summary>
        /// 参照履歴を削除する
        /// </summary>
        /// <param name="ankenId">案件ID</param>
        /// <param name="version">同時実行制御用のバージョン</param>
        /// <returns>同時実行制御発動時はエラー</returns>
        public async Task<IActionResult> OnPostDeleteHistoryAsync(int ankenId, uint version)
        {
            // 顧客名の必須チェックエラーを削除
            ModelState.Clear();

            // 存在性チェック
            if (!await IsExistAnkenAsync(ankenId))
            {
                ModelState.AddModelError(string.Empty, ErrorSelectedDataNotExists);
                return ModelState.ErrorJson()!;
            }

            await DeleteHistoryAsync(LoginInfo.User.SyainBaseId, ankenId, version);

            // 同時実行制御が働いたとき
            if (!ModelState.IsValid)
            {
                return ModelState.ErrorJson()!;
            }

            return Success();
        }

        /// <summary>
        /// カード選択時
        /// </summary>
        /// <param name="ankenId">案件ID</param>
        /// <returns><see cref="正常"/></returns>
        public async Task<IActionResult> OnPostSelectAsync(long ankenId)
        {
            var anken = await FindAnkenAsync(ankenId);

            // 存在性チェック
            if (anken == null)
            {
                // BIndPropertyのエラーを削除
                ModelState.Clear();

                ModelState.AddModelError(string.Empty, ErrorSelectedDataNotExists);
                return ModelState.ErrorJson()!;
            }

            // 登録または更新を行い、参照履歴超過分を削除
            await MaintainAnkenSansyouRirekiAsync(db, anken!, LoginInfo.User.SyainBaseId, timeProvider.Now());

            await db.SaveChangesAsync();

            return Success();
        }

        // ======================================
        // メソッド
        // ======================================

        public async Task<Anken?> FindAnkenAsync(long ankenId)
        {
            return await db.Ankens
                .Where(a => a.Id == ankenId)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// 顧客会社IDから顧客名を取得する。
        /// </summary>
        /// <param name="kokyakuId">顧客会社ID</param>
        /// <returns>顧客名</returns>
        private async Task<string?> GetKokyakuNameAsync(long kokyakuId)
        {
            return await db.KokyakuKaishas
                .Where(k => k.Id == kokyakuId
                    && k.EigyoBaseSyain!.Syains.Any(
                        // NOTE: 単純にDateOnly.MaxValueと比較するとマッチしない（文字列比較になっている？）
                        //       ので時間部分が00:00:0の0DateTime型として比較する
                        s => s.EndYmd.ToDateTime(TimeOnly.MinValue) == DateTime.MaxValue.ToDateOnly().ToDateTime()
                    )
                )
                .Select(k => k.Name)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 案件情報が存在するかどうかを確認する
        /// </summary>
        /// <param name="ankenId">案件ID</param>
        /// <returns>存在すればtrue</returns>
        private async Task<bool> IsExistAnkenAsync(long ankenId)
        {
            return await db.Ankens
                .AsNoTracking()
                .AnyAsync(k => k.Id == ankenId);
        }


        /// <summary>
        /// 総件数と参照履歴を取得する
        /// </summary>
        /// <param name="syainBaseId">社員BASE ID</param>
        /// <param name="pageIndex">ページのインデックス</param>
        /// <param name="today">社員有効期限を確認する日付</param>
        /// <returns>タプル(総件数, 参照履歴のリスト)</returns>
        private async Task<(int total, List<AnkenViewModel> ankens)> GetReferenceHistoriesAsync(long syainBaseId, int pageIndex, DateOnly today)
        {
            var query = db.AnkenSansyouRirekis
                .AsNoTracking()
                .AsSplitQuery()
                .Include(r => r.Anken)
                    .ThenInclude(a => a.SyainBase)
                        .ThenInclude(s => s!.Syains.Where(s => s.StartYmd <= today && today <= s.EndYmd))
                .Include(r => r.Anken)
                    .ThenInclude(a => a.KingsJuchu)
                .Include(r => r.Anken)
                    .ThenInclude(a => a.KokyakuKaisya)
                .Where(r => r.SyainBaseId == syainBaseId)
                .OrderByDescending(r => r.SansyouTime);

            // 総件数を取得
            var count = await query.CountAsync();

            // 表示するデータを取得
            var result = await query
                .Skip(PageSize * pageIndex)
                .Take(PageSize)
                .Select(r => new AnkenViewModel(r))
                .ToListAsync();

            return (count, result);
        }

        /// <summary>
        /// 総件数と案件情報を取得する
        /// </summary>
        /// <param name="model">検索条件のモデル</param>
        /// <param name="pageIndex">ページのインデックス</param>
        /// <returns>案件情報のビューモデルのリスト</returns>
        private async Task<(int total, List<AnkenViewModel> ankens)> SearchAnkensAsync(AnkenSearchModel model, int pageIndex)
        {
            var today = timeProvider.Today();

            // NOTE: varだとIQueryable<Anken, KokyakuKaisha?>型になるため明示的に宣言
            IQueryable<Anken> query = db.Ankens
                .AsSplitQuery()
                .AsNoTracking()
                .Include(a => a.SyainBase)
                    .ThenInclude(s => s!.Syains.Where(s => s.StartYmd <= today && today <= s.EndYmd))
                .Include(a => a.KingsJuchu)
                .Include(a => a.KokyakuKaisya);

            if (model.IsFormEmpty)
            {
                // 現在の年度を取得
                var nendo = today.GetFiscalYear();
                // 条件に追加
                // NOTE: nullチェックを入れないとKINGS受注登録データがないときに例外が発生する
                query = query.Where(a => a.KingsJuchu == null || a.KingsJuchu!.Nendo == nendo);
            }
            else
            {
                // 日付の範囲指定をFrom < Toに補正する
                model.ChaYmd.NormalizeDateRange();

                // 各項目が空白ではない場合に条件を追加
                //プロジェクト番号
                if (!string.IsNullOrWhiteSpace(model.JuchuuNo.ProjectNo))
                {
                    query = query.Where(a => a.KingsJuchu!.ProjectNo.StartsWith(model.JuchuuNo.ProjectNo));
                }
                //受注番号
                if (!string.IsNullOrWhiteSpace(model.JuchuuNo.JuchuuNo))
                {
                    query = query.Where(a => a.KingsJuchu!.JuchuuNo!.StartsWith(model.JuchuuNo.JuchuuNo));
                }
                //受注行番号
                if (model.JuchuuNo.JuchuuGyoNo.HasValue)
                {
                    query = query.Where(a => a.KingsJuchu!.JuchuuGyoNo == model.JuchuuNo.JuchuuGyoNo);
                }
                //着工開始日
                if (model.ChaYmd.From.HasValue)
                {
                    query = query.Where(a => a.KingsJuchu!.ChaYmd >= model.ChaYmd.From);
                }
                //着工終了日
                if (model.ChaYmd.To.HasValue)
                {
                    query = query.Where(a => a.KingsJuchu!.ChaYmd <= model.ChaYmd.To);
                }
                //顧客名
                if (!string.IsNullOrWhiteSpace(model.KokyakuName))
                {
                    string kokyakuName = StringUtil.NormalizeString(model.KokyakuName);
                    string kokyakuNameKana = StringUtil.NormalizeString(model.KokyakuName);
                    query = query.Where(a => a.KokyakuKaisya!.SearchName.Contains(kokyakuName)
                        || a.KokyakuKaisya.SearchNameKana.Contains(kokyakuNameKana));
                }
                //案件名
                if (!string.IsNullOrWhiteSpace(model.AnkenName))
                {
                    string ankenName = StringUtil.NormalizeString(model.AnkenName);
                    query = query.Where(a => a.SearchName.Contains(ankenName));
                }
                //責任者ID
                if (model.SekininSyaBaseId.HasValue)
                {
                    query = query.Where(a => a.SyainBaseId == model.SekininSyaBaseId);
                }
            }

            // 「自部署の案件のみ」がtrueのとき（施工部門コード）
            if (model.IsOwnBusyoOnly)
            {
                query = query.Where(a => a.KingsJuchu!.SekouBumonCd == LoginInfo.User.BusyoCode);
            }
            // 「凍結案件を表示」がfalseのとき
            if (!model.ShowGenkaToketu)
            {
                query = query.Where(a => !a.KingsJuchu!.IsGenkaToketu);
            }

            // 総件数を取得
            var total = await query.CountAsync();

            // 並び順
            query = model.SortKey switch
            {
                // 顧客名昇順
                顧客名 => query.OrderBy(a => a.KokyakuKaisya!.Name),
                // 着工日降順
                着工日 => query.OrderByDescending(a => a.KingsJuchu!.ChaYmd),
                _ => query
            };

            // 表示するデータを取得
            var result = await query
                .Skip(PageSize * pageIndex)
                .Take(PageSize)
                .Select(rireki => new AnkenViewModel(rireki))
                .ToListAsync();

            return (total, result);
        }

        /// <summary>
        /// 部分ページをクライアントに返却する
        /// </summary>
        /// <param name="model">ページモデル</param>
        /// <returns><see cref="正常"/>とレンダリング結果のHTML</returns>
        private async Task<IActionResult> RespondPageAsync(PartialPageModel model)
        {
            var data = await PartialToJsonAsync("_AnkenPage", model);
            return SuccessJson(null, data);
        }

        /// <summary>
        /// 参照履歴を削除する
        /// </summary>
        /// <param name="syainBaseId">社員BASE ID</param>
        /// <param name="ankenId">案件ID</param>
        /// <param name="version">表示データのバージョン</param>
        /// <exception cref="DbUpdateConcurrencyException">
        /// 表示データのバージョンとDB内のデータのバージョンが相異なるとき（排他制御）
        /// </exception>
        private async Task DeleteHistoryAsync(long syainBaseId, int ankenId, uint version)
        {
            // 削除対象のデータを取得
            var targetRireki = await db.AnkenSansyouRirekis
                .SingleOrDefaultAsync(
                    history => history.SyainBaseId == syainBaseId && history.AnkenId == ankenId
                );

            if (targetRireki is null)
            {
                return;
            }

            // 同時実行制御用にバージョンを設定
            db.SetOriginalValue(targetRireki, entity => entity.Version, version);

            // 削除
            db.AnkenSansyouRirekis
                .Remove(targetRireki);

            await SaveWithConcurrencyCheckAsync(string.Format(ErrorConflictReload, "参照履歴"));
        }
    }

    /// <summary>
    /// 顧客会社リストとページャーのラッパー
    /// </summary>
    public class PartialPageModel
    {
        /// <summary>
        /// 履歴参照フラグ
        /// NOTE: カードの削除ボタンの表示・非表示で使用される
        /// </summary>
        public required bool IsReferHistory { get; set; }

        /// <summary>
        /// 顧客会社ビューモデルのリスト
        /// </summary>
        public required List<IndexModel.AnkenViewModel> Ankens { get; set; }

        /// <summary>
        /// ページャーのモデル
        /// </summary>
        public required PagerModel Pager { get; set; }
    }
}
