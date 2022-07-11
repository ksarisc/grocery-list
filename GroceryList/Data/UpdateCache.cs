using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GroceryList.Data;

public interface IUpdateCache
{
    public const string Format = "yyyy-MM-dd HH:mm:ss";

    public Task<bool> IsUpdatedAsync(string key, string clientTime);
    public Task<bool> IsUpdatedAsync(string key, DateTime clientTime);

    public Task UpdateAsync(string key);
}

public class UpdateCache : IUpdateCache
{
    private readonly ConcurrentDictionary<string, DateTime> cache = new(StringComparer.Ordinal);

    public async Task<bool> IsUpdatedAsync(string key, string clientTime)
    {
        if (!DateTime.TryParseExact(key, IUpdateCache.Format, null, System.Globalization.DateTimeStyles.None, out var time))
        {
            //throw new ArgumentOutOfRangeException(nameof(clientTime));
            return false;
        }
        return await IsUpdatedAsync(key, time);
    }

    public Task<bool> IsUpdatedAsync(string key, DateTime clientTime)
    {
        if (!cache.TryGetValue(key, out var cacheTime)) return Task.FromResult(false);

        return Task.FromResult(cacheTime > clientTime);
    }

    public Task UpdateAsync(string key)
    {
        var time = DateTime.UtcNow;
        cache.AddOrUpdate(key, time, (k, curr) => time > curr ? time : curr);
        return Task.CompletedTask;
    }
}
