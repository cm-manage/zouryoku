using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// サービス実行,サービス実行履歴：区分
    /// </summary>
    public enum ServiceClassification : int
    {
        [Display(Name = "連携プログラム稼働")]
        連携プログラム稼働 = 1,
        [Display(Name = "過労運転防止")]
        過労運転防止 = 2,
        [Display(Name = "有給未取得アラート")]
        有給未取得アラート = 3,
        [Display(Name = "チャット連携")]
        チャット連携 = 4,
    }
}
