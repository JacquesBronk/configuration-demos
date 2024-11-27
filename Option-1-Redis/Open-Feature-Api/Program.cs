using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Open_Feature_Api.FeatureProviders;
using OpenFeature;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSingleton<IConnectionMultiplexer>
    (ConnectionMultiplexer.Connect("redis:6379,defaultDatabase=1,abortConnect=false,connectTimeout=1000,asyncTimeout=5000,syncTimeout=5000,connectRetry=3,keepAlive=30"));

// Add memory cache for feature flags
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<FeatureProvider>(sp =>
{
    var redis = sp.GetRequiredService<IConnectionMultiplexer>();
    var cache = sp.GetRequiredService<IMemoryCache>();
    var logger = sp.GetRequiredService<ILogger<RedisFeatureProvider>>();
    return new RedisFeatureProvider(redis, cache, logger);
});


builder.Services.AddSingleton<FeatureClient>(sp =>
{
    var provider = sp.GetRequiredService<FeatureProvider>();
    Api.Instance.SetProviderAsync(provider);
    return Api.Instance.GetClient();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/feature-flags", async (IConnectionMultiplexer redis) =>
    {
        var db = redis.GetDatabase();
        var endpoints = redis.GetEndPoints();
        var server = redis.GetServer(endpoints.First());

        // Retrieve all keys in the default database
        var keys = server.Keys(pattern: "*").ToArray();

        var flags = new List<object>();

        foreach (var key in keys)
        {
            var value = await db.StringGetAsync(key);
            flags.Add(new { Key = (string)key, Value = (string)value });
        }

        return Results.Ok(flags);
    })
    .WithName("GetAllFeatureFlags")
    .WithTags("Feature Flags");

app.MapPost("/feature-flags/{key}", async ([FromRoute] string key, [FromBody] string value, IConnectionMultiplexer redis) =>
    {
        var db = redis.GetDatabase();
        await db.StringSetAsync(key, value);

        var sub = redis.GetSubscriber();
        await sub.PublishAsync("feature_flag_updates", key);

        return Results.Ok(new { Key = key, Value = value });
    })
    .WithName("UpdateFeatureFlag")
    .WithTags("Feature Flags");

app.MapGet("/use-feature", async (FeatureClient client) =>
    {
        bool isEnabled = await client.GetBooleanValueAsync("feature-a", false);
        Console.WriteLine($"Feature 'feature-a' is enabled: {isEnabled}");
        if (isEnabled)
        {
            return Results.Ok("hoorah");
        }
        else
        {
            return Results.StatusCode(423);
        }
    })
    .WithName("UseFeature")
    .WithTags("Feature Usage");

app.Run();