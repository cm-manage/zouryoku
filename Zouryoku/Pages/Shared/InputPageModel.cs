using CommonLibrary.Extensions;
using Zouryoku.Pages.Utils;
using ZouryokuCommonLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Web;
using static System.Web.HttpUtility;
using Model.Data;

namespace Zouryoku.Pages.Shared
{
    public abstract class InputPageModel<MySelf> : BasePageModel<MySelf>
    {
        protected InputPageModel(ZouContext db, ILogger<MySelf> logger, IOptions<AppConfig> optionsAccessor, TimeProvider? timeProvider = null) : base(db, logger, optionsAccessor, timeProvider) { }

        /// <summary>
        /// 現在のURLを返します
        /// </summary>
        public abstract string URL();

        /// <summary>
        /// リダイレクト時にJavaScript実行用
        /// </summary>
        public bool IsCompleted { get; set; } = false;
        /// <summary>
        /// リダイレクト時にJavaScript実行用
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// エラーメッセージ表示用
        /// </summary>
        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="result">登録データ</param>
        /// <param name="action">登録処理(引数：RegisterParam)</param>
        public IActionResult RegisterAction(Func<RegisterParam> action)
        {
            var idParamUrl = string.Empty;
            var isUpdate = false;
            var registerParam = new RegisterParam();

            try
            {
                // register() から newParam を取得し、isUpdate を編集
                registerParam = action();
                isUpdate = !string.IsNullOrEmpty(registerParam.Id);

                var q = GetParamUrl(registerParam);
                idParamUrl = URL() + "?" + q;
            }
            catch (DbUpdateConcurrencyException e)
            {
                logger.LogError(e, "");
                logger.LogError($"(id={registerParam.Id})が見つかりませんでした。");
                return Redirect(ShowErrorAndCloseIziModalModel.AlredyDeletedURL);
            }
            catch (TimeoutException e)
            {
                logger.LogError(e, "");
                return Error("タイムアウトエラーです。更新行のロックが設定時間内に解除されませんでした。");
            }
            catch (Exception e)
            {
                logger.LogError(e, "エラー処理済み");
                if (isUpdate)
                {
                    return RedirectParamMessage(idParamUrl, GetInnerException(e).Message);
                }
                else
                {
                    return Error(GetInnerException(e).Message);
                }
            }
            return Redirect(idParamUrl);
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="result">登録データ</param>
        /// <param name="action">登録処理(引数：RegisterParam)</param>
        public IActionResult RegisterAction(Action action)
        {
            var idParamUrl = string.Empty;

            try
            {
                action();
                var q = GetParamUrl(new RegisterParam()
                {
                    IsCompleted = true,
                });
                idParamUrl = URL() + "?" + q;
            }
            catch (TimeoutException e)
            {
                return Error("タイムアウトエラーです。更新行のロックが設定時間内に解除されませんでした。:" + GetInnerException(e).Message);
            }
            catch (Exception e)
            {
                return Error(GetInnerException(e).Message);
            }
            return Redirect(idParamUrl);
        }

        public RedirectResult RedirectParamMessage(string url, string message)
        {
            return Redirect(url + "&message=" + UrlEncode(message));
        }

        private static string? GetParamUrl(RegisterParam registerParam)
        {
            var q = ParseQueryString("");
            if (registerParam != null)
            {
                q.Add("IsCompleted", registerParam.IsCompleted ? "true" : "false");
                q.Add("IsDeleted", registerParam.IsDeleted ? "true" : "false");
                if (!string.IsNullOrWhiteSpace(registerParam.Id))
                {
                    q.Add("Id", registerParam.Id);
                }
                if (registerParam.QueryParam.NotEmpty())
                {
                    registerParam.QueryParam.ForEach(x =>
                    {
                        q.Add(x.Key, x.Value);
                    });
                }
            }
            return q.ToString();
        }
    }

    public class RegisterParam
    {
        public Dictionary<string, string>? QueryParam { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        /// <remarks>IDを使った共通処理をするために使用</remarks>
        public string? Id { get; set; }

        /// <summary>
        /// 削除処理か?
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 正常終了か?
        /// </summary>
        public bool IsCompleted { get; set; } = true;

        /// <summary>
        /// メッセージ
        /// </summary>
        public string? Message { get; set; }
    }
}
