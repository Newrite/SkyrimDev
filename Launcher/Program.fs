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

  let deserializeFromFile fileName =
    Log.TraceInf
    <| sprintf "Deserialize from file: %s" fileName

    File.ReadAllText(fileName)
    |> JsonSerializer.Deserialize<Dictionary<string, string>>

  let deserializeRespons<'a> respons =
    Log.TraceInf
    <| sprintf "Deserialize respons: %A" respons

    match respons with
    | Ok ok ->
        try

          Json.deserialize<'a> ok |> Ok
        with :? JsonDeserializationError as eX ->
          Log.TraceExc
          <| sprintf "Deserialize error Err:%s %s" eX.Message eX.StackTrace

          Log.TraceErr
          <| sprintf "Deserialize error Err:%s" eX.Message

          Error
          <| sprintf "Deserialize error Err:%s" eX.Message
    | Error err ->

        Error err


module Server =

  [<Literal>]
  let private serverUrl = @"http://api.juliarepository.space"

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
    Log.TraceInf "Try get link from yandex disk"

    try
      let url = yandexUrl

      Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "path", pathToFile ], headers = headers ())
      |> Ok
    with :? WebException as eX ->
      Log.TraceExc
      <| sprintf "Can't get link from yandex disk Err:%s %s" eX.Message eX.StackTrace

      Log.TraceErr
      <| sprintf "Can't get link from yandex disk Err:%s" eX.Message

      Error
      <| sprintf "Can't get link from yandex disk Err:%s" eX.Message

  let private getDropboxLink pathToFile =
    Log.TraceInf "Try get link from dropbox disk"

    try
      let url = dropboxUrl

      Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "path", pathToFile ], headers = headers ())
      |> Ok
    with :? WebException as eX ->
      Log.TraceExc
      <| sprintf "Can't get link from dropbox disk Err:%s %s" eX.Message eX.StackTrace

      Log.TraceErr
      <| sprintf "Can't get link from dropbox disk Err:%s" eX.Message

      Error
      <| sprintf "Can't get link from dropbox disk Err:%s" eX.Message

  let getModmapFromServer () =
    Log.TraceInf "Try get modmap from server"

    let deserialize (text: string) =
      Log.TraceInf "Deserialize modmap"

      text
      |> JsonSerializer.Deserialize<Dictionary<string, string>>

    try
      let url = modmapUrl

      Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "version", "release" ], headers = headers ())
      |> deserialize
      |> Ok
    with :? WebException as eX ->
      Log.TraceExc
      <| sprintf "Can't get modmap from server Err:%s %s" eX.Message eX.StackTrace

      Log.TraceErr
      <| sprintf "Can't get modmap from server Err:%s" eX.Message

      Error
      <| sprintf "Can't get modmap from server Err:%s" eX.Message

  let getServerStatus () =
    Log.TraceInf "Try get server status"

    try
      let url = serverStatusURL

      Http.RequestString(url, httpMethod = HttpMethod.Get, query = [], headers = headers ())
      |> Ok
    with :? WebException as eX ->
      Log.TraceExc
      <| sprintf "Can't get server status Err:%s %s" eX.Message eX.StackTrace

      Log.TraceErr
      <| sprintf "Can't get server status Err:%s" eX.Message

      Error
      <| sprintf "Can't get server status Err:%s" eX.Message


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

  let private downloadFile (url: string) (downloadPath: string) (sumFromServer: string) =

    Log.TraceInf
    <| sprintf "Get link, start download file: %s" downloadPath

    let checkSum firstSum secondSum =
      Log.TraceInf "Start check files sum..."
      firstSum = secondSum

    let directoryPath (filePath: string) =
      let i = filePath.LastIndexOf('\\')
      filePath.Substring(0, i)

    let createDirectory path =
      Directory.CreateDirectory(path)
      |> sprintf "Create directory: %A"
      |> Log.TraceInf

      downloadPath

    let startDownloadFile path =
      use wc = new WebClient()

      Log.TraceInf
      <| sprintf "Start download file: %s.temp" path

      wc.DownloadFile(System.Uri(url), path + ".temp")
      Log.TraceInf <| sprintf "Delete old file: %s" path
      File.Delete(path)

      Log.TraceInf
      <| sprintf "Rename new file: %s.temp -> %s" path path

      File.Move(path + ".temp", path)
      path

    let GetDownloadFileSum path =
      use sha = new SHA1Managed()

      Log.TraceInf
      <| sprintf "Get new file SHA1: %s" path

      convert
      <| sha.ComputeHash(File.ReadAllBytes(path))

    let ComposeOfDownload =
      directoryPath
      >> createDirectory
      >> startDownloadFile
      >> GetDownloadFileSum
      >> checkSum

    match ComposeOfDownload downloadPath sumFromServer with
    | true ->
        Log.TraceInf
        <| sprintf "Successful download: %s" downloadPath
    | false ->
        Log.TraceErr
        <| sprintf "SHA1 does not match, try again download %s" downloadPath

        ComposeOfDownload downloadPath sumFromServer
        |> sprintf "Status of again download %s: %b" downloadPath
        |> Log.TraceInf

  let startDownload pathToFile fullFilePath summSHA =
    Log.TraceInf
    <| sprintf "lets start get link for %s" pathToFile

    getYandexLink pathToFile
    |> SerDeser.deserializeRespons<Types.Respons>
    |> function
    | Ok resp ->
        if resp.status then
          downloadFile resp.link fullFilePath summSHA
          Ok "Go download another file"
        else
          Error
          <| sprintf "Problem with get link from disk, server info: %s" resp.info
    | Error err -> Error err


module Launcher =
  let private files sPath =
    Log.TraceInf "Get all files in mods\\!PS"

    let rec filesUnder (basePath: string) =
      seq {
        if basePath.Contains("mods\\!PS") then
          yield! Directory.GetFiles(basePath)

        for subDir in Directory.GetDirectories(basePath) do
          yield! filesUnder subDir
      }

    filesUnder sPath

  let modmapClient sPath =
    Log.TraceInf "Create client modmap"
    let dict = Dictionary<string, string>()
    use sha = new SHA1Managed()

    files sPath
    |> Seq.map (fun x -> (x, File.ReadAllBytes(x)))
    |> Seq.iter (fun (x, y) -> dict.Add(x.Replace(sPath, "").Replace('\\', '/'), (convert <| sha.ComputeHash(y))))

    Log.TraceInf "Alright, we get SHA1 of all files"
    dict

  let RealWork (mapServer: Dictionary<string, string>) (mapClient: Dictionary<string, string>) =

    Log.TraceInf "Real work starts now"

    let fullPath (sKey: string) = currentPath + sKey.Replace('/', '\\')

    let listForDownload =
      [ for KeyValue (k, v) in mapServer do
          match mapClient.ContainsKey(k) with
          | true ->
              let check = v = mapClient.[k]

              if check then
                Log.TraceInf <| sprintf "File %s... Ok" k
              else
                Log.TraceInf
                <| sprintf "File %s... sum dosent match, will download in %s" k (fullPath k)

                yield (k, v)
          | false ->
              Log.TraceInf
              <| sprintf "File %s... don't found, will download in %s" k (fullPath k)

              yield (k, v) ]

    Log.TraceInf
    <| sprintf "Files to downloads: %d" listForDownload.Length

    let downloadParallel (filesList: (string * string) list) =
      Log.TraceInf
      <| sprintf "Start downloads, num of maximum threads: %d" 7

      let parallelOption =
        ParallelOptions(MaxDegreeOfParallelism = 7)

      Parallel.ForEach(
        filesList,
        parallelOption,
        (fun (file, sum) ->
          Server.startDownload file (fullPath file) sum
          |> function
          | Ok resp -> Log.TraceInf <| sprintf "%s" resp
          | Error err -> Log.TraceErr <| sprintf "%s" err)
      )

    let deleteFiles () =
      Log.TraceInf "Check files to delete"

      for k in mapClient.Keys do
        match mapServer.ContainsKey(k) with
        | true ->
            Log.TraceInf
            <| sprintf "File %s... ok, file exists in server modmap" k
        | false ->
            Log.TraceInf
            <| sprintf "File %s... no found in server modmap, delete in: %s" k (fullPath k)

            File.Delete(fullPath k)

    downloadParallel listForDownload |> ignore
    deleteFiles ()

[<EntryPoint>]
let main _ =
  Log.TraceDeb
  <| sprintf "Start info currentPath:%s exeFileName: %s" currentPath exeFileName
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
          | Ok modmapServer ->
              Launcher.RealWork modmapServer (Launcher.modmapClient currentPath)
              0
          | Error err ->
              printfn "%s" err
              System.Console.ReadKey() |> ignore
              1
      | false -> 2
  | Error err ->
      printfn "%s" err
      System.Console.ReadKey() |> ignore
      3
