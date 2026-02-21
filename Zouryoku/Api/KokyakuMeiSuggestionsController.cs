using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Zouryoku.Attributes;
using static Zouryoku.Utils.StringUtil;

namespace Zouryoku.Api
{
    /// <summary>
    /// 顧客名のオートコンプリート時の候補取得用APIコントローラー
    /// GET: api/KokyakuMeiSuggestions?term={term}
    /// </summary>
    /// <param name="db">DBコンテキスト</param>
    [Route("api/[controller]")]
    [ApiController]
    [FunctionAuthorization]
    public class KokyakuMeiSuggestionsController(ZouContext db) : ControllerBase
    {
        // 候補の最大取得件数
        private const int MaxSuggestionsCount = 5;

        private readonly ZouContext _db = db;

        /// <summary>
        /// 顧客名のオートコンプリート候補の取得メソッド
        /// 入力文字列を正規化し、検索用顧客名称と検索用顧客名称カナに対する部分一致検索を行い、
        /// 顧客名カナ昇順で5件まで取得する
        /// </summary>
        /// <param name="term">入力文字列</param>
        /// <returns>
        /// JSON形式の顧客名称のリスト
        /// キーは"label"
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetSuggestionsAsync(string? term)
        {
            // termが空文字のときは空リストを返却
            if (string.IsNullOrEmpty(term))
                return new JsonResult(new List<string>());

            // 検索ワードを正規化
            var normalized = NormalizeString(term);

            // 顧客名の候補を取得
            // 検索用顧客名称と検索用顧客名カナに入力が含まれるものを検索する
            // 最大表示件数分だけを顧客名カナ昇順で取得
            // NOTE: DistinctとOrderByを組み合わせるとEF CoreがOrderByを無視するため、GroupByで代替している
            var suggestions = await _db.KokyakuKaishas
                .Where(customer => customer.SearchName.Contains(normalized)
                    || customer.SearchNameKana.Contains(normalized))
                .GroupBy(customer => customer.Name)
                .Select(grp => new
                {
                    Name = grp.Key,
                    grp.First().NameKana,
                })
                .OrderBy(x => x.NameKana)
                .Take(MaxSuggestionsCount)
                .Select(x => new { label = x.Name })
                .ToListAsync();

            // JSON形式で返却
            return new JsonResult(suggestions);
        }
    }
}
