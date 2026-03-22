using FindFun.Server.Shared.Pagination;

namespace FindFun.Server.Features.Parks.Get;

public sealed record GetParksRequest(
    string? Search,
    string? SortBy,
    string? SortDirection,
    int Page = 1,
    int PageSize = 10,
    int? MunicipalityId = null,
    double? Latitude = null,
    double? Longitude = null,
    double? RadiusKm = null
);

public sealed record PagedParksResponse(
    IReadOnlyList<GetParkResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
) : PagedResponse<GetParkResponse>(Items, TotalCount, Page, PageSize, TotalPages);
