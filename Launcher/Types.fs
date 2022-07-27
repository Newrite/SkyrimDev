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
    | SetCurrentDownloadFile of string option
    | IncreaseDownloadedCounter
    | IncreaseCorruptedCounter
    | SetFilesToDownload of int
    | DownloadFinished
    | StartDownload
    | PrintCorruptedFiles

    | SetCurrentDeleteFile of string option
    | IncreaseDeletedCounter
    | SetFilesToDelete of int
    | DeleteFinished
    | StartDelete

    | StartUpdateFilesProcess
    | SetCurrentProgramProcess of string option
    | StartUpdateLauncher
    | SetVersionLauncher of LauncherVersion
    | SwitchDisk
    | Finished
    | CheckFiles
    | ChoosePath

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
      IsInDownload: bool
      SumOfFileDownload: int
      CurrentDownloadProgress: float }

  type StateDeletes =
    { DeletedFile: string option
      IsInDelete: bool
      SumOfFileDelete: int
      CurrentDeleteProgress: float }

  type State =
    { IsInUpdate: bool
      StateDownloads: StateDownloads
      StateDeletes: StateDeletes
      CorruptedFiles: int
      ProgrammProcess: string option
      Disk: Disk
      Version: LauncherVersion }

  type ServerStatus = ServerOK

  type URL =
    | URL of string
    member self.Value =
      match self with
      | URL value -> value

  type Headers =
    | Headers of (string * string) list
    member self.Value =
      match self with
      | Headers value -> value

  type Query =
    | Query of (string * string) list
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