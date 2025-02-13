using System.Text.Json;
using StackExchange.Redis;

namespace VehicleService.Data;

public class RedisCacheService(IConnectionMultiplexer redis)
{
    private readonly IDatabase _cache = redis.GetDatabase();

    public async Task SetCacheAsync<T>(string key, T data, TimeSpan expiration)
    {
        var jsonData = JsonSerializer.Serialize(data);
        await _cache.StringSetAsync(key, jsonData, expiration);
    }

    public async Task<T> GetCacheAsync<T>(string key)
    {
        var jsonData = await _cache.StringGetAsync(key);
        return jsonData.HasValue ? JsonSerializer.Deserialize<T>(jsonData) : default;
    }

    public async Task RemoveCacheAsync(string key)
    {
        await _cache.KeyDeleteAsync(key);
    }
}