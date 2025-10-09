namespace Woof.Api.DataAccess.Models.Definition;

public class LoopStep : WorkflowStep
{
    public LoopStepParameters Parameters { get; set; } = new();
}

public class LoopStepParameters
{
    public int LoopCount { get; set; }
}