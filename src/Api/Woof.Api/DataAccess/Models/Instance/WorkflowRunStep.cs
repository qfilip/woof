using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.DataAccess.Models.Instance;

public class WorkflowRunStep
{
    public WorkflowRunStep(WorkflowStep step)
    {
        State = new(step);
    }

    public StepState<WorkflowStep> State { get; set; }
    public WorkflowRunStep? Next { get; set; }
}
