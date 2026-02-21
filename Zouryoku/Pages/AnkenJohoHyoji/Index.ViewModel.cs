using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Data;

namespace Zouryoku.Pages.AnkenJohoHyoji
{
    public partial class IndexModel
    {
        /// <summary>
        /// 画面表示用ViewModel
        /// </summary>
        public ViewModel IndexViewModel { get; private set; } = default!;

        private const string HideClass = "d-none";

        public class ViewModel
        {
            /// <summary>
            /// 登録可能フラグ
            /// </summary>
            public required bool CanAdd { get; set; }

            /// <summary>
            /// ログイン情報
            /// </summary>
            public required LoginInfo LoginInfo { get; set; }

            /// <summary>
            /// 表示対象案件（エンティティ）
            /// </summary>
            public Anken? Anken { private get; set; }

            /// <summary>
            /// 案件ID
            /// </summary>
            public long? Id => Anken?.Id;

            /// <summary>
            /// KINGS受注ID
            /// </summary>
            public long? KingsJuchuId => Anken?.KingsJuchuId;

            /// <summary>
            /// 顧客会社ID
            /// </summary>
            public long? KokyakuKaisyaId => Anken?.KokyakuKaisyaId;

            [Display(Name = "受注工番")]
            public string? DispJuchuNo => Anken?.KingsJuchu?.KingsJuchuNo;

            [Display(Name = "受注件名")]
            public string? Bukken => Anken?.KingsJuchu?.Bukken;

            [Display(Name = "案件名")]
            public string? AnkenName => Anken?.Name;

            [Display(Name = "受注種類")]
            public string? JyutyuSyuruiName => Anken?.JyutyuSyurui?.Name;

            /// <summary>
            /// 顧客名＋支店名
            /// </summary>
            [Display(Name = "顧客情報")]
            public string? KokyakuName
            {
                // 顧客会社と支店名を半角スペースで連結して返す
                // 支店名が無い場合は顧客会社名のみ返す
                // どちらも無い場合はnullを返す
                get
                {
                    var kokyakuKaisya = Anken?.KokyakuKaisya;
                    if (kokyakuKaisya is null)
                    {
                        return null;
                    }
                    if (kokyakuKaisya.Shiten is null)
                    {
                        return kokyakuKaisya.Name;
                    }
                    // 顧客会社名と支店名を半角スペースで連結
                    return $"{kokyakuKaisya.Name} {kokyakuKaisya.Shiten}";
                }
            }

            [Display(Name = "弊社責任者")]
            public string? SyainName => Anken?.SyainBase?.Syains.Select(s => s.Name).FirstOrDefault();

            [Display(Name = "案件内容")]
            public string? Naiyou => Anken?.Naiyou;

            /// <summary>
            /// 編集ボタンのCSSクラス
            /// 編集可能な場合は、ボタンを表示するために空文字、不可の場合は"d-none"で非表示にする
            /// </summary>
            public string EditButtonClass =>
                CanAdd
                && Anken?.KingsJuchu != null
                && LoginInfo.User.BusyoCode.Equals(Anken.KingsJuchu.SekouBumonCd)
                    ? string.Empty : HideClass;

            /// <summary>
            /// KINGS受注リンク表示スタイルクラス
            /// </summary>
            public string KingsJuchuLinkClass => Anken != null && Anken.KingsJuchuId.HasValue ? string.Empty : HideClass;

            /// <summary>
            /// 顧客会社リンク表示スタイルクラス
            /// </summary>
            public string KokyakuKaisyaLinkClass => Anken != null && Anken.KokyakuKaisyaId.HasValue ? string.Empty : HideClass;
        }
    }
}