using CommonLibrary.Extensions;
using Model.Model;
using static Zouryoku.Utils.Const;

namespace Zouryoku.Pages.KinmuNippouMiKakuteiCheck
{
    public partial class NotifyModel
    {
        /// <summary>
        /// 未確定通知履歴のビューモデル
        /// </summary>
        public class MikakuteiTsuchiRirekiViewModel
        {
            private readonly MikakuteiTsuchiRireki _rireki;

            // ======================================
            // コンストラクタ
            // ======================================

            public MikakuteiTsuchiRirekiViewModel(MikakuteiTsuchiRireki rireki)
                => _rireki = rireki;

            // ======================================
            // プロパティ
            // ======================================

            /// <summary>
            /// 画面に表示する送信履歴情報。
            /// </summary>
            /// <value><see cref="SendDateTime"/> <see cref="SyainName"/>が<see cref="SendCount"/>名に送信</value>
            public string RirekiDisplay
                => NippouMikakuteiTsuchiSendHistoryStr.Format(SendDateTime.ToString("MM/dd"), SyainName, SendCount);

            /// <value>
            /// 送信日時
            /// </value>
            public DateTime SendDateTime => _rireki.TuutiSousinNitizi;

            /// <value>
            /// 送信社員の名前
            /// </value>
            public string SyainName => _rireki.SendSyainBase.Syains.First().Name;

            /// <value>
            /// 送信先の社員数
            /// </value>
            public int SendCount => _rireki.SyainTsuchiRirekiRels.Count;
        }
    }
}
