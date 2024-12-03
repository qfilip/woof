namespace Woof.Api.DataAccess.Models.Definition;

public class WorkflowStep
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public required string ExecutableName { get; set; }
    public string? Arguments { get; set; }
    public WorkflowStep? Next { get; set; }
}
