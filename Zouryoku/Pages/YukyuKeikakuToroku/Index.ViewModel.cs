using Model.Enums;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using ZouryokuCommonLibrary.Utils;
using static Model.Enums.LeavePlanStatus;

namespace Zouryoku.Pages.YukyuKeikakuToroku
{
    public partial class IndexModel
    {
        /// <summary>
        /// <see cref="YukyuKeikaku"/> と <see cref="YukyuKeikakuMeisai"/> のビューモデル
        /// </summary>
        public class YukyuKeikakuViewModel
        {
            /// <summary>
            /// 取得時点で休暇計画のレコードが存在するかどうかを示すプロパティ
            /// </summary>
            public bool RecordExists => 0 < Meisais.Count && Meisais[0].Id is not null;
            // 非存在時は明細IDが null となる（1件チェックで十分）

            /// <summary>
            /// <see cref="YukyuKeikaku.Status"/>
            /// レコード未登録時は null (非表示) であり <see cref="未申請"/> (表示) とは異なる
            /// レコード未登録時に null という状態を必須プロパティとして明示的に扱うため required を指定
            /// </summary>
            [Display(Name = "ステータス")]
            public required LeavePlanStatus? YukyuKeikakuStatus { get; init; }

            /// <summary>
            /// ステータスの表示可否を判定するプロパティ
            /// </summary>
            public bool YukyuKeikakuStatusIsVisible => YukyuKeikakuStatus is not null;

            /// <summary>
            /// <see cref="YukyuNendo.StartDate"/>
            /// </summary>
            public required DateOnly YukyuNendoStartDate { get; init; }

            /// <summary>
            /// <see cref="YukyuNendo.EndDate"/>
            /// </summary>
            public required DateOnly YukyuNendoEndDate { get; init; }

            /// <summary>
            /// <see cref="YukyuKeikaku.Version"/>
            /// </summary>
            public uint Version { get; init; }

            /// <summary>
            /// <see cref="YukyuKeikaku.YukyuKeikakuMeisais"/>
            /// </summary>
            [Length(ConstRequiredYukyuKeikakuMeisaiCount, ConstRequiredYukyuKeikakuMeisaiCount)]
            public required IReadOnlyList<Meisai> Meisais { get; init; }

            /// <summary>
            /// 休暇計画のステータスに応じて、一覧画面でステータス文字列に適用するCSSクラス名を返却する。
            /// 返却されたCSSクラスをステータス表示要素に付与することで、「未申請／承認待ち／承認済」などの状態を色で判別できるようにする。
            /// </summary>
            public string YukyuKeikakuStatusClass => YukyuKeikakuStatus switch
            {
                未申請 => "app-text-not-registered",
                事業部承認待ち or 人財承認待ち => "app-text-waiting",
                承認済 => "app-text-approved",
                _ => ""
            };

            /// <summary>
            /// ステータスが『事業部承認待ち』または『人財承認待ち』の場合は予定日の入力を不可（disabled）とする。
            /// </summary>
            public bool IsDisabled => YukyuKeikakuStatus == 事業部承認待ち || YukyuKeikakuStatus == 人財承認待ち;

            /// <summary>
            /// 空のビューモデルを取得する
            /// </summary>
            public static YukyuKeikakuViewModel Empty => new()
            {
                YukyuKeikakuStatus = null,
                YukyuNendoEndDate = default,
                YukyuNendoStartDate = default,
                Meisais = []
            };
        }

        /// <summary>
        /// <see cref="YukyuKeikakuMeisai"/>
        /// </summary>
        public class Meisai
        {
            /// <summary>
            /// <see cref="YukyuKeikakuMeisai.Id"/>
            /// </summary>
            public long? Id { get; init; }

            /// <summary>
            /// <see cref="YukyuKeikakuMeisai.Ymd"/>
            /// 必須項目だが、初期表示時はnullで表示するためnull許容型を使用
            /// </summary>
            [Display(Name = "休暇予定日")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public required DateOnly? Ymd { get; init; }

            /// <summary>
            /// <see cref="YukyuKeikakuMeisai.IsTokukyu"/>
            /// </summary>
            [Display(Name = "特別休暇")]
            public required bool IsTokukyu { get; init; }

            /// <summary>
            /// <see cref="YukyuKeikakuMeisai.Version"/>
            /// </summary>
            public uint Version { get; init; }
        }
    }
}
