using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Model.Enums;
using Model.Model;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Utils;
using static Model.Enums.LeavePlanStatus;

namespace Zouryoku.Pages.YukyuKeikakuJigyobuShonin
{
    public partial class IndexModel
    {
        /// <summary>
        /// 事業部承認画面で使用するビューモデル。
        /// </summary>
        /// <remarks>
        /// このページには単項目チェック（データアノテーションによる項目単位の自動バリデーション）仕様がないため、
        /// モデルバインディング時の自動検証は行わず、ビジネスロジック側で一括して検証を行う設計としている。
        /// そのため、このビューモデル全体に対して <see cref="ValidateNeverAttribute"/> を付与し、
        /// モデルバインディング時の自動検証をスキップしている。代わりに、入力値の妥当性チェックは
        /// <c>JigyoubuShonin</c> ページの <see cref="ValidateNotChecked"/> メソッド内で実施する。
        /// </remarks>
        [ValidateNever]
        public class JigyoubuShoninViewModel
        {
            private const double PercentageMultiplier = 100;

            public Authority Authority { get; }

            /// <summary>
            /// 画面上のボタンを表示するかどうかを取得する。
            /// 権限なし（<see cref="Authority.None"/>）以外を持つ場合に <c>true</c> を返す。
            /// </summary>
            public bool ButtonsAreVisible => Authority != Authority.None;

            /// <summary>
            /// 画面上の部署列を表示するかどうかを取得する。
            /// 人財権限（<see cref="Authority.Jinzai"/>）を持つ場合に <c>true</c> を返す。
            /// </summary>
            public bool BusyoColumnIsVisible => Authority == Authority.Jinzai;

            /// <summary>
            /// 1 列分の列幅がテーブル全体に対して占める割合（パーセンテージ文字列）を取得する。
            /// </summary>
            public string SingleColumnWidthPercentage { get; }

            /// <summary>
            /// 明細列全体（<see cref="MeisaiPerYukyuKeikaku"/> 列分）の列幅がテーブル全体に対して、
            /// 占める割合（パーセンテージ文字列）を取得する。
            /// </summary>
            public string MeisaiColumnWidthPercentage { get; }

            public IReadOnlyList<Keikaku> Keikakus { get; init; }

            /// <summary>
            /// 空のビューモデルを取得する
            /// </summary>
            public JigyoubuShoninViewModel()
            {
                // 使用しない値を任意の非 null 値で埋める
                Authority = default;
                SingleColumnWidthPercentage = string.Empty;
                MeisaiColumnWidthPercentage = string.Empty;

                // 既定の ASP.NET Core ModelBinder は設定済みのリストを差し替えられないため null を設定する
                // また、他画面と実装方法を統一するため、null 非許容プロパティに null! を設定している
                Keikakus = null!;
            }

            /// <summary>
            /// 権限と有給計画一覧からビューモデルを生成するコンストラクター。
            /// </summary>
            /// <param name="authority">承認者の権限情報。</param>
            /// <param name="keikakus">画面に表示する有給計画一覧。</param>
            public JigyoubuShoninViewModel(Authority authority, IReadOnlyList<Keikaku> keikakus)
            {
                Authority = authority;
                Keikakus = keikakus;

                var totalColumnCount = CalculateTotalColumnCount();

                // 実数で除算させるために double 型にキャスト
                SingleColumnWidthPercentage = $"{((double)1 / totalColumnCount) * PercentageMultiplier:0.000}%";
                MeisaiColumnWidthPercentage = $"{((double)MeisaiPerYukyuKeikaku / totalColumnCount) * PercentageMultiplier:0.000}%";
            }

            /// <summary>
            /// <see cref="Keikaku"/> にチェックボックスを表示するかを取得する
            /// </summary>
            public bool GetCheckboxIsVisible(int index)
            {
                if (Authority == Authority.Jinzai) return true;
                if (Authority == Authority.None) return false;

                // 部門長の場合
                var keikaku = Keikakus[index];
                return keikaku.YukyuKeikakuStatus == 事業部承認待ち;
            }

            /// <summary>
            /// 一覧の合計列数を計算する。
            /// </summary>
            /// <returns>
            /// ステータス列・部署列（表示対象の場合）・氏名列・有給計画明細列を合計した列数。
            /// </returns>
            private int CalculateTotalColumnCount() =>
                    1 /* ステータス列 */ +
                    (BusyoColumnIsVisible ? 1 : 0) /* 部署列 */ +
                    1 /* 氏名列 */ +
                    MeisaiPerYukyuKeikaku /* 明細列数 (7) */;
        }

        /// <summary>
        /// <see cref="YukyuKeikaku"/>
        /// </summary>
        public class Keikaku
        {
            /// <summary>
            /// <see cref="YukyuKeikaku.Id"/>
            /// </summary>
            public required long? Id { get; init; }

            /// <summary>
            /// <see cref="YukyuKeikaku.Status"/>
            /// </summary>
            [Display(Name = "ステータス")]
            public required LeavePlanStatus? YukyuKeikakuStatus { get; init; }

            /// <summary>
            /// 有給計画ステータスの表示用テキストを取得します。nullの場合は「未入力」を返します。
            /// </summary>
            public string? YukyuKeikakuStatusText => YukyuKeikakuStatus switch
            {
                null => Const.NotEntered,
                _ => YukyuKeikakuStatus.GetDisplayName()
            };

            /// <summary>
            /// <see cref="Syain.Name"/>
            /// </summary>
            [Display(Name = "氏名")]
            public required string SyainName { get; init; }

            /// <summary>
            /// <see cref="Busyo.Name"/>
            /// </summary>
            [Display(Name = "部署")]
            public required string BusyoName { get; init; }

            /// <summary>
            /// <see cref="YukyuKeikaku.YukyuKeikakuMeisais"/>
            /// </summary>
            public required IReadOnlyList<Meisai> Meisais { get; init; }

            public bool IsChecked { get; set; }

            /// <summary>
            /// <see cref="YukyuKeikaku.Version"/>
            /// </summary>
            public uint Version { get; set; }

            /// <summary>
            /// ステータスは値によってフォントの色を変えて表示
            /// </summary>
            public string YukyuKeikakuStatusColorClass => YukyuKeikakuStatus switch
            {
                null => "app-text-not-entered", // 未入力
                未申請 => "app-text-not-registered",
                _ => "app-text-processed" // 事業部承認待ち or 人財承認待ち or 承認済
            };
        }

        /// <summary>
        /// <see cref="YukyuKeikakuMeisai"/>
        /// </summary>
        public class Meisai
        {
            /// <summary>
            /// <see cref="YukyuKeikakuMeisai.Ymd"/>
            /// </summary>
            [Display(Name = "休暇予定日")]
            [DisplayFormat(DataFormatString = @"{0:M/d(ddd)}")]
            public required DateOnly? Ymd { get; init; }

            /// <summary>
            /// <see cref="YukyuKeikakuMeisai.IsTokukyu"/>
            /// </summary>
            [Display(Name = "特別休暇")]
            public required bool? IsTokukyu { get; init; }

            /// <summary>
            /// レコード未登録時用の <see cref="Keikaku.Meisais"/> を取得する (件数は <see cref="MeisaiPerYukyuKeikakuValue"/>)
            /// </summary>
            public static ImmutableArray<Meisai> EmptyMeisais { get; } = [.. Enumerable.Range(0, MeisaiPerYukyuKeikakuValue)
                .Select(_ => new Meisai { Ymd = null, IsTokukyu = null })];

            /// <summary>
            /// 特別休暇によってフォントの色を変えて表示
            /// </summary>
            public string ColorClass => IsTokukyu is true ? "app-text-tokukyu-ymd" : "app-text-normal-ymd";
        }

        /// <summary>
        /// このページでのユーザーの権限
        /// </summary>
        public enum Authority
        {
            /// <summary>
            /// 通常社員ユーザー
            /// </summary>
            None,

            /// <summary>
            /// <see cref="BusyoBasis.Bumoncyo"/> に該当するユーザー
            /// </summary>
            Bumoncyo,

            /// <summary>
            /// <see cref="Syain.Kengen"/> == <see cref="計画休暇承認"/> のユーザー
            /// </summary>
            Jinzai
        }

        /// <summary>
        /// このページのアクション（ボタン押下）種別
        /// </summary>
        public enum ActionType
        {
            /// <summary>
            /// 「差し戻し」ボタン押下に対応するアクション種別。
            /// </summary>
            SendBack,

            /// <summary>
            /// 「承認」ボタン押下に対応するアクション種別。
            /// </summary>
            Approve
        }
    }
}
