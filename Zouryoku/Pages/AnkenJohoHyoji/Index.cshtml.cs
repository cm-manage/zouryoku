using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;

namespace Zouryoku.Pages.AnkenJohoHyoji
{
    /// <summary>
    /// 案件情報表示ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public partial class IndexModel : BasePageModel<IndexModel>
    {
        // ---------------------------------------------
        // 定数
        // ---------------------------------------------
        /// <summary>
        /// 画面描画ページ名
        /// </summary>
        private const string PartialScreenName = "_IndexPartial";

        /// <summary>
        /// エラー表示用ラベル
        /// </summary>
        private const string AnkenInfoLabel = "案件情報";

        // ---------------------------------------------
        // DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        public IndexModel(
            ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> options, ICompositeViewEngine viewEngine, TimeProvider? timeProvider = null)
            : base(db, logger, options, viewEngine, timeProvider) { }

        public override bool UseInputAssets => true;

        // ---------------------------------------------
        // プライベートプロパティ
        // ---------------------------------------------
        /// <summary>
        /// 本日の日付
        /// </summary>
        private DateOnly Today => timeProvider.Today();

        // ---------------------------------------------
        // OnGet
        // ---------------------------------------------

        /// <summary>
        /// 初期表示
        /// 取得したパラメータを画面に反映
        /// 無効なアクセスは画面にアクセスできないようにする
        /// </summary>
        /// <param name="id">案件ID</param>
        /// <param name="canAdd">登録可否</param>
        /// <returns>アクション結果</returns>
        public async Task<IActionResult> OnGetAsync(long id, bool canAdd)
        {
            // 案件情報の取得
            Anken? anken = await FetchAnkenAsync(id);

            // 案件情報の存在チェック
            if (anken is null)
            {
                return RedirectToPage("/ErrorMessage", new { errorMessage = Const.ErrorSelectedDataNotExists });
            }

            // 画面描画用ViewModelの作成
            IndexViewModel = new()
            {
                CanAdd = canAdd,
                LoginInfo = LoginInfo,
                Anken = anken,
            };

            // 案件参照履歴を保存
            await RegisterRirekiAsync(id);

            // 画面を表示
            return Page();
        }

        /// <summary>
        /// 初期処理 / 修正ボタン押下時の画面再描画処理
        /// 案件情報を取得し、画面描画用の部分ビューを返却する
        /// </summary>
        /// <param name="id">案件ID</param>
        /// <param name="canAdd">登録可能フラグ</param>
        /// <returns>アクション結果</returns>
        public async Task<IActionResult> OnGetSearchAsync(long id, bool canAdd)
        {
            // 案件情報の取得
            Anken? anken = await FetchAnkenAsync(id);

            if (anken is null)
            {
                ModelState.AddModelError
                    (string.Empty,
                    string.Format(Const.ErrorNotFound, AnkenInfoLabel, id));
            }

            JsonResult? errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            // 画面描画用ViewModelの作成
            IndexViewModel = new()
            {
                CanAdd = canAdd,
                LoginInfo = LoginInfo,
                Anken = anken,
            };

            var partialHtml = await PartialToJsonAsync(PartialScreenName, IndexViewModel);
            return SuccessJson(null, partialHtml);
        }

        /// <summary>
        /// 受注情報表示画面を表示する前の受注情報存在チェック処理
        /// </summary>
        /// <param name="juchuId">受注ID</param>
        /// <returns>アクション結果</returns>
        public async Task<IActionResult> OnGetCheckExistsJuchuAsync(long juchuId)
        {
            // 受注情報存在チェック
            var isExists = await db.KingsJuchus
                .AsNoTracking()
                .AnyAsync(data => data.Id == juchuId);

            // データが存在しない場合、エラーメッセージを返却
            if (!isExists)
            {
                ModelState.AddModelError(string.Empty, Const.ErrorSelectedDataNotExists);
            }

            JsonResult? errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            return Success();
        }

        /// <summary>
        /// 顧客情報表示画面を表示する前の顧客情報存在チェック処理
        /// </summary>
        /// <param name="kokyakuId">顧客ID</param>
        /// <returns>アクション結果</returns>
        public async Task<IActionResult> OnGetCheckExistsKokyakuAsync(long kokyakuId)
        {
            // 顧客情報存在チェック
            var isExists = await db.KokyakuKaishas
                .AsNoTracking()
                .AnyAsync(data => data.Id == kokyakuId);

            // データが存在しない場合、エラーメッセージを返却
            if (!isExists)
            {
                ModelState.AddModelError(string.Empty, Const.ErrorSelectedDataNotExists);
            }

            JsonResult? errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            return Success();
        }

        // ---------------------------------------------
        // DBアクセス用メソッド
        // ---------------------------------------------

        /// <summary>
        /// 表示案件情報取得
        /// </summary>
        /// <param name="ankenId">案件ID</param>
        /// <returns>案件情報</returns>
        private async Task<Anken?> FetchAnkenAsync(long ankenId) =>
            await db.Ankens
                .Include(a => a.SyainBase)
                .ThenInclude(sb => sb!.Syains.Where(s => s.StartYmd <= Today && Today <= s.EndYmd))
                .Include(a => a.KingsJuchu)
                .Include(a => a.KokyakuKaisya)
                .Include(a => a.JyutyuSyurui)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == ankenId);

        /// <summary>
        /// 案件参照履歴登録用の案件情報取得処理
        /// ※注意：この後 Anken を Attach するため、AsNoTracking は使用しない。
        /// </summary>
        /// <param name="ankenId">案件ID</param>
        /// <returns>案件情報</returns>
        private async Task<Anken?> FetchAnkenForRirekiAsync(long ankenId) =>
            await db.Ankens
                .SingleOrDefaultAsync(x => x.Id == ankenId);

        // ---------------------------------------------
        // プライベートメソッド
        // ---------------------------------------------

        /// <summary>
        /// 案件参照履歴を保存
        /// </summary>
        /// <param name="id">案件ID</param>
        /// <returns>アクション結果</returns>
        private async Task RegisterRirekiAsync(long id)
        {
            // 案件情報の取得
            Anken? anken = await FetchAnkenForRirekiAsync(id);

            // 参照履歴保存前の存在チェック
            if (anken is null)
            {
                throw new InvalidOperationException("案件情報が存在しません。");
            }

            // 案件参照履歴を保存
            await AnkenSansyouRirekisUtil.MaintainAnkenSansyouRirekiAsync(db, anken, LoginInfo.User.SyainBaseId, timeProvider.Now());
            await db.SaveChangesAsync();
        }
    }
}
