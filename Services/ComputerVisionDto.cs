using System.Text.Json.Serialization;

namespace PhotoDescriber.Services
{
    // These classes exist only to mirror the JSON shape returned by
    // https://{endpoint}/vision/v3.2/analyze?visualFeatures=Description,Tags
    // They are intentionally kept separate from Models/ImageAnalysisResult,
    // which is the clean, presentation-friendly shape the rest of the app
    // uses - the mapping between the two happens once, inside
    // AzureComputerVisionService.

    internal class ComputerVisionResponseDto
    {
        [JsonPropertyName("description")]
        public DescriptionDto? Description { get; set; }

        [JsonPropertyName("tags")]
        public List<TagDto>? Tags { get; set; }
    }

    internal class DescriptionDto
    {
        [JsonPropertyName("captions")]
        public List<CaptionDto>? Captions { get; set; }
    }

    internal class CaptionDto
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }

    internal class TagDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}
