using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Input;
using YamlDotNet.Serialization;
using System.Text.Json;

namespace Woof.Api.DataAccess;

public interface IFileStore<T> where T : FileEntity
{
    public Task<U> QueryAsync<U>(Func<List<T>, U> query);

    public void Command(Action<List<T>> command);

    public void Update(T entity);

    public Task UpdateAsync(T entity);

    public Task CompleteAsync();
}

public class FileEntity
{
    public Guid Id { get; set; }
}
