namespace ArcaneCurse

open NetScriptFramework
open NetScriptFramework.SkyrimSE
open NetScriptFramework.Tools
open Wrapper
open Supporter

[<AutoOpen>]
module private InternalVariables =
    let Log = Log.Init "Arcane Curse"
    let lichKeyword = @"POT_IsLichRace"
    let hetotTome = @"MRL_AC_KYWD_HetotTome"

module private CursedGlobals =

    //EditorID кейвордов для гримуаров
    let private halfDamageKeyword = @"MRL_AC_KYWD_HalfDecurseBookDamage"
    let private fullDamageKeyword = @"MRL_AC_KYWD_DecurseBookDamage"
    let private halfCostKeyword = @"MRL_AC_KYWD_HalfDecurseBookCost"
    let private fullCostKeyword = @"MRL_AC_KYWD_DecurseBookCost"
    let private halfSummonKeyword = @"MRL_AC_KYWD_HalfDecurseBookConjuration"
    let private fullSummonKeyword = @"MRL_AC_KYWD_DecurseBookConjuration"
        
    //Активный шаблон для матчинга надетого гримуара
    let private (|HalfDamage|FullDamage|HalfCost|FullCost|HalfSummon|FullSummon|NoFind|) (player: PlayerCharacter) =
        if player.WornHasKeywordText(halfDamageKeyword) then HalfDamage
        elif player.WornHasKeywordText(fullDamageKeyword) then FullDamage
        elif player.WornHasKeywordText(halfCostKeyword) then HalfCost
        elif player.WornHasKeywordText(fullCostKeyword) then FullCost
        elif player.WornHasKeywordText(halfSummonKeyword) then HalfSummon
        elif player.WornHasKeywordText(fullSummonKeyword) then FullSummon
        else NoFind

    let private (|Hetot|_|) (player: PlayerCharacter) =
      if player.WornHasKeywordText(hetotTome) then Some Hetot else None

    //Обработка глобальных переменных, через них выводится описание в способности о текущем состоянии дебафа от аркан курсы
    let handleGlobals (gDamage: TESGlobal) (gCost: TESGlobal) (gSummon: TESGlobal) (player: PlayerCharacter) =
        
        //Проверяем что инстанс игрока не нулл и после этого продолжаем выполнение
        if player <> null then

            //Текущее значение аркан курсы используемое для рассчета цифр для описания
            let currentCurseValue =
                let value = player.GetActorValue(ActorValueIndices.EnchantingSkillAdvance)
                if value >= 100.f then 100.f else value

            //Функция выставления значений глобальным переменным, соответствует реализации перка аркан курсы
            let setGlobals fDamageMult fCostMult fSummonMult =
                gDamage.FloatValue <- currentCurseValue * fDamageMult
                gCost.FloatValue <- currentCurseValue * fCostMult
                gSummon.FloatValue <- currentCurseValue * fSummonMult

            let hetotSet fCostMult fSummonMult =
                gDamage.FloatValue <- 75.f
                gCost.FloatValue <- currentCurseValue * fCostMult
                gSummon.FloatValue <- currentCurseValue * fSummonMult

            //Матчинг надетых гримуаров, если гримуара нет использует стандартные множители
            match player with
            | Hetot -> hetotSet 0.5f 0.25f
            | HalfDamage -> setGlobals 0.25f 0.5f 0.25f
            | FullDamage -> setGlobals 0.f 0.5f 0.25f
            | HalfCost -> setGlobals 0.5f 0.25f 0.25f
            | FullCost -> setGlobals 0.5f 0.f 0.25f
            | HalfSummon -> setGlobals 0.5f 0.5f 0.125f
            | FullSummon -> setGlobals 0.5f 0.5f 0.f
            | NoFind -> setGlobals 0.5f 0.5f 0.25f

        else Log <| sprintf "Player instance null"

module private OnSpellCast =

    let private lichMult (actor: #Actor) =
      if actor.HasKeywordText(lichKeyword) then 0.5f else 1.f

    let isHetot (actor: #Actor) =
      actor.WornHasKeywordText(hetotTome)

    type MultsFromPerk = { SpellLevelCurseMult: float32; SpellLevelDebuff: float32 }

    [<RequireQualifiedAccess>]
    module MultsFromPerk =

      let create curseMult debuffMult = { SpellLevelCurseMult = curseMult; SpellLevelDebuff = debuffMult }

    type CursesValue = { CurseValue: float32; DebuffValue: float32 }

    [<RequireQualifiedAccess>]
    module CursesValue =
      
      let create curseValue debuffValue = { CurseValue = curseValue; DebuffValue = debuffValue }
        
    [<NoComparison>]
    type ContextMagicCast =
        { Owner: Actor
          Caster: ActorMagicCaster
          CursedDebuff: SpellItem
          Spell: SpellItem
          Mults: CursesValue
          IsHetot: bool }

    let private castingPerks = 
      [| 0xF2CA8u, MultsFromPerk.create 0.5f 3.f, ActorValueIndices.DestructionMod
         0xF2CAAu, MultsFromPerk.create 0.5f 3.f, ActorValueIndices.RestorationMod
         0xF2CA6u, MultsFromPerk.create 0.5f 3.f, ActorValueIndices.AlterationMod
         0xF2CA7u, MultsFromPerk.create 0.5f 3.f, ActorValueIndices.ConjurationMod
         0xF2CA9u, MultsFromPerk.create 0.5f 3.f, ActorValueIndices.IllusionMod 
         0xC44BFu, MultsFromPerk.create 1.0f 6.f, ActorValueIndices.DestructionMod
         0xC44C7u, MultsFromPerk.create 1.0f 6.f, ActorValueIndices.RestorationMod
         0xC44B7u, MultsFromPerk.create 1.0f 6.f, ActorValueIndices.AlterationMod
         0xC44BBu, MultsFromPerk.create 1.0f 6.f, ActorValueIndices.ConjurationMod
         0xC44C3u, MultsFromPerk.create 1.0f 6.f, ActorValueIndices.IllusionMod 
         0xC44C0u, MultsFromPerk.create 2.0f 9.f, ActorValueIndices.DestructionMod
         0xC44C8u, MultsFromPerk.create 2.0f 9.f, ActorValueIndices.RestorationMod
         0xC44B8u, MultsFromPerk.create 2.0f 9.f, ActorValueIndices.AlterationMod
         0xC44BCu, MultsFromPerk.create 2.0f 9.f, ActorValueIndices.ConjurationMod
         0xC44C4u, MultsFromPerk.create 2.0f 9.f, ActorValueIndices.IllusionMod 
         0xC44C1u, MultsFromPerk.create 3.0f 12.f, ActorValueIndices.DestructionMod
         0xC44C9u, MultsFromPerk.create 3.0f 12.f, ActorValueIndices.RestorationMod
         0xC44B9u, MultsFromPerk.create 3.0f 12.f, ActorValueIndices.AlterationMod
         0xC44BDu, MultsFromPerk.create 3.0f 12.f, ActorValueIndices.ConjurationMod
         0xC44C5u, MultsFromPerk.create 3.0f 12.f, ActorValueIndices.IllusionMod 
         0xC44C2u, MultsFromPerk.create 4.0f 15.f, ActorValueIndices.DestructionMod
         0xC44CAu, MultsFromPerk.create 4.0f 15.f, ActorValueIndices.RestorationMod
         0xC44BAu, MultsFromPerk.create 4.0f 15.f, ActorValueIndices.AlterationMod
         0xC44BEu, MultsFromPerk.create 4.0f 15.f, ActorValueIndices.ConjurationMod
         0xC44C6u, MultsFromPerk.create 4.0f 15.f, ActorValueIndices.IllusionMod |]

    let private getCurseMult (actor: #Actor) (av: ActorValueIndices) =
      let avf = actor.GetActorValue(av)
      if avf > 60.f then 0.4f
      elif avf <= 0.f then 1.0f
      else 1.f - (avf / 100.f)

    let getCurseValue (formID: uint32) (actor: #Actor) =

        let v = Array.tryFind (fun (i, _, _) -> i = formID) castingPerks
        if v.IsSome then
          let (_, Mults, AVI) = v.Value
          let curseMult = Mults.SpellLevelCurseMult * (getCurseMult actor AVI) * (lichMult actor)
          let curseDebuffMult = Mults.SpellLevelDebuff * (getCurseMult actor AVI) * (lichMult actor)
          Some (CursesValue.create curseMult curseDebuffMult)
        else
          None

    let private applyCurse (ctx: ContextMagicCast) =

        ctx.Owner.DamageActorValue(ActorValueIndices.Stamina, -ctx.Mults.DebuffValue)

        for effect in ctx.CursedDebuff.Effects do
            effect.Magnitude <- ctx.Mults.DebuffValue

        ctx.Owner.CastSpell(ctx.CursedDebuff, ctx.Owner, ctx.Owner)
        |>function
        | true -> ()
        | false -> Log <| sprintf "Fail cast debuff"

        if ctx.IsHetot then
          ctx.Owner.ModActorValue(ActorValueIndices.EnchantingSkillAdvance, ctx.Mults.CurseValue * -3.f)
        else
          ctx.Owner.ModActorValue(ActorValueIndices.EnchantingSkillAdvance, ctx.Mults.CurseValue)

    let handleSpell (ctx: ContextMagicCast) =

      match ctx.Spell.SpellData.CastingType with
      | EffectSettingCastingTypes.Concentration ->

        async {
          while ctx.Caster.State = MagicCastingStates.Concentrating do
            applyCurse { ctx with Mults = { ctx.Mults with CurseValue = ctx.Mults.CurseValue * 0.5f; DebuffValue = ctx.Mults.DebuffValue * 0.5f } }
            System.Threading.Thread.Sleep(1000) } |> Async.StartAsTask |> ignore

      | _ -> applyCurse ctx

    let enchantmentHandler (caster: Actor) =

      if caster.WornHasKeywordText(hetotTome) then
        caster.ModActorValue(ActorValueIndices.EnchantingSkillAdvance, 10.f)
      else
        let currentCurseValue = caster.GetActorValue(ActorValueIndices.EnchantingSkillAdvance)

        if currentCurseValue <= 5.f then
          caster.ModActorValue(ActorValueIndices.EnchantingSkillAdvance, -currentCurseValue)
        else
          caster.ModActorValue(ActorValueIndices.EnchantingSkillAdvance,
              -(1.0f + ( (currentCurseValue / 60.f) * (currentCurseValue / 60.f))) * 4.f)

type ArcaneCursePlugin() =

    inherit Plugin()

    let curseDebuffID = 0x808u
    let gCurseDamageID = 0xDA8u
    let gCurseCostID = 0xDA9u
    let gCurseSummonID = 0xDA7u

    let cursePlugin = "Requiem for a Dream - ArcaneCurse.esp"

    let timer = new Timer()

    let mutable curseDebuffSpell : SpellItem = null
    let mutable gCurseDamage : TESGlobal = null
    let mutable gCurseCost : TESGlobal = null
    let mutable gCursSummon : TESGlobal = null

    let mutable modInit = false

    let mutable lastTimer = 0L

    override self.Key = "arcanecurse"
    override self.Name = "Arcane Curse"
    override self.Author = "newrite"
    override self.RequiredLibraryVersion = 10
    override self.Version = 1


    override self.Initialize loadedAny =
      self.init()
      true

    member private _.DebuffSpell : SpellItem Option =

        if curseDebuffSpell = null && curseDebuffID <> 0u && cursePlugin <> null then

            curseDebuffSpell <- Call.TESFormLookupFormFromFile(curseDebuffID, cursePlugin) :?> SpellItem

            if curseDebuffSpell <> null then
                Some curseDebuffSpell
            else
                Log <| sprintf "Fail get spell debuff"
                None

        else
            Some curseDebuffSpell

    member private _.Globals : (TESGlobal * TESGlobal * TESGlobal) Option =
    
        let mutable check = true

        let gCurseInit (tesGlobal: TESGlobal) tesGlobalID =
            
            if tesGlobal = null && gCurseDamageID <> 0u && cursePlugin <> null then

                let Global = Call.TESFormLookupFormFromFile(tesGlobalID, cursePlugin) :?> TESGlobal

                if Global = null then

                    check <- false

                Global

            else
                tesGlobal

        gCurseDamage <- gCurseInit gCurseDamage gCurseDamageID
        gCurseCost <- gCurseInit gCurseCost gCurseCostID
        gCursSummon <- gCurseInit gCursSummon gCurseSummonID

        if check then
            Some (gCurseDamage, gCurseCost, gCursSummon)
        else
            Log <| sprintf "Fail get globals"
            None

    member private self.init() =

        Events.OnMagicCasterFire.Register((fun eArg ->
            
            let actor =
              match eArg.Caster with
              | :? ActorMagicCaster as c -> Some c
              | _ -> None

            match (self.DebuffSpell, actor) with
            | (Some debuffSpell, Some caster) ->
                match eArg.Item with
                | :? SpellItem as spell ->
                  if caster <> null 
                    && caster.IsValid 
                    && caster.Owner <> null 
                    && caster.Owner.IsValid 
                    && spell.SpellData.CastingPerk <> null 
                    && caster.Owner.IsInCombat then
                    let mults = OnSpellCast.getCurseValue spell.SpellData.CastingPerk.FormId caster.Owner
                    if mults.IsSome then
                      let ctx = 
                        { OnSpellCast.Owner = caster.Owner
                          OnSpellCast.Caster = caster
                          OnSpellCast.CursedDebuff = debuffSpell
                          OnSpellCast.Spell = spell
                          OnSpellCast.Mults = mults.Value
                          OnSpellCast.IsHetot = OnSpellCast.isHetot caster.Owner }
                      OnSpellCast.handleSpell ctx
                  
                | :? EnchantmentItem as _ ->
                  if caster <> null 
                    && caster.IsValid 
                    && caster.Owner <> null 
                    && caster.Owner.IsValid 
                    && caster.Owner.IsInCombat then
                    OnSpellCast.enchantmentHandler caster.Owner
                | _ -> ()
            | _ -> ()), 100) |> ignore

        Events.OnWeaponFireProjectilePosition.Register((fun eArg ->

            let owner = 
                match eArg.Attacker with
                | :? Actor as ac -> Some ac
                | _ -> None

            if owner.IsSome && owner.Value <> null && owner.Value.IsValid && owner.Value.IsInCombat then

              if eArg.Weapon <> null && eArg.Weapon.FormEnchanting <> null &&
                  (eArg.Weapon.WeaponData.AnimationType = WeaponTypes8.Bow
                  || eArg.Weapon.WeaponData.AnimationType = WeaponTypes8.Crossbow) then

                  OnSpellCast.enchantmentHandler owner.Value), 100) |> ignore

        Events.OnFrame.Register(fun eArg ->

            if modInit then
                match self.Globals, timer.Time - lastTimer with
                | Some (damage, cost, summon), t when t >= 100L ->
                    CursedGlobals.handleGlobals damage cost summon (Call.PlayerInstance())
                    lastTimer <- timer.Time
                | _ -> ()) |> ignore


        Events.OnMainMenu.Register((fun eArg ->

            if not modInit then
                Log <| sprintf "Init in main menu - Ok"

                self.Globals
                |>function
                | Some _ -> Log <| sprintf "Success load Globals"
                | None -> Log <| sprintf "Fail load Globals in MainMenu - it ok"

                self.DebuffSpell
                |>function
                | Some _ -> Log <| sprintf "Success load DebuffSpell"
                | None -> Log <| sprintf "Fail load DebuffSpell in MainMenu - it ok"

                modInit <- true), flags = EventRegistrationFlags.Distinct) |> ignore

        ()