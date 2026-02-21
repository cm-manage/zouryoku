using Microsoft.Extensions.Time.Testing;

namespace ZouryokuTest
{
    /// <summary>
    /// <see cref="FakeTimeProvider"/> のテスト用拡張メソッド。
    /// </summary>
    public static class FakeTimeProviderExtensions
    {
        /// <summary>
        /// ローカル日時を直接指定して <see cref="FakeTimeProvider"/> の現在時刻を設定する。
        /// 内部でローカルタイムゾーンの UTC オフセットを自動補正するため、
        /// テストコード側でタイムゾーン差（例: 日本の +9 時間）を意識する必要がない。
        /// </summary>
        /// <example>
        /// <code>
        /// // ローカル時刻 2025/07/01 09:00 として設定される（UTC 換算は自動）
        /// fakeTimeProvider.SetLocalNow(new DateTime(2025, 7, 1, 9, 0, 0));
        /// </code>
        /// </example>
        public static void SetLocalNow(this FakeTimeProvider provider, DateTime localDateTime)
        {
            var offset = TimeZoneInfo.Local.GetUtcOffset(localDateTime);
            provider.SetLocalTimeZone(TimeZoneInfo.Local);
            provider.SetUtcNow(new DateTimeOffset(localDateTime, offset).ToUniversalTime());
        }
    }
}
