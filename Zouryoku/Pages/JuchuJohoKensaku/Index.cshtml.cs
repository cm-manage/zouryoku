using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Extensions;
using Model.Model;
using Zouryoku.Attributes;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Model.Enums.ContractClassification;
using static Zouryoku.Pages.JuchuJohoKensaku.IndexModel.JuchuJohoSearchModel;
using static Zouryoku.Pages.JuchuJohoKensaku.IndexModel.JuchuJohoSearchModel.KeiyakuJoutai;
using static Zouryoku.Pages.JuchuJohoKensaku.IndexModel.JuchuJohoSearchModel.SortKeyList;
using static Zouryoku.Utils.Const;
using static Zouryoku.Utils.KingsJuchuSansyouRirekisUtil;

namespace Zouryoku.Pages.JuchuJohoKensaku
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
            IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine)
            : base(db, logger, optionsAccessor, viewEngine) { }

        // ======================================
        // フィールド
        // ======================================

        /// <summary>
        /// 部分ページに引き渡すモデル
        /// </summary>
        public PartialPageModel PartialPage => new()
        {
            IsReferHistory = this.IsReferHistory,
            Juchus = this.Juchus,
            Pager = this.Pager
        };

        /// <summary>
        /// 履歴参照フラグ
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public bool IsReferHistory { get; set; } = true;

        /// <summary>
        /// 検索結果のリスト
        /// </summary>
        public List<JuchuJohoViewModel> Juchus { get; set; } = [];

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

        // iziModal, SweetAlert2用アセットを有効化
        public override bool UseInputAssets => true;

        /// <summary>
        /// 検索条件を格納するモデル
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public required JuchuJohoSearchModel SearchConditions { get; set; }

        // ======================================
        // リクエスト処理
        // ======================================

        // -- GET -------------------------------

        /// <summary>
        /// 初期表示用に各プロパティの初期値を設定するハンドラー
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // 検索条件の初期化
            SearchConditions = new()
            {
                SekouBusyoCd = LoginInfo.User.BusyoCode,
                Keiyaku = すべて,
                SortKey = 受注先顧客,
            };

            // プロパティの設定
            IsReferHistory = true;

            // 受注参照履歴情報を取得
            (SearchResultCount, Juchus) = await GetReferenceHistoriesAsync(LoginInfo.User.SyainBaseId, 0);

            return Page();
        }

        /// <summary>
        /// 参照履歴の最初のページを表示するハンドラー
        /// </summary>
        public async Task<IActionResult> OnGetReferenceHistoryAsync()
        {
            // 履歴参照フラグをtrueに
            IsReferHistory = true;

            // 参照履歴を取得
            (SearchResultCount, Juchus) = await GetReferenceHistoriesAsync(LoginInfo.User.SyainBaseId, 0);

            // 部分ページを返答
            return await RespondPageAsync(PartialPage);
        }

        /// <summary>
        /// 検索処理を実行する
        /// </summary>
        /// <returns>検索結果</returns>
        public async Task<IActionResult> OnGetSearchJuchusAsync()
        {
            if (!string.IsNullOrWhiteSpace(SearchConditions.JucKin))
            {
                // 受注金額の整形（カンマ除去 → long変換）
                var noComma = SearchConditions.JucKin.Replace(",", "");

                if (!long.TryParse(noComma, out var kin))
                {
                    ModelState.AddModelError(
                        nameof(SearchConditions.JucKin),
                        string.Format(ErrorNumberRangeLessThan, "受注金額", long.MaxValue.ToString("N0"))
                    );
                    return CommonErrorResponse();
                }
                else
                {
                    SearchConditions.JucKinLong = kin;
                }
            }

            // 入力値チェック
            var errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            // 履歴参照フラグをfalseにする
            IsReferHistory = false;

            (SearchResultCount, Juchus) = await SearchJuchusAsync(SearchConditions, 0);

            return await RespondPageAsync(PartialPage);
        }

        /// <summary>
        /// ページを移動する
        /// </summary>
        /// <param name="pageOffset">ページのオフセット</param>
        /// <param name="pageIndex">ページのインデックス</param>
        /// <param name="isReferHistory">参照履歴を取得するかどうかのフラグ</param>
        /// <returns>移動先の表示データ</returns>
        public async Task<IActionResult> OnGetMovePageAsync(int pageOffset, int pageIndex, bool isReferHistory)
        {
            // 遷移先のページ番号が負の場合は0に補正
            PageIndex = Math.Max(0, pageIndex + pageOffset);

            IsReferHistory = isReferHistory;

            (SearchResultCount, Juchus) = await GetJuchusAsync(IsReferHistory, SearchConditions);

            // 表示するページ番号がページ総数より大きい場合は最後のページへ移動し再取得
            // NOTE: PageIndexは0-base、ページ総数は1-baseなので、baseを合わせるために+1する
            if (Pager.PagesNum < PageIndex + 1)
            {
                PageIndex = Pager.PagesNum - 1;
                (SearchResultCount, Juchus) = await GetJuchusAsync(IsReferHistory, SearchConditions);
            }

            return await RespondPageAsync(PartialPage);

            // 受注情報リストを取得するためのローカル関数
            // 履歴参照時は参照履歴情報を、検索時は登録情報を取得する
            async Task<(int total, List<JuchuJohoViewModel> juchus)> GetJuchusAsync(bool isReferHistory, JuchuJohoSearchModel model)
            {
                if (isReferHistory)
                {
                    // 参照履歴情報
                    return await GetReferenceHistoriesAsync(LoginInfo.User.SyainBaseId, PageIndex);
                }
                else
                {
                    // 登録情報
                    return await SearchJuchusAsync(model, PageIndex);
                }
            }
        }

        /// <summary>
        /// 受注登録情報が存在するかどうかの確認
        /// </summary>
        /// <param name="juchuId">KINGS受注ID</param>
        /// <returns>存在すれば正常、しなければエラーとメッセージ</returns>
        public async Task<IActionResult> OnGetCheckExistenceAsync(long juchuId)
        {
            var isExist = await IsExistJuchuAsync(juchuId);
            if (isExist)
            {
                return Success();
            }

            ModelState.AddModelError(string.Empty, ErrorSelectedDataNotExists);
            return CommonErrorResponse();
        }

        /// <summary>
        /// 送り担当者リストボックス情報を生成する
        /// </summary>
        /// <param name="iriBusCd">送り元部署コード</param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetSearchOkrTansAsync(string? iriBusCd)
        {
            var list = await GetOkrTansAsync(iriBusCd);

            return new JsonResult(new
            {
                status = ResponseStatus.正常,
                data = list,
            });
        }

        // -- POST ------------------------------

        /// <summary>
        /// 参照履歴を削除する
        /// </summary>
        /// <param name="juchuId">KINGS受注ID</param>
        /// <param name="version">同時実行制御用のバージョン</param>
        /// <returns>同時実行制御発動時はエラー</returns>
        public async Task<IActionResult> OnPostDeleteHistoryAsync(int juchuId, uint version)
        {
            // エラーを削除
            ModelState.Clear();

            // 存在チェック
            var rirekiId = await IsExistJuchuRirekiAsync(LoginInfo.User.SyainBaseId, juchuId);
            if (!rirekiId.HasValue)
            {
                ModelState.AddModelError(string.Empty, ErrorSelectedDataNotExists);
                return CommonErrorResponse();
            }

            // 取得した受注参照履歴情報を削除
            await DeleteHistoryAsync(rirekiId.Value, version);

            // 同時実行制御が働いたとき
            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            return Success();
        }

        /// <summary>
        /// カード選択時
        /// </summary>
        /// <param name="juchuId">受注ID</param>
        /// <returns><see cref="正常"/></returns>
        public async Task<IActionResult> OnPostSelectAsync(long juchuId)
        {
            var isExist = await IsExistJuchuAsync(juchuId);

            // 存在チェック
            if (!isExist)
            {
                // BIndPropertyのエラーを削除
                ModelState.Clear();

                ModelState.AddModelError(string.Empty, ErrorSelectedDataNotExists);
                return CommonErrorResponse();
            }

            // 登録または更新を行い、参照履歴超過分を削除
            await MaintainKingsJuchuSansyouRirekiAsync(db, juchuId, LoginInfo.User.SyainBaseId);

            await db.SaveChangesAsync();

            return Success();
        }

        // ======================================
        // メソッド
        // ======================================

        /// <summary>
        /// 送り元部署から送り担当者情報を取得する
        /// </summary>
        /// <param name="iriBusCd">送り元部署コード</param>
        /// <returns>送り担当者情報リスト</returns>
        private async Task<List<OkrTanInfo>> GetOkrTansAsync(string? iriBusCd)
        {
            var query = db.KingsJuchus
                .AsNoTracking()
                .Where(k => k.IriBusCd != null);

            if (!string.IsNullOrEmpty(iriBusCd))
            {
                // 入力ありの場合のみ、絞り込む
                query = query.Where(k => k.IriBusCd == iriBusCd);
            }

            return await query
                .GroupBy(k => new
                {
                    k.OkrTanCd1,
                    k.OkrTanNm1
                })
                .Select(g => new OkrTanInfo
                {
                    Value = g.Key.OkrTanCd1,
                    Text = g.Key.OkrTanNm1
                })
                .ToListAsync();
        }

        /// <summary>
        /// KINGS受注登録に存在するかどうかを確認する
        /// </summary>
        /// <param name="juchuId">KINGS受注ID</param>
        /// <returns>存在すればtrue</returns>
        private async Task<bool> IsExistJuchuAsync(long juchuId)
        {
            return await db.KingsJuchus
                .AsNoTracking()
                .AnyAsync(k => k.Id == juchuId);
        }

        /// <summary>
        /// KINGS受注参照履歴に存在するかどうかを確認する
        /// </summary>
        /// <param name="syainBaseId">社員BASE ID</param>
        /// <param name="juchuId">KINGS受注ID</param>
        /// <returns>履歴ID　存在しなければnull</returns>
        private async Task<long?> IsExistJuchuRirekiAsync(long syainBaseId, long juchuId)
        {
            var rireki = await db.KingsJuchuSansyouRirekis
                .Where(k => k.SyainBaseId == syainBaseId && k.KingsJuchuId == juchuId)
                .AsNoTracking()
                .SingleOrDefaultAsync();

            return rireki?.Id;
        }

        /// <summary>
        /// 総件数と参照履歴を取得する
        /// </summary>
        /// <param name="syainBaseId">社員BASE ID</param>
        /// <param name="pageIndex">ページのインデックス</param>
        /// <returns>タプル(総件数, 参照履歴のリスト)</returns>
        private async Task<(int total, List<JuchuJohoViewModel> juchus)> GetReferenceHistoriesAsync(long syainBaseId, int pageIndex)
        {
            var query = db.KingsJuchuSansyouRirekis
                .AsSplitQuery()
                .AsNoTracking()
                .Include(r => r.KingsJuchu)
                .Where(r => r.SyainBaseId == syainBaseId)
                .OrderByDescending(r => r.SansyouTime);

            // 総件数を取得
            var count = await query.CountAsync();

            // 表示するデータを取得
            var result = await query
                .Skip(PageSize * pageIndex)
                .Take(PageSize)
                .Select(r => new JuchuJohoViewModel(r))
                .ToListAsync();

            return (count, result);
        }

        /// <summary>
        /// 総件数と受注登録情報を取得する
        /// </summary>
        /// <param name="model">検索条件のモデル</param>
        /// <param name="pageIndex">ページのインデックス</param>
        /// <returns>タプル(総件数, 受注登録情報のリスト)</returns>
        private async Task<(int total, List<JuchuJohoViewModel> juchus)> SearchJuchusAsync(JuchuJohoSearchModel model, int pageIndex)
        {
            IQueryable<KingsJuchu> query = db.KingsJuchus
                .AsNoTracking();

            if (model.IsFormEmpty)
            {
                // 画面.検索条件がすべて空白の場合に、年度条件を追加する

                var today = DateTime.Now.ToDateOnly();
                var nendo = today.GetFiscalYear();
                query = query.Where(j => j.Nendo == nendo);
            }
            else
            {
                // 画面.検索条件の各項目が空白ではない場合に、条件を追加する

                // 日付の範囲指定をFrom < Toに補正する
                model.ChaYmd.NormalizeDateRange();

                // 各項目が空白ではない場合に条件を追加
                // プロジェクト番号
                if (!string.IsNullOrWhiteSpace(model.JuchuuNo.ProjectNo))
                {
                    query = query.Where(j => j.ProjectNo.StartsWith(model.JuchuuNo.ProjectNo));
                }
                // 受注番号
                if (!string.IsNullOrWhiteSpace(model.JuchuuNo.JuchuuNo))
                {
                    query = query.Where(j => j.JuchuuNo != null && j.JuchuuNo.StartsWith(model.JuchuuNo.JuchuuNo));
                }
                // 受注行番号
                if (model.JuchuuNo.JuchuuGyoNo.HasValue)
                {
                    query = query.Where(j => j.JuchuuGyoNo == model.JuchuuNo.JuchuuGyoNo);
                }
                // 着工開始日
                if (model.ChaYmd.From.HasValue)
                {
                    query = query.Where(j => model.ChaYmd.From <= j.ChaYmd);
                }
                // 着工終了日
                if (model.ChaYmd.To.HasValue)
                {
                    query = query.Where(j => j.ChaYmd <= model.ChaYmd.To);
                }
                // 施工部署
                if (!string.IsNullOrWhiteSpace(model.SekouBusyoCd))
                {
                    query = query.Where(j => j.SekouBumonCd == model.SekouBusyoCd);
                }
                // 件名
                if (!string.IsNullOrWhiteSpace(model.Bukken))
                {
                    string bukken = StringUtil.NormalizeString(model.Bukken);
                    query = query.Where(j => j.SearchBukken.Contains(bukken));
                }
                // 送り元部署
                if (!string.IsNullOrWhiteSpace(model.IriBusCd))
                {
                    query = query.Where(j => j.IriBusCd == model.IriBusCd);
                }
                // 送り担当者
                if (!string.IsNullOrWhiteSpace(model.OkrTanCd1))
                {
                    query = query.Where(j => j.OkrTanCd1 == model.OkrTanCd1);
                }
                // 受注金額
                if (model.JucKinLong != null)
                {
                    query = query.Where(j => model.JucKinLong <= j.JucKin);
                }
                // 顧客名
                if (!string.IsNullOrWhiteSpace(model.KokyakuName))
                {
                    string kokyakuName = StringUtil.NormalizeString(model.KokyakuName);
                    query = query.Where(j =>
                           j.SearchKeiNm != null && j.SearchKeiNm.Contains(kokyakuName)
                        || j.SearchKeiKn != null && j.SearchKeiKn.Contains(kokyakuName)
                        || j.SearchJucNm != null && j.SearchJucNm.Contains(kokyakuName)
                        || j.SearchJucKn != null && j.SearchJucKn.Contains(kokyakuName)
                    );
                }
            }

            // 契約状態
            switch (model.Keiyaku)
            {
                case 自営:
                    query = query.Where(j => j.KeiyakuJoutaiKbn == 経費
                        || j.KeiyakuJoutaiKbn == 受注_自営
                        || j.KeiyakuJoutaiKbn == 仮受注_自営);
                    break;
                case 協同受け:
                    query = query.Where(j => j.KeiyakuJoutaiKbn == 受注_共同
                        || j.KeiyakuJoutaiKbn == 仮受注_共同);
                    break;
                case 依頼受け:
                    query = query.Where(j => j.KeiyakuJoutaiKbn == 受注_社内取引);
                    break;
            }

            // 総件数を取得
            var total = await query.CountAsync();

            // 並び順
            query = model.SortKey switch
            {
                // 受注先カナ昇順
                受注先顧客 => query.OrderBy(j => j.JucKn),
                // 契約先カナ昇順
                契約先顧客 => query.OrderBy(j => j.KeiKn),
                // 件名昇順
                受注件名 => query.OrderBy(j => j.Bukken),
                // 着工日降順
                着工日 => query.OrderByDescending(j => j.ChaYmd),
                // 受注日降順
                受注日 => query.OrderByDescending(j => j.JucYmd),
                _ => query
            };

            // 表示するデータを取得
            var result = await query
                .Skip(PageSize * pageIndex)
                .Take(PageSize)
                .Select(rireki => new JuchuJohoViewModel(rireki))
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
            var data = await PartialToJsonAsync("_JuchuPage", model);
            return SuccessJson(null, data);
        }

        /// <summary>
        /// 参照履歴を削除する
        /// </summary>
        /// <param name="rirekiId">受注参照履歴ID</param>
        /// <param name="version">表示データのバージョン</param>
        /// <exception cref="DbUpdateConcurrencyException">
        /// 表示データのバージョンとDB内のデータのバージョンが相異なるとき（排他制御）
        /// </exception>
        private async Task DeleteHistoryAsync(long rirekiId, uint version)
        {
            // 削除対象のデータを取得
            var targetRireki = await db.KingsJuchuSansyouRirekis
                .SingleOrDefaultAsync(r => r.Id == rirekiId);

            // 削除対象が存在しないとき処理を終了
            if (targetRireki is null)
                return;

            // 同時実行制御用にバージョンを設定
            db.SetOriginalValue(targetRireki, entity => entity.Version, version);

            // 削除
            db.KingsJuchuSansyouRirekis
                .Remove(targetRireki);

            await SaveWithConcurrencyCheckAsync(string.Format(ErrorConflictReload, "参照履歴"));
        }
    }

    /// <summary>
    /// 受注情報リストとページャーのラッパー
    /// </summary>
    public class PartialPageModel
    {
        /// <summary>
        /// 履歴参照フラグ
        /// NOTE: カードの削除ボタンの表示・非表示で使用される
        /// </summary>
        public required bool IsReferHistory { get; set; }

        /// <summary>
        /// 受注情報ビューモデルのリスト
        /// </summary>
        public required List<IndexModel.JuchuJohoViewModel> Juchus { get; set; }

        /// <summary>
        /// ページャーのモデル
        /// </summary>
        public required PagerModel Pager { get; set; }
    }
}
