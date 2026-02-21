using Model.Enums;
using System.Reflection;
using Zouryoku.Attributes;
using Zouryoku.Enums;

namespace Zouryoku.Extensions
{
    public static class MenuCodeExtensions
    {
        /// <summary>
        /// アプリケーション起動時にMenuInfoAttributeを一括取得
        /// MenuInfoAttributeが未定義の値が存在する場合、
        /// 設計不整合として例外を発生させる
        /// </summary>
        private static readonly Dictionary<MenuCode, MenuInfoAttribute> Cache =
        Enum.GetValues<MenuCode>()
            .ToDictionary(
                code => code,
                code => {
                    var attr = typeof(MenuCode)
                        .GetField(code.ToString())!
                        .GetCustomAttribute<MenuInfoAttribute>();

                    return attr ?? throw new InvalidOperationException($"MenuCode.{code}にMenuInfoAttributeが設定されていません。");
                });

        /// <summary>
        ///　MenuInfoAttributeを取得する
        /// </summary>
        public static MenuInfoAttribute GetMenuInfoAttribute(this MenuCode menuCode)
            => Cache[menuCode];

        /// <summary>
        /// MenuInfoAttributeからタイトルを取得する
        /// </summary>
        public static string GetTitle(this MenuCode menuCode)
            => menuCode.GetMenuInfoAttribute().Title;

        /// <summary>
        /// MenuInfoAttributeからURLを取得する
        /// </summary>
        public static string GetUrl(this MenuCode menuCode)
            => menuCode.GetMenuInfoAttribute().Url;

        /// <summary>
        /// MenuInfoAttributeから画面表示に必要な権限を取得する
        /// </summary>
        public static EmployeeAuthority GetKengen(this MenuCode menuCode)
            => menuCode.GetMenuInfoAttribute().Kengen;

        /// <summary>
        /// MenuInfoAttributeからデバイス別の表示可否を取得する
        /// </summary>
        /// <param name="deviceType">表示するデバイスの種類</param>
        public static bool GetCanDisplay(this MenuCode menuCode, DeviceType deviceType)
        {
            return deviceType switch
            {
                DeviceType.PC => menuCode.GetMenuInfoAttribute().CanDisplayPc,
                DeviceType.MOBILE => menuCode.GetMenuInfoAttribute().CanDisplayMobile,
                _ => false,
            };
        }
    }
}
