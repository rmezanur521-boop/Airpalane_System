using AirplaneSystem.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AirplaneSystem.Infrastructure.ExternalServices.Cache;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache) => _cache = cache;

    public T? Get<T>(string key) => _cache.TryGetValue(key, out T? value) ? value : default;

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue) options.AbsoluteExpirationRelativeToNow = expiration;
        else options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        _cache.Set(key, value, options);
    }

    public void Remove(string key) => _cache.Remove(key);

    public bool TryGet<T>(string key, out T? value) => _cache.TryGetValue(key, out value);
}
