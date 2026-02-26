using Microsoft.AspNetCore.Mvc;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Enums;
using Zouryoku.Extensions;
using Zouryoku.Services;
using Zouryoku.Utils;

using static Zouryoku.Enums.DeviceType;

namespace Zouryoku.Components.ViewComponents
{
    /// <summary>
    /// メニュー表示用ViewComponent
    /// </summary>
    public class MenuViewComponent : ViewComponent
    {
        // ---------------------------------------------
        // DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        private readonly MenuService _menuService;
        public MenuViewComponent(MenuService menuService)
        {
            _menuService = menuService;
        }

        // ---------------------------------------------
        // プライベートプロパティ
        // ---------------------------------------------
        /// <summary>
        /// ログイン情報
        /// </summary>
        private LoginInfo LoginInfo => HttpContext.Session.LoginInfo();

        /// <summary>
        /// デバイスタイプ
        /// </summary>
        private DeviceType DeviceType
        {
            get
            {
                string device = LoginUtil.GetBrowserAndDevice(HttpContext.Request).Device;
                return device is "Windows " or "Mac OS" ? PC : MOBILE;
            }
        }

        // ---------------------------------------------
        // InvokeAsync
        // ---------------------------------------------
        public Task<IViewComponentResult> InvokeAsync()
        {
            // ログインユーザー情報を取得
            Syain loginUser = LoginInfo.User;

            // メニュー情報を取得してビューに渡す
            // Component.InvokeAsyncはTask<IViewComponentResult>を返却する必要があるため、
            // 同期処理結果を即座に返却する
            return Task.FromResult<IViewComponentResult>
                (View(_menuService.FilterMenus(_menuService.CreateMenu(loginUser), loginUser, DeviceType)));
        }
    }
}
