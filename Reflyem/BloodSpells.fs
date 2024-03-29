﻿namespace Reflyem

open NetScriptFramework.SkyrimSE
open Wrapper
open Supporter
open System

open Extensions

[<AutoOpen>]
module private InternalBloodSpells =

  let indexAvDrainHealth = 24

  let magFailSoundId = uint32 0x3D0D3

  let skyrim = @"Skyrim.esm"

  let castHealthTreshold = 15.f

[<RequireQualifiedAccess>]
module BloodSpellsPlugin =

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

      if sound.IsNotNullAndValid || player.IsNotNullAndValid then
        try
          flashHudMeter indexAvDrainHealth
          Call.MessageHUD("Недостаточно здоровья для произношения заклинания.", null, true)
          playSound sound player
        with ex ->
          Log <| sprintf "Exception occured when invoke failCast(), message %s" ex.Message

      else

        Log "Error in failcast sound or player null or invalid"

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

    let isActorMagicCasterValidAndIsSpellBloodSpell (caster: ActorMagicCaster) (magicItem: MagicItem) =
      caster <> null && caster.IsValid && caster.Owner <> null && caster.Owner.IsValid 
      && magicItemNotNullAndIsSpell magicItem && magicItem.HasKeyword(RefConfig.BloodSpellsKwd.Value)

    let isMagicCasterValidAndIsSpellBloodSpell (magicCaster: MagicCaster) (magicItem: MagicItem) =
      magicCaster <> null && magicCaster.IsValid && magicItemNotNullAndIsSpell magicItem 
      && magicItem.HasKeyword(RefConfig.BloodSpellsKwd.Value)

    let isActorValidAndIsSpellBloodSpell (actor: Actor) (magicItem: MagicItem) =
      actor <> null && actor.IsValid && magicItemNotNullAndIsSpell magicItem 
      && magicItem.HasKeyword(RefConfig.BloodSpellsKwd.Value)

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
    if RefConfig.BloodSpellsKwd.IsNone then
      Log <| sprintf "Keyword for blood spells is None"
      false
    else
      Log <| sprintf "Bloodspells Ok"
      true

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
        && castHandler.Caster.GetActorValue(ActorValueIndices.Health) <= castHealthTreshold && castHandler.Caster.MagicCasters <> null then
        for caster in castHandler.Caster.MagicCasters do
          let validCaster = caster <> null && caster.IsValid && caster.CastItem <> null && caster.CastItem.IsValid
          if validCaster && caster.State = MagicCastingStates.Concentrating && caster.CastItem.HasKeyword(RefConfig.BloodSpellsKwd.Value) then
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

    let allow = 
      let check =
        Functions.isActorValidAndIsSpellBloodSpell eArg.Caster eArg.Item 
        && Functions.isConcentrationSpell eArg.Item |> not

      if check then
        let playerOnly = if RefConfig.BloodSpellsOnlyPlayer then eArg.Caster.IsPlayer else true
        check && playerOnly
      else
        check

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

    let validAndBaseAllow = Functions.isActorMagicCasterValidAndIsSpellBloodSpell eArg.Spender eArg.Item

    if not validAndBaseAllow then
      ()
    else

    if RefConfig.BloodSpellsOnlyPlayer && not eArg.Spender.Owner.IsPlayer then
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
    if Functions.isMagicCasterValidAndIsSpellBloodSpell eArg.Caster eArg.Item 
       && Functions.isConcentrationSpell eArg.Item |> not then

      match eArg.Caster with
      | :? ActorMagicCaster as c when c <> null 
          && c.IsValid && c.Owner <> null && c.Owner.IsValid 
          && castersDict.ContainsKey(c.Owner.Address) ->

        if RefConfig.BloodSpellsOnlyPlayer && c.Owner.IsPlayer |> not then
          ()
        else

        let castHandler, _ = castersDict[c.Owner.Address]

        castHandler.DualCast.ResetToWork()

        if c.ActorCasterType = EquippedSpellSlots.RightHand then
          castHandler.RightCast.ResetToWork()

        if c.ActorCasterType = EquippedSpellSlots.LeftHand then
          castHandler.LeftCast.ResetToWork()

      | _ -> ()