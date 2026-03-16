using Microsoft.AspNetCore.Mvc;

namespace FindFun.Server.Features.Parks.Get;

public static class GetParkEndpoint
{
    public static void MapGetParkEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("{parkId:int}", async (GetParkHandler handler, int parkId, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(parkId, cancellationToken);
            return result.IsValid ? Results.Ok(result.Data) : Results.Problem(result!.ProblemDetails!);
        })
        .WithName("GetPark")
        .WithTags("Parks");
    }
}