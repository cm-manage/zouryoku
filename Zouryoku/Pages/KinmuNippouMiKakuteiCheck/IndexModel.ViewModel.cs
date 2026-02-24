using CommonLibrary.Extensions;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using static Model.Enums.DailyReportStatusClassification;

namespace Zouryoku.Pages.KinmuNippouMiKakuteiCheck
{
    public partial class IndexModel
    {
        /// <summary>
        /// 日報情報表示用のビューモデル
        /// </summary>
        /// <param name="syain">社員マスタのエンティティ</param>
        public class MikakuteiSyainViewModel(Syain syain)
        {
            private readonly Syain _syain = syain;

            /// <value>部署名</value>
            [Display(Name = "部署")]
            public string BusyoName => _syain.Busyo.Name;

            /// <value>社員番号</value>
            [Display(Name = "社員番号")]
            public string SyainCode => _syain.Code;

            /// <value>社員名</value>
            [Display(Name = "社員氏名")]
            public string SyainName => _syain.Name;

            /// <value>社員BaseID</value>
            public long SyainBaseId => _syain.SyainBase.Id;

            /// <value>確定日</value>
            [Display(Name = "最終確定日")]
            [DisplayFormat(NullDisplayText = "-")]
            public DateOnly? LastKakuteiYmd { get; } =
                syain.Nippous.Any(n => n.TourokuKubun == 確定保存) ?
                    syain.Nippous.Where(n => n.TourokuKubun == 確定保存).Max(n => n.NippouYmd) : null;
        }

        /// <summary>
        /// 検索条件用のバインドモデル
        /// </summary>
        public class NippouSearchViewModel
        {
            /// <value>部署の検索条件</value>
            [Display(Name = "部署")]
            public BusyoCondition Busyo { get; set; } = new();

            /// <value>日付</value>
            [Display(Name = "日付")]
            [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
            [Required]
            public DateOnly Date { get; set; }

            /// <summary>
            /// 部署に関する条件を格納するクラス
            /// </summary>
            public class BusyoCondition
            {
                /// <value>検索範囲</value>
                public BusyoRange Range { get; set; }

                /// <value>部署名</value>
                public string Name { get; set; } = string.Empty;

                /// <value>部署ID</value>
                public long? Id { get; set; }
            }
        }
    }
}
