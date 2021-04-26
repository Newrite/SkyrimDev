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
    //base.Title <- "Launcher"
    base.Height <- 350.0
    base.Width <- 600.0
    //base.IsHitTestVisible <- false
    base.ExtendClientAreaToDecorationsHint <- true
    base.ExtendClientAreaTitleBarHeightHint <- -1.0
    //base.SystemDecorations <- SystemDecorations.BorderOnly
    base.TransparencyLevelHint <- WindowTransparencyLevel.AcrylicBlur
    base.ExtendClientAreaChromeHints <- ExtendClientAreaChromeHints.PreferSystemChrome

    this.VisualRoot.VisualRoot.Renderer.DrawFps <- true
    //this.VisualRoot.VisualRoot.Renderer.DrawDirtyRects <- true
    Elmish.Program.mkProgram GUI.Launcher.init GUI.Launcher.update GUI.Launcher.view
    |> Program.withHost this
    |> Program.withConsoleTrace
    |> Program.run

type App() =
  inherit Application()
  //FluentTheme(baseUri = null, Mode = FluentThemeMode.Light)
  override this.Initialize() =
    //this.Styles.Load "avares://Avalonia.Themes.Fluent/FluentDark.xaml"
    this.Styles.Load "avares://Avalonia.Themes.Default/DefaultTheme.xaml"
    this.Styles.Load "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml"
    this.Styles.Load "avares://Launcher/Styles.xaml"
  //this.Styles.Add(FluentTheme(uri ""))

  override this.OnFrameworkInitializationCompleted() =
    match this.ApplicationLifetime with
    | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
        let mainWindow = MainWindow()
        desktopLifetime.MainWindow <- mainWindow
    | _ -> ()

[<EntryPoint>]
let main args =

  AppBuilder
    .Configure<App>()
    .UsePlatformDetect()
    .UseSkia()
    .StartWithClassicDesktopLifetime(args)
