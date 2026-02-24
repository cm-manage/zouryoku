using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Data;
using System;
using System.Threading.Tasks;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using ZouryokuCommonLibrary;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Model.Enums;
using Zouryoku.Utils; // 定数利用

namespace Zouryoku.Pages.Maintenance.PcLogs
{
    /// <summary>
    /// PCログ入力（新規 / 編集兼用）ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class InputModel : BasePageModel<InputModel>
    {
        private readonly ZouContext context;

        public InputModel(ZouContext context, ILogger<InputModel> logger, IOptions<AppConfig> options, TimeProvider? timeProvider = null)
            : base(context, logger, options, timeProvider)
        {
            this.context = context;
        }
        public override bool UseInputAssets { get; } = true;

        /// <summary>
        /// 編集モードかどうか
        /// </summary>
        public bool IsEdit => PcLog?.Id >0;

        /// <summary>
        /// 入力対象PCログ (ViewModel)
        /// </summary>
        [BindProperty]
        public PcLogModel PcLog { get; set; } = new PcLogModel();

        /// <summary>
        ///画面初期表示（新規/編集）
        /// </summary>
        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id.HasValue)
            {
                var existing = await context.PcLogs.FirstOrDefaultAsync(x => x.Id == id.Value);
                if (existing == null)
                {
                    return NotFound();
                }
                PcLog = PcLogModel.FromEntity(existing);
            }
            else
            {
                PcLog.Datetime = timeProvider.Now();
            }
            return Page();
        }

        /// <summary>
        /// 入力送信（新規/更新）
        /// </summary>
        public async Task<IActionResult> OnPostRegisterAsync()
        {
            //追加の手動必須チェック (Datetime既定値防止)
            if (PcLog.Datetime == DateTime.MinValue)
            {
                ModelState.AddModelError("PcLog.Datetime", string.Format(ZouryokuCommonLibrary.Utils.Const.ErrorRequired, "日時"));
            }

            var errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            if (IsEdit)
            {
                var entity = await context.PcLogs.FirstOrDefaultAsync(x => x.Id == PcLog.Id);
                if (entity == null) return NotFound();
                // 更新項目反映
                entity.Datetime = PcLog.Datetime;
                entity.PcName = PcLog.PcName;
                entity.UserName = PcLog.UserName;
                entity.SyainId = PcLog.SyainId;
                entity.Operation = PcLog.Operation;
                context.Attach(entity).State = EntityState.Modified;
            }
            else
            {
                var entity = PcLog.ToEntity();
                context.PcLogs.Add(entity);
            }
            await context.SaveChangesAsync();
            return Success();
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

            var target = await context.PcLogs.FirstOrDefaultAsync(x => x.Id == PcLog.Id);
            if (target == null)
            {
                return NotFound();
            }
            context.PcLogs.Remove(target);
            await context.SaveChangesAsync();
            return Success();
        }
    }
    
    /// <summary>
    /// PCログ入力用ビュー / バインドモデル
    /// EFエンティティ <see cref="PcLog"/> と同等のプロパティを持ち、入力検証属性を付与
    /// </summary>
    public class PcLogModel
    {
        /// <summary>ID (編集時のみ利用)</summary>
        [Display(Name = "ID")]
        public long Id { get; set; }

        /// <summary>日時</summary>
        [Display(Name = "日時")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [DataType(DataType.DateTime)]
        public DateTime Datetime { get; set; }

        /// <summary>コンピューター名</summary>
        [Display(Name = "コンピューター名")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [StringLength(50, ErrorMessage = Const.ErrorLength)]
        public string PcName { get; set; } = string.Empty;

        /// <summary>ログオンユーザ名</summary>
        [Display(Name = "ログオンユーザ名")]
        [StringLength(50, ErrorMessage = Const.ErrorLength)]
        public string? UserName { get; set; }

        /// <summary>社員ID</summary>
        [Display(Name = "社員ID")]
        [Range(1, long.MaxValue, ErrorMessage = Const.ErrorNumberRangeMoreThanEqual)]
        public long? SyainId { get; set; }

        /// <summary>操作種別</summary>
        [Display(Name = "操作種別")]
        [Required(ErrorMessage = Const.ErrorRequired)]
        [EnumDataType(typeof(PcOperationType))]
        public PcOperationType Operation { get; set; }

        /// <summary>
        /// ビューモデルからエンティティへ変換
        /// </summary>
        public PcLog ToEntity()
            => new PcLog
            {
                Id = Id,
                Datetime = Datetime,
                PcName = PcName,
                UserName = UserName,
                SyainId = SyainId,
                Operation = Operation
            };

        /// <summary>
        /// エンティティからビューモデルへ変換
        /// </summary>
        public static PcLogModel FromEntity(PcLog entity)
            => new PcLogModel
            {
                Id = entity.Id,
                Datetime = entity.Datetime,
                PcName = entity.PcName,
                UserName = entity.UserName,
                SyainId = entity.SyainId,
                Operation = entity.Operation
            };
    }
}
