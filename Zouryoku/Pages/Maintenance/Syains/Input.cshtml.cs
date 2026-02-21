using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;

namespace Zouryoku.Pages.Maintenance.Syains
{
    /// <summary>
    /// 顔写真入力（新規登録）ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class InputModel : BasePageModel<InputModel>
    {
        private readonly ZouContext _context;

        public InputModel(ZouContext context, ILogger<InputModel> logger, IOptions<AppConfig> options)
            : base(context, logger, options)
        {
            _context = context;
        }

        public override bool UseInputAssets { get; } = true;

        /// <summary>
        /// 入力対象の顔写真 (ViewModel)
        /// </summary>
        [BindProperty]
        public SyainPhotoModel SyainPhoto { get; set; } = new();

        /// <summary>
        /// 画面初期表示
        /// </summary>
        public async Task<IActionResult> OnGetAsync(long? id)
        {
            // idがある場合は編集モード
            if (id.HasValue && id.Value > 0)
            {
                var syainBaseId = LoginInfo?.User?.SyainBaseId;
                if (syainBaseId == null || syainBaseId <= 0)
                {
                    return BadRequest("ユーザー情報を取得できませんでした。");
                }

                // 指定のIDの写真情報を取得
                var photo = await _context.SyainPhotos
                    .AsNoTracking()
                    .Where(p => p.Id == id.Value && p.SyainBaseId == syainBaseId && !p.Deleted)
                    .Include(p => p.PhotoAfterProcessTnData)
                    .FirstOrDefaultAsync();

                if (photo == null)
                {
                    return NotFound("写真が見つかりません。");
                }

                // ViewModel に編集対象の写真情報を設定
                SyainPhoto.Id = photo.Id;
                SyainPhoto.PhotoName = photo.PhotoName;
                SyainPhoto.IsEditMode = true;

                // 写真データを Base64 に変換して設定
                var photoData = photo.PhotoAfterProcessTnData
                    .FirstOrDefault(pt => pt.Photo != null);

                if (photoData?.Photo != null)
                {
                    SyainPhoto.CurrentPhotoBase64 = Convert.ToBase64String(photoData.Photo);
                }
            }

            return Page();
        }

        /// <summary>
        /// 写真登録
        /// </summary>
        public async Task<IActionResult> OnPostRegisterAsync(IFormFile? photoFile)
        {
            // ログインユーザーの社員BASE IDを取得
            var syainBaseId = LoginInfo?.User?.SyainBaseId;
            if (syainBaseId == null || syainBaseId <= 0)
            {
                ModelState.AddModelError("", "ユーザー情報を取得できませんでした。");
                return ModelState.ErrorJson()!;
            }

            // 写真ファイルの検証
            if (photoFile == null || photoFile.Length == 0)
            {
                ModelState.AddModelError("photoFile", string.Format(Const.ErrorRequired, "顔写真"));
                return ModelState.ErrorJson()!;
            }

            ValidatePhotoFile(photoFile);
            if (!ModelState.IsValid)
            {
                return ModelState.ErrorJson()!;
            }

            // ファイルをバイト配列に変換
            byte[] photoData;
            using (var stream = photoFile.OpenReadStream())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    photoData = memoryStream.ToArray();
                }
            }

            // 既存の設定中の写真をすべて非選択に変更
            var selectedPhotos = await _context.SyainPhotos
                .Where(p => p.SyainBaseId == syainBaseId && p.Selected && !p.Deleted)
                .ToListAsync();

            foreach (var photo in selectedPhotos)
            {
                photo.Selected = false;
            }

            // 次の連番を取得（削除済みも含めて最大値を取得）
            var maxSeq = await _context.SyainPhotos
                .Where(p => p.SyainBaseId == syainBaseId)
                .MaxAsync(p => (int?)p.Seq) ?? 0;

            // 新しい写真レコードを作成
            var newPhoto = new SyainPhoto
            {
                SyainBaseId = (long)syainBaseId,
                PhotoName = System.IO.Path.GetFileNameWithoutExtension(photoFile.FileName),
                UploadTime = DateTime.Now,
                Selected = true,
                Deleted = false,
                Seq = maxSeq + 1
            };

            _context.SyainPhotos.Add(newPhoto);
            await _context.SaveChangesAsync();

            // 処理済み写真データを保存（原始ファイルのまま保存）
            var photoDataRecord = new PhotoAfterProcessTnData
            {
                SyainPhoto = newPhoto,
                Photo = photoData
            };

            _context.PhotoAfterProcessTnDatas.Add(photoDataRecord);
            await _context.SaveChangesAsync();

            return Success();
        }

        /// <summary>
        /// 写真削除
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            var syainBaseId = LoginInfo?.User?.SyainBaseId;
            if (syainBaseId == null || syainBaseId <= 0)
            {
                return ErrorJson("ユーザー情報を取得できませんでした。");
            }

            // 指定のIDの写真を取得
            var photo = await _context.SyainPhotos
                .Where(p => p.Id == id && p.SyainBaseId == syainBaseId && !p.Deleted)
                .FirstOrDefaultAsync();

            if (photo == null)
            {
                return ErrorJson("写真が見つかりません。");
            }

            // 論理削除：Seleted フラグを false,Deleted フラグを true に設定
            photo.Selected = false;
            photo.Deleted = true;
            await _context.SaveChangesAsync();

            return Success();
        }

        /// <summary>
        /// 画像ファイルの検証
        /// </summary>
        private void ValidatePhotoFile(IFormFile photoFile)
        {
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (photoFile.Length > maxFileSize)
            {
                ModelState.AddModelError("photoFile", "ファイルサイズは5MB以下である必要があります。");
                return;
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = System.IO.Path.GetExtension(photoFile.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("photoFile", "JPG、PNG、GIF形式のみアップロード可能です。");
            }
        }

        /// <summary>
        /// 顔写真入力用ビュー / バインドモデル
        /// </summary>
        public class SyainPhotoModel
        {
            /// <summary>ID (編集時のみ利用)</summary>
            [Display(Name = "ID")]
            public long Id { get; set; }

            /// <summary>写真名</summary>
            [Display(Name = "写真名")]
            public string PhotoName { get; set; } = string.Empty;

            /// <summary>編集モードかどうか</summary>
            public bool IsEditMode { get; set; }

            /// <summary>現在の写真（Base64）</summary>
            public string CurrentPhotoBase64 { get; set; } = string.Empty;
        }
    }
}
