using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.Dtos;

public record AddSequentialStepDto(
    Guid WorkflowId,
    Guid ParentStepId,
    SequentialStep Step
);