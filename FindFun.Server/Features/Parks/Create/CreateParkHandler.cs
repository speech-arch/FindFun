using FindFun.Server.Domain;
using FindFun.Server.Infrastructure;
using FindFun.Server.Shared;
using FindFun.Server.Shared.File;
using FindFun.Server.Shared.Validations;
using Microsoft.EntityFrameworkCore;

namespace FindFun.Server.Features.Parks.Create;

public class CreateParkHandler(FindFunDbContext dbContext, ILogger<CreateParkHandler> logger)
{
    // this should return the image urls and the id of the created park 
    public async Task<Result<CreateParkResponse>> HandleAsync(CreateParkRequest request, FileUpLoad fileUpLoad, CancellationToken cancellationToken)
    {
        // use transaction

        var coordinates = ValidationHelper.ParseCoordinate(request.Coordinates);
        if (!coordinates.IsValid)
            return Result<CreateParkResponse>.Failure(coordinates.ProblemDetails!);

        var amenityGroup = ValidationHelper.ParseAmenityGroup(request.Amenities);
        if (!amenityGroup.IsValid)
            return Result<CreateParkResponse>.Failure(amenityGroup.ProblemDetails!);

        var municipalityId = await dbContext.Municipalities.AsNoTracking()
            .Where(m => m.OfficialNa6 == request.Locality)
            .Select(m => m.Gid)
            .FirstOrDefaultAsync(cancellationToken);

        if (municipalityId == 0)
        {
            return StatusCodes.Status400BadRequest.CreateProblemResult<CreateParkRequest, CreateParkResponse>("Locality", "Locality not found.");
        }

        var existingAddress = await dbContext.Addresses
            .Include(a => a.Street)
            .FirstOrDefaultAsync(a =>
                a.Line == request.FormattedAddress
                && a.Street!.MunicipioGid == municipalityId
                && a.Street.Name == request.Street, cancellationToken);

        if (existingAddress is not null)
        {
            var parkExists = await dbContext.Parks
                .AsNoTracking()
                .AnyAsync(p => p.AddressId == existingAddress.Id, cancellationToken);

            if (parkExists)
                return StatusCodes.Status409Conflict.CreateProblemResult<CreateParkRequest, CreateParkResponse>("Address", "The address already exists.");
        }

        Street? street = existingAddress?.Street;
        if (street is null)
        {
            street = await dbContext.Streets
                .FirstOrDefaultAsync(s => s.Name == request.Street && s.MunicipioGid == municipalityId, cancellationToken);

            if (street is null)
            {
                street = new Street(request.Street!, municipalityId);
                await dbContext.Streets.AddAsync(street, cancellationToken);
            }
        }

        var address = existingAddress ?? new Address(
            request.FormattedAddress!,
            request.PostalCode!,
            street!,
            coordinates.Data!.Longitude,
            coordinates.Data.Latitude,
            request.Number!
        );

        if (existingAddress is null)
            await dbContext.Addresses.AddAsync(address, cancellationToken);

        var closingScheduleEntries = ValidationHelper.ParseClosingSchedule(request.ClosingSchedule);
        var park = new Park(
            request.Name,
            request.Description!,
            address,
            request.EntranceFee,
            request.IsFree,
            request.Organizer,
            request.ParkType,
            request.AgeRecommendation

        );
        await dbContext.Parks.AddAsync(park, cancellationToken);

        if (closingScheduleEntries.Count > 0)
        {
            var closingSchedule = new ClosingSchedule(closingScheduleEntries);
            park.SetClosingSchedule(closingSchedule);
            await dbContext.AddAsync(closingSchedule, cancellationToken);
        }

        var uploaded = await fileUpLoad.FilesUpLoader(request.ParkImages, "parks", cancellationToken);
        if (uploaded.Any(r => !r.IsValid))
        {
            await FileValidation.DeleteUploadedFilesAsync(uploaded, fileUpLoad, cancellationToken);
            return StatusCodes.Status400BadRequest.CreateProblemResult<CreateParkRequest, CreateParkResponse>("ParkImages", "One or more images failed to upload.");
        }

        var images = uploaded.Select(r => new ParkImage(r.Data!)).ToList();
        park.AddImages(images);

        var (amenityNames, amenityDescription) = amenityGroup.Data;
        var amenity = new Amenity { Name = amenityNames, Description = amenityDescription ?? string.Empty };
        park.AddAmenity(amenity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<CreateParkResponse>.Success(new CreateParkResponse(park.Id, images.Select(i => i.Url).ToList()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create park '{ParkName}'", request.Name);
            await FileValidation.DeleteUploadedFilesAsync(uploaded, fileUpLoad, cancellationToken);
            return StatusCodes.Status500InternalServerError.CreateProblemResult<CreateParkRequest, CreateParkResponse>("Park", "Failed to create park.");
        }
    }
}