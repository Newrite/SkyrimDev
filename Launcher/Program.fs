open System.IO
open System.Collections.Generic
open System.Security.Cryptography
open System.Net
open FSharp.Data
open FSharp.Json
open System.Text.Json
open System.Threading.Tasks
open Microsoft.VisualBasic

let serializeToFile (d: Dictionary<string, string>) fileName =
    let opt = JsonSerializerOptions(WriteIndented = true)
    use writer = File.Open(fileName, FileMode.Create) |> StreamWriter
    JsonSerializer.Serialize(d, opt) |> writer.Write
    writer.Flush()

let deserializeFromFile fileName =
    File.ReadAllText(fileName)
    |>JsonSerializer.Deserialize<Dictionary<string, string>>

let [<Literal>] path = @"G:\testpapka"

module Downloads =
    
    type Respons = {
        info: string
        link: string
        status: bool
    }
    
    let [<Literal>] private serverUrl = @"http://10.10.10.10:3030"
    
    let [<Literal>] private dropboxUrl = serverUrl + "/dropbox"
    let [<Literal>] private yandexUrl = serverUrl + "/yandex"
    let [<Literal>] private modmapUrl = serverUrl + "/modmap"
    let [<Literal>] private tokenServer = "Bearer OMEGALULISPOWER!"
    
    let private headers () =
        [ "Accept", "application/json"
          "Authorization", tokenServer ]
        
    let private deserializeRespons<'a> respons =
    
        match respons with
        | Ok (ok) ->
            try
    
                Json.deserialize<'a> ok |> Ok
            with :? JsonDeserializationError as eX ->
    
                Error
                <| sprintf "deserializeRespons<'a> JsonDeserializationError Err:%s P: %A" eX.Message respons
        | Error (err) ->
    
            Error err
        
    let private getYandexLink pathToFile =

        try
            let url = yandexUrl

            Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "path", pathToFile ], headers = headers ())
            |> Ok
        with :? WebException as eX ->

            Error
            <| sprintf "%s" eX.Message

    let private getDropboxLink pathToFile =

        try
            let url = dropboxUrl

            Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "path", pathToFile ], headers = headers ())
            |> Ok
        with :? WebException as eX ->

            Error
            <| sprintf "%s" eX.Message
            
    let getModmapFromServer() =
        let deserialize (text: string) =
            text
            |>JsonSerializer.Deserialize<Dictionary<string, string>>
            
        try
            let url = modmapUrl

            Http.RequestString(url, httpMethod = HttpMethod.Get, query = ["version", "release"], headers = headers ())
            |> deserialize |> Ok
        with :? WebException as eX ->

            Error
            <| sprintf "%s" eX.Message
            
    let convert (byteArray: byte[])=
            byteArray
            |>Array.fold (fun acc elem -> acc + elem.ToString()) ""
            
    let private downloadFile (url: string) (toPath: string) (sumSHA: string) =
        use sha = new SHA1Managed()
        let pathWithoutFile =
            let i = toPath.LastIndexOf('\\')
            toPath.Substring(0, i)
        Directory.CreateDirectory(pathWithoutFile) |> printfn "%A"
        use wc = new WebClient()
        wc.DownloadFile(System.Uri(url), toPath+".temp")
        File.Delete(toPath)
        FileSystem.Rename(toPath+".temp", toPath)
        let newSumSHA = convert <| sha.ComputeHash(File.ReadAllBytes(toPath))
        if newSumSHA <> sumSHA then printfn "ALARM bad download"
        
    let startDownload pathToFile fullFilePath summSHA =
        getYandexLink pathToFile
        |>deserializeRespons<Respons>
        |>function
        | Ok (resp) ->
            if resp.status then
                downloadFile resp.link fullFilePath summSHA
                Ok "Download successful"
            else Error "Status not ok"
        | Error (err) -> Error err



//Downloads.getModmapFromServer()
//|>function
//|Ok(dict) -> for KeyValue(k, v) in dict do
//        printfn "key = %s value = %s" k v
//|Error(err) -> printfn "%s" err

let files sPath =
    let rec filesUnder (basePath: string) =
        seq {
            //printfn "%s" basePath
            if basePath.Contains("mods\\!PS") then
                yield! Directory.GetFiles(basePath)
            for subDir in Directory.GetDirectories(basePath) do
                yield! filesUnder subDir
        }
    filesUnder sPath
        
let seqHASH sPath =
    let dict = Dictionary<string, string>()
    use sha = new SHA1Managed()
    files sPath
    |> Seq.map (fun x -> (x, File.ReadAllBytes(x)))
    |> Seq.iter (fun (x, y) -> dict.Add(x.Replace(sPath, "").Replace('\\','/'), (Downloads.convert <| sha.ComputeHash(y))))
    dict

//serializeToFile (seqHASH path) "modmap.json"

let compression (mapServer: Dictionary<string, string>) (mapClient: Dictionary<string, string>) =
    
    let fullPath (sKey: string) = path + sKey.Replace('/', '\\')
    
    let checkToDownload =
        let downloadSeq = [
            for KeyValue(k, v) in mapServer do
                match mapClient.ContainsKey(k) with
                | true ->
                    //printfn "Full path = %s key = %s value1 = %s value2 = %s" fullPath k v mapClient.[k]
                    let check = v = mapClient.[k]
                    if check then printfn "Все в порядке" else
                        printfn "Сумма файлов несовпадает. Вызываем асинхронную функцию на скачивание в директорию %s"
                        <| fullPath k
                        yield (k, v)
                | false ->
                    printfn "Такого файла нет. Добавляем в пул на скачивание в директорию %s"
                    <| fullPath k
                    yield (k, v)
                    ]
        let parallelOption = ParallelOptions(MaxDegreeOfParallelism = 7)
        printfn "Всего файлов надо скачать: %d" downloadSeq.Length
        Parallel.ForEach(downloadSeq, parallelOption, (fun (file, sum) ->
            Downloads.startDownload file (fullPath file) sum
            |>function
            | Ok(resp) -> printfn "%s" resp
            | Error (err) -> printfn "%s" err))
                
    let checkToDelete =
        for k in mapClient.Keys do
            match mapServer.ContainsKey(k) with
            | true -> printfn "Все хорошо, такой файл есть в мапе сервера."
            | false -> printfn "Такого файла в мапе сервера нет, запускаем функцию удаления в путь %s"
                       <| fullPath k
                       File.Delete(fullPath k)
                       
    checkToDownload |> printfn "%A"
    checkToDelete
            
Downloads.getModmapFromServer()
|>function
|Ok(dict) -> compression dict (seqHASH path)
|Error(err) -> printfn "%s" err