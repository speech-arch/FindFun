using FindFun.Server.Features.Reviews.Create;
using FindFun.Server.Features.Reviews.Get;
using FindFun.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FindFun.Server.Features.Reviews;

public static class ReviewsEndpoint
{
    public static IEndpointRouteBuilder MapReviews(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reviews").WithTags("Reviews");
        group.MapReviewsEndpoint();
        group.MapGetReviews();
        return app;
    }
}
