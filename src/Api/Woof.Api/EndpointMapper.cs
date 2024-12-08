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
        
        group.MapGet("", async (JsonFileStore<Workflow> store) =>
        {
            var result = await store.QueryAsync(xs => xs);
            return Results.Ok(result);
        });

        group.MapGet("find", (Guid workflowId, JsonFileStore<Workflow> store) =>
        {
            var result = store.QueryAsync(xs => xs.FirstOrDefault(x => x.Id == workflowId));
            return Results.Ok(result);
        });

        group.MapPost("create", async (CreateWorkflowDto dto, WorkflowBuilderService wfs) =>
        {
            var result = await wfs.CreateAsync(dto.WorkflowName);
            return Results.Ok(result);
        });

        group.MapPost("add_next_step", async (AddNextStepDto dto, WorkflowBuilderService wfs) =>
        {
            var result = await wfs.AddNextStepAsync(dto);
            return result.ToResult();
        });
    }

    public static void AddRunEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("runs");

        group.MapPost("", async (RunWorkflowDto dto, WorkflowExecutionService wes) =>
        {
            var result = await wes.StartWorkflowAsync(dto.WorkflowId);
            return result.ToResult();
        });
    }
}
