namespace SecurityService.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    [ExcludeFromCodeCoverage]
    public static class Extensions
    {
        #region Methods

        /// <summary>
        /// Converts to listasyncsafe.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">source</exception>
        public static Task<List<TSource>> ToListAsyncSafe<TSource>(this IQueryable<TSource> source,
                                                                   CancellationToken cancellationToken)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!(source is IAsyncEnumerable<TSource>))
                return Task.FromResult(source.ToList());
            return source.ToListAsync(cancellationToken);
        }

        #endregion
    }
}