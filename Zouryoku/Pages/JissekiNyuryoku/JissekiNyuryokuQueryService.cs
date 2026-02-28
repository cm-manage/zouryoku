using CommonLibrary.Extensions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Enums;
using Model.Model;

namespace Zouryoku.Pages.JissekiNyuryoku
{
    public class JissekiNyuryokuQueryService(ZouContext db)
    {
        private readonly ZouContext _db = db;

        /// <summary>
        /// Application Configを取得する
        /// </summary>
        /// <returns>Application Config</returns>
        public async Task<ApplicationConfig?> FetchApplicatationConfig() =>
            await _db.ApplicationConfigs.FirstOrDefaultAsync();

        /// <summary>
        /// 社員BaseIdで有給休暇残を取得するクエリ
        /// </summary>
        /// <param name="syainBaseId">社員BaseId</param>
        /// <returns>有給休暇残を取得するクエリ</returns>
        public IQueryable<YuukyuuZan> QueryYuukyuuZanBySyainBaseId(long syainBaseId) =>
            _db.YuukyuuZans
                .Where(sb => sb.SyainBaseId == syainBaseId);

        /// <summary>
        /// 取得期限が切れていない & 未取得 & 休日出勤日が最も古い振替休暇残を取得するクエリ
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns>振替休暇残のクエリ</returns>
        public IQueryable<FurikyuuZan> QueryEffectiveFurikyuuZan(long syainId, DateOnly jissekiDate) =>
            _db.FurikyuuZans
                .Where(x => x.SyainId == syainId && jissekiDate <= x.DaikyuuKigenYmd &&
                ((x.IsOneDay == true && (x.SyutokuState == LeaveBalanceFetchStatus.未 || x.SyutokuState == LeaveBalanceFetchStatus.半日))
                || (x.IsOneDay == false && x.SyutokuState == LeaveBalanceFetchStatus.未)
                ))
                .OrderBy(x => x.KyuujitsuSyukkinYmd);

        /// <summary>
        /// 指定した社員・実績日に対して有効な
        /// 「時間外労働時間制限拡張」の伺い申請ヘッダを取得するクエリ。
        ///
        /// 作業日が実績日の月初～実績日に含まれており、
        /// かつ無効化されていない申請のうち、
        /// 申請明細に「時間外労働時間制限拡張」が含まれるものを対象とする。
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日（判定基準日）</param>
        /// <returns>
        /// 条件に合致する時間外労働制限拡張の伺い申請ヘッダを取得するクエリ
        /// </returns>
        public IQueryable<UkagaiHeader> QueryJikangaiKakuchoSinsei(long syainId, DateOnly jissekiDate) =>
            _db.UkagaiHeaders
                .Include(u => u.UkagaiShinseis)
                .Where(row =>
                    row.SyainId == syainId &&
                    !row.Invalid &&
                    jissekiDate.GetStartOfMonth() <= row.WorkYmd &&
                    row.WorkYmd <= jissekiDate &&
                    row.UkagaiShinseis.Any(u =>
                        u.UkagaiSyubetsu == InquiryType.時間外労働時間制限拡張
                    )
                );

        /// <summary>
        /// 更新対象の日報案件を取得する（更新用）
        /// </summary>
        /// <param name="nippouId">日報のID</param>
        /// <returns>更新対象の日報案件</returns>
        public async Task<List<NippouAnken>> FetchNippouAnkensForUpdate(long nippouId) =>
            await _db.NippouAnkens
                .Where(na => na.NippouId == nippouId)
                .ToListAsync();

        /// <summary>
        /// 時間外労働制限時間拡張の伺いヘッダを取得する（更新用）
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns></returns>
        public async Task<UkagaiHeader?> FetchUkagaiHeaderOfJikangaiSeigenKakuchoForUpdate(long syainId, DateOnly jissekiDate) =>
            await _db.UkagaiHeaders
                .Where(uh => jissekiDate.GetStartOfMonth() < uh.WorkYmd && uh.WorkYmd < jissekiDate.GetEndOfMonth())
                .Where(uh => uh.SyainId == syainId && uh.Invalid == true && uh.UkagaiShinseis.Any(s => s.UkagaiSyubetsu == InquiryType.時間外労働時間制限拡張))
                .FirstOrDefaultAsync();

        /// <summary>
        /// 更新対象の日報を取得する（更新用）
        /// </summary>
        /// <param name="id">日報のId</param>
        /// <returns>更新対象の日報</returns>
        public async Task<Nippou?> FetchNippouForUpdateAsync(long id) =>
            await _db.Nippous
                .Where(n => n.Id == id)
                .SingleOrDefaultAsync();

        /// <summary>
        /// 振替休暇残を取得する（更新処理用）
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns>振替休暇残</returns>
        public async Task<FurikyuuZan?> FetchFurikyuZanOfKyuujitsuSyukkinForUpdateAsync(long syainId, DateOnly jissekiDate) =>
            await _db.FurikyuuZans
                .FirstOrDefaultAsync(fz => fz.SyainId == syainId && fz.KyuujitsuSyukkinYmd == jissekiDate);

        /// <summary>
        /// 指定した取得年月日の振替休暇残を取得する（更新用）
        /// 複数ある場合は、休日出勤日が古い日付のものを取得する
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績年月日</param>
        /// <returns>振替休暇残</returns>
        public async Task<FurikyuuZan?> FetchFurikyuuZanBySyutokuYmdForUpdateAsync(long syainId, DateOnly jissekiDate) =>
            await _db.FurikyuuZans
                .Where(row => row.SyainId == syainId && row.SyutokuYmd1 == jissekiDate)
                .OrderBy(row => row.KyuujitsuSyukkinYmd)
                .FirstOrDefaultAsync();

        /// <summary>
        /// 指定した取得年月日が取得予定日1又は取得予定日2に一致する振替休暇残を取得する（更新用）
        /// 複数ある場合は、休日出勤日が古い日付のものを取得する
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns></returns>
        public async Task<FurikyuuZan?> FetchFurikyuuZanBySyutokuYmd1or2ForUpdateAsync(long syainId, DateOnly jissekiDate) =>
            await _db.FurikyuuZans
                .Where(row => row.SyainId == syainId && (row.SyutokuYmd1 == jissekiDate || row.SyutokuYmd2 == jissekiDate))
                .OrderBy(row => row.KyuujitsuSyukkinYmd)
                .FirstOrDefaultAsync();

        /// <summary>
        /// 取得期限が切れていない & 未取得 & 休日出勤日が最も古い振替休暇残を取得する
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns>振替休暇残</returns>
        public async Task<FurikyuuZan?> FetchEffectiveFurikyuuZanForUpdateAsync(long syainId, DateOnly jissekiDate) =>
            await QueryEffectiveFurikyuuZan(syainId, jissekiDate)
                .FirstOrDefaultAsync();

        /// <summary>
        /// 有給残日数を取得する（更新用）
        /// </summary>
        /// <param name="syainBaseId">社員BaseId</param>
        /// <returns>有給残日数</returns>
        public async Task<YuukyuuZan?> FetchYuukyuuZanForUpdateAsync(long syainBaseId) =>
            await QueryYuukyuuZanBySyainBaseId(syainBaseId)
                .FirstOrDefaultAsync();

        // 有給残日数を取得する
        private async Task<YuukyuuZan?> FetchYuukyuuZanDataAsync(long syainBaseId) =>
            await FirstOrDefaultNoTrackingAsync(_db.YuukyuuZans.Where(row => row.SyainBaseId == syainBaseId));

        /// <summary>
        /// 代理入力履歴を取得する（更新用）
        /// </summary>
        /// <param name="nippouId">日報Id</param>
        /// <returns>代理入力履歴</returns>
        public async Task<DairiNyuryokuRireki?> FetchDairiNyuuryokuRirekiForUpdate(long nippouId) =>
            await _db.DairiNyuryokuRirekis
                .Where(row => row.NippouId == nippouId)
                .FirstOrDefaultAsync();

        /// <summary>
        /// 指定した社員・実績日に対して有効な
        /// 「時間外労働時間制限拡張」の伺い申請ヘッダを取得する。（更新用）
        ///
        /// 作業日が実績日の月初～実績日に含まれており、
        /// かつ無効化されていない申請のうち、
        /// 申請明細に「時間外労働時間制限拡張」が含まれるものを対象とする。
        ///
        /// 該当する申請が存在しない場合は null を返す。
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日（判定基準日）</param>
        /// <returns>
        /// 条件に合致する時間外労働制限拡張の伺い申請ヘッダ。
        /// </returns>
        public async Task<UkagaiHeader?> FetchJikangaiKakuchoSinseiForUpdateAsync(long syainId, DateOnly jissekiDate) =>
            await QueryJikangaiKakuchoSinsei(syainId, jissekiDate)
                .FirstOrDefaultAsync();






        /// <summary>
        /// 日報を取得する
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns>日報</returns>
        public async Task<Nippou?> FetchNippouDataAsync(long syainId, DateOnly jissekiDate) => 
            await FirstOrDefaultNoTrackingAsync(
                _db.Nippous
                .Where(n => n.SyainId == syainId && n.NippouYmd == jissekiDate)
                .Include(n => n.SyukkinKubunId1Navigation)
                .Include(n => n.SyukkinKubunId2Navigation)
                .AsSplitQuery()
                );

        /// <summary>
        /// 実績日付より後の日付で確定済の日報があるか
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns>true:実績日付より後の日付で確定済み日報がある場合、false:そうでない場合</returns>
        public async Task<bool> HasKakuteizumiNippouAferDate(long syainId, DateOnly jissekiDate) =>
            await _db.Nippous
                    .AnyAsync(n => n.SyainId == syainId && jissekiDate < n.NippouYmd && n.TourokuKubun == DailyReportStatusClassification.確定保存);

        /// <summary>
        /// 日報⇔案件を取得する
        /// </summary>
        /// <param name="nippouId">日報のId</param>
        /// <returns>日報⇔案件</returns>
        public async Task<List<NippouAnken>> FetchNippouAnkensAsync(long nippouId) => 
            await _db.NippouAnkens
                    .Where(row => row.NippouId == nippouId)
                    .Include(row => row.Ankens)
                        .ThenInclude(row => row.KingsJuchu)
                    .OrderBy(row => row.Id)
                    .AsNoTracking()
                    .ToListAsync();

        /// <summary>
        /// 案件を取得する
        /// </summary>
        /// <param name="ankenId">案件ID</param>
        /// <returns>案件</returns>
        public async Task<Anken?> FetchAnkenById(long ankenId) =>
            await _db.Ankens
                    .Where(a => a.Id == ankenId)
                    .Include(a => a.KingsJuchu)
                    .SingleOrDefaultAsync();

        /// <summary>
        /// 振替休暇残を社員IDと実績年月日より取得する
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns>振替休暇残</returns>
        public async Task<FurikyuuZan?> FetchFurikyuZanBySyainIdAndDate(long syainId, DateOnly jissekiDate) =>
            await FirstOrDefaultNoTrackingAsync(
                _db.FurikyuuZans
                .Where(row => row.SyainId == syainId && row.KyuujitsuSyukkinYmd == jissekiDate)
                );

        /// <summary>
        /// 出勤区分をコード順で全て取得する
        /// </summary>
        /// <returns>出勤区分</returns>
        public async Task<List<SyukkinKubun>> FetchSyukkinKubnAll() =>
            await _db.SyukkinKubuns
                .OrderBy(row => row.CodeString)
                .AsNoTracking()
                .ToListAsync();

        /// <summary>
        /// 列挙体定数値の出勤区分から出勤区分を取得する
        /// </summary>
        /// <param name="syukkinKubun">出勤区分（列挙体定数）</param>
        /// <returns>出勤区分</returns>
        public async Task<SyukkinKubun?> FetchSyukkinKubunByCodeAsync(AttendanceClassification syukkinKubun) =>
            await FirstOrDefaultNoTrackingAsync(
                _db.SyukkinKubuns
                .Where(row => row.CodeString == ((int)syukkinKubun).ToString("D2"))
                );

        /// <summary>
        /// 有給残日数を取得する
        /// </summary>
        /// <param name="syainBaseId">社員BaseId</param>
        /// <returns>有給残日数</returns>
        public async Task<YuukyuuZan?> FetchYuukyuuZanAsync(long syainBaseId) =>
            await QueryYuukyuuZanBySyainBaseId(syainBaseId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

        /// <summary>
        /// 休日出勤日に一致する振替休暇残を取得する
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="kyuujitsuSyukkinDate">休日出勤日</param>
        /// <returns></returns>
        public async Task<bool> HasFurikyuuZanByKyuujitsuSyukkinDate(long syainId, DateOnly kyuujitsuSyukkinDate) =>
            await
                _db.FurikyuuZans
                .AnyAsync
                (fz => fz.SyainId == syainId && fz.KyuujitsuSyukkinYmd == kyuujitsuSyukkinDate);

        /// <summary>
        /// 取得期限が切れていない & 未取得 の振替休暇残を取得する
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns>振替休暇残</returns>
        public async Task<List<FurikyuuZan>> FetchEffectiveFurikyuuZanAllAsync(long syainId, DateOnly jissekiDate) =>
            await QueryEffectiveFurikyuuZan(syainId, jissekiDate).AsNoTracking().ToListAsync();


        /// <summary>
        /// 実績入力の対象社員マスタを取得する
        /// </summary>
        /// <param name="syainBaseId">社員BaseId</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns>実績入力の対象社員マスタ</returns>
        public async Task<Syain?> FetchNippouSyainAsync(long syainBaseId, DateOnly jissekiDate) =>
            await FirstOrDefaultNoTrackingAsync(
                _db.Syains
                .Where(row => row.SyainBaseId == syainBaseId && row.StartYmd <= jissekiDate && jissekiDate <= row.EndYmd)
                .Include(s => s.Busyo)
                    .ThenInclude(b => b.BusyoBase)
                .Include(row => row.KintaiZokusei)
                .AsSplitQuery()
                );

        /// <summary>
        /// 勤怠打刻取得する
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns></returns>
        public async Task<List<WorkingHour>> FetchWorkingHoursListAsync(long syainId, DateOnly jissekiDate) =>
            await _db.WorkingHours
                .Where(wh => wh.SyainId == syainId && wh.Hiduke == jissekiDate && wh.Deleted == false)
                .OrderBy(wh => wh.SyukkinTime ?? wh.TaikinTime)
                .AsNoTracking()
                .ToListAsync();

        /// <summary>
        /// 伺いヘッダーと伺い申請を取得する
        /// </summary>
        /// <param name="syainId"></param>
        /// <param name="jissekiDate"></param>
        /// <returns></returns>
        public async Task<List<UkagaiHeader>> FetchUkagaiHeadersAsync(long syainId, DateOnly jissekiDate) =>
            await _db.UkagaiHeaders
                .AsNoTracking()
                .Where(uh =>
                        uh.SyainId == syainId &&
                        uh.WorkYmd == jissekiDate &&
                        uh.Invalid == false)
                .Include(uh => uh.UkagaiShinseis)
                .OrderByDescending(uh => uh.ShinseiYmd)
                .ThenByDescending(uh => uh.Id)
                .ToListAsync();

        // 汎用の DB ヘルパーを追加（AsNoTracking を付与して使い回す）
        // ※再利用可能なクエリの取得を簡潔にするため
        private static Task<T?> FirstOrDefaultNoTrackingAsync<T>(IQueryable<T> query) where T : class
            => query.AsNoTracking().FirstOrDefaultAsync();

        /// <summary>
        /// 対象日付の非稼働日を取得する
        /// </summary>
        /// <param name="date">対象日付</param>
        /// <returns>非稼働日</returns>
        public async Task<Hikadoubi?> FetchHikadoubiDataAsync(DateOnly date) =>
            await FirstOrDefaultNoTrackingAsync(_db.Hikadoubis.Where(row => row.Ymd == date));

        /// <summary>
        /// 指定した実績日を基準に、直近3か月分の日報を取得する。
        /// ※ 対象期間は「2か月前の月初 ～ 実績日前日まで」
        /// </summary>        
        /// <returns>日報データ</returns>
        public async Task<List<Nippou>> FetchThreeMonthNippousAsync(long syainId, DateOnly jissekiDate) =>
            await _db.Nippous
                .Where(n => n.SyainId == syainId &&
                            jissekiDate.AddMonths(-2).GetStartOfMonth() <= n.NippouYmd && n.NippouYmd <= jissekiDate.AddDays(-1))
                .AsNoTracking()
                .ToListAsync();

        /// <summary>
        /// 指定した実績日を基準に、当月分の日報を取得する。
        /// ※ 対象期間は「月初 ～ 実績日前日まで」
        /// </summary>
        /// <returns>日報データ</returns>
        public async Task<List<Nippou>> FetchThisMonthNippousAsync(long syainId, DateOnly jissekiDate) =>
            await _db.Nippous
                    .Where(n => n.SyainId == syainId &&
                                jissekiDate.GetStartOfMonth() <= n.NippouYmd && n.NippouYmd <= jissekiDate.AddDays(-1))
                    .AsNoTracking()
                    .ToListAsync();

        /// <summary>
        /// 指定した社員・実績日に対して有効な
        /// 「時間外労働時間制限拡張」の伺い申請ヘッダを取得する。
        ///
        /// 作業日が実績日の月初～実績日に含まれており、
        /// かつ無効化されていない申請のうち、
        /// 申請明細に「時間外労働時間制限拡張」が含まれるものを対象とする。
        ///
        /// 該当する申請が存在しない場合は null を返す。
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日（判定基準日）</param>
        /// <returns>
        /// 条件に合致する時間外労働制限拡張の伺い申請ヘッダ。
        /// </returns>
        public async Task<UkagaiHeader?> FetchJikangaiKakuchoSinseiAsync(long syainId, DateOnly jissekiDate) =>
            await QueryJikangaiKakuchoSinsei(syainId, jissekiDate)
                .AsNoTracking()
                .FirstOrDefaultAsync();
    }

}
