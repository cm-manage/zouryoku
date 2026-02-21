using CommonLibrary.Extensions;
using EnumsNET;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Model.Data;
using Model.Enums;
using Model.Model;
using ZouryokuCommonLibrary.Utils;
using static Zouryoku.Pages.JissekiNyuryoku.IndexModel;
using static Model.Enums.ApprovalStatus;
using static Model.Enums.AttendanceClassification;
using static Model.Enums.InquiryType;
using static Model.Enums.EmployeeWorkType;
using static Model.Enums.EmployeeAuthority;

namespace Zouryoku.Pages.JissekiNyuryoku
{
    public class ConfirmValidation
    {
        // ---------------------------------------------
        // 定数
        // ---------------------------------------------

        private const string SelectAttendanceClassification = "出勤区分を選択してください。";
        private const string HolidayOnWeekdayError = "平日に『休日』を選択する事は出来ません。";
        private const string HolidayWorkOnWeekdayError = "平日に『休日出勤』を選択する事は出来ません。";
        private const string SelectHolidayOnWeekend = "休日は『休日』もしくは『休日出勤』を選択してください。";
        private const string InvalidAttendanceClassification = "出勤区分が不正です。";
        private const string EnterSubstituteHolidayDate = "振替休暇予定日を入力してください。";
        private const string CannotSelectHolidayWithClockIn = "出退勤の打刻があるため、休日を選択する事は出来ません。";
        private const string SelectAnnualPaidLeaveOneDay = "「年次有給休暇(1日)」を選択してください。";
        private const string SelectHalfDayWorkDueToShortHours = "実働時間が4時間以下ですので『半日勤務』を選択してください。";
        private const string CannotUsePartTimeWork = "出勤区分『パート勤務』を使用する事は出来ません。";
        private const string SelectNormalWork = "『通常勤務』を選択してください。";
        private const string CannotTakePhysiologicalLeave = "生理休暇を取得することは出来ません。";
        private const string AnnualHalfDayPaidLeaveLimit = "半日有給休暇を取得できるのは年間10回までです。";
        private const string PaidLeaveInfoNotRegistered = "有給情報が登録されていません。";
        private const string CannotTakeSubstituteHoliday = "振替休暇を取得する事は出来ません。";
        private const string CannotTakeHalfDaySubstituteHoliday = "半日振休を取得する事は出来ません。";
        private const string TakeSubstituteHolidayFirst = "振替休暇から先に取得してください。";
        private const string CannotTakeHalfDayPaidLeave = "半日有給休暇を取得する事が出来ません。";
        private const string AbsenceWithSubstituteHolidayAvailable = "『欠勤』が選択されていますが半日振休が取得可能です。";
        private const string CannotTakeAnnualPaidLeave = "有給休暇を取得する事は出来ません。";
        private const string PlannedAnnualPaidLeaveLimit = "計画有給休暇の年間取得回数は5回までです。";
        private const string PlannedSpecialLeaveLimit = "計画特別休暇の年間取得回数は2回までです。";
        private const string EnterWorkPerformance = "実績を入力してください。";
        private const string MaxFiveProjectCodes = "工番を5つ以上選択する事は出来ません。";
        private const string SelectProjectCode = "工番を選択してください。";
        private const string CreateProjectInfoForHolidayWork = "休日出勤の場合は工番情報を作成してください。";
        private const string CreateProjectInfoForNormalWork = "勤務の場合は工番情報を作成してください。";
        private const string HolidayWorkShortHoursProjectLimit = "休日出勤4時間以下の場合は工番を2つ以上選択する事はできません。";
        private const string CannotRegisterFutureWorkPerformance = "本日より未来の勤務日報を登録する事は出来ません。";
        private const string HalfDayWorkProjectLimit = "半日勤務時に工番を3つ以上選択する事は出来ません。";
        private const string CannotSelectProjectDuringLeave = "休暇時に工番を選択する事は出来ません。";
        private const string CannotConfirmSupportGroupOrder = "支援グループの受注番号では確定処理出来ません。";
        private const string SelectedProjectCodeCannotBeUsed = "選択された工番は使用出来ません。";
        private const string OvertimeLimitExceeded = "時間外労働時間の制限をこれ以上超過することは出来ません。";
        private const string OvertimeLimitUnapproved = "　時間外労働時間の上限を超えており、指示入力が提出されていない、または、認められていません。";
        private const string NotWorkingCannotSelectFormat = "出勤していないため、{0}を選択することはできません。";
        private const string PaidLeaveDataNotFoundFormat = "社員コード{0}の有給残日数(YuukyuuZans)テーブルデータがありません。";

        private ModelStateDictionary ModelState { get; set; }
        private readonly ZouContext _context;

        private readonly Syain SyainInfo;
        private readonly DateOnly JissekiDate;

        private readonly bool IsWorkDay;
        private readonly DateOnly? Furiyotei;

        private readonly NippouViewModel NippouData;
        private readonly AttendanceClassification SyukkinKubun1;
        private readonly AttendanceClassification SyukkinKubun2;

        
        // Constructor
        public ConfirmValidation(Syain syainInfo, DateOnly  jissekiDate, NippouViewModel nippouData, bool isWorkDay, DateOnly? furiYotei, ZouContext context, ModelStateDictionary modelState)
        {
            _context = context;
            ModelState = modelState;

            SyainInfo = syainInfo;
            JissekiDate = jissekiDate;

            IsWorkDay = isWorkDay;
            Furiyotei = furiYotei;

            NippouData = nippouData;
            SyukkinKubun1 = nippouData.SyukkinKubun1;
            SyukkinKubun2 = nippouData.SyukkinKubun2;

        }


        // 確定、一時保存、両方に共通する　Validation
        private async Task CommonValidationAsync(SyukkinKubun? kubun1Info)
        {
            // 3 画面.出勤区分1＝00：未選択
            if (SyukkinKubun1 == AttendanceClassification.None)
            {
                ModelState.AddModelError(string.Empty, SelectAttendanceClassification);
            }

            // 4 稼働日の場合
            if (IsWorkDay)
            {
                // 4-1 画面.出勤区分1＝01：休日
                if (SyukkinKubun1 == 休日)
                {
                    ModelState.AddModelError(string.Empty, HolidayOnWeekdayError);
                }
                else if (SyukkinKubun1 == AttendanceClassification.休日出勤)
                {
                    // 4-2 上記以外、画面.出勤区分１＝06：AttendanceClassification.休日出勤
                    ModelState.AddModelError(string.Empty, HolidayWorkOnWeekdayError);
                }
            }

            // 5 非稼働日の場合
            if (!IsWorkDay)
            {
                if (SyukkinKubun1 != 休日 && SyukkinKubun1 != AttendanceClassification.休日出勤)
                {
                    // 5-1 画面.出勤区分１≠（06：AttendanceClassification.休日出勤・01：休日）
                    ModelState.AddModelError(string.Empty, SelectHolidayOnWeekend);
                }
                else if (SyukkinKubun2 != AttendanceClassification.None)
                {
                    // 5-2 上記以外　＆　画面.出勤区分２≠00：未選択
                    ModelState.AddModelError(string.Empty, InvalidAttendanceClassification);
                }
                else if (SyukkinKubun1 == AttendanceClassification.休日出勤 && Furiyotei == null)
                {
                    // 5-3 上記以外　＆　振替休暇予定日が未入力の場合
                    ModelState.AddModelError(string.Empty, EnterSubstituteHolidayDate);
                }
            }

            if (kubun1Info != null
                && (kubun1Info.IsSyukkin != true && kubun1Info.IsVacation == true) || (kubun1Info?.IsSyukkin != true && kubun1Info?.IsVacation != true))
            {
                if (SyukkinKubun2 != AttendanceClassification.None)
                {
                    // 6-1 画面.出勤区分２≠00：未選択
                    ModelState.AddModelError(string.Empty, InvalidAttendanceClassification);
                }
                else if (SyukkinKubun1 == 休日 && SyukkinKubun2 == AttendanceClassification.None
                    && HasClockInTime())
                {
                    // 6-2 上記以外　＆　画面.出勤区分１＝01：休日　＆　画面.出勤区分２＝00：未選択　＆　画面.出退勤時間1の出勤時間≠ブランク
                    ModelState.AddModelError(string.Empty, CannotSelectHolidayWithClockIn);
                }
            }
            //else if (SyukkinKubun1 == 半日勤務 
            //    || SyukkinKubun1 == 半日勤務午前
            //    || SyukkinKubun1 == 半日勤務午後)
            // Not found 2 other
            else if (SyukkinKubun1 == 半日勤務)
            {
                // 7
                if (SyukkinKubun2 != AttendanceClassification.None)
                {
                    // 7-1 画面.出勤区分２≠00：未選択
                    ModelState.AddModelError(string.Empty, InvalidAttendanceClassification);
                }
            }
            else if (SyukkinKubun1 == 半日振休 || SyukkinKubun1 == 半日有給)
            {
                // 8
                if (SyukkinKubun2 == AttendanceClassification.None)
                {
                    // 8-1 画面.出勤区分2＝00：未選択
                    ModelState.AddModelError(string.Empty, InvalidAttendanceClassification);
                }
                if (SyukkinKubun1 == 半日有給 && SyukkinKubun2 == 半日有給)
                {
                    // 8-2 画面.出勤区分1と画面.出勤区分2共に08：半日有給
                    ModelState.AddModelError(string.Empty, SelectAnnualPaidLeaveOneDay);
                }
            }
            else if (kubun1Info != null && 
                !((kubun1Info.IsSyukkin != true && kubun1Info.IsVacation == true) 
                || (kubun1Info?.IsSyukkin != true && kubun1Info?.IsVacation != true)) && !(kubun1Info?.IsSyukkin == true && kubun1Info?.IsVacation == true))
            {
                // 9
                if (SyainInfo.KintaiZokusei.Code != パート && SyukkinKubun2 != AttendanceClassification.None)
                {
                    // 9-1  勤怠属性.コード≠6：パート　＆　画面.出勤区分2≠00：未選択
                    ModelState.AddModelError(string.Empty, InvalidAttendanceClassification);
                }
            }

            if (!CheckWorkingTimes())
            {
                // 10 勤務時間がない
                if (SyukkinKubun1 == 通常勤務 || SyukkinKubun1 == AttendanceClassification.休日出勤 ||
                   SyukkinKubun1 == 半日勤務 || SyukkinKubun1 == パート勤務)
                {
                    ModelState.AddModelError(string.Empty, string.Format(NotWorkingCannotSelectFormat, SyukkinKubun1.GetName()));
                }
            }

            if (SyukkinKubun1 == 通常勤務 && GetTotalWorkTimeMinute() != 0 && GetTotalWorkTimeMinute() <= 240)
            {
                // 11 通常勤務を選択時
                ModelState.AddModelError(string.Empty, SelectHalfDayWorkDueToShortHours);
            }

            if (SyukkinKubun1 == パート勤務 && SyainInfo.KintaiZokuseiId != (int)パート)
            {
                // 12 パートを選択時
                ModelState.AddModelError(string.Empty, CannotUsePartTimeWork);
            }

            if (SyukkinKubun1 == 半日勤務 && GetTotalWorkTimeMinute() >= 480)
            {
                // 13 半日勤務を選択時
                ModelState.AddModelError(string.Empty, SelectNormalWork);
            }

            
        }

        // 一時保存の Validation
        public async Task TemporarySaveValidationAsync()
        {
            var kubun1Info = await FetchSyukkinKubunDataAsync(NippouData.SyukkinKubunCodeString1);
            var remainingCompensatoryLeave = await GetRemainingNumberOfCompensatoryLeaveAsync(SyainInfo.Id, JissekiDate);

            await CommonValidationAsync(kubun1Info);

            if ((SyukkinKubun1 == 生理休暇 || SyukkinKubun2 == 生理休暇) && SyainInfo.Seibetsu != '2')
            {
                // 15 生理休暇を選択時
                ModelState.AddModelError(string.Empty, CannotTakePhysiologicalLeave);
            }

            // 1日有給休暇を選択時
            // 20 - 3 上記以外　＆　画面.出勤区分2＝08：有給休暇　＆　画面.出勤区分1＝08：半日有給
            if (SyukkinKubun2 == 年次有給休暇_1日 &&
                SyukkinKubun1 == 半日有給)
            {
                ModelState.AddModelError(string.Empty, InvalidAttendanceClassification);
            }
            

            // 計画特別休暇を選択時
            if (SyukkinKubun1 == 計画特別休暇 && SyukkinKubun2 == AttendanceClassification.None)
            {
                ModelState.AddModelError(string.Empty, InvalidAttendanceClassification);
            }

        }


        // 確定の Validation
        public async Task FinalConfirmValidationAsync(List<NippouAnkenViewModel> nippouAnkens, decimal totalOvertime)
        {
            var kubun1Info = await FetchSyukkinKubunDataAsync(NippouData.SyukkinKubunCodeString1);
            var yukyuuZan = await FetchYuukyuuZanDataAsync(SyainInfo.SyainBaseId);
            var remainingCompensatoryLeave = await GetRemainingNumberOfCompensatoryLeaveAsync(SyainInfo.Id, JissekiDate);
            var remainingPaidLeave = await GetRemainNumberOfPaidVacationAsync(SyainInfo.SyainBaseId);
            await CommonValidationAsync(kubun1Info);

            // 14
            if (SyukkinKubun1 == 半日有給 ||
                (SyukkinKubun1 == 半日勤務 && SyukkinKubun2 == 半日有給))
            {
                if (yukyuuZan?.HannitiKaisuu >= 10)
                {
                    // 14-1 有給休暇残.半日回数＞＝10
                    ModelState.AddModelError(string.Empty, AnnualHalfDayPaidLeaveLimit);
                } else if (yukyuuZan == null)
                {
                    // 14-2 有給休暇残が取得できない
                    ModelState.AddModelError(string.Empty, PaidLeaveInfoNotRegistered);
                }
            }

            //15 生理休暇を選択時
            if ((SyukkinKubun1 == 生理休暇 || SyukkinKubun2 == 生理休暇) && SyainInfo.Seibetsu != '2')
            {
                ModelState.AddModelError(string.Empty, CannotTakePhysiologicalLeave);
            }

            //16 振替休暇を選択時
            if (SyukkinKubun1 == 振替休暇 && SyukkinKubun2 == AttendanceClassification.None)
            {
                if (remainingCompensatoryLeave <= 0.5m)
                {
                    // 16-1 振替休暇の残日数＜＝0.5日
                    ModelState.AddModelError(string.Empty, CannotTakeSubstituteHoliday);
                }
            }

            // 17 半日振替休暇を選択時
            if (SyukkinKubun1 == 半日振休 || SyukkinKubun2 == 半日振休)
            {
                if (remainingCompensatoryLeave <= 0)
                {
                    // 17-1　振替休暇の残日数＜＝0日
                    ModelState.AddModelError(string.Empty, CannotTakeHalfDaySubstituteHoliday);
                }
            }

            // 18 半日有給休暇を選択時
            if (SyukkinKubun1 == 半日有給 || SyukkinKubun2 == 半日有給)
            {
                // 18-1　画面.出勤区分1又は画面出勤区分2のいずれか一方＝10：半日振休　＆　振替休暇の残日数-0.5＞０
                if ((SyukkinKubun1 == 半日振休 || SyukkinKubun2 == 半日振休)
                    && remainingCompensatoryLeave - 0.5m > 0)
                {
                    ModelState.AddModelError(string.Empty, TakeSubstituteHolidayFirst);
                }
                // 18-2　画面.出勤区分１又は画面出勤区分２のいずれか一方≠10：半日振休　＆　振替休暇の残日数＞０
                if ((SyukkinKubun1 != 半日振休 || SyukkinKubun2 != 半日振休)
                    && remainingCompensatoryLeave > 0)
                {
                    ModelState.AddModelError(string.Empty, TakeSubstituteHolidayFirst);
                }
                else if (remainingPaidLeave < 0.5m)
                {
                    // 18-3　上記以外　＆　有給休暇の残日数＜0.5　の場合
                    ModelState.AddModelError(string.Empty, CannotTakeHalfDayPaidLeave);
                }
            }

            // 19　画面.出勤区分1≠25：欠勤　＆　画面.出勤区分2＝25：欠勤　＆　以下の場合
            if (SyukkinKubun1 != 欠勤 && SyukkinKubun2 == 欠勤)
            {
                if (SyukkinKubun1 == 半日振休 && remainingCompensatoryLeave - 0.5m >= 0.5m)
                {
                    // 19-1
                    ModelState.AddModelError(string.Empty, AbsenceWithSubstituteHolidayAvailable);
                }
                else if (SyukkinKubun1 != 半日振休 && remainingCompensatoryLeave >= 0.5m)
                {
                    // 19-2
                    ModelState.AddModelError(string.Empty, AbsenceWithSubstituteHolidayAvailable);
                }
                else if (SyukkinKubun1 != 半日有給 && remainingPaidLeave - 0.5m >= 0.5m)
                {
                    // 19-3
                    ModelState.AddModelError(string.Empty, AbsenceWithSubstituteHolidayAvailable);
                }
                else if (SyukkinKubun1 != 半日有給 && remainingPaidLeave >= 0.5m)
                {
                    // 19-4
                    ModelState.AddModelError(string.Empty, AbsenceWithSubstituteHolidayAvailable);
                }
            }

            // 20 1日有給休暇を選択時
            if (SyukkinKubun1 == 年次有給休暇_1日 || SyukkinKubun2 == 年次有給休暇_1日)
            {
                if (remainingCompensatoryLeave > 0)
                {
                    // 20-1　振替休暇の残日数＞0
                    ModelState.AddModelError(string.Empty, TakeSubstituteHolidayFirst);
                }
                else if (remainingPaidLeave < 1)
                {
                    // 20-2　上記以外　＆　有給休暇の残日数＜1
                    ModelState.AddModelError(string.Empty, CannotTakeAnnualPaidLeave);
                }
                else if (SyukkinKubun2 == 年次有給休暇_1日 &&
                    SyukkinKubun1 == 半日有給)
                {
                    // 20-3　上記以外　＆　画面.出勤区分2＝08：有給休暇　＆　画面.出勤区分1＝08：半日有給
                    ModelState.AddModelError(string.Empty, InvalidAttendanceClassification);
                }
            }

            // 21 計画有給を選択時
            if (SyukkinKubun1 == 計画有給休暇)
            {
                if (remainingPaidLeave < 1)
                {
                    // 21-1　有給休暇の残日数＜1
                    ModelState.AddModelError(string.Empty, CannotTakeAnnualPaidLeave);
                }
                else if (yukyuuZan?.KeikakuYukyuSu >= 5)
                {
                    //21-2　上記以外　＆　計画有給年間取得上限である5回＜＝有給残日数.計画有給数
                    ModelState.AddModelError(string.Empty, PlannedAnnualPaidLeaveLimit);
                }
                else if (yukyuuZan == null)
                {
                    //21-3　有給残日数が取得できない
                    ModelState.AddModelError(string.Empty, string.Format(PaidLeaveDataNotFoundFormat, SyainInfo.Code));
                }
            }

            // 22 計画特別休暇を選択時
            if (SyukkinKubun1 == 計画特別休暇)
            {
                
                if (yukyuuZan?.KeikakuYukyuSu >= 2)
                {
                    // 22-1　計画特別休暇年間取得上限である2回＜＝有給残日数.計画特別休暇数
                    ModelState.AddModelError(string.Empty, PlannedSpecialLeaveLimit);
                }
                else if (yukyuuZan == null)
                {
                    // 22-2　上記以外　＆　有給残日数が取得できない
                    ModelState.AddModelError(string.Empty, string.Format(PaidLeaveDataNotFoundFormat, SyainInfo.Code));
                }
                else if (SyukkinKubun1 == 計画特別休暇 && SyukkinKubun2 == AttendanceClassification.None)
                {
                    // 22-3　上記以外　＆　（画面.出勤区分1＝33：計画特別休暇　＆　画面.出勤区分2＝00：未選択）以外
                    ModelState.AddModelError(string.Empty, InvalidAttendanceClassification);
                }
            }

            if (!ModelState.IsValid)
            {
                return;
            }

            // 23　画面.実績入力（明細）の入力件数＝０
            if (nippouAnkens.Count == 0)
            {
                ModelState.AddModelError(string.Empty, EnterWorkPerformance);
                return;
            }

            // 24　原価連動トグルボタンONの件数＞4
            int isLinkedCount = nippouAnkens.Count(x => x.IsLinked);
            if (isLinkedCount > 4)
            {
                ModelState.AddModelError(string.Empty, MaxFiveProjectCodes);
                return;
            }

            // 25　1日勤務の場合の工番チェック
            if (kubun1Info != null && 
                !(
                    (kubun1Info.IsSyukkin == false && kubun1Info.IsVacation == true) 
                    || (kubun1Info.IsSyukkin == false && kubun1Info.IsVacation == false)
                ) &&
                !(kubun1Info.IsSyukkin == true && kubun1Info.IsVacation == true)
                )
            {

                //25-1　原価連動トグルボタンONの件数＝0　＆　実績入力の明細数＞0
                if (isLinkedCount == 0 && nippouAnkens.Count > 0)
                {
                    ModelState.AddModelError(string.Empty, SelectProjectCode);
                    return;
                }
                else if (isLinkedCount == 0 && SyukkinKubun1 == AttendanceClassification.休日出勤)
                {
                    //25-2　上記以外　＆　原価連動トグルボタンONの件数＝0　＆　画面.出勤区分1＝06：AttendanceClassification.休日出勤
                    ModelState.AddModelError(string.Empty, CreateProjectInfoForHolidayWork);
                    return;
                }
                else if (isLinkedCount == 0 && SyukkinKubun1 != AttendanceClassification.休日出勤)
                {
                    //25-3　上記以外　＆　原価連動トグルボタンONの件数＝0　＆　画面.出勤区分１≠06：AttendanceClassification.休日出勤
                    ModelState.AddModelError(string.Empty, CreateProjectInfoForNormalWork);
                    return;
                }
                else if (
                    isLinkedCount > 2
                    && SyukkinKubun1 == AttendanceClassification.休日出勤
                    &&
                    (
                        (
                        (!IsWorkDay && JissekiDate.DayOfWeek != DayOfWeek.Sunday)
                                    && NippouData.DJitsudou <= 4
                                )
                        ||
                        (JissekiDate.DayOfWeek == DayOfWeek.Sunday
                            && NippouData.NJitsudou <= 4
                                )
                    )
                )
                {
                    // 25-4
                    ModelState.AddModelError(string.Empty, HolidayWorkShortHoursProjectLimit);
                    return;
                }

            }
            bool isHalfDayLeave = (SyukkinKubun1 == 半日振休 || SyukkinKubun1 == 半日有給) &&
                      (SyukkinKubun2 == 半日振休 || SyukkinKubun2 == 半日有給);

            // 26　勤務状態の場合は未来チェック
            if (kubun1Info != null && 
                !(
                    (kubun1Info.IsSyukkin == false && kubun1Info.IsVacation == true) ||
                    (kubun1Info.IsSyukkin == false && kubun1Info.IsVacation == false)
                ) && isHalfDayLeave && JissekiDate > DateOnly.FromDateTime(DateTime.Today)
            )
            {
                ModelState.AddModelError(string.Empty, CannotRegisterFutureWorkPerformance);
                return;
            }

            // 27 半日勤務の場合の工番チェック
            //  if(SyukkinKubun1 == 半日勤務午前 ||
            //  SyukkinKubun1 == 半日勤務午後 ||
            //  SyukkinKubun1 == 半日勤務)
            // not found two other
            if (SyukkinKubun1 == 半日勤務)
            {
                if (isLinkedCount > 2)
                {
                    ModelState.AddModelError(string.Empty, HalfDayWorkProjectLimit);
                    return;

                }
                else if (isLinkedCount == 0)
                {
                    ModelState.AddModelError(string.Empty, SelectProjectCode);
                    return;
                }
            }
            // 28　休暇及び休日の場合
            if (((kubun1Info != null &&
                ((kubun1Info.IsSyukkin == false && kubun1Info.IsVacation == true) ||
                (kubun1Info.IsSyukkin == false && kubun1Info.IsVacation == false))
                )
                ||
                (SyukkinKubun1 == 半日有給 || SyukkinKubun1 == 半日振休))
                && isLinkedCount > 0)
            {
                ModelState.AddModelError(string.Empty, CannotSelectProjectDuringLeave);
                return;
            }

            // 30
            // 300番台工番チェック(連動する人のみ)
            var ankenIds = nippouAnkens
                    .Select(x => x.AnkensId)
                    .ToList();
            bool hasExpenseCode = await _context.Ankens
                    .AnyAsync(a =>
                        ankenIds.Contains(a.Id) &&
                        a.KingsJuchu != null &&
                        a.KingsJuchu.HiyouShubetuCd == 2
                    );


            if (SyainInfo.IsGenkaRendou == true && hasExpenseCode)
            {
                ModelState.AddModelError(string.Empty, CannotConfirmSupportGroupOrder);
                return;
            }

            // 31
            // 工番が使用可能か否かの最終チェック
            bool hasGenkaFrozen = await _context.Ankens
                .AnyAsync(a =>
                    ankenIds.Contains(a.Id) &&
                    a.KingsJuchu != null &&
                    a.KingsJuchu.IsGenkaToketu == true
                );

            if (hasGenkaFrozen)
            {
                ModelState.AddModelError(string.Empty, SelectedProjectCodeCannotBeUsed);
                return;
            }


            // 32　稼働制限時間・伺いに関するチェック
            if (SyainInfo.Kengen != 勤怠データ出力)
            {
                UkagaiHeader? ukagai = await _context.UkagaiHeaders.Where(h =>
                    JissekiDate.GetStartOfMonth() <= h.WorkYmd && h.WorkYmd <= JissekiDate.GetEndOfMonth() &&
                    h.SyainId == SyainInfo.Id &&
                    h.UkagaiShinseis.Any(us => us.UkagaiSyubetsu == 時間外労働時間制限拡張) &&
                    h.Invalid == false).FirstOrDefaultAsync();

                if(ukagai != null)
                {
                    bool exceedOvertime = !await OvertimeLimitCheckAsync(totalOvertime, ukagai);

                    if (exceedOvertime)
                    {
                        if (ukagai.Status != 承認)
                        {
                            ModelState.AddModelError(string.Empty, OvertimeLimitExceeded);
                            return;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, OvertimeLimitUnapproved);
                            return;
                        }
                    }
                }
                
            }
        }


        // 社員が制限を超えていないか確認する
        private async Task<bool> OvertimeLimitCheckAsync(decimal totalOvertimeToCheck, UkagaiHeader? ukagai)
        {
            var overtimeLimit = 0m;
            if(ukagai?.Status != 承認)
            {
                overtimeLimit = SyainInfo.KintaiZokusei.MaxLimitTime * 60;
            } else
            {
                overtimeLimit = SyainInfo.KintaiZokusei.SeigenTime * 60;
            }

            if(overtimeLimit != 0 && overtimeLimit < totalOvertimeToCheck)
            {
                return true;
            } else
            {
                return false;
            }

        }


        // 社員の振替休暇の残日数
        private async Task<decimal> GetRemainingNumberOfCompensatoryLeaveAsync(long syainId, DateOnly jissekiDate)
        {
            var furikyuu = await _context.FurikyuuZans.AsNoTracking().Where(x =>
            x.SyainId == syainId &&
            x.DaikyuuKigenYmd >= jissekiDate &&
            (
                (x.IsOneDay == true && (x.SyutokuState == LeaveBalanceFetchStatus.未 || x.SyutokuState == LeaveBalanceFetchStatus.半日))
                || (x.IsOneDay == false && x.SyutokuState == LeaveBalanceFetchStatus.未)
            )).ToListAsync();
            var totalFurikyu = furikyuu.Sum(x =>
            {
                if (x.IsOneDay == true && x.SyutokuState == LeaveBalanceFetchStatus.未) return 1m;
                if (x.IsOneDay == true && x.SyutokuState == LeaveBalanceFetchStatus.半日) return 0.5m;
                if (x.IsOneDay == false && x.SyutokuState == LeaveBalanceFetchStatus.未) return 0.5m;
                else return 0m;
            }
            );
            return totalFurikyu;
        }


        // 勤務時間１～３全てがブランクかどうか
        private bool CheckWorkingTimes() => NippouData.SyukkinHm1 != null || NippouData.TaisyutsuHm1 != null ||
               NippouData.SyukkinHm2 != null || NippouData.TaisyutsuHm2 != null ||
               NippouData.SyukkinHm3 != null || NippouData.TaisyutsuHm3 != null;


        // 出退勤時間1の出勤時間
        private bool HasClockInTime() => NippouData.SyukkinHm1 != null && NippouData.TaisyutsuHm1 != null;


        // 総労働時間
        private int GetTotalWorkTimeMinute() => TimeCalculator.CalculationJitsudouTime(NippouData.SyukkinHm1?.ToString("HHmm") ?? "",
                       NippouData.TaisyutsuHm1?.ToString("HHmm") ?? "",
                       NippouData.SyukkinHm2?.ToString("HHmm") ?? "",
                       NippouData.TaisyutsuHm2?.ToString("HHmm") ?? "",
                       NippouData.SyukkinHm3?.ToString("HHmm") ?? "",
                       NippouData.TaisyutsuHm3?.ToString("HHmm") ?? "");


        // 有給休暇の残日数
        private async Task<decimal> GetRemainNumberOfPaidVacationAsync(long syainBaseId)
        {
            var Yukyuu = await FetchYuukyuuZanDataAsync(syainBaseId);
            if (Yukyuu == null) return 0;
            var remainingYukyuu = Yukyuu.KeikakuYukyuSu + Yukyuu.Kurikoshi - Yukyuu.Syouka;
            return remainingYukyuu;
        }


        // 出勤区分Dataを取得する
        private async Task<SyukkinKubun?> FetchSyukkinKubunDataAsync(string? code) => await _context.SyukkinKubuns.AsNoTracking()
            .Where(row => row.CodeString == code).FirstOrDefaultAsync();


        // 有給残Dataを取得する
        private async Task<YuukyuuZan?> FetchYuukyuuZanDataAsync(long syainBaseId) => await _context.YuukyuuZans.AsNoTracking()
                .Where(row => row.SyainBaseId == syainBaseId).FirstOrDefaultAsync();

    }
}


