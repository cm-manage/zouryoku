using LanguageExt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using static LanguageExt.Prelude;

namespace CommonLibrary.Extensions
{
    public static class IEnumerableExtensions
    {
        public static Option<A> FirstOption<A>(this IEnumerable<A> enumerable)
        {
            var array = enumerable as A[] ?? enumerable.ToArray();
            if (array.Empty())
            {
                return Option<A>.None;
            }
            return Option<A>.Some(array.First());
        }

        public static Option<A> LastOption<A>(this IEnumerable<A> enumerable, Func<A, bool> test)
        {
            var array = enumerable as A[] ?? enumerable.ToArray();
            return array.Where(test).LastOption();
        }

        public static Option<A> LastOption<A>(this IEnumerable<A> enumerable)
        {
            var array = enumerable as A[] ?? enumerable.ToArray();
            if (array.Empty())
            {
                return Option<A>.None;
            }
            return Option<A>.Some(array.Last());
        }

        public static Option<A> FirstOption<A>(this IEnumerable<A> enumerable, Func<A, bool> test)
        {
            var array = enumerable as A[] ?? enumerable.ToArray();
            return array.Where(test).FirstOption();
        }

        public static Option<B> MaxOption<A, B>(this IEnumerable<A> enumerable, Func<A, B> func)
        {
            var array = enumerable as A[] ?? enumerable.ToArray();
            if (array.Empty())
            {
                return None;
            }
            return Optional(array.Max(func));
        }

        public static Option<B> MinOption<A, B>(this IEnumerable<A> enumerable, Func<A, B> func)
        {
            var array = enumerable as A[] ?? enumerable.ToArray();
            if (array.Empty())
            {
                return None;
            }
            return Optional(array.Min(func));
        }


        public static IEnumerable<IEnumerable<T>> OriginalChunk<T>(this IEnumerable<T> source, int chunkSize)
        {
            if (chunkSize <= 0)
                throw new ArgumentException("Chunk size must be greater than 0.", nameof(chunkSize));

            while (source.Any())
            {
                yield return source.Take(chunkSize);
                source = source.Skip(chunkSize);
            }
        }

        /// <summary>
        /// ListにはForEachが定義されている、そのために毎回ToList()するのが面倒なので
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
            => source.ToList().ForEach(action);

        //public static IEnumerable<(T, T)> ToPair<T>(this IEnumerable<T> list) where T : OnOffPair
        //    => list
        //        .OrderBy(w => w.TaskTime)
        //        .Chunk(2)
        //        .Select(ienum => ienum.ToList())
        //        .Select(pair => (pair[0], pair[1]));

        //public static List<R> ToPairAndApply<T, R>(this IEnumerable<T> list, Func<(T, T), R> func) where T : OnOffPair
        //    => list
        //        .ToPair()
        //        .Select(pair => func(pair))
        //        .ToList();

        public static List<A> ToList<A>(this Array array)
        {
            return array.GetEnumerator().ToList<A>();
        }

        public static List<A> ToList<A>(this IEnumerator enumerator)
        {
            var list = new List<A>();
            while (enumerator.MoveNext())
            {
                list.Add((A)enumerator.Current);
            }
            return list;
        }

        public static Option<A> IndexOption<A>(this IEnumerable<A> enumerable, int index)
        {
            try
            {
                var array = enumerable as A[] ?? enumerable.ToArray();
                return Option<A>.Some(array[index]);
            }
            catch
            {
                return Option<A>.None;
            }
        }

        public static IEnumerable<Tuple<A, int>> ZipWithIndex<A>(this IEnumerable<A> enumerable, int startIndex = 0)
        {
            var array = enumerable as A[] ?? enumerable.ToArray();
            var indexes = Enumerable.Range(startIndex, array.Count());
            return array.Zip(indexes, System.Tuple.Create);
        }

        public static void ForEach<A, B>(this IEnumerable<Tuple<A, B>> enumerable, Action<A, B> action)
        {
            enumerable.ForEach(tuple => action(tuple.Item1, tuple.Item2));
        }

        public static void ForEach<A, B, C>(this IEnumerable<Tuple<A, B, C>> enumerable, Action<A, B, C> action)
        {
            enumerable.ForEach(tuple => action(tuple.Item1, tuple.Item2, tuple.Item3));
        }

        public static A AddReturn<A>(this ICollection<A> list, A a)
        {
            list.Add(a);
            return a;
        }

        /// <summary>
        /// ソート対象を指定するラムダを引数にとる関数を返します。
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="q"></param>
        /// <param name="ascOrDesc"></param>
        /// <returns></returns>
        public static Func<Func<A, dynamic?>, IOrderedEnumerable<A>> GridSortOrder<A>(this IEnumerable<A> q, AscOrDesc ascOrDesc)
            => sortField => ascOrDesc == AscOrDesc.Desc
                ? q.OrderByDescending(sortField)
                : q.OrderBy(sortField);

        public static List<A> GridLimitOffset<A>(this IEnumerable<A> data, int limit, int offset)
            => data.Skip(limit).Take(offset).ToList();

        public static bool Empty<A>(this IEnumerable<A>? data)
            => data == null || !data.Any();

        public static bool NotEmpty<A>([NotNullWhen(true)] this IEnumerable<A>? data)
            => !data.Empty();

        /// <summary>
        /// リスト結合（要素無の場合空文字を返す）
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="data"></param>
        /// <param name="sep">結合文字</param>
        /// <returns></returns>
        public static string Join<A>(this IEnumerable<A> data, string sep = ",")
        {
            var list = data.Where(d => d != null).Map(d => d?.ToString() ?? "").Where(d => !string.IsNullOrEmpty(d)).ToList();
            return list.NotEmpty()
                ? list.Aggregate((a, b) => a += $"{sep}{b}")
                : "";
        }

        /// <summary>
        /// コレクションの要素が存在するかの確認
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Option<List<A>> ToListOption<A>(this IQueryable<A> list, Func<A, bool>? predicate = null)
        {
            var result = predicate == null ? list.ToList() : list.Where(predicate).ToList();
            return result.Empty() ? Option<List<A>>.None : Option<List<A>>.Some(result);
        }

        /// <summary>
        /// コレクションの要素が存在するかの確認
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Option<List<A>> ToListOption<A>(this IEnumerable<A> list, Func<A, bool>? predicate = null)
            => list.Empty() ? Option<List<A>>.None : list.AsQueryable().ToListOption(predicate);

        /// <summary>
        /// コレクションの要素が1つのみ存在するかの確認
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="list"></param>
        public static Option<A> ToSingleOption<A>(this IQueryable<A> list, Func<A, bool>? predicate = null)
            => list.ToListOption(predicate)
                .Some(x => x.Count() == 1 ? Option<A>.Some(x.First()) : Option<A>.None)
                .None(() => Option<A>.None);

        /// <summary>
        /// コレクションの要素が1つのみ存在するかの確認
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="list"></param>
        public static Option<A> ToSingleOption<A>(this IEnumerable<A> list)
            => list.AsQueryable().ToSingleOption();

        /// <summary>
        /// null許容の数値リスト合計を取得する
        /// 要素0または全て null のときは null を返す
        /// </summary>
        public static A? SumNullable<A>(this IEnumerable<A?> list) where A : struct, INumber<A>
            => list.Empty() || !list.Any(x => x != null)
                ? null
                : list.Where(x => x != null)
                    .Aggregate((a, b) => a + b);

        /// <summary>
        /// null許容の数値リスト合計を取得する
        /// 要素0または全て null のときは null を返す
        /// </summary>
        public static B? SumNullable<A, B>(this IEnumerable<A> list, Func<A, B?> selector) where B : struct, INumber<B>
            => list.Select(selector).SumNullable();

        /// <summary>
        /// シーケンスを<paramref name="count"/>の数を上限に分割します。
        /// </summary>
        /// <param name="source">シーケンス。</param>
        /// <param name="count">分割した要素の中に含まれるシーケンスの数。</param>
        /// <returns>分割された要素。</returns>
        public static IEnumerable<IEnumerable<TSource>> Buffer<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (count < 0) throw new ArgumentOutOfRangeException("count");

            var sourceSize = source.Count();
            return Enumerable.Range(0, sourceSize / count + (sourceSize % count == 0 ? 0 : 1)).Select(i => source.Skip(i * count).Take(count));
        }
    }
}
