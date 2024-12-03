using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.DataAccess.Models.Instance;

public class LoopRunStep : WorkflowRunStep
{
    public LoopRunStep(WorkflowStep step) : base(step)
    {
    }

    public int CurrentLoopCount { get; set; }
}
