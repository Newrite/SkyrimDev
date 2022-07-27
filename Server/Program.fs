//dotnet publish -c Release  -r linux-arm -p:PublishSingleFile=true --self-contained true
//builds for arm orange pi pc+
//for build on other platform edit TWBot.fsproj

open Microsoft.AspNetCore.Http
open Saturn
open Giraffe
open System.IO
open System.Collections.Generic
open System.Collections.Concurrent
open System.Security.Cryptography
open System.Text.Json
open Dropbox
open Server
open YandexDisk
open Microsoft.Extensions.Logging


module Constants =
  [<Literal>]
  let fileNameModMapRelease = "modmap.json"

  [<Literal>]
  let fileNameServerStatus =
    @"serverstatus.json"

  [<Literal>]
  let fileNameLauncher =
    @"Launcher.exe"

  [<Literal>]
  let Authorization = "Authorization"


module ServerAPI =

  type ResponsStatus =
    { statusline: bool
      information: string }
    
  [<Literal>]
  let versionURL = "/1"
  
  [<Literal>]
  let healthURL = "/health"

  [<Literal>]
  let qDiskPathKey = "path"

  [<Literal>]
  let qDiskYandexPathURL = versionURL + "/yandex"

  [<Literal>]
  let qDiskDropboxURL = versionURL + "/dropbox"

  [<Literal>]
  let qModmapKey = "version"

  [<Literal>]
  let qModmapValueSaturn = "saturn"

  [<Literal>]
  let qModmapURL = versionURL + "/modmap"

  [<Literal>]
  let qServerStatusURL = versionURL + "/status"

  [<Literal>]
  let qLauncherKey = "type"

  [<Literal>]
  let qLauncherDownloadValue = "download"

  [<Literal>]
  let qLauncherHASHValue = "hash"

  [<Literal>]
  let qLauncherURL = versionURL + "/launcher"


  let modmap () =
    let deserializeFromFile fileName =
      let text = File.ReadAllText(fileName)

      text
      |> JsonSerializer.Deserialize<Dictionary<string, string>>

    deserializeFromFile Constants.fileNameModMapRelease

  let launcherBinaryData () =
    File.ReadAllBytes(Constants.fileNameLauncher)

  let launcherHASH () =
    use sha = new SHA1Managed()
    launcherBinaryData () |> sha.ComputeHash


  let checkHeader (headers: IHeaderDictionary) headerToCheck (valueToFind: string) =
    if headers.ContainsKey(headerToCheck) then
      headers.[headerToCheck]
        .ToString()
        .Contains(valueToFind)
    else
      false

  let getResponsStatus () =
    let deserializeFromFile fileName =
      File.ReadAllText(fileName)
      |> JsonSerializer.Deserialize<ResponsStatus>

    deserializeFromFile Constants.fileNameServerStatus

module DiskRequests =
  let private dropboxDiskInstance =
    new Api.DropboxClient(Tokens.DropboxToken)

  let private yandexDiskInstance =
    new Client.Http.DiskHttpApi(Tokens.YandexToken)

  let private memoize (f: 'a -> 'b) =
    let dict = ConcurrentDictionary<'a, 'b * int64>()
    let time () = System.DateTime.Now.Ticks / 10000000L

    let memoizedFunc (input: 'a) =
      if dict.ContainsKey(input) then
        let lastTime = snd dict.[input]

        if (time () - lastTime) > 10800L then
          dict.Remove(input) |> ignore

      match dict.TryGetValue(input) with
      | true, x -> fst x
      | false, _ ->
          let answer = f input

          dict.TryAdd(input, (answer, time ()))
          |> ignore

          answer

    memoizedFunc

  let private _requestToYandexDisk path =
    try
      let file =
        yandexDiskInstance.Files.GetDownloadLinkAsync(path)

      file.Wait()

      {| Status = true
         Link = file.Result.Href
         Info = "Ok" |}
    with eX ->
      {| Status = false
         Link = ""
         Info = "Can't get file from yandex" |}

  let private _requestToDropboxDisk (path: string) =
    try
      let file =
        dropboxDiskInstance.Files.GetTemporaryLinkAsync(path)

      file.Wait()

      {| Status = true
         Link = file.Result.Link
         Info = "Ok" |}
    with eX ->
      {| Status = false
         Link = ""
         Info = "Can't get file from dropbox" |}

  let requestToDisk next (ctx: HttpContext) request =
    match ctx.Request.Query.ContainsKey(ServerAPI.qDiskPathKey) with
    | true ->
        (ctx.Request.Query.[ServerAPI.qDiskPathKey]
          .ToString()
         |> request
         |> json)
          next
          ctx
    | false -> (RequestErrors.METHOD_NOT_ALLOWED "no query") next ctx

  let requestToYandexDisk = memoize _requestToYandexDisk
  let requestToDropboxDisk = memoize _requestToDropboxDisk

module Pipes =
  let Auth =
    pipeline {
      plug
        (fun next ctx ->
          match ServerAPI.checkHeader ctx.Request.Headers Constants.Authorization Tokens.ServerToken with
          | true -> next ctx
          | false -> (RequestErrors.UNAUTHORIZED "Bearer" "Server" "Authorization error.") next ctx)
    }

  let AcceptJson = pipeline { plug acceptJson }


module Routers =

  let (|ModmapRelease|LauncherDownload|LauncherHASH|NoQuery|BadRequest|) (query: IQueryCollection) =

    if query.ContainsKey(ServerAPI.qModmapKey) then
      if query.[ServerAPI.qModmapKey]
           .ToString()
           .Contains(ServerAPI.qModmapValueSaturn) then
        ModmapRelease
      else
        BadRequest

    elif query.ContainsKey(ServerAPI.qLauncherKey) then
      if query.[ServerAPI.qLauncherKey]
           .ToString()
           .Contains(ServerAPI.qLauncherHASHValue) then
        LauncherHASH
      elif query.[ServerAPI.qLauncherKey]
             .ToString()
             .Contains(ServerAPI.qLauncherDownloadValue) then
        LauncherDownload
      else
        BadRequest

    else
      NoQuery

  let Status =
    router { get ServerAPI.qServerStatusURL (json <| ServerAPI.getResponsStatus ()) }

  let Modmap =
    router {
      get
        ServerAPI.qModmapURL
        (fun next ctx ->
          match ctx.Request.Query with
          | ModmapRelease -> (json <| ServerAPI.modmap ()) next ctx
          | BadRequest -> RequestErrors.BAD_REQUEST "Wrong request." next ctx
          | NoQuery -> RequestErrors.METHOD_NOT_ALLOWED "no query" next ctx
          | _ -> ServerErrors.NOT_IMPLEMENTED "What the fuck." next ctx)
    }


  let YandexDisk =
    router {
      get
        ServerAPI.qDiskYandexPathURL
        (fun next ctx -> DiskRequests.requestToDisk next ctx DiskRequests.requestToYandexDisk)
    }

  let DropBoxDisk =
    router {
      get
        ServerAPI.qDiskDropboxURL
        (fun next ctx -> DiskRequests.requestToDisk next ctx DiskRequests.requestToDropboxDisk)
    }

  let Launcher =
    router {

      get
        ServerAPI.qLauncherURL
        (fun next ctx ->
          match ctx.Request.Query with
          | LauncherHASH ->
              (ServerAPI.launcherHASH ()
               |> Seq.fold (fun acc elem -> acc + elem.ToString()) ""
               |> text)
                next
                ctx
          | LauncherDownload ->
              ServerAPI.launcherBinaryData ()
              |> ctx.WriteBytesAsync
          | BadRequest -> RequestErrors.BAD_REQUEST "Wrong request." next ctx
          | NoQuery -> RequestErrors.METHOD_NOT_ALLOWED "no query" next ctx
          | _ -> ServerErrors.NOT_IMPLEMENTED "What the fuck." next ctx)
    }

  let Disk =
    router {
      pipe_through Pipes.AcceptJson
      pipe_through Pipes.Auth
      forward "" YandexDisk
      forward "" DropBoxDisk
    }
    
  let HealthCheck =
    router {
      get ServerAPI.healthURL (setStatusCode 200)
    }


let mainRouter =
  router {
    not_found_handler (text "Error: 404 wrong page")
    forward "" Routers.HealthCheck
    forward "" Routers.Launcher
    forward "" Routers.Status
    forward "" Routers.Modmap
    forward "" Routers.Disk
  }

let configureLogging (logging: ILoggingBuilder) =
  logging.SetMinimumLevel(LogLevel.None) |> ignore


[<EntryPoint>]
let main _ =
  let app =
    application {
      url "http://0.0.0.0:3030"
      use_router mainRouter
      memory_cache
      use_gzip
    //logging configureLogging
    }

  run app
  0