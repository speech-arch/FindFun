using FindFun.Server.Shared.File;
using System.ComponentModel.DataAnnotations;

namespace FindFun.Server.Features.Parks.Create;

public record CreateParkRequest(
    [Required]
    [StringLength(100, MinimumLength = 2)]
    string Name,
    [StringLength(500)]
    string? Description,
    [Required]
    string? Organizer,
    bool IsFree,
    decimal EntranceFee,
    string? AgeRecommendation,
    [Required]
    string Amenities,
    [Required]
    string ParkType,
    [Required]
    IFormFileCollection ParkImages,

    string? ClosingSchedule,
     [Required]
     string Coordinates,

    [StringLength(500, MinimumLength = 2)]
     string? FormattedAddress,

    [StringLength(100, MinimumLength = 2)]
     string? Street,

    [StringLength(20, MinimumLength = 1)]
     string? Number,

    [StringLength(100, MinimumLength = 2)]
    string? Locality,

    [StringLength(5, MinimumLength = 5)]
     string? PostalCode
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        return FileValidation.ValidateFiles(ParkImages);
    }
}
