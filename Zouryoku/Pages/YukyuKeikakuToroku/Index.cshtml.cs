using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Extensions;
using Model.Model;
using System.Collections.Immutable;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Model.Enums.LeavePlanStatus;

namespace Zouryoku.Pages.YukyuKeikakuToroku
{
    /// <summary>
    /// 計画有給休暇登録画面
    /// </summary>
    [FunctionAuthorization]
    public partial class IndexModel : BasePageModel<IndexModel>
    {
        // ---------------------------------------------
        // 1. 定数
        // ---------------------------------------------
        // DataAnnotations の Length 属性では static プロパティが使用できないため、private const で定義し、
        // それを public static プロパティで公開する。
        private const int ConstRequiredYukyuKeikakuMeisaiCount = 7;
        public static int RequiredYukyuKeikakuMeisaiCount => ConstRequiredYukyuKeikakuMeisaiCount;
        public static int RequiredTokukyuCount => 2;

        public static string ErrorConflictYukyuKeikaku { get; } = string.Format(Const.ErrorConflict, "計画有給休暇");

        // ---------------------------------------------
        // 2. DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        public IndexModel(ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> options)
            : base(db, logger, options)
        {
        }

        // ---------------------------------------------
        // 3. 通常のプロパティ（画面表示用）
        // ---------------------------------------------
        public override bool UseInputAssets => true;

        /// <summary>
        /// ログインユーザーの今年度の有給休暇計画モデルを取得または設定します。
        /// </summary>
        public YukyuKeikakuViewModel LoginUsersYukyuKeikaku { get; private set; } = YukyuKeikakuViewModel.Empty;

        // ---------------------------------------------
        // 4. OnGet
        // ---------------------------------------------
        /// <summary>
        /// イベント仕様_初期処理
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            LoginUsersYukyuKeikaku = await GetLoginUsersYukyuKeikakuForThisYearViewModelAsync();
            return Page();
        }

        // ---------------------------------------------
        // 5. OnPost
        // ---------------------------------------------
        /// <summary>
        /// イベント仕様_申請（ボタン押下）
        /// イベント仕様_再申請（ボタン押下）
        /// </summary>
        public async Task<IActionResult> OnPostRegisterAsync(YukyuKeikakuViewModel loginUsersYukyuKeikaku)
        {
            // 登録前チェック
            // 単項目チェック
            JsonResult? errorJson = ModelState.ErrorJson();
            if (errorJson is not null) return errorJson;

            // 複合項目チェック
            ValidateTokukyuCountIsExactly2(loginUsersYukyuKeikaku);
            ValidateYmdNoDuplicate(loginUsersYukyuKeikaku);
            if (!ModelState.IsValid) return CommonErrorResponse();

            // 登録・更新条件の設定
            var newStatus = 事業部承認待ち;

            // 登録・更新処理の前に最新のレコードを取得することで、楽観的排他制御による同時実行の競合を検知する
            var yukyuKeikaku = await LoginUsersYukyuKeikakuForThisYear.Include(yk => yk.YukyuKeikakuMeisais).SingleOrDefaultAsync();

            // 同時実行制御
            // 取得時と登録時でレコードの有無が不整合な場合は同時実行エラーとする
            if (!IsConsistentWithExistingData(yukyuKeikaku, loginUsersYukyuKeikaku))
                return CommonErrorResponseWithMessage(ErrorConflictYukyuKeikaku);

            // 計画有給休暇情報の登録・更新
            if (yukyuKeikaku is null)
            {
                await InsertNewYukyuKeikaku(loginUsersYukyuKeikaku, newStatus);
            }
            else
            {
                UpdateExistingYukyuKeikaku(yukyuKeikaku, loginUsersYukyuKeikaku, newStatus);
                UpdateExistingYukyuKeikakuMeisais(yukyuKeikaku.YukyuKeikakuMeisais, loginUsersYukyuKeikaku);
                if (!ModelState.IsValid) return CommonErrorResponse();
            }

            await SaveWithConcurrencyCheckAsync(ErrorConflictYukyuKeikaku);
            if (!ModelState.IsValid) return CommonErrorResponse();

            // 完了メッセージを表示（ビューで処理）
            // 画面を表示（ビューで処理）
            return Success();
        }

        /// <summary>
        /// イベント仕様_取下げ（ボタン押下）
        /// </summary>
        public async Task<IActionResult> OnPostRevokeAsync(YukyuKeikakuViewModel loginUsersYukyuKeikaku)
        {
            // 取下げ時はバリデーション不要のため、ModelStateをクリアする
            ModelState.Clear();

            // 登録・更新条件の設定
            var newStatus = 未申請;

            // 更新処理の前に最新のレコードを取得することで、楽観的排他制御による同時実行の競合を検知する
            var yukyuKeikaku = await LoginUsersYukyuKeikakuForThisYear.SingleOrDefaultAsync();

            // 同時実行制御
            // 取得時と登録時でレコードの有無が不整合な場合は同時実行エラーとする
            if (yukyuKeikaku is null) return CommonErrorResponseWithMessage(ErrorConflictYukyuKeikaku);

            // 計画有給休暇情報の更新
            UpdateExistingYukyuKeikaku(yukyuKeikaku, loginUsersYukyuKeikaku, newStatus);
            await SaveWithConcurrencyCheckAsync(ErrorConflictYukyuKeikaku);
            if (!ModelState.IsValid) return CommonErrorResponse();

            // 完了メッセージを表示（ビューで処理）
            // 画面を表示（ビューで処理）
            return Success();
        }

        // ---------------------------------------------
        // 6. private メソッド
        // ---------------------------------------------
        /// <summary>
        /// ログインユーザーの今年度の有給休暇計画レコード
        /// </summary>
        private IQueryable<YukyuKeikaku> LoginUsersYukyuKeikakuForThisYear => db.YukyuKeikakus
            .Where(yk => yk.YukyuNendo.IsThisYear) // 今年度分
            .Where(yk => yk.SyainBaseId == LoginInfo.User.SyainBaseId); // ログインユーザのみ

        /// <summary>
        /// 取得時と登録時でレコードの有無が一致しているか確認する。
        /// </summary>
        private static bool IsConsistentWithExistingData(YukyuKeikaku? yukyuKeikaku, YukyuKeikakuViewModel yukyuKeikakuViewModel)
        {
            var recordExists = yukyuKeikaku is not null;
            return recordExists == yukyuKeikakuViewModel.RecordExists;
        }

        /// <summary>
        /// メッセージを指定して共通エラーレスポンスを返す
        /// </summary>
        private IActionResult CommonErrorResponseWithMessage(string message)
        {
            ModelState.AddModelError("", message);
            return CommonErrorResponse();
        }

        // ---------------------------------------------
        // ViewModel 生成メソッド
        // ---------------------------------------------
        private async Task<YukyuKeikakuViewModel> GetLoginUsersYukyuKeikakuForThisYearViewModelAsync()
        {
            // 検索結果情報を取得
            // 検索結果の取得を試行
            var yukyuKeikaku = await LoginUsersYukyuKeikakuForThisYear
                .Include(yk => yk.YukyuNendo)
                .Include(yk => yk.YukyuKeikakuMeisais)
                .AsNoTracking()
                .SingleOrDefaultAsync();

            if (yukyuKeikaku is null)
            {
                // 新規情報を作成
                // レコードが存在しない場合は未申請状態のViewModelを作成
                return await CreateNewYukyuKeikakuForThisYearViewModelAsync();
            }

            // 取得した情報からViewModelを作成
            return CreateYukyuKeikakuViewModelFromEntity(yukyuKeikaku);
        }

        private async Task<YukyuKeikakuViewModel> CreateNewYukyuKeikakuForThisYearViewModelAsync()
        {
            // 当年度の初日と最終日を取得
            var yukyuNendo = await db.YukyuNendos.SingleAsync(yn => yn.IsThisYear);

            // 初期状態を設定
            var yukyuKeikakuViewModel = new YukyuKeikakuViewModel
            {
                YukyuKeikakuStatus = null,
                YukyuNendoStartDate = yukyuNendo.StartDate,
                YukyuNendoEndDate = yukyuNendo.EndDate,
                Meisais = Enumerable.Range(0, RequiredYukyuKeikakuMeisaiCount)
                    .Select(_ => new Meisai
                    {
                        Ymd = null,
                        IsTokukyu = false
                    })
                    .ToImmutableArray()
            };
            return yukyuKeikakuViewModel;
        }

        private static YukyuKeikakuViewModel CreateYukyuKeikakuViewModelFromEntity(YukyuKeikaku yukyuKeikaku)
        {
            var yukyuKeikakuViewModel = new YukyuKeikakuViewModel
            {
                YukyuKeikakuStatus = yukyuKeikaku.Status,
                YukyuNendoStartDate = yukyuKeikaku.YukyuNendo.StartDate,
                YukyuNendoEndDate = yukyuKeikaku.YukyuNendo.EndDate,
                Version = yukyuKeikaku.Version,
                Meisais = yukyuKeikaku.YukyuKeikakuMeisais
                    .OrderBy(ykm => ykm.Ymd)
                    .Select(ykm => new Meisai
                    {
                        Id = ykm.Id,
                        Ymd = ykm.Ymd,
                        IsTokukyu = ykm.IsTokukyu,
                        Version = ykm.Version
                    })
                    .ToImmutableArray()
            };
            return yukyuKeikakuViewModel;
        }

        // ---------------------------------------------
        // ViewModel 更新メソッド
        // ---------------------------------------------
        private async Task InsertNewYukyuKeikaku(YukyuKeikakuViewModel yukyuKeikakuViewModel, LeavePlanStatus newStatus)
        {
            var yukyuNendo = await db.YukyuNendos.SingleAsync(yn => yn.IsThisYear);

            // 計画有給休暇情報の登録
            var yukyuKeikaku = new YukyuKeikaku
            {
                YukyuNendo = yukyuNendo,
                SyainBaseId = LoginInfo.User.SyainBaseId,
                Status = newStatus,

                // 計画有給休暇明細情報の登録
                YukyuKeikakuMeisais = yukyuKeikakuViewModel.Meisais
                    .Select(m => new YukyuKeikakuMeisai
                    {
                        Ymd = m.Ymd!.Value, // ModelState.IsValid で null チェック済み。冗長な Null チェックは避ける。
                        IsTokukyu = m.IsTokukyu
                    })
                    .ToImmutableArray()
            };
            db.YukyuKeikakus.Add(yukyuKeikaku);
        }

        private void UpdateExistingYukyuKeikaku(
            YukyuKeikaku yukyuKeikaku, YukyuKeikakuViewModel yukyuKeikakuViewModel, LeavePlanStatus newStatus)
        {
            // 計画有給休暇情報の更新
            yukyuKeikaku.Status = newStatus;
            db.SetOriginalValue(yukyuKeikaku, yk => yk.Version, yukyuKeikakuViewModel.Version);
        }

        private void UpdateExistingYukyuKeikakuMeisais(
            ICollection<YukyuKeikakuMeisai> yukyuKeikakuMeisais, YukyuKeikakuViewModel yukyuKeikakuViewModel)
        {
            // 計画有給休暇明細情報の更新
            foreach (var ykm in yukyuKeikakuMeisais)
            {
                var viewModelMeisai = yukyuKeikakuViewModel.Meisais.SingleOrDefault(m => m.Id == ykm.Id);
                if (viewModelMeisai is null)
                {
                    // 楽観排他制御
                    ModelState.AddModelError("", ErrorConflictYukyuKeikaku);
                    return;
                }

                ykm.Ymd = viewModelMeisai.Ymd!.Value; // ModelState.IsValid で null チェック済み。冗長な Null チェックは避ける。
                ykm.IsTokukyu = viewModelMeisai.IsTokukyu;
                db.SetOriginalValue(ykm, ykm => ykm.Version, viewModelMeisai.Version);
            }
        }

        // ---------------------------------------------
        // バリデーションメソッド
        // ---------------------------------------------
        /// <summary>
        /// チェック仕様_特別休暇が2日分ちょうどチェックされているか確認する。
        /// </summary>
        private void ValidateTokukyuCountIsExactly2(YukyuKeikakuViewModel yukyuKeikakuViewModel)
        {
            var tokukyuCount = yukyuKeikakuViewModel.Meisais.Count(m => m.IsTokukyu);
            if (tokukyuCount != RequiredTokukyuCount)
            {
                ModelState.AddModelError(nameof(yukyuKeikakuViewModel.Meisais), Const.ErrorThereAreNotExactly2Tokukyus);
            }
        }

        /// <summary>
        /// チェック仕様_休暇予定日に同じ日付が入力されていないか確認する。
        /// </summary>
        private void ValidateYmdNoDuplicate(YukyuKeikakuViewModel yukyuKeikakuViewModel)
        {
            var duplicateExists = yukyuKeikakuViewModel.Meisais
                .Where(m => m.Ymd is not null)
                .GroupBy(m => m.Ymd)
                .Any(g => 1 < g.Count());

            if (duplicateExists)
            {
                ModelState.AddModelError(nameof(yukyuKeikakuViewModel.Meisais), Const.ErrorYmdDuplicate);
            }
        }
    }
}
