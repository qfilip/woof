using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.Dtos;

public record AddNextStepDto<T>(
    Guid WorkflowId,
    Guid? ParentStepId,
    T Step
) where T : WorkflowStep;