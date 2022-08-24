namespace Reflyem

open Supporter
open NetScriptFramework.SkyrimSE

open ExtEvent
open Extensions

module BloodshieldPlugin =

  // module Domain =
  // 
  //   type BloodshieldEffect =
  //     { mutable Damage: float32
  //       mutable DamageTick: float32 }
  // 
  //     with
  //     static member Create damage damageTick =
  //       Log <| sprintf "Create bloodshield effect with damage %f and damageTick %f" damage damageTick
  //       { Damage = damage
  //         DamageTick = damageTick }
  //   
  //   type Bloodshield =
  //     { Actor: Actor
  //       Effects: System.Collections.Generic.List<BloodshieldEffect> }
  // 
  //     with
  // 
  //     member self.AddEffect effect =
  //       Log <| sprintf "Added new bloodshield effect with damage %f and damageTick %f" effect.Damage effect.DamageTick
  //       self.Effects.Add(effect)
  // 
  //     member self.CleanEffects() =
  //       
  //       self.Effects.RemoveAll(fun effect -> effect.Damage <= 0.f) |> ignore
  // 
  //     member self.Iterate() =
  //       for effect in self.Effects do
  //         Log <| sprintf "Iterate effect with damage %f and damageTick %f" effect.Damage effect.DamageTick
  //         if effect.Damage - effect.DamageTick <= 0.f then
  //           self.Actor.DamageActorValue(ActorValueIndices.Health, -effect.Damage)
  //           effect.Damage <- 0.f
  //         else
  //           self.Actor.DamageActorValue(ActorValueIndices.Health, -effect.DamageTick)
  //           effect.Damage <- effect.Damage - effect.DamageTick
  // 
  //     static member Create actor effect =
  //       let list = new System.Collections.Generic.List<BloodshieldEffect>()
  //       list.Add(effect)
  //       { Actor = actor
  //         Effects = list }
  // 
  // let data = System.Collections.Concurrent.ConcurrentDictionary<nativeint, Domain.Bloodshield>()

  let init() =
    if RefConfig.BloodshieldSpellToCast.IsNone then
      Log "BloodshieldPlugin spell not found"
      false
    else
      let spell = RefConfig.BloodshieldSpellToCast.Value
      if spell.Effects <> null && spell.Effects.IsValid && spell.Effects.Count > 0 then
        Log "BloodshieldPlugin spell found"
        true
      else
        Log "BloodshieldPlugin spell is bad"
        false

  let onWeaponHit(eArg: OnHitWeaponArgs) resultDamage =
    
    if eArg.Attacked <> null && eArg.Attacked.IsValid then

      let bloodshieldPercent = eArg.Attacked.GetActorValue(RefConfig.BloodshieldActorValue)
      if bloodshieldPercent > 0.f then

        let spell = RefConfig.BloodshieldSpellToCast.Value

        let bloodDuration =
            let duration = spell.Effects[0].Duration
            if duration > 0 then duration else 5
            |> float32

        let damageMult = eArg.Attacked.GettingDamageMult

        let bloodDamage = (resultDamage * damageMult) * (bloodshieldPercent / 100.f)
        let bloodDamageTick = (bloodDamage / bloodDuration) / damageMult

        spell.Effects[0].Magnitude <- bloodDamageTick

        eArg.Attacked.CastSpell(spell, eArg.Attacked, eArg.Attacked)
        |> fun b -> if not b then Log "Failed cast bloodshield spell"


        // if data.ContainsKey(eArg.Attacked.Address) then
        //   let effect = Domain.BloodshieldEffect.Create bloodDamage bloodDamageTick
        //   data[eArg.Attacked.Address].AddEffect effect
        // else
        //   let effect = Domain.BloodshieldEffect.Create bloodDamage bloodDamageTick
        //   data.TryAdd(eArg.Attacked.Address, Domain.Bloodshield.Create eArg.Attacked effect) |> ignore

        bloodDamage / damageMult

      else

        0.f

    else

      0.f


  // let onFrame100ms() =
  // 
  //   for key in data.Keys do
  //     let actor = data[key].Actor
  //     if actor <> null && actor.IsValid && not actor.IsDead then
  //       data[key].Iterate()
  //       data[key].CleanEffects()
  //     else
  //       data[key].CleanEffects()