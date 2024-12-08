using Woof.Api.DataAccess.Models.Instance;

namespace Woof.Api.Services.Abstractions;

public interface IStepRunner<T> where T : class, IRunStepParameter
{
    Task<string> RunStepAsync(WorkflowRunStep step, T parameters);
}
