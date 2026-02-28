using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Model.Data;
using Model.Enums;
using Model.Extensions;
using Model.Model;
using ZouryokuCommonLibrary.Utils;

namespace Zouryoku.Pages.JissekiNyuryoku
{
    public class JissekiNyuryokuCreateViewModelService(
                ZouContext db,
                AppSettings appSettings,
                ApplicationConfig applicationConfig,
                Syain syain,
                DateOnly jisekiDate,
                bool isDairiInput,
                List<Syuttaikin> inputSyuttaikins,
                Syain loginUser,
                DateOnly nowDate
            )
    {
        // 出勤区分の未選択オプション
        private static readonly SelectListItem SyukkinKbnNone = new() { Text = "--- 区分選択 ---", Value = "00" };


        /// <summary>
        /// 休暇日数残タイプ
        /// </summary>
        public enum KyuukaZanType
        {
            Over = 0,       // 1日以上
            Half = 1,       // 半日
            None = 2,       // なし
        }

        private readonly JissekiNyuryokuCommonService _commonService = new(db);
        private readonly JissekiNyuryokuQueryService _queryService = new(db);

        private readonly AppSettings _appSettings = appSettings;

        private readonly ApplicationConfig _applicationConfig = applicationConfig;
        private readonly Syain _syain = syain;
        private readonly DateOnly _jissekiDate = jisekiDate;
        private readonly bool _isDairiInput = isDairiInput;
        private readonly List<Syuttaikin> _inputSyuttaikins = inputSyuttaikins;
        private readonly Syain _loginUser = loginUser;
        private readonly DateOnly _nowDate = nowDate;

        private Nippou? _nippou = null;
        private List<WorkingHour> _workingHours = [];
        private List<SyukkinKubun> _syukkinKubuns = [];
        private List<UkagaiHeader> _ukagaiHeaders = [];
        private List<NippouAnken> _nippouAnkens = [];
        private FurikyuuZan? _furikyuuZan;
        private Nippou? _lastNippou;
        private List<SelectListItem> _bumonProcessList = [];

        private List<Syuttaikin> _syuttaikins = [];

        public async Task<NippouInputViewModel> CreateViewModelAsync()
        {
            var vm = new NippouInputViewModel();

            // 必要なデータを取得
            _nippou = await _queryService.FetchNippouDataAsync(_syain.Id, _jissekiDate); ;
            _workingHours = await _queryService.FetchWorkingHoursListAsync(_syain.Id, _jissekiDate); ;
            _syukkinKubuns = await _queryService.FetchSyukkinKubnAll();
            _ukagaiHeaders = await _queryService.FetchUkagaiHeadersAsync(_syain.Id, _jissekiDate);
            if (_nippou != null)
            {
                _nippouAnkens = await _queryService.FetchNippouAnkensAsync(_nippou.Id);
            }
            _furikyuuZan = await _queryService.FetchFurikyuZanBySyainIdAndDate(_syain.Id, _jissekiDate);
            _lastNippou = await _queryService.FetchNippouDataAsync(_syain.Id, _jissekiDate.AddDays(-1));
            _bumonProcessList = await _commonService.GetBumonProcessListAsync();

            // 出退勤時間のセットを適用
            _syuttaikins = await BuildShuttaikins();

            vm.JissekiDate = _jissekiDate;
            vm.SyainBaseId = _syain.SyainBaseId;
            vm.IsDairiInput = _isDairiInput;

            // 日報のID
            vm.Id = _nippou?.Id;

            // 日報のVersion
            vm.Version = _nippou?.Version;

            vm.SyukkinHm1 = _syuttaikins[0].Syukkin;
            vm.TaisyutsuHm1 = _syuttaikins[0].Taisyutsu;
            vm.SyukkinHm2 = _syuttaikins[1].Syukkin;
            vm.TaisyutsuHm2 = _syuttaikins[1].Taisyutsu;
            vm.SyukkinHm3 = _syuttaikins[2].Syukkin;
            vm.TaisyutsuHm3 = _syuttaikins[2].Taisyutsu;

            vm.Syuttaikin1 = FormatRange(_syuttaikins[0]);
            vm.Syuttaikin2 = FormatRange(_syuttaikins[1]);
            vm.Syuttaikin3 = FormatRange(_syuttaikins[2]);

            // 出勤区分のリストを構築
            (vm.SyukkinKubun1List, vm.SyukkinKubun2List) = BuildSyukkinKbnList();

            // 実績時間の設定
            vm.TotalJissekiJikan = _nippouAnkens.Sum(a => a.JissekiJikan);

            // 出勤区分の設定
            (vm.SyukkinKubun1, vm.SyukkinKubun2) = await GetDefaultSyukkinKbunAsync(vm.JitsudouTime);

            // 申請状況
            vm.ShinseiInfos = BuildUkagaiHeaderData(_ukagaiHeaders);

            //振替休日取得予定日に振休残の取得予定日を設定
            vm.FurikyuYoteiDate = _furikyuuZan?.SyutokuYoteiYmd;

            // 実績入力欄の設定
            vm.JissekiInputs = _nippouAnkens.Select((na, i) =>
                JissekiInputViewModel.FromEntity(na, i, _bumonProcessList))
                .ToList();

            // 時間を計算する
            vm.JitsudouTime = GetJitsudouTime(_syuttaikins); ;
            vm.YakanTime = _syuttaikins.Sum(v => v.YakanTime);
            vm.ShinyaTime = _syuttaikins.Sum(v => v.ShinyaTime);
            vm.YakanShinyaTime = _syuttaikins.Sum(v => v.YakanShinyaTime);
            var timeContainer = await _commonService.CalcJissekiNyuryokuTimeAsync(vm);
            vm.DJitsudou = timeContainer.DJitsudou;
            vm.NJitsudou = timeContainer.NJitsudou;
            vm.TotalZangyo = timeContainer.TotalZangyo;

            // 前日までの残業時間合計
            vm.Total3MonthZangyoTotal = await _commonService.Calc3MonthZangyoJikanAsync(_syain.Id, _jissekiDate);
            vm.RuisekiJikangai = await _commonService.CalcThisMonthZangyoJikanAsync(_syain.Id, _jissekiDate);

            // ボタン表示制御
            vm.IsIchijiHozonButtonVisible = IsIchijiHozonButtonVisible();
            vm.IsKakuteiButtonVisible = await IsKakuteiButtonVisibleAsync();
            vm.IsKakuteiKaijoButtonVisible = IsKakuteiKaijoButtonVisible();

            // 画面上部のメッセージ
            vm.MessageString = await GetMessage(vm);

            return vm;
        }

        /// <summary>
        /// 確定ボタンの表示制御
        /// </summary>
        /// <returns>表示：true、非表示：false</returns>
        public async Task<bool> IsKakuteiButtonVisibleAsync()
        {
            // 出退勤のどちらか片方が未入力の出退勤あり
            if (HasMissingTime)
                return false;

            // 日報が確定している
            if (IsNippouConfirmed)
                return false;

            var isHikadoubi = await _commonService.IsHikadoubiAsync(_jissekiDate);

            // 出退勤1～3のいずれかに入力あり & 非稼働日 & 承認済みの休日出勤の申請がない
            if (HasInputTime && isHikadoubi && !HasKyujitsuSyukkinApproved)
                return false;

            // 出退勤時間が夜間作業の時間帯を含む ＆　夜間作業の伺い申請が未申請又は未承認 の場合
            if (HasYakanSagyo && !HasYakanApproved)
                return false;

            // 出退勤時間が早朝作業の時間帯を含む ＆　早朝作業の伺い申請が未申請又は未承認 の場合
            if (HasSouchouSagyo && !HasSouchouApproved)
                return false;

            // 出退勤時間が深夜作業の時間帯を含む ＆　深夜作業の伺い申請が未申請又は未承認 の場合
            if (HasShinyaSagyo && !HasShinyaApproved)
                return false;

            // アプリコンフィグ.日報停止日＜＝INパラメータ.実績年月日　の場合
            if (IsKakuteiStop)
                return false;

            // 前日の日報実績が 未登録 or 前日の日報実績の日報登録状況区分‐≠1確定保存 の場合
            if (_lastNippou == null || _lastNippou.TourokuKubun != DailyReportStatusClassification.確定保存)
                return false;

            // 代理入力でない & 他人の日報
            if (IsEditoerble)
                return false;

            return true;
        }

        /// <summary>
        /// 確定解除ボタンの表示制御
        /// </summary>
        /// <returns>表示：true、非表示：false</returns>
        public bool IsKakuteiKaijoButtonVisible()
        {
            // 日報が未登録
            if (_nippou == null)
                return false;

            // 日報が一時保存の状態
            if (IsNippouTempSaved)
                return false;

            // 日報実績.確定年月日 < システム日付
            if (_nippou != null &&
                IsNippouConfirmed
                &&
                _nippou.NippouYmd < _nowDate)
                return false;

            // 代理入力でない & 他人の日報
            if (IsEditoerble)
                return false;

            return true;
        }

        /// <summary>
        /// 一時保存ボタンの表示制御
        /// </summary>
        /// <returns>表示：true、非表示：false</returns>
        public bool IsIchijiHozonButtonVisible()
        {
            // 日報が確定している
            if (IsNippouConfirmed)
                return false;

            // 代理入力でない & 他人の日報
            if (IsEditoerble)
                return false;

            return true;
        }

        // 出退勤時間のセットを適用するメソッド
        public async Task<List<Syuttaikin>> BuildShuttaikins()
        {
            // 日報が存在する場合は日報の時間を優先、代理入力または日報の時間入力がある場合は入力された時間を使用、それ以外は勤怠打刻の時間を使用
            // 出退勤時間のセットを保持するリスト
            List<Syuttaikin> syuttaikins = [];

            if (_nippou != null)
            {
                // 日報の出退勤時間で出退勤時間のセットを作成
                syuttaikins =
                [
                    new(_nippou.SyukkinHm1, _nippou.TaisyutsuHm1),
                    new(_nippou.SyukkinHm2, _nippou.TaisyutsuHm2),
                    new(_nippou.SyukkinHm3, _nippou.TaisyutsuHm3),
                ];
                return syuttaikins;
            }

            if (_isDairiInput || (_syain!.KintaiZokusei.Code.IsNippouTimeInput()))
            {
                // 勤務日報時間入力の出退勤時間で出退勤時間のセットを作成
                syuttaikins = _inputSyuttaikins;
                return syuttaikins;
            }

            // 伺いヘッダーから伺い種別コードが早朝のもののみに絞り込み、そのなかで先頭要素の作業開始時刻を取得
            var sochoUkagai = _ukagaiHeaders.SelectMany(uh => uh.UkagaiShinseis.Where(us => us.UkagaiSyubetsu == InquiryType.早朝作業)).FirstOrDefault();
            var shijiStartTime = sochoUkagai?.UkagaiHeader.KaishiJikoku?.ToStrByHHmmNoColon();

            var isRefreshDay = await _commonService.IsRefreshDayAsync(_jissekiDate);

            // 勤怠打刻の時間を補正して出退勤時間のセットを作成
            syuttaikins = _workingHours.Select(wh =>
            {

                // 勤怠打刻の時間を補正
                var (shukkin, taikin) = TimeCalculator.Hosei(
                    wh.SyukkinTime?.ToTimeOnly().ToStrByHHmmNoColon() ?? string.Empty,
                    wh.TaikinTime?.ToTimeOnly().ToStrByHHmmNoColon() ?? string.Empty,
                    HasYakanApproved,
                    HasShinyaApproved,
                    HasSouchouApproved,
                    HasRefreshDayApproved,
                    isRefreshDay,
                    shijiStartTime);

                var start = Syuttaikin.ToTimeOnlyFromHHmm(shukkin);
                var end = Syuttaikin.ToTimeOnlyFromHHmm(taikin);

                return new Syuttaikin(start, end);
            }).ToList();
            // 3件になるまで作成する
            while (syuttaikins.Count < 3)
            {
                syuttaikins.Add(new Syuttaikin(null, null));
            }

            return syuttaikins;
        }

        public int GetJitsudouTime(List<Syuttaikin> Syuttaikins)
        {
            return TimeCalculator.CalculationJitsudouTime(
                Syuttaikins.ElementAtOrDefault(0)?.SyukkinStr ?? string.Empty,
                Syuttaikins.ElementAtOrDefault(0)?.TaisyutsuStr ?? string.Empty,
                Syuttaikins.ElementAtOrDefault(1)?.SyukkinStr ?? string.Empty,
                Syuttaikins.ElementAtOrDefault(1)?.TaisyutsuStr ?? string.Empty,
                Syuttaikins.ElementAtOrDefault(2)?.SyukkinStr ?? string.Empty,
                Syuttaikins.ElementAtOrDefault(2)?.TaisyutsuStr ?? string.Empty
            );
        }

        public List<ShinseiInfoViewModel> BuildUkagaiHeaderData(List<UkagaiHeader> ukagaiHeaders)
        {
            return ukagaiHeaders.Select(u =>
            {
                return new ShinseiInfoViewModel(u);
            }).ToList();
        }

        /// <summary>
        /// 出勤区分のリストを構築
        /// </summary>
        public (List<SelectListItem>, List<SelectListItem>) BuildSyukkinKbnList()
        {
            // 出勤区分1のリストを作成
            var SyukkinKubun1List = _syukkinKubuns.Where(row => row.IsNeedKubun1 == true)
            .Select(row => new SelectListItem
            {
                Text = row.Name,
                Value = row.Code.ToString()
            })
            .ToList();
            SyukkinKubun1List.Insert(0, SyukkinKbnNone);

            // 出勤区分2のリストを作成
            var SyukkinKubun2List = _syukkinKubuns.Where(row => row.IsNeedKubun2 == true)
                .Select(row => new SelectListItem
                {
                    Text = row.Name,
                    Value = row.Code.ToString()
                })
                .ToList();
            SyukkinKubun2List.Insert(0, SyukkinKbnNone);

            return (SyukkinKubun1List, SyukkinKubun2List);
        }

        // 出勤区分の設定
        public async Task<(AttendanceClassification, AttendanceClassification)> GetDefaultSyukkinKbunAsync(decimal jitsudouTime)
        {
            var kubun1 = AttendanceClassification.None;
            var kubun2 = AttendanceClassification.None;

            // 日報登録済の場合、日報の出勤区分
            if (_nippou != null)
            {
                var syukkinKubun1 = _syukkinKubuns.Where(row => row.Id == _nippou.SyukkinKubunId1).SingleOrDefault();
                var syukkinKubun2 = _syukkinKubuns.Where(row => row.Id == _nippou.SyukkinKubunId2).SingleOrDefault();

                kubun1 = syukkinKubun1?.Code ?? AttendanceClassification.None;
                kubun2 = syukkinKubun2?.Code ?? AttendanceClassification.None;
                if (_loginUser.SyainBaseId != _syain.SyainBaseId && kubun1 == AttendanceClassification.生理休暇)
                {
                    syukkinKubun1 = _syukkinKubuns.Where(row => row.Code == AttendanceClassification.その他特別休暇).FirstOrDefault();
                    kubun1 = syukkinKubun1?.Code ?? AttendanceClassification.None;
                }
                if (_loginUser.SyainBaseId != _syain.SyainBaseId && kubun2 == AttendanceClassification.生理休暇)
                {
                    syukkinKubun2 = _syukkinKubuns.Where(row => row.Code == AttendanceClassification.その他特別休暇).FirstOrDefault();
                    kubun2 = syukkinKubun2?.Code ?? AttendanceClassification.None;
                }
                return (kubun1, kubun2);
            }

            // 日報未登録の場合の初期設定

            var isHikadoubi = await _commonService.IsHikadoubiAsync(_jissekiDate);

            // 未出勤の場合
            if (jitsudouTime == 0)
            {
                // 非稼働日の場合
                if (isHikadoubi)
                {
                    kubun1 = AttendanceClassification.休日;
                    kubun2 = AttendanceClassification.None;
                    return (kubun1, kubun2);
                }

                // 休暇残日数による初期設定
                return await GetDefaultSyukkinKbnByKyuukaZanAsync();
            }

            // 非稼働日の場合
            if (isHikadoubi)
            {
                kubun1 = AttendanceClassification.休日出勤;
                kubun2 = AttendanceClassification.None;
                return (kubun1, kubun2);
            }

            // パート
            if (_syain?.KintaiZokusei.Code == EmployeeWorkType.パート)
            {
                kubun1 = AttendanceClassification.パート勤務;
                kubun2 = AttendanceClassification.None;
                return (kubun1, kubun2);
            }

            // flex < 実働時間
            if (Time.flex < jitsudouTime)
            {
                kubun1 = AttendanceClassification.通常勤務;
                kubun2 = AttendanceClassification.None;
                return (kubun1, kubun2);
            }

            // 実働時間 <= flex
            if (jitsudouTime <= Time.flex)
            {
                kubun1 = AttendanceClassification.半日勤務;
                kubun2 = await GetSyukkinKubun2MixedHolidayAsync();
                return (kubun1, kubun2);
            }

            return (kubun1, kubun2);
        }

        /// <summary>
        /// 休暇残日数による出勤区分の初期設定を取得
        /// </summary>
        /// <returns>出勤区分１と出勤区分２の初期設定のペア</returns>
        //public (AttendanceClassification, AttendanceClassification) GetDefaultSyukkinKbnByKyuukaZan(KyuukaZanType furikyuuZanType, KyuukaZanType yuukyuuZanType)
        public async Task<(AttendanceClassification, AttendanceClassification)> GetDefaultSyukkinKbnByKyuukaZanAsync()
        {
            // 振替休暇の残日数
            var furikyuuZanNissuu = await _commonService.GetFurikyuZanNissuAsync(_syain.Id, _jissekiDate);

            // 有給休暇残
            var yuukyuuZan = await _queryService.FetchYuukyuuZanAsync(_syain.SyainBaseId);
            // 有給休暇の残日数
            var yuukyuuZanNissuu = GetYuukyuuZanNissu(yuukyuuZan);

            // 振替休暇残の休暇日数残Type
            var furikyuuZanType = GetKyuukaZanType(furikyuuZanNissuu);
            // 有給休暇残の休暇日数残Type
            var yuukyuuZanType = GetKyuukaZanType(yuukyuuZanNissuu);

            // 振休1日以上
            if (furikyuuZanType == KyuukaZanType.Over)
                return (AttendanceClassification.振替休暇, AttendanceClassification.None);

            // 振休残半日
            if (furikyuuZanType == KyuukaZanType.Half)
            {
                // 有給残1日以上
                if (yuukyuuZanType == KyuukaZanType.Over)
                    return (AttendanceClassification.半日振休, AttendanceClassification.半日有給);

                // 有給残半日
                if (yuukyuuZanType == KyuukaZanType.Half)
                    return (AttendanceClassification.半日振休, AttendanceClassification.半日有給);

                // 有給残なし
                if (yuukyuuZanType == KyuukaZanType.None)
                    return (AttendanceClassification.半日振休, AttendanceClassification.欠勤);

                // 上記以外
                return (AttendanceClassification.None, AttendanceClassification.None);
            }

            // 振休残なし
            if (furikyuuZanType == KyuukaZanType.None)
            {
                // 有給残1日以上
                if (yuukyuuZanType == KyuukaZanType.Over)
                    return (AttendanceClassification.年次有給休暇_1日, AttendanceClassification.None);

                // 有給残半日
                if (yuukyuuZanType == KyuukaZanType.Half)
                    return (AttendanceClassification.半日有給, AttendanceClassification.欠勤);

                // 有給残なし
                if (yuukyuuZanType == KyuukaZanType.None)
                    return (AttendanceClassification.欠勤, AttendanceClassification.None);

                // 上記以外
                return (AttendanceClassification.None, AttendanceClassification.None);
            }

            // 上記以外
            return (AttendanceClassification.None, AttendanceClassification.None);
        }

        public async Task<AttendanceClassification> GetSyukkinKubun2MixedHolidayAsync()
        {
            // 振替休暇の残日数
            var furikyuuZanNissuu = await _commonService.GetFurikyuZanNissuAsync(_syain.Id, _jissekiDate);

            // 有給休暇残
            var yuukyuuZan = await _queryService.FetchYuukyuuZanAsync(_syain.SyainBaseId);
            // 有給休暇の残日数
            var yuukyuuZanNissuu = GetYuukyuuZanNissu(yuukyuuZan);
            // 半日有給休暇の取得回数
            var yuukyuuHalfKaisuu = GetHalfYuukyuuKaisu(yuukyuuZan);

            // 振替休暇残の休暇日数残Type
            var furikyuuZanType = GetKyuukaZanType(furikyuuZanNissuu);
            // 有給休暇残の休暇日数残Type
            var yuukyuuZanType = GetKyuukaZanType(yuukyuuZanNissuu);

            var tempSyukkinKbn = AttendanceClassification.None;

            // 半日有給の取得上限
            var yuukyuuHalfLimit = _appSettings.MaxHannichiYuukyuuDays;

            if (furikyuuZanType == KyuukaZanType.Half || furikyuuZanType == KyuukaZanType.Over)
            {
                tempSyukkinKbn = AttendanceClassification.半日振休;
            }
            else
            {
                if (yuukyuuZanType == KyuukaZanType.Over)
                {
                    if (yuukyuuHalfLimit <= yuukyuuHalfKaisuu)
                    {
                        tempSyukkinKbn = AttendanceClassification.年次有給休暇_1日;
                    }
                    else
                    {
                        tempSyukkinKbn = AttendanceClassification.半日有給;
                    }
                }
                else if (yuukyuuZanType == KyuukaZanType.Half)
                {
                    if (yuukyuuHalfKaisuu < yuukyuuHalfLimit)
                    {
                        tempSyukkinKbn = AttendanceClassification.半日有給;
                    }
                    else
                    {
                        tempSyukkinKbn = AttendanceClassification.欠勤;
                    }
                }
                else
                {
                    tempSyukkinKbn = AttendanceClassification.欠勤;
                }
            }

            // 出勤区分２の区分であれば、その出勤区分を返却、以外はNone
            return _syukkinKubuns.Any(s => s.IsNeedKubun2 && s.Code == tempSyukkinKbn) ? tempSyukkinKbn : AttendanceClassification.None;
        }

        public async Task<string?> GetMessage(NippouInputViewModel vm)
        {
            // アプリConfig.日報停止日＜＝INパラメータ.実績年月日　の場合
            if (IsKakuteiStop)
            {
                return "決算運用のため１０月中旬までの期間、確定を停止しています。";
            }

            // 出退勤時間1～3の何れかが以下の条件に該当するような打刻漏れがある場合。
            // 出勤時間が有り＆退勤時間が無し
            // 出勤時間が無し＆退勤時間が有り
            if (HasMissingTime)
            {
                return "出退勤の打刻漏れがあります。打刻の修正を行ってください。";
            }

            IList<string> shijiNames = [];
            var isHikadoubi = await _commonService.IsHikadoubiAsync(_jissekiDate);

            // 非稼働日 & 休日出勤の申請情報が存在しない又は存在するが未承認の場合。
            if (isHikadoubi && !HasKyujitsuSyukkinApproved)
            {
                shijiNames.Add("休日出勤");
            }

            // 夜間作業をしている & 夜間作業の申請情報が存在しない又は存在するが未承認の場合。
            if (HasYakanSagyo && !HasYakanApproved)
            {
                shijiNames.Add("夜間作業");
            }

            // 早朝作業をしている & 早朝作業の申請情報が存在しない又は存在するが未承認の場合。
            if (HasSouchouSagyo && !HasSouchouApproved)
            {
                shijiNames.Add("早朝作業");
            }

            // 深夜作業をしている & 深夜作業の申請情報が存在しない又は存在するが未承認の場合。
            if (HasShinyaSagyo && !HasShinyaApproved)
            {
                shijiNames.Add("深夜作業");
            }

            if (shijiNames.Count != 0)
            {
                var msg = $"{String.Join("・", shijiNames)}の指示がでていない、又は承認されていません。";
                return msg;
            }

            // 未承認の 早朝作業 or リフレッシュデー残業 の申請情報が存在する場合
            shijiNames = [];
            if (HasSouchouUnapproved)
            {
                shijiNames.Add("早朝作業");
            }
            if (HasRefreshDayUnapproved)
            {
                shijiNames.Add("リフレッシュデー残業");
            }
            if (shijiNames.Count != 0)
            {
                var msg = $"{String.Join("・", shijiNames)}の指示を申請中ですが、最終承認されていません。";
                return msg;
            }

            // 勤怠打刻の時間に修正フラグが立っている & 未承認の打刻時間修正の申請情報が存在する場合
            if (HasEditedWorkingHour && HasDakokuUpdateUnApproved)
            {
                return "打刻時間修正の指示を申請中ですが、最終承認されていません。";
            }

            // 残業時間制限メッセージ
            if (_jissekiDate == _jissekiDate.GetEndOfMonth())
            {
                var isOver = await _commonService.IsOverZangyoSeigenJikanAsync(_syain, vm);

                if (isOver)
                {
                    var jikanngaiKakuchoShinsei = await _queryService.FetchJikangaiKakuchoSinseiAsync(_syain!.Id, _jissekiDate);

                    if (jikanngaiKakuchoShinsei?.Status != ApprovalStatus.承認)
                    {
                        return "残業時間が制限時間を超えますが、時間外労働時間拡張申請が未申請、または最終承認されていないため確定できません。";
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// 休暇日数残タイプ取得
        /// </summary>
        /// <param name="zanNissu">休暇の残日数</param>
        /// <returns>休暇日数残タイプ</returns>
        public KyuukaZanType GetKyuukaZanType(decimal zanNissu)
        {
            // 1日以上
            if (Time.hanniti < zanNissu)
                return KyuukaZanType.Over;

            // 半日
            if (zanNissu == Time.hanniti)
                return KyuukaZanType.Half;

            // ない
            return KyuukaZanType.None;
        }

        /// <summary>
        /// 有給休暇の残日数を取得する
        /// </summary>
        public decimal GetYuukyuuZanNissu(YuukyuuZan? yuukyuuzan) =>
            yuukyuuzan == null ? 0m : yuukyuuzan.Wariate + yuukyuuzan.Kurikoshi - yuukyuuzan.Syouka;

        /// <summary>
        /// 半日有給休暇の取得回数を取得する
        /// </summary>
        public decimal GetHalfYuukyuuKaisu(YuukyuuZan? yuukyuuZan) =>
            yuukyuuZan == null ? 0m : yuukyuuZan.HannitiKaisuu;

        // --------------------------
        //　伺い申請の属性
        // --------------------------

        // 承認済み早朝作業の申請があるか
        public bool HasSouchouApproved =>
            _syain != null && _syain.KintaiZokusei.Code.IsNippouInputUnlimited() ||
            _ukagaiHeaders.Any(uh => uh.UkagaiShinseis.Any(us => us.UkagaiSyubetsu == InquiryType.早朝作業 && uh.Status == ApprovalStatus.承認));

        // 承認済み夜間作業の申請があるか
        public bool HasYakanApproved =>
            _syain != null && _syain.KintaiZokusei.Code.IsNippouInputUnlimited() ||
            _ukagaiHeaders.Any(uh => uh.UkagaiShinseis.Any(us => us.UkagaiSyubetsu == InquiryType.夜間作業 && uh.Status == ApprovalStatus.承認));

        // 承認済み深夜作業の申請があるか
        public bool HasShinyaApproved =>
            _syain != null && _syain.KintaiZokusei.Code.IsNippouInputUnlimited() ||
            _ukagaiHeaders.Any(uh => uh.UkagaiShinseis.Any(us => us.UkagaiSyubetsu == InquiryType.深夜作業 && uh.Status == ApprovalStatus.承認));

        // 承認済み休日出勤の申請があるか
        public bool HasKyujitsuSyukkinApproved =>
            _syain != null && _syain.KintaiZokusei.Code.IsNippouInputUnlimited() ||
            _ukagaiHeaders.Any(uh => uh.UkagaiShinseis.Any(us => us.UkagaiSyubetsu == InquiryType.休日出勤 && uh.Status == ApprovalStatus.承認));

        // 未承認の早朝作業の申請があるか
        public bool HasSouchouUnapproved =>
            _syain != null && !_syain.KintaiZokusei.Code.IsNippouInputUnlimited() &&
            _ukagaiHeaders.Any(uh => uh.UkagaiShinseis.Any(us => us.UkagaiSyubetsu == InquiryType.早朝作業 && uh.Status != ApprovalStatus.承認));

        // 未承認のリフレッシュデー残業の申請情報があるか
        public bool HasRefreshDayUnapproved =>
            _syain != null && !_syain.KintaiZokusei.Code.IsNippouInputUnlimited() &&
            _ukagaiHeaders.Any(uh => uh.UkagaiShinseis.Any(us => us.UkagaiSyubetsu == InquiryType.リフレッシュデー残業 && uh.Status != ApprovalStatus.承認));

        // 未承認の打刻時間修正の申請があるか
        public bool HasDakokuUpdateUnApproved =>
            _syain != null && !_syain.KintaiZokusei.Code.IsNippouInputUnlimited() &&
            _ukagaiHeaders.Any(uh => uh.UkagaiShinseis.Any(us => us.UkagaiSyubetsu == InquiryType.打刻時間修正 && uh.Status != ApprovalStatus.承認));

        // 承認済みリフレッシュデー残業の申請があるか
        public bool HasRefreshDayApproved => 
            _ukagaiHeaders.Any(uh => uh.UkagaiShinseis.Any(us => us.UkagaiSyubetsu == InquiryType.リフレッシュデー残業 && uh.Status == ApprovalStatus.承認));

        // --------------------
        // 出退勤時間の属性
        // --------------------

        // 出勤時間のセットのいずれかが早朝時間帯に該当するか
        public bool HasSouchouSagyo => _syuttaikins.Any(ts =>
            TimeCalculator.GetIncludeTimeWithout休憩(ts.SyukkinStr, ts.TaisyutsuStr, Time.早朝作業) > 0
        );

        // 出退勤時間のセットのいずれかが夜間時間帯に該当するか
        public bool HasYakanSagyo => _syuttaikins.Any(ts =>
            TimeCalculator.GetIncludeTimeWithout休憩(ts.SyukkinStr, ts.TaisyutsuStr, Time.夜間作業) > 0
        );

        // 出退勤時間のセットのいずれかが深夜時間帯に該当するか
        public bool HasShinyaSagyo => _syuttaikins.Any(ts =>
            TimeCalculator.GetIncludeTimeWithout休憩(ts.SyukkinStr, ts.TaisyutsuStr, Time.深夜作業) > 0
        );

        // ---------------------
        // 勤怠打刻の属性
        // ---------------------

        // 勤怠打刻のいずれかに修正フラグが立っているか
        public bool HasEditedWorkingHour => _workingHours.Any(wh => wh.Edited);

        // 日報確定停止期間か
        public bool IsKakuteiStop => _applicationConfig.NippoStopDate <= _jissekiDate;

        // 出退勤時間1～3の何れかが以下の条件に該当するような打刻漏れがあるか
        // 出勤時間が有り＆退勤時間が無し
        // 出勤時間が無し＆退勤時間が有り
        public bool HasMissingTime => _syuttaikins.Any(ts => ts.Syukkin.HasValue ^ ts.Taisyutsu.HasValue);

        public bool HasInputTime => _syuttaikins.Any(ts => ts.Syukkin.HasValue && ts.Taisyutsu.HasValue);

        // ---------------------
        // 日報の属性
        // ---------------------

        // 日報が確定の状態
        public bool IsNippouConfirmed => _nippou != null && _nippou.TourokuKubun == DailyReportStatusClassification.確定保存;

        // 日報が一時保存の状態
        public bool IsNippouTempSaved => _nippou != null && _nippou.TourokuKubun == DailyReportStatusClassification.一時保存;

        // 代理入力でない & 他人の日報
        public bool IsEditoerble => !_isDairiInput && _loginUser.SyainBaseId != _syain.SyainBaseId;


        public static string FormatRange(Syuttaikin syuttaikin)
        {
            var syukkinHm = syuttaikin.Syukkin;
            var taisyutsuHm = syuttaikin.Taisyutsu;

            if (!syukkinHm.HasValue && !taisyutsuHm.HasValue)
            {
                return "-";
            }
            var taisyutsuStr = taisyutsuHm == (new TimeOnly(0, 0, 0)) ? "24:00" : taisyutsuHm.ToStrByHHmmOrEmpty();
            return $"{syukkinHm.ToStrByHHmmOrEmpty()} ～ {taisyutsuStr}";
        }

    }
}
