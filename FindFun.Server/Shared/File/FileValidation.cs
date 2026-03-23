using System.ComponentModel.DataAnnotations;
using FindFun.Server.Shared.File.Resources;
using Microsoft.AspNetCore.Http;

namespace FindFun.Server.Shared.File;

public class FileValidation
{
    public static IEnumerable<ValidationResult> ValidateFiles(IFormFileCollection files)
    {
        if (files is null || files.Count == 0)
            yield return new ValidationResult(string.Format(Messages.NoFilesProvided, nameof(files)), [nameof(files)]);

        foreach (var file in files!)
        {
            var result = ValidateFile(file);
            if (result.Any())
                yield return result.First();
        }
    }
    public static IEnumerable<ValidationResult> ValidateFile(IFormFile file)
    {
        var fileSize = 10 << 20;// 10 MB 
        if (file.Length > fileSize || file.Length == 0)
            yield return new ValidationResult(string.Format(Messages.FileSizeInvalid, nameof(file)), [nameof(file)]);

        var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var fileExtensions = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!permittedExtensions.Contains(fileExtensions))
            yield return new ValidationResult(string.Format(Messages.InvalidFileExtension, nameof(file)), [nameof(file)]);
    }
    public static async Task DeleteUploadedFilesAsync(List<Result<string>> fileResult, FileUpLoad fileUpLoad, CancellationToken cancellationToken)
    {
        await Task.WhenAll(fileResult.Where(r => r.IsValid && r.Data is not null)
            .Select(r => fileUpLoad.DeleteFileAsync(GetRelativePathFromUrl(r.Data!), cancellationToken)));
    }
    public static string GetRelativePathFromUrl(string url)
    {
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);
        return uri.IsAbsoluteUri ? uri.AbsolutePath.TrimStart('/') : url;
    }
}
