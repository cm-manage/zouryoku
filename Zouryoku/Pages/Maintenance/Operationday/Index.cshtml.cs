using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Model;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.Maintenance.Operationday
{
    /// <summary>
    /// 稼働日マスタメンテナンスページモデル
    /// </summary>
    [FunctionAuthorizationAttribute]
    public class IndexModel(ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> optionsAccessor, TimeProvider? timeProvider = null) : BasePageModel<IndexModel>(db, logger, optionsAccessor, timeProvider)
    {
        /// <summary>
        /// 入力画面用共通CSS/JSをレイアウトで読み込むかどうかのフラグ
        /// </summary>
        public override bool UseInputAssets { get; } = true;

        /// <summary>
        /// 選択された年
        /// </summary>
        public int SelectedYear { get; set; }

        /// <summary>
        /// 年選択用セレクトリスト
        /// </summary>
        public List<SelectListItem> YearSelectList { get; set; } = new();

        /// <summary>
        /// 稼働日更新データ
        /// </summary>
        [BindProperty]
        public OperationDayUpdateModel UpdateModel { get; set; } = new();

        /// <summary>
        /// 操作日マスタデータリスト
        /// </summary>
        public List<OperationDayViewModel> OperationDays { get; set; } = new();

        /// <summary>
        /// 年選択用リスト（最大7年前から5年後）
        /// </summary>
        public List<int> YearList { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // 現在の年を取得
            int currentYear = timeProvider.Now().Year;

            // SelectedYearが指定されていない場合は現在の年を設定
            if (SelectedYear == 0)
            {
                SelectedYear = currentYear;
            }

            // 年リストを生成（現在の年から4年前から3年後まで）
            GenerateYearSelectList(currentYear);

            // 選択された年の全日付を生成
            var allDatesInYear = GenerateAllDatesInYear(SelectedYear);

            // 非稼働日情報を取得
            var hikadoubis = await db.Hikadoubis
                .Where(h => h.Ymd.Year == SelectedYear)
                .ToListAsync();

            // 実績確定締め日テーブルから報告確定期限日情報を取得
            var jissekiKakuteiSimebis = await db.JissekiKakuteiSimebis
                .Where(j => j.KakuteiKigenYmd.Year == SelectedYear)
                .ToListAsync();

            // 操作日マスタデータを作成
            OperationDays = allDatesInYear.Select(date =>
            {
                var hikadoubiData = hikadoubis.FirstOrDefault(h => h.Ymd == date);
                var jissekiData = jissekiKakuteiSimebis.FirstOrDefault(j => j.KakuteiKigenYmd == date);

                return new OperationDayViewModel
                {
                    Ymd = date,
                    SyukusaijitsuFlag = hikadoubiData?.SyukusaijitsuFlag ?? HolidayFlag.それ以外,
                    RefreshDay = hikadoubiData?.RefreshDay ?? RefreshDayFlag.それ以外,
                    KakuteiKigenYmd = jissekiData?.KakuteiKigenYmd,
                };
            }).ToList();
            return Page();
        }

        /// <summary>
        /// 年リストを生成してセレクトリストを作成
        /// </summary>
        private void GenerateYearSelectList(int currentYear)
        {
            YearSelectList.Clear();
            for (int i = currentYear - 4; i <= currentYear + 3; i++)
            {
                YearSelectList.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString(),
                    Selected = (i == SelectedYear)
                });
            }
        }

        /// <summary>
        /// 年度変更時に操作日テーブル部分のみを再描画するハンドラ
        /// </summary>
        public async Task<IActionResult> OnGetTableAsync(int year)
        {
            SelectedYear = year;

            if (SelectedYear == 0)
            {
                SelectedYear = timeProvider.Now().Year;
            }

            var allDatesInYear = GenerateAllDatesInYear(SelectedYear);

            var hikadoubis = await db.Hikadoubis
                .Where(h => h.Ymd.Year == SelectedYear)
                .ToListAsync();

            var jissekiKakuteiSimebis = await db.JissekiKakuteiSimebis
                .Where(j => j.KakuteiKigenYmd.Year == SelectedYear)
                .ToListAsync();

            OperationDays = allDatesInYear.Select(date =>
            {
                var hikadoubiData = hikadoubis.FirstOrDefault(h => h.Ymd == date);
                var jissekiData = jissekiKakuteiSimebis.FirstOrDefault(j => j.KakuteiKigenYmd == date);

                return new OperationDayViewModel
                {
                    Ymd = date,
                    SyukusaijitsuFlag = hikadoubiData?.SyukusaijitsuFlag ?? HolidayFlag.それ以外,
                    RefreshDay = hikadoubiData?.RefreshDay ?? RefreshDayFlag.それ以外,
                    KakuteiKigenYmd = jissekiData?.KakuteiKigenYmd,
                };
            }).ToList();

            return new PartialViewResult
            {
                ViewName = "_OperationdayTable",
                ViewData = ViewData
            };
        }

        /// <summary>
        /// 指定の年の全日付を生成（1月1日から12月31日）
        /// </summary>
        private List<DateOnly> GenerateAllDatesInYear(int year)
        {
            var dates = new List<DateOnly>();
            var startDate = new DateOnly(year, 1, 1);
            var endDate = new DateOnly(year, 12, 31);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                dates.Add(date);
            }

            return dates;
        }

        /// <summary>
        /// 稼働日マスタデータを更新
        /// </summary>
        public async Task<IActionResult> OnPostRegisterAsync()
        {
            // バリデーションチェック
            // 必須チェック
            if (!UpdateModel.Year.HasValue)
            {
                ModelState.AddError(() => UpdateModel.Year, string.Format(ZouryokuCommonLibrary.Utils.Const.ErrorRequired, "年度"));
            }

            // 実績確定締め日入力チェック
            if (UpdateModel.OperationDays.Count > 0)
            {
                // 年月ごとにグループ化
                var groupedByMonth = UpdateModel.OperationDays
                    .Select((x, index) => new { Item = x, Index = index })
                    .GroupBy(x => new { x.Item.Ymd.Year, x.Item.Ymd.Month });

                foreach (var monthGroup in groupedByMonth)
                {
                    // 対象月の1〜15日 / 16日〜月末 のフラグ付き要素を取得（インデックス付き）
                    var firstHalf = monthGroup
                        .Where(x => x.Item.IsKakuteiKigen && x.Item.Ymd.Day <= 15)
                        .ToList();

                    var secondHalf = monthGroup
                        .Where(x => x.Item.IsKakuteiKigen && x.Item.Ymd.Day >= 16)
                        .ToList();

                    var ym = new DateOnly(monthGroup.Key.Year, monthGroup.Key.Month, 1);

                    // 1〜15日で2つ以上 true → エラー（該当行にエラーを付与）
                    if (firstHalf.Count > 1)
                    {
                        var message = $"{ym:yyyy年M月}の1日〜15日に設定できる実績確定締め日は1日だけです。";

                        foreach (var item in firstHalf)
                        {
                            int idx = item.Index;
                            ModelState.AddError(
                                () => UpdateModel.OperationDays[idx].IsKakuteiKigen,
                                message);
                        }
                    }

                    // 16日〜月末で2つ以上 true → エラー（該当行にエラーを付与）
                    if (secondHalf.Count > 1)
                    {
                        var message = $"{ym:yyyy年M月}の16日〜月末に設定できる実績確定締め日は1日だけです。";

                        foreach (var item in secondHalf)
                        {
                            int idx = item.Index;
                            ModelState.AddError(
                                () => UpdateModel.OperationDays[idx].IsKakuteiKigen,
                                message);
                        }
                    }

                    // 1〜15日に1件もないのに、16日〜月末に true が1件以上 → エラー
                    if (firstHalf.Count == 0 && secondHalf.Count > 0)
                    {
                        var message = $"{ym:yyyy年M月}1日〜15日に実績確定締め日がない場合、16日以降に設定することはできません。";

                        foreach (var item in secondHalf)
                        {
                            int idx = item.Index;
                            ModelState.AddError(
                                () => UpdateModel.OperationDays[idx].IsKakuteiKigen,
                                message);
                        }
                    }
                }
            }


            var errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            int targetYear = UpdateModel.Year!.Value;
            var checkedHikadoubis = new List<OperationDayDataModel>();
            var checkedKakuteiKigenDates = new List<DateOnly>();

            // フォームから修正されたデータを取得
            foreach (var operationDay in UpdateModel.OperationDays)
            {
                // リフレッシュデー　祝祭日
                if (operationDay.IsSyukusaijitsu || operationDay.IsRefreshDay)
                {
                    checkedHikadoubis.Add(new OperationDayDataModel
                    {
                        Ymd = operationDay.Ymd,
                        IsSyukusaijitsu = operationDay.IsSyukusaijitsu,
                        IsRefreshDay = operationDay.IsRefreshDay,
                    });
                }
                // 実績確定締め日
                if (operationDay.IsKakuteiKigen)
                {
                    checkedKakuteiKigenDates.Add(operationDay.Ymd);
                }
            }

            // Hikadoubis テーブルの更新処理（リフレッシュデー　祝祭日の更新）
            // 該当年の既存データをすべて削除
            var existingHikadoubis = await db.Hikadoubis
                .Where(h => h.Ymd.Year == targetYear)
                .ToListAsync();
            db.Hikadoubis.RemoveRange(existingHikadoubis);

            // フォームから送信されたデータから、祝祭日またはリフレッシュデーが true の日付を処理
            if (checkedHikadoubis.Count > 0)
            {
                foreach (var operationDay in checkedHikadoubis)
                {
                    var hikadoubi = new Hikadoubi
                    {
                        Ymd = operationDay.Ymd,
                        SyukusaijitsuFlag = operationDay.IsSyukusaijitsu ? HolidayFlag.祝祭日 : HolidayFlag.それ以外,
                        RefreshDay = operationDay.IsRefreshDay ? RefreshDayFlag.リフレッシュデー : RefreshDayFlag.それ以外
                    };
                    await db.Hikadoubis.AddAsync(hikadoubi);
                }
            }

            // JissekiKakuteiSimebis テーブルの更新処理 （実績確定締め日の更新）
            // 対象年度の既存 実績確定締め日レコードを取得
            var existingJissekiKakuteiSimebis = await db.JissekiKakuteiSimebis
                .Where(j => j.KakuteiKigenYmd.Year == targetYear)
                .ToListAsync();

            foreach(var day in checkedKakuteiKigenDates)
            {
                var existing = existingJissekiKakuteiSimebis
                        .FirstOrDefault(x => x.KakuteiKigenYmd == day);

                if (existing is null)
                {
                    // 新規追加
                    var jissekiKakuteiSimebi = new JissekiKakuteiSimebi
                    {
                        KakuteiKigenYmd = day
                    };
                    await db.JissekiKakuteiSimebis.AddAsync(jissekiKakuteiSimebi);
                }
            }

            // 画面側でチェックが外された締め日のレコードは削除する（確定期限日ベース）
            var toDelete = existingJissekiKakuteiSimebis
                .Where(x => !checkedKakuteiKigenDates.Contains(x.KakuteiKigenYmd))
                .ToList();

            db.JissekiKakuteiSimebis.RemoveRange(toDelete);

            await db.SaveChangesAsync();
            return SuccessJson();
        }
    }

    /// <summary>
    /// 操作日マスタ表示用ViewModel
    /// </summary>
    public class OperationDayViewModel
    {
        /// <summary>
        /// 年月日
        /// </summary>
        public DateOnly Ymd { get; set; }

        public string DisplayYmd
        {
            get
            {
                var dateTime = Ymd.ToDateTime();
                return dateTime.ToString("yyyy/MM/dd(ddd)");
            }
        }

        /// <summary>
        /// 祝祭日フラグ
        /// </summary>
        public HolidayFlag SyukusaijitsuFlag { get; set; }

        /// <summary>
        /// リフレッシュデーフラグ
        /// </summary>
        public RefreshDayFlag RefreshDay { get; set; }

        /// <summary>
        /// 報告確定期限日
        /// </summary>
        public DateOnly? KakuteiKigenYmd { get; set; }


        /// <summary>
        /// 祝祭日区分の表示文字列
        /// </summary>
        public string SyukusaijitsuDisplay
        {
            get
            {
                var dateTime = Ymd.ToDateTime(TimeOnly.MinValue);
                var dayOfWeek = dateTime.DayOfWeek;

                // データベースで祝祭日フラグが設定されている場合
                if (SyukusaijitsuFlag == HolidayFlag.祝祭日)
                {
                    return "祝日";
                }

                // 曜日で判定
                return dayOfWeek switch
                {
                    System.DayOfWeek.Saturday => "土曜",
                    System.DayOfWeek.Sunday => "日曜",
                    _ => "平日"
                };
            }
        }

        /// <summary>
        /// リフレッシュデーは1の場合○を表示
        /// </summary>
        public string RefreshDayDisplay => RefreshDay == RefreshDayFlag.リフレッシュデー ? "○" : "";

        /// <summary>
        /// 報告確定期限日の表示文字列
        /// </summary>
        public string KakuteiKigenYmdDisplay => Ymd == KakuteiKigenYmd ? "○" : "";

        /// <summary>
        /// 行のCSSクラス名を取得
        /// </summary>
        public string RowClass
        {
            get
            {
                var dateTime = Ymd.ToDateTime(TimeOnly.MinValue);

                // 祝祭日フラグが1の場合
                if (SyukusaijitsuFlag == HolidayFlag.祝祭日)
                {
                    return "holiday-row";
                }

                // 曜日で判定
                var dayOfWeek = dateTime.DayOfWeek;
                return dayOfWeek switch
                {
                    System.DayOfWeek.Saturday => "saturday-row",
                    System.DayOfWeek.Sunday => "holiday-row",
                    _ => "workday-row"
                };
            }
        }
    }

    /// <summary>
    /// 稼働日更新用ViewModel
    /// </summary>
    public class OperationDayUpdateModel
    {
        /// <summary>
        /// 年
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// 操作日データリスト（インデックスベース）
        /// </summary>
        public List<OperationDayDataModel> OperationDays { get; set; } = [];
    }

    /// <summary>
    /// 稼働日単一日付のデータ
    /// </summary>
    public class OperationDayDataModel
    {
        /// <summary>
        /// 年月日
        /// </summary>
        public DateOnly Ymd { get; set; }

        /// <summary>
        /// 祝祭日フラグ（true：祝祭日、false：それ以外）
        /// </summary>
        public bool IsSyukusaijitsu { get; set; }

        /// <summary>
        /// リフレッシュデーフラグ（true：リフレッシュデー、false：それ以外）
        /// </summary>
        public bool IsRefreshDay { get; set; }

        /// <summary>
        /// 確定期限日フラグ（true：確定期限日、false：それ以外）
        /// </summary>
        public bool IsKakuteiKigen { get; set; }
    }
}
