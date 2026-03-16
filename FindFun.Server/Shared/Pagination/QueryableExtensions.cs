using Microsoft.EntityFrameworkCore;

namespace FindFun.Server.Shared.Pagination;

public static class QueryableExtensions
{
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(this IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var pageAdjusted = page <= 0 ? 1 : page;
        var pageSizeAdjusted = pageSize <= 0 ? 10 : pageSize;

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageAdjusted - 1) * pageSizeAdjusted)
            .Take(pageSizeAdjusted)
            .ToListAsync(cancellationToken);

        var totalPages = pageSizeAdjusted == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSizeAdjusted);

        return new PagedResponse<T>(items, totalCount, pageAdjusted, pageSizeAdjusted, totalPages);
    }
}
