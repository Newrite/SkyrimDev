namespace Blood

open NetScriptFramework.Tools
open NetScriptFramework.SkyrimSE
open Wrapper
open Supporter

[<Sealed>]
type public Settings() =

  [<ConfigValue("ModName", "Имя мода", "ФормИД указанные в настрокйах ищутся в этом моде")>]
  member val ModName = "BloodMagick.esp" with get, set

  [<ConfigValue("BloodSpellsKeyword",
                "Кейворд на спелах которые должны тратить здоровье",
                "Заклинания с этим кейвордом расходуют здоровье, а не магию",
                ConfigEntryFlags.PreferHex)>]
  member val BloodSpellsKeyword = 0x01F3C6u with get, set

  [<ConfigValue("OnlyPlayer",
                "Только для игрока",
                "Только у игрока особое взаимодействие с этими заклинаниями, нпс используют их как обычно")>]
  member val OnlyPlayer = false with get, set

  [<ConfigValue("BloodAbilityEnable",
                "Наличие абилки, а не кейворда на заклинании",
                "Использует айди указанное выше что проверить на заклинателе наличие абилки по этому айди, вместо проверки кейворда на спеле")>]
  member val BloodAbilityEnable = false with get, set

  [<ConfigValue("BloodSpellsPercentCostEnable",
                "Спелы МК так же дополнительно расходуют хп в процентах",
                "Использует магнитуду эффекта с кейвордом (который указан ниже), для определения сколько дополнительно хп должен потратить спелл в процентах")>]
  member val BloodSpellsPercentCostEnable = false with get, set

  [<ConfigValue("BloodSpellsPercentCostKeyword",
                "Кейворд на спелах который указывает процентный расход",
                "Заклинания с этим кейвордом расходуют здоровье дополнительно расходуют хп в процентах",
                ConfigEntryFlags.PreferHex)>]
  member val BloodSpellsPercentCostKeyword = 0x800u with get, set

  [<ConfigValue("BloodSpellsPercentConvert",
                "Процент конвертации магии в здоровье",
                "Значение которое используется для определения того какой процент от стоимость должен тратиться в виде хп, если установлено 0 то эта настройка не задействуется.")>]
  member val BloodSpellsPercentConvert = 0.f with get, set


  member self.BloodSpellsKwd =
    match Call.TESFormLookupFormFromFile(self.BloodSpellsKeyword, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  member self.BloodSpellsPercentKwd =
    match Call.TESFormLookupFormFromFile(self.BloodSpellsPercentCostKeyword, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  member self.BloodSpellsAb =
    match Call.TESFormLookupFormFromFile(self.BloodSpellsKeyword, self.ModName) with
    | :? SpellItem as k -> Some k
    | _ -> None

  member internal self.Load() =
    ConfigFile.LoadFrom<Settings>(self, "BloodMagick", true)

[<AutoOpen>]
module Singeltones =

  let Log = Log.Init "BloodMagick"

  let config = Settings()

module Foo =

  module private Boo =

    let internal mult x y = x * y

  let private add x y = x + y

  let multAndAdd x y = Boo.mult x y + add x y
