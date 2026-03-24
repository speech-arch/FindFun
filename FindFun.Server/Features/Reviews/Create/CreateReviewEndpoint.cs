using Microsoft.AspNetCore.Mvc;

namespace FindFun.Server.Features.Reviews.Create;

public static class CreateReviewEndpoint
{
    public static void MapReviewsEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:int}", async (int id, [FromBody] CreateReviewRequest request, CreateReviewHandler handler, CancellationToken cancellationToken) =>
        {
            var body = request with { Id = id.ToString() };
            var result = await handler.HandleAsync(body, cancellationToken);
            return result.IsValid
                ? Results.CreatedAtRoute("GetReview", new { id = result.Data!.ReviewId }, result.Data)
                : Results.Problem(result!.ProblemDetails!);
        })
        .WithName("CreateReview")
        .WithTags("Reviews");
    }
}
