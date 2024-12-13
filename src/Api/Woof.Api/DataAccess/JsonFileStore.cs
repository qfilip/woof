using System.Text.Json;
using Woof.Api.DataAccess.Entities;

namespace Woof.Api.DataAccess;

public class JsonFileStore<T> : IFileStore<T> where T : FileEntity
{
    private readonly string _filePath;
    private List<Action<List<T>>> _commands = new();
    private static SemaphoreSlim Gate = new(1);
    
    private JsonFileStore(string filePath)
    {
        _filePath = filePath;
    }

    public static JsonFileStore<T> Create(IWebHostEnvironment env, string fileName)
    {
        var filePath = Path.Combine(env.WebRootPath, fileName);

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, "[]");

        return new JsonFileStore<T>(filePath);
    }

    public async Task<U> QueryAsync<U>(Func<List<T>, U> query)
    {
        await Gate.WaitAsync();
        var text = await File.ReadAllTextAsync(_filePath);
        var data = JsonSerializer.Deserialize<List<T>>(text)!;

        var result = query(data);
        Gate.Release();

        return result;
    }

    public void Command(Action<List<T>> command) => _commands.Add(command);

    public void Update(T entity)
    {
        _commands.Add(xs => _commands.Add(xs => UpdateCommand(xs, entity)));
    }

    public Task UpdateAsync(T entity)
    {
        _commands.Add(xs => UpdateCommand(xs, entity));
        return CompleteAsync();
    }

    private void UpdateCommand(List<T> xs, T target)
    {
        var i = xs.FindIndex(x => x.Id == target.Id);
        if (i == -1)
            throw new InvalidOperationException($"Entity {target.Id} not found");

        xs[i] = target;
    }

    public async Task CompleteAsync()
    {
        await Gate.WaitAsync();
        var text = await File.ReadAllTextAsync(_filePath);
        var data = JsonSerializer.Deserialize<List<T>>(text)!;

        foreach (var cmd in _commands)
            cmd(data);

        _commands.Clear();
        var newText = JsonSerializer.Serialize(data);
        await File.WriteAllTextAsync(_filePath, newText);
        Gate.Release();
    }
}

public static class JsonFileStoreExtensions
{
    public static void AddJsonFileStore<T>(
        this IServiceCollection services,
        IWebHostEnvironment env,
        string fileName) where T : FileEntity
    {
        services.AddScoped<IFileStore<T>>(_ => JsonFileStore<T>.Create(env, fileName));
    }
}