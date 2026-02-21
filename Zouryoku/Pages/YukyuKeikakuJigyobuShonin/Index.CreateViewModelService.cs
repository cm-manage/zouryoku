using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Model;
using System.Collections.Immutable;
using static Model.Enums.LeavePlanStatus;

namespace Zouryoku.Pages.YukyuKeikakuJigyobuShonin
{
    public partial class IndexModel
    {
        private class CreateViewModelService(ZouContext db)
        {
            /// <summary>
            /// <see cref="Busyo.InverseOya"/> を再帰的に辿り、指定した親部署ID配下の全ての部署IDを取得する
            /// </summary>
            private static HashSet<long> GetInverseOyaBusyoIdSet(long busyoId, ILookup<long?, Busyo> inverseOyaBusyos)
            {
                var busyoIdSet = new HashSet<long>();
                var busyoIdStack = new Stack<long>();
                busyoIdStack.Push(busyoId);

                while (busyoIdStack.TryPop(out var currentBusyoId))
                {
                    if (!busyoIdSet.Add(currentBusyoId)) continue; // 循環を排除（既に追加済みであれば子要素も処理しない）

                    foreach (var b in inverseOyaBusyos[currentBusyoId].Where(b => b.BusyoBase.BumoncyoId is null))  // 部門長部署は対象外
                    {
                        busyoIdStack.Push(b.Id);
                    }
                }
                return busyoIdSet;
            }

            /// <summary>
            /// ログインユーザーの権限が人財承認権限か否かに応じて、部門長系または人財系の一覧表示用ビューモデル生成処理に振り分け、
            /// ログインユーザーが所属する部門長部署配下の部署に属する社員とその当年度の計画有給休暇情報を読み込み、
            /// 画面表示用の <see cref="JigyoubuShoninViewModel"/> を生成する。
            /// </summary>
            /// <param name="loginUserAuthority">
            /// ログインユーザーの権限。人財承認権限（<see cref="Authority.Jinzai"/>）の場合は人財用の処理に、
            /// それ以外の部門長またはそれに準じる権限の場合は部門長用の処理に振り分けられる。
            /// </param>
            /// <param name="bumoncyoBusyoId">ログインユーザーの所属部門長部署ID。部門長系の処理で使用される。</param>
            /// <param name="allBusyos">全ての部署情報を部署IDをキーとしたディクショナリで指定する。部門長系の処理で使用される。</param>
            /// <returns>部門長系または人財系のいずれかの処理で生成された、一覧表示用の <see cref="JigyoubuShoninViewModel"/>。</returns>
            public Task<JigyoubuShoninViewModel> CreateViewModelByAuthorityAsync(
                Authority loginUserAuthority, long bumoncyoBusyoId, Dictionary<long, Busyo> allBusyos)
            {
                if (loginUserAuthority != Authority.Jinzai)
                {
                    return CreateViewModelForBumoncyoBusyoAsync(loginUserAuthority, bumoncyoBusyoId, allBusyos);
                }

                return CreateViewModelForJinzaiAsync(loginUserAuthority);
            }

            /// <summary>
            /// 部門長権限での一覧表示ビューモデルを生成する内部実装メソッド。
            /// <see cref="CreateViewModelByAuthorityAsync(Authority, long, Dictionary{long, Busyo})"/> から呼び出される。
            /// </summary>
            /// <param name="loginUserAuthority">ログインユーザーの権限。部門長またはそれに準じる権限を指定する。</param>
            /// <param name="bumoncyoBusyoId">ログインユーザーの所属部門長部署ID。</param>
            /// <param name="allBusyos">全ての部署情報を部署IDをキーとしたディクショナリで指定する。</param>
            /// <returns>
            /// 部門長部署配下の全社員と、
            /// その社員ごとの当年度の計画有給休暇情報を含む <see cref="JigyoubuShoninViewModel"/>。
            /// </returns>
            private async Task<JigyoubuShoninViewModel> CreateViewModelForBumoncyoBusyoAsync(
               Authority loginUserAuthority, long bumoncyoBusyoId, Dictionary<long, Busyo> allBusyos)
            {
                // 検索条件の初期化
                var busyoIdSet = GetInverseOyaBusyoIdSet(bumoncyoBusyoId, allBusyos.Values.ToLookup(b => b.OyaId));

                // 検索結果の取得
                var syains = await db.Syains
                    .Where(s => busyoIdSet.Contains(s.BusyoId))
                    .OrderBy(s => s.Busyo.Jyunjyo)
                    .ThenByDescending(s => s.Jyunjyo)
                    .Include(s => s.Busyo)
                    .Include(s => s.SyainBase.YukyuKeikakus.Where(yk => yk.YukyuNendo.IsThisYear))
                    .ThenInclude(yk => yk.YukyuKeikakuMeisais)
                    .AsNoTracking()
                    .AsSplitQuery() // 社員Base→計画有給休暇→計画有給休暇明細で 1:多:多 になるので AsSplitQuery でデカルト積を回避
                    .ToListAsync();

                return CreateViewModel(loginUserAuthority, syains.Select(s => (s, s.SyainBase.YukyuKeikakus.SingleOrDefault())));
            }

            /// <summary>
            /// 人財承認権限ユーザー向けに、当年度の計画有給休暇が人財承認待ち状態の社員を抽出し、
            /// 表示用の <see cref="JigyoubuShoninViewModel"/> を生成する内部実装メソッド。
            /// <see cref="CreateViewModelByAuthorityAsync(Authority, long, Dictionary{long, Busyo})"/> から呼び出される。
            /// </summary>
            /// <param name="loginUserAuthority">ログインユーザーの権限（人財承認権限を含む想定）。</param>
            /// <returns>人財承認待ちの社員と、その当年度の計画有給休暇を含む <see cref="JigyoubuShoninViewModel"/>。</returns>
            private async Task<JigyoubuShoninViewModel> CreateViewModelForJinzaiAsync(Authority loginUserAuthority)
            {
                // 最終承認者用検索結果の取得
                var syains = await db.Syains
                    .Where(s => s.SyainBase.YukyuKeikakus
                        .Where(yk => yk.YukyuNendo.IsThisYear)
                        .Any(yk => yk.Status == 人財承認待ち))
                    .OrderBy(s => s.Busyo.Jyunjyo)
                    .ThenByDescending(s => s.Jyunjyo)
                    .Include(s => s.Busyo)
                    .Include(s => s.SyainBase.YukyuKeikakus.Where(yk => yk.YukyuNendo.IsThisYear))
                    .ThenInclude(yk => yk.YukyuKeikakuMeisais)
                    .AsNoTracking()
                    .AsSplitQuery() // 社員Base→計画有給休暇→計画有給休暇明細で 1:多:多 になるので AsSplitQuery でデカルト積を回避
                    .ToListAsync();

                return CreateViewModel(loginUserAuthority, syains.Select(s => (s, s.SyainBase.YukyuKeikakus.SingleOrDefault())));
            }

            /// <summary>
            /// 社員と計画有給休暇のペア コレクションから、画面表示用の <see cref="JigyoubuShoninViewModel"/> を生成する。
            /// 計画有給休暇が未登録の社員については、計画情報を未入力として補完したレコードを作成する。
            /// </summary>
            /// <param name="loginUserAuthority">
            /// ログインユーザーの権限情報。
            /// 生成される <see cref="JigyoubuShoninViewModel"/> の <c>Authority</c> プロパティに設定される。
            /// </param>
            /// <param name="syainAndYukyuKeikakus">
            /// 社員と、その社員に紐づく当年度の計画有給休暇（存在しない場合は <see langword="null"/>）のペアの列挙。
            /// 各ペアは <see cref="Keikaku"/> ビューモデルに変換され、
            /// <see cref="JigyoubuShoninViewModel"/> の <c>Keikakus</c> に格納される。
            /// </param>
            /// <returns>
            /// 指定された社員と計画有給休暇情報を基に構築された <see cref="JigyoubuShoninViewModel"/> オブジェクト。
            /// </returns>
            private static JigyoubuShoninViewModel CreateViewModel(
                Authority loginUserAuthority, IEnumerable<(Syain syain, YukyuKeikaku? yukyuKeikaku)> syainAndYukyuKeikakus) =>
                new JigyoubuShoninViewModel(
                    loginUserAuthority,
                    syainAndYukyuKeikakus.Select(o =>
                    {
                        var yukyuKeikaku = o.yukyuKeikaku;

                        return new Keikaku
                        {
                            // 計画有給休暇が未登録の社員のレコードは未入力として補完する
                            Id = yukyuKeikaku?.Id,
                            YukyuKeikakuStatus = yukyuKeikaku?.Status,
                            SyainName = o.syain.Name,
                            BusyoName = o.syain.Busyo.Name,
                            Version = yukyuKeikaku?.Version ?? 0,
                            Meisais = yukyuKeikaku?.YukyuKeikakuMeisais
                                .OrderBy(ykm => ykm.Ymd)
                                .Select(ykm => new Meisai
                                {
                                    Ymd = ykm.Ymd,
                                    IsTokukyu = ykm.IsTokukyu
                                })
                                .ToImmutableArray() ?? Meisai.EmptyMeisais
                        };
                    })
                    .ToImmutableArray());
        }
    }
}
