using FindFun.Server.Shared.File;
using Microsoft.AspNetCore.Mvc;

namespace FindFun.Server.Features.Parks.Create;

public static class CreateParkEndpoint
{
    public static void MapCreateParkEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (CreateParkHandler handler, FileUpLoad fileUpLoad, [FromForm] CreateParkRequest request, CancellationToken cancellationToken) =>
        {
            // should return the id of the created park and use CreatedAtRoute instead of OK
            var result = await handler.HandleAsync(request, fileUpLoad, cancellationToken);
            return result.IsValid ? Results.CreatedAtRoute("GetPark", new { parkId = result?.Data?.ParkId }, result?.Data) : Results.Problem(result!.ProblemDetails!);
        })
        .WithName("CreatePark")
        .WithTags("Parks").DisableAntiforgery();
        // .AddEndpointFilter<RequestValidationFilter<CreateParkRequest>>(); now this is not needed due to the latest inprovements in data annotations for records and validation in .NET 10 if this is keep the validation will be executed twice 
    }
}
