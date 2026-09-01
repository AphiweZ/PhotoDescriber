using PhotoDescriber.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC (Requirement 1 & 6: Razor views for the home page and result display)
builder.Services.AddControllersWithViews();

// In-memory cache backing IAnalysisCacheService (Requirement 5: caching).
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 200; // cap the number of cached analyses kept in memory
});

// Blob storage: one implementation registered against its interface,
// so everything else in the app depends only on IBlobStorageService.
builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();

// Computer Vision: registered as a typed HttpClient so the framework
// manages the HttpClient lifetime/connection pooling for us.
builder.Services.AddHttpClient<IImageAnalysisService, AzureComputerVisionService>();

builder.Services.AddSingleton<IAnalysisCacheService, MemoryAnalysisCacheService>();

// The orchestrator composes the three services above.
builder.Services.AddScoped<IPhotoAnalysisOrchestrator, PhotoAnalysisOrchestrator>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
