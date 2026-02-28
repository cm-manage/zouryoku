using CommonLibrary.Extensions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Enums;
using Model.Model;
using ZouryokuCommonLibrary.Utils;
using static Model.Enums.AttendanceClassification;

namespace Zouryoku.Pages.JissekiNyuryoku
{
    public class JissekiNyuryokuFurikyuuYuukyuuService(ZouContext db, AppSettings appSettings)
    {
        private readonly ZouContext _db = db;
        private readonly JissekiNyuryokuQueryService _queryService = new(db);
        private readonly JissekiNyuryokuCommonService _commonService = new(db);
        private readonly AppSettings _appSettings = appSettings;

        /// <summary>
        /// 振替休暇と有給休暇の消化の取り消し
        /// </summary>
        /// <param name="nippouSyain">対象社員マスタ</param>
        /// <param name="vm">画面入力内容</param>
        /// <param name="dayType">DayType</param>
        /// <returns></returns>
        public async Task CancelFurikyuuAndYuukyuuSyoukaAsync(Syain nippouSyain, NippouInputViewModel vm)
        {

            // 出勤区分1＝休日出勤 & 出勤区分2＝未選択
            if (vm.SyukkinKubun1 == 休日出勤 && vm.SyukkinKubun2 == None)
            {
                var dayType = await _commonService.GetDayTypeAsync(vm.JissekiDate);

                // 土日祝祭日
                // 日曜 ＆ 勤怠属性が管理
                if (dayType == DayType.土曜祝祭日 ||
                    (nippouSyain.KintaiZokusei.Code == EmployeeWorkType.管理 && dayType == DayType.日曜))
                {
                    // 休日出勤日の振替休暇残を削除
                    await DeleteFurikyuuZanAsync(nippouSyain.Id, vm.JissekiDate);
                }
            }

            // 出勤区分1＝年次有給休暇（1日） & 出勤区分2＝未選択
            // 出勤区分2＝年次有給休暇（1日）
            if ((vm.SyukkinKubun1 == 年次有給休暇_1日 && vm.SyukkinKubun2 == None) ||
                vm.SyukkinKubun2 == 年次有給休暇_1日)
            {
                // 1日有給休暇の消化を取り消す
                await CancelOneDayPaidLeaveAsync(nippouSyain.SyainBaseId);
            }

            // 出勤区分1＝32：計画有給休暇
            if (vm.SyukkinKubun1 == 計画有給休暇)
            {
                // 計画有給休暇の消化を取り消す
                await CancelOneDayPlannedPaidLeaveAsync(nippouSyain.SyainBaseId);
            }

            // 出勤区分1＝半日有給 or 出勤区分2＝半日有給 の場合
            if (vm.SyukkinKubun1 == 半日有給 || vm.SyukkinKubun2 == 半日有給)
            {
                // 半日有給の消化を取り消す
                await CancelHalfDayPaidLeaveAsync(nippouSyain.SyainBaseId, vm.SyukkinKubun1);
            }

            //出勤区分1＝計画特別休暇
            if (vm.SyukkinKubun1 == 計画特別休暇)
            {
                // 計画特別休暇の消化を取り消す
                await CancelPlannedSpecialDayAsync(nippouSyain.SyainBaseId);
            }

            // 出勤区分1＝振替休暇 & 出勤区分2＝未選択
            if (vm.SyukkinKubun1 == 振替休暇 && vm.SyukkinKubun2 == None)
            {
                // 振替休暇の消化を取り消す
                await CancelFurikyuuAsync(nippouSyain.Id, vm.JissekiDate);
            }

            // 出勤区分1＝半日振休 or 出勤区分2＝半日振休
            if (vm.SyukkinKubun1 == 半日振休 || vm.SyukkinKubun2 == 半日振休)
            {
                await CancelHalfFurikyuuAsync(nippouSyain.Id, vm.JissekiDate);
            }
        }

        /// <summary>
        /// 振替休暇と有給休暇の消化処理を行う
        /// </summary>
        /// <param name="nippouSyain">対象社員マスタ</param>
        /// <param name="vm">画面入力内容</param>
        /// <returns></returns>
        public async Task TakeFurikyuuAndYuukyuuSyoukaAsync(Syain nippouSyain, NippouInputViewModel vm)
        {
            // 出勤区分１＝09：振替休暇 ＆　出勤区分２＝00：未選択 の場合
            if (vm.SyukkinKubun1 == 振替休暇 && vm.SyukkinKubun2 == None)
            {
                await TakeFurikyuuAsync(nippouSyain.Id, vm.JissekiDate);
            }

            // 出勤区分1＝10：半日振替休暇　or　出勤区分2＝10：半日振替休暇　の場合
            if (vm.SyukkinKubun1 == 半日振休 || vm.SyukkinKubun2 == 半日振休)
            {
                await TakeHalfFurikyuuAsync(nippouSyain.Id, vm.JissekiDate);
            }

            // 出勤区分1＝07：年次有給休暇（1日）　or　出勤区分2＝07：年次有給休暇（1日）　の場合
            if (vm.SyukkinKubun1 == 年次有給休暇_1日 || vm.SyukkinKubun2 == 年次有給休暇_1日)
            {
                await TakePaidLeaveAsync(nippouSyain.SyainBaseId, Time.ichinichi);
            }

            // 出勤区分1＝32：計画有給休暇の場合
            if (vm.SyukkinKubun1 == 計画有給休暇)
            {
                await TakePlannedPaidLeaveAsync(nippouSyain.SyainBaseId, Time.ichinichi);
            }

            // 出勤区分1＝08：半日有給休暇　or　出勤区分2＝08：半日有給休暇　の場合
            if (vm.SyukkinKubun1 == 半日有給 || vm.SyukkinKubun2 == 半日有給)
            {
                await TakePaidLeaveAsync(nippouSyain.SyainBaseId, Time.hanniti);
            }

            // 出勤区分1＝33：計画特別休暇の場合
            if (vm.SyukkinKubun1 == 計画特別休暇)
            {
                await TakePlannedSpecialLeaveAsync(nippouSyain.SyainBaseId);
            }

            var dayType = await _commonService.GetDayTypeAsync(vm.JissekiDate);

            // 出勤区分1＝06：休日出勤　＆　パラメータ.実績年月日が土曜祝祭日　の場合
            if (vm.SyukkinKubun1 == 休日出勤 && dayType == DayType.土曜祝祭日)
            {
                var isAdd = await CreateNewFurikyuuZanAsync(nippouSyain.Id, vm.JissekiDate, vm.JitsudouTime, vm.FurikyuYoteiDate);
                // 振休残を登録した場合、振休残の通知処理
                if (isAdd)
                {
                    await SendCompensatoryLeaveNotificationIfNeededAsync(nippouSyain, vm.JissekiDate);
                }
            }

            // 勤怠属性.コード＝4：管理　＆　画面.出勤区分1＝06：休日出勤　＆　INパラメータ.実績年月日が日曜　の場合、休日出勤時の処理を行う
            if (nippouSyain.KintaiZokusei.Code == EmployeeWorkType.管理 && vm.SyukkinKubun1 == 休日出勤 && dayType == DayType.日曜)
            {
                var isAdd = await CreateNewFurikyuuZanAsync(nippouSyain.Id, vm.JissekiDate, vm.JitsudouTime, vm.FurikyuYoteiDate);
                // 振休残を登録した場合、振休残の通知処理
                if (isAdd)
                {
                    await SendCompensatoryLeaveNotificationIfNeededAsync(nippouSyain, vm.JissekiDate);
                }
            }
        }

        /// <summary>
        /// 1日振替休暇の消化を行う
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns></returns>
        public async Task TakeFurikyuuAsync(long syainId, DateOnly jissekiDate)
        {
            var furikyuu = await _queryService.FetchEffectiveFurikyuuZanForUpdateAsync(syainId, jissekiDate);

            if (furikyuu == null)
                return;

            // 1日振休が未取得の場合
            if (furikyuu.IsOneDay == true && furikyuu.SyutokuState == LeaveBalanceFetchStatus.未)
            {
                furikyuu.SyutokuYmd1 = jissekiDate;
                furikyuu.SyutokuState = LeaveBalanceFetchStatus._1日;
                return;
            }

            // 上記以外、半日振替を2回消化して1日振替休暇を消化する
            await TakeHalfFurikyuuAsync(syainId, jissekiDate);
            await TakeHalfFurikyuuAsync(syainId, jissekiDate);
        }

        // 半日振替休暇の取得を行う。
        public async Task TakeHalfFurikyuuAsync(long syainId, DateOnly jissekiDate)
        {
            var furikyuu = await _queryService.FetchEffectiveFurikyuuZanForUpdateAsync(syainId, jissekiDate);

            if (furikyuu == null) return;

            // 1日振休が半日取得済み
            if (furikyuu.IsOneDay && furikyuu.SyutokuState == LeaveBalanceFetchStatus.半日)
            {
                furikyuu.SyutokuYmd2 = jissekiDate;
                furikyuu.SyutokuState = LeaveBalanceFetchStatus._1日;
                return;
            }

            // 1日振休が未取得
            if (furikyuu.IsOneDay && furikyuu.SyutokuState == LeaveBalanceFetchStatus.未)
            {
                furikyuu.SyutokuYmd1 = jissekiDate;
                furikyuu.SyutokuState = LeaveBalanceFetchStatus.半日;
                return;
            }

            // 半日振休が未取得
            if (!furikyuu.IsOneDay && furikyuu.SyutokuState == LeaveBalanceFetchStatus.未)
            {
                furikyuu.SyutokuYmd1 = jissekiDate;
                furikyuu.SyutokuState = LeaveBalanceFetchStatus._1日;
                return;
            }
        }

        /// <summary>
        /// 1日有給休暇の消化を行う
        /// </summary>
        /// <param name="syainBaseId">社員BaseId</param>
        /// <param name="daysToTake">消化する有給休暇の日数</param>
        /// <returns></returns>
        public async Task TakePaidLeaveAsync(long syainBaseId, decimal daysToTake)
        {
            var yuukyuuZan = await _queryService.FetchYuukyuuZanForUpdateAsync(syainBaseId);

            if (yuukyuuZan == null) return;

            yuukyuuZan.Syouka += daysToTake;
        }

        /// <summary>
        /// 計画有給休暇の消化を行う
        /// </summary>
        /// <param name="syainBaseId">社員BaseId</param>
        /// <param name="daysToTake">消化する計画有給休暇の日数</param>
        /// <returns></returns>
        public async Task TakePlannedPaidLeaveAsync(long syainBaseId, decimal daysToTake)
        {
            var yuukyuuZan = await _queryService.FetchYuukyuuZanForUpdateAsync(syainBaseId);

            if (yuukyuuZan == null) return;

            yuukyuuZan.KeikakuYukyuSu += (short)daysToTake;
        }

        // 計画特別休暇の取得を行う。
        public async Task TakePlannedSpecialLeaveAsync(long syainBaseId)
        {
            var yuukyuuZan = await _queryService.FetchYuukyuuZanForUpdateAsync(syainBaseId);

            if (yuukyuuZan == null) return;

            yuukyuuZan.KeikakuTokukyuSu += 1;
        }

        /// <summary>
        /// 振替休暇残を登録する
        /// 既に同じ休日出勤日の振替休暇残が登録済みの場合は、登録しない
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <param name="jitsudouTime">実働時間</param>
        /// <param name="furikyuYoteiDate">振替休暇取得予定日</param>
        /// <returns>true:登録した場合、false:既に登録済みの場合</returns>
        public async Task<bool> CreateNewFurikyuuZanAsync(long syainId, DateOnly jissekiDate, int jitsudouTime, DateOnly? furikyuYoteiDate)
        {
            var hasHurikyuu = await _queryService.HasFurikyuuZanByKyuujitsuSyukkinDate(syainId, jissekiDate);

            // 登録済みの場合は登録しない
            if (hasHurikyuu)
                return false;

            bool isOneDay = Time.kyuujistuHalfBorder < jitsudouTime;

            FurikyuuZan data = new()
            {
                SyainId = syainId,
                KyuujitsuSyukkinYmd = jissekiDate,
                DaikyuuKigenYmd = jissekiDate.AddMonths(6).GetEndOfMonth(),
                IsOneDay = isOneDay,
                SyutokuState = LeaveBalanceFetchStatus.未,
                SyutokuYoteiYmd = furikyuYoteiDate,
            };
            _db.FurikyuuZans.Add(data);

            return true;
        }

        /// <summary>
        /// 振替休暇残の合計日数が閾値の日数を超えた場合、部門長にチャット送信するデータを登録する
        /// </summary>
        /// <param name="nippouSyain">日報の社員</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns></returns>
        public async Task SendCompensatoryLeaveNotificationIfNeededAsync(Syain nippouSyain, DateOnly jissekiDate)
        {
            var furikyuuZan = await _queryService.FetchEffectiveFurikyuuZanAllAsync(nippouSyain.Id, jissekiDate);
            var totalZan = furikyuuZan.Sum(x =>
                                {
                                    if (x.IsOneDay == true && x.SyutokuState == LeaveBalanceFetchStatus.未) return Time.ichinichi;
                                    if (x.IsOneDay == true && x.SyutokuState == LeaveBalanceFetchStatus.半日) return Time.hanniti;
                                    if (x.IsOneDay == false && x.SyutokuState == LeaveBalanceFetchStatus.未) return Time.hanniti;
                                    else return 0m;
                                });

            if (totalZan < _appSettings.FurikyuAlertThresholdDays)
                return;

            var bumoncho = await _db.Syains
                .Include(s => s.SyainBase)
                .Where(sb => sb.Id == nippouSyain.Busyo.BusyoBase.BumoncyoId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (bumoncho == null) return;

            string content = $"{nippouSyain.Name}さんの振替休暇残日数が{totalZan}日に増加しました。\n振替休暇の取得促進をお願いします。";

            var messageContent = new MessageContent
            {
                SyainId = bumoncho.Id,
                Content = content,
                FunctionType = FunctionalClassification.有給未取得アラート
            };

            _db.MessageContents.Add(messageContent);
        }

        /// <summary>
        /// 確定時に登録した振替休暇残を削除する
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns></returns>
        public async Task DeleteFurikyuuZanAsync(long syainId, DateOnly jissekiDate)
        {
            var furikyuuZan = await _queryService.FetchFurikyuZanOfKyuujitsuSyukkinForUpdateAsync(syainId, jissekiDate);

            if (furikyuuZan == null)
                return;

            _db.FurikyuuZans.Remove(furikyuuZan);
        }

        /// <summary>
        /// 1日有給休暇の消化を取り消す
        /// </summary>
        /// <param name="syainBaseId">社員BaseId</param>
        /// <returns></returns>
        public async Task CancelOneDayPaidLeaveAsync(long syainBaseId)
        {
            var yuukyuuZan = await _queryService.FetchYuukyuuZanForUpdateAsync(syainBaseId);

            if (yuukyuuZan == null)
                return;

            yuukyuuZan.Syouka -= 1;

        }

        /// <summary>
        /// 計画有給休暇の消化を取り消す
        /// </summary>
        /// <param name="syainBaseId">社員BaseId</param>
        /// <returns></returns>
        public async Task CancelOneDayPlannedPaidLeaveAsync(long syainBaseId)
        {
            var yuukyuuZan = await _queryService.FetchYuukyuuZanForUpdateAsync(syainBaseId);

            if (yuukyuuZan == null)
                return;

            yuukyuuZan.Syouka -= 1;
            yuukyuuZan.KeikakuYukyuSu -= 1;
        }

        /// <summary>
        /// 半日有給の消化を取り消す
        /// </summary>
        /// <param name="syainBaseId">社員BaseId</param>
        /// <param name="sukkinKubun1">出勤区分1</param>
        /// <returns></returns>
        public async Task CancelHalfDayPaidLeaveAsync(long syainBaseId, AttendanceClassification sukkinKubun1)
        {
            var yuukyuuZan = await _queryService.FetchYuukyuuZanForUpdateAsync(syainBaseId);

            if (yuukyuuZan == null)
                return;

            yuukyuuZan.Syouka -= 0.5m;

            // 出勤区分1が半日勤務の場合
            if (sukkinKubun1 == AttendanceClassification.半日勤務)
            {
                yuukyuuZan.HannitiKaisuu -= 1;
            }
        }

        /// <summary>
        /// 計画特別休暇の消化を取り消す
        /// </summary>
        /// <param name="syainBaseId">社員BaseId</param>
        /// <returns></returns>
        public async Task CancelPlannedSpecialDayAsync(long syainBaseId)
        {
            var yuukyuuZan = await _queryService.FetchYuukyuuZanForUpdateAsync(syainBaseId);

            if (yuukyuuZan == null)
                return;

            yuukyuuZan.KeikakuTokukyuSu -= 1;
        }

        /// <summary>
        /// 振替休暇の消化を取り消す
        /// </summary>
        /// <param name="syainId">社員Id</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns></returns>
        public async Task CancelFurikyuuAsync(long syainId, DateOnly jissekiDate)
        {
            var furikyuu = await _queryService.FetchFurikyuuZanBySyutokuYmdForUpdateAsync(syainId, jissekiDate);

            if (furikyuu == null)
                return;

            var isOneDay = furikyuu.IsOneDay;
            var syutokuState = furikyuu.SyutokuState;

            furikyuu.SyutokuYmd1 = null;
            furikyuu.SyutokuState = LeaveBalanceFetchStatus.未;

            if ((isOneDay && syutokuState == LeaveBalanceFetchStatus.半日) ||
                (!isOneDay && syutokuState == LeaveBalanceFetchStatus._1日))
            {
                await CancelHalfFurikyuuAsync(syainId, jissekiDate);
            }
        }

        /// <summary>
        /// 半日振替休暇の消化を取り消す
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <returns></returns>
        public async Task CancelHalfFurikyuuAsync(long syainId, DateOnly jissekiDate)
        {
            var furikyuu = await _queryService.FetchFurikyuuZanBySyutokuYmd1or2ForUpdateAsync(syainId, jissekiDate);

            if (furikyuu == null)
                return;

            if (furikyuu.SyutokuYmd1 == jissekiDate)
            {
                furikyuu.SyutokuYmd1 = null;
                furikyuu.SyutokuState = LeaveBalanceFetchStatus.未;

            }
            else if (furikyuu.SyutokuYmd2 == jissekiDate)
            {
                furikyuu.SyutokuYmd2 = null;
                furikyuu.SyutokuState = LeaveBalanceFetchStatus.半日;
            }
        }
    }
}
