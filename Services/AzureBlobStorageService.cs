using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace PhotoDescriber.Services
{
    /// <summary>
    /// Stores uploaded photos in an Azure Storage blob container
    /// (Requirement 3: Store the Image in Blob Storage).
    /// </summary>
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlobStorage:ConnectionString"]
                ?? throw new InvalidOperationException("Missing configuration: AzureBlobStorage:ConnectionString");

            var containerName = configuration["AzureBlobStorage:ContainerName"]
                ?? throw new InvalidOperationException("Missing configuration: AzureBlobStorage:ContainerName");

            _containerClient = new BlobContainerClient(connectionString, containerName);

            // Make sure the container exists and photos are publicly readable
            // so the browser can display them directly from Blob Storage.
            _containerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task<string> UploadAsync(Stream content, string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var blobName = $"{Guid.NewGuid()}{extension}";

            var blobClient = _containerClient.GetBlobClient(blobName);

            var headers = new BlobHttpHeaders
            {
                ContentType = GetContentType(extension)
            };

            content.Position = 0;
            await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = headers });

            return blobClient.Uri.ToString();
        }

        private static string GetContentType(string extension) => extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }
}
