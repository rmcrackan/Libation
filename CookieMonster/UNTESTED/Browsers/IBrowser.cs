using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CookieMonster
{
    internal interface IBrowser
    {
        Task<IEnumerable<CookieValue>> GetAllCookiesAsync();
    }
}
