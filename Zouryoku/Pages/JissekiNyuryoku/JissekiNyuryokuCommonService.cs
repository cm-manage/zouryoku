using CommonLibrary.Extensions;
using LanguageExt;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Enums;
using Model.Extensions;
using Model.Model;
using ZouryokuCommonLibrary.Utils;

namespace Zouryoku.Pages.JissekiNyuryoku
{
    public class JissekiNyuryokuCommonService(ZouContext db)
    {
        private readonly ZouContext _db = db;
        private readonly JissekiNyuryokuQueryService _queryService = new(db);

        /// <summary>
        /// 日報へ登録する曜日の数値を取得する
        /// </summary>
        /// <param name="date">対象日付</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static short GetYoubiNumber(DateOnly date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Sunday => 1,
                DayOfWeek.Monday => 2,
                DayOfWeek.Tuesday => 3,
                DayOfWeek.Wednesday => 4,
                DayOfWeek.Thursday => 5,
                DayOfWeek.Friday => 6,
                DayOfWeek.Saturday => 7,
                _ => throw new ArgumentOutOfRangeException(nameof(date), "パラメータの値が不正です。")
            };
        }

        /// <summary>
        /// 対象日がリフレッシュデーか
        /// </summary>
        /// <param name="date">対象日</param>
        /// <returns>true:リフレッシュデー、false:リフレッシュデー以外、</returns>
        public async Task<bool> IsRefreshDayAsync(DateOnly date)
        {
            if (date.DayOfWeek == DayOfWeek.Wednesday || date.DayOfWeek == DayOfWeek.Friday)
                return true;

            var hikadoubi = await _queryService.FetchHikadoubiDataAsync(date);
            return hikadoubi?.RefreshDay == RefreshDayFlag.リフレッシュデー;
        }
        /// <summary>
        /// 対象日が非稼働日か
        /// </summary>
        /// <param name="date">対象日</param>
        /// <returns>true：非稼働日である、false：非稼働日ではない</returns>
        public async Task<bool> IsHikadoubiAsync(DateOnly date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return true;

            var hikadoubi = await _queryService.FetchHikadoubiDataAsync(date);
            return hikadoubi?.SyukusaijitsuFlag == HolidayFlag.祝祭日;
        }

        /// <summary>
        /// 対象日付のDayType取得
        /// </summary>
        /// <param name="date">対象日付</param>
        /// <returns>対象日付のDayType</returns>
        public async Task<DayType> GetDayTypeAsync(DateOnly date)
        {
            static bool IsWeekDay(DateOnly date)
            => date.DayOfWeek switch
            {
                DayOfWeek.Saturday => false,
                DayOfWeek.Sunday => false,
                _ => true
            };

            var _hikadoubi = await _queryService.FetchHikadoubiDataAsync(date);

            if (_hikadoubi == null || (_hikadoubi.SyukusaijitsuFlag == HolidayFlag.それ以外 && IsWeekDay(date)))
            {
                return DayType.平日;
            }
            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                return DayType.日曜;
            }
            return DayType.土曜祝祭日;
        }

        /// <summary>
        /// 日報へ登録する各時間を計算する
        /// </summary>
        /// <param name="vm">画面入力内容</param>
        /// <returns></returns>
        public async Task<TimeContainer> CalcJissekiNyuryokuTimeAsync(NippouInputViewModel vm)
        {
            var dayType = await GetDayTypeAsync(vm.JissekiDate);

            // 日報へ登録する各時間を計算する
            var timeCalc = TimeCalculationFactory.Create(dayType);
            var timeContainer = timeCalc.Calculate(vm);

            return timeContainer;
        }

        /// <summary>
        /// 出勤区分１と出勤区分２のそれぞれを列挙体定数値からIdへ変換する
        /// </summary>
        /// <param name="syukkinKbn1">出勤区分１</param>
        /// <param name="syukkinKbn2">出勤区分２</param>
        /// <returns>出勤区分１と出勤区分２のそれぞれのIdのペア</returns>
        public async Task<(long, long?)> ConvertSyukkinKbnId(AttendanceClassification syukkinKbn1, AttendanceClassification? syukkinKbn2)
        {
            var syukkinKbns = await _queryService.FetchSyukkinKubnAll();

            var syukkinKbnData1 = syukkinKbns.FirstOrDefault(x => x.Code == syukkinKbn1);
            var syukkinKbnData2 = syukkinKbns.FirstOrDefault(x => x.Code == syukkinKbn2);

            return (syukkinKbnData1!.Id, syukkinKbnData2?.Id);
        }


        // 部門プロセスリストを構築する際に使用するDTO
        private record BumonProcessRecord(long Id, string Code, string Name);

        /// <summary>
        /// 部門プロセスリストを取得する
        /// </summary>
        /// <returns>部門プロセスリスト</returns>
        public async Task<List<SelectListItem>> GetBumonProcessListAsync()
        {
            // 子のプロセスを取得
            var children = await _db.BumonProcesses
                    .AsNoTracking()
                    .Where(x => x.KaisyaProcessId != null && !x.Deleted)
                    .ToListAsync();

            // 親のプロセスを取得
            var parents = await _db.BumonProcesses
                .AsNoTracking()
                .Where(x => x.KaisyaProcessId == null && !x.Deleted)
                .ToListAsync();

            // 親のプロセスを辞書化
            var parentMap = parents.ToDictionary(x => x.Id);

            // コードと名前の組み立て & ソート
            var records = children.Select(child =>
            {
                // 親を持たないプロセス
                if (child.OyaId == null)
                    return new BumonProcessRecord(child.Id, child.Code.Trim(), $"{child.Code} {child.Name}");

                // 親を持つプロセス
                var parent = parentMap[child.OyaId.Value];
                return new BumonProcessRecord(child.Id, parent.Code.Trim() + child.Code.Trim(), $"{parent.Code.Trim()}{child.Code.Trim()} {parent.Name}({child.Name})");
            })
                .OrderBy(x => x.Code)
                .ToList();

            // ドロップダウンリスト用にビルド
            var items = records.Select(x =>
                    new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    }
                ).ToList();

            return items;
        }

        /// <summary>
        /// 振替休暇の残日数を取得する
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns>振替休暇の残日数</returns>
        public async Task<decimal> GetFurikyuZanNissuAsync(long syainId, DateOnly jissekiDate)
        {
            // 取得可能な振替休暇残
            var furikyuZansNotReached = await _queryService.FetchEffectiveFurikyuuZanAllAsync(syainId, jissekiDate);

            return furikyuZansNotReached.Sum(f =>
            {
                if (f.IsOneDay && f.SyutokuState == LeaveBalanceFetchStatus.未)
                    return Time.ichinichi;

                if (f.IsOneDay && f.SyutokuState == LeaveBalanceFetchStatus.半日)
                    return Time.hanniti;

                if (!f.IsOneDay && f.SyutokuState == LeaveBalanceFetchStatus.未)
                    return Time.hanniti;

                return Time.syukkinNashi;
            });
        }

        /// <summary>
        /// 時間外労働時間制限拡張の伺いを有効状態にします
        /// </summary>
        /// <param name="vm">実績入力画面の入力内容</param>
        /// <param name="nippouSyain">対象社員マスタ</param>
        /// <returns></returns>
        public async Task EnableJikangaiSeigenKakuchoAsync(NippouInputViewModel vm, Syain nippouSyain)
        {
            // 時間外労働時間制限拡張の伺い申請の有効化
            // 月末の日報の場合に処理
            if (vm.JissekiDate != vm.JissekiDate.GetEndOfMonth())
                return;

            var ukagaiHeader = await _queryService.FetchUkagaiHeaderOfJikangaiSeigenKakuchoForUpdate(nippouSyain!.Id, vm.JissekiDate);

            if (ukagaiHeader == null)
                return;

            if (!ukagaiHeader.Invalid)
                return;

            ukagaiHeader.Invalid = false;
        }

        /// <summary>
        /// 代理入力履歴を登録する
        /// </summary>
        /// <param name="nippou">日報</param>
        /// <param name="dairiSyainId">代理入力を行った社員のId</param>
        /// <param name="sousa">日報実績操作</param>
        /// <param name="sousaDt">代理入力を行った日時</param>
        /// <returns></returns>
        public void InsertDairiNyuryokuRireki(Nippou nippou, long dairiSyainId, DailyReportOperation sousa, DateTime sousaDt)
        {
            // 代理入力履歴
            DairiNyuryokuRireki dairiNyuryokuRireki = new()
            {
                DairiNyuryokuSyainId = dairiSyainId,
                DairiNyuryokuTime = sousaDt,
                NippouId = nippou.Id,
                NippouSousa = sousa,
                Invalid = false
            };
            _db.DairiNyuryokuRirekis.Add(dairiNyuryokuRireki);
        }

        /// <summary>
        /// 代理入力履歴を無効にする
        /// </summary>
        /// <param name="nippou">日報</param>
        public async Task InvalidateDairiNyuuryokuRirekiAsync(Nippou nippou)
        {
            var dairiNyuurokuRireki = await _queryService.FetchDairiNyuuryokuRirekiForUpdate(nippou.Id);

            if (dairiNyuurokuRireki == null)
                return;

            if (!dairiNyuurokuRireki.Invalid && dairiNyuurokuRireki.NippouSousa == DailyReportOperation.確定)
            {
                dairiNyuurokuRireki.Invalid = true;
            }
        }

        /// <summary>
        /// 日報を登録する
        /// </summary>
        /// <param name="vm">画面入力内容</param>
        /// <param name="syain">社員</param>
        /// <param name="timeContainer">時間計算結果</param>
        /// <param name="tourokuKubun">登録区分</param>
        public async Task<Nippou> InsertNippouAsync(NippouInputViewModel vm, Syain syain, TimeContainer timeContainer, DailyReportStatusClassification tourokuKubun)
        {
            // 出勤区分をIDに変換
            var syukkinKbnIdPair = await ConvertSyukkinKbnId(vm.SyukkinKubun1, vm.SyukkinKubun2);

            // 日報
            var nippou = new Nippou
            {
                SyainId = syain.Id,
                NippouYmd = vm.JissekiDate,
                Youbi = GetYoubiNumber(vm.JissekiDate),
                SyukkinHm1 = vm.SyukkinHm1,
                TaisyutsuHm1 = vm.TaisyutsuHm1,
                SyukkinHm2 = vm.SyukkinHm2,
                TaisyutsuHm2 = vm.TaisyutsuHm2,
                SyukkinHm3 = vm.SyukkinHm3,
                TaisyutsuHm3 = vm.TaisyutsuHm3,
                // 平日
                HJitsudou = timeContainer.Jitsudou,
                HZangyo = timeContainer.HZangyo,
                HWarimashi = timeContainer.HShinya,
                HShinyaZangyo = timeContainer.HZangyoShinya,
                // 土曜
                DJitsudou = timeContainer.DJitsudou,
                DZangyo = timeContainer.DZangyo,
                DWarimashi = timeContainer.DJitsudouShinya,
                DShinyaZangyo = timeContainer.DZangyoShinya,
                // 日曜
                NJitsudou = timeContainer.NJitsudou,
                NShinya = timeContainer.NShinya,
                // みなし対象外用残業
                TotalZangyo = timeContainer.TotalZangyo,
                KaisyaCode = (NippousCompanyCode)syain.KaisyaCode,
                IsRendouZumi = false,
                RendouYmd = null,
                TourokuKubun = tourokuKubun,
                KakuteiYmd = null,
                SyukkinKubunId1 = syukkinKbnIdPair.Item1,
                SyukkinKubunId2 = syukkinKbnIdPair.Item2,
            };

            _db.Nippous.Add(nippou);

            return nippou;
        }

        /// <summary>
        /// 日報を更新する
        /// </summary>
        /// <param name="nippou">日報</param>
        /// <param name="vm">画面入力内容</param>
        /// <param name="syain">社員</param>
        /// <param name="timeContainer">時間計算結果</param>
        /// <param name="tourokuKubun">登録区分</param>
        /// <returns></returns>
        public async Task UpdateNippouAsync(Nippou nippou, NippouInputViewModel vm, Syain syain, TimeContainer timeContainer, DailyReportStatusClassification tourokuKubun)
        {
            // 出勤区分をIDに変換
            var syukkinKbnIdPair = await ConvertSyukkinKbnId(vm.SyukkinKubun1, vm.SyukkinKubun2);

            //日報データを更新
            nippou.SyainId = syain.Id;
            nippou.NippouYmd = vm.JissekiDate;
            nippou.Youbi = GetYoubiNumber(vm.JissekiDate);
            nippou.SyukkinHm1 = vm.SyukkinHm1;
            nippou.TaisyutsuHm1 = vm.TaisyutsuHm1;
            nippou.SyukkinHm2 = vm.SyukkinHm2;
            nippou.TaisyutsuHm2 = vm.TaisyutsuHm2;
            nippou.SyukkinHm3 = vm.SyukkinHm3;
            nippou.TaisyutsuHm3 = vm.TaisyutsuHm3;
            // 平日
            nippou.HJitsudou = timeContainer.Jitsudou;
            nippou.HZangyo = timeContainer.HZangyo;
            nippou.HWarimashi = timeContainer.HShinya;
            nippou.HShinyaZangyo = timeContainer.HZangyoShinya;
            // 土曜
            nippou.DJitsudou = timeContainer.DJitsudou;
            nippou.DZangyo = timeContainer.DZangyo;
            nippou.DWarimashi = timeContainer.DJitsudouShinya;
            nippou.DShinyaZangyo = timeContainer.DZangyoShinya;
            // 日曜
            nippou.NJitsudou = timeContainer.NJitsudou;
            nippou.NShinya = timeContainer.NShinya;
            // みなし対象外用残業
            nippou.TotalZangyo = timeContainer.TotalZangyo;
            nippou.KaisyaCode = (NippousCompanyCode)syain.KaisyaCode;
            nippou.IsRendouZumi = false;
            nippou.RendouYmd = null;
            nippou.TourokuKubun = tourokuKubun;
            nippou.KakuteiYmd = null;
            nippou.SyukkinKubunId1 = syukkinKbnIdPair.Item1;
            nippou.SyukkinKubunId2 = syukkinKbnIdPair.Item2;
            _db.SetOriginalValue(nippou, e => e.Version, vm.Version);
        }

        /// <summary>
        /// 日報⇔案件を登録又は更新する
        /// </summary>
        /// <param name="nippou">日報</param>
        /// <param name="vm">画面入力内容</param>
        /// <returns></returns>
        /// <exception cref="DbUpdateConcurrencyException">更新対象の日報⇔案件が存在しない場合</exception>
        public async Task InsertOrUpdateOrDeleteNippouAnken(Nippou nippou, NippouInputViewModel vm)
        {
            // 更新対象の日報案件
            var nippouAnkens = await _queryService.FetchNippouAnkensForUpdate(nippou.Id);

            // 日報案件の登録・更新・削除処理
            foreach (var item in vm.JissekiInputs)
            {
                // Id（日報案件のId）に値がある場合は、更新 or 削除
                if (item.Id.HasValue)
                {
                    // 存在チェック
                    if (!nippouAnkens.Any(na => na.Id == item.Id))
                    {
                        // 存在するはずが、存在しない
                        throw new DbUpdateConcurrencyException(string.Format(Const.ErrorNotExists, "日報案件", item.Id));
                    }

                    var nippouAnken = nippouAnkens.FirstOrDefault(na => na.Id == item.Id);

                    // 削除
                    if (item.IsDelete)
                    {
                        DeleteNippouAnken(nippouAnken!, item);
                        continue;
                    }

                    // 更新
                    UpdateNippouAnken(nippouAnken!, item);
                    continue;
                }

                // 登録
                InsertNippouAnken(nippou, item);
            }
        }

        /// <summary>
        /// 日報⇔案件を登録する（複数登録）
        /// </summary>
        /// <param name="nippou">登録時の日報</param>
        /// <param name="vm">画面入力内容</param>
        public void InsertNippouAnkenList(Nippou nippou, NippouInputViewModel vm)
        {
            foreach (var item in vm.JissekiInputs)
            {
                InsertNippouAnken(nippou, item);
            }
        }

        /// <summary>
        /// 日報⇔案件を登録する
        /// </summary>
        /// <param name="nippou">登録時の日報</param>
        /// <param name="vm">画面入力内容</param>
        public void InsertNippouAnken(Nippou nippou, JissekiInputViewModel vm)
        {
            var newNippouAnken = new NippouAnken
            {
                NippouId = nippou.Id,
                AnkensId = vm.AnkensId!.Value,
                KokyakuName = vm.KokyakuName!,
                AnkenName = vm.AnkenName!,
                JissekiJikan = vm.JissekiJikan!.Value,
                KokyakuKaisyaId = vm.KokyakuKaisyaId!.Value,
                BumonProcessId = vm.BumonProcessId,
                IsLinked = vm.IsLinked,
            };
            _db.NippouAnkens.Add(newNippouAnken);
        }

        /// <summary>
        /// 日報⇔案件を更新する
        /// </summary>
        /// <param name="nippou">更新対象の日報案件</param>
        /// <param name="vm">画面入力内容</param>
        public void UpdateNippouAnken(NippouAnken nippouAnken, JissekiInputViewModel vm)
        {
            // 更新
            nippouAnken.AnkensId = vm.AnkensId!.Value;
            nippouAnken.KokyakuName = vm.KokyakuName!;
            nippouAnken.AnkenName = vm.AnkenName!;
            nippouAnken.JissekiJikan = vm.JissekiJikan!.Value;
            nippouAnken.KokyakuKaisyaId = vm.KokyakuKaisyaId!.Value;
            nippouAnken.BumonProcessId = vm.BumonProcessId;
            nippouAnken.IsLinked = vm.IsLinked;
            _db.SetOriginalValue(nippouAnken, e => e.Version, vm.Version);
        }

        /// <summary>
        /// 日報⇔案件を削除する
        /// </summary>
        /// <param name="nippou">削除対象の日報案件</param>
        /// <param name="vm">画面入力内容</param>
        public void DeleteNippouAnken(NippouAnken nippouAnken, JissekiInputViewModel vm)
        {
            _db.SetOriginalValue(nippouAnken!, e => e.Version, vm.Version);
            _db.NippouAnkens.Remove(nippouAnken!);
        }

        /// <summary>
        /// 残業時間の合計が制限時間を超過するかどうかを判定する。
        ///
        /// 勤怠属性に設定されている制限時間（分）を上限とし、
        /// 実績日前日までの残業時間に実績日当日の残業時間を加算した
        /// 累計残業時間が制限時間を超えた場合に true を返す。
        ///
        /// 制限時間が 0 の場合は、制限なしとして false を返す。
        /// </summary>
        /// <param name="syain">対象社員</param>
        /// <param name="vm">画面入力内容</param>
        /// <returns>
        /// 時間外労働制限を超えている場合場合は true、
        /// 上記場合は false
        /// </returns>
        public async Task<bool> IsOverZangyoSeigenJikanAsync(Syain syain, NippouInputViewModel vm)
        {
            decimal CheckZangyoTargetTime(KintaiZokusei kintaiZokusei)
            {
                return (kintaiZokusei.IsOvertimeLimit3m ? vm.Total3MonthZangyoTotal : vm.RuisekiJikangai) + vm.TotalZangyo;
            }

            // 勤怠属性
            var kintaiZokusei = syain.KintaiZokusei;

            // 残業制限時間
            var limitTime = kintaiZokusei.SeigenTime * 60;

            // 残業時間合計
            var zangyoTime = CheckZangyoTargetTime(kintaiZokusei);

            return (limitTime != 0 && limitTime < zangyoTime);
        }

        /// <summary>
        /// 3ヶ月残業時間合計
        /// 3ヶ月分（2ヶ月前の月初～前日）の日報からみなし対象外用残業の合計を計算する。
        /// 月初～月末の残業時間がマイナスの場合、その月の残業時間は 0 として扱う。
        /// 月末までのデータがない場合は、その月の合計がマイナスであってもマイナスのまま扱う。
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="date">日付</param>
        /// <returns>3ヶ月残業時間合計</returns>
        public async Task<decimal> Calc3MonthZangyoJikanAsync(long syainId, DateOnly date)
        {
            // 2ヶ月前の月初～前日までの日報を取得
            var threeMonthNippous = await _queryService.FetchThreeMonthNippousAsync(syainId, date);

            // 月ごとにグループ化してみなし対象外用残業を合計
            var monthlyTotals = threeMonthNippous
                .GroupBy(r => new { r.NippouYmd.Year, r.NippouYmd.Month })
                .Select(g =>
                {
                    var monthlyTotal = g.Sum(r => r.TotalZangyo) ?? 0m;

                    DateOnly maxDateInMonth = g.Max(r => r.NippouYmd);
                    DateOnly endOfMonth = maxDateInMonth.GetEndOfMonth();
                    bool isMonthFinish = endOfMonth == maxDateInMonth;
                    return (monthlyTotal < 0 && isMonthFinish)
                        ? 0
                        : monthlyTotal;
                });
            return monthlyTotals.Sum();
        }

        /// <summary>
        /// 1ヶ月残業時間の合計を取得する
        /// 月初～前日までの日報からみなし対象外用残業の合計を計算する。
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="date">日付</param>
        /// <returns>1ヶ月残業時間の合計</returns>
        public async Task<decimal> CalcThisMonthZangyoJikanAsync(long syainId, DateOnly date)
        {
            var thisMonthNippous = await _queryService.FetchThisMonthNippousAsync(syainId, date);

            return thisMonthNippous.Sum(r => r.TotalZangyo) ?? 0;
        }
    }
}