using GaiaApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<GaiaApiSettings>(builder.Configuration.GetSection("GaiaApi"));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("GaiaHackPolicy", policy =>
    {
        policy.WithOrigins(
                "https://gaiahack.ing",           // Your main domain
                "http://gaiahack.ing",            // HTTP version (if needed)
                "https://www.gaiahack.ing",       // WWW version (if used)
                "http://localhost:3000",          // For local development
                "http://localhost:8080"           // For local development
            )
            .AllowAnyMethod()                     // Allow GET, POST, PUT, DELETE, etc.
            .AllowAnyHeader()                     // Allow any headers
            .WithExposedHeaders("Content-Disposition") // Expose specific headers if needed
            .AllowCredentials();                  // Allow cookies/credentials if needed
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// IMPORTANT: Use CORS before routing and authorization
app.UseCors("GaiaHackPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
