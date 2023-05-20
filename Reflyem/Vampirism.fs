namespace Reflyem

open Supporter
open NetScriptFramework.SkyrimSE

open ExtEvent
open Extensions

module VampirismPlugin =

  let init() =
    match RefConfig.VampirismConfig with
    | 1 -> 
      RefConfig.VampirismHealthFromEffectKeyword.IsSome
    | 2 ->
      RefConfig.VampirismStaminaFromEffectKeyword.IsSome
    | 4 ->
      RefConfig.VampirismMagickaFromEffectKeyword.IsSome
    | 3 ->
      RefConfig.VampirismHealthFromEffectKeyword.IsSome
      && RefConfig.VampirismStaminaFromEffectKeyword.IsSome
    | 5 ->
      RefConfig.VampirismHealthFromEffectKeyword.IsSome
      && RefConfig.VampirismMagickaFromEffectKeyword.IsSome
    | 6 ->
      RefConfig.VampirismStaminaFromEffectKeyword.IsSome
      && RefConfig.VampirismMagickaFromEffectKeyword.IsSome
    | 7 ->
      RefConfig.VampirismHealthFromEffectKeyword.IsSome
      && RefConfig.VampirismStaminaFromEffectKeyword.IsSome
      && RefConfig.VampirismMagickaFromEffectKeyword.IsSome
    | _ ->
      Log "Vampirism off"
      false

  let private vampirismFromActorValue vampirismAv avRestore (eArg: OnHitWeaponArgs) =
    let vampirismPercent = eArg.Attacker.GetActorValue(vampirismAv)
    if vampirismPercent > 0.f then

      let damageMult = eArg.Attacked.GettingDamageMult

      let vampirism =
        let vampirismBase = (eArg.ResultDamage * damageMult) * (vampirismPercent / 100.f)
        if vampirismBase > eArg.Attacked.GetActorValue(avRestore) then
          eArg.Attacked.GetActorValue(avRestore)
        else
          vampirismBase
      
      eArg.Attacker.RestoreActorValue(avRestore, vampirism)

  let private vampirismFromEffect vampirismEffectKwd avRestore (eArg: OnHitWeaponArgs) =
    let vampirismPercent =
      let mutable percent = 0.f
      for effect in eArg.Attacked.ActiveEffects do
        if effect <> null && effect.IsValid && not effect.IsInactive && effect.BaseEffect <> null && effect.BaseEffect.IsValid then
          if effect.BaseEffect.HasKeyword(vampirismEffectKwd) && effect.Magnitude > percent then
            percent <- effect.Magnitude
      percent
    if vampirismPercent > 0.f then

      let damageMult = eArg.Attacked.GettingDamageMult

      let vampirism =
        let vampirismBase = (eArg.ResultDamage * damageMult) * (vampirismPercent / 100.f)
        if vampirismBase > eArg.Attacked.GetActorValue(avRestore) then
          eArg.Attacked.GetActorValue(avRestore)
        else
          vampirismBase
      
      eArg.Attacker.RestoreActorValue(avRestore, vampirism)

  let private vampirism vampirismAv vampirismEffectKwd avRestore eArg =
    vampirismFromActorValue vampirismAv avRestore eArg
    vampirismFromEffect vampirismEffectKwd avRestore eArg

  let onWeaponHit(eArg: OnHitWeaponArgs) =
    
    if eArg.Attacker <> null && eArg.Attacker.IsValid && eArg.Attacked <> null && eArg.Attacked.IsValid then

      match RefConfig.VampirismConfig with
      | 1 -> 
        vampirism RefConfig.VampirismHealthActorValue RefConfig.VampirismHealthFromEffectKeyword.Value ActorValueIndices.Health eArg
      | 2 ->
        vampirism RefConfig.VampirismStaminaActorValue RefConfig.VampirismStaminaFromEffectKeyword.Value ActorValueIndices.Stamina eArg
      | 4 ->
        vampirism RefConfig.VampirismMagickaActorValue RefConfig.VampirismMagickaFromEffectKeyword.Value ActorValueIndices.Magicka eArg
      | 3 ->
        vampirism RefConfig.VampirismHealthActorValue RefConfig.VampirismHealthFromEffectKeyword.Value ActorValueIndices.Health eArg
        vampirism RefConfig.VampirismStaminaActorValue RefConfig.VampirismStaminaFromEffectKeyword.Value ActorValueIndices.Stamina eArg
      | 5 ->
        vampirism RefConfig.VampirismHealthActorValue RefConfig.VampirismHealthFromEffectKeyword.Value ActorValueIndices.Health eArg
        vampirism RefConfig.VampirismMagickaActorValue RefConfig.VampirismMagickaFromEffectKeyword.Value ActorValueIndices.Magicka eArg
      | 6 ->
        vampirism RefConfig.VampirismStaminaActorValue RefConfig.VampirismStaminaFromEffectKeyword.Value ActorValueIndices.Stamina eArg
        vampirism RefConfig.VampirismMagickaActorValue RefConfig.VampirismMagickaFromEffectKeyword.Value ActorValueIndices.Magicka eArg
      | 7 ->
        vampirism RefConfig.VampirismHealthActorValue RefConfig.VampirismHealthFromEffectKeyword.Value ActorValueIndices.Health eArg
        vampirism RefConfig.VampirismStaminaActorValue RefConfig.VampirismStaminaFromEffectKeyword.Value ActorValueIndices.Stamina eArg
        vampirism RefConfig.VampirismMagickaActorValue RefConfig.VampirismMagickaFromEffectKeyword.Value ActorValueIndices.Magicka eArg
      | _ ->
        Log "Vampirism off but proc"