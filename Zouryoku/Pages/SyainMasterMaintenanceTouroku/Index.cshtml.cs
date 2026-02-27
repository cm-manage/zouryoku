using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Model;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;

namespace Zouryoku.Pages.SyainMasterMaintenanceTouroku;

/// <summary>
/// 社員マスタ登録画面モデル
/// </summary>
[FunctionAuthorizationAttribute]
public class IndexModel : BasePageModel<IndexModel>
{
    /// <summary>
    /// 有効終了日の最大値
    /// </summary>
    private static readonly DateOnly MaxEndYmd = new(9999, 12, 31);

    public IndexModel(
        ZouContext db,
        ILogger<IndexModel> logger,
        IOptions<AppConfig> options,
        ICompositeViewEngine viewEngine,
        TimeProvider? timeProvider = null)
        : base(db, logger, options, viewEngine, timeProvider) { }

    public override bool UseInputAssets { get; } = true;

    /// <summary>
    /// 入力モデル
    /// </summary>
    [BindProperty]
    public SyainInputModel Input { get; set; } = new();

    /// <summary>業務種別選択肢</summary>
    public IEnumerable<SelectListItem> GyoumuTypeOptions { get; private set; } = [];

    /// <summary>勤怠属性選択肢</summary>
    public IEnumerable<SelectListItem> KintaiZokuseiOptions { get; private set; } = [];

    /// <summary>利用会社選択肢</summary>
    public IEnumerable<SelectListItem> CompanyOptions { get; private set; } = [];

    /// <summary>ロール選択肢</summary>
    public IEnumerable<SelectListItem> RoleOptions { get; private set; } = [];

    /// <summary>出張職位選択肢</summary>
    public IEnumerable<SelectListItem> SyucyoSyokuiOptions { get; private set; } = [];

    /// <summary>
    /// 初期表示
    /// </summary>
    /// <param name="id">社員BASEマスタID</param>
    public async Task<IActionResult> OnGetAsync(long? id)
    {
        await LoadOptionsAsync();
        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);

        if (!id.HasValue)
        {
            Input = SyainInputModel.CreateForCreate(today);
            return Page();
        }

        var syain = await GetCurrentSyainByBaseIdAsync(id.Value);

        if (syain is null)
        {
            ModelState.AddModelError(string.Empty, Const.ErrorSelectedDataNotExists);
            Input = SyainInputModel.CreateForCreate(today);
            return Page();
        }

        var yuukyuuZan = await db.YuukyuuZans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SyainBaseId == id.Value);

        var overtimeExcessLimit = await db.OvertimeExcessLimits
            .AsNoTracking()
            .Where(x => x.SyainBaseId == id.Value)
            .OrderByDescending(x => x.DisabledYm)
            .FirstOrDefaultAsync();

        Input = SyainInputModel.FromEntity(
            syain,
            yuukyuuZan,
            overtimeExcessLimit,
            DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime));

        return Page();
    }

    /// <summary>
    /// 登録
    /// </summary>
    public async Task<IActionResult> OnPostRegisterAsync()
    {
        ValidateOvertimeExcessLimitInput();

        JsonResult? errorJson = ModelState.ErrorJson();
        if (errorJson is not null)
        {
            return errorJson;
        }

        var busyo = await ValidateAndGetBusyoAsync();
        if (busyo is null)
        {
            return CommonErrorResponse();
        }

        if (Input.IsCreate)
        {
            var existsSameCode = await db.Syains.AnyAsync(s => s.Code == Input.Code);
            if (existsSameCode)
            {
                ModelState.AddModelError(
                    nameof(Input.Code),
                    string.Format(Const.ErrorUnique, "社員番号", Input.Code));
                return CommonErrorResponse();
            }

            await HandleCreateAsync(busyo);
        }
        else
        {
            var syain = await db.Syains.SingleOrDefaultAsync(s => s.Id == Input.Id);
            if (syain is null)
            {
                ModelState.AddModelError(nameof(Input.Id), $"syainId:{Input.Id} の社員が見つかりません。");
                return CommonErrorResponse();
            }

            var syainBase = await db.SyainBases.SingleOrDefaultAsync(sb => sb.Id == Input.SyainBaseId);
            if (syainBase is null)
            {
                ModelState.AddModelError(nameof(Input.SyainBaseId), $"syainBaseId:{Input.SyainBaseId} の" +
                    $"社員が見つかりません。");
                return CommonErrorResponse();
            }

            var existsSameCode = await db.Syains.AnyAsync(s =>
                s.Code == Input.Code
                && s.SyainBaseId != Input.SyainBaseId);
            if (existsSameCode)
            {
                ModelState.AddModelError(
                    nameof(Input.Code),
                    string.Format(Const.ErrorUnique, "社員番号", Input.Code));
                return CommonErrorResponse();
            }

            var applyDate = Input.StartDate!.Value;
            if (HasRirekiChange(syain, busyo) && applyDate < syain.StartYmd)
            {
                ModelState.AddModelError(
                    nameof(Input.StartDate),
                    string.Format(Const.ErrorMoreThanDateTime, "適用開始日", "有効開始日"));
                return CommonErrorResponse();
            }

            await HandleUpdateAsync(syain, syainBase, busyo, applyDate);
        }

        await db.SaveChangesAsync();
        return Success();
    }

    /// <summary>
    /// ロール既定権限取得
    /// </summary>
    /// <param name="roleId">ロールID</param>
    public async Task<IActionResult> OnGetRoleDefaultsAsync(long roleId)
    {
        var role = await db.UserRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role is null)
        {
            return ErrorJson(string.Format(Const.ErrorNotExists, "ロール", roleId));
        }

        await LoadOptionsAsync();
        Input.UserRoleId = roleId;
        SetPermissionChecks(role.Kengen);

        var html = await PartialToJsonAsync("_AuthoritySection", this);
        return SuccessJson(data: html);
    }

    private async Task<Busyo?> ValidateAndGetBusyoAsync()
    {
        if (!Input.BusyoId.HasValue)
        {
            ModelState.AddModelError(nameof(Input.BusyoId), string.Format(Const.ErrorSelectRequired, "部署"));
            return null;
        }

        var busyo = await db.Busyos
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == Input.BusyoId.Value);

        if (busyo is null)
        {
            ModelState.AddModelError(nameof(Input.BusyoId), string.Format(Const.ErrorNotExists, "部署",
                Input.BusyoId.Value));
            return null;
        }

        if (Input.KintaiZokuseiId.HasValue)
        {
            var existsKintaiZokusei = await db.KintaiZokuseis.AnyAsync(x => x.Id == Input.KintaiZokuseiId.Value);
            if (!existsKintaiZokusei)
            {
                ModelState.AddModelError(
                    nameof(Input.KintaiZokuseiId),
                    string.Format(Const.ErrorNotExists, "勤怠属性", Input.KintaiZokuseiId.Value));
            }
        }

        if (Input.UserRoleId.HasValue)
        {
            var existsRole = await db.UserRoles.AnyAsync(x => x.Id == Input.UserRoleId.Value);
            if (!existsRole)
            {
                ModelState.AddModelError(
                    nameof(Input.UserRoleId),
                    string.Format(Const.ErrorNotExists, "ロール", Input.UserRoleId.Value));
            }
        }

        if (Input.GyoumuTypeId.HasValue)
        {
            var existsGyoumuType = await db.GyoumuTypes.AnyAsync(x => x.Id == Input.GyoumuTypeId.Value);
            if (!existsGyoumuType)
            {
                ModelState.AddModelError(
                    nameof(Input.GyoumuTypeId),
                    string.Format(Const.ErrorNotExists, "業務種別", Input.GyoumuTypeId.Value));
            }
        }

        return ModelState.IsValid ? busyo : null;
    }

    private async Task HandleCreateAsync(Busyo busyo)
    {
        var syainBase = new SyainBasis
        {
            Name = Input.Name,
            Code = Input.Code
        };
        db.SyainBases.Add(syainBase);

        var syain = new Syain
        {
            SyainBase = syainBase,
            SyokusyuCode = 0,
            SyokusyuBunruiCode = 0,
            Jyunjyo = 0
        };

        ApplyInputToSyain(
            syain,
            busyo,
            Input.StartDate!.Value,
            MaxEndYmd);

        db.Syains.Add(syain);

        db.YuukyuuZans.Add(new YuukyuuZan
        {
            SyainBase = syainBase,
            Wariate = Input.Wariate ?? 0m,
            Kurikoshi = Input.Kurikoshi ?? 0m,
            Syouka = Input.Syouka ?? 0m,
            HannitiKaisuu = Input.HannitiKaisuu,
            KeikakuYukyuSu = 0,
            KeikakuTokukyuSu = 0
        });

        await UpsertOvertimeExcessLimitAsync(0, syainBase);
    }

    private async Task HandleUpdateAsync(Syain syain, SyainBasis syainBase, Busyo busyo, DateOnly applyDate)
    {
        syainBase.Name = Input.Name;
        syainBase.Code = Input.Code;

        if (!HasRirekiChange(syain, busyo) || applyDate == syain.StartYmd)
        {
            var startYmd = Input.StartYmd ?? syain.StartYmd;
            var endYmd = Input.EndYmd ?? syain.EndYmd;
            ApplyInputToSyain(syain, busyo, startYmd, endYmd);
        }
        else
        {
            syain.EndYmd = applyDate.AddDays(-1);

            var newSyain = new Syain
            {
                SyainBaseId = syain.SyainBaseId,
                SyokusyuCode = syain.SyokusyuCode,
                SyokusyuBunruiCode = syain.SyokusyuBunruiCode,
                Jyunjyo = syain.Jyunjyo
            };

            ApplyInputToSyain(newSyain, busyo, applyDate, MaxEndYmd);
            db.Syains.Add(newSyain);
        }

        await UpsertYuukyuuZanAsync(syainBase.Id);
        await UpsertOvertimeExcessLimitAsync(syainBase.Id);
    }

    private async Task UpsertYuukyuuZanAsync(long syainBaseId)
    {
        var yuukyuuZan = await db.YuukyuuZans.SingleOrDefaultAsync(x => x.SyainBaseId == syainBaseId);
        if (yuukyuuZan is null)
        {
            db.YuukyuuZans.Add(new YuukyuuZan
            {
                SyainBaseId = syainBaseId,
                Wariate = Input.Wariate ?? 0m,
                Kurikoshi = Input.Kurikoshi ?? 0m,
                Syouka = Input.Syouka ?? 0m,
                HannitiKaisuu = Input.HannitiKaisuu,
                KeikakuYukyuSu = 0,
                KeikakuTokukyuSu = 0
            });
            return;
        }

        yuukyuuZan.Wariate = Input.Wariate ?? 0m;
        yuukyuuZan.Kurikoshi = Input.Kurikoshi ?? 0m;
        yuukyuuZan.Syouka = Input.Syouka ?? 0m;
    }

    private async Task UpsertOvertimeExcessLimitAsync(long syainBaseId, SyainBasis? syainBase = null)
    {
        if (!Input.IsOvertimeExcessLimitStart)
        {
            if (syainBaseId <= 0)
            {
                return;
            }

            var existing = await db.OvertimeExcessLimits
                .Where(x => x.SyainBaseId == syainBaseId)
                .ToListAsync();

            if (existing.Count > 0)
            {
                db.OvertimeExcessLimits.RemoveRange(existing);
            }
            return;
        }

        if (!TryParseOvertimeExcessLimitYm(Input.OvertimeExcessLimitYm, out var disabledYm))
        {
            // 入力検証済みだが、万一パースできない場合は何も更新しない。
            return;
        }

        OvertimeExcessLimit? existingLimit = null;
        List<OvertimeExcessLimit> duplicateLimits = [];

        if (syainBaseId > 0)
        {
            var existingLimits = await db.OvertimeExcessLimits
                .Where(x => x.SyainBaseId == syainBaseId)
                .OrderBy(x => x.Id)
                .ToListAsync();

            if (existingLimits.Count > 0)
            {
                existingLimit = existingLimits[0];
                duplicateLimits = existingLimits.Skip(1).ToList();
            }
        }

        if (existingLimit is null)
        {
            var newLimit = new OvertimeExcessLimit
            {
                DisabledYm = disabledYm
            };

            if (syainBase is not null)
            {
                newLimit.SyainBase = syainBase;
            }
            else
            {
                newLimit.SyainBaseId = syainBaseId;
            }

            db.OvertimeExcessLimits.Add(newLimit);
            return;
        }

        existingLimit.DisabledYm = disabledYm;
        if (duplicateLimits.Count > 0)
        {
            db.OvertimeExcessLimits.RemoveRange(duplicateLimits);
        }
    }

    private void ValidateOvertimeExcessLimitInput()
    {
        if (!Input.IsOvertimeExcessLimitStart)
        {
            Input.OvertimeExcessLimitYm = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(Input.OvertimeExcessLimitYm))
        {
            ModelState.AddModelError(
                nameof(Input.OvertimeExcessLimitYm),
                string.Format(Const.ErrorRequired, "残業超過制限開始年月"));
            return;
        }

        if (!TryParseOvertimeExcessLimitYm(Input.OvertimeExcessLimitYm, out var parsedYm))
        {
            ModelState.AddModelError(
                nameof(Input.OvertimeExcessLimitYm),
                string.Format(Const.ErrorInvalidInput, "残業超過制限開始年月"));
            return;
        }

        Input.OvertimeExcessLimitYm = parsedYm.ToString("yyyy/MM", CultureInfo.InvariantCulture);
    }

    private static bool TryParseOvertimeExcessLimitYm(string? rawValue, out DateOnly parsedYm)
    {
        parsedYm = default;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        var normalized = rawValue.Trim().Replace("-", "/");
        var candidates = new[]
        {
            normalized,
            normalized + "/01"
        };
        var formats = new[] { "yyyy/MM", "yyyy/M", "yyyy/MM/dd" };

        foreach (var candidate in candidates)
        {
            if (DateOnly.TryParseExact(
                candidate,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate))
            {
                parsedYm = new DateOnly(parsedDate.Year, parsedDate.Month, 1);
                return true;
            }
        }

        return false;
    }

    private void ApplyInputToSyain(Syain syain, Busyo busyo, DateOnly startYmd, DateOnly endYmd)
    {
        syain.Code = Input.Code;
        syain.Name = Input.Name;
        syain.KanaName = Input.KanaName;
        syain.Seibetsu = Input.Seibetsu!.Value;
        syain.BusyoCode = busyo.Code;
        syain.NyuusyaYmd = Input.NyuusyaYmd!.Value;
        syain.StartYmd = startYmd;
        syain.EndYmd = endYmd;
        syain.Kyusyoku = Input.Kyusyoku!.Value;
        syain.SyucyoSyokui = Input.SyucyoSyokui!.Value;
        syain.KingsSyozoku = Input.KingsSyozoku;
        syain.KaisyaCode = Input.KaisyaCode!.Value;
        syain.IsGenkaRendou = Input.IsGenkaRendou;
        syain.EMail = EmptyToNull(Input.EMail);
        syain.KeitaiMail = EmptyToNull(Input.KeitaiMail);
        syain.Kengen = BuildKengen();
        syain.Retired = Input.Retired;
        syain.GyoumuTypeId = Input.GyoumuTypeId;
        syain.PhoneNumber = EmptyToNull(Input.PhoneNumber);
        syain.BusyoId = busyo.Id;
        syain.KintaiZokuseiId = Input.KintaiZokuseiId!.Value;
        syain.UserRoleId = Input.UserRoleId!.Value;
    }

    private bool HasRirekiChange(Syain syain, Busyo busyo) =>
        syain.Name != Input.Name
        || syain.BusyoId != busyo.Id
        || syain.Kyusyoku != Input.Kyusyoku!.Value
        || syain.SyucyoSyokui != Input.SyucyoSyokui!.Value
        || syain.KintaiZokuseiId != Input.KintaiZokuseiId!.Value
        || syain.IsGenkaRendou != Input.IsGenkaRendou
        || syain.KaisyaCode != Input.KaisyaCode!.Value;

    private EmployeeAuthority BuildKengen()
    {
        var flags = new[]
        {
            Input.Perm1Checked,
            Input.Perm2Checked,
            Input.Perm3Checked,
            Input.Perm4Checked,
            Input.Perm5Checked,
            Input.Perm6Checked,
            Input.Perm7Checked,
            Input.Perm8Checked,
            Input.Perm9Checked,
            Input.Perm10Checked,
            Input.Perm11Checked,
            Input.Perm12Checked,
            Input.Perm13Checked,
            Input.Perm14Checked,
            Input.Perm15Checked,
        };

        var kengen = EmployeeAuthority.None;
        for (var i = 0; i < flags.Length; i++)
        {
            if (flags[i])
            {
                kengen |= (EmployeeAuthority)(1 << i);
            }
        }

        return kengen;
    }

    private static bool HasAuthority(EmployeeAuthority authority, int bitIndex)
        => authority.HasFlag((EmployeeAuthority)(1 << bitIndex));

    private void SetPermissionChecks(EmployeeAuthority authority)
    {
        Input.Perm1Checked = HasAuthority(authority, 0);
        Input.Perm2Checked = HasAuthority(authority, 1);
        Input.Perm3Checked = HasAuthority(authority, 2);
        Input.Perm4Checked = HasAuthority(authority, 3);
        Input.Perm5Checked = HasAuthority(authority, 4);
        Input.Perm6Checked = HasAuthority(authority, 5);
        Input.Perm7Checked = HasAuthority(authority, 6);
        Input.Perm8Checked = HasAuthority(authority, 7);
        Input.Perm9Checked = HasAuthority(authority, 8);
        Input.Perm10Checked = HasAuthority(authority, 9);
        Input.Perm11Checked = HasAuthority(authority, 10);
        Input.Perm12Checked = HasAuthority(authority, 11);
        Input.Perm13Checked = HasAuthority(authority, 12);
        Input.Perm14Checked = HasAuthority(authority, 13);
        Input.Perm15Checked = HasAuthority(authority, 14);
    }

    private async Task<Syain?> GetCurrentSyainByBaseIdAsync(long syainBaseId) =>
        //await db.Syains
        //    .AsNoTracking()
        //    .Include(s => s.Busyo)
        //    .FirstOrDefaultAsync(s =>
        //        s.SyainBaseId == syainBaseId &&
        //        s.EndYmd == MaxEndYmd);

        await db.Syains
            .AsNoTracking()
            .Include(s => s.Busyo)
            .FirstOrDefaultAsync(s =>
                s.SyainBaseId == syainBaseId);

    private async Task LoadOptionsAsync()
    {
        var gyoumuTypeItems = await db.GyoumuTypes
            .AsNoTracking()
            .OrderBy(x => x.Jyunjyo)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();

        var kintaiZokuseiItems = await db.KintaiZokuseis
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();

        var roleItems = await db.UserRoles
            .AsNoTracking()
            .OrderBy(x => x.Jyunjo)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();

        var companyItems = Enum.GetValues<NippousCompanyCode>()
            .Select(x => new SelectListItem
            {
                Value = ((short)x).ToString(),
                Text = (x as Enum).GetDisplayName() ?? x.ToString()
            })
            .ToList();

        var syucyoSyokuiItems = Enum.GetValues<BusinessTripRole>()
            .Select(x => new SelectListItem
            {
                Value = ((short)x).ToString(),
                Text = (x as Enum).GetDisplayName() ?? x.ToString()
            })
            .ToList();

        GyoumuTypeOptions = AddEmptyOption(gyoumuTypeItems, "未選択");
        KintaiZokuseiOptions = AddEmptyOption(kintaiZokuseiItems, "選択してください");
        RoleOptions = AddEmptyOption(roleItems, "選択してください");
        CompanyOptions = AddEmptyOption(companyItems, "選択してください");
        SyucyoSyokuiOptions = AddEmptyOption(syucyoSyokuiItems, "選択してください");
    }

    private static IEnumerable<SelectListItem> AddEmptyOption(IEnumerable<SelectListItem> items, string emptyText)
    {
        var list = new List<SelectListItem>
        {
            new() { Value = string.Empty, Text = emptyText }
        };
        list.AddRange(items);
        return list;
    }

    private static string? EmptyToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>
    /// 画面入力モデル
    /// </summary>
    public class SyainInputModel
    {
        public bool IsCreate { get; set; }

        public long Id { get; set; }

        public long SyainBaseId { get; set; }

        [Display(Name = "社員番号")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [StringLength(5, ErrorMessage = Const.ErrorLength)]
        [RegularExpression(@"^\d+$", ErrorMessage = Const.ErrorNumber)]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "社員氏名")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [StringLength(32, ErrorMessage = Const.ErrorLength)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "社員氏名カナ")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [StringLength(32, ErrorMessage = Const.ErrorLength)]
        public string KanaName { get; set; } = string.Empty;

        [Display(Name = "入社年月日")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public DateOnly? NyuusyaYmd { get; set; }

        [Display(Name = "性別")]
        [Required(ErrorMessage = Const.ErrorSelectRequired)]
        public char? Seibetsu { get; set; }

        [Display(Name = "部署")]
        [Required(ErrorMessage = Const.ErrorSelectRequired)]
        public long? BusyoId { get; set; }

        public string BusyoName { get; set; } = string.Empty;

        public string BusyoCode { get; set; } = string.Empty;

        [Display(Name = "業務種別")]
        public long? GyoumuTypeId { get; set; }

        [Display(Name = "適用開始日")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public DateOnly? StartDate { get; set; }

        [Display(Name = "有効開始日")]
        public DateOnly? StartYmd { get; set; }

        [Display(Name = "有効終了日")]
        public DateOnly? EndYmd { get; set; }

        [Display(Name = "級職")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [Range(0, short.MaxValue, ErrorMessage = Const.ErrorNumberRangeMoreThanEqual)]
        public short? Kyusyoku { get; set; }

        [Display(Name = "出張職位")]
        [Required(ErrorMessage = Const.ErrorSelectRequired)]
        public BusinessTripRole? SyucyoSyokui { get; set; }

        [Display(Name = "KINGS所属")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [StringLength(5, ErrorMessage = Const.ErrorLength)]
        [RegularExpression(@"^\d+$", ErrorMessage = Const.ErrorNumber)]
        public string KingsSyozoku { get; set; } = string.Empty;

        [Display(Name = "勤怠属性")]
        [Required(ErrorMessage = Const.ErrorSelectRequired)]
        public long? KintaiZokuseiId { get; set; }

        public bool IsGenkaRendou { get; set; }

        [Display(Name = "利用会社")]
        [Required(ErrorMessage = Const.ErrorSelectRequired)]
        public short? KaisyaCode { get; set; }

        [Display(Name = "Email")]
        [StringLength(50, ErrorMessage = Const.ErrorLength)]
        [EmailAddress(ErrorMessage = Const.ErrorInvalidInput)]
        public string? EMail { get; set; }

        [Display(Name = "携帯Mail")]
        [StringLength(50, ErrorMessage = Const.ErrorLength)]
        [EmailAddress(ErrorMessage = Const.ErrorInvalidInput)]
        public string? KeitaiMail { get; set; }

        [Display(Name = "携帯番号")]
        [StringLength(15, ErrorMessage = Const.ErrorLength)]
        [RegularExpression(@"^\d{3}-\d{4}-\d{4}$", ErrorMessage = Const.ErrorInvalidInput)]
        public string? PhoneNumber { get; set; }

        public bool Retired { get; set; }

        [Display(Name = "有給割当日数")]
        [Range(typeof(decimal), "0", "99", ErrorMessage = Const.ErrorNumberRangeLessThanEqual)]
        public decimal? Wariate { get; set; }

        [Display(Name = "有給繰越日数")]
        public decimal? Kurikoshi { get; set; }

        [Display(Name = "有給消化日数")]
        public decimal? Syouka { get; set; }

        [Display(Name = "半日有給消化回数")]
        public short HannitiKaisuu { get; set; }

        [Display(Name = "残業超過制限開始")]
        public bool IsOvertimeExcessLimitStart { get; set; }

        [Display(Name = "残業超過制限開始年月")]
        public string? OvertimeExcessLimitYm { get; set; }

        [Display(Name = "ロール")]
        [Required(ErrorMessage = Const.ErrorSelectRequired)]
        public long? UserRoleId { get; set; }

        public long Kengen { get; set; }

        public bool Perm1Checked { get; set; }
        public bool Perm2Checked { get; set; }
        public bool Perm3Checked { get; set; }
        public bool Perm4Checked { get; set; }
        public bool Perm5Checked { get; set; }
        public bool Perm6Checked { get; set; }
        public bool Perm7Checked { get; set; }
        public bool Perm8Checked { get; set; }
        public bool Perm9Checked { get; set; }
        public bool Perm10Checked { get; set; }
        public bool Perm11Checked { get; set; }
        public bool Perm12Checked { get; set; }
        public bool Perm13Checked { get; set; }
        public bool Perm14Checked { get; set; }
        public bool Perm15Checked { get; set; }

        public static SyainInputModel CreateForCreate(DateOnly today) => new()
        {
            IsCreate = true,
            StartDate = today,
            StartYmd = today,
            EndYmd = MaxEndYmd,
            Seibetsu = '1',
            IsGenkaRendou = false,
            Retired = false,
            IsOvertimeExcessLimitStart = false,
        };

        public static SyainInputModel FromEntity(
            Syain syain,
            YuukyuuZan? yuukyuuZan,
            OvertimeExcessLimit? overtimeExcessLimit,
            DateOnly today) => new()
        {
            IsCreate = false,
            Id = syain.Id,
            SyainBaseId = syain.SyainBaseId,
            Code = syain.Code,
            Name = syain.Name,
            KanaName = syain.KanaName,
            NyuusyaYmd = syain.NyuusyaYmd,
            Seibetsu = syain.Seibetsu,
            BusyoId = syain.BusyoId,
            BusyoName = syain.Busyo.Name,
            BusyoCode = syain.Busyo.Code,
            GyoumuTypeId = syain.GyoumuTypeId,
            StartDate = today,
            StartYmd = syain.StartYmd,
            EndYmd = syain.EndYmd,
            Kyusyoku = syain.Kyusyoku,
            SyucyoSyokui = syain.SyucyoSyokui,
            KingsSyozoku = syain.KingsSyozoku,
            KintaiZokuseiId = syain.KintaiZokuseiId,
            IsGenkaRendou = syain.IsGenkaRendou,
            KaisyaCode = syain.KaisyaCode,
            EMail = syain.EMail,
            KeitaiMail = syain.KeitaiMail,
            PhoneNumber = syain.PhoneNumber,
            Retired = syain.Retired,
            Wariate = yuukyuuZan?.Wariate,
            Kurikoshi = yuukyuuZan?.Kurikoshi,
            Syouka = yuukyuuZan?.Syouka,
            HannitiKaisuu = yuukyuuZan?.HannitiKaisuu ?? 0,
            IsOvertimeExcessLimitStart = overtimeExcessLimit is not null,
            OvertimeExcessLimitYm = overtimeExcessLimit?.DisabledYm.ToString("yyyy/MM"),
            UserRoleId = syain.UserRoleId,
            Kengen = (long)syain.Kengen,
            Perm1Checked = HasAuthority(syain.Kengen, 0),
            Perm2Checked = HasAuthority(syain.Kengen, 1),
            Perm3Checked = HasAuthority(syain.Kengen, 2),
            Perm4Checked = HasAuthority(syain.Kengen, 3),
            Perm5Checked = HasAuthority(syain.Kengen, 4),
            Perm6Checked = HasAuthority(syain.Kengen, 5),
            Perm7Checked = HasAuthority(syain.Kengen, 6),
            Perm8Checked = HasAuthority(syain.Kengen, 7),
            Perm9Checked = HasAuthority(syain.Kengen, 8),
            Perm10Checked = HasAuthority(syain.Kengen, 9),
            Perm11Checked = HasAuthority(syain.Kengen, 10),
            Perm12Checked = HasAuthority(syain.Kengen, 11),
            Perm13Checked = HasAuthority(syain.Kengen, 12),
            Perm14Checked = HasAuthority(syain.Kengen, 13),
            Perm15Checked = HasAuthority(syain.Kengen, 14),
        };
    }
}

