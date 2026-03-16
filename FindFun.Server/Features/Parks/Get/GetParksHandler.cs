using FindFun.Server.Domain;
using FindFun.Server.Infrastructure;
using FindFun.Server.Shared;
using FindFun.Server.Shared.Pagination;
using FindFun.Server.Shared.Sorting;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace FindFun.Server.Features.Parks.Get;

public class GetParksHandler(FindFunDbContext dbContext)
{
    private const double DegreesPerKm = 111.32;

    public async Task<Result<PagedParksResponse>> HandleAsync(GetParksRequest request, CancellationToken cancellationToken)
    {
        var query = dbContext.Parks
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => true);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchValue = request.Search.Trim();
            query = query.Where(p => p.Name.Contains(searchValue)
            || p.Address.Line.Contains(searchValue)
            || p.Address.Street!.Name.Contains(searchValue)
            );
        }

        if (request.MunicipalityId is { } municipalityId)
        {
            query = query.Where(p => p.Address.Street!.Municipio!.Gid == municipalityId);
        }

        var sortDir = (request.SortDirection ?? SortDirections.Asc).ToLowerInvariant();
        var sortBy = (request.SortBy ?? SortKeys.Name).ToLowerInvariant();

        Point? point = null;
        if (request is { Latitude: { } latitude, Longitude: { } longitude })
        {
            point = new Point(longitude, latitude) { SRID = 4326 };

            if (request.RadiusKm is { } radiusKm)
            {
                var radiusDegrees = radiusKm / DegreesPerKm;
                query = query.Where(p => p.Address.Coordinates!.Distance(point) <= radiusDegrees);
            }
        }
        query = point is not null ? ApplySortDirection(query, sortDir, point) : ApplySort(query, sortDir, sortBy);

        var projected = query
            .Select(p => new GetParkResponse(
                p.Id.ToString(),
                p.Name,
                p.Description,
                p.AddressId.ToString(),
                p.Address.Line,
                p.Address.Coordinates!.Y,
                p.Address.Coordinates.X,
                p.Address.Street!.Municipio!.OfficialNa6,
                p.Address.Street.Municipio.OfficialNa4,
                p.Address.Street.Municipio.OfficialNa,
                Array.Empty<GetParkReviewResponse>(),
                p.Amenities.Select(a => a.Amenity.Name).ToList(),
                p.ParkType!,
                p.Images.Select(i => i.Url).ToList(),
                p.Address.Street.Name,
                0
            ));

        var paged = await projected.ToPagedResponseAsync(request.Page, request.PageSize, cancellationToken);

        var response = new PagedParksResponse(paged.Items, paged.TotalCount, paged.Page, paged.PageSize, paged.TotalPages);

        return Result<PagedParksResponse>.Success(response);
    }

    private static IQueryable<Park> ApplySortDirection(IQueryable<Park> query, string sortDir, Point point)
    {
        query = sortDir == SortDirections.Asc
            ? query.OrderBy(p => p.Address.Coordinates!.Distance(point))
            : query.OrderByDescending(p => p.Address.Coordinates!.Distance(point));
        return query;
    }

    private static IQueryable<Park> ApplySort(IQueryable<Park> query, string sortDir, string sortBy)
    {
        query = sortBy switch
        {
            SortKeys.Municipality => sortDir == SortDirections.Asc ? query.OrderBy(p => p.Address.Street!.Municipio!.OfficialNa6) : query.OrderByDescending(p => p.Address.Street!.Municipio!.OfficialNa6),
            SortKeys.Province => sortDir == SortDirections.Asc ? query.OrderBy(p => p.Address.Street!.Municipio!.OfficialNa4) : query.OrderByDescending(p => p.Address.Street!.Municipio!.OfficialNa4),
            _ => sortDir == SortDirections.Asc ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
        };
        return query;
    }
}
