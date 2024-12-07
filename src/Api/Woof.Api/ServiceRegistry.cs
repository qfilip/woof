﻿using System.Threading.Channels;
using Woof.Api.DataAccess;
using Woof.Api.DataAccess.Entities;
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

        builder.Services.AddJsonFileStore<Workflow>(builder.Environment, "workflows.json");
        builder.Services.AddJsonFileStore<WorkflowRun>(builder.Environment, "workflow_runs.json");

        // services
        builder.Services.AddScoped<WorkflowBuilderService>();
        builder.Services.AddScoped<WorkflowExecutionService>();
        builder.Services.AddSingleton(_ => new ExecSearchService(Path.Combine(builder.Environment.WebRootPath, "functions")));

        // runners
        builder.Services.AddScoped<IRunner, Runner>();
        
        builder.Services.AddScoped<IStepRunner<LoopRunStepParameters>, LoopStepRunner>();
        builder.Services.AddScoped<IStepRunner<SequentialRunStepParameters>, SequentialStepRunner>();
    }
}