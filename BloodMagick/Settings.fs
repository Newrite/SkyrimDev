namespace BloodMagick

open NetScriptFramework.Tools
open NetScriptFramework.SkyrimSE
open Wrapper

[<Sealed>]
type public Settings() =

    let mutable bloodMagickAbility = 0x23F6CCu

    let mutable indexStateToDrain = 24

    let mutable modName = "Requiem - Breaking Bad.esp"

    [<ConfigValue("ModName", "Mod name where FormIDs", "Form id gets from this file")>]
    member private self.ModName
        with get() = modName
        and set name = modName <- name

    [<ConfigValue("BloodMagickAbility", "Spell on player for activate blood magick", "When ability on player, magic cost health (or stat from indexStateToDrain", ConfigEntryFlags.PreferHex)>]
    member private self.BloodMagickAbility
        with get() = bloodMagickAbility
        and set id = bloodMagickAbility <- id

    [<ConfigValue("IndexStateToDrain", "Index of state, indexes: https://en.uesp.net/wiki/Skyrim_Mod:Actor_Value_Indices", "Index actor value for cost magicka")>]
    member private self.IndexStateToDrain
        with get() = indexStateToDrain
        and set id = indexStateToDrain <- id


    member self.BloodMagickSpell = Call.TESFormLookupFormFromFile(self.BloodMagickAbility, self.ModName) :?> SpellItem

    member self.indexState = self.IndexStateToDrain

    member internal self.Load() = ConfigFile.LoadFrom<Settings>(self, "BloodMagick", true);