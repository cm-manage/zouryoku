using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using static Zouryoku.Utils.Const;

namespace Zouryoku.Pages.BusyoSentaku
{
    /// <summary>
    /// 部署選択ページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class IndexModel : BasePageModel<IndexModel>
    {
        public IndexModel(ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> options)
            : base(db, logger, options) { }

        public override bool UseInputAssets => true;

        /// <summary>
        /// 複数選択フラグ
        /// </summary>
        public bool MultiFlag { get; set; }

        /// <summary>
        /// FancyTreeで初期選択させる部署IDリスト
        /// </summary>
        public List<long> PreSelectedIds { get; set; } = [];

        /// <summary>
        /// 選択検証リクエスト
        /// </summary>
        public class ValidateSelectionRequest
        {
            /// <summary>
            /// 選択された部署IDリスト
            /// </summary>
            [Display(Name = "部署")]
            public List<long> SelectedIds { get; set; } = [];
        }

        /// <summary>
        /// 画面初期表示
        /// </summary>
        /// <param name="multiFlag">複数選択フラグ</param>
        /// <param name="preSelectedIds">FancyTreeで初期選択させる部署IDリスト</param>
        /// <returns>部署選択ページ</returns>
        public IActionResult OnGet(bool multiFlag, List<long>? preSelectedIds)
        {
            MultiFlag = multiFlag;
            PreSelectedIds = preSelectedIds ?? [];
            return Page();
        }

        /// <summary>
        /// 部署ツリー取得（Fancytreeのsourceを生成する）
        /// </summary>
        /// <returns>部署ツリーJSONデータ</returns>
        public async Task<IActionResult> OnGetTreeAsync()
        {
            var today = DateTime.Now.ToDateOnly();
            var allBusyo = await db.Busyos
                .AsNoTracking()
                .Where(b => b.IsActive && b.StartYmd <= today && today <= b.EndYmd)
                .OrderBy(b => b.Jyunjyo)
                .ToListAsync();

            var lookup = allBusyo.ToLookup(b => b.OyaId);
            var tree = BuildFancyTree(null, lookup);

            return new JsonResult(tree);
        }

        /// <summary>
        /// （複数選択）確定ボタン押下時バリデーションチェック
        /// </summary>
        /// <param name="input">選択検証リクエスト</param>
        /// <returns>選択検証結果JSONデータ</returns>
        public IActionResult OnPostValidateSelection(ValidateSelectionRequest input)
        {
            if (input.SelectedIds == null || input.SelectedIds.Count == 0)
            {
                ModelState.AddModelError(nameof(input.SelectedIds),
                    string.Format(ErrorSelectRequired, "部署"));
            }

            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            return SuccessJson();
        }

        /// <summary>
        /// FancyTree用部署ツリー構築
        /// </summary>
        /// <param name="parentId">親部署ID</param>
        /// <param name="lookup">部署IDをキー、部署リストを値とするルックアップ</param>
        /// <returns>FancyTree用部署ツリーノードリスト</returns>
        private List<FancyNode> BuildFancyTree(long? parentId, ILookup<long?, Busyo> lookup)
        {
            return lookup[parentId]
                .Select(b => new FancyNode
                {
                    Title = b.Name,
                    Key = b.Id,
                    Folder = lookup[b.Id].Any(),
                    Data = new FancyData
                    {
                        Jyunjyo = b.Jyunjyo,
                        Code = b.Code,
                        OyaId = b.OyaId
                    },
                    Children = BuildFancyTree(b.Id, lookup)
                })
                .ToList();
        }
    }

    /// <summary>
    /// FancyTreeノード
    /// </summary>
    public class FancyNode
    {
        /// <summary>
        /// 表示（＝部署名）
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// キー（＝ID）
        /// </summary>
        public long Key { get; set; }

        /// <summary>
        /// フォルダか（子要素があるか）
        /// </summary>
        public bool Folder { get; set; }

        /// <summary>
        /// 子要素リスト（再帰）
        /// </summary>
        public List<FancyNode> Children { get; set; } = [];

        /// <summary>
        /// FancyData
        /// </summary>
        public FancyData Data { get; set; } = new();
    }

    /// <summary>
    /// FancyTreeノードデータ
    /// </summary>
    public class FancyData
    {
        /// <summary>
        /// 並び順序
        /// </summary>
        public int Jyunjyo { get; set; }

        /// <summary>
        /// 部署番号
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 親ID
        /// </summary>
        public long? OyaId { get; set; }
    }
}
