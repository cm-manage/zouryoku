using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Model;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Model.Enums.ApprovalStatus;
using static Model.Enums.DailyReportStatusClassification;
using static Model.Enums.InquiryType;

namespace Zouryoku.Pages.KinmuNippouJikanNyuryoku
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
            #region コンボボックス選択肢生成
            /// <summary>
            /// 時間選択肢
            /// </summary>
            public static IEnumerable<SelectListItem> GetHourListItems() =>
                Enumerable.Range(0, MaxHour + 1).Select(i => new SelectListItem
                {
                    Value = i.ToString(""),
                    Text = i.ToString("00"),
                });

            /// <summary>
            /// 分選択肢
            /// </summary>
            /// <returns></returns>
            public static IEnumerable<SelectListItem> GetMinuteListItems() =>
                Enumerable.Range(0, MaxMinute + 1).Select(i => new SelectListItem
                {
                    Value = i.ToString(""),
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
        /// 勤務日報時間入力ViewModel (ViewModel)
        /// </summary>
        [BindProperty]
        public KinmuNippouNyuryokuViewModel ViewModel { get; set; } = new KinmuNippouNyuryokuViewModel();
        #endregion

        #region ViewModel定義
        // ---------------------------------------------
        // ViewModel定義
        // ---------------------------------------------
        /// <summary>勤務日報時間入力ViewModel</summary>
        public class KinmuNippouNyuryokuViewModel
        {
            //----------------------------------------------
            // 入力情報
            //----------------------------------------------
            /// <summary>社員ID</summary>
            public long SyainId { get; set; }

            /// <summary>実績年月日</summary>
            public DateOnly JissekiDate { get; set; }

            /// <summary>代理入力かどうか</summary>
            public bool IsDairiInput { get; set; }

            /// <summary>出退勤ラベルタイトル</summary>
            public static ImmutableArray<string> LabelTitle { get; } = ["出退勤１", "出退勤２", "出退勤３"];

            /// <summary>出退勤時間１～３</summary>
            public List<TimeSet> TimeSets { get; set; } = [new TimeSet(), new TimeSet(), new TimeSet()];

            /// <summary>申請入力</summary>
            [Display(Name = "申請入力")]
            public string? ShinseiInput { get; set; } = string.Empty;

            /// <summary>日報確定済みかどうか</summary>
            public bool IsKakutei => TourokuKubun == 確定保存;

            /// <summary>登録状況区分</summary>
            public DailyReportStatusClassification? TourokuKubun { get; set; }

            /// <summary>
            /// 出退勤時間をセット
            /// </summary>
            /// <param name="ranges">出退勤時間範囲配列</param>
            /// <returns></returns>
            public void SetTimeRanges(TimeRange[] ranges)
            {
                for (int i = 0; i < ranges.Length && i < TimeSets.Count; i++)
                {
                    var r = ranges[i];
                    TimeSets[i].Start.Hour = r.Start?.Hour;
                    TimeSets[i].Start.Minute = r.Start?.Minute;
                    TimeSets[i].End.Hour = r.End?.Hour;
                    TimeSets[i].End.Minute = r.End?.Minute;
                }
            }
        }

        /// <summary>出退勤セットモデル</summary>
        public class TimeSet
        {
            /// <summary>出勤時間</summary>
            public TimeInput Start { get; set; } = new();

            /// <summary>退勤時間</summary>  
            public TimeInput End { get; set; } = new();
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
            public bool IsHalfInput => (Hour is not null && Minute is null) || (Hour is null && Minute is not null);

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

            /// <summary>コンストラクタ</summary>
            /// <param name="start">出勤時間</param>
            /// <param name="end">退勤時間</param>
            public TimeRange(TimeOnly? start, TimeOnly? end)
            {
                Start = start;
                End = end;
            }
        }
        #endregion

        #region 入力内容モデル

        /// <summary>
        /// 入力内容出力モデル
        /// </summary>
        /// <remarks>
        /// インターフェース仕様:
        /// - 社員ID: 社員ID
        /// - 実績年月日: 入力中の実績年月日
        /// - 出勤時間1: 選択した出勤時間1
        /// - 退勤時間1: 選択した退勤時間1
        /// - 出勤時間2: 選択した出勤時間2
        /// - 退勤時間2: 選択した退勤時間2
        /// - 出勤時間3: 選択した出勤時間3
        /// - 退勤時間3: 選択した退勤時間3
        /// - 代理入力フラグ: Inputされた内容をそのままOutputする
        /// </remarks>
        public class TimeInputResult
        {
            /// <summary>社員ID</summary>
            public long SyainId { get; set; }

            /// <summary>実績日</summary>
            public DateOnly JissekiDate { get; set; }

            /// <summary>出勤時間1</summary>
            public TimeOnly? SyukkinTime1 { get; set; }

            /// <summary>退勤時間1</summary>
            public TimeOnly? TaikinTime1 { get; set; }

            /// <summary>出勤時間2</summary>
            public TimeOnly? SyukkinTime2 { get; set; }

            /// <summary>退勤時間2</summary>
            public TimeOnly? TaikinTime2 { get; set; }

            /// <summary>出勤時間3</summary>
            public TimeOnly? SyukkinTime3 { get; set; }

            /// <summary>退勤時間3</summary>
            public TimeOnly? TaikinTime3 { get; set; }

            /// <summary>代理入力かどうか</summary>
            public bool IsDairiInput { get; set; }
        }

        #endregion

        #region コンストラクタ
        // ---------------------------------------------
        // DI（サービス、DB、ロガーなど）
        // ---------------------------------------------
        public IndexModel(ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> options)
            : base(db, logger, options)
        {
        }
        #endregion

        #region GET処理
        /// <summary>GET Index</summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日</param>
        /// <param name="isDairiInput">代理入力かどうか</param>
        /// <returns>ページ情報</returns>
        public async Task<IActionResult> OnGetAsync(long syainId, DateOnly jissekiDate, bool isDairiInput)
        {
            // 検索条件① 日報実績の検索
            // → 日報実績が存在する場合、ViewModelへセットして返却
            var nippou = await GetNippouAsync(syainId, jissekiDate);

            if (nippou != null)
            {
                ViewModel = await BuildViewModelFromNippou(syainId, jissekiDate, nippou, isDairiInput);
                return Page();
            }

            // 検索条件② 勤怠打刻の検索
            // → 勤怠打刻が存在する場合、ViewModelへセットして返却
            var workingHours = await GetWorkingHourAsync(syainId, jissekiDate);
            if (0 < workingHours.Count)
            {
                ViewModel = await BuildViewModelFromWorkingHours(syainId, jissekiDate, workingHours, isDairiInput);
                return Page();
            }

            // 新規
            // → ViewModelを新規作成して返却
            ViewModel = BuildNewViewModel(syainId, jissekiDate, isDairiInput);
            return Page();
        }
        #endregion

        #region POST処理
        /// <summary>登録処理</summary>
        /// <returns>結果</returns>
        public async Task<IActionResult> OnPostRegisterAsync()
        {
            // 関連性チェック
            ValidateRegister();

            if (!ModelState.IsValid)
            {
                return CommonErrorResponse();
            }

            var nippouOutput = ConvertTimeInputResult();
            return SuccessJson(data: nippouOutput);
        }
        #endregion

        #region DB取得処理
        /// <summary>
        /// 日報実績入力を検索する
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日</param>
        /// <returns>日報実績データ</returns>
        private async Task<Nippou?> GetNippouAsync(long syainId, DateOnly jissekiDate)
        {
            Nippou? nippou = await db.Nippous                   // 日報実績
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(x => x.SyainId == syainId && x.NippouYmd == jissekiDate);

            return nippou;
        }

        /// <summary>
        /// 勤怠打刻を検索する。
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日</param>
        /// <returns>勤怠打刻データ</returns>
        private async Task<List<WorkingHour>> GetWorkingHourAsync(long syainId, DateOnly jissekiDate)
        {
            List<WorkingHour> workingHours = await db.WorkingHours                      // 勤怠打刻
                                               .Where(x => x.SyainId == syainId         // 社員ID
                                                        && x.Hiduke == jissekiDate      // 実績日
                                                        && !x.Deleted)                  // 未削除
                                               .OrderBy(x => x.SyukkinTime)
                                               .AsNoTracking()
                                               .ToListAsync();

            // 勤怠打刻データ一覧を返却
            return workingHours;
        }
        #endregion

        #region ViewModel構築
        /// <summary>
        /// 日報実績からViewModelを構築する
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日</param>
        /// <param name="nippou">日報実績情報</param>
        /// <param name="isDairiInput">代理入力かどうか</param>
        /// <returns>Viewモデル</returns>
        private async Task<KinmuNippouNyuryokuViewModel> BuildViewModelFromNippou(
            long syainId,
            DateOnly jissekiDate,
            Nippou nippou,
            bool isDairiInput)
        {
            // ViewModelへ変換
            var viewModel = new KinmuNippouNyuryokuViewModel
            {
                // 社員ID
                SyainId = nippou.SyainId,

                // 実績日 
                JissekiDate = nippou.NippouYmd,

                // 代理入力
                IsDairiInput = isDairiInput,

                // 登録状況区分
                TourokuKubun = nippou.TourokuKubun,
            };
            viewModel.SetTimeRanges(new[]
            {
                new TimeRange(nippou.SyukkinHm1, nippou.TaisyutsuHm1),
                new TimeRange(nippou.SyukkinHm2, nippou.TaisyutsuHm2),
                new TimeRange(nippou.SyukkinHm3, nippou.TaisyutsuHm3)
            });

            // 伺い申請情報取得
            viewModel.ShinseiInput = await BuildUkagaiShinseisInfo(syainId, jissekiDate);

            return viewModel;
        }

        /// <summary>
        /// 勤怠打刻からViewModelを構築する
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績日</param>
        /// <param name="workingHours">勤怠打刻情報</param>
        /// <param name="isDairiInput">代理入力かどうか</param>
        /// <returns>Viewモデル</returns>
        private async Task<KinmuNippouNyuryokuViewModel> BuildViewModelFromWorkingHours(
            long syainId,
            DateOnly jissekiDate,
            List<WorkingHour> workingHours,
            bool isDairiInput)
        {
            var viewModel = new KinmuNippouNyuryokuViewModel
            {
                // 社員ID
                SyainId = syainId,
                // 実績日
                JissekiDate = jissekiDate,
                // 代理入力
                IsDairiInput = isDairiInput,
            };

            var timeRanges = workingHours
                .Select(h => new TimeRange(
                    h.SyukkinTime?.ToTimeOnly(),
                    h.TaikinTime?.ToTimeOnly()))
                .ToArray();
            viewModel.SetTimeRanges(timeRanges);

            // 伺い申請情報取得
            viewModel.ShinseiInput = await BuildUkagaiShinseisInfo(syainId, jissekiDate);

            return viewModel;
        }

        /// <summary>
        /// 新規ViewModel構築
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績年月日</param>
        /// <param name="isDairiInput">代理入力かどうか</param>
        /// <returns>Viewモデル</returns>
        private static KinmuNippouNyuryokuViewModel BuildNewViewModel(long syainId, DateOnly jissekiDate, bool isDairiInput)
        {
            return new KinmuNippouNyuryokuViewModel
            {
                SyainId = syainId,
                JissekiDate = jissekiDate,
                IsDairiInput = isDairiInput,
                ShinseiInput = string.Empty
            };
        }

        /// <summary>
        /// 伺い申請情報生成
        /// 伺い種別を改行区切りで連結する
        /// </summary>
        /// <param name="syainId">社員ID</param>
        /// <param name="jissekiDate">実績年月日</param>
        /// <returns>
        /// 承認済みの伺い種別の表示名を改行区切りで連結した文字列。
        /// 対象となる伺い申請が存在しない場合（承認ステータス以外のみの場合など）は空文字列を返す。
        /// </returns>
        private async Task<string> BuildUkagaiShinseisInfo(long syainId, DateOnly jissekiDate)
        {
            var inquiryTypes = await db.UkagaiShinseis
                .Where(us => 
                    us.UkagaiSyubetsu != 時間外労働時間制限拡張 && // 伺い種別のうち時間外労働時間制限拡張は除く
                    us.UkagaiHeader.SyainId == syainId &&
                    us.UkagaiHeader.WorkYmd == jissekiDate &&
                    us.UkagaiHeader.Status == 承認)
                .OrderBy(us => us.UkagaiSyubetsu)
                .Select(us => us.UkagaiSyubetsu) // 伺い種別を抽出する
                .Distinct() // 重複は排除
                .ToListAsync();

            // 伺い種別の表示名を改行区切りで連結する
            return string.Join(Environment.NewLine, inquiryTypes.Select(t => t.GetDisplayName()));
        }
        #endregion

        #region Validationチェック
        /// <summary>
        /// Validationチェック
        /// </summary>
        private void ValidateRegister()
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
            if (!ModelState.IsValid)
            {
                return;
            }

            // 出勤時間と退勤時間の重複を確認
            ValidateWorkingHourOverlaps(ViewModel.TimeSets, ViewModel.JissekiDate);
            if (!ModelState.IsValid)
            {
                return;
            }

            // 出勤時間と退勤時間の逆転を確認
            ValidateWorkingHourOrder(ViewModel.TimeSets);
            if (!ModelState.IsValid)
            {
                return;
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

            // 出勤と退勤の片方のみ入力チェック
            var syukkin = syukkinTimeInput.AsTimeOnly;
            var taikin = taikinTimeInput.AsTimeOnly;
            if (syukkin.HasValue != taikin.HasValue)
            {
                AddError(string.Format(Const.ErrorInputRequired, inputLabel + "、出勤と退勤の時間両方"));
                return;
            }

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
        private void ValidateWorkingHourOverlaps(List<TimeSet> timeSets, DateOnly baseDate)
        {
            // 有効な入力のみDateTimeに変換
            var ranges = new List<(string Label, DateTime Start, DateTime End)>();

            for (int i = 0; i < timeSets.Count; i++)
            {
                var set = timeSets[i];
                var label = $"出退勤{i + 1}";

                var start = set.Start.AsTimeOnly;
                var end = set.End.AsTimeOnly;

                if (start is null || end is null) continue;

                var startDt = new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, start.Value.Hour, start.Value.Minute, 0);

                // 退勤時間の00:00は翌日扱いする
                var endDate = (end.Value == TimeOnly.MinValue)
                    ? baseDate.AddDays(1)
                    : baseDate;

                var endDt = new DateTime(endDate.Year, endDate.Month, endDate.Day, end.Value.Hour, end.Value.Minute, 0);

                ranges.Add((label, startDt, endDt));
            }

            if (ranges.Count <= 1) return;

            // 並び順を修正
            var ordered = ranges
                .OrderBy(x => x.Start)
                .ThenBy(x => x.End)
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
        /// 出勤時間と退勤時間の逆転を確認する。
        /// </summary>
        /// <param name="timeSets">入力された出退勤時間</param>
        private void ValidateWorkingHourOrder(List<TimeSet> timeSets)
        {
            for (int i = 0; i < timeSets.Count - 1; i++)
            {
                var currentTimeSet = timeSets[i];
                var nextTimeSet = timeSets[i + 1];
                if (currentTimeSet.End.AsTimeOnly > nextTimeSet.Start.AsTimeOnly)
                {
                    ModelState.AddModelError(string.Empty, string.Format(Const.ErrorReverse, $"出退勤{i + 1}と出退勤{i + 2}"));
                    return;
                }
            }
        }
        #endregion

        #region ViewModel→入力値出力
        /// <summary>
        /// 入力情報を出力用JSONに変換する
        /// </summary>
        private TimeInputResult ConvertTimeInputResult()
        {
            // 設計書のインターフェース仕様に合わせて、出退勤時間は個別プロパティである必要がある
            // インターフェース仕様は TimeInputResult クラスのXMLドキュメントを参照
            var nippouOutput = new TimeInputResult
            {
                SyainId = ViewModel.SyainId,
                JissekiDate = ViewModel.JissekiDate,
                IsDairiInput = ViewModel.IsDairiInput,
            };

            for (int i = 0; i < ViewModel.TimeSets.Count; i++)
            {
                var set = ViewModel.TimeSets[i];

                // 設計書のインターフェース仕様に合わせて、1～3件目までを個別プロパティにセットする
                // インターフェース仕様は TimeInputResult クラスのXMLドキュメントを参照
                switch (i)
                {
                    case 0:
                        nippouOutput.SyukkinTime1 = set.Start.AsTimeOnly;
                        nippouOutput.TaikinTime1 = set.End.AsTimeOnly;
                        break;
                    case 1:
                        nippouOutput.SyukkinTime2 = set.Start.AsTimeOnly;
                        nippouOutput.TaikinTime2 = set.End.AsTimeOnly;
                        break;
                    case 2:
                        nippouOutput.SyukkinTime3 = set.Start.AsTimeOnly;
                        nippouOutput.TaikinTime3 = set.End.AsTimeOnly;
                        break;
                }
            }

            return nippouOutput;
        }
        #endregion
    }
}
