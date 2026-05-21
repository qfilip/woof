using Woof.Api.DataAccess;
using Woof.Api.DataAccess.Entities;
using Woof.Api.DataAccess.Models.Definition;
using Woof.Api.Dtos;
using Woof.Api.Services;

namespace Woof.Api;

public static class EndpointMapper
{
    public static void AddDefinitionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("definitions");
        
        group.MapGet("", async (IFileStore<Workflow> store) =>
        {
            var result = await store.QueryAsync(xs => xs);
            return Results.Ok(result);
        });

        group.MapGet("find", (Guid workflowId, IFileStore<Workflow> store) =>
        {
            var result = store.QueryAsync(xs => xs.FirstOrDefault(x => x.Id == workflowId));
            return Results.Ok(result);
        });

        group.MapPost("create", async (CreateWorkflowDto dto, WorkflowBuilderService wfs) =>
        {
            var result = await wfs.CreateAsync(dto.WorkflowName);
            return Results.Ok(result);
        });

        group.MapPost("add_sequential", async (AddNextStepDto<SequentialStep> dto, WorkflowBuilderService wfs) =>
        {
            var opcode = await wfs.AddNextStepAsync(dto);
            return opcode.ToResult();
        });

        group.MapPost("add_loop", async (AddNextStepDto<LoopStep> dto, WorkflowBuilderService wfs) =>
        {
            var opcode = await wfs.AddNextStepAsync(dto);
            return opcode.ToResult();
        });
    }

    public static void AddRunEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("runs");

        group.MapPost("", async (RunWorkflowDto dto, WorkflowExecutionService wes) =>
        {
            var opcode = await wes.StartWorkflowAsync(dto.WorkflowId);
            return opcode.ToResult(202);
        });
    }
}
