using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.Dtos;

public record AddNextStepDto(
    Guid WorkflowId,
    Guid? ParentStepId,
    WorkflowStep Step
);