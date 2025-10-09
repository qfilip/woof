namespace Woof.Api.DataAccess.Models.Instance;

public class LoopRunStep : WorkflowRunStep
{
    public LoopRunStepParameters Parameters { get; set; } = new();
}

public class LoopRunStepParameters : IRunStepParameter
{
    public int LoopCount { get; set; }
    public int CurrentLoopCount { get; set; }
}