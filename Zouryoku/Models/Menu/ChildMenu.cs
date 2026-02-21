using Zouryoku.Enums;
using Zouryoku.Extensions;

namespace Zouryoku.Models.Menu
{
    /// <summary>
    /// メニューの子要素
    /// </summary>
    /// <param name="menuCode">子要素のメニューコード</param>
    /// <param name="iconClass">メニューに表示するアイコンのクラス名</param>
    /// <param name="isOpenModal">モーダルで開くかどうか</param>
    /// <param name="param">画面遷移時に必要な引数</param>
    public class ChildMenu(MenuCode menuCode, string iconClass, bool isOpenModal, IDictionary<string, string>? param = null)
    {
        /// <summary>
        /// メニューコード
        /// </summary>
        public MenuCode MenuCode { get; } = menuCode;

        /// <summary>
        /// アイコンのクラス名
        /// </summary>
        public string IconClass { get; } = iconClass;

        /// <summary>
        /// モーダルで開くかどうか
        /// </summary>
        public bool IsOpenModal { get; } = isOpenModal;

        /// <summary>
        /// 画面遷移時に必要な引数
        /// </summary>
        public IDictionary<string, string>? Param { get; } = param;

        /// <summary>
        /// メニューに表示するタイトル
        /// </summary>
        public string DisplayName => MenuCode.GetTitle();

        /// <summary>
        /// 遷移先のURL
        /// </summary>
        public string Url => MenuCode.GetUrl();
    }
}
