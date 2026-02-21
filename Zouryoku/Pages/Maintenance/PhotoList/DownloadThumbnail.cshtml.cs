using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using System.Security.Cryptography;
using Zouryoku.Attributes;

namespace Zouryoku.Pages.Maintenance.PhotoList
{
    /// <summary>
    /// 社員のサムネイル写真をダウンロード
    /// </summary>
    public class DownloadThumbnailModel : PageModel
    {
        private const int MinImageHeaderBytes = 4;
        private readonly ZouContext _context;
        private readonly ILogger<DownloadThumbnailModel> _logger;

        [FunctionAuthorizationAttribute]
        public DownloadThumbnailModel(ZouContext context, ILogger<DownloadThumbnailModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(long baseid)
        {
            var syainBase = await _context.SyainBases
                .AsNoTracking()
                .Where(sb => sb.Id == baseid)
                .Include(sb => sb.SyainPhotos)
                    .ThenInclude(sp => sp.PhotoAfterProcessTnData)
                .FirstOrDefaultAsync();

            if (syainBase == null)
            {
                return NotFound();
            }

            var selectedPhoto = syainBase.SyainPhotos
                .Where(p => p.Selected && !p.Deleted)
                .FirstOrDefault();

            if (selectedPhoto == null)
            {
                return NotFound();
            }

            var photoData = selectedPhoto
                .PhotoAfterProcessTnData
                .FirstOrDefault()?
                .Photo;

            if (photoData == null)
            {
                return NotFound();
            }

            var mimeType = DetectImageMimeType(photoData);

            // 写真データの更新日時とハッシュ値を含めたETagを生成
            var eTag = GenerateETag(baseid, selectedPhoto.UploadTime, photoData);

            // キャッシュヘッダーとETagを設定
            SetCacheHeaders(eTag);

            // If-None-MatchヘッダーとETagを比較
            if (Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch))
            {
                // クライアントが送信したETagと一致する場合は304 Not Modifiedを返す
                if (ifNoneMatch.Contains(eTag))
                {
                    return StatusCode(304);
                }
            }

            return File(photoData, mimeType);
        }

        /// <summary>
        /// キャッシュヘッダーとETagを設定
        /// </summary>
        /// <param name="eTag">レスポンスヘッダーに設定するETag値</param>
        private void SetCacheHeaders(string eTag)
        {
            Response.Headers.CacheControl = "public, max-age=86400";
            Response.Headers.ETag = eTag;
        }

        /// <summary>
        /// ETagを生成（baseid + アップロード時間 + 写真データハッシュ）
        /// </summary>
        private static string GenerateETag(long baseid, DateTime uploadTime, byte[] photoData)
        {
            // baseid + UploadTimeのTicksを組み合わせてハッシュ値を計算
            var eTagSource = $"{baseid}_{uploadTime.Ticks}";

            // 追加のセキュリティとしてデータのハッシュも含める
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(photoData);
                var hashString = Convert.ToBase64String(hash)[..8]; // 最初の8文字を使用

                eTagSource = $"{eTagSource}_{hashString}";
            }

            return $"\"{eTagSource}\"";
        }

        /// <summary>
        /// 画像バイナリからMIMEタイプを判定
        /// </summary>
        private static string DetectImageMimeType(byte[] imageData)
        {
            if (imageData.Length < MinImageHeaderBytes)
            {
                return "image/jpeg";
            }

            return (imageData[0], imageData[1], imageData[2], imageData[3]) switch
            {
                (0xFF, 0xD8, _, _) => "image/jpeg",
                (0x89, 0x50, 0x4E, 0x47) => "image/png",
                (0x47, 0x49, 0x46, _) => "image/gif",
                _ => "image/jpeg"
            };
        }
    }
}