using Woof.Api.DataAccess.Entities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Woof.Api.DataAccess;

public class YamlFileStore<T> where T : YamlEntity
{
    private readonly string _filePath;
    private List<Action<List<T>>> _commands = new();
    
    private static ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private static IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private static SemaphoreSlim Gate = new(1);

    private YamlFileStore(string filePath)
    {
        _filePath = filePath;
    }

    public static YamlFileStore<T> Create(IWebHostEnvironment env, string fileName)
    {
        var filePath = Path.Combine(env.WebRootPath, fileName);

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, "[]");

        return new YamlFileStore<T>(filePath);
    }

    public async Task<U> QueryAsync<U>(Func<List<T>, U> query)
    {
        await Gate.WaitAsync();
        var text = await File.ReadAllTextAsync(_filePath);
        var data = Deserializer.Deserialize<List<T>>(text);

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
        var data = Deserializer.Deserialize<List<T>>(text);

        foreach (var cmd in _commands)
            cmd(data);

        _commands.Clear();
        var newText = Serializer.Serialize(data);
        await File.WriteAllTextAsync(_filePath, newText);
        Gate.Release();
    }
}

public static class YamlFileStoreExtensions
{
    public static void AddYamlFileStore<T>(this IServiceCollection services, IWebHostEnvironment env, string fileName) where T : YamlEntity
    {
        services.AddScoped(_ => YamlFileStore<T>.Create(env, fileName));
    }
}
