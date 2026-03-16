using Microsoft.AspNetCore.Mvc;

namespace FindFun.Server.Features.Parks.Get;

public static class GetParksEndpoint
{
    public static void MapGetParksEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("", async (
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] int? municipalityId,
            [FromQuery] double? latitude,
            [FromQuery] double? longitude,
            [FromQuery] double? radiusKm,
            GetParksHandler handler,
            CancellationToken cancellationToken) =>
        {
            var request = new GetParksRequest(
                Search: search,
                SortBy: sortBy,
                SortDirection: sortDirection,
                Page: page ?? 1,
                PageSize: pageSize ?? 10,
                MunicipalityId: municipalityId,
                Latitude: latitude,
                Longitude: longitude,
                RadiusKm: radiusKm
            );

            var result = await handler.HandleAsync(request, cancellationToken);
            return result.IsValid ? Results.Ok(result.Data) : Results.Problem(result!.ProblemDetails!);
        })
        .WithName("GetParks")
        .WithTags("Parks");
    }
}
