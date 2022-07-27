namespace Supporter

open NetScriptFramework.Tools
open NetScriptFramework.SkyrimSE
open Wrapper

[<Sealed>]
type public Settings() =

    // for PLAYER
    let mutable eventGlobalWeaponSwingLeft = 0x80Eu
    let mutable eventGlobalWeaponSwingLeftPower = 0x80Fu
    let mutable eventGlobalWeaponSwingRight = 0x810u
    let mutable eventGlobalWeaponSwingRightPower = 0x811u
    let mutable eventGlobalJump = 0x812u
    let mutable eventGlobalBowDraw = 0x813u
    let mutable eventGlobalArrowRelease = 0x814u
    let mutable eventGlobalBowReset = 0x815u
    let mutable eventGlobalBoltRelease = 0x816u
    let mutable eventGlobalReloadStart = 0x817u
    let mutable eventGlobalReloadStop = 0x818u
    let mutable eventGlobalWeapEquipOut = 0x819u


    // for NPC
    let mutable eventWeaponSwingLeft = 0x800u
    let mutable eventWeaponSwingLeftPower = 0x802u
    let mutable eventWeaponSwingRight = 0x803u
    let mutable eventWeaponSwingRightPower = 0x804u
    let mutable eventJump = 0x805u
    let mutable eventBowDraw = 0x806u
    let mutable eventArrowRelease = 0x807u
    let mutable eventBowReset = 0x808u
    let mutable eventBoltRelease = 0x809u
    let mutable eventReloadStart = 0x80Au
    let mutable eventReloadStop = 0x80Bu
    let mutable eventWeapEquipOut = 0x80Cu

    let mutable modName = "Newrite_NETScriptLibs.esp"

    [<ConfigValue("ModName", "Mod name where FormIDs", "Form id gets from this file")>]
    member private self.ModName
        with get() = modName
        and set name = modName <- name
    
    // for PLAYER

    [<ConfigValue("EventWeaponSwingLeftGlobal", "Global for WeaponSwingLeft form id", "Global modify when WeaponSwingLeft anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventWeaponSwingLeftGlobal
        with get() = eventGlobalWeaponSwingLeft
        and set id = eventGlobalWeaponSwingLeft <- id
    
    [<ConfigValue("EventWeaponSwingLeftPowerGlobal", "Global for WeaponSwingLeftPower form id", "Global modify when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventWeaponSwingLeftPowerGlobal
        with get() = eventGlobalWeaponSwingLeftPower
        and set id = eventGlobalWeaponSwingLeftPower <- id
    
    [<ConfigValue("EventWeaponSwingRightGlobal", "Global for WeaponSwingRight form id", "Global modify when WeaponSwing anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventWeaponSwingRightGlobal
        with get() = eventGlobalWeaponSwingRight
        and set id = eventGlobalWeaponSwingRight <- id
    
    [<ConfigValue("EventWeaponSwingRightPowerGlobal", "Spell for WeaponSwingRightPower form id", "Global modify when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventWeaponSwingRightPowerGlobal
        with get() = eventGlobalWeaponSwingRightPower
        and set id = eventGlobalWeaponSwingRightPower <- id
    
    [<ConfigValue("EventJumpGlobal", "Global for JumpUP form id", "Global modify when JumpUp anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventJumpGlobal
        with get() = eventGlobalJump
        and set id = eventGlobalJump <- id
    
    [<ConfigValue("EventBowDrawGlobal", "Global for BowDraw form id", "Global modify when BowDraw anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventBowDrawGlobal
        with get() = eventGlobalBowDraw
        and set id = eventGlobalBowDraw <- id
    
    [<ConfigValue("EventArrowReleaseGlobal", "Global for ArrowRelease form id", "Global modify when ArrowRelease with bow equip anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventArrowReleaseGlobal
        with get() = eventGlobalArrowRelease
        and set id = eventGlobalArrowRelease <- id
    
    [<ConfigValue("EventBowResetGlobal", "Global for BowReset form id", "Global modify when BowReset anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventBowResetGlobal
        with get() = eventGlobalBowReset
        and set id = eventGlobalBowReset <- id
    
    [<ConfigValue("EventBoltReleaseGlobal", "Global for BoltRelease form id", "Global modify when ArrowRelease with crossbow equip anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventBoltReleaseGlobal
        with get() = eventGlobalBoltRelease
        and set id = eventGlobalBoltRelease <- id
    
    [<ConfigValue("EventReloadStartGlobal", "Global for ReloadStart form id", "Global modify when ReloadStart anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventReloadStartGlobal
        with get() = eventGlobalReloadStart
        and set id = eventGlobalReloadStart <- id
    
    [<ConfigValue("EventReloadStopGlobal", "Global for ReloadStop form id", "Global modify when ReloadStip anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventReloadStopGlobal
        with get() = eventGlobalReloadStop
        and set id = eventGlobalReloadStop <- id
    
    [<ConfigValue("EventWeapEquipOutGlobal", "Global for WeapEquipOut form id", "Global modify when WeapEquipOut anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventWeapEquipOutGlobal
        with get() = eventGlobalWeapEquipOut
        and set id = eventGlobalWeapEquipOut <- id

    // for NPC
    
    [<ConfigValue("EventWeaponSwingLeft", "Spell for WeaponSwingLeft form id", "Spell cast when WeaponSwingLeft anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventWeaponSwingLeft
        with get() = eventWeaponSwingLeft
        and set id = eventWeaponSwingLeft <- id
    
    [<ConfigValue("EventWeaponSwingLeftPower", "Spell for WeaponSwingLeftPower form id", "Spell cast when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventWeaponSwingLeftPower
        with get() = eventWeaponSwingLeftPower
        and set id = eventWeaponSwingLeftPower <- id
    
    [<ConfigValue("EventWeaponSwingRight", "Spell for WeaponSwingRight form id", "Spell cast when WeaponSwing anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventWeaponSwingRight
        with get() = eventWeaponSwingRight
        and set id = eventWeaponSwingRight <- id
    
    [<ConfigValue("EventWeaponSwingRightPower", "Spell for WeaponSwingRightPower form id", "Spell cast when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventWeaponSwingRightPower
        with get() = eventWeaponSwingRightPower
        and set id = eventWeaponSwingRightPower <- id
    
    [<ConfigValue("EventJump", "Spell for JumpUP form id", "Spell cast when JumpUp anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventJump
        with get() = eventJump
        and set id = eventJump <- id
    
    [<ConfigValue("EventBowDraw", "Spell for BowDraw form id", "Spell cast when BowDraw anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventBowDraw
        with get() = eventBowDraw
        and set id = eventBowDraw <- id
    
    [<ConfigValue("EventArrowRelease", "Spell for ArrowRelease form id", "Spell cast when ArrowRelease with bow equip anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventArrowRelease
        with get() = eventArrowRelease
        and set id = eventArrowRelease <- id
    
    [<ConfigValue("EventBowReset", "Spell for BowReset form id", "Spell cast when BowReset anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventBowReset
        with get() = eventBowReset
        and set id = eventBowReset <- id
    
    [<ConfigValue("EventBoltRelease", "Spell for BoltRelease form id", "Spell cast when ArrowRelease with crossbow equip anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventBoltRelease
        with get() = eventBoltRelease
        and set id = eventBoltRelease <- id
    
    [<ConfigValue("EventReloadStart", "Spell for ReloadStart form id", "Spell cast when ReloadStart anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventReloadStart
        with get() = eventReloadStart
        and set id = eventReloadStart <- id
    
    [<ConfigValue("EventReloadStop", "Spell for ReloadStop form id", "Spell cast when ReloadStip anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventReloadStop
        with get() = eventReloadStop
        and set id = eventReloadStop <- id
    
    [<ConfigValue("EventWeapEquipOut", "Spell for WeapEquipOut form id", "Spell cast when WeapEquipOut anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member private self.EventWeapEquipOut
        with get() = eventWeapEquipOut
        and set id = eventWeapEquipOut <- id
    
    // for PLAYER
    member self.GlobalWeaponSwingLeft = 
        match Call.TESFormLookupFormFromFile(self.EventWeaponSwingLeftGlobal, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalWeaponSwingLeftPower =
        match Call.TESFormLookupFormFromFile(self.EventWeaponSwingLeftPowerGlobal, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalWeaponSwingRight =
        match Call.TESFormLookupFormFromFile(self.EventWeaponSwingRightGlobal, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalWeaponSwingRightPower =
        match Call.TESFormLookupFormFromFile(self.EventWeaponSwingRightPowerGlobal, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalJump =
        match Call.TESFormLookupFormFromFile(self.EventJumpGlobal, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalBowDraw =
        match Call.TESFormLookupFormFromFile(self.EventBowDrawGlobal, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalArrowRelease =
        match Call.TESFormLookupFormFromFile(self.EventArrowReleaseGlobal, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalBowReset =
        match Call.TESFormLookupFormFromFile(self.EventBowResetGlobal, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalBoltRelease =
        match Call.TESFormLookupFormFromFile(self.EventBoltReleaseGlobal, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalReloadStart =
        match Call.TESFormLookupFormFromFile(self.EventReloadStartGlobal, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalReloadStop =
        match Call.TESFormLookupFormFromFile(self.EventReloadStopGlobal, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalWeapEquipOut =
        match Call.TESFormLookupFormFromFile(self.EventWeapEquipOutGlobal, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None


    // for NPC
    member self.SpellWeaponSwingLeft = 
        match Call.TESFormLookupFormFromFile(self.EventWeaponSwingLeft, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellWeaponSwingLeftPower =
        match Call.TESFormLookupFormFromFile(self.EventWeaponSwingLeftPower, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellWeaponSwingRight =
        match Call.TESFormLookupFormFromFile(self.EventWeaponSwingRight, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellWeaponSwingRightPower =
        match Call.TESFormLookupFormFromFile(self.EventWeaponSwingRightPower, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellJump =
        match Call.TESFormLookupFormFromFile(self.EventJump, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellBowDraw =
        match Call.TESFormLookupFormFromFile(self.EventBowDraw, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellArrowRelease =
        match Call.TESFormLookupFormFromFile(self.EventArrowRelease, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellBowReset =
        match Call.TESFormLookupFormFromFile(self.EventBowReset, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellBoltRelease =
        match Call.TESFormLookupFormFromFile(self.EventBoltRelease, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellReloadStart =
        match Call.TESFormLookupFormFromFile(self.EventReloadStart, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellReloadStop =
        match Call.TESFormLookupFormFromFile(self.EventReloadStop, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellWeapEquipOut =
        match Call.TESFormLookupFormFromFile(self.EventWeapEquipOut, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None


    // for PLAYER
    member self.EventGlobalsList =
        [ self.GlobalArrowRelease
          self.GlobalBoltRelease
          self.GlobalBowDraw
          self.GlobalBowReset
          self.GlobalJump
          self.GlobalReloadStart
          self.GlobalReloadStop
          self.GlobalWeapEquipOut
          self.GlobalWeaponSwingLeft
          self.GlobalWeaponSwingLeftPower
          self.GlobalWeaponSwingRight
          self.GlobalWeaponSwingRightPower ]


    // for NPC
    member self.EventSpellList =
        [ self.SpellArrowRelease
          self.SpellBoltRelease
          self.SpellBowDraw
          self.SpellBowReset
          self.SpellJump
          self.SpellReloadStart
          self.SpellReloadStop
          self.SpellWeapEquipOut
          self.SpellWeaponSwingLeft
          self.SpellWeaponSwingLeftPower
          self.SpellWeaponSwingRight
          self.SpellWeaponSwingRightPower ]

    member internal self.Load() = ConfigFile.LoadFrom<Settings>(self, "Supporter", true);