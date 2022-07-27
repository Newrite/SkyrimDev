module Launcher.Config

open FSharp.Configuration

[<AutoOpen>]
module Config =
    type Yaml = YamlConfig<"Launcher.yaml">
    
    let Config = Yaml()
    
    Config.Server.dropboxURL.AbsolutePath |> printfn "%s"