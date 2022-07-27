namespace ResourceManager

open NetScriptFramework.Tools
open NetScriptFramework.SkyrimSE
open Wrapper

[<Sealed>]
type public Settings() =

    let mutable unarmedWeaponID = 0x5923u

    let mutable keywordStaminaToMagicka = 0x5924u
    let mutable keywordStaminaToHealth = 0x5925u
    let mutable keywordHealthToMagicka = 0x5926u
    let mutable keywordHealthToStamina = 0x5927u
    let mutable keywordMagickaToHealth = 0x5928u
    let mutable keywordMagickaToStamina = 0x5929u

    let mutable globalSoulsOn = 0x80Au
    let mutable globalWeightMult = 0x802u
    let mutable globalMult = 0x801u
    let mutable globalJumpValue = 0x807u
    let mutable globalPowerModifier = 0x5922u
    let mutable globalRecoveryTime = 0x805u
    let mutable globalMultRecoveryValue = 0x806u

    let mutable keywordArrowWeightNoneID = 0x9d1f4du
    let mutable keywordArrowWeightLightID = 0x9d1f49u
    let mutable keywordArrowWeightMediumID = 0x9d1f4au
    let mutable keywordArrowWeightHeavyID = 0x9d1f4bu
    let mutable keywordArrowWeightMassiveID = 0x9d1f4cu

    let mutable staminaSoulsON = true
    let mutable healthSoulsON = false
    let mutable magickaSoulsON = false

    let mutable arrowBoltReleaseSpellID = 0x808u

    let mutable crossbowPerkID = 0x17b8c1u

    let mutable modName = "Requiem for a Dream - Resource Manager.esp"

    let requiem = "Requiem.esp"

    [<ConfigValue("ModName", "Mod name where FormIDs", "Form id gets from this file")>]
    member private self.ModName
        with get() = modName
        and set name = modName <- name

    [<ConfigValue("KeywordStaminaToMagicka", "Keyword conversion stamina to magicka form id", "When keyword apply on actor or item on actor then stamina cost from weapon conversion to magicka cost", ConfigEntryFlags.PreferHex)>]
    member private self.KeywordStaminaToMagicka
        with get() = keywordStaminaToMagicka
        and set id = keywordStaminaToMagicka <- id

    [<ConfigValue("KeywordStaminaToHealth", "Keyword conversion stamina to health form id", "When keyword apply on actor or item on actor then stamina cost from weapon conversion to health cost", ConfigEntryFlags.PreferHex)>]
    member private self.KeywordStaminaToHealth
        with get() = keywordStaminaToHealth
        and set id = keywordStaminaToHealth <- id

    [<ConfigValue("KeywordHealthToMagicka", "Keyword conversion health to magicka form id", "When keyword apply on actor or item on actor then health cost from weapon conversion to magicka cost", ConfigEntryFlags.PreferHex)>]
    member private self.KeywordHealthToMagicka
        with get() = keywordHealthToMagicka
        and set id = keywordHealthToMagicka <- id

    [<ConfigValue("KeywordHealthToStamina", "Keyword conversion health to stamina form id", "When keyword apply on actor or item on actor then health cost from weapon conversion to stamina cost", ConfigEntryFlags.PreferHex)>]
    member private self.KeywordHealthToStamina
        with get() = keywordHealthToStamina
        and set id = keywordHealthToStamina <- id

    [<ConfigValue("KeywordMagickaToHealth", "Keyword conversion magicka to health form id", "When keyword apply on actor or item on actor then magicka cost from weapon conversion to health cost", ConfigEntryFlags.PreferHex)>]
    member private self.KeywordMagickaToHealth
        with get() = keywordMagickaToHealth
        and set id = keywordMagickaToHealth <- id

    [<ConfigValue("KeywordMagickaToStamina", "Keyword conversion magicka to stamina form id", "When keyword apply on actor or item on actor then magicka cost from weapon conversion to stamina cost", ConfigEntryFlags.PreferHex)>]
    member private self.KeywordMagickaToStamina
        with get() = keywordMagickaToStamina
        and set id = keywordMagickaToStamina <- id
    
    [<ConfigValue("GlobalSoulsON", "Toggles soulsmod global form id", "Global variable for set on or off souls mod for resource consumption, 0.0 - off, anyway else - on", ConfigEntryFlags.PreferHex)>]
    member private self.GlobalSoulsON
        with get() = globalSoulsOn
        and set id = globalSoulsOn <- id

    [<ConfigValue("GlobalWeightMult", "Global weight mult form id", "Global variable mult weapon weight, or damage if weight = 0, for calc cost", ConfigEntryFlags.PreferHex)>]
    member private self.GlobalWeightMult
        with get() = globalWeightMult
        and set id = globalWeightMult <- id

    [<ConfigValue("GlobalMult", "Global mult form id", "Global variable mult all calc cost", ConfigEntryFlags.PreferHex)>]
    member private self.GlobalMult
        with get() = globalMult
        and set id = globalMult <- id

    [<ConfigValue("GlobalJumpValue", "Global jump cost id", "Global variable flat jump cost", ConfigEntryFlags.PreferHex)>]
    member private self.GlobalJumpValue
        with get() = globalJumpValue
        and set id = globalJumpValue <- id

    [<ConfigValue("GlobalPowerModifier", "Global power modifier cost form id", "Global variable mult power attack costa", ConfigEntryFlags.PreferHex)>]
    member private self.GlobalPowerModifier
        with get() = globalPowerModifier
        and set id = globalPowerModifier <- id

    [<ConfigValue("GlobalRecoveryTime", "Global recovery time for souls form id", "Global variable set deley before resource restore (in seconds)", ConfigEntryFlags.PreferHex)>]
    member private self.GlobalRecoveryTime
        with get() = globalRecoveryTime
        and set id = globalRecoveryTime <- id

    [<ConfigValue("GlobalMultRecoveryValue", "Global mult recovery value for souls form id", "Global variable set mult how much resource restore (coef, default 0.7)", ConfigEntryFlags.PreferHex)>]
    member private self.GlobalMultRecoveryValue
        with get() = globalMultRecoveryValue
        and set id = globalMultRecoveryValue <- id

    [<ConfigValue("UnarmedWeaponID", "Weapon unarmed stat form id", "Need for weapon stat if used hand to hand", ConfigEntryFlags.PreferHex)>]
    member private self.UnarmedWeaponID
        with get() = unarmedWeaponID
        and set id = unarmedWeaponID <- id

    [<ConfigValue("StaminaSoulsON", "Stamina souls", "If souls on, check it work for stamina or not")>]
    member self.StaminaSoulsON
        with get() = staminaSoulsON
        and set bool = staminaSoulsON <- bool

    [<ConfigValue("HealthSoulsON", "Health souls", "If souls on, check it work for health or not")>]
    member self.HealthSoulsON
        with get() = healthSoulsON
        and set bool = healthSoulsON <- bool

    [<ConfigValue("MagickaSoulsON", "Magicka souls", "If souls on, check it work for magicka or not")>]
    member self.MagickaSoulsON
        with get() = magickaSoulsON
        and set bool = magickaSoulsON <- bool

    [<ConfigValue("ArrowBoltReleaseSpellID", "Spell cast arrow bolt release form id", "Spell cast when actor fire from bow or crossbow (example damage magicka when fire Bound Arrow)", ConfigEntryFlags.PreferHex)>]
    member private self.ArrowBoltReleaseSpellID
        with get() = arrowBoltReleaseSpellID
        and set id = arrowBoltReleaseSpellID <- id

    [<ConfigValue("CrossbowPerkID", "Perk crossbow reload form id", "If actor have this perk, cost for crossbow reload 30% less", ConfigEntryFlags.PreferHex)>]
    member private self.CrossbowPerkID
        with get() = crossbowPerkID
        and set id = crossbowPerkID <- id

    [<ConfigValue("KeywordArrowWeightNoneID", "Keyword arrow none weight form id", "Use this keyword for calc draw or reload cost for bow or crossbow if arrowbolt weight none", ConfigEntryFlags.PreferHex)>]
    member private self.KeywordArrowWeightNoneID
        with get() = keywordArrowWeightNoneID
        and set id = keywordArrowWeightNoneID <- id

    [<ConfigValue("KeywordArrowWeightLightID", "Keyword arrow light weight form id", "Use this keyword for calc draw or reload cost for bow or crossbow if arrowbolt weight light", ConfigEntryFlags.PreferHex)>]
    member private self.KeywordArrowWeightLightID
        with get() = keywordArrowWeightLightID
        and set id = keywordArrowWeightLightID <- id

    [<ConfigValue("KeywordArrowWeightMediumID", "Keyword arrow medium weight form id", "Use this keyword for calc draw or reload cost for bow or crossbow if arrowbolt weight medium", ConfigEntryFlags.PreferHex)>]
    member private self.KeywordArrowWeightMediumID
        with get() = keywordArrowWeightMediumID
        and set id = keywordArrowWeightMediumID <- id

    [<ConfigValue("KeywordArrowWeightHeavyID", "Keyword arrow heavy weight form id", "Use this keyword for calc draw or reload cost for bow or crossbow if arrowbolt weight heavy", ConfigEntryFlags.PreferHex)>]
    member private self.KeywordArrowWeightHeavyID
        with get() = keywordArrowWeightHeavyID
        and set id = keywordArrowWeightHeavyID <- id

    [<ConfigValue("KeywordArrowWeightMassiveID", "Keyword arrow massive weight form id", "Use this keyword for calc draw or reload cost for bow or crossbow if arrowbolt weight massive", ConfigEntryFlags.PreferHex)>]
    member private self.KeywordArrowWeightMassiveID
        with get() = keywordArrowWeightMassiveID
        and set id = keywordArrowWeightMassiveID <- id


    member self.StaminaToMagicka = Call.TESFormLookupFormFromFile(self.KeywordStaminaToMagicka, self.ModName) :?> BGSKeyword

    member self.StaminaToHealth = Call.TESFormLookupFormFromFile(self.KeywordStaminaToHealth, self.ModName) :?> BGSKeyword

    member self.HealthToMagicka = Call.TESFormLookupFormFromFile(self.KeywordHealthToMagicka, self.ModName) :?> BGSKeyword

    member self.HealthToStamina = Call.TESFormLookupFormFromFile(self.KeywordHealthToStamina, self.ModName) :?> BGSKeyword

    member self.MagickaToHealth = Call.TESFormLookupFormFromFile(self.KeywordMagickaToHealth, self.ModName) :?> BGSKeyword

    member self.MagickaToStamina = Call.TESFormLookupFormFromFile(self.KeywordMagickaToStamina, self.ModName) :?> BGSKeyword

    member self.KeywordArrowWeightNone = Call.TESFormLookupFormFromFile(self.KeywordArrowWeightNoneID, requiem) :?> BGSKeyword

    member self.KeywordArrowWeightLight = Call.TESFormLookupFormFromFile(self.KeywordArrowWeightLightID, requiem) :?> BGSKeyword

    member self.KeywordArrowWeightMedium = Call.TESFormLookupFormFromFile(self.KeywordArrowWeightMediumID, requiem) :?> BGSKeyword

    member self.KeywordArrowWeightHeavy = Call.TESFormLookupFormFromFile(self.KeywordArrowWeightHeavyID, requiem) :?> BGSKeyword

    member self.KeywordArrowWeightMassive = Call.TESFormLookupFormFromFile(self.KeywordArrowWeightMassiveID, requiem) :?> BGSKeyword

    member self.UnarmedWeapon = Call.TESFormLookupFormFromFile(self.UnarmedWeaponID, self.ModName) :?> TESObjectWEAP

    member self.ArrowBoltReleaseSpell = Call.TESFormLookupFormFromFile(self.ArrowBoltReleaseSpellID, self.ModName) :?> SpellItem

    member self.CrossbowPerk = Call.TESFormLookupFormFromFile(self.CrossbowPerkID, requiem) :?> BGSPerk

    member self.SoulsON() =
        Call.TESFormLookupFormFromFile(self.GlobalSoulsON, self.ModName) :?> TESGlobal
        |>function
        |value when value.FloatValue <> 0.f -> true
        |_ -> false

    member self.RecoveryTime() =
        let value = Call.TESFormLookupFormFromFile(self.GlobalRecoveryTime, self.ModName) :?> TESGlobal
        int64(value.FloatValue * 1000.f)

    member self.WeightMult() = Call.TESFormLookupFormFromFile(self.GlobalWeightMult, self.ModName) :?> TESGlobal

    member self.MultRecoveryValue() = Call.TESFormLookupFormFromFile(self.GlobalMultRecoveryValue, self.ModName) :?> TESGlobal

    member self.Mult() = Call.TESFormLookupFormFromFile(self.GlobalMult, self.ModName) :?> TESGlobal

    member self.JumpValue() = Call.TESFormLookupFormFromFile(self.GlobalJumpValue, self.ModName) :?> TESGlobal

    member self.PowerModifier() = Call.TESFormLookupFormFromFile(self.GlobalPowerModifier, self.ModName) :?> TESGlobal

    member internal self.Load() = ConfigFile.LoadFrom<Settings>(self, "ResourceManager", true);