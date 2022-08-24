



namespace ArcaneCurse
    
    module private InternalVariables =
        
        val Log: (string -> unit)
        
        val lichKeyword: string
        
        val hetotTome: string
    
    module private CursedGlobals =
        
        val halfDamageKeyword: string
        
        val fullDamageKeyword: string
        
        val halfCostKeyword: string
        
        val fullCostKeyword: string
        
        val halfSummonKeyword: string
        
        val fullSummonKeyword: string
        
        val (|HalfDamage|FullDamage|HalfCost|FullCost|HalfSummon|FullSummon|NoFind|) :
          player: NetScriptFramework.SkyrimSE.PlayerCharacter ->
            Choice<unit,unit,unit,unit,unit,unit,unit>
        
        val (|Hetot|_|) :
          player: NetScriptFramework.SkyrimSE.PlayerCharacter -> unit option
        
        val handleGlobals:
          gDamage: NetScriptFramework.SkyrimSE.TESGlobal ->
            gCost: NetScriptFramework.SkyrimSE.TESGlobal ->
            gSummon: NetScriptFramework.SkyrimSE.TESGlobal ->
            player: NetScriptFramework.SkyrimSE.PlayerCharacter -> unit
    
    module private OnSpellCast =
        
        val lichMult: actor: #NetScriptFramework.SkyrimSE.Actor -> float32
        
        val isHetot: actor: #NetScriptFramework.SkyrimSE.Actor -> bool
        
        type MultsFromPerk =
            {
              SpellLevelCurseMult: float32
              SpellLevelDebuff: float32
            }
        
        module MultsFromPerk =
            
            val create:
              curseMult: float32 -> debuffMult: float32 -> MultsFromPerk
        
        type CursesValue =
            {
              CurseValue: float32
              DebuffValue: float32
            }
        
        module CursesValue =
            
            val create:
              curseValue: float32 -> debuffValue: float32 -> CursesValue
        
        [<NoComparison>]
        type ContextMagicCast =
            {
              Owner: NetScriptFramework.SkyrimSE.Actor
              Caster: NetScriptFramework.SkyrimSE.ActorMagicCaster
              CursedDebuff: NetScriptFramework.SkyrimSE.SpellItem
              Spell: NetScriptFramework.SkyrimSE.SpellItem
              Mults: CursesValue
              IsHetot: bool
            }
        
        val castingPerks:
          (uint32 * MultsFromPerk *
           NetScriptFramework.SkyrimSE.ActorValueIndices)[]
        
        val getCurseMult:
          actor: #NetScriptFramework.SkyrimSE.Actor ->
            av: NetScriptFramework.SkyrimSE.ActorValueIndices -> float32
        
        val getCurseValue:
          formID: uint32 ->
            actor: #NetScriptFramework.SkyrimSE.Actor -> CursesValue option
        
        val applyCurse: ctx: ContextMagicCast -> unit
        
        val handleSpell: ctx: ContextMagicCast -> unit
        
        val enchantmentHandler:
          caster: NetScriptFramework.SkyrimSE.Actor -> unit
    
    type ArcaneCursePlugin =
        inherit NetScriptFramework.Plugin
        
        new: unit -> ArcaneCursePlugin
        
        override Initialize: loadedAny: bool -> bool
        
        member private init: unit -> unit
        
        override Author: string
        
        member
          private DebuffSpell: Option<NetScriptFramework.SkyrimSE.SpellItem>
        
        member
          private Globals: Option<NetScriptFramework.SkyrimSE.TESGlobal *
                                  NetScriptFramework.SkyrimSE.TESGlobal *
                                  NetScriptFramework.SkyrimSE.TESGlobal>
        
        override Key: string
        
        override Name: string
        
        override RequiredLibraryVersion: int
        
        override Version: int

