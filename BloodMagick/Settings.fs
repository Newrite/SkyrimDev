namespace Blood

open NetScriptFramework.Tools
open NetScriptFramework.SkyrimSE
open Wrapper
open Supporter

[<Sealed>]
type public Settings() =

  // BloodSpells plugin
  let mutable bloodSpellsKeyword = 0x01F3C6u
  let mutable bloodSpellsPercentCostKeyword = 0x800u
  let mutable bloodSpellsOnlyPlayer = false
  let mutable bloodAbilityEnable = false
  let mutable bloodSpellsPercentCost = false

  // Main
  let mutable modName = "BloodMagick.esp"

  [<ConfigValue("ModName",
  "Имя мода",
  "ФормИД указанные в настрокйах ищутся в этом моде")>]
  member private self.ModName
    with get() = modName
    and set name = modName <- name

  [<ConfigValue("BloodSpellsKeyword",
  "Кейворд на спелах которые должны тратить здоровье",
  "Заклинания с этим кейвордом расходуют здоровье, а не магию", ConfigEntryFlags.PreferHex)>]
  member private self.BloodSpellsKeyword
    with get() = bloodSpellsKeyword
    and set id = bloodSpellsKeyword <- id

  [<ConfigValue("OnlyPlayer", 
  "Только для игрока",
  "Только у игрока особое взаимодействие с этими заклинаниями, нпс используют их как обычно")>]
  member self.OnlyPlayer
    with get() = bloodSpellsOnlyPlayer
    and set check = bloodSpellsOnlyPlayer <- check
    
  [<ConfigValue("BloodAbilityEnable", 
  "Наличие абилки, а не кейворда на заклинании",
  "Использует айди указанное выше что проверить на заклинателе наличие абилки по этому айди, вместо проверки кейворда на спеле")>]
  member self.BloodAbilityEnable
    with get() = bloodAbilityEnable
    and set check = bloodAbilityEnable <- check
    
  [<ConfigValue("BloodSpellsPercentCostEnable", 
  "Спелы МК так же дополнительно расходуют хп в процентах",
  "Использует магнитуду эффекта с кейвордом (который указан ниже), для определения сколько дополнительно хп должен потратить спелл в процентах")>]
  member self.BloodSpellsPercentCostEnable
    with get() = bloodSpellsPercentCost
    and set check = bloodSpellsPercentCost <- check
    
  [<ConfigValue("BloodSpellsPercentCostKeyword",
  "Кейворд на спелах который указывает процентный расход",
  "Заклинания с этим кейвордом расходуют здоровье дополнительно расходуют хп в процентах", ConfigEntryFlags.PreferHex)>]
  member private self.BloodSpellsPercentCostKeyword
    with get() = bloodSpellsPercentCostKeyword
    and set id = bloodSpellsPercentCostKeyword <- id


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

  member internal self.Load() = ConfigFile.LoadFrom<Settings>(self, "BloodMagick", true);

[<AutoOpen>]
module Singeltones =

  let Log = Log.Init "BloodMagick"
  
  let config = Settings()
  
module Foo =
  
  module private Boo =
    
    let internal mult x y = x * y
  
  let private add x y = x + y
  
  let multAndAdd x y = Boo.mult x y + add x y
  
  