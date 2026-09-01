using System.Net.Http.Headers;
using System.Text.Json;
using PhotoDescriber.Models;

namespace PhotoDescriber.Services
{
    /// <summary>
    /// Calls the Microsoft Cognitive Services "Computer Vision" REST API
    /// directly (as the assignment brief describes: "standard REST calls
    /// over the Internet to the Cognitive Services APIs") to obtain a
    /// natural-language description of a photo.
    /// (Requirement 4.1: process the image using the API, 4.2: return a text description).
    /// </summary>
    public class AzureComputerVisionService : IImageAnalysisService
    {
        private const string AnalyzePath = "/vision/v3.2/analyze?visualFeatures=Description,Tags&language=en";

        private readonly HttpClient _httpClient;

        // HttpClient is injected as a typed client (see Program.cs) so its
        // lifetime and BaseAddress/default headers are managed centrally by
        // the DI container, rather than "new HttpClient()"-ed here.
        public AzureComputerVisionService(HttpClient httpClient, IConfiguration configuration)
        {
            var endpoint = configuration["AzureComputerVision:Endpoint"]
                ?? throw new InvalidOperationException("Missing configuration: AzureComputerVision:Endpoint");

            var subscriptionKey = configuration["AzureComputerVision:SubscriptionKey"]
                ?? throw new InvalidOperationException("Missing configuration: AzureComputerVision:SubscriptionKey");

            httpClient.BaseAddress = new Uri(endpoint.TrimEnd('/'));
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            _httpClient = httpClient;
        }

        public async Task<ImageAnalysisResult> AnalyzeAsync(Stream imageContent)
        {
            imageContent.Position = 0;

            using var streamContent = new StreamContent(imageContent);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            using var response = await _httpClient.PostAsync(AnalyzePath, streamContent);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Computer Vision API call failed ({(int)response.StatusCode} {response.StatusCode}): {body}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<ComputerVisionResponseDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return MapToResult(dto);
        }

        private static ImageAnalysisResult MapToResult(ComputerVisionResponseDto? dto)
        {
            var captions = (dto?.Description?.Captions ?? new List<CaptionDto>())
                .Select(c => new ImageCaption
                {
                    Text = c.Text,
                    Confidence = c.Confidence
                })
                .OrderByDescending(c => c.Confidence)
                .ToList();

            var best = captions.FirstOrDefault();

            return new ImageAnalysisResult
            {
                PrimaryDescription = best?.Text ?? "No description could be generated for this image.",
                PrimaryConfidencePercentage = best?.ConfidencePercentage ?? 0,
                AllCaptions = captions,
                Tags = (dto?.Tags ?? new List<TagDto>())
                    .OrderByDescending(t => t.Confidence)
                    .Select(t => t.Name)
                    .ToList()
            };
        }
    }
}
