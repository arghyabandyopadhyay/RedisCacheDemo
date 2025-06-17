using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register Redis
var redisHost = builder.Configuration["REDIS_HOST"] ?? "localhost";
var redisPort = builder.Configuration["REDIS_PORT"] ?? "6379";
var redisConnectionString = $"{redisHost}:{redisPort}";
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var cacheKey = "weatherforecast";
    var slidingExpiration = TimeSpan.FromMinutes(5);
    var cached = await db.StringGetAsync(cacheKey);

    if (!cached.IsNullOrEmpty)
    {
        // Reset expiration on cache hit (sliding expiration)
        await db.KeyExpireAsync(cacheKey, slidingExpiration);

        return Results.Ok(System.Text.Json.JsonSerializer.Deserialize<WeatherForecast[]>(cached!));
    }

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    await db.StringSetAsync(
        cacheKey,
        System.Text.Json.JsonSerializer.Serialize(forecast),
        slidingExpiration
    );

    return Results.Ok(forecast);
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
