namespace Woof.Webapi
#nowarn "20"
open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)

        let app = builder.Build()

        app.Run()

        exitCode
