using System.Diagnostics;
using Woof.Api.DataAccess.Models.Instance;
using Woof.Api.Services.Abstractions;

namespace Woof.Api.Services.Runners;

public class Runner : IRunner
{
    private readonly IStepRunner<LoopRunStep> _loopRunner;
    private readonly IStepRunner<SequentialRunStep> _sequentialRunner;

    public Runner(
        IStepRunner<LoopRunStep> loopRunner,
        IStepRunner<SequentialRunStep> sequentialRunner)
    {
        _loopRunner = loopRunner;
        _sequentialRunner = sequentialRunner;
    }
    public Task<string> RunStepAsync<T>(T step) where T : WorkflowRunStep
    {
        var runTask = step switch
        {
            LoopRunStep loop => _loopRunner.RunStepAsync(loop),
            SequentialRunStep seq => _sequentialRunner.RunStepAsync(seq),
            _ => throw new UnreachableException($"Step parameters not found for {step.Id}")
        };

        return runTask;
    }
}
