using FindFun.Server.Features.Parks;
using FindFun.Server.Features.Reviews;

namespace FindFun.Server.Features;

public static class EndpointGroup
{
        public static void MapEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapParks()
            .MapReviews();
    }
}
