using System.Text.Json.Serialization;

namespace Woof.Api.DataAccess.Models.Instance;

[JsonDerivedType(typeof(SequentialRunStep), typeDiscriminator: "sequentialRunStep")]
[JsonDerivedType(typeof(LoopRunStep), typeDiscriminator: "loopRunStep")]
public class WorkflowRunStep
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public required string ExecutablePath { get; set; }
    public string? Arguments { get; set; }
    public StepState State { get; set; } = new();
    public WorkflowRunStep? Next { get; set; }
}
