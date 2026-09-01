namespace PhotoDescriber.Models
{
    /// <summary>
    /// Everything the Index view needs to render either the empty
    /// upload form, or the form plus the result of a completed analysis.
    /// </summary>
    public class PhotoResultViewModel
    {
        /// <summary>Publicly accessible URL of the photo in Blob Storage.</summary>
        public string? ImageUrl { get; set; }

        public ImageAnalysisResult? Analysis { get; set; }

        /// <summary>
        /// True when the description came from the in-memory cache instead
        /// of a fresh call to the Computer Vision API.
        /// </summary>
        public bool ServedFromCache { get; set; }

        public string? ErrorMessage { get; set; }

        public bool HasResult => Analysis is not null && ErrorMessage is null;
    }
}
