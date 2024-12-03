namespace Woof.Api.DataAccess.Models.Definition;

public class WorkflowStep
{
    public WorkflowStep(WorkflowUnit unit)
    {
        Unit = unit;
    }
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public WorkflowUnit Unit { get; set; }
    public WorkflowStep? Next { get; set; }
}
