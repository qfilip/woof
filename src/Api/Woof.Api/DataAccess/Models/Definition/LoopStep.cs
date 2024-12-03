namespace Woof.Api.DataAccess.Models.Definition;

public class LoopStep : WorkflowStep
{
    public LoopStep(WorkflowUnit unit) : base(unit)
    {
    }

    public int LoopCount { get; set; }
}
