



namespace ResourceManager
    
    [<Sealed>]
    type Settings =
        
        new: unit -> Settings
        
        member JumpValue: unit -> NetScriptFramework.SkyrimSE.TESGlobal
        
        member internal Load: unit -> bool
        
        member Mult: unit -> NetScriptFramework.SkyrimSE.TESGlobal
        
        member MultRecoveryValue: unit -> NetScriptFramework.SkyrimSE.TESGlobal
        
        member PowerModifier: unit -> NetScriptFramework.SkyrimSE.TESGlobal
        
        member RecoveryTime: unit -> int64
        
        member SoulsON: unit -> bool
        
        member WeightMult: unit -> NetScriptFramework.SkyrimSE.TESGlobal
        
        member ArrowBoltReleaseSpell: NetScriptFramework.SkyrimSE.SpellItem
        
        [<NetScriptFramework.Tools.ConfigValue
          ("ArrowBoltReleaseSpellID", "Spell cast arrow bolt release form id",
           "Spell cast when actor fire from bow or crossbow (example damage magicka when fire Bound Arrow)",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private ArrowBoltReleaseSpellID: uint32
        
        member CrossbowPerk: NetScriptFramework.SkyrimSE.BGSPerk
        
        [<NetScriptFramework.Tools.ConfigValue
          ("CrossbowPerkID", "Perk crossbow reload form id",
           "If actor have this perk, cost for crossbow reload 30% less",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private CrossbowPerkID: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("GlobalJumpValue", "Global jump cost id",
           "Global variable flat jump cost",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private GlobalJumpValue: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("GlobalMult", "Global mult form id",
           "Global variable mult all calc cost",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private GlobalMult: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("GlobalMultRecoveryValue",
           "Global mult recovery value for souls form id",
           "Global variable set mult how much resource restore (coef, default 0.7)",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private GlobalMultRecoveryValue: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("GlobalPowerModifier", "Global power modifier cost form id",
           "Global variable mult power attack costa",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private GlobalPowerModifier: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("GlobalRecoveryTime", "Global recovery time for souls form id",
           "Global variable set deley before resource restore (in seconds)",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private GlobalRecoveryTime: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("GlobalSoulsON", "Toggles soulsmod global form id",
           "Global variable for set on or off souls mod for resource consumption, 0.0 - off, anyway else - on",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private GlobalSoulsON: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("GlobalWeightMult", "Global weight mult form id",
           "Global variable mult weapon weight, or damage if weight = 0, for calc cost",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private GlobalWeightMult: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("HealthSoulsON", "Health souls",
           "If souls on, check it work for health or not",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member HealthSoulsON: bool
        
        member HealthToMagicka: NetScriptFramework.SkyrimSE.BGSKeyword
        
        member HealthToStamina: NetScriptFramework.SkyrimSE.BGSKeyword
        
        member KeywordArrowWeightHeavy: NetScriptFramework.SkyrimSE.BGSKeyword
        
        [<NetScriptFramework.Tools.ConfigValue
          ("KeywordArrowWeightHeavyID", "Keyword arrow heavy weight form id",
           "Use this keyword for calc draw or reload cost for bow or crossbow if arrowbolt weight heavy",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private KeywordArrowWeightHeavyID: uint32
        
        member KeywordArrowWeightLight: NetScriptFramework.SkyrimSE.BGSKeyword
        
        [<NetScriptFramework.Tools.ConfigValue
          ("KeywordArrowWeightLightID", "Keyword arrow light weight form id",
           "Use this keyword for calc draw or reload cost for bow or crossbow if arrowbolt weight light",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private KeywordArrowWeightLightID: uint32
        
        member KeywordArrowWeightMassive: NetScriptFramework.SkyrimSE.BGSKeyword
        
        [<NetScriptFramework.Tools.ConfigValue
          ("KeywordArrowWeightMassiveID", "Keyword arrow massive weight form id",
           "Use this keyword for calc draw or reload cost for bow or crossbow if arrowbolt weight massive",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private KeywordArrowWeightMassiveID: uint32
        
        member KeywordArrowWeightMedium: NetScriptFramework.SkyrimSE.BGSKeyword
        
        [<NetScriptFramework.Tools.ConfigValue
          ("KeywordArrowWeightMediumID", "Keyword arrow medium weight form id",
           "Use this keyword for calc draw or reload cost for bow or crossbow if arrowbolt weight medium",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private KeywordArrowWeightMediumID: uint32
        
        member KeywordArrowWeightNone: NetScriptFramework.SkyrimSE.BGSKeyword
        
        [<NetScriptFramework.Tools.ConfigValue
          ("KeywordArrowWeightNoneID", "Keyword arrow none weight form id",
           "Use this keyword for calc draw or reload cost for bow or crossbow if arrowbolt weight none",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private KeywordArrowWeightNoneID: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("KeywordHealthToMagicka",
           "Keyword conversion health to magicka form id",
           "When keyword apply on actor or item on actor then health cost from weapon conversion to magicka cost",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private KeywordHealthToMagicka: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("KeywordHealthToStamina",
           "Keyword conversion health to stamina form id",
           "When keyword apply on actor or item on actor then health cost from weapon conversion to stamina cost",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private KeywordHealthToStamina: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("KeywordMagickaToHealth",
           "Keyword conversion magicka to health form id",
           "When keyword apply on actor or item on actor then magicka cost from weapon conversion to health cost",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private KeywordMagickaToHealth: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("KeywordMagickaToStamina",
           "Keyword conversion magicka to stamina form id",
           "When keyword apply on actor or item on actor then magicka cost from weapon conversion to stamina cost",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private KeywordMagickaToStamina: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("KeywordStaminaToHealth",
           "Keyword conversion stamina to health form id",
           "When keyword apply on actor or item on actor then stamina cost from weapon conversion to health cost",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private KeywordStaminaToHealth: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("KeywordStaminaToMagicka",
           "Keyword conversion stamina to magicka form id",
           "When keyword apply on actor or item on actor then stamina cost from weapon conversion to magicka cost",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private KeywordStaminaToMagicka: uint32
        
        [<NetScriptFramework.Tools.ConfigValue
          ("MagickaSoulsON", "Magicka souls",
           "If souls on, check it work for magicka or not",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member MagickaSoulsON: bool
        
        member MagickaToHealth: NetScriptFramework.SkyrimSE.BGSKeyword
        
        member MagickaToStamina: NetScriptFramework.SkyrimSE.BGSKeyword
        
        [<NetScriptFramework.Tools.ConfigValue
          ("ModName", "Mod name where FormIDs", "Form id gets from this file",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member private ModName: string
        
        [<NetScriptFramework.Tools.ConfigValue
          ("StaminaSoulsON", "Stamina souls",
           "If souls on, check it work for stamina or not",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member StaminaSoulsON: bool
        
        member StaminaToHealth: NetScriptFramework.SkyrimSE.BGSKeyword
        
        member StaminaToMagicka: NetScriptFramework.SkyrimSE.BGSKeyword
        
        member UnarmedWeapon: NetScriptFramework.SkyrimSE.TESObjectWEAP
        
        [<NetScriptFramework.Tools.ConfigValue
          ("UnarmedWeaponID", "Weapon unarmed stat form id",
           "Need for weapon stat if used hand to hand",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private UnarmedWeaponID: uint32

namespace ResourceManager
    
    module private InternalVariables =
        
        val Log: (string -> unit)
        
        val Settings: Settings
        
        val Timer: NetScriptFramework.Tools.Timer
    
    module private Manager =
        
        type ActorValues =
            {
              AttackCost: NetScriptFramework.SkyrimSE.ActorValueIndices
              PowerAttackCost: NetScriptFramework.SkyrimSE.ActorValueIndices
            }
        
        [<RequireQualifiedAccess>]
        type DrainValue =
            | Stamina
            | Health
            | Magicka
            | HealthStamina
            | MagickaStamina
            | HealthMagicka
            | HealthMagickaStamina
            
            static member
              EvalActorValue: actor: NetScriptFramework.SkyrimSE.Actor
                              -> weap: NetScriptFramework.SkyrimSE.TESObjectWEAP
                                -> NetScriptFramework.SkyrimSE.TESObjectWEAP *
                                   DrainValue
        
        [<RequireQualifiedAccess>]
        type AttackState =
            | Draw of DrainValue * float32
            | Reload of DrainValue * float32
            | Swing
            | None
        
        [<RequireQualifiedAccess>]
        type WeaponType =
            | Ranged
            | Melee
        
        [<NoComparison>]
        type Attacker =
            {
              Address: nativeint
              LastAttackTime: int64
              CurrentAttack: AttackState
              IsFirstAttack: bool
              NextStaminaDrain: float32
              NextHealthDrain: float32
              NextMagickaDrain: float32
              TotalStaminaDrain: float32
              TotalHealthDrain: float32
              TotalMagickaDrain: float32
              Owner: NetScriptFramework.SkyrimSE.Actor
            }
        
        [<RequireQualifiedAccess; NoComparison>]
        type AnimationAction =
            | Attack of NetScriptFramework.SkyrimSE.TESObjectWEAP * DrainValue
            | PowerAttack of
              NetScriptFramework.SkyrimSE.TESObjectWEAP * DrainValue
            | Jump
            | BowDraw of NetScriptFramework.SkyrimSE.TESObjectWEAP * DrainValue
            | ArrowRelease
            | BowReset
            | BoltRelease
            | ReloadStart of
              NetScriptFramework.SkyrimSE.TESObjectWEAP * DrainValue
            | ReloadStop
            | WeapEquipOut
        
        [<NoComparison>]
        type Context =
            {
              Values: ActorValues
              Animation: AnimationAction
              SpellForArrowCost: NetScriptFramework.SkyrimSE.SpellItem
              AttackerState: Attacker
              HotReloadPerk: NetScriptFramework.SkyrimSE.BGSPerk
            }
        
        val KeywordHealthDrain: unit -> NetScriptFramework.SkyrimSE.BGSKeyword
        
        val KeywordMagickaDrain: unit -> NetScriptFramework.SkyrimSE.BGSKeyword
        
        val KeywordStaminaDrain: unit -> NetScriptFramework.SkyrimSE.BGSKeyword
        
        val AttackersCache:
          System.Collections.Concurrent.ConcurrentDictionary<nativeint,Attacker>
        
        val ammoWeight: actor: NetScriptFramework.SkyrimSE.Actor -> float32
        
        val evalWeaponWeight:
          weap: NetScriptFramework.SkyrimSE.TESObjectWEAP -> float32
        
        val evalDrainMelee:
          ctx: Context -> isPowerAttack: bool
          -> weap: NetScriptFramework.SkyrimSE.TESObjectWEAP -> float32
        
        val evalDrainRanged:
          ctx: Context -> weap: NetScriptFramework.SkyrimSE.TESObjectWEAP
            -> float32
        
        val UpdateCache: (nativeint -> Attacker -> Attacker)
        
        val DrainsHandler:
          attk: Attacker -> drainAmount: float32 -> actorValueDrain: DrainValue
          -> attackType: WeaponType -> unit
        
        val ResourceManagerHandler: ctx: Context -> unit
    
    type ResourceManagerPlugin =
        inherit NetScriptFramework.Plugin
        
        new: unit -> ResourceManagerPlugin
        
        override Initialize: loadedAny: bool -> bool
        
        member private init: unit -> unit
        
        override Author: string
        
        override Key: string
        
        override Name: string
        
        override RequiredLibraryVersion: int
        
        override Version: int

