using Woof.Api.DataAccess.Models;
using Woof.Api.DataAccess.Models.Definition;
using Woof.Api.DataAccess.Models.Instance;

namespace Woof.Api.DataAccess.Entities;

public static class StepExtensions
{
    public static T MapTo<T>(this T source) where T : WorkflowStep, new()
    {
        return new T()
        {
            Id = source.Id,
            Name = source.Name,
            Type = IStep.GetType<T>(),
            Arguments = source.Arguments,
            ExecutableName = source.ExecutableName,
        };
    }

    public static WorkflowRunStep MapTo<TRunStep, TStep>(this TStep step, string executablePath, Action<TRunStep>? modifier = null)
        where TRunStep : WorkflowRunStep, new()
        where TStep : WorkflowStep
    {
        var runStep = new TRunStep()
        {
            Id = step.Id,
            Name = step.Name,
            State = new(),
            Arguments = step.Arguments,
            ExecutablePath = executablePath,
            Type = IStep.GetType<TRunStep>()
        };

        modifier?.Invoke(runStep);

        return runStep;
    }
}
