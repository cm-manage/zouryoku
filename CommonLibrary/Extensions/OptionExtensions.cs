using System;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CommonLibrary.Extensions
{
    public static class OptionExtensions
    {
        /// <summary>
        /// データを取り出します。Noneの場合はExceptionをthrowします。
        /// </summary>
        public static A GetOrThrowException<A>(this Option<A> op, string errorMessage)
            => op.IfNone(() => throw new Exception(errorMessage));

        /// <summary>
        /// 空文字と空白文字もNoneになるOptionを返します。
        /// </summary>
        public static Option<string> OptionalT(object? obj)
            => Optional(obj).Map(m => m.ToString() ?? string.Empty).Filter(s => !string.IsNullOrWhiteSpace(s));
    }
}
