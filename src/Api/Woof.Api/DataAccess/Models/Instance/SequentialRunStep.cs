using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.DataAccess.Models.Instance;

public class SequentialRunStep : WorkflowRunStep
{
    public SequentialRunStep(WorkflowStep step) : base(step)
    {
    }
}
