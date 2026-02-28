using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Model.Model;
using System.ComponentModel.DataAnnotations;

namespace Zouryoku.Pages.KinmuNippouKakunin
{
    public partial class IndexModel
    {
        /// <summary>
        /// 勤務日報確認画面で使用する検索条件。
        /// </summary>
        /// <remarks>
        /// このページには単項目チェック（データアノテーションによる項目単位の自動バリデーション）仕様がないため、
        /// モデルバインディング時の自動検証は行わず、ビジネスロジック側で一括して検証を行う設計としている。
        /// </remarks>
        [ValidateNever]
        public class DaysQuery
        {
            /// <summary>
            /// 対象年月
            /// </summary>
            public DateOnly? TargetYm { get; init; }

            /// <summary>
            /// <see cref="TargetYm"/> の表示用テキスト
            /// 表示テンプレートの影響を受けないためのプロパティであり <see cref="DisplayFormatAttribute"/> は使用しない。
            /// </summary>
            public string TargetYmText => TargetYm?.ToString("yyyy/MM") ?? "";

            public string TargetYmPickerValue => TargetYm?.ToString("yyyy-MM") ?? "";

            /// <summary>
            /// 対象社員ID
            /// </summary>
            public long? TargetSyainId { get; init; }

        }

        /// <summary>
        /// 勤務日報確認画面で使用するビューモデル。
        /// </summary>
        public class DaysViewModel
        {
            /// <summary>
            /// 対象社員名
            /// </summary>
            public required string TargetSyainName { get; init; }

            /// <summary>
            /// 対象社員部署名
            /// </summary>
            public required IReadOnlyList<string> TargetSyainHierarchicalBusyoNames { get; init; }

            private const char HierarchicalBusyoNameSeparatorChar = '＞';

            /// <summary>
            /// 対象社員部署名
            /// </summary>
            public string TargetSyainJoinedBusyoName =>
                string.Join(HierarchicalBusyoNameSeparatorChar, TargetSyainHierarchicalBusyoNames);

            /// <summary>
            /// 対象年月の表
            /// </summary>
            public required IReadOnlyList<Day> Days { get; init; }

            public static DaysViewModel Empty { get; } = new DaysViewModel
            {
                TargetSyainName = "",
                TargetSyainHierarchicalBusyoNames = [],
                Days = [],
            };

            public string DisplayHours(decimal? minutes)
            {
                if (minutes is null || minutes == 0) return "";
                else return TimeSpan.FromMinutes((double)minutes).ToString(@"hh\:mm");
            }

            public string DisplayHours(TimeOnly? hours)
            {
                if (hours is null || hours == TimeOnly.MinValue) return "";
                else return hours.Value.ToString(@"HH\:mm");
            }
        }

        /// <summary>
        /// 対象年月の表の1行
        /// </summary>
        /// <param name="date">行の日付</param>
        /// <param name="isHikadoubi">行の日付が非稼働日のとき true。</param>
        /// <param name="nippou">行の勤務日報実績データ。null の場合は空行としてカレンダーを埋める。</param>
        public class Day(DateOnly date, bool isHikadoubi, Nippou? nippou = null)
        {
            /// <summary>
            /// この行の日付
            /// </summary>
            [DisplayFormat(DataFormatString = "{0:dd(ddd)}")]
            public DateOnly Date => date;

            /// <summary>
            /// <see cref="Hikadoubi"/> 存在
            /// </summary>
            public bool IsHikadoubi => isHikadoubi;

            /// <summary>
            /// <see cref="Nippou.SyukkinKubunId1Navigation"/> → <see cref="SyukkinKubun.Name"/>
            /// </summary>
            public string? SyukkinKubun1 => nippou?.SyukkinKubunId1Navigation.Name;

            /// <summary>
            /// <see cref="Nippou.SyukkinKubunId2Navigation"/> → <see cref="SyukkinKubun.Name"/>
            /// </summary>
            public string? SyukkinKubun2 => nippou?.SyukkinKubunId2Navigation?.Name;

            private static TimeOnly[] GetNonNullTimeArray(params IEnumerable<TimeOnly?> times) => [.. times
                .Where(t => t is not null)
                .Select(t => t!.Value)];

            /// <summary>
            /// [<see cref="Nippou.SyukkinHm1"/>, <see cref="Nippou.SyukkinHm2"/>, <see cref="Nippou.SyukkinHm3"/>]
            /// </summary>
            public IReadOnlyList<TimeOnly> SyukkinHms { get; } = GetNonNullTimeArray(
                nippou?.SyukkinHm1,
                nippou?.SyukkinHm2,
                nippou?.SyukkinHm3);

            /// <summary>
            /// [<see cref="Nippou.TaisyutsuHm1"/>, <see cref="Nippou.TaisyutsuHm2"/>, <see cref="Nippou.TaisyutsuHm3"/>]
            /// </summary>
            public IReadOnlyList<TimeOnly> TaisyutsuHms { get; } = GetNonNullTimeArray(
                nippou?.TaisyutsuHm1,
                nippou?.TaisyutsuHm2,
                nippou?.TaisyutsuHm3);

            /// <summary>
            /// <see cref="Nippou.HZangyo"/>
            /// </summary>
            public decimal? HZangyo => nippou?.HZangyo;

            /// <summary>
            /// <see cref="Nippou.HWarimashi"/>
            /// </summary>
            public decimal? HWarimashi => nippou?.HWarimashi;

            /// <summary>
            /// <see cref="Nippou.HShinyaZangyo"/>
            /// </summary>
            public decimal? HShinyaZangyo => nippou?.HShinyaZangyo;

            /// <summary>
            /// <see cref="Nippou.DZangyo"/>
            /// </summary>
            public decimal? DZangyo => nippou?.DZangyo;

            /// <summary>
            /// <see cref="Nippou.DWarimashi"/>
            /// </summary>
            public decimal? DWarimashi => nippou?.DWarimashi;

            /// <summary>
            /// <see cref="Nippou.DShinyaZangyo"/>
            /// </summary>
            public decimal? DShinyaZangyo => nippou?.DShinyaZangyo;

            /// <summary>
            /// <see cref="Nippou.NJitsudou"/>
            /// </summary>
            public decimal? NJitsudou => nippou?.NJitsudou;

            /// <summary>
            /// <see cref="Nippou.NShinya"/>
            /// </summary>
            public decimal? NShinya => nippou?.NShinya;

            /// <summary>
            /// <see cref="Nippou.NippouAnkens"/>
            /// </summary>
            public IReadOnlyList<DayAnken> Ankens { get; } = nippou?.NippouAnkens
                .Select(na => na.Ankens)
                .Where(a => a.KingsJuchu is not null)
                .Select(a => new DayAnken(a, a.KingsJuchu!))
                .ToList() ?? [];

            /// <summary>
            /// 土日祝で背景色を変更
            /// </summary>
            public string LineClass => isHikadoubi is true ? "app-line--holiday" : Date.DayOfWeek switch
            {
                DayOfWeek.Sunday => "app-line--sunday",
                DayOfWeek.Saturday => "app-line--saturday",
                _ => "app-line--weekday"
            };
        }

        /// <summary>
        /// <see cref="Anken"/>
        /// </summary>
        public class DayAnken(Anken anken, KingsJuchu kingsJuchu)
        {
            /// <summary>
            /// <see cref="Anken.Name"/>
            /// </summary>
            public string AnkenName => anken.Name;

            /// <summary>
            /// <see cref="Anken.KingsJuchu"/> → <see cref="KingsJuchu.ProjectNo"/>
            /// </summary>
            public string ProjectNo => kingsJuchu.ProjectNo;

            /// <summary>
            /// <see cref="Anken.KingsJuchu"/> → <see cref="KingsJuchu.JuchuuNo"/>
            /// </summary>
            public string? JuchuuNo => kingsJuchu.JuchuuNo;

            /// <summary>
            /// <see cref="Anken.KingsJuchu"/> → <see cref="KingsJuchu.JuchuuGyoNo"/>
            /// </summary>
            public short? JuchuuGyoNo => kingsJuchu.JuchuuGyoNo;

            /// <summary>
            /// <see cref="Anken.KingsJuchu"/> → <see cref="KingsJuchu.ChaYmd"/>
            /// </summary>
            [DisplayFormat(DataFormatString = "{0:yy/MM}")]
            public DateOnly ChaYmd => kingsJuchu.ChaYmd;
        }
    }
}
