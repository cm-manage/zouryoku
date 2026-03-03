using CommonLibrary.Extensions;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Extensions;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Zouryoku.Utils.KokyakuKaisyaSansyouRirekisUtil;

namespace Zouryoku.Pages.KokyakuMeiKensaku
{
    [FunctionAuthorization]
    public class IndexModel : BasePageModel<IndexModel>
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
            IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine, TimeProvider? timeProvider = null)
            : base(db, logger, optionsAccessor, viewEngine, timeProvider) { }

        // ======================================
        // フィールド
        // ======================================

        /// <summary>
        /// 部分ページに引き渡すモデル
        /// </summary>
        public CustomerNameSearchPageModel CustomerNameSearchPage => new()
        {
            IsReferHistory = this.IsReferHistory,
            Customers = this.Customers,
            Pager = this.Pager
        };

        /// <summary>
        /// 履歴検索フラグ
        /// </summary>
        public bool IsReferHistory { get; set; } = true;

        /// <summary>
        /// 検索結果のリスト
        /// </summary>
        public List<CustomerViewModel> Customers { get; set; } = [];

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

        // iziModal, SweetAlert2用アセットを有効化
        public override bool UseInputAssets => true;

        /// <summary>
        /// 検索ワード（顧客名）
        /// </summary>
        [BindProperty(SupportsGet = true)]
        [Display(Name = "顧客名")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public string? CustomerName { get; set; }

        // ======================================
        // リクエスト処理
        // ======================================

        // -- GET -------------------------------

        /// <summary>
        /// 初期表示用のフィールド値設定
        /// </summary>
        /// <param name="canCardClick">カード選択可能かどうか</param>
        public async Task OnGetAsync(bool canCardClick = false)
        {
            // 検索ワードのエラー表示を防ぐ
            ModelState.Clear();
            // 参照履歴を取得
            (SearchResultCount, Customers) = await GetReferenceHistoriesAsync(LoginInfo.User.SyainBaseId, 0);

            // フィールドの設定
            CanCardClick = canCardClick;
            IsReferHistory = true;
        }

        /// <summary>
        /// 参照履歴を取得する
        /// </summary>
        /// <returns>参照履歴の最初の20件</returns>
        public async Task<IActionResult> OnGetReferenceHistoryAsync()
        {
            // 履歴検索フラグをtrueに
            IsReferHistory = true;

            // 参照履歴を取得
            (SearchResultCount, Customers) = await GetReferenceHistoriesAsync(LoginInfo.User.SyainBaseId, 0);

            // 部分ページを返答
            return await RespondPageAsync(CustomerNameSearchPage);
        }

        /// <summary>
        /// 検索処理を実行する
        /// </summary>
        /// <returns>検索結果</returns>
        public async Task<IActionResult> OnGetSearchCustomersAsync()
        {
            // 入力値チェック
            var errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            // 履歴検索フラグをfalseにする
            IsReferHistory = false;

            (SearchResultCount, Customers) = await SearchCustomersAsync(CustomerName!, 0);

            return await RespondPageAsync(CustomerNameSearchPage);
        }

        /// <summary>
        /// ページを移動する
        /// </summary>
        /// <param name="offset">ページのオフセット</param>
        /// <param name="pageIndex">ページのインデックス</param>
        /// <param name="isReferHistory">参照履歴を取得するかどうかのフラグ</param>
        /// <returns>移動先の表示データ</returns>
        public async Task<IActionResult> OnGetMovePageAsync(int offset, int pageIndex, bool isReferHistory)
        {
            // 遷移先のページ番号が負の場合は0に補正
            PageIndex = Math.Max(0, pageIndex + offset);

            IsReferHistory = isReferHistory;

            (SearchResultCount, Customers) = await GetCustomersAsync(IsReferHistory, CustomerName!);

            // 表示するページ番号がページ総数より大きい場合は最後のページへ移動し再取得
            // NOTE: PageIndexは0-base、ページ総数は1-baseなので、baseを合わせるために+1する
            if (Pager.PagesNum < PageIndex + 1)
            {
                PageIndex = Pager.PagesNum - 1;
                (SearchResultCount, Customers) = await GetCustomersAsync(IsReferHistory, CustomerName!);
            }

            return await RespondPageAsync(CustomerNameSearchPage);

            // 顧客リストを取得するためのローカル関数
            // 履歴参照時は参照履歴を、検索時は検索結果を取得する
            async Task<(int total, List<CustomerViewModel> customers)> GetCustomersAsync(bool isReferHistory,
                string customerName)
                => isReferHistory ? await GetReferenceHistoriesAsync(LoginInfo.User.SyainBaseId, PageIndex)
                    : await SearchCustomersAsync(customerName, PageIndex);
        }

        /// <summary>
        /// 顧客会社情報が存在するかどうかの確認
        /// </summary>
        /// <param name="customerId">顧客会社ID</param>
        /// <returns>存在すれば正常、しなければエラーとメッセージ</returns>
        public async Task<IActionResult> OnGetCheckExistenceAsync(long customerId)
        {
            var isExist = await IsExistCustomerAsync(customerId);
            if (isExist)
            {
                return Success();
            }

            return ErrorJson(Const.ErrorSelectedDataNotExists);
        }

        // -- POST ------------------------------

        /// <summary>
        /// 参照履歴を削除する
        /// </summary>
        /// <param name="customerId">顧客会社ID</param>
        /// <param name="version">同時実行制御用のバージョン</param>
        /// <returns>同時実行制御発動時はエラー</returns>
        public async Task<IActionResult> OnPostDeleteHistoryAsync(long customerId, uint version)
        {
            // 顧客名の必須チェックエラーを削除
            ModelState.Clear();

            // 存在性チェック
            if (!await IsExistCustomerAsync(customerId))
            {
                return ErrorJson(Const.ErrorSelectedDataNotExists);
            }

            await DeleteHistoryAsync(LoginInfo.User.SyainBaseId, customerId, version);

            var errorJson = ModelState.ErrorJson();
            // 同時実行制御が働いたとき
            if (errorJson is not null)
                return errorJson;

            return Success();
        }

        /// <summary>
        /// カード選択時
        /// </summary>
        /// <param name="customerId">顧客会社ID</param>
        /// <returns><see cref="Model.Enums.ResponseStatus.正常"/></returns>
        public async Task<IActionResult> OnPostSelectAsync(long customerId)
        {
            // 存在性チェック
            if (!await IsExistCustomerAsync(customerId))
            {
                return ErrorJson(Const.ErrorSelectedDataNotExists);
            }

            // 登録または更新を行い、参照履歴超過分を削除
            await MaintainKokyakuKaisyaSansyouRirekiAsync(db, customerId, LoginInfo.User.SyainBaseId, timeProvider.Now());

            await db.SaveChangesAsync();

            return Success();
        }

        // ======================================
        // メソッド
        // ======================================

        /// <summary>
        /// 顧客情報が存在するかどうかを確認する
        /// </summary>
        /// <param name="customerId">顧客会社ID</param>
        /// <returns>存在すればtrue</returns>
        private async Task<bool> IsExistCustomerAsync(long customerId)
        {
            return await db.KokyakuKaishas
                .AsNoTracking()
                .AnyAsync(k => k.Id == customerId);
        }

        /// <summary>
        /// 総件数と参照履歴を取得する
        /// </summary>
        /// <param name="empBaseId">社員BASE ID</param>
        /// <param name="pageIndex">ページのインデックス</param>
        /// <returns>タプル(総件数, 参照履歴のリスト)</returns>
        private async Task<(int total, List<CustomerViewModel> customers)> GetReferenceHistoriesAsync(long empBaseId, int pageIndex)
        {
            var today = timeProvider.Today();

            var query = db.KokyakuKaisyaSansyouRirekis
                .AsNoTracking()
                .Where(history => history.SyainBaseId == empBaseId)
                .OrderByDescending(history => history.SansyouTime)
                .Include(history => history.KokyakuKaisya)
                    .ThenInclude(customer => customer.EigyoBaseSyain!.Syains.Where(s => s.StartYmd <= today && today <= s.EndYmd));

            // 総件数を取得
            var count = await query.CountAsync();

            // 表示するデータを取得
            var result = await query
                .Skip(PageSize * pageIndex)
                .Take(PageSize)
                .Select(history => new CustomerViewModel(history))
                .ToListAsync();

            return (count, result);
        }

        /// <summary>
        /// 総件数と顧客情報を取得する
        /// </summary>
        /// <param name="word">検索ワード（顧客名）</param>
        /// <param name="pageIndex">ページのインデックス</param>
        /// <returns>顧客情報のビューモデルのリスト</returns>
        private async Task<(int total, List<CustomerViewModel> customers)> SearchCustomersAsync(string word, int pageIndex)
        {
            var searchWord = StringUtil.NormalizeString(word);
            var today = timeProvider.Today();

            var query = db.KokyakuKaishas
                .AsNoTracking()
                .Where(customer => customer.SearchName.Contains(searchWord) || customer.SearchNameKana.Contains(searchWord))
                .OrderBy(customer => customer.NameKana)
                .Include(customer => customer.EigyoBaseSyain)
                    .ThenInclude(empBase => empBase!.Syains.Where(s => s.StartYmd <= today && today <= s.EndYmd));

            // 総件数を取得
            var total = await query.CountAsync();

            // 表示するデータを取得
            var result = await query
                .Skip(PageSize * pageIndex)
                .Take(PageSize)
                .Select(history => new CustomerViewModel(history))
                .ToListAsync();

            return (total, result);
        }

        /// <summary>
        /// 部分ページをクライアントに返却する
        /// </summary>
        /// <param name="model">ページモデル</param>
        /// <returns><see cref="Model.Enums.ResponseStatus.正常"/>とレンダリング結果のHTML</returns>
        private async Task<IActionResult> RespondPageAsync(CustomerNameSearchPageModel model)
        {
            var data = await PartialToJsonAsync("_KokyakuKaishasPage", model);
            return SuccessJson(null, data);
        }

        /// <summary>
        /// 参照履歴を削除する
        /// </summary>
        /// <param name="empBaseId">社員BASE ID</param>
        /// <param name="customerId">顧客会社ID</param>
        /// <param name="version">表示データのバージョン</param>
        /// <exception cref="DbUpdateConcurrencyException">
        /// 表示データのバージョンとDB内のデータのバージョンが相異なるとき（排他制御）
        /// </exception>
        private async Task DeleteHistoryAsync(long empBaseId, long customerId, uint version)
        {
            // 削除対象のデータを取得
            var targetHistory = await db.KokyakuKaisyaSansyouRirekis
                .SingleOrDefaultAsync(
                    history => history.SyainBaseId == empBaseId && history.KokyakuKaisyaId == customerId
                );

            // 削除対象が存在しないとき処理を終了
            if (targetHistory is null)
                return;

            // 同時実行制御用にバージョンを設定
            db.SetOriginalValue(targetHistory, entity => entity.Version, version);

            // 削除
            db.KokyakuKaisyaSansyouRirekis
                .Remove(targetHistory);

            await SaveWithConcurrencyCheckAsync(string.Format(Const.ErrorConflictReload, "参照履歴"));
        }
    }

    /// <summary>
    /// 顧客会社リストとページャーのラッパー
    /// </summary>
    public class CustomerNameSearchPageModel
    {
        /// <summary>
        /// 履歴参照フラグ
        /// NOTE: カードの削除ボタンの表示・非表示で使用される
        /// </summary>
        public required bool IsReferHistory { get; set; }

        /// <summary>
        /// 顧客会社ビューモデルのリスト
        /// </summary>
        public required List<CustomerViewModel> Customers { get; set; }

        /// <summary>
        /// ページャーのモデル
        /// </summary>
        public required PagerModel Pager { get; set; }
    }

    /// <summary>
    /// 顧客情報のビューモデル
    /// </summary>
    public class CustomerViewModel
    {
        // ======================================
        // フィールド
        // ======================================
        // 顧客会社と顧客会社参照履歴のどちらか一方が設定される
        private readonly KokyakuKaisha? _customer;
        private readonly KokyakuKaisyaSansyouRireki? _history;

        // ======================================
        // コンストラクタ
        // ======================================

        /// <summary>
        /// 顧客会社エンティティからビューモデルを生成するコンストラクタ
        /// </summary>
        /// <param name="customer">顧客会社テーブルのエンティティ</param>
        public CustomerViewModel(KokyakuKaisha customer) => _customer = customer;

        /// <summary>
        /// 参照履歴エンティティからビューモデルを生成するコンストラクタ
        /// </summary>
        /// <param name="history">顧客会社参照履歴テーブルのエンティティ</param>
        public CustomerViewModel(KokyakuKaisyaSansyouRireki history) => _history = history;

        // 顧客会社情報のソース
        // _customerか_historyのどちらか一方が設定されていることを前提に、それを返す
        // 存在しない場合は例外をスローする
        private KokyakuKaisha Source
            => _customer ?? _history?.KokyakuKaisya
               ?? throw new InvalidOperationException("顧客会社情報と顧客会社参照履歴の情報の両方が設定されていません。");

        // ======================================
        // プロパティ
        // ======================================

        [Display(Name = "顧客名")]
        public string Name => Source.Name;

        [Display(Name = "住所")]
        public string? Address => $"{Source.Jyuusyo1}{Source.Jyuusyo2}";

        [Display(Name = "電話番号")]
        public string? Tel => Source.Tel;

        // インスタンス生成時点で有効期限を確認したエンティティを入れている想定
        [Display(Name = "担当者名")]
        public string? SalesPersonName
            => Source.EigyoBaseSyain?.Syains.FirstOrDefault()?.Name;

        public long KokyakuKaishaId => Source.Id;

        public uint? Version => _history?.Version;
    }
}
