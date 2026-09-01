using PhotoDescriber.Models;

namespace PhotoDescriber.Services
{
    /// <summary>
    /// Concrete workflow implementation. Every collaborator is injected as
    /// an interface (Dependency Inversion Principle), so this class
    /// contains only orchestration logic - it doesn't know or care whether
    /// storage is Azure, AWS or local disk, or whether the AI is
    /// Cognitive Services or something else.
    /// </summary>
    public class PhotoAnalysisOrchestrator : IPhotoAnalysisOrchestrator
    {
        private readonly IBlobStorageService _storageService;
        private readonly IImageAnalysisService _analysisService;
        private readonly IAnalysisCacheService _cacheService;
        private readonly ILogger<PhotoAnalysisOrchestrator> _logger;

        public PhotoAnalysisOrchestrator(
            IBlobStorageService storageService,
            IImageAnalysisService analysisService,
            IAnalysisCacheService cacheService,
            ILogger<PhotoAnalysisOrchestrator> logger)
        {
            _storageService = storageService;
            _analysisService = analysisService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<PhotoResultViewModel> ProcessUploadAsync(Stream fileContent, string fileName)
        {
            var model = new PhotoResultViewModel();

            try
            {
                // 1. Fingerprint the image so identical uploads share a cache entry.
                var contentHash = await ImageHasher.ComputeHashAsync(fileContent);

                // 2. Store the image in Blob Storage regardless of cache status,
                //    so every upload the user makes is durably kept and can be displayed.
                var imageUrl = await _storageService.UploadAsync(fileContent, fileName);
                model.ImageUrl = imageUrl;

                // 3. Check the cache before paying for another AI call.
                if (_cacheService.TryGet(contentHash, out var cachedResult) && cachedResult is not null)
                {
                    _logger.LogInformation("Cache hit for image hash {Hash}", contentHash);
                    model.Analysis = cachedResult;
                    model.ServedFromCache = true;
                    return model;
                }

                _logger.LogInformation("Cache miss for image hash {Hash} - calling Computer Vision", contentHash);

                // 4. Call the AI processing module.
                var analysis = await _analysisService.AnalyzeAsync(fileContent);

                // 5. Remember the result for next time.
                _cacheService.Set(contentHash, analysis);

                model.Analysis = analysis;
                model.ServedFromCache = false;
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process uploaded photo {FileName}", fileName);
                model.ErrorMessage = $"Sorry, something went wrong analyzing this photo: {ex.Message}";
                return model;
            }
        }
    }
}
