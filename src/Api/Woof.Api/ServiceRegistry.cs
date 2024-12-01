using System.Threading.Channels;
using Woof.Api.DataAccess.Entities;
using Woof.Api.DataAccess.Extensions;
using Woof.Api.DataAccess;
using Woof.Api.Services;

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

        // database
        builder.Services.AddSingleton(_ => new LiteDbConfig(builder.Environment));
        
        builder.Services.AddScoped(sp =>
        {
            var config = sp.GetRequiredService<LiteDbConfig>()!;
            return new LiteDbContext<Workflow>(config, "workflows");
        });

        builder.Services.AddScoped(sp =>
        {
            var config = sp.GetRequiredService<LiteDbConfig>()!;
            return new LiteDbContext<WorkflowRun>(config, "workflow_runs");
        });

        // services
        builder.Services.AddScoped<WorkflowBuilderService>();
        builder.Services.AddSingleton(_ => new ExecSearchService(Path.Combine(builder.Environment.WebRootPath, "functions")));
    }
}