using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.DataAccess.Models.Instance;

public class WorkflowRunStep
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public required string ExecutablePath { get; set; }
    public string? Arguments { get; set; }
    public StepState State { get; set; } = new();
    public WorkflowRunStep? Next { get; set; }
}
