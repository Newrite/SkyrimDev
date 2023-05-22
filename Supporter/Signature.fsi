

namespace FSharp

namespace Supporter
    
    [<Sealed>]
    type Settings =
        
        new: unit -> Settings
        
        member internal Load: unit -> bool
        
        [<NetScriptFramework.Tools.ConfigValue
          ("ArrowReleaseGlobalId", "Global for ArrowRelease form id",
           "Global modify when ArrowRelease with bow equip anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member ArrowReleaseGlobalId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("ArrowReleaseSpellId", "Spell for ArrowRelease form id",
           "Spell cast when ArrowRelease with bow equip anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member ArrowReleaseSpellId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("BoltReleaseGlobalId", "Global for BoltRelease form id",
           "Global modify when ArrowRelease with crossbow equip anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member BoltReleaseGlobalId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("BoltReleaseSpellId", "Spell for BoltRelease form id",
           "Spell cast when ArrowRelease with crossbow equip anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member BoltReleaseSpellId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("BowDrawGlobalId", "Global for BowDraw form id",
           "Global modify when BowDraw anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member BowDrawGlobalId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("BowDrawSpellid", "Spell for BowDraw form id",
           "Spell cast when BowDraw anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member BowDrawSpellid: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("BowResetGlobalId", "Global for BowReset form id",
           "Global modify when BowReset anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member BowResetGlobalId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("BowResetSpellId", "Spell for BowReset form id",
           "Spell cast when BowReset anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member BowResetSpellId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("DiffMultHPByPCLGlobalId",
           "Global with gamesettings mult damage by player",
           "Global has value of GameSetting - fDiffMultHPByPCL",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member DiffMultHPByPCLGlobalId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("DiffMultHPToPCLGlobalId",
           "Global with gamesettings mult damage to player",
           "Global has value of GameSetting - fDiffMultHPToPCL",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member DiffMultHPToPCLGlobalId: uint32
        
        member
          EventGlobalsList: NetScriptFramework.SkyrimSE.TESGlobal option list
        
        member EventSpellList: NetScriptFramework.SkyrimSE.SpellItem option list
        
        member
          GameSettingDamageByPlayer: NetScriptFramework.SkyrimSE.TESGlobal option
        
        member
          GameSettingDamageToPlayer: NetScriptFramework.SkyrimSE.TESGlobal option
        
        member GlobalArrowRelease: NetScriptFramework.SkyrimSE.TESGlobal option
        
        member GlobalBoltRelease: NetScriptFramework.SkyrimSE.TESGlobal option
        
        member GlobalBowDraw: NetScriptFramework.SkyrimSE.TESGlobal option
        
        member GlobalBowReset: NetScriptFramework.SkyrimSE.TESGlobal option
        
        member GlobalJump: NetScriptFramework.SkyrimSE.TESGlobal option
        
        member GlobalReloadStart: NetScriptFramework.SkyrimSE.TESGlobal option
        
        member GlobalReloadStop: NetScriptFramework.SkyrimSE.TESGlobal option
        
        member GlobalWeapEquipOut: NetScriptFramework.SkyrimSE.TESGlobal option
        
        member
          GlobalWeaponSwingLeft: NetScriptFramework.SkyrimSE.TESGlobal option
        
        member
          GlobalWeaponSwingLeftPower: NetScriptFramework.SkyrimSE.TESGlobal option
        
        member
          GlobalWeaponSwingRight: NetScriptFramework.SkyrimSE.TESGlobal option
        
        member
          GlobalWeaponSwingRightPower: NetScriptFramework.SkyrimSE.TESGlobal option
        
        [<NetScriptFramework.Tools.ConfigValue
          ("JumpGlobalId", "Global for JumpUP form id",
           "Global modify when JumpUp anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member JumpGlobalId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("JumpSpellId", "Spell for JumpUP form id",
           "Spell cast when JumpUp anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member JumpSpellId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("ModName", "Mod name where FormIDs", "Form id gets from this file",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member ModName: string
        
        [<NetScriptFramework.Tools.ConfigValue
          ("OnAdjustEffectHookEnable", "DEPRECATED On Adjust Effect Hook",
           "Disable or enable hook",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member OnAdjustEffectHookEnable: bool
        
        [<NetScriptFramework.Tools.ConfigValue
          ("OnAnimationHackHookEnable", "DEPRECATED On Animation Hack Hook",
           "Disable or enable hook",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member OnAnimationHackHookEnable: bool
        
        [<NetScriptFramework.Tools.ConfigValue
          ("OnAttackDataHookEnable", "DEPRECATED On Attack Data Hook",
           "Disable or enable hook",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member OnAttackDataHookEnable: bool
        
        [<NetScriptFramework.Tools.ConfigValue
          ("OnWeaponHitHookEnable", "DEPRECATED On Weapon Hit Hook",
           "Disable or enable hook",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member OnWeaponHitHookEnable: bool
        
        [<NetScriptFramework.Tools.ConfigValue
          ("ReloadStartGlobalId", "Global for ReloadStart form id",
           "Global modify when ReloadStart anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member ReloadStartGlobalId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("ReloadStartSpellId", "Spell for ReloadStart form id",
           "Spell cast when ReloadStart anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member ReloadStartSpellId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("ReloadStopGlobalId", "Global for ReloadStop form id",
           "Global modify when ReloadStip anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member ReloadStopGlobalId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("ReloadStopSpellId", "Spell for ReloadStop form id",
           "Spell cast when ReloadStip anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member ReloadStopSpellId: uint32
        
        member SpellArrowRelease: NetScriptFramework.SkyrimSE.SpellItem option
        
        member SpellBoltRelease: NetScriptFramework.SkyrimSE.SpellItem option
        
        member SpellBowDraw: NetScriptFramework.SkyrimSE.SpellItem option
        
        member SpellBowReset: NetScriptFramework.SkyrimSE.SpellItem option
        
        member SpellJump: NetScriptFramework.SkyrimSE.SpellItem option
        
        member SpellReloadStart: NetScriptFramework.SkyrimSE.SpellItem option
        
        member SpellReloadStop: NetScriptFramework.SkyrimSE.SpellItem option
        
        member SpellWeapEquipOut: NetScriptFramework.SkyrimSE.SpellItem option
        
        member
          SpellWeaponSwingLeft: NetScriptFramework.SkyrimSE.SpellItem option
        
        member
          SpellWeaponSwingLeftPower: NetScriptFramework.SkyrimSE.SpellItem option
        
        member
          SpellWeaponSwingRight: NetScriptFramework.SkyrimSE.SpellItem option
        
        member
          SpellWeaponSwingRightPower: NetScriptFramework.SkyrimSE.SpellItem option
        
        [<NetScriptFramework.Tools.ConfigValue
          ("WeapEquipOutGlobalId", "Global for WeapEquipOut form id",
           "Global modify when WeapEquipOut anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member WeapEquipOutGlobalId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("WeapEquipOutSpellId", "Spell for WeapEquipOut form id",
           "Spell cast when WeapEquipOut anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member WeapEquipOutSpellId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("WeaponSwingLeftGlobalId", "Global for WeaponSwingLeft form id",
           "Global modify when WeaponSwingLeft anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member WeaponSwingLeftGlobalId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("WeaponSwingLeftPowerGlobalId",
           "Global for WeaponSwingLeftPower form id",
           "Global modify when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member WeaponSwingLeftPowerGlobalId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("WeaponSwingLeftPowerSpellId",
           "Spell for WeaponSwingLeftPower form id",
           "Spell cast when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member WeaponSwingLeftPowerSpellId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("WeaponSwingLeftSpellId", "Spell for WeaponSwingLeft form id",
           "Spell cast when WeaponSwingLeft anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member WeaponSwingLeftSpellId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("WeaponSwingRightGlobalId", "Global for WeaponSwingRight form id",
           "Global modify when WeaponSwing anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member WeaponSwingRightGlobalId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("WeaponSwingRightPowerGlobalId",
           "Spell for WeaponSwingRightPower form id",
           "Global modify when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member WeaponSwingRightPowerGlobalId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("WeaponSwingRightPowerSpellId",
           "Spell for WeaponSwingRightPower form id",
           "Spell cast when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member WeaponSwingRightPowerSpellId: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("WeaponSwingRightSpellId", "Spell for WeaponSwingRight form id",
           "Spell cast when WeaponSwing anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member WeaponSwingRightSpellId: uint32

namespace Supporter
    
    [<AutoOpen>]
    module Utils =
        
        val inline (^) : f: ('a -> 'b) -> x: 'a -> 'b
        
        module Log =
            
            val Init: modName: string -> msg: string -> unit
        
        [<AutoOpen>]
        module internal InternalSupporter =
            
            val Settings: Settings
            
            val LogSup: (string -> unit)
        
        val GameSettingDamageByPlayer: unit -> float32
        
        val GameSettingDamageToPlayer: unit -> float32
        
        val writeNativeLog:
          ctx: NetScriptFramework.CPURegisters -> logName: string -> unit
        
        val tryReadFloat: ptr: nativeint -> name: string -> unit
        
        val tryCast<'a when 'a :> NetScriptFramework.IMemoryObject> :
          ptr: nativeint -> name: string -> unit
            when 'a :> NetScriptFramework.IMemoryObject
    
    [<RequireQualifiedAccess>]
    module internal Addresses =
        
        [<Literal>]
        val GetEquippedWeap: uint64 = 38781UL
        
        [<Literal>]
        val IsDualCasting: uint64 = 37815UL
        
        [<Literal>]
        val SetAvRegenDelay: uint64 = 38526UL
        
        [<Literal>]
        val GetCurrentGameTime: uint64 = 56475UL
        
        [<Literal>]
        val RealHoursPassed: uint64 = 54842UL
        
        [<Literal>]
        val FlashHudMeter: uint64 = 51907UL
        
        [<Literal>]
        val PlaySound: uint64 = 32301UL
        
        [<Literal>]
        val OnWeaponHit: uint64 = 37673UL
        
        [<Literal>]
        val OnAdjustEffect: uint64 = 33763UL
        
        [<Literal>]
        val OnAttackData: uint64 = 37650UL
        
        [<Literal>]
        val OnTempering: uint64 = 50477UL
        
        [<Literal>]
        val OnMagicHit: uint64 = 43015UL
        
        [<Literal>]
        val OnMagicProjectileConusHit: uint64 = 42982UL
        
        [<Literal>]
        val OnMagicProjectileFlameHit: uint64 = 42728UL
        
        [<Literal>]
        val OnRestoreActorValue: uint64 = 37510UL
    
    [<RequireQualifiedAccess>]
    module internal Offsets =
        
        [<Literal>]
        val OnWeaponHit: int = 960
        
        [<Literal>]
        val OnAdjustEffect: int = 1187
        
        [<Literal>]
        val OnAttackData: int = 366
        
        [<Literal>]
        val OnTempering: int = 277
        
        [<Literal>]
        val OnMagicHit: int = 534
        
        [<Literal>]
        val OnMagicProjectileConusHit: int = 2105
        
        [<Literal>]
        val OnMagicProjectileFlameHit: int = 1098
        
        [<Literal>]
        val OnRestoreActorValue: int = 374
    
    module Extensions =
        type NetScriptFramework.IMemoryObject with
            
            member IsNotNullAndValid: bool
        type NetScriptFramework.SkyrimSE.ActiveEffect with
            
            member IsPowerDurationScaling: bool
        type NetScriptFramework.SkyrimSE.ActiveEffect with
            
            member IsPowerMagnitudeScaling: bool
        type NetScriptFramework.SkyrimSE.ActiveEffect with
            
            member Magnitude: float32 with set
        type NetScriptFramework.SkyrimSE.ActiveEffect with
            
            member Duration: float32 with set
        
        [<RequireQualifiedAccess>]
        type WeaponSlot =
            | Right
            | Left
            
            member Int: int
        
        val private GetCountRestoreAV:
          actor: NetScriptFramework.SkyrimSE.Actor ->
            av: NetScriptFramework.SkyrimSE.ActorValueIndices -> float32
        type NetScriptFramework.SkyrimSE.Actor with
            
            member
              GetEquippedWeapon: slot: WeaponSlot ->
                                   NetScriptFramework.SkyrimSE.TESObjectWEAP option
        type NetScriptFramework.SkyrimSE.Actor with
            
            member IsPlayer: bool
        type NetScriptFramework.SkyrimSE.Actor with
            
            member GettingDamageMult: float32
        type NetScriptFramework.SkyrimSE.Actor with
            
            member
              HasActiveEffectWithKeyword: keyword: NetScriptFramework.SkyrimSE.BGSKeyword ->
                                            bool
        type NetScriptFramework.SkyrimSE.Actor with
            
            member
              HasAbsoluteKeyword: keyword: NetScriptFramework.SkyrimSE.BGSKeyword ->
                                    bool
        type NetScriptFramework.SkyrimSE.Actor with
            
            member IsDualCasting: unit -> bool
        type NetScriptFramework.SkyrimSE.Actor with
            
            member SetAvRegenDelay: av: int -> amount: float32 -> unit
        type NetScriptFramework.SkyrimSE.Actor with
            
            member
              GetValueFlatRestore: av: NetScriptFramework.SkyrimSE.ActorValueIndices ->
                                     float32
        type NetScriptFramework.SkyrimSE.Actor with
            
            member
              GetValueOfRegeneration: av: NetScriptFramework.SkyrimSE.ActorValueIndices ->
                                        avRate: NetScriptFramework.SkyrimSE.ActorValueIndices ->
                                        avMultRate: NetScriptFramework.SkyrimSE.ActorValueIndices ->
                                        float32
        type NetScriptFramework.SkyrimSE.Actor with
            
            member
              GetValueRegenerationAndRestore: av: NetScriptFramework.SkyrimSE.ActorValueIndices ->
                                                avRate: NetScriptFramework.SkyrimSE.ActorValueIndices ->
                                                avMultRate: NetScriptFramework.SkyrimSE.ActorValueIndices ->
                                                float32
        
        val getCurrentGameTime: unit -> float32
        
        val realHoursPassed: unit -> float32
        
        val flashHudMeter: av: int -> unit
        
        val playSound:
          sound: NetScriptFramework.SkyrimSE.TESForm ->
            actor: NetScriptFramework.SkyrimSE.Actor -> unit
    
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
        
        type OnAnimationArgs =
            inherit System.EventArgs
            
            new: animation: Animations *
                 source: NetScriptFramework.SkyrimSE.Actor -> OnAnimationArgs
            
            member Animation: Animations
            
            member Source: NetScriptFramework.SkyrimSE.Actor
        
        type AnimationDelegate =
            delegate of obj * OnAnimationArgs -> unit
        
        val animationRising: Event<AnimationDelegate,OnAnimationArgs>
        
        val OnAnimation: IEvent<AnimationDelegate,OnAnimationArgs>
        
        type OnHitWeaponArgs =
            inherit System.EventArgs
            
            new: ctx: NetScriptFramework.CPURegisters *
                 attacker: NetScriptFramework.SkyrimSE.Actor *
                 attacked: NetScriptFramework.SkyrimSE.Actor *
                 data: NetScriptFramework.SkyrimSE.HitData -> OnHitWeaponArgs
            
            member Attacked: NetScriptFramework.SkyrimSE.Actor
            
            member Attacker: NetScriptFramework.SkyrimSE.Actor
            
            member Context: NetScriptFramework.CPURegisters
            
            member HitData: NetScriptFramework.SkyrimSE.HitData
            
            member ResultDamage: float32
        
        type HitWeaponDelegate =
            delegate of obj * OnHitWeaponArgs -> unit
        
        val hitWeaponRising: Event<HitWeaponDelegate,OnHitWeaponArgs>
        
        val OnHitWeapon: IEvent<HitWeaponDelegate,OnHitWeaponArgs>
        
        type OnHitMagicArgs =
            inherit System.EventArgs
            
            new: ctx: NetScriptFramework.CPURegisters *
                 caster: NetScriptFramework.SkyrimSE.MagicCaster *
                 target: NetScriptFramework.SkyrimSE.TESObjectREFR *
                 proj: NetScriptFramework.SkyrimSE.Projectile -> OnHitMagicArgs
            
            member Caster: NetScriptFramework.SkyrimSE.MagicCaster
            
            member Context: NetScriptFramework.CPURegisters
            
            member Projectile: NetScriptFramework.SkyrimSE.Projectile
            
            member Target: NetScriptFramework.SkyrimSE.TESObjectREFR
        
        type HitMagicDelegate =
            delegate of obj * OnHitMagicArgs -> unit
        
        val hitMagicRising: Event<HitMagicDelegate,OnHitMagicArgs>
        
        val OnHitMagic: IEvent<HitMagicDelegate,OnHitMagicArgs>
        
        type OnAdjustEffectArgs =
            inherit System.EventArgs
            
            new: ctx: NetScriptFramework.CPURegisters *
                 activeEffect: NetScriptFramework.SkyrimSE.ActiveEffect *
                 magicTarget: NetScriptFramework.SkyrimSE.MagicTarget ->
                   OnAdjustEffectArgs
            
            member ActiveEffect: NetScriptFramework.SkyrimSE.ActiveEffect
            
            member Context: NetScriptFramework.CPURegisters
            
            member EffectDuration: float32
            
            member EffectMagnitude: float32
            
            member MagicTarget: NetScriptFramework.SkyrimSE.MagicTarget
            
            member PowerValue: float32
        
        type AdjustEffectDelegate =
            delegate of obj * OnAdjustEffectArgs -> unit
        
        val adjustEffectRising: Event<AdjustEffectDelegate,OnAdjustEffectArgs>
        
        val OnAdjustEffect: IEvent<AdjustEffectDelegate,OnAdjustEffectArgs>
        
        type OnAttackDataArgs =
            inherit System.EventArgs
            
            new: ctx: NetScriptFramework.CPURegisters *
                 actor: NetScriptFramework.SkyrimSE.Actor *
                 attackData: NetScriptFramework.SkyrimSE.BGSAttackData ->
                   OnAttackDataArgs
            
            member Actor: NetScriptFramework.SkyrimSE.Actor
            
            member AttackData: NetScriptFramework.SkyrimSE.BGSAttackData
            
            member Context: NetScriptFramework.CPURegisters
        
        type AttackDataDelegate =
            delegate of obj * OnAttackDataArgs -> unit
        
        val attackDataRising: Event<AttackDataDelegate,OnAttackDataArgs>
        
        val OnAttackData: IEvent<AttackDataDelegate,OnAttackDataArgs>
    
    [<RequireQualifiedAccess>]
    module Memory =
        
        val tryReadPointer: ptr: nativeint -> bool * nativeint
    
    module internal Hooks =
        
        val installHookOnWeaponHit: unit -> unit
        
        val installHookOnAdjustEffect: unit -> unit
        
        val installHookOnAttackData: unit -> unit
    
    type SupporterPlugin =
        inherit NetScriptFramework.Plugin
        
        new: unit -> SupporterPlugin
        
        override Initialize: bool -> bool
        
        member private init: unit -> unit
        
        override Author: string
        
        override Key: string
        
        override Name: string
        
        override RequiredLibraryVersion: int
        
        override Version: int

