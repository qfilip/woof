using System.Text.Json;

namespace Woof.Api.DataAccess;

public class JsonFileStore<T>
{
    private IList<T> _data;
    private readonly string _filePath;

    private JsonFileStore(IList<T> data, string filePath)
    {
        _data = data;
        _filePath = filePath;
    }
    public static JsonFileStore<T> Create(string filePath)
    {
        if (!File.Exists(filePath))
            File.WriteAllText(filePath, "[]");

        var data = JsonSerializer.Deserialize<List<T>>(filePath);

        return new JsonFileStore<T>(data!, filePath);
    }

    public IList<T> Data { get => _data; }

    public void Complete()
    {
        var json = JsonSerializer.Serialize(_data);
        File.WriteAllText(_filePath, json);
    }

    public Task CompleteAsync()
    {
        var json = JsonSerializer.Serialize(_data);
        return File.WriteAllTextAsync(_filePath, json);
    }
}

public static class JsonFileStoreExtensions
{
    public static void AddJsonFileStore<T>(this IServiceCollection services, string filePath)
    {
        services.AddScoped(_ => JsonFileStore<T>.Create(filePath));
    }
}
