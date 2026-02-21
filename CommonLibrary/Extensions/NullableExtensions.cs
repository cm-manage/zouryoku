using System;

namespace CommonLibrary.Extensions
{
    public static class NullableExtensions
    {

    }

    // ジェネリック型パラメータ・メソッド名が同じだが、異なるジェネリック制約で定義したいためクラスを分ける
    public static class NullableExtensionsToClass
    {
        /// <summary>
        /// 値があるときのみ class => class 変換を行う
        /// </summary>
        public static B? IfHasValue<A, B>(this A? a, Func<A, B> func) where A : class where B : class
            => a != null ? func(a) : null;

        /// <summary>
        /// 値があるときのみ struct => class 変換を行う
        /// </summary>
        public static B? IfHasValue<A, B>(this A? a, Func<A, B> func) where A : struct where B : class
            => a.HasValue ? func(a.Value) : null;
    }

    // ジェネリック型パラメータ・メソッド名が同じだが、異なるジェネリック制約で定義したいためクラスを分ける
    public static class NullableExtensionsToStruct
    {
        /// <summary>
        /// 値があるときのみ struct => struct 変換を行う
        /// </summary>
        public static B? IfHasValue<A, B>(this A? a, Func<A, B> func) where A : struct where B : struct
            => a.HasValue ? func(a.Value) : null;

        /// <summary>
        /// 値があるときのみ class => struct 変換を行う
        /// </summary>
        public static B? IfHasValue<A, B>(this A? a, Func<A, B> func) where A : class where B : struct
            => a != null ? func(a) : null;
    }
}