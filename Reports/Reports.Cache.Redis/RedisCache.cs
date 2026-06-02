using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Reports.Abstractions.Storage;

namespace Reports.Cache.Redis;

/// <inheritdoc cref="ICacheStorage"/>
public class RedisCache : ICacheStorage
{
    private readonly IDistributedCache _cache;
    private readonly List<string> _keys;

    public RedisCache(IDistributedCache cache)
    {
        _cache = cache;
        _keys = [];
    }
    
    /// <inheritdoc cref="ICacheStorage.TryGetAsync{T}"/>
    public async Task<T?> TryGetAsync<T>(string key)
    {
        var value = await _cache.GetStringAsync(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }

    /// <inheritdoc cref="ICacheStorage.SetAsync{T}"/>
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
        
        _keys.Add(key);
    }

    /// <inheritdoc cref="ICacheStorage.RemoveAsync"/>
    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
        
        _keys.Remove(key);
    }

    /// <inheritdoc cref="ICacheStorage.RemoveByPrefixAsync"/>
    public async Task RemoveByPrefixAsync(string prefix)
    {
        foreach (var key in _keys.Where(k => k.StartsWith(prefix)))
        {
            await _cache.RemoveAsync(key);
        }
    }
}