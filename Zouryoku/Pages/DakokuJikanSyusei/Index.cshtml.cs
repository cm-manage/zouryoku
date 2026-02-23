using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Extensions;
using Model.Model;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Model.Enums.DailyReportStatusClassification;
using static Model.Enums.EmployeeWorkType;
using static Model.Enums.ApprovalStatus;
using static Model.Enums.InquiryType;

namespace Zouryoku.Pages.DakokuJikanSyusei
{

    [FunctionAuthorization]
    public class IndexModel : BasePageModel<IndexModel>
    {
        #region 時間コンボボックスリスト作成
        /// <summary>
        /// 時コンボボックスに表示する時刻の最大値（0～23時）。
        /// </summary>
        /// <remarks>
        /// 24:00 は別途「翌日 00:00」として扱うため、この定数は 23 までを上限とする。
        /// そのため、<see cref="GetHourListItems"/> では 00～23 時の 24 個の選択肢を生成する。
        /// </remarks>
        private const int MaxHour = 23;

        /// <summary>
        /// 分コンボボックスに表示する分の最大値（0～59分）。
        /// </summary>
        /// <remarks>
        /// 分は常に 0～59 の範囲で扱うため、この定数は 59 を上限とし、
        /// <see cref="MinuteInterval"/> と組み合わせて分の選択肢を生成する。
        /// </remarks>
        private const int MaxMinute = 59;

        /// <summary>
        /// 時間コンボボックスリスト
        /// </summary>
        public class TimeSelectListHelper
        {
            /// <summary>
            /// 分コンボボックスの選択間隔（分単位）。
            /// </summary>
            /// <remarks>
            /// 1 の場合は 00, 01, 02, …, 59 の 1 分刻み、
            /// 5 の場合は 00, 05, 10, …, 55 の 5 分刻みといった形で
            /// <see cref="GetMinuteListItems"/> 内の分の刻み幅を制御する。
            /// </remarks>
            private const int MinuteInterval = 1;

            #region コンボボックス選択肢生成
            /// <summary>
            /// 時間選択肢
            /// </summary>
            public static IEnumerable<SelectListItem> GetHourListItems() =>
                Enumerable.Range(0, MaxHour + 1).Select(i => new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString("00"),
                });

            /// <summary>
            /// 分選択肢
            /// </summary>
            /// <returns></returns>
            public static IEnumerable<SelectListItem> GetMinuteListItems() =>
                Enumerable.Range(0, (MaxMinute / MinuteInterval) + 1)
                    .Select(i => i * MinuteInterval)
                    .Select(i => new SelectListItem
                    {
                        Value = i.ToString(),
                        Text = i.ToString("00")
                    });
            #endregion
        }
        #endregion

        #region プロパティ
        // ---------------------------------------------
        // 通常のプロパティ（画面表示用）
        // ---------------------------------------------

        public override bool UseInputAssets => true;

        /// <summary>
        /// 打刻時間修正ViewModel (ViewModel)
        /// </summary>
        [BindProperty]
        public DakokuJikanSyuseiViewModel ViewModel { get; set; } = new DakokuJikanSyuseiViewModel();
        #endregion

        #region ViewModel定義
        // ---------------------------------------------
        // ViewModel定義
        // ---------------------------------------------
        /// <summary>打刻時間修正ViewModel</summary>
        public class DakokuJikanSyuseiViewModel
        {
            //----------------------------------------------
            // 入力情報
            //----------------------------------------------
            /// <summary>社員ID</summary>
            public long SyainId { get; set; }

            /// <summary>実績年月日</summary>
            public DateOnly JissekiDate { get; set; }

            /// <summary>伺いヘッダID</summary>
            public long? UkagaiHeaderId { get; set; }

            /// <summary>出退勤ラベルタイトル</summary>
            public static ImmutableArray<string> LabelTitle { get; } = ["出退勤１", "出退勤２", "出退勤３"];

            /// <summary>出退勤時間１～３</summary>
            public List<TimeSet> TimeSets { get; set; } = [new TimeSet(), new TimeSet(), new TimeSet()];

            /// <summary>削除済みの出退勤時間</summary>
            public List<TimeSet> DeletedTimeSets { get; set; } = [];

            /// <summary>修正理由</summary>
            [Display(Name = "修正理由")]
            public string SyuseiReason { get; set; } = string.Empty;

            /// <summary>伺い入力ヘッダのバージョン</summary>
            public uint? UkagaiHeaderVersion { get; set; }

            /// <summary>伺い申請のバージョン</summary>
            public List<uint?> UkagaiShinseiVersions { get; set; } = [];

            /// <summary>日報確定済みかどうか</summary>
            public bool IsKakutei => TorokuKubun == 確定保存;

            /// <summary>登録状況区分</summary>
            public DailyReportStatusClassification? TorokuKubun { get; set; }            

            /// <summary>
            /// 表示用の出退勤時間をセット
            /// </summary>
            /// <param name="ranges">出退勤時間範囲配列</param>
            public void SetTimeRanges(TimeRange[] ranges)
            {
                for (int i = 0; i < ranges.Length && i < TimeSets.Count; i++)
                {
                    var r = ranges[i];
                    var s = TimeSets[i];

                    s.Start.Hour = r.Start?.Hour;
                    s.Start.Minute = r.Start?.Minute;
                    s.End.Hour = r.End?.Hour;
                    s.End.Minute = r.End?.Minute;
                    s.Version = r.Version;
                    s.Deleted = r.Deleted;
                }
            }

            public void SetDeletedTimeRanges(TimeRange[] ranges)
            {
                var timeSets = new List<TimeSet>();
                for (int i = 0;i < ranges.Length; i++)
                {
                    var r = ranges[i];

                    timeSets.Add(
                        new TimeSet
                        {
                            Start = new TimeInput
                            {
                                Hour = r.Start?.Hour,
                                Minute = r.Start?.Minute,
                            },
                            End = new TimeInput
                            {
                                Hour = r.End?.Hour,
                                Minute = r.End?.Minute,
                            },
                            Version = r.Version,
                            Deleted = r.Deleted,
                        }
                    );
                }

                DeletedTimeSets = timeSets;
            }
        }

        /// <summary>出退勤セットモデル</summary>
        public class TimeSet
        {
            /// <summary>出勤時間</summary>
            public TimeInput Start { get; set; } = new();

            /// <summary>退勤時間</summary>  
            public TimeInput End { get; set; } = new();

            /// <summary>バージョン</summary>
            public uint? Version { get; set; }

            /// <summary>削除フラグ</summary>
            public bool Deleted { get; set; }
        }

        /// <summary>時間入力モデル</summary>
        public class TimeInput
        {
            /// <summary>時</summary>
            public int? Hour { get; set; }

            /// <summary>分</summary>
            public int? Minute { get; set; }

            /// <summary>
            /// 時と分の片方のみ入力されているかどうか
            /// </summary>
            public bool IsHalfInput => (Hour is not null) ^ (Minute is not null);

            /// <summary>
            /// 時分からTimeOnlyを生成（どちらかがnullの場合はnullを返す）
            /// </summary>
            public TimeOnly? AsTimeOnly => TryGetTimeOnly(out var timeOnly) ? timeOnly : null;

            /// <summary>
            /// 時分からTimeOnlyを生成（どちらかがnullの場合はfalseを返す）
            /// </summary>
            private bool TryGetTimeOnly(out TimeOnly timeOnly)
            {
                timeOnly = default;
                if (Hour is null || Minute is null) return false;

                // UI側ではselect要素で入力値が制限されているが、パラメータ改ざんや別画面・APIからの呼び出し、
                // 将来的なUI変更などに備えてサーバー側でも時と分の範囲チェックを行う。
                // また、このチェックによりTimeOnlyコンストラクタでの例外発生も防止する。
                if (Hour.Value < 0 || MaxHour < Hour.Value) return false;
                if (Minute.Value < 0 || MaxMinute < Minute.Value) return false;

                timeOnly = new TimeOnly(Hour.Value, Minute.Value);
                return true;
            }
        }

        /// <summary>出退勤時間範囲モデル</summary>
        public class TimeRange
        {
            /// <summary>開始時間</summary>
            public TimeOnly? Start { get; }

            /// <summary>終了時間</summary>
            public TimeOnly? End { get; }

            /// <summary>バージョン</summary>
            public uint? Version { get; set; }

            /// <summary>削除フラグ</summary>
            public bool Deleted { get; set; }

            /// <summary>コンストラクタ</summary>
            /// <param name="start">出勤時間</param>
            /// <param name="end">退勤時間</param>
            public TimeRange(TimeOnly? start, TimeOnly? end, uint? version, bool deleted)
            {
                Start = start;
                End = end;
                Version = version;
                Deleted = deleted;
            }
        }
        #endregion

        #region コンストラクタ
        // ---------------------------------------------
        // DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        public IndexModel(ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> options, TimeProvider? timeProvider = null)
            : base(db, logger, options, timeProvider)
        {
        }
        #endregion

        #region GET処理
        /// <summary>GET Index</summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日</param>
        /// <returns>ページ情報</returns>
        public async Task<IActionResult> OnGetAsync(long syainId, DateOnly jissekiDate)
        {
            // 表示用の勤怠打刻情報を取得
            var workingHours = await GetWorkingHoursAsync(syainId, jissekiDate);

            // 表示する勤怠打刻が存在する場合、ViewModelへセットして返却
            if (workingHours.Where(x => !x.Deleted).Any())
            {
                ViewModel = await BuildViewModelFromWorkingHours(syainId, jissekiDate, workingHours);
                return Page();
            }

            // 新規
            // → ViewModelを新規作成して返却
            ViewModel = BuildNewViewModel(syainId, jissekiDate);
            return Page();
        }
        #endregion

        #region Post処理
        /// <summary>登録処理</summary>
        /// <returns>結果</returns>
        public async Task<IActionResult> OnPostRegisterAsync()
        {
            // 代理入力かどうかを確認
            var loginUserInfo = await GetSyainAsync(LoginInfo.User.Id);

            bool isNotDairi = (loginUserInfo.KintaiZokusei.Code == _3か月60時間 ||
                loginUserInfo.KintaiZokusei.Code == 月45時間 ||
                loginUserInfo.KintaiZokusei.Code == 管理) &&
                (!loginUserInfo.IsInstructionApprover &&
                !loginUserInfo.IsFinalInstructionApprover) &&
                loginUserInfo.Id == ViewModel.SyainId;

            // 関連性チェック
            await ValidateRegister(isNotDairi);

            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            // 更新する勤怠打刻の取得
            //     ※Tracking済み
            var workingHours = await FetchWorkingHoursAsync(ViewModel.SyainId, ViewModel.JissekiDate);

            var lookup = workingHours.ToLookup(x => x.Deleted);

            // 削除されていない勤怠打刻
            var notDeletedWorkingHours = lookup[false].ToList();

            // 削除済み勤怠打刻
            var deletedWorkingHours = lookup[true].ToList();

            // 更新対象の伺い申請情報を取得
            var ukagaishinseis = await GetUkagaiShinseisAsync(ViewModel.UkagaiHeaderId);

            // バージョンの件数を確認
            ValidateVersions(workingHours, ukagaishinseis);

            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            // 勤怠打刻に伺いヘッダIDが登録されている場合、伺いヘッダ/伺い申請を削除する
            if (ViewModel.UkagaiHeaderId is not null)
            {
                await DeleteUkagaisAsync(ViewModel.UkagaiHeaderId.Value);
            }

            // 削除フラグがTrueの勤怠打刻情報が存在する場合、削除フラグがFalseの勤怠打刻情報を削除する。
            if (0 < deletedWorkingHours.Count)
            {
                await DeleteWorkingHoursAsync(notDeletedWorkingHours);
            }

            // 入力値をDBに登録
            await RegisterWorkingHoursAsync(workingHours, isNotDairi);

            await SaveWithConcurrencyCheckAsync(string.Format(Const.ErrorConflictReload, "打刻情報"));
            
            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            return Success();
        }
        #endregion

        #region DB取得処理
        /// <summary>
        /// 表示する勤怠打刻を検索する。
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日</param>
        /// <returns>勤怠打刻データ</returns>
        private async Task<List<WorkingHour>> GetWorkingHoursAsync(long syainId, DateOnly jissekiDate)
        {
            // 勤怠打刻データ一覧を返却
            return await db.WorkingHours                    // 勤怠打刻
                    .Where(x => x.SyainId == syainId        // 社員ID
                            && x.Hiduke == jissekiDate)     // 実績日
                    .Include(x => x.UkagaiHeader)
                        .ThenInclude(x => x!.UkagaiShinseis)
                    .Include(x => x.Syain)
                        .ThenInclude(x => x.Nippous.Where(x => x.NippouYmd == jissekiDate))
                    .OrderBy(x => x.SyukkinTime)
                    .AsSplitQuery()
                    .AsNoTracking()
                    .ToListAsync();
        }

        /// <summary>
        /// 更新用の勤怠打刻を検索する。
        ///     ※注意：AsNoTracking は使用しない。
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日</param>
        /// <returns>勤怠打刻データ</returns>
        private async Task<List<WorkingHour>> FetchWorkingHoursAsync(long syainId, DateOnly jissekiDate)
        {
            // 勤怠打刻データ一覧を返却
            return await db.WorkingHours
                .Where(x => x.SyainId == syainId
                && x.Hiduke == jissekiDate)
                .Include(x => x.UkagaiHeader)
                    .ThenInclude(x => x!.UkagaiShinseis)
                .OrderBy(x => x.SyukkinTime)
                .AsSplitQuery()
                .ToListAsync();
        }

        /// <summary>
        /// 社員情報を取得する。
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <returns>社員情報</returns>
        private async Task<Syain> GetSyainAsync(long syainId)
        {
            // 社員情報を返却
            return await db.Syains
                .Include(x => x.KintaiZokusei)
                .AsNoTracking()
                .FirstAsync(x => x.Id == syainId);
        }

        /// <summary>
        /// 伺いヘッダ情報を取得する。
        /// </summary>
        /// <param name="ukagaiHeaderId">伺いヘッダID</param>
        /// <returns>伺いヘッダ情報</returns>
        private async Task<UkagaiHeader?> GetUkagaiHeaderAsync(long ukagaiHeaderId)
        {
            return await db.UkagaiHeaders
                .FirstOrDefaultAsync(x => x.Id == ukagaiHeaderId);
        }

        /// <summary>
        /// 伺い申請情報を取得する。
        /// </summary>
        /// <param name="ukagaiHeaderId">伺いヘッダID</param>
        /// <returns>伺い申請情報</returns>
        private async Task<List<UkagaiShinsei>> GetUkagaiShinseisAsync(long? ukagaiHeaderId)
        {
            if (ukagaiHeaderId is null) return new List<UkagaiShinsei>();

            return await db.UkagaiShinseis
                .Where(x => x.UkagaiHeaderId == ukagaiHeaderId)
                .ToListAsync();
        }
        #endregion

        #region ViewModel構築
        /// <summary>
        /// 勤怠打刻からViewModelを構築する
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日</param>
        /// <param name="workingHours">勤怠打刻情報</param>
        /// <returns>Viewモデル</returns>
        private async Task<DakokuJikanSyuseiViewModel> BuildViewModelFromWorkingHours(
            long syainId,
            DateOnly jissekiDate,
            List<WorkingHour> workingHours)
        {
            var notDeletedWorkinghours = workingHours.Where(x => !x.Deleted).OrderBy(x => x.SyukkinTime).ToList();
            var viewModel = new DakokuJikanSyuseiViewModel
            {
                // 社員ID
                SyainId = syainId,

                // 実績日
                JissekiDate = jissekiDate,

                // 伺いヘッダID
                UkagaiHeaderId = notDeletedWorkinghours
                .Where(x => x.UkagaiHeaderId != null)
                .Select(x => x.UkagaiHeaderId)
                .FirstOrDefault() ?? null,

                // 修正理由
                SyuseiReason = notDeletedWorkinghours
                .FirstOrDefault()?
                .UkagaiHeader?
                .Biko ?? string.Empty,

                // 伺い入力ヘッダのバージョン
                UkagaiHeaderVersion = notDeletedWorkinghours
                .FirstOrDefault(x => x.UkagaiHeaderId != null)?
                .UkagaiHeader?
                .Version,

                // 伺い申請のバージョン
                UkagaiShinseiVersions = notDeletedWorkinghours
                .FirstOrDefault(x => x.UkagaiHeaderId != null)?
                .UkagaiHeader?
                .UkagaiShinseis
                .Select(x => (uint?)x.Version)
                .ToList() ?? new List<uint?>(),

                // 登録状況区分
                TorokuKubun = notDeletedWorkinghours
                .FirstOrDefault()?
                .Syain
                .Nippous
                .FirstOrDefault()?
                .TourokuKubun,
            };

            // 出退勤時間
            var timeRanges = workingHours
                .OrderBy(x => x.SyukkinTime)
                .Select(h => new TimeRange(
                    h.SyukkinTime?.ToTimeOnly(),
                    h.TaikinTime?.ToTimeOnly(),
                    h.Version,
                    h.Deleted
                    ))
                .ToArray();

            // 表示用出退勤時間
            var viewTimeRanges = timeRanges.Where(x => !x.Deleted).ToArray();
            viewModel.SetTimeRanges(viewTimeRanges);

            // 削除済み出退勤時間
            var deletedTimeRanges = timeRanges.Where(x => x.Deleted).ToArray();
            viewModel.SetDeletedTimeRanges(deletedTimeRanges);

            return viewModel;
        }

        /// <summary>
        /// 新規ViewModel構築
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績年月日</param>
        /// <param name="isDairiInput">代理入力かどうか</param>
        /// <returns>Viewモデル</returns>
        private static DakokuJikanSyuseiViewModel BuildNewViewModel(long syainId, DateOnly jissekiDate)
        {
            return new DakokuJikanSyuseiViewModel
            {
                SyainId = syainId,
                JissekiDate = jissekiDate,
                SyuseiReason = string.Empty,
            };
        }
        #endregion

        #region Validationチェック
        /// <summary>
        /// Validationチェック
        /// </summary>
        private async Task ValidateRegister(bool isNotDairi)
        {
            // --- 入力チェック ---

            // モデル情報チェック
            if (!ModelState.IsValid)
            {
                return;
            }

            // 日報確定済の場合、編集不可
            if (ViewModel.IsKakutei)
            {
                ModelState.AddModelError(string.Empty, Const.ErrorNippouLocked);
                return;
            }

            // 出退勤セットをまとめる
            for (int i = 0; i < ViewModel.TimeSets.Count; i++)
            {
                var set = ViewModel.TimeSets[i];
                ValidateInputTimeCheck(set.Start, set.End, $"出退勤{i + 1}");
            }

            // 出勤時間と退勤時間の重複を確認
            ValidateWorkingHourOverlaps(ViewModel.TimeSets, ViewModel.JissekiDate);

            // 出退勤が全て未入力の場合
            var isAllEmpty =
                ViewModel.TimeSets.All(s => 
                !s.Start.Hour.HasValue &&
                !s.Start.Minute.HasValue &&
                !s.End.Hour.HasValue &&
                !s.End.Minute.HasValue);

            if (isAllEmpty)
            {
                ModelState.AddModelError(string.Empty, string.Format(Const.ErrorInputRequired, "出退勤時間"));
                return;
            }
            
            // 修正理由の入力チェック
            if (isNotDairi)
            {
                if (string.IsNullOrWhiteSpace(ViewModel.SyuseiReason))
                {
                    ModelState.AddModelError(string.Empty, string.Format(Const.ErrorInputRequired, "修正理由"));
                    return;
                }
            }
        }

        /// <summary>
        /// 出退勤入力チェック
        /// </summary>
        /// <remarks>
        /// UI上は「24:00」（翌日）という入力ができないため、利用者は翌日退勤を「00:00」として入力する
        /// </remarks>
        /// <param name="syukkinTimeInput">出勤時間</param>
        /// <param name="taikinTimeInput">退勤時間</param>
        /// <param name="inputLabel">ラベル(エラーメッセージの対象項目名)</param>
        private void ValidateInputTimeCheck(TimeInput syukkinTimeInput, TimeInput taikinTimeInput, string inputLabel)
        {
            // エラーメッセージ追加
            void AddError(string message) =>
                ModelState.AddModelError(string.Empty, message);

            // --- 入力チェック ---

            // 時と分の片方のみ入力されている場合
            // 出勤と退勤の両方チェック
            if (syukkinTimeInput.IsHalfInput || taikinTimeInput.IsHalfInput)
            {
                AddError(string.Format(Const.ErrorSet, inputLabel + "、時間と分の両方"));
                return;
            }

            var syukkin = syukkinTimeInput.AsTimeOnly;
            var taikin = taikinTimeInput.AsTimeOnly;

            // 出勤・退勤ともに未入力の場合は以降のチェック不要
            if (syukkin is null && taikin is null) return;

            // 退勤 < 出勤 のチェック
            // UI上の「00:00」は「24:00」相当として扱う特別仕様（詳細はValidateInputTimeCheckのXMLドキュメント<remarks>参照）

            // 退勤時刻が「00:00」に相当する場合（TimeOnly.MinValue＝翌日の00:00［24:00相当］）は翌日退勤として扱うため、
            // エラー判定から除外する

            // 00:00（翌日）の場合はエラーチェック対象外
            if (taikin == TimeOnly.MinValue) return;

            if (taikin < syukkin)
            {
                AddError(string.Format(Const.ErrorReverse, inputLabel + "、出退勤時間"));
                return;
            }
        }


        /// <summary>
        /// 出勤時間と退勤時間の重複を確認する。
        /// </summary>
        /// <param name="timeSets">入力された出退勤時間</param>
        /// <param name="baseDate">基準日</param>
        private void ValidateWorkingHourOverlaps(List<TimeSet> timeSets, DateOnly baseDate)
        {
            // DateTimeに変換
            var ranges = new List<(string Label, DateTime Start, DateTime End)>();

            for (int i = 0; i < timeSets.Count; i++)
            {
                var set = timeSets[i];
                var label = $"出退勤{i + 1}";

                // 出勤時間と退勤時間が入力されていない場合
                if (set.Start.AsTimeOnly is null && set.End.AsTimeOnly is null) continue;

                // 時と分の片方のみ入力されている場合
                // 出勤と退勤の両方チェック
                if (set.Start!.IsHalfInput || set.End.IsHalfInput) continue;

                var start = set.Start.AsTimeOnly;
                var end = set.End.AsTimeOnly;

                // 出勤時間の入力値をDateTimeに変換
                var isStartEmpty = start is null;

                DateTime startDt = baseDate.ToDateTime(isStartEmpty ? TimeOnly.MinValue : start);

                // 退勤時間の入力値をDateTimeに変換
                //      入力されていない場合、最大値を設定
                var isEndEmpty = end is null;
                
                // 退勤時間の00:00は翌日扱いする
                var endDate = (isEndEmpty || end == TimeOnly.MinValue)
                    ? baseDate.AddDays(1)
                    : baseDate;

                DateTime endDt = endDate.ToDateTime(isEndEmpty ? TimeOnly.MinValue : end);

                ranges.Add((label, startDt, endDt));
            }

            if (ranges.Count <= 1) return;

            // 並び順を修正
            var ordered = ranges
                .OrderBy(x => x.Start)
                .ToList();

            // 重複を検出
            for (int i = 1; i < ordered.Count; i++)
            {
                var set = ordered[i - 1];
                var nextSet = ordered[i];

                bool isOverlap = nextSet.Start < set.End;

                // 重複している場合、エラーメッセージを表示
                if (isOverlap)
                {
                    ModelState.AddModelError(string.Empty, string.Format(Const.ErrorOverlapInputTime, set.Label, nextSet.Label));
                    return;
                }
            }
        }

        /// <summary>
        /// バージョン件数を確認
        /// </summary>
        /// <param name="workingHours">勤怠打刻情報</param>
        /// <param name="ukagaiShinseis">伺い申請情報</param>
        private void ValidateVersions(List<WorkingHour> workingHours, List<UkagaiShinsei> ukagaiShinseis)
        {
            // 勤怠打刻のバージョン件数と更新用勤怠打刻情報の件数確認
            var deletedWorkingHours = workingHours.Where(x => x.Deleted).ToList();

            if (ViewModel.DeletedTimeSets.Count(x => x.Version != null) != deletedWorkingHours.Count)
            {
                ModelState.AddModelError(string.Empty, string.Format(Const.ErrorConflictReload, "打刻情報"));
                return;
            }

            var notDeletedWorkingHours = workingHours.Where(x => !x.Deleted).ToList();

            if (ViewModel.TimeSets.Count(x => x.Version != null) != notDeletedWorkingHours.Count)
            {
                ModelState.AddModelError(string.Empty, string.Format(Const.ErrorConflictReload, "打刻情報"));
                return;
            }

            // 伺い申請情報のバージョン件数と伺い申請情報の件数確認
            if (ViewModel.UkagaiShinseiVersions.Count != ukagaiShinseis.Count)
            {
                ModelState.AddModelError(string.Empty, string.Format(Const.ErrorConflictReload, "打刻情報"));
                return;
            }
        }
        #endregion

        #region DB操作処理
        /// <summary>
        /// 勤怠打刻情報を削除する。
        /// </summary>
        /// <param name="workingHours">勤怠打刻情報</param>
        /// <param name="ukagaiHeaderId">伺いヘッダID</param>
        private async Task DeleteWorkingHoursAsync(List<WorkingHour> notDeletedWorkingHours)
        {
            // 勤怠打刻を削除
            var targetWorkingHours = notDeletedWorkingHours
                .OrderBy(x => x.SyukkinTime)
                .ToList();

            for (int i = 0; i < targetWorkingHours.Count; i++)
            {
                // バージョンをセット
                db.SetOriginalValue(targetWorkingHours[i], e => e.Version, ViewModel.TimeSets[i].Version);

                db.WorkingHours.Remove(targetWorkingHours[i]);
            }
        }

        /// <summary>
        /// 伺いヘッダ情報、伺い申請情報を削除する。
        /// </summary>
        /// <param name="ukagaiHeadearId">伺いヘッダID</param>
        private async Task DeleteUkagaisAsync(long ukagaiHeadearId)
        {
            // 伺い申請情報を削除
            var targetUkagaiShinseis = await GetUkagaiShinseisAsync(ukagaiHeadearId);

            for (int i = 0; i < targetUkagaiShinseis.Count; i++)
            {
                // バージョンをセット
                db.SetOriginalValue(targetUkagaiShinseis[i], e => e.Version, ViewModel.UkagaiShinseiVersions[i]);

                db.UkagaiShinseis.Remove(targetUkagaiShinseis[i]);
            }

            // 伺い入力ヘッダを削除
            var targetUkagaiHeader = await GetUkagaiHeaderAsync(ukagaiHeadearId);
            
            if (targetUkagaiHeader is not null)
            {
                // バージョンをセット
                db.SetOriginalValue(targetUkagaiHeader, e => e.Version, ViewModel.UkagaiHeaderVersion);

                db.UkagaiHeaders.Remove(targetUkagaiHeader);
            }
        }

        /// <summary>
        /// 入力情報をDBに登録する
        /// </summary>
        /// <param name="workingHours">勤怠打刻情報</param>
        /// <param name="isNotDairi">代理入力かどうか</param>
        private async Task RegisterWorkingHoursAsync(List<WorkingHour> workingHours, bool isNotDairi)
        {
            // 伺い入力ヘッダを新規作成
            UkagaiHeader ukagaiHeader = new()
            {
                // 社員ID
                //    修正者の社員IDを登録
                SyainId = LoginInfo.User.Id,
                // 申請年月日
                ShinseiYmd = DateTime.Now.ToDateOnly(),
                // ステータス
                Status = isNotDairi ? 承認待 : 承認,
                // 作業日付
                WorkYmd = ViewModel.JissekiDate,
                // 備考
                Biko = ViewModel.SyuseiReason,
                // 無効フラグ
                Invalid = false,
            };

            db.UkagaiHeaders.Add(ukagaiHeader);

            // 伺い申請情報を新規作成
            UkagaiShinsei ukagaiShinsei = new()
            {
                // 伺い種別
                UkagaiSyubetsu = 打刻時間修正,
                // 伺い入力ヘッダID
                UkagaiHeader = ukagaiHeader,
            };

            db.UkagaiShinseis.Add(ukagaiShinsei);

            // 勤怠打刻を新規作成
            for(int i = 0; i < ViewModel.TimeSets.Count; i++)
            {
                var timeset = ViewModel.TimeSets[i];

                if (timeset.Start.AsTimeOnly is null && timeset.End.AsTimeOnly is null) continue;

                // 入力値
                var jissekiDate = ViewModel.JissekiDate;
                var syukkinHour = timeset.Start.Hour;
                var syukkinMinute = timeset.Start.Minute;
                var taikinHour = timeset.End.Hour;
                var taikinMinute = timeset.End.Minute;

                // 退勤時間が00:00の場合
                TimeOnly? taikinTo = (taikinHour is not null && taikinMinute is not null)
                    ? new TimeOnly(taikinHour.Value, taikinMinute.Value)
                    : null;

                var isTaikinNextDay = taikinTo == TimeOnly.MinValue;

                // 新規作成
                WorkingHour workingHour = new()
                {
                    // 社員ID
                    SyainId = ViewModel.SyainId,
                    // 日付
                    Hiduke = ViewModel.JissekiDate,
                    // 出勤緯度
                    SyukkinLatitude = 0,
                    // 出勤経度
                    SyukkinLongitude = 0,
                    // 退勤緯度
                    TaikinLatitude = 0,
                    // 退勤経度
                    TaikinLongitude = 0,
                    // 出勤時間
                    SyukkinTime = (timeset.Start.Hour is null && timeset.Start.Minute is null)
                    ? null
                    : new DateTime
                    (
                        jissekiDate.Year,
                        jissekiDate.Month,
                        jissekiDate.Day,
                        syukkinHour ?? 0,
                        syukkinMinute ?? 0,
                        0
                    ),
                    // 退勤時間
                    //  00:00の場合、実績日付に1日追加する
                    TaikinTime = (timeset.End.Hour is null && timeset.End.Minute is null)
                    ? null
                    : isTaikinNextDay ?
                    new DateTime
                    (
                        jissekiDate.Year,
                        jissekiDate.Month,
                        jissekiDate.Day,
                        taikinHour ?? 0,
                        taikinMinute ?? 0,
                        0
                    ).AddDays(1)
                    :
                    new DateTime
                    (
                        jissekiDate.Year,
                        jissekiDate.Month,
                        jissekiDate.Day,
                        taikinHour ?? 0,
                        taikinMinute ?? 0,
                        0
                    ),
                    // 修正フラグ
                    Edited = true,
                    // 削除フラグ
                    Deleted = false,
                    // 修正社員ID
                    EditSyainId = LoginInfo.User.Id,
                    // 伺い入力ヘッダID
                    UkagaiHeader = ukagaiHeader,
                };

                db.WorkingHours.Add(workingHour);
            }

            // 勤怠打刻を更新
            // 削除フラグ = True の勤怠打刻情報を取得
            var deletedWorkingHours = workingHours.Where(x => x.Deleted).OrderBy(x => x.SyukkinTime).ToList();

            // 初回修正時
            if (deletedWorkingHours.Count == 0)
            {
                var sortedWorkingHours = workingHours.Where(x => !x.Deleted).OrderBy(x => x.SyukkinTime).ToList();
                for (int i = 0; i < sortedWorkingHours.Count; i++)
                {
                    // 削除フラグ
                    sortedWorkingHours[i].Deleted = true;
                    // 修正社員ID
                    sortedWorkingHours[i].EditSyainId = LoginInfo.User.Id;
                    // 伺い入力ヘッダID
                    sortedWorkingHours[i].UkagaiHeader = ukagaiHeader;
                    // バージョンをセット
                    db.SetOriginalValue(sortedWorkingHours[i], e => e.Version, ViewModel.TimeSets[i].Version);
                }
            }
            // 2回目以降の修正時
            else
            {
                for (int i = 0; i < deletedWorkingHours.Count; i++)
                {
                    // 伺い入力ヘッダID
                    deletedWorkingHours[i].UkagaiHeader = ukagaiHeader;
                    // バージョンをセット
                    db.SetOriginalValue(deletedWorkingHours[i], e => e.Version, ViewModel.DeletedTimeSets[i].Version);
                }
            }
        }
        #endregion
    }
}
