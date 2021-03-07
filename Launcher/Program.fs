open System.IO
open System.Security.Cryptography
open System.Collections.Generic



let EnumerateDirectoryFilesInfo root =
    let rec traverse (d: DirectoryInfo) =
        seq {
            for f in d.GetFiles() do
                yield f

            for dd in d.GetDirectories() do
                yield! traverse dd
        }

    traverse (DirectoryInfo(root))


let filesInfo =
    EnumerateDirectoryFilesInfo(Directory.GetCurrentDirectory().ToString())

let seqHASH =
    filesInfo
    |> Seq.map (fun x -> File.ReadAllBytes(x.ToString()))
    |> Seq.map (fun x -> SHA256Managed().ComputeHash(x))

let dict =
    let d = Dictionary<FileInfo, string>()
    Seq.iter2 (fun (x: byte []) y -> d.Add(y, Array.fold (fun x y -> x + y.ToString()) "" x)) seqHASH filesInfo
    d

for key in dict do
    printf $"{key}\n"

System.Console.ReadKey() |> ignore
