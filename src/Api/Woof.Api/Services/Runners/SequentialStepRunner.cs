using Woof.Api.DataAccess.Models.Instance;
using Woof.Api.Services.Abstractions;

namespace Woof.Api.Services.Runners;

public class SequentialStepRunner : IStepRunner<SequentialRunStep>
{
    public Task<string> RunStepAsync(SequentialRunStep step)
    {
        step.State.Completed = true;
        return IRunner.RunUnitAsync(step.ExecutablePath, step.Arguments);
    }
}
