using CommonLibrary.Extensions;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using static Zouryoku.Utils.Const;

namespace Zouryoku.Pages.AnkenMeiKensaku
{
    /// <summary>
    /// <seealso cref="IndexModel"/>で使用するビュー／バインドモデル
    /// </summary>
    public partial class IndexModel
    {
        /// <summary>
        /// 案件名検索の検索条件バインドモデル
        /// </summary>
        public class AnkenSearchModel
        {
            /// <summary>
            /// 並び替え条件用の列挙体
            /// </summary>
            public enum SortKeyList
            {
                [Display(Name = "顧客名")]
                顧客名 = 1,
                [Display(Name = "着工日")]
                着工日 = 2,
            }

            [Display(Name = "受注番号")]
            public JuchuuNoWrapper JuchuuNo { get; set; } = new();

            [Display(Name = "着工日")]
            public ChaYmdRange ChaYmd { get; set; } = new();

            [Display(Name = "顧客名")]
            public string? KokyakuName { get; set; }

            [Display(Name = "案件名")]
            public string? AnkenName { get; set; }

            [Display(Name = "自部署の案件のみ")]
            public bool IsOwnBusyoOnly { get; set; } = true;

            [Display(Name = "凍結案件を表示")]
            public bool ShowGenkaToketu { get; set; } = false;

            [Display(Name = "責任者")]
            public string? SekininSyaName { get; set; }

            [Display(Name = "責任者ID")]
            public long? SekininSyaBaseId { get; set; }

            [Display(Name = "並び順")]
            public SortKeyList SortKey { get; set; } = SortKeyList.顧客名;

            /// <summary>
            /// フォームから得た検索条件が空かどうかを表す真理値。
            /// 検索時に年度の条件を付加するかどうかを判定するために利用する。
            /// </summary>
            /// <value>すべての検索条件が未指定の場合に true。</value>
            public bool IsFormEmpty =>
                // 受注番号
                // NOTE: ビルド時のNullReferenceを避けるためにnullチェックを行う
                (JuchuuNo is null
                    || (string.IsNullOrWhiteSpace(JuchuuNo.ProjectNo)
                        && string.IsNullOrWhiteSpace(JuchuuNo.JuchuuNo)
                        && !JuchuuNo.JuchuuGyoNo.HasValue))
                // 着工日
                && (ChaYmd is null
                    // NOTE: ビルド時のNullReferenceを避けるためにnullチェックを行う
                    || (!ChaYmd.From.HasValue
                        && !ChaYmd.To.HasValue))
                // 顧客名
                && string.IsNullOrWhiteSpace(KokyakuName)
                // 案件名
                && string.IsNullOrWhiteSpace(AnkenName)
                // 責任者ID
                && !SekininSyaBaseId.HasValue;

            /// <summary>
            /// 着工日の範囲指定検索条件用のラッパークラス
            /// </summary>
            public class ChaYmdRange
            {
                public DateOnly? From { get; set; }

                public DateOnly? To { get; set; }

                /// <summary>
                /// <see cref="To"/> < <see cref="From"/>となっているときに、<see cref="From"/> < <see cref="To"/>となるように修正する。
                /// </summary>
                public void NormalizeDateRange()
                {
                    if (From is not null && To is not null && To < From)
                    {
                        (From, To) = (To, From);
                    }
                }
            }

            /// <summary>
            /// プロジェクト番号、受注番号、行番号を扱うラッパークラス
            /// </summary>
            public class JuchuuNoWrapper
            {
                [Display(Name = "プロジェクト番号")]
                public string? ProjectNo { get; set; }

                [Display(Name = "受注番号")]
                public string? JuchuuNo { get; set; }

                [Display(Name = "受注行番号")]
                // NOTE: Range属性で整数値以外を禁止する
                [Range(0, short.MaxValue, ErrorMessage = ErrorNumberRangeMoreThanEqual)]
                public short? JuchuuGyoNo { get; set; }
            }
        }

        /// <summary>
        /// 案件情報のビューモデル
        /// </summary>
        /// <param name="anken">案件情報のエンティティ</param>
        public class AnkenViewModel
        {
            private readonly Anken? _anken;
            private readonly AnkenSansyouRireki? _rireki;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="anken">案件情報のエンティティ</param>
            public AnkenViewModel(Anken anken) => _anken = anken;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="rireki">案件参照履歴のエンティティ</param>
            public AnkenViewModel(AnkenSansyouRireki rireki) => _rireki = rireki;

            /// <summary>
            /// 案件情報へアクセスするためのプロパティ
            /// </summary>
            private Anken Source
                => _anken ?? _rireki?.Anken
                    ?? throw new InvalidOperationException("案件情報と案件参照履歴の情報の両方が設定されていません。");

            [Display(Name = "顧客名")]
            public string? KokyakuName => Source.KokyakuKaisya?.Name;

            public long? KokyakuId => Source.KokyakuKaisyaId;

            [Display(Name = "案件名")]
            public string AnkenName => Source.Name;

            public long AnkenId => Source.Id;

            [Display(Name = "商品名")]
            public string? ShouhinName => Source.KingsJuchu?.ShouhinName;

            /// <value>プロジェクト番号-受注番号-受注行番号</value>
            [Display(Name = "受注番号")]
            public string? JuchuuNo => Source.KingsJuchu?.KingsJuchuNo;


            [Display(Name = "受注金額")]
            public string? JucKin => Source.KingsJuchu is null ? null : string.Format("{0:#,0}", Source.KingsJuchu?.JucKin);

            [Display(Name = "着工日")]
            public string? ChaYmd => Source.KingsJuchu?.ChaYmd.YMDSlash();

            [Display(Name = "納期")]
            public string? NsyYmd => Source.KingsJuchu?.NsyYmd?.YMDSlash();

            /// <summary>
            /// 責任者名
            /// </summary>
            /// <remarks>エンティティに紐づく社員は有効期限から一意に定まっている前提</remarks>
            [Display(Name = "責任者")]
            public string? SyainName => Source.SyainBase?
                .Syains?
                .SingleOrDefault()?
                .Name;

            /// <summary>
            /// 同時実行制御用のバージョン
            /// </summary>
            /// <remarks>案件参照履歴の単一削除時に使用する</remarks>
            public uint? Version => _rireki?.Version;
        }
    }
}
