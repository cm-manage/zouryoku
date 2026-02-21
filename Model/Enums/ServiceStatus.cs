using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// サービス実行履歴：ステータス
    /// </summary>
    public enum ServiceStatus : int
    {
        [Display(Name = "正常")]
        正常 = 1,
        [Display(Name = "エラー")]
        エラー = 2,
    }
}
