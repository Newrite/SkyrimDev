module Launcher.Shell

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Elmish
open Launcher.Logic
open CLogger

module Launcher =

  let initFunction (dispatch: Message -> unit) =
    async {
      UpdateLauncher.renameAndDeleteOldLauncherFiles dispatch
      
      (Server.getServerStatus(), dispatch)
      |>UpdateLauncher.checkLauncherUpdate
      |> fst
      |> function
      | Ok resp ->
          match resp with
          | true -> Update |> SetVersionLauncher |> dispatch
          | false -> Outdate |> SetVersionLauncher |> dispatch
      | Error err ->
          Log.TraceErr
          <| sprintf "Error when try check version: %s" err

          Some(err) |> SetDownloadedFile |> dispatch
    }

  let init () =
    { StateDownloads =
        { DownloadedFile = None
          IsDownload = false
          SumOfFileDownload = 100
          CurrentDownloadProgress = 76.0 }
      IsInUpdate = false
      ProgrammProcess = None
      Disk = YandexDisk
      Version = NotYeatCheck },
    Cmd.ofSub (fun (dispatch: Message -> unit) ->
      initFunction dispatch |> Async.Start)

  module Update =
    
    let private setDownloadedFile file state =
      { state with
          StateDownloads =
            { state.StateDownloads with
                DownloadedFile = file } }
  
    let private increaseDownloadedCounter state =
      { state with
          StateDownloads =
            { state.StateDownloads with
                CurrentDownloadProgress = state.StateDownloads.CurrentDownloadProgress + 1.0 } }
  
    let private setUpdateProgramProcess newProcess (state: State) =
      { state with
          ProgrammProcess = newProcess }
  
    let private setFilesToDownload filesCount state =
      { state with
          StateDownloads =
            { state.StateDownloads with
                SumOfFileDownload = filesCount } }
  
    let private startUpdate state = { state with IsInUpdate = true }
  
    let private switchDisk state = { state with Disk = state.Disk.Change }
  
    let private downloadFinished state =
      { state with
          StateDownloads =
            { state.StateDownloads with
                IsDownload = false
                CurrentDownloadProgress = 0.0 }
          IsInUpdate = false }
  
    let private startDownload state =
      { state with
          StateDownloads =
            { state.StateDownloads with
                IsDownload = true
                CurrentDownloadProgress = 0.0 } }
  
    let private setVersionLauncher version state = { state with Version = version }
  
    let update (msg: Message) state =
      match msg with
      | SetDownloadedFile file -> setDownloadedFile file state, Cmd.none
      | IncreaseDownloadedCounter -> increaseDownloadedCounter state, Cmd.none
      | SetUpdateProgramProcess newProcess -> setUpdateProgramProcess newProcess state, Cmd.none
      | SetFilesToDownload filesCount -> setFilesToDownload filesCount state, Cmd.none
      | StartUpdate -> startUpdate state, Cmd.none
      | SwitchDisk -> switchDisk state, Cmd.none
      | DownloadFinished -> downloadFinished state, Cmd.none
      | StartDownload -> startDownload state, Cmd.none
      | SetVersionLauncher version -> setVersionLauncher version state, Cmd.none

  module View =  
  
    module private ProgressBars =
      let progressBarDownloadState (state: State) (dispatch: Message -> unit) =
        let progress () =
          state.StateDownloads.CurrentDownloadProgress
          / ((float state.StateDownloads.SumOfFileDownload)
             / 100.0)
    
        ProgressBar.create [ Grid.row 3
                             Grid.column 1
                             Grid.columnSpan 8
                             ProgressBar.isVisible true //state.IsDownload
                             ProgressBar.value <| progress ()
                             ProgressBar.classes [ "downloadbar" ]
                             ProgressBar.showProgressText true ]

    module private TextBlocks =
      
      let textBlockTitleApp (state: State) (dispatch: Message -> unit) =
        TextBlock.create [ TextBlock.dock Dock.Top
                           TextBlock.isHitTestVisible false
                           TextBlock.text "This is launcher"
                           TextBlock.classes [ "appname" ] ]
    
      let textBlockDownloadsFile (state: State) (dispatch: Message -> unit) =
        TextBlock.create [ Grid.column 0
                           Grid.row 2
                           TextBlock.text "Download 30 of 7302"
                           Grid.columnSpan 10
                           TextBlock.classes [ "launcherupdate" ] ]
    
      let textBlockDownloadedFile (state: State) (dispatch: Message -> unit) =
        TextBlock.create [ Grid.column 1
                           Grid.row 5
                           TextBlock.text "Download... /body_msn.dds"
                           if state.StateDownloads.DownloadedFile.IsSome then
                             TextBlock.text state.StateDownloads.DownloadedFile.Value
                           Grid.columnSpan 8
                           TextBlock.classes [ "currentoperation" ] ]
    
      let textBlockLauncherVersion (state: State) (dispatch: Message -> unit) =
        TextBlock.create [ Grid.column 0
                           Grid.row 7
                           match state.Version with
                           | Update -> TextBlock.text "Launcher is up to date"
                           | Outdate -> TextBlock.text "Have new version, can update"
                           | NotYeatCheck -> TextBlock.text "Check launcher version..."
                           Grid.columnSpan 4
                           TextBlock.classes [ "launcherupdate" ] ]
    
      let textBlockProgrammProcess (state: State) (dispatch: Message -> unit) =
        TextBlock.create [ Grid.column 1
                           Grid.row 4
                           if state.ProgrammProcess.IsSome then
                             TextBlock.text state.ProgrammProcess.Value
                           Grid.columnSpan 5
                           TextBlock.classes [ "currentoperation" ] ]
    
      let textBlockDiskDescription (state: State) (dispatch: Message -> unit) =
        TextBlock.create [ Grid.column 8
                           Grid.row 6
                           TextBlock.text "Choose disk"
                           Grid.columnSpan 2
                           TextBlock.classes [ "choosedisk" ] ]
  
    module private Buttons =
      
      let buttonUpdate (state: State) (dispatch: Message -> unit) =
        Button.create [ Grid.column 9
                        Grid.row 9
                        Button.isHitTestVisible true
                        Button.classes [ "main"; "update" ]
                        Button.content "Update"
                        Button.isEnabled
                        <| ((not state.IsInUpdate)
                            && not (state.Version = NotYeatCheck))
                        Button.onClick (fun _ -> dispatch StartUpdate) ]
        
      let buttonDownload (state: State) (dispatch: Message -> unit) =
        Button.create [ Grid.column 8
                        Grid.columnSpan 2
                        Grid.row 8
                        Button.isHitTestVisible true
                        Button.classes [ "main"; "download" ]
                        Button.content "Download"
                        Button.isEnabled
                        <| ((not state.IsInUpdate)
                            && not (state.Version = NotYeatCheck)
                            && not (state.StateDownloads.IsDownload))
                        Button.onClick (fun _ -> dispatch StartDownload) ]
    
      let buttonFileCheck (state: State) (dispatch: Message -> unit) =
        Button.create [ Grid.column 8
                        Grid.row 9
                        Button.isHitTestVisible true
                        Button.classes [ "main"; "filescheck" ]
                        Button.content "Files check" ]
      //Button.onClick (fun _ -> dispatch StopUpdate) ]
    
    module private RadioButtons =
      
      let radioButtonDropbox (state: State) (dispatch: Message -> unit) =
        RadioButton.create [ Grid.column 9
                             Grid.row 7
                             RadioButton.classes [ "diskradio" ]
                             RadioButton.content "Dropbox"
                             RadioButton.isChecked state.Disk.IsDropbox
                             RadioButton.onChecked (fun _ -> dispatch SwitchDisk) ]
    
      let radioButtonYandex (state: State) (dispatch: Message -> unit) =
        RadioButton.create [ Grid.column 8
                             Grid.row 7
                             RadioButton.classes [ "diskradio" ]
                             RadioButton.content "Yandex"
                             RadioButton.isChecked state.Disk.IsYandex
                             RadioButton.onChecked (fun _ -> dispatch SwitchDisk) ]
    
    module private Grids =
      
      let grid (state: State) (dispatch: Message -> unit) =
        Grid.create [ Grid.rowDefinitions "*, *, *, *, *, *, *, *, 1.1*, 1.1*"
                      Grid.columnDefinitions "2*, *, *, *, *, *, *, *, 2*, 2*"
                      Grid.showGridLines false
                      Grid.children [ ProgressBars.progressBarDownloadState state dispatch
                                      TextBlocks.textBlockDownloadedFile state dispatch
                                      TextBlocks.textBlockProgrammProcess state dispatch
                                      TextBlocks.textBlockLauncherVersion state dispatch
                                      TextBlocks.textBlockDiskDescription state dispatch
                                      TextBlocks.textBlockDownloadsFile state dispatch
                                      Buttons.buttonDownload state dispatch
                                      Buttons.buttonUpdate state dispatch
                                      Buttons.buttonFileCheck state dispatch
                                      RadioButtons.radioButtonYandex state dispatch
                                      RadioButtons.radioButtonDropbox state dispatch ] ]
  
    let view (state: State) (dispatch: Message -> unit) =  
      DockPanel.create [ DockPanel.children [ TextBlocks.textBlockTitleApp state dispatch
                                              Grids.grid state dispatch ] ]