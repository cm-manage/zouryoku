using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Zouryoku.Attributes;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using ZouryokuCommonLibrary.Utils;
using static LanguageExt.Prelude;
using static Model.Enums.ApprovalStatus;
using static Model.Enums.AttendanceClassification;
using static Model.Enums.DailyReportStatusClassification;
using static Model.Enums.InquiryType;
using static Model.Enums.ResponseStatus;
using Const = Zouryoku.Utils.Const;



namespace Zouryoku.Pages.JissekiNyuryoku
{
    /// <summary>
    /// 実績入力ページモデル
    /// </summary>
    [FunctionAuthorization]
    public class IndexModel : BasePageModel<IndexModel>
    {
        private readonly ZouContext _context;
        private readonly ILogger<IndexModel> _logger;
        private readonly IOptions<AppConfig> _optionsAccessor;

        public IndexModel(ZouContext context, ILogger<IndexModel> logger, IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine) : base(context, logger, optionsAccessor, viewEngine)
        {
            _context = context;
            _logger = logger;
            _optionsAccessor = optionsAccessor;
        }

        public override bool UseInputAssets { get; } = true;

        [BindProperty]
        public long SyainId { get; set; }

        public long SyainBaseId { get; set; }

        [BindProperty]
        public DateOnly JissekiDate { get; set; }

        [BindProperty]
        public bool IsDairiInput { get; set; }

        [BindProperty]
        public NippouViewModel NippouData { get; set; } = new NippouViewModel();

        private const string ErrorNippouDeleted = "この日報は解除されています。";
        private const string ErrorNippouAccountLinked = "この日報は経理連動が完了しているので、確定解除出来ません。";
        private const string ErrorNippouSubsequentConfirmed = "以降の確定日があるため確定解除出来ません。最終確定日から順に解除してください。";
        private const string ErrorInternalServer = "サーバー内部でエラーが発生しました。";
        private const string ErrorEmployeeNotFound = "社員が見つかりません。";

        [BindProperty]
        public UkagaiHeadersViewModel UkagaiHeadersData { get; set; } = default!;

        [BindProperty]
        public CardsViewModel NippouAnkenCards { get; set; } = new CardsViewModel();

        [BindProperty(SupportsGet = true)]
        public string? From { get; set; }

        [BindProperty]
        public DateOnly? FuriYoteiDate { get; set; }

        public bool ConfirmButton { get; set; } = true;
        public bool TemporarySaveButton { get; set; } = true;
        public bool UnconfirmButton { get; set; } = true;
        public string? MessageString { get; set; }


        //  初期化を正しい C# 構文に修正
        public List<SelectListItem> SyukkinKubun1List { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> SyukkinKubun2List { get; set; } = new List<SelectListItem>();

        private Syain LoginUser { get; set; } = default!;


        // 汎用の DB ヘルパーを追加（AsNoTracking を付与して使い回す）
        // ※再利用可能なクエリの取得を簡潔にするため
        private Task<T?> FirstOrDefaultNoTrackingAsync<T>(IQueryable<T> query) where T : class
            => query.AsNoTracking().FirstOrDefaultAsync();


        // ---------------------------------------------
        // OnGet
        // ---------------------------------------------
        public async Task<IActionResult> OnGetAsync(long syainBaseId, DateOnly jissekiDate, bool isDairiInput,
            TimeOnly? syukkinHm1, TimeOnly? taisyutsuHm1,
            TimeOnly? syukkinHm2, TimeOnly? taisyutsuHm2,
            TimeOnly? syukkinHm3, TimeOnly? taisyutsuHm3)
        {
            JissekiDate = jissekiDate;
            IsDairiInput = isDairiInput;
            SyainBaseId = syainBaseId;

            var loginInfoOption = HttpContext.Session.Get<LoginInfo>();
            if (loginInfoOption.IsNone)
            {
                _logger.LogWarning("未ログイン状態でスケジュール申請画面にアクセスされました。");
                return RedirectToPage("/Logins/Index");
            }

            loginInfoOption.IfSome(info => LoginUser = info.User);
            // 12 Application Config
            var appconfig = await _context.ApplicationConfigs.FirstOrDefaultAsync() ?? throw new InvalidOperationException("app_config が未登録です。");


            // 9 社員マスタ取得
            var syainEntity = await FetchSyainDataByBaseIdAsync(syainBaseId, jissekiDate);
            if (syainEntity == null) return NotFound();

            SyainId = syainEntity.Id;

            // 1 日報実績取得
            var nippouEntity = await FetchNippouDataAsync(syainEntity.Id, jissekiDate);


            // 6 勤怠打刻取得  // 最大3件
            var workingHoursEntities = await FetchWorkingHoursListAsync(syainEntity.Id, jissekiDate);

            // 7 出勤区分名称取得
            var kubunsEntity = await _context.SyukkinKubuns.AsNoTracking().ToListAsync();
            SyukkinKubun1List = kubunsEntity.Where(row => row.IsNeedKubun1 == true)
            .Select(row => new SelectListItem
            {
                Value = row.CodeString,
                Text = row.Name
            })
            .ToList();
            SyukkinKubun2List = kubunsEntity.Where(row => row.IsNeedKubun2 == true)
                .Select(row => new SelectListItem
                {
                    Value = row.CodeString,
                    Text = row.Name
                })
            .ToList();

            // 8 BUMON LIST
            await LoadBumonProcessListAsync();


            // 14 指示情報取得
            var ukagaiHeaderEntity = await FetchUkagaiHeadersListAsync(syainEntity.Id, jissekiDate);


            if (ukagaiHeaderEntity.Any())
            {
                // 5.出勤区分名取得
                if (nippouEntity != null)
                {
                    var syukkinKubun1 = await FirstOrDefaultNoTrackingAsync(_context.SyukkinKubuns.Where(row => row.Id == nippouEntity.SyukkinKubunId1));
                    var syukkinKubun2 = await FirstOrDefaultNoTrackingAsync(_context.SyukkinKubuns.Where(row => row.Id == nippouEntity.SyukkinKubunId2));

                    NippouData.SyukkinKubun1 = syukkinKubun1?.Code ?? AttendanceClassification.None;
                    NippouData.SyukkinKubun2 = syukkinKubun2?.Code ?? AttendanceClassification.None;
                    if (LoginUser.Id != SyainId && NippouData.SyukkinKubun1 == 生理休暇)
                    {
                        syukkinKubun1 = await FirstOrDefaultNoTrackingAsync(_context.SyukkinKubuns.Where(row => row.Code == その他特別休暇));
                        NippouData.SyukkinKubun1 = syukkinKubun1?.Code ?? AttendanceClassification.None;
                    }
                    if (LoginUser.Id != SyainId && NippouData.SyukkinKubun2 == 生理休暇)
                    {
                        syukkinKubun2 = await FirstOrDefaultNoTrackingAsync(_context.SyukkinKubuns.Where(row => row.Code == その他特別休暇));
                        NippouData.SyukkinKubun2 = syukkinKubun2?.Code ?? AttendanceClassification.None;
                    }
                }


                // 2.振替休暇残取得
                var furikyuEntity = await FirstOrDefaultNoTrackingAsync(_context.FurikyuuZans.Where(row => row.SyainId == syainEntity.Id
                && row.KyuujitsuSyukkinYmd == jissekiDate));

                // 3 日報実績⇔案件取得
                if (nippouEntity != null)
                {
                    var nippouAnkensEntities = await _context.NippouAnkens.Where(row => row.NippouId == nippouEntity.Id)
                        .Include(row => row.Ankens).ThenInclude(row => row.KingsJuchu).OrderBy(row => row.Id)
                        .ToListAsync();

                    NippouAnkenCards.NippouAnkens.AddRange(
                        nippouAnkensEntities
                            .Select(NippouAnkenViewModel.FromEntity)
                    );
                }


            }
            else
            {

                // 10 勤怠属性
                var kintai = await _context.KintaiZokuseis.AsNoTracking().Where(row => row.Id == syainEntity.KintaiZokuseiId)
                    .FirstOrDefaultAsync();

                if (isDairiInput || (kintai?.Code == EmployeeWorkType.フリー || kintai?.Code == EmployeeWorkType.パート || kintai?.Code == EmployeeWorkType.標準社員外))
                {
                    NippouData.SyukkinHm1 = syukkinHm1;
                    NippouData.TaisyutsuHm1 = taisyutsuHm1;
                    NippouData.SyukkinHm2 = syukkinHm2;
                    NippouData.TaisyutsuHm2 = taisyutsuHm2;
                    NippouData.SyukkinHm3 = syukkinHm3;
                    NippouData.TaisyutsuHm3 = taisyutsuHm3;
                }
                else
                {
                    foreach (var workingHoursEntity in workingHoursEntities)
                    {
                        var ukagai = await _context.UkagaiHeaders.AsNoTracking().Where(row => row.Id == workingHoursEntity.UkagaiHeaderId && row.Invalid == false)
                                            .FirstOrDefaultAsync();
                        if (ukagai != null)
                        {
                            ukagaiHeaderEntity.Add(ukagai);
                        }
                    }
                }
            }
            // holiday
            var holiday = await FetchHolidayDataAsync(jissekiDate);
            bool isWorkday = await IsWorkingWeekDayAsync(jissekiDate);

            // 15.1 出退勤時間
            GetNippouData(nippouEntity, workingHoursEntities, jissekiDate, syainEntity, ukagaiHeaderEntity, holiday, syukkinHm1, taisyutsuHm1, syukkinHm2, taisyutsuHm2, syukkinHm3, taisyutsuHm3);

            MessageString = await CheckForNotificationMessageAsync(NippouData, syainEntity, workingHoursEntities, isWorkday, appconfig, ukagaiHeaderEntity, jissekiDate);

            await CheckConfirmButtonAsync(nippouEntity, LoginInfo.User.SyainBaseId, syainEntity, jissekiDate, isDairiInput);

            await CheckUnconfirmButtonAsync(nippouEntity, jissekiDate, LoginInfo.User.SyainBaseId, syainBaseId, isDairiInput);

            await CheckTemporarySaveButtonAsync(nippouEntity, LoginInfo.User.SyainBaseId, syainBaseId, isDairiInput);

            if (nippouEntity == null)
            {
                await GetKubunDataAsync(isWorkday, jissekiDate, syainEntity);
            }
            var ukagaiData = ukagaiHeaderEntity.FirstOrDefault(u => u.WorkYmd == jissekiDate);
            if (ukagaiData != null) UkagaiHeadersData = UkagaiHeadersViewModel.FromEntity(ukagaiData);


            // 画面への入力可能 / 画面への入力は不可能
            if (nippouEntity == null || nippouEntity.TourokuKubun == 一時保存)
            {
                ViewData["DisableAllInput"] = false;
            }
            if (nippouEntity != null && nippouEntity.TourokuKubun == 確定保存)
            {
                ViewData["DisableAllInput"] = true;
            }
            if (isDairiInput == false && LoginUser.SyainBaseId != syainBaseId)
            {
                ViewData["DisableAllInput"] = true;
            }
            return Page();
        }


        /// <summary>
        /// 15 - 1 データベースから日報データを取得
        /// </summary>
        /// <param name="nippou">日報</param>
        /// <param name="workingHours">勤怠打刻</param>
        /// <param name="holiday">非稼働日</param>
        private void GetNippouData(Nippou? nippou, List<WorkingHour> workingHours, DateOnly jissekiDate, Syain syainInfo, List<UkagaiHeader> ukagaiHeaders, Hikadoubi? holiday,
            TimeOnly? syukkinHm1, TimeOnly? taisyutsuHm1,
            TimeOnly? syukkinHm2, TimeOnly? taisyutsuHm2,
            TimeOnly? syukkinHm3, TimeOnly? taisyutsuHm3)
        {
            if (nippou != null)
            {
                // 1-1）日報実績が存在する場合

                NippouData.Id = nippou.Id;
                NippouData.NippouYmd = nippou.NippouYmd;
                NippouData.SyukkinHm1 = nippou.SyukkinHm1;
                NippouData.TaisyutsuHm1 = nippou.TaisyutsuHm1;
                NippouData.SyukkinHm2 = nippou.SyukkinHm2;
                NippouData.TaisyutsuHm2 = nippou.TaisyutsuHm2;
                NippouData.SyukkinHm3 = nippou.SyukkinHm3;
                NippouData.TaisyutsuHm3 = nippou.TaisyutsuHm3;

            }
            else if (IsDairiInput == true || syainInfo.KintaiZokuseiId is (int)EmployeeWorkType.フリー or
                (int)EmployeeWorkType.標準社員外 or (int)EmployeeWorkType.パート)
            {
                // 1-2)上記以外
                NippouData.SyukkinHm1 = syukkinHm1;
                NippouData.TaisyutsuHm1 = taisyutsuHm1;
                NippouData.SyukkinHm2 = syukkinHm2;
                NippouData.TaisyutsuHm2 = taisyutsuHm2;
                NippouData.SyukkinHm3 = syukkinHm3;
                NippouData.TaisyutsuHm3 = taisyutsuHm3;
            }
            else
            {
                // 上記以外の場合、勤怠打刻を元に編集する。

                bool night = false;
                bool lateNight = false;
                bool earlyMorning = false;
                bool refresh = false;
                bool refreshDay = false;

                List<InquiryType> inquries = new List<InquiryType>();

                var ApprovedUkagai = ukagaiHeaders.Where(row => row.Status == 承認).ToList();
                foreach (UkagaiHeader ukagai in ApprovedUkagai)
                {

                    var ukagaiShinseis = ukagai
                        .UkagaiShinseis.ToList();
                    foreach (var type in ukagaiShinseis)
                    {
                        if (!inquries.Contains(type.UkagaiSyubetsu)) inquries.Add(type.UkagaiSyubetsu);
                    }
                }

                // 勤怠属性.コードが、3:フリー、5:標準社員外の場合、true
                if (syainInfo.KintaiZokuseiId is (int)EmployeeWorkType.フリー or (int)EmployeeWorkType.標準社員外)
                {
                    night = true;
                    lateNight = true;
                    earlyMorning = true;
                    refresh = true;
                }
                else
                {
                    // 指示情報に承認済みの5:夜間作業が存在する場合true
                    if (inquries.Contains(夜間作業))
                        night = true;

                    // 2 for late-night
                    if (inquries.Contains(深夜作業))
                        lateNight = true;

                    // 3 for early-morning
                    if (inquries.Contains(早朝作業))
                        earlyMorning = true;

                    // 4 for refresh
                    if (inquries.Contains(リフレッシュデー残業))
                        refresh = true;
                }

                // 5 for refresh day
                // 論理が誤っていたため修正：水曜または金曜、または休業日のリフレッシュフラグが対象
                if (jissekiDate.DayOfWeek == DayOfWeek.Wednesday || jissekiDate.DayOfWeek == DayOfWeek.Friday
                    || (holiday != null && holiday.RefreshDay == RefreshDayFlag.リフレッシュデー))
                {
                    refreshDay = true;
                }

                string? syukkinHm;
                string? taisyutsuHm;
                if (inquries.Contains(早朝作業))
                {
                    var ukagaiMorning = ukagaiHeaders.FirstOrDefault(u =>
                    u.UkagaiShinseis.Any(us => us.UkagaiSyubetsu == 早朝作業)
                    && u.Status == 承認);

                    for (int i = 0; i < Math.Min(3, workingHours.Count); i++)
                    {
                        var wh = workingHours[i];

                        (syukkinHm, taisyutsuHm) = TimeCalculator.Hosei(wh?.SyukkinTime?.ToString("HHmm") ?? "",
                           wh?.TaikinTime?.ToString("HHmm") ?? "",
                           night, lateNight, earlyMorning, refresh, refreshDay,
                           Some(ukagaiMorning?.KaishiJikoku?.ToString("HHmm") ?? "")
                           );

                        switch (i)
                        {
                            case 0:
                                (NippouData.SyukkinHm1, NippouData.TaisyutsuHm1) = (FromStringToTimeOnly(syukkinHm), FromStringToTimeOnly(taisyutsuHm));
                                break;
                            case 1:
                                (NippouData.SyukkinHm2, NippouData.TaisyutsuHm2) = (FromStringToTimeOnly(syukkinHm), FromStringToTimeOnly(taisyutsuHm));
                                break;
                            case 2:
                                (NippouData.SyukkinHm3, NippouData.TaisyutsuHm3) = (FromStringToTimeOnly(syukkinHm), FromStringToTimeOnly(taisyutsuHm));
                                break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Math.Min(3, workingHours.Count); i++)
                    {
                        var wh = workingHours[i];

                        (syukkinHm, taisyutsuHm) = TimeCalculator.Hosei(wh?.SyukkinTime?.ToString("HHmm") ?? "",
                            wh?.TaikinTime?.ToString("HHmm") ?? "",
                            night, lateNight, earlyMorning, refresh, refreshDay,
                            LanguageExt.Prelude.None
                            );

                        switch (i)
                        {
                            case 0:
                                (NippouData.SyukkinHm1, NippouData.TaisyutsuHm1) = (FromStringToTimeOnly(syukkinHm), FromStringToTimeOnly(taisyutsuHm));
                                break;
                            case 1:
                                (NippouData.SyukkinHm2, NippouData.TaisyutsuHm2) = (FromStringToTimeOnly(syukkinHm), FromStringToTimeOnly(taisyutsuHm));
                                break;
                            case 2:
                                (NippouData.SyukkinHm3, NippouData.TaisyutsuHm3) = (FromStringToTimeOnly(syukkinHm), FromStringToTimeOnly(taisyutsuHm));
                                break;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 問題がある場合は通知メッセージを送信します
        /// </summary>
        /// <param name="jissekiDate">実績年月日</param>
        /// <param name="nippou">日報</param>
        /// <param name="syainData">社員</param>
        /// <param name="isWorkDay">営業日ですか</param>
        /// <param name="applicationConfig">app config</param>
        /// <param name="ukagaiHeader">うかがいへーダ</param>
        private async Task<string?> CheckForNotificationMessageAsync(NippouViewModel nippou, Syain syainData, List<WorkingHour> workingHours, bool isWorkDay, ApplicationConfig applicationConfig, List<UkagaiHeader> ukagaiHeaders, DateOnly jissekiDate)
        {
            // 状態 : 1 アプリConfig.日報停止日＜＝INパラメータ.実績年月日　の場合
            if (applicationConfig.NippoStopDate <= jissekiDate)
            {
                ConfirmButton = false;
                return "決算運用のため１０月中旬までの期間、確定確定を停止しています。";
            }

            // 状態 : 2   面表示している出退勤時間1～3の何れかが以下の条件に該当するような打刻漏れがある場合。

            bool hasMissingTime = nippou.SyukkinHm1.HasValue ^ nippou.TaisyutsuHm1.HasValue ||
            nippou.SyukkinHm2.HasValue ^ nippou.TaisyutsuHm2.HasValue ||
            nippou.SyukkinHm3.HasValue ^ nippou.TaisyutsuHm3.HasValue;

            if (hasMissingTime)
            {
                ConfirmButton = false;
                return "出退勤の打刻漏れがあります。打刻の修正を行ってください。";
            }

            var ukagaiHeader = ukagaiHeaders.FirstOrDefault(u => u.LastShoninYmd.HasValue);

            // 状態 : 3  伺い申請が必要な勤怠属性の社員で、以下の場合
            List<string> messageParameter = new List<string>();
            if (syainData.KintaiZokuseiId != (int)EmployeeWorkType.フリー && syainData.KintaiZokuseiId != (int)EmployeeWorkType.標準社員外)
            {
                bool hasWorkTime = nippou.SyukkinHm1.HasValue || nippou.TaisyutsuHm1.HasValue ||
                    nippou.SyukkinHm2.HasValue || nippou.TaisyutsuHm2.HasValue ||
                    nippou.SyukkinHm3.HasValue || nippou.TaisyutsuHm3.HasValue;

                // 状態  : 3 - 1  休日出勤の申請情報が存在しない又は存在するが未承認の場合。
                bool hasApprovedHolidayWork = ukagaiHeader != null && HasApprovedInquiry(InquiryType.休日出勤, ukagaiHeader);
                if (!isWorkDay && hasWorkTime && !hasApprovedHolidayWork)
                {
                    ConfirmButton = false;
                    messageParameter.Add("休日出勤");
                }


                // 状態 : 3 - 2   夜間作業の申請情報が存在しない又は存在するが未承認の場合。
                bool hasApprovedNightWork = ukagaiHeader != null && HasApprovedInquiry(夜間作業, ukagaiHeader);
                int nightMinute = TimeCalculator.GetIncludeTimeWithout休憩(nippou.SyukkinHm1?.ToString("HHmm") ?? "", nippou.TaisyutsuHm1?.ToString("HHmm") ?? "", (0, 5 * 60))
                    + TimeCalculator.GetIncludeTimeWithout休憩(nippou.SyukkinHm2?.ToString("HHmm") ?? "", nippou.TaisyutsuHm2?.ToString("HHmm") ?? "", (0, 5 * 60))
                    + TimeCalculator.GetIncludeTimeWithout休憩(nippou.SyukkinHm3?.ToString("HHmm") ?? "", nippou.TaisyutsuHm3?.ToString("HHmm") ?? "", (0, 5 * 60));
                if (nightMinute != 0 && !hasApprovedNightWork)
                {
                    ConfirmButton = false;
                    messageParameter.Add("夜間作業");
                }

                // 状態 : 3 - 3    早朝作業の申請情報が存在しない又は存在するが未承認の場合。
                bool hasApprovedMorning = ukagaiHeader != null && HasApprovedInquiry(早朝作業, ukagaiHeader);
                int morningMinute = TimeCalculator.GetIncludeTimeWithout休憩(nippou.SyukkinHm1?.ToString("HHmm") ?? "", nippou.TaisyutsuHm1?.ToString("HHmm") ?? "", (5 * 60, 8 * 60 + 30))
                    + TimeCalculator.GetIncludeTimeWithout休憩(nippou.SyukkinHm2?.ToString("HHmm") ?? "", nippou.TaisyutsuHm2?.ToString("HHmm") ?? "", (5 * 60, 8 * 60 + 30))
                    + TimeCalculator.GetIncludeTimeWithout休憩(nippou.SyukkinHm3?.ToString("HHmm") ?? "", nippou.TaisyutsuHm3?.ToString("HHmm") ?? "", (5 * 60, 8 * 60 + 30));

                if (morningMinute != 0 && !hasApprovedMorning)
                {
                    ConfirmButton = false;
                    messageParameter.Add("早朝作業");
                }


                // 状態 : 3 - 4    深夜作業の申請情報が存在しない又は存在するが未承認の場合。
                bool hasApprovedLateNight = ukagaiHeader != null && HasApprovedInquiry(深夜作業, ukagaiHeader);

                int lateNightMinute = TimeCalculator.GetIncludeTimeWithout休憩(nippou.SyukkinHm1?.ToString("HHmm") ?? "", nippou.TaisyutsuHm1?.ToString("HHmm") ?? "", (22 * 60, 24 * 60))
                    + TimeCalculator.GetIncludeTimeWithout休憩(nippou.SyukkinHm2?.ToString("HHmm") ?? "", nippou.TaisyutsuHm2?.ToString("HHmm") ?? "", (22 * 60, 24 * 60))
                    + TimeCalculator.GetIncludeTimeWithout休憩(nippou.SyukkinHm3?.ToString("HHmm") ?? "", nippou.TaisyutsuHm3?.ToString("HHmm") ?? "", (22 * 60, 24 * 60));

                if (lateNightMinute != 0 && !hasApprovedLateNight)
                {
                    ConfirmButton = false;
                    messageParameter.Add("深夜作業");
                }
                if (messageParameter.Count != 0)
                {
                    return $"{String.Join("・", messageParameter)}の指示がでていない、又は承認されていません。";
                }
            }
            else
            {
                if (ukagaiHeader == null) return null;
                // 状態 : 4
                bool hasApprovalMorning = HasApprovedInquiry(早朝作業, ukagaiHeader);

                if (!hasApprovalMorning)
                {
                    messageParameter.Add("早朝作業");
                }
                bool hasApprovedRefresh = HasApprovedInquiry(リフレッシュデー残業, ukagaiHeader);

                if (!hasApprovedRefresh)
                {
                    messageParameter.Add("リフレッシュデー残業");
                }
                if (messageParameter.Count != 0)
                {
                    return $"{String.Join("・", messageParameter)}の指示を申請中ですが、最終承認されていません。";
                }
            }
            if (ukagaiHeader == null) return null;

            // 状態 : 5  打刻時間修正
            bool hasApprovedShusei = HasApprovedInquiry(打刻時間修正, ukagaiHeader);
            if (!hasApprovedShusei && workingHours.Any(row => row.Edited == true))
            {
                return "打刻時間修正の指示を申請中ですが、最終承認されていません。";
            }

            // 状態 : 6 残業時間制限メッセージ
            bool hasApprovedOvertime = HasApprovedInquiry(時間外労働時間制限拡張, ukagaiHeader);
            bool hasOvertime = await HasExceedOverTimeAsync(syainData, isWorkDay, jissekiDate);
            if (hasOvertime && jissekiDate == jissekiDate.GetEndOfMonth() && !hasApprovedOvertime)
            {
                return "残業時間が制限時間を超えますが、時間外労働時間拡張申請が未申請、または最終承認されていないため確定できません。";
            }

            return null;
        }


        /// <summary>
        /// 確定ボタンの表示が有効かどうか
        /// </summary>
        /// <param name="nippou">日報</param>
        private async Task CheckConfirmButtonAsync(Nippou? nippou, long loginBaseId, Syain syainEntity, DateOnly jissekiDate, bool isDairiInput)
        {
            if (nippou != null && nippou.TourokuKubun == 確定保存)
            {
                ConfirmButton = false;
            }

            var previousDay = await _context.Nippous.Where(row => row.SyainId == syainEntity.Id
            && row.NippouYmd < jissekiDate
            && row.TourokuKubun == 確定保存)
                .OrderByDescending(row => row.NippouYmd).FirstOrDefaultAsync();

            if (previousDay == null)
            {
                ConfirmButton = false;
            }

            if (isDairiInput == false && loginBaseId != syainEntity.SyainBaseId)
            {
                ConfirmButton = false;
            }

        }


        /// <summary>
        /// 確定解除ボタン
        /// </summary>
        /// <param name="nippou">日報</param>
        private async Task CheckUnconfirmButtonAsync(Nippou? nippou, DateOnly jissekiDate, long loginBaseId, long paramBaseId, bool isDairiInput)
        {
            if (nippou == null || nippou.TourokuKubun == 一時保存)
            {
                UnconfirmButton = false;
            }
            if (nippou?.KakuteiYmd != DateOnly.FromDateTime(DateTime.Today))
            {
                UnconfirmButton = false;
            }
            if (isDairiInput == false && loginBaseId != paramBaseId)
            {
                UnconfirmButton = false;
            }
        }


        /// <summary>
        /// 一時保存ボタンボタンの表示が有効かどうか
        /// </summary>
        /// <param name="nippou">日報</param>
        private async Task CheckTemporarySaveButtonAsync(Nippou? nippou, long loginBaseId, long paramBaseId, bool isDairiInput)
        {
            if (nippou != null && nippou.TourokuKubun == 確定保存)
            {
                TemporarySaveButton = false;
            }
            if (isDairiInput == false && loginBaseId != paramBaseId)
            {
                TemporarySaveButton = false;
            }

        }


        /// <summary>
        /// 日報実績が存在する場合の出勤区分
        /// </summary>
        /// <param name="isWorkday">営業日ですか</param>
        private async Task GetKubunDataAsync(bool isWorkday, DateOnly jissekiDate, Syain syainInfo)
        {
            var remainingCompensatory = await GetRemainingNumberOfCompensatoryLeaveAsync(syainInfo.Id, jissekiDate);
            var remainingPaid = await GetRemainNumberOfPaidVacationAsync(syainInfo.SyainBaseId);

            // 9-1)
            if (NippouData.TotalWorkingHoursInMinute <= 0)
            {

                if (!isWorkday)
                {
                    // 1 非稼働日の場合
                    NippouData.SyukkinKubun1 = 休日;
                    NippouData.SyukkinKubun2 = AttendanceClassification.None;
                    return;
                }
                else
                {
                    // 2-3 振替休暇の残日数＝0.5日の場合
                    if (remainingCompensatory == 0.5m)
                    {
                        if (remainingPaid >= 0.5m)
                        {
                            // 2-3-1 有給休暇の残日数＞0.5日の場合
                            // 2-3-2 有給休暇の残日数＝0.5日の場合
                            NippouData.SyukkinKubun1 = 半日振休;
                            NippouData.SyukkinKubun2 = 半日有給;
                            return;
                        }
                        else if (remainingPaid == 0)
                        {
                            // 2-3-3 有給休暇の残日数＝0日の場合
                            NippouData.SyukkinKubun1 = 半日振休;
                            NippouData.SyukkinKubun2 = 欠勤;
                            return;
                        }
                        else
                        {
                            // 2-3-4 上記以外
                            NippouData.SyukkinKubun1 = AttendanceClassification.None;
                            NippouData.SyukkinKubun2 = AttendanceClassification.None;
                            return;
                        }
                    }

                    // 2-4 振替休暇の残日数＝0日の場合
                    else if (remainingCompensatory == 0)
                    {

                        if (remainingPaid > 0.5m)
                        {
                            // 2-4-1 有給休暇の残日数＞0.5日の場合
                            NippouData.SyukkinKubun1 = 年次有給休暇_1日;
                            NippouData.SyukkinKubun2 = AttendanceClassification.None;
                            return;
                        }
                        else if (remainingPaid == 0.5m)
                        {
                            // 2-4-2 有給休暇の残日数＝0.5日の場合
                            NippouData.SyukkinKubun1 = 半日有給;
                            NippouData.SyukkinKubun2 = 欠勤;
                            return;
                        }
                        else if (remainingPaid == 0)
                        {
                            // 2-4-3 有給休暇の残日数＝0日の場合
                            NippouData.SyukkinKubun1 = 欠勤;
                            NippouData.SyukkinKubun2 = AttendanceClassification.None;
                            return;
                        }
                        else
                        {
                            // 2-4-4 上記以外
                            NippouData.SyukkinKubun1 = AttendanceClassification.None;
                            NippouData.SyukkinKubun2 = AttendanceClassification.None;
                            return;
                        }
                    }
                    else
                    {
                        // 2-5
                        NippouData.SyukkinKubun1 = AttendanceClassification.None;
                        NippouData.SyukkinKubun2 = AttendanceClassification.None;
                        return;
                    }
                }

            }
            else
            {
                //  9-2) 上記以外 ＆　非稼働日 の場合
                if (!isWorkday)
                {
                    NippouData.SyukkinKubun1 = AttendanceClassification.休日出勤;
                    NippouData.SyukkinKubun2 = AttendanceClassification.None;
                    return;
                }

            }

            // 9-3) 勤怠属性＝6：パート　の場合
            if (syainInfo.KintaiZokuseiId == (int)EmployeeWorkType.パート)
            {
                NippouData.SyukkinKubun1 = パート勤務;
                NippouData.SyukkinKubun2 = AttendanceClassification.None;
                return;
            }
            if (NippouData.TotalWorkingHoursInMinute > 240)
            {
                // 9-4) 4ｘ60＜画面.総労働時間　の場合
                NippouData.SyukkinKubun1 = 通常勤務;
                NippouData.SyukkinKubun2 = AttendanceClassification.None;
                return;
            }

            // 9-5) 画面.総労働時間＜＝4ｘ60　の場合
            else if (NippouData.TotalWorkingHoursInMinute <= 240)
            {
                NippouData.SyukkinKubun1 = 半日勤務;

                var yukyuu = await FetchYuukyuuZanDataAsync(syainInfo.SyainBaseId);
                if (remainingCompensatory >= 0.5m)
                {
                    // 4 振替休暇の残日数＞＝0.5日の場合
                    NippouData.SyukkinKubun2 = 半日振休;
                }
                else
                {
                    // 5-1 有給休暇の残日数＞0.5日の場合
                    if (remainingPaid > 0.5m)
                    {
                        if (yukyuu != null && yukyuu.HannitiKaisuu >= 10)
                        {
                            // 5-1-1 半日有給休暇の上限10回＜＝有給休暇残日数.半日有給取得数　の場合
                            NippouData.SyukkinKubun2 = 年次有給休暇_1日;
                        }
                        else
                        {
                            // 5-1-2 上記以外
                            NippouData.SyukkinKubun2 = 半日有給;
                        }
                    }
                    // 5-2 有給休暇の残日数＝0.5日の場合
                    else if (remainingPaid == 0.5m)
                    {

                        if (yukyuu != null && yukyuu.HannitiKaisuu < 10)
                        {
                            // 5-2-1 有給休暇残日数.半日有給取得数＜半日有給休暇の上限10回　の場合
                            NippouData.SyukkinKubun2 = 半日有給;

                        }
                        else
                        {
                            // 5-2-2 上記以外
                            NippouData.SyukkinKubun2 = 欠勤;

                        }
                    }
                    else
                    {
                        // 5- 3 上記以外
                        NippouData.SyukkinKubun2 = 欠勤;

                    }
                }

                if (!SyukkinKubun2List.Any(x => x.Value == NippouData.SyukkinKubunCodeString2))
                {
                    NippouData.SyukkinKubun2 = AttendanceClassification.None;
                    return;
                }

                return;
            }
        }


        /// <summary>
        /// 前回の日報からのコピー
        /// </summary>
        /// <param name="syainId">社員番号</param>
        /// <param name="jissekiDate">実績年月日</param>
        public async Task<IActionResult> OnPostCopyFromLastDateAsync(long syainId, DateOnly jissekiDate)
        {
            ModelState.Clear();

            var nippouList = await _context.Nippous.Where(row => row.SyainId == syainId && row.NippouYmd < jissekiDate)
                .Include(p => p.SyukkinKubunId1Navigation).Include(p => p.SyukkinKubunId2Navigation)
                .OrderByDescending(row => row.NippouYmd).ToListAsync();

            if (!nippouList.Any())
            {
                return SuccessJson(data: null);
            }

            var nippou = nippouList.FirstOrDefault(n => n.SyukkinKubunId1Navigation.IsSyukkin == true || n?.SyukkinKubunId2Navigation?.IsSyukkin == true);
            if (nippou == null)
            {
                return SuccessJson(data: null);
            }

            await LoadBumonProcessListAsync();

            var yesterdayNippouAnken = await _context.NippouAnkens
                .Where(row => row.NippouId == nippou.Id
                    && row.Ankens != null
                    && row.Ankens.KingsJuchu != null
                    && row.Ankens.KingsJuchu.IsGenkaToketu == false)
                .Include(row => row.Ankens)
                    .ThenInclude(row => row.KingsJuchu)
                .ToListAsync();

            NippouAnkenCards.NippouAnkens.AddRange(
                yesterdayNippouAnken.Select(item => new NippouAnkenViewModel
                {
                    IsLinked = item.IsLinked,
                    KingsJuchuNo = item.Ankens?.KingsJuchu?.KingsJuchuNo,
                    AnkensId = item.AnkensId,
                    AnkenName = item.AnkenName,
                    ChaYmd = item.Ankens?.KingsJuchu?.ChaYmd,
                    JuchuuNo = item.Ankens?.KingsJuchu?.JuchuuNo,
                    KokyakuKaisyaId = item.KokyakuKaisyaId,
                    KokyakuName = item.KokyakuName,
                    BumonProcessId = item.BumonProcessId,
                })
            );

            var modelView = await PartialToJsonAsync("_IndexPartial", NippouAnkenCards);
            return SuccessJson(data: modelView);
        }


        /// <summary>
        /// 新しい日報案件を追加
        /// </summary>
        /// <param name="nippouAnkenCards">日報案件</param>
        public async Task<IActionResult> OnPostAddNippouAnkenAsync()
        {
            await LoadBumonProcessListAsync();

            NippouAnkenCards.NippouAnkens.Add(new NippouAnkenViewModel());
            var data = await PartialToJsonAsync("_IndexPartial", NippouAnkenCards);
            return SuccessJson(data: data);

        }


        /// <summary>
        /// 報案件のコピー
        /// </summary>
        /// <param name="index">card　のindex no.</param>
        public async Task<IActionResult> OnPostCopyNippouAnkenAsync(int index)
        {
            await LoadBumonProcessListAsync();
            NippouAnkenViewModel itemToCopy = NippouAnkenCards.NippouAnkens[index];
            if (itemToCopy.IsGenkaToketu == true)
            {
                NippouAnkenCards.NippouAnkens.Add(new NippouAnkenViewModel
                {
                    IsLinked = itemToCopy.IsLinked,
                    IsGenkaToketu = itemToCopy.IsGenkaToketu,
                    KokyakuKaisyaId = itemToCopy.KokyakuKaisyaId,
                    KokyakuName = itemToCopy.KokyakuName,
                    BumonProcessId = itemToCopy.BumonProcessId,
                });
            }
            else
            {
                NippouAnkenCards.NippouAnkens.Add(new NippouAnkenViewModel
                {
                    IsLinked = itemToCopy.IsLinked,
                    IsGenkaToketu = itemToCopy.IsGenkaToketu,
                    KingsJuchuNo = itemToCopy.KingsJuchuNo,
                    AnkensId = itemToCopy.AnkensId,
                    AnkenName = itemToCopy.AnkenName,
                    ChaYmd = itemToCopy.ChaYmd,
                    JuchuuNo = itemToCopy.JuchuuNo,
                    KokyakuKaisyaId = itemToCopy.KokyakuKaisyaId,
                    KokyakuName = itemToCopy.KokyakuName,
                    BumonProcessId = itemToCopy.BumonProcessId,
                });
            }
            var data = await PartialToJsonAsync("_IndexPartial", NippouAnkenCards);
            return SuccessJson(data: data);
        }


        /// <summary>
        /// 日報案件を消去
        /// </summary>
        /// <param name="index">card　のindex no.</param>
        /// <param name="nippouAnkenCards">日報案件Model</param>
        public async Task<IActionResult> OnPostDeleteNippouAnkenAsync(int index)
        {
            await LoadBumonProcessListAsync();

            NippouAnkenCards.NippouAnkens.RemoveAt(index);
            ModelState.Clear();
            var data = await PartialToJsonAsync("_IndexPartial", NippouAnkenCards);
            return SuccessJson(data: data);
        }


        /// <summary>
        /// 確定解除が有効かどうか
        /// </summary>
        /// <param name="syainId">社員の番号</param>
        /// <param name="jissekiDate">実績年月日</param>
        public async Task<IActionResult> OnGetCancelConfirmValidateAsync(long syainId, DateOnly jissekiDate)
        {
            var nippou = await FetchNippouDataAsync(syainId, jissekiDate);
            if (nippou == null)
            {
                return ErrorJson(ErrorNippouDeleted);
            }

            if (nippou.IsRendouZumi == true)
            {
                return ErrorJson(ErrorNippouAccountLinked);
            }
            var latestConfirmedNippou = await _context.Nippous.Where(row => row.SyainId == syainId && row.TourokuKubun == 確定保存)
                .OrderByDescending(row => row.NippouYmd).FirstOrDefaultAsync();

            if (latestConfirmedNippou != null && latestConfirmedNippou.NippouYmd != nippou.NippouYmd)
            {
                return ErrorJson(ErrorNippouSubsequentConfirmed);
            }
            return SuccessJson();
        }


        /// <summary>
        /// 確定解除
        /// </summary>
        /// <param name="syainId">社員の番号</param>
        /// <param name="jissekiDate">実績年月日</param>
        /// <param name="isDairiInput">代理</param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostCancelConfirmAsync(long syainId, DateOnly jissekiDate, bool isDairiInput, long syainBaseId)
        {
            try
            {

                bool isWorkDay = await IsWorkingWeekDayAsync(jissekiDate);
                CompensatoryPaidLeave leave = new(jissekiDate, syainId, NippouData, isWorkDay, _context, FuriYoteiDate, _optionsAccessor.Value);
                await leave.UpdateCancelConfirmLeaveAsync(syainBaseId, jissekiDate);

                var nippou = await _context.Nippous.Where(row => row.SyainId == syainId && row.NippouYmd == jissekiDate)
                    .FirstOrDefaultAsync();

                if (nippou == null)
                {
                    return ErrorJson(Const.EmptyReadData);
                }

                nippou.TourokuKubun = 一時保存;

                if (jissekiDate == jissekiDate.GetEndOfMonth())
                {
                    var ukagaiHeader = await _context.UkagaiHeaders
                    .Where(row => jissekiDate.GetStartOfMonth() < row.WorkYmd && row.WorkYmd < jissekiDate.GetEndOfMonth())
                    .Where(row => row.SyainId == syainId &&
                    row.Invalid == true &&
                    row.UkagaiShinseis.Any(s => s.UkagaiSyubetsu == 時間外労働時間制限拡張))
                    .FirstOrDefaultAsync();

                    if (ukagaiHeader != null)
                    {
                        ukagaiHeader.Invalid = false;
                    }

                }

                if (isDairiInput)
                {
                    DairiNyuryokuRireki dairiHistory = new DairiNyuryokuRireki
                    {
                        DairiNyuryokuSyainId = LoginInfo.User.Id,
                        DairiNyuryokuTime = DateTime.Now,
                        NippouId = nippou.Id,
                        NippouSousa = DailyReportOperation.確定解除,
                        Invalid = false
                    };
                    await _context.DairiNyuryokuRirekis.AddAsync(dairiHistory);
                }

                await _context.SaveChangesAsync();



                return SuccessJson();

            }
            catch (Exception ex)
            {
                return ErrorJson(ErrorInternalServer);
            }
        }


        /// <summary>
        /// 一時保存
        /// </summary>
        /// <param name="syainId">社員の番号</param>
        /// <param name="jissekiDate">実績年月日</param>
        /// <param name="isDairiInput">代理</param>
        public async Task<IActionResult> OnPostTemporarySaveAsync(long syainId, DateOnly jissekiDate, bool isDairiInput)
        {

            try
            {
                bool isWorkDay = await IsWorkingWeekDayAsync(jissekiDate);
                var syainDetail = await FetchSyainDataAsync(syainId);
                if (syainDetail == null)
                {
                    return ErrorJson(ErrorEmployeeNotFound);
                }

                // 入力チェック
                ConfirmValidation validate = new ConfirmValidation(syainDetail, jissekiDate, NippouData, isWorkDay, FuriYoteiDate, _context, ModelState);
                await validate.TemporarySaveValidationAsync();



                if (!ModelState.IsValid)
                {
                    var messages = ModelState.Where(x => x.Value?.Errors.Count > 0)
                                 .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                                 .ToList();
                    return new JsonResult(new
                    {
                        status = エラー,
                        message = string.Join("\n", messages)
                    });
                }

                // 3 日報実績登録
                var nippou = await _context.Nippous
                .Where(row => row.SyainId == syainId && row.NippouYmd == jissekiDate).FirstOrDefaultAsync();

                long? syukkinId1 = await GetIdOfKubunByCodeAsync(NippouData.SyukkinKubunCodeString1);
                long? syukkinId2 = await GetIdOfKubunByCodeAsync(NippouData.SyukkinKubunCodeString2);

                // 日報の詳細な勤務時間を取得する
                await GetNippouDetailWorkHoursAsync(isWorkDay, jissekiDate);

                if (nippou == null)
                {
                    // 日報を作成
                    nippou = new Nippou
                    {
                        SyainId = syainId,
                        NippouYmd = jissekiDate,
                        Youbi = GetYoubi(jissekiDate),
                        SyukkinHm1 = NippouData.SyukkinHm1,
                        TaisyutsuHm1 = NippouData.TaisyutsuHm1,
                        SyukkinHm2 = NippouData.SyukkinHm2,
                        TaisyutsuHm2 = NippouData.TaisyutsuHm2,
                        SyukkinHm3 = NippouData.SyukkinHm3,
                        TaisyutsuHm3 = NippouData.TaisyutsuHm3,
                        HJitsudou = NippouData.HJitsudou,
                        HZangyo = NippouData.HZangyo,
                        HWarimashi = NippouData.HWarimashi,
                        HShinyaZangyo = NippouData.HShinyaZangyo,
                        DJitsudou = NippouData.DJitsudou,
                        DZangyo = NippouData.DZangyo,
                        DWarimashi = NippouData.DWarimashi,
                        DShinyaZangyo = NippouData.DShinyaZangyo,
                        NJitsudou = NippouData.NJitsudou,
                        NShinya = NippouData.NShinya,
                        TotalZangyo = NippouData.TotalZangyo,
                        KaisyaCode = (NippousCompanyCode)syainDetail.KaisyaCode,
                        //KintaiZokuseiCode = (short)syainDetail.KintaiZokusei.Code,
                        IsRendouZumi = false,
                        RendouYmd = null,
                        TourokuKubun = 一時保存,
                        KakuteiYmd = DateOnly.FromDateTime(DateTime.Today),
                        SyukkinKubunId1 = (long)syukkinId1,
                        SyukkinKubunId2 = syukkinId2,

                    };
                    await _context.Nippous.AddAsync(nippou);

                    await _context.SaveChangesAsync();

                }
                else
                {
                    //日報データを更新
                    nippou.SyainId = syainId;
                    nippou.NippouYmd = jissekiDate;
                    nippou.Youbi = GetYoubi(jissekiDate);
                    nippou.SyukkinHm1 = NippouData.SyukkinHm1;
                    nippou.TaisyutsuHm1 = NippouData.TaisyutsuHm1;
                    nippou.SyukkinHm2 = NippouData.SyukkinHm2;
                    nippou.TaisyutsuHm2 = NippouData.TaisyutsuHm2;
                    nippou.SyukkinHm3 = NippouData.SyukkinHm3;
                    nippou.TaisyutsuHm3 = NippouData.TaisyutsuHm3;
                    nippou.HJitsudou = NippouData.HJitsudou;
                    nippou.HZangyo = NippouData.HZangyo;
                    nippou.HWarimashi = NippouData.HWarimashi;
                    nippou.HShinyaZangyo = NippouData.HShinyaZangyo;
                    nippou.DJitsudou = NippouData.DJitsudou;
                    nippou.DZangyo = NippouData.DZangyo;
                    nippou.DWarimashi = NippouData.DWarimashi;
                    nippou.DShinyaZangyo = NippouData.DShinyaZangyo;
                    nippou.NJitsudou = NippouData.NJitsudou;
                    nippou.NShinya = NippouData.NShinya;
                    nippou.TotalZangyo = NippouData.TotalZangyo;
                    nippou.KaisyaCode = (NippousCompanyCode)syainDetail.KaisyaCode;
                    //nippou.KintaiZokuseiCode = (short)syainDetail.KintaiZokusei.Code;
                    nippou.IsRendouZumi = false;
                    nippou.RendouYmd = null;
                    nippou.TourokuKubun = 一時保存;
                    nippou.KakuteiYmd = DateOnly.FromDateTime(DateTime.Today);
                    nippou.SyukkinKubunId1 = (long)syukkinId1;
                    nippou.SyukkinKubunId2 = syukkinId2;

                    // 実績入力（明細）削除
                    await _context.NippouAnkens
                        .Where(x => x.NippouId == nippou.Id)
                        .ExecuteDeleteAsync();

                    await _context.SaveChangesAsync();
                }

                // 実績入力（明細）登録
                var rows = NippouAnkenCards.NippouAnkens.Select(x => new NippouAnken
                {
                    NippouId = nippou!.Id,
                    AnkensId = x.AnkensId!.Value,
                    KokyakuName = x.KokyakuName ?? "",
                    AnkenName = x.AnkenName ?? "",
                    JissekiJikan = x.JissekiJikan ?? 0,
                    KokyakuKaisyaId = x.KokyakuKaisyaId!.Value,
                    BumonProcessId = x.BumonProcessId,
                    IsLinked = x.IsLinked
                });


                _context.NippouAnkens.AddRange(rows);
                await _context.SaveChangesAsync();

                return SuccessJson();

            }
            catch (Exception ex)
            {
                return ErrorJson(ErrorInternalServer);
            }
        }


        /// <summary>
        /// 確定 check
        /// </summary>
        /// <param name="syainId">社員の番号</param>
        /// <param name="jissekiDate">実績年月日</param>
        /// <param name="isDairiInput">代理</param>
        public async Task<IActionResult> OnPostFinalConfirmCheckAsync(long syainId, DateOnly jissekiDate, bool isDairiInput)
        {
            if (NippouAnkenCards.NippouAnkens.Any(row => row.JissekiJikan == 0))
            {
                ModelState.AddModelError(string.Empty, string.Format(Const.ErrorRequired, "実績時間"));
            }
            if (NippouAnkenCards.NippouAnkens.Any(row => string.IsNullOrEmpty(row.KingsJuchuNo)))
            {
                ModelState.AddModelError(string.Empty, string.Format(Const.ErrorRequired, "受注番号"));
            }

            if (!ModelState.IsValid)
            {
                var messages = ModelState.Where(x => x.Value?.Errors.Count > 0)
                             .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                             .ToList();

                return ErrorJson(string.Join("\n", messages));
            }

            var syainInfo = await _context.Syains.AsNoTracking().Where(row => row.Id == syainId).FirstOrDefaultAsync();
            foreach (NippouAnkenViewModel item in NippouAnkenCards.NippouAnkens)
            {
                if (item.IsLinked == true && item.AnkensId != null)
                {
                    var anken = await _context.Ankens.AsNoTracking()
                        .Where(row => row.Id == item.AnkensId.Value)
                        .Include(row => row.KingsJuchu)
                        .FirstOrDefaultAsync();

                    if (anken?.KingsJuchu?.BusyoId != syainInfo?.BusyoId)
                    {
                        return SuccessJson(data: "他部署の受注番号が選択されています。\n 確定してよろしいですか？ \n （本日を過ぎると確定を解除出来なくなります）");
                    }
                }
            }

            var hikadoubi = await FetchHolidayDataAsync(jissekiDate);

            if (jissekiDate.DayOfWeek == DayOfWeek.Wednesday || jissekiDate.DayOfWeek == DayOfWeek.Friday
                || hikadoubi?.RefreshDay == RefreshDayFlag.リフレッシュデー)
            {
                List<WorkingHour> workingHours = await FetchWorkingHoursListAsync(syainId, jissekiDate);
                List<UkagaiHeader> ukagai = await FetchUkagaiHeadersListAsync(syainId, jissekiDate);

                bool hasApprovedRefreshDayZangyo = ukagai.Any(u =>
                u.Status == 承認 &&
                    u.UkagaiShinseis.Any(s =>
                        s.UkagaiSyubetsu == リフレッシュデー残業));

                var gamenTimes = new List<TimeOnly?>
                                {
                                    NippouData.TaisyutsuHm1,
                                    NippouData.TaisyutsuHm2,
                                    NippouData.TaisyutsuHm3
                                };

                for (int i = 0; i < workingHours.Count; i++)
                {
                    if (workingHours[i].TaikinTime is DateTime taikinTime)
                    {
                        TimeOnly whTaikinTo = TimeOnly.FromDateTime(taikinTime);
                        if (i >= gamenTimes.Count || gamenTimes[i] == null) continue;

                        if (new TimeOnly(17, 30) <= whTaikinTo && whTaikinTo <= new TimeOnly(22, 0))
                        {
                            TimeSpan diff = whTaikinTo - gamenTimes[i]!.Value;
                            if (diff > TimeSpan.FromMinutes(15) && !hasApprovedRefreshDayZangyo)
                            {
                                return SuccessJson(data: "リフレッシュデーの時間外労働申請が行われていないため、勤務時間を補正します。\n 確定してよろしいですか？ \n （本日を過ぎると確定を解除出来なくなります）");
                            }
                        }
                    }
                }
            }

            if (NippouData.TotalWorkingHoursInMinute < (60 * NippouData.TotalJissekiJikan))
            {
                return SuccessJson(data: "勤務時間と実績の時間合計に差があります。\n 確定してもよろしいですか？ \n （本日を過ぎると確定を解除出来なくなります）");
            }

            return SuccessJson(data: "確定してよろしいですか？\n （本日を過ぎると確定を解除できなくなります）");
        }


        /// <summary>
        /// 確定
        /// </summary>
        /// <param name="syainId">社員の番号</param>
        /// <param name="jissekiDate">実績年月日</param>
        /// <param name="isDairiInput">代理</param>
        public async Task<IActionResult> OnPostFinalConfirmAsync(long syainId, DateOnly jissekiDate, bool isDairiInput)
        {
            try
            {
                var syainDetail = await FetchSyainDataAsync(syainId);
                if (syainDetail == null)
                {
                    return new JsonResult(new
                    {
                        status = エラー,
                    });
                }

                bool isWorkDay = await IsWorkingWeekDayAsync(jissekiDate);

                await GetNippouDetailWorkHoursAsync(isWorkDay, jissekiDate);
                var totalOvertime = await GetTotalOverTimeAsync(syainDetail, jissekiDate);

                ConfirmValidation validate = new ConfirmValidation(syainDetail, jissekiDate, NippouData, isWorkDay, FuriYoteiDate, _context, ModelState);
                await validate.FinalConfirmValidationAsync(NippouAnkenCards.NippouAnkens, totalOvertime);

                if (!ModelState.IsValid)
                {
                    var messages = ModelState.Where(x => x.Value?.Errors.Count > 0)
                                 .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                                 .ToList();

                    return new JsonResult(new
                    {
                        status = エラー,
                        message = string.Join("\n", messages)
                    });
                }

                CompensatoryPaidLeave leave = new(jissekiDate, syainId, NippouData, isWorkDay, _context, FuriYoteiDate, _optionsAccessor.Value);
                await leave.UpdateConfirmLeaveAsync();


                Nippou? nippou = await _context.Nippous
                .Where(row => row.SyainId == syainId && row.NippouYmd == jissekiDate).FirstOrDefaultAsync();

                long? syukkinId1 = await GetIdOfKubunByCodeAsync(NippouData.SyukkinKubunCodeString1);
                long? syukkinId2 = await GetIdOfKubunByCodeAsync(NippouData.SyukkinKubunCodeString2);

                if (nippou == null)
                {
                    // create nippou
                    nippou = new Nippou
                    {
                        SyainId = syainId,
                        NippouYmd = jissekiDate,
                        Youbi = GetYoubi(jissekiDate),
                        SyukkinHm1 = NippouData.SyukkinHm1,
                        TaisyutsuHm1 = NippouData.TaisyutsuHm1,
                        SyukkinHm2 = NippouData.SyukkinHm2,
                        TaisyutsuHm2 = NippouData.TaisyutsuHm2,
                        SyukkinHm3 = NippouData.SyukkinHm3,
                        TaisyutsuHm3 = NippouData.TaisyutsuHm3,
                        HJitsudou = NippouData.HJitsudou,
                        HZangyo = NippouData.HZangyo,
                        HWarimashi = NippouData.HWarimashi,
                        HShinyaZangyo = NippouData.HShinyaZangyo,
                        DJitsudou = NippouData.DJitsudou,
                        DZangyo = NippouData.DZangyo,
                        DWarimashi = NippouData.DWarimashi,
                        DShinyaZangyo = NippouData.DShinyaZangyo,
                        NJitsudou = NippouData.NJitsudou,
                        NShinya = NippouData.NShinya,
                        TotalZangyo = NippouData.TotalZangyo,
                        KaisyaCode = (NippousCompanyCode)syainDetail.KaisyaCode,
                        //KintaiZokuseiCode = (short)syainDetail.KintaiZokusei.Code,
                        IsRendouZumi = false,
                        RendouYmd = null,
                        TourokuKubun = 確定保存,
                        KakuteiYmd = DateOnly.FromDateTime(DateTime.Today),
                        SyukkinKubunId1 = (long)syukkinId1,
                        SyukkinKubunId2 = syukkinId2,
                    };

                    await _context.Nippous.AddAsync(nippou);
                }
                else
                {
                    nippou.SyainId = syainId;
                    nippou.NippouYmd = jissekiDate;
                    nippou.Youbi = GetYoubi(jissekiDate);
                    nippou.SyukkinHm1 = NippouData.SyukkinHm1;
                    nippou.TaisyutsuHm1 = NippouData.TaisyutsuHm1;
                    nippou.SyukkinHm2 = NippouData.SyukkinHm2;
                    nippou.TaisyutsuHm2 = NippouData.TaisyutsuHm2;
                    nippou.SyukkinHm3 = NippouData.SyukkinHm3;
                    nippou.TaisyutsuHm3 = NippouData.TaisyutsuHm3;
                    nippou.HJitsudou = NippouData.HJitsudou;
                    nippou.HZangyo = NippouData.HZangyo;
                    nippou.HWarimashi = NippouData.HWarimashi;
                    nippou.HShinyaZangyo = NippouData.HShinyaZangyo;
                    nippou.DJitsudou = NippouData.DJitsudou;
                    nippou.DZangyo = NippouData.DZangyo;
                    nippou.DWarimashi = NippouData.DWarimashi;
                    nippou.DShinyaZangyo = NippouData.DShinyaZangyo;
                    nippou.NJitsudou = NippouData.NJitsudou;
                    nippou.NShinya = NippouData.NShinya;
                    nippou.TotalZangyo = NippouData.TotalZangyo;
                    nippou.KaisyaCode = (NippousCompanyCode)syainDetail.KaisyaCode;
                    //nippou.KintaiZokuseiCode = (short)syainDetail.KintaiZokusei.Code;
                    nippou.IsRendouZumi = false;
                    nippou.RendouYmd = null;
                    nippou.TourokuKubun = 確定保存;
                    nippou.KakuteiYmd = DateOnly.FromDateTime(DateTime.Today);
                    nippou.SyukkinKubunId1 = (long)syukkinId1;
                    nippou.SyukkinKubunId2 = syukkinId2;


                    var nippouAnkenRowsToDelete = await _context.NippouAnkens
                        .Where(x => x.NippouId == nippou.Id)
                        .ExecuteDeleteAsync();

                }


                var rows = NippouAnkenCards.NippouAnkens.Select(x => new NippouAnken
                {
                    NippouId = nippou!.Id,
                    AnkensId = x.AnkensId!.Value,
                    KokyakuName = x.KokyakuName ?? "",
                    AnkenName = x.AnkenName ?? "",
                    JissekiJikan = x.JissekiJikan ?? 0,
                    KokyakuKaisyaId = x.KokyakuKaisyaId!.Value,
                    BumonProcessId = x.BumonProcessId,
                    IsLinked = x.IsLinked
                });


                _context.NippouAnkens.AddRange(rows);

                // 時間外労働時間制限拡張の伺い申請の無効化
                var overtimeLimit = syainDetail.KintaiZokusei.SeigenTime * 60;

                if (overtimeLimit != 0 && overtimeLimit >= totalOvertime)
                {
                    var overtimeExpansionUkagai = await _context.UkagaiHeaders.Where(
                        row => jissekiDate.GetStartOfMonth() <= row.WorkYmd && row.WorkYmd <= jissekiDate.GetEndOfMonth()
                        && row.SyainId == SyainId && row.UkagaiShinseis.Any(u =>
                        u.UkagaiSyubetsu == 時間外労働時間制限拡張) &&
                        row.Invalid == false).Include(row => row.UkagaiShinseis).FirstOrDefaultAsync();

                    if (overtimeExpansionUkagai != null)
                    {
                        overtimeExpansionUkagai.Invalid = true;
                    }
                }

                //代理入力時の代理入力履歴の登録
                if (isDairiInput)
                {
                    DairiNyuryokuRireki dairiHistory = new DairiNyuryokuRireki
                    {
                        DairiNyuryokuSyainId = LoginInfo.User.Id,
                        DairiNyuryokuTime = DateTime.Now,
                        NippouId = nippou.Id,
                        NippouSousa = (short)DailyReportOperation.確定,
                        Invalid = false
                    };
                    await _context.DairiNyuryokuRirekis.AddAsync(dairiHistory);

                }
                else
                {
                    var history = await _context.DairiNyuryokuRirekis.Where(row =>
                    row.NippouId == nippou.Id && row.NippouSousa == (short)DailyReportOperation.確定
                    && row.Invalid == false).FirstOrDefaultAsync();

                    if (history != null)
                    {
                        history.Invalid = true;
                    }
                }

                await _context.SaveChangesAsync();

                return SuccessJson();
            }
            catch (Exception ex)
            {
                return ErrorJson(ErrorInternalServer);
            }

        }


        // 超過時間の計算
        private async Task<bool> HasExceedOverTimeAsync(Syain syainInfo, bool isWorkDay, DateOnly jissekiDate)
        {
            var overTimeLimit = syainInfo.KintaiZokusei.SeigenTime * 60m;

            var overtimeToCheck = 0m;

            await GetNippouDetailWorkHoursAsync(isWorkDay, jissekiDate);
            overtimeToCheck = await GetTotalOverTimeAsync(syainInfo, jissekiDate);
            if (overTimeLimit != 0 && overTimeLimit < overtimeToCheck)
            {
                return true;
            }

            return false;
        }


        // 残業の計算
        private async Task<decimal> GetTotalOverTimeAsync(Syain syainInfo, DateOnly jissekiDate)
        {
            var totalOvertime = 0m;
            if (syainInfo.KintaiZokusei.IsOvertimeLimit3m == true)
            {
                totalOvertime = await Calculate3MonthZangyoAsync(jissekiDate);
            }
            else
            {
                totalOvertime = await Calculate1MonthZangyoAsync(jissekiDate);
            }
            totalOvertime += NippouData.TotalZangyo;
            return totalOvertime;
        }


        // 3ヶ月残業時間合計
        private async Task<decimal> Calculate3MonthZangyoAsync(DateOnly jissekiDate)
        {
            var firstDayOfPriorTwoMonths = jissekiDate.GetStartOfLastMonth().AddMonths(-1);

            var threeMonthNippous = await _context.Nippous
                .AsNoTracking()
                .Where(r =>
                    r.SyainId == SyainId &&
                    firstDayOfPriorTwoMonths <= r.NippouYmd && r.NippouYmd < jissekiDate)
                .ToListAsync();

            var monthlyTotals = threeMonthNippous
                .GroupBy(r => new { r.NippouYmd.Year, r.NippouYmd.Month })
                .Select(g =>
                {
                    var monthlyTotal = g.Sum(r => r.TotalZangyo) ?? 0m;

                    DateOnly maxDateInMonth = g.Max(r => r.NippouYmd);
                    DateOnly endOfMonth = maxDateInMonth.GetEndOfMonth();
                    bool isMonthFinish = endOfMonth <= jissekiDate;
                    bool hasMonthEndData = maxDateInMonth == endOfMonth;
                    return (monthlyTotal < 0 && isMonthFinish)
                        ? 0
                        : monthlyTotal;
                });
            return monthlyTotals.Sum();
        }


        //  1ヶ月残業時間合計
        private async Task<decimal> Calculate1MonthZangyoAsync(DateOnly jissekiDate)
        {
            var thisMonthNippous = await _context.Nippous.AsNoTracking()
                .Where(row => row.SyainId == SyainId &&
                jissekiDate.GetStartOfMonth() <= row.NippouYmd && row.NippouYmd < jissekiDate)
                .ToListAsync();

            return thisMonthNippous.Sum(r => r.TotalZangyo) ?? 0m;
        }

        // 日報実績の時間算出
        private async Task GetNippouDetailWorkHoursAsync(bool isWorkDay, DateOnly jissekiDate)
        {
            // null 安全な ToString("HHmm") を使うように修正
            int nightMinute = TimeCalculator.GetIncludeTimeWithout休憩(NippouData.SyukkinHm1?.ToString("HHmm") ?? "", NippouData.TaisyutsuHm1?.ToString("HHmm") ?? "", (0, 5 * 60))
                    + TimeCalculator.GetIncludeTimeWithout休憩(NippouData.SyukkinHm2?.ToString("HHmm") ?? "", NippouData.TaisyutsuHm2?.ToString("HHmm") ?? "", (0, 5 * 60))
                    + TimeCalculator.GetIncludeTimeWithout休憩(NippouData.SyukkinHm3?.ToString("HHmm") ?? "", NippouData.TaisyutsuHm3?.ToString("HHmm") ?? "", (0, 5 * 60));
            int lateNightMinute = TimeCalculator.GetIncludeTimeWithout休憩(NippouData.SyukkinHm1?.ToString("HHmm") ?? "", NippouData.TaisyutsuHm1?.ToString("HHmm") ?? "", (22 * 60, 24 * 60))
                    + TimeCalculator.GetIncludeTimeWithout休憩(NippouData.SyukkinHm2?.ToString("HHmm") ?? "", NippouData.TaisyutsuHm2?.ToString("HHmm") ?? "", (22 * 60, 24 * 60))
                    + TimeCalculator.GetIncludeTimeWithout休憩(NippouData.SyukkinHm3?.ToString("HHmm") ?? "", NippouData.TaisyutsuHm3?.ToString("HHmm") ?? "", (22 * 60, 24 * 60));

            int nightOvertime = nightMinute + lateNightMinute;

            if (isWorkDay)
            {
                List<AttendanceClassification> halfDayUnpaidKubun = [非常勤休暇,
                    出産休業, 業務上傷病休業,
                    介護休業, 欠勤];

                if ((NippouData.SyukkinKubun1 == 半日勤務 || NippouData.SyukkinKubun1 == パート勤務)
                    && !halfDayUnpaidKubun.Contains(NippouData.SyukkinKubun2) && NippouData.SyukkinKubun2 != AttendanceClassification.None)
                {
                    if (NippouData.TaisyutsuHm1 <= new TimeOnly(13, 0))
                    {
                        NippouData.HJitsudou += (4 * 60) + 30;
                    }
                    if (new TimeOnly(12, 0) <= NippouData.SyukkinHm1)
                    {
                        NippouData.HJitsudou += (3 * 60) + 30;
                    }
                    if (NippouData.TaisyutsuHm1 - new TimeOnly(13, 0) < new TimeOnly(12, 0) - NippouData.SyukkinHm1)
                    {
                        NippouData.HJitsudou += (4 * 60) + 30;
                    }
                    if (new TimeOnly(12, 0) - NippouData.TaisyutsuHm1 < NippouData.SyukkinHm1 - new TimeOnly(13, 0))
                    {
                        NippouData.HJitsudou += (3 * 60) + 30;
                    }
                    else
                    {
                        NippouData.HJitsudou += 0;
                    }
                }
                else
                {
                    NippouData.HJitsudou += 0;
                }

                NippouData.HZangyo = NippouData.HJitsudou - (8m * 60m);
                NippouData.HWarimashi = TimeCalculator.CalculationWarimashiTime(NippouData.TotalWorkingHoursInMinute, nightOvertime);
                NippouData.HShinyaZangyo = TimeCalculator.CalculationShinyaCyokinTime(NippouData.TotalWorkingHoursInMinute, nightOvertime);
                NippouData.TotalZangyo = NippouData.HZangyo ?? 0m;

            }
            else if (jissekiDate.DayOfWeek == DayOfWeek.Saturday)
            {
                NippouData.DJitsudou = NippouData.TotalWorkingHoursInMinute;
                NippouData.DZangyo = Math.Max(0m, NippouData.TotalWorkingHoursInMinute - (8m * 60m));
                NippouData.DWarimashi = TimeCalculator.CalculationWarimashiTime(NippouData.TotalWorkingHoursInMinute, nightOvertime);
                NippouData.DShinyaZangyo = TimeCalculator.CalculationShinyaCyokinTime(NippouData.TotalWorkingHoursInMinute, nightOvertime);
                NippouData.TotalZangyo = NippouData.TotalWorkingHoursInMinute - (8m * 60m);
            }
            else if (jissekiDate.DayOfWeek == DayOfWeek.Sunday)
            {
                NippouData.NJitsudou = NippouData.TotalWorkingHoursInMinute;
                NippouData.NShinya = nightOvertime;
                NippouData.TotalZangyo = NippouData.TotalWorkingHoursInMinute;
            }

        }


        // stringからTimeOnlyに 変更する
        private static TimeOnly? FromStringToTimeOnly(string timeString)
        {
            return string.IsNullOrWhiteSpace(timeString)
            ? null : TimeOnly.ParseExact(timeString, "HHmm", null);
        }


        // 平日
        private async Task<bool> IsWorkingWeekDayAsync(DateOnly jissekiDate)
        {
            var holiday = await FetchHolidayDataAsync(jissekiDate);

            if (jissekiDate.DayOfWeek != DayOfWeek.Sunday && jissekiDate.DayOfWeek != DayOfWeek.Saturday)
            {
                if (holiday == null || holiday.SyukusaijitsuFlag == HolidayFlag.それ以外)
                    return true;
            }
            return false;
        }


        // 振替休暇の残日数
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


        // 有給休暇の残日数 
        private async Task<decimal> GetRemainNumberOfPaidVacationAsync(long syainBaseId)
        {
            var Yukyuu = await FetchYuukyuuZanDataAsync(syainBaseId);
            if (Yukyuu == null) return 0m;
            var remainingYukyuu = Yukyuu.Wariate + Yukyuu.Kurikoshi - Yukyuu.Syouka;
            return remainingYukyuu;
        }


        // 曜日　number
        private short GetYoubi(DateOnly date)
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
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        // 伺いのステータス
        private bool HasApprovedInquiry(InquiryType inquiryType,
            UkagaiHeader ukagaiHeader)
        {
            var ukagaishinsei = ukagaiHeader.UkagaiShinseis
                .FirstOrDefault(row => row.UkagaiHeaderId == ukagaiHeader.Id);
            return ukagaiHeader.Status == 承認 && ukagaishinsei != null
                && ukagaishinsei.UkagaiSyubetsu == inquiryType;
        }


        //　部門
        private async Task LoadBumonProcessListAsync()
        {
            ViewData["BumonProcessList"] = await _context.BumonProcesses.AsNoTracking()
                .Select(row => new SelectListItem
                {
                    Value = row.Id.ToString(),
                    Text = row.KaisyaProcess.Oya != null
                        ? row.KaisyaProcess.Oya.Code
                          + row.KaisyaProcess.Code
                          + " "
                          + row.KaisyaProcess.Oya.Name
                          + "("
                          + row.KaisyaProcess.Name
                          + ")"
                        : row.KaisyaProcess.Code
                          + " "
                          + row.KaisyaProcess.Name
                })
                .ToListAsync();
        }


        // 日報の出勤区分foreign keyはCodeじゃないでIdですから
        private async Task<long?> GetIdOfKubunByCodeAsync(string code) => await _context.SyukkinKubuns.AsNoTracking()
            .Where(row => row.CodeString == code).Select(p => (long?)p.Id).FirstOrDefaultAsync();


        // 有給残Dataを取得する
        private async Task<YuukyuuZan?> FetchYuukyuuZanDataAsync(long syainBaseId) => await FirstOrDefaultNoTrackingAsync(_context.YuukyuuZans.Where(row => row.SyainBaseId == syainBaseId));


        // 社員のdataをSyainBaseIdより取得する
        private async Task<Syain?> FetchSyainDataByBaseIdAsync(long syainBaseId, DateOnly jissekiDate) => await FirstOrDefaultNoTrackingAsync(_context.Syains
            .Where(row => row.SyainBaseId == syainBaseId && row.StartYmd < jissekiDate && jissekiDate < row.EndYmd).Include(row => row.KintaiZokusei));


        // 社員のdataを取得する
        private async Task<Syain?> FetchSyainDataAsync(long syainId) => await FirstOrDefaultNoTrackingAsync(_context.Syains.Where(row => row.Id == syainId).Include(row => row.KintaiZokusei));


        // 日報のdataを取得する
        private async Task<Nippou?> FetchNippouDataAsync(long syainId, DateOnly jissekiDate) => await FirstOrDefaultNoTrackingAsync(_context.Nippous.Where(row => row.SyainId == syainId && row.NippouYmd == jissekiDate)
            .Include(row => row.SyukkinKubunId1Navigation).Include(row => row.SyukkinKubunId2Navigation));


        // 実績年月日の非稼働日のdataを取得する
        private async Task<Hikadoubi?> FetchHolidayDataAsync(DateOnly jissekiDate) => await FirstOrDefaultNoTrackingAsync(_context.Hikadoubis.Where(row => row.Ymd == jissekiDate));


        // 勤怠打刻取得する、最大3件
        private async Task<List<WorkingHour>> FetchWorkingHoursListAsync(long? syainId, DateOnly jissekiDate) => await _context.WorkingHours.AsNoTracking().Where(row => row.SyainId == syainId && row.Hiduke == jissekiDate && row.Deleted == false)
                .OrderBy(row => row.SyukkinTime ?? row.TaikinTime)
                .Take(3)
                .ToListAsync();


        // 伺いヘッダーリストを取得する
        private async Task<List<UkagaiHeader>> FetchUkagaiHeadersListAsync(long syainId, DateOnly jissekiDate) => await _context.UkagaiHeaders
                .Where(row => row.SyainId == syainId &&
                row.WorkYmd == jissekiDate &&
                row.Invalid == false).Include(row => row.UkagaiShinseis)
                .OrderByDescending(row => row.ShinseiYmd)
                .ThenByDescending(row => row.Id).ToListAsync();


        // クラス定義の構文修正と初期化
        public class CardsViewModel
        {
            public List<NippouAnkenViewModel> NippouAnkens { get; init; } = new List<NippouAnkenViewModel>();

        }


        // 日報実績⇔案件View Model
        public class NippouAnkenViewModel
        {
            public bool IsLinked { get; set; } = false;

            [Display(Name = "受注番号")]
            public string? KingsJuchuNo { get; set; }

            public long? AnkensId { get; set; }

            [Display(Name = "件名")]
            public string? AnkenName { get; set; }

            [Display(Name = "着工日")]
            public DateOnly? ChaYmd { get; set; }

            public string? JuchuuNo { get; set; }

            public long? KokyakuKaisyaId { get; set; }

            [Display(Name = "顧客名")]
            public string? KokyakuName { get; set; } = null!;

            [Display(Name = "プロセス")]
            public long? BumonProcessId { get; set; }

            public bool? IsGenkaToketu { get; set; } = false;

            [Display(Name = "実績時間")]
            public short? JissekiJikan { get; set; }


            public static NippouAnkenViewModel FromEntity(NippouAnken entity)
                => new NippouAnkenViewModel
                {
                    IsLinked = entity.IsLinked,
                    KingsJuchuNo = entity.Ankens.KingsJuchu?.KingsJuchuNo,
                    AnkensId = entity.AnkensId,
                    AnkenName = entity.AnkenName,
                    ChaYmd = entity.Ankens.KingsJuchu?.ChaYmd,
                    JuchuuNo = entity.Ankens.KingsJuchu?.JuchuuNo,
                    KokyakuKaisyaId = entity.KokyakuKaisyaId,
                    KokyakuName = entity.KokyakuName,
                    BumonProcessId = entity.BumonProcessId,
                    JissekiJikan = entity.JissekiJikan,
                    IsGenkaToketu = entity.Ankens.KingsJuchu?.IsGenkaToketu
                };
        }


        // 日報実績　View Model
        public class NippouViewModel
        {
            public long Id { get; set; }
            public DateOnly NippouYmd { get; set; }
            public TimeOnly? SyukkinHm1 { get; set; }
            public TimeOnly? TaisyutsuHm1 { get; set; }
            public TimeOnly? SyukkinHm2 { get; set; }
            public TimeOnly? TaisyutsuHm2 { get; set; }
            public TimeOnly? SyukkinHm3 { get; set; }
            public TimeOnly? TaisyutsuHm3 { get; set; }
            public string SyukkinKubunCodeString1 { get; set; } = "00";
            public AttendanceClassification SyukkinKubun1
            {
                get
                {
                    if (string.IsNullOrEmpty(SyukkinKubunCodeString1))
                        return AttendanceClassification.None;

                    if (int.TryParse(SyukkinKubunCodeString1, out var value))
                        return (AttendanceClassification)value;

                    return AttendanceClassification.None;
                }
                set
                {
                    SyukkinKubunCodeString1 = ((int)value).ToString("D2");
                }
            }

            public string SyukkinKubunCodeString2 { get; set; } = "00";
            public AttendanceClassification SyukkinKubun2
            {
                get
                {
                    if (string.IsNullOrEmpty(SyukkinKubunCodeString2))
                        return AttendanceClassification.None;

                    if (int.TryParse(SyukkinKubunCodeString2, out var value))
                        return (AttendanceClassification)value;

                    return AttendanceClassification.None;
                }
                set
                {
                    SyukkinKubunCodeString2 = ((int)value).ToString("D2");
                }
            }

            public decimal? HJitsudou { get; set; } = 0m;
            public decimal? HZangyo { get; set; } = 0m;
            public decimal? HWarimashi { get; set; } = 0m;
            public decimal? HShinyaZangyo { get; set; } = 0m;
            public decimal? DJitsudou { get; set; } = 0m;
            public decimal? DZangyo { get; set; } = 0m;
            public decimal? DWarimashi { get; set; } = 0m;
            public decimal? DShinyaZangyo { get; set; } = 0m;
            public decimal? NJitsudou { get; set; } = 0m;
            public decimal? NShinya { get; set; } = 0m;
            public decimal TotalZangyo { get; set; } = 0m;



            [JsonInclude]
            public string Syuttaikin1 => FormatRange(SyukkinHm1, TaisyutsuHm1);

            [JsonInclude]
            public string Syuttaikin2 => FormatRange(SyukkinHm2, TaisyutsuHm2);

            [JsonInclude]
            public string Syuttaikin3 => FormatRange(SyukkinHm3, TaisyutsuHm3);

            public int TotalWorkingHoursInMinute => TimeCalculator.CalculationJitsudouTime(SyukkinHm1?.ToString("HHmm") ?? "",
                       TaisyutsuHm1?.ToString("HHmm") ?? "",
                       SyukkinHm2?.ToString("HHmm") ?? "",
                       TaisyutsuHm2?.ToString("HHmm") ?? "",
                       SyukkinHm3?.ToString("HHmm") ?? "",
                       TaisyutsuHm3?.ToString("HHmm") ?? "");

            public DateOnly? FurikyuYoteiDate { get; set; }


            [Display(Name = "勤務時間")]
            public string TotalWorkingHours
            {
                get
                {
                    TimeSpan totalWorkTime = TimeSpan.FromMinutes(TotalWorkingHoursInMinute);
                    return totalWorkTime.ToString(@"h\:mm");
                }
            }

            public int TotalJissekiJikan { get; set; }

            private static string FormatRange(TimeOnly? syukkinHm, TimeOnly? taisyutsuHm)
            {
                if (!syukkinHm.HasValue || !taisyutsuHm.HasValue)
                {
                    return "-";
                }
                if (syukkinHm == taisyutsuHm)
                {
                    return "-";
                }
                return $"{syukkinHm.Value:H:mm} ~ {taisyutsuHm.Value:H:mm}";
            }

        }


        // 伺いヘッダーViewModel
        public class UkagaiHeadersViewModel()
        {
            public long Id { get; set; }
            public long SyainId { get; set; }

            [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
            public DateOnly ShinseiYmd { get; set; }
            public long? ShoninSyainId { get; set; }

            [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
            public DateOnly? ShoninYmd { get; set; }
            public long? LastShoninSyainId { get; set; }
            public DateOnly? LastShoninYmd { get; set; }
            public ApprovalStatus Status { get; set; }
            public DateOnly WorkYmd { get; set; }
            public TimeOnly? KaishiJikoku { get; set; }
            public TimeOnly? SyuryoJikoku { get; set; }
            public string? Biko { get; set; }
            public bool Invalid { get; set; }
            public List<InquiryType> UkagaiSyubetsu { get; set; } = new List<InquiryType>();
            public string ShinseiNaiyou => UkagaiSyubetsu.Any() ?
                String.Join("・", UkagaiSyubetsu.Select(x => x.GetDisplayName())) : "";

            public static UkagaiHeadersViewModel FromEntity(UkagaiHeader entity) => new UkagaiHeadersViewModel
            {
                Id = entity.Id,
                SyainId = entity.SyainId,
                ShinseiYmd = entity.ShinseiYmd,
                ShoninSyainId = entity.ShoninSyainId,
                ShoninYmd = entity.ShoninYmd,
                LastShoninSyainId = entity.LastShoninSyainId,
                LastShoninYmd = entity.LastShoninYmd,
                Status = entity.Status,
                WorkYmd = entity.WorkYmd,
                KaishiJikoku = entity.KaishiJikoku,
                SyuryoJikoku = entity.SyuryoJikoku,
                Biko = entity.Biko,
                Invalid = entity.Invalid,
                UkagaiSyubetsu = entity.UkagaiShinseis.Select(row => row.UkagaiSyubetsu).ToList()
            };
        }
    }
}
