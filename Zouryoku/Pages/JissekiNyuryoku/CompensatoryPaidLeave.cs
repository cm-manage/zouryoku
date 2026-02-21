using CommonLibrary.Extensions;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Enums;
using Model.Model;
using static Zouryoku.Pages.JissekiNyuryoku.IndexModel;
using static Model.Enums.AttendanceClassification;
using static Model.Enums.LeaveBalanceFetchStatus;

namespace Zouryoku.Pages.JissekiNyuryoku
{
    public class CompensatoryPaidLeave
    {

        // ---------------------------------------------
        // 定数
        // ---------------------------------------------
        const decimal THRESHOLD_DAYS = 10m;

        private readonly ZouContext _context;
        private readonly DateOnly JissekiDate;
        private readonly bool IsWorkDay;
        private readonly long SyainId;
        private readonly int RealWorkingMinute;
        private readonly DateOnly? FurikyuYoteiDate;
        private readonly AppConfig _appConfig;
        private AttendanceClassification SyukkinKubun1 { get; set; }
        private AttendanceClassification SyukkinKubun2 { get; set; }


        // Constructor
        public CompensatoryPaidLeave(DateOnly jissekiDate, long syainId,
            NippouViewModel nippouData,
            bool isWorkDay,
            ZouContext context, DateOnly? furiYotei, AppConfig appConfig)
        {
            JissekiDate = jissekiDate;
            SyainId = syainId;
            _context = context;
            IsWorkDay = isWorkDay;
            _appConfig = appConfig;
            RealWorkingMinute = nippouData.TotalWorkingHoursInMinute;
            FurikyuYoteiDate = furiYotei;
            _appConfig = appConfig;
            SyukkinKubun1 = nippouData.SyukkinKubun1;
            SyukkinKubun2 = nippouData.SyukkinKubun2;
        }

        // 確定を行う際に更新される休暇
        public async Task UpdateConfirmLeaveAsync()
        {
            Syain? syainInfo = await _context.Syains.AsNoTracking().FirstOrDefaultAsync(row => row.Id == SyainId);
            if (syainInfo == null) return;

            // 出勤区分１＝09：振替休暇 ＆　出勤区分２＝00：未選択 の場合
            if (SyukkinKubun1 == 振替休暇 &&
                SyukkinKubun2 == None)
            {
                await TakeFurikyuuAsync(SyainId, JissekiDate);
            }

            // 出勤区分1＝10：半日振替休暇　or　出勤区分2＝10：半日振替休暇　の場合
            if (SyukkinKubun1 == 半日振休 ||
                SyukkinKubun2 == 半日振休)
            {
                await TakeHalfFurikyuuAsync(SyainId, JissekiDate);
            }

            // 出勤区分1＝07：年次有給休暇（1日）　or　出勤区分2＝07：年次有給休暇（1日）　の場合
            if (SyukkinKubun1 == 年次有給休暇_1日 ||
                SyukkinKubun2 == 年次有給休暇_1日)
            {
                await TakePaidLeaveAsync(syainInfo.SyainBaseId);
            }

            // 出勤区分1＝32：計画有給休暇の場合
            if (SyukkinKubun1 == 計画有給休暇)
            {
                await TakePlannedPaidLeaveAsync(syainInfo.SyainBaseId);
            }

            // 出勤区分1＝08：半日有給休暇　or　出勤区分2＝08：半日有給休暇　の場合
            if(SyukkinKubun1 == 半日有給 ||
                SyukkinKubun2 == 半日有給)
            {
                await TakeHalfPlannedPaidLeaveAsync(syainInfo.SyainBaseId);
            }

            // 出勤区分1＝33：計画特別休暇の場合
            if (SyukkinKubun1 == 計画特別休暇)
            {
                await TakePlannedSpecialLeaveAsync(syainInfo.SyainBaseId);
            }

            // 出勤区分1＝06：休日出勤　＆　パラメータ.実績年月日が土曜祝祭日　の場合
            if (SyukkinKubun1 == 休日出勤)
            {
                if (!IsWorkDay && JissekiDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    await CreateNewFurikyuuAsync(SyainId, JissekiDate, RealWorkingMinute, FurikyuYoteiDate);
                    await SendCompensatoryLeaveNotificationIfNeededAsync(SyainId, JissekiDate);
                }
                else if (JissekiDate.DayOfWeek == DayOfWeek.Sunday && syainInfo.KintaiZokuseiId == (int)EmployeeWorkType.管理)
                {
                    await CreateNewFurikyuuAsync(SyainId, JissekiDate, RealWorkingMinute, FurikyuYoteiDate);
                    await SendCompensatoryLeaveNotificationIfNeededAsync(SyainId, JissekiDate);
                }
            }
            
        }


        public async Task UpdateCancelConfirmLeaveAsync(long syainBaseId, DateOnly jissekiDate)
        {
            var syainInfo = await _context.Syains.AsNoTracking()
                .Where(row => row.SyainBaseId == syainBaseId && 
                row.StartYmd < jissekiDate && jissekiDate < row.EndYmd)
                .Include(row => row.KintaiZokusei).FirstOrDefaultAsync();
            if (syainInfo == null) return;


            //出勤区分1＝06：休日出勤　＆　出勤区分2＝00：未選択 の場合
            if (SyukkinKubun1 == 休日出勤 && 
                SyukkinKubun2 == None)
            {
              
                if (!IsWorkDay && jissekiDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    await DeleteFurikyuuZanAsync(syainInfo.Id, jissekiDate);
                } else if (jissekiDate.DayOfWeek == DayOfWeek.Sunday && syainInfo.KintaiZokuseiId == (int) EmployeeWorkType.管理)
                {
                    await DeleteFurikyuuZanAsync(syainInfo.Id, jissekiDate);
                }
            }

            // 出勤区分1＝07：年次有給休暇（1日）　＆　出勤区分2＝00：未選択 の場合
            if (SyukkinKubun1 == 年次有給休暇_1日 &&
                SyukkinKubun2 == None)
            {
                await CancelOneDayPaidLeaveAsync(syainInfo.SyainBaseId);
            } 
            // 出勤区分2＝07：年次有給休暇（１日）　の場合
            else if (SyukkinKubun2 == 年次有給休暇_1日)
            {
                await CancelOneDayPaidLeaveAsync(syainInfo.SyainBaseId);
            }


            // 出勤区分1＝32：計画有給休暇　の場合、
            if (SyukkinKubun1 == 計画有給休暇)
            {
                await CancelOneDayPlannedPaidLeaveAsync(syainInfo.SyainBaseId);
            }


            // 出勤区分1＝08：半日有給 or 画面.出勤区分2＝08：半日有給 の場合
            if (SyukkinKubun1 == 半日有給 ||
                SyukkinKubun2 == 半日有給)
            {
                await CancelHalfDayPaidLeaveAsync(syainInfo.SyainBaseId, SyukkinKubun1);
            }


            //出勤区分1＝33：計画特別休暇 の場合
            if (SyukkinKubun1 == 計画特別休暇)
            {
                await CancelPlannedSpecialDayAsync(syainInfo.SyainBaseId);
            }


            // 出勤区分1＝09：振替休暇 ＆　画面.出勤区分2＝00：未選択 の場合
            if (SyukkinKubun1 == 振替休暇 &&
                SyukkinKubun2 == None)
            {
                await CancelFurikyuuAsync(syainInfo.Id, jissekiDate);
            }


            // 出勤区分1＝10：半日振休　or　画面.出勤区分2＝10：半日振休　の場合
            if (SyukkinKubun1 == 半日振休 ||
                SyukkinKubun2 == 半日振休)
            {
                
                await CancelHalfFurikyuuAsync(syainInfo.Id, jissekiDate);
            }
        }


        // 振替休暇の取得を行う。
        private async Task TakeFurikyuuAsync(long syainId, DateOnly jissekiDate)
        {
            var furikyuu = await _context.FurikyuuZans.Where(x =>
            x.SyainId == syainId &&
            x.DaikyuuKigenYmd >= jissekiDate &&
            (
                (x.IsOneDay == true && (x.SyutokuState == 未 || x.SyutokuState == 半日))
                || (x.IsOneDay == false && x.SyutokuState == 未)
            )).FirstOrDefaultAsync();

            if (furikyuu == null) return;

            if (furikyuu.IsOneDay == true && furikyuu.SyutokuState == 未)
            {
                furikyuu.SyutokuYmd1 = jissekiDate;
                furikyuu.SyutokuState = _1日;
                await _context.SaveChangesAsync();
            }
            else if (furikyuu.IsOneDay == true && furikyuu.SyutokuState == 半日)
            {
                furikyuu.SyutokuYmd2 = jissekiDate;
                furikyuu.SyutokuState = _1日;
                await _context.SaveChangesAsync();
                await TakeHalfFurikyuuAsync(syainId, jissekiDate);
            }
            else if (furikyuu.IsOneDay == true && furikyuu.SyutokuState == 未)
            {
                furikyuu.SyutokuYmd1 = jissekiDate;
                furikyuu.SyutokuState = 半日;
                await _context.SaveChangesAsync();
                await TakeHalfFurikyuuAsync(syainId, jissekiDate);
            }
            else if (furikyuu.IsOneDay == false && furikyuu.SyutokuState == 未)
            {
                furikyuu.SyutokuYmd1 = jissekiDate;
                furikyuu.SyutokuState = _1日;
                await _context.SaveChangesAsync();
                await TakeHalfFurikyuuAsync(syainId, jissekiDate);
            }
        }


        // 半日振替休暇の取得を行う。
        private async Task TakeHalfFurikyuuAsync(long syainId, DateOnly jissekiDate)
        {
            var furikyuu = await _context.FurikyuuZans.Where(x =>
            x.SyainId == syainId &&
            x.DaikyuuKigenYmd >= jissekiDate &&
            (
                (x.IsOneDay == true && (x.SyutokuState == 未 || x.SyutokuState == 半日))
                || (x.IsOneDay == false && x.SyutokuState == 未)
            )).FirstOrDefaultAsync();
            if (furikyuu == null) return;
            if (furikyuu.IsOneDay == true && furikyuu.SyutokuState == 未)
            {
                furikyuu.SyutokuYmd1 = jissekiDate;
                furikyuu.SyutokuState = 半日;
                await _context.SaveChangesAsync();
            }
            else if (furikyuu.IsOneDay == true && furikyuu.SyutokuState == 半日)
            {
                furikyuu.SyutokuYmd2 = jissekiDate;
                furikyuu.SyutokuState = _1日;
                await _context.SaveChangesAsync();
            } 
            else if (furikyuu.IsOneDay == false && furikyuu.SyutokuState == 未)
            {
                furikyuu.SyutokuYmd1 = jissekiDate;
                furikyuu.SyutokuState = _1日;
                await _context.SaveChangesAsync();
            }
        }


        // 1日有給休暇の取得を行う。
        private async Task TakePaidLeaveAsync(long syainBaseId)
        {
            var yukyuuZan = await _context.YuukyuuZans
                .FirstOrDefaultAsync(row => row.SyainBaseId == syainBaseId);
            if (yukyuuZan == null) return;

            yukyuuZan.Syouka += 1;
            await _context.SaveChangesAsync();
        }


        // 計画有給休暇の取得を行う。
        private async Task TakePlannedPaidLeaveAsync(long syainBaseId)
        {
            var yukyuuZan = await _context.YuukyuuZans
                .FirstOrDefaultAsync(row => row.SyainBaseId == syainBaseId);
            if (yukyuuZan == null) return;
            yukyuuZan.KeikakuYukyuSu += 1;
            await _context.SaveChangesAsync();
        }


        // 半日有給休暇の取得を行う。
        private async Task TakeHalfPlannedPaidLeaveAsync(long syainBaseId)
        {
            var yukyuuZan = await _context.YuukyuuZans
                .FirstOrDefaultAsync(row => row.SyainBaseId == syainBaseId);
            if (yukyuuZan == null) return;
            yukyuuZan.Syouka += 0.5m;
            yukyuuZan.HannitiKaisuu += 1;
            await _context.SaveChangesAsync();
        }


        // 計画特別休暇の取得を行う。
        private async Task TakePlannedSpecialLeaveAsync(long syainBaseId)
        {
            var yuukyuuZan = await _context.YuukyuuZans
                .FirstOrDefaultAsync(row => row.SyainBaseId == syainBaseId);
            if (yuukyuuZan == null) return;
            yuukyuuZan.KeikakuTokukyuSu += 1;

            await _context.SaveChangesAsync();
        }


        // 新規振替休暇残を登録する。
        public async Task CreateNewFurikyuuAsync(long syainId, DateOnly jissekiDate, int workingHour, DateOnly? furikyuYoteiDate)
        {
            bool isItOneDay = workingHour > 240;
            FurikyuuZan data = new FurikyuuZan
            {
                SyainId = syainId,
                KyuujitsuSyukkinYmd = jissekiDate,
                DaikyuuKigenYmd = jissekiDate.AddMonths(6).GetEndOfMonth(),
                SyutokuState = 未,
                IsOneDay = isItOneDay,
                SyutokuYoteiYmd = furikyuYoteiDate ?? null,
            };
            await _context.FurikyuuZans.AddAsync(data);
            await _context.SaveChangesAsync();
        }


        // 確定時に登録した振替休暇残を削除する。
        private async Task DeleteFurikyuuZanAsync(long syainId, DateOnly jissekiDate)
        {
            var leave = await _context.FurikyuuZans
                .FirstOrDefaultAsync(row => row.SyainId == syainId 
                && row.KyuujitsuSyukkinYmd == jissekiDate);

            if (leave == null) return;
            
            _context.FurikyuuZans.Remove(leave);
            await _context.SaveChangesAsync();
            
        }


        // 1日有給休暇の確定を取り消す。
        private async Task CancelOneDayPaidLeaveAsync(long syainBaseId)
        {
            var yukyuuZan = await _context.YuukyuuZans
                .FirstOrDefaultAsync(row => row.SyainBaseId == syainBaseId);

            if (yukyuuZan == null) return;


            yukyuuZan.Syouka -= 1;
            
            await _context.SaveChangesAsync();
        }


        // 計画有給休暇の確定を取り消す。
        private async Task CancelOneDayPlannedPaidLeaveAsync(long syainBaseId)
        {
            var yukyuuZan = await _context.YuukyuuZans
                .FirstOrDefaultAsync(row => row.SyainBaseId == syainBaseId);
            if (yukyuuZan == null) return;
         
            yukyuuZan.Syouka -= 1;
            yukyuuZan.KeikakuYukyuSu -= 1;

            await _context.SaveChangesAsync();
        }


        // 半日有給の確定を取り消す。
        private async Task CancelHalfDayPaidLeaveAsync(long syainBaseId, AttendanceClassification kubun1)
        {
            var yuukyuuZan = await _context.YuukyuuZans
                .FirstOrDefaultAsync(row => row.SyainBaseId == syainBaseId);
            if (yuukyuuZan == null) return;
            yuukyuuZan.Syouka -= 0.5m;
            if(kubun1 == AttendanceClassification.半日勤務)
            {
                yuukyuuZan.HannitiKaisuu -= 1;
            }

            await _context.SaveChangesAsync();

        }


        // 計画特別休暇の確定を取り消す。
        private async Task CancelPlannedSpecialDayAsync(long syainBaseId)
        {
            var yuukyuuZan = await _context.YuukyuuZans
                .FirstOrDefaultAsync(row => row.SyainBaseId == syainBaseId);
            if (yuukyuuZan == null) return;

            yuukyuuZan.KeikakuTokukyuSu -= 1;

            await _context.SaveChangesAsync();
        }


        // 振替休暇の確定を取り消す。
        private async Task CancelFurikyuuAsync(long syainId, DateOnly jissekiDate)
        {
            var furikyuu = await _context.FurikyuuZans
                .Where(row => row.SyainId == syainId && row.SyutokuYmd1 == jissekiDate)
                .OrderBy(row => row.KyuujitsuSyukkinYmd).FirstOrDefaultAsync();
            if(furikyuu == null) return;

            bool needSecondHalf = (furikyuu.IsOneDay == true && furikyuu.SyutokuState == 半日)
                || (furikyuu.IsOneDay == false && furikyuu.SyutokuState == _1日);

            furikyuu.SyutokuState = 未;
            furikyuu.SyutokuYmd1 = null;

            await _context.SaveChangesAsync();

            if (needSecondHalf)
            {
                var secondFurikyuu = await _context.FurikyuuZans
                    .Where(row => row.SyainId == syainId && row.Id != furikyuu.Id)
                    .Where(row => row.SyutokuYmd1 == jissekiDate || row.SyutokuYmd2 == jissekiDate)
                    .OrderBy(row => row.KyuujitsuSyukkinYmd)
                    .FirstOrDefaultAsync();
                if(secondFurikyuu == null) return;

                if(secondFurikyuu.SyutokuYmd1 == jissekiDate)
                {
                    secondFurikyuu.SyutokuYmd1 = null;
                    secondFurikyuu.SyutokuState = 未;

                } else if (secondFurikyuu.SyutokuYmd2 == jissekiDate)
                {
                    secondFurikyuu.SyutokuYmd2 = null;
                    secondFurikyuu.SyutokuState = 半日;
                }

                await _context.SaveChangesAsync();
            }
        }


        // 半日振替休暇の取り消しを行う。
        private async Task CancelHalfFurikyuuAsync(long syainId, DateOnly jissekiDate)
        {
            var furikyuu = await _context.FurikyuuZans
                   .Where(row => row.SyainId == syainId)
                   .Where(row => row.SyutokuYmd1 == jissekiDate || row.SyutokuYmd2 == jissekiDate)
                   .OrderBy(row => row.KyuujitsuSyukkinYmd)
                   .FirstOrDefaultAsync();
            if (furikyuu == null) return;

            if (furikyuu.SyutokuYmd1 == jissekiDate)
            {
                furikyuu.SyutokuYmd1 = null;
                furikyuu.SyutokuState = 未;

            }
            else if (furikyuu.SyutokuYmd2 == jissekiDate)
            {
                furikyuu.SyutokuYmd2 = null;
                furikyuu.SyutokuState = 半日;
            }

            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// 振替休暇残の合計日数が10日以上の場合、部門長にメール通知を送信する
        /// </summary>
        /// <param name="syainId">社員ID</param>
        private async Task SendCompensatoryLeaveNotificationIfNeededAsync(long syainId, DateOnly jissekiDate)
        {
            var syain = await _context.Syains
                .Include(s => s.SyainBase)
                .Include(s => s.Busyo)
                    .ThenInclude(b => b.BusyoBase)
                        .ThenInclude(bb => bb.Bumoncyo)
                            .ThenInclude(sb => sb.Syains.Where(s => s.StartYmd <= DateOnly.FromDateTime(DateTime.Today) && s.EndYmd > DateOnly.FromDateTime(DateTime.Today)))
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == syainId);

            if (syain == null) return;

            var furikyuu = await _context.FurikyuuZans.AsNoTracking().Where(x =>
            x.SyainId == syainId &&
            x.DaikyuuKigenYmd >= jissekiDate &&
            (
                (x.IsOneDay == true && (x.SyutokuState == 未 || x.SyutokuState == 半日))
                || (x.IsOneDay == false && x.SyutokuState == 未)
            )).ToListAsync();
            var totalDays = furikyuu.Sum(x =>
                {
                    if (x.IsOneDay == true && x.SyutokuState == 未) return 1m;
                    if (x.IsOneDay == true && x.SyutokuState == 半日) return 0.5m;
                    if (x.IsOneDay == false && x.SyutokuState == 未) return 0.5m;
                    else return 0m;
                }
            );

            if (totalDays < THRESHOLD_DAYS) return;

            var bumoncho = syain.Busyo?.BusyoBase?.Bumoncyo?.Syains.FirstOrDefault();
            if (bumoncho == null || string.IsNullOrEmpty(bumoncho.EMail)) return;

            string body = $"{syain.Name}さんの振替休暇残日数が{totalDays}日に増加しました。\n振替休暇の取得促進をお願いします。";

            var message = new MessageContent
            {
                SyainId = bumoncho.Id,
                Content = body,
                SendDatetime = null,
                FunctionType = FunctionalClassification.有給未取得アラート
            };

            await _context.MessageContents.AddAsync(message);
            await _context.SaveChangesAsync();

        }
    }
}


