namespace Zouryoku.Pages.Shared
{
    /// <summary>
    /// ページャーに渡すモデル
    /// </summary>
    public class PagerModel
    {
        /// <summary>
        /// ページ番号（0-base）
        /// </summary>
        public required int PageIndex { get; set; }

        /// <summary>
        /// 項目の合計
        /// </summary>
        public required int Total { get; set; }

        /// <summary>
        /// 1ページ当たりの項目数
        /// </summary>
        public required int PageSize { get; set; }

        /// <summary>
        /// ページ数
        /// </summary>
        public int PagesNum => (Total - 1) / PageSize + 1;
    }
}
