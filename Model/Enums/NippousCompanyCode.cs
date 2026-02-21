using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 日報実績：会社コード
    /// </summary>
    public enum NippousCompanyCode : short
    {
        [Display(Name = "協和")]
        協和 = 1,
        [Display(Name = "KBS")]
        KBS = 2,
    }
}
