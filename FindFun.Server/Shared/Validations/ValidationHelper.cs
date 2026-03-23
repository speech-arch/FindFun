using FindFun.Server.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using FindFun.Server.Shared.Resources;

namespace FindFun.Server.Shared.Validations;

public static class ValidationHelper
{
    public static Result<CoordinateDto> ParseCoordinate(string? coordinates)
    {
        var parts = coordinates?.Split(',');
        if (parts?.Length == 2
            && double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude)
            && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude))
        {
            return Result<CoordinateDto>.Success(new CoordinateDto(longitude, latitude));
        }

        return Result<CoordinateDto>.Failure(new ValidationProblemDetails
        {
            Errors = new Dictionary<string, string[]> { { Messages.FieldCoordinates, [Messages.InvalidCoordinateFormat] } }
        });
    }

    public static List<ClosingScheduleEntry> ParseClosingSchedule(string? closingSchedule)
    {
        // this should be improved later to consider errprs 
        var result = new List<ClosingScheduleEntry>();
        if (!string.IsNullOrWhiteSpace(closingSchedule))
        {
            var schedules = closingSchedule
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var schedule in schedules)
            {
                var dayAndTimes = schedule.Split(':', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (dayAndTimes.Length < 2)
                    continue;

                var times = dayAndTimes[1].Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                result.Add(new ClosingScheduleEntry(dayAndTimes[0], times[0], times.Length < 2 ? string.Empty : times[1], false));
            }
        }
        return result;
    }

    public static Result<(string, string?)> ParseAmenityGroup(string amenities)
    {
        // here the code should be imporoved to ensure the error case works as expected and return validationResult so the this can be called in dto validation 
        if (string.IsNullOrWhiteSpace(amenities))
        {
            return Result<(string, string?)>.Failure(new ValidationProblemDetails
            {
                Errors = new Dictionary<string, string[]> { { Messages.FieldAmenities, [Messages.AmenitiesCannotBeEmpty] } }
            });
        }

        var parts = amenities.Split(':', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var note = parts.Length > 1 ? parts[1] : null;
        return Result<(string, string?)>.Success((parts[0], note));
    }
    public static ValidationResult EntranceValidation(bool isFree, decimal entranceFee)
    {
        if (isFree && entranceFee != 0m)
        {
            return new ValidationResult(Messages.EntranceFeeMustBeZeroWhenFree, [Messages.FieldEntranceFee]);
        }
        if (!isFree && entranceFee <= 0m)
        {
            return new ValidationResult(Messages.EntranceFeeGreaterThanZero, [Messages.FieldEntranceFee]);
        }
        return ValidationResult.Success!;
    }
}

public record CoordinateDto([Required] double Longitude, [Required] double Latitude);
