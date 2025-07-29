using MLModelApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MLModelService>();
builder.Services.AddSingleton<SentimentService>();

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
