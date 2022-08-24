namespace Reflyem

open NetScriptFramework
open NetScriptFramework.SkyrimSE
open Supporter

[<AutoOpen>]
module internal InternalReflyem =
  
  module Domain =
    
    [<RequireQualifiedAccess>]
    type ShieldConfig =
      | BloodAfterMana
      | ManaPriority
      | BloodPriority
      | BloodAndManaEquals
      | HigherAV

  let ShieldConfig() =
    match RefConfig.ShieldConfig with
    | 0 -> Domain.ShieldConfig.BloodAfterMana
    | 1 -> Domain.ShieldConfig.ManaPriority
    | 2 -> Domain.ShieldConfig.BloodPriority
    | 3 -> Domain.ShieldConfig.BloodAndManaEquals
    | 4 -> Domain.ShieldConfig.HigherAV
    | _ -> Domain.ShieldConfig.BloodAfterMana

type Reflyem() =

  inherit Plugin()

  let mutable modInit = false
  let mutable bloodshieldPluginInit = false
  let mutable bloodSpellsPluginInit = false
  let mutable manashieldPluginInit = false
  let mutable healthGatePluginInit = false
  let mutable vampirisimPluginInit = false
  let mutable fenixSpeedCastingPluginInit = false
  let mutable dualPowerPluginInit = false
  let mutable widgetPluginInit = false

  let mutable lastTime10ms = 0L
  let mutable lastTime100ms = 0L
  let mutable lastTime500ms = 0L

  let timer = Tools.Timer()

  override _.Key = "reflyem"
  override _.Name = "Reflyem"
  override _.Author = "newrite"
  override _.RequiredLibraryVersion = 10
  override _.Version = 1


  override self.Initialize _ =
      
    self.init()
    true
      
  member private _.init() =
      
    RefConfig.Load()
    |> function
    | true ->  Log <| sprintf "Settings load"
    | false -> Log <| sprintf "Can't load settings, use default"

    Events.OnMainMenu.Register(fun _ ->
      if not modInit then
        bloodSpellsPluginInit       <- BloodSpellsPlugin.init()
        widgetPluginInit            <- WidgetPlugin.init()
        healthGatePluginInit        <- HealthGatePlugin.init()
        manashieldPluginInit        <- ManashieldPlugin.init()
        fenixSpeedCastingPluginInit <- FenixSpeedCastingPlugin.init()
        bloodshieldPluginInit       <- BloodshieldPlugin.init()
        dualPowerPluginInit         <- DualPowerPlugin.init()
        vampirisimPluginInit        <- true
        timer.Start()
        modInit <- true
      ) |> ignore

    Events.OnFrame.Register(fun _ ->

      if modInit then

        let now = timer.Time

        if bloodSpellsPluginInit then
          BloodSpellsPlugin.onFrameAlways()

        if now - lastTime10ms >= 10 then

          if bloodSpellsPluginInit then
            BloodSpellsPlugin.onFrame10ms()

          lastTime10ms <- now

        if now - lastTime100ms >= 100 then
          
          //if bloodSpellsPluginInit then
          //  BloodshieldPlugin.onFrame100ms()

          lastTime100ms <- now

        if now - lastTime500ms >= 500 then
          
          if widgetPluginInit then
            WidgetPlugin.onFrame500ms()

          if fenixSpeedCastingPluginInit then
            FenixSpeedCastingPlugin.onFrame500ms()

          lastTime500ms <- now

      ) |> ignore

    Events.OnInterruptCast.Register(fun eArg ->

      if modInit then

        if bloodSpellsPluginInit then
          BloodSpellsPlugin.onInterruptCast eArg

      ) |> ignore
  
    Events.OnCalculateMagicCost.Register(fun eArg ->

      if modInit then

        if bloodSpellsPluginInit then
          BloodSpellsPlugin.onCalculateMagickCost eArg

      ) |> ignore
    
    Events.OnSpendMagicCost.Register(fun eArg ->

      if modInit then

        if bloodSpellsPluginInit then  
          BloodSpellsPlugin.onSpendMagicCost eArg

      ) |> ignore
    
    Events.OnMagicCasterFire.Register(fun eArg ->

      if modInit then

        if bloodSpellsPluginInit then
          BloodSpellsPlugin.onMagicCasterFire eArg

      ) |> ignore

    ExtEvent.OnAdjustEffect.Add(fun eArg ->
      
      if modInit then
        
        if dualPowerPluginInit then
          DualPowerPlugin.onAdjustEffect eArg

      )

    ExtEvent.OnHitWeapon.Add(fun eArg ->

      if modInit then
        
        match ShieldConfig() with
        | Domain.ShieldConfig.BloodAfterMana ->

          let manaAbsorb = 
            if manashieldPluginInit then
              ManashieldPlugin.onWeaponHit eArg eArg.ResultDamage
            else 0.f
          let bloodAbsorb =
            if bloodshieldPluginInit then
              BloodshieldPlugin.onWeaponHit eArg (eArg.ResultDamage - manaAbsorb)
            else 0.f

          eArg.ResultDamage <- eArg.ResultDamage - (manaAbsorb + bloodAbsorb)

        | Domain.ShieldConfig.ManaPriority ->

          if manashieldPluginInit && bloodshieldPluginInit then
            let manaAbsorb = ManashieldPlugin.onWeaponHit eArg eArg.ResultDamage
            if manaAbsorb > 0.f then
              eArg.ResultDamage <- eArg.ResultDamage - manaAbsorb

            else
              let bloodAbsorb = BloodshieldPlugin.onWeaponHit eArg eArg.ResultDamage
              eArg.ResultDamage <- eArg.ResultDamage - bloodAbsorb

          elif manashieldPluginInit then
            let manaAbsorb = ManashieldPlugin.onWeaponHit eArg eArg.ResultDamage
            eArg.ResultDamage <- eArg.ResultDamage - manaAbsorb

          elif bloodshieldPluginInit then
            let bloodAbsorb = BloodshieldPlugin.onWeaponHit eArg eArg.ResultDamage
            eArg.ResultDamage <- eArg.ResultDamage - bloodAbsorb

        | Domain.ShieldConfig.BloodPriority ->

          if manashieldPluginInit && bloodshieldPluginInit then
            let bloodAbsorb = BloodshieldPlugin.onWeaponHit eArg eArg.ResultDamage 
            if bloodAbsorb > 0.f then
              eArg.ResultDamage <- eArg.ResultDamage - bloodAbsorb

            else
              let manaAbsorb = ManashieldPlugin.onWeaponHit eArg eArg.ResultDamage
              eArg.ResultDamage <- eArg.ResultDamage - manaAbsorb

          elif bloodshieldPluginInit then
            let bloodAbsorb = BloodshieldPlugin.onWeaponHit eArg eArg.ResultDamage
            eArg.ResultDamage <- eArg.ResultDamage - bloodAbsorb

          elif manashieldPluginInit then
            let manaAbsorb = ManashieldPlugin.onWeaponHit eArg eArg.ResultDamage
            eArg.ResultDamage <- eArg.ResultDamage - manaAbsorb

        | Domain.ShieldConfig.BloodAndManaEquals ->

          if manashieldPluginInit && bloodshieldPluginInit && eArg.Attacked <> null && eArg.Attacked.IsValid then
            let bloodAv = eArg.Attacked.GetActorValue(RefConfig.BloodshieldActorValue)
            let manaAv = eArg.Attacked.GetActorValue(RefConfig.ManashieldActorValue)

            if bloodAv > 0.f && manaAv > 0.f then
              let manaAbsorb = ManashieldPlugin.onWeaponHit eArg (eArg.ResultDamage / 2.f)
              let bloodAbsorb = BloodshieldPlugin.onWeaponHit eArg (eArg.ResultDamage / 2.f)

              eArg.ResultDamage <- eArg.ResultDamage - (manaAbsorb + bloodAbsorb)

            elif bloodAv > 0.f then
              let bloodAbsorb = BloodshieldPlugin.onWeaponHit eArg eArg.ResultDamage

              eArg.ResultDamage <- eArg.ResultDamage -  bloodAbsorb

            elif manaAv > 0.f then
              let manaAbsorb = ManashieldPlugin.onWeaponHit eArg eArg.ResultDamage

              eArg.ResultDamage <- eArg.ResultDamage - manaAbsorb

          elif bloodshieldPluginInit then
            let bloodAbsorb = BloodshieldPlugin.onWeaponHit eArg eArg.ResultDamage
            eArg.ResultDamage <- eArg.ResultDamage - bloodAbsorb

          elif manashieldPluginInit then
            let manaAbsorb = ManashieldPlugin.onWeaponHit eArg eArg.ResultDamage
            eArg.ResultDamage <- eArg.ResultDamage - manaAbsorb

        | Domain.ShieldConfig.HigherAV ->

          if manashieldPluginInit && bloodshieldPluginInit && eArg.Attacked <> null && eArg.Attacked.IsValid then

            let bloodAv = eArg.Attacked.GetActorValue(RefConfig.BloodshieldActorValue)
            let manaAv = eArg.Attacked.GetActorValue(RefConfig.ManashieldActorValue)

            if bloodAv >= manaAv then
              let bloodAbsorb = BloodshieldPlugin.onWeaponHit eArg eArg.ResultDamage
              eArg.ResultDamage <- eArg.ResultDamage - bloodAbsorb

            elif manaAv > 0.f then
              let manaAbsorb = ManashieldPlugin.onWeaponHit eArg eArg.ResultDamage
              eArg.ResultDamage <- eArg.ResultDamage - manaAbsorb

          elif bloodshieldPluginInit then
            let bloodAbsorb = BloodshieldPlugin.onWeaponHit eArg eArg.ResultDamage
            eArg.ResultDamage <- eArg.ResultDamage - bloodAbsorb

          elif manashieldPluginInit then
            let manaAbsorb = ManashieldPlugin.onWeaponHit eArg eArg.ResultDamage
            eArg.ResultDamage <- eArg.ResultDamage - manaAbsorb


        if healthGatePluginInit then
          HealthGatePlugin.onWeaponHit eArg

        if vampirisimPluginInit then
          VampirismPlugin.onWeaponHit eArg

      )