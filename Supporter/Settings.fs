namespace Supporter

open NetScriptFramework.Tools
open NetScriptFramework.SkyrimSE
open Wrapper

[<Sealed>]
type Settings() =

    [<ConfigValue("ModName", "Mod name where FormIDs", "Form id gets from this file")>]
    member val ModName = "Newrite_NETScriptLibs.esp" with get, set
    
    // for PLAYER

    [<ConfigValue("WeaponSwingLeftGlobalId",
    "Global for WeaponSwingLeft form id",
    "Global modify when WeaponSwingLeft anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val WeaponSwingLeftGlobalId = 0x80Eu with get, set
    
    [<ConfigValue("WeaponSwingLeftPowerGlobalId",
    "Global for WeaponSwingLeftPower form id",
    "Global modify when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val WeaponSwingLeftPowerGlobalId = 0x80Fu with get, set
    
    [<ConfigValue("WeaponSwingRightGlobalId",
    "Global for WeaponSwingRight form id",
    "Global modify when WeaponSwing anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val WeaponSwingRightGlobalId = 0x810u with get, set
    
    [<ConfigValue("WeaponSwingRightPowerGlobalId",
    "Spell for WeaponSwingRightPower form id",
    "Global modify when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val WeaponSwingRightPowerGlobalId = 0x811u with get, set
    
    [<ConfigValue("JumpGlobalId",
    "Global for JumpUP form id",
    "Global modify when JumpUp anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val JumpGlobalId = 0x812u with get, set
    
    [<ConfigValue("BowDrawGlobalId",
    "Global for BowDraw form id",
    "Global modify when BowDraw anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val BowDrawGlobalId = 0x813u with get, set
    
    [<ConfigValue("ArrowReleaseGlobalId",
    "Global for ArrowRelease form id",
    "Global modify when ArrowRelease with bow equip anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val ArrowReleaseGlobalId = 0x814u with get, set
    
    [<ConfigValue("BowResetGlobalId",
    "Global for BowReset form id",
    "Global modify when BowReset anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val BowResetGlobalId = 0x815u with get, set
    
    [<ConfigValue("BoltReleaseGlobalId",
    "Global for BoltRelease form id",
    "Global modify when ArrowRelease with crossbow equip anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val BoltReleaseGlobalId = 0x816u with get, set
    
    [<ConfigValue("ReloadStartGlobalId",
    "Global for ReloadStart form id",
    "Global modify when ReloadStart anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val ReloadStartGlobalId = 0x817u with get, set
    
    [<ConfigValue("ReloadStopGlobalId",
    "Global for ReloadStop form id",
    "Global modify when ReloadStip anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val ReloadStopGlobalId = 0x818u with get, set
    
    [<ConfigValue("WeapEquipOutGlobalId",
    "Global for WeapEquipOut form id",
    "Global modify when WeapEquipOut anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val WeapEquipOutGlobalId = 0x819u with get, set

    // for NPC
    
    [<ConfigValue("WeaponSwingLeftSpellId",
    "Spell for WeaponSwingLeft form id",
    "Spell cast when WeaponSwingLeft anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val WeaponSwingLeftSpellId = 0x800u with get, set
    
    [<ConfigValue("WeaponSwingLeftPowerSpellId",
    "Spell for WeaponSwingLeftPower form id",
    "Spell cast when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val WeaponSwingLeftPowerSpellId = 0x802u with get, set
    
    [<ConfigValue("WeaponSwingRightSpellId",
    "Spell for WeaponSwingRight form id",
    "Spell cast when WeaponSwing anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val WeaponSwingRightSpellId = 0x803u with get, set
    
    [<ConfigValue("WeaponSwingRightPowerSpellId",
    "Spell for WeaponSwingRightPower form id",
    "Spell cast when WeaponSwingLeft anim is rising and it is power attack, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val WeaponSwingRightPowerSpellId = 0x804u with get, set
    
    [<ConfigValue("JumpSpellId",
    "Spell for JumpUP form id",
    "Spell cast when JumpUp anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val JumpSpellId = 0x805u with get, set
    
    [<ConfigValue("BowDrawSpellid",
    "Spell for BowDraw form id",
    "Spell cast when BowDraw anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val BowDrawSpellid = 0x806u with get, set
    
    [<ConfigValue("ArrowReleaseSpellId",
    "Spell for ArrowRelease form id",
    "Spell cast when ArrowRelease with bow equip anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val ArrowReleaseSpellId = 0x807u with get, set
    
    [<ConfigValue("BowResetSpellId",
    "Spell for BowReset form id",
    "Spell cast when BowReset anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val BowResetSpellId = 0x808u with get, set
    
    [<ConfigValue("BoltReleaseSpellId", 
    "Spell for BoltRelease form id",
    "Spell cast when ArrowRelease with crossbow equip anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val BoltReleaseSpellId = 0x809u with get, set
    
    [<ConfigValue("ReloadStartSpellId",
    "Spell for ReloadStart form id",
    "Spell cast when ReloadStart anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val ReloadStartSpellId = 0x80Au with get, set
    
    [<ConfigValue("ReloadStopSpellId",
    "Spell for ReloadStop form id",
    "Spell cast when ReloadStip anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val ReloadStopSpellId = 0x80Bu with get, set
    
    [<ConfigValue("WeapEquipOutSpellId",
    "Spell for WeapEquipOut form id",
    "Spell cast when WeapEquipOut anim is rising, need for hook this event in plugin", ConfigEntryFlags.PreferHex)>]
    member val WeapEquipOutSpellId = 0x80Cu with get, set

    [<ConfigValue("DiffMultHPByPCLGlobalId",
    "Global with gamesettings mult damage by player",
    "Global has value of GameSetting - fDiffMultHPByPCL", ConfigEntryFlags.PreferHex)>]
    member val DiffMultHPByPCLGlobalId = 0x81Du with get, set
    
    [<ConfigValue("DiffMultHPToPCLGlobalId",
    "Global with gamesettings mult damage to player",
    "Global has value of GameSetting - fDiffMultHPToPCL", ConfigEntryFlags.PreferHex)>]
    member val DiffMultHPToPCLGlobalId = 0x81Cu with get, set

    [<ConfigValue("OnAdjustEffectHookEnable",
    "On Adjust Effect Hook",
    "Disable or enable hook")>]
    member val OnAdjustEffectHookEnable = true with get, set

    [<ConfigValue("OnWeaponHitHookEnable",
    "On Weapon Hit Hook",
    "Disable or enable hook")>]
    member val OnWeaponHitHookEnable = true with get, set

    [<ConfigValue("OnAttackDataHookEnable",
    "On Attack Data Hook",
    "Disable or enable hook")>]
    member val OnAttackDataHookEnable = true with get, set

    [<ConfigValue("OnAnimationHackHookEnable",
    "On Animation Hack Hook",
    "Disable or enable hook")>]
    member val OnAnimationHackHookEnable = true with get, set
    
    // for PLAYER
    member self.GlobalWeaponSwingLeft = 
        match Call.TESFormLookupFormFromFile(self.WeaponSwingLeftGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalWeaponSwingLeftPower =
        match Call.TESFormLookupFormFromFile(self.WeaponSwingLeftPowerGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalWeaponSwingRight =
        match Call.TESFormLookupFormFromFile(self.WeaponSwingRightGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalWeaponSwingRightPower =
        match Call.TESFormLookupFormFromFile(self.WeaponSwingRightPowerGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalJump =
        match Call.TESFormLookupFormFromFile(self.JumpGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalBowDraw =
        match Call.TESFormLookupFormFromFile(self.BowDrawGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalArrowRelease =
        match Call.TESFormLookupFormFromFile(self.ArrowReleaseGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalBowReset =
        match Call.TESFormLookupFormFromFile(self.BowResetGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalBoltRelease =
        match Call.TESFormLookupFormFromFile(self.BoltReleaseGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalReloadStart =
        match Call.TESFormLookupFormFromFile(self.ReloadStartGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalReloadStop =
        match Call.TESFormLookupFormFromFile(self.ReloadStopGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GlobalWeapEquipOut =
        match Call.TESFormLookupFormFromFile(self.WeapEquipOutGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None


    // for NPC
    member self.SpellWeaponSwingLeft = 
        match Call.TESFormLookupFormFromFile(self.WeaponSwingLeftSpellId, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellWeaponSwingLeftPower =
        match Call.TESFormLookupFormFromFile(self.WeaponSwingLeftPowerSpellId, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellWeaponSwingRight =
        match Call.TESFormLookupFormFromFile(self.WeaponSwingRightSpellId, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellWeaponSwingRightPower =
        match Call.TESFormLookupFormFromFile(self.WeaponSwingRightPowerSpellId, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellJump =
        match Call.TESFormLookupFormFromFile(self.JumpSpellId, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellBowDraw =
        match Call.TESFormLookupFormFromFile(self.BowDrawSpellid, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellArrowRelease =
        match Call.TESFormLookupFormFromFile(self.ArrowReleaseSpellId, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellBowReset =
        match Call.TESFormLookupFormFromFile(self.BowResetSpellId, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellBoltRelease =
        match Call.TESFormLookupFormFromFile(self.BoltReleaseSpellId, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellReloadStart =
        match Call.TESFormLookupFormFromFile(self.ReloadStartSpellId, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellReloadStop =
        match Call.TESFormLookupFormFromFile(self.ReloadStopSpellId, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.SpellWeapEquipOut =
        match Call.TESFormLookupFormFromFile(self.WeapEquipOutSpellId, self.ModName) with
        | :? SpellItem as spell -> Some spell
        | _ -> None

    member self.GameSettingDamageByPlayer =
        match Call.TESFormLookupFormFromFile(self.DiffMultHPByPCLGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
        | _ -> None

    member self.GameSettingDamageToPlayer =
        match Call.TESFormLookupFormFromFile(self.DiffMultHPToPCLGlobalId, self.ModName) with
        | :? TESGlobal as globalValue -> Some globalValue
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