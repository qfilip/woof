using System.Threading.Channels;
using Woof.Api.DataAccess;
using Woof.Api.DataAccess.Entities;
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

        // messages
        builder.Services.AddSingleton(_ => Channel.CreateUnbounded<WorkflowRun>());
        builder.Services.AddSingleton(sp => sp.GetRequiredService<Channel<WorkflowRun>>().Writer);
        builder.Services.AddSingleton(sp => sp.GetRequiredService<Channel<WorkflowRun>>().Reader);
        builder.Services.AddHostedService<ChannelHostingService>();

        builder.Services.AddYamlFileStore<Workflow>(builder.Environment, "workflows.yaml");
        builder.Services.AddYamlFileStore<WorkflowRun>(builder.Environment, "workflow_runs.yaml");

        // services
        builder.Services.AddScoped<WorkflowBuilderService>();
        builder.Services.AddScoped<WorkflowExecutionService>();
        builder.Services.AddSingleton(_ => new ExecSearchService(Path.Combine(builder.Environment.WebRootPath, "functions")));

        // runners
        builder.Services.AddScoped<IRunner, Runner>();
        var type = typeof(IStepRunner<>);
        var assembly = type.Assembly;

        var stepRunners = assembly
          .GetTypes()
          .Where(x =>
             x.GetInterface(type.Name) != null &&
             !x.IsAbstract &&
             !x.IsInterface)
          .ToList();

        foreach (var sr in stepRunners)
            builder.Services.AddTransient(sr);
    }
}