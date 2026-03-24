using FindFun.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FindFun.Server.Features.Reviews.Get
{
    public static class GetReview
    {
        public static void MapGetReviews(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{id:guid}", async (Guid id, FindFunDbContext db, CancellationToken cancellationToken) =>
            {
                var review = await db.Reviews.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
                return review is null
                    ? Results.NotFound()
                    : Results.Ok(new
                    {
                        review.Id,
                        review.Content,
                        review.Rating,
                        review.UserName,
                        review.CreatedAt,
                        review.ParkId
                    });
            })
            .WithName("GetReview")
            .WithTags("Reviews");

        }
    }
}