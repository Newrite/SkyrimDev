



namespace BloodMagick
    
    module private InternalBloodSpells =
        
        val indexAvDrainHealth: int
        
        val magFailSoundId: uint32
        
        val bloodMagickAbilityId: uint32
        
        val skyrim: string
        
        val bloodMagickMod: string
        
        val castHealthTreshold: float32
        
        val bloodMagickAbility: unit -> NetScriptFramework.SkyrimSE.SpellItem
        
        val Log: (string -> unit)
    
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
            
            val isActorMagicCasterValidAndUnderBloodMagick:
              caster: NetScriptFramework.SkyrimSE.ActorMagicCaster ->
                magicItem: NetScriptFramework.SkyrimSE.MagicItem -> bool
            
            val isActorValidAndUnderBloodMagick:
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

