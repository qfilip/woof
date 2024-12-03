using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.DataAccess.Models.Instance;

public class LoopRunStep : WorkflowRunStep
{
    public int LoopCount { get; set; }
    public int CurrentLoopCount { get; set; }
}
