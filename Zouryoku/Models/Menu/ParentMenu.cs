using Zouryoku.Enums;
using Zouryoku.Extensions;

namespace Zouryoku.Models.Menu
{
    /// <summary>
    /// メニューの親要素
    /// </summary>
    /// <param name="displayOwnName">自身に表示するタイトル</param>
    /// <param name="hasChild">紐づく子要素を設定するかどうか</param>
    /// <param name="ownIconClass">親要素のアイコンクラス名</param>
    /// <param name="ownMenuCode">自身のメニューコード</param>
    public class ParentMenu(string displayOwnName, bool hasChild, string ownIconClass, MenuCode? ownMenuCode = null)
    {
        /// <summary>
        /// 自身に表示するタイトル
        /// メニューコードが設定されている場合はそちらのタイトルを優先する
        /// </summary>
        public string DisplayOwnName { get; }
            = ownMenuCode is null ? displayOwnName : ownMenuCode.Value.GetTitle();

        /// <summary>
        /// 紐づく子要素を設定するかどうか
        /// true: 子要素あり, false: 子要素なし
        /// </summary>
        public bool HasChild { get; } = hasChild;

        /// <summary>
        /// アイコンのクラス名
        /// </summary>
        public string OwnIconClass { get; } = ownIconClass;

        /// <summary>
        /// 自身のメニューコード
        /// </summary>
        public MenuCode? OwnMenuCode { get; } = ownMenuCode;

        /// <summary>
        /// 紐づいている子要素一覧
        /// HasChildがtrueの場合に使用される
        /// </summary>
        public List<ChildMenu> ChildMenus { get; private set; } = [];

        /// <summary>
        /// 自身の遷移先URL
        /// HasChildがfalseの場合に使用される
        /// メニューコードに基づいてURLを取得する
        /// </summary>
        public string OwnUrl => OwnMenuCode?.GetUrl() ?? "/";

        /// <summary>
        /// 子要素を追加し、親要素のインスタンスを返すメソッドチェーン用のメソッド
        /// 画面に表示する一覧を構築する際に使用する
        /// </summary>
        /// <param name="menuCode">子要素のメニューコード</param>
        /// <param name="iconClass">子要素のアイコンCSSクラス</param>
        /// <param name="isOpneModal">子要素をモーダルで開くかどうか</param>
        /// <param name="param">子要素の画面遷移時に必要な引数</param>
        /// <returns>子要素を追加した親要素のインスタンス</returns>
        public ParentMenu AddChildMenu(MenuCode menuCode, string iconClass, bool isOpneModal, IDictionary<string, string>? param = null)
        {
            ChildMenus.Add(new(menuCode, iconClass, isOpneModal, param));
            return this;
        }

        /// <summary>
        /// 子要素一覧を変更し、親要素のインスタンスを返すメソッド
        /// 構築した一覧を一括で変更したい場合に使用する
        /// </summary>
        /// <param name="childMenus">変更後の子要素一覧</param>
        /// <returns>子要素一覧を変更した親要素のインスタンス</returns>
        public ParentMenu ChangeChildMenus(List<ChildMenu> childMenus)
        {
            ChildMenus = childMenus;
            return this;
        }
    }
}
