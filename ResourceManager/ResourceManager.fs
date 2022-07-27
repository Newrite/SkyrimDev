namespace ResourceManager

open NetScriptFramework
open NetScriptFramework.SkyrimSE
open NetScriptFramework.Tools
open Wrapper
open Supporter

[<AutoOpen>]
module private InternalVariables =
    let Log = Log.Init "Resource Manager"
    let Settings = new ResourceManager.Settings()
    let Timer = new Timer()

module private Manager =

    // Public Types ///
    type ActorValues =
        { AttackCost: ActorValueIndices;
          PowerAttackCost: ActorValueIndices; }

    [<RequireQualifiedAccess>]
    type DrainValue =
        |Stamina
        |Health
        |Magicka
        |HealthStamina
        |MagickaStamina
        |HealthMagicka
        |HealthMagickaStamina

    [<RequireQualifiedAccess>]
    type AttackState =
        |Draw of DrainValue * float32
        |Reload of DrainValue * float32
        |Swing
        |None

    [<RequireQualifiedAccess>]
    type WeaponType =
        |Ranged
        |Melee
    
    [<NoComparison>]
    type Attacker =
        { Address: nativeint
          LastAttackTime: int64
          CurrentAttack: AttackState
          IsFirstAttack: bool
          NextStaminaDrain: float32
          NextHealthDrain: float32
          NextMagickaDrain:float32
          TotalStaminaDrain: float32
          TotalHealthDrain:float32
          TotalMagickaDrain: float32
          Owner: Actor }

    [<RequireQualifiedAccess>]
    [<NoComparison>]
    type AnimationAction =
        | Attack of TESObjectWEAP * DrainValue
        | PowerAttack of TESObjectWEAP * DrainValue
        | Jump
        | BowDraw of TESObjectWEAP * DrainValue
        | ArrowRelease
        | BowReset
        | BoltRelease
        | ReloadStart of TESObjectWEAP * DrainValue
        | ReloadStop
        | WeapEquipOut

    [<NoComparison>]
    type Context =
        { Values: ActorValues
          Animation: AnimationAction;
          SpellForArrowCost: SpellItem;
          AttackerState: Attacker;
          HotReloadPerk: BGSPerk; }

    // Public Variables ///
    let KeywordHealthDrain() = Call.TESFormLookupFormFromFile(0x2C7E5u, "Skyrim.esm") :?> BGSKeyword
    let KeywordMagickaDrain() = Call.TESFormLookupFormFromFile(0x2CDC0u, "Skyrim.esm") :?> BGSKeyword
    let KeywordStaminaDrain() = Call.TESFormLookupFormFromFile(0x2C7DFu, "Skyrim.esm") :?> BGSKeyword
    let AttackersCache = new System.Collections.Concurrent.ConcurrentDictionary<nativeint, Attacker>()
            
    // Private Functions ///
    let private ammoWeight (actor: Actor) =
        let kNone = Settings.KeywordArrowWeightNone
        let kLight = Settings.KeywordArrowWeightLight
        let kMedium = Settings.KeywordArrowWeightMedium
        let kHeavy = Settings.KeywordArrowWeightHeavy
        let kMassive = Settings.KeywordArrowWeightMassive
        if kNone <> null && kLight <> null && kMedium <> null && kHeavy <> null && kMassive <> null then
            if actor.WornHasKeyword(kMassive) then 6.f
            elif actor.WornHasKeyword(kHeavy) then 4.5f
            elif actor.WornHasKeyword(kMedium) then 3.f
            elif actor.WornHasKeyword(kLight) then 1.5f
            else 0.f
        else
            0.f

    let private evalWeaponWeight (weap: TESObjectWEAP) =
        if weap.WeightValue = 0.f then
            float32(weap.AttackDamage) * 0.2f * Settings.WeightMult().FloatValue
        else 
            weap.WeightValue * Settings.WeightMult().FloatValue

    let private evalDrainMelee ctx isPowerAttack weap =

         let infamy =
            match ctx.AttackerState.Owner.GetActorValue(ActorValueIndices.Infamy) with
            | value when value > 1.f -> value
            | _ -> 1.f

         let weight = evalWeaponWeight weap

         let attackMult =
            match ctx.AttackerState.Owner.GetActorValue(ctx.Values.AttackCost) with
            | value when value > 90.f -> 1.f - (90.f / 100.f)
            | value when value <= 0.f -> 1.f
            | value -> 1.f - (value / 100.f)

         let powerAttackMult =
             match ctx.AttackerState.Owner.GetActorValue(ctx.Values.PowerAttackCost) with
             | _ when isPowerAttack = false -> 0.f
             | value when value > 90.f -> 1.f - (90.f / 100.f)
             | value when value <= 0.f -> 1.f
             | value -> 1.f - (value / 100.f)

         let calcFlat flatValue =
             flatValue + (1.f - (weight * 3.f / 100.f))

         let evalDrainValue flatValue =
            (weight * attackMult * infamy) + (weight * infamy * powerAttackMult * Settings.PowerModifier().FloatValue) + calcFlat flatValue

         evalDrainValue 5.f * Settings.Mult().FloatValue

    let private evalDrainRanged ctx weap =

         let infamy =
            match ctx.AttackerState.Owner.GetActorValue(ActorValueIndices.Infamy) with
            | value when value > 1.f -> value
            | _ -> 1.f

         let weight = evalWeaponWeight weap

         let perkMult =
            if ctx.AttackerState.Owner.HasPerk(ctx.HotReloadPerk) && weap.WeaponData.AnimationType = WeaponTypes8.Crossbow then
                0.7f
            else
                1.f

         let attackMult =
            match ctx.AttackerState.Owner.GetActorValue(ctx.Values.AttackCost) with
            | value when value > 90.f -> 1.f - (90.f / 100.f)
            | value when value <= 0.f -> 1.f
            | value -> 1.f - (value / 100.f)

         ((weight + ammoWeight ctx.AttackerState.Owner) * attackMult * infamy * perkMult * Settings.Mult().FloatValue)


    // Type Extension ///
    type DrainValue with
        
        static member EvalActorValue (actor: Actor) (weap: TESObjectWEAP) =
            
            let staminaToHealth =
                actor.WornHasKeyword(Settings.StaminaToHealth) || actor.HasKeyword(Settings.StaminaToHealth)
            let staminaToMagick =
                actor.WornHasKeyword(Settings.StaminaToMagicka) || actor.HasKeyword(Settings.StaminaToMagicka)
            let healthToStamina =
                actor.WornHasKeyword(Settings.HealthToStamina) || actor.HasKeyword(Settings.HealthToStamina)
            let magickaToStamina =
                actor.WornHasKeyword(Settings.MagickaToStamina) || actor.HasKeyword(Settings.MagickaToStamina)
            let healthToMagicka =
                actor.WornHasKeyword(Settings.HealthToMagicka) || actor.HasKeyword(Settings.HealthToMagicka)
            let magickaToHealth =
                actor.WornHasKeyword(Settings.MagickaToHealth) || actor.HasKeyword(Settings.MagickaToHealth)

            let weapMask : int[,] = Array2D.zeroCreate 1 3
            let actorMask : int[,] =
                Array2D.init 3 3
                    ^fun i1 i2 ->
                        if i1 = i2 then 1 else 0

            Log <| sprintf "Start masks weap: %A actor: %A" weapMask actorMask

            weapMask.[0,0] <- 1
            if weap.HasKeyword(KeywordHealthDrain()) then
                weapMask.[0,0] <- 0
                weapMask.[0,1] <- 1
            if weap.HasKeyword(KeywordMagickaDrain()) then
                weapMask.[0,0] <- 0
                weapMask.[0,2] <- 1
            if weap.HasKeyword(KeywordStaminaDrain()) then
                weapMask.[0,0] <- 1

            if staminaToHealth then
               actorMask.[0,*] <- [|0;1;0|]
            if staminaToMagick then
               actorMask.[0,*] <- [|0;0;1|]
            if healthToMagicka then
               actorMask.[1,*] <- [|0;0;1|]
            if healthToStamina then
               actorMask.[1,*] <- [|1;0;0|]
            if magickaToHealth then
               actorMask.[2,*] <- [|0;1;0|]
            if magickaToStamina then
               actorMask.[2,*] <- [|1;0;0|]

          (*    WeapMask
               ST HP MP
              [1  0  0 ]   *)

          (*    ActorMask
                ST HP MP
             ST[1  0  0 ]
             HP[0  1  0 ] 
             MP[0  0  1 ]  *)
            

            let matrixMultiply (matrix1 : _[,]) (matrix2 : _[,]) =
                let result_row = (matrix1.GetLength 0)
                let result_column = (matrix2.GetLength 1)
                let ret = Array2D.create result_row result_column 0
                for x in 0 .. result_row - 1 do
                    for y in 0 .. result_column - 1 do
                        let mutable acc = 0
                        for z in 0 .. (matrix1.GetLength 1) - 1 do
                            acc <- acc + matrix1.[x,z] * matrix2.[z,y]
                        ret.[x,y] <- acc
                ret

            let mask = matrixMultiply weapMask actorMask

            let mutable maskSum = 0
            
            if mask.[0,0] > 0 then maskSum <- maskSum + 1
            if mask.[0,1] > 0 then maskSum <- maskSum + 2
            if mask.[0,2] > 0 then maskSum <- maskSum + 4

            

            Log <| sprintf "After masks weap: %A actor: %A mask: %A" weapMask actorMask mask
            Log <| sprintf "Mask Sum %A" maskSum

            match maskSum with
            |1 -> weap, Stamina
            |2 -> weap, Health
            |4 -> weap, Magicka 
            |3 -> weap, HealthStamina
            |5 -> weap, MagickaStamina
            |6 -> weap, HealthMagicka
            |7 -> weap, HealthMagickaStamina
            |_ ->
                Log <| sprintf "Incorrect sum of mask"
                weap, Stamina

    // Public Functions ///
    let UpdateCache =
        let updateCache address newAttackerState =
            AttackersCache.AddOrUpdate(
                address,
                newAttackerState,
                (fun _ _ -> newAttackerState))
        updateCache

    let DrainsHandler attk drainAmount actorValueDrain attackType =

        if attk.Owner <> null && attk.Owner.IsValid then
            
            let stamina = attk.Owner.GetActorValue(ActorValueIndices.Stamina)
            let health = attk.Owner.GetActorValue(ActorValueIndices.Health)
            let magicka = attk.Owner.GetActorValue(ActorValueIndices.Magicka)

            let staminaDrain =
                let sum = attk.NextStaminaDrain + drainAmount
                if stamina > sum then sum else stamina
            let healthDrain =
                let sum = attk.NextHealthDrain + drainAmount
                if health > sum then sum else health
            let magickaDrain =
                let sum = attk.NextMagickaDrain + drainAmount
                if magicka > sum then sum else magicka

            let addDrain =
                if Settings.SoulsON() then
                    if attk.IsFirstAttack then 0.f else drainAmount
                else
                    0.f

            let addDrainStamina =
                if Settings.StaminaSoulsON then addDrain else 0.f

            let addDrainHealth =
                if Settings.HealthSoulsON then addDrain else 0.f

            let addDrainMagicka =
                if Settings.MagickaSoulsON then addDrain else 0.f

            let matchDrain addValueStamina addValueHealth addValueMagicka =
                match actorValueDrain with
                |DrainValue.Stamina ->
                    attk.Owner.DamageActorValue(ActorValueIndices.Stamina, -staminaDrain)
                    UpdateCache attk.Address
                        { attk with
                            Attacker.IsFirstAttack = false
                            Attacker.TotalStaminaDrain =
                              attk.TotalStaminaDrain + staminaDrain;
                            Attacker.NextStaminaDrain =
                              attk.NextStaminaDrain + addValueStamina;
                            Attacker.LastAttackTime = Timer.Time } |> ignore

                |DrainValue.Health ->
                    attk.Owner.DamageActorValue(ActorValueIndices.Health, -healthDrain)
                    UpdateCache attk.Address
                        { attk with
                            Attacker.IsFirstAttack = false
                            Attacker.TotalHealthDrain =
                              attk.TotalHealthDrain + healthDrain;
                            Attacker.NextHealthDrain =
                              attk.NextHealthDrain + addValueHealth;
                            Attacker.LastAttackTime = Timer.Time } |> ignore

                |DrainValue.Magicka ->
                    attk.Owner.DamageActorValue(ActorValueIndices.Magicka, -magickaDrain)
                    UpdateCache attk.Address
                        { attk with
                            Attacker.IsFirstAttack = false
                            Attacker.TotalMagickaDrain =
                              attk.TotalMagickaDrain + magickaDrain;
                            Attacker.NextMagickaDrain =
                              attk.NextMagickaDrain + addValueMagicka;
                            Attacker.LastAttackTime = Timer.Time } |> ignore

                |DrainValue.HealthStamina ->
                    attk.Owner.DamageActorValue(ActorValueIndices.Stamina, -(staminaDrain * 0.5f))
                    attk.Owner.DamageActorValue(ActorValueIndices.Health, -(healthDrain * 0.5f))
                    UpdateCache attk.Address
                        { attk with
                            Attacker.IsFirstAttack = false
                            Attacker.TotalStaminaDrain =
                              attk.TotalStaminaDrain + (staminaDrain * 0.5f);
                            Attacker.NextStaminaDrain =
                              attk.NextStaminaDrain + (addValueStamina * 0.5f);
                            Attacker.TotalHealthDrain =
                              attk.TotalHealthDrain + (healthDrain * 0.5f);
                            Attacker.NextHealthDrain =
                              attk.NextHealthDrain + (addValueHealth * 0.5f);
                            Attacker.LastAttackTime = Timer.Time } |> ignore

                |DrainValue.MagickaStamina ->
                    attk.Owner.DamageActorValue(ActorValueIndices.Stamina, -(staminaDrain * 0.5f))
                    attk.Owner.DamageActorValue(ActorValueIndices.Magicka, -(magickaDrain * 0.5f))
                    UpdateCache attk.Address
                        { attk with
                            Attacker.IsFirstAttack = false
                            Attacker.TotalStaminaDrain =
                              attk.TotalStaminaDrain + (staminaDrain * 0.5f);
                            Attacker.NextStaminaDrain =
                              attk.NextStaminaDrain + (addValueStamina * 0.5f);
                            Attacker.TotalMagickaDrain =
                              attk.TotalMagickaDrain + (magickaDrain * 0.5f);
                            Attacker.NextMagickaDrain =
                              attk.NextMagickaDrain + (addValueMagicka * 0.5f);
                            Attacker.LastAttackTime = Timer.Time } |> ignore

                |DrainValue.HealthMagicka ->
                    attk.Owner.DamageActorValue(ActorValueIndices.Magicka, -(magickaDrain * 0.5f))
                    attk.Owner.DamageActorValue(ActorValueIndices.Health, -(healthDrain * 0.5f))
                    UpdateCache attk.Address
                        { attk with
                            Attacker.IsFirstAttack = false
                            Attacker.TotalHealthDrain =
                              attk.TotalHealthDrain + (healthDrain * 0.5f);
                            Attacker.NextHealthDrain =
                              attk.NextHealthDrain + (addValueHealth * 0.5f);
                            Attacker.TotalMagickaDrain =
                              attk.TotalMagickaDrain + (magickaDrain * 0.5f);
                            Attacker.NextMagickaDrain =
                              attk.NextMagickaDrain + (addValueMagicka * 0.5f);
                            Attacker.LastAttackTime = Timer.Time } |> ignore

                |DrainValue.HealthMagickaStamina ->
                    attk.Owner.DamageActorValue(ActorValueIndices.Magicka, -(magickaDrain * 0.35f))
                    attk.Owner.DamageActorValue(ActorValueIndices.Health, -(healthDrain * 0.35f))
                    attk.Owner.DamageActorValue(ActorValueIndices.Stamina, -(staminaDrain * 0.35f))
                    UpdateCache attk.Address
                        { attk with
                            Attacker.IsFirstAttack = false
                            Attacker.TotalHealthDrain =
                              attk.TotalHealthDrain + (healthDrain * 0.35f);
                            Attacker.NextHealthDrain =
                              attk.NextHealthDrain + (addValueHealth * 0.35f);
                            Attacker.TotalMagickaDrain =
                              attk.TotalMagickaDrain + (magickaDrain * 0.35f);
                            Attacker.NextMagickaDrain =
                              attk.NextMagickaDrain + (addValueMagicka * 0.35f);
                            Attacker.TotalStaminaDrain =
                              attk.TotalStaminaDrain + (staminaDrain * 0.35f);
                            Attacker.NextStaminaDrain =
                              attk.NextStaminaDrain + (addValueStamina * 0.35f);
                            Attacker.LastAttackTime = Timer.Time } |> ignore

            match attackType with
            |WeaponType.Melee -> matchDrain addDrainStamina addDrainHealth addDrainMagicka
            |WeaponType.Ranged -> matchDrain 0.f 0.f 0.f
        else Log <| sprintf "Invalid attacker owner, skip drain"

    let ResourceManagerHandler ctx =

        let updateNoState() =
            let attacker =
                { ctx.AttackerState with CurrentAttack = AttackState.None }
            UpdateCache ctx.AttackerState.Address attacker |> ignore

        match ctx.Animation with
        | AnimationAction.Attack (weapon, drainActorValue) ->
            let drainAmount = evalDrainMelee ctx false weapon
            let attacker =
                { ctx.AttackerState with CurrentAttack = AttackState.Swing }
            DrainsHandler attacker drainAmount drainActorValue WeaponType.Melee

        | AnimationAction.PowerAttack (weapon, drainActorValue) ->
            let drainAmount = evalDrainMelee ctx true weapon
            let attacker =
                { ctx.AttackerState with CurrentAttack = AttackState.Swing }
            DrainsHandler attacker drainAmount drainActorValue WeaponType.Melee

        | AnimationAction.Jump ->
            let infamy =
                match ctx.AttackerState.Owner.GetActorValue(ActorValueIndices.Infamy) with
                | value when value > 1.f -> value
                | _ -> 1.f
            let drainAmount = Settings.JumpValue().FloatValue * Settings.Mult().FloatValue * infamy
            ctx.AttackerState.Owner.DamageActorValue(ActorValueIndices.Stamina, -drainAmount)

        | AnimationAction.BowDraw (weapon, drainActorValue) ->
            let drainAmount = (evalDrainRanged ctx weapon) * 0.1f
            let attacker =
                { ctx.AttackerState with
                   CurrentAttack = AttackState.Draw (drainActorValue, drainAmount);
                   NextStaminaDrain = drainAmount;
                   NextHealthDrain = drainAmount;
                   NextMagickaDrain = drainAmount }
            UpdateCache ctx.AttackerState.Address attacker |> ignore

        | AnimationAction.ArrowRelease ->
            ctx.AttackerState.Owner.CastSpell(ctx.SpellForArrowCost, ctx.AttackerState.Owner, ctx.AttackerState.Owner)
            |>function
            |true -> ()
            |false -> Log <| sprintf "Failed cast spell when arrow release"
            updateNoState()

        | AnimationAction.BowReset ->
            updateNoState()

        | AnimationAction.BoltRelease ->
            ctx.AttackerState.Owner.CastSpell(ctx.SpellForArrowCost, ctx.AttackerState.Owner, ctx.AttackerState.Owner)
            |>function
            |true -> ()
            |false -> Log <| sprintf "Failed cast spell when bolt release"
            updateNoState()

        | AnimationAction.ReloadStart (weapon, drainActorValue) ->
            let drainAmount = evalDrainRanged ctx weapon * 0.1f
            let attacker =
                { ctx.AttackerState with
                   CurrentAttack = AttackState.Draw (drainActorValue, drainAmount);
                   NextStaminaDrain = drainAmount;
                   NextHealthDrain = drainAmount;
                   NextMagickaDrain = drainAmount }
            UpdateCache ctx.AttackerState.Address attacker |> ignore

        | AnimationAction.ReloadStop ->
            updateNoState()

        | AnimationAction.WeapEquipOut ->
            match ctx.AttackerState.CurrentAttack with
            |AttackState.Draw _ when AttackersCache.ContainsKey(ctx.AttackerState.Address) ->
                updateNoState()
            |AttackState.Reload _ when AttackersCache.ContainsKey(ctx.AttackerState.Address) ->
                updateNoState()
            |_ -> ()

type public ResourceManagerPlugin() =

    inherit Plugin()

    let mutable init = false

    override self.Key = "resourcemanager"
    override self.Name = "Resource Manager"
    override self.Author = "newrite"
    override self.RequiredLibraryVersion = 10
    override self.Version = 1

    override self.Initialize loadedAny =
        self.init()
        true

    member private self.init() =

        Settings.Load()
        |>function
        |true -> ()
        |false -> Log <| sprintf "Can't load settings"

        ExtEvent.OnAnimationEvent.AddHandler(new ExtEvent.AnimDelegate(fun _ args ->

            if init then

                let contextBuilder animation =

                    let attacker =
                        match Manager.AttackersCache.TryGetValue(args.Source.Address) with
                        |true, attk -> attk
                        |false, _ ->
                         { Manager.Attacker.Address = args.Source.Address;
                            Manager.Attacker.CurrentAttack = Manager.AttackState.None;
                            Manager.Attacker.IsFirstAttack = true;
                            Manager.Attacker.LastAttackTime = Timer.Time;
                            Manager.Attacker.NextHealthDrain = 0.f;
                            Manager.Attacker.NextMagickaDrain = 0.f;
                            Manager.Attacker.NextStaminaDrain = 0.f;
                            Manager.Attacker.Owner = args.Source;    
                            Manager.Attacker.TotalHealthDrain = 0.f;
                            Manager.Attacker.TotalMagickaDrain = 0.f;
                            Manager.Attacker.TotalStaminaDrain = 0.f; }

                    { Manager.Values =
                        { Manager.ActorValues.AttackCost = ActorValueIndices.AlchemySkillAdvance;
                          Manager.ActorValues.PowerAttackCost = ActorValueIndices.MarksmanSkillAdvance }
                      Manager.Animation = animation;
                      Manager.SpellForArrowCost = Settings.ArrowBoltReleaseSpell;
                      Manager.AttackerState = attacker;
                      Manager.HotReloadPerk = Settings.CrossbowPerk }

                let handler = contextBuilder>>Manager.ResourceManagerHandler
            

                match args.Anim with
                | OnAnimation.WeaponSwingLeft ->
                    args.Source.GetEquippedWeapon(WeaponSlot.Left)
                    |> Option.defaultValue Settings.UnarmedWeapon
                    |> Manager.DrainValue.EvalActorValue args.Source
                    |> (Manager.AnimationAction.Attack>>handler)

                | OnAnimation.WeaponSwingLeftPower ->
                    args.Source.GetEquippedWeapon(WeaponSlot.Left)
                    |> Option.defaultValue Settings.UnarmedWeapon
                    |> Manager.DrainValue.EvalActorValue args.Source
                    |> (Manager.AnimationAction.PowerAttack>>handler)

                | OnAnimation.WeaponSwingRight ->
                    args.Source.GetEquippedWeapon(WeaponSlot.Right)
                    |> Option.defaultValue Settings.UnarmedWeapon
                    |> Manager.DrainValue.EvalActorValue args.Source
                    |> (Manager.AnimationAction.Attack>>handler)

                | OnAnimation.WeaponSwingRightPower ->
                    args.Source.GetEquippedWeapon(WeaponSlot.Right)
                    |> Option.defaultValue Settings.UnarmedWeapon
                    |> Manager.DrainValue.EvalActorValue args.Source
                    |> (Manager.AnimationAction.PowerAttack>>handler)

                | OnAnimation.Jump -> Manager.AnimationAction.Jump |> handler
                | OnAnimation.BowDraw ->
                    args.Source.GetEquippedWeapon(WeaponSlot.Right)
                    |> Option.iter ^fun weap ->
                        Manager.DrainValue.EvalActorValue args.Source weap
                        |>(Manager.AnimationAction.BowDraw>>handler)

                | OnAnimation.ArrowRelease -> Manager.AnimationAction.ArrowRelease |> handler
                | OnAnimation.BowReset -> Manager.AnimationAction.BowReset |> handler
                | OnAnimation.BoltRelease -> Manager.AnimationAction.BoltRelease |> handler
                | OnAnimation.ReloadStart ->
                    args.Source.GetEquippedWeapon(WeaponSlot.Right)
                    |> Option.iter ^fun weap ->
                        Manager.DrainValue.EvalActorValue args.Source weap
                        |>(Manager.AnimationAction.ReloadStart>>handler)

                | OnAnimation.ReloadStop -> Manager.AnimationAction.ReloadStop |> handler
                | OnAnimation.WeapEquipOut -> Manager.AnimationAction.WeapEquipOut |> handler ))

        Events.OnMainMenu.Register((fun _ ->

            if not init then
                Log <| sprintf "Init in main menu - Ok"
                Timer.Start()
                if not Timer.IsRunning then
                    Log <| sprintf "Fail to start timer"
                else 
                    Log <| sprintf "Timer started"
                    init <- true), flags = EventRegistrationFlags.Distinct) |> ignore

        Events.OnFrame.Register(fun eArg ->

            if init then
                 for KeyValue(addr, attk) in Manager.AttackersCache do
                    match attk.CurrentAttack with
                    |Manager.AttackState.Draw (drainActorValue, drainAmount) 
                        when (Timer.Time - attk.LastAttackTime) >= 100L ->
                            Manager.DrainsHandler attk drainAmount drainActorValue Manager.WeaponType.Ranged

                    |Manager.AttackState.Reload (drainActorValue, drainAmount) 
                        when (Timer.Time - attk.LastAttackTime) >= 100L ->
                            Manager.DrainsHandler attk drainAmount drainActorValue Manager.WeaponType.Ranged

                    |Manager.AttackState.Swing when Settings.SoulsON() &&
                        (Timer.Time - attk.LastAttackTime) >= Settings.RecoveryTime() ->
                        let recoveryMult = Settings.MultRecoveryValue().FloatValue

                        if Settings.StaminaSoulsON && attk.Owner <> null && attk.Owner.IsValid then
                            attk.Owner.RestoreActorValue(ActorValueIndices.Stamina, attk.TotalStaminaDrain * recoveryMult)
                        if Settings.HealthSoulsON && attk.Owner <> null && attk.Owner.IsValid then
                            attk.Owner.RestoreActorValue(ActorValueIndices.Health, attk.TotalHealthDrain * recoveryMult)
                        if Settings.MagickaSoulsON && attk.Owner <> null && attk.Owner.IsValid then
                            attk.Owner.RestoreActorValue(ActorValueIndices.Magicka, attk.TotalMagickaDrain * recoveryMult)

                        match Manager.AttackersCache.TryRemove(addr) with
                        |false, _ -> Log <| sprintf "Failed remove %d from cache" addr
                        |_ -> ()
                        
                    |_ when (Timer.Time - attk.LastAttackTime) >= 6000L ->
                        match Manager.AttackersCache.TryRemove(addr) with
                        |false, _ -> Log <| sprintf "Failed remove %d from cache" addr
                        |_ -> ()
                    |_ -> ()) |> ignore

        ()