using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using Model.Model;
using System.Text;
using Zouryoku.Pages.KinmuNippouJikanNyuryoku;
using Zouryoku.Pages.Shared;
using Zouryoku.Utils;
using static Model.Enums.ResponseStatus;
using static Zouryoku.Pages.KinmuNippouJikanNyuryoku.IndexModel;

namespace ZouryokuTest.Pages.KinmuNippouJikanNyuryoku
{
    /// <summary>
    /// 勤務日報時間入力 テストクラス
    /// </summary>
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {
        private static DateOnly DefaultTestDate => new(2026, 1, 1);

        #region テストデータ生成
        /// <summary>
        /// 日報データ作成(モック)
        /// </summary>
        /// <param name="id">日報実績ID</param>
        /// <param name="syainId">社員ID</param>
        /// <param name="nippouYmd">実績年月日</param>
        /// <param name="syukkin1">出勤１</param>
        /// <param name="taisyutsu1">退勤１</param>
        /// <param name="syukkin2">出勤２</param>
        /// <param name="taisyutsu2">退勤２</param>
        /// <param name="syukkin3">出勤３</param>
        /// <param name="taisyutsu3">退勤３</param>
        /// <param name="tourokuKubun">登録状況区分</param>
        /// <returns>日報データ</returns>
        private static Nippou CreateNippou(
            int id = 1,
            int syainId = 1,
            DateOnly? nippouYmd = null,
            (int hour, int minute)? syukkin1 = null, (int hour, int minute)? taisyutsu1 = null,
            (int hour, int minute)? syukkin2 = null, (int hour, int minute)? taisyutsu2 = null,
            (int hour, int minute)? syukkin3 = null, (int hour, int minute)? taisyutsu3 = null,
            DailyReportStatusClassification tourokuKubun = default)
        {
            var d = nippouYmd ?? DefaultTestDate;

            return new Nippou
            {
                Id = id,
                SyainId = syainId,
                NippouYmd = d,
                SyukkinHm1 = syukkin1.HasValue ? new TimeOnly(syukkin1.Value.hour, syukkin1.Value.minute, 0) : null,
                TaisyutsuHm1 = taisyutsu1.HasValue ? new TimeOnly(taisyutsu1.Value.hour, taisyutsu1.Value.minute, 0) : null,
                SyukkinHm2 = syukkin2.HasValue ? new TimeOnly(syukkin2.Value.hour, syukkin2.Value.minute, 0) : null,
                TaisyutsuHm2 = taisyutsu2.HasValue ? new TimeOnly(taisyutsu2.Value.hour, taisyutsu2.Value.minute, 0) : null,
                SyukkinHm3 = syukkin3.HasValue ? new TimeOnly(syukkin3.Value.hour, syukkin3.Value.minute, 0) : null,
                TaisyutsuHm3 = taisyutsu3.HasValue ? new TimeOnly(taisyutsu3.Value.hour, taisyutsu3.Value.minute, 0) : null,
                TourokuKubun = tourokuKubun,
            };
        }

        /// <summary>
        /// 勤怠打刻データ作成(モック)
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="syainId">社員ID</param>
        /// <param name="date">実績日</param>
        /// <param name="startHour">出勤時間(時)</param>
        /// <param name="startMinute">出勤時間(分)</param>
        /// <param name="endHour">退勤時間(時)</param>
        /// <param name="endMinute">退勤時間(分)</param>
        /// <param name="deleted">削除フラグ</param>
        /// <returns>勤怠打刻データ</returns>
        private static WorkingHour CreateWorkingHour(
            int id, int syainId, DateOnly date,
            int startHour, int startMinute, int endHour, int endMinute, bool deleted = false) => new()
            {
                Id = id,
                SyainId = syainId,
                Hiduke = date,
                SyukkinTime = new DateTime(date.Year, date.Month, date.Day, startHour, startMinute, 0),
                TaikinTime = new DateTime(date.Year, date.Month, date.Day, endHour, endMinute, 0),
                Deleted = deleted
            };

        /// <summary>
        /// 伺い申請ヘッダ生成
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="syainId">社員ID</param>
        /// <param name="workYmd">作業日付</param>
        /// <param name="approvalStatus">ステータス</param>
        /// <returns>伺い申請ヘッダ</returns>
        private static UkagaiHeader CreateUkagaiHeader(long id, long syainId, DateOnly workYmd, ApprovalStatus approvalStatus) => new()
        {
            Id = id,
            SyainId = syainId,
            WorkYmd = workYmd,
            Status = approvalStatus
        };

        /// <summary>
        /// 伺い申請情報詳細生成
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="ukagaiHeaderId">伺いヘッダID</param>
        /// <param name="ukagaiSyubetsu">伺い種別</param>
        /// <returns>伺い申請情報</returns>
        private static UkagaiShinsei CreateUkagaiShinsei(long id, long ukagaiHeaderId, InquiryType ukagaiSyubetsu) => new()
        {
            Id = id,
            UkagaiHeaderId = ukagaiHeaderId,
            UkagaiSyubetsu = ukagaiSyubetsu
        };
        #endregion

        #region Assertチェック
        /// <summary>
        /// 時間入力値テスト
        /// </summary>
        /// <param name="actual">実行結果</param>
        /// <param name="hour">想定値(時)</param>
        /// <param name="minute">想定値(分)</param>
        private static void AssertTime(TimeInput actual, int? hour, int? minute)
        {
            Assert.AreEqual(hour, actual.Hour);
            Assert.AreEqual(minute, actual.Minute);
        }

        /// <summary>
        /// 時間入力値テスト(出勤時間・退勤時間)
        /// </summary>
        /// <param name="actual">実行結果</param>
        /// <param name="startTime">出勤時間</param>
        /// <param name="endTime">退勤時間</param>
        private static void AssertTimeSetEquals(TimeSet actual, (int? hour, int? minute) startTime, (int? hour, int? minute) endTime)
        {
            AssertTime(actual.Start, startTime.hour, startTime.minute);
            AssertTime(actual.End, endTime.hour, endTime.minute);
        }

        /// <summary>
        /// 入力内容出力モデルテスト
        /// </summary>
        /// <param name="expectedSyainId">社員ID</param>
        /// <param name="expectedJissekiDate">実績日</param>
        /// <param name="expectedIsDairiInput">代理入力かどうか</param>
        /// <param name="actualActionResult">実行結果</param>
        private static void AssertTimeSetResultEquals(
            long expectedSyainId, DateOnly expectedJissekiDate, bool expectedIsDairiInput,
            TimeOnly? expectedSyukkinTime1, TimeOnly? expectedTaikinTime1,
            TimeOnly? expectedSyukkinTime2, TimeOnly? expectedTaikinTime2,
            TimeOnly? expectedSyukkinTime3, TimeOnly? expectedTaikinTime3,
            IActionResult actualActionResult)
        {
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(actualActionResult, "JsonResult が返るべきです。");
            var responseJson = Assert.IsInstanceOfType<ResponseJson>(jsonResult.Value, "ResponseJson が返るべきです。");
            Assert.AreEqual(正常, responseJson.Status, "ステータスが一致しません。");

            var timeInputResult = Assert.IsInstanceOfType<TimeInputResult>(responseJson.Data, "TimeInputResult が返るべきです。");
            Assert.AreEqual(expectedSyainId, timeInputResult.SyainId, "SyainId が一致しません。");
            Assert.AreEqual(expectedJissekiDate, timeInputResult.JissekiDate, "JissekiDate が一致しません。");
            Assert.AreEqual(expectedSyukkinTime1, timeInputResult.SyukkinTime1, "SyukkinTime1 が一致しません。");
            Assert.AreEqual(expectedTaikinTime1, timeInputResult.TaikinTime1, "TaikinTime1 が一致しません。");
            Assert.AreEqual(expectedSyukkinTime2, timeInputResult.SyukkinTime2, "SyukkinTime2 が一致しません。");
            Assert.AreEqual(expectedTaikinTime2, timeInputResult.TaikinTime2, "TaikinTime2 が一致しません。");
            Assert.AreEqual(expectedSyukkinTime3, timeInputResult.SyukkinTime3, "SyukkinTime3 が一致しません。");
            Assert.AreEqual(expectedTaikinTime3, timeInputResult.TaikinTime3, "TaikinTime3 が一致しません。");
            Assert.AreEqual(expectedIsDairiInput, timeInputResult.IsDairiInput, "IsDairiInput が一致しません。");
        }
        #endregion

        #region テスト用モデル生成
        /// <summary>
        /// テスト用のページモデルを作成します。<see cref="IndexModel.ViewModel"/> は IndexModel のコンストラクタで初期化されます。
        /// </summary>
        /// <returns>テストで使用する IndexModel インスタンス。</returns>
        private IndexModel CreateModel() => new(db, GetLogger<IndexModel>(), options)
        {
            PageContext = GetPageContext(),
            TempData = GetTempData()
        };
        #endregion

        #region テスト用入力データ生成
        /// <summary>
        /// 出退勤時間データ作成
        /// </summary>
        /// <param name="startHour">出勤時間「時」</param>
        /// <param name="startMinute">出勤時間「分」</param>
        /// <param name="endHour">退勤時間「時」</param>
        /// <param name="endMinute">退勤時間「分」</param>
        /// <returns>
        /// 出退勤時間データ<see cref="TimeSet"/>
        /// </returns>
        private static TimeSet CreateTimeSet(int? startHour, int? startMinute, int? endHour, int? endMinute) => new()
        {
            Start = new TimeInput { Hour = startHour, Minute = startMinute },
            End = new TimeInput { Hour = endHour, Minute = endMinute },
        };
        #endregion

        #region テストケース１ 日報実績あり・勤怠打刻データありの時、日報実績データからデータ取得されること
        /// <summary>
        /// テストケース１ 日報実績あり・勤怠打刻データありの時、日報実績データからデータ取得されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_日報実績あり_打刻データあり_日報実績データを取得()
        {
            // Given
            var nippou = CreateNippou(
                syukkin1: (9, 30), taisyutsu1: (12, 45),
                syukkin2: (13, 28), taisyutsu2: (18, 16),
                syukkin3: (19, 42), taisyutsu3: (20, 30)
            );

            var workingHours = new[]
            {
                CreateWorkingHour(1, 1, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(2, 1, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(3, 1, DefaultTestDate, 17, 37, 18, 28),
            };

            db.Nippous.Add(nippou);
            db.WorkingHours.AddRange(workingHours);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // When
            await model.OnGetAsync(1, new DateOnly(2026, 1, 1), false);

            // Then
            // 出勤・退勤時間１
            AssertTimeSetEquals(model.ViewModel.TimeSets[0], (9, 30), (12, 45));

            // 出勤・退勤時間２
            AssertTimeSetEquals(model.ViewModel.TimeSets[1], (13, 28), (18, 16));

            // 出勤・退勤時間３
            AssertTimeSetEquals(model.ViewModel.TimeSets[2], (19, 42), (20, 30));
        }
        #endregion

        #region テストケース２ 日報実績あり・勤怠打刻データなしの時、日報実績データからデータ取得されること
        /// <summary>
        /// テストケース２ 日報実績あり・勤怠打刻データなしの時、日報実績データからデータ取得されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_日報実績あり_勤怠打刻データなし_日報実績データを取得()
        {
            // Given
            var nippou = CreateNippou(
                syukkin1: (9, 30), taisyutsu1: (12, 45),
                syukkin2: (13, 28), taisyutsu2: (18, 16),
                syukkin3: (19, 42), taisyutsu3: (20, 30)
            );

            db.Nippous.Add(nippou);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // When
            await model.OnGetAsync(1, new DateOnly(2026, 1, 1), false);

            // Then
            // 出勤・退勤時間１
            AssertTimeSetEquals(model.ViewModel.TimeSets[0], (9, 30), (12, 45));

            // 出勤・退勤時間２
            AssertTimeSetEquals(model.ViewModel.TimeSets[1], (13, 28), (18, 16));

            // 出勤・退勤時間３
            AssertTimeSetEquals(model.ViewModel.TimeSets[2], (19, 42), (20, 30));
        }
        #endregion

        #region テストケース３ 日報実績なし・勤怠打刻データありの時、勤怠打刻データからデータ取得されること
        /// <summary>
        /// テストケース３ 日報実績なし・勤怠打刻データありの時、勤怠打刻データからデータ取得されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_日報実績なし_勤怠打刻データありの時_勤怠打刻データを取得()
        {
            // Given
            var nippou = CreateNippou(
                nippouYmd: new DateOnly(2026, 1, 2),
                syukkin1: (9, 30), taisyutsu1: (12, 45),
                syukkin2: (13, 28), taisyutsu2: (18, 16),
                syukkin3: (19, 42), taisyutsu3: (20, 30)
            );

            var workingHours = new[]
            {
                CreateWorkingHour(1, 1, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(2, 1, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(3, 1, DefaultTestDate, 17, 37, 18, 28),
            };

            db.Nippous.Add(nippou);
            db.WorkingHours.AddRange(workingHours);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // When
            await model.OnGetAsync(1, new DateOnly(2026, 1, 1), false);

            // Then
            // 出勤・退勤時間１
            AssertTimeSetEquals(model.ViewModel.TimeSets[0], (10, 24), (11, 12));

            // 出勤・退勤時間２
            AssertTimeSetEquals(model.ViewModel.TimeSets[1], (12, 18), (16, 48));

            // 出勤・退勤時間３
            AssertTimeSetEquals(model.ViewModel.TimeSets[2], (17, 37), (18, 28));
        }
        #endregion

        #region テストケース４ 日報実績なし・勤怠打刻データあり(削除フラグ＝TRUEのデータを含む)の時、勤怠打刻データからデータ取得されること
        /// <summary>
        /// テストケース４ 日報実績なし・勤怠打刻データあり(削除フラグ＝TRUEのデータを含む)の時、勤怠打刻データからデータ取得されること
        /// ただし、削除フラグがTrueのデータは出力対象外
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_日報実績なし_勤怠打刻データあり_削除データ含む_勤怠打刻データを取得()
        {
            // Given
            var workingHours = new[]
            {
                CreateWorkingHour(1, 1, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(2, 1, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(3, 1, DefaultTestDate, 17, 37, 18, 28, true),
            };

            db.WorkingHours.AddRange(workingHours);
            await db.SaveChangesAsync();

            // When
            var model = CreateModel();
            await model.OnGetAsync(1, new DateOnly(2026, 1, 1), false);

            // Then
            // 出勤・退勤時間１
            AssertTimeSetEquals(model.ViewModel.TimeSets[0], (10, 24), (11, 12));

            // 出勤・退勤時間２
            AssertTimeSetEquals(model.ViewModel.TimeSets[1], (12, 18), (16, 48));

            // 出勤・退勤時間３
            AssertTimeSetEquals(model.ViewModel.TimeSets[2], (null, null), (null, null));
        }
        #endregion

        #region テストケース５ 日報実績なし・勤怠打刻データなし
        /// <summary>
        /// テストケース５ 日報実績なし・勤怠打刻データなし
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_日報実績なし_勤怠打刻データなし_nullになる()
        {
            // Given
            var nippou = CreateNippou(
                nippouYmd: new DateOnly(2026, 1, 2),
                syukkin1: (9, 30), taisyutsu1: (12, 45),
                syukkin2: (13, 28), taisyutsu2: (18, 16),
                syukkin3: (19, 42), taisyutsu3: (20, 30)
            );

            var workingHours = new[]
            {
                CreateWorkingHour(1, 1, new DateOnly(2026, 1, 2), 10, 24, 11, 12),
                CreateWorkingHour(2, 1, new DateOnly(2026, 1, 2), 12, 18, 16, 48),
                CreateWorkingHour(3, 1, new DateOnly(2026, 1, 2), 17, 37, 18, 28),
            };

            db.Nippous.Add(nippou);
            db.WorkingHours.AddRange(workingHours);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // When
            await model.OnGetAsync(1, new DateOnly(2026, 1, 1), false);

            // Then
            // 出勤・退勤時間１
            AssertTimeSetEquals(model.ViewModel.TimeSets[0], (null, null), (null, null));

            // 出勤・退勤時間２
            AssertTimeSetEquals(model.ViewModel.TimeSets[1], (null, null), (null, null));

            // 出勤・退勤時間３
            AssertTimeSetEquals(model.ViewModel.TimeSets[2], (null, null), (null, null));
        }
        #endregion

        #region テストケース６ 日報実績の社員ID違い
        /// <summary>
        /// テストケース６ 日報実績の社員ID違い
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_日報実績データ社員ID違い_データ取得できない()
        {
            // Given
            db.Nippous.AddRange(
                CreateNippou(
                    id: 1,
                    syainId: 1,
                    syukkin1: (9, 30), taisyutsu1: (12, 45),
                    syukkin2: (13, 28), taisyutsu2: (18, 16),
                    syukkin3: (19, 42), taisyutsu3: (20, 30)
                ),
                CreateNippou(
                    id: 2,
                    syainId: 2,
                    syukkin1: (9, 30), taisyutsu1: (12, 45),
                    syukkin2: (13, 28), taisyutsu2: (18, 16),
                    syukkin3: (19, 42), taisyutsu3: (20, 30)
                ));
            db.WorkingHours.AddRange(
                CreateWorkingHour(1, 1, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(2, 1, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(3, 1, DefaultTestDate, 17, 37, 18, 28),
                CreateWorkingHour(4, 2, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(5, 2, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(6, 2, DefaultTestDate, 17, 37, 18, 28));
            await db.SaveChangesAsync();

            var model = CreateModel();

            // When
            await model.OnGetAsync(3, new DateOnly(2026, 1, 1), false);

            // Then
            // 出勤・退勤時間１
            AssertTimeSetEquals(model.ViewModel.TimeSets[0], (null, null), (null, null));

            // 出勤・退勤時間２
            AssertTimeSetEquals(model.ViewModel.TimeSets[1], (null, null), (null, null));

            // 出勤・退勤時間３
            AssertTimeSetEquals(model.ViewModel.TimeSets[2], (null, null), (null, null));
        }
        #endregion

        #region テストケース７ 日報実績の実績日違い
        /// <summary>
        /// テストケース７ 日報実績の実績日違い
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_日報実績あり_実績日違い_データ取得できない()
        {
            // Given
            var nippous = new[]
            {
                CreateNippou(
                    syukkin1: (9, 30), taisyutsu1: (12, 45),
                    syukkin2: (13, 28), taisyutsu2: (18, 16),
                    syukkin3: (19, 42), taisyutsu3: (20, 30)
                ),
                CreateNippou(
                    id: 2,
                    syainId: 2,
                    syukkin1: (9, 30), taisyutsu1: (12, 45),
                    syukkin2: (13, 28), taisyutsu2: (18, 16),
                    syukkin3: (19, 42), taisyutsu3: (20, 30)
                ),
            };

            var workingHours = new[]
            {
                CreateWorkingHour(1, 1, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(2, 1, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(3, 1, DefaultTestDate, 17, 37, 18, 28),
                CreateWorkingHour(4, 2, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(5, 2, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(6, 2, DefaultTestDate, 17, 37, 18, 28),
            };

            db.Nippous.AddRange(nippous);
            db.WorkingHours.AddRange(workingHours);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // When
            await model.OnGetAsync(1, new DateOnly(2026, 1, 2), false);

            // Then
            // 出勤・退勤時間１
            AssertTimeSetEquals(model.ViewModel.TimeSets[0], (null, null), (null, null));

            // 出勤・退勤時間２
            AssertTimeSetEquals(model.ViewModel.TimeSets[1], (null, null), (null, null));

            // 出勤・退勤時間３
            AssertTimeSetEquals(model.ViewModel.TimeSets[2], (null, null), (null, null));
        }
        #endregion

        #region テストケース８ 勤怠打刻の社員ID違い
        /// <summary>
        /// テストケース８ 勤怠打刻の社員ID違い
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_勤怠打刻社員ID違い_データ取得できない()
        {
            // Given
            var workingHours = new[]
            {
                CreateWorkingHour(1, 1, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(2, 1, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(3, 1, DefaultTestDate, 17, 37, 18, 28),
                CreateWorkingHour(4, 2, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(5, 2, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(6, 2, DefaultTestDate, 17, 37, 18, 28),
            };

            db.WorkingHours.AddRange(workingHours);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // When
            await model.OnGetAsync(3, new DateOnly(2026, 1, 1), false);

            // Then
            // 出勤・退勤時間１
            AssertTimeSetEquals(model.ViewModel.TimeSets[0], (null, null), (null, null));

            // 出勤・退勤時間２
            AssertTimeSetEquals(model.ViewModel.TimeSets[1], (null, null), (null, null));

            // 出勤・退勤時間３
            AssertTimeSetEquals(model.ViewModel.TimeSets[2], (null, null), (null, null));
        }
        #endregion

        #region テストケース９ 勤怠打刻の実績日違い
        /// <summary>
        /// テストケース９ 勤怠打刻の実績日違い
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_勤怠打刻実績日違い_データが取得できない()
        {
            // Given
            var workingHours = new[]
            {
                CreateWorkingHour(1, 1, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(2, 1, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(3, 1, DefaultTestDate, 17, 37, 18, 28),
                CreateWorkingHour(4, 2, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(5, 2, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(6, 2, DefaultTestDate, 17, 37, 18, 28),
            };

            db.WorkingHours.AddRange(workingHours);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // When
            await model.OnGetAsync(1, new DateOnly(2026, 1, 2), false);

            // Then
            // 出勤・退勤時間１
            AssertTimeSetEquals(model.ViewModel.TimeSets[0], (null, null), (null, null));

            // 出勤・退勤時間２
            AssertTimeSetEquals(model.ViewModel.TimeSets[1], (null, null), (null, null));

            // 出勤・退勤時間３
            AssertTimeSetEquals(model.ViewModel.TimeSets[2], (null, null), (null, null));
        }
        #endregion

        #region テストケース１０ 伺い申請情報のステータスが承認待ち
        /// <summary>
        /// テストケース１０ 伺い申請情報のステータスが承認待ちの場合、出力は空となること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_伺い申請情報のステータスが承認待ち_申請情報は空になる()
        {
            // Given
            var workingHours = new[]
            {
                CreateWorkingHour(1, 1, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(2, 1, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(3, 1, DefaultTestDate, 17, 37, 18, 28),
                CreateWorkingHour(4, 2, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(5, 2, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(6, 2, DefaultTestDate, 17, 37, 18, 28),
            };

            var ukagaiHeaders = new[]
            {
                CreateUkagaiHeader(1, 1, DefaultTestDate, ApprovalStatus.承認待),
            };

            var ukagaiShinseis = new[]
            {
                CreateUkagaiShinsei(1, 1, InquiryType.リフレッシュデー残業),
                CreateUkagaiShinsei(2, 1, InquiryType.休日出勤),
                CreateUkagaiShinsei(3, 1, InquiryType.時間外労働時間制限拡張),
                CreateUkagaiShinsei(4, 1, InquiryType.夜間作業),
                CreateUkagaiShinsei(5, 1, InquiryType.早朝作業),
                CreateUkagaiShinsei(6, 1, InquiryType.深夜作業),
                CreateUkagaiShinsei(7, 1, InquiryType.休暇申請),
                CreateUkagaiShinsei(8, 1, InquiryType.テレワーク),
                CreateUkagaiShinsei(9, 1, InquiryType.打刻時間修正),
            };

            db.WorkingHours.AddRange(workingHours);
            db.UkagaiHeaders.AddRange(ukagaiHeaders);
            db.UkagaiShinseis.AddRange(ukagaiShinseis);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // When
            await model.OnGetAsync(1, new DateOnly(2026, 1, 1), false);

            // Then
            // 申請入力
            Assert.AreEqual(string.Empty, model.ViewModel.ShinseiInput);
        }
        #endregion

        #region テストケース１１ 伺い申請情報のステータスが承認済
        /// <summary>
        /// テストケース１１ 伺い申請情報のステータスが承認済の場合、時間外労働時間制限拡張以外が出力されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_伺い申請情報のステータスが承認済_時間外労働時間制限拡張以外を出力()
        {
            // Given
            var workingHours = new[]
            {
                CreateWorkingHour(1, 1, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(2, 1, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(3, 1, DefaultTestDate, 17, 37, 18, 28),
                CreateWorkingHour(4, 2, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(5, 2, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(6, 2, DefaultTestDate, 17, 37, 18, 28),
            };

            var ukagaiHeaders = new[]
            {
                CreateUkagaiHeader(1, 1, DefaultTestDate, ApprovalStatus.承認),
            };

            var ukagaiShinseis = new[]
            {
                CreateUkagaiShinsei(1, 1, InquiryType.リフレッシュデー残業),
                CreateUkagaiShinsei(2, 1, InquiryType.休日出勤),
                CreateUkagaiShinsei(3, 1, InquiryType.時間外労働時間制限拡張),
                CreateUkagaiShinsei(4, 1, InquiryType.夜間作業),
                CreateUkagaiShinsei(5, 1, InquiryType.早朝作業),
                CreateUkagaiShinsei(6, 1, InquiryType.深夜作業),
                CreateUkagaiShinsei(7, 1, InquiryType.休暇申請),
                CreateUkagaiShinsei(8, 1, InquiryType.テレワーク),
                CreateUkagaiShinsei(9, 1, InquiryType.打刻時間修正),
            };

            db.WorkingHours.AddRange(workingHours);
            db.UkagaiHeaders.AddRange(ukagaiHeaders);
            db.UkagaiShinseis.AddRange(ukagaiShinseis);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // When
            await model.OnGetAsync(1, new DateOnly(2026, 1, 1), false);

            // Then
            var expected = new StringBuilder()
                            .AppendLine("リフレッシュデー残業")
                            .AppendLine("休日出勤")
                            .AppendLine("夜間作業")
                            .AppendLine("早朝作業")
                            .AppendLine("深夜作業")
                            .AppendLine("休暇申請")
                            .AppendLine("テレワーク")
                            .Append("打刻時間修正")
                            .ToString();

            Assert.AreEqual(expected, model.ViewModel.ShinseiInput);
        }
        #endregion

        #region テストケース１２ 伺い申請情報のステータスが差戻
        /// <summary>
        /// テストケース１２ 伺い申請情報のステータスが差戻の場合、空が出力されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_伺い申請情報ステータスが差戻_申請情報は空になる()
        {
            // Given
            var workingHours = new[]
            {
                CreateWorkingHour(1, 1, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(2, 1, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(3, 1, DefaultTestDate, 17, 37, 18, 28),
                CreateWorkingHour(4, 2, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(5, 2, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(6, 2, DefaultTestDate, 17, 37, 18, 28),
            };

            var ukagaiHeaders = new[]
            {
                CreateUkagaiHeader(1, 1, DefaultTestDate, ApprovalStatus.差戻),
            };

            var ukagaiShinseis = new[]
            {
                CreateUkagaiShinsei(1, 1, InquiryType.リフレッシュデー残業),
                CreateUkagaiShinsei(2, 1, InquiryType.休日出勤),
                CreateUkagaiShinsei(3, 1, InquiryType.時間外労働時間制限拡張),
                CreateUkagaiShinsei(4, 1, InquiryType.夜間作業),
                CreateUkagaiShinsei(5, 1, InquiryType.早朝作業),
                CreateUkagaiShinsei(6, 1, InquiryType.深夜作業),
                CreateUkagaiShinsei(7, 1, InquiryType.休暇申請),
                CreateUkagaiShinsei(8, 1, InquiryType.テレワーク),
                CreateUkagaiShinsei(9, 1, InquiryType.打刻時間修正),
            };

            db.WorkingHours.AddRange(workingHours);
            db.UkagaiHeaders.AddRange(ukagaiHeaders);
            db.UkagaiShinseis.AddRange(ukagaiShinseis);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // When
            await model.OnGetAsync(1, new DateOnly(2026, 1, 1), false);

            // Then
            Assert.AreEqual(string.Empty, model.ViewModel.ShinseiInput);
        }
        #endregion

        #region テストケース１３ 伺い申請ヘッダの社員ID違い
        /// <summary>
        /// テストケース１３ 伺い申請ヘッダの社員ID違いの場合、空が出力されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_伺い申請ヘッダの社員ID違い_申請情報は空になる()
        {
            // Given
            var workingHours = new[]
            {
                CreateWorkingHour(1, 1, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(2, 1, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(3, 1, DefaultTestDate, 17, 37, 18, 28),
                CreateWorkingHour(4, 2, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(5, 2, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(6, 2, DefaultTestDate, 17, 37, 18, 28),
            };

            var ukagaiHeaders = new[]
            {
                CreateUkagaiHeader(1, 1, DefaultTestDate, ApprovalStatus.承認),
            };

            var ukagaiShinseis = new[]
            {
                CreateUkagaiShinsei(1, 1, InquiryType.リフレッシュデー残業),
                CreateUkagaiShinsei(2, 1, InquiryType.休日出勤),
                CreateUkagaiShinsei(3, 1, InquiryType.時間外労働時間制限拡張),
                CreateUkagaiShinsei(4, 1, InquiryType.夜間作業),
                CreateUkagaiShinsei(5, 1, InquiryType.早朝作業),
                CreateUkagaiShinsei(6, 1, InquiryType.深夜作業),
                CreateUkagaiShinsei(7, 1, InquiryType.休暇申請),
                CreateUkagaiShinsei(8, 1, InquiryType.テレワーク),
                CreateUkagaiShinsei(9, 1, InquiryType.打刻時間修正),
            };

            db.WorkingHours.AddRange(workingHours);
            db.UkagaiHeaders.AddRange(ukagaiHeaders);
            db.UkagaiShinseis.AddRange(ukagaiShinseis);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // When
            await model.OnGetAsync(2, new DateOnly(2026, 1, 1), false);

            // Then
            // 申請入力
            Assert.AreEqual(string.Empty, model.ViewModel.ShinseiInput);
        }
        #endregion

        #region テストケース１４ 伺い申請ヘッダの実績日違い
        /// <summary>
        /// テストケース１４ 伺い申請ヘッダの実績日違いの場合、空が出力されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_伺い申請ヘッダの実績日違い_申請データは空になる()
        {
            // Given
            var workingHours = new[]
            {
                CreateWorkingHour(1, 1, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(2, 1, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(3, 1, DefaultTestDate, 17, 37, 18, 28),
                CreateWorkingHour(4, 2, DefaultTestDate, 10, 24, 11, 12),
                CreateWorkingHour(5, 2, DefaultTestDate, 12, 18, 16, 48),
                CreateWorkingHour(6, 2, DefaultTestDate, 17, 37, 18, 28),
            };

            var ukagaiHeaders = new[]
            {
                CreateUkagaiHeader(1, 1, DefaultTestDate, ApprovalStatus.承認),
            };

            var ukagaiShinseis = new[]
            {
                CreateUkagaiShinsei(1, 1, InquiryType.リフレッシュデー残業),
                CreateUkagaiShinsei(2, 1, InquiryType.休日出勤),
                CreateUkagaiShinsei(3, 1, InquiryType.時間外労働時間制限拡張),
                CreateUkagaiShinsei(4, 1, InquiryType.夜間作業),
                CreateUkagaiShinsei(5, 1, InquiryType.早朝作業),
                CreateUkagaiShinsei(6, 1, InquiryType.深夜作業),
                CreateUkagaiShinsei(7, 1, InquiryType.休暇申請),
                CreateUkagaiShinsei(8, 1, InquiryType.テレワーク),
                CreateUkagaiShinsei(9, 1, InquiryType.打刻時間修正),
            };

            db.WorkingHours.AddRange(workingHours);
            db.UkagaiHeaders.AddRange(ukagaiHeaders);
            db.UkagaiShinseis.AddRange(ukagaiShinseis);
            await db.SaveChangesAsync();

            var model = CreateModel();

            // When
            await model.OnGetAsync(1, new DateOnly(2026, 1, 2), false);

            // Then
            // 申請入力
            Assert.AreEqual(string.Empty, model.ViewModel.ShinseiInput);
        }
        #endregion

        #region テストケース１５　出勤時間１～３・退勤時間「時」「分」未入力→エラーにならないこと
        /// <summary>
        /// テストケース１５　出勤時間１～３・退勤時間「時」「分」未入力（全てnull）→エラーにならないこと
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出退勤時間未入力_エラーにならない()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.SyainId = 1;
            model.ViewModel.JissekiDate = DefaultTestDate;
            model.ViewModel.IsDairiInput = true;
            model.ViewModel.TimeSets = [
                CreateTimeSet(null, null, null, null),
                CreateTimeSet(null, null, null, null),
                CreateTimeSet(null, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertTimeSetResultEquals(
                model.ViewModel.SyainId, model.ViewModel.JissekiDate, model.ViewModel.IsDairiInput,
                null, null, null, null, null, null,
                result);
        }
        #endregion

        #region テストケース１６　出勤時間１「時」入力済「分」・退勤時間未入力→エラーになること
        /// <summary>
        /// テストケース１６　出勤時間１「時」入力済「分」・退勤時間未入力→エラーになること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間１_時入力済_分未入力_退勤時間未入力_エラーになる()
        {
            var model = CreateModel();
            model.ViewModel.TimeSets = [
                CreateTimeSet(10, null, null, null),
                CreateTimeSet(null, null, null, null),
                CreateTimeSet(null, null, null, null),
            ];

            var result = await model.OnPostRegisterAsync();

            AssertErrors(result, string.Format(Const.ErrorSet, "出退勤1、時間と分の両方"));
        }
        #endregion

        #region テストケース１７　出勤時間１「時」未入力「分」入力済　退勤時間未入力→エラーになること
        /// <summary>
        /// テストケース１７　出勤時間１「時」未入力「分」入力済→エラーになること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間１_時未入力_分入力済_退勤時間未入力_エラーを返却()
        {
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, 10, null, null),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
            ];

            var result = await model.OnPostRegisterAsync();

            AssertErrors(result, string.Format(Const.ErrorSet, "出退勤1、時間と分の両方"));
        }
        #endregion

        #region テストケース１８　退勤時間１「時」入力済「分」・出勤時間未入力→エラーになること
        /// <summary>
        /// テストケース１８　退勤時間１「時」入力済「分」・出勤時間未入力→エラーになること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_退勤時間１_時入力済_分未入力_出勤時間未入力_エラーになる()
        {
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, 10, null),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
            ];

            var result = await model.OnPostRegisterAsync();

            AssertErrors(result, string.Format(Const.ErrorSet, "出退勤1、時間と分の両方"));
        }
        #endregion

        #region テストケース１９　退勤時間１「時」未入力「分」入力済　出勤時間未入力→エラーになること
        /// <summary>
        /// テストケース１９　退勤時間１「時」未入力「分」入力済　出勤時間未入力→エラーになること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_退勤時間１_時未入力_分入力済_出勤時間未入力_エラーになる()
        {
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, 10),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
            ];

            var result = await model.OnPostRegisterAsync();

            AssertErrors(result, string.Format(Const.ErrorSet, "出退勤1、時間と分の両方"));
        }
        #endregion

        #region テストケース２０　出勤時間２「時」入力済「分」・退勤時間未入力→エラーになること
        /// <summary>
        /// テストケース２０　出勤時間２「時」入力済「分」・退勤時間未入力→エラーになること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間２_時入力済_分未入力_退勤時間未入力_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(10, null, null, null),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorSet, "出退勤2、時間と分の両方"));
        }
        #endregion

        #region テストケース２１　出勤時間２「時」未入力「分」入力済　退勤時間未入力→エラーになること
        /// <summary>
        /// テストケース２１　出勤時間２「時」未入力「分」入力済→エラーになること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間２_時未入力_分入力済_退勤時間未入力_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, 30, null, null),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorSet, "出退勤2、時間と分の両方"));
        }
        #endregion

        #region テストケース２２　退勤時間２「時」入力済「分」・出勤時間未入力→エラーになること
        /// <summary>
        /// テストケース２２　退勤時間２「時」入力済「分」・出勤時間未入力→エラーになること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_退勤時間２_時入力済_分未入力_出勤時間未入力_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, 15, null),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorSet, "出退勤2、時間と分の両方"));
        }
        #endregion

        #region テストケース２３　退勤時間２「時」未入力「分」入力済　出勤時間未入力→エラーになること
        /// <summary>
        /// テストケース２３　退勤時間２「時」未入力「分」入力済　出勤時間未入力→エラーになること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_退勤時間２_時未入力_分入力済_出勤時間未入力_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, 30),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorSet, "出退勤2、時間と分の両方"));
        }
        #endregion

        #region テストケース２４　出勤時間３「時」入力済「分」・退勤時間未入力→エラーになること
        /// <summary>
        /// テストケース２４　出勤時間３「時」入力済「分」・退勤時間未入力→エラーになること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間３_時入力済_分未入力_退勤時間未入力_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(10, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorSet, "出退勤3、時間と分の両方"));
        }
        #endregion

        #region テストケース２５　出勤時間３「時」未入力「分」入力済　退勤時間未入力→エラーになること
        /// <summary>
        /// テストケース２５　出勤時間３「時」未入力「分」入力済→エラーになること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間３_時未入力_分入力済_退勤時間未入力_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, 30, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorSet, "出退勤3、時間と分の両方"));
        }
        #endregion

        #region テストケース２６　退勤時間３「時」入力済「分」・出勤時間未入力→エラーになること
        /// <summary>
        /// テストケース２６　退勤時間３「時」入力済「分」・出勤時間未入力→エラーになること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_退勤時間３_時入力済_分未入力_出勤時間未入力_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, 18, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorSet, "出退勤3、時間と分の両方"));
        }
        #endregion

        #region テストケース２７　退勤時間３「時」未入力「分」入力済　出勤時間未入力→エラーになること
        /// <summary>
        /// テストケース２７　退勤時間３「時」未入力「分」入力済　出勤時間未入力→エラーになること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_退勤時間３_時未入力_分入力済_出勤時間未入力_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, 30),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorSet, "出退勤3、時間と分の両方"));
        }
        #endregion

        #region テストケース２８ 出勤時間１入力済み　退勤時間未入力→エラーとなること
        /// <summary>
        /// テストケース２８ 出勤時間１入力済み　退勤時間未入力→エラーとなること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間１入力済_退勤時間未入力_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(8, 30, null, null),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorInputRequired, "出退勤1、出勤と退勤の時間両方"));
        }
        #endregion

        #region テストケース２９ 出勤時間１未入力　退勤時間入力済→エラーとなること
        /// <summary>
        /// テストケース２９ 出勤時間１未入力　退勤時間入力済→エラーとなること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間１未入力_退勤時間入力済_エラーになること()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, 17, 30),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorInputRequired, "出退勤1、出勤と退勤の時間両方"));
        }
        #endregion

        #region テストケース３０ 出勤時間２入力済み　退勤時間未入力→エラーとなること
        /// <summary>
        /// テストケース３０ 出勤時間２入力済み　退勤時間未入力→エラーとなること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間２入力済_退勤時間未入力_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(13, 0, null, null),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorInputRequired, "出退勤2、出勤と退勤の時間両方"));
        }
        #endregion

        #region テストケース３１ 出勤時間２未入力　退勤時間入力済→エラーとなること
        /// <summary>
        /// テストケース３１ 出勤時間２未入力　退勤時間入力済→エラーとなること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間２未入力_退勤時間入力済_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, 20, 30),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorInputRequired, "出退勤2、出勤と退勤の時間両方"));
        }
        #endregion

        #region テストケース３２ 出勤時間３入力済み　退勤時間未入力→エラーとなること
        /// <summary>
        /// テストケース３２ 出勤時間３入力済み　退勤時間未入力→エラーとなること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間３入力済_退勤時間未入力_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(22, 0, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorInputRequired, "出退勤3、出勤と退勤の時間両方"));
        }
        #endregion

        #region テストケース３３ 出勤時間３未入力　退勤時間入力済→エラーとなること
        /// <summary>
        /// テストケース３３ 出勤時間３未入力　退勤時間入力済→エラーとなること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間３未入力_退勤時間入力済_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, 23, 0),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorInputRequired, "出退勤3、出勤と退勤の時間両方"));
        }
        #endregion

        #region テストケース３４ 出勤時間１退勤＜出勤時間入力済→エラーとなること
        /// <summary>
        /// テストケース３４ 出勤時間１退勤＜出勤時間入力済→エラーとなること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間１より退勤時間１が前_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(23, 0, 22, 59),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorReverse, "出退勤1、出退勤時間"));
        }
        #endregion

        #region テストケース３５ 出勤時間１退勤(0時)＜出勤時間入力済→エラーとならないこと
        /// <summary>
        /// テストケース３５ 出勤時間１退勤(0時)＜出勤時間入力済→エラーとならないこと
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間１より退勤時間１が前_退勤時間0時_エラーにならない()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(23, 59, 0, 0),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            await model.OnPostRegisterAsync();

            // Then
            Assert.IsTrue(model.ModelState.IsValid);
        }
        #endregion

        #region テストケース３６ 出勤時間２退勤＜出勤時間入力済→エラーとなること
        /// <summary>
        /// テストケース３６ 出勤時間２退勤＜出勤時間入力済→エラーとなること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間２より退勤時間２が前_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(23, 0, 22, 59),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorReverse, "出退勤2、出退勤時間"));
        }
        #endregion

        #region テストケース３７ 出勤時間２退勤(0時)＜出勤時間入力済→エラーとならないこと
        /// <summary>
        /// テストケース３７ 出勤時間２退勤(0時)＜出勤時間入力済→エラーとならないこと
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間２より退勤時間２が前_退勤時間0時_エラーにならない()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(23, 59, 0, 0),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            await model.OnPostRegisterAsync();

            // Then
            Assert.IsTrue(model.ModelState.IsValid);
        }
        #endregion

        #region テストケース３８ 出勤時間３退勤＜出勤時間入力済→エラーとなること
        /// <summary>
        /// テストケース３８ 出勤時間３退勤＜出勤時間入力済→エラーとなること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間３より退勤時間３が前_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(23, 0, 22, 59),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorReverse, "出退勤3、出退勤時間"));
        }
        #endregion

        #region テストケース３９ 出勤時間３退勤(0時)＜出勤時間入力済→エラーとならないこと
        /// <summary>
        /// テストケース３９ 出勤時間３退勤(0時)＜出勤時間入力済→エラーとならないこと
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間３より退勤時間３が前_退勤時間0時_エラーにならない()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(null, null, null, null),
               CreateTimeSet(23, 59, 0, 0),
            ];

            // When
            await model.OnPostRegisterAsync();

            // Then
            Assert.IsTrue(model.ModelState.IsValid);
        }
        #endregion

        #region テストケース４０ 日報実績あり日報確定済み→エラーとなること
        /// <summary>
        /// テストケース４０ 日報実績あり日報確定済み→エラーとなること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_日報確定済_エラーになる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(8, 0, 18, 0),
               CreateTimeSet(20, 0, 23, 0),
               CreateTimeSet(1, 0, 5, 0),
            ];
            model.ViewModel.TourokuKubun = DailyReportStatusClassification.確定保存;

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, Const.ErrorNippouLocked);
        }
        #endregion

        #region テストケース４１ 日報実績の登録状況区分が「１：確定保存」以外
        /// <summary>
        /// テストケース４１ 日報実績の登録状況区分が「１：確定保存」以外→エラーにならないこと
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_日報実績の登録状況区分が確定保存以外_エラーにならない()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.SyainId = 1;
            model.ViewModel.JissekiDate = DefaultTestDate;
            model.ViewModel.IsDairiInput = true;
            model.ViewModel.TimeSets = [
               CreateTimeSet(8, 0, 18, 0),
               CreateTimeSet(20, 0, 23, 0),
               CreateTimeSet(23, 0, 0, 0),
            ];
            model.ViewModel.TourokuKubun = DailyReportStatusClassification.一時保存;

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertTimeSetResultEquals(
                model.ViewModel.SyainId, model.ViewModel.JissekiDate, model.ViewModel.IsDairiInput,
                new TimeOnly(8, 0), new TimeOnly(18, 0),
                new TimeOnly(20, 0), new TimeOnly(23, 0),
                new TimeOnly(23, 0), new TimeOnly(0, 0),
                result);
        }
        #endregion

        #region テストケース４２ 出勤時間１正常範囲出勤時間２３異常範囲→出退勤時間１のみとなること
        /// <summary>
        /// テストケース４２ 出勤時間１正常範囲出勤時間２３異常範囲→出退勤時間１のみとなること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間１正常範囲出勤時間２３異常範囲_出退勤時間１のみとなる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(0, 0, 18, 0),
               CreateTimeSet(-1, 0, null, null),
               CreateTimeSet(0, -1, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertTimeSetResultEquals(
                model.ViewModel.SyainId, model.ViewModel.JissekiDate, model.ViewModel.IsDairiInput,
                new TimeOnly(0, 0), new TimeOnly(18, 0),
                null, null,
                null, null,
                result);
        }
        #endregion

        #region テストケース４３ 退勤時間１正常範囲退勤時間２３異常範囲→出退勤時間１のみとなること
        /// <summary>
        /// テストケース４３ 退勤時間１正常範囲退勤時間２３異常範囲→出退勤時間１のみとなること
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_退勤時間１正常範囲退勤時間２３異常範囲_出退勤時間１のみとなる()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(8, 0, 23, 59),
               CreateTimeSet(null, null, 24, 59),
               CreateTimeSet(null, null, 23, 60),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertTimeSetResultEquals(
                model.ViewModel.SyainId, model.ViewModel.JissekiDate, model.ViewModel.IsDairiInput,
                new TimeOnly(8, 0), new TimeOnly(23, 59),
                null, null,
                null, null,
                result);
        }
        #endregion

        #region テストケース４４ 出勤時間２<=退勤時間１→エラー
        /// <summary>
        /// テストケース４４ 出勤時間２<=退勤時間１→エラー
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間2が退勤時間1以下_エラーになること()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(8, 0, 13, 0),
               CreateTimeSet(12, 0, 14, 0),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorOverlapInputTime, "出退勤1", "出退勤2"));
        }
        #endregion

        #region テストケース４５ 出勤時間３<=退勤時間１→エラー
        /// <summary>
        /// テストケース４５ 出勤時間３<=退勤時間１→エラー
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間3が退勤時間1以下_エラーになること()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(8, 0, 13, 0),
               CreateTimeSet(13, 0, 14, 0),
               CreateTimeSet(12, 0, 17, 0),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorOverlapInputTime, "出退勤1", "出退勤3"));
        }
        #endregion

        #region テストケース４６ 出勤時間３<=退勤時間２→エラー
        /// <summary>
        /// テストケース４６ 出勤時間３<=退勤時間２→エラー
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出勤時間3が退勤時間2以下_エラーになること()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(8, 0, 13, 0),
               CreateTimeSet(13, 0, 16, 0),
               CreateTimeSet(15, 0, 17, 0),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorOverlapInputTime, "出退勤2", "出退勤3"));
        }
        #endregion

        #region テストケース４７ 出退勤時間２<=出退勤時間１→エラー
        /// <summary>
        /// テストケース４７ 出退勤時間２<=出退勤時間１→エラー
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出退勤時間2が出退勤時間1以下_エラーになること()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(12, 0, 13, 0),
               CreateTimeSet(10, 0, 11, 0),
               CreateTimeSet(null, null, null, null),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorReverse, $"出退勤1と出退勤2"));
        }
        #endregion

        #region テストケース４８ 出退勤時間３<=出退勤時間１→エラー
        /// <summary>
        /// テストケース４８ 出退勤時間３<=出退勤時間１→エラー
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出退勤時間3が出退勤時間1以下_エラーになること()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(12, 0, 13, 0),
               CreateTimeSet(14, 0, 15, 0),
               CreateTimeSet(10, 0, 11, 0),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorReverse, $"出退勤2と出退勤3"));
        }
        #endregion

        #region テストケース４９ 出退勤時間３<=出退勤時間２→エラー
        /// <summary>
        /// テストケース４９ 出退勤時間３<=出退勤時間２→エラー
        /// </summary>
        [TestMethod]
        public async Task OnPostRegisterAsync_出退勤時間3が出退勤時間2以下_エラーになること()
        {
            // Given
            var model = CreateModel();
            model.ViewModel.TimeSets = [
               CreateTimeSet(12, 0, 13, 0),
               CreateTimeSet(16, 0, 17, 0),
               CreateTimeSet(14, 0, 15, 0),
            ];

            // When
            var result = await model.OnPostRegisterAsync();

            // Then
            AssertErrors(result, string.Format(Const.ErrorReverse, $"出退勤2と出退勤3"));
        }
        #endregion
    }
}
