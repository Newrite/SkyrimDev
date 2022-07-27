namespace BloodMagick

open NetScriptFramework
open NetScriptFramework.SkyrimSE
open Wrapper
open Supporter
open System

[<AutoOpen>]
module private InternalVariables =
    let Log = Log.Init "Blood Magick"
    
    let Settings = BloodMagick.Settings()

[<AutoOpen>]
module BloodMagickPlugin =
    
    let costsDict = System.Collections.Concurrent.ConcurrentDictionary<nativeint,float32>()

    [<RequireQualifiedAccess>]
    type MenuState =
      | MagicMenuOpen
      | MenuClosing
      | WaitOpen

type BloodMagickPlugin() =

    inherit Plugin()
    let mutable modInit = false
    let timer = Tools.Timer()
    let mutable lastFrameTimer = 0L
    let mutable menuState = MenuState.WaitOpen
    let mutable isDualCasting = false

    override self.Key = "bloodmagick"
    override self.Name = "Blood Magick"
    override self.Author = "newrite"
    override self.RequiredLibraryVersion = 10
    override self.Version = 1


    override self.Initialize _ =
        self.init()
        true
        
    member private self.CheckPlayer(actor: #Actor) =
      actor.HasKeywordText(@"PlayerKeyword")
        
    member private self.init() =
        
        Settings.Load()
        |>function
        | true -> Log <| sprintf "settings load"
        | false -> Log <| sprintf "Can't load settings, use def"


        let failCast() =
        //  
        //  let showMessage() =
        //
        //    isDualCasting <- false
        //
        //    //let fnAddr_FailedMsg = Main.GameInfo.GetAddressOf(11295uL)
        //    //if fnAddr_FailedMsg <> IntPtr.Zero then
        //    //  let msg = Call.InvokeCdecl(fnAddr_FailedMsg, 1)
        //    //  if msg <> IntPtr.Zero then
        //    //    let str = Memory.ReadString(msg, false)
        //    //    if str <> null && str.Length > 0 then
        //    //      Log <| sprintf "MSG FAIL before: %s" str
        //    //      let c1251 = Text.Encoding.GetEncoding("windows-1251")
        //    //      let utf8 = Text.Encoding.GetEncoding("UTF-8")
        //    //      let bytes = Text.Encoding.Convert(utf8, c1251, c1251.GetBytes(str))
        //    //      let newStr = c1251.GetString(bytes)
        //    //      Log <| sprintf "MSG FAIL after: %s" newStr
        //    //      Call.MessageHUD(newStr, null, true)
        //    Call.MessageHUD("Недостаточно здоровья для произношения заклинания.", null, true)
        
          let playSound() =
            let file = @"Skyrim.esm"
            let idSD = uint32 0x3D0D3
            let player = Call.PlayerInstance()
            let snd = Call.TESFormLookupFormFromFile(idSD, file)
            let fnAddr_BGSSoundDescriptor_PlaySound = Main.GameInfo.GetAddressOf(32301uL)
            if fnAddr_BGSSoundDescriptor_PlaySound <> IntPtr.Zero
               && snd <> null && player <> null && player.Node <> null then
              Call.InvokeCdecl(fnAddr_BGSSoundDescriptor_PlaySound, snd.Address, 0, player.Position.Address, player.Node.Address) 
              |> ignore

          isDualCasting <- false
          flashHudMeter Settings.indexState
          Call.MessageHUD("Недостаточно здоровья для произношения заклинания.", null, true)
          playSound()

        let isConcentrationSpell (magicItem: MagicItem) =
          match magicItem with
          | :? SpellItem as spell ->
              if spell.AVEffectSetting <> null then
                spell.AVEffectSetting.CastingType = EffectSettingCastingTypes.Concentration
              else 
                false
          | _ -> false

        let magicItemNotNullAndIsSpell (magicItem: MagicItem) =
          if magicItem.FormType = FormTypes.Shout
             || magicItem.FormType = FormTypes.ScrollItem 
             || magicItem.AVEffectSetting = null then 
            false
          else
            match magicItem with
            | null -> false
            | :? TESShout as _ -> false
            | :? ScrollItem as _ -> false
            | :? EnchantmentItem as _ -> false
            | :? SpellItem as s -> s.SpellData.SpellType = SpellTypes.Spell
            | _ -> false
            
        let isUnderBloodMagickAndHasSpell (caster: Actor) (magicItem: MagicItem) =
            caster <> null && caster.IsValid && caster.HasSpell(Settings.BloodMagickSpell) && caster.HasSpell(magicItem)

        let isValidActorCaster (caster: ActorMagicCaster) =
          caster <> null && caster.IsValid && caster.Owner <> null && caster.Owner.IsValid

        Events.OnMainMenu.Register(fun _ ->
          if not modInit then modInit <- true
          ) |> ignore

        Events.OnInterruptCast.Register(fun eArg ->
          //Log <| sprintf "In interrapt cast"
          if eArg.Caster <> null && eArg.Caster.IsValid then
            match eArg.Caster with
            | :? ActorMagicCaster as amc ->
              //Log <| sprintf "Cast to ActorMagicCaster"
              if amc <> null && amc.IsValid && amc.Owner <> null && amc.Owner.IsValid && amc.Owner.HasSpell(Settings.BloodMagickSpell) then
                isDualCasting <- false
            | :? Actor as actor ->
              //Log <| sprintf "Cast to Actor"
              if actor <> null && actor.IsValid && actor.HasSpell(Settings.BloodMagickSpell) then
                isDualCasting <- false
            | _ -> () //Log <| sprintf "No casting in interrapt"
          ) |> ignore

        Events.OnFrame.Register(fun _ ->

          if not modInit then
            ()
          else

          let isMenuOpen = Call.MenuManagerInstance().IsMenuOpen("MagicMenu")

          match menuState with
          | MenuState.MenuClosing ->
            menuState <- MenuState.WaitOpen
            Call.PlayerInstance().ModActorValue(ActorValueIndices.IllusionMod, 0.5f)
            Call.PlayerInstance().ModActorValue(ActorValueIndices.IllusionMod, -0.5f)
          | MenuState.MagicMenuOpen ->
            if not isMenuOpen then
              menuState <- MenuState.MenuClosing
          | MenuState.WaitOpen ->
            if isMenuOpen then
              menuState <- MenuState.MagicMenuOpen

          ) |> ignore
    
        Events.OnCalculateMagicCost.Register(fun eArg ->

          if magicItemNotNullAndIsSpell eArg.Item 
             && isConcentrationSpell eArg.Item |> not 
             && isUnderBloodMagickAndHasSpell eArg.Caster eArg.Item then

            costsDict.AddOrUpdate(eArg.Item.Address, eArg.ResultValue, fun _ _ -> eArg.ResultValue)
            |> ignore

            if Call.MenuManagerInstance().IsMenuOpen("MagicMenu") then
              ()
            else 
              eArg.ResultValue <- 0.f

          ) |> ignore
        
        Events.OnSpendMagicCost.Register(fun eArg ->

          let av = enum<ActorValueIndices>(Settings.indexState)
          
          let validCasterUnderBloodMagickAndIsSpell = 
            isValidActorCaster eArg.Spender 
            && magicItemNotNullAndIsSpell eArg.Item 
            && isUnderBloodMagickAndHasSpell eArg.Spender.Owner eArg.Item

          if validCasterUnderBloodMagickAndIsSpell && isConcentrationSpell eArg.Item then

            if eArg.Spender.Owner.GetActorValue(av) <= 5.f then
              failCast()
              eArg.Spender.InterruptCast()
              //flashHudMeter Settings.indexState
              //Call.MessageHUD("Not enough health to cast", "MAGFail", true)
            else
              eArg.Spender.Owner.SetAvRegenDelay Settings.indexState 0.1f
              eArg.ActorValueIndex <- Settings.indexState

          if validCasterUnderBloodMagickAndIsSpell && isConcentrationSpell eArg.Item |> not
            && costsDict.ContainsKey(eArg.Item.Address) then

            let dualCast = eArg.Spender.Owner.IsDualCasting()
            //Log <| sprintf "DualCast %A" dualCast
            isDualCasting <- dualCast
            let mult = if isDualCasting then 2.f else 1.f
            if eArg.Spender.Owner.GetActorValue(av) <= (costsDict[eArg.Item.Address] * mult) then
              failCast()
              eArg.Spender.InterruptCast()
            else
              eArg.Spender.Owner.SetAvRegenDelay Settings.indexState 0.1f

          ) |> ignore
        
        Events.OnMagicCasterFire.Register(fun eArg ->
          //Log <| sprintf "Magic caster fire"
          let mult = if isDualCasting then 2.f else 1.f

          if costsDict.ContainsKey(eArg.Item.Address)
             && eArg.Caster <> null 
             && eArg.Caster.IsValid 
             && magicItemNotNullAndIsSpell eArg.Item
             && isConcentrationSpell eArg.Item |> not then

            let cost = costsDict[eArg.Item.Address] * mult
            match eArg.Caster with
            | :? ActorMagicCaster as c ->
                if isUnderBloodMagickAndHasSpell c.Owner eArg.Item then 
                  c.Owner.DamageActorValue(ActorValueIndices.Health, -cost)
            | _ -> ()

          ) |> ignore