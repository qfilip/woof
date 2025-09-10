namespace Woof.Api.DataAccess.Models.Definition;

public class LoopStep : WorkflowStep
{
    public required LoopStepParameters Parameters { get; set; }
}

public class LoopStepParameters
{
    public int LoopCount { get; set; }
}