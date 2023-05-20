namespace RequiemDodge

open NetScriptFramework
open NetScriptFramework.SkyrimSE
open Wrapper

open XInput.Wrapper

type Reflyem() =

  inherit Plugin()

  let mutable modInit = false
  let mutable enabledGamepad = false

  let mutable pressedTime = 0L

  let timer = Tools.Timer()

  override _.Key = "requiem_dodge"
  override _.Name = "RequiemDodge"
  override _.Author = "newrite"
  override _.RequiredLibraryVersion = 10
  override _.Version = 1

  override self.Initialize _ =
      
    self.init()
    true
      
  member private _.init() =

    let logger = GetLogger "RequiemDodge"
    
    DodgeSettings.Load()
    |> function
    | true ->  logger <| sprintf "Settings load"
    | false -> logger <| sprintf "Can't load settings, use default"

    let dodgeButton  = enum<Tools.VirtualKeys> DodgeSettings.DodgeButton
    let dodgeButtonGamepad =
      System.Enum.Parse(typedefof<X.Gamepad.ButtonFlags>, string DodgeSettings.GamepadDodgeButton)
      :?> X.Gamepad.ButtonFlags

    Events.OnMainMenu.Register(fun _ ->
      if not modInit then
        logger <| sprintf "Dodge spell id is %d modname is %s" DodgeSettings.DodgeSpellId DodgeSettings.ModName
        if DodgeSettings.DodgeSpell.IsSome then
          timer.Start()
          enabledGamepad <- DodgeSettings.EnableGamepad && X.Available
          logger <| sprintf "Keyboard button key - %A, Gamepad flag - %A" dodgeButton dodgeButtonGamepad
          logger <| sprintf "Enabled gamepad in setings %A, state of gamepad %A" DodgeSettings.EnableGamepad enabledGamepad
          modInit <- true
      ) |> ignore

    Events.OnFrame.Register(fun _ ->

      if modInit then

        if enabledGamepad then X.Gamepad1.Update() |> ignore

        let isPressed = Tools.Input.IsPressed dodgeButton || (enabledGamepad && X.Gamepad1.ButtonsState.HasFlag dodgeButtonGamepad)
        let time = timer.Time

        if isPressed && pressedTime = 0L then pressedTime <- time

        if not isPressed && pressedTime <> 0L then
          logger "Check Dodge"
          let canDodge = time - pressedTime <= DodgeSettings.ButtonPressTiming
          pressedTime <- 0L
          if canDodge then
            logger "Dodge"
            let player = Call.PlayerInstance()
            if player <> null && player.IsValid then
              player.CastSpell(DodgeSettings.DodgeDrainSpell.Value, player, player) |> ignore
              player.CastSpell(DodgeSettings.DodgeSpell.Value, player, player) |> ignore

      ) |> ignore