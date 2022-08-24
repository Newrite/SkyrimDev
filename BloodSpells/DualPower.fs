namespace Reflyem

open Supporter

open Extensions
open ExtEvent

module DualPowerPlugin =

  let init() =
    // if RefConfig.DualCastPower.IsNone || RefConfig.DualCastPowerKeyword.IsNone then
    //   Log "DualCastPower kwd or global not found"
    //   false
    // else
    //   Log "DualCastPower kwd and global found"
    //   true
    false

  let onAdjustEffect(eArg: OnAdjustEffectArgs) =

    if eArg.ActiveEffect <> null && eArg.ActiveEffect.IsValid 
       && eArg.ActiveEffect.Caster <> null && eArg.ActiveEffect.Caster.IsValid then

      let caster = eArg.ActiveEffect.Caster
      let keyword = RefConfig.DualCastPowerKeyword.Value

      if caster.HasAbsoluteKeyword keyword && eArg.PowerValue >= 2.25f then

        let powerAdd =
          if RefConfig.DualCastPower.Value.FloatValue > 0.f then RefConfig.DualCastPower.Value.FloatValue else 0.f

        eArg.EffectMagnitude <- eArg.PowerValue + powerAdd
  