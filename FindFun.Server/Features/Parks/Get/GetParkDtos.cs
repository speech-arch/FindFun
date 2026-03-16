using System.Collections.Generic;

namespace FindFun.Server.Features.Parks.Get;

public sealed record GetParkRequest(int ParkId);

public sealed record GetParkReviewResponse(
    string Id,
    string UserName,
    string Content,
    int Rating,
    string CreatedAt
);

public sealed record GetParkResponse(
    string Id,
    string Name,
    string Description,
    string LocationId,
    string LocationName,
    double Latitude,
    double Longitude,
    string MunicipalityName,
    string ProvinceName,
    string AutonomousCommunityName,
    IReadOnlyList<GetParkReviewResponse> Reviews,
    IReadOnlyList<string> Amenities,
    string ParkType,
    IReadOnlyList<string> ParkImages,
    string Street,
    double AverageRating
);