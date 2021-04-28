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

module Utils =
  let currentPath = Directory.GetCurrentDirectory()

  let exeFileName =
    let pathToFile = Assembly.GetExecutingAssembly().Location
    let i = pathToFile.LastIndexOf('\\')

    pathToFile
      .Substring(i + 1)
      .Replace(".dll", ".exe")

  let getFileNameFromPath (path: string) =
    path.LastIndexOf('\\') |> path.Substring

  let private directoryPath (fullPathToFile: string) =
    let i = fullPathToFile.LastIndexOf('\\')
    fullPathToFile.Substring(0, i)

  let private createDirectory fullPathToFile =
    Directory.CreateDirectory(fullPathToFile)
    |> sprintf "Create directory: %A"
    |> Log.TraceInf

  let fullPath (pathToFile: string) =
    currentPath + pathToFile.Replace('/', '\\')

  let createDirectoryIfNotExist = directoryPath >> createDirectory

  let convertByteArrayToString (byteArray: byte []) =

    byteArray
    |> Array.fold (fun acc elem -> acc + elem.ToString()) ""

  let jsonResponsToType<'a> respons =
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
    | Error err -> Error err

  let requestStringResult (url: URL) (method: HttpMethodAlias) (query: Query) (headers: Headers) =
    try
      Http.RequestString(url = url.Value, query = query.Value, headers = headers.Value, httpMethod = method.Value)
      |> Ok
    with eX ->
      Log.TraceExc
      <| sprintf "Exception when try http request Err:%s Exc:%s" eX.Message eX.StackTrace

      Log.TraceErr
      <| sprintf "Exception when try http request Err:%s" eX.Message

      Error eX.Message

  let requestStreamResult (url: URL) (method: HttpMethodAlias) (query: Query) (headers: Headers) =
    try
      Http.RequestStream(url = url.Value, query = query.Value, headers = headers.Value, httpMethod = method.Value)
      |> Ok
    with eX ->
      Log.TraceExc
      <| sprintf "Exception when try http request Err:%s Exc:%s" eX.Message eX.StackTrace

      Log.TraceErr
      <| sprintf "Exception when try http request Err:%s" eX.Message

      Error eX.Message

module Server =

  [<Literal>]
  let serverUrl = @"http://api.juliarepository.space"

  [<Literal>]
  let versionURL = "/1"

  [<Literal>]
  let dropboxUrl = serverUrl + "/dropbox"

  [<Literal>]
  let yandexUrl = serverUrl + "/yandex"

  [<Literal>]
  let modmapUrl = serverUrl + "/modmap"

  [<Literal>]
  let serverStatusURL = serverUrl + "/status"

  [<Literal>]
  let LauncherURL = serverUrl + "/launcher"

  [<Literal>]
  let tokenServer = "Bearer OMEGALULISPOWER!"

  let headers =
    Headers
    <| [ "Accept", "application/json"
         "Authorization", tokenServer ]

  let getYandexLink pathToFile =
    Log.TraceInf "Try get link from yandex disk"
    Utils.requestStringResult (URL yandexUrl) GET (Query [ "path", pathToFile ]) headers

  let getDropboxLink pathToFile =
    Log.TraceInf "Try get link from dropbox disk"
    Utils.requestStringResult (URL dropboxUrl) GET (Query [ "path", pathToFile ]) headers

  let getModmapFromServer () =

    Log.TraceInf "Try get modmap from server"

    let deserializeToDict (result: Result<string, string>) =
      Log.TraceInf "Deserialize modmap"

      match result with
      | Ok respons ->
          respons
          |> JsonSerializer.Deserialize<Dictionary<string, string>>
          |> Ok
      | Error err -> Error err

    Utils.requestStringResult (URL modmapUrl) GET (Query [ "version", "release" ]) headers
    |> deserializeToDict

  let getServerStatus () =
    Log.TraceInf "Try get server status"

    Utils.requestStringResult (URL serverStatusURL) GET (Query []) headers
    |> Utils.jsonResponsToType<ResponsStatus>
    |> function
    | Ok respons ->
        if respons.statusline then
          Ok ServerOK
        else
          Error respons.information
    | Error err -> Error err

module UpdateLauncher =

  let renameAndDeleteOldLauncherFiles (dispatch: Message -> unit) =
    try
      Some "Delete and rename old files..."
      |> SetCurrentProgramProcess
      |> dispatch

      if
        (File.Exists(Utils.exeFileName) |> not)
        && File.Exists(Utils.exeFileName + ".temp")
      then
        Log.TraceDeb
        <| sprintf "Rename %s to %s" Utils.exeFileName Utils.exeFileName
           + ".temp"

        File.Move(Utils.exeFileName + ".temp", Utils.exeFileName)

      if File.Exists(Utils.exeFileName + ".temp") then
        Log.TraceDeb
        <| sprintf "Delete %s" Utils.exeFileName + ".temp"

        File.Delete(Utils.exeFileName + ".temp")

      if File.Exists(Utils.exeFileName + ".delete") then
        Log.TraceDeb
        <| sprintf "Delete %s" Utils.exeFileName + ".delete"

        File.Delete(Utils.exeFileName + ".delete")
    with eX ->
      Log.TraceExc
      <| sprintf "Exception when try Delete and rename old files Err:%s Exc:%s" eX.Message eX.StackTrace

      Log.TraceErr
      <| sprintf "Exception when try Delete and rename old files Err:%s" eX.Message

  let private launcherSumFromServer (serverResult: Result<ServerStatus, string>, dispatch: Message -> unit) =
    Log.TraceInf "Get launcher hash sum from server"

    match serverResult with
    | Ok _ ->
        Some "Get SHA1 sum from server"
        |> SetCurrentProgramProcess
        |> dispatch

        Log.TraceInf "Start request for hash sum"
        (Utils.requestStringResult (URL Server.LauncherURL) GET (Query [ "type", "hash" ]) Server.headers), dispatch
    | Error err -> Error err, dispatch

  let private compareLaunchersSum (hash, dispatch: Message -> unit) =
    Log.TraceInf "Compare launchers hash sum"

    match hash with
    | Ok hashServer ->
        Some "Compare launchers hash sum"
        |> SetCurrentProgramProcess
        |> dispatch

        use sha = new SHA1Managed()

        let clientSum =
          File.ReadAllBytes(Utils.exeFileName)
          |> sha.ComputeHash
          |> Utils.convertByteArrayToString

        Log.TraceDeb
        <| sprintf "ServerSide HASH: %s ClientSideHASH: %s" hashServer clientSum

        hashServer = clientSum |> Ok, dispatch
    | Error err -> Error err, dispatch

  let private launcherStreamHTTP (checkHashResult, dispatch: Message -> unit) =
    Log.TraceInf "Check for launcher stream from server trough HTTPClient"

    match checkHashResult with
    | Ok check ->
        if not check then
          try
            Some "Download new launcher..."
            |> SetCurrentProgramProcess
            |> dispatch

            Log.TraceInf "Start get launcher stream from server trough HTTPClient"

            let url = Server.LauncherURL

            Utils.requestStreamResult (URL Server.LauncherURL) GET (Query [ "type", "download" ]) Server.headers
            |> function
            | Ok stream -> stream |> Some |> Ok, dispatch
            | Error err -> Error err, dispatch
          with eX ->
            Log.TraceErr
            <| sprintf "Error when try to get launcher stream from server trough HTTPClient: %s" eX.Message

            Log.TraceExc <| eX.StackTrace

            Error
            <| sprintf "Error when try to get launcher stream from server trough HTTPClient: %s" eX.Message,
            dispatch
        else
          Log.TraceInf "Update don't required, all ok"
          Ok None, dispatch
    | Error err -> Error err, dispatch

  let private downloadLauncher
    (
      responsStream: Result<HttpResponseWithStream option, string>,
      dispatch: Message -> unit
    ) =
    Log.TraceInf "Check for required download launcher"

    match responsStream with
    | Ok stream ->
        if stream.IsSome then
          Log.TraceInf "Launcher need update, start stream to file..."

          Some "Have new version, start write..."
          |> SetCurrentProgramProcess
          |> dispatch

          use outputFile =
            new FileStream(Utils.exeFileName + ".temp", FileMode.Create)

          stream.Value.ResponseStream.CopyTo(outputFile)
          outputFile.Flush()
          outputFile.Dispose()
          outputFile.Close()
          File.Move(Utils.exeFileName, Utils.exeFileName + ".delete")
          Log.TraceInf "Launcher successful update, it start new version automatically"

          Process.Start(Utils.exeFileName + ".temp")
          |> sprintf "Start new process: %A"
          |> Log.TraceInf

          Ok false
        else
          Log.TraceInf "Download not required, all ok"
          Ok true
    | Error err -> Error err


  let checkLauncherUpdate =
    launcherSumFromServer >> compareLaunchersSum


  let updateLauncher = launcherStreamHTTP >> downloadLauncher



module Download =

  let private getFileStream (dispatch: Message -> unit) (link: Result<string, string>) =
    match link with
    | Ok href ->
        Ok
        <| Utils.requestStreamResult (URL href) GET (Query []) (Headers [])
    | Error err -> Error err

  let private checkFilesSum sum (dispatch: Message -> unit) (pathResult: Result<string, string>) =
    match pathResult with
    | Ok path ->
        use sha = new SHA1Managed()

        Log.TraceInf
        <| sprintf "Get new file SHA1: %s" path

        let sumSnd =
          Utils.convertByteArrayToString
          <| sha.ComputeHash(File.ReadAllBytes(path))

        sum = sumSnd |> ignore
        Ok()
    | Error err -> Error err

  let private writeFileFromStream
    fullPathToFile
    (dispatch: Message -> unit)
    (responsStream: Result<HttpResponseWithStream, string>)
    =
    match responsStream with
    | Ok stream ->

        use outputFile =
          new FileStream(fullPathToFile, FileMode.Create)

        stream.ResponseStream.CopyTo(outputFile)
        outputFile.Flush()
        outputFile.Dispose()
        outputFile.Close()
        Ok fullPathToFile
    | Error err -> Error err

  let private getLinkFromDisk pathToFile (dispatch: Message -> unit) (getDiskLink: string -> Result<string, string>) =
    getDiskLink pathToFile
    |> Utils.jsonResponsToType<Respons>
    |> function
    | Ok resp ->
        if resp.status then
          Ok resp.link
        else
          Error
          <| sprintf "Problem with get link from disk, server info: %s" resp.info
    | Error err -> Error err

  let private resolveFileStream
    (dispatch: Message -> unit)
    (resp: Result<Result<HttpResponseWithStream, string>, string>)
    =
    match resp with
    | Ok streamResult ->
        match streamResult with
        | Ok stream -> Ok stream
        | Error err -> Error err
    | Error err -> Error err

  let private fullDownloadProcess pathToFile fullPathToFile (dispatch: Message -> unit) fileSum =
    getLinkFromDisk pathToFile dispatch
    >> getFileStream dispatch
    >> resolveFileStream dispatch
    >> writeFileFromStream fullPathToFile dispatch
    >> checkFilesSum fileSum dispatch

  let startDownload pathToFile fullPathToFile fileSum (disk: Disk) (dispatch: Message -> unit) =
    match disk with
    | DropboxDisk -> fullDownloadProcess pathToFile fullPathToFile dispatch fileSum Server.getDropboxLink
    | YandexDisk -> fullDownloadProcess pathToFile fullPathToFile dispatch fileSum Server.getYandexLink


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

  let private modmapClient sPath =
    Log.TraceInf "Create client modmap"
    let dict = Dictionary<string, string>()
    use sha = new SHA1Managed()

    files sPath
    |> Seq.map (fun x -> (x, File.ReadAllBytes(x)))
    |> Seq.iter
         (fun (x, y) ->
           dict.Add(
             x.Replace(sPath, "").Replace('\\', '/'),
             (Utils.convertByteArrayToString
              <| sha.ComputeHash(y))
           ))

    Log.TraceInf "Alright, we get SHA1 of all files"
    dict


  let private getFilesForDownload
    (mapClient: Dictionary<string, string>)
    (mapServer: Dictionary<string, string>)
    (dispatch: Message -> unit)
    =

    [ for KeyValue (k, v) in mapServer do
        match mapClient.ContainsKey(k) with
        | true ->

            if v = mapClient.[k] then
              Log.TraceInf <| sprintf "File %s... Ok" k
            else
              Log.TraceInf
              <| sprintf "File %s... sum dosent match, will download in %s" k (Utils.fullPath k)

              yield (k, v)
        | false ->
            Log.TraceInf
            <| sprintf "File %s... don't found, will download in %s" k (Utils.fullPath k)

            yield (k, v) ]

  let private getFilesForDelete
    (mapClient: Dictionary<string, string>)
    (mapServer: Dictionary<string, string>)
    (dispatch: Message -> unit)
    =
    Log.TraceInf "Check files to delete"

    Some "Delete old files... "
    |> SetCurrentProgramProcess
    |> dispatch

    [ for KeyValue (k, v) in mapClient do
        match mapServer.ContainsKey(k) with
        | true ->
            Log.TraceInf
            <| sprintf "File %s... ok, file exists in server modmap" k
        | false -> yield k ]

  let private deleteOutdateFiles (deleteList: string list) (dispatch: Message -> unit) =
    Log.TraceInf "Check files to delete"

    Some "Delete old files... "
    |> SetCurrentProgramProcess
    |> dispatch

    for path in deleteList do
      Log.TraceInf
      <| sprintf "File %s... no found in server modmap, delete in: %s" path (Utils.fullPath path)

      sprintf "Delete: %s" (Utils.fullPath path)
      |> Some
      |> SetCurrentDownloadFile
      |> dispatch

      File.Delete(Utils.fullPath path)
      
  let filesCheck (mapServer: Dictionary<string, string>) (dispatch: Message -> unit) =
    let mapClient = modmapClient Utils.currentPath

    let listForDownload =
      getFilesForDownload mapClient mapServer dispatch

    let listForDelete =
      getFilesForDelete mapClient mapServer dispatch
      
    listForDownload.Length, listForDelete.Length

  let updateMods (mapServer: Dictionary<string, string>) (disk: Disk) (dispatch: Message -> unit) =

    Log.TraceInf "Real work starts now"

    let mapClient = modmapClient Utils.currentPath

    let listForDownload =
      getFilesForDownload mapClient mapServer dispatch

    let listForDelete =
      getFilesForDelete mapClient mapServer dispatch

    SetFilesToDownload listForDownload.Length
    |> dispatch

    Log.TraceInf
    <| sprintf "Files to downloads: %d" listForDownload.Length

    let downloadParallel (filesList: (string * string) list) =
      Some "Download files... "
      |> SetCurrentProgramProcess
      |> dispatch

      StartDownload |> dispatch

      Log.TraceInf
      <| sprintf "Start downloads, num of maximum threads: %d" 7

      let parallelOption =
        ParallelOptions(MaxDegreeOfParallelism = 7)

      Parallel.ForEach(
        filesList,
        parallelOption,
        (fun (file, sum) ->
          Utils.createDirectoryIfNotExist (Utils.fullPath file)

          Download.startDownload file (Utils.fullPath file) sum disk dispatch
          |> function
          | Ok _ -> Log.TraceInf <| sprintf "Successful download"
          | Error err -> Log.TraceErr <| sprintf "%s" err)
      )

    downloadParallel listForDownload |> ignore
    deleteOutdateFiles listForDelete dispatch
