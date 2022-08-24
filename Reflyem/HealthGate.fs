namespace Reflyem

open Supporter
open NetScriptFramework.SkyrimSE

open Extensions
open ExtEvent

module HealthGatePlugin =

  let init() =
    if RefConfig.HealthGateKwd.IsNone || RefConfig.HealthGatePercent.IsNone then
      Log "HealthGate kwd or global not found"
      false
    else
      Log "HealthGate kwd and global found"
      true
  
  let onWeaponHit(eArg: OnHitWeaponArgs) =
    
    if eArg.Attacked <> null 
       && eArg.Attacked.IsValid 
       && eArg.Attacked.HasAbsoluteKeyword RefConfig.HealthGateKwd.Value then

      let healthGatePercent =
        if not eArg.Attacked.IsPlayer then
          80.f
        else
          let percentFromGlobal = RefConfig.HealthGatePercent.Value.FloatValue
          if percentFromGlobal > 0.f then percentFromGlobal else 80.f

      let damageMult = eArg.Attacked.GettingDamageMult

      let healthTreshold = eArg.Attacked.GetActorValueMax(ActorValueIndices.Health) * (healthGatePercent / 100.f)
      if (eArg.ResultDamage * damageMult) >= healthTreshold then
        eArg.ResultDamage <- (healthTreshold / damageMult)