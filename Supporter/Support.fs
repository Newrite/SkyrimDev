namespace Supporter

open NetScriptFramework
open NetScriptFramework.SkyrimSE
open Wrapper
open System


#nowarn "26"
#nowarn "52"

[<AutoOpen>]
module Utils =

    let inline (^) f x = f x
    
    //Функция для логирования, пишет в лог нетскрипта и выводит такую же нотификацию
    module Log =
        let Init modName msg =

            #if DEBUG
                let l = Main.Log
        
                if l <> null then
                    l.AppendLine(modName+": "+ msg)
        
                Main.WriteDebugMessage(msg)
                Call.MessageHUD(msg, null, true)
            #else
                let l = Main.Log
        
                if l <> null then
                    l.AppendLine(modName+": "+ msg)
        
                Main.WriteDebugMessage(msg)
            #endif

    [<AutoOpen>]
    module internal InternalSupporter =
        let Settings = new Settings()
        let LogSup = Log.Init "Supporter"

    let GameSettingDamageByPlayer() =
      match Settings.GameSettingDamageByPlayer with
      | Some g -> g.FloatValue
      | None -> 1.f

    let GameSettingDamageToPlayer() =
      match Settings.GameSettingDamageToPlayer with
      | Some g -> g.FloatValue
      | None -> 1.f

    let writeNativeLog (ctx: CPURegisters) logName =
      let logNameWithTime = sprintf "Data\\NativeLogs\\%s%d.txt" logName DateTime.Now.Ticks
      Main.WriteNativeCrashLog(ctx, Int32.MinValue, logNameWithTime)
      |> sprintf "LogWriteSucces: %b" |> LogSup

    let tryReadFloat ptr (name: string) =
      try
        let pointer = Memory.ReadPointer(ptr)
        if pointer <> IntPtr.Zero then
          LogSup <| sprintf "Read float N: %s F: %f" name (Memory.ReadFloat(pointer))
      with exn ->
        LogSup <| sprintf "Error N: %s MSG: %s" name exn.Message

    let tryCast<'a when 'a :> IMemoryObject> ptr (name: string) =
      try
        let pointer = Memory.ReadPointer(ptr)
        if pointer <> IntPtr.Zero then
          let result: 'a = MemoryObject.FromAddress<'a>(pointer)
          LogSup <| sprintf "Read N: %s RES: %A TYPE: %A" name result result.TypeInfo.Info.Name
      with exn ->
        LogSup <| sprintf "Error N: %s MSG: %s" name exn.Message    


[<RequireQualifiedAccess>]
module internal Addresses =

  let [<Literal>] GetEquippedWeap = 38781uL

  let [<Literal>] IsDualCasting = 37815uL

  let [<Literal>] SetAvRegenDelay = 38526uL

  let [<Literal>] GetCurrentGameTime = 56475uL

  let [<Literal>] RealHoursPassed = 54842uL

  let [<Literal>] FlashHudMeter = 51907uL

  let [<Literal>] PlaySound = 32301uL

  let [<Literal>] OnWeaponHit = 37673uL

  let [<Literal>] OnAdjustEffect = 33763uL

  let [<Literal>] OnAttackData = 37650uL

  let [<Literal>] OnTempering = 50477uL

  let [<Literal>] OnMagicHit = 43015uL

  let [<Literal>] OnMagicProjectileConusHit = 42982uL

  let [<Literal>] OnMagicProjectileFlameHit = 42728uL

  let [<Literal>] OnRestoreActorValue = 37510uL

[<RequireQualifiedAccess>]
module internal Offsets =
  
  let [<Literal>] OnWeaponHit = 0x3C0

  let [<Literal>] OnAdjustEffect = 0x4A3

  let [<Literal>] OnAttackData = 0x16E

  let [<Literal>] OnTempering = 0x115

  let [<Literal>] OnMagicHit = 0x216

  let [<Literal>] OnMagicProjectileConusHit = 0x839

  let [<Literal>] OnMagicProjectileFlameHit = 0x44A

  let [<Literal>] OnRestoreActorValue = 0x176

module Extensions =

  type IMemoryObject with
    
    member self.IsNotNullAndValid =
      self <> null && self.IsValid

  type ActiveEffect with

    member self.IsPowerDurationScaling =
      if self <> null && self.IsValid && self.BaseEffect <> null && self.BaseEffect.IsValid then
        if self.BaseEffect.EffectFlags.HasFlag(EffectSettingFlags.PowerAffectsDuration) then
          true
        else
          false
      else
        false

    member self.IsPowerMagnitudeScaling =
      if self <> null && self.IsValid && self.BaseEffect <> null && self.BaseEffect.IsValid then
        if self.BaseEffect.EffectFlags.HasFlag(EffectSettingFlags.PowerAffectsMagnitude) then
          true
        else
          false
      else
        false

    member self.Magnitude
      with set(magnitude: float32) =
        if self <> null && self.IsValid then
          Memory.WriteFloat(self.Address + (nativeint 0x78), magnitude)

    member self.Duration
      with set(duration: float32) =
        if self <> null && self.IsValid then
          Memory.WriteFloat(self.Address + (nativeint 0x74), duration)

  [<RequireQualifiedAccess>]
  type WeaponSlot =
    | Right
    | Left

    member self.Int =
      match self with
      | Right -> 0
      | Left -> 1

  let private GetCountRestoreAV (actor: Actor) av =

    let validateActiveEffect (activeEffect: ActiveEffect) = 
      activeEffect.BaseEffect <> null 
      && activeEffect.IsValid && not activeEffect.IsInactive 
      && activeEffect.BaseEffect.EffectFlags.HasFlag(EffectSettingFlags.Recover) |> not
      && activeEffect.BaseEffect.EffectFlags.HasFlag(EffectSettingFlags.Detrimental) |> not

    let valuePeakModOrValueModIsAv (activeEffect: ActiveEffect) =
      (activeEffect.BaseEffect.Archetype = Archetypes.PeakValueMod
      || activeEffect.BaseEffect.Archetype = Archetypes.ValueMod) 
      && activeEffect.BaseEffect.PrimaryActorValue = av

    let mutable restoreValueCounter = 0.f

    

    for activeEffect in actor.ActiveEffects do

      if validateActiveEffect activeEffect then

         if valuePeakModOrValueModIsAv activeEffect then

            //LogSup <| sprintf "Step1 value: %f" activeEffect.Magnitude
            restoreValueCounter <- restoreValueCounter + activeEffect.Magnitude

         elif activeEffect.BaseEffect.Archetype = Archetypes.DualValueMod then

          if activeEffect.BaseEffect.PrimaryActorValue = av && activeEffect.BaseEffect.SecondaryActorValue <> av then
            //LogSup <| sprintf "Step2 value: %f" activeEffect.Magnitude
            restoreValueCounter <- restoreValueCounter + activeEffect.Magnitude

          elif activeEffect.BaseEffect.SecondaryActorValue = av && activeEffect.BaseEffect.SecondaryActorValue = av then
            //LogSup <| sprintf "Step3 value: %f %f" activeEffect.Magnitude (activeEffect.Magnitude * activeEffect.BaseEffect.SecondaryActorValueWeight)
            restoreValueCounter <- restoreValueCounter + (activeEffect.Magnitude * activeEffect.BaseEffect.SecondaryActorValueWeight)
            restoreValueCounter <- restoreValueCounter + activeEffect.Magnitude

          elif activeEffect.BaseEffect.SecondaryActorValue = av then
            //LogSup <| sprintf "Step4 value: %f" (activeEffect.Magnitude * activeEffect.BaseEffect.SecondaryActorValueWeight)
            restoreValueCounter <- restoreValueCounter + (activeEffect.Magnitude * activeEffect.BaseEffect.SecondaryActorValueWeight)

    restoreValueCounter

  type Actor with

    member self.GetEquippedWeapon (slot: WeaponSlot) =
      let addr_GetEquippedWeap = Main.GameInfo.GetAddressOf(Addresses.GetEquippedWeap)
      let item = MemoryObject.FromAddress<ExtraContainerChanges.ItemEntry>(Memory.InvokeCdecl(addr_GetEquippedWeap, self.Process.Cast<ActorProcess>(), slot.Int))
      if item <> null && item.IsValid then
        let weap = item.Template :?> TESObjectWEAP
        if weap <> null && weap.IsValid then Some weap else None
      else
        None

    member self.IsPlayer = self.HasKeywordText(@"PlayerKeyword")

    member self.GettingDamageMult =
      if self.IsPlayer || self.IsPlayerTeammate || self.IsHostileToActor(Call.PlayerInstance()) |> not then
        GameSettingDamageToPlayer()
      else
        GameSettingDamageByPlayer()

    member self.HasActiveEffectWithKeyword keyword =
      let mutable hasKwdEffect = false
      for activeEffect in self.ActiveEffects do
        if not hasKwdEffect && activeEffect.BaseEffect <> null && activeEffect.BaseEffect.HasKeyword(keyword) then
          hasKwdEffect <- true
      hasKwdEffect

    member self.HasAbsoluteKeyword keyword =
      self.HasKeyword keyword || self.WornHasKeyword keyword || self.HasActiveEffectWithKeyword keyword

    member self.IsDualCasting () =
      let addr_IsDualCasting = Main.GameInfo.GetAddressOf(Addresses.IsDualCasting)
      Memory.InvokeCdecl(addr_IsDualCasting, self.Cast<TESObjectREFR>()).ToBool()

    member self.SetAvRegenDelay (av: int) (amount: float32) =
      let addr_SetAVRegenDelay = Main.GameInfo.GetAddressOf(Addresses.SetAvRegenDelay)
      Memory.InvokeCdecl(addr_SetAVRegenDelay, self.Process.Cast<ActorProcess>(), av, amount) |> ignore

    member self.GetValueFlatRestore av = GetCountRestoreAV self av

    // av - Actor Value like Health \ Magic \ Stamina
    // avRate - Base rate of regeneration like 0.2 - 0.2% of max av
    // avRateMult - Multiply for avRate like 100 - 0.2 * 2. - 0.4% of max av
    member self.GetValueOfRegeneration av avRate avMultRate =

      let rate = self.GetActorValue(avRate)
      let multRate = self.GetActorValue(avMultRate)

      if rate <= 0.f || multRate <= -100.f then 
        0.f
      else

      let maxValue = self.GetActorValueMax(av)
      let percent = rate * (1.f + (multRate / 100.f))
      let valueOfRegeneration = (maxValue / 100.f) * percent

      valueOfRegeneration

    member self.GetValueRegenerationAndRestore av avRate avMultRate = 
      self.GetValueFlatRestore av + self.GetValueOfRegeneration av avRate avMultRate

  let getCurrentGameTime() =
    let addr_GetCurrentGameTime = Main.GameInfo.GetAddressOf(Addresses.GetCurrentGameTime)
    Memory.InvokeCdeclF(addr_GetCurrentGameTime)

  let realHoursPassed() =
    let addr_RealHoursPassed = Main.GameInfo.GetAddressOf(Addresses.RealHoursPassed)
    Memory.InvokeCdeclF(addr_RealHoursPassed)

  let flashHudMeter (av: int) =
    let addr_FlashHudMeter = Main.GameInfo.GetAddressOf(Addresses.FlashHudMeter)
    if addr_FlashHudMeter <> IntPtr.Zero then
      Memory.InvokeCdecl(addr_FlashHudMeter, av) |> ignore
    else
      LogSup "addr_FlashHudMeter zero ptr"

  let playSound (sound: TESForm) (actor: Actor) =
    let fnAddr_BGSSoundDescriptor_PlaySound = Main.GameInfo.GetAddressOf(Addresses.PlaySound)
    if fnAddr_BGSSoundDescriptor_PlaySound <> IntPtr.Zero
       && sound <> null && actor <> null && actor.Node <> null then
      Memory.InvokeCdecl(fnAddr_BGSSoundDescriptor_PlaySound, sound.Address, 0, actor.Position.Address, actor.Node.Address) 
      |> ignore
    else
      LogSup "fnAddr_BGSSoundDescriptor_PlaySound zero ptr or null argument"

module ExtEvent =

    [<RequireQualifiedAccess>]
    type Animations =
        | WeaponSwingRight
        | WeaponSwingLeft
        | WeaponSwingRightPower
        | WeaponSwingLeftPower
        | Jump
        | BowDraw
        | ArrowRelease
        | BowReset
        | BoltRelease
        | ReloadStart
        | ReloadStop
        | WeapEquipOut
    
    type OnAnimationArgs(animation: Animations, source: Actor) =
        inherit EventArgs()
    
        member _.Animation = animation
        member _.Source = source
    
    type AnimationDelegate = delegate of obj * OnAnimationArgs -> unit
    
    let animationRising =
        new Event<AnimationDelegate, OnAnimationArgs>()

    let OnAnimation = animationRising.Publish

    type OnHitWeaponArgs(ctx: CPURegisters, attacker: Actor, attacked: Actor, data: HitData) =
      inherit EventArgs()

      member _.Attacker = attacker
      member _.Attacked = attacked
      member _.HitData = data
      member _.ResultDamage
        with get() = data.TotalDamage
        and  set(damage: float32) = Memory.WriteFloat(ctx.DX + (nativeint 0x50), damage)
      member _.Context = ctx

    type HitWeaponDelegate = delegate of obj * OnHitWeaponArgs -> unit

    let hitWeaponRising =
      new Event<HitWeaponDelegate, OnHitWeaponArgs>()

    let OnHitWeapon = hitWeaponRising.Publish

    type OnHitMagicArgs(ctx: CPURegisters, caster: MagicCaster, target: TESObjectREFR, proj: Projectile) =
      inherit EventArgs()

      member _.Caster = caster
      member _.Target = target
      member _.Projectile = proj
      member _.Context = ctx

    type HitMagicDelegate = delegate of obj * OnHitMagicArgs -> unit

    let hitMagicRising =
      new Event<HitMagicDelegate, OnHitMagicArgs>()

    let OnHitMagic = hitMagicRising.Publish

    type OnAdjustEffectArgs(ctx: CPURegisters, activeEffect: ActiveEffect, magicTarget: MagicTarget) =
      inherit EventArgs()

      member _.ActiveEffect = activeEffect
      member _.MagicTarget = magicTarget
      member _.PowerValue
        with get() = ctx.XMM2f
        // and set(value) = ctx.XMM2f <- value
      member _.EffectMagnitude
        with get() = activeEffect.Magnitude
        and set(magnitude: float32) = Memory.WriteFloat(ctx.CX + (nativeint 0x78), magnitude)
      member _.EffectDuration
        with get() = activeEffect.Duration
        and set(magnitude: float32) = Memory.WriteFloat(ctx.CX + (nativeint 0x74), magnitude)
      member _.Context = ctx

    type AdjustEffectDelegate = delegate of obj * OnAdjustEffectArgs -> unit

    let adjustEffectRising =
      new Event<AdjustEffectDelegate, OnAdjustEffectArgs>()

    let OnAdjustEffect = adjustEffectRising.Publish

    type OnAttackDataArgs(ctx: CPURegisters, actor: Actor, attackData: BGSAttackData) =
      inherit EventArgs()

      member _.Actor = actor
      member _.AttackData = attackData
      member _.Context = ctx

    type AttackDataDelegate = delegate of obj * OnAttackDataArgs -> unit

    let attackDataRising =
      new Event<AttackDataDelegate, OnAttackDataArgs>()

    let OnAttackData = attackDataRising.Publish

[<RequireQualifiedAccess>]
module Memory =

  let tryReadPointer ptr =
    let mutable _ptr: nativeint = IntPtr.Zero
    let result = Memory.TryReadPointer(ptr, &_ptr)
    if result && _ptr <> IntPtr.Zero then
      result, _ptr
    else
      false, _ptr

module internal Hooks =
  
  let installHookOnWeaponHit() =

    LogSup "Install On Weapon Hit hook..."

    let hookParam = new HookParameters()
    hookParam.Address <- Main.GameInfo.GetAddressOf(Addresses.OnWeaponHit, Offsets.OnWeaponHit)
    hookParam.IncludeLength <- 5
    hookParam.ReplaceLength <- 5
    hookParam.Before <- fun ctx ->
      //writeNativeLog ctx "On-WeaponHit"
      let hitDataResult, _ = Memory.tryReadPointer ctx.DX
      //let hitDataResultR8, _ = Memory.tryReadPointer ctx.R8
      let attackerResult, _ = Memory.tryReadPointer ctx.BX
      let attackedResult, _ = Memory.tryReadPointer ctx.CX
      //LogSup <| sprintf "HitData: %b Attacker: %b Attacked: %b R8: %b" hitDataResult attackerResult attackedResult hitDataResultR8
      if hitDataResult && attackerResult && attackedResult then
        //LogSup "Success args for weaponhit"
        //writeNativeLog ctx "On-WeaponHit"
        let hitData = MemoryObject.FromAddress<HitData>(ctx.DX)
        let attacker = MemoryObject.FromAddress<Actor>(ctx.BX)
        let attacked = MemoryObject.FromAddress<Actor>(ctx.CX)
        ExtEvent.hitWeaponRising.Trigger(ctx, new ExtEvent.OnHitWeaponArgs(ctx, attacker, attacked, hitData))
      //else
      //  LogSup "Not success args for weapon hit"
    hookParam.After <- fun _ -> ()
    Memory.WriteHook(hookParam)

    LogSup "Success install On Weapon Hit hook!"
  
  let installHookOnAdjustEffect() =

    LogSup "Install On Adjust Effect hook..."
  
    let hookParam = new HookParameters()
    hookParam.Address <- Main.GameInfo.GetAddressOf(Addresses.OnAdjustEffect, Offsets.OnAdjustEffect)
    hookParam.IncludeLength <- 5
    hookParam.ReplaceLength <- 5
    hookParam.Before <- fun ctx ->
      //writeNativeLog ctx "On-AdjustEffect"
      let activeEffectResult, _ = Memory.tryReadPointer ctx.CX
      let magicTargetResult, _ = Memory.tryReadPointer ctx.R14
      if activeEffectResult && magicTargetResult then
        let activeEffect = MemoryObject.FromAddress<ActiveEffect>(ctx.CX)
        let magicTarget = MemoryObject.FromAddress<MagicTarget>(ctx.R14)
        let powerValue = ctx.XMM2f
        ExtEvent.adjustEffectRising.Trigger(ctx, new ExtEvent.OnAdjustEffectArgs(ctx, activeEffect, magicTarget))
    hookParam.After <- fun _ -> ()
    Memory.WriteHook(hookParam)

    LogSup "Success install On Adjust Effect hook!"

  let installHookOnAttackData() =

    LogSup "Install On Attack Data hook..."
  
    let hookParam = new HookParameters()
    hookParam.Address <- Main.GameInfo.GetAddressOf(Addresses.OnAttackData, Offsets.OnAttackData)
    hookParam.IncludeLength <- 5
    hookParam.ReplaceLength <- 5
    hookParam.Before <- fun ctx ->
      // try
        // writeNativeLog ctx "On-Attack"
        let firstResultRead, r14 = Memory.tryReadPointer ctx.R14
        let actorResultRead, _ = Memory.tryReadPointer ctx.DI
        // LogSup <| sprintf "firstResultRead: %b actorResultRead: %b" firstResultRead actorResultRead
        if firstResultRead && actorResultRead then
          let attackData = MemoryObject.FromAddress<BGSAttackData>(r14)
          let actor = MemoryObject.FromAddress<Actor>(ctx.DI)
          // LogSup "Okay read"
          ExtEvent.attackDataRising.Trigger(ctx, new ExtEvent.OnAttackDataArgs(ctx, actor, attackData))
      // with ex ->
        // LogSup <| sprintf "Exception occured in onAttackData message %s and stack \n%s" ex.Message ex.StackTrace
    hookParam.After <- fun _ -> ()
    Memory.WriteHook(hookParam)

    LogSup "Success install On Attack Data hook!"

open ExtEvent

type SupporterPlugin() =

  inherit Plugin()

  let mutable init = false
  let mutable lastTime = 0L
  let timer = Tools.Timer()

  override _.Key = "supporterplugin"
  override _.Name = "Supporter"
  override _.Author = "newrite"
  override _.RequiredLibraryVersion = 10
  override _.Version = 1

  override self.Initialize _ =

    Settings.Load()
    |> function
    | true -> LogSup <| sprintf "Settings load"
    | false -> LogSup <| sprintf "Can't load settings"
      
    if Settings.OnAdjustEffectHookEnable then Hooks.installHookOnAdjustEffect()
    if Settings.OnAttackDataHookEnable then Hooks.installHookOnAttackData()
    if Settings.OnWeaponHitHookEnable then Hooks.installHookOnWeaponHit()

    self.init()
    true

  member private self.init() =

    let inline animRegister (animation: Animations) (actor: #Actor) =
      // LogSup <| sprintf "Animation %A was register for actor with level %d" animation actor.Level
      animationRising.Trigger(self, new OnAnimationArgs(animation, actor))

    Events.OnMagicCasterFire.Register(fun eArg ->

      let someCaster =
          match eArg.Caster with
          | :? ActorMagicCaster as c -> Some c
          | _ -> None
      let someSpell =
          match eArg.Item with
          | :? SpellItem as s -> Some s
          | _ -> None

      match (someCaster, someSpell, init) with
      | (Some caster, Some spell, true) ->
        match spell with
        | _ when spell = Settings.SpellArrowRelease.Value ->
          animRegister Animations.ArrowRelease caster.Owner
        | _ when spell = Settings.SpellBoltRelease.Value ->
          animRegister Animations.BoltRelease caster.Owner
        | _ when spell = Settings.SpellBowDraw.Value ->
          animRegister Animations.BowDraw caster.Owner
        | _ when spell = Settings.SpellBowReset.Value ->
          animRegister Animations.BowReset caster.Owner
        | _ when spell = Settings.SpellJump.Value ->
          animRegister Animations.Jump caster.Owner
        | _ when spell = Settings.SpellReloadStart.Value ->
          animRegister Animations.ReloadStart caster.Owner
        | _ when spell = Settings.SpellReloadStop.Value ->
          animRegister Animations.ReloadStop caster.Owner
        | _ when spell = Settings.SpellWeapEquipOut.Value ->
          animRegister Animations.WeapEquipOut caster.Owner
        | _ when spell = Settings.SpellWeaponSwingLeft.Value ->
          animRegister Animations.WeaponSwingLeft caster.Owner
        | _ when spell = Settings.SpellWeaponSwingLeftPower.Value ->
          animRegister Animations.WeaponSwingLeftPower caster.Owner
        | _ when spell = Settings.SpellWeaponSwingRight.Value ->
          animRegister Animations.WeaponSwingRight caster.Owner
        | _ when spell = Settings.SpellWeaponSwingRightPower.Value ->
          animRegister Animations.WeaponSwingRightPower caster.Owner
        | _ -> ()
      | _ -> ()

      ()) |> ignore

    let handlerGlobal (globalVal: TESGlobal) animation actor =
      if globalVal.FloatValue > 0.0f then
        globalVal.FloatValue <- globalVal.FloatValue - 1.0f
        animRegister animation actor

    Events.OnFrame.Register(fun _ ->

      if init && timer.Time - lastTime > 100 then

        lastTime <- timer.Time

        let player = Call.PlayerInstance()
        if player <> null && player.IsValid then
          handlerGlobal Settings.GlobalArrowRelease.Value Animations.ArrowRelease player 
          handlerGlobal Settings.GlobalBoltRelease.Value Animations.BoltRelease player 
          handlerGlobal Settings.GlobalBowDraw.Value Animations.BowDraw player 
          handlerGlobal Settings.GlobalBowReset.Value Animations.BowReset player 
          handlerGlobal Settings.GlobalJump.Value Animations.Jump player 
          handlerGlobal Settings.GlobalReloadStart.Value Animations.ReloadStart player 
          handlerGlobal Settings.GlobalReloadStop.Value Animations.ReloadStop player 
          handlerGlobal Settings.GlobalWeapEquipOut.Value Animations.WeapEquipOut player 
          handlerGlobal Settings.GlobalWeaponSwingLeft.Value Animations.WeaponSwingLeft player 
          handlerGlobal Settings.GlobalWeaponSwingLeftPower.Value Animations.WeaponSwingLeftPower player 
          handlerGlobal Settings.GlobalWeaponSwingRight.Value Animations.WeaponSwingRight player 
          handlerGlobal Settings.GlobalWeaponSwingRightPower.Value Animations.WeaponSwingRightPower player 
    ) |> ignore

    Events.OnMainMenu.Register(fun _ ->
    
      if not init && Settings.OnAnimationHackHookEnable then
        LogSup <| sprintf "Init in main menu - Ok"
        LogSup "Install On Animation hack..."
        if List.contains None Settings.EventSpellList || List.contains None Settings.EventGlobalsList then
          LogSup <| sprintf "On Animation Hack: One or more global or spell event in settings is None"
        else
          LogSup "On Animation hack success init"
          timer.Start()
          init <- true
      ) |> ignore