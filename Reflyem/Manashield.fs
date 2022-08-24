namespace Reflyem

open Supporter
open NetScriptFramework.SkyrimSE

open ExtEvent
open Extensions

module ManashieldPlugin =

  let init() =
    if RefConfig.MagickaCostPerDamage.IsNone then
      Log "ManashieldPlugin global not found"
      false
    else
      Log "ManashieldPlugin global found"
      true

  let onWeaponHit(eArg: OnHitWeaponArgs) resultDamage =
    
    if eArg.Attacked <> null && eArg.Attacked.IsValid then

      let manashieldPercent = eArg.Attacked.GetActorValue(RefConfig.ManashieldActorValue)
      if manashieldPercent > 0.f then

        let damageMult = eArg.Attacked.GettingDamageMult

        let absorbDamage = (resultDamage * damageMult) * (manashieldPercent / 100.f)
        let magicka = eArg.Attacked.GetActorValue(ActorValueIndices.Magicka)

        let mutable canAbsorb = 0.f
        let mutable magickaDamage = 0.f
        let costPerDamage = 
          if eArg.Attacked.IsPlayer then RefConfig.MagickaCostPerDamage.Value.FloatValue * 0.5f else 0.5f
        for absorb in 0.f..0.5f..absorbDamage do
          if magickaDamage < magicka then
            magickaDamage <- magickaDamage + costPerDamage
            canAbsorb <- absorb

        eArg.Attacked.DamageActorValue(ActorValueIndices.Magicka, -magickaDamage)
        canAbsorb / damageMult

      else
        0.f
    else
      0.f