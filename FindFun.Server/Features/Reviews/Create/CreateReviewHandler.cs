using FindFun.Server.Domain;
using FindFun.Server.Infrastructure;
using FindFun.Server.Shared;
using FindFun.Server.Shared.Validations;
using FindFun.Server.Shared.Resources;

namespace FindFun.Server.Features.Reviews.Create;

public class CreateReviewHandler
{
    private readonly FindFunDbContext _dbContext;
    private readonly ILogger<CreateReviewHandler> _logger;

    public CreateReviewHandler(FindFunDbContext dbContext, ILogger<CreateReviewHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

public sealed record CreateReviewResponse(Guid ReviewId);

    public async Task<Result<CreateReviewResponse>> HandleAsync(CreateReviewRequest request, CancellationToken cancellationToken)
    {
        if (!int.TryParse(request.Id, out var parkId))
        {
            return StatusCodes.Status400BadRequest.CreateProblemResult<CreateReviewRequest, CreateReviewResponse>(Messages.FieldId, Messages.ValidationErrorsOccurred);
        }

        var park = await _dbContext.Parks.FindAsync([parkId], cancellationToken);
        if (park is null)
        {
            return StatusCodes.Status404NotFound.CreateProblemResult<CreateReviewRequest, CreateReviewResponse>(Messages.FieldId, Messages.ParkNotFound);
        }

        var review = new Review(request.Content, request.Rating, request.UserName)
        {
            ParkId = parkId
        };

        await _dbContext.Reviews.AddAsync(review, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<CreateReviewResponse>.Success(new CreateReviewResponse(review.Id));
    }
}
