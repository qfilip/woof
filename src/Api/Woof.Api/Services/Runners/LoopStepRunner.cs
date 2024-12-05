using Woof.Api.DataAccess.Models.Instance;
using Woof.Api.Services.Abstractions;

namespace Woof.Api.Services.Runners;

public class LoopStepRunner : IStepRunner<LoopRunStep>
{
    public async Task<string> RunStepAsync(LoopRunStep step)
    {
        if (step.CurrentLoopCount == step.LoopCount)
        {
            step.State.Completed = true;
            return string.Empty;
        }

        var stdErr = await IRunner.RunUnitAsync(step.ExecutablePath, step.Arguments);
        step.CurrentLoopCount++;

        return stdErr;
    }
}
