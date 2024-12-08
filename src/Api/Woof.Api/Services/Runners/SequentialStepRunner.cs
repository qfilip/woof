using Woof.Api.DataAccess.Models.Instance;
using Woof.Api.Services.Abstractions;

namespace Woof.Api.Services.Runners;

public class SequentialStepRunner : IStepRunner<SequentialRunStepParameters>
{
    public Task<string> RunStepAsync(WorkflowRunStep step, SequentialRunStepParameters _)
    {
        step.State.Completed = true;
        return IRunner.RunUnitAsync(step.ExecutablePath, step.Arguments);
    }
}
