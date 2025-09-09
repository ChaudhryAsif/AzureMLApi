using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using MLModelApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MLModelService>();
builder.Services.AddSingleton<SentimentService>();
builder.Services.AddSingleton<VisionService>();

// Load configuration
var config = builder.Configuration;

// ✅ Register configuration-based services first
builder.Services.AddSingleton(sp =>
{
    var key = config["AzureVision:SubscriptionKey"];
    var endpoint = config["AzureVision:Endpoint"];

    return new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
    {
        Endpoint = endpoint
    };
});

// ✅ Register model classifier with IConfiguration injection (via constructor)
builder.Services.AddSingleton<BoldClassifier>();

// ✅ Now register OcrService (depends on ComputerVisionClient)
builder.Services.AddSingleton<OcrService>();

var app = builder.Build();

// Add static files middleware
//app.UseStaticFiles(); // 👈 Required for Swagger UI

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Hello from the API!");

app.Run();
