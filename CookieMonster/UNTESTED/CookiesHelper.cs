using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dinah.Core.Collections.Generic;

namespace CookieMonster
{
    public static class CookiesHelper
    {
        internal static IEnumerable<IBrowser> GetBrowsers()
            => AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IBrowser).IsAssignableFrom(p) && !p.IsAbstract && !p.IsInterface)
                .Select(t => Activator.CreateInstance(t) as IBrowser)
                .ToList();

        /// <summary>all. including expired</summary>
        public static async Task<IEnumerable<CookieValue>> GetAllCookieValuesAsync()
        {
            //// foreach{await} runs in serial
            //var allCookies = new List<CookieValue>();
            //foreach (var b in GetBrowsers())
            //{
            //    var browserCookies = await b.GetAllCookiesAsync().ConfigureAwait(false);
            //    allCookies.AddRange(browserCookies);
            //}

            //// WhenAll runs in parallel
            // this 1st step LOOKS like a bug which runs each method until completion. However, since we don't use await, it's actually returning a Task. That resulting task is awaited asynchronously
            var browserTasks = GetBrowsers().Select(b => b.GetAllCookiesAsync());
            var results = await Task.WhenAll(browserTasks).ConfigureAwait(false);
            var allCookies = results.SelectMany(a => a).ToList();

            if (allCookies.Any(c => !c.IsValid))
                throw new Exception("some date time was converted way too far");

            foreach (var c in allCookies)
                c.Domain = c.Domain.TrimEnd('/');

            // for each domain+name, only keep the 1 with the most recent access
            var sortedCookies = allCookies
                .OrderByDescending(c => c.LastAccess)
                .DistinctBy(c => new { c.Domain, c.Name })
                .ToList();

            return sortedCookies;
        }

        /// <summary>not expired</summary>
        public static async Task<IEnumerable<CookieValue>> GetLiveCookieValuesAsync()
            => (await GetAllCookieValuesAsync().ConfigureAwait(false))
            .Where(c => !c.HasExpired)
            .ToList();
    }
}
