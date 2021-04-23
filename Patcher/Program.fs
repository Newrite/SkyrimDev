open System
open System.Text
open Mutagen.Bethesda
open Mutagen.Bethesda.Skyrim
open Noggog
open System.Linq
open System.IO

Console.OutputEncoding <- Encoding.Default

module Path =
  [<Literal>]
  let outName = @"Elf.esp"

  let delete = File.Delete(outName)

  let outMod =
    SkyrimMod(ModKey.FromNameAndExtension(Path.GetFileName(outName.AsSpan())), SkyrimRelease.SkyrimSE)

module Key =
  let Heavy =
    FormKeys.SkyrimSE.Skyrim.Keyword.ArmorHeavy

  let Light =
    FormKeys.SkyrimSE.Skyrim.Keyword.ArmorLight

  let Shield =
    FormKeys.SkyrimSE.Skyrim.Keyword.ArmorShield

  let Cuirass =
    FormKeys.SkyrimSE.Skyrim.Keyword.ArmorCuirass


module MutArmor =

  let Heavy (armor: IArmorGetter) (keys: Collections.Generic.List<IFormLink<IKeywordGetter>>) =
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


  let Light (armor: IArmorGetter) (keys: Collections.Generic.List<IFormLink<IKeywordGetter>>) =
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


  let Shield (armor: IArmorGetter) (keys: Collections.Generic.List<IFormLink<IKeywordGetter>>) =
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

  let Template (armor: IArmorGetter) (tempOfArmor: IArmorGetter) =
    printf " Обновляем цену из шаблона"

    let armorCopy =
      Path.outMod.Armors.GetOrAddAsOverride(armor :?> Armor)

    printf $" {armorCopy.Value} -> {tempOfArmor.Value}"
    armorCopy.Value <- tempOfArmor.Value
    ignore


module ModGet =

  let modGetter =
    let path = Directory.GetCurrentDirectory()

    let loader =
      LoadOrder.GetListings(GameRelease.SkyrimSE, DirectoryPath(path))

    let loadOrder =
      LoadOrder.Import<SkyrimMod>(
        dataFolder = DirectoryPath(path),
        loadOrder = loader,
        gameRelease = GameRelease.SkyrimSE
      )
      |> (fun (x: LoadOrder<IModListing<SkyrimMod>>) ->
        let order = new LoadOrder<IModListing<SkyrimMod>>()

        for _mod in x.ListedOrder do
          if _mod.Enabled then order.Add(_mod)

        order)

    loadOrder.PriorityOrder.Cast<IModListing<IModGetter>>()

  let cache = modGetter.ToUntypedImmutableLinkCache()

module Patcher =
  let startMutateArmor =

    let modsGetter = ModGet.modGetter

    let mutateTemplateArmor =
      for armor in modsGetter.WinningOverrides<IArmorGetter>() do

        match armor.TemplateArmor.IsNull with
        | false ->
            match armor.TemplateArmor.ResolveAll(ModGet.cache) with
            | enumTempOfArmor ->
                let tempOfArmor = enumTempOfArmor.Last()

                if tempOfArmor.Value <> armor.Value then
                  printf $"Берем {armor.Name}... имеет шаблон!"
                  MutArmor.Template armor tempOfArmor |> ignore
                  printfn " готово."
        | _ -> ()

    let mutateArmor () =
      for armor in modsGetter.WinningOverrides<IArmorGetter>() do

        match armor.Keywords with
        | null -> ()
        | keys ->
            printf $"Берем {armor.Name}..."
            MutArmor.Heavy armor (keys.ToList()) |> ignore
            MutArmor.Light armor (keys.ToList()) |> ignore
            MutArmor.Shield armor (keys.ToList()) |> ignore
            printfn " готово."

      ()

    mutateTemplateArmor
    mutateArmor

[<EntryPoint>]
let main args =

  Path.delete
  Patcher.startMutateArmor
  Path.outMod.WriteToBinaryParallel(Path.outName)
  printfn " Патч успешно создан."
  Console.ReadKey() |> ignore

  0
