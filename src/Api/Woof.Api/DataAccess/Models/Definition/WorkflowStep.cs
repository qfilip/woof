using System.Text.Json.Serialization;

namespace Woof.Api.DataAccess.Models.Definition;

[JsonDerivedType(typeof(SequentialStep), typeDiscriminator: "sequentialStep")]
[JsonDerivedType(typeof(LoopStep), typeDiscriminator: "loopStep")]
public class WorkflowStep : IStep
{
    public Guid Id { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
    public required string ExecutableName { get; set; }
    public string? Arguments { get; set; }
    public WorkflowStep? Next { get; set; }
}
