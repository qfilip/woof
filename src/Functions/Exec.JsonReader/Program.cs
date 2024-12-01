using System.Text.Json.Serialization;

Console.WriteLine("Finished");

public class Parameters
{
    public string? P1 { get; set; }
    public string? P2 { get; set; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Parameters))]
internal partial class AppSerializerContext : JsonSerializerContext
{
}
