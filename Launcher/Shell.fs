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

      (Server.getServerStatus (), dispatch)
      |> UpdateLauncher.checkLauncherUpdate
      |> fst
      |> function
      | Ok resp ->
          match resp with
          | true ->
              None |> SetCurrentProgramProcess |> dispatch
              Update |> SetVersionLauncher |> dispatch
          | false ->
              None |> SetCurrentProgramProcess |> dispatch
              Outdate |> SetVersionLauncher |> dispatch
      | Error err ->
          Log.TraceErr
          <| sprintf "Error when try check version: %s" err

          Utils.SetCPP dispatch err
    }

  let startUpdateLauncherFunction (dispatch: Message -> unit) =
    async {
      (Server.getServerStatus (), dispatch)
      |> UpdateLauncher.checkLauncherUpdate
      |> UpdateLauncher.updateLauncher
      |> function
      | Ok result ->
          if result then
            dispatch Finished
          else
            Utils.SetCPP dispatch "Launcher will restart, if not, please, restart manually"
            System.Environment.Exit(0)
      | Error err -> Utils.SetCPP dispatch err
    }

  let checkFilesFunction (dispatch: Message -> unit) =
    async {
      Server.getModmapFromServer ()
      |> function
      | Ok dict ->
          Utils.SetCPP dispatch "Start check files for delete and download"
          let needDownload, needDelete = Launcher.filesCheck dict dispatch

          Utils.SetCPP dispatch
          <| sprintf "Files need to delete: %d need to download: %d" needDelete needDownload

          dispatch Finished
      | Error err ->
          Utils.SetCPP dispatch err
          dispatch Finished
    }

  let startUpdateFilesProcessFunction (disk: Disk) (dispatch: Message -> unit) =
    async {
      Server.getModmapFromServer ()
      |> function
      | Ok dict -> Launcher.updateMods dict disk dispatch
      | Error err ->
          err
          |> Some
          |> SetCurrentProgramProcess
          |> dispatch
    }
    
  let choosePathFunction (ofd: OpenFolderDialog) (window: Window) (dispatch: Message -> unit) =
    async {
      Utils.SetCPP dispatch "Choosing new MO2 path..."
      let result = ofd.ShowAsync(window)
      result.Wait()
      match result.IsCompleted with
      |true ->
        if result.Result <> "" then
          result.Result |> System.IO.Directory.SetCurrentDirectory
        None |> SetCurrentProgramProcess |> dispatch
      |false -> Utils.SetCPP dispatch "Fail when choose new path..."
    }

  let init () =
    { StateDownloads =
        { DownloadedFile = None
          IsInDownload = false
          SumOfFileDownload = 0
          CurrentDownloadProgress = 0.0 }
      StateDeletes =
        { DeletedFile = None
          IsInDelete = false
          SumOfFileDelete = 0
          CurrentDeleteProgress = 0.0 }
      CorruptedFiles = 0
      IsInUpdate = false
      ProgrammProcess = None
      Disk = YandexDisk
      Version = NotYeatCheck },
    Cmd.ofSub (fun (dispatch: Message -> unit) -> initFunction dispatch |> Async.Start)

  module Update =

    let private setCurrentDownloadFile file state =
      { state with
          StateDownloads =
            { state.StateDownloads with
                DownloadedFile = file } }

    let private increaseDownloadedCounter state =
      { state with
          StateDownloads =
            { state.StateDownloads with
                CurrentDownloadProgress = state.StateDownloads.CurrentDownloadProgress + 1.0 } }

    let private increaseCorruptedFiles state =
      { state with
          CorruptedFiles = state.CorruptedFiles + 1 }

    let private setCurrentProgramProcess newProcess (state: State) =
      { state with
          ProgrammProcess = newProcess }

    let private setFilesToDownload filesCount state =
      { state with
          StateDownloads =
            { state.StateDownloads with
                SumOfFileDownload = filesCount } }

    let private startUpdateLauncher state = { state with IsInUpdate = true }

    let private switchDisk state = { state with Disk = state.Disk.Change }

    let private downloadFinished state =
      { state with
          StateDownloads =
            { state.StateDownloads with
                IsInDownload = false }
          IsInUpdate = false }

    let private startDownload state =
      { state with
          StateDownloads =
            { state.StateDownloads with
                IsInDownload = true } }

    let private startUpdateFilesProcess state =
      { state with
          IsInUpdate = true
          StateDownloads =
            { state.StateDownloads with
                SumOfFileDownload = 0
                CurrentDownloadProgress = 0.0 }
          StateDeletes =
            { state.StateDeletes with
                SumOfFileDelete = 0
                CurrentDeleteProgress = 0.0 } }

    let private setVersionLauncher version state = { state with Version = version }

    let private setCurrentDeletedFile file state =
      { state with
          StateDeletes =
            { state.StateDeletes with
                DeletedFile = file } }

    let private increaseDeletedCounter state =
      { state with
          StateDeletes =
            { state.StateDeletes with
                CurrentDeleteProgress = state.StateDeletes.CurrentDeleteProgress + 1.0 } }

    let private deleteFinished state =
      { state with
          StateDeletes =
            { state.StateDeletes with
                IsInDelete = false } }

    let private startDelete state =
      { state with
          StateDeletes =
            { state.StateDeletes with
                IsInDelete = true } }

    let private setFilesToDelete count state =
      { state with
          StateDeletes =
            { state.StateDeletes with
                SumOfFileDelete = count } }

    let private finished state =
      { state with
          StateDownloads =
            { state.StateDownloads with
                IsInDownload = false }
          StateDeletes =
            { state.StateDeletes with
                IsInDelete = false }
          IsInUpdate = false }

    let private checkFiles state = { state with IsInUpdate = true }
    
    let private choosePath state = state
    
    let private printCorruptedFiles state = state

    let update (msg: Message) state =
      match msg with
      | SetCurrentDownloadFile file -> setCurrentDownloadFile file state, Cmd.none
      | IncreaseDownloadedCounter -> increaseDownloadedCounter state, Cmd.none
      | IncreaseCorruptedCounter -> increaseCorruptedFiles state, Cmd.none
      | SetFilesToDownload filesCount -> setFilesToDownload filesCount state, Cmd.none
      | DownloadFinished -> downloadFinished state, Cmd.none
      | StartDownload -> startDownload state, Cmd.none
      | PrintCorruptedFiles ->
          printCorruptedFiles state,
          Cmd.ofSub
            (fun (dispatch: Message -> unit) ->
              if state.CorruptedFiles > 0 then
                Utils.SetCPP dispatch
                <| sprintf "We have %d corrupted files, try download adain" state.CorruptedFiles)

      | SetCurrentDeleteFile file -> setCurrentDeletedFile file state, Cmd.none
      | IncreaseDeletedCounter -> increaseDeletedCounter state, Cmd.none
      | SetFilesToDelete filesCount -> setFilesToDelete filesCount state, Cmd.none
      | DeleteFinished -> deleteFinished state, Cmd.none
      | StartDelete -> startDelete state, Cmd.none

      | StartUpdateFilesProcess ->
          startUpdateFilesProcess state,
          Cmd.ofSub
            (fun (dispatch: Message -> unit) ->
              startUpdateFilesProcessFunction state.Disk dispatch
              |> Async.Start)
      | SetCurrentProgramProcess newProcess -> setCurrentProgramProcess newProcess state, Cmd.none
      | StartUpdateLauncher ->
          startUpdateLauncher state,
          Cmd.ofSub
            (fun (dispatch: Message -> unit) ->
              startUpdateLauncherFunction dispatch
              |> Async.Start)
      | SetVersionLauncher version -> setVersionLauncher version state, Cmd.none
      | SwitchDisk -> switchDisk state, Cmd.none
      | Finished ->
          finished state,
          Cmd.ofSub
            (fun (dispatch: Message -> unit) ->
              None |> SetCurrentDeleteFile |> dispatch
              None |> SetCurrentDownloadFile |> dispatch)
      | CheckFiles ->
          checkFiles state, Cmd.ofSub (fun (dispatch: Message -> unit) -> checkFilesFunction dispatch |> Async.Start)
      | ChoosePath ->
        choosePath state, Cmd.ofSub (fun (dispatch: Message -> unit) ->
          choosePathFunction (OpenFolderDialog()) (Window()) dispatch
          |>Async.Start)

  module View =

    module private ProgressBars =
      let progressBarDownloadState (state: State) (dispatch: Message -> unit) =
        let progress () =
          if state.StateDownloads.IsInDownload then
            state.StateDownloads.CurrentDownloadProgress
            / ((float state.StateDownloads.SumOfFileDownload)
               / 100.0)
          elif state.StateDeletes.IsInDelete then
            state.StateDeletes.CurrentDeleteProgress
            / ((float state.StateDeletes.SumOfFileDelete) / 100.0)
          else
            100.0

        ProgressBar.create [ Grid.row 3
                             Grid.column 1
                             Grid.columnSpan 8
                             ProgressBar.isVisible true //state.IsDownload
                             ProgressBar.value <| progress ()
                             ProgressBar.classes [ "downloadbar" ]
                             ProgressBar.showProgressText
                             <| (state.StateDeletes.IsInDelete
                                 || state.StateDownloads.IsInDownload) ]

    module private TextBlocks =

      let textBlockTitleApp (state: State) (dispatch: Message -> unit) =
        TextBlock.create [ TextBlock.dock Dock.Top
                           TextBlock.isHitTestVisible false
                           TextBlock.text "Saturn Updater"
                           TextBlock.classes [ "appname" ] ]

      let textBlockDownloadsFile (state: State) (dispatch: Message -> unit) =
        TextBlock.create [ Grid.column 0
                           Grid.row 2
                           if state.StateDownloads.IsInDownload then
                             TextBlock.text
                             <| sprintf
                                  "Download %d of %d"
                                  (int state.StateDownloads.CurrentDownloadProgress)
                                  state.StateDownloads.SumOfFileDownload
                           if state.StateDeletes.IsInDelete then
                             TextBlock.text
                             <| sprintf
                                  "Delete %d of %d"
                                  (int state.StateDeletes.CurrentDeleteProgress)
                                  state.StateDeletes.SumOfFileDelete
                           Grid.columnSpan 10
                           TextBlock.classes [ "launcherupdate" ] ]

      let textBlockDownloadedFile (state: State) (dispatch: Message -> unit) =
        TextBlock.create [ Grid.column 1
                           Grid.row 5
                           if state.StateDownloads.DownloadedFile.IsSome then
                             TextBlock.text state.StateDownloads.DownloadedFile.Value
                           if state.StateDeletes.DeletedFile.IsSome then
                             TextBlock.text state.StateDeletes.DeletedFile.Value
                           Grid.columnSpan 9
                           TextBlock.classes [ "currentoperation" ] ]

      let textBlockLauncherVersion (state: State) (dispatch: Message -> unit) =
        TextBlock.create [ Grid.column 0
                           Grid.row 8
                           match state.Version with
                           | Update -> TextBlock.text "Launcher is up to date"
                           | Outdate -> TextBlock.text "New version available, can update"
                           | NotYeatCheck -> TextBlock.text "Check launcher version..."
                           Grid.columnSpan 5
                           TextBlock.classes [ "launcherupdate" ] ]

      let textBlockProgramPath (state: State) (dispatch: Message -> unit) =
        TextBlock.create [ Grid.column 0
                           Grid.row 9
                           TextBlock.text <| System.IO.Directory.GetCurrentDirectory()
                           Grid.columnSpan 7
                           TextBlock.classes [ "path" ] ]

      let textBlockProgrammProcess (state: State) (dispatch: Message -> unit) =
        TextBlock.create [ Grid.column 1
                           Grid.row 4
                           if state.ProgrammProcess.IsSome then
                             TextBlock.text state.ProgrammProcess.Value
                           Grid.columnSpan 9
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
                        Grid.row 8
                        Button.isHitTestVisible true
                        Button.classes [ "main"; "update" ]
                        Button.content "Update launcher"
                        Button.isEnabled
                        <| ((not state.IsInUpdate)
                            && not (state.Version = NotYeatCheck)
                            && not (state.StateDownloads.IsInDownload))
                        Button.onClick (fun _ -> dispatch StartUpdateLauncher) ]

      let buttonDownload (state: State) (dispatch: Message -> unit) =
        Button.create [ Grid.column 9
                        Grid.row 9
                        Button.isHitTestVisible true
                        Button.classes [ "main"; "update" ]
                        Button.content "Download"
                        Button.isEnabled
                        <| ((not state.IsInUpdate)
                            && not (state.Version = NotYeatCheck)
                            && not (state.StateDownloads.IsInDownload))
                        Button.onClick (fun _ -> dispatch StartUpdateFilesProcess) ]

      let buttonFileCheck (state: State) (dispatch: Message -> unit) =
        Button.create [ Grid.column 8
                        Grid.row 9
                        Button.isHitTestVisible true
                        Button.classes [ "main"; "filescheck" ]
                        Button.content "Check files"
                        Button.isEnabled
                        <| ((not state.IsInUpdate)
                            && not (state.Version = NotYeatCheck)
                            && not (state.StateDownloads.IsInDownload))
                        Button.onClick
                          (fun _ -> dispatch CheckFiles) ]

      let buttonChoosePath (state: State) (dispatch: Message -> unit) =
        Button.create [ Grid.column 8
                        Grid.row 8
                        Button.isHitTestVisible true
                        Button.classes [ "main"; "filescheck" ]
                        Button.content "Choose path"
                        Button.isEnabled
                        <| ((not state.IsInUpdate)
                            && not (state.Version = NotYeatCheck)
                            && not (state.StateDownloads.IsInDownload))
                        Button.onClick
                          (fun _ -> dispatch ChoosePath) ]

    module private RadioButtons =

      let radioButtonDropbox (state: State) (dispatch: Message -> unit) =
        RadioButton.create [ Grid.column 9
                             Grid.row 7
                             RadioButton.classes [ "diskradio" ]
                             RadioButton.content "Dropbox"
                             RadioButton.isChecked state.Disk.IsDropbox
                             RadioButton.isEnabled
                             <| ((not state.IsInUpdate)
                                 && not (state.Version = NotYeatCheck)
                                 && not (state.StateDownloads.IsInDownload))
                             RadioButton.onChecked (fun _ -> dispatch SwitchDisk) ]

      let radioButtonYandex (state: State) (dispatch: Message -> unit) =
        RadioButton.create [ Grid.column 8
                             Grid.row 7
                             RadioButton.classes [ "diskradio" ]
                             RadioButton.content "Yandex"
                             RadioButton.isChecked state.Disk.IsYandex
                             RadioButton.isEnabled
                             <| ((not state.IsInUpdate)
                                 && not (state.Version = NotYeatCheck)
                                 && not (state.StateDownloads.IsInDownload))
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
                                      TextBlocks.textBlockProgramPath state dispatch
                                      Buttons.buttonDownload state dispatch
                                      Buttons.buttonUpdate state dispatch
                                      Buttons.buttonFileCheck state dispatch
                                      Buttons.buttonChoosePath state dispatch
                                      RadioButtons.radioButtonYandex state dispatch
                                      RadioButtons.radioButtonDropbox state dispatch ] ]

    let view (state: State) (dispatch: Message -> unit) =
      DockPanel.create [ DockPanel.children [ TextBlocks.textBlockTitleApp state dispatch
                                              Grids.grid state dispatch ] ]