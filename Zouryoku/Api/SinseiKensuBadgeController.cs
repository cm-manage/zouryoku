using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Enums;
using Model.Model;
using Zouryoku.Attributes;
using Zouryoku.Data;
using Zouryoku.Extensions;

using static Model.Enums.ApprovalStatus;

namespace Zouryoku.Api
{
    /// <summary>
    /// 共通ヘッダ・メニューの申請確認のバッジ表示用APIコントローラー
    /// GET: api/SinseiKensuBadge/count
    /// </summary>
    /// <param name="db">DBコンテキスト</param>
    [Route("api/[controller]")]
    [ApiController]
    [FunctionAuthorization]
    public class SinseiKensuBadgeController(ZouContext db, TimeProvider? timeProvider) : ControllerBase
    {
        // ---------------------------------------------
        // プライベートプロパティ
        // ---------------------------------------------
        private LoginInfo LoginInfo => HttpContext.Session.LoginInfo();

        // ---------------------------------------------
        // DI
        // ---------------------------------------------
        private readonly TimeProvider timeProvider = timeProvider ?? TimeProvider.System;

        // ---------------------------------------------
        // 申請件数取得API
        // ---------------------------------------------
        /// <summary>
        /// 申請確認のバッジ表示用の申請件数の取得メソッド
        /// </summary>
        /// <returns>
        /// 指示承認者の場合、一次承認予定の申請件数を取得
        /// 最終指示承認者の場合、最終承認予定の申請件数を取得
        /// 両方の権限を持つユーザーの場合、両方の件数を合算して取得
        /// その他のユーザーの場合は0を返却
        /// </returns>
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCountAsync()
        {
            Syain loginUser = LoginInfo.User;

            // 指示承認者・最終指示承認者のどちらでもない場合
            // 以降の処理を行わない
            if (!loginUser.IsInstructionApprover && !loginUser.IsFinalInstructionApprover)
                return 0;

            int totalApprovalCount = 0;

            // 指示承認者の場合、一次承認予定の申請件数を加算
            if (loginUser.IsInstructionApprover)
            {
                totalApprovalCount += await FetchInstructionCountAsync(loginUser, timeProvider.Today());

            }

            // 最終指示承認者の場合、最終承認予定の申請件数を加算
            if (loginUser.IsFinalInstructionApprover)
            {
                totalApprovalCount += await FetchFinalInstructionCountAsync();
            }

            return totalApprovalCount;
        }

        // ---------------------------------------------
        // 指示承認者の未承認申請件数取得ロジック
        // ---------------------------------------------
        /// <summary>
        /// 権限が指示承認者の場合の未承認申請件数の取得
        /// 対象部署：自部署、承認部署が自部署になっている部署、他の指示承認者がいない子部署
        /// </summary>
        /// <param name="loginUser">申請を承認するログインユーザー</param>
        /// <param name="today">指定日</param>
        /// <returns>
        /// ログインユーザーが承認を行う一次承認待ち申請件数
        /// 指定した日付時点で有効な部署・指示承認者情報をもとに件数を取得
        /// </returns>
        private async Task<int> FetchInstructionCountAsync(Syain loginUser, DateOnly today)
        {
            // 影響部門範囲
            HashSet<long> impactBusyoIds = await GetImpactDepartmentIdsAsync(loginUser.BusyoId, today);

            // 部署内の社員の申請件数の合計を取得
            return await db.UkagaiHeaders
                .Where(u => u.ShoninSyainId == null)
                .Where(u => impactBusyoIds.Contains(u.Syain.BusyoId))
                .Where(u => u.Status == 承認待)
                .Where(u => u.Invalid == false)
                .AsNoTracking()
                .CountAsync();
        }

        /// <summary>
        /// 指示承認者が申請を処理する必要がある部署を取得
        /// </summary>
        /// <param name="busyoId">ログインユーザーが所属している部署ID</param>
        /// <param name="today">指定日</param>
        /// <returns>ログインユーザーが承認を行う必要がある部署IDのHashSet情報</returns>
        public async Task<HashSet<long>> GetImpactDepartmentIdsAsync(long busyoId, DateOnly today)
        {
            // 有効な部署を全件取得
            List<Busyo> allBusyos = await FetchActiveBusyosAsync(today);

            // 子部署の取得

            // 親IDと子部署のLookup作成
            // 子部署の再起取得処理を効率化
            ILookup<long, Busyo> lookup = allBusyos
                .Where(b => b.OyaId.HasValue)
                .ToLookup(b => b.OyaId!.Value);

            // 親IDと子部署のLookupを使用し承認対象の子部署を取得
            IEnumerable<long> childIds = GetChildDepartmentIds(busyoId, lookup, []);

            // 自部署＋承認部署が設定されている部署の取得
            IEnumerable<long> approvalIds = allBusyos
                .Where(b => b.Id == busyoId || b.ShoninBusyoId == busyoId)
                .Select(b => b.Id);

            // 子部署＋自部署＋承認部署が設定されている部署の合算
            // HashSetで重複排除を高速化
            return childIds
                .Concat(approvalIds)
                .ToHashSet();
        }

        /// <summary>
        /// 現在適用中の部署を全件取得
        /// </summary>
        /// <param name="today">指定日</param>
        /// <returns>現在適用中の部署情報リスト</returns>
        private async Task<List<Busyo>> FetchActiveBusyosAsync(DateOnly today)
        {
            return await db.Busyos
                // 指示承認者の社員情報のみを含めて部署を取得
                .Include(b => b.Syains.Where(s =>
                    s.Kengen.HasFlag(EmployeeAuthority.指示承認者) && s.StartYmd <= today && today <= s.EndYmd))
                .Where(x => x.IsActive && x.StartYmd <= today && today <= x.EndYmd)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// 子部署の再帰的取得
        /// </summary>
        /// <param name="lookup"> 親ID → 子部署のLookup</param>
        /// <param name="parentId">親部署ID</param>
        /// <param name="visitedId">探索済みID</param>
        /// <returns>子部署IDの列挙</returns>
        private static IEnumerable<long> GetChildDepartmentIds(
            long parentId,
            ILookup<long, Busyo> lookup,
            HashSet<long> visitedId)
        {
            // 無限再起対策
            if (!visitedId.Add(parentId))
            {
                return [];
            }

            return lookup[parentId]
                // 指示承認者の社員がいない部署のみを対象とする
                .Where(b => b.Syains.Count == 0)
                .SelectMany(b =>
                    new[] { b.Id }
                    .Concat(GetChildDepartmentIds(b.Id, lookup, visitedId)));
        }

        // ---------------------------------------------
        // 最終指示承認者の未承認申請件数取得ロジック
        // ---------------------------------------------
        /// <summary>
        ///　権限が最終指示承認者の場合の未承認申請件数の取得
        /// </summary>
        /// <returns>全体の最終承認待ちの申請件数</returns>
        private async Task<int> FetchFinalInstructionCountAsync()
        {
            //最終承認待ちの申請件数の合計を取得
            return await db.UkagaiHeaders
                .Where(u => u.ShoninSyainId != null)
                .Where(u => u.LastShoninSyainId == null)
                .Where(u => u.Status == 承認待)
                .Where(u => u.Invalid == false)
                .AsNoTracking()
                .CountAsync();
        }
    }
}
