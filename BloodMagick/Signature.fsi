



namespace BloodMagick
    
    [<Sealed>]
    type Settings =
        
        new: unit -> Settings
        
        member internal Load: unit -> bool
        
        [<NetScriptFramework.Tools.ConfigValue
          ("BloodMagickAbility", "Spell on player for activate blood magick",
           "When ability on player, magic cost health (or stat from indexStateToDrain",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private BloodMagickAbility: uint32
        
        member BloodMagickSpell: NetScriptFramework.SkyrimSE.SpellItem
        
        [<NetScriptFramework.Tools.ConfigValue
          ("IndexStateToDrain",
           "Index of state, indexes: https://en.uesp.net/wiki/Skyrim_Mod:Actor_Value_Indices",
           "Index actor value for cost magicka",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member private IndexStateToDrain: int
        
        [<NetScriptFramework.Tools.ConfigValue
          ("ModName", "Mod name where FormIDs", "Form id gets from this file",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member private ModName: string
        
        member indexState: int

namespace BloodMagick
    
    module private InternalVariables =
        
        val Log: (string -> unit)
        
        val Settings: Settings
    
    module BloodMagickPlugin =
        
        val costsDict:
          System.Collections.Concurrent.ConcurrentDictionary<nativeint,float32>
        
        [<RequireQualifiedAccess>]
        type MenuState =
            | MagicMenuOpen
            | MenuClosing
            | WaitOpen
    
    type BloodMagickPlugin =
        inherit NetScriptFramework.Plugin
        
        new: unit -> BloodMagickPlugin
        
        member
          private CheckPlayer: actor: #NetScriptFramework.SkyrimSE.Actor -> bool
        
        override Initialize: bool -> bool
        
        member private init: unit -> unit
        
        override Author: string
        
        override Key: string
        
        override Name: string
        
        override RequiredLibraryVersion: int
        
        override Version: int

