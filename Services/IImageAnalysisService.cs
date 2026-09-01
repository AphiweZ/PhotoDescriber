using PhotoDescriber.Models;

namespace PhotoDescriber.Services
{
    /// <summary>
    /// Abstraction over "whichever AI service can describe an image".
    /// The rest of the application only ever talks to this interface,
    /// so the concrete Cognitive Services implementation could later be
    /// swapped for a different provider or a mock in tests.
    /// </summary>
    public interface IImageAnalysisService
    {
        /// <summary>
        /// Sends the image bytes to the AI service and returns a
        /// structured description of what is in the photo.
        /// </summary>
        Task<ImageAnalysisResult> AnalyzeAsync(Stream imageContent);
    }
}
