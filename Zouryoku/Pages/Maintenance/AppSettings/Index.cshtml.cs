using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;

namespace Zouryoku.Pages.Maintenance.AppSettings
{
    [FunctionAuthorizationAttribute]
    public class IndexModel : BasePageModel<IndexModel>
    {
        private readonly ZouContext context;

        public IndexModel(ZouContext context, ILogger<IndexModel> logger, IOptions<AppConfig> options)
            : base(context, logger, options)
        {
            this.context = context;
        }

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
            var config = await context.ApplicationConfigs.FirstOrDefaultAsync();
            if (config is not null)
            {
                AppSetting = new AppSettingModel()
                {
                    NippoStopDate = config?.NippoStopDate ?? default,
                    MsClientId = config?.MsClientId ?? string.Empty,
                    MsTenantId = config?.MsTenantId ?? string.Empty,
                    MsClientSecret = config?.MsClientSecret ?? string.Empty,
                    SmtpUser = config?.SmtpUser ?? string.Empty,
                    SmtpPassword = config?.SmtpPassword ?? string.Empty
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

            var appconfig = await context.ApplicationConfigs.FirstOrDefaultAsync();
            appconfig.NippoStopDate = AppSetting.NippoStopDate;
            appconfig.MsClientSecret = AppSetting.MsClientSecret;
            appconfig.MsClientId = AppSetting.MsClientId;
            appconfig.MsTenantId = AppSetting.MsTenantId;
            appconfig.SmtpUser = AppSetting.SmtpUser;
            appconfig.SmtpPassword = AppSetting.SmtpPassword;

            await context.SaveChangesAsync();
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
            public string MsTenantId { get; set; } = null!;

            /// <summary>MSクライアントID</summary>
            [Display(Name = "MSクライアントID")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public string MsClientId { get; set; } = null!;

            /// <summary>MSクライアントシークレット</summary>
            [Display(Name = "MSクライアントシークレット")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public string MsClientSecret { get; set; } = null!;

            /// <summary>SMTPユーザ</summary>
            [Display(Name = "SMTPユーザ")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public string SmtpUser { get; set; } = null!;

            /// <summary>SMTPパスワード</summary>
            [Display(Name = "SMTPパスワード")]
            [Required(ErrorMessage = Const.ErrorRequired)]
            public string SmtpPassword { get; set; } = null!;
        }
    }
}