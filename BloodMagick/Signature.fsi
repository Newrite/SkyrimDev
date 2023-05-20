

namespace FSharp

namespace Blood
    
    [<Sealed>]
    type Settings =
        
        new: unit -> Settings
        
        member internal Load: unit -> bool
        
        [<NetScriptFramework.Tools.ConfigValue
          ("BloodAbilityEnable", "Наличие абилки, а не кейворда на заклинании",
           "Использует айди указанное выше что проверить на заклинателе наличие абилки по этому айди, вместо проверки кейворда на спеле",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member BloodAbilityEnable: bool
        
        member BloodSpellsAb: NetScriptFramework.SkyrimSE.SpellItem option
        
        [<NetScriptFramework.Tools.ConfigValue
          ("BloodSpellsKeyword",
           "Кейворд на спелах которые должны тратить здоровье",
           "Заклинания с этим кейвордом расходуют здоровье, а не магию",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private BloodSpellsKeyword: uint32
        
        member BloodSpellsKwd: NetScriptFramework.SkyrimSE.BGSKeyword option
        
        [<NetScriptFramework.Tools.ConfigValue
          ("BloodSpellsPercentCostEnable",
           "Спелы МК так же дополнительно расходуют хп в процентах",
           "Использует магнитуду эффекта с кейвордом (который указан ниже), для определения сколько дополнительно хп должен потратить спелл в процентах",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member BloodSpellsPercentCostEnable: bool
        
        [<NetScriptFramework.Tools.ConfigValue
          ("BloodSpellsPercentCostKeyword",
           "Кейворд на спелах который указывает процентный расход",
           "Заклинания с этим кейвордом расходуют здоровье дополнительно расходуют хп в процентах",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (8UL))>]
        member private BloodSpellsPercentCostKeyword: uint32
        
        member
          BloodSpellsPercentKwd: NetScriptFramework.SkyrimSE.BGSKeyword option
        
        [<NetScriptFramework.Tools.ConfigValue
          ("ModName", "Имя мода",
           "ФормИД указанные в настрокйах ищутся в этом моде",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member private ModName: string
        
        [<NetScriptFramework.Tools.ConfigValue
          ("OnlyPlayer", "Только для игрока",
           "Только у игрока особое взаимодействие с этими заклинаниями, нпс используют их как обычно",
           enum<NetScriptFramework.Tools.ConfigEntryFlags> (0UL))>]
        member OnlyPlayer: bool
    
    [<AutoOpen>]
    module Singeltones =
        
        val Log: (string -> unit)
        
        val config: Settings
    
    module Foo =
        
        module private Boo =
            
            val mult: x: int -> y: int -> int
        
        val private add: x: int -> y: int -> int
        
        val multAndAdd: x: int -> y: int -> int

namespace Blood
    
    [<AutoOpen>]
    module private InternalBloodSpells =
        
        val indexAvDrainHealth: int
        
        val magFailSoundId: uint32
        
        val skyrim: string
        
        val castHealthTreshold: float32
    
    [<RequireQualifiedAccess>]
    module BloodMagickPlugin =
        
        module Domen =
            
            type CastCostHandler =
                {
                  mutable Accum: float32
                  mutable Cost: float32
                  mutable Working: bool
                  mutable IterMax: float32
                  mutable IterNow: float32
                }
                
                static member
                  Create: unit -> BloodMagickPlugin.Domen.CastCostHandler
                
                member
                  InterruptHandle: asyncRestore: (float32 ->
                                                    NetScriptFramework.SkyrimSE.Actor ->
                                                    Async<unit>) ->
                                     actor: NetScriptFramework.SkyrimSE.Actor ->
                                     unit
                
                member
                  IterationHandle: failCast: (unit -> unit) ->
                                     actor: NetScriptFramework.SkyrimSE.Actor ->
                                     unit
                
                member ResetToWork: unit -> unit
                
                member
                  StartWorking: numberOfIter: float32 ->
                                  costForIter: float32 -> unit
            
            [<NoComparison>]
            type CastsHandHandler =
                {
                  Caster: NetScriptFramework.SkyrimSE.Actor
                  DualCast: BloodMagickPlugin.Domen.CastCostHandler
                  LeftCast: BloodMagickPlugin.Domen.CastCostHandler
                  RightCast: BloodMagickPlugin.Domen.CastCostHandler
                }
                
                static member
                  Create: caster: NetScriptFramework.SkyrimSE.Actor ->
                            BloodMagickPlugin.Domen.CastsHandHandler
            
            type CostDict =
                System.Collections.Concurrent.ConcurrentDictionary<nativeint,
                                                                   float32>
            
            [<RequireQualifiedAccess>]
            type MenuState =
                | MagicMenuOpen
                | MenuClosing
                | WaitOpen
        
        module private Functions =
            
            val getNumberOfIterationsAndCostForIter:
              chargetTime: float32 -> cost: float32 -> float32 * float32
            
            val failCast: unit -> unit
            
            val canSpendOrFailIterruptIfNot:
              spender: NetScriptFramework.SkyrimSE.ActorMagicCaster ->
                cost: float32 -> bool
            
            val failCastWithHP: unit -> unit
            
            val isConcentrationSpell:
              magicItem: NetScriptFramework.SkyrimSE.MagicItem -> bool
            
            val magicItemNotNullAndIsSpell:
              magicItem: NetScriptFramework.SkyrimSE.MagicItem -> bool
            
            val isAllowBloodCast:
              caster: NetScriptFramework.SkyrimSE.Actor ->
                magicItem: NetScriptFramework.SkyrimSE.MagicItem -> bool
            
            val isAllowBloodCastMagicCaster:
              caster: NetScriptFramework.SkyrimSE.MagicCaster ->
                magicItem: NetScriptFramework.SkyrimSE.MagicItem -> bool
            
            val isActorMagicCasterValidAndIsSpellBloodSpell:
              caster: NetScriptFramework.SkyrimSE.ActorMagicCaster ->
                magicItem: NetScriptFramework.SkyrimSE.MagicItem -> bool
            
            val isMagicCasterValidAndIsSpellBloodSpell:
              magicCaster: NetScriptFramework.SkyrimSE.MagicCaster ->
                magicItem: NetScriptFramework.SkyrimSE.MagicItem -> bool
            
            val isActorValidAndIsSpellBloodSpell:
              actor: NetScriptFramework.SkyrimSE.Actor ->
                magicItem: NetScriptFramework.SkyrimSE.MagicItem -> bool
            
            val asyncHandleRestore:
              amountRestore: float32 ->
                caster: NetScriptFramework.SkyrimSE.Actor -> Async<unit>
        
        val castersDict:
          System.Collections.Concurrent.ConcurrentDictionary<nativeint,
                                                             (BloodMagickPlugin.Domen.CastsHandHandler *
                                                              BloodMagickPlugin.Domen.CostDict)>
        
        val mutable menuState: BloodMagickPlugin.Domen.MenuState
        
        val init: unit -> bool
        
        val onInterruptCast:
          eArg: NetScriptFramework.SkyrimSE.InterruptCastEventArgs -> unit
        
        val onFrameAlways: unit -> unit
        
        val onFrame10ms: unit -> unit
        
        val onCalculateMagickCost:
          eArg: NetScriptFramework.SkyrimSE.CalculateMagicCostEventArgs -> unit
        
        val onSpendMagicCost:
          eArg: NetScriptFramework.SkyrimSE.SpendMagicCostEventArgs -> unit
        
        val onMagicCasterFire:
          eArg: NetScriptFramework.SkyrimSE.MagicCasterFireEventArgs -> unit
    
    type BloodMagickPlugin =
        inherit NetScriptFramework.Plugin
        
        new: unit -> BloodMagickPlugin
        
        override Initialize: bool -> bool
        
        member private init: unit -> unit
        
        override Author: string
        
        override Key: string
        
        override Name: string
        
        override RequiredLibraryVersion: int
        
        override Version: int

