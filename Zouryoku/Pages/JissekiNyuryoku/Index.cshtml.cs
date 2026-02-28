using CommonLibrary.Extensions;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Model;
using System.Data;
using Zouryoku.Attributes;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using ZouryokuCommonLibrary.Utils;
using static Zouryoku.Utils.Const;

namespace Zouryoku.Pages.JissekiNyuryoku
{
    /// <summary>
    /// 実績入力ページモデル
    /// </summary>
    [FunctionAuthorization]
    public class IndexModel(ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> optionsAccessor, ICompositeViewEngine viewEngine) : 
        BasePageModel<IndexModel>(db, logger, optionsAccessor, viewEngine)
    {
        public override bool UseInputAssets { get; } = true;

        [BindProperty]
        public NippouInputViewModel NippouInput { get; set; } = new();

        // ---------------------------------------------
        // OnGet
        // ---------------------------------------------
        /// <summary>
        /// OnGet
        /// </summary>
        /// <param name="syainBaseId">社員BaseId</param>
        /// <param name="jissekiDate">実績日付</param>
        /// <param name="isDairiInput">代理入力か否か</param>
        /// <param name="syukkinHm1">出勤時間１</param>
        /// <param name="taisyutsuHm1">退出時間１</param>
        /// <param name="syukkinHm2">出勤時間２</param>
        /// <param name="taisyutsuHm2">退出時間２</param>
        /// <param name="syukkinHm3">出勤時間３</param>
        /// <param name="taisyutsuHm3">退出時間３</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<IActionResult> OnGetAsync(long syainBaseId, DateOnly jissekiDate, bool isDairiInput,
            TimeOnly? syukkinHm1, TimeOnly? taisyutsuHm1,
            TimeOnly? syukkinHm2, TimeOnly? taisyutsuHm2,
            TimeOnly? syukkinHm3, TimeOnly? taisyutsuHm3
            )
        {
            List<Syuttaikin> inputSyuttaikins =
                [
                    new Syuttaikin(syukkinHm1, taisyutsuHm1),
                    new Syuttaikin(syukkinHm2, taisyutsuHm2),
                    new Syuttaikin(syukkinHm3, taisyutsuHm3),
                ];

            var queryService = new JissekiNyuryokuQueryService(db);

            // Application Config
            var applicationConfig = await queryService.FetchApplicatationConfig() ??
                    throw new InvalidOperationException("アプリconfig が未登録です。");

            // 社員マスタ取得
            var syain = await queryService.FetchNippouSyainAsync(syainBaseId, jissekiDate) ??
                    throw new InvalidOperationException("社員マスタが未登録です。。");

            var viewService = new JissekiNyuryokuCreateViewModelService(
                            db,
                            appSettings,
                            applicationConfig,
                            syain,
                            jissekiDate,
                            isDairiInput,
                            inputSyuttaikins,
                            LoginInfo.User,
                            timeProvider.Now().ToDateOnly()
                );
            NippouInput = await viewService.CreateViewModelAsync();

            return Page();
        }

        /// <summary>
        /// 過去の日報から出勤扱いの直近の日報からコピーする
        /// </summary>
        /// <param name="index">追加する実績入力欄の先頭のインデックス</param>
        public async Task<IActionResult> OnPostCopyFromLastDateAsync(int index)
        {

            var yesterday = NippouInput.JissekiDate.AddDays(-1);
            var syainBaseId = NippouInput.SyainBaseId;

            // 過去の日報を取得（新しい日報順）
            var nippous = await db.Nippous
                .Where(n => n.Syain.SyainBaseId == syainBaseId && n.NippouYmd <= yesterday)
                .Include(p => p.SyukkinKubunId1Navigation)
                .Include(p => p.SyukkinKubunId2Navigation)
                .OrderByDescending(n => n.NippouYmd)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            if (nippous.Count == 0)
            {
                return SuccessJson(data: null);
            }

            // 出勤扱いの直近の日報からコピーする
            var nippou = nippous.FirstOrDefault(n => n.SyukkinKubunId1Navigation.IsSyukkin == true || n?.SyukkinKubunId2Navigation?.IsSyukkin == true);

            if (nippou == null)
            {
                return SuccessJson(data: null);
            }

            var commonService = new JissekiNyuryokuCommonService(db);

            // コピー元の日報案件を取得（原価凍結案件は除く）
            var yesterdayNippouAnken = await db.NippouAnkens
                .Where(row => row.NippouId == nippou.Id
                    && row.Ankens != null
                    && row.Ankens.KingsJuchu != null
                    && row.Ankens.KingsJuchu.IsGenkaToketu == false)
                .Include(row => row.Ankens)
                    .ThenInclude(row => row.KingsJuchu)
                .ToListAsync();

            var bumonProcessList = await commonService.GetBumonProcessListAsync();

            var startIndex = index;
            var jissekiInputs = yesterdayNippouAnken.Select((n, i) => new JissekiInputViewModel
            {
                Index = startIndex + i,
                IsLinked = n.IsLinked,
                KingsJuchuNo = n.Ankens?.KingsJuchu?.KingsJuchuNo,
                AnkensId = n.AnkensId,
                AnkenName = n.AnkenName,
                ChaYmd = n.Ankens?.KingsJuchu?.ChaYmd,
                KokyakuKaisyaId = n.KokyakuKaisyaId,
                KokyakuName = n.KokyakuName,
                BumonProcessId = n.BumonProcessId,
                BumonProcessList = bumonProcessList,
            });

            var data = await PartialToJsonAsync("_JissekiInputListPartial", jissekiInputs);
            return SuccessJson(data: data);
        }

        /// <summary>
        /// 新しい実績情報入力欄を追加
        /// </summary>
        /// <param name="index">追加する実績入力欄のインデックス番号</param>
        public async Task<IActionResult> OnPostAddNippouAnkenInputAsync(int index)
        {
            var commonService = new JissekiNyuryokuCommonService(db);

            var vm = new JissekiInputViewModel()
            {
                Index = index,
                BumonProcessList = await commonService.GetBumonProcessListAsync(),
            };

            var data = await PartialToJsonAsync("_JissekiInputSinglePartial", vm);
            return SuccessJson(data: data);
        }

        /// <summary>
        /// 実績情報入力欄のコピー
        /// </summary>
        /// <param name="index">コピー元の実績入力欄のインデックス</param>
        public async Task<IActionResult> OnPostCopyNippouAnkenInputAsync(int index)
        {
            var commonService = new JissekiNyuryokuCommonService(db);

            // コピー元の入力内容
            var source = NippouInput.JissekiInputs[index];

            // コピーする            
            var vm = new JissekiInputViewModel()
            {
                Index = NippouInput.JissekiInputs.Count,
                KokyakuName = source.KokyakuName,
                KokyakuKaisyaId = source.KokyakuKaisyaId,
                BumonProcessId = source.BumonProcessId,
                IsLinked = source.IsLinked,
                BumonProcessList = await commonService.GetBumonProcessListAsync(),
            };

            //コピー元の受注が原価凍結されていない場合は、案件・受注の情報もコピー
            if (source.IsGenkaToketu != true)
            {
                vm.AnkensId = source.AnkensId;
                vm.AnkenName = source.AnkenName;
                vm.KingsJuchuNo = source.KingsJuchuNo;
                vm.ChaYmd = source.ChaYmd;
                vm.IsGenkaToketu = source.IsGenkaToketu;
            }

            var data = await PartialToJsonAsync("_JissekiInputSinglePartial", vm);
            return SuccessJson(data: data);
        }

        /// <summary>
        /// 確定解除
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostCancelConfirmAsync()
        {
            var queryService = new JissekiNyuryokuQueryService(db);

            // 日報の社員取得
            var nippouSyain = await queryService.FetchNippouSyainAsync(NippouInput.SyainBaseId, NippouInput.JissekiDate) ??
                    throw new InvalidOperationException("社員マスタが未登録です。。");

            // 更新対象の日報取得
            var nippou = await queryService.FetchNippouForUpdateAsync(NippouInput.Id!.Value);

            if (nippou == null)
            {
                // 存在するはずが、存在しない
                ModelState.AddModelError(string.Empty, string.Format(ErrorNotExists, "日報", NippouInput.Id));
                return CommonErrorResponse();
            }

            // 実行時チェック
            var validator = new JissekiNyuryokuUnConfirmValidator(db, nippou, ModelState);
            await validator.ValidateOnUnconfirmAsync();

            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            // 日報を確定解除（一時保存にする）
            nippou.TourokuKubun = DailyReportStatusClassification.一時保存;

            var commonService = new JissekiNyuryokuCommonService(db);

            // 時間外労働時間制限拡張の伺い申請の有効化
            // 月末の日報の場合に処理
            await commonService.EnableJikangaiSeigenKakuchoAsync(NippouInput, nippouSyain!);

            var furikyuuYuukyuuService = new JissekiNyuryokuFurikyuuYuukyuuService(db, appSettings);

            // 振替休暇と有給休暇の消化の取り消し処理
            await furikyuuYuukyuuService.CancelFurikyuuAndYuukyuuSyoukaAsync(nippouSyain!, NippouInput);

            // 代理入力登録
            if (NippouInput.IsDairiInput)
            {
                commonService.InsertDairiNyuryokuRireki(nippou, LoginInfo.User.Id, DailyReportOperation.確定解除, timeProvider.Now());
            }

            // DB保存
            await SaveWithConcurrencyCheckAsync(string.Format(ErrorConflictReload, "日報"));

            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            return SuccessJson();
        }

        /// <summary>
        /// 一時保存
        /// </summary>
        public async Task<IActionResult> OnPostTemporarySaveAsync()
        {
            // 単項目チェック
            JsonResult? errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            var queryService = new JissekiNyuryokuQueryService(db);

            var nippouSyain = await queryService.FetchNippouSyainAsync(NippouInput.SyainBaseId, NippouInput.JissekiDate) ??
                    throw new InvalidOperationException("社員マスタが未登録です。。");

            var commonService = new JissekiNyuryokuCommonService(db);

            // 日報へ登録する各時間を計算する
            var timeContainer = await commonService.CalcJissekiNyuryokuTimeAsync(NippouInput);

            Nippou? nippou;

            // 新規登録
            if (NippouInput.Id == null)
            {
                // 日報
                nippou = await commonService.InsertNippouAsync(NippouInput, nippouSyain, timeContainer, DailyReportStatusClassification.一時保存);

                // 日報案件
                commonService.InsertNippouAnkenList(nippou, NippouInput);
            }
            else
            // 更新
            {
                // 更新対象の日報
                nippou = await queryService.FetchNippouForUpdateAsync(NippouInput.Id.Value);

                // 存在するはずが、存在しない
                if (nippou == null)
                {
                    ModelState.AddModelError(string.Empty, string.Format(ErrorNotExists, "日報", NippouInput.Id));
                    return CommonErrorResponse();
                }

                //日報データを更新
                await commonService.UpdateNippouAsync(nippou, NippouInput, nippouSyain, timeContainer, DailyReportStatusClassification.一時保存);

                // 日報案件の登録・更新・削除処理
                try
                {
                    await commonService.InsertOrUpdateOrDeleteNippouAnken(nippou, NippouInput);
                }
                catch (DBConcurrencyException ex)
                {
                    // 更新、削除で対象の日報案件が存在するはずが、存在しない
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return CommonErrorResponse();
                }
            }

            // DB保存
            await SaveWithConcurrencyCheckAsync(string.Format(ErrorConflictReload, "日報"));

            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            return SuccessJson();
        }

        /// <summary>
        /// 確定 check
        /// </summary>
        public async Task<IActionResult> OnPostFinalConfirmCheckAsync()
        {
            var queryService = new JissekiNyuryokuQueryService(db);

            // 日報の社員
            var nippouSyain = await queryService.FetchNippouSyainAsync(NippouInput.SyainBaseId, NippouInput.JissekiDate) ??
                    throw new InvalidOperationException("社員マスタが未登録です。。");

            // 他部署の案件を使用しているか
            var ankenIds = NippouInput.JissekiInputs.Select(x => x.AnkensId).ToList();

            var busyoBaseIds = await db.Ankens.AsNoTracking()
                .Where(row => ankenIds.Contains(row.Id))
                .Select(a => a.KingsJuchu!.Busyo!.BusyoBaseId)
                .ToListAsync();

            var hasOtherBusyoBaseId = busyoBaseIds.Any(id => id != nippouSyain!.Busyo.BusyoBaseId);

            if (hasOtherBusyoBaseId)
            {
                var message = $"{ErrorOtherBusyoOrdeSelected}\r\n{ConfirmFixNippou}\r\n{ConfirmFixCautionNippou}";
                return WarningJson(message);
            }


            var commonService = new JissekiNyuryokuCommonService(db);

            bool isRefreshDay = await commonService.IsRefreshDayAsync(NippouInput.JissekiDate);

            if (isRefreshDay)
            {
                List<WorkingHour> workingHours = await queryService.FetchWorkingHoursListAsync(nippouSyain!.Id, NippouInput.JissekiDate);
                List<UkagaiHeader> ukagai = await queryService.FetchUkagaiHeadersAsync(nippouSyain.Id, NippouInput.JissekiDate);

                bool hasApprovedRefreshDayZangyo = ukagai.Any(u =>
                        u.Status == ApprovalStatus.承認 && u.UkagaiShinseis.Any(s => s.UkagaiSyubetsu == InquiryType.リフレッシュデー残業));

                if (!hasApprovedRefreshDayZangyo)
                {
                    var allowableDiff = TimeSpan.FromMinutes(15);

                    var vmTaisyutsuTimes = new List<TimeOnly?>
                                {
                                    NippouInput.TaisyutsuHm1,
                                    NippouInput.TaisyutsuHm2,
                                    NippouInput.TaisyutsuHm3
                                };
                    for (int i = 0; i < Math.Min(workingHours.Count, vmTaisyutsuTimes.Count); i++)
                    {
                        var vmTaisyutsuHm = vmTaisyutsuTimes[i];
                        if (vmTaisyutsuHm is null) continue;

                        if (workingHours[i].TaikinTime is not DateTime whTaisyutsuDateTime) continue;

                        var whTaisyutsuHm = whTaisyutsuDateTime.ToTimeOnly();
                        var whTaisyutsuJikan = Time.ConvertJikan(whTaisyutsuHm.ToStrByHHmmNoColon());
                        if (whTaisyutsuJikan < Time.リフレッシュ.Item1 || Time.リフレッシュ.Item2 < whTaisyutsuJikan) continue;

                        if (allowableDiff < vmTaisyutsuHm - whTaisyutsuHm)
                        {
                            var message = $"{JikanHoseiForRefreshDay}\r\n{ConfirmFixNippou}\r\n{ConfirmFixCautionNippou}";
                            return WarningJson(message);
                        }
                    }
                }
            }

            if (NippouInput.JitsudouTime < (60 * NippouInput.TotalJissekiJikan))
            {
                var message = $"{JitsudouJissekiMismatch}\r\n{ConfirmFixNippou}\r\n{ConfirmFixCautionNippou}";
                return WarningJson(message);
            }

            return WarningJson($"{ConfirmFixNippou}\r\n{ConfirmFixCautionNippou}");
        }

        /// <summary>
        /// 確定
        /// </summary>
        public async Task<IActionResult> OnPostFinalConfirmAsync()
        {
            // 単項目チェック
            JsonResult? errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            var queryService = new JissekiNyuryokuQueryService(db);

            var nippouSyain = await queryService.FetchNippouSyainAsync(NippouInput.SyainBaseId, NippouInput.JissekiDate) ??
                throw new InvalidOperationException("社員マスタが未登録です。。");

            var commonService = new JissekiNyuryokuCommonService(db);

            // 日報へ登録する各時間を計算する
            var timeContainer = await commonService.CalcJissekiNyuryokuTimeAsync(NippouInput);

            // 入力チェックで使用する時間を更新
            NippouInput.DJitsudou = timeContainer.DJitsudou;
            NippouInput.NJitsudou = timeContainer.NJitsudou;
            NippouInput.TotalZangyo = timeContainer.TotalZangyo;

            JissekiNyuryokuConfirmValidator validator = new(
                db,
                appSettings,
                NippouInput,
                nippouSyain,
                timeProvider.Today(),
                ModelState
                );
            await validator.FinalConfirmValidationAsync();


            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            // 振休、有給の消化処理
            var furikyuuYuukyuuService = new JissekiNyuryokuFurikyuuYuukyuuService(db, appSettings);
            await furikyuuYuukyuuService.TakeFurikyuuAndYuukyuuSyoukaAsync(nippouSyain, NippouInput);

            Nippou? nippou;

            // 新規登録
            if (NippouInput.Id == null)
            {
                // 日報
                nippou = await commonService.InsertNippouAsync(NippouInput, nippouSyain, timeContainer, DailyReportStatusClassification.確定保存);

                // 日報案件
                commonService.InsertNippouAnkenList(nippou, NippouInput);
            }
            else
            // 更新
            {
                nippou = await queryService.FetchNippouForUpdateAsync(NippouInput.Id.Value);

                // 存在するはずが、存在しない
                if (nippou == null)
                {
                    ModelState.AddModelError(string.Empty, string.Format(ErrorNotExists, "日報", NippouInput.Id));
                    return CommonErrorResponse();
                }

                // 日報
                await commonService.UpdateNippouAsync(nippou, NippouInput, nippouSyain, timeContainer, DailyReportStatusClassification.確定保存);

                // 日報案件の登録・更新・削除処理
                try
                {
                    await commonService.InsertOrUpdateOrDeleteNippouAnken(nippou, NippouInput);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // 更新、削除で対象の日報案件が存在するはずが、存在しない
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return CommonErrorResponse();
                }
            }

            // 月末の場合
            // 時間外労働時間制限拡張の伺い申請の無効化
            if (NippouInput.JissekiDate == NippouInput.JissekiDate.GetEndOfMonth())
            {
                var isOver = await commonService.IsOverZangyoSeigenJikanAsync(nippouSyain, NippouInput);

                if (isOver)
                {
                    var ukagai = await queryService.FetchJikangaiKakuchoSinseiForUpdateAsync(nippouSyain.Id, NippouInput.JissekiDate);
                    ukagai?.Invalid = true;
                }
            }

            //代理入力時
            if (NippouInput.IsDairiInput)
            {
                // 代理入力履歴登録
                commonService.InsertDairiNyuryokuRireki(nippou, LoginInfo.User.Id, DailyReportOperation.確定, timeProvider.Now());
            }
            else
            {
                // 代理入力履歴を無効化
                await commonService.InvalidateDairiNyuuryokuRirekiAsync(nippou);
            }

            // DB保存
            await SaveWithConcurrencyCheckAsync(string.Format(ErrorConflictReload, "日報"));

            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            return SuccessJson();
        }
    }
}
