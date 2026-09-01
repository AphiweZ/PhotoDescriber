namespace PhotoDescriber.Models
{
    /// <summary>
    /// Represents one caption ("description") suggestion returned by the
    /// Computer Vision API, together with the model's confidence in it.
    /// </summary>
    public class ImageCaption
    {
        public string Text { get; set; } = string.Empty;

        public double Confidence { get; set; }

        /// <summary>
        /// Confidence expressed as a whole-number percentage, e.g. 87 for 0.87.
        /// Kept here (rather than in the view) so the formatting rule lives
        /// in one place only.
        /// </summary>
        public int ConfidencePercentage => (int)Math.Round(Confidence * 100);
    }
}
