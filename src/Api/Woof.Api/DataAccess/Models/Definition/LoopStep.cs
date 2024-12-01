namespace Woof.Api.DataAccess.Models.Definition;

public class LoopStep : WorkflowStep
{
    public int LoopCount { get; set; }
    public WorkflowUnit? Unit { get; set; }
}
