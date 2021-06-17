using System;
using System.Linq;

namespace DataLayer
{
    public static class GenericPaging
    {
        public static IQueryable<T> Page<T>(this IQueryable<T> query, int pageNumZeroStart, int pageSize)
        {
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "pageSize must be at least 1");

            if (pageNumZeroStart > 0)
                query = query.Skip(pageNumZeroStart * pageSize);

            return query.Take(pageSize);
        }
    }
}