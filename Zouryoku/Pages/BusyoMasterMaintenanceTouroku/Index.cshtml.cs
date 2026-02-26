using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Extensions;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;

namespace Zouryoku.Pages.BusyoMasterMaintenanceTouroku
{
    /// <summary>
    ///  部署マスタ登録ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class InputModel : BasePageModel<InputModel>
    {
        public InputModel(ZouContext db, ILogger<InputModel> logger, IOptions<AppConfig> options)
            : base(db, logger, options) { }

        public override bool UseInputAssets { get; } = true;

        /// <summary>
        /// Viewモデル
        /// </summary>
        [BindProperty]
        public BusyoViewModel Input { get; set; } = new();

        /// <summary>
        /// システム日付
        /// </summary>
        private static readonly DateOnly Today = DateTime.Now.ToDateOnly();

        /// <summary>
        /// 9999/12/31
        /// </summary>
        private static readonly DateOnly MaxEndYmd = new(9999, 12, 31);

        /// <summary>
        /// 初期表示
        /// </summary>
        /// <param name="id">部署ID</param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (!id.HasValue)
            {
                // 新規作成モード
                // 新規作成時の初期値設定
                Input = new BusyoViewModel
                {
                    IsCreate = true,
                    ApplyDate = Today,
                    StartYmd = Today,
                    EndYmd = MaxEndYmd,
                };
            }
            else
            {
                // 更新モード
                // 更新対象部署マスタ取得
                var busyo = await GetBusyoForEditAsync(id.Value);

                // 存在チェック
                if (busyo == null)
                {
                    return RedirectToPage("/ErrorMessage", new { errorMessage = Const.ErrorSelectedDataNotExists });
                }

                // Viewモデルに変換
                Input = BusyoViewModel.FromEntity(busyo);
            }

            return Page();
        }

        /// <summary>
        /// 登録ボタン押下処理
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostRegisterAsync()
        {
            // 単項目チェック
            JsonResult? errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            // 業務ルールチェック
            await ValidateBusinessRulesAsync();
            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            // 新規作成・更新処理
            if (Input.IsCreate)
            {
                // 新規作成モード
                HandleCreate();
            }
            else
            {
                // 更新モード
                await HandleUpdateAsync();
            }

            // 排他制御つきSave
            await SaveWithConcurrencyCheckAsync(string.Format(Const.ErrorConflictReload, "部署"));

            // 排他エラー
            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            return Success();
        }

        /// <summary>
        /// 業務ルールチェック
        /// </summary>
        private async Task ValidateBusinessRulesAsync()
        {
            if (Input.IsCreate)
            {
                // 新規作成モード時チェック
                await ValidateCreateRulesAsync();
            }
            else
            {
                // 更新モード時チェック
                await ValidateUpdateRulesAsync();
            }
        }

        /// <summary>
        /// 新規作成モード時の業務ルールチェック
        /// </summary>
        private async Task ValidateCreateRulesAsync()
        {
            // 部署番号存在チェック
            var existsSameCode = await ExistsSameBusyoCodeAsync(Input.BusyoCode);
            if (existsSameCode)
            {
                ModelState.AddModelError(
                    nameof(Input.BusyoCode),
                    string.Format(Const.ErrorUnique, "部署番号", Input.BusyoCode)
                );
            }
        }

        /// <summary>
        /// 更新モード時の業務ルールチェック
        /// </summary>
        private async Task ValidateUpdateRulesAsync()
        {
            // 履歴対象の変更がある場合、適用開始日チェック
            var busyo = await GetBusyoByIdAsync(Input.BusyoId);

            if (HasRirekiChange(busyo) && Input.ApplyDate < Input.StartYmd)
            {
                ModelState.AddModelError(
                    nameof(Input.ApplyDate),
                    string.Format(Const.ErrorMoreThanDateTime, "適用開始日", "有効開始日")
                );
            }
        }

        /// <summary>
        /// 新規作成処理
        /// </summary>
        private void HandleCreate()
        {
            // 部署BASEマスタINSERT
            var busyoBase = InsertBusyoBase();

            // 部署マスタINSERT
            InsertBusyo(busyoBase);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private async Task HandleUpdateAsync()
        {
            // 更新対象部署BASEマスタ取得
            var busyoBase = await GetBusyoBaseByIdAsync(Input.BusyoBaseId);

            // 部署BASEマスタUPDATE
            UpdateBusyoBase(busyoBase);

            // 更新対象部署マスタ取得
            var busyo = await GetBusyoByIdAsync(Input.BusyoId);

            // 部署マスタUPDATE
            await UpdateBusyoAsync(busyoBase, busyo);
        }

        /// <summary>
        /// 部署マスタUPDATE
        /// </summary>
        /// <param name="busyoBase">部署BASEマスタ</param>
        /// <param name="busyo">部署マスタ</param>
        private async Task UpdateBusyoAsync(BusyoBasis busyoBase, Busyo busyo)
        {
            // 履歴対象の変更がない場合、もしくは適用開始日と有効開始日が同日の場合
            if (!HasRirekiChange(busyo) || Input.ApplyDate == Input.StartYmd)
            {
                // 単純更新
                UpdateBusyoWithoutRireki(busyo);
            }
            else
            {
                // 履歴あり更新
                await UpdateBusyoWithRirekiAsync(busyo, busyoBase);
            }
        }

        /// <summary>
        /// 履歴対象項目の変更を確認する
        /// </summary>
        /// <param name="busyo">DBデータ</param>
        /// <returns>変更フラグ</returns>
        private bool HasRirekiChange(Busyo busyo)
        {
            return
                busyo.Name != Input.BusyoName
                || busyo.OyaId != Input.OyaId
                || busyo.OyaCode != Input.OyaCode
                || busyo.IsActive != Input.IsActive
            ;
        }

        /// <summary>
        /// 部署BASEマスタINSERT
        /// </summary>
        /// <returns>新規登録した部署BASEマスタ</returns>
        private BusyoBasis InsertBusyoBase()
        {
            // 部署BASEマスタINSERT
            var busyoBase = new BusyoBasis
            {
                // ID自動採番
                Name = Input.BusyoName,
                BumoncyoId = Input.BumoncyoId
            };

            db.BusyoBases.Add(busyoBase);

            return busyoBase;
        }

        /// <summary>
        /// 部署BASEマスタUPDATE
        /// </summary>
        private void UpdateBusyoBase(BusyoBasis busyoBase)
        {
            // 更新項目
            busyoBase.BumoncyoId = Input.BumoncyoId;

            // 排他更新
            db.SetOriginalValue(busyoBase, e => e.Version, Input.BusyoBaseVersion);
        }

        /// <summary>
        /// 部署マスタINSERT
        /// </summary>
        /// <param name="busyoBase">部署BASEマスタ</param>
        private Busyo InsertBusyo(BusyoBasis busyoBase)
        {
            var busyo = new Busyo()
            {
                // ID自動採番
                Code = Input.BusyoCode,
                Name = Input.BusyoName,
                KanaName = Input.BusyoKanaName,
                OyaCode = Input.OyaCode,
                StartYmd = Input.ApplyDate,
                EndYmd = MaxEndYmd,
                Jyunjyo = 0,
                KasyoCode = Input.KasyoCode,
                KaikeiCode = Input.KaikeiCode,
                KeiriCode = Input.KeiriCode,
                IsActive = Input.IsActive,
                Ryakusyou = Input.BusyoRyakusyou,
                BusyoBase = busyoBase,// 自動採番されたBusyoBaseId
                OyaId = Input.OyaId,
                ShoninBusyoId = Input.ShoninBusyoId,
            };

            db.Busyos.Add(busyo);

            return busyo;
        }

        /// <summary>
        /// 履歴対象項目の変更がない場合の部署マスタ更新
        /// </summary>
        /// <param name="busyo">更新対象部署マスタ</param>
        private void UpdateBusyoWithoutRireki(Busyo busyo)
        {
            // 部署マスタUPDATE
            // 更新項目
            busyo.Code = Input.BusyoCode;
            busyo.Name = Input.BusyoName;
            busyo.KanaName = Input.BusyoKanaName;
            busyo.OyaCode = Input.OyaCode;
            busyo.StartYmd = Input.StartYmd;
            busyo.EndYmd = Input.EndYmd;
            busyo.KasyoCode = Input.KasyoCode;
            busyo.KaikeiCode = Input.KaikeiCode;
            busyo.KeiriCode = Input.KeiriCode;
            busyo.IsActive = Input.IsActive;
            busyo.Ryakusyou = Input.BusyoRyakusyou;
            busyo.OyaId = Input.OyaId;
            busyo.ShoninBusyoId = Input.ShoninBusyoId;

            // 排他更新
            db.SetOriginalValue(busyo, e => e.Version, Input.BusyoVersion);
        }

        /// <summary>
        /// 更新対象の部署マスタを無効化した上で、同一部署コードで新規登録する。
        /// </summary>
        /// <param name="oldBusyo">無効とする部署マスタ</param>
        /// <param name="baseEntity">部署BASEマスタ</param>
        /// <returns></returns>
        private async Task UpdateBusyoWithRirekiAsync(Busyo oldBusyo, BusyoBasis baseEntity)
        {
            // 部署マスタUPDATE(無効化)
            DisableOldBusyo(oldBusyo);

            // 部署マスタINSERT
            var newBusyo = InsertBusyo(baseEntity);

            // 部署に所属する社員を取得
            var syains = await GetTargetSyainsAsync(oldBusyo.Id);

            // 社員マスタUPDATE(無効化)＋INSERT(新規登録)
            UpdateSyainWithRireki(syains, newBusyo);
        }

        /// <summary>
        /// 部署マスタを無効化する
        /// </summary>
        /// <param name="busyo">無効とする部署マスタ</param>
        private void DisableOldBusyo(Busyo busyo)
        {
            busyo.EndYmd = Input.ApplyDate.AddDays(-1);
            busyo.IsActive = false;

            db.SetOriginalValue(busyo, e => e.Version, Input.BusyoVersion);
        }

        /// <summary>
        /// 社員マスタUPDATE(無効化)＋INSERT(新規登録)
        /// </summary>
        /// <param name="syains">対象の社員マスタリスト</param>
        /// <param name="newBusyo">新規登録する部署マスタ</param>
        private void UpdateSyainWithRireki(List<Syain> syains, Busyo newBusyo)
        {
            // 社員マスタUPDATE(無効化)
            ApplySyainsToDisable(syains);

            // 社員マスタINSERT(新規登録) - 一括登録
            InsertSyains(syains, newBusyo);
        }

        /// <summary>
        /// 社員マスタを無効化する
        /// </summary>
        /// <param name="syains">無効とする社員マスタリスト</param>
        private void ApplySyainsToDisable(List<Syain> syains)
        {
            // 社員マスタUPDATE(無効化)
            syains.ForEach(s => s.EndYmd = Input.ApplyDate.AddDays(-1));
        }

        /// <summary>
        /// 社員マスタを新規登録する
        /// 新規登録の元になる社員マスタの部署IDを新規登録する部署マスタのIDに変更して一括登録
        /// </summary>
        /// <param name="syains">新規登録の元になる社員マスタリスト</param>
        /// <param name="newBusyo">新規登録する部署マスタ</param>
        private void InsertSyains(List<Syain> syains, Busyo newBusyo)
        {
            var newSyains = syains.Select(s => CreateNewSyain(s, newBusyo)).ToList();
            db.Syains.AddRange(newSyains);
        }

        /// <summary>
        /// 新規登録する社員マスタを作成する
        /// </summary>
        /// <param name="oldSyain">新規登録の元になる社員マスタ</param>
        /// <param name="newBusyo">新規登録する部署マスタ</param>
        /// <returns>新規登録する社員マスタ</returns>
        private Syain CreateNewSyain(Syain oldSyain, Busyo newBusyo)
        {
            return new Syain
            {
                // ID自動採番
                // 引継ぎ項目
                Code = oldSyain.Code,
                Name = oldSyain.Name,
                KanaName = oldSyain.KanaName,
                Seibetsu = oldSyain.Seibetsu,
                BusyoCode = oldSyain.BusyoCode,
                SyokusyuCode = oldSyain.SyokusyuCode,
                SyokusyuBunruiCode = oldSyain.SyokusyuBunruiCode,
                NyuusyaYmd = oldSyain.NyuusyaYmd,
                Kyusyoku = oldSyain.Kyusyoku,
                SyucyoSyokui = oldSyain.SyucyoSyokui,
                KingsSyozoku = oldSyain.KingsSyozoku,
                KaisyaCode = oldSyain.KaisyaCode,
                IsGenkaRendou = oldSyain.IsGenkaRendou,
                EMail = oldSyain.EMail,
                KeitaiMail = oldSyain.KeitaiMail,
                Kengen = oldSyain.Kengen,
                Jyunjyo = oldSyain.Jyunjyo,
                Retired = oldSyain.Retired,
                GyoumuTypeId = oldSyain.GyoumuTypeId,
                PhoneNumber = oldSyain.PhoneNumber,
                SyainBaseId = oldSyain.SyainBaseId,
                KintaiZokuseiId = oldSyain.KintaiZokuseiId,
                UserRoleId = oldSyain.UserRoleId,

                // 更新項目
                StartYmd = Input.ApplyDate,
                EndYmd = MaxEndYmd,
                Busyo = newBusyo
            };
        }

        // ------------------------------
        // 画面表示用のクエリ
        // ------------------------------
        /// <summary>
        /// 画面表示用に編集対象の部署マスタを取得する
        /// </summary>
        /// <param name="busyoId">部署ID</param>
        /// <returns>編集対象の部署マスタ</returns>
        private async Task<Busyo?> GetBusyoForEditAsync(long busyoId) =>
            await db.Busyos
                .Include(b => b.BusyoBase)              // 部署 → 部署BASE → 部門長（社員）
                    .ThenInclude(bb => bb.Bumoncyo)
                .Include(b => b.ShoninBusyo)            // 部署 → 承認部署（自己参照）
                .Include(b => b.Oya)                    // 部署 → 親部署（自己参照）
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == busyoId);

        // ------------------------------
        // 更新処理用のクエリ
        // ------------------------------
        /// <summary>
        /// 部署BASEマスタをIDで取得する
        /// </summary>
        /// <param name="id">部署BASEID</param>
        /// <returns>部署BASEマスタ</returns>
        private async Task<BusyoBasis> GetBusyoBaseByIdAsync(long id) =>
            await db.BusyoBases.SingleAsync(b => b.Id == id);

        /// <summary>
        /// 部署マスタをIDで取得する
        /// </summary>
        /// <param name="id">部署ID</param>
        /// <returns>部署マスタ</returns>
        private async Task<Busyo> GetBusyoByIdAsync(long id) =>
            await db.Busyos.SingleAsync(b => b.Id == id);

        /// <summary>
        /// 部署コードによる存在チェック
        /// </summary>
        /// <param name="busyoCode">部署コード</param>
        /// <returns>
        /// 存在する場合：true
        /// 存在しない場合：false
        /// </returns>
        private async Task<bool> ExistsSameBusyoCodeAsync(string busyoCode) =>
            await db.Busyos
                .AnyAsync(b => b.Code == busyoCode);

        /// <summary>
        /// 対象社員リストを取得する
        /// 部署IDとシステム日付で絞り込み
        /// </summary>
        /// <param name="busyoId">部署ID</param>
        /// <returns>対象社員リスト</returns>
        private async Task<List<Syain>> GetTargetSyainsAsync(long busyoId) =>
            await db.Syains
                .Where(s => s.BusyoId == busyoId &&
                            s.StartYmd <= Today &&
                            Today <= s.EndYmd)
                .ToListAsync();

        /// <summary>
        /// 部署情報のViewモデル
        /// </summary>
        public class BusyoViewModel
        {
            /// <summary>
            /// 新規作成モード(非表示)
            /// </summary>
            public bool IsCreate { get; set; }

            /// <summary>ID(非表示)</summary>
            public long BusyoId { get; set; }

            /// <summary>部署BASE_ID(非表示)</summary>
            public long BusyoBaseId { get; set; }

            /// <summary>部署番号</summary>
            [Display(Name = "部署番号")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            [StringLength(3, ErrorMessage = Const.ErrorLength)]
            public string BusyoCode { get; set; } = string.Empty;

            /// <summary>部署名称</summary>
            [Display(Name = "部署名称")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            [StringLength(32, ErrorMessage = Const.ErrorLength)]
            public string BusyoName { get; set; } = string.Empty;

            /// <summary>部署名称カナ</summary>
            [Display(Name = "部署名称カナ")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            [StringLength(64, ErrorMessage = Const.ErrorLength)]
            public string BusyoKanaName { get; set; } = string.Empty;

            /// <summary>部署略称</summary>
            [Display(Name = "部署略称")]
            [StringLength(10, ErrorMessage = Const.ErrorLength)]
            public string? BusyoRyakusyou { get; set; }

            /// <summary>親部署名称(表示のみ)</summary>
            [Display(Name = "親部署")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public string? OyaName { get; set; }

            /// <summary>親部署ID(非表示)</summary>
            public long? OyaId { get; set; }

            /// <summary>親部署番号(非表示)</summary>
            [Display(Name = "親部署")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            [StringLength(3, ErrorMessage = Const.ErrorLength)]
            public string OyaCode { get; set; } = string.Empty;

            /// <summary>適用開始日</summary>
            [Display(Name = "適用開始日")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public DateOnly ApplyDate { get; set; }

            /// <summary>有効開始日(表示のみ)</summary>
            [Display(Name = "有効開始日")]
            public DateOnly StartYmd { get; set; }

            /// <summary>有効終了日(表示のみ)</summary>
            [Display(Name = "有効終了日")]
            public DateOnly EndYmd { get; set; }

            /// <summary>箇所コード</summary>
            [Display(Name = "箇所コード")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            [StringLength(2, ErrorMessage = Const.ErrorLength)]
            public string KasyoCode { get; set; } = string.Empty;

            /// <summary>会計コード</summary>
            [Display(Name = "会計コード")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            [StringLength(3, ErrorMessage = Const.ErrorLength)]
            public string KaikeiCode { get; set; } = string.Empty;

            /// <summary>経理コード</summary>
            [Display(Name = "経理コード")]
            [StringLength(2, ErrorMessage = Const.ErrorLength)]
            public string? KeiriCode { get; set; }

            /// <summary>アクティブフラグ</summary>
            [Display(Name = "アクティブフラグ")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public bool IsActive { get; set; }

            /// <summary>部門長名(表示のみ)</summary>
            [Display(Name = "部門長")]
            public string? BumoncyoName { get; set; }

            /// <summary>部門長ID(非表示)</summary>
            [Display(Name = "部門長ID")]
            public long? BumoncyoId { get; set; }

            /// <summary>承認部署名(表示のみ)</summary>
            [Display(Name = "承認部署")]
            public string? ShoninBusyoName { get; set; }

            /// <summary>承認部署ID(非表示)</summary>
            [Display(Name = "承認部署ID")]
            public long? ShoninBusyoId { get; set; }

            public uint BusyoVersion { get; set; }

            public uint? BusyoBaseVersion { get; set; }

            /// <summary>
            /// エンティティからViewモデルを作成する
            /// </summary>
            /// <param name="busyo">部署エンティティ</param>
            /// <returns>部署Viewモデル</returns>
            public static BusyoViewModel FromEntity(Busyo busyo)
            {
                return new BusyoViewModel
                {
                    IsCreate = false,
                    BusyoId = busyo.Id,
                    BusyoBaseId = busyo.BusyoBaseId,
                    BusyoCode = busyo.Code,
                    BusyoName = busyo.Name,
                    BusyoKanaName = busyo.KanaName,
                    BusyoRyakusyou = busyo.Ryakusyou,
                    OyaName = busyo.Oya?.Name,
                    OyaId = busyo.OyaId,
                    OyaCode = busyo.OyaCode,
                    ApplyDate = Today,
                    StartYmd = busyo.StartYmd,
                    EndYmd = busyo.EndYmd,
                    KasyoCode = busyo.KasyoCode,
                    KaikeiCode = busyo.KaikeiCode,
                    KeiriCode = busyo.KeiriCode,
                    IsActive = busyo.IsActive,
                    BumoncyoName = busyo.BusyoBase?.Bumoncyo?.Name,
                    BumoncyoId = busyo.BusyoBase?.BumoncyoId,
                    ShoninBusyoName = busyo.ShoninBusyo?.Name,
                    ShoninBusyoId = busyo.ShoninBusyoId,

                    BusyoVersion = busyo.Version,
                    BusyoBaseVersion = busyo.BusyoBase?.Version,
                };

            }
        }
    }
}
