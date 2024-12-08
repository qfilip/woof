using System.Diagnostics;
using Woof.Api.DataAccess.Models.Instance;
using Woof.Api.Services.Abstractions;

namespace Woof.Api.Services.Runners;

public class Runner : IRunner
{
    private readonly IStepRunner<LoopRunStepParameters> _loopRunner;
    private readonly IStepRunner<SequentialRunStepParameters> _sequentialRunner;

    public Runner(
        IStepRunner<LoopRunStepParameters> loopRunner,
        IStepRunner<SequentialRunStepParameters> sequentialRunner)
    {
        _loopRunner = loopRunner;
        _sequentialRunner = sequentialRunner;
    }
    public Task<string> RunStepAsync<T>(T step) where T : WorkflowRunStep
    {
        var runTask = step switch
        {
            { LoopParameters: not null } => _loopRunner.RunStepAsync(step, step.LoopParameters),
            { SequentialParameters: not null } => _sequentialRunner.RunStepAsync(step, step.SequentialParameters),
            _ => throw new UnreachableException($"Step parameters not found for {step.Id}")
        };

        return runTask;
    }
}
