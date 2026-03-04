using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.Maintenance.Syains
{
    /// <summary>
    /// 個人設定ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class IndexModel(
      　ZouContext db,
      　ILogger<IndexModel> logger,
      　IOptions<AppConfig> optionsAccessor,
        ICompositeViewEngine viewEngine,
        timeProviderTimeProvider? timeProvider = null)
        : BasePageModel<IndexModel>(db, logger, optionsAccessor, viewEngine, timeProvider)
    {
        /// <summary>
        /// 顔写真設定情報
        /// </summary>
        public UserPhotoInfo PhotoInfo { get; set; } = new();

        /// <summary>
        /// 入力画面用共通CSS/JSをレイアウトで読み込むかどうかのフラグ
        /// </summary>
        public override bool UseInputAssets { get; } = true;

        /// <summary>
        /// 画面初期表示
        /// </summary>
        public async Task OnGetAsync()
        {
            // ログインユーザーの社員情報を取得
            var syain = await db.Syains
                .AsNoTracking()
                .Where(s => s.Id == LoginInfo.User.Id)
                .Include(s => s.SyainBase)
                    .ThenInclude(sb => sb.SyainPhotos)
                        .ThenInclude(sp => sp.PhotoAfterProcessTnData)
                .FirstOrDefaultAsync();

            if (syain?.SyainBase != null)
            {
                PhotoInfo.SyainName = syain.Name;
                PhotoInfo.SyainBaseId = syain.SyainBaseId;

                // すべての写真を取得（削除されていないもの）
                PhotoInfo.AllPhotos = syain.SyainBase.SyainPhotos
                    .Where(p => !p.Deleted)
                    .OrderByDescending(p => p.UploadTime)
                    .Select(p =>
                    {
                        var photoData = p.PhotoAfterProcessTnData
                            .FirstOrDefault(pt => pt.Photo != null);

                        return new PhotoItemInfo
                        {
                            Id = p.Id,
                            PhotoName = p.PhotoName,
                            UploadTime = p.UploadTime,
                            IsSelected = p.Selected,
                            HasPhotoData = photoData != null,
                            PhotoBase64 = photoData != null
                                ? Convert.ToBase64String(photoData.Photo)
                                : string.Empty
                        };
                    })
                    .ToList();

                // 設定中の写真情報も保持
                var selectedPhoto = syain.SyainBase.SyainPhotos
                    .Where(p => p.Selected && !p.Deleted)
                    .FirstOrDefault();

                if (selectedPhoto?.PhotoAfterProcessTnData.Any(pt => pt.Photo != null) == true)
                {
                    var photoData = selectedPhoto.PhotoAfterProcessTnData
                        .FirstOrDefault(pt => pt.Photo != null);

                    if (photoData?.Photo != null)
                    {
                        PhotoInfo.CurrentPhotoBase64 = Convert.ToBase64String(photoData.Photo);
                        PhotoInfo.HasPhoto = true;
                        PhotoInfo.CurrentPhotoId = selectedPhoto.Id;
                        PhotoInfo.PhotoName = selectedPhoto.PhotoName;
                    }
                }
            }
        }

        /// <summary>
        /// 写真の選択を変更（他の写真を有効にする）
        /// </summary>
        public async Task<IActionResult> OnPostSelectPhotoAsync(long photoId)
        {
            var syainBaseId = LoginInfo.User.SyainBaseId;
            if (syainBaseId <= 0)
            {
                return ErrorJson("ユーザー情報を取得できませんでした。");
            }

            // トランザクション開始
            using var transaction = await db.Database.BeginTransactionAsync();

            // 既存の設定中の写真をすべて非選択に変更
            var selectedPhotos = await db.SyainPhotos
                .Where(p => p.SyainBaseId == syainBaseId)
                .ToListAsync();

            foreach (var photo in selectedPhotos)
            {
                photo.Selected = false;
            }

            // 指定の写真を選択
            var targetPhoto = await db.SyainPhotos
                .Where(p => p.Id == photoId && p.SyainBaseId == syainBaseId && !p.Deleted)
                .FirstOrDefaultAsync();

            if (targetPhoto == null)
            {
                return ErrorJson("写真が見つかりません。");
            }

            targetPhoto.Selected = true;
            // UploadTimeを現在時刻に更新してETagを変更させ、ブラウザキャッシュを無効化
            targetPhoto.UploadTime = timeProvider.Now();

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return SuccessJson("写真を設定しました。");
        }

        /// <summary>
        /// ユーザーの顔写真情報
        /// </summary>
        public class UserPhotoInfo
        {
            /// <summary>社員名</summary>
            public string SyainName { get; set; } = string.Empty;

            /// <summary>社員BASEマスタID</summary>
            public long SyainBaseId { get; set; }

            /// <summary>写真があるかどうか</summary>
            public bool HasPhoto { get; set; }

            /// <summary>現在の設定中の写真ID</summary>
            public long CurrentPhotoId { get; set; }

            /// <summary>現在の設定中の写真（Base64）</summary>
            public string CurrentPhotoBase64 { get; set; } = string.Empty;

            /// <summary>現在の写真名</summary>
            public string PhotoName { get; set; } = string.Empty;

            /// <summary>すべての写真情報</summary>
            public List<PhotoItemInfo> AllPhotos { get; set; } = new();
        }

        /// <summary>
        /// 写真情報
        /// </summary>
        public class PhotoItemInfo
        {
            /// <summary>写真ID</summary>
            public long Id { get; set; }

            /// <summary>写真名</summary>
            public string PhotoName { get; set; } = string.Empty;

            /// <summary>アップロード日時</summary>
            public DateTime UploadTime { get; set; }

            /// <summary>設定中かどうか</summary>
            public bool IsSelected { get; set; }

            /// <summary>写真データがあるかどうか</summary>
            public bool HasPhotoData { get; set; }

            /// <summary>写真（Base64）</summary>
            public string PhotoBase64 { get; set; } = string.Empty;
        }
    }
}