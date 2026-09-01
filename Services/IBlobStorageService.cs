namespace PhotoDescriber.Services
{
    /// <summary>
    /// Abstraction over "wherever we durably store the uploaded photo".
    /// Depending on this interface (rather than the concrete Azure class)
    /// throughout the rest of the app means the storage provider could be
    /// swapped out (e.g. for local disk in a unit test, or AWS S3) without
    /// touching any calling code - Dependency Inversion Principle.
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads the given stream to blob storage and returns the
        /// publicly accessible URL of the stored blob.
        /// </summary>
        /// <param name="content">The file content to store.</param>
        /// <param name="originalFileName">Original file name, used to derive a unique blob name and content type.</param>
        Task<string> UploadAsync(Stream content, string originalFileName);
    }
}
