open System
open System.Text
open Mutagen.Bethesda
open Mutagen.Bethesda.Oblivion
open Mutagen.Bethesda.Skyrim
open Noggog
open System.Linq
open System.IO

Console.OutputEncoding <- Encoding.UTF8
let outPath = @"Elf.esp"

let outputMod =
    new SkyrimMod(ModKey.FromNameAndExtension(Path.GetFileName(outPath.AsSpan())), SkyrimRelease.SkyrimSE)

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
        | keysList when keysList.Contains(Key.Heavy)
                        && keysList.Contains(Key.Cuirass) ->
            let copy = armor.DeepCopy()
            printf " Тяжелая кираса"
            copy.ArmorRating <- armor.ArmorRating * (float32 2.5) + (float32 10.0)
            printf $" Мутируем значение брони {armor.ArmorRating}->{copy.ArmorRating}..."
            outputMod.Armors.RecordCache.Set(copy)
            printf $" Пишем в {outPath}..."
            ignore
        | keysList when keysList.Contains(Key.Heavy)
                        && not (keysList.Contains(Key.Cuirass)) ->
            let copy = armor.DeepCopy()
            printf " Тяжелый доспех"
            copy.ArmorRating <- armor.ArmorRating * (float32 2.5)
            printf $" Мутируем значение брони {armor.ArmorRating}->{copy.ArmorRating}..."
            outputMod.Armors.RecordCache.Set(copy)
            printf $" Пишем в {outPath}..."
            ignore
        | _ -> ignore

    let Light (armor: IArmorGetter) (keys: Collections.Generic.List<IFormLink<IKeywordGetter>>) =
        match keys with
        | keysList when keysList.Contains(Key.Light)
                        && keysList.Contains(Key.Cuirass) ->
            let copy = armor.DeepCopy()
            printf " Легкая кираса"
            copy.ArmorRating <- armor.ArmorRating * (float32 1.5) + (float32 10.0)
            printf $" Мутируем значение брони {armor.ArmorRating}->{copy.ArmorRating}..."
            outputMod.Armors.RecordCache.Set(copy)
            printf $" Пишем в {outPath}..."
            ignore
        | keysList when keysList.Contains(Key.Light)
                        && not (keysList.Contains(Key.Cuirass)) ->
            let copy = armor.DeepCopy()
            printf " Легкий доспех"
            copy.ArmorRating <- armor.ArmorRating * (float32 1.5)
            printf $" Мутируем значение брони {armor.ArmorRating}->{copy.ArmorRating}..."
            outputMod.Armors.RecordCache.Set(copy)
            printf $" Пишем в {outPath}..."
            ignore
        | _ -> ignore

    let Shield (armor: IArmorGetter) (keys: Collections.Generic.List<IFormLink<IKeywordGetter>>) =
        match keys with
        | keysList when keysList.Contains(Key.Shield)
                        && not
                            (keysList.Contains(Key.Light)
                             || keysList.Contains(Key.Heavy)) ->
            match armor.BodyTemplate.ArmorType with
            | x when x = ArmorType.HeavyArmor ->
                let copy = armor.DeepCopy()
                printf " Тяжелый щит"
                copy.Keywords.Add(Key.Heavy)
                printf " добавляем KW брони"
                outputMod.Armors.RecordCache.Set(copy)
                printf $" Пишем в {outPath}..."
                ignore
            | x when x = ArmorType.LightArmor ->
                let copy = armor.DeepCopy()
                printf " Легкий щит"
                copy.Keywords.Add(Key.Light)
                printf " добавляем KW брони"
                outputMod.Armors.RecordCache.Set(copy)
                printf $" Пишем в {outPath}..."
                ignore
            | x when x = ArmorType.Clothing -> ignore
            | _ -> ignore
        | _ -> ignore

    let Template (armor: IArmorGetter) (tempOfArmor: IArmorGetter) =
        printf " Обновляем цену из шаблона"
        let copy = armor.DeepCopy()
        printf $" {copy.Value} -> {tempOfArmor.Value}"
        copy.Value <- tempOfArmor.Value
        printf $" Пишем в {outPath}..."
        outputMod.Armors.RecordCache.Set(copy)
        ignore


[<EntryPoint>]
let main args =
    let modGetter =
        let path = Directory.GetCurrentDirectory()

        let loader =
            LoadOrder.GetListings(GameRelease.SkyrimSE, DirectoryPath(path))

        let loadOrder =
            LoadOrder.Import<SkyrimMod>
                (dataFolder = DirectoryPath(path), loadOrder = loader, gameRelease = GameRelease.SkyrimSE)

        loadOrder.PriorityOrder.Cast<IModListing<IModGetter>>()

    let cache = modGetter.ToUntypedImmutableLinkCache()

    for armor in modGetter.WinningOverrides<IArmorGetter>() do

        match armor.Keywords with
        | null -> ()
        | keys ->
            match armor.TemplateArmor.IsNull with
            | false ->
                match cache.Resolve(armor.TemplateArmor.FormKey, armor.GetType()) with
                | :? IArmorGetter as arm ->
                    printf $"Берем {armor.Name}... имеет шаблон!"
                    MutArmor.Template arm armor |> ignore
                    printfn " готово."
                | _ -> ()
            | true ->
                printf $"Берем {armor.Name}..."
                MutArmor.Heavy armor (keys.ToList()) |> ignore
                MutArmor.Light armor (keys.ToList()) |> ignore
                MutArmor.Shield armor (keys.ToList()) |> ignore
                printfn " готово."

    outputMod.WriteToBinaryParallel(outPath)
    printfn " Патч успешно создан."
    Console.ReadKey() |> ignore

    0