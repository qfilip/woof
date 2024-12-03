using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.DataAccess.Models.Instance;

public class StepState<T> where T : WorkflowStep
{
    public StepState(T step)
    {
        Step = step;
    }
    public T Step { get; set; }
    public bool Completed { get; set; }
    public string? Status { get; set; }
    public string? StdErr { get; set; }
}
