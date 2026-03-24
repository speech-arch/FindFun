using FindFun.Server.Features.Reviews.Create;
using FindFun.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FindFun.Server.Features.Reviews;

public static class ReviewsEndpoint
{
    public static IEndpointRouteBuilder MapReviews(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reviews").WithTags("Reviews");
        group.MapReviewsEndpoint();
        group.MapGet("/{id:guid}", async (Guid id, FindFunDbContext db, CancellationToken cancellationToken) =>
        {
            var review = await db.Reviews.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
            return review is null
                ? Results.NotFound()
                : Results.Ok(new
                {
                    Id = review.Id,
                    review.Content,
                    review.Rating,
                    review.UserName,
                    review.CreatedAt,
                    review.ParkId
                });
        })
        .WithName("GetReview")
        .WithTags("Reviews");
        return app;
    }
}
