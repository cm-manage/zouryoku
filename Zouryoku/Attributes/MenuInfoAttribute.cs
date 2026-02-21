using Model.Enums;

namespace Zouryoku.Attributes
{
    /// <summary>
    /// メニュー情報
    /// 画面表示に必要な情報を属性として定義
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MenuInfoAttribute : Attribute
    {
        /// <summary>
        /// タイトル
        /// メニューに表示される文字列
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// URL
        /// OnGetのパラメータは含まない
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// 権限
        /// 画面にアクセスするために必要な権限
        /// 社員.権限.HasFlag(権限)で判定
        /// </summary>
        public EmployeeAuthority Kengen { get; }

        /// <summary>
        /// PCで表示可能か
        /// true:表示可能、false:表示不可
        /// </summary>
        public bool CanDisplayPc { get; }

        /// <summary>
        /// Mobileで表示可能か
        /// true:表示可能、false:表示不可
        /// </summary>
        public bool CanDisplayMobile { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MenuInfoAttribute(
            string title = "",
            string url = "/",
            EmployeeAuthority kengen = EmployeeAuthority.None,
            bool canDisplayPc = true,
            bool canDisplayMobile = true)
        {
            Title = title;
            Url = url;
            Kengen = kengen;
            CanDisplayPc = canDisplayPc;
            CanDisplayMobile = canDisplayMobile;
        }
    }
}
