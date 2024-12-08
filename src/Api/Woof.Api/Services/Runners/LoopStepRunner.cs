using Woof.Api.DataAccess.Models.Instance;
using Woof.Api.Services.Abstractions;

namespace Woof.Api.Services.Runners;

public class LoopStepRunner : IStepRunner<LoopRunStepParameters>
{
    public async Task<string> RunStepAsync(WorkflowRunStep step, LoopRunStepParameters parameters)
    {
        if (parameters.CurrentLoopCount == parameters.LoopCount)
        {
            step.State.Completed = true;
            return string.Empty;
        }

        var stdErr = await IRunner.RunUnitAsync(step.ExecutablePath, step.Arguments);
        parameters.CurrentLoopCount++;

        return stdErr;
    }
}
