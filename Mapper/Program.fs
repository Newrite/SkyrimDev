//dotnet publish -c Release  -r win-x64 -p:PublishSingleFile=true --self-contained true
//cli for build
open System.IO
open System.Text.Json
open System.Collections.Generic
open System.Security.Cryptography

let serializeToFile fileName (d: Dictionary<string, string>) =
    let opt = JsonSerializerOptions(WriteIndented = true)
    use writer = File.Open(fileName, FileMode.Create) |> StreamWriter
    JsonSerializer.Serialize(d, opt) |> writer.Write
    writer.Flush()
    
let files filesPath =
    let rec filesUnder (basePath: string) =
        seq {
            if basePath.Contains("mods\\!PS") then
                yield! Directory.GetFiles(basePath)
            for subDir in Directory.GetDirectories(basePath) do
                yield! filesUnder subDir
        }
    filesUnder filesPath, filesPath
        
let seqHASH (seqPath, basePath) =
    let convert (byteArray: byte[])=
        byteArray
        |>Array.fold (fun acc elem -> acc + elem.ToString()) ""
    let dict = Dictionary<string, string>()
    use sha = new SHA1Managed()
    seqPath
    |> Seq.map (fun x -> (x, File.ReadAllBytes(x)))
    |> Seq.iter (fun (x, y) -> dict.Add(x.Replace(basePath, "").Replace('\\','/'), (convert <| sha.ComputeHash(y))))
    dict
    
[<EntryPoint>]
let main _ =
    Directory.GetCurrentDirectory() |> files |> seqHASH |> serializeToFile "modmap.json"
    0