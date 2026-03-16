using System.Collections.Generic;

namespace FindFun.Server.Shared.Pagination;

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
