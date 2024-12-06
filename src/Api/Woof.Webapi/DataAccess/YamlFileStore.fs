namespace Woof.Webapi.DataAccess

open System.IO
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions

type YamlFileStore<'T> private (data: list<'T>, filePath: string) =
    let mutable data = data
    let filePath = filePath
    
    static member serializer =
        SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build()

    static member deserializer =
        DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build()

    static member Create<'T>(filePath) =
      match File.Exists(filePath) with
      | true -> ()
      | false -> File.WriteAllText(filePath, "[]");    
      
      let text = File.ReadAllText(filePath)
      let data = YamlFileStore.deserializer.Deserialize<list<'T>>(text)

      YamlFileStore<'T>(data, filePath)

    member val Data = data with get, set

    member this.Complete() =
      let yaml = YamlFileStore<'T>.serializer.Serialize(this.Data)
      File.WriteAllText(filePath, yaml)