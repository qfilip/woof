using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.DataAccess.Models.Instance;

public class InitialRunStep : WorkflowRunStep
{
    public InitialRunStep(WorkflowStep step) : base(step)
    {
    }
}
