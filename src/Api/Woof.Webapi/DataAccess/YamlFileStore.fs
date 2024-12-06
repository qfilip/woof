namespace Woof.Webapi.DataAccess

open System.IO
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open System.Threading
open Microsoft.AspNetCore.Hosting

type YamlFileStore<'T> private (filePath: string) =
    let filePath = filePath

    let mutable commands: list<list<'T> -> list<'T>> = [];
    
    static member serializer =
        SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build()

    static member deserializer =
        DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build()

    static member semaphore = new SemaphoreSlim(1)

    static member Create<'T>(env: IWebHostEnvironment, fileName: string) =
        let filePath = Path.Combine(env.WebRootPath, fileName)
        match File.Exists(filePath) with
        | true -> ()
        | false -> File.WriteAllText(filePath, "[]");    

        YamlFileStore<'T>(filePath)

    member this.FindAsync(predicate: 'T -> bool) = task {
        do! YamlFileStore<'T>.semaphore.WaitAsync()
        let! text = File.ReadAllTextAsync(filePath)
        let data = YamlFileStore<'T>.deserializer.Deserialize<list<'T>>(text)
        do YamlFileStore<'T>.semaphore.Release() |> ignore

        let result =
            data
            |> List.find predicate

        return result
    }

    member this.Do(command: list<'T> -> list<'T>) = commands <- command::commands
        
    member this.Complete() = task {
        do! YamlFileStore<'T>.semaphore.WaitAsync()
        let! text = File.ReadAllTextAsync(filePath)
        let mutable data = YamlFileStore<'T>.deserializer.Deserialize<list<'T>>(text)

        for cmd in commands do
            data <- cmd data

        let yaml = YamlFileStore<'T>.serializer.Serialize(data)
        do! File.WriteAllTextAsync(filePath, yaml)
        commands <- []
    }