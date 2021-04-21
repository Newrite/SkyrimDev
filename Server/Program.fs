//dotnet publish -c Release  -r linux-arm -p:PublishSingleFile=true --self-contained true
//builds for arm orange pi pc+
//for build on other platform edit TWBot.fsproj

open Saturn
open Giraffe
open Microsoft
open System.IO
open System.Collections.Generic
open System.Text.Json
open Dropbox
open Server
open YandexDisk
open Microsoft.Extensions.Logging


module Constants =
    let [<Literal>] fileNameModMapRelease = "modmap.json"
    let [<Literal>] serverStatus = true
    let [<Literal>] information = "Already ok"
    let [<Literal>] Authorization = "Authorization"
    
    
module ServerAPI =
    
    type ResponsStatus =
        { StatusLine: bool
          Information: string }
    
    let [<Literal>] qPathKey = "path"
    let [<Literal>] qModmapKey = "version"
    let [<Literal>] qModmapValueRelease = "release"
    let modmap ()=
        let deserializeFromFile fileName =
            File.ReadAllText(fileName)
            |>JsonSerializer.Deserialize<Dictionary<string, string>>
        deserializeFromFile Constants.fileNameModMapRelease
        
    let checkHeader (headers: AspNetCore.Http.IHeaderDictionary) headerToCheck (valueToFind: string) =
        if headers.ContainsKey(headerToCheck)  then
            headers.[headerToCheck].ToString().Contains(valueToFind)
        else
            false

    let getResponsStatus = { StatusLine = Constants.serverStatus; Information = Constants.information }

module DiskRequests =
    let private dropboxDiskInstance = new Api.DropboxClient(Tokens.DropboxToken)
    
    let private yandexDiskInstance = new Client.Http.DiskHttpApi(Tokens.YandexToken)
    
    let private memoize(f: 'a -> 'b) =
        let dict = new Dictionary<'a, 'b>()
        
        let memoizedFunc (input: 'a) =
            match dict.TryGetValue(input) with
            |true, x -> x
            |false, _ ->
                let answer = f input
                dict.Add(input, answer)
                answer
                
        memoizedFunc
    
    let private _requestToYandexDisk path =
        try
            let file = yandexDiskInstance.Files.GetDownloadLinkAsync(path)
            file.Wait()
            {|Status = true ;Link = file.Result.Href; Info = "Ok"|}
        with _ as eX -> {|Status = false; Link = ""; Info = "Can't get file from yandex"|}
        
    let private _requestToDropboxDisk path =
        try
            let file = dropboxDiskInstance.Files.GetTemporaryLinkAsync("/"+path)
            file.Wait()
            {|Status = true ;Link = file.Result.Link; Info = "Ok"|}
        with _ as eX -> {|Status = false; Link = ""; Info = "Can't get file from dropbox"|}
        
    let requestToDisk next (ctx: AspNetCore.Http.HttpContext) request =
        match ctx.Request.Query.ContainsKey(ServerAPI.qPathKey) with
        |true ->
            (ctx.Request.Query.[ServerAPI.qPathKey].ToString()
             |> request
             |> json) next ctx
        |false -> (RequestErrors.METHOD_NOT_ALLOWED "no query") next ctx
        
    let requestToYandexDisk = memoize _requestToYandexDisk
    let requestToDropboxDisk = memoize _requestToDropboxDisk

    

module Pipes =
    let Auth = pipeline {
        plug (fun next ctx ->
            match ServerAPI.checkHeader ctx.Request.Headers Constants.Authorization Tokens.ServerToken with
            | true -> next ctx
            | false -> (RequestErrors.UNAUTHORIZED "Bearer" "Server" "Authorization error.") next ctx)
    }
    
    let AcceptJson = pipeline {
        plug acceptJson
    } 
        
        
module Routers =
    let Server = router {
        get "/status" (json ServerAPI.getResponsStatus )
    }
    
    let Modmap = router {
        get "/modmap" (fun next ctx ->
            match ctx.Request.Query.ContainsKey(ServerAPI.qModmapKey) with
            |true when ctx.Request.Query.[ServerAPI.qModmapKey].ToString().Contains(ServerAPI.qModmapValueRelease) ->
                (json <| ServerAPI.modmap) next ctx
            |true -> (RequestErrors.BAD_REQUEST "Wrong request.") next ctx
            |false -> (RequestErrors.METHOD_NOT_ALLOWED "no query") next ctx
            |_ -> (ServerErrors.NOT_IMPLEMENTED "What the fuck.") next ctx)
    }
    
    
    let YandexDisk = router {
        get "/yandex" (fun next ctx -> DiskRequests.requestToDisk next ctx DiskRequests.requestToYandexDisk)
    }
    
    let DropBoxDisk = router {
        get "/dropbox" (fun next ctx -> DiskRequests.requestToDisk next ctx DiskRequests.requestToDropboxDisk)
    }
    
    let Disk = router {
        pipe_through Pipes.AcceptJson
        forward "" YandexDisk
        forward "" DropBoxDisk
    }

let mainRouter = router {
    not_found_handler (text "Error: 404 wrong page")
    pipe_through Pipes.Auth
    forward "" Routers.Server
    forward "" Routers.Modmap
    forward "" Routers.Disk
}

let configureLogging (logging: ILoggingBuilder) =
    logging.SetMinimumLevel(LogLevel.None) |> ignore

let app = application {
    url "http://0.0.0.0:3030"
    use_router mainRouter
    memory_cache
    use_gzip
    //logging configureLogging
}

run app