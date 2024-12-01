using Woof.Api.DataAccess.Entities;
using Woof.Api.DataAccess;
using Woof.Api.Dtos;
using Woof.Api.Services;
using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api;

public static class EndpointMapper
{
    public static void AddDefinitionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("definition");
        group.MapGet("", (LiteDbContext<Workflow> wfdb) => Results.Ok(wfdb.GetAll()));

        group.MapGet("find", (Guid workflowId, LiteDbContext<Workflow> wfdb) =>
            Results.Ok(wfdb.FindAll(x => x.Id == workflowId)));

        group.MapPost("create", (CreateWorkflowDto dto, WorkflowBuilderService wfs) =>
            Results.Ok(wfs.Create(dto.WorkflowName!)));

        group.MapPost("add_sequential", (AddSequentialStepDto dto, WorkflowBuilderService wfs) =>
            wfs.AddNextStep<SequentialStep>(dto.WorkflowId, dto.ParentStepId, dto.Step).ToResult());

        group.MapPost("add_loop", (AddLoopStepDto dto, WorkflowBuilderService wfs) =>
            wfs.AddNextStep<LoopStep>(dto.WorkflowId, dto.ParentStepId, dto.Step).ToResult());
    }

    public static void AddRunEndpoints(this WebApplication app)
    {

    }
}
