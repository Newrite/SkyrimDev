open System.IO
open System.Collections.Generic
open System.Security.Cryptography
open System.Net
open FSharp.Data
open FSharp.Json
open System.Text.Json
open System.Threading.Tasks
open System.Reflection
open System.Diagnostics
open CLogger

let currentPath = Directory.GetCurrentDirectory()

let exeFileName =
  let pathToFile = Assembly.GetExecutingAssembly().Location
  let i = pathToFile.LastIndexOf('\\')

  pathToFile
    .Substring(i + 1)
    .Replace(".dll", ".exe")

let convert (byteArray: byte []) =
  byteArray
  |> Array.fold (fun acc elem -> acc + elem.ToString()) ""

module Types =

  type Respons =
    { info: string
      link: string
      status: bool }

  type ResponsStatus =
    { statusline: bool
      information: string }

module SerDeser =

  let serializeToFile (d: Dictionary<string, string>) fileName =
    Log.TraceErr "Error!"

    let opt =
      JsonSerializerOptions(WriteIndented = true)

    use writer =
      File.Open(fileName, FileMode.Create)
      |> StreamWriter

    JsonSerializer.Serialize(d, opt) |> writer.Write
    writer.Flush()

  let deserializeFromFile fileName =
    File.ReadAllText(fileName)
    |> JsonSerializer.Deserialize<Dictionary<string, string>>

  let deserializeRespons<'a> respons =
    printfn "%A" respons

    match respons with
    | Ok (ok) ->
        try

          Json.deserialize<'a> ok |> Ok
        with :? JsonDeserializationError as eX ->

          Error
          <| sprintf "deserializeRespons<'a> JsonDeserializationError Err:%s P: %A" eX.Message respons
    | Error (err) ->

        Error err


module Server =

  [<Literal>]
  let private serverUrl = @"http://localhost:3030"

  [<Literal>]
  let private dropboxUrl = serverUrl + "/dropbox"

  [<Literal>]
  let private yandexUrl = serverUrl + "/yandex"

  [<Literal>]
  let private modmapUrl = serverUrl + "/modmap"

  [<Literal>]
  let private serverStatusURL = serverUrl + "/status"

  [<Literal>]
  let private LauncherURL = serverUrl + "/launcher"

  [<Literal>]
  let private tokenServer = "Bearer OMEGALULISPOWER!"

  let private headers () =
    [ "Accept", "application/json"
      "Authorization", tokenServer ]

  let private getYandexLink pathToFile =

    try
      let url = yandexUrl

      Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "path", pathToFile ], headers = headers ())
      |> Ok
    with :? WebException as eX ->

      Error <| sprintf "%s" eX.Message

  let private getDropboxLink pathToFile =

    try
      let url = dropboxUrl

      Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "path", pathToFile ], headers = headers ())
      |> Ok
    with :? WebException as eX ->

      Error <| sprintf "%s" eX.Message

  let getModmapFromServer () =
    let deserialize (text: string) =
      text
      |> JsonSerializer.Deserialize<Dictionary<string, string>>

    try
      let url = modmapUrl

      Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "version", "release" ], headers = headers ())
      |> deserialize
      |> Ok
    with :? WebException as eX ->

      Error <| sprintf "%s" eX.Message

  let getServerStatus () =

    try
      let url = serverStatusURL

      Http.RequestString(url, httpMethod = HttpMethod.Get, query = [], headers = headers ())
      |> Ok
    with :? WebException as eX ->

      Error <| sprintf "%s" eX.Message


  let updateLauncher serverRespons =

    Log.TraceInf "Start check launcher for update"

    let renameAndDeleteOlderFiles () =
      if
        (File.Exists(exeFileName) |> not)
        && File.Exists(exeFileName + ".temp")
      then
        Log.TraceDeb
        <| sprintf "Rename %s to %s" exeFileName exeFileName
           + ".temp"

        File.Move(exeFileName + ".temp", exeFileName)

      if File.Exists(exeFileName + ".temp") then
        Log.TraceDeb
        <| sprintf "Delete %s" exeFileName + ".temp"

        File.Delete(exeFileName + ".temp")

      if File.Exists(exeFileName + ".delete") then
        Log.TraceDeb
        <| sprintf "Delete %s" exeFileName + ".delete"

        File.Delete(exeFileName + ".delete")

    let GetLauncherSumFromServer (serverResult: Result<Types.ResponsStatus, string>) =

      Log.TraceInf "Get launcher hash sum from server"

      match serverResult with
      | Ok respons ->
          if respons.statusline then
            try
              Log.TraceInf "Start request for hash sum"
              let url = LauncherURL

              Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "type", "hash" ], headers = headers ())
              |> Ok
            with :? WebException as eX ->

              Log.TraceErr
              <| sprintf "Error when try to get launcher hash from server: %s" eX.Message

              Log.TraceExc <| eX.StackTrace

              Error
              <| sprintf "Error when try to get launcher hash from server: %s" eX.Message
          else
            Log.TraceErr <| sprintf "Server send he not OK"
            Error "Server send he not OK"
      | Error err -> Error err

    let compareLaunchersSum hash =

      Log.TraceInf "Compare launchers hash sum"

      match hash with
      | Ok hashServer ->
          use sha = new SHA1Managed()

          let clientSum =
            File.ReadAllBytes(exeFileName)
            |> sha.ComputeHash
            |> convert

          Log.TraceDeb
          <| sprintf "ServerSide HASH: %s ClientSideHASH: %s" hashServer clientSum

          hashServer = clientSum |> Ok
      | Error err -> Error err

    let GetLauncherStreamHTTP checkHashResult =

      Log.TraceInf "Check for launcher stream from server trough HTTPClient"

      match checkHashResult with
      | Ok check ->
          if not check then
            try
              Log.TraceInf "Start get launcher stream from server trough HTTPClient"
              let url = LauncherURL

              Http.RequestStream(url, httpMethod = HttpMethod.Get, query = [ "type", "download" ], headers = headers ())
              |> Some
              |> Ok
            with :? WebException as eX ->
              Log.TraceErr
              <| sprintf "Error when try to get launcher stream from server trough HTTPClient: %s" eX.Message

              Log.TraceExc <| eX.StackTrace

              Error
              <| sprintf "Error when try to get launcher stream from server trough HTTPClient: %s" eX.Message
          else
            Log.TraceInf "Update don't required, all ok"
            Ok None
      | Error err -> Error err


    let TryDownloadLauncher (responsStream: Result<HttpResponseWithStream option, string>) =

      Log.TraceInf "Check for required download launcher"

      match responsStream with
      | Ok stream ->
          if stream.IsSome then
            Log.TraceInf "Launcher need update, start stream to file..."

            use outputFile =
              new FileStream(exeFileName + ".temp", FileMode.Create)

            stream.Value.ResponseStream.CopyTo(outputFile)
            outputFile.Flush()
            outputFile.Dispose()
            outputFile.Close()
            File.Move(exeFileName, exeFileName + ".delete")
            Log.TraceInf "Launcher successful update, it start new version automatically"

            Process.Start(exeFileName + ".temp")
            |> sprintf "Start new process: %A"
            |> Log.TraceInf

            Ok false
          else
            Log.TraceInf "Download not required, all ok"
            Ok true
      | Error err -> Error err

    renameAndDeleteOlderFiles ()

    GetLauncherSumFromServer serverRespons
    |> compareLaunchersSum
    |> GetLauncherStreamHTTP
    |> TryDownloadLauncher

  let private downloadFile (url: string) (toPath: string) (sumSHA: string) =
    use sha = new SHA1Managed()

    let pathWithoutFile =
      let i = toPath.LastIndexOf('\\')
      toPath.Substring(0, i)

    Directory.CreateDirectory(pathWithoutFile)
    |> printfn "%A"

    use wc = new WebClient()
    wc.DownloadFile(System.Uri(url), toPath + ".temp")
    File.Delete(toPath)
    File.Move(toPath + ".temp", toPath)

    let newSumSHA =
      convert
      <| sha.ComputeHash(File.ReadAllBytes(toPath))

    if newSumSHA <> sumSHA then
      printfn "ALARM bad download"

  let startDownload pathToFile fullFilePath summSHA =
    getYandexLink pathToFile
    |> SerDeser.deserializeRespons<Types.Respons>
    |> function
    | Ok (resp) ->
        if resp.status then
          downloadFile resp.link fullFilePath summSHA
          Ok "Download successful"
        else
          Error "Status not ok"
    | Error (err) -> Error err


module Launcher =
  let private files sPath =
    let rec filesUnder (basePath: string) =
      seq {
        if basePath.Contains("mods\\!PS") then
          yield! Directory.GetFiles(basePath)

        for subDir in Directory.GetDirectories(basePath) do
          yield! filesUnder subDir
      }

    filesUnder sPath

  let modmapClient sPath =
    let dict = Dictionary<string, string>()
    use sha = new SHA1Managed()

    files sPath
    |> Seq.map (fun x -> (x, File.ReadAllBytes(x)))
    |> Seq.iter (fun (x, y) -> dict.Add(x.Replace(sPath, "").Replace('\\', '/'), (convert <| sha.ComputeHash(y))))

    dict

  let compression (mapServer: Dictionary<string, string>) (mapClient: Dictionary<string, string>) =

    let fullPath (sKey: string) = currentPath + sKey.Replace('/', '\\')

    let checkToDownload =
      let downloadSeq =
        [ for KeyValue (k, v) in mapServer do
            match mapClient.ContainsKey(k) with
            | true ->
                let check = v = mapClient.[k]

                if check then
                  printfn "Все в порядке"
                else
                  printfn "Сумма файлов несовпадает. Вызываем асинхронную функцию на скачивание в директорию %s"
                  <| fullPath k

                  yield (k, v)
            | false ->
                printfn "Такого файла нет. Добавляем в пул на скачивание в директорию %s"
                <| fullPath k

                yield (k, v) ]

      let parallelOption =
        ParallelOptions(MaxDegreeOfParallelism = 7)

      printfn "Всего файлов надо скачать: %d" downloadSeq.Length

      Parallel.ForEach(
        downloadSeq,
        parallelOption,
        (fun (file, sum) ->
          Server.startDownload file (fullPath file) sum
          |> function
          | Ok (resp) -> printfn "%s" resp
          | Error (err) -> printfn "%s" err)
      )

    let checkToDelete =
      for k in mapClient.Keys do
        match mapServer.ContainsKey(k) with
        | true -> printfn "Все хорошо, такой файл есть в мапе сервера."
        | false ->
            printfn "Такого файла в мапе сервера нет, запускаем функцию удаления в путь %s"
            <| fullPath k

            File.Delete(fullPath k)

    checkToDownload |> printfn "%A"
    checkToDelete

[<EntryPoint>]
let main _ =
  System.Threading.Thread.Sleep(3000)

  Server.getServerStatus ()
  |> SerDeser.deserializeRespons<Types.ResponsStatus>
  |> Server.updateLauncher
  |> function
  | Ok status ->
      match status with
      | true ->
          Server.getModmapFromServer ()
          |> function
          | Ok (modmapServer) ->
              Launcher.compression modmapServer (Launcher.modmapClient currentPath)
              0
          | Error (err) ->
              printfn "%s" err
              System.Console.ReadKey() |> ignore
              1
      | false -> 2
  | Error (err) ->
      printfn "%s" err
      System.Console.ReadKey() |> ignore
      3
