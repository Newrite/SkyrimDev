namespace Reflyem

open Wrapper

module FenixSpeedCastingPlugin =

  let init() =
    if RefConfig.SpeedCasting.IsNone then
      Log "SpeedCasting global not found"
      false
    else
      Log "SpeedCasting global found"
      true
  
  let onFrame500ms() =
    
    let player = Call.PlayerInstance()

    if player <> null then
      let speedCastingAV =
        let av = player.GetActorValue(RefConfig.SpeedCastingActorValue)
        if av >= 75.f then 75.f else av

      RefConfig.SpeedCasting.Value.FloatValue <- (1.f - (speedCastingAV / 100.f))