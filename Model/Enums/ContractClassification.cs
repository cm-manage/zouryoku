using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 契約状態区分
    /// </summary>
    public enum ContractClassification : int
    {
        [Display(Name = "経費")]
        経費 = 0,
        [Display(Name = "仮受注_自営")]
        仮受注_自営 = 30,
        [Display(Name = "仮受注_共同")]
        仮受注_共同 = 40,
        [Display(Name = "受注_自営")]
        受注_自営 = 50,
        [Display(Name = "受注_共同")]
        受注_共同 = 60,
        [Display(Name = "受注_社内取引")]
        受注_社内取引 = 70,
        [Display(Name = "取消")]
        取消 = 99,
    }
}
