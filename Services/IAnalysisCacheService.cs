using PhotoDescriber.Models;

namespace PhotoDescriber.Services
{
    /// <summary>
    /// Abstraction over caching of AI analysis results, keyed by a hash of
    /// the image content. Kept separate from IImageAnalysisService so the
    /// "call the AI" concern and the "remember what the AI said" concern
    /// can vary independently (Single Responsibility Principle) - e.g. the
    /// cache could later be backed by Redis/distributed cache instead of
    /// in-memory without the analysis service ever knowing.
    /// </summary>
    public interface IAnalysisCacheService
    {
        bool TryGet(string key, out ImageAnalysisResult? result);

        void Set(string key, ImageAnalysisResult result);
    }
}
