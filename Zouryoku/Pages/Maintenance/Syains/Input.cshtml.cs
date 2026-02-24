using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Extensions;
using Model.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
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
    public class InputModel(ZouContext db, ILogger<InputModel> logger, IOptions<AppConfig> optionsAccessor, TimeProvider? timeProvider = null) : BasePageModel<InputModel>(db, logger, optionsAccessor, timeProvider)
    {
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
            var syainBaseId = LoginInfo.User.SyainBaseId;
            if (syainBaseId <= 0)
            {
                return BadRequest("ユーザー情報を取得できませんでした。");
            }

            // idがある場合は編集モード
            if (id.HasValue && id.Value > 0)
            {
                // 指定のIDの写真情報を取得
                var photo = await db.SyainPhotos
                    .AsNoTracking()
                    .Where(p => p.Id == id.Value && !p.Deleted)
                    .Include(p => p.PhotoData)
                    .FirstOrDefaultAsync();

                if (photo == null)
                {
                    return NotFound("写真が見つかりません。");
                }

                // ViewModel に編集対象の写真情報を設定
                SyainPhoto.Id = photo.Id;
                SyainPhoto.PhotoName = photo.PhotoName;
                SyainPhoto.IsUpdate = true;

                // 写真データを Base64 に変換して設定
                var photoData = photo.PhotoData
                    .FirstOrDefault(pt => pt.Photo != null);

                if (photoData?.Photo != null)
                {
                    SyainPhoto.CurrentPhotoBase64 = Convert.ToBase64String(photoData.Photo);
                }
            }

            return Page();
        }

        private const int OriginalJpegQuality = 90;
        private const int ThumbnailJpegQuality = 75;

        /// <summary>
        /// 写真登録
        /// </summary>
        public async Task<IActionResult> OnPostRegisterAsync(IFormFile? photoFile)
        {
            var syainBaseId = LoginInfo.User.SyainBaseId;
            if (syainBaseId <= 0)
            {
                ModelState.AddModelError("SyainPhoto.Id", "ユーザー情報を取得できませんでした。"); 
                return ModelState.ErrorJson()!;
            }
            var isNew = SyainPhoto.Id <= 0;
            var hasUpload = photoFile is { Length: > 0 };

            // 必須・存在チェック
            if (isNew && !hasUpload)
            {
                // 新規登録時はアップロード必須
                ModelState.AddModelError("SyainPhoto.PhotoFile", string.Format(Const.ErrorRequired, "顔写真"));
                return ModelState.ErrorJson()!;
            }

            byte[] originalPhotoData;

            if (hasUpload)
            {
                // アップロード画像の検証
                ValidatePhotoFile(photoFile!);
                if (!ModelState.IsValid)
                {
                    return ModelState.ErrorJson()!;
                }

                // アップロード画像を JPEG に正規化して元画像として使用
                await using var uploadStream = photoFile!.OpenReadStream();
                await using var uploadMemory = new MemoryStream();
                await uploadStream.CopyToAsync(uploadMemory);
                var uploadedBytes = uploadMemory.ToArray();
                // jpegに変換
                try
                {
                    using var uploadedImage = Image.Load(uploadedBytes);
                    // EXIF Orientation を考慮して向きを正規化
                    uploadedImage.Mutate(x => x.AutoOrient());

                    await using var jpegStream = new MemoryStream();
                    await uploadedImage.SaveAsJpegAsync(jpegStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                    {
                        Quality = OriginalJpegQuality 
                    });
                    originalPhotoData = jpegStream.ToArray();
                }
                catch
                {
                    // 画像として読み込めない場合はエラー扱い
                    ModelState.AddModelError("SyainPhoto.PhotoFile", "画像の読み込みに失敗しました。写真を再選択してください。");
                    return ModelState.ErrorJson()!;
                }
            }
            else
            {
                // 更新モードでアップロード無し → 既存画像を元画像として使用
                var existingPhoto = await db.SyainPhotos
                    .AsNoTracking()
                    .Where(p => p.Id == SyainPhoto.Id && !p.Deleted)
                    .Include(p => p.PhotoData)
                    .FirstOrDefaultAsync();

                if (existingPhoto == null)
                {
                    ModelState.AddModelError("SyainPhoto.PhotoFile", "既存の写真情報が見つかりません。写真を選択してください。");
                    return ModelState.ErrorJson()!;
                }

                var existingPhotoData = existingPhoto.PhotoData.FirstOrDefault(pt => pt.Photo != null);
                if (existingPhotoData?.Photo == null)
                {
                    ModelState.AddModelError("SyainPhoto.PhotoFile", "既存の写真データが見つかりません。写真を選択してください。");
                    return ModelState.ErrorJson()!;
                }

                originalPhotoData = existingPhotoData.Photo;
                // SyainPhoto.PhotoFile は null のままで良い（元画像は originalPhotoData から取得済み）
            }

            // トリミング処理
            byte[] photoData;
            try
            {
                using var image = Image.Load(originalPhotoData);
                // EXIF Orientation を考慮して向きを正規化
                image.Mutate(x => x.AutoOrient());

                // スマホで「トリミング指定なし」の場合はクロップしない
                var shouldCrop = SyainPhoto.CropWidth > 0 && SyainPhoto.CropHeight > 0;

                if (shouldCrop)
                {
                    var cropX = Math.Max(0, SyainPhoto.CropX);
                    var cropY = Math.Max(0, SyainPhoto.CropY);
                    var cropWidth = SyainPhoto.CropWidth;
                    var cropHeight = SyainPhoto.CropHeight;

                    if (cropX + cropWidth > image.Width)
                    {
                        cropWidth = image.Width - cropX;
                    }

                    if (cropY + cropHeight > image.Height)
                    {
                        cropHeight = image.Height - cropY;
                    }

                    // 幅・高さが0以下になったらクロップしない（防御）
                    if (cropWidth > 0 && cropHeight > 0)
                    {
                        var rectangle = new Rectangle(cropX, cropY, cropWidth, cropHeight);
                        image.Mutate(x => x.Crop(rectangle));
                    }
                }

                // サムネイル用にリサイズして容量削減
                const int maxThumbWidth = 400;
                const int maxThumbHeight = 400;

                var scale = Math.Min(
                    (double)maxThumbWidth / image.Width,
                    (double)maxThumbHeight / image.Height
                );

                if (scale < 1.0)
                {
                    var resizedWidth = (int)(image.Width * scale);
                    var resizedHeight = (int)(image.Height * scale);

                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(resizedWidth, resizedHeight)
                    }));
                }

                using var outStream = new MemoryStream();
                // JPEG 品質を指定して保存（デフォルトより容量を軽くする）
                await image.SaveAsJpegAsync(outStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                {
                    Quality = ThumbnailJpegQuality
                });
                photoData = outStream.ToArray();
            }
            catch
            {
                // トリミング失敗時はエラーとして処理を中断（サムネイル保存は行わない）
                ModelState.AddModelError("SyainPhoto.PhotoFile", "トリミング処理に失敗しました。");
                return ModelState.ErrorJson()!;
            }

            // SyainPhotos レコードの取得 / 作成
            var seq = await db.SyainPhotos
                .Where(p => p.SyainBaseId == syainBaseId && !p.Deleted)
                .MaxAsync(p => (int?)p.Seq + 1) ?? 0;

            var record = await db.SyainPhotos.FirstOrDefaultAsync(x => x.Id == SyainPhoto.Id && !x.Deleted)
                ?? await db.SyainPhotos.AddReturnAsync(new SyainPhoto
                {
                    Seq = seq,
                    Selected = seq == 0,
                    SyainBaseId = syainBaseId,
                    Deleted = false
                });

            record.UploadTime = timeProvider.Now();

            if (hasUpload)
            {
                // アップロードがあるときだけファイル名を更新
                record.PhotoName = Path.GetFileNameWithoutExtension(photoFile!.FileName);
            }

            // 元画像（PhotoDatas）の更新（アップロード時のみ）
            if (hasUpload)
            {
                var photoDataRecord = await db.PhotoDatas.FirstOrDefaultAsync(x => x.SyainPhotoId == record.Id)
                    ?? await db.PhotoDatas.AddReturnAsync(new PhotoData
                    {
                        SyainPhoto = record
                    });

                photoDataRecord.Photo = originalPhotoData;
            }

            // トリミング後画像（PhotoAfterProcessTnDatas）は常に更新
            var photoAfterProcessTnDataRecord =
                await db.PhotoAfterProcessTnDatas.FirstOrDefaultAsync(x => x.SyainPhotoId == record.Id)
                ?? await db.PhotoAfterProcessTnDatas.AddReturnAsync(new PhotoAfterProcessTnData
                {
                    SyainPhoto = record
                });

            photoAfterProcessTnDataRecord.Photo = photoData;

            await db.SaveChangesAsync();

            return Success();
        }

        /// <summary>
        /// 写真削除
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var syainBaseId = LoginInfo?.User?.SyainBaseId;
            if (syainBaseId == null || syainBaseId <= 0)
            {
                ModelState.AddModelError("SyainPhoto.Id", "ユーザー情報を取得できませんでした。");
                return ModelState.ErrorJson()!;
            }

            var record = await db.SyainPhotos.FirstOrDefaultAsync(p => p.Id == SyainPhoto.Id && p.SyainBaseId == syainBaseId && !p.Deleted);

            if (record == null)
            {
                ModelState.AddModelError("SyainPhoto.PhotoFile",  "顔写真は存在しません。");
                return ModelState.ErrorJson()!;
            }

            record.Selected = false;
            record.Deleted = true;

            await db.SaveChangesAsync();

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
                ModelState.AddModelError("SyainPhoto.PhotoFile", "ファイルサイズは5MB以下である必要があります。");
                return;
            }

            var allowedExtensions = new[] { ".bmp", ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = System.IO.Path.GetExtension(photoFile.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("SyainPhoto.PhotoFile", "BMP、JPG、PNG、GIF形式のみアップロード可能です。");
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

            public IFormFile? PhotoFile { get; set; }

            /// <summary>更新かどうか</summary>
            public bool IsUpdate { get; set; }

            /// <summary>現在の写真（Base64）</summary>
            public string CurrentPhotoBase64 { get; set; } = string.Empty;

            /// <summary>
            /// トリミング開始位置X
            /// </summary>
            public int CropX { get; set; }

            /// <summary>
            /// トリミング開始位置Y
            /// </summary>
            public int CropY { get; set; }

            /// <summary>
            /// トリミング幅
            /// </summary>
            public int CropWidth { get; set; }

            /// <summary>
            /// トリミング高さ
            /// </summary>
            public int CropHeight { get; set; }
        }
    }
}
