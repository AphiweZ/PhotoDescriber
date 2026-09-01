using Microsoft.AspNetCore.Mvc;
using PhotoDescriber.Models;
using PhotoDescriber.Services;

namespace PhotoDescriber.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" };
        private const long MaxFileSizeBytes = 4 * 1024 * 1024; // 4 MB - Computer Vision's own limit is 4MB for this API version

        private readonly IPhotoAnalysisOrchestrator _orchestrator;

        public HomeController(IPhotoAnalysisOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        // Requirement 1: Home page with browse window to select an image.
        [HttpGet]
        public IActionResult Index()
        {
            return View(new PhotoResultViewModel());
        }

        // Requirement 2: Upload Image (and trigger the rest of the pipeline).
        [HttpPost]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<IActionResult> Upload(IFormFile photo)
        {
            var model = new PhotoResultViewModel();

            if (photo is null || photo.Length == 0)
            {
                model.ErrorMessage = "Please choose a photo before clicking Upload & Describe.";
                return View("Index", model);
            }

            var extension = Path.GetExtension(photo.FileName);
            if (!AllowedExtensions.Contains(extension.ToLowerInvariant()))
            {
                model.ErrorMessage = $"Unsupported file type '{extension}'. Please upload a JPG, PNG, GIF, BMP or WEBP image.";
                return View("Index", model);
            }

            if (photo.Length > MaxFileSizeBytes)
            {
                model.ErrorMessage = "That image is too large. Please choose a photo under 4 MB.";
                return View("Index", model);
            }

            await using var stream = photo.OpenReadStream();
            model = await _orchestrator.ProcessUploadAsync(stream, photo.FileName);

            return View("Index", model);
        }
    }
}
