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

namespace Zouryoku.Pages.Maintenance.AppSettings
{
    [FunctionAuthorizationAttribute]
    public class IndexModel(ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> optionsAccessor, TimeProvider? timeProvider = null) : BasePageModel<IndexModel>(db, logger, optionsAccessor, timeProvider)
    {
        /// <summary>
        /// 入力画面用共通CSS/JSをレイアウトで読み込むかどうかのフラグ
        /// </summary>
        public override bool UseInputAssets { get; } = true;

        [BindProperty]
        public AppSettingModel AppSetting { get; set; } = new AppSettingModel();
        /// <summary>
        /// 画面初期表示
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // ApplicationConfigの最初のレコードを非同期で取得し、nullチェックを行う
            var record = await db.ApplicationConfigs.AsNoTracking()
                .FirstOrDefaultAsync();

            if (record is not null)
            {
                AppSetting = new AppSettingModel()
                {
                    NippoStopDate = record.NippoStopDate,
                    MsClientId = record.MsClientId,
                    MsTenantId = record.MsTenantId,
                    MsClientSecret = record.MsClientSecret,
                    SmtpUser = record.SmtpUser,
                    SmtpPassword = record.SmtpPassword
                };
            }
            else
            {
                AppSetting = new AppSettingModel()
                {
                    NippoStopDate = DateOnly.FromDateTime(DateTime.Today)
                };
            }

            return Page();
        }

        /// <summary>
        /// 入力送信（新規/更新）
        /// </summary>
        public async Task<IActionResult> OnPostRegisterAsync()
        {
            //追加の手動必須チェック
            if (AppSetting.NippoStopDate == DateOnly.MinValue)
            {
                ModelState.AddModelError("AppSetting.NippoStopDate", string.Format(ZouryokuCommonLibrary.Utils.Const.ErrorRequired, "日報停止日"));
            }

            var errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            var record = await db.ApplicationConfigs.FirstOrDefaultAsync()
                ?? await db.ApplicationConfigs.AddReturnAsync(new ApplicationConfig());
            record.NippoStopDate = AppSetting.NippoStopDate;
            record.MsClientSecret = AppSetting.MsClientSecret;
            record.MsClientId = AppSetting.MsClientId;
            record.MsTenantId = AppSetting.MsTenantId;
            record.SmtpUser = AppSetting.SmtpUser;
            record.SmtpPassword = AppSetting.SmtpPassword;

            await db.SaveChangesAsync();
            return Success();
        }

        public class AppSettingModel
        {
            /// <summary>日報停止日</summary>
            [Display(Name = "日報停止日")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            [DataType(DataType.Date)]
            public DateOnly NippoStopDate { get; set; }

            /// <summary>MSテナントID</summary>
            [Display(Name = "MSテナントID")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public string MsTenantId { get; set; } = string.Empty;

            /// <summary>MSクライアントID</summary>
            [Display(Name = "MSクライアントID")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public string MsClientId { get; set; } = string.Empty;

            /// <summary>MSクライアントシークレット</summary>
            [Display(Name = "MSクライアントシークレット")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public string MsClientSecret { get; set; } = string.Empty;

            /// <summary>SMTPユーザ</summary>
            [Display(Name = "SMTPユーザ")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public string SmtpUser { get; set; } = string.Empty;

            /// <summary>SMTPパスワード</summary>
            [Display(Name = "SMTPパスワード")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public string SmtpPassword { get; set; } = string.Empty;
        }
    }
}