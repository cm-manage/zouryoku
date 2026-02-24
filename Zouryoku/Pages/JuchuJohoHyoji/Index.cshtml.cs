using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Zouryoku.Utils.KingsJuchuSansyouRirekisUtil;

namespace Zouryoku.Pages.JuchuJohoHyoji
{
    /// <summary>
    /// 受注情報表示のページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class IndexModel : BasePageModel<IndexModel>
    {
        // ---------------------------------------------
        // 定数
        // ---------------------------------------------

        // ---------------------------------------------
        // DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        public IndexModel(
            ZouContext db,
            ILogger<IndexModel> logger,
            IOptions<AppConfig> options)
            : base(db, logger, options)
        {
        }

        // ---------------------------------------------
        // プロパティ
        // ---------------------------------------------
        /// <summary>
        /// 受注情報表示用ViewModel
        /// </summary>
        public JuchuViewModel JuchuView { get; set; } = new JuchuViewModel();

        // ---------------------------------------------
        // OnGet
        // ---------------------------------------------
        /// <summary>
        /// 初期表示
        ///     表示用受注情報を取得
        ///     受注参照履歴を登録/更新
        ///         参照履歴が最大件数を超過した場合、削除処理を実行
        /// </summary>
        /// <param name="id">受注ID</param>
        /// <returns>PageResult</returns>
        public async Task<IActionResult> OnGetAsync(long id)
        {
            // 受注情報を取得
            var juchuData = await GetKingsJuchuAsync(id);

            // データが取得できなかった場合
            if (juchuData is null)
            {
                return RedirectToPage("/ErrorMessage", new {errorMessage = Const.ErrorSelectedDataNotExists});
            }

            // エンティティをViewModelに変換
            JuchuView = new JuchuViewModel()
            {
                Juchu = juchuData,
            };

            // 条件を満たす場合、受注参照履歴を登録/更新/削除処理を実行する
            var now = timeProvider.Now();
            await MaintainKingsJuchuSansyouRirekiAsync(db, id, LoginInfo.User.SyainBaseId, now);

            await db.SaveChangesAsync();

            return Page();
        }

        // ---------------------------------------------
        // プライベートメソッド
        // ---------------------------------------------
        /// <summary>
        /// 受注情報を取得
        /// 条件に合致する受注情報が存在しない場合は <c>null</c> を返します。
        /// </summary>
        /// <param name="juchuId">受注Id</param>
        /// <returns>受注情報</returns>
        private async Task<KingsJuchu?> GetKingsJuchuAsync(long juchuId)
        {
            return await db.KingsJuchus
                .AsNoTracking()
                .Include(x => x.Busyo)
                .FirstOrDefaultAsync(x => x.Id == juchuId);
        }

        // ---------------------------------------------
        // ViewModel
        // ---------------------------------------------
        /// <summary>
        /// 受注情報ビュー / バインドモデル
        /// EFエンティティ <see cref="KingsJuchu"/> と同等のプロパティを持つ
        /// </summary>
        public class JuchuViewModel
        {
            /// ==================================
            /// プロパティ
            /// ==================================

            /// <summary>
            /// 表示対象受注（エンティティ）
            /// </summary>
            public KingsJuchu Juchu { private get; set; } = new KingsJuchu();

            /// <summary>ID</summary>
            public long Id => Juchu.Id;

            /// <summary>プロジェクト番号</summary>
            [Display(Name = "プロジェクト番号")]
            public string? ProjectNo => Juchu.ProjectNo;

            /// <summary>受注番号</summary>
            [Display(Name = "受注番号")]
            public string? JuchuNo => Juchu.JuchuuNo;

            /// <summary>行番号</summary>
            [Display(Name = "行番号")]
            public string? JuchuGyoBangou => Juchu.JuchuuGyoNo?.ToString();

            /// <summary>契約状態</summary>
            [Display(Name = "契約状態")]
            public string? KeiyakuJotai => Juchu.KeiyakuJoutaiKbnName;

            /// <summary>施工部門</summary>
            [Display(Name = "施工部門")]
            public string SekoBumon => Juchu.Busyo.Name;

            /// <summary>受注日</summary>
            [Display(Name = "受注日")]
            public string JuchuYmd => Juchu.JucYmd.ToString("yyyy/MM/dd");

            /// <summary>契約先</summary>
            [Display(Name = "契約先")]
            public string? KeiNm => Juchu.KeiNm;

            /// <summary>受注先</summary>
            [Display(Name = "受注先")]
            public string? JuchuNm => Juchu.JucNm;

            /// <summary>物件名</summary>
            [Display(Name = "物件名")]
            public string Bukken => Juchu.Bukken;

            /// <summary>商品名</summary>
            [Display(Name = "商品名")]
            public string? ShohinName => Juchu.ShouhinName;

            /// <summary>費用種別</summary>
            [Display(Name = "費用種別")]
            public string HiyoShubetuName => Juchu.HiyouShubetuCdName;

            /// <summary>受注金額</summary>
            [Display(Name = "受注金額")]
            public string JuchuKin => Juchu.JucKin.ToString("N0");

            /// <summary>送担当者</summary>
            [Display(Name = "送担当者")]
            public string? OkrTanName => Juchu.OkrTanNm1;

            /// <summary>担当者</summary>
            [Display(Name = "担当者")]
            public string? TanName => Juchu.OkrTanNm1;

            /// <summary>受担当者</summary>
            [Display(Name = "受担当者")]
            public string? UkTanName => Juchu.UkeTanNm1;

            /// <summary>着工日</summary>
            [Display(Name = "着工日")]
            public string ChaYmd => Juchu.ChaYmd.ToString("yyyy/MM/dd");

            /// <summary>納期竣工</summary>
            [Display(Name = "納期竣工")]
            public string? NsyYmd => Juchu.NsyYmd?.ToString("yyyy/MM/dd");

            /// <summary>売上計画日</summary>
            [Display(Name = "売上計画日")]
            public string? KurYmd => Juchu.KurYmd?.ToString("yyyy/MM/dd");

            /// <summary>入金計画日</summary>
            [Display(Name = "入金計画日")]
            public string? KnyYmd => Juchu.KnyYmd?.ToString("yyyy/MM/dd");

            /// <summary>原価凍結</summary>
            [Display(Name = "原価凍結")]
            public string GenkaToketu => Juchu.IsGenkaToketu ? "凍結済み" : "未";

            /// <summary>原価凍結日</summary>
            [Display(Name = "原価凍結日")]
            public string? ToketuYmd => Juchu.ToketuYmd?.ToString("yyyy/MM/dd");

            /// <summary>備考</summary>
            [Display(Name = "備考")]
            public string? Biko => Juchu.Biko;

            /// <summary>契約状態区分</summary>
            [Display(Name = "契約状態区分")]
            public ContractClassification? KeiyakuJoutaiKbn => Juchu.KeiyakuJoutaiKbn;
        }
    }
}
