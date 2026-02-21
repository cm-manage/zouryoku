using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Model;
using System.Collections.Immutable;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using ZouryokuCommonLibrary;
using ZouryokuCommonLibrary.Utils;

namespace Zouryoku.Pages.Example
{
    /// <summary>
    /// 画面UI方針サンプル
    /// </summary>
    [FunctionAuthorization]
    public partial class IndexModel : BasePageModel<IndexModel>
    {
        // ---------------------------------------------
        // 1. 定数
        // ---------------------------------------------
        public static int TimePerDay => 3;

        private const string ClosingDateNotConfirmedErrorMessage = "締め日確定後に実施してください。";

        private static readonly ImmutableArray<string> AutocompleteExamples =
        [
            "山田商店",
            "山田ホールディングス",
            "山田ソリューションズ",
        ];



        // ---------------------------------------------
        // 2. DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        public IndexModel(
            ZouContext context, ILogger<IndexModel> logger, IOptions<AppConfig> options, ICompositeViewEngine viewEngine)
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
        public override bool UseInputAssets => true;

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
            // ダミーデータ作成
            var dummySyain = CreateDummySyain();


            // 1 検索結果情報を取得 /////////////////////////////////////////////////////////////////////////

            // 1-1 検索結果の取得
            var days = dummySyain.Nippous
                .Select(n => new
                {
                    Syain = dummySyain,
                    Nippou = n,
                    WorkingHours = dummySyain.WorkingHourSyains.Where(wh => wh.Hiduke == n.NippouYmd),
                    PcLogs = dummySyain.PcLogs.Where(pl => pl.Datetime.ToDateOnly() == n.NippouYmd),
                    dummySyain.UkagaiHeaders.FirstOrDefault(uh => uh.ShinseiYmd == n.NippouYmd, new UkagaiHeader()).UkagaiShinseis
                })
                .Select(o => new Day
                {
                    Name = o.Syain.Name,
                    Date = o.Nippou.NippouYmd,
                    SyukkinKubun1 = o.Nippou.SyukkinKubunId1Navigation.Name,
                    SyukkinKubun1IsHoliday = o.Nippou.SyukkinKubunId1Navigation.IsHoliday,
                    WorkingHours = [.. o.WorkingHours.Select(wh => new DayWorkingHour
                    {
                        SyukkinTime = wh.SyukkinTime,
                        TaikinTime = wh.TaikinTime,
                        SyukkinHasLocation = wh.UkagaiHeader != null,
                        TaikinHasLocation = wh.UkagaiHeader != null,
                    })],
                    Nippous =
                    [
                        new DayNippou
                        {
                            SyukkinHm = o.Nippou.SyukkinHm1,
                            TaisyutsuHm = o.Nippou.TaisyutsuHm1,
                            SyukkinHasLocation = false,
                            TaisyutsuHasLocation = false
                        },
                        new DayNippou
                        {
                            SyukkinHm = o.Nippou.SyukkinHm2,
                            TaisyutsuHm = o.Nippou.TaisyutsuHm2,
                            SyukkinHasLocation = false,
                            TaisyutsuHasLocation = false
                        },
                        new DayNippou
                        {
                            SyukkinHm = o.Nippou.SyukkinHm3,
                            TaisyutsuHm = o.Nippou.TaisyutsuHm3,
                            SyukkinHasLocation = false,
                            TaisyutsuHasLocation = false
                        },
                    ],
                    PcLogs = [.. o.PcLogs
                        .Where(pl => pl.Operation == PcOperationType.ログオン)
                        .Select(logOn => new
                        {
                            LogOn = logOn,
                            LogOff = o.PcLogs
                                .Where(pl => pl.Operation == PcOperationType.ログオフ)
                                .Where(pl => pl.Datetime.Date == logOn.Datetime.Date && pl.PcName == logOn.PcName)
                                .Where(pl => pl.Datetime > logOn.Datetime)
                                .OrderBy(pl => pl.Datetime)
                                .FirstOrDefault()
                        })
                        .Select(o => new DayPcLog
                        {
                            PcName = o.LogOn.PcName,
                            LogOnDateTime = o.LogOn?.Datetime,
                            LogOffDateTime = o.LogOff?.Datetime
                        })],
                    UkagaiSyubetsu = o.UkagaiShinseis.Select(us => (InquiryType?)us.UkagaiSyubetsu).FirstOrDefault()
                })
                .ToList();

            viewModel = viewModel with
            {
                Days = days
            };

            // （テスト用データ設定）
            TestViewModel = viewModel;


            // 2 画面を表示 ///////////////////////////////////////////////////////////////////////////////
            var data = await PartialToJsonAsync("_IndexPartial", viewModel);
            return SuccessWithData(data);
        }

        /// <summary>
        /// オートコンプリートサンプル用ハンドラ
        /// </summary>
        public async Task<IActionResult> OnGetAutocompleteAsync(string term)
        {
            var result = AutocompleteExamples.Where(s => s.Contains(term));

            return new JsonResult(result);
        }



        // ---------------------------------------------
        // 6. OnPost
        // ---------------------------------------------
        /// <summary>
        /// エラーメッセージプレビュー
        /// </summary>
        public async Task<IActionResult> OnPostPreviewErrorAsync(ViewModel viewModel)
        {
            // 1 登録前チェック /////////////////////////////////////////////////////////////////////////////

            // 〇〇を確認する。（サンプル）
            if (string.IsNullOrWhiteSpace(viewModel.TextBox) || true)
            {
                ModelState.AddModelError("TextBox", ClosingDateNotConfirmedErrorMessage);
            }
            if (!ModelState.IsValid) return CommonErrorResponse();


            // 2 完了メッセージを表示（ビューで処理） /////////////////////////////////////////////////
            // 3 画面を表示（ビューで処理） ///////////////////////////////////////////////////////////
            return Success();
        }

        /// <summary>
        /// プレビュー用POSTメソッド
        /// </summary>
        public async Task<IActionResult> OnPostPreviewSuccessAsync(ViewModel viewModel)
        {
            // 1 登録前チェック /////////////////////////////////////////////////////////////////////////////

            // チェックなし


            // 2 完了メッセージを表示（ビューで処理） /////////////////////////////////////////////////
            // 3 画面を表示（ビューで処理） ///////////////////////////////////////////////////////////
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
