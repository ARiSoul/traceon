using System.Text.Json;

namespace Arisoul.Traceon.Maui.Infrastructure.Storage;

public class FileStorage<T>(string filePath)
{
    private readonly string _filePath = filePath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public async Task<List<T>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return [];

        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<T>>(json, _options) ?? [];
    }

    public async Task SaveAsync(List<T> data)
    {
        var json = JsonSerializer.Serialize(data, _options);
        await File.WriteAllTextAsync(_filePath, json);
    }
}
