module Launcher.Logic

open System.IO
open System.Collections.Generic
open System.Net.Http
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

module Types =

  type Respons =
    { info: string
      link: string
      status: bool }

  type ResponsStatus =
    { statusline: bool
      information: string }
    
  type Msg =
  | SetDownloadedFile of string option
  | IncreaseDownloadedCounter
  | SetUpdateProgramProcess of string option
  | SetFilesToDownload of int
  | StartUpdate
  | DownloadFinished
  | SwitchDisk
  | StartDownload
  
  type ServerStatus =
    ServerOK
    
  type URL =
    | URL of string
    member self.Value =
      match self with
      | URL value -> value
      
  type Headers =
    |Headers of (string * string) list
    member self.Value =
      match self with
      | Headers value -> value
    
  type Query =
    |Query of (string * string) list
    member self.Value =
      match self with
      | Query value -> value
      
  type HttpMethod =
    | GET
    | POST
    | PUT
    | DELETE
    member self.Value =
      match self with
      | GET -> "GET"
      | POST -> "POST"
      | PUT -> "PUT"
      | DELETE -> "DELETE"


module Server =

  [<Literal>]
  let private serverUrl = @"http://api.juliarepository.space"
  
  [<Literal>]
  let private versionURL = "/1"

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

  let private headers =
    Types.Headers <| [ "Accept", "application/json" ; "Authorization", tokenServer ]
      
  let requestStringResult (url: Types.URL) (method: Types.HttpMethod) (query: Types.Query) (headers: Types.Headers) =
    try
      Http.RequestString(url = url.Value, query = query.Value, headers = headers.Value, httpMethod = method.Value) |> Ok
    with _ as eX ->
      Log.TraceExc
      <| sprintf "Exception when try http request Err:%s Exc:%s" eX.Message eX.StackTrace

      Log.TraceErr
      <| sprintf "Exception when try http request Err:%s" eX.Message

      Error eX.Message
      
  let requestStreamResult (url: Types.URL) (method: Types.HttpMethod) (query: Types.Query) (headers: Types.Headers) =
    try
      Http.RequestStream(url = url.Value, query = query.Value, headers = headers.Value, httpMethod = method.Value) |> Ok
    with _ as eX ->
      Log.TraceExc
      <| sprintf "Exception when try http request Err:%s Exc:%s" eX.Message eX.StackTrace

      Log.TraceErr
      <| sprintf "Exception when try http request Err:%s" eX.Message

      Error eX.Message
      
  let private getYandexLink pathToFile =
    Log.TraceInf "Try get link from yandex disk"
    requestStringResult (Types.URL yandexUrl) Types.GET (Types.Query ["path", pathToFile]) headers     

  let private getDropboxLink pathToFile =
    Log.TraceInf "Try get link from dropbox disk"
    requestStringResult (Types.URL dropboxUrl) Types.GET  (Types.Query ["path", pathToFile]) headers
    
  let getModmapFromServer () =
    
    Log.TraceInf "Try get modmap from server"
    
    let deserializeToDict (result: Result<string, string>) =
      Log.TraceInf "Deserialize modmap"
      match result with
      |Ok respons -> respons |> JsonSerializer.Deserialize<Dictionary<string, string>> |> Ok
      |Error err -> Error err
      
    requestStringResult (Types.URL modmapUrl) Types.GET (Types.Query ["path", "release"]) headers
    |>deserializeToDict

  let getServerStatus ()=
    Log.TraceInf "Try get server status"
    requestStringResult (Types.URL serverStatusURL) Types.GET (Types.Query []) headers
    |>deserializeRespons<Types.ResponsStatus>
    |>function
    |Ok respons ->
      if respons.statusline
        then Ok Types.ServerOK
        else Error respons.information
    |Error err -> Error err


  let renameAndDeleteOlderFiles (dispatch: Types.Msg -> unit) =
    try
      Some "Delete and rename old files..."|> Types.SetUpdateProgramProcess |> dispatch
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
      with _ as eX ->
        Log.TraceExc
        <| sprintf "Exception when try Delete and rename old files Err:%s Exc:%s" eX.Message eX.StackTrace
  
        Log.TraceErr
        <| sprintf "Exception when try Delete and rename old files Err:%s" eX.Message
        
  let private launcherSumFromServer (serverResult: Result<Types.ServerStatus, string>, dispatch: Types.Msg -> unit) =
    Log.TraceInf "Get launcher hash sum from server"
    match serverResult with
    | Ok _ ->
      Some "Get SHA1 sum from server"|> Types.SetUpdateProgramProcess |> dispatch
      Log.TraceInf "Start request for hash sum"
      (requestStringResult (Types.URL LauncherURL) Types.GET (Types.Query ["type", "hash"]) headers), dispatch
    | Error err -> Error err, dispatch
    
  let private compareLaunchersSum (hash, dispatch: Types.Msg -> unit) =
    Log.TraceInf "Compare launchers hash sum"
    match hash with
    | Ok hashServer ->
        Some "Compare launchers hash sum"|> Types.SetUpdateProgramProcess |> dispatch
        use sha = new SHA1Managed()
        let clientSum =
          File.ReadAllBytes(exeFileName)
          |> sha.ComputeHash
          |> convert
        Log.TraceDeb
        <| sprintf "ServerSide HASH: %s ClientSideHASH: %s" hashServer clientSum
        hashServer = clientSum |> Ok, dispatch
    | Error err -> Error err, dispatch
    
  let private launcherStreamHTTP (checkHashResult, dispatch: Types.Msg -> unit) =
    Log.TraceInf "Check for launcher stream from server trough HTTPClient"
    match checkHashResult with
    | Ok check ->
        if not check then
          try
            Some "Download new launcher..."|> Types.SetUpdateProgramProcess |> dispatch
            Log.TraceInf "Start get launcher stream from server trough HTTPClient"
            let url = LauncherURL
            requestStreamResult (Types.URL LauncherURL) Types.GET (Types.Query [ "type", "download" ]) headers
            |>function
            |Ok stream -> stream |> Some |> Ok, dispatch
            |Error err -> Error err, dispatch
          with _ as eX ->
            Log.TraceErr
            <| sprintf "Error when try to get launcher stream from server trough HTTPClient: %s" eX.Message
            Log.TraceExc <| eX.StackTrace
            Error
            <| sprintf "Error when try to get launcher stream from server trough HTTPClient: %s" eX.Message, dispatch
        else
          Log.TraceInf "Update don't required, all ok"
          Ok None, dispatch
    | Error err -> Error err, dispatch
    
  let private downloadLauncher (responsStream: Result<HttpResponseWithStream option, string>, dispatch: Types.Msg -> unit) =
    Log.TraceInf "Check for required download launcher"
    match responsStream with
    | Ok stream ->
        if stream.IsSome then
          Log.TraceInf "Launcher need update, start stream to file..."
          Some "Have new version, start write..." |> Types.SetUpdateProgramProcess |> dispatch
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
    
    
  let checkLauncherUpdate =
    launcherSumFromServer
    >>compareLaunchersSum
    
    
  let updateLauncher =
    launcherStreamHTTP
    >>downloadLauncher
    
  
  

  let private downloadFile (url: string) (downloadPath: string) (sumFromServer: string) (dispatch: Types.Msg -> unit) =

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
      Some path |> Types.SetDownloadedFile |> dispatch
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
        Types.IncreaseDownloadedCounter |> dispatch
        Log.TraceInf
        <| sprintf "Successful download: %s" downloadPath
    | false ->
        Log.TraceErr
        <| sprintf "SHA1 does not match, try again download %s" downloadPath

        ComposeOfDownload downloadPath sumFromServer
        |> sprintf "Status of again download %s: %b" downloadPath
        |> Log.TraceInf

  let startDownload pathToFile fullFilePath summSHA (dispatch: Types.Msg -> unit) =
    Log.TraceInf
    <| sprintf "lets start get link for %s" pathToFile

    getYandexLink pathToFile
    |> deserializeRespons<Types.Respons>
    |> function
    | Ok resp ->
        if resp.status then
          downloadFile resp.link fullFilePath summSHA dispatch
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

  let RealWork (mapServer: Dictionary<string, string>) (mapClient: Dictionary<string, string>) (dispatch: Types.Msg -> unit) =

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
    Types.SetFilesToDownload listForDownload.Length |> dispatch
    Log.TraceInf
    <| sprintf "Files to downloads: %d" listForDownload.Length

    let downloadParallel (filesList: (string * string) list) =
      Some "Download files... " |> Types.SetUpdateProgramProcess |> dispatch
      Types.StartDownload |> dispatch
      Log.TraceInf
      <| sprintf "Start downloads, num of maximum threads: %d" 7

      let parallelOption =
        ParallelOptions(MaxDegreeOfParallelism = 7)

      Parallel.ForEach(
        filesList,
        parallelOption,
        (fun (file, sum) ->
          Server.startDownload file (fullPath file) sum dispatch
          |> function
          | Ok resp -> Log.TraceInf <| sprintf "%s" resp
          | Error err -> Log.TraceErr <| sprintf "%s" err)
      )

    let deleteFiles () =
      Log.TraceInf "Check files to delete"
      Some "Delete old files... " |> Types.SetUpdateProgramProcess |> dispatch
      for k in mapClient.Keys do
        match mapServer.ContainsKey(k) with
        | true ->
            Log.TraceInf
            <| sprintf "File %s... ok, file exists in server modmap" k
        | false ->
            Log.TraceInf
            <| sprintf "File %s... no found in server modmap, delete in: %s" k (fullPath k)
            sprintf "Delete: %s" (fullPath k)
            |>Some |> Types.SetDownloadedFile |> dispatch
            File.Delete(fullPath k)

    downloadParallel listForDownload |> ignore
    deleteFiles ()

