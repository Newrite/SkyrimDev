namespace Reflyem

open Supporter
open Wrapper
open NetScriptFramework.SkyrimSE

open Extensions

module WidgetPlugin =

  let init() =
    match (RefConfig.WidgetGlobalHealth, RefConfig.WidgetGlobalMagicka, RefConfig.WidgetGlobalStamina) with
    | Some _, Some _, Some _ ->
      Log "Globals for widget is ok"
      true
    | _ ->
      Log "Globals for widget is not ok"
      false
  
  let onFrame500ms() =
    
    let player = Call.PlayerInstance()

    if player <> null then

      let healthGlobal = RefConfig.WidgetGlobalHealth.Value
      let magickaGlobal = RefConfig.WidgetGlobalMagicka.Value
      let staminaGlobal = RefConfig.WidgetGlobalStamina.Value

      healthGlobal.FloatValue <- player.GetValueRegenerationAndRestore ActorValueIndices.Health ActorValueIndices.HealRate ActorValueIndices.HealRateMult
      magickaGlobal.FloatValue <- player.GetValueRegenerationAndRestore ActorValueIndices.Magicka ActorValueIndices.MagickaRate ActorValueIndices.MagickaRateMult
      staminaGlobal.FloatValue <- player.GetValueRegenerationAndRestore ActorValueIndices.Stamina ActorValueIndices.StaminaRate ActorValueIndices.StaminaRateMult