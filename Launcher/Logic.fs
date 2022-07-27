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
open Launcher.Config


module Utils =
  
  let SetCPP (dispatch: Message -> unit) processName =
    processName
    |> Some
    |> SetCurrentProgramProcess
    |> dispatch

  let currentPath() = Directory.GetCurrentDirectory()

  let exeFileName =
    let pathToFile = Assembly.GetExecutingAssembly().Location
    let i = pathToFile.LastIndexOf('\\')

    //pathToFile
    //  .Substring(i + 1)
    //  .Replace(".dll", ".exe")
    pathToFile.Replace(".dll", ".exe")

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
    currentPath() + pathToFile.Replace('/', '\\')

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
      
      
  let tryes (func: 'a -> 'b) (data:#_) =
    try
      Ok <| func data
    with _ as eX ->
      Log.TraceExc <| sprintf "%s" eX.StackTrace
      Log.TraceErr <| sprintf "%s" eX.Message
      Error <| eX.Message
      
      
  tryes jsonResponsToType<Dictionary<string, string>> (Ok("ok")) |> ignore
  tryes createDirectoryIfNotExist @"E:\foo" |> ignore
  
  let inline (^) f x = f (lazy(x))
  
  let tryes2 (func: Lazy<'a>) =
    try
      Ok <| func.Force()
    with _ as eX ->
      Log.TraceExc <| sprintf "%s" eX.StackTrace
      Log.TraceErr <| sprintf "%s" eX.Message
      Error <| eX.Message
      
  tryes2 ^ Http.RequestStream("", [], [], "") |> ignore
  
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

(*  [<Literal>]
  let serverUrl = @"https://api.juliarepository.space"

  [<Literal>]
  let versionURL = "/1"

  [<Literal>]
  let dropboxUrl = serverUrl + versionURL + "/dropbox"

  [<Literal>]
  let yandexUrl = serverUrl + versionURL + "/yandex"

  [<Literal>]
  let modmapUrl = serverUrl + versionURL + "/modmap"

  [<Literal>]
  let serverStatusURL = serverUrl + versionURL + "/status"

  [<Literal>]
  let LauncherURL = serverUrl + versionURL + "/launcher"

  [<Literal>]
  let tokenServer = "OMEGALULISPOWER!"*)

  let headers =
    Headers
    <| [ "Accept", "application/json"
         "Authorization", Config.Server.tokenServer ]

  let getYandexLink pathToFile =
    Log.TraceInf "Try get link from yandex disk"
    Utils.requestStringResult (URL Config.Server.yandexURL) GET (Query [ "path", pathToFile ]) headers

  let getDropboxLink pathToFile =
    Log.TraceInf "Try get link from dropbox disk"
    Utils.requestStringResult (URL Config.Server.dropboxURL) GET (Query [ "path", pathToFile ]) headers

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

    Utils.requestStringResult (URL Config.Server.modmapURL) GET (Query [ "version", "saturn" ]) headers
    |> deserializeToDict

  let getServerStatus () =
    Log.TraceInf "Try get server status"

    Utils.requestStringResult (URL Config.Server.serverStatusURL) GET (Query []) headers
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
      Utils.SetCPP dispatch "Delete and rename old files..."

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
    Log.TraceInf "Try get launcher hash sum from server"
    Utils.SetCPP dispatch "Try get launcher hash sum from server"

    match serverResult with
    | Ok _ ->

        Log.TraceInf "Start request for hash sum"
        (Utils.requestStringResult (URL <| launcherURL()) GET (Query [ "type", "hash" ]) Server.headers), dispatch
    | Error err -> Error err, dispatch

  let private compareLaunchersSum (hash, dispatch: Message -> unit) =
    Log.TraceInf "Compare launchers hash sum"

    match hash with
    | Ok hashServer ->
        Utils.SetCPP dispatch "Compare launchers hash sum"

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
            Utils.SetCPP dispatch "Download new launcher..."

            Log.TraceInf "Start get launcher stream from server trough HTTPClient"

            let url = Config.Server.launcherURL

            Utils.requestStreamResult (URL Config.Server.launcherURL) GET (Query [ "type", "download" ]) Server.headers
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
          Utils.SetCPP dispatch "Update don't required, all ok"
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

          Utils.SetCPP dispatch "Have new version, start write to file..."

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
        Log.TraceInf "Try get file stream"
        Utils.SetCPP dispatch "Try get file stream..."

        Ok
        <| Utils.requestStreamResult (URL href) GET (Query []) (Headers [])
    | Error err -> Error err

  let private checkFilesSum sum (dispatch: Message -> unit) (pathResult: Result<string, string>) =
    match pathResult with
    | Ok path ->
        Utils.SetCPP dispatch "Expr SHA1 sum of downloaded file"
        use sha = new SHA1Managed()

        Log.TraceInf
        <| sprintf "Get new file SHA1: %s" path

        let sumSnd =
          Utils.convertByteArrayToString
          <| sha.ComputeHash(File.ReadAllBytes(path))

        if sum <> sumSnd then
          Log.TraceInf
          <| sprintf "Downloaded file is corrupted: SHA sum server: %s SHA sum new file: %s" sum sumSnd

          Utils.SetCPP dispatch "Downloaded file is corrupted..."
          dispatch IncreaseCorruptedCounter

        Ok()
    | Error err -> Error err

  let private writeFileFromStream
    fullPathToFile
    (dispatch: Message -> unit)
    (responsStream: Result<HttpResponseWithStream, string>)
    =
    match responsStream with
    | Ok stream ->
        Log.TraceInf
        <| sprintf "Write stream downloaded file: %s" fullPathToFile

        Utils.SetCPP dispatch "Write stream to file"

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
          Utils.SetCPP dispatch "Try get link from disk"
          Ok resp.link
        else
          Utils.SetCPP dispatch "Problem with get link from disk"

          Log.TraceErr
          <| sprintf "Problem with get link from disk, server info: %s" resp.info

          Error
          <| sprintf "Problem with get link from disk, server info: %s" resp.info
    | Error err -> Error err

  let private resolveFileStream
    (dispatch: Message -> unit)
    (resp: Result<Result<HttpResponseWithStream, string>, string>)
    =
    match resp with
    | Ok streamResult ->
        Utils.SetCPP dispatch "Resolver file stream result..."

        match streamResult with
        | Ok stream ->
            Utils.SetCPP dispatch "Stream ok"
            Ok stream
        | Error err ->
            Utils.SetCPP dispatch "Error when try get stream"
            Error err
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
        if basePath.Contains(@"mods\!PS") then
          yield! Directory.GetFiles(basePath)

        for subDir in Directory.GetDirectories(basePath) do
          yield! filesUnder subDir
      }

    filesUnder sPath

  let deleteEmptyFoldersRecursive sPath =
    Log.TraceInf "Delete all empty folders in mods\\!PS"

    let rec dirsUnder (basePath: string) =

      for subDir in Directory.GetDirectories(basePath) do
        dirsUnder subDir

      if basePath.Contains(@"mods\!PS") then
        if (Directory.GetFiles(basePath).Length = 0)
           && (Directory.GetDirectories(basePath).Length = 0) then
          Log.TraceInf
          <| sprintf "Delete empty folder %s" basePath

          Directory.Delete(basePath)

    dirsUnder sPath

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
    Utils.SetCPP dispatch "Start create list files for download"
    Log.TraceInf "Start create list files for download"

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
    Log.TraceInf "Start create list files for delete"

    Utils.SetCPP dispatch "Start create list files for delete"

    [ for KeyValue (k, v) in mapClient do
        match mapServer.ContainsKey(k) with
        | true ->
            Log.TraceInf
            <| sprintf "File %s... ok, file exists in server modmap" k
        | false ->
            Log.TraceInf
            <| sprintf "File %s... not in map, will delete" k

            yield k ]

  let private deleteOutdateFiles (deleteList: string list) (dispatch: Message -> unit) =
    Log.TraceInf "Delete old files... "

    Utils.SetCPP dispatch "Delete old files... "

    for path in deleteList do

      sprintf "Delete... %s" (Utils.getFileNameFromPath <| Utils.fullPath path)
      |> Some
      |> SetCurrentDeleteFile
      |> dispatch

      File.Delete(Utils.fullPath path)
      dispatch IncreaseDeletedCounter

  let filesCheck (mapServer: Dictionary<string, string>) (dispatch: Message -> unit) =

    Log.TraceInf "Create client modmap"
    Utils.SetCPP dispatch "Create client modmap"

    let mapClient = modmapClient <| Utils.currentPath()

    let listForDownload =
      getFilesForDownload mapClient mapServer dispatch

    let listForDelete =
      getFilesForDelete mapClient mapServer dispatch

    listForDownload.Length, listForDelete.Length

  let updateMods (mapServer: Dictionary<string, string>) (disk: Disk) (dispatch: Message -> unit) =

    Log.TraceInf "Real work starts now"
    Utils.SetCPP dispatch "Real work starts now"

    Log.TraceInf "Create client modmap"
    Utils.SetCPP dispatch "Create client modmap"

    let mapClient = modmapClient <| Utils.currentPath()

    let listForDownload =
      getFilesForDownload mapClient mapServer dispatch

    let listForDelete =
      getFilesForDelete mapClient mapServer dispatch

    Log.TraceInf
    <| sprintf "Files to downloads: %d" listForDownload.Length

    Log.TraceInf
    <| sprintf "Files to delete: %d" listForDelete.Length

    let downloadParallel (filesList: (string * string) list) =

      Log.TraceInf
      <| sprintf "Start downloads, num of maximum threads: %d" -1

      Utils.SetCPP dispatch
      <| sprintf "Start downloads, num of maximum threads: %d" -1

      let parallelOption =
        ParallelOptions(MaxDegreeOfParallelism = -1)

      Parallel.ForEach(
        filesList,
        parallelOption,
        (fun (file, sum) ->
          Utils.createDirectoryIfNotExist (Utils.fullPath file)

          sprintf "Download... %s" (Utils.getFileNameFromPath <| Utils.fullPath file)
          |> Some
          |> SetCurrentDownloadFile
          |> dispatch

          Download.startDownload file (Utils.fullPath file) sum disk dispatch
          |> function
          | Ok _ ->
              dispatch IncreaseDownloadedCounter
              Log.TraceInf <| sprintf "Successful download"
          | Error err -> Log.TraceErr <| sprintf "%s" err)
      )

    listForDownload.Length
    |> SetFilesToDownload
    |> dispatch

    StartDownload |> dispatch

    downloadParallel listForDownload |> ignore

    DownloadFinished |> dispatch

    listForDelete.Length
    |> SetFilesToDelete
    |> dispatch

    StartDelete |> dispatch

    deleteOutdateFiles listForDelete dispatch

    Utils.SetCPP dispatch "Now delete empty folders"

    deleteEmptyFoldersRecursive <| Utils.currentPath()
    DeleteFinished |> dispatch

    Utils.SetCPP dispatch "Ok, download and delete old files finished"
    dispatch PrintCorruptedFiles

    dispatch Finished
