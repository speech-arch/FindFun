using System.ComponentModel.DataAnnotations;

namespace FindFun.Server.Infrastructure.Options;

public class ConnectionStrings
{
    [Required]
    [MinLength(1)]
    public string FindFun { get; init; } = default!;
    [Required]
    [MinLength(1)]
    public string Blobs { get; init; } = default!;
    [Required]
    [MinLength(1)]
    public string ContainerName { get; set; } = default!;
}