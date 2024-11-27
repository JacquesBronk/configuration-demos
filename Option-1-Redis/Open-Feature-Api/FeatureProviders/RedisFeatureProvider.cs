using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;
using OpenFeature;
using OpenFeature.Constant;
using OpenFeature.Model;
using StackExchange.Redis;

namespace Open_Feature_Api.FeatureProviders;

public class RedisFeatureProvider : FeatureProvider
{
    public override Metadata GetMetadata()
    {
        return new("redis-provider");
    }

    private readonly IDatabase _database;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RedisFeatureProvider> _logger;

    public RedisFeatureProvider(IConnectionMultiplexer redis, IMemoryCache cache, ILogger<RedisFeatureProvider> logger)
    {
        Console.WriteLine("RedisFeatureProvider constructor called.");
        var redis1 = redis;
        _database = redis1.GetDatabase();
        var subscriber = redis1.GetSubscriber();
        _cache = cache;
        _logger = logger;

        try
        {
            _logger.LogInformation("Subscribing to feature_flag_updates channel...");

            subscriber.Subscribe(new RedisChannel("feature_flag_updates", RedisChannel.PatternMode.Literal), OnFlagUpdated);

            _logger.LogInformation("Subscription established.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to feature_flag_updates channel.");
        }
    }

    private void OnFlagUpdated(RedisChannel channel, RedisValue message)
    {
        if (!message.HasValue) return;
        
        var flagKey = (string)message;
        _logger.LogInformation($"Flag updated: {flagKey}");
        if (flagKey != null) _cache.Remove(flagKey);
    }

    public override async Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = new CancellationToken())
    {
        return await ResolveValueAsync(flagKey, defaultValue, bool.TryParse);
    }

    public override async Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = new CancellationToken())
    {
        return await ResolveValueAsync(flagKey, defaultValue, (string s, out string result) =>
        {
            result = s;
            return true;
        });
    }

    public override async Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = new CancellationToken())
    {
        return await ResolveValueAsync(flagKey, defaultValue, int.TryParse);
    }

    public override async Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = new CancellationToken())
    {
        return await ResolveValueAsync(flagKey, defaultValue, double.TryParse);
    }

    public override async Task<ResolutionDetails<Value>> ResolveStructureValueAsync(
        string flagKey, Value defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache.TryGetValue(flagKey, out Value cachedValue))
            {
                return new ResolutionDetails<Value>(flagKey, cachedValue);
            }

            var flagValue = await _database.StringGetAsync(flagKey).ConfigureAwait(false);

            _logger.LogInformation($"Flag value for {flagKey}: {flagValue}");
            
            if (flagValue.IsNullOrEmpty)
            {
                return new ResolutionDetails<Value>(
                    flagKey,
                    defaultValue,
                    ErrorType.FlagNotFound,
                    "Flag not found"
                );
            }

            try
            {
                var jsonNode = JsonNode.Parse(flagValue);

                if (jsonNode == null)
                {
                    return new ResolutionDetails<Value>(
                        flagKey,
                        defaultValue,
                        ErrorType.TypeMismatch,
                        "Flag value is null"
                    );
                }

                var value = ConvertJsonNodeToValue(jsonNode);

                // Cache the value
                _cache.Set(flagKey, value);

                return new ResolutionDetails<Value>(flagKey, value);
            }
            catch (Exception ex)
            {
                return new ResolutionDetails<Value>(
                    flagKey,
                    defaultValue,
                    ErrorType.TypeMismatch,
                    $"Error deserializing flag value: {ex.Message}"
                );
            }
        }
        catch (Exception ex)
        {
            return new ResolutionDetails<Value>(
                flagKey,
                defaultValue,
                ErrorType.General,
                ex.Message
            );
        }
    }

    private async Task<ResolutionDetails<T>> ResolveValueAsync<T>(string flagKey, T defaultValue, TryParse<T> tryParse)
    {
        try
        {
            if (_cache.TryGetValue(flagKey, out T cachedValue))
            {
                return new ResolutionDetails<T>(flagKey, cachedValue);
            }

            var flagValue = await _database.StringGetAsync(flagKey).ConfigureAwait(false);

            _logger.LogInformation($"Flag value for {flagKey}: {flagValue}");
            
            if (flagValue.IsNullOrEmpty)
            {
                return new ResolutionDetails<T>(
                    flagKey,
                    defaultValue,
                    ErrorType.FlagNotFound,
                    "Flag not found"
                );
            }

            if (tryParse(flagValue, out T result))
            {
                // Cache the result
                _cache.Set(flagKey, result);

                return new ResolutionDetails<T>(flagKey, result);
            }
            else
            {
                return new ResolutionDetails<T>(
                    flagKey,
                    defaultValue,
                    ErrorType.TypeMismatch,
                    "Invalid flag value"
                );
            }
        }
        catch (Exception ex)
        {
            return new ResolutionDetails<T>(
                flagKey,
                defaultValue,
                ErrorType.General,
                ex.Message
            );
        }
    }

    private Value ConvertJsonNodeToValue(JsonNode jsonNode)
    {
        switch (jsonNode)
        {
            case JsonValue jsonValue:
                var value = jsonValue.GetValue<object>();

                return value switch
                {
                    bool b => new Value(b),
                    int i => new Value(i),
                    long l => new Value(l),
                    double d => new Value(d),
                    float f => new Value(f),
                    string s => new Value(s),
                    _ => new Value(value.ToString())
                };

            case JsonArray jsonArray:
                var list = new List<Value>();
                foreach (var item in jsonArray)
                {
                    list.Add(ConvertJsonNodeToValue(item));
                }

                return new Value(list);

            case JsonObject jsonObject:
                var dictionary = new Dictionary<string, Value>();
                foreach (var kvp in jsonObject)
                {
                    dictionary[kvp.Key] = ConvertJsonNodeToValue(kvp.Value);
                }

                return new Value(dictionary);

            default:
                return new Value(jsonNode.ToString());
        }
    }

    private delegate bool TryParse<T>(string s, out T result);
}