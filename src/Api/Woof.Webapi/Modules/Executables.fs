module Executables

open System.IO
open Microsoft.AspNetCore.Hosting

let mutable private executableDirectory: string option = None

let private checkDirectorySet callback =
    match executableDirectory with
    | None -> failwith "Executable directory not set"
    | Some x -> callback x

let setExecutableDirectory (env: IWebHostEnvironment) (dir: string) =
    let value = Path.Combine(env.WebRootPath, dir)
    match Directory.Exists(value) with
    | false -> failwith "Executable directory not found"
    | true ->
        executableDirectory = Some value

let findExecutable name =
    let tryFindExecutable dir =
        let executable = Path.Combine(dir, name)

        match File.Exists(executable) with
        | false -> Error "Executable file not found"
        | true -> Ok executable

    checkDirectorySet tryFindExecutable
