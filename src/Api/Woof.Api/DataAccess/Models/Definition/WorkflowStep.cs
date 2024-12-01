namespace Woof.Api.DataAccess.Models.Definition;

public class WorkflowStep
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public WorkflowStep? Next { get; set; }
}
