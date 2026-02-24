using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Model.Data;
using Zouryoku.Pages.Shared;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Utils;
using ZouryokuCommonLibrary;
using CommonLibrary.Extensions;

namespace Zouryoku.Pages.Logins
{
    /// <summary>
    /// 開発・検証用のメールアドレス直接入力バイパスログインページ
    /// </summary>
    public class BypassLoginModel : NotSessionBasePageModel<BypassLoginModel>
    {
        public BypassLoginModel(
            ZouContext db,
            ILogger<BypassLoginModel> logger,
            IOptions<AppConfig> optionsAccessor) : base(db, logger, optionsAccessor)
        {
        }

        /// <summary>ログイン用メールアドレス</summary>
        [BindProperty]
        [Required(ErrorMessage = "メールアドレスは必須です。")]
        [EmailAddress(ErrorMessage = "メールアドレス形式で入力してください。")]
        public string Email { get; set; } = string.Empty;

        public void OnGet()
        {
            // 画面表示のみ
        }

        /// <summary>
        /// メールアドレスで社員を特定しセッションへ LoginInfo を登録
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            logger.LogInformation("システム共通 設定値サンプル = " + appSettings.HogeThreshold);

            var today = DateTime.Today.ToDateOnly();
            // 社員検索（メール一致）
            var syains = await db.Syains
                .Include(s => s.Busyo)
                .Include(s => s.SyainBase)
                .Where(s => s.EMail == Email)
                .ToListAsync();

            if (syains.Count == 0)
            {
                ModelState.AddModelError(nameof(Email), "対象のメールアドレスは登録されていません。");
                return Page();
            }

            var syain = syains
                .Where(s => s.StartYmd <= today && today <= s.EndYmd)
                .Where(s => !s.Retired)
                .FirstOrDefault();

            if (syain == null)
            {
                ModelState.AddModelError(nameof(Email), "対象の社員は退職済みです。");
                return Page();
            }

            // LoginInfo 作成
            var loginInfo = new LoginInfo
            {
                User = syain,
                EntraEmail = Email,
                EntraDisplayName = syain.Name,
                AuthenticationMethod = "Bypass",
                LastRefreshedAt = timeProvider.Now()
            };

            // セッション保存
            HttpContext.Session.Set(loginInfo);

            // アクセスログ（簡易）
            await LoginUtil.CreateAccessLogAsync(Request, db, loginInfo);

            logger.LogInformation("バイパスログイン成功: Email={Email}, UserId={UserId}", Email, syain.Id);

            // トップページへ遷移
            return RedirectToPage("/Index");
        }
    }
}
