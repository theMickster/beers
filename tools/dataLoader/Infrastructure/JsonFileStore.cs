using Newtonsoft.Json;

namespace BeersDataLoader.Infrastructure;

internal static class JsonFileStore
{
    internal static async Task<List<T>> ReadListAsync<T>(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return JsonConvert.DeserializeObject<List<T>>(json) ?? [];
    }

    internal static async Task WriteIndentedAsync<T>(string filePath, T payload)
    {
        var json = JsonConvert.SerializeObject(payload, Formatting.Indented);
        await File.WriteAllTextAsync(filePath, json);
    }
}
