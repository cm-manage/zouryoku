using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Extensions;
using Model.Model;
using System.Collections.Immutable;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Model.Enums.LeavePlanStatus;

namespace Zouryoku.Pages.YukyuKeikakuJigyobuShonin
{
    /// <summary>
    /// 計画有給休暇事業部承認
    /// </summary>
    [FunctionAuthorization]
    public partial class IndexModel : BasePageModel<IndexModel>
    {
        // ---------------------------------------------
        // 1. 定数
        // ---------------------------------------------
        /// <summary>
        /// 計画有給休暇1件あたりの最大明細数（7日分の計画）
        /// マジックナンバー回避のため定数として定義
        /// </summary>
        private const int MeisaiPerYukyuKeikakuValue = 7;

        /// <summary>
        /// 計画有給休暇1件あたりの最大明細数（7日分の計画）
        /// </summary>
        public static int MeisaiPerYukyuKeikaku => MeisaiPerYukyuKeikakuValue;

        public static string ErrorConflictReloadYukyuKeikaku => string.Format(Const.ErrorConflictReload, "計画有給休暇");

        private static string ErrorReadBusyo => string.Format(Const.ErrorRead, "部署マスタ");

        private static string ErrorRegisterUnauthorized => string.Format(Const.ErrorRegister, "ログインユーザー", "権限不適格");

        // ---------------------------------------------
        // 2. DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        public IndexModel(
            ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> options, ICompositeViewEngine viewEngine)
            : base(db, logger, options, viewEngine)
        {
        }

        // ---------------------------------------------
        // 3. 通常のプロパティ（画面表示用）
        // ---------------------------------------------
        public override bool UseInputAssets => true;

        /// <summary>
        /// ログインユーザーが閲覧する事業部承認情報を保持するビュー モデル。
        /// </summary>
        public JigyoubuShoninViewModel LoginUserJigyoubuShoninViewModel { get; private set; } = new JigyoubuShoninViewModel();

        // ---------------------------------------------
        // 4. OnGet
        // ---------------------------------------------
        /// <summary>
        /// 計画有給休暇事業部承認画面の初期表示用検索ハンドラ。
        /// ログインユーザーの権限（人財・部門長など）に応じて検索範囲を切り替え、取得した検索結果を元に画面表示用ビュー
        /// モデルを構築し、その結果を描画した Razor Pages のページレスポンスを返却する。
        /// </summary>
        /// <returns>
        /// ログインユーザーの権限に応じた検索結果を描画した計画有給休暇事業部承認画面のページレスポンス。
        /// </returns>
        public async Task<IActionResult> OnGetAsync()
        {
            // ログインユーザの権限情報を取得
            var allBusyos = await GetAllBusyosWithRelationsAsync();
            if (!TryGetBumoncyoBusyoId(LoginInfo.User.BusyoId, allBusyos, out var bumoncyoBusyoId))
                return CommonErrorResponseWithMessage(ErrorReadBusyo);

            var loginUserAuthority = GetLoginUserAuthority(allBusyos[bumoncyoBusyoId]);

            // 検索結果情報を取得
            var viewModel = await CreateViewModelAsync(loginUserAuthority, bumoncyoBusyoId, allBusyos);

            // 画面を表示
            LoginUserJigyoubuShoninViewModel = viewModel;
            return Page();
        }

        // ---------------------------------------------
        // 5. OnPost
        // ---------------------------------------------
        /// <summary>
        /// 承認対象として選択された計画有給休暇を差し戻し（未申請）状態に更新する。
        /// </summary>
        /// <param name="viewModel">
        /// 画面で選択された計画有給休暇および検索条件・表示情報を保持するビューモデル。
        /// 差し戻し対象となる行がこのモデルに格納されていることを前提とする。
        /// </param>
        /// <returns>
        /// 入力検証が成功し、対象データのステータス更新が完了した場合は、
        /// 更新後の一覧を描画する部分ビューを JSON 形式で返却する。
        /// 単項目・複合項目の検証エラーが発生した場合は、エラー内容を含む JSON 応答を返却する。
        /// </returns>
        public Task<IActionResult> OnPostSendBackAsync(JigyoubuShoninViewModel viewModel) =>
            SendBackOrApproveAsync(viewModel, ActionType.SendBack);

        /// <summary>
        /// 選択された計画有給休暇について、ログインユーザの権限に応じて承認状態へ更新する。
        /// 人事権限（<see cref="Authority.Jinzai"/>）を持つ場合は「承認済」、それ以外の場合は「人財承認待ち」に更新する。
        /// </summary>
        /// <param name="viewModel">
        /// 承認対象として選択された計画有給休暇および検索条件を保持するビューモデル。
        /// </param>
        /// <returns>更新成功時は画面再表示用の応答、入力不備や検証エラー時はエラー内容を含む JSON 応答または共通エラー応答。</returns>
        public Task<IActionResult> OnPostApproveAsync(JigyoubuShoninViewModel viewModel) =>
            SendBackOrApproveAsync(viewModel, ActionType.Approve);

        private async Task<IActionResult> SendBackOrApproveAsync(JigyoubuShoninViewModel viewModel, ActionType actionType)
        {
            // 登録前チェック
            // 複合項目チェック
            ValidateNotChecked(viewModel);
            if (!ModelState.IsValid) return CommonErrorResponse();

            // ログインユーザの権限情報を取得
            var allBusyos = await GetAllBusyosWithRelationsAsync();
            if (!TryGetBumoncyoBusyoId(LoginInfo.User.BusyoId, allBusyos, out var bumoncyoBusyoId))
                return CommonErrorResponseWithMessage(ErrorReadBusyo);

            var loginUserAuthority = GetLoginUserAuthority(allBusyos[bumoncyoBusyoId]);
            if (loginUserAuthority == Authority.None)
                return CommonErrorResponseWithMessage(ErrorRegisterUnauthorized);

            // 更新条件の設定
            LeavePlanStatus newStatus;
            if (actionType == ActionType.Approve)
            {
                newStatus = loginUserAuthority == Authority.Jinzai ? 承認済 : 人財承認待ち;
            }
            else
            {
                newStatus = 未申請;
            }

            // 計画有給休暇情報の更新
            await UpdateStatusAsync(viewModel, newStatus);
            if (!ModelState.IsValid) return CommonErrorResponse();

            // 完了メッセージを表示（ビューで処理）
            // 画面を再表示（ビューで処理）
            var partialViewModel = await CreateViewModelAsync(loginUserAuthority, bumoncyoBusyoId, allBusyos);
            var data = await PartialToJsonAsync("_YukyuKeikakuList", partialViewModel);
            return SuccessJson(data: data);
        }

        // ---------------------------------------------
        // 6. private メソッド
        // ---------------------------------------------
        /// <summary>
        /// メッセージを指定して共通エラーレスポンスを返す
        /// </summary>
        private IActionResult CommonErrorResponseWithMessage(string message)
        {
            ModelState.AddModelError("", message);
            return CommonErrorResponse();
        }

        private async Task<Dictionary<long, Busyo>> GetAllBusyosWithRelationsAsync() => await db.Busyos
            .Include(b => b.Oya)
            .Include(b => b.BusyoBase)
            .AsNoTracking()
            .ToDictionaryAsync(b => b.Id);

        /// <summary>
        /// ログインユーザーの権限 <see cref="Authority"/> を取得する
        /// </summary>
        /// <param name="bumoncyoBusyo">ログインユーザーの所属部門長部署。</param>
        /// <returns>
        /// ログインユーザーの権限を表す <see cref="Authority"/>。
        /// 計画休暇承認権限を持つ場合は <see cref="Authority.Jinzai"/>、
        /// 部門長に該当する場合は <see cref="Authority.Bumoncyo"/>、
        /// それ以外の場合は <see cref="Authority.None"/> を返す。
        /// </returns>
        private Authority GetLoginUserAuthority(Busyo bumoncyoBusyo)
        {
            if (LoginInfo.User.IsPlannedLeaveApproval) return Authority.Jinzai;
            if (bumoncyoBusyo.BusyoBase.BumoncyoId == LoginInfo.User.SyainBaseId) return Authority.Bumoncyo;
            return Authority.None;
        }

        /// <summary>
        /// 部署階層を遡り、IDで指定した部署の部門長部署IDを取得する。
        /// </summary>
        /// <param name="busyoId">探索の起点となる部署ID。</param>
        /// <param name="allBusyos">親子関係を含む全ての部署一覧。</param>
        /// <param name="bumoncyoBusyoId">部署階層を遡って特定した部門長部署の部署ID。</param>
        /// <returns>
        /// 部門長部署が見つかった場合 true 、見つからなかった (部署マスタを正しく読み取れなかった) 場合 false 。
        /// </returns>
        private static bool TryGetBumoncyoBusyoId(long busyoId, Dictionary<long, Busyo> allBusyos, out long bumoncyoBusyoId)
        {
            var visited = new HashSet<long>();
            long? currentBusyoId = busyoId;
            while (currentBusyoId is not null)
            {
                // 指定されている部署が存在しない場合、または循環が検出された場合、false を返す。
                if (!allBusyos.TryGetValue(currentBusyoId.Value, out var currentBusyo) || !visited.Add(currentBusyo.Id))
                {
                    bumoncyoBusyoId = default;
                    return false;
                }

                if (currentBusyo.BusyoBase.BumoncyoId is not null)
                {
                    bumoncyoBusyoId = currentBusyoId.Value;
                    return true;
                }

                currentBusyoId = currentBusyo.OyaId;
            }

            // 全社 (Id = null) まで遡っても部門長部署が見つからなかった場合、false を返す。
            bumoncyoBusyoId = default;
            return false;
        }

        /// <summary>
        /// チェック仕様_チェックボックスの選択が1件以上あるかチェックする。
        /// </summary>
        private void ValidateNotChecked(JigyoubuShoninViewModel viewModel)
        {
            // チェックボックスの選択が1件以上あるかチェックする。
            if (!viewModel.Keikakus.Any(k => k.IsChecked))
            {
                ModelState.AddModelError("Keikakus", Const.ErrorNotChecked);
            }
        }

        /// <summary>
        /// 部門長権限での一覧表示ビューモデルを生成する。
        /// </summary>
        private Task<JigyoubuShoninViewModel> CreateViewModelAsync(
            Authority loginUserAuthority, long bumoncyoBusyoId, Dictionary<long, Busyo> allBusyos)
        {
            // 権限に応じたビューモデルを生成する
            var createViewModelService = new CreateViewModelService(db);
            return createViewModelService.CreateViewModelByAuthorityAsync(loginUserAuthority, bumoncyoBusyoId, allBusyos);
        }

        // ---------------------------------------------
        // ViewModel 更新メソッド
        // ---------------------------------------------
        /// <summary>
        /// チェックされた計画有給休暇のステータスを一括更新し、楽観的並行性制御エラーを検出します。
        /// </summary>
        /// <param name="viewModel">画面から送信された計画有給休暇一覧などの情報。</param>
        /// <param name="newStatus">更新後に設定する計画有給休暇ステータス。</param>
        /// <returns>更新の完了まで待機するタスク。</returns>
        private async Task UpdateStatusAsync(JigyoubuShoninViewModel viewModel, LeavePlanStatus newStatus)
        {
            // 更新条件の設定
            // CommonValidateで1件以上選択されていることは保証済
            var checkedYukyuKeikakus = viewModel.Keikakus
                .Where(k => k.IsChecked && k.Id is not null)
                .ToImmutableArray();
            // EFCore の WHERE 句用の配列
            var checkedYukyuKeikakuIds = checkedYukyuKeikakus
                .Select(k => k.Id!.Value)
                .ToImmutableHashSet();
            var yukyuKeikakus = await db.YukyuKeikakus
                .Where(yk => checkedYukyuKeikakuIds.Contains(yk.Id))
                .ToDictionaryAsync(yk => yk.Id); // IDは一意制約のため安定
            if (checkedYukyuKeikakuIds.Count != yukyuKeikakus.Count)
            {
                ModelState.AddModelError("", ErrorConflictReloadYukyuKeikaku);
                return;
            }

            // 計画有給休暇情報の更新
            foreach (var viewModelKeikaku in checkedYukyuKeikakus)
            {
                var yk = yukyuKeikakus[viewModelKeikaku.Id!.Value];
                yk.Status = newStatus;
                db.SetOriginalValue(yk, yk => yk.Version, viewModelKeikaku.Version);
            }
            await SaveWithConcurrencyCheckAsync(ErrorConflictReloadYukyuKeikaku);
        }
    }
}
