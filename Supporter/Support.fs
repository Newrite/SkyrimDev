namespace Supporter

open NetScriptFramework
open NetScriptFramework.SkyrimSE
open Wrapper
open System

[<AutoOpen>]
module Utils =

    [<RequireQualifiedAccess>]
    type OnAnimation =
        | WeaponSwingRight
        | WeaponSwingLeft
        | WeaponSwingRightPower
        | WeaponSwingLeftPower
        | Jump
        | BowDraw
        | ArrowRelease
        | BowReset
        | BoltRelease
        | ReloadStart
        | ReloadStop
        | WeapEquipOut

    let inline (^) f x = f x
    
    //Функция для логирования, пишет в лог нетскрипта и выводит такую же нотификацию
    module Log =
        let Init modName msg =

            #if DEBUG
                let l = Main.Log
        
                if l <> null then
                    l.AppendLine(modName+": "+ msg)
        
                Main.WriteDebugMessage(msg)
                Call.MessageHUD(msg, null, true)
            #else
                let l = Main.Log
        
                if l <> null then
                    l.AppendLine(modName+": "+ msg)
        
                Main.WriteDebugMessage(msg)
            #endif

    [<RequireQualifiedAccess>]
    type WeaponSlot =
        | Right
        | Left

        member self.Int =
            match self with
            | Right -> 0
            | Left -> 1

    type Actor with

        member self.GetEquippedWeapon (slot: WeaponSlot) =
            let addr_GetEquippedWeap = Main.GameInfo.GetAddressOf(38781uL)
            let item = MemoryObject.FromAddress<ExtraContainerChanges.ItemEntry>(Call.InvokeCdecl(addr_GetEquippedWeap, self.Process.Cast<ActorProcess>(), slot.Int))
            if item <> null then
                let weap = item.Template :?> TESObjectWEAP
                if weap <> null then Some weap else None
            else
                None

        member self.IsDualCasting () =
          let addr_IsDualCasting = Main.GameInfo.GetAddressOf(37815uL)
          Call.InvokeCdecl(addr_IsDualCasting, self.Cast<TESObjectREFR>()).ToBool()

        member self.SetAvRegenDelay (av: int) (amount: float32) =
          let addr_SetAVRegenDelay = Main.GameInfo.GetAddressOf(38526uL)
          Call.InvokeCdecl(addr_SetAVRegenDelay, self.Process.Cast<ActorProcess>(), av, amount) |> ignore

    let getCurrentGameTime() =
      let addr_GetCurrentGameTime = Main.GameInfo.GetAddressOf(56475uL)
      Call.InvokeCdeclF(addr_GetCurrentGameTime)

    let realHoursPassed() =
      let addr_RealHoursPassed = Main.GameInfo.GetAddressOf(54842uL)
      Call.InvokeCdeclF(addr_RealHoursPassed)

    let flashHudMeter (av: int) =
      let addr_FlashHudMeter = Main.GameInfo.GetAddressOf(51907uL)
      Call.InvokeCdecl(addr_FlashHudMeter, av) |> ignore

    let playSound () = 52054


[<AutoOpen>]
module private InternalVariable =
    let Settings = new Settings()
    let Log = Log.Init "Supporter"


module ExtEvent =
    
    type AnimArgs(animation: OnAnimation, source: Actor) =

        inherit EventArgs()
        let anim = animation
        let src = source
    
        member self.Anim = anim
        member self.Source = src
    
    type AnimDelegate = delegate of obj * AnimArgs -> unit
    
    let animationRising =
        new Event<AnimDelegate, AnimArgs>()

    let OnAnimationEvent = animationRising.Publish

type SupporterPlugin() =

    inherit Plugin()

    let mutable init = false

    override self.Key = "supporterplugin"
    override self.Name = "Supporter"
    override self.Author = "newrite"
    override self.RequiredLibraryVersion = 10
    override self.Version = 1

    override self.Initialize loadedAny =
        self.init()
        true

    member private self.init() =

        InternalVariable.Settings.Load()
        |>function
        |true -> ()
        |false -> Log <| sprintf "Can't load settings"


        let inline animRegister (animation: OnAnimation) (actor: #Actor) =
          ExtEvent.animationRising.Trigger(self, new ExtEvent.AnimArgs(animation, actor))

        Events.OnMagicCasterFire.Register((fun eArg ->

            let someCaster =
                match eArg.Caster with
                | :? ActorMagicCaster as c -> Some c
                | _ -> None
            let someSpell =
                match eArg.Item with
                | :? SpellItem as s -> Some s
                | _ -> None

            match (someCaster, someSpell, init) with
            | (Some caster, Some spell, true) ->
                match spell with
                | _ when spell = Settings.SpellArrowRelease.Value ->
                        animRegister OnAnimation.ArrowRelease caster.Owner
                | _ when spell = Settings.SpellBoltRelease.Value ->
                        animRegister OnAnimation.BoltRelease caster.Owner
                | _ when spell = Settings.SpellBowDraw.Value ->
                        animRegister OnAnimation.BowDraw caster.Owner
                | _ when spell = Settings.SpellBowReset.Value ->
                        animRegister OnAnimation.BowReset caster.Owner
                | _ when spell = Settings.SpellJump.Value ->
                        animRegister OnAnimation.Jump caster.Owner
                | _ when spell = Settings.SpellReloadStart.Value ->
                        animRegister OnAnimation.ReloadStart caster.Owner
                | _ when spell = Settings.SpellReloadStop.Value ->
                        animRegister OnAnimation.ReloadStop caster.Owner
                | _ when spell = Settings.SpellWeapEquipOut.Value ->
                        animRegister OnAnimation.WeapEquipOut caster.Owner
                | _ when spell = Settings.SpellWeaponSwingLeft.Value ->
                        animRegister OnAnimation.WeaponSwingLeft caster.Owner
                | _ when spell = Settings.SpellWeaponSwingLeftPower.Value ->
                        animRegister OnAnimation.WeaponSwingLeftPower caster.Owner
                | _ when spell = Settings.SpellWeaponSwingRight.Value ->
                        animRegister OnAnimation.WeaponSwingRight caster.Owner
                | _ when spell = Settings.SpellWeaponSwingRightPower.Value ->
                        animRegister OnAnimation.WeaponSwingRightPower caster.Owner
                | _ -> ()
            | _ -> ()

            ())) |> ignore

        let handlerGlobal (globalVal: TESGlobal) animation actor =
          if globalVal.FloatValue > 0.0f then
            globalVal.FloatValue <- globalVal.FloatValue - 1.0f
            animRegister animation actor

        Events.OnFrame.Register(fun eArg ->

            if init then
                handlerGlobal Settings.GlobalArrowRelease.Value OnAnimation.ArrowRelease (Call.PlayerInstance() :> Actor)
                handlerGlobal Settings.GlobalBoltRelease.Value OnAnimation.BoltRelease (Call.PlayerInstance() :> Actor)
                handlerGlobal Settings.GlobalBowDraw.Value OnAnimation.BowDraw (Call.PlayerInstance() :> Actor)
                handlerGlobal Settings.GlobalBowReset.Value OnAnimation.BowReset (Call.PlayerInstance() :> Actor)
                handlerGlobal Settings.GlobalJump.Value OnAnimation.Jump (Call.PlayerInstance() :> Actor)
                handlerGlobal Settings.GlobalReloadStart.Value OnAnimation.ReloadStart (Call.PlayerInstance() :> Actor)
                handlerGlobal Settings.GlobalReloadStop.Value OnAnimation.ReloadStop (Call.PlayerInstance() :> Actor)
                handlerGlobal Settings.GlobalWeapEquipOut.Value OnAnimation.WeapEquipOut (Call.PlayerInstance() :> Actor)
                handlerGlobal Settings.GlobalWeaponSwingLeft.Value OnAnimation.WeaponSwingLeft (Call.PlayerInstance() :> Actor)
                handlerGlobal Settings.GlobalWeaponSwingLeftPower.Value OnAnimation.WeaponSwingLeftPower (Call.PlayerInstance() :> Actor)
                handlerGlobal Settings.GlobalWeaponSwingRight.Value OnAnimation.WeaponSwingRight (Call.PlayerInstance() :> Actor)
                handlerGlobal Settings.GlobalWeaponSwingRightPower.Value OnAnimation.WeaponSwingRightPower (Call.PlayerInstance() :> Actor)
          ) |> ignore

        Events.OnMainMenu.Register((fun _ ->
        
            if not init then
                Log <| sprintf "Init in main menu - Ok"
                if List.contains None Settings.EventSpellList || List.contains None Settings.EventGlobalsList then
                    Log <| sprintf "One or more global or spell event in settings is None"
                else
                    init <- true
            ), flags = EventRegistrationFlags.Distinct) |> ignore

        ()