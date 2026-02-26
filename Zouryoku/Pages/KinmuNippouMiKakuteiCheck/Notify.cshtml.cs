using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using static Model.Enums.AchievementClassification;
using static Model.Enums.EmployeeAuthority;
using static Model.Enums.FunctionalClassification;
using static Zouryoku.Utils.Const;
using static Zouryoku.Utils.JissekiKakuteiSimeUtil;

namespace Zouryoku.Pages.KinmuNippouMiKakuteiCheck
{
    /// <summary>
    /// 日報未確定通知画面のページモデル
    /// </summary>
    [FunctionAuthorization(勤務日報未確定チェック)]
    public partial class NotifyModel : BasePageModel<NotifyModel>
    {
        /// <summary>
        /// 未確定通知履歴テーブルに保持する最大件数。
        /// </summary>
        public const int MaxMikakuteiTsuchiRirekiCount = 5;

        // ======================================
        // DI
        // ======================================

        public NotifyModel(ZouContext db, ILogger<NotifyModel> logger,
            IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine, TimeProvider timeProvider)
            : base(db, logger, optionsAccessor, viewEngine, timeProvider) { }

        // ======================================
        // フィールド
        // ======================================

        /// <summary>
        /// 未確定通知送信履歴データのリスト。
        /// </summary>
        /// <value>送信履歴の表示用ビューモデルのリスト</value>
        public List<MikakuteiTsuchiRirekiViewModel> MikakuteiTsuchiRirekis { get; set; } = [];

        /// <summary>
        /// 送信予定メッセージ欄のテキストのバインドプロパティ。
        /// </summary>
        /// <value>送信予定メッセージ</value>
        [BindProperty(SupportsGet = true)]
        [Display(Name = "送信予定メッセージ")]
        public string SendMessage { get; set; } = string.Empty;

        /// <summary>
        /// 通知対象社員の社員BaseIDの配列のバインドプロパティ。
        /// </summary>
        /// <value>未確定通知を受信する社員の社員BaseIDの配列</value>
        /// <remarks>勤務日報未確定チェック画面から初回遷移時に受け取り、画面で保持する。</remarks>
        [BindProperty(SupportsGet = true)]
        public long[] ReceiveSyainBaseIds { get; set; } = [];

        public override bool UseInputAssets => true;

        // ======================================
        // ハンドラ
        // ======================================

        /// <summary>
        /// 初期遷移時のハンドラー。
        /// </summary>
        /// <param name="syainBaseIds">未確定通知を送信する宛先の配列</param>
        public async Task<IActionResult> OnGetAsync(long[] syainBaseIds)
        {
            // システム日付
            var today = timeProvider.Today();

            // 認可
            // ----------------------------------

            // 通知対象の実績期間
            var jissekiSpan = await GetCanNotifyJissekiSpanAsync(db, today);

            // システム日付が通知可能期間外なら403へリダイレクト
            if (!await IsInNotificationPeriodAsync(
                db, today, jissekiSpan.JissekiSimebiYmd, jissekiSpan.JissekiKakuteiKigenInfo.KakuteiKigenYmd))
            {
                return new RedirectResult("/page403/");
            }

            // 画面項目の設定
            // ----------------------------------

            // 送信先の社員BASE ID
            ReceiveSyainBaseIds = syainBaseIds;

            // 未確定通知送信履歴
            MikakuteiTsuchiRirekis = await GetMikakuteiTsuchiRirekisAsync();

            // 送信予定メッセージ

            // 送信予定メッセージ内の前半／後半の設定
            var kubunStr = jissekiSpan.JissekiKakuteiKigenInfo.Kubun switch
            {
                中締め => "前半",
                月末締め => "後半",
                一か月締め => "",
                _ => ""
            };
            // 送信予定メッセージを作成
            SendMessage = NippouMikakuteiTsuchiMessage.Format(jissekiSpan.JissekiStartYmd.Month, kubunStr,
                jissekiSpan.JissekiStartYmd.Day, jissekiSpan.JissekiSimebiYmd.Day);

            return Page();
        }

        /// <summary>
        /// 通知を送信するハンドラー。
        /// </summary>
        public async Task<IActionResult> OnPostNotifyAsync()
        {
            // システム日時
            var now = timeProvider.Now();
            // システム日付
            var today = now.ToDateOnly();

            // 認可
            // ----------------------------------

            // 通知対象の実績期間
            var jissekiSpan = await GetCanNotifyJissekiSpanAsync(db, today);

            // システム日付が通知可能期間外なら403へリダイレクト
            if (!await IsInNotificationPeriodAsync(
                db, today, jissekiSpan.JissekiSimebiYmd, jissekiSpan.JissekiKakuteiKigenInfo.KakuteiKigenYmd))
            {
                return new RedirectResult("/page403/");
            }

            // データの登録
            // ----------------------------------

            // 送信内容に登録するデータ
            var messageContent = new MessageContent
            {
                SyainId = LoginInfo.User.Id,
                Content = SendMessage,
                FunctionType = 未確定通知,
            };
            await db.AddAsync(messageContent);

            // 未確定通知送信履歴に登録するデータ
            var rireki = new MikakuteiTsuchiRireki()
            {
                JissekiKakuteiSimebiId = jissekiSpan.JissekiKakuteiKigenInfo.JissekiKakuteiSimebiId,
                SendSyainBaseId = LoginInfo.User.SyainBaseId,
                TuutiSousinNitizi = now,
                SendMessage = SendMessage
            };
            await db.AddAsync(rireki);

            // 中間テーブル（社員⇔未確定通知履歴）へデータを登録
            await AddSyainTsuchiRirekiRelsAsync(rireki, ReceiveSyainBaseIds);

            // 古い未確定通知送信履歴データの削除
            // ----------------------------------

            // コミット済みの未確定通知履歴データで、5件目以降のもののリスト
            var oldRirekis = await FetchOldMikakuteiTsuchiRirekis(MaxMikakuteiTsuchiRirekiCount - 1);

            foreach (var oldRireki in oldRirekis)
            {
                // 中間テーブルのデータも削除する
                db.RemoveRange(oldRireki.SyainTsuchiRirekiRels);
                db.Remove(oldRireki);
            }

            // トランザクションのコミット
            // ----------------------------------

            await db.SaveChangesAsync();

            return Success();
        }

        // ======================================
        // メソッド
        // ======================================

        /// <summary>
        /// 未確定通知履歴を全件取得する。
        /// </summary>
        /// <returns>未確定通知履歴のリスト</returns>
        private async Task<List<MikakuteiTsuchiRirekiViewModel>> GetMikakuteiTsuchiRirekisAsync()
        {
            return await db.MikakuteiTsuchiRirekis
                .AsSplitQuery()
                .AsNoTracking()
                .Include(rireki => rireki.SendSyainBase)
                    .ThenInclude(syainBase => syainBase.Syains)
                .Include(rireki => rireki.SyainTsuchiRirekiRels)
                .OrderByDescending(rireki => rireki.TuutiSousinNitizi)
                .Select(rireki => new MikakuteiTsuchiRirekiViewModel(rireki))
                .ToListAsync();
        }

        /// <summary>
        /// 社員⇔未確定通知履歴テーブルにデータを追加する。
        /// </summary>
        /// <param name="rireki">親となる未確定通知履歴エンティティ</param>
        /// <param name="receivedSyainBaseIds">通知対象社員の社員BaseIDの配列</param>
        private async Task AddSyainTsuchiRirekiRelsAsync(MikakuteiTsuchiRireki rireki, long[] receivedSyainBaseIds)
        {
            // 挿入するデータのリスト
            var rels = new List<SyainTsuchiRirekiRel>();

            foreach (var syainBaseId in receivedSyainBaseIds)
            {
                rels.Add(new SyainTsuchiRirekiRel()
                {
                    MikakuteiTsuchiRireki = rireki,
                    TsuchiSyainBaseId = syainBaseId,
                });
            }

            await db.AddRangeAsync(rels);
        }

        /// <summary>
        /// 古い未確定通知履歴データを取得する。
        /// </summary>
        /// <param name="keepCount">残しておきたい件数</param>
        /// <returns>送信時間降順で<paramref name="keepCount"/> + 1件目以降の未確定通知履歴データ</returns>
        private async Task<List<MikakuteiTsuchiRireki>> FetchOldMikakuteiTsuchiRirekis(int keepCount)
        {
            return await db.MikakuteiTsuchiRirekis
                // NOTE: 中間テーブルをIncludeすることで同時に削除できるようにする
                .Include(r => r.SyainTsuchiRirekiRels)
                .OrderByDescending(r => r.TuutiSousinNitizi)
                .Skip(keepCount)
                .ToListAsync();
        }
    }
}
