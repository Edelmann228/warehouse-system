using StackExchange.Redis;
using System.Text.Json;

namespace WarehouseApi.Services;

public class CacheService
{
    private readonly IDatabase _db;

    // IConnectionMultiplexer приходит через DI (настраивается в Program.cs)
    public CacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    // Получить значение из кэша. Вернёт null, если ключ отсутствует или устарел.
    public async Task<T?> GetAsync<T>(string key)
    {
        var val = await _db.StringGetAsync(key);
        if (val.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(val!);
    }

    // Сохранить значение в кэш с заданным временем жизни (TTL)
    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, ttl);
    }

    // Удалить ключ (инвалидация кэша после изменения данных)
    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}
