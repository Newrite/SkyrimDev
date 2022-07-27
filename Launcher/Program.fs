open Avalonia
open Avalonia.Controls
open Avalonia.Platform
open Elmish
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI
open Avalonia.FuncUI.Elmish
open Avalonia.Controls.ApplicationLifetimes
open Launcher


type MainWindow() as this =
  inherit HostWindow()

  do
    base.Height <- 375.0
    base.Width <- 625.0
    base.Classes <- Classes([ "mainwindow" ])
    base.ExtendClientAreaToDecorationsHint <- true
    base.ExtendClientAreaTitleBarHeightHint <- -1.0
    base.TransparencyLevelHint <- WindowTransparencyLevel.AcrylicBlur
    base.ExtendClientAreaChromeHints <- ExtendClientAreaChromeHints.PreferSystemChrome

    //this.VisualRoot.VisualRoot.Renderer.DrawFps <- true
    //this.VisualRoot.VisualRoot.Renderer.DrawDirtyRects <- true
    Elmish.Program.mkProgram Shell.Launcher.init Shell.Launcher.Update.update Shell.Launcher.View.view
    |> Program.withHost this
    |> Program.withConsoleTrace
    |> Program.run

type App() =
  inherit Application()

  override this.Initialize() =
    this.Styles.Load "avares://Avalonia.Themes.Default/DefaultTheme.xaml"
    this.Styles.Load "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml"
    //this.Styles.Add (FluentTheme(baseUri = null, Mode = FluentThemeMode.Dark))
    this.Styles.Load "avares://Launcher/Styles.axaml"

  override this.OnFrameworkInitializationCompleted() =
    match this.ApplicationLifetime with
    | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
        let mainWindow = MainWindow()
        desktopLifetime.MainWindow <- mainWindow
    | _ -> ()

[<EntryPoint>]
let main args =
  
  //let proc = System.Diagnostics.Process.GetCurrentProcess()
  //proc.MainWindowHandle

  AppBuilder
    .Configure<App>()
    .UsePlatformDetect()
    .UseSkia()
    .StartWithClassicDesktopLifetime(args)
