using Woof.Api.DataAccess.Models.Definition;
using Woof.Api.DataAccess.Models.Instance;

namespace Woof.Api.Services.Abstractions;

public interface IStepRunner<T> where T : WorkflowRunStep
{
    Task<string> RunStepAsync(T step);
}
