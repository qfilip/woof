using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.Dtos;

public record AddLoopStepDto(
    Guid WorkflowId,
    Guid ParentStepId,
    WorkflowStep Step
);