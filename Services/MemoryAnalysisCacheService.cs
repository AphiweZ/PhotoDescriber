using Microsoft.Extensions.Caching.Memory;
using PhotoDescriber.Models;

namespace PhotoDescriber.Services
{
    /// <summary>
    /// Caches AI analysis results in server memory
    /// (Requirement 5: Caching Responses + Use Cache Appropriately).
    ///
    /// Why cache here, and why key on content hash?
    /// The Computer Vision call is the slowest and only billed part of the
    /// pipeline. Two uploads of the *same* photo (a common case: a user
    /// double-clicks "Upload", or re-tests the same sample image while
    /// demonstrating the app) should not trigger a second paid API call or
    /// a second round-trip. Hashing the file bytes means the cache hits
    /// regardless of the file name, so identical images are always
    /// recognised as identical.
    /// </summary>
    public class MemoryAnalysisCacheService : IAnalysisCacheService
    {
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public MemoryAnalysisCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool TryGet(string key, out ImageAnalysisResult? result)
        {
            return _cache.TryGetValue(key, out result);
        }

        public void Set(string key, ImageAnalysisResult result)
        {
            _cache.Set(key, result, new MemoryCacheEntryOptions
            {
                SlidingExpiration = CacheDuration,
                Size = 1
            });
        }
    }
}
