using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FindFun.Server.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace FindFun.Server.Shared.File;

public class FileUpLoad(BlobServiceClient blobServiceClient, IOptions<ConnectionStrings> connectionStrings)
{
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
    private readonly IOptions<ConnectionStrings> _connectionStrings = connectionStrings;

    public async Task<Result<string>> FileUpLoader(IFormFile files, string folderName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var containerName = _connectionStrings.Value?.ContainerName;
        if (string.IsNullOrWhiteSpace(containerName))
            throw new InvalidOperationException("Configuration key 'ConnectionStrings:Blobs' not found.");

        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(publicAccessType: PublicAccessType.Blob, cancellationToken: cancellationToken);

        var fileExtension = Path.GetExtension(files.FileName);
        var safeFileName = $"{Guid.NewGuid()}{fileExtension}".ToLowerInvariant();

        var blobName = $"{folderName.Trim('/')}/{safeFileName}";
        var blobClient = container.GetBlobClient(blobName);

        await using var stream = files.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = files.ContentType }, cancellationToken: cancellationToken);

        return Result<string>.Success(blobClient.Uri.ToString());
    }

    public async Task<Result<bool>> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var containerName = _connectionStrings.Value?.ContainerName;
        if (string.IsNullOrWhiteSpace(containerName))
            throw new InvalidOperationException("Configuration key 'ConnectionStrings:Blobs' not found.");

        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobName = relativePath.TrimStart('/');
        var idx = blobName.IndexOf(containerName + '/');
        if (idx >= 0)
        {
            blobName = blobName[(idx + containerName.Length + 1)..];
        }
        var blobClient = container.GetBlobClient(blobName);

        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        return response.Value ? Result<bool>.Success(true) : Result<bool>.Failure(false);
    }

    public async Task<List<Result<string>>> FilesUpLoader(IFormFileCollection files, string folderName, CancellationToken cancellationToken)
    {
        var result = files.Select(file => FileUpLoader(file, folderName, cancellationToken));
        return [.. await Task.WhenAll(result)];
    }
}
