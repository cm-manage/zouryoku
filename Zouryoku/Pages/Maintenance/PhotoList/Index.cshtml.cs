using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.Maintenance.PhotoList
{
    /// <summary>
    /// 顔写真一覧ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class IndexModel : BasePageModel<IndexModel>
    {
        private readonly ZouContext _context;

        /// <summary>
        /// １行の写真数
        /// </summary>
        private const int RowPhotoCount = 10;

        /// <summary>
        /// 部署ごとの社員写真情報
        /// </summary>
        public List<BusyoPhotoGroup> BusyoPhotoGroups { get; set; } = new();

        public IndexModel(ZouContext context, ILogger<IndexModel> logger, IOptions<AppConfig> options)
            : base(context, logger, options)
            => _context = context;

        /// <summary>
        /// 顔写真一覧画面の初期表示処理を行います。
        /// 全部署と所属社員の写真情報を階層的に取得し、部署ごとにグループ化して表示します。
        /// </summary>
        public async Task OnGetAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            const long rootBusyoId = 0;

            // 全部署を1回のクエリで取得（N+1問題を解決）
            var allBusyos = await _context.Busyos
                .AsNoTracking()
                .Where(b =>
                    b.IsActive &&
                    b.StartYmd < today &&
                    b.EndYmd > today)
                .OrderBy(b => b.Jyunjyo)
                .ToListAsync();

            // メモリ上で階層構造を構築
            var busyosByParent = allBusyos
                .GroupBy(b => b.OyaId ?? 0)
                .ToDictionary(g => g.Key, g => g.ToList());

            // ルート部署から再帰的に子部署を取得
            var hierarchicalBusyos = GetBusyoHierarchy(rootBusyoId, busyosByParent);
            
            var busyoIds = hierarchicalBusyos.Select(b => b.Id).ToList();

            // 全社員を1回のクエリで取得
            var allSyains = await _context.Syains
                .AsNoTracking()
                .Where(s => busyoIds.Contains(s.BusyoId) && !s.Retired)
                .Include(s => s.SyainBase)
                    .ThenInclude(sb => sb.SyainPhotos)
                        .ThenInclude(sp => sp.PhotoAfterProcessTnData)
                .ToListAsync();
         
            // メモリ上で部署ごとにグループ化して処理
            var syainsByBusyo = allSyains.GroupBy(s => s.BusyoId).ToDictionary(g => g.Key, g => g.ToList());

            // 一意な部署のみを取得（重複排除）
            var uniqueBusyos = hierarchicalBusyos.DistinctBy(b => b.Id).ToList();
            // 社員がいる部署のみを処理
            foreach (var busyo in uniqueBusyos.Where(b => syainsByBusyo.ContainsKey(b.Id)))
            {
                var syains = syainsByBusyo[busyo.Id];

                var syainPhotos = syains
                    .OrderBy(s => s.Jyunjyo)
                    .Select(s => 
                    {
                        var selectedPhoto = s.SyainBase?.SyainPhotos
                            .Where(p => p.Selected && !p.Deleted)
                            .FirstOrDefault();
                        
                        var hasPhoto = selectedPhoto is { PhotoAfterProcessTnData: not null } &&
                                       selectedPhoto.PhotoAfterProcessTnData.Any(pt => pt.Photo != null);
                        
                        var photoBase64 = string.Empty;
                        var uploadTime = DateTime.MinValue;
                        
                        if (hasPhoto)
                        {
                            var photoData = selectedPhoto!.PhotoAfterProcessTnData
                                .FirstOrDefault(pt => pt.Photo != null);
                            if (photoData?.Photo != null)
                            {
                                photoBase64 = Convert.ToBase64String(photoData.Photo);
                            }
                            uploadTime = selectedPhoto.UploadTime;
                        }
                        
                        return new SyainPhotoInfo
                        {
                            SyainId = s.Id,
                            SyainBaseId = s.SyainBaseId,
                            SyainName = s.Name,
                            HasPhoto = hasPhoto,
                            PhotoBase64 = photoBase64,
                            UploadTime = uploadTime
                        };
                    })
                    .ToList();

                BusyoPhotoGroups.Add(new BusyoPhotoGroup
                {
                    BusyoName = busyo.Name,
                    PhotoChunks = ChunkPhotosByRow(syainPhotos)
                });
            }
        }

        /// <summary>
        /// 社員写真を1行の人数ごとにチャンク分割
        /// </summary>
        private List<List<SyainPhotoInfo>> ChunkPhotosByRow(List<SyainPhotoInfo> photos)
        {
            var chunks = new List<List<SyainPhotoInfo>>();

            for (int i = 0; i < photos.Count; i += RowPhotoCount)
            {
                chunks.Add(photos.Skip(i).Take(RowPhotoCount).ToList());
            }

            return chunks;
        }

        /// <summary>
        /// メモリ上で部署の階層構造を再帰的に構築
        /// </summary>
        private List<Busyo> GetBusyoHierarchy(long parentBusyoId, Dictionary<long, List<Busyo>> busyosByParent, int depth = 0)
        {
            const int maxDepth = 10;

            if (depth > maxDepth || !busyosByParent.TryGetValue(parentBusyoId, out var children))
            {
                return new();
            }

            var result = new List<Busyo>();

            // メモリ上で子部署を再帰的に取得
            foreach (var child in children)
            {
                result.Add(child);
                var grandChildren = GetBusyoHierarchy(child.Id, busyosByParent, depth + 1);
                result.AddRange(grandChildren);
            }

            return result;
        }

        /// <summary>
        /// 部署ごとの社員写真グループ
        /// </summary>
        public class BusyoPhotoGroup
        {
            public string BusyoName { get; set; } = string.Empty;
            public List<List<SyainPhotoInfo>> PhotoChunks { get; set; } = new();
        }

        /// <summary>
        /// 社員の写真情報
        /// </summary>
        public class SyainPhotoInfo
        {
            public long SyainId { get; set; }
            public long SyainBaseId { get; set; }
            public string SyainName { get; set; } = string.Empty;
            public bool HasPhoto { get; set; }
            /// <summary>設定中の写真（Base64）</summary>
            public string PhotoBase64 { get; set; } = string.Empty;
            /// <summary>写真のアップロード日時</summary>
            public DateTime UploadTime { get; set; }
        }
    }
}