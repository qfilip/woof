using Woof.Api.DataAccess.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Woof.Api.DataAccess;

public class YamlFileStore<T, U> : IFileStore<T>
    where T : FileEntity
    where U : IStep
{
    private readonly string _filePath;
    private List<Action<List<T>>> _commands = new();
    
    private ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private IDeserializer Deserializer;

    private static SemaphoreSlim Gate = new(1);

    private YamlFileStore(string filePath, IDictionary<string, Type> typeDiscriminators)
    {
        _filePath = filePath;
        Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithTypeDiscriminatingNodeDeserializer(o =>
        {
            o.AddKeyValueTypeDiscriminator<U>("type", typeDiscriminators);
        })
        .Build();
    }

    public static YamlFileStore<T, U> Create(IWebHostEnvironment env, string fileName, IDictionary<string, Type> typeDiscriminators)
    {
        var filePath = Path.Combine(env.WebRootPath, fileName);

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, "[]");

        return new YamlFileStore<T, U>(filePath, typeDiscriminators);
    }

    public async Task<TResult> QueryAsync<TResult>(Func<List<T>, TResult> query)
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
    public static void AddYamlFileStore<T, U>(
        this IServiceCollection services,
        IWebHostEnvironment env,
        string fileName, IDictionary<string, Type> typeDiscriminators)
        where T : FileEntity
        where U : IStep
    {
        services.AddScoped<IFileStore<T>>(_ => YamlFileStore<T, U>.Create(env, fileName, typeDiscriminators));
    }
}
