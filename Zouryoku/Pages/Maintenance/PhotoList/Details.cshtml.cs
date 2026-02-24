using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.Maintenance.PhotoList
{
    /// <summary>
    /// 写真リスト詳細ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class DetailsModel(ZouContext db, ILogger<DetailsModel> logger, IOptions<AppConfig> optionsAccessor) : BasePageModel<DetailsModel>(db, logger, optionsAccessor)
    {
        public override bool UseInputAssets { get; } = true;

        /// <summary>
        /// 表示対象の社員情報
        /// </summary>
        public SyainDetailViewModel Syain { get; private set; } = default!;

        /// <summary>
        /// 社員詳細情報を取得する
        /// </summary>
        /// <param name="id">社員基礎ID（syain_bases.id）</param>
        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            // syain_bases.id 経由で社員を特定し、部署と写真を読み込む
            var syain = await db.Syains
                .AsNoTracking()
                .Include(s => s.Busyo)
                .Include(s => s.SyainBase)
                    .ThenInclude(sb => sb.SyainPhotos)
                        .ThenInclude(sp => sp.PhotoAfterProcessTnData)
                .FirstOrDefaultAsync(s => s.SyainBaseId == id.Value && !s.Retired);

            if (syain is null)
            {
                return NotFound();
            }

            // 表示用 ViewModel に詰め替え
            var vm = new SyainDetailViewModel
            {
                SyainNumber = syain.Code,
                Name = syain.Name,
                NameKana = syain.KanaName,
                BusyoName = syain.Busyo?.Name ?? string.Empty,
                NyusyaYmd = syain.NyuusyaYmd,
                MailAddress = syain.EMail ?? string.Empty,
                TelNumber = syain.PhoneNumber ?? string.Empty
            };

            var selectedPhoto = syain.SyainBase?.SyainPhotos
                .Where(p => p.Selected && !p.Deleted)
                .FirstOrDefault();

            if (selectedPhoto is not null)
            {
                var photoData = selectedPhoto.PhotoAfterProcessTnData
                    .FirstOrDefault(pt => pt.Photo != null);

                if (photoData?.Photo is not null)
                {
                    vm.PhotoBase64 = Convert.ToBase64String(photoData.Photo);
                }
            }

            Syain = vm;

            return Page();
        }
    }

    /// <summary>
    /// 社員詳細表示用 ViewModel
    /// </summary>
    public class SyainDetailViewModel
    {
        /// <summary>社員番号</summary>
        public string SyainNumber { get; set; } = string.Empty;

        /// <summary>氏名</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>氏名（カナ）</summary>
        public string NameKana { get; set; } = string.Empty;

        /// <summary>所属部署</summary>
        public string BusyoName { get; set; } = string.Empty;

        /// <summary>入社年月日</summary>
        public DateOnly? NyusyaYmd { get; set; }

        /// <summary>メールアドレス</summary>
        public string MailAddress { get; set; } = string.Empty;

        /// <summary>電話番号</summary>
        public string TelNumber { get; set; } = string.Empty;

        /// <summary>顔写真（Base64）</summary>
        public string PhotoBase64 { get; set; } = string.Empty;
    }
}
