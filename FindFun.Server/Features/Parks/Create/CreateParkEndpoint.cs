using FindFun.Server.Shared;
using FindFun.Server.Shared.File;
using FindFun.Server.Shared.Validations;
using Microsoft.AspNetCore.Mvc;

namespace FindFun.Server.Features.Parks.Create;

public static class CreateParkEndpoint
{
    public static void MapCreateParkEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("", async (CreateParkHandler handler, FileUpLoad fileUpLoad, [FromForm] CreateParkRequest request, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, fileUpLoad, cancellationToken);
            return result.IsValid ? Results.Ok(result.Data) : Results.Problem(result!.ProblemDetails!);
        })
        .WithName("CreatePark")
        .WithTags("Parks").DisableAntiforgery();
        // .AddEndpointFilter<RequestValidationFilter<CreateParkRequest>>(); now this is not needed due to the latest inprovements in data annotations for records and validation in .NET 10 if this is keep the validation will be executed twice 
    }
}
