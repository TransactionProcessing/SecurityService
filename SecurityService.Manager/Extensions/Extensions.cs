using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.Manager.Extensions
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Primitives;

    public static class Extensions
    {
        public static Task<List<TSource>> ToListAsyncSafe<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!(source is IAsyncEnumerable<TSource>))
                return Task.FromResult(source.ToList());
            return source.ToListAsync(cancellationToken);
        }
    }
}
