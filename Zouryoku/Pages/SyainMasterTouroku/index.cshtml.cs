using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using ZouryokuCommonLibrary;

namespace Zouryoku.Pages.SyainMastaTouroku
{
    /// <summary>
    /// 社員作成ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class InsertModel : BasePageModel<InsertModel>
    {
        private readonly ZouContext context;

        public InsertModel(ZouContext context, ILogger<InsertModel> logger, IOptions<AppConfig> options)
            : base(context, logger, options)
        {
            this.context = context;
        }

        public override bool UseInputAssets => true;

        /// <summary>
        /// 入力モデル
        /// </summary>
        [BindProperty]
        public SyainInputModel Input { get; set; } = new SyainInputModel();

        /// <summary>
        /// 編集モード判定
        /// </summary>
        public bool IsEdit => Input?.Id > 0;

        /// <summary>
        /// 勤怠属性の選択肢
        /// </summary>
        public SelectList KintaiZokuseiOptions { get; set; } = default!;

        /// <summary>
        /// 業務種別の選択肢
        /// </summary>
        public SelectList GyoumuTypeOptions { get; set; } = default!;

        /// <summary>
        /// 利用会社の選択肢
        /// </summary>
        public SelectList CompanyOptions { get; set; } = default!;

        /// <summary>
        /// ロールの選択肢
        /// </summary>
        public SelectList RoleOptions { get; set; } = default!;

        /// <summary>
        /// 初期表示
        /// </summary>
        public async Task<IActionResult> OnGetAsync(long? id)
        {
            // 勤怠属性一覧をロード
            var kintaiList = await context.KintaiZokuseis
                .AsNoTracking()
                .Select(k => new { k.Id, k.Name })
                .ToListAsync();

            KintaiZokuseiOptions = new SelectList(kintaiList, "Id", "Name");

            // 業務種別一覧をロード
            var gyoumuTypes = await context.GyoumuTypes
                .AsNoTracking()
                .Select(g => new { g.Id, g.Name })
                .ToListAsync();

            GyoumuTypeOptions = new SelectList(gyoumuTypes, "Id", "Name");

            // 利用会社一覧をロード
            var companies = await context.KokyakuKaishas
                .AsNoTracking()
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            CompanyOptions = new SelectList(companies, "Id", "Name");

            // ロール一覧をロード
            var roles = await context.UserRoles
                .AsNoTracking()
                .Select(r => new RoleMapItem
                {
                    Id = r.Id,
                    Name = r.Name,
                    Kengen = (int)r.Kengen
                })
               .ToListAsync();

            Input.RoleMap = roles;
            RoleOptions = new SelectList(roles, "Id", "Name");


            if (id.HasValue)
            {
                var existing = await context.Syains
                    .Include(s => s.Busyo)
                    .Include(s => s.KintaiZokusei)
                    .Include(s => s.UserRole)
                    .FirstOrDefaultAsync(x => x.Id == id.Value);

                if (existing == null) return NotFound();

                Input = SyainInputModel.FromEntity(existing);

                if (existing.UserRole != null)
                {
                    var authority = (EmployeeAuthority)existing.Kengen;
                    Input.Perm1Checked = authority.HasFlag(EmployeeAuthority.労働状況報告);
                    Input.Perm2Checked = authority.HasFlag(EmployeeAuthority.勤務日報未確定チェック);
                    Input.Perm3Checked = authority.HasFlag(EmployeeAuthority.出退勤一覧画面の部署選択);
                    Input.Perm4Checked = authority.HasFlag(EmployeeAuthority.出退勤一覧の打刻位置確認);
                    Input.Perm5Checked = authority.HasFlag(EmployeeAuthority.PCログ出力);
                    Input.Perm6Checked = authority.HasFlag(EmployeeAuthority.管理機能利用_人財向け);
                    Input.Perm7Checked = authority.HasFlag(EmployeeAuthority.指示承認者);
                    Input.Perm8Checked = authority.HasFlag(EmployeeAuthority.計画休暇承認);
                    Input.Perm9Checked = authority.HasFlag(EmployeeAuthority.労働最終警告メール送信対象者);
                    Input.Perm10Checked = authority.HasFlag(EmployeeAuthority.勤務日報未確定者への通知);
                    Input.Perm11Checked = authority.HasFlag(EmployeeAuthority.出退勤一覧の打刻時間修正);
                    Input.Perm12Checked = authority.HasFlag(EmployeeAuthority.部門プロセス設定);
                    Input.Perm13Checked = authority.HasFlag(EmployeeAuthority.勤怠データ出力);
                    Input.Perm14Checked = authority.HasFlag(EmployeeAuthority.管理機能利用_その他);
                    Input.Perm15Checked = authority.HasFlag(EmployeeAuthority.指示最終承認者);
                }

                // 有給関連を取得
                var yuukyuu = await (from yz in context.YuukyuuZans
                                     join sb in context.SyainBases on yz.SyainBaseId equals sb.Id
                                     join s in context.Syains on sb.Id equals s.SyainBaseId
                                     where s.Name == existing.Name
                                     select new
                                     {
                                         yz.Wariate,
                                         yz.Kurikoshi,
                                         yz.Syouka
                                     }).FirstOrDefaultAsync();

                if (yuukyuu != null)
                {
                    Input.Wariate = yuukyuu.Wariate;
                    Input.Kurikoshi = yuukyuu.Kurikoshi;
                    Input.Syouka = yuukyuu.Syouka;
                }
            }
            else
            {
                // 新規作成時の初期値
                Input = new SyainInputModel
                {
                    StartDate = DateOnly.FromDateTime(DateTime.Now),
                    StartYmd = DateOnly.FromDateTime(DateTime.Now),
                    EndYmd = new DateOnly(9999, 12, 31),
                    NyuusyaYmd = DateOnly.FromDateTime(DateTime.Now),

                    // 有給関連の初期値
                    Wariate = 0,
                    Kurikoshi = 0,
                    Syouka = 0,
                    Kengen = EmployeeAuthority.None
                };
            }

            return Page();
        }


        /// <summary>
        /// 登録処理
        /// </summary>
        public async Task<IActionResult> OnPostRegisterAsync()
        {
            // 1) 事前バリデーション（DateOnly の 0001-01-01 対策と必須チェック）
            // 画面で未選択 → DateOnly の既定値(0001-01-01)が入るため、業務上の最小許容日に補正
            // 未選択が許容されないなら ModelState エラーにして返す
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (Input.StartYmd == default)
            {
                // 業務運用上の最低日付（例：1900-01-01 など）を決めるならそこに揃える
                // とりあえず「今日」に補正。未選択を許容しないなら ModelState.AddModelError を使う。
                Input.StartYmd = today;
            }
            if (Input.EndYmd == default)
            {
                // デフォルトで無期限
                Input.EndYmd = new DateOnly(9999, 12, 31);
            }

            var errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }


            // 2) 権限フラグ合成（enum のフラグを Input から合成）
            var syainEntityFromInput = Input.ToEntity();
            syainEntityFromInput.Kengen = EmployeeAuthority.None;
            if (Input.Perm1Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.労働状況報告);
            if (Input.Perm2Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.勤務日報未確定チェック);
            if (Input.Perm3Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.出退勤一覧画面の部署選択);
            if (Input.Perm4Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.出退勤一覧の打刻位置確認);
            if (Input.Perm5Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.PCログ出力);
            if (Input.Perm6Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.管理機能利用_人財向け);
            if (Input.Perm7Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.指示承認者);
            if (Input.Perm8Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.計画休暇承認);
            if (Input.Perm9Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.労働最終警告メール送信対象者);
            if (Input.Perm10Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.勤務日報未確定者への通知);
            if (Input.Perm11Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.出退勤一覧の打刻時間修正);
            if (Input.Perm12Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.部門プロセス設定);
            if (Input.Perm13Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.勤怠データ出力);
            if (Input.Perm14Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.管理機能利用_その他);
            if (Input.Perm15Checked) syainEntityFromInput.SetKengen(EmployeeAuthority.指示最終承認者);

            // 3) 新規 or 編集 の分岐
            if (!IsEdit)
            {
                // --- 新規登録 ---
                // 3-1) 社員ベース作成 → SaveChanges で ID 確定
                var syainBase = new SyainBasis
                {
                    Name = Input.Name,
                    Code = Input.Code
                };
                context.SyainBases.Add(syainBase);
                await context.SaveChangesAsync(); // Id 確定

                // 3-2) 社員エンティティ紐付け（FK を必ずセット）
                syainEntityFromInput.SyainBaseId = syainBase.Id;
                // Start/End は補正済み。未指定なら Today/9999-12-31 を前段で統一
                context.Syains.Add(syainEntityFromInput);

                // 3-3) 有給残の作成（社員ベースに紐付け）
                var newYuukyuu = new YuukyuuZan
                {
                    SyainBaseId = syainBase.Id,
                    Wariate = Input.Wariate,
                    Kurikoshi = Input.Kurikoshi,
                    Syouka = Input.Syouka
                };
                context.YuukyuuZans.Add(newYuukyuu);

                // 3-4) 部署ナビゲーション（必要に応じて）
                // 既存部署に紐付ける運用なら検索して BusyoId を設定
                if (!string.IsNullOrEmpty(Input.BusyoCode))
                {
                    var busyo = await context.Busyos
                        .FirstOrDefaultAsync(b => b.Code == Input.BusyoCode && b.EndYmd >= today);
                    if (busyo is not null)
                    {
                        syainEntityFromInput.BusyoId = busyo.Id;
                    }
                    else if (!string.IsNullOrWhiteSpace(Input.BusyoName))
                    {
                        // 部署新規作成 → SaveChanges 後に ID を紐付け
                        var newBusyo = new Model.Model.Busyo
                        {
                            Code = Input.BusyoCode ?? string.Empty,
                            Name = Input.BusyoName!,
                            StartYmd = Input.StartYmd,
                            EndYmd = new DateOnly(9999, 12, 31)
                        };
                        context.Busyos.Add(newBusyo);
                        await context.SaveChangesAsync();
                        syainEntityFromInput.BusyoId = newBusyo.Id;
                    }
                }

                await context.SaveChangesAsync();
                return Success();
            }
            else
            {
                // --- 編集 ---
                var existing = await context.Syains
                    .Include(s => s.Busyo)
                    .FirstOrDefaultAsync(x => x.Id == Input.Id && x.Code == Input.Code);

                if (existing == null)
                {
                    ModelState.AddModelError(nameof(Input.Code), $"syainCode:{Input.Code} の社員がみつかりません。");
                    if (errorJson is not null) return errorJson;
                    return BadRequest();
                }

                // 4) 履歴対象差分チェック（enum→short の比較は明示キャスト）
                bool hasHistoryChange =
                existing.Name != Input.Name ||
                (existing.Busyo?.Name ?? string.Empty) != (Input.BusyoName ?? string.Empty) ||
                existing.Kyusyoku != Input.Kyusyoku ||
                existing.SyucyoSyokui != Input.SyucyoSyokui ||
                existing.KintaiZokuseiId != Input.KintaiZokuseiId ||
                existing.IsGenkaRendou != Input.IsGenkaRendou ||
                existing.KaisyaCode != Input.KaisyaCode;

                // StartYmd が変わったら無条件で履歴化
                bool startDateChanged = existing.StartYmd != Input.StartDate;

                // 最終的な履歴化判定
                bool shouldCreateHistory = startDateChanged || hasHistoryChange;


                if (shouldCreateHistory)
                {
                    // 4-1) 既存レコードを履歴化（終了日 = 新開始日の前日）
                    if (Input.StartYmd > DateOnly.MinValue)
                    {
                        existing.EndYmd = Input.StartYmd.AddDays(-1);
                    }
                    else
                    {
                        // ガード：最小値なら、履歴終了日は最小値を維持（または業務最小日に合わせる）
                        existing.EndYmd = DateOnly.MinValue;
                    }

                    // 4-2) 新規レコードを同ベースで作成（FK を引き継ぎ）
                    var newEntity = Input.ToEntity();
                    newEntity.Id = 0;
                    newEntity.SyainBaseId = existing.SyainBaseId; // 既存ベースを引き継ぐ（要件2）
                    newEntity.Kengen = syainEntityFromInput.Kengen; // 合成済み権限を反映
                    newEntity.StartYmd = Input.StartYmd;
                    newEntity.EndYmd = new DateOnly(9999, 12, 31);
                    newEntity.SyokusyuCode = existing.SyokusyuCode;
                    newEntity.SyokusyuBunruiCode = existing.SyokusyuBunruiCode;

                    // 部署ナビゲーション：コードで既存部署を検索→なければ作成して紐付け
                    if (!string.IsNullOrEmpty(Input.BusyoCode))
                    {
                        var busyo = await context.Busyos
                            .FirstOrDefaultAsync(b => b.Code == Input.BusyoCode && b.EndYmd >= today);
                        if (busyo is not null)
                        {
                            newEntity.BusyoId = busyo.Id;
                        }
                        else if (!string.IsNullOrWhiteSpace(Input.BusyoName))
                        {
                            var newBusyo = new Model.Model.Busyo
                            {
                                Code = Input.BusyoCode ?? string.Empty,
                                Name = Input.BusyoName!,
                                StartYmd = Input.StartYmd,
                                EndYmd = new DateOnly(9999, 12, 31)
                            };
                            context.Busyos.Add(newBusyo);
                            await context.SaveChangesAsync();
                            newEntity.BusyoId = newBusyo.Id;
                        }
                    }

                    context.Syains.Add(newEntity);
                    await context.SaveChangesAsync();
                    return Success();
                }
                else
                {
                    // 4-3) 通常の差分更新（履歴なし）
                    existing.Code = Input.Code;
                    existing.Name = Input.Name;
                    existing.KanaName = Input.KanaName;
                    existing.NyuusyaYmd = Input.NyuusyaYmd;
                    existing.Seibetsu = Input.Seibetsu;
                    existing.BusyoCode = Input.BusyoCode;
                    existing.GyoumuTypeId = Input.GyoumuTypeId;
                    existing.StartYmd = Input.StartYmd == default ? today : Input.StartYmd;
                    existing.EndYmd = Input.EndYmd == default ? new DateOnly(9999, 12, 31) : Input.EndYmd;
                    existing.Kyusyoku = Input.Kyusyoku;
                    existing.SyucyoSyokui = Input.SyucyoSyokui; // enum→short
                    existing.KingsSyozoku = Input.KingsSyozoku;
                    existing.KintaiZokuseiId = Input.KintaiZokuseiId;
                    existing.IsGenkaRendou = Input.IsGenkaRendou;
                    existing.KaisyaCode = Input.KaisyaCode;
                    existing.EMail = Input.EMail;
                    existing.KeitaiMail = Input.KeitaiMail;
                    existing.PhoneNumber = Input.PhoneNumber;
                    existing.Retired = Input.Retired;
                    existing.UserRoleId = Input.UserRoleId;
                    existing.Kengen = syainEntityFromInput.Kengen;

                    // 部署紐付け更新（存在すれば紐付け、なければ作成して紐付け）
                    if (!string.IsNullOrEmpty(Input.BusyoCode))
                    {
                        var busyo = await context.Busyos
                            .FirstOrDefaultAsync(b => b.Code == Input.BusyoCode && b.EndYmd >= today);
                        if (busyo is not null)
                        {
                            existing.BusyoId = busyo.Id;
                        }
                    }

                    // 4-4) 有給残の更新（社員ベースIDで紐付け）
                    var yuukyuu = await context.YuukyuuZans
                        .FirstOrDefaultAsync(y => y.SyainBaseId == existing.SyainBaseId);

                    if (yuukyuu != null)
                    {
                        yuukyuu.Wariate = Input.Wariate;
                        yuukyuu.Kurikoshi = Input.Kurikoshi;
                        yuukyuu.Syouka = Input.Syouka;
                    }
                    else
                    {
                        var newYuukyuu = new YuukyuuZan
                        {
                            SyainBaseId = existing.SyainBaseId,
                            Wariate = Input.Wariate,
                            Kurikoshi = Input.Kurikoshi,
                            Syouka = Input.Syouka
                        };
                        context.YuukyuuZans.Add(newYuukyuu);
                    }

                    await context.SaveChangesAsync();
                    return Success();
                }
            }
        }

        public async Task<FileResult> OnGetPhotoAsync(long syainId)
        {
            var photoBinary = await context.PhotoAfterProcessTnDatas
                .Where(p => p.SyainPhoto.SyainBase.Syains.Any(s => s.Id == syainId))
                .Select(p => p.Photo)
                .FirstOrDefaultAsync();

            if (photoBinary == null)
            {
                return File(Array.Empty<byte>(), "image/png");
            }

            return File(photoBinary, "image/jpeg");
        }


        public async Task<IActionResult> OnGetRoleDefaults(int roleId)
        {
            var role = await context.UserRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null) return NotFound();

            var dummySyain = new Syain { Kengen = (EmployeeAuthority)role.Kengen };


            return new JsonResult(new
            {
                dummySyain.IsPcLogOutput,
                dummySyain.IsCheckStampPosition,
                dummySyain.IsCorrectingTimeStamps,
                dummySyain.IsSelectDepartment,
                dummySyain.IsFinalLaborWarningEmailRecipients,
                dummySyain.IsLaborStatusReport,
                dummySyain.IsCheckPendingReports,
                dummySyain.IsNotificationReportUnconfirmed,
                dummySyain.IsAttendanceDataOutput,
                dummySyain.IsInstructionApprover,
                dummySyain.IsFinalInstructionApprover,
                dummySyain.IsManagementFunctionsOther,
                dummySyain.IsManagementFunctionsHumanResources,
                dummySyain.IsPlannedLeaveApproval,
                dummySyain.IsDepartmentProcessSettings,
            });
        }
    }


    /// <summary>
    /// 入力モデル
    /// </summary>
    public class SyainInputModel
    {
        /// <summary>ID (編集時のみ利用)</summary>
        [Display(Name = "ID")]
        public long Id { get; set; }

        /// <summary>社員番号</summary>
        [Display(Name = "社員番号")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [MaxLength(5, ErrorMessage = Const.ErrorLength)]
        public string Code { get; set; } = string.Empty;

        /// <summary>社員氏名</summary>
        [Display(Name = "社員氏名")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [MaxLength(32, ErrorMessage = Const.ErrorLength)]
        public string Name { get; set; } = string.Empty;

        /// <summary>社員氏名カナ</summary>
        [Display(Name = "社員氏名カナ")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [MaxLength(32, ErrorMessage = Const.ErrorLength)]
        public string KanaName { get; set; } = string.Empty;

        /// <summary>入社年月日</summary>
        [Display(Name = "入社年月日")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public DateOnly NyuusyaYmd { get; set; }

        /// <summary>性別</summary>
        [Display(Name = "性別")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public char Seibetsu { get; set; } = '1';

        /// <summary>部署ID</summary>
        public long? BusyoId { get; set; }

        /// <summary>部署コード</summary>
        public string BusyoCode { get; set; }

        /// <summary>部署名</summary>
        [Display(Name = "部署名")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public string? BusyoName { get; set; }

        /// <summary>業務タイプID</summary>
        public long? GyoumuTypeId { get; set; }

        /// <summary>適用開始日</summary>
        [Display(Name = "適用開始日")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        /// <summary>有効開始日</summary>
        [Display(Name = "有効開始日")]
        public DateOnly StartYmd { get; set; }

        /// <summary>有効終了日</summary>
        [Display(Name = "有効終了日")]
        public DateOnly EndYmd { get; set; }

        /// <summary>級職</summary>
        [Display(Name = "級職")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public short Kyusyoku { get; set; }

        /// <summary>出張職位</summary>
        //[Display(Name = "出張職位")]
        //[Required(ErrorMessage = Const.ErrorRequired)]
        //public short SyucyoSyokui { get; set; }

        /// <summary>出張職位</summary>
        [Display(Name = "出張職位")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        public BusinessTripRole SyucyoSyokui { get; set; }

        /// <summary>KINGS所属</summary>
        [Display(Name = "KINGS所属")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [MaxLength(5, ErrorMessage = Const.ErrorLength)]
        public string KingsSyozoku { get; set; }

        /// <summary>勤怠属性ID</summary>
        public long KintaiZokuseiId { get; set; }

        /// <summary>原価連動フラグ</summary>
        [Display(Name = "原価連動フラグ")]
        public bool IsGenkaRendou { get; set; }

        /// <summary>利用会社</summary>
        [Display(Name = "利用会社")]
        public short KaisyaCode { get; set; }

        /// <summary>EMail</summary>
        [Display(Name = "EMail")]
        [MaxLength(50, ErrorMessage = Const.ErrorLength)]
        public string? EMail { get; set; }

        /// <summary>携帯Mail</summary>
        [Display(Name = "携帯Mail")]
        [MaxLength(50, ErrorMessage = Const.ErrorLength)]
        public string? KeitaiMail { get; set; }

        /// <summary>携帯番号</summary>
        [Display(Name = "携帯番号")]
        [MaxLength(15, ErrorMessage = Const.ErrorLength)]
        public string? PhoneNumber { get; set; }

        /// <summary>退職フラグ</summary>
        [Display(Name = "退職フラグ")]
        public bool Retired { get; set; }

        /// <summary>有給割当日数</summary>
        [Display(Name = "有給割当日数")]
        public decimal Wariate { get; set; }

        /// <summary>有給繰越日数</summary>
        [Display(Name = "有給繰越日数")]
        public decimal Kurikoshi { get; set; }

        /// <summary>有給消化日数</summary>
        [Display(Name = "有給消化日数")]
        public decimal Syouka { get; set; }

        /// <summary>ロール</summary>
        public long UserRoleId { get; set; }

        /// <summary>権限</summary>
        public EmployeeAuthority Kengen { get; set; }

        /// <summary>労働状況報告権限</summary>
        public bool Perm1Checked { get; set; }

        /// <summary>勤務日報未確定チェック権限</summary>
        public bool Perm2Checked { get; set; }

        /// <summary>出退勤一覧画面の部署選択権限</summary>
        public bool Perm3Checked { get; set; }

        /// <summary>出退勤一覧の打刻位置確認権限</summary>
        public bool Perm4Checked { get; set; }

        /// <summary>PCログ出力権限</summary>
        public bool Perm5Checked { get; set; }

        /// <summary>管理機能利用（人財向け）権限</summary>
        public bool Perm6Checked { get; set; }

        /// <summary>指示承認者権限</summary>
        public bool Perm7Checked { get; set; }

        /// <summary>計画休暇承認権限</summary>
        public bool Perm8Checked { get; set; }

        /// <summary>労働最終警告メール送信対象者権限</summary>
        public bool Perm9Checked { get; set; }

        /// <summary>勤務日報未確定者への通知権限</summary>
        public bool Perm10Checked { get; set; }

        /// <summary>出退勤一覧の打刻時間修正権限</summary>
        public bool Perm11Checked { get; set; }

        /// <summary>部門プロセス設定権限</summary>
        public bool Perm12Checked { get; set; }

        /// <summary>勤怠データ出力権限</summary>
        public bool Perm13Checked { get; set; }

        /// <summary>管理機能利用（その他）権限</summary>
        public bool Perm14Checked { get; set; }

        /// <summary>権限のためのロールマップ</summary>
        public List<RoleMapItem>? RoleMap { get; set; }

        /// <summary>指示最終承認者権限</summary>
        public bool Perm15Checked { get; set; }

        /// <summary>並び順序</summary>
        public short Jyunjyo { get; set; }

        public Syain ToEntity()
        {
            return new Syain
            {
                Code = Code,
                Name = Name,
                KanaName = KanaName,
                NyuusyaYmd = NyuusyaYmd,
                Seibetsu = Seibetsu,
                BusyoCode = BusyoCode,
                GyoumuTypeId = GyoumuTypeId,
                StartYmd = StartYmd,
                EndYmd = EndYmd,
                Kyusyoku = Kyusyoku,
                SyucyoSyokui = SyucyoSyokui,
                KingsSyozoku = KingsSyozoku,
                KintaiZokuseiId = KintaiZokuseiId,
                IsGenkaRendou = IsGenkaRendou,
                KaisyaCode = KaisyaCode,
                EMail = EMail,
                KeitaiMail = KeitaiMail,
                PhoneNumber = PhoneNumber,
                Retired = Retired,
                UserRoleId = UserRoleId,
                Kengen = Kengen,
                Jyunjyo = Jyunjyo,
            };
        }

        public static SyainInputModel FromEntity(Syain entity)
        {
            // 社員ベースから有給残を取得（最新レコードを想定）
            var yuukyuu = entity.SyainBase?.YuukyuuZans?.FirstOrDefault();

            return new SyainInputModel
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                KanaName = entity.KanaName,
                NyuusyaYmd = entity.NyuusyaYmd,
                Seibetsu = entity.Seibetsu,
                BusyoCode = entity.BusyoCode,
                BusyoName = entity.Busyo?.Name,
                GyoumuTypeId = entity.GyoumuTypeId,
                StartYmd = entity.StartYmd,
                EndYmd = entity.EndYmd,
                Kyusyoku = entity.Kyusyoku,
                SyucyoSyokui = entity.SyucyoSyokui,
                KingsSyozoku = entity.KingsSyozoku,
                KintaiZokuseiId = entity.KintaiZokuseiId,
                IsGenkaRendou = entity.IsGenkaRendou,
                KaisyaCode = entity.KaisyaCode,
                EMail = entity.EMail,
                KeitaiMail = entity.KeitaiMail,
                PhoneNumber = entity.PhoneNumber,
                Retired = entity.Retired,

                // 有給残情報をセット
                Wariate = yuukyuu?.Wariate ?? 0,
                Kurikoshi = yuukyuu?.Kurikoshi ?? 0,
                Syouka = yuukyuu?.Syouka ?? 0,

                UserRoleId = entity.UserRoleId,
                Kengen = entity.Kengen
            };
        }

    }

    public class RoleMapItem
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public int Kengen { get; set; }
    }
}
