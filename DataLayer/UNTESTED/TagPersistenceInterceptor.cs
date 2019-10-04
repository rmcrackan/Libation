using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core.Collections.Generic;
using Dinah.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    internal class TagPersistenceInterceptor : IDbInterceptor
    {
        public void Executing(DbContext context)
        {
            doWork__EFCore(context);
        }

        public void Executed(DbContext context) { }

        static void doWork__EFCore(DbContext context)
        {
            // persist tags:
            var modifiedEntities = context.ChangeTracker.Entries().Where(p => p.State.In(EntityState.Modified, EntityState.Added)).ToList();
            var tagSets = modifiedEntities.Select(e => e.Entity as UserDefinedItem).Where(a => a != null).ToList();
            foreach (var t in tagSets)
                FileManager.TagsPersistence.Save(t.Book.AudibleProductId, t.Tags);
        }

        #region // notes: working with proxies, esp EF 6
        // EF 6: entities are proxied with lazy loading when collections are virtual
        // EF Core: lazy loading is supported in 2.1 (there is a version of lazy loading with proxy-wrapping and a proxy-less version with DI) but not on by default and are not supported here

        //static void doWork_EF6(DbContext context)
        //{
        //    var modifiedEntities = context.ChangeTracker.Entries().Where(p => p.State == EntityState.Modified).ToList();
        //    var unproxiedEntities = modifiedEntities.Select(me => UnProxy(context, me.Entity)).ToList();

        //    // persist tags
        //    var tagSets = unproxiedEntities.Select(ue => ue as UserDefinedItem).Where(a => a != null).ToList();
        //    foreach (var t in tagSets)
        //        FileManager.TagsPersistence.Save(t.ProductId, t.TagsRaw);
        //}

        //// https://stackoverflow.com/a/25774651
        //private static T UnProxy<T>(DbContext context, T proxyObject) where T : class
        //{
        //    // alternative: https://docs.microsoft.com/en-us/ef/ef6/fundamentals/proxies
        //    var proxyCreationEnabled = context.Configuration.ProxyCreationEnabled;
        //    try
        //    {
        //        context.Configuration.ProxyCreationEnabled = false;
        //        return context.Entry(proxyObject).CurrentValues.ToObject() as T;
        //    }
        //    finally
        //    {
        //        context.Configuration.ProxyCreationEnabled = proxyCreationEnabled;
        //    }
        //}
        #endregion
    }
}
