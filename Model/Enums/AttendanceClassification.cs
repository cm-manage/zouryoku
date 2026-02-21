using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 出勤区分
    /// </summary>
    public enum AttendanceClassification
    {
        [Display(Name = "None")]
        None = 0,
        [Display(Name = "休日")]
        休日 = 1,
        [Display(Name = "通常勤務")]
        通常勤務 = 2,
        [Display(Name = "休日出勤")]
        休日出勤 = 3,
        [Display(Name = "年次有給休暇（1日）")]
        年次有給休暇_1日 = 4,
        [Display(Name = "半日有給")]
        半日有給 = 5,
        [Display(Name = "振替休暇")]
        振替休暇 = 6,
        [Display(Name = "半日振休")]
        半日振休 = 7,
        [Display(Name = "結婚（本人）")]
        結婚_本人 = 8,
        [Display(Name = "忌引（父母・配偶者）")]
        忌引_父母_配偶者 = 9,
        [Display(Name = "忌引（兄弟・配偶者の父母）")]
        忌引_兄弟_配偶者の父母 = 10,
        [Display(Name = "忌引（孫・祖父母・その他）")]
        忌引_孫_祖父母_その他 = 11,
        [Display(Name = "非常勤休暇")]
        非常勤休暇 = 12,
        [Display(Name = "生理休暇")]
        生理休暇 = 13,
        [Display(Name = "罹災休暇")]
        罹災休暇 = 14,
        [Display(Name = "出産休暇（妻）")]
        出産休暇_妻 = 15,
        [Display(Name = "出産休業")]
        出産休業 = 16,
        [Display(Name = "育児休業")]
        育児休業 = 17,
        [Display(Name = "業務上傷病休業")]
        業務上傷病休業 = 18,
        [Display(Name = "介護休業")]
        介護休業 = 19,
        [Display(Name = "欠勤")]
        欠勤 = 20,
        [Display(Name = "その他特別休暇")]
        その他特別休暇 = 21,
        [Display(Name = "転勤休暇")]
        転勤休暇 = 22,
        [Display(Name = "勤続30年リフレッシュ休暇")]
        勤続30年リフレッシュ休暇 = 23,
        [Display(Name = "半日勤務")]
        半日勤務 = 24,
        [Display(Name = "パート勤務")]
        パート勤務 = 25,
        [Display(Name = "計画有給休暇")]
        計画有給休暇 = 26,
        [Display(Name = "計画特別休暇")]
        計画特別休暇 = 27,
    }
}
