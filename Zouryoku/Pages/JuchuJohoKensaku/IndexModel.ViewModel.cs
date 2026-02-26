using CommonLibrary.Extensions;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using static Zouryoku.Utils.Const;

namespace Zouryoku.Pages.JuchuJohoKensaku
{
    /// <summary>
    /// <seealso cref="IndexModel"/>で使用するビュー／バインドモデル
    /// </summary>
    public partial class IndexModel
    {
        /// <summary>
        /// 受注情報検索の検索条件バインドモデル
        /// </summary>
        public class JuchuJohoSearchModel
        {
            /// <summary>
            /// 受注番号
            /// </summary>
            [Display(Name = "受注番号")]
            public JuchuuNoWrapper JuchuuNo { get; set; } = new();

            /// <summary>
            /// 着工日
            /// </summary>
            [Display(Name = "着工日")]
            public ChaYmdRange ChaYmd { get; set; } = new();

            /// <summary>
            /// 施工部署
            /// </summary>
            [Display(Name = "施工部署")]
            public string? SekouBusyoCd { get; set; }

            /// <summary>
            /// 契約状態
            /// </summary>
            [Display(Name = "契約状態")]
            public KeiyakuJoutai Keiyaku { get; set; }

            /// <summary>
            /// 顧客名
            /// </summary>
            [Display(Name = "顧客名")]
            public string? KokyakuName { get; set; }

            /// <summary>
            /// 件名
            /// </summary>
            [Display(Name = "件名")]
            public string? Bukken { get; set; }

            /// <summary>
            /// 送り元部署コード
            /// </summary>
            [Display(Name = "送り元部署")]
            public string? IriBusCd { get; set; }

            /// <summary>
            /// 送り担当者コード
            /// </summary>
            [Display(Name = "送り担当者")]
            public string? OkrTanCd1 { get; set; }

            /// <summary>
            /// 受注金額
            /// </summary>
            [Display(Name = "受注金額")]
            [CurrencyRange(0, long.MaxValue, ErrorMessage = ErrorRange)]
            public string? JucKin { get; set; }

            /// <summary>
            /// 並び順
            /// </summary>
            [Display(Name = "並び順")]
            public SortKeyList SortKey { get; set; } = SortKeyList.受注先顧客;

            /// <summary>
            /// 契約状態条件用の列挙体
            /// </summary>
            public enum KeiyakuJoutai
            {
                [Display(Name = "すべて")]
                すべて = 1,

                [Display(Name = "自営")]
                自営 = 2,

                [Display(Name = "協同受け")]
                協同受け = 3,

                [Display(Name = "依頼受け")]
                依頼受け = 4
            }

            /// <summary>
            /// 並び替え条件用の列挙体
            /// </summary>
            public enum SortKeyList
            {
                [Display(Name = "受注先顧客")]
                受注先顧客 = 1,

                [Display(Name = "契約先顧客")]
                契約先顧客 = 2,

                [Display(Name = "受注件名")]
                受注件名 = 3,

                [Display(Name = "着工日")]
                着工日 = 4,

                [Display(Name = "受注日")]
                受注日 = 5
            }

            /// <summary>
            /// 送り担当者リストボックス用
            /// </summary>
            public class OkrTanInfo
            {
                /// <summary>
                /// 値
                /// </summary>
                public string? Value { get; set; }

                /// <summary>
                /// 表示ラベル
                /// </summary>
                public string? Text { get; set; }
            }

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
                // 施工部署
                && string.IsNullOrWhiteSpace(SekouBusyoCd)
                // 顧客名
                && string.IsNullOrWhiteSpace(KokyakuName)
                // 件名
                && string.IsNullOrWhiteSpace(Bukken)
                // 送り元部署
                && string.IsNullOrWhiteSpace(IriBusCd)
                // 送り担当者
                && string.IsNullOrWhiteSpace(OkrTanCd1)
                // 受注金額
                && JucKin is null;

            /// <summary>
            /// 着工日の範囲指定検索条件用のラッパークラス
            /// </summary>
            public class ChaYmdRange
            {
                /// <summary>
                /// 着工日FROM
                /// </summary>
                public DateOnly? From { get; set; }

                /// <summary>
                /// 着工日TO
                /// </summary>
                public DateOnly? To { get; set; }

                /// <summary>
                /// TO→FROMとなっているときに、FROM→TOとなるように修正する。
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
            /// プロジェクト番号、受注番号、受注行番号を扱うラッパークラス
            /// </summary>
            public class JuchuuNoWrapper
            {
                /// <summary>
                /// プロジェクト番号
                /// </summary>
                [Display(Name = "プロジェクト番号")]
                public string? ProjectNo { get; set; }

                /// <summary>
                /// 受注番号
                /// </summary>
                [Display(Name = "受注番号")]
                public string? JuchuuNo { get; set; }

                /// <summary>
                /// 受注行番号
                /// </summary>
                [Display(Name = "受注行番号")]
                // NOTE: Range属性で整数値以外を禁止する
                [Range(0, short.MaxValue, ErrorMessage = ErrorRange)]
                public short? JuchuuGyoNo { get; set; }
            }
        }

        /// <summary>
        /// 受注情報のビューモデル
        /// </summary>
        /// <param name="juchu">受注情報のエンティティ</param>
        public class JuchuJohoViewModel
        {
            private readonly KingsJuchu? _juchu;
            private readonly KingsJuchuSansyouRireki? _rireki;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="juchu">受注情報のエンティティ</param>
            public JuchuJohoViewModel(KingsJuchu juchu) => _juchu = juchu;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="rireki">受注参照履歴のエンティティ</param>
            public JuchuJohoViewModel(KingsJuchuSansyouRireki rireki) => _rireki = rireki;

            /// <summary>
            /// 受注情報へアクセスするためのプロパティ
            /// </summary>
            private KingsJuchu Source
                => _juchu ?? _rireki?.KingsJuchu
                    ?? throw new InvalidOperationException("受注情報と受注参照履歴の情報の両方が設定されていません。");

            /// <summary>
            /// 受注番号(表示用)
            /// </summary>
            /// <value>
            /// プロジェクト番号-受注番号-受注行番号
            /// </value>
            [Display(Name = "受注番号")]
            public string? JuchuuNoForDisplay => Source.KingsJuchuNo;

            /// <summary>
            /// プロジェクト番号(非表示)
            /// </summary>
            public string ProjectNo => Source.ProjectNo;

            /// <summary>
            /// 受注番号(非表示)
            /// </summary>
            public string? JuchuuNo => Source.JuchuuNo;

            /// <summary>
            /// 受注行番号(非表示)
            /// </summary>
            public short? JuchuuGyoNo => Source.JuchuuGyoNo;

            /// <summary>
            /// 受注先顧客
            /// </summary>
            [Display(Name = "受注先顧客")]
            public string? JuchuuKokyakuName => Source.JucNm;

            /// <summary>
            /// 契約先顧客
            /// </summary>
            [Display(Name = "契約先顧客")]
            public string? KeiyakuKokyakuName => Source.KeiNm;

            /// <summary>
            /// 受注件名
            /// </summary>
            [Display(Name = "受注件名")]
            public string Bukken => Source.Bukken;

            /// <summary>
            /// 商品名
            /// </summary>
            [Display(Name = "商品名")]
            public string? ShouhinName => Source.ShouhinName;

            /// <summary>
            /// 受注金額
            /// </summary>
            [Display(Name = "受注金額")]
            public string JucKin => string.Format("{0:#,0}", Source.JucKin);

            /// <summary>
            /// 着工日
            /// </summary>
            [Display(Name = "着工日")]
            public string ChaYmd => Source.ChaYmd.YMDSlash();

            /// <summary>
            /// 受注日
            /// </summary>
            [Display(Name = "受注日")]
            public string JucYmd => Source.JucYmd.YMDSlash();

            /// <summary>
            /// 受注取消
            /// </summary>
            [Display(Name = "受注取消")]
            public string Deleted => Source.IsGenkaToketu ? "◎" : "";

            /// <summary>
            /// KINGS受注ID
            /// </summary>
            public long? JuchuId => Source.Id;

            /// <summary>
            /// 同時実行制御用のバージョン
            /// </summary>
            /// <remarks>案件参照履歴の単一削除時に使用する</remarks>
            public uint? Version => _rireki?.Version;
        }
    }
}
