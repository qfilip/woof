module WorklowEndpoints

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder

open Definitions
open Dtos
open Woof.Webapi.DataAccess

let private create =
    Func<CreateWorkflowDto, YamlFileStore<Workflow>, Task<IResult>>(fun dto fs -> task {
        let! result = Definitions.createWorkflow dto fs
        return Results.Ok(result)
    })

let mapEndpoints (app: WebApplication) =
    let g = app.MapGroup("workflows")
    
    g.MapGet("create", create)