module Launcher.GUI


open System.Threading
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Elmish

open CLogger

module Launcher =


  let main (dispatch: Logic.Types.Msg -> unit) =
    async {
      Log.TraceDeb
      <| sprintf "Start info currentPath:%s exeFileName: %s" Logic.currentPath Logic.exeFileName

      Some "Checking server"
      |> Logic.Types.SetUpdateProgramProcess
      |> dispatch

      Logic.Server.getServerStatus ()
      |> Logic.Server.updateLauncher dispatch
      |> function
      | Ok status ->
          match status with
          | true ->
              Logic.Server.getModmapFromServer ()
              |> function
              | Ok modmapServer ->
                  Some "Successful get modmap from server"
                  |> Logic.Types.SetUpdateProgramProcess
                  |> dispatch

                  Logic.Launcher.RealWork modmapServer (Logic.Launcher.modmapClient Logic.currentPath) dispatch
              | Error err ->
                  sprintf "Error while try get modmap from server: %s" err
                  |> Some
                  |> Logic.Types.SetUpdateProgramProcess
                  |> dispatch
          | false ->
              Some "Successful download new version, will close after 3s"
              |> Logic.Types.SetUpdateProgramProcess
              |> dispatch

              Thread.Sleep(3000)
              System.Environment.Exit(1)
      | Error err ->
          sprintf "Error while try update launcher: %s" err
          |> Some
          |> Logic.Types.SetUpdateProgramProcess
          |> dispatch

      dispatch Logic.Types.DownloadFinished
    }

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

  type State =
    { DownloadedFile: string option
      IsDownload: bool
      IsInUpdate: bool
      ProgrammProcess: string option
      SumOfFileDownload: int
      CurrentDownloadProgress: float
      Disk: Disk }

  let init () =
    { DownloadedFile = None
      IsDownload = false
      IsInUpdate = false
      ProgrammProcess = None
      SumOfFileDownload = 0
      CurrentDownloadProgress = 0.0
      Disk = YandexDisk },
    Cmd.none

  let update (msg: Logic.Types.Msg) state =
    match msg with
    | Logic.Types.SetDownloadedFile file -> { state with DownloadedFile = file }, Cmd.none
    | Logic.Types.IncreaseDownloadedCounter ->
        { state with
            CurrentDownloadProgress = state.CurrentDownloadProgress + 1.0 },
        Cmd.none
    | Logic.Types.SetUpdateProgramProcess newProcess ->
        { state with
            ProgrammProcess = newProcess },
        Cmd.none
    | Logic.Types.SetFilesToDownload filesCount ->
        { state with
            SumOfFileDownload = filesCount },
        Cmd.none
    | Logic.Types.StartUpdate ->
        { state with IsInUpdate = true },
        Cmd.ofSub (fun (dispatch: Logic.Types.Msg -> unit) -> main dispatch |> Async.Start)
    | Logic.Types.SwitchDisk -> { state with Disk = state.Disk.Change }, Cmd.none
    | Logic.Types.DownloadFinished ->
        { state with
            IsDownload = false
            CurrentDownloadProgress = 0.0
            IsInUpdate = false },
        Cmd.none
    | Logic.Types.StartDownload ->
        { state with
            IsDownload = true
            CurrentDownloadProgress = 0.0 },
        Cmd.none

  let textBlockTitleApp (state: State) (dispatch: Logic.Types.Msg -> unit) =
    TextBlock.create [ TextBlock.dock Dock.Top
                       TextBlock.text "This is launcher"
                       TextBlock.classes [ "appname" ] ]

  let progressBarDownloadState (state: State) (dispatch: Logic.Types.Msg -> unit) =
    let progress () =
      state.CurrentDownloadProgress
      / ((float state.SumOfFileDownload) / 100.0)

    ProgressBar.create [ Grid.row 4
                         Grid.column 1
                         Grid.columnSpan 8
                         ProgressBar.isVisible state.IsDownload
                         ProgressBar.value <| progress ()
                         ProgressBar.classes [ "downloadbar" ] ]

  let textBlockDownloadedFile (state: State) (dispatch: Logic.Types.Msg -> unit) =
    TextBlock.create [ Grid.column 1
                       Grid.row 5
                       if state.DownloadedFile.IsSome then
                         TextBlock.text state.DownloadedFile.Value
                       Grid.columnSpan 8
                       TextBlock.classes [ "currentoperation" ] ]

  let textBlockProgrammProcess (state: State) (dispatch: Logic.Types.Msg -> unit) =
    TextBlock.create [ Grid.column 3
                       Grid.row 3
                       if state.ProgrammProcess.IsSome then
                         TextBlock.text state.ProgrammProcess.Value
                       Grid.columnSpan 5
                       TextBlock.classes [ "currentoperation" ] ]

  let buttonUpdate (state: State) (dispatch: Logic.Types.Msg -> unit) =
    Button.create [ Grid.column 9
                    Grid.row 9
                    Button.classes [ "main"; "update" ]
                    Button.content "Update"
                    Button.isEnabled <| not state.IsInUpdate
                    Button.onClick (fun _ -> dispatch Logic.Types.StartUpdate) ]

  let buttonFileCheck (state: State) (dispatch: Logic.Types.Msg -> unit) =
    Button.create [ Grid.column 8
                    Grid.row 9
                    Button.classes [ "main"; "filescheck" ]
                    Button.content "Files check" ]
  //Button.onClick (fun _ -> dispatch StopUpdate) ]

  let radioButtonDropbox (state: State) (dispatch: Logic.Types.Msg -> unit) =
    RadioButton.create [ Grid.column 9
                         Grid.row 8
                         RadioButton.classes [ "diskradio" ]
                         RadioButton.content "Dropbox"
                         RadioButton.isChecked state.Disk.IsDropbox
                         RadioButton.onChecked (fun _ -> dispatch Logic.Types.SwitchDisk) ]

  let radioButtonYandex (state: State) (dispatch: Logic.Types.Msg -> unit) =
    RadioButton.create [ Grid.column 8
                         Grid.row 8
                         RadioButton.classes [ "diskradio" ]
                         RadioButton.content "Yandex"
                         RadioButton.isChecked state.Disk.IsYandex
                         RadioButton.onChecked (fun _ -> dispatch Logic.Types.SwitchDisk) ]

  let grid (state: State) (dispatch: Logic.Types.Msg -> unit) =
    Grid.create [ Grid.rowDefinitions "*, *, *, *, *, *, *, *, *, *"
                  Grid.columnDefinitions "2*, *, * *, *, *, *, *, 2*, 2*"
                  Grid.showGridLines false
                  Grid.children [ progressBarDownloadState state dispatch
                                  textBlockDownloadedFile state dispatch
                                  textBlockProgrammProcess state dispatch
                                  buttonUpdate state dispatch
                                  buttonFileCheck state dispatch
                                  radioButtonYandex state dispatch
                                  radioButtonDropbox state dispatch ] ]

  let view (state: State) (dispatch: Logic.Types.Msg -> unit) =
    let progress () =
      state.CurrentDownloadProgress
      / ((float state.SumOfFileDownload) / 100.0)

    DockPanel.create [ DockPanel.children [ textBlockTitleApp state dispatch
                                            grid state dispatch ] ]