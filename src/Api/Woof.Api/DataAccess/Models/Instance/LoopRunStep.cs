namespace Woof.Api.DataAccess.Models.Instance;

public class LoopRunStep : WorkflowRunStep
{
    public required LoopRunStepParameters Parameters { get; set; }
}

public class LoopRunStepParameters : IRunStepParameter
{
    public int LoopCount { get; set; }
    public int CurrentLoopCount { get; set; }
}