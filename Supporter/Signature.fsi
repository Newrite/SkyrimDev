



namespace Supporter
    
    [<Sealed>]
    type Settings =
        
        new: unit -> Settings
        
        member internal Load: unit -> bool
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventArrowRelease", "Spell for ArrowRelease form id",
           "Spell cast when ArrowRelease with bow equip anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventArrowRelease: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventArrowReleaseGlobal", "Global for ArrowRelease form id",
           "Global modify when ArrowRelease with bow equip anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventArrowReleaseGlobal: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventBoltRelease", "Spell for BoltRelease form id",
           "Spell cast when ArrowRelease with crossbow equip anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventBoltRelease: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventBoltReleaseGlobal", "Global for BoltRelease form id",
           "Global modify when ArrowRelease with crossbow equip anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventBoltReleaseGlobal: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventBowDraw", "Spell for BowDraw form id",
           "Spell cast when BowDraw anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventBowDraw: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventBowDrawGlobal", "Global for BowDraw form id",
           "Global modify when BowDraw anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventBowDrawGlobal: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventBowReset", "Spell for BowReset form id",
           "Spell cast when BowReset anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventBowReset: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventBowResetGlobal", "Global for BowReset form id",
           "Global modify when BowReset anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventBowResetGlobal: uint32
        
        member
          EventGlobalsList: NetScriptFramework.SkyrimSE.TESGlobal option list
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventJump", "Spell for JumpUP form id",
           "Spell cast when JumpUp anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventJump: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventJumpGlobal", "Global for JumpUP form id",
           "Global modify when JumpUp anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventJumpGlobal: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventReloadStart", "Spell for ReloadStart form id",
           "Spell cast when ReloadStart anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventReloadStart: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventReloadStartGlobal", "Global for ReloadStart form id",
           "Global modify when ReloadStart anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventReloadStartGlobal: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventReloadStop", "Spell for ReloadStop form id",
           "Spell cast when ReloadStip anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventReloadStop: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventReloadStopGlobal", "Global for ReloadStop form id",
           "Global modify when ReloadStip anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventReloadStopGlobal: uint32
        
        member EventSpellList: NetScriptFramework.SkyrimSE.SpellItem option list
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventWeapEquipOut", "Spell for WeapEquipOut form id",
           "Spell cast when WeapEquipOut anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventWeapEquipOut: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventWeapEquipOutGlobal", "Global for WeapEquipOut form id",
           "Global modify when WeapEquipOut anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventWeapEquipOutGlobal: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventWeaponSwingLeft", "Spell for WeaponSwingLeft form id",
           "Spell cast when WeaponSwingLeft anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventWeaponSwingLeft: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventWeaponSwingLeftGlobal", "Global for WeaponSwingLeft form id",
           "Global modify when WeaponSwingLeft anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventWeaponSwingLeftGlobal: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventWeaponSwingLeftPower", "Spell for WeaponSwingLeftPower form id",
           "Spell cast when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventWeaponSwingLeftPower: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventWeaponSwingLeftPowerGlobal",
           "Global for WeaponSwingLeftPower form id",
           "Global modify when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventWeaponSwingLeftPowerGlobal: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventWeaponSwingRight", "Spell for WeaponSwingRight form id",
           "Spell cast when WeaponSwing anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventWeaponSwingRight: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventWeaponSwingRightGlobal", "Global for WeaponSwingRight form id",
           "Global modify when WeaponSwing anim is rising, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventWeaponSwingRightGlobal: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventWeaponSwingRightPower",
           "Spell for WeaponSwingRightPower form id",
           "Spell cast when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventWeaponSwingRightPower: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("EventWeaponSwingRightPowerGlobal",
           "Spell for WeaponSwingRightPower form id",
           "Global modify when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private EventWeaponSwingRightPowerGlobal: uint32
        
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
          ("ModName", "Mod name where FormIDs", "Form id gets from this file",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member private ModName: string
        
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

namespace Supporter
    
    module Utils =
        
        [<RequireQualifiedAccess>]
        type OnAnimation =
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
        
        val inline (^) : f: ('a -> 'b) -> x: 'a -> 'b
        
        module Log =
            
            val Init: modName: string -> msg: string -> unit
        
        [<RequireQualifiedAccess>]
        type WeaponSlot =
            | Right
            | Left
            
            member Int: int
        type Actor with
            
            member
              GetEquippedWeapon: slot: WeaponSlot
                                   -> NetScriptFramework.SkyrimSE.TESObjectWEAP option
        type Actor with
            
            member IsDualCasting: unit -> bool
        type Actor with
            
            member SetAvRegenDelay: av: int -> amount: float32 -> unit
        
        val getCurrentGameTime: unit -> float32
        
        val realHoursPassed: unit -> float32
        
        val flashHudMeter: av: int -> unit
        
        val playSound: unit -> int
    
    module private InternalVariable =
        
        val Settings: Settings
        
        val Log: (string -> unit)
    
    module ExtEvent =
        
        type AnimArgs =
            inherit System.EventArgs
            
            new: animation: Utils.OnAnimation *
                 source: NetScriptFramework.SkyrimSE.Actor -> AnimArgs
            
            member Anim: Utils.OnAnimation
            
            member Source: NetScriptFramework.SkyrimSE.Actor
        
        type AnimDelegate =
            delegate of obj * AnimArgs -> unit
        
        val animationRising: Event<AnimDelegate,AnimArgs>
        
        val OnAnimationEvent: IEvent<AnimDelegate,AnimArgs>
    
    type SupporterPlugin =
        inherit NetScriptFramework.Plugin
        
        new: unit -> SupporterPlugin
        
        override Initialize: loadedAny: bool -> bool
        
        member private init: unit -> unit
        
        override Author: string
        
        override Key: string
        
        override Name: string
        
        override RequiredLibraryVersion: int
        
        override Version: int

