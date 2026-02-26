using Model.Model;
using Zouryoku.Utils;
using static Model.Enums.AchievementClassification;
using static Model.Enums.RefreshDayFlag;
using static Zouryoku.Utils.JissekiKakuteiSimeUtil;

namespace ZouryokuTest.Utils;

/// <summary>
/// 実績確定締め日ユーティリティ のユニットテスト
/// </summary>
[TestClass]
public class JissekiKakuteiSimeUtilTests : BaseInMemoryDbContextTest
{
    /// <summary>
    /// 前提: 月末以前と月末以降の2つの確定期限が存在する
    /// 操作: 実績確定期限情報を取得する
    /// 結果: 中締めと月末締めの2件が正しく分類されて返される
    /// </summary>
    [TestMethod]
    public async Task GetKakuteiShimeKigen_When2つの確定期限がある_Then中締めと月末締めが返される()
    {
        // 準備 (Arrange)
        var targetMonth = new DateOnly(2024, 2, 1);
        db.JissekiKakuteiSimebis.AddRange(
            new JissekiKakuteiSimebi { Id = 1, KakuteiKigenYmd = new DateOnly(2024, 2, 28) }, // 中締め (<= 2/29)
            new JissekiKakuteiSimebi { Id = 2, KakuteiKigenYmd = new DateOnly(2024, 3, 10) }  // 月末締め (> 2/29)
        );
        await db.SaveChangesAsync();

        // 実行 (Act)
        var result = await GetKakuteiShimeKigenAsync(db, targetMonth);

        // 検証 (Assert)
        Assert.HasCount(2, result, "2件の確定期限が返されるべきです。");

        // 1件目
        var result1 = result.SingleOrDefault(k => k.JissekiKakuteiSimebiId == 1);
        Assert.IsNotNull(result1);
        Assert.AreEqual(new DateOnly(2024, 2, 28), result1.KakuteiKigenYmd, "1件目の確定期限が一致しません。");
        Assert.AreEqual(中締め, result1.Kubun, "1件目は中締めであるべきです。");

        // 2件目
        var result2 = result.SingleOrDefault(k => k.JissekiKakuteiSimebiId == 2);
        Assert.IsNotNull(result2);
        Assert.AreEqual(new DateOnly(2024, 3, 10), result2.KakuteiKigenYmd, "2件目の確定期限が一致しません。");
        Assert.AreEqual(月末締め, result2.Kubun, "2件目は月末締めであるべきです。");
    }

    /// <summary>
    /// 前提: 月末以降の確定期限のみが存在する（中締め無し）
    /// 操作: 実績確定期限情報を取得する
    /// 結果: 一か月締めとして分類されて返される
    /// </summary>
    [TestMethod]
    public async Task GetKakuteiShimeKigen_When月末以降の確定期限のみ_Then一か月締めが返される()
    {
        // 準備 (Arrange)
        var targetMonth = new DateOnly(2024, 2, 1);
        db.JissekiKakuteiSimebis.Add(new JissekiKakuteiSimebi { Id = 1, KakuteiKigenYmd = new DateOnly(2024, 3, 5) });
        await db.SaveChangesAsync();

        // 実行 (Act)
        var result = await GetKakuteiShimeKigenAsync(db, targetMonth);

        // 検証 (Assert)
        Assert.HasCount(1, result, "1件の確定期限が返されるべきです。");
        Assert.AreEqual(1, result[0].JissekiKakuteiSimebiId, "実績確定締め日IDが一致しません。");
        Assert.AreEqual(new DateOnly(2024, 3, 5), result[0].KakuteiKigenYmd, "確定期限が一致しません。");
        Assert.AreEqual(一か月締め, result[0].Kubun, "一か月締めであるべきです。");
    }

    /// <summary>
    /// 前提: 月末以前の確定期限のみが存在する
    /// 操作: 実績確定期限情報を取得する
    /// 結果: 中締めとして分類されて返される
    /// </summary>
    [TestMethod]
    public async Task GetKakuteiShimeKigen_When月末以前の確定期限のみ_Then中締めが返される()
    {
        // 準備 (Arrange)
        var targetMonth = new DateOnly(2024, 2, 1);
        db.JissekiKakuteiSimebis.Add(new JissekiKakuteiSimebi { Id = 1, KakuteiKigenYmd = new DateOnly(2024, 2, 25) });
        await db.SaveChangesAsync();

        // 実行 (Act)
        var result = await GetKakuteiShimeKigenAsync(db, targetMonth);

        // 検証 (Assert)
        Assert.HasCount(1, result, "1件の確定期限が返されるべきです。");
        Assert.AreEqual(1, result[0].JissekiKakuteiSimebiId, "実績確定締め日IDが一致しません。");
        Assert.AreEqual(new DateOnly(2024, 2, 25), result[0].KakuteiKigenYmd, "確定期限が一致しません。");
        Assert.AreEqual(中締め, result[0].Kubun, "中締めであるべきです。");
    }

    /// <summary>
    /// 前提: 検索範囲外の確定期限のみが存在する
    /// 操作: 実績確定期限情報を取得する
    /// 結果: 空のリストが返される
    /// </summary>
    [TestMethod]
    public async Task GetKakuteiShimeKigen_When検索範囲外の確定期限のみ_Then空のリストが返される()
    {
        // 準備 (Arrange)
        var targetMonth = new DateOnly(2024, 2, 1);
        // 検索範囲: 2/16 ～ 3/15
        db.JissekiKakuteiSimebis.AddRange(
            new JissekiKakuteiSimebi { KakuteiKigenYmd = new DateOnly(2024, 2, 15) }, // 範囲外（前）
            new JissekiKakuteiSimebi { KakuteiKigenYmd = new DateOnly(2024, 3, 16) }  // 範囲外（後）
        );
        await db.SaveChangesAsync();

        // 実行 (Act)
        var result = await GetKakuteiShimeKigenAsync(db, targetMonth);

        // 検証 (Assert)
        Assert.IsEmpty(result, "検索範囲外のため、0件が返されるべきです。");
    }

    /// <summary>
    /// 前提: 確定期限が月末日ちょうどである
    /// 操作: 実績確定期限情報を取得する
    /// 結果: 中締めとして分類されて返される
    /// </summary>
    [TestMethod]
    public async Task GetKakuteiShimeKigen_When確定期限が月末日ちょうど_Then中締めが返される()
    {
        // 準備 (Arrange)
        var targetMonth = new DateOnly(2024, 2, 1);
        db.JissekiKakuteiSimebis.Add(
            new JissekiKakuteiSimebi { Id = 1, KakuteiKigenYmd = new DateOnly(2024, 2, 29) } // 月末日ちょうど
        );
        await db.SaveChangesAsync();

        // 実行 (Act)
        var result = await GetKakuteiShimeKigenAsync(db, targetMonth);

        // 検証 (Assert)
        Assert.AreEqual(中締め, result[0].Kubun, "月末日ちょうどの場合、中締めとして返されるべきです。");
    }

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
            Ymd = new(2026, 2, 19),
            RefreshDay = それ以外,
        };
        db.Add(hikadoubi);
        db.SaveChanges();

        // 実績締め日は2026/02/15とする
        var simebi = new DateOnly(2026, 2, 15);

        // Act
        // ----------------------------------

        var baseDate = new DateOnly(2026, 2, nowDay);
        // 実績締め日は2026/02/15とする
        var result = await IsInNotificationPeriodAsync(db, baseDate, simebi, kakuteiYmd);

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
            Ymd = new(2026, 2, 19),
            RefreshDay = それ以外,
        };
        db.Add(hikadoubi);
        db.SaveChanges();

        // 実績締め日は2026/02/15とする
        var simebi = new DateOnly(2026, 2, 15);

        // Act
        // ----------------------------------

        var baseDate = new DateOnly(2026, 2, nowDay);
        var result = await IsInNotificationPeriodAsync(db, baseDate, simebi, kakuteiYmd);

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
    public async Task GetCanNotifyJissekiSpanAsync_先月が一か月締め_今月が一か月締め_先月の一か月締めの実績期間情報を取得すること(int baseDay)
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
        var jissekiSpan = await GetCanNotifyJissekiSpanAsync(db, baseDate);

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
    public async Task GetCanNotifyJissekiSpanAsync_先月が一か月締めでない_今月が一か月締め_先月の月末締めを取得すること(int baseDay)
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
        var jissekiSpan = await GetCanNotifyJissekiSpanAsync(db, baseDate);

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
    public async Task GetCanNotifyJissekiSpanAsync_先月が一か月締め_今月が一か月締めでない_実績期間情報を取得すること(int baseDay)
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
        var jissekiSpan = await GetCanNotifyJissekiSpanAsync(db, baseDate);

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
    public async Task GetCanNotifyJissekiSpanAsync_先月が一か月締めでない_今月が一か月締めでない_実績期間情報を取得すること(int baseDay)
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
        db.AddRange([lastMonthNakajimeKakuteibi, lastMonthGetsumatsujimeKakuteibi, thisMonthNakajimeKakuteibi,
                thisMonthGetsumatsujimeKakuteibi]);
        db.SaveChanges();

        // Act
        // ----------------------------------

        var baseDate = new DateOnly(2026, 2, baseDay);
        var jissekiSpan = await GetCanNotifyJissekiSpanAsync(db, baseDate);

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

    [TestMethod]
    public async Task GetCanNotifyJissekiSpanAsync_実績確定締め日データが存在しない_例外()
    {
        // Arrange
        // ----------------------------------

        // 確定期限データが存在しないと仮定

        // Act & Assert
        // ----------------------------------

        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => GetCanNotifyJissekiSpanAsync(db, new(2026, 2, 16)));
        Assert.AreEqual("実績確定締め日データに不整合があります。", ex.Message);
        Assert.IsNotNull(ex.InnerException);
        Assert.AreEqual("実績確定締め日データが存在しません。", ex.InnerException.Message);
    }

    [TestMethod]
    public async Task GetCanNotifyJissekiSpanAsync_実績確定締め日データが不正_例外()
    {
        // Arrange
        // ----------------------------------

        // 中締めデータを複数作成
        // 今月分の確定期限（中締め）
        db.AddRange([new JissekiKakuteiSimebi()
        {
            KakuteiKigenYmd = new DateOnly(2026, 2, 18)
        },
        new JissekiKakuteiSimebi()
        {
            KakuteiKigenYmd = new DateOnly(2026, 2, 17)
        }]);

        // Act & Assert
        // ----------------------------------

        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => GetCanNotifyJissekiSpanAsync(db, new(2026, 2, 16)));
        Assert.AreEqual("実績確定締め日データに不整合があります。", ex.Message);
    }
}