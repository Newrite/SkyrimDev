namespace SpellFury

open NetScriptFramework
open NetScriptFramework.SkyrimSE
open NetScriptFramework.Tools
open Wrapper
open Supporter
open Extensions

[<AutoOpen>]
module private InternalVariables =
    let Log = Log.Init "Spell Fury"

[<RequireQualifiedAccess>]
module SpellFury =
  
  let getHighestSchool (actor: Actor) =

    let mutable av = ActorValueIndices.Destruction

    if actor.GetActorValue(ActorValueIndices.Conjuration) > actor.GetActorValue(av) then
      av <- ActorValueIndices.Conjuration

    if actor.GetActorValue(ActorValueIndices.Illusion) > actor.GetActorValue(av) then
      av <- ActorValueIndices.Illusion

    if actor.GetActorValue(ActorValueIndices.Alteration) > actor.GetActorValue(av) then
      av <- ActorValueIndices.Alteration

    if actor.GetActorValue(ActorValueIndices.Restoration) > actor.GetActorValue(av) then
      av <- ActorValueIndices.Restoration

    if actor.GetActorValue(ActorValueIndices.Destruction) > actor.GetActorValue(av) then
      av <- ActorValueIndices.Destruction

    av

  let getWeaponDamage (actor: Actor) =
    match actor.GetEquippedWeapon(WeaponSlot.Right) with
    | Some weap -> weap.AttackDamage
    | None ->
      match actor.GetEquippedWeapon(WeaponSlot.Left) with
      | Some weap -> weap.AttackDamage
      | None -> actor.GetActorValue(ActorValueIndices.UnarmedDamage) |> uint16

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


type SpellFuryPlugin() =

    inherit Plugin()

    let perkId01 = 0x592Du
    let perkId02 = 0x80Au
    let perkId03 = 0x5935u

    let globalCastSpeedId = 0x592Eu
    let globalMagickaRestorePerHitId = 0x5920u
    let globalSpellCostReductionId = 0x592Fu
    let globalTresholdDrainMagickaId = 0x592Au

    let spellArmorBuffId = 0x5936u
    let abilityMagickaDrainId = 0x5933u
    let abilityCostReductionId = 0x807u
    let abilitySpellFury = 0x809u
    let abilityManaShield = 0x5946u

    let furyPlugin = "Requiem for a Dream - Spellfury.esp"

    let timer = new Timer()

    let mutable modInit = false
    let mutable lastTimer = 0L
    let mutable lastMaxMagicka = 0.f
    let mutable lastSpellCostReduction = 0.f

    override self.Key = "spellfury"
    override self.Name = "Spell Fury"
    override self.Author = "newrite"
    override self.RequiredLibraryVersion = 10
    override self.Version = 1


    override self.Initialize _ =
      self.init()
      true

    member private self.Perk01() =
      Call.TESFormLookupFormFromFile(perkId01, furyPlugin) :?> BGSPerk

    member private self.Perk02() =
      Call.TESFormLookupFormFromFile(perkId02, furyPlugin) :?> BGSPerk

    member private self.Perk03() =
      Call.TESFormLookupFormFromFile(perkId03, furyPlugin) :?> BGSPerk

    member private self.CastSpeed() =
      Call.TESFormLookupFormFromFile(globalCastSpeedId, furyPlugin) :?> TESGlobal

    member private self.MagickaRestorePerHit() =
      Call.TESFormLookupFormFromFile(globalMagickaRestorePerHitId, furyPlugin) :?> TESGlobal

    member private self.SpellCostReduction() =
      Call.TESFormLookupFormFromFile(globalSpellCostReductionId, furyPlugin) :?> TESGlobal

    member private self.TresholdDrainMagicka() =
      Call.TESFormLookupFormFromFile(globalTresholdDrainMagickaId, furyPlugin) :?> TESGlobal

    member private self.ArmorBuff() =
      Call.TESFormLookupFormFromFile(spellArmorBuffId, furyPlugin) :?> SpellItem

    member private self.MagickaDrain() =
      Call.TESFormLookupFormFromFile(abilityMagickaDrainId, furyPlugin) :?> SpellItem

    member private self.CostReduction() =
      Call.TESFormLookupFormFromFile(abilityCostReductionId, furyPlugin) :?> SpellItem

    member private self.SpellFury() =
      Call.TESFormLookupFormFromFile(abilitySpellFury, furyPlugin) :?> SpellItem

    member private self.ManaShield() =
      Call.TESFormLookupFormFromFile(abilityManaShield, furyPlugin) :?> SpellItem

    member private self.UpdateAbilitys(player: Actor) =
      
      let spellCostReduction = self.SpellCostReduction().FloatValue
      let maxMagicka = player.GetActorValueMax(ActorValueIndices.Magicka)

      if System.Math.Abs(spellCostReduction - lastSpellCostReduction) > 0.01f then
        let costReduction = self.CostReduction()
        if player.HasSpell(costReduction) then
          player.RemoveSpell(costReduction) |> ignore
        for effect in costReduction.Effects do
          effect.Magnitude <- spellCostReduction
        player.AddSpell(costReduction, false) |> ignore
        lastSpellCostReduction <- spellCostReduction

      if System.Math.Abs(maxMagicka - lastMaxMagicka) > 0.01f then
        let magickaDrain = self.MagickaDrain()
        if player.HasSpell(magickaDrain) then
          player.RemoveSpell(magickaDrain) |> ignore
        for effect in magickaDrain.Effects do
          effect.Magnitude <- (maxMagicka / 100.f) * 3.f
        player.AddSpell(magickaDrain, false) |> ignore
        lastMaxMagicka <- maxMagicka

    member private self.CleanAbilitys(player: Actor) =

      let costReduction = self.CostReduction()
      let magickaDrain = self.MagickaDrain()

      if player.HasSpell(costReduction) then
        player.RemoveSpell(costReduction) |> ignore

      if player.HasSpell(magickaDrain) then
        player.RemoveSpell(magickaDrain) |> ignore

    member private self.CheckAbility(player: Actor) =
      let costReduction = self.CostReduction()
      let magickaDrain = self.MagickaDrain()

      if player.HasSpell(costReduction) |> not then
        player.AddSpell(costReduction, false) |> ignore

      if player.HasSpell(magickaDrain) |> not then
        player.AddSpell(magickaDrain, false) |> ignore

    member private self.init() =
      
      Events.OnMainMenu.Register(fun _ ->

        if self.Perk01() <> null
           && self.Perk02() <> null
           && self.Perk03() <> null
           && self.ArmorBuff() <> null
           && self.CastSpeed() <> null
           && self.CostReduction() <> null
           && self.MagickaDrain() <> null
           && self.MagickaRestorePerHit() <> null
           && self.SpellCostReduction() <> null
           && self.TresholdDrainMagicka() <> null
           && self.SpellFury() <> null
           && self.ManaShield() <> null then
          if not modInit then
            timer.Start()
            Log "Timer start, all TESForms is ok, mod init"
            modInit <- true
        else
          Log "Fail load one or more of TESForm, mod not init"
      
        ) |> ignore

      ExtEvent.OnHitWeapon.Add(fun eArg ->
        if modInit then
          //Log "Weapon hit"
          //if eArg.Attacked <> null && eArg.Attacked.IsValid then
          //  Log <| sprintf "HasManashield: %b" (eArg.Attacked.HasSpell(self.ManaShield()))
          //  Log <| sprintf "HasSpellfury: %b" (eArg.Attacked.HasSpell(self.SpellFury()))
          //else
          //  Log "Not valid actor"
          if eArg.Attacked <> null && eArg.Attacked.IsValid && eArg.Attacked.HasSpell(self.ManaShield()) && eArg.Attacked.HasSpell(self.SpellFury()) then
            //Log "Conditions success"
            let mult =
              if eArg.Attacked.HasPerk(self.Perk03()) then 0.55f
              elif eArg.Attacked.HasPerk(self.Perk02()) then 0.7f
              elif eArg.Attacked.HasPerk(self.Perk01()) then 0.85f
              else 1.0f

            let magicka = eArg.Attacked.GetActorValue(ActorValueIndices.Magicka)
            let damageTotalNew = eArg.HitData.TotalDamage * mult
            let damageToMagicka = eArg.HitData.TotalDamage - damageTotalNew

            //Log <| sprintf "Magicka: %f DamageTotalNew: %f DamageToMagicka: %f" magicka damageTotalNew damageToMagicka
            if magicka >= damageToMagicka then

              //Log <| sprintf "Magicka above"
              eArg.Attacked.DamageActorValue(ActorValueIndices.Magicka, -damageToMagicka)
              eArg.ResultDamage <- damageTotalNew

            else

              //Log <| sprintf "Magicka low: %f" (damageToMagicka - magicka)
              eArg.Attacked.DamageActorValue(ActorValueIndices.Magicka, -magicka)
              eArg.ResultDamage <- damageTotalNew + (damageToMagicka - magicka)
      )

      Events.OnFrame.Register(fun _ ->

        if modInit && timer.Time - lastTimer >= 500 then

          let player = Call.PlayerInstance()

          if player.HasSpell(self.SpellFury()) then

            self.TresholdDrainMagicka().FloatValue <- (player.GetActorValue(SpellFury.getHighestSchool player) * 0.5f) / 100.f
            self.MagickaRestorePerHit().FloatValue <- (float32 (SpellFury.getWeaponDamage player) * 0.5f) + (float32 (player.Level) * 0.5f)
            self.SpellCostReduction().FloatValue <- self.MagickaRestorePerHit().FloatValue * 0.5f

            if player.HasPerk(self.Perk03()) then
              self.CastSpeed().FloatValue <- 0.55f
            elif player.HasPerk(self.Perk02()) then
              self.CastSpeed().FloatValue <- 0.7f
            elif player.HasPerk(self.Perk01()) then
              self.CastSpeed().FloatValue <- 0.85f
            else
              self.CastSpeed().FloatValue <- 1.f

            self.UpdateAbilitys player
            self.CheckAbility player

          else

            self.TresholdDrainMagicka().FloatValue <- 0.f
            self.MagickaRestorePerHit().FloatValue <- 0.f
            self.SpellCostReduction().FloatValue <- 0.f
            self.CastSpeed().FloatValue <- 1.f

            lastMaxMagicka <- 0.f
            lastSpellCostReduction <- 0.f

            self.CleanAbilitys player

          lastTimer <- timer.Time

        ) |> ignore

      Events.OnMagicCasterFire.Register(fun eArg ->
        if modInit && eArg.Caster <> null && eArg.Caster.IsValid 
           && SpellFury.magicItemNotNullAndIsSpell eArg.Item then
          match eArg.Caster with
          | :? ActorMagicCaster as actor ->
            if actor <> null && actor.IsValid && actor.Owner <> null && actor.Owner.IsValid 
               && actor.Owner.HasSpell(self.SpellFury()) && actor.Owner.HasPerk(self.Perk02()) then
              actor.Owner.CastSpell(self.ArmorBuff(), actor.Owner, actor.Owner) |> ignore
          | _ -> ()
        ) |> ignore

      
