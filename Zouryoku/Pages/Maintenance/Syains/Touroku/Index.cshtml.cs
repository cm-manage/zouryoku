using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Model.Enums.EmployeeAuthority;
using static Model.Enums.EmployeeWorkType;
using static Zouryoku.Pages.YukyuKeikakuJigyobuShonin.IndexModel;
// TODO: using staticを使用してEmployeeAuthority.~~~を省略する

namespace Zouryoku.Pages.Maintenance.Syains.Touroku;

/// <summary>
/// 社員マスタ登録画面モデル
/// </summary>
[FunctionAuthorization]
public class IndexModel : BasePageModel<IndexModel>
{
    /// <summary>
    /// 社員権限Enumの一覧を返します。Noneは含みません。
    /// </summary>
    public IEnumerable<EmployeeAuthority> AllAuthorities =>
        Enum.GetValues<EmployeeAuthority>()
            .Where(authority => authority != None);

    /// <summary>
    /// 有効終了日の最大値
    /// </summary>
    private static readonly DateOnly MaxEndYmd = new(9999, 12, 31);

    public IndexModel(
        ZouContext db,
        ILogger<IndexModel> logger,
        IOptions<AppConfig> options,
        ICompositeViewEngine viewEngine,
        // デフォルト値は不要
        TimeProvider? timeProvider = null)
        : base(db, logger, options, viewEngine, timeProvider) { }

    public override bool UseInputAssets => true;

    /// <summary>
    /// 入力モデル
    /// </summary>
    [BindProperty]
    public SyainInputModel Input { get; set; } = new();

    // TODO: セレクトボックス作成にSelectListItemは使わない方針

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
        var today = timeProvider.Today();

        if (!id.HasValue)
        {
            Input = SyainInputModel.CreateForCreate(today);
            return Page();
        }

        var syain = await GetCurrentSyainByBaseIdAsync(id.Value);

        if (syain is null)
        {
            // TODO: 共通エラーページに飛ばす
            ModelState.AddModelError(string.Empty, Const.ErrorSelectedDataNotExists);
            Input = SyainInputModel.CreateForCreate(today);
            return Page();
        }

        var yuukyuuZan = syain?.SyainBase.YuukyuuZans.FirstOrDefault();

        var overtimeExcessLimit = syain?.SyainBase.OvertimeExcessLimits
            .OrderByDescending(x => x.DisabledYm)
            .FirstOrDefault();

        // TODO: FromEntityは廃止された
        Input = SyainInputModel.FromEntity(
            syain,
            //yuukyuuZan,
            //overtimeExcessLimit,
            today);

        return Page();
    }

    /// <summary>
    /// 登録
    /// </summary>
    public async Task<IActionResult> OnPostRegisterAsync()
    {
        // 単項目チェック
        JsonResult? errorJson = ModelState.ErrorJson();
        if (errorJson is not null)
        {
            return errorJson;
        }

        ValidateOvertimeExcessLimitInput();

        // TODO: 他のValidationと分ける必要はある？
        if (!ModelState.IsValid)
        {
            return ModelState.ErrorJson()!;
        }

        var busyo = await ValidateAndGetBusyoAsync();
        if (busyo is null)
        {
            return ModelState.ErrorJson()!;
        }

        if (Input.IsCreate)
        {
            var existsSameCode = await db.Syains.AnyAsync(s => s.Code == Input.Code);
            if (existsSameCode)
            {
                ModelState.AddModelError(
                    nameof(Input.Code),
                    string.Format(Const.ErrorUnique, "社員番号", Input.Code));

                return ModelState.ErrorJson()!;
            }

            await HandleCreateAsync(busyo);
        }
        else
        {
            var syain = await db.Syains.SingleOrDefaultAsync(s => s.Id == Input.Id);
            if (syain is null)
            {
                // TODO: エラーメッセージはConst化
                ModelState.AddModelError(nameof(Input.Id), string.Format(Const.ErrorSyainNonExistance, "syainId", Input.Id));

                return ModelState.ErrorJson()!;
            }

            var syainBase = await db.SyainBases.SingleOrDefaultAsync(sb => sb.Id == Input.SyainBaseId);
            if (syainBase is null)
            {
                // TODO: エラーメッセージはConst化
                ModelState.AddModelError(nameof(Input.SyainBaseId), string.Format(Const.ErrorSyainNonExistance, "syainBaseId", Input.SyainBaseId));

                return ModelState.ErrorJson()!;
            }

            var existsSameCode = await db.Syains.AnyAsync(s =>
                s.Code == Input.Code
                && s.SyainBaseId != Input.SyainBaseId);
            if (existsSameCode)
            {
                ModelState.AddModelError(
                    nameof(Input.Code),
                    string.Format(Const.ErrorUnique, "社員番号", Input.Code));

                return ModelState.ErrorJson()!;
            }

            // TODO: StartDateをnull許容にする意味は？
            var applyDate = Input.StartDate!.Value;
            if (HasRirekiChange(syain, busyo, Input) && applyDate < syain.StartYmd)
            {
                ModelState.AddModelError(
                    nameof(Input.StartDate),
                    string.Format(Const.ErrorMoreThanDateTime, "適用開始日", "有効開始日"));

                return ModelState.ErrorJson()!;
            }

            syainBase.Name = Input.Name;
            syainBase.Code = Input.Code;
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
        // TODO: First？
        var role = await db.UserRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role is null)
        {
            // TODO: ModelState.ErrorJson()にする
            return ErrorJson(string.Format(Const.ErrorNotExists, "ロール", roleId));
        }

        await LoadOptionsAsync();
        Input.UserRoleId = roleId;
        Input.SelectedAuthorities = Enum.GetValues<EmployeeAuthority>()
            .Where(a => a != EmployeeAuthority.None && role.Kengen.HasFlag(a))
            .ToList();

        var html = await PartialToJsonAsync("_AuthoritySection", this);
        return SuccessJson(data: html);
    }

    /// <summary>
    /// 部署マスタ存在チェック
    /// 入力された部署IDの必須チェックと存在チェックを行い、問題なければ部署エンティティを返す。
    /// 併せて、勤怠属性・ロール・業務種別の選択値が存在するかを検証し、エラーがあれば ModelState に追加する。
    /// </summary>
    private async Task<Busyo?> ValidateAndGetBusyoAsync()
    {
        if (!Input.BusyoId.HasValue)
        {
            ModelState.AddModelError(nameof(Input.BusyoId), string.Format(Const.ErrorSelectRequired, "部署"));
            return null;
        }

        // TODO: First?
        var busyo = await db.Busyos
            .SingleOrDefaultAsync(b => b.Id == Input.BusyoId.Value);
        if (busyo is null)
        {
            ModelState.AddModelError(nameof(Input.BusyoId), string.Format(Const.ErrorNotExists, "部署",
                Input.BusyoId.Value));
            return null;
        }

        if (Input.KintaiZokuseiId.HasValue)
        {
            // TODO: EmployeeWorkTypeを使う
            var value = Input.KintaiZokuseiId.Value;
            if (!Enum.IsDefined(typeof(EmployeeWorkType), value))
            {
                ModelState.AddModelError(
                    nameof(Input.KintaiZokuseiId),
                    string.Format(Const.ErrorNotExists, "勤怠属性", value));
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

    /// <summary>
    /// 社員マスタの新規登録処理
    /// 社員Baseを作成し、最大終了日を持つ初期社員レコードを追加する。
    /// あわせて有給残情報を初期化し、残業超過制限の開始設定がある場合は登録する。
    /// </summary>
    /// <param name="busyo">事前検証済みの部署エンティティ</param>
    private async Task HandleCreateAsync(Busyo busyo)
    {
        var syainBase = new SyainBasis
        {
            Name = Input.Name,
            Code = Input.Code
        };
        await db.SyainBases.AddAsync(syainBase);

        // TODO: マジックナンバーを定数化する
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
            // TODO: なぜNULL許容？
            Input.StartDate!.Value,
            MaxEndYmd);

        await db.Syains.AddAsync(syain);

        await db.YuukyuuZans.AddAsync(new YuukyuuZan
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

    /// <summary>
    /// 社員マスタの更新処理。
    /// 履歴対象項目の変更有無に応じて、既存行の更新または履歴追加を行う。
    /// 有給残および残業超過制限も併せて更新する。
    /// </summary>
    /// <param name="syain">現在有効な社員レコード</param>
    /// <param name="syainBase">社員Base</param>
    /// <param name="busyo">部署エンティティ</param>
    /// <param name="applyDate">適用開始日</param>
    private async Task HandleUpdateAsync(Syain syain, SyainBasis syainBase, Busyo busyo, DateOnly applyDate)
    {
        // TODO: 引数を書き換えるのは基本的に危険 やめる 参照1なので、各ロジックをハンドラ内でやれば十分では？
        //syainBase.Name = Input.Name;
        //syainBase.Code = Input.Code;

        if (!HasRirekiChange(syain, busyo, Input) || applyDate == syain.StartYmd)
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
            await db.Syains.AddAsync(newSyain);
        }

        await UpsertYuukyuuZanAsync(syainBase.Id);
        await UpsertOvertimeExcessLimitAsync(syainBase.Id);
    }

    /// <summary>
    /// 有給残情報のUpsert処理。
    /// 対象社員Baseに有給残レコードが存在しない場合は新規追加し、
    /// 存在する場合は割当・繰越・消化日数を入力値で更新する。
    /// </summary>
    /// <param name="syainBaseId">社員Baseの識別子</param>
    private async Task UpsertYuukyuuZanAsync(long syainBaseId)
    {
        var yuukyuuZan = await db.YuukyuuZans.SingleOrDefaultAsync(x => x.SyainBaseId == syainBaseId);
        if (yuukyuuZan is null)
        {
            await db.YuukyuuZans.AddAsync(new YuukyuuZan
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

    /// <summary>
    /// 残業超過制限のUpsert処理。
    /// 無効時は削除、有効時は開始年月を設定して追加または更新する。
    /// 重複レコードが存在する場合は整理する。
    /// </summary>
    /// <param name="syainBaseId">社員BaseID</param>
    /// <param name="syainBase">新規登録時の社員Base（更新時はnull可）</param>
    private async Task UpsertOvertimeExcessLimitAsync(long syainBaseId, SyainBasis? syainBase = null)
    {
        if (!Input.IsOvertimeExcessLimitStart)
        {
            // 新規時はまだDBに存在しないため削除不要
            if (syainBaseId == 0)
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

            await db.OvertimeExcessLimits.AddAsync(newLimit);
            return;
        }

        existingLimit.DisabledYm = disabledYm;
        if (duplicateLimits.Count > 0)
        {
            db.OvertimeExcessLimits.RemoveRange(duplicateLimits);
        }
    }

    /// <summary>
    /// 残業超過制限開始設定の入力検証を行う。
    /// フラグと開始年月の整合性を確認し、必要に応じて値を正規化する。
    /// </summary>
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

        Input.OvertimeExcessLimitYm = parsedYm.YMSlash();
    }

    /// <summary>
    /// 開始年月文字列、DateOnlyに 変換処理
    /// 年月指定の場合は当月1日として扱う。
    /// </summary>
    /// <param name="rawValue">入力値</param>
    /// <param name="parsedYm">変換後の年月（1日固定）</param>
    /// <returns>変換成功時 true</returns>
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

    // TODO: 引数を関数内で書き換えない 戻り値でSyainインスタンスを出せばいい もしくはExtensions化
    private void ApplyInputToSyain(Syain syain, Busyo busyo, DateOnly startYmd, DateOnly endYmd)
    {
        syain.Code = Input.Code;
        syain.Name = Input.Name;
        syain.KanaName = Input.KanaName;
        // TODO: !を付けるならNULL許容にする必要はないのでは？
        syain.Seibetsu = Input.Seibetsu!.Value;
        syain.BusyoCode = busyo.Code;
        // TODO: !を付けるならNULL許容にする必要はないのでは？
        syain.NyuusyaYmd = Input.NyuusyaYmd!.Value;
        syain.StartYmd = startYmd;
        syain.EndYmd = endYmd;
        // TODO: !を付けるならNULL許容にする必要はないのでは？
        syain.Kyusyoku = Input.Kyusyoku!.Value;
        // TODO: !を付けるならNULL許容にする必要はないのでは？
        syain.SyucyoSyokui = Input.SyucyoSyokui!.Value;
        syain.KingsSyozoku = Input.KingsSyozoku;
        // TODO: !を付けるならNULL許容にする必要はないのでは？
        syain.KaisyaCode = Input.KaisyaCode!.Value;
        syain.IsGenkaRendou = Input.IsGenkaRendou;
        syain.EMail = EmptyToNull(Input.EMail);
        syain.KeitaiMail = EmptyToNull(Input.KeitaiMail);
        //syain.Kengen = BuildKengen();
        syain.Kengen = Input.SelectedAuthorities.Aggregate(EmployeeAuthority.None, (acc, x) => acc | x);
        syain.Retired = Input.Retired;
        syain.GyoumuTypeId = Input.GyoumuTypeId;
        syain.PhoneNumber = EmptyToNull(Input.PhoneNumber);
        // TODO: Busyoにbusyoを直接指定する方が安全
        syain.Busyo = busyo;
        syain.BusyoCode = busyo.Code;
        // TODO: syain.KintaiZokusei.CodeにEmployeeWorkTypeを指定する
        syain.KintaiZokuseiId = (short)Input.KintaiZokuseiId.Value;
        // TODO: !を付けるならNULL許容にする必要はないのでは？
        syain.UserRoleId = Input.UserRoleId!.Value;
    }

    // TODO: Inputは引数で受け取る（可読性が悪い）
    /// <summary>
    /// 履歴対象項目の変更有無の判定処理
    /// </summary>
    /// <param name="syain">現在有効な社員</param>
    /// <param name="busyo">部署エンティティ</param>
    /// <returns>変更ありの場合 true</returns>
    private bool HasRirekiChange(Syain syain, Busyo busyo, SyainInputModel input) =>
        syain.Name != input.Name
        || syain.BusyoId != busyo.Id
        || syain.Kyusyoku != input.Kyusyoku!.Value
        || syain.SyucyoSyokui != input.SyucyoSyokui!.Value
        || syain.KintaiZokuseiId != (short)input.KintaiZokuseiId!.Value
        || syain.IsGenkaRendou != input.IsGenkaRendou
        || syain.KaisyaCode != input.KaisyaCode!.Value;

    /// <summary>
    /// 社員BaseIDに紐づく社員レコード取得処理
    /// 有給残と残業超過制限もincludeで取得
    /// ※現在有効行を取得する場合は、有効終了日条件を追加すること。
    /// </summary>
    /// <param name="syainBaseId">社員BaseのID</param>
    /// <returns>該当社員レコード。存在しない場合は null。</returns>
    private async Task<Syain?> GetCurrentSyainByBaseIdAsync(long syainBaseId) =>
        await db.Syains
            .AsNoTracking()
            .Include(s => s.Busyo)
            .Include(s => s.SyainBase)
                .ThenInclude(b => b.YuukyuuZans)
            .Include(s => s.SyainBase)
                .ThenInclude(b => b.OvertimeExcessLimits)
            .FirstOrDefaultAsync(s => s.SyainBaseId == syainBaseId);

    // TODO: 削除する ビュー側で作成する SelectListItemは使用しない
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

    // TODO: viewでやる 削除
    private static IEnumerable<SelectListItem> AddEmptyOption(IEnumerable<SelectListItem> items, string emptyText)
    {
        var list = new List<SelectListItem>
        {
            new() { Value = string.Empty, Text = emptyText }
        };
        list.AddRange(items);
        return list;
    }

    // TODO: 関数名以上の機能がある
    private static string? EmptyToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>
    /// 画面入力モデル
    /// </summary>
    public class SyainInputModel
    {
        // TODO: RequiredでNULL許容なのはどういう意味ですか？

        /// <summary>
        /// 新規登録モードを示すフラグ。
        /// 社員Baseおよび社員レコードを新規作成する場合は true、
        /// 既存レコードを更新する場合は false。
        /// </summary>
        public bool IsCreate { get; set; }

        /// <summary>
        /// 社員マスタ（syainsテーブル）の主キーID。
        /// 更新処理時に使用。新規登録時は 0。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 社員Base（syain_basesテーブル）の主キーID。
        /// 履歴管理のID、社員レコードを紐づける。
        /// 更新処理時に使用
        /// </summary>
        public long SyainBaseId { get; set; }

        /// <summary>
        /// 社員番号
        /// </summary>
        [Display(Name = "社員番号")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [StringLength(5, ErrorMessage = Const.ErrorLength)]
        [RegularExpression(@"^\d+$", ErrorMessage = Const.ErrorNumber)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 社員氏名
        /// </summary>
        [Display(Name = "社員氏名")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [StringLength(32, ErrorMessage = Const.ErrorLength)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 社員氏名カナ
        /// </summary>
        [Display(Name = "社員氏名カナ")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [StringLength(32, ErrorMessage = Const.ErrorLength)]
        public string KanaName { get; set; } = string.Empty;

        /// <summary>
        /// 入社年月日
        /// </summary>
        [Display(Name = "入社年月日")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public DateOnly? NyuusyaYmd { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        [Display(Name = "性別")]
        [Required(ErrorMessage = Const.ErrorSelectRequired)]
        public char? Seibetsu { get; set; }

        /// <summary>
        /// 部署
        /// </summary>
        [Display(Name = "部署")]
        [Required(ErrorMessage = Const.ErrorSelectRequired)]
        public long? BusyoId { get; set; }

        /// <summary>
        /// 部署名
        /// </summary>
        public string BusyoName { get; set; } = string.Empty;

        /// <summary>
        /// 部署コード
        /// </summary>
        public string BusyoCode { get; set; } = string.Empty;

        /// <summary>
        /// 業務種別
        /// </summary>
        [Display(Name = "業務種別")]
        public long? GyoumuTypeId { get; set; }

        /// <summary>
        /// 適用開始日
        /// </summary>
        [Display(Name = "適用開始日")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public DateOnly? StartDate { get; set; }

        /// <summary>
        /// 有効開始日
        /// </summary>
        [Display(Name = "有効開始日")]
        public DateOnly? StartYmd { get; set; }

        /// <summary>
        /// 有効終了日
        /// </summary>
        [Display(Name = "有効終了日")]
        public DateOnly? EndYmd { get; set; }

        /// <summary>
        /// 級職
        /// </summary>
        [Display(Name = "級職")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [Range(0, short.MaxValue, ErrorMessage = Const.ErrorNumberRangeMoreThanEqual)]
        public short? Kyusyoku { get; set; }

        /// <summary>
        /// 出張職位
        /// </summary>
        [Display(Name = "出張職位")]
        [Required(ErrorMessage = Const.ErrorSelectRequired)]
        public BusinessTripRole? SyucyoSyokui { get; set; }

        /// <summary>
        /// KINGS所属
        /// </summary>
        [Display(Name = "KINGS所属")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [StringLength(5, ErrorMessage = Const.ErrorLength)]
        [RegularExpression(@"^\d+$", ErrorMessage = Const.ErrorNumber)]
        public string KingsSyozoku { get; set; } = string.Empty;

        /// <summary>
        /// 勤怠属性
        /// </summary>
        [Display(Name = "勤怠属性")]
        [Required(ErrorMessage = Const.ErrorSelectRequired)]
        public EmployeeWorkType? KintaiZokuseiId { get; set; }

        /// <summary>
        /// 原価連動フラグ
        /// </summary>
        public bool IsGenkaRendou { get; set; }

        /// <summary>
        /// 利用会社
        /// </summary>
        [Display(Name = "利用会社")]
        [Required(ErrorMessage = Const.ErrorSelectRequired)]
        public short? KaisyaCode { get; set; }

        // TODO: EMailAddress属性を使用する
        /// <summary>
        /// Email
        /// </summary>
        [Display(Name = "Email")]
        [StringLength(50, ErrorMessage = Const.ErrorLength)]
        [EmailAddress(ErrorMessage = Const.ErrorInvalidInput)]
        public string? EMail { get; set; }

        // TODO: EMailAddress属性を使用する
        /// <summary>
        /// 携帯Mail
        /// </summary>
        [Display(Name = "携帯Mail")]
        [StringLength(50, ErrorMessage = Const.ErrorLength)]
        [EmailAddress(ErrorMessage = Const.ErrorInvalidInput)]
        public string? KeitaiMail { get; set; }

        // TODO: Phone属性を検討する
        /// <summary>
        /// 携帯番号
        /// </summary>
        [Display(Name = "携帯番号")]
        [StringLength(15, ErrorMessage = Const.ErrorLength)]
        [RegularExpression(@"^\d{3}-\d{4}-\d{4}$", ErrorMessage = Const.ErrorInvalidInput)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 退職フラグ
        /// </summary>
        public bool Retired { get; set; }

        /// <summary>
        /// 有給割当日数
        /// </summary>
        [Display(Name = "有給割当日数")]
        [Range(typeof(decimal), "0", "99", ErrorMessage = Const.ErrorNumberRangeLessThanEqual)]
        public decimal? Wariate { get; set; }

        /// <summary>
        /// 有給繰越日数
        /// </summary>
        [Display(Name = "有給繰越日数")]
        public decimal? Kurikoshi { get; set; }

        /// <summary>
        /// 有給消化日数
        /// </summary>
        [Display(Name = "有給消化日数")]
        public decimal? Syouka { get; set; }

        /// <summary>
        /// 半日有給消化回数
        /// </summary>
        [Display(Name = "半日有給消化回数")]
        public short HannitiKaisuu { get; set; }

        /// <summary>
        /// 残業超過制限開始
        /// </summary>
        [Display(Name = "残業超過制限開始")]
        public bool IsOvertimeExcessLimitStart { get; set; }

        /// <summary>
        /// 残業超過制限開始年月
        /// </summary>
        [Display(Name = "残業超過制限開始年月")]
        public string? OvertimeExcessLimitYm { get; set; }

        /// <summary>
        /// ロール
        /// </summary>
        [Display(Name = "ロール")]
        [Required(ErrorMessage = Const.ErrorSelectRequired)]
        public long? UserRoleId { get; set; }

        /// <summary>
        /// 社員権限
        /// </summary>
        public long Kengen { get; set; }

        // TODO: 以下不要 EmployeeAuthority型のプロパティ1つで実現できるはず
        /// <summary>
        /// 選択社員権限
        /// </summary>
        public List<EmployeeAuthority> SelectedAuthorities { get; set; } = [];

        // TODO: プロパティのデフォルト値をこれにすればいい
        /// <summary>
        /// 新規登録画面用の SyainInputModel 生成。
        /// 適用開始日・有効開始日を本日で初期化し、
        /// 有効終了日は最大日付で設定する。
        /// その他の初期値も新規登録前提で設定する。
        /// </summary>
        /// <param name="today">基準日（通常は当日）</param>
        /// <returns>初期化済みの SyainInputModel</returns>
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

        // TODO: privateでSyainエンティティを保持し、各プロパティはそこから値を取得するようにする
        // すべてSyainからたどれませんか？
        /// <summary>
        /// 社員Entityおよび関連情報からInputModelを生成
        /// 既存社員の編集画面表示用に、
        /// 有給残情報および残業超過制限情報も含めて初期値を設定する。
        /// </summary>
        /// <param name="syain">現在有効な社員エンティティ</param>
        /// <param name="today">適用開始日の初期値として使用する基準日</param>
        /// <returns>編集用に初期化された SyainInputModel</returns>
        public static SyainInputModel FromEntity(
            Syain syain,
            DateOnly today)
        {
            var yuukyuuZan = syain.SyainBase.YuukyuuZans.FirstOrDefault();

            var overtimeExcessLimit = syain.SyainBase.OvertimeExcessLimits
                .OrderByDescending(x => x.DisabledYm)
                .FirstOrDefault();

            return new SyainInputModel

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
                KingsSyozoku = syain.KingsSyozoku.Trim(),
                KintaiZokuseiId = (EmployeeWorkType)syain.KintaiZokuseiId,
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
                // TODO: DateTime.ToYYYYMMSlash()を使用する DateOnlyExtensionsに定義してもいい
                OvertimeExcessLimitYm = overtimeExcessLimit?.DisabledYm.YMSlash(),
                UserRoleId = syain.UserRoleId,
                // TODO: キャストしない
                SelectedAuthorities = Enum.GetValues<EmployeeAuthority>()
                    .Where(a => a != EmployeeAuthority.None && syain.Kengen.HasFlag(a))
                    .ToList(),
            };
        }
    }
}
