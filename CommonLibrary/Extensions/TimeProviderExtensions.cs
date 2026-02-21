using System;

namespace CommonLibrary.Extensions
{
    /// <summary>
    /// <see cref="TimeProvider"/> の拡張メソッド。
    /// このプロジェクトでは日本時間のみを扱うため、ローカル日時を簡潔に取得できるメソッドを提供する。
    /// </summary>
    public static class TimeProviderExtensions
    {
        /// <summary>
        /// 現在のローカル日時を <see cref="DateTime"/> として取得する。
        /// </summary>
        public static DateTime Now(this TimeProvider timeProvider)
            => timeProvider.GetLocalNow().DateTime;

        /// <summary>
        /// 現在のローカル日付を <see cref="DateOnly"/> として取得する。
        /// </summary>
        public static DateOnly Today(this TimeProvider timeProvider)
            => DateOnly.FromDateTime(timeProvider.Now());
    }
}
