namespace PhotoDescriber.Models
{
    /// <summary>
    /// Aggregate result of analysing a single photo: the best caption,
    /// any alternative captions, and the descriptive tags Cognitive
    /// Services detected. This is the object that flows from the
    /// AI service, through the cache, up to the view.
    /// </summary>
    public class ImageAnalysisResult
    {
        /// <summary>
        /// The highest-confidence, human readable description of the photo,
        /// e.g. "a dog lying on a couch".
        /// </summary>
        public string PrimaryDescription { get; set; } = string.Empty;

        public int PrimaryConfidencePercentage { get; set; }

        public List<ImageCaption> AllCaptions { get; set; } = new();

        public List<string> Tags { get; set; } = new();
    }
}
