using FindFun.Server.Features.Parks.Create;
using FindFun.Server.Features.Parks.Get;
using FindFun.Server.Features.Reviews;

namespace FindFun.Server.Features.Parks;

public static class ParksEndpoint
{
    public static RouteGroupBuilder MapParks(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/parks");
        group.MapCreateParkEndpoint();
        group.MapGetParkEndpoint();
        group.MapGetParksEndpoint();
        return group;
    }
}