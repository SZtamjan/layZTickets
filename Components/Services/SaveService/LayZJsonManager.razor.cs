using System.Text.Json;

public interface ILayZJsonManager
{
    Task SaveAsync<T>(T data, string fileName);
    Task<T?> LoadAsync<T>(string fileName);
}

public sealed class LayZJsonManager : ILayZJsonManager
{
    private readonly string _dataFolder;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public LayZJsonManager(IHostEnvironment env)
    {
        _dataFolder = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(_dataFolder);
    }

    public async Task SaveAsync<T>(T data, string fileName)
    {
        var path = Path.Combine(_dataFolder, fileName);
        var json = JsonSerializer.Serialize(data, _options);
        await File.WriteAllTextAsync(path, json);
    }

    public async Task<T?> LoadAsync<T>(string fileName)
    {
        var path = Path.Combine(_dataFolder, fileName);

        if (!File.Exists(path))
            return default;

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<T>(json, _options);
    }
}