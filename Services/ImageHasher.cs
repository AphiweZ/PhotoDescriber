using System.Security.Cryptography;

namespace PhotoDescriber.Services
{
    /// <summary>
    /// Produces a stable, content-based identifier for an image, used as
    /// the cache key. Deliberately a small static helper rather than a
    /// registered service - it has no state and no dependencies, so there
    /// is nothing DI needs to manage.
    /// </summary>
    public static class ImageHasher
    {
        public static async Task<string> ComputeHashAsync(Stream content)
        {
            content.Position = 0;
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(content);
            content.Position = 0;
            return Convert.ToHexString(hashBytes);
        }
    }
}
