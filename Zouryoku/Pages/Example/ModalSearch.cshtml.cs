using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using ZouryokuCommonLibrary;

namespace Zouryoku.Pages.Example
{
    /// <summary>
    /// 画面UI方針サンプル - 検索画面（モーダル）
    /// </summary>
    [FunctionAuthorization]
    public partial class ModalSearchModel : BasePageModel<ModalRegisterModel>
    {
        // ---------------------------------------------
        // 1. 定数
        // ---------------------------------------------
        // なし



        // ---------------------------------------------
        // 2. DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        public ModalSearchModel(
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
            var data = await PartialToJsonAsync("_ModalSearchPartial", viewModel);
            return SuccessWithData(data);
        }



        //// ---------------------------------------------
        //// 6. OnPost
        //// ---------------------------------------------
        /// なし



        // ---------------------------------------------
        // 7. private メソッド
        // ---------------------------------------------
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
