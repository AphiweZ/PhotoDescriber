# Photo Describer

An ASP.NET Core MVC (.NET 8) application that lets a user upload a photo,
stores it in Azure Blob Storage, sends it to the Microsoft Cognitive
Services Computer Vision API for analysis, caches the AI response, and
displays a text description of the photo back to the user.

## 1. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- An Azure subscription with:
  - A **Computer Vision** (or multi-service **Cognitive Services**) resource
  - A **Storage account** with Blob Storage enabled

Both can be created for free at [portal.azure.com](https://portal.azure.com)
or via the Azure free tier.

## 2. Configure your Azure credentials

Open `appsettings.json` (or, better, set these via `dotnet user-secrets` /
environment variables so keys never end up in source control) and fill in:

```json
"AzureComputerVision": {
  "Endpoint": "https://YOUR-RESOURCE-NAME.cognitiveservices.azure.com",
  "SubscriptionKey": "YOUR_COMPUTER_VISION_KEY"
},
"AzureBlobStorage": {
  "ConnectionString": "YOUR_STORAGE_ACCOUNT_CONNECTION_STRING",
  "ContainerName": "photos"
}
```

- **Endpoint / SubscriptionKey**: found on your Computer Vision resource's
  "Keys and Endpoint" page in the Azure Portal.
- **ConnectionString**: found on your Storage account's "Access keys" page.
- **ContainerName**: any name you like; the app creates the container
  automatically (with public blob read access, so uploaded photos can be
  displayed in the browser) if it doesn't already exist.

Using `dotnet user-secrets` instead (recommended for local dev):

```bash
dotnet user-secrets init
dotnet user-secrets set "AzureComputerVision:Endpoint" "https://..."
dotnet user-secrets set "AzureComputerVision:SubscriptionKey" "..."
dotnet user-secrets set "AzureBlobStorage:ConnectionString" "..."
```

## 3. Run it

```bash
dotnet restore
dotnet run
```

Then browse to the URL shown in the console (e.g. `https://localhost:7203`).

## 4. How it works

```
User's browser
     |
     |  POST /Home/Upload (multipart/form-data)
     v
HomeController --------------------------------------------+
     |  delegates to                                        |
     v                                                       |
IPhotoAnalysisOrchestrator (PhotoAnalysisOrchestrator)       |
     |-- 1. ImageHasher.ComputeHashAsync()  --> SHA-256 key  |
     |-- 2. IBlobStorageService.UploadAsync()  ------> Azure Blob Storage
     |-- 3. IAnalysisCacheService.TryGet(key)  ------> IMemoryCache
     |        cache hit?  -> return cached ImageAnalysisResult
     |        cache miss? |
     |-- 4. IImageAnalysisService.AnalyzeAsync() -----> Cognitive Services
     |        Computer Vision REST API (/vision/v3.2/analyze)
     |-- 5. IAnalysisCacheService.Set(key, result)
     v
PhotoResultViewModel --> Views/Home/Index.cshtml
```

### OOP design notes

- **Single Responsibility**: each service does exactly one thing -
  `AzureBlobStorageService` only knows about storage, `AzureComputerVisionService`
  only knows about calling the Vision API, `MemoryAnalysisCacheService` only
  knows about caching.
- **Dependency Inversion / Interface Segregation**: the controller and the
  orchestrator depend on interfaces (`IBlobStorageService`,
  `IImageAnalysisService`, `IAnalysisCacheService`), never on the concrete
  Azure classes. Any of them could be swapped (e.g. a `LocalDiskStorageService`
  for testing) without touching the orchestrator or controller.
- **Composition over a "god" controller**: `HomeController` only translates
  HTTP <-> model; all real workflow logic lives in `PhotoAnalysisOrchestrator`,
  which is itself just composed from its three collaborators.
- **DTO vs. domain model separation**: `ComputerVisionDto.cs` mirrors the raw
  JSON from the API; `ImageAnalysisResult` is the clean shape the rest of the
  app (and the Razor view) actually works with. The mapping between them
  happens in exactly one place.

### Caching strategy (Requirement 5)

Photos are fingerprinted with SHA-256 before upload. If the same image bytes
are uploaded again (same demo photo re-tested, a double-submitted form,
etc.), the cached `ImageAnalysisResult` is reused and the Computer Vision API
is **not** called a second time - saving both latency and API cost. The
result page shows a **"FROM CACHE"** badge whenever this happens, so the
behaviour is visible during a demo. Cache entries expire after 1 hour of
inactivity (sliding expiration).

## 5. Project structure

```
PhotoDescriber/
├── Controllers/
│   └── HomeController.cs          # thin controller: HTTP <-> models only
├── Models/
│   ├── ImageCaption.cs
│   ├── ImageAnalysisResult.cs
│   └── PhotoResultViewModel.cs
├── Services/
│   ├── IBlobStorageService.cs / AzureBlobStorageService.cs
│   ├── IImageAnalysisService.cs / AzureComputerVisionService.cs / ComputerVisionDto.cs
│   ├── IAnalysisCacheService.cs / MemoryAnalysisCacheService.cs
│   ├── IPhotoAnalysisOrchestrator.cs / PhotoAnalysisOrchestrator.cs
│   └── ImageHasher.cs
├── Views/Home/Index.cshtml        # upload form + result display
├── wwwroot/css/site.css
├── appsettings.json                # fill in Azure credentials here
└── Program.cs                      # DI registration
```
