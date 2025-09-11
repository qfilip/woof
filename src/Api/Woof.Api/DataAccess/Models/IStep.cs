namespace Woof.Api.DataAccess.Models;

public interface IStep
{
    string Type { get; set; }
    static string SetType<T>(T step) where T : IStep
        => step.Type = typeof(T).Name.ToLower();
}
