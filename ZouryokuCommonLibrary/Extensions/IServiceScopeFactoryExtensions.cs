using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Model.Data;

namespace ZouryokuCommonLibrary.Extensions
{
    public static class IServiceScopeFactoryExtensions
    {
        public static void Using(this IServiceScopeFactory scopeFactory, Action<ZouContext> action)
        {
            //logger.LogDebug($"{TaskName} 処理を開始します。");
            using (var scope = scopeFactory.CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<ZouContext>())
            using (var transaction = db.Database.BeginTransaction())
            {
                action(db);
                // SaveChange()呼んだ後でも、途中でこけたら RollBack したいので。
                transaction.Commit();
            }
        }

        public static async Task UsingAsync(this IServiceScopeFactory scopeFactory, Func<ZouContext, Task> action)
        {
            using (var scope = scopeFactory.CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<ZouContext>())
            using (var transaction = db.Database.BeginTransaction())
            {
                await action(db);
                // SaveChange()呼んだ後でも、途中でこけたら RollBack したいので。
                transaction.Commit();
            }
        }

        public static A Using<A>(this IServiceScopeFactory scopeFactory, Func<ZouContext, A> func)
        {
            //logger.LogDebug($"{TaskName} 処理を開始します。");
            using (var scope = scopeFactory.CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<ZouContext>())
            using (var transaction = db.Database.BeginTransaction())
            {
                var result = func(db);
                // SaveChange()呼んだ後でも、途中でこけたら RollBack したいので。
                transaction.Commit();
                return result;
            }
        }

        public static async Task<A> UsingAsync<A>(this IServiceScopeFactory scopeFactory, Func<ZouContext, Task<A>> func)
        {
            using (var scope = scopeFactory.CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<ZouContext>())
            using (var transaction = db.Database.BeginTransaction())
            {
                var result = await func(db);
                // SaveChange()呼んだ後でも、途中でこけたら RollBack したいので。
                transaction.Commit();
                return result;
            }
        }

        /// <summary>
        /// transaction が利用可能なDB接続
        /// </summary>
        public static void Using(this IServiceScopeFactory scopeFactory, Action<ZouContext, IDbContextTransaction> action)
        {
            //logger.LogDebug($"{TaskName} 処理を開始します。");
            using (var scope = scopeFactory.CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<ZouContext>())
            using (var transaction = db.Database.BeginTransaction())
            {
                action(db, transaction);
                // SaveChange()呼んだ後でも、途中でこけたら RollBack したいので。
                transaction.Commit();
            }
        }

        /// <summary>
        /// transaction が利用可能なDB接続
        /// </summary>
        public static A Using<A>(this IServiceScopeFactory scopeFactory, Func<ZouContext, IDbContextTransaction, A> func)
        {
            //logger.LogDebug($"{TaskName} 処理を開始します。");
            using (var scope = scopeFactory.CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<ZouContext>())
            using (var transaction = db.Database.BeginTransaction())
            {
                var result = func(db, transaction);
                // SaveChange()呼んだ後でも、途中でこけたら RollBack したいので。
                transaction.Commit();
                return result;
            }
        }
    }
}
