using CommonLibrary.Extensions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Model.Extensions
{
    public static class IQueryableExtensions
    {
        [Obsolete("use FirstOrDefaultAsync")]
        public static Option<A> FirstOptionQ<A>(this IQueryable<A> query, Expression<Func<A, bool>> func) where A : class
            => query.Where(func).FirstOptionQ();
        
        [Obsolete("use FirstOrDefaultAsync")]
        public static async Task<Option<A>> FirstOptionQAsync<A>(this IQueryable<A> query, Expression<Func<A, bool>> func) where A : class
            => await query.Where(func).FirstOptionQAsync();

        [Obsolete("use FirstOrDefaultAsync")]
        public static Option<A> FirstOptionQ<A>(this IQueryable<A> query) where A : class
            => Optional(query.FirstOrDefault());

        [Obsolete("use FirstOrDefaultAsync")]
        public static async Task<Option<A>> FirstOptionQAsync<A>(this IQueryable<A> query) where A : class
            => Optional(await query.FirstOrDefaultAsync());

        public static Option<B> MaxOptionQ<A, B>(this IQueryable<A> query, Expression<Func<A, B>> selector) where A : class
        {
            if (query.Any())
            {
                return Optional(query.Max(selector));
            }
            return None;
        }

        public static Option<B> MinOptionQ<A, B>(this IQueryable<A> query, Expression<Func<A, B>> selector) where A : class
        {
            if (query.Any())
            {
                return Optional(query.Min(selector));
            }
            return None;
        }

        /// <summary>
        /// ソート対象を指定するラムダを引数にとる関数を返します。
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="q"></param>
        /// <param name="ascOrDesc"></param>
        /// <returns></returns>
        public static Func<Expression<Func<A, dynamic?>>, IOrderedQueryable<A>> GridSortOrder<A>(this IQueryable<A> q, AscOrDesc ascOrDesc)
            => sortField => ascOrDesc == AscOrDesc.Desc
                ? q.OrderByDescending(sortField)
                : q.OrderBy(sortField);

        public static List<A> GridLimitOffset<A>(this IQueryable<A> data, int limit, int offset)
            => data.Skip(limit).Take(offset).ToList();

        public static async Task<List<A>> GridLimitOffsetAsync<A>(this IQueryable<A> data, int limit, int offset)
            => await data.Skip(limit).Take(offset).ToListAsync();
    }
}
