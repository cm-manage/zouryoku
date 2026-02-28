using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Extensions;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;

namespace Zouryoku.Pages.Ankens
{
    /// <summary>
    /// 案件入力（新規 / 編集兼用）ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class InputModel : BasePageModel<InputModel>
    {
        // ---------------------------------------------
        // 定数
        // ---------------------------------------------
        private const string AnkenInfoLabel = "案件情報";
        private const string SelfDepartmentLabel = "自部署";
        private const string JuchuLabel = "受注";
        private const string JissekiLabel = "実績";

        private const long NewIdValue = 0;

        // ---------------------------------------------
        // DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        public InputModel(ZouContext db, ILogger<InputModel> logger, IOptions<AppConfig> options, TimeProvider? timeProvider = null)
            : base(db, logger, options, timeProvider)
        { }

        // ---------------------------------------------
        // プライベートプロパティ
        // ---------------------------------------------
        /// <summary>
        /// 本日の日付
        /// </summary>
        private DateOnly Today => timeProvider.Today();

        // ---------------------------------------------
        // BindProperty（フォームバインド用）
        // ---------------------------------------------
        /// <summary>
        /// 入力対象案件 (ViewModel)
        /// </summary>
        [BindProperty]
        public AnkenInputModel Anken { get; set; } = new();

        // ---------------------------------------------
        // 通常のプロパティ（画面表示用）
        // ---------------------------------------------

        public override bool UseInputAssets => true;

        /// <summary>
        /// 編集モードかどうか
        /// </summary>
        public bool IsEdit => NewIdValue < Anken?.Id;

        /// <summary>
        /// 受注種類セレクトリスト
        /// </summary>
        public List<SelectListItem> JyutyuSyuruiOptions { get; set; } = [];

        // ---------------------------------------------
        // OnGet
        // ---------------------------------------------

        /// <summary>
        ///画面初期表示（新規/編集）
        /// </summary>
        public async Task<IActionResult> OnGetAsync(long? id)
        {
            // 受注種類の取得
            JyutyuSyuruiOptions = await FetchJyutyuSyuruiOptionsAsync();

            // 案件情報の取得
            if (id.HasValue)
            {
                Anken? existing = await FetchAnkenAsync(id.Value);

                if (existing is null)
                {
                    return RedirectToPage("/ErrorMessage", new { errorMessage = Const.ErrorSelectedDataNotExists });
                }
                Anken = AnkenInputModel.FromEntity(existing);
            }

            // 画面を表示
            return Page();
        }

        // ---------------------------------------------
        // OnPost
        // ---------------------------------------------

        /// <summary>
        /// 入力送信（新規/更新）
        /// </summary>
        public async Task<IActionResult> OnPostRegisterAsync()
        {
            // 単項目チェック
            JsonResult? errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            // ログインユーザー情報取得
            Syain user = LoginInfo.User;

            // 登録前チェック
            await ValidateRegisterAsync(user);

            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            Anken? entity;

            // 案件情報の登録・更新
            if (IsEdit)
            {
                // 更新
                entity = await db.Ankens.FirstOrDefaultAsync(x => x.Id == Anken.Id);
                if (entity is null)
                {
                    ModelState.AddModelError
                        (string.Empty,
                        string.Format(Const.ErrorNotFound, AnkenInfoLabel, Anken.Id));
                    return CommonErrorResponse();
                }

                // 更新項目反映
                entity.Name = Anken.AnkenName.Trim();
                entity.Naiyou = Anken.Naiyou;
                entity.KokyakuKaisyaId = Anken.KokyakuKaisyaId;
                entity.KingsJuchuId = Anken.KingsJuchuId;
                entity.JyutyuSyuruiId = Anken.JyutyuSyuruiId;
                entity.SyainBaseId = Anken.SyainBaseId;
                entity.SearchName = StringUtil.NormalizeString(Anken.AnkenName.Trim());

                db.SetOriginalValue(entity, e => e.Version, Anken.Version);
            }
            else
            {
                // 新規登録
                entity = Anken.ToEntity();
                db.Ankens.Add(entity);
            }

            // 案件参照履歴保存
            await AnkenSansyouRirekisUtil.MaintainAnkenSansyouRirekiAsync(db, entity, user.SyainBaseId, timeProvider.Now());

            // DB保存
            await SaveWithConcurrencyCheckAsync(string.Format(Const.ErrorConflictReload, AnkenInfoLabel));

            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            // 画面に遷移
            return SuccessJson(data: entity.Id.ToString());
        }

        /// <summary>
        /// 削除送信（編集モード時のみ）
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            if (!IsEdit)
            {
                return BadRequest();
            }

            // 単項目チェック
            JsonResult? errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            // 削除前チェック
            await ValidateDeleteAsync();

            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            // 削除処理
            Anken? target = await db.Ankens
                .Include(data => data.AnkenSansyouRirekis)
                .FirstOrDefaultAsync(x => x.Id == Anken.Id);
            if (target is null)
            {
                ModelState.AddModelError
                    (string.Empty,
                    string.Format(Const.ErrorNotFound, AnkenInfoLabel, Anken.Id));
                return CommonErrorResponse();
            }

            // 紐づく案件情報参照履歴を削除
            if (0 < target.AnkenSansyouRirekis.Count)
            {
                db.AnkenSansyouRirekis.RemoveRange(target.AnkenSansyouRirekis);
            }

            db.Ankens.Remove(target);
            db.SetOriginalValue(target, e => e.Version, Anken.Version);

            // DB保存
            await SaveWithConcurrencyCheckAsync(string.Format(Const.ErrorConflictReload, AnkenInfoLabel));

            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            // 画面に遷移
            return Success();
        }

        // ---------------------------------------------
        // プライベートメソッド
        // ---------------------------------------------

        /// <summary>
        /// 受注種類ドロップダウンの取得
        /// </summary>
        /// <returns>受注種類ドロップダウン情報</returns>
        private async Task<List<SelectListItem>> FetchJyutyuSyuruiOptionsAsync()
        {
            return await db.JyutyuSyuruis
                .Select(j => new SelectListItem(j.Name, j.Id.ToString()))
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// 表示案件情報取得
        /// </summary>
        /// <param name="ankenId">案件ID</param>
        /// <returns>案件情報</returns>
        private async Task<Anken?> FetchAnkenAsync(long ankenId)
        {
            return await db.Ankens
                .Include(a => a.SyainBase)
                .ThenInclude(sb => sb!.Syains.Where(s => s.StartYmd <= Today && Today <= s.EndYmd))
                .Include(a => a.KingsJuchu)
                .Include(a => a.KokyakuKaisya)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == ankenId);
        }

        /// <summary>
        /// 登録前サーバ側検証
        /// </summary>
        /// <param name="user">ログインユーザー</param>
        /// <returns></returns>
        private async Task ValidateRegisterAsync(Syain user)
        {
            // KINGS受注
            await ValidateKingsJuchuAsync(user);

            // 顧客会社：存在チェック
            await ValidateExistsAsync(db.KokyakuKaishas.AsNoTracking(),
                data => data.Id == Anken.KokyakuKaisyaId);

            // 社員（任意入力）：存在チェック
            if (Anken.SyainBaseId is not null)
            {
                await ValidateExistsAsync(db.Syains.AsNoTracking(),
                    data => data.SyainBaseId == Anken.SyainBaseId);
            }

            // 受注種類（任意入力）：存在チェック
            if (Anken.JyutyuSyuruiId is not null)
            {
                await ValidateExistsAsync(db.JyutyuSyuruis.AsNoTracking(),
                    data => data.Id == Anken.JyutyuSyuruiId);
            }
        }

        /// <summary>
        /// KINGS受注チェック
        /// </summary>
        /// <param name="user">ログインユーザー</param>
        private async Task ValidateKingsJuchuAsync(Syain user)
        {

            // 存在チェック
            KingsJuchu? existing = await db.KingsJuchus
                .AsNoTracking()
                .FirstOrDefaultAsync(data => data.Id == Anken.KingsJuchuId);

            if (existing is null)
            {
                ModelState.AddModelError(string.Empty, Const.ErrorSelectedDataNotExists);
                return;
            }

            // 自部署チェック
            if (string.IsNullOrEmpty(existing.SekouBumonCd) || existing.SekouBumonCd != user.BusyoCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    string.Format(Const.ErrorRequiredSubItem, SelfDepartmentLabel, JuchuLabel));
            }
        }

        /// <summary>
        /// 存在チェック共通処理
        /// </summary>
        /// <typeparam name="TSource">エンティティ型</typeparam>
        /// <param name="source">クエリソース</param>
        /// <param name="predicate">条件</param>
        private async Task ValidateExistsAsync<TSource>(
            IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate)
            where TSource : class
        {
            bool exists = await source.AnyAsync(predicate);
            if (!exists)
            {
                ModelState.AddModelError(string.Empty, Const.ErrorSelectedDataNotExists);
            }
        }

        /// <summary>
        /// 削除前サーバー側検証
        /// </summary>
        /// <returns></returns>
        private async Task ValidateDeleteAsync()
        {
            // 日報実績⇔案件：存在チェック
            bool existsNippou = await db.NippouAnkens
                .AsNoTracking()
                .AnyAsync(data => data.AnkensId == Anken.Id);

            if (existsNippou)
            {
                ModelState.AddModelError(string.Empty, string.Format(Const.ErrorLinked, AnkenInfoLabel, JissekiLabel));
            }
        }
    }

    /// <summary>
    /// 案件入力用ビューモデル
    /// </summary>
    public class AnkenInputModel
    {
        [Display(Name = "案件")]
        public long Id { get; set; }

        [Display(Name = "受注情報")]
        public long? KingsJuchuId { get; set; }

        [Display(Name = "受注種類")]
        public long? JyutyuSyuruiId { get; set; }

        [Display(Name = "顧客情報")]
        public long? KokyakuKaisyaId { get; set; }

        [Display(Name = "弊社責任者")]
        public long? SyainBaseId { get; set; }

        [Display(Name = "受注工番")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public string? ProjectNo { get; set; }

        [Display(Name = "受注番号")]
        public string? JuchuuNo { get; set; }

        [Display(Name = "受注行番号")]
        public string? JuchuuGyoNo { get; set; }

        [Display(Name = "受注件名")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public string? Bukken { get; set; }

        [Display(Name = "案件名")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [StringLength(128, ErrorMessage = Const.ErrorLength)]
        public string AnkenName { get; set; } = string.Empty;

        [Display(Name = "顧客情報")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public string? KokyakuName { get; set; }

        [Display(Name = "弊社責任者")]
        public string? SyainName { get; set; }

        [Display(Name = "案件内容")]
        [StringLength(2000, ErrorMessage = Const.ErrorLength)]
        public string? Naiyou { get; set; }

        public uint Version { get; set; }

        /// <summary>
        /// ビューモデルからエンティティへ変換
        /// </summary>
        public Anken ToEntity()
            => new()
            {
                Id = Id,
                KingsJuchuId = KingsJuchuId,
                KokyakuKaisyaId = KokyakuKaisyaId,
                JyutyuSyuruiId = JyutyuSyuruiId,
                SyainBaseId = SyainBaseId,
                Name = AnkenName.Trim(),
                SearchName = StringUtil.NormalizeString(AnkenName.Trim()),
                Naiyou = Naiyou
            };

        /// <summary>
        /// エンティティからビューモデルへ変換
        /// </summary>
        public static AnkenInputModel FromEntity(Anken entity)
            => new()
            {
                Id = entity.Id,
                KingsJuchuId = entity.KingsJuchuId,
                JyutyuSyuruiId = entity.JyutyuSyuruiId,
                KokyakuKaisyaId = entity.KokyakuKaisyaId,
                KokyakuName = entity.KokyakuKaisya?.Name,
                SyainBaseId = entity.SyainBaseId,
                ProjectNo = entity.KingsJuchu?.ProjectNo,
                JuchuuNo = entity.KingsJuchu?.JuchuuNo,
                JuchuuGyoNo = entity.KingsJuchu?.JuchuuGyoNo?.ToString(),
                Bukken = entity.KingsJuchu?.Bukken,
                AnkenName = entity.Name,
                SyainName = entity.SyainBase?.Syains.Select(s => s.Name).FirstOrDefault(),
                Naiyou = entity.Naiyou,
                Version = entity.Version
            };
    }
}