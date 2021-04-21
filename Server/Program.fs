open Saturn
open Giraffe
open Microsoft
open System.IO
open System.Collections.Generic
open System.Text.Json
open Dropbox
open YandexDisk
open Microsoft.Extensions.Logging

module Constants =
    let [<Literal>] qPathKey = "path"
    let [<Literal>] qModmapKey = "version"
    let [<Literal>] qModmapValueRelease = "release"
    let [<Literal>] fileNameModMapRelease = "E:\Programming\F#\LaunchServer\modmap.json"
    let [<Literal>] serverStatus = true
    let [<Literal>] information = "Already ok"
    let [<Literal>] Authorization = "Authorization"
    let [<Literal>] Token = "Bearer OMEGALULISPOWER!"
    let [<Literal>] tokendbx = "t4ld3cJMM9IAAAAAAAAAAXuuGeo2mLcMZxaYXjl7oaIg_up7yVTeJmp23ekpxERM"
    let [<Literal>] tokendisk = "AQAAAABT8kstAAcN-S_wbWiH8kcMgqgu20WWdo4"
    
    let modmap =
        let deserializeFromFile fileName =
            File.ReadAllText(fileName)
            |>JsonSerializer.Deserialize<Dictionary<string, string>>
        deserializeFromFile fileNameModMapRelease

module DiskRequests =
    let private dropboxDiskInstance = new Api.DropboxClient(Constants.tokendbx)
    
    let private yandexDiskInstance = new Client.Http.DiskHttpApi(Constants.tokendisk)
    
    let private memoize(f: 'a -> 'b) (filter: Dictionary<'a, 'b> -> unit) =
        let dict = new Dictionary<'a, 'b>()
        
        let memoizedFunc (input: 'a) =
            filter dict
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
        match ctx.Request.Query.ContainsKey(Constants.qPathKey) with
        |true ->
            (ctx.Request.Query.[Constants.qPathKey].ToString()
             |> request
             |> json) next ctx
        |false -> (RequestErrors.METHOD_NOT_ALLOWED "no query") next ctx
        
    let requestToYandexDisk = memoize _requestToYandexDisk (fun dict -> printfn "%A" dict)
    let requestToDropboxDisk = memoize _requestToDropboxDisk (fun dict -> printfn "%A" dict)

type ResponsStatus =
    { StatusLine: bool
      Information: string }
    
let checkHeader (headers: AspNetCore.Http.IHeaderDictionary) headerToCheck (valueToFind: string) =
    if headers.ContainsKey(headerToCheck)  then
        headers.[headerToCheck].ToString().Contains(valueToFind)
    else
        false

let getResponsStatus = { StatusLine = Constants.serverStatus; Information = Constants.information }

module Pipes =
    let Auth = pipeline {
        plug (fun next ctx ->
            match checkHeader ctx.Request.Headers Constants.Authorization Constants.Token with
            | true -> next ctx
            | false -> (RequestErrors.UNAUTHORIZED "Bearer" "Server" "Authorization error.") next ctx)
    }
    
    let AcceptJson = pipeline {
        plug acceptJson
    } 
        
        
module Routers =
    let Server = router {
        get "/status" (json getResponsStatus )
    }
    
    let Modmap = router {
        get "/modmap" (fun next ctx ->
            match ctx.Request.Query.ContainsKey(Constants.qModmapKey) with
            |true when ctx.Request.Query.[Constants.qModmapKey].ToString().Contains(Constants.qModmapValueRelease) ->
                (json <| Constants.modmap) next ctx
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
        //pipe_through pAcceptJson
        forward "" YandexDisk
        forward "" DropBoxDisk
    }

let mainRouter = router {
    not_found_handler (text "Error: 404 wrong page")
    //pipe_through pAuth
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