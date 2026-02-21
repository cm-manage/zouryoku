using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Model.Extensions
{
    public static class EntityExtensions
    {
        public static A AddReturn<A>(this DbSet<A> entities, A a) where A : class
        {
            return entities.Add(a).Entity;
        }
        public static async Task<A> AddReturnAsync<A>(this DbSet<A> entities, A a) where A : class
        {
            return (await entities.AddAsync(a)).Entity;
        }

        public static void SetOriginalValue<T, TProperty>(
            this DbContext context,
            T entity,
            Expression<Func<T, TProperty>> propertyExpression,
            TProperty originalValue) where T : class
        {
            context.Entry(entity).Property(propertyExpression).OriginalValue = originalValue;
        }
    }
}
