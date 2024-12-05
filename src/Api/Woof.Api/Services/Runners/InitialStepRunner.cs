using Woof.Api.DataAccess.Models.Instance;
using Woof.Api.Services.Abstractions;

namespace Woof.Api.Services.Runners;

public class InitialStepRunner : IStepRunner<InitialRunStep>
{
    public Task<string> RunStepAsync(InitialRunStep step)
    {
        step.State.Completed = true;
        return Task.FromResult(string.Empty);
    }
}
