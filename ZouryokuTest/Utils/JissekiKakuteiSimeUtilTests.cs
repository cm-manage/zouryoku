using Model.Enums;
using Model.Model;
using Zouryoku.Utils;

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
        var result = await JissekiKakuteiSimeUtil.GetKakuteiShimeKigenAsync(db, targetMonth);

        // 検証 (Assert)
        Assert.AreEqual(2, result.Count, "2件の確定期限が返されるべきです。");
        Assert.AreEqual(1, result[0].JissekiKakuteiSimebiId, "1件目の実績確定締め日IDが一致しません。");
        Assert.AreEqual(new DateOnly(2024, 2, 28), result[0].KakuteiKigenYmd, "1件目の確定期限が一致しません。");
        Assert.AreEqual(AchievementClassification.中締め, result[0].Kubun, "1件目は中締めであるべきです。");
        Assert.AreEqual(2, result[1].JissekiKakuteiSimebiId, "2件目の実績確定締め日IDが一致しません。");
        Assert.AreEqual(new DateOnly(2024, 3, 10), result[1].KakuteiKigenYmd, "2件目の確定期限が一致しません。");
        Assert.AreEqual(AchievementClassification.月末締め, result[1].Kubun, "2件目は月末締めであるべきです。");
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
        var result = await JissekiKakuteiSimeUtil.GetKakuteiShimeKigenAsync(db, targetMonth);

        // 検証 (Assert)
        Assert.AreEqual(1, result.Count, "1件の確定期限が返されるべきです。");
        Assert.AreEqual(1, result[0].JissekiKakuteiSimebiId, "実績確定締め日IDが一致しません。");
        Assert.AreEqual(new DateOnly(2024, 3, 5), result[0].KakuteiKigenYmd, "確定期限が一致しません。");
        Assert.AreEqual(AchievementClassification.一か月締め, result[0].Kubun, "一か月締めであるべきです。");
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
        var result = await JissekiKakuteiSimeUtil.GetKakuteiShimeKigenAsync(db, targetMonth);

        // 検証 (Assert)
        Assert.AreEqual(1, result.Count, "1件の確定期限が返されるべきです。");
        Assert.AreEqual(1, result[0].JissekiKakuteiSimebiId, "実績確定締め日IDが一致しません。");
        Assert.AreEqual(new DateOnly(2024, 2, 25), result[0].KakuteiKigenYmd, "確定期限が一致しません。");
        Assert.AreEqual(AchievementClassification.中締め, result[0].Kubun, "中締めであるべきです。");
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
        var result = await JissekiKakuteiSimeUtil.GetKakuteiShimeKigenAsync(db, targetMonth);

        // 検証 (Assert)
        Assert.AreEqual(0, result.Count, "検索範囲外のため、0件が返されるべきです。");
    }
}
