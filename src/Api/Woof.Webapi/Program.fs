namespace Woof.Webapi
#nowarn "20"
open System
open Woof.Webapi.DataAccess
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)
        
        builder.Services
            .AddScoped<YamlFileStore<Definitions.Workflow>>(fun _ ->
                YamlFileStore<Definitions.Workflow>.Create(builder.Environment, "workflows.yaml"))

        builder.Services
            .AddScoped<YamlFileStore<Instances.WorkflowRun>>(fun _ ->
                YamlFileStore<Instances.WorkflowRun>.Create(builder.Environment, "workflow_runs.yaml"))

        Executables.setExecutableDirectory builder.Environment "executables"

        builder.Services.AddCors(fun b -> b.AddDefaultPolicy(
            fun o -> 
                o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin() |> ignore))
        
        let app = builder.Build()
        app.UseCors()

        WorklowEndpoints.mapEndpoints app

        app.Run()

        exitCode
