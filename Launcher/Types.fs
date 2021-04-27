[<AutoOpen>]
module Launcher.Types

[<AutoOpen>]
module Types =

  type Respons =
    { info: string
      link: string
      status: bool }

  type ResponsStatus =
    { statusline: bool
      information: string }
    
  type LauncherVersion =
    | Outdate
    | Update
    | NotYeatCheck
    
  type Message =
  | SetDownloadedFile of string option
  | IncreaseDownloadedCounter
  | SetUpdateProgramProcess of string option
  | SetFilesToDownload of int
  | StartUpdate
  | DownloadFinished
  | SwitchDisk
  | StartDownload
  | SetVersionLauncher of LauncherVersion
  
  type Disk =
    | YandexDisk
    | DropboxDisk
    
    member self.Change =
      match self with
      | YandexDisk -> DropboxDisk
      | DropboxDisk -> YandexDisk

    member self.IsYandex =
      match self with
      | YandexDisk -> true
      | DropboxDisk -> false

    member self.IsDropbox =
      match self with
      | DropboxDisk -> true
      | YandexDisk -> false
  
  type StateDownloads =
    { DownloadedFile: string option
      IsDownload: bool
      SumOfFileDownload: int
      CurrentDownloadProgress: float }
  
  type State =
    { IsInUpdate: bool
      StateDownloads: StateDownloads
      ProgrammProcess: string option
      Disk: Disk
      Version: LauncherVersion }
  
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
      
  type HttpMethodAlias =
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