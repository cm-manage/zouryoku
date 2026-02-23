using Model.Model;
using Zouryoku.Utils;
using static Model.Enums.AchievementClassification;
using static Model.Enums.RefreshDayFlag;
using static Zouryoku.Utils.MikakuteiTsuchiUtil;

namespace ZouryokuTest.Utils
{
    [TestClass]
    public partial class MikakuteiTsuchiUtilTests : BaseInMemoryDbContextTest
    {
        // ======================================
        // IsInNotificationPeriodAsync
        // ======================================

        [TestMethod]
        [DataRow(16, DisplayName = "境界値：実績締め日の翌日")]
        [DataRow(17, DisplayName = "代表値：実績締め日の翌日から確定期限の翌営業日までの間")]
        [DataRow(20, DisplayName = "境界値：確定期限の翌営業日")]
        public async Task IsInNotificationPeriodAsync_基準日付が通知可能期間に含まれている_true(int nowDay)
        {
            // Arrange
            // ----------------------------------

            var kakuteiYmd = new DateOnly(2026, 2, 18);
            // 通知可能期間の終了日に確定期限の翌営業日を取得しているかを確認するために作成
            // 2026/02/19が非稼働日なので、通知可能期間の終了日は2026/2/20になる
            var hikadoubi = new Hikadoubi()
            {
                Ymd = kakuteiYmd.AddDays(1),
                RefreshDay = それ以外,
            };
            db.Add(hikadoubi);
            db.SaveChanges();

            var jissekiKakuteiKigenInfo = new JissekiKakuteiKigenInfo(hikadoubi.Id, hikadoubi.Ymd, 中締め);
            // 実績締め日は2026/02/15とする
            var jissekiSpan = new JissekiSpan(2026, 2, 1, 15, jissekiKakuteiKigenInfo);

            // Act
            // ----------------------------------

            var baseDate = new DateOnly(2026, 2, nowDay);
            var result = await IsInNotificationPeriodAsync(db, baseDate, jissekiSpan);

            // Assert
            // ----------------------------------

            Assert.IsTrue(result);
        }

        [TestMethod]
        [DataRow(10, DisplayName = "代表値：実績締め日の前日以前")]
        [DataRow(15, DisplayName = "境界値：実績締め日")]
        [DataRow(21, DisplayName = "境界値：確定期限の翌営業日の翌日")]
        [DataRow(28, DisplayName = "代表値：確定期限の翌営業日の翌々日以降")]
        public async Task IsInNotificationPeriodAsync_基準日付が通知可能期間に含まれていない_false(int nowDay)
        {
            // Arrange
            // ----------------------------------

            // 確定期限
            var kakuteiYmd = new DateOnly(2026, 2, 18);
            // 通知可能期間の終了日に確定期限の翌営業日を取得しているかを確認するために作成
            // 2026/02/19が非稼働日なので、通知可能期間の終了日は2026/2/20になる
            var hikadoubi = new Hikadoubi()
            {
                Ymd = kakuteiYmd.AddDays(1),
                RefreshDay = それ以外,
            };
            db.Add(hikadoubi);
            db.SaveChanges();

            var jissekiKakuteiKigenInfo = new JissekiKakuteiKigenInfo(1, kakuteiYmd, 中締め);
            // 実績締め日は2026/02/15とする
            var jissekiSpan = new JissekiSpan(2026, 2, 1, 15, jissekiKakuteiKigenInfo);

            // Act
            // ----------------------------------

            var baseDate = new DateOnly(2026, 2, nowDay);
            var result = await IsInNotificationPeriodAsync(db, baseDate, jissekiSpan);

            // Assert
            // ----------------------------------

            Assert.IsFalse(result);
        }

        // ======================================
        // GetJissekiSpan
        // ======================================

        [TestMethod]
        [DataRow(1, DisplayName = "境界値：月初のとき")]
        [DataRow(15, DisplayName = "代表値")]
        [DataRow(28, DisplayName = "境界値：月末のとき")]
        public async Task GetJissekiSpan_先月が一か月締め_今月が一か月締め_先月の一か月締めの実績期間情報を取得すること(int baseDay)
        {
            // Arrange
            // ----------------------------------

            // 先月分の確定期限
            var lastMonthIkkagetsujimeKakuteibi = new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new DateOnly(2026, 2, 3),
            };
            // 今月分の確定期限
            var thisMonthIkkagetsujimeKakuteibi = new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new DateOnly(2026, 3, 3)
            };
            db.AddRange([lastMonthIkkagetsujimeKakuteibi, thisMonthIkkagetsujimeKakuteibi]);
            db.SaveChanges();

            // Act
            // ----------------------------------

            var baseDate = new DateOnly(2026, 2, baseDay);
            var jissekiSpan = await GetJissekiSpanAsync(db, baseDate);

            // Assert
            // ----------------------------------

            // 期待される実績開始日
            var expectedJissekiStartYmd = new DateOnly(2026, 1, 1);
            Assert.AreEqual(expectedJissekiStartYmd, jissekiSpan.JissekiStartYmd);

            // 期待される実績締め日
            var expectedJissekiSimebiYmd = new DateOnly(2026, 1, 31);
            Assert.AreEqual(expectedJissekiSimebiYmd, jissekiSpan.JissekiSimebiYmd);

            // 期待される確定期限情報
            var expectedKakuteiKigenInfo
                = new JissekiKakuteiKigenInfo(lastMonthIkkagetsujimeKakuteibi.Id, lastMonthIkkagetsujimeKakuteibi.KakuteiKigenYmd, 一か月締め);
            Assert.AreEqual(expectedKakuteiKigenInfo, jissekiSpan.JissekiKakuteiKigenInfo);
        }

        [TestMethod]
        [DataRow(1, DisplayName = "境界値：月初のとき")]
        [DataRow(15, DisplayName = "代表値")]
        [DataRow(28, DisplayName = "境界値：月末のとき")]
        public async Task GetJissekiSpan_先月が一か月締めでない_今月が一か月締め_先月の月末締めを取得すること(int baseDay)
        {
            // Arrange
            // ----------------------------------

            // 先月分の確定期限（中締め）
            var lastMonthNakajimeKakuteibi = new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new DateOnly(2026, 1, 18),
            };
            // 先月分の確定期限（月末締め）
            var lastMonthGetsumatsujimeKakuteibi = new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new DateOnly(2026, 2, 3),
            };
            // 今月分の確定期限
            var thisMonthIkkagetsujimeKakuteibi = new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new DateOnly(2026, 3, 3)
            };
            db.AddRange([lastMonthNakajimeKakuteibi, lastMonthGetsumatsujimeKakuteibi, thisMonthIkkagetsujimeKakuteibi]);
            db.SaveChanges();

            // Act
            // ----------------------------------

            var baseDate = new DateOnly(2026, 2, baseDay);
            var jissekiSpan = await GetJissekiSpanAsync(db, baseDate);

            // Assert
            // ----------------------------------

            // 期待される実績開始日
            var expectedJissekiStartYmd = new DateOnly(2026, 1, 16);
            Assert.AreEqual(expectedJissekiStartYmd, jissekiSpan.JissekiStartYmd);

            // 期待される実績締め日
            var expectedJissekiSimebiYmd = new DateOnly(2026, 1, 31);
            Assert.AreEqual(expectedJissekiSimebiYmd, jissekiSpan.JissekiSimebiYmd);

            // 期待される確定期限情報
            var expectedKakuteiKigenInfo
                = new JissekiKakuteiKigenInfo(lastMonthGetsumatsujimeKakuteibi.Id, lastMonthGetsumatsujimeKakuteibi.KakuteiKigenYmd, 月末締め);
            Assert.AreEqual(expectedKakuteiKigenInfo, jissekiSpan.JissekiKakuteiKigenInfo);
        }

        [TestMethod]
        [DataRow(1, DisplayName = "先月の一か月締めを取得（境界値：月初のとき）")]
        [DataRow(8, DisplayName = "先月の一か月締めを取得（代表値）")]
        [DataRow(15, DisplayName = "先月の一か月締めを取得（境界値：中締め日のとき）")]
        [DataRow(16, DisplayName = "今月の中締めを取得（境界値：中締め日翌日のとき）")]
        [DataRow(22, DisplayName = "今月の中締めを取得（代表値）")]
        [DataRow(28, DisplayName = "今月の中締めを取得（境界値：月末のとき）")]
        public async Task GetJissekiSpan_先月が一か月締め_今月が一か月締めでない_実績期間情報を取得すること(int baseDay)
        {
            // Arrange
            // ----------------------------------

            // 先月分の確定期限
            var lastMonthIkkagetsujimeKakuteibi = new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new DateOnly(2026, 2, 3),
            };
            // 今月分の確定期限（中締め）
            var thisMonthNakajimeKakuteibi = new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new DateOnly(2026, 2, 18)
            };
            // 今月分の確定期限（月末締め）
            var thisMonthGetsumatsujimeKakuteibi = new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new DateOnly(2026, 3, 3)
            };
            db.AddRange([lastMonthIkkagetsujimeKakuteibi, thisMonthNakajimeKakuteibi, thisMonthGetsumatsujimeKakuteibi]);
            db.SaveChanges();

            // Act
            // ----------------------------------

            var baseDate = new DateOnly(2026, 2, baseDay);
            var jissekiSpan = await GetJissekiSpanAsync(db, baseDate);

            // Assert
            // ----------------------------------

            // 先月の実績情報を取得しているべきかどうか
            var isLastMonthFetched = baseDay <= 15;

            // 期待される実績開始日
            var expectedJissekiStartYmd = isLastMonthFetched ? new DateOnly(2026, 1, 1) : new DateOnly(2026, 2, 1);
            Assert.AreEqual(expectedJissekiStartYmd, jissekiSpan.JissekiStartYmd);

            // 期待される実績締め日
            var expectedJissekiSimebiYmd = isLastMonthFetched ? new DateOnly(2026, 1, 31) : new DateOnly(2026, 2, 15);
            Assert.AreEqual(expectedJissekiSimebiYmd, jissekiSpan.JissekiSimebiYmd);

            // 期待される確定期限情報
            var expectedKakuteiKigenInfo
                = isLastMonthFetched ?
                    new JissekiKakuteiKigenInfo(lastMonthIkkagetsujimeKakuteibi.Id, lastMonthIkkagetsujimeKakuteibi.KakuteiKigenYmd, 一か月締め)
                    : new JissekiKakuteiKigenInfo(thisMonthNakajimeKakuteibi.Id, thisMonthNakajimeKakuteibi.KakuteiKigenYmd, 中締め);
            Assert.AreEqual(expectedKakuteiKigenInfo, jissekiSpan.JissekiKakuteiKigenInfo);
        }

        [TestMethod]
        [DataRow(1, DisplayName = "先月の月末締めを取得（境界値：月初のとき）")]
        [DataRow(8, DisplayName = "先月の月末締めを取得（代表値）")]
        [DataRow(15, DisplayName = "先月の月末締めを取得（境界値：中締め日のとき）")]
        [DataRow(16, DisplayName = "今月の中締めを取得（境界値：中締め日翌日のとき）")]
        [DataRow(22, DisplayName = "今月の中締めを取得（代表値）")]
        [DataRow(28, DisplayName = "今月の中締めを取得（境界値：月末のとき）")]
        public async Task GetJissekiSpan_先月が一か月締めでない_今月が一か月締めでない_実績期間情報を取得すること(int baseDay)
        {
            // Arrange
            // ----------------------------------

            // 先月分の確定期限（中締め）
            var lastMonthNakajimeKakuteibi = new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new DateOnly(2026, 1, 18),
            };
            // 先月分の確定期限（月末締め）
            var lastMonthGetsumatsujimeKakuteibi = new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new DateOnly(2026, 2, 3),
            };
            // 今月分の確定期限（中締め）
            var thisMonthNakajimeKakuteibi = new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new DateOnly(2026, 2, 18)
            };
            // 今月分の確定期限（月末締め）
            var thisMonthGetsumatsujimeKakuteibi = new JissekiKakuteiSimebi()
            {
                KakuteiKigenYmd = new DateOnly(2026, 3, 3)
            };
            db.AddRange([lastMonthNakajimeKakuteibi, lastMonthGetsumatsujimeKakuteibi, thisMonthNakajimeKakuteibi, thisMonthGetsumatsujimeKakuteibi]);
            db.SaveChanges();

            // Act
            // ----------------------------------

            var baseDate = new DateOnly(2026, 2, baseDay);
            var jissekiSpan = await GetJissekiSpanAsync(db, baseDate);

            // Assert
            // ----------------------------------

            // 先月の実績情報を取得しているべきかどうか
            var isLastMonthFetched = baseDay <= 15;

            // 期待される実績開始日
            var expectedJissekiStartYmd = isLastMonthFetched ? new DateOnly(2026, 1, 16) : new DateOnly(2026, 2, 1);
            Assert.AreEqual(expectedJissekiStartYmd, jissekiSpan.JissekiStartYmd);

            // 期待される実績締め日
            var expectedJissekiSimebiYmd = isLastMonthFetched ? new DateOnly(2026, 1, 31) : new DateOnly(2026, 2, 15);
            Assert.AreEqual(expectedJissekiSimebiYmd, jissekiSpan.JissekiSimebiYmd);

            // 期待される確定期限情報
            var expectedKakuteiKigenInfo
                = isLastMonthFetched ?
                    new JissekiKakuteiKigenInfo(lastMonthGetsumatsujimeKakuteibi.Id, lastMonthGetsumatsujimeKakuteibi.KakuteiKigenYmd, 月末締め)
                    : new JissekiKakuteiKigenInfo(thisMonthNakajimeKakuteibi.Id, thisMonthNakajimeKakuteibi.KakuteiKigenYmd, 中締め);
            Assert.AreEqual(expectedKakuteiKigenInfo, jissekiSpan.JissekiKakuteiKigenInfo);
        }
    }
}
