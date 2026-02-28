using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Model.Data;
using Model.Enums;
using Model.Model;
using ZouryokuCommonLibrary.Utils;
using static Model.Enums.AttendanceClassification;
using static Model.Enums.EmployeeWorkType;
using static Zouryoku.Utils.Const;

namespace Zouryoku.Pages.JissekiNyuryoku
{
    public class JissekiNyuryokuConfirmValidator(
        ZouContext db,
        AppSettings appSettings,
        NippouInputViewModel vm,
        Syain syain,
        DateOnly today,
        ModelStateDictionary modelState)
    {
        private readonly AppSettings _appSettings = appSettings;
        private readonly JissekiNyuryokuQueryService _queryService = new(db);
        private readonly JissekiNyuryokuCommonService _commonService = new(db);
        private readonly NippouInputViewModel _vm = vm;
        private readonly Syain _syain = syain;
        private readonly DateOnly _today = today;
        private readonly ModelStateDictionary _modelState = modelState;

        // 確定の Validation
        public async Task FinalConfirmValidationAsync()
        {
            // 選択必須チェック
            CheckRequired();

            // 非稼働日か否か
            var isHikadoubi = await _commonService.IsHikadoubiAsync(_vm.JissekiDate);

            // 稼働日チェック
            CheckKadoubi(isHikadoubi);

            // 非稼働日チェック
            CheckHikadoubi(isHikadoubi);

            // 出勤区分１のマスタを取得
            var syukkinKubunInfo1 = await _queryService.FetchSyukkinKubunByCodeAsync(_vm.SyukkinKubun1) ??
                throw new InvalidOperationException($"「{_vm.SyukkinKubun1.GetDisplayName}」が出勤区分のマスタに登録されていません。");

            // 休暇チェック
            if (syukkinKubunInfo1.IsHolidayKubun)
            {
                CheckKyuuka();
            }
            // 上記以外 & 出勤区分１＝半日勤務
            else if (_vm.SyukkinKubun1 == 半日勤務)
            {
                // 半日勤務時チェック
                CheckSyukkinHalf();
            }
            // 上記以外 & 出勤区分１＝（半日振休、半日有給）
            else if (_vm.SyukkinKubun1 == 半日振休 || _vm.SyukkinKubun1 == 半日有給)
            {

                // 半日休暇 + 半日休暇(1日休暇も含む)チェック
                CheckKyuukaHalf();
            }
            // 出勤区分１が休暇の区分ではない＆半日勤務の区分ではない
            else if (!syukkinKubunInfo1.IsHolidayKubun && !syukkinKubunInfo1.IsHalfKimuKubun)
            {
                // 1日勤務のチェック
                CheckSyukkin();
            }

            // 出退勤時間が全てブランク時のチェック
            CheckNoneWorkingHour(syukkinKubunInfo1);

            // 通常勤務を選択時チェック
            CheckTsuujyouKinmu();

            // パートを選択時チェック
            CheckPartKinmu();

            // 半日勤務を選択時チェック
            HannitiCheck();

            // 有給残取得
            var yuukyuuZan = await _queryService.FetchYuukyuuZanAsync(_syain.SyainBaseId);
            // 半日有給休暇の取得上限
            var maxHannichiYuukyuuDays = _appSettings.MaxHannichiYuukyuuDays;

            // 半日有給休暇チェック
            CheckHalfYuukyuuTimes(yuukyuuZan, maxHannichiYuukyuuDays);

            // 生理休暇を選択時
            CheckSeiriKyuuka();

            // 振替休暇の残日数
            var furikyuuZanNissu = await _commonService.GetFurikyuZanNissuAsync(_syain.Id, _vm.JissekiDate);

            // 振替休暇チェック
            CheckFurikyuuOfDay(furikyuuZanNissu);

            // 半日振休チェック
            CheckFurikyuuOfHalfDay(furikyuuZanNissu);

            // 有給休暇の残日数
            var yuukyuuZanNissu = yuukyuuZan!.KeikakuYukyuSu + yuukyuuZan.Kurikoshi - yuukyuuZan.Syouka;

            // 半日有給チェック
            CheckYuukyuuOfHalfDay(furikyuuZanNissu, yuukyuuZanNissu);

            // 半日欠勤チェック
            CheckKekkinOfHalfDay(furikyuuZanNissu, yuukyuuZanNissu);

            // 1日有給休暇チェック
            CheckYuukyuuOfDay(furikyuuZanNissu, yuukyuuZanNissu);

            // 日計画有給休暇チェック
            var maxKeikakuYuukyuuDays = _appSettings.MaxKeikakuYuukyuuDays;
            var keikakuYuukyuuSu = yuukyuuZan!.KeikakuYukyuSu;
            KeikakuYuukyuuCheck(yuukyuuZanNissu, keikakuYuukyuuSu, maxKeikakuYuukyuuDays);

            // 計画特別休暇チェック
            var maxKeikakuTokukyuuDays = _appSettings.MaxKeikakuTokukyuuDays;
            var keikakuTokukyuuSu = yuukyuuZan.KeikakuTokukyuSu;
            KeikakuTokukyuuCheck(keikakuTokukyuuSu, maxKeikakuTokukyuuDays);

            // ここまでのエラーを全てreturn
            if (!_modelState.IsValid)
            {
                return;
            }

            // 実績入力欄の0件チェック
            if (!CheckRequiredJissekiInput())
            {
                return;
            }

            // 原価連動ONの上限数チェック
            if (!CheckGenkaRendouMaxCount())
            {
                return;
            }

            var dayType = await _commonService.GetDayTypeAsync(_vm.JissekiDate);

            // 1日勤務時の工番選択の妥当性をチェック
            if (!CheckKoubanForOnedayKinmu(syukkinKubunInfo1, dayType))
            {
                return;
            }

            // 未来日チェック
            if (!CheckFutureDate(syukkinKubunInfo1))
            {
                return;
            }

            // 半日勤務の場合の工番チェック
            if (!CheckKoubanForHalfdayKinmu())
            {
                return;
            }

            // 休暇及び休日の工番チェック
            if (!CheckKoubanForKyuujitsu(syukkinKubunInfo1))
            {
                return;
            }

            // 案件の妥当性チェック
            if (!(await CheckAnkenAsync()))
            {
                return;
            }

            // 時間外労働時間拡張チェック
            await CheckKinmuLimitUkagai();

        }

        /// <summary>
        /// 選択必須チェック
        /// </summary>
        public void CheckRequired()
        {
            // 出勤区分１＝00：未選択
            if (_vm.SyukkinKubun1 == AttendanceClassification.None)
            {
                _modelState.AddModelError(string.Empty, SelectAttendanceClassification);
            }
        }

        /// <summary>
        /// 稼働日チェック
        /// </summary>
        /// <param name="isHikadoubi">非稼働日かどうか</param>
        public void CheckKadoubi(bool isHikadoubi)
        {
            if (!isHikadoubi)
            {
                // 出勤区分１＝休日
                if (_vm.SyukkinKubun1 == 休日)
                {
                    _modelState.AddModelError(string.Empty, HolidayOnWeekdayError);
                }
                // 出勤区分１＝休日出勤
                else if (_vm.SyukkinKubun1 == 休日出勤)
                {
                    _modelState.AddModelError(string.Empty, HolidayWorkOnWeekdayError);
                }
            }

        }

        /// <summary>
        /// 非稼働日チェック
        /// </summary>
        /// <param name="isHikadoubi">非稼働日かどうか</param>
        public void CheckHikadoubi(bool isHikadoubi)
        {
            if (isHikadoubi)
            {
                // 出勤区分１≠（休日出勤、休日）
                if (_vm.SyukkinKubun1 != 休日出勤 && _vm.SyukkinKubun1 != 休日)
                {
                    _modelState.AddModelError(string.Empty, SelectHolidayOnWeekend);
                }
                // 上記以外 & 出勤区分２≠未選択
                else if (_vm.SyukkinKubun2 != AttendanceClassification.None)
                {
                    _modelState.AddModelError(string.Empty, InvalidAttendanceClassification);
                }
                // 上記以外 & 振替休暇予定日が未入力の場合
                else if (_vm.SyukkinKubun1 == 休日出勤 && _vm.FurikyuYoteiDate == null)
                {
                    _modelState.AddModelError(string.Empty, EnterSubstituteHolidayDate);
                }
            }
        }

        /// <summary>
        /// 休暇チェック
        /// </summary>
        public void CheckKyuuka()
        {
            // 出勤区分２≠未選択
            if (_vm.SyukkinKubun2 != AttendanceClassification.None)
            {
                _modelState.AddModelError(string.Empty, InvalidAttendanceClassification);
            }
            // 上記以外 & 出勤区分１＝休日 & 出勤区分２＝未選択 & 出退勤時間１の出勤時間≠ブランク
            else if (_vm.SyukkinKubun1 == 休日 && _vm.SyukkinKubun2 == AttendanceClassification.None
                && _vm.SyukkinHm1 != null)
            {
                _modelState.AddModelError(string.Empty, CannotSelectHolidayWithClockIn);
            }
        }

        /// <summary>
        /// 半日勤務時チェック
        /// </summary>
        public void CheckSyukkinHalf()
        {
            // 出勤区分２≠未選択
            if (_vm.SyukkinKubun2 != AttendanceClassification.None)
            {
                _modelState.AddModelError(string.Empty, InvalidAttendanceClassification);
            }
        }

        /// <summary>
        /// 半日休暇 + 半日休暇(1日休暇も含む)チェック
        /// </summary>
        public void CheckKyuukaHalf()
        {
            // 出勤区分２＝未選択
            if (_vm.SyukkinKubun2 == AttendanceClassification.None)
            {
                _modelState.AddModelError(string.Empty, InvalidAttendanceClassification);
            }

            // 出勤区分１と出勤区分２共に半日有給
            if (_vm.SyukkinKubun1 == 半日有給 && _vm.SyukkinKubun2 == 半日有給)
            {
                _modelState.AddModelError(string.Empty, SelectAnnualPaidLeaveOneDay);
            }
        }

        /// <summary>
        /// 1日勤務のチェック
        /// </summary>
        public void CheckSyukkin()
        {
            // 勤怠属性≠パート & 出勤区分２≠未選択
            if (_syain.KintaiZokusei.Code != パート && _vm.SyukkinKubun2 != AttendanceClassification.None)
            {
                _modelState.AddModelError(string.Empty, InvalidAttendanceClassification);
            }
        }

        /// <summary>
        /// 出退勤時間が全てブランク時のチェック
        /// </summary>
        /// <param name="syukkinKubunInfo1">出勤区分１のマスタ</param>
        public void CheckNoneWorkingHour(SyukkinKubun syukkinKubunInfo1)
        {
            // 出退勤全てがブランク
            if (_vm.IsSyuttaikinAllBlank)
            {
                // 出勤区分１＝(通常勤務、休日出勤、半日勤務、パート)
                if (_vm.SyukkinKubun1 == 通常勤務 || _vm.SyukkinKubun1 == 休日出勤 ||
                   _vm.SyukkinKubun1 == 半日勤務 || _vm.SyukkinKubun1 == パート勤務)
                {
                    _modelState.AddModelError(string.Empty, string.Format(NotWorkingCannotSelectFormat, syukkinKubunInfo1.Name));
                }
            }
        }

        /// <summary>
        /// 通常勤務を選択時チェック
        /// </summary>
        public void CheckTsuujyouKinmu()
        {
            // 通常勤務を選択時
            // 出勤区分１＝通常勤務 & 総労働時間≠0 & 総労働時間 <= 4時間(240分)																					
            if (_vm.SyukkinKubun1 == 通常勤務 && _vm.JitsudouTime != 0 && _vm.JitsudouTime <= Time.flex)
            {
                _modelState.AddModelError(string.Empty, SelectHalfDayWorkDueToShortHours);
            }
        }

        /// <summary>
        /// パートを選択時チェック
        /// </summary>
        public void CheckPartKinmu()
        {
            // 出勤区分１＝パート & 勤怠属性≠パート
            if (_vm.SyukkinKubun1 == パート勤務 && _syain.KintaiZokusei.Code != パート)
            {
                _modelState.AddModelError(string.Empty, CannotUsePartTimeWork);
            }
        }

        /// <summary>
        /// 半日勤務を選択時チェック
        /// </summary>
        public void HannitiCheck()
        {
            // 出勤区分１＝半日勤務 & 8時間(480分) <= 総労働時間																					
            if (_vm.SyukkinKubun1 == 半日勤務 && Time.kitei <= _vm.JitsudouTime)
            {
                _modelState.AddModelError(string.Empty, SelectNormalWork);
            }
        }

        /// <summary>
        /// 半日有給休暇チェック
        /// </summary>
        /// <param name="yuukyuuZan"></param>
        /// <param name="maxHannichiYuukyuuDays"></param>
        public void CheckHalfYuukyuuTimes(YuukyuuZan? yuukyuuZan, short maxHannichiYuukyuuDays)
        {
            if (_vm.SyukkinKubun1 == 半日有給 || (_vm.SyukkinKubun2 == 半日有給 && _vm.SyukkinKubun1 == 半日勤務))
            {
                if (yuukyuuZan == null)
                {
                    // 有給残が取得できない
                    _modelState.AddModelError(string.Empty, PaidLeaveDataNotFoundFormat);
                    return;
                }

                // 半日有給の上限チェック
                if (maxHannichiYuukyuuDays <= yuukyuuZan.HannitiKaisuu)
                {
                    _modelState.AddModelError(string.Empty, string.Format(AnnualHalfDayPaidLeaveLimit, maxHannichiYuukyuuDays));
                }
            }

        }

        /// <summary>
        /// 生理休暇チェック
        /// </summary>
        public void CheckSeiriKyuuka()
        {
            // 女性のみ選択可能
            if ((_vm.SyukkinKubun1 == 生理休暇 || _vm.SyukkinKubun2 == 生理休暇) && _syain.Seibetsu != '2')
            {
                _modelState.AddModelError(string.Empty, CannotTakePhysiologicalLeave);
            }
        }

        /// <summary>
        /// 振替休暇チェック
        /// </summary>
        /// <param name="furikyuuZanNissu"></param>
        public void CheckFurikyuuOfDay(decimal furikyuuZanNissu)
        {
            if (_vm.SyukkinKubun1 == 振替休暇 && _vm.SyukkinKubun2 == AttendanceClassification.None)
            {
                // 振替休暇の残日数 <= 0.5日
                if (furikyuuZanNissu <= Time.hanniti)
                {
                    _modelState.AddModelError(string.Empty, CannotTakeSubstituteHoliday);
                }
            }

        }

        /// <summary>
        /// 半日振休チェック
        /// </summary>
        /// <param name="furikyuuZanNissu"></param>
        public void CheckFurikyuuOfHalfDay(decimal furikyuuZanNissu)
        {

            // 半日振替休暇を選択時
            if (_vm.SyukkinKubun1 == 半日振休 || _vm.SyukkinKubun2 == 半日振休)
            {
                // 振休が残っていない
                if (furikyuuZanNissu <= 0)
                {
                    _modelState.AddModelError(string.Empty, CannotTakeHalfDaySubstituteHoliday);
                }
            }
        }

        /// <summary>
        /// 半日有給チェック
        /// </summary>
        /// <param name="furikyuuZanNissu">振替休暇の残日数</param>
        /// <param name="yuukyuuZanNissu">有給休暇の残日数</param>
        public void CheckYuukyuuOfHalfDay(decimal furikyuuZanNissu, decimal yuukyuuZanNissu)
        {
            if (_vm.SyukkinKubun1 == 半日有給 || _vm.SyukkinKubun2 == 半日有給)
            {
                // 半日有給ではないほうの出勤区分
                var otherSyukkinKubun = _vm.OtherSyukkinKubun(半日有給);

                var furikyuuZan = otherSyukkinKubun == 半日振休 ? furikyuuZanNissu - Time.hanniti : furikyuuZanNissu;

                // 振休を優先して取得
                if (0 < furikyuuZan)
                {
                    _modelState.AddModelError(string.Empty, TakeSubstituteHolidayFirst);

                }
                // 有給が残っていない
                else if (yuukyuuZanNissu < Time.hanniti)
                {
                    _modelState.AddModelError(string.Empty, CannotTakeHalfDayPaidLeave);

                }
            }
        }

        /// <summary>
        /// 半日欠勤チェック
        /// </summary>
        /// <param name="furikyuuZanNissuu">振替休暇の残日数</param>
        /// <param name="yuukyuuZanNissuu">有給休暇の残日数</param>
        public void CheckKekkinOfHalfDay(decimal furikyuuZanNissuu, decimal yuukyuuZanNissuu)
        {
            // 出勤区分１≠欠勤 & 出勤区分２＝欠勤
            if (_vm.SyukkinKubun1 != 欠勤 && _vm.SyukkinKubun2 == 欠勤)
            {
                // 欠勤ではないほうの出勤区分
                var otherSyukkinKubun = _vm.OtherSyukkinKubun(欠勤);

                // 振替休暇残数を取得
                var furikyuuZan = otherSyukkinKubun == 半日振休 ? furikyuuZanNissuu - Time.hanniti : furikyuuZanNissuu;

                // 有給休暇残数を取得
                var yuukyuuZan = otherSyukkinKubun == 半日有給 ? yuukyuuZanNissuu - Time.hanniti : yuukyuuZanNissuu;

                // 振休取得可能
                if (Time.hanniti <= furikyuuZan)
                {
                    _modelState.AddModelError(string.Empty, string.Format(AbsenceWithSubstituteHolidayAvailable, "半日振休"));
                }
                // 有給取得可能
                else if (Time.hanniti <= yuukyuuZan)
                {
                    _modelState.AddModelError(string.Empty, string.Format(AbsenceWithSubstituteHolidayAvailable, "半日有給"));
                }
            }
        }

        /// <summary>
        /// 1日有給休暇チェック
        /// </summary>
        /// <param name="furikyuuZanNissuu">振替休暇の残日数</param>
        /// <param name="yuukyuuZanNissuu">有給休暇の残日数</param>
        public void CheckYuukyuuOfDay(decimal furikyuuZanNissuu, decimal yuukyuuZanNissuu)
        {
            if (_vm.SyukkinKubun1 == 年次有給休暇_1日 || _vm.SyukkinKubun2 == 年次有給休暇_1日)
            {
                // 振休残が残っている場合、振休を優先で取得
                if (0 < furikyuuZanNissuu)
                {
                    _modelState.AddModelError(string.Empty, TakeSubstituteHolidayFirst);
                }
                // 有給残が1日未満
                else if (yuukyuuZanNissuu < Time.ichinichi)
                {
                    _modelState.AddModelError(string.Empty, CannotTakeAnnualPaidLeave);
                }
                // 不正な出勤区分の組み合わせ
                else if (_vm.SyukkinKubun2 == 年次有給休暇_1日 && _vm.SyukkinKubun1 == 半日有給)
                {
                    _modelState.AddModelError(string.Empty, InvalidAttendanceClassification);
                }
            }
        }

        /// <summary>
        /// 計画有給チェック
        /// </summary>
        /// <param name="yuukyuuZanNissuu">有給休暇の残日数</param>
        /// <param name="KeikakuYukyuSu">取得済み計画有給日数</param>
        /// <param name="maxKeikakuYuukyuuDays">計画有給休暇の取得上限日数</param>
        public void KeikakuYuukyuuCheck(decimal yuukyuuZanNissuu, short KeikakuYukyuSu, short maxKeikakuYuukyuuDays)
        {
            if (_vm.SyukkinKubun1 == 計画有給休暇)
            {
                // 有給残が1日未満
                if (yuukyuuZanNissuu < Time.ichinichi)
                {
                    _modelState.AddModelError(string.Empty, CannotTakeAnnualPaidLeave);
                }
                // 取得上限を超えている
                else if (maxKeikakuYuukyuuDays <= KeikakuYukyuSu)
                {
                    _modelState.AddModelError(string.Empty, string.Format(PlannedAnnualPaidLeaveLimit, maxKeikakuYuukyuuDays));
                }
                // 不正な出勤区分の組み合わせ
                else if (!(_vm.SyukkinKubun1 == 計画有給休暇 && _vm.SyukkinKubun2 == AttendanceClassification.None))
                {
                    _modelState.AddModelError(string.Empty, InvalidAttendanceClassification);
                }
            }
        }

        /// <summary>
        /// 計画特別休暇チェック
        /// </summary>
        /// <param name="KeikakuTokukyuSu">取得済み計画特別休暇日数</param>
        /// <param name="maxKeikakuTokukyuuDays">計画特別休暇の取得上限日数</param>
        public void KeikakuTokukyuuCheck(short KeikakuTokukyuSu, short maxKeikakuTokukyuuDays)
        {
            if (_vm.SyukkinKubun1 == 計画特別休暇)
            {
                // 取得上限を超えている
                if (maxKeikakuTokukyuuDays <= KeikakuTokukyuSu)
                {
                    _modelState.AddModelError(string.Empty, string.Format(PlannedSpecialLeaveLimit, maxKeikakuTokukyuuDays));
                }
                // 不正な出勤区分の組み合わせ
                else if (!(_vm.SyukkinKubun1 == 計画特別休暇 && _vm.SyukkinKubun2 == AttendanceClassification.None))
                {
                    _modelState.AddModelError(string.Empty, InvalidAttendanceClassification);
                }
            }
        }


        /// <summary>
        /// 実績入力欄の0件チェック
        /// </summary>
        /// <returns></returns>
        public bool CheckRequiredJissekiInput()
        {
            // 実績入力（明細）の入力件数＝０
            if (_vm.JissekiInputs.Count == 0)
            {
                _modelState.AddModelError(string.Empty, EnterWorkPerformance);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 原価連動ONの上限数チェック
        /// </summary>
        /// <returns></returns>
        public bool CheckGenkaRendouMaxCount()
        {
            int isLinkedCount = _vm.JissekiInputs.Count(x => x.IsLinked);
            if (4 < isLinkedCount)
            {
                _modelState.AddModelError(string.Empty, MaxFiveProjectCodes);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 1日勤務時の工番選択の妥当性をチェック
        /// </summary>
        /// <param name="syukkinKubunInfo1">出勤区分１のマスタ</param>
        /// <param name="dayType">DayType</param>
        /// <returns></returns>
        public bool CheckKoubanForOnedayKinmu(SyukkinKubun syukkinKubunInfo1, DayType dayType)
        {
            // 25 1日勤務の場合の工番チェック
            if (!syukkinKubunInfo1.IsHolidayKubun && !syukkinKubunInfo1.IsHalfKimuKubun)
            {
                int isLinkedCount = _vm.JissekiInputs.Count(x => x.IsLinked);

                // 原価連動トグルボタンONがない
                if (isLinkedCount == 0)
                {
                    _modelState.AddModelError(string.Empty, SelectProjectCode);
                    return false;
                }

                // 4時間以下の休日出勤で３つ以上選択
                if (2 < isLinkedCount && _vm.SyukkinKubun1 == 休日出勤)
                {
                    if ((dayType == DayType.土曜祝祭日 && _vm.DJitsudou <= 4) ||
                        (dayType == DayType.日曜 && _vm.NJitsudou <= 4))
                    {
                        _modelState.AddModelError(string.Empty, HolidayWorkShortHoursProjectLimit);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 未来日チェック
        /// </summary>
        /// <param name="syukkinKubunInfo1"></param>
        /// <returns></returns>
        public bool CheckFutureDate(SyukkinKubun syukkinKubunInfo1)
        {
            // 26 勤務状態の場合は未来チェック
            if (!syukkinKubunInfo1.IsHolidayKubun && !(_vm.IsHannitiKyuuka && _vm.IsHannitiKyuuka2))
            {
                if (_today < _vm.JissekiDate)
                {
                    _modelState.AddModelError(string.Empty, CannotRegisterFutureWorkPerformance);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 半日勤務の場合の工番チェック
        /// </summary>
        public bool CheckKoubanForHalfdayKinmu()
        {
            if (_vm.SyukkinKubun1 == 半日勤務)
            {
                int isLinkedCount = _vm.JissekiInputs.Count(x => x.IsLinked);

                if (2 < isLinkedCount)
                {
                    _modelState.AddModelError(string.Empty, HalfDayWorkProjectLimit);
                    return false;

                }

                if (isLinkedCount == 0)
                {
                    _modelState.AddModelError(string.Empty, SelectProjectCode);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 休暇及び休日の工番チェック
        /// </summary>
        /// <param name="syukkinKubunInfo1"></param>
        /// <returns></returns>
        public bool CheckKoubanForKyuujitsu(SyukkinKubun syukkinKubunInfo1)
        {
            // 28 休暇及び休日の場合
            if (syukkinKubunInfo1.IsHolidayKubun || _vm.IsHannitiKyuuka)
            {
                int isLinkedCount = _vm.JissekiInputs.Count(x => x.IsLinked);
                if (0 < isLinkedCount)
                {
                    _modelState.AddModelError(string.Empty, CannotSelectProjectDuringLeave);
                    return false;
                }
            }

            return true;

        }

        /// <summary>
        /// 案件の妥当性チェック
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckAnkenAsync()
        {
            var jissekiInputs = _vm.JissekiInputs;
            foreach (var i in jissekiInputs)
            {
                var ankenId = i.AnkensId;
                var anken = await _queryService.FetchAnkenById(ankenId!.Value);

                // 案件が不在
                if (anken == null)
                {
                    _modelState.AddModelError(string.Empty, string.Format(ErrorNotFound, "案件", ankenId));
                    return false;
                }

                // 300番台工番チェック(連動する人のみ)
                if (_syain.IsGenkaRendou && anken.KingsJuchu?.HiyouShubetuCd == 2)
                {
                    _modelState.AddModelError(string.Empty, CannotConfirmSupportGroupOrder);
                    return false;
                }

                // 工番が使用可能か否かの最終チェック
                if (anken.KingsJuchu?.IsGenkaToketu ?? false)
                {
                    _modelState.AddModelError(string.Empty, SelectedProjectCodeCannotBeUsed);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 時間外労働時間拡張チェック
        /// </summary>
        /// <returns></returns>
        public async Task CheckKinmuLimitUkagai()
        {
            decimal CheckZangyoLimitTime(bool extended)
            {
                return extended ? _syain.KintaiZokusei.MaxLimitTime * 60 : _syain.KintaiZokusei.SeigenTime * 60;
            }

            decimal CheckZangyoTargetTime()
            {
                return (_syain.KintaiZokusei.IsOvertimeLimit3m ? _vm.Total3MonthZangyoTotal : _vm.RuisekiJikangai) + _vm.TotalZangyo;
            }

            // 権限が512のbitが立っている社員（制限なしの社員）はチェック対象外
            if ((((int)_syain.Kengen) & 512) == 512)
                return;

            // 時間外労働時間制限拡張
            var kakuchoShinsei = await _queryService.FetchJikangaiKakuchoSinseiAsync(_syain.Id, _vm.JissekiDate);

            // 申請が承認されているか
            var extended = kakuchoShinsei != null && kakuchoShinsei.Status == ApprovalStatus.承認;

            // 制限時間
            decimal limitTime = CheckZangyoLimitTime(extended);

            // チェック対象の残業時間合計
            decimal zangyoTime = CheckZangyoTargetTime();

            // 制限時間を超えている場合、エラー
            if (limitTime != 0 && limitTime < zangyoTime)
            {
                if (extended)
                {
                    _modelState.AddModelError(string.Empty, OvertimeLimitExceeded);
                    return;
                }
                else
                {
                    _modelState.AddModelError(string.Empty, OvertimeLimitUnapproved);
                    return;
                }
            }
        }
    }
}
