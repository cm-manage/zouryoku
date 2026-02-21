using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Zouryoku.Attributes;
using static Zouryoku.Utils.StringUtil;

namespace Zouryoku.Api
{
    /// <summary>
    /// 契約先・受注先のオートコンプリート時の候補取得用APIコントローラー
    /// GET: api/KeiyakusakiJuchusakiSuggestions?term={term}
    /// </summary>
    /// <param name="db">DBコンテキスト</param>
    [Route("api/[controller]")]
    [ApiController]
    [FunctionAuthorization]
    public class KeiyakusakiJuchusakiSuggestionsController(ZouContext db) : ControllerBase
    {
        // 候補の最大取得件数
        private const int MaxSuggestionsCount = 5;

        private readonly ZouContext _db = db;

        /// <summary>
        /// 契約先・受注先のオートコンプリート候補の取得メソッド
        /// 入力文字列を正規化し、検索用契約先,検索用契約先カナ,
        /// 検索用受注先,検索用受注先カナに対する部分一致検索を行う
        /// </summary>
        /// <param name="term">入力文字列</param>
        /// <returns>
        /// JSON形式の契約先・受注先のリスト
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

            // 契約先・受注先の候補を取得
            // ① 契約先（検索用契約先名称・検索用契約先カナ）を検索
            // ② 受注先（検索用受注先名称・検索用受注先カナ）を検索
            // ③ ①と②をUNIONで結合
            // ④ 名称でグループ化して重複排除
            // ⑤ 最大表示件数分だけをカナ昇順で取得
            // NOTE: DistinctとOrderByを組み合わせるとEF CoreがOrderByを無視するため、GroupByで代替している

            // 契約先を検索
            var keiQuery = _db.KingsJuchus
                .Where(j =>
                    (j.SearchKeiNm != null && j.SearchKeiNm.Contains(normalized)) ||
                    (j.SearchKeiKn != null && j.SearchKeiKn.Contains(normalized)))
                .Select(j => new
                {
                    Name = j.KeiNm,
                    NameKana = j.KeiKn
                });

            // 受注先を検索
            var jucQuery = _db.KingsJuchus
                .Where(j =>
                    (j.SearchJucNm != null && j.SearchJucNm.Contains(normalized)) ||
                    (j.SearchJucKn != null && j.SearchJucKn.Contains(normalized)))
                .Select(j => new
                {
                    Name = j.JucNm,
                    NameKana = j.JucKn
                });

            // 契約先・受注先を結合し、名称で重複排除してカナ昇順で取得
            var suggestions = await keiQuery
                .Union(jucQuery)
                .Where(x => x.Name != null)
                .GroupBy(x => x.Name)
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
