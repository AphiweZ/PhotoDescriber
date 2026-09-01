using PhotoDescriber.Models;

namespace PhotoDescriber.Services
{
    /// <summary>
    /// Coordinates the full "upload a photo, describe it" workflow:
    /// cache lookup -> blob storage -> AI analysis -> cache write.
    /// The controller depends only on this interface, so it stays a thin
    /// adapter between HTTP and the application's real logic.
    /// </summary>
    public interface IPhotoAnalysisOrchestrator
    {
        Task<PhotoResultViewModel> ProcessUploadAsync(Stream fileContent, string fileName);
    }
}
