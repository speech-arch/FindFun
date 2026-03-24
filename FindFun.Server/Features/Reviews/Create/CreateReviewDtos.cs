using System.ComponentModel.DataAnnotations;

namespace FindFun.Server.Features.Reviews.Create;

public sealed record CreateReviewRequest(
    [Required]
    string UserName,
    [Required]
    string Content,
    [Range(1,5)]
    int Rating,
    [Required]
    string Id
);
