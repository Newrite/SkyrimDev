﻿namespace BloodMagick

open NetScriptFramework
open NetScriptFramework.SkyrimSE
open Wrapper
open Supporter
open System

open Extensions

[<AutoOpen>]
module private InternalBloodSpells =

  let indexAvDrainHealth = 24

  let magFailSoundId = uint32 0x3D0D3

  let bloodMagickAbilityId = uint32 0x23F6CCu

  let skyrim = @"Skyrim.esm"

  let bloodMagickMod = "Requiem - Breaking Bad.esp"

  let castHealthTreshold = 15.f

  let bloodMagickAbility() = Call.TESFormLookupFormFromFile(bloodMagickAbilityId, bloodMagickMod) :?> SpellItem

  let Log = Log.Init "BloodMagick"

open InternalBloodSpells

[<RequireQualifiedAccess>]
module BloodMagickPlugin =

  module Domen =
  
    type CastCostHandler =
      { mutable Accum: float32
        mutable Cost: float32
        mutable Working: bool 
        mutable IterMax: float32
        mutable IterNow: float32 }
    
    with 
      static member Create() =
        { Accum = 0.f
          Cost = 0.f
          Working = false
          IterMax = 0.f
          IterNow = 0.f}

      member self.ResetToWork() =
        self.Accum <- 0.f
        self.Cost <- 0.f
        self.IterMax <- 0.f
        self.IterNow <- 0.f
        self.Working <- false

      member self.StartWorking numberOfIter costForIter =
        self.Accum <- 0.f
        self.Cost <- costForIter
        self.IterMax <- numberOfIter
        self.IterNow <- 0.f
        self.Working <- true

      member self.InterruptHandle (asyncRestore: float32 -> Actor -> Async<unit>) actor =
        if self.Accum > 0.f then
          asyncRestore self.Accum actor |> Async.Start
        self.ResetToWork()

      member self.IterationHandle (failCast: unit -> unit) (actor: Actor) =
        self.IterNow <- self.IterNow + 1.f
        if actor.GetActorValue(ActorValueIndices.Health) <= self.Cost then
          actor.InterruptCast()
          if actor.IsPlayer then
            failCast()
          self.IterNow <- self.IterMax
        else
          actor.DamageActorValue(ActorValueIndices.Health, -self.Cost)
          self.Accum <- self.Accum + self.Cost

    [<NoComparison>]
    type CastsHandHandler =
      { Caster: Actor
        DualCast: CastCostHandler
        LeftCast: CastCostHandler
        RightCast: CastCostHandler }
    
    with 
      static member Create caster =
        { Caster = caster
          DualCast = CastCostHandler.Create()
          LeftCast = CastCostHandler.Create()
          RightCast = CastCostHandler.Create() }
    
    type CostDict = Collections.Concurrent.ConcurrentDictionary<nativeint,float32>
  
    [<RequireQualifiedAccess>]
    type MenuState =
      | MagicMenuOpen
      | MenuClosing
      | WaitOpen

  module private Functions = 

    let getNumberOfIterationsAndCostForIter chargetTime cost =

      let numIter = 
        if chargetTime <= 0.f then 
          (0.05f * 1000.f) / 10.f 
        else
          (chargetTime * 1000.f) / 10.f

      let costForIter = cost / numIter

      (numIter, costForIter)

    let failCast() =

      let sound = Call.TESFormLookupFormFromFile(magFailSoundId, skyrim)
      let player = Call.PlayerInstance()

      flashHudMeter indexAvDrainHealth
      Call.MessageHUD("Недостаточно здоровья для произношения заклинания.", null, true)
      playSound sound player

    let canSpendOrFailIterruptIfNot (spender: ActorMagicCaster) cost =
      if spender.Owner.GetActorValue(ActorValueIndices.Health) <= cost then
        if spender.Owner.IsPlayer then
          failCast()
        spender.InterruptCast()
        false
      else
        true

    let failCastWithHP() =

      Call.MessageHUD("Я не могу произнести это заклинание...", null, true)
      let sound = Call.TESFormLookupFormFromFile(magFailSoundId, skyrim)
      let player = Call.PlayerInstance()
      playSound sound player

    let isConcentrationSpell (magicItem: MagicItem) =
      match magicItem with
      | :? SpellItem as spell ->
        let isConcFromSpellData = 
          if spell.SpellData <> null then
            spell.SpellData.CastingType = EffectSettingCastingTypes.Concentration
          else
            false
        let isConcFromEffect =
          if spell.AVEffectSetting <> null then
            spell.AVEffectSetting.CastingType = EffectSettingCastingTypes.Concentration
          else 
            false
        isConcFromEffect || isConcFromSpellData
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

    let isActorMagicCasterValidAndUnderBloodMagick (caster: ActorMagicCaster) (magicItem: MagicItem) =
      caster <> null && caster.IsValid && caster.Owner <> null && caster.Owner.IsValid 
      && magicItemNotNullAndIsSpell magicItem && caster.Owner.HasSpell(bloodMagickAbility())
    
    let isActorValidAndUnderBloodMagick (actor: Actor) (magicItem: MagicItem) =
      actor <> null && actor.IsValid && magicItemNotNullAndIsSpell magicItem 
      && actor.HasSpell(bloodMagickAbility())

    let asyncHandleRestore amountRestore (caster: Actor) = async {
      let mutable alreadyRestore = 0.f
      let restoreForIter = amountRestore / 25.f
      while alreadyRestore < amountRestore do
        caster.RestoreActorValue(ActorValueIndices.Health, restoreForIter)
        alreadyRestore <- alreadyRestore + restoreForIter
        do! Async.Sleep(10)
    }

  let castersDict = Collections.Concurrent.ConcurrentDictionary<nativeint, Domen.CastsHandHandler * Domen.CostDict>()

  let mutable menuState = Domen.MenuState.WaitOpen

  let init() =
    let bm = bloodMagickAbility()
    if bm <> null && bm.IsValid then
      Log <| sprintf "BloodMagick Ok"
      true
    else
      Log <| sprintf "BloodMagick null ability"
      false

  let onInterruptCast (eArg: InterruptCastEventArgs )=
    //Log <| sprintf "In interrapt cast"
    if eArg.Caster <> null && eArg.Caster.IsValid then
      match eArg.Caster with
      | :? ActorMagicCaster as amc ->
        //Log <| sprintf "Cast to ActorMagicCaster"

        if amc <> null && amc.IsValid && amc.Owner <> null && amc.Owner.IsValid && castersDict.ContainsKey(amc.Owner.Address) then

          let castHandler, _ = castersDict[amc.Owner.Address]

          castHandler.DualCast.InterruptHandle Functions.asyncHandleRestore amc.Owner

          if amc.ActorCasterType = EquippedSpellSlots.RightHand then
            castHandler.RightCast.InterruptHandle Functions.asyncHandleRestore amc.Owner

          if amc.ActorCasterType = EquippedSpellSlots.LeftHand then
            castHandler.LeftCast.InterruptHandle Functions.asyncHandleRestore amc.Owner

      | :? Actor as actor ->
        //Log <| sprintf "Cast to Actor"
        if actor <> null && actor.IsValid && castersDict.ContainsKey(actor.Address) then

          let castHandler, _ = castersDict[actor.Address]

          castHandler.DualCast.InterruptHandle Functions.asyncHandleRestore actor

      | _ -> () //Log <| sprintf "No casting in interrapt"

  let onFrameAlways() =
    for key in castersDict.Keys do
      let castHandler, _ = castersDict[key]

      if castHandler.Caster <> null && castHandler.Caster.IsValid 
         && castHandler.Caster.HasSpell(bloodMagickAbility()) && castHandler.Caster.GetActorValue(ActorValueIndices.Health) <= castHealthTreshold then
        for caster in castHandler.Caster.MagicCasters do
          let validCaster = caster <> null && caster.IsValid
          if validCaster && caster.State = MagicCastingStates.Concentrating then
            caster.InterruptCast()
            Functions.failCast()

  let onFrame10ms() =
    let isMenuOpen = Call.MenuManagerInstance().IsMenuOpen("MagicMenu")

    let player = Call.PlayerInstance()

    if player <> null then

      match menuState with
      | Domen.MenuState.MenuClosing ->
        menuState <- Domen.MenuState.WaitOpen
        player.ModActorValue(ActorValueIndices.IllusionMod, 0.5f) // Call this for 
        player.ModActorValue(ActorValueIndices.IllusionMod, -0.5f) // force recalc magick cost
      | Domen.MenuState.MagicMenuOpen ->
        if not isMenuOpen then
          menuState <- Domen.MenuState.MenuClosing
      | Domen.MenuState.WaitOpen ->
        if isMenuOpen then
          menuState <- Domen.MenuState.MagicMenuOpen

    for key in castersDict.Keys do
      let castHandler, _ = castersDict[key]

      if castHandler.Caster <> null && castHandler.Caster.IsValid then

        if castHandler.DualCast.Working && castHandler.DualCast.IterNow < castHandler.DualCast.IterMax then
          castHandler.DualCast.IterationHandle Functions.failCast castHandler.Caster

        if castHandler.LeftCast.Working && castHandler.LeftCast.IterNow < castHandler.LeftCast.IterMax then
          castHandler.LeftCast.IterationHandle Functions.failCast castHandler.Caster

        if castHandler.RightCast.Working && castHandler.RightCast.IterNow < castHandler.RightCast.IterMax then
          castHandler.RightCast.IterationHandle Functions.failCast castHandler.Caster

  let onCalculateMagickCost (eArg: CalculateMagicCostEventArgs) =

    let allow = Functions.isActorValidAndUnderBloodMagick eArg.Caster eArg.Item && Functions.isConcentrationSpell eArg.Item |> not

    if not allow then
      ()
    else

    let _, costDict =
      let key = eArg.Caster.Address
      if castersDict.ContainsKey(key) then
        castersDict[key]
      else
        let newCostDict = new Domen.CostDict()
        let newCastHandler = Domen.CastsHandHandler.Create eArg.Caster
        castersDict.TryAdd(key, (newCastHandler, newCostDict)) |> ignore
        (newCastHandler, newCostDict)

    costDict.AddOrUpdate(eArg.Item.Address, eArg.ResultValue, fun _ _ -> eArg.ResultValue) |> ignore

    if Call.MenuManagerInstance().IsMenuOpen("MagicMenu") && eArg.Caster.IsPlayer then
      ()
    else 
      eArg.ResultValue <- 0.f

  let onSpendMagicCost (eArg: SpendMagicCostEventArgs) =

    let validAndBaseAllow = Functions.isActorMagicCasterValidAndUnderBloodMagick eArg.Spender eArg.Item

    if not validAndBaseAllow then
      ()
    else

    let spell = match eArg.Item with | :? SpellItem as s -> s | _ -> null
    let validSpell = spell <> null && spell.IsValid
    let concentrationSpell = Functions.isConcentrationSpell eArg.Item

    if validSpell && concentrationSpell then

      if eArg.Spender.Owner.GetActorValue(ActorValueIndices.Health) <= castHealthTreshold then
        if eArg.Spender.Owner.IsPlayer then
          Functions.failCast()
        eArg.Spender.InterruptCast()
      else
        eArg.Spender.Owner.SetAvRegenDelay indexAvDrainHealth 0.1f
        eArg.ActorValueIndex <- indexAvDrainHealth

    if validSpell && not concentrationSpell && castersDict.ContainsKey(eArg.Spender.Owner.Address) then

      let dualCast = eArg.Spender.Owner.IsDualCasting()

      let castHandler, costDict = castersDict[eArg.Spender.Owner.Address]

      let mult = if dualCast then 2.f else 1.f
      let cost = costDict[eArg.Item.Address] * mult

      eArg.Spender.Owner.SetAvRegenDelay indexAvDrainHealth 0.1f
      let numIter, costForIter = Functions.getNumberOfIterationsAndCostForIter spell.SpellData.ChargeTime cost

      if not castHandler.DualCast.Working 
         && Functions.canSpendOrFailIterruptIfNot eArg.Spender cost then

        castHandler.DualCast.StartWorking numIter costForIter

      elif eArg.Spender.ActorCasterType = EquippedSpellSlots.RightHand 
           && not castHandler.RightCast.Working
           && not castHandler.DualCast.Working 
           && Functions.canSpendOrFailIterruptIfNot eArg.Spender cost then

        castHandler.RightCast.StartWorking numIter costForIter

      elif eArg.Spender.ActorCasterType = EquippedSpellSlots.LeftHand 
           && not castHandler.LeftCast.Working
           && not castHandler.DualCast.Working
           && Functions.canSpendOrFailIterruptIfNot eArg.Spender cost then

        castHandler.LeftCast.StartWorking numIter costForIter

  let onMagicCasterFire (eArg: MagicCasterFireEventArgs) =
    
    match eArg.Caster with
    | :? ActorMagicCaster as actorMagicCaster 
      when actorMagicCaster <> null && actorMagicCaster.IsValid && actorMagicCaster.Owner <> null && actorMagicCaster.Owner.IsValid
        && Functions.isActorMagicCasterValidAndUnderBloodMagick actorMagicCaster eArg.Item
        && Functions.isConcentrationSpell eArg.Item |> not 
        && castersDict.ContainsKey(actorMagicCaster.Owner.Address) ->

        let castHandler, _ = castersDict[actorMagicCaster.Owner.Address]

        castHandler.DualCast.ResetToWork()

        if actorMagicCaster.ActorCasterType = EquippedSpellSlots.RightHand then
          castHandler.RightCast.ResetToWork()

        if actorMagicCaster.ActorCasterType = EquippedSpellSlots.LeftHand then
          castHandler.LeftCast.ResetToWork()

    | _ -> ()


type BloodMagickPlugin() =

  inherit Plugin()
  let mutable modInit = false
  let mutable lastTime10ms = 0L
  let timer = Tools.Timer()

  override _.Key = "bloodmagick"
  override _.Name = "Blood Magick"
  override _.Author = "newrite"
  override _.RequiredLibraryVersion = 10
  override _.Version = 1


  override self.Initialize _ =
    self.init()
    true
      
  member private _.init() =
    
    Events.OnMainMenu.Register(fun _ ->
      if not modInit then
        modInit <- BloodMagickPlugin.init()
        timer.Start()
      ) |> ignore

    Events.OnFrame.Register(fun _ ->

      if modInit then
        
        let now = timer.Time

        BloodMagickPlugin.onFrameAlways()

        if now - lastTime10ms >= 10 then
          BloodMagickPlugin.onFrame10ms()
          lastTime10ms <- now

      ) |> ignore


    Events.OnInterruptCast.Register(fun eArg ->

      if modInit then
        BloodMagickPlugin.onInterruptCast eArg

      ) |> ignore
  
    Events.OnCalculateMagicCost.Register(fun eArg ->

      if modInit then
        BloodMagickPlugin.onCalculateMagickCost eArg

      ) |> ignore
    
    Events.OnSpendMagicCost.Register(fun eArg ->

      if modInit then
        BloodMagickPlugin.onSpendMagicCost eArg

      ) |> ignore
    
    Events.OnMagicCasterFire.Register(fun eArg ->

      if modInit then
        BloodMagickPlugin.onMagicCasterFire eArg

      ) |> ignore

