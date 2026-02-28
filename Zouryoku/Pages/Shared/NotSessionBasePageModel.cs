using ZouryokuCommonLibrary.Utils;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ZouryokuCommonLibrary.Utils.ResponseMesages;
using static LanguageExt.Prelude;
using Model.Data;
using Microsoft.Extensions.Options;
using Model.Enums;

namespace Zouryoku.Pages.Shared
{
    public abstract class NotSessionBasePageModel<MySelf> : PageModel
    {
        protected readonly ZouContext db;
        protected readonly ILogger<MySelf> logger;
        protected readonly AppSettings appSettings;
        protected readonly TimeProvider timeProvider;

        protected NotSessionBasePageModel(ZouContext db, ILogger<MySelf> logger, IOptions<AppConfig> optionsAccessor, TimeProvider? timeProvider = null)
        {
            this.db = db;
            this.logger = logger;
            this.appSettings = optionsAccessor.Value.AppSettings;
            // TimeProviderはテスト用に注入可能とする（指定がない場合はシステムの現在時刻を使用）
            this.timeProvider = timeProvider ?? TimeProvider.System;
        }

        public static Exception GetInnerException(Exception ex)
        {
            Exception currentEx = ex;
            while (currentEx.InnerException != null)
            {
                currentEx = currentEx.InnerException;
            }
            return currentEx;
        }

        public static string GetControlId(params string[] keys)
            => "#" + keys.Aggregate((a, b) => a + "_" + b);

        public IActionResult Success(string? message = null)
            => StatusCode(200, SuccessMessage(message));

        /// <summary>
        /// 成功レスポンスをJSON形式で返却
        /// </summary>
        /// <param name="message">メッセージ（省略可）</param>
        /// <param name="data">追加データ（省略可）</param>
        /// <returns>ResponseJson形式のJSONレスポンス</returns>
        /// <remarks>
        /// このメソッドは、common.jsで定義されているレスポンスステータス定数を使用する
        /// JavaScript関数（sendAjax、fileSendAjax、fileSendRegisterAjax等）から呼び出される
        /// サーバー側のハンドラーメソッドで使用します。
        /// レスポンスのStatusプロパティは、ResponseStatus.正常（値：1）に設定されます。
        /// </remarks>
        public JsonResult SuccessJson(string? message = null, object? data = null)
            => new JsonResult(new ResponseJson
            {
                Status = ResponseStatus.正常,
                Message = message,
                Data = data
            });

        /// <summary>
        /// 警告レスポンスをJSON形式で返却
        /// </summary>
        /// <param name="message">警告メッセージ</param>
        /// <param name="data">追加データ（省略可）</param>
        /// <returns>ResponseJson形式のJSONレスポンス</returns>
        /// <remarks>
        /// このメソッドは、common.jsで定義されているレスポンスステータス定数を使用する
        /// JavaScript関数（sendAjax、fileSendAjax、fileSendRegisterAjax等）から呼び出される
        /// サーバー側のハンドラーメソッドで使用します。
        /// レスポンスのStatusプロパティは、ResponseStatus.警告（値：2）に設定され、
        /// 確認ダイアログにて警告メッセージが表示されます。
        /// </remarks>
        public JsonResult WarningJson(string? message = null, object? data = null)
            => new JsonResult(new ResponseJson
            {
                Status = ResponseStatus.警告,
                Message = message,
                Data = data
            });

        public IActionResult Error(string message)
        {
            logger.LogError(message);
            return StatusCode(200, ErrorMessage(message));
        }

        /// <summary>
        /// エラーレスポンスをJSON形式で返却（ログも記録）
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        /// <param name="data">追加データ（省略可）</param>
        /// <returns>ResponseJson形式のJSONレスポンス</returns>
        /// <remarks>
        /// このメソッドは、common.jsで定義されているレスポンスステータス定数を使用する
        /// JavaScript関数（sendAjax、fileSendAjax、fileSendRegisterAjax等）から呼び出される
        /// サーバー側のハンドラーメソッドで使用します。
        /// レスポンスのStatusプロパティは、ResponseStatus.エラー（値：3）に設定され、
        /// エラー内容はログにも記録されます。
        /// </remarks>
        public JsonResult ErrorJson(string message, object? data = null)
        {
            logger.LogError(message);
            return new JsonResult(new ResponseJson
            {
                Status = ResponseStatus.エラー,
                Message = message,
                Data = data
            });
        }

        public IActionResult Error(List<string> message)
        {
            var mes = string.Empty;
            if (message.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                mes = message.Where(x => !string.IsNullOrWhiteSpace(x)).Aggregate((a, b) => a + Environment.NewLine + b);
            }
            logger.LogError(mes);
            return StatusCode(200, ErrorMessage(mes));
        }

        public IActionResult Error(List<ResponseModel> models)
        {
            var mes = string.Empty;
            if (models.Any(x => x.IsError || x.IsWarning))
            {
                mes = models.Select(x => x.Message).Aggregate((a, b) => a + Environment.NewLine + b);
            }
            logger.LogError(mes);
            return StatusCode(200, ErrorMessage(models));
        }

        public IActionResult Warning(string message)
        {
            logger.LogWarning(message);
            return StatusCode(200, WarningMessage(message));
        }

        public IActionResult Warning(List<string> message)
        {
            var mes = string.Empty;
            if (message.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                mes = message.Where(x => !string.IsNullOrWhiteSpace(x)).Aggregate((a, b) => a + Environment.NewLine + b);
            }
            logger.LogWarning(mes);
            return StatusCode(200, WarningMessage(mes));
        }

        public IActionResult Warning(List<ResponseModel> models)
        {
            var mes = string.Empty;
            if (models.Any(x => x.IsError || x.IsWarning))
            {
                mes = models.Select(x => x.Message).Aggregate((a, b) => a + Environment.NewLine + b);
            }
            logger.LogWarning(mes);
            return StatusCode(200, WarningMessage(models));
        }
    }
    
    public class ResponseJson
    {
        public ResponseStatus Status { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }
}
