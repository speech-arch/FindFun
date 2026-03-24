using FindFun.Server.Infrastructure;
using FindFun.Server.Shared;
using FindFun.Server.Shared.Resources;
using FindFun.Server.Shared.Validations;
using Microsoft.EntityFrameworkCore;

namespace FindFun.Server.Features.Parks.Get;

public class GetParkHandler(FindFunDbContext dbContext)
{
    public async Task<Result<GetParkResponse>> HandleAsync(int parkId, CancellationToken cancellationToken)
    {
        var response = await dbContext.Parks
        .AsNoTracking().AsSplitQuery()
        .Where(p => p.Id == parkId)
            .Select(p => new GetParkResponse(
            p.Id.ToString(),
            p.Name,
            p.Description ?? string.Empty,
            p.AddressId.ToString(),
            p.Address.Line,
            p.Address.Coordinates!.Y,
            p.Address.Coordinates.X,
            p.Address.Street!.Municipio!.OfficialNa6,
            p.Address.Street.Municipio.OfficialNa4,
            p.Address.Street.Municipio.OfficialNa,
                p.Reviews.Select(r => new GetParkReviewResponse(r.Id.ToString(), r.UserId.ToString(), r.Content, r.Rating, r.CreatedAt.ToString("o"))).ToList(),
            p.Amenities.Select(a => a.Amenity.Name).ToList(),
            p.ParkType ?? string.Empty,
            p.Images.Select(i => i.Url).ToList(),
            p.Address.Street.Name,
            0
        ))
        .FirstOrDefaultAsync(cancellationToken);

        if (response is null)
        {
            return StatusCodes.Status404NotFound.CreateProblemResult<GetParkRequest, GetParkResponse>(
                Messages.FieldParkId,
                Messages.ParkNotFound
            );
        }
        return Result<GetParkResponse>.Success(response);
    }
}