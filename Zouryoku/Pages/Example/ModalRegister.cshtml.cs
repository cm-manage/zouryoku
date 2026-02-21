using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using ZouryokuCommonLibrary;
using ZouryokuCommonLibrary.Utils;

namespace Zouryoku.Pages.Example
{
    /// <summary>
    /// 画面UI方針サンプル - 登録画面（モーダル）
    /// </summary>
    [FunctionAuthorization]
    public partial class ModalRegisterModel : BasePageModel<ModalRegisterModel>
    {
        // ---------------------------------------------
        // 1. 定数
        // ---------------------------------------------
        private const string RequiredOneForFiveCardsErrorMessage = "1～5個のカードを登録してください。";



        // ---------------------------------------------
        // 2. DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        public ModalRegisterModel(
            ZouContext context, ILogger<ModalRegisterModel> logger, IOptions<AppConfig> options, ICompositeViewEngine viewEngine)
            : base(context, logger, options, viewEngine)
        {
        }



        // ---------------------------------------------
        // 3. BindProperty（フォームバインド用）
        // ---------------------------------------------
        // なし



        // ---------------------------------------------
        // 4. 通常のプロパティ（画面表示用）
        // ---------------------------------------------
        /// <summary>
        /// 【自動テスト専用】自動テスト専用のプロパティです。実運用では使用しないでください。
        /// </summary>
        public ViewModel? TestViewModel { get; private set; }



        // ---------------------------------------------
        // 5. OnGet
        // ---------------------------------------------
        /// <summary>
        /// イベント仕様_初期処理
        /// </summary>
        public async Task<IActionResult> OnGetSearchAsync(ViewModel viewModel)
        {
            // （テスト用データ設定）
            TestViewModel = viewModel;


            // 2 画面を表示 ///////////////////////////////////////////////////////////////////////////////
            var data = await PartialToJsonAsync("_ModalRegisterPartial", viewModel);
            return SuccessWithData(data);
        }

        /// <summary>
        /// イベント仕様_追加処理
        /// </summary>
        public async Task<IActionResult> OnGetInsertAsync(ViewModel viewModel)
        {
            // カードを追加
            viewModel.Cards.Add(new Card
            {
                Name = $"Card_{viewModel.Cards.Count}"
            });

            // （テスト用データ設定）
            TestViewModel = viewModel;


            // 2 画面を表示 ///////////////////////////////////////////////////////////////////////////////
            var data = await PartialToJsonAsync("_ModalRegisterPartial", viewModel);
            return SuccessWithData(data);
        }

        /// <summary>
        /// イベント仕様_削除処理
        /// </summary>
        public async Task<IActionResult> OnGetDeleteAsync(ViewModel viewModel, int index)
        {
            // カードを削除
            viewModel.Cards.RemoveAt(index);

            // （テスト用データ設定）
            TestViewModel = viewModel;


            // 2 画面を表示 ///////////////////////////////////////////////////////////////////////////////
            var data = await PartialToJsonAsync("_ModalRegisterPartial", viewModel);
            return SuccessWithData(data);
        }



        //// ---------------------------------------------
        //// 6. OnPost
        //// ---------------------------------------------
        public async Task<IActionResult> OnPostRegisterAsync(ViewModel viewModel)
        {
            // 1 登録前チェック /////////////////////////////////////////////////////////////////////////////

            // 1～5個のカードが必要。
            if (viewModel.Cards.Count <= 0 || 6 <= viewModel.Cards.Count)
            {
                ModelState.AddModelError("Cards", RequiredOneForFiveCardsErrorMessage);
            }
            if (!ModelState.IsValid) return CommonErrorResponse();

            // 2 アウトプットを返却 ///////////////////////////////////////////////////////////////////////////////
            return Success();
        }



        // ---------------------------------------------
        // 7. private メソッド
        // ---------------------------------------------
        /// <summary>
        /// この画面での共通のエラーレスポンス構造を返す
        /// </summary>
        private IActionResult CommonErrorResponse() => Error(ModelState.Errors()
            .SelectMany(e => e.Value.Select(v => new ResponseModel(v, e.Key)))
            .ToList());

        private ObjectResult SuccessWithData<T>(T value) => StatusCode(200, new StatusDataPair<T>(ResponseStatus.正常, value));



        // ---------------------------------------------
        // 画面固有の型
        // ---------------------------------------------
        /// <summary>
        /// レスポンス用クラス（匿名型だとテストできないため作成）
        /// </summary>
        public record StatusDataPair<T>(ResponseStatus Status, T Data);
    }
}
