namespace Reflyem

open Supporter
open NetScriptFramework.SkyrimSE

open ExtEvent
open Extensions

module VampirismPlugin =

  let onWeaponHit(eArg: OnHitWeaponArgs) =
    
    if eArg.Attacker <> null && eArg.Attacker.IsValid && eArg.Attacked <> null && eArg.Attacked.IsValid then

      let vampirismPercent = eArg.Attacker.GetActorValue(RefConfig.VampirismActorValue)
      if vampirismPercent > 0.f then

        let damageMult = eArg.Attacked.GettingDamageMult

        let vampirism =
          let vampirismBase = (eArg.ResultDamage * damageMult) * (vampirismPercent / 100.f)
          if vampirismBase > eArg.Attacked.GetActorValue(ActorValueIndices.Health) then
            eArg.Attacked.GetActorValue(ActorValueIndices.Health)
          else
            vampirismBase
        
        eArg.Attacker.RestoreActorValue(ActorValueIndices.Health, vampirism)