using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 伺い入力ヘッダ：ステータス
    /// </summary>
    public enum ApprovalStatus : short
    {
        [Display(Name = "承認待")]
        承認待 = 0,
        [Display(Name = "承認")]
        承認 = 1,
        [Display(Name = "差戻")]
        差戻 = 2,
    }
}
