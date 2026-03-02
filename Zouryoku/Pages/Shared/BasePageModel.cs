using CommonLibrary.Extensions;
using CommonLibrary.Utils;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Data;
using Zouryoku.Extensions;
using ZouryokuCommonLibrary.Utils;

namespace Zouryoku.Pages.Shared
{
    public abstract class BasePageModel<MySelf> : NotSessionBasePageModel<MySelf>
    {
        // TODO 文字列のキーの使い方がよくわからん
        protected const string SelectedKey = "SelectedKey";
        protected const string MultiSelectedKey = "MultiSelectedKey";
        protected const string MultiSelectableKey = "MultiSelectableKey";
        protected readonly StopwatchUtil<MySelf> stopwatchUtil; 
        protected readonly ICompositeViewEngine? viewEngine;

        protected BasePageModel(ZouContext db, ILogger<MySelf> logger, IOptions<AppConfig> optionsAccessor, TimeProvider? timeProvider = null) : base(db, logger, optionsAccessor, timeProvider)
        {
            stopwatchUtil = new(logger);
        }

        protected BasePageModel(ZouContext db, ILogger<MySelf> logger, IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine, TimeProvider? timeProvider = null) : this(db, logger, optionsAccessor, timeProvider)
        {
            this.viewEngine = viewEngine;
        }

        public LoginInfo LoginInfo => HttpContext.Session.LoginInfo();

        /// <summary>
        /// 入力画面用共通CSS/JSをレイアウトで読み込むかどうかのフラグ
        /// </summary>
        public virtual bool UseInputAssets { get; } = false;

        /// <summary>
        /// 複数選択Keyの保存処理
        /// </summary>
        /// <param name="record"></param>
        /// <remarks>初期処理で使用するために主に使用</remarks>
        public Option<A> GetSelectKey<A>(bool withClear = true)
            => withClear ? HttpContext.Session.GetAndClear<A>(SelectedKey)
                : HttpContext.Session.Get<A>(SelectedKey);

        /// <summary>
        /// 複数選択Keyの保存処理
        /// </summary>
        /// <param name="record"></param>
        /// <remarks>初期処理で使用するために主に使用</remarks>
        public Option<A> GetMultiSelectKeys<A>(bool withClear = true)
            => withClear ? HttpContext.Session.GetAndClear<A>(MultiSelectedKey)
                : HttpContext.Session.Get<A>(MultiSelectedKey);

        /// <summary>
        /// 複数選択可能Keyの取得処理
        /// </summary>
        /// <param name="withClear">trueで取得後セッションクリア</param>
        /// <remarks>初期処理で使用するために主に使用</remarks>
        public Option<A> GetMultiSelectableKeys<A>(bool withClear = true)
            => withClear ? HttpContext.Session.GetAndClear<A>(MultiSelectableKey)
                : HttpContext.Session.Get<A>(MultiSelectableKey);

        /// <summary>
        /// Sessionから対象Keyを保存
        /// </summary>
        /// <param name="keys">対象Key</param>
        public void AddSelectKey(string key)
        {
            HttpContext.Session.Set(key, SelectedKey);
        }

        /// <summary>
        /// Sessionから対象Keyを保存
        /// </summary>
        /// <param name="keys">対象Key</param>
        public void AddMultiSelectKey(List<string> keys)
        {
            var selectedRecords = HttpContext.Session.Get<List<string>>(MultiSelectedKey)
                .Some(x =>
                {
                    x.AddRange(keys);
                    return x.Distinct().ToList();
                })
                .None(() => keys);

            HttpContext.Session.Set(selectedRecords, MultiSelectedKey);
        }

        /// <summary>
        /// Sessionから選択可能Keyを保存
        /// </summary>
        /// <param name="keys">選択可能Key</param>
        public void AddMultiSelectableKey(List<string> keys)
        {
            var selectedRecords = HttpContext.Session.Get<List<string>>(MultiSelectableKey)
                .Some(x =>
                {
                    x.AddRange(keys);
                    return x.Distinct().ToList();
                })
                .None(() => keys);

            HttpContext.Session.Set(selectedRecords, MultiSelectableKey);
        }

        /// <summary>
        /// Sessionから対象Keyを削除
        /// </summary>
        /// <param name="keys">対象Key</param>
        public void RemoveMultiSelectKey(List<string> keys)
        {
            HttpContext.Session.Get<List<string>>(MultiSelectedKey)
                .IfSome(x =>
                {
                    keys.ForEach(y => x.Remove(y));

                    HttpContext.Session.Set(x, MultiSelectedKey);
                });
        }

        /// <summary>
        /// Sessionから選択可能Keyを削除
        /// </summary>
        /// <param name="keys">選択可能Key</param>
        public void RemoveMultiSelectableKey(List<string> keys)
        {
            HttpContext.Session.Get<List<string>>(MultiSelectableKey)
                .IfSome(x =>
                {
                    keys.ForEach(y => x.Remove(y));

                    HttpContext.Session.Set(x, MultiSelectableKey);
                });
        }

        /// <summary>
        /// PiartialをJson文字列として返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<string> PartialToJsonAsync<T>(string viewName, T model)
        {
            using var sw = new StringWriter();
            var viewResult = viewEngine?.FindView(PageContext, viewName, false);
            if (viewResult?.View == null)
            {
                throw new ArgumentNullException($"{viewName} does not match any available view");
            }
            var viewData = new ViewDataDictionary<T>(ViewData, model);
            var viewContext = new ViewContext(
                PageContext,
                viewResult.View,
                viewData,
                TempData,
                sw,
                new HtmlHelperOptions()
            );
            await viewResult.View.RenderAsync(viewContext);
            return sw.GetStringBuilder().ToString();
        }

        /// <summary>
        /// 排他制御を行うDB保存共通処理
        /// </summary>
        /// <param name="message">画面に表示する排他エラーメッセージ</param>
        public async Task SaveWithConcurrencyCheckAsync(string message)
        {
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // 楽観排他制御
                ModelState.AddModelError(string.Empty, message);
            }
        }
    }
}
