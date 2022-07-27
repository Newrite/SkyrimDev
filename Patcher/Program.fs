open System
open Mutagen.Bethesda
open Mutagen.Bethesda.Skyrim
open Mutagen.Bethesda.Environments
open System.Linq
open System.IO

//let lo = LoadOrder.Import<ISkyrimMod>(env.DataFolderPath, a, SkyrimRelease.SkyrimSE)
//|>Seq.iter (fun mode -> mode.EditorID |> printfn "%A")

//let printAllWeapon enva =
//    for id in enva do
//        
//
//printAllWeapon env.LoadOrder.PriorityOrder.ToLoadOrder

module Path =
  [<Literal>]
  let outName = @"Elf.esp"

  let delete = File.Delete(outName)

  let outMod =
    SkyrimMod(Plugins.ModKey.FromNameAndExtension(Path.GetFileName(outName.AsSpan())), SkyrimRelease.SkyrimSE)

module Key =
  let Heavy =
    FormKeys.SkyrimSE.Skyrim.Keyword.ArmorHeavy

  let Light =
    FormKeys.SkyrimSE.Skyrim.Keyword.ArmorLight

  let Shield =
    FormKeys.SkyrimSE.Skyrim.Keyword.ArmorShield

  let Cuirass =
    FormKeys.SkyrimSE.Skyrim.Keyword.ArmorCuirass

type Address =
    |Address of int

    member self.V =
        match self with
        |Address a -> a
    

let unwrapAddress (Address a) = a

module MutArmor =

  let Heavy (armor: IArmorGetter) (keys: Collections.Generic.IReadOnlyList<Plugins.IFormLinkGetter<IKeywordGetter>>) =
    match keys with
    | keysList when
      keysList.Contains(Key.Heavy)
      && keysList.Contains(Key.Cuirass) ->
        let armorCopy =
          Path.outMod.Armors.GetOrAddAsOverride(armor :?> Armor)

        printf " Тяжелая кираса"
        armorCopy.ArmorRating <- armor.ArmorRating * (float32 2.5) + (float32 10.0)
        printf $" Мутируем значение брони {armor.ArmorRating}->{armorCopy.ArmorRating}..."
        ignore
    | keysList when
      keysList.Contains(Key.Heavy)
      && not (keysList.Contains(Key.Cuirass)) ->
        let armorCopy =
          Path.outMod.Armors.GetOrAddAsOverride(armor :?> Armor)

        printf " Тяжелый доспех"
        armorCopy.ArmorRating <- armor.ArmorRating * (float32 2.5)
        printf $" Мутируем значение брони {armor.ArmorRating}->{armorCopy.ArmorRating}..."
        ignore
    | _ -> ignore


  let Light (armor: IArmorGetter) (keys: Collections.Generic.IReadOnlyList<Plugins.IFormLinkGetter<IKeywordGetter>>) =
    match keys with
    | keysList when
      keysList.Contains(Key.Light)
      && keysList.Contains(Key.Cuirass) ->
        let armorCopy =
          Path.outMod.Armors.GetOrAddAsOverride(armor :?> Armor)

        printf " Легкая кираса"
        armorCopy.ArmorRating <- armor.ArmorRating * (float32 1.5) + (float32 10.0)
        printf $" Мутируем значение брони {armor.ArmorRating}->{armorCopy.ArmorRating}..."
        ignore
    | keysList when
      keysList.Contains(Key.Light)
      && not (keysList.Contains(Key.Cuirass)) ->
        let armorCopy =
          Path.outMod.Armors.GetOrAddAsOverride(armor :?> Armor)

        printf " Легкий доспех"
        armorCopy.ArmorRating <- armor.ArmorRating * (float32 1.5)
        printf $" Мутируем значение брони {armor.ArmorRating}->{armorCopy.ArmorRating}..."
        ignore
    | _ -> ignore


  let Shield (armor: IArmorGetter) (keys: Collections.Generic.IReadOnlyList<Plugins.IFormLinkGetter<IKeywordGetter>>) =
    match keys with
    | keysList when
      keysList.Contains(Key.Shield)
      && not (
        keysList.Contains(Key.Light)
        || keysList.Contains(Key.Heavy)
      ) ->
        match armor.BodyTemplate.ArmorType with
        | shield when shield = ArmorType.HeavyArmor ->
            let armorCopy =
              Path.outMod.Armors.GetOrAddAsOverride(armor :?> Armor)

            printf " Тяжелый щит"
            armorCopy.Keywords.Add(Key.Heavy)
            printf " добавляем KW брони"
            ignore
        | shield when shield = ArmorType.LightArmor ->
            let armorCopy =
              Path.outMod.Armors.GetOrAddAsOverride(armor :?> Armor)

            printf " Легкий щит"
            armorCopy.Keywords.Add(Key.Light)
            printf " добавляем KW брони"
            ignore
        | _ -> ignore
    | _ -> ignore

  [<RequireQualifiedAccess>]
  module Script =

    [<RequireQualifiedAccess>]
    module Entry =

      let Build entryName (propertyList: ScriptProperty list) =
        let entry = new ScriptEntry(Name = entryName)
        propertyList
        |>List.iter (fun property -> entry.Properties.Add(property))
        entry

      let Build entryName (property: ScriptProperty) =
        let entry = new ScriptEntry(Name = entryName)
        entry.Properties.Add(property)
        entry

  [<RequireQualifiedAccess>]
  module VMAdapter =

    let Build (scriptEntryList: ScriptEntry list) =
      let vm = new VirtualMachineAdapter()
      scriptEntryList
      |>List.iter (fun e -> vm.Scripts.Add(e))
      vm

  let AddScriptToNpc (npc: #INpcGetter) =
    let copy = npc.DeepCopy()
    if copy.VirtualMachineAdapter = null then
      copy.VirtualMachineAdapter <- VMAdapter.Build [
        Script.Entry.Build "XP" [
          ScriptFloatProperty(Name = "AmountXP", Data = 20.f)
          ScriptStringProperty(Name = "ScriptName", Data = "Hello")
          ScriptBoolProperty(Name = "Trues", Data = true)
        ]
      ]
    Path.outMod.Npcs.GetOrAddAsOverride(copy)

  let Template (armor: #IArmorGetter) (tempOfArmor: #IArmorGetter) =
    printf " Обновляем цену из шаблона"
    let armorCopy =
      let copy = armor.DeepCopy()
      if copy.VirtualMachineAdapter = null then
        copy.VirtualMachineAdapter <- VMAdapter.Build [
          Script.Entry.Build "XP" [
            ScriptFloatProperty(Name = "AmountXP", Data = 20.f)
            ScriptStringProperty(Name = "ScriptName", Data = "Hello")
            ScriptBoolProperty(Name = "Trues", Data = true)
          ]
        ]
      Path.outMod.Armors.GetOrAddAsOverride(copy)

    printfn $" {armorCopy.Value} -> {tempOfArmor.Value}"
    armorCopy.Value <- tempOfArmor.Value
    ignore

try
    let env = GameEnvironment.Typical.Skyrim(SkyrimRelease.SkyrimSE)
    let a = 
        (env.LinkCache.PriorityOrder |> Seq.rev).ToUntypedImmutableLinkCache()

    Path.delete
    
    a.PriorityOrder
    |>Seq.iter (fun item -> printfn "%A" <| item.ToString())

    a.PriorityOrder.WinningOverrides<IArmorGetter>()
    |>Seq.map (fun armor ->
        match armor.TemplateArmor.IsNull with
        |false ->
            match armor.TemplateArmor.TryResolve<IArmorGetter>(a) with
            |tempArmor ->
                if tempArmor.Value <> armor.Value then
                    printfn "Несовпадение цены"
                    Some (armor, tempArmor)
                else
                    None
        |_-> None)
    |>Seq.iter (fun someArmor -> 
        match someArmor with
        | Some (armor, tempArmor) ->
            printfn "Мутируем"
            MutArmor.Template armor tempArmor |> ignore
        | None -> ())
    
    Path.outMod.WriteToBinaryParallel(Path.outName)
with _ as ex ->
    printfn "%s" ex.Message

Console.ReadKey() |> ignore