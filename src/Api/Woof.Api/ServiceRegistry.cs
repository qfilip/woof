using System.Threading.Channels;
using Woof.Api.DataAccess;
using Woof.Api.DataAccess.Entities;
using Woof.Api.DataAccess.Models;
using Woof.Api.DataAccess.Models.Definition;
using Woof.Api.DataAccess.Models.Instance;
using Woof.Api.Messaging;
using Woof.Api.Services;
using Woof.Api.Services.Abstractions;
using Woof.Api.Services.Runners;

namespace Woof.Api;

public static class ServiceRegistry
{
    public static void AddAppServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddLogging();

        // messages
        builder.Services.AddSingleton(_ => Channel.CreateUnbounded<WorkflowRun>());
        builder.Services.AddSingleton(sp => sp.GetRequiredService<Channel<WorkflowRun>>().Writer);
        builder.Services.AddSingleton(sp => sp.GetRequiredService<Channel<WorkflowRun>>().Reader);
        builder.Services.AddHostedService<ChannelHostingService>();

        // file stores
        var workflowStepsDiscriminators = GetSteps<WorkflowStep>();
        var workflowRunStepsDiscriminators = GetSteps<WorkflowRunStep>();

        builder.Services.AddYamlFileStore<Workflow, WorkflowStep>(builder.Environment, "workflows.yaml", workflowStepsDiscriminators);
        builder.Services.AddYamlFileStore<WorkflowRun, WorkflowRunStep>(builder.Environment, "workflow_runs.yaml", workflowRunStepsDiscriminators);

        // services
        builder.Services.AddScoped<WorkflowBuilderService>();
        builder.Services.AddScoped<WorkflowExecutionService>();
        builder.Services.AddSingleton(_ => new ExecSearchService(Path.Combine(builder.Environment.WebRootPath, "functions")));

        // runners
        builder.Services.AddScoped<IRunner, Runner>();
        
        builder.Services.AddScoped<IStepRunner<LoopRunStep>, LoopStepRunner>();
        builder.Services.AddScoped<IStepRunner<SequentialRunStep>, SequentialStepRunner>();
    }

    private static Dictionary<string, Type> GetSteps<T>() where T : IStep
    {
        var baseType = typeof(T);

        var arr = baseType
            .Assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(baseType))
            .ToArray();
        
        return arr.ToDictionary(x => x.Name.ToLower(), x => x);
    }
}