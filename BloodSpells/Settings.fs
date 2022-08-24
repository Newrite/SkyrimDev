namespace Reflyem

open NetScriptFramework.Tools
open NetScriptFramework.SkyrimSE
open Wrapper
open Supporter

[<Sealed>]
type public Settings() =

  // BloodSpells plugin
  let mutable bloodSpellsKeyword = 0x01F3C6u
  let mutable bloodSpellsOnlyPlayer = false

  // WidgetRestoreValue plugin
  let mutable widgetGlobalIdHealth = 0x01F3C1u
  let mutable widgetGlobalIdMagicka = 0x01F3C3u
  let mutable widgetGlobalIdStamina = 0x01F3C2u

  // Manashield plugin
  let mutable manashieldActorValueIndex = 120
  let mutable magickaGlobalPerDamage = 0x01F3C7u

  // Bloodshield plugin
  let mutable bloodshieldActorValueIndex = 119
  let mutable bloodshieldSpellId = 0x01F5E3u

  // Healthgate plugin
  let mutable healthGateKeyword = 0x01F3C8u
  let mutable healthGateHealthPercentGlobalId = 0x01F41Eu

  // Vampirism
  let mutable vampirismActorValueIndex = 117

  // FenixConfigurable SpeedCasting
  let mutable speedCastingGlobalId = 0x238394u
  let mutable speedCastingActorValueIndex = 129

  // DualPower
  let mutable dualPowerGlobalId = 0x01F41Du
  let mutable dualPowerKeywordId = 0x01F41Cu

  // Main
  let mutable modName = "Reflyem.esp"
  
  // 0 - последовательно (первый манашилд)
  // 1 - манашилд перекрывает бладшилд
  // 2 - бладшилд перекрывает манашилд
  // 3 - каждый берет равную часть урона
  // 4 - работает тот у которого ав выше, бладшилд приоритет при равных ав
  let mutable bloodManaShieldConfig = 0

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
  member private self.OnlyPlayer
    with get() = bloodSpellsOnlyPlayer
    and set check = bloodSpellsOnlyPlayer <- check

  [<ConfigValue("HealthRegenGlobal",
  "Глобалка для виджета хп",
  "Записывает в нее значение регенерации здоровья", ConfigEntryFlags.PreferHex)>]
  member private self.HealthRegenGlobal
    with get() = widgetGlobalIdHealth
    and set id = widgetGlobalIdHealth <- id

  [<ConfigValue("MagickaRegenGlobal",
  "Глобалка для виджета мп",
  "Записывает в нее значение регенерации здоровья", ConfigEntryFlags.PreferHex)>]
  member private self.MagickaRegenGlobal
    with get() = widgetGlobalIdMagicka
    and set id = widgetGlobalIdMagicka <- id

  [<ConfigValue("StaminaRegenGlobal",
  "Глобалка для виджета ст",
  "Записывает в нее значение регенерации здоровья", ConfigEntryFlags.PreferHex)>]
  member private self.StaminaRegenGlobal
    with get() = widgetGlobalIdStamina
    and set id = widgetGlobalIdStamina <- id

  [<ConfigValue("ManashieldActorValueIndex",
  "Индекс AV для Щита магии",
  "Какое actor value используется для регулировки того сколько процентов поглощает Щит магии")>]
  member private self.ManashieldActorValueIndex
    with get() = manashieldActorValueIndex
    and set index = manashieldActorValueIndex <- index

  [<ConfigValue("MagickaPerDamage",
  "Трата магии за единицу поглощения Щита магии ",
  "ИД глобалки в которой указано сколько магии уходит на блокирование одной единицы физического урона Щитом магии, нпс всегда тратят 1 единицу за 1 единицу урона", ConfigEntryFlags.PreferHex)>]
  member private self.MagickaPerDamage
    with get() = magickaGlobalPerDamage
    and set id = magickaGlobalPerDamage <- id

  [<ConfigValue("BloodshieldActorValueIndex",
  "Индекс AV для Кровавого щита",
  "Какое actor value используется для регулировки того сколько процентов урона откладывает Кровавый щит")>]
  member private self.BloodshieldActorValueIndex
    with get() = bloodshieldActorValueIndex
    and set index = bloodshieldActorValueIndex <- index

  [<ConfigValue("BloodshieldSpell",
  "Заклинание урона Кровавого щита", 
  "Айди заклинания которое кастуется с магнитудой урона который надо нанести за время указанное в этом спеле, если оно 0 тогда 5 секунд.", ConfigEntryFlags.PreferHex)>]
  member private self.BloodshieldSpellId
    with get() = bloodshieldSpellId
    and set value = bloodshieldSpellId <- value

  [<ConfigValue("HealthGateKeyword",
  "Кейворд который требуется для активации",
  "Кейворд может быть как на актере, так и на предметах либо в магических эффектах", ConfigEntryFlags.PreferHex)>]
  member private self.HealthGateKeyword
    with get() = healthGateKeyword
    and set id = healthGateKeyword <- id

  [<ConfigValue("HealthGateHealthPercentGlobalId",
  "Глобалка что определяет процент здоровья для порога",
  "Урон от физической атаки при активации HealthGate не может превышать этот процент от максимального здоровья, при орицательном значении используется значение 80%, на нпс всегда 80%", ConfigEntryFlags.PreferHex)>]
  member private self.HealthGateHealthPercentGlobalId
    with get() = healthGateHealthPercentGlobalId
    and set value = healthGateHealthPercentGlobalId <- value

  [<ConfigValue("VampirismActorValueIndex",
  "Индекс AV для Вампиризма",
  "Какое actor value используется для регулировки того на сколько процентов от урона проходит исцеление")>]
  member private self.VampirismActorValueIndex
    with get() = vampirismActorValueIndex
    and set index = vampirismActorValueIndex <- index

  [<ConfigValue("SpeedCastingGlobalId",
  "Глобалка которая отвечает за скорость каста",
  "Позволяет привязанному AV модифицировать глобалку исходя из своего значения (например 40 AV = 0.6 значение глобалки)", ConfigEntryFlags.PreferHex)>]
  member private self.SpeedCastingGlobalId
    with get() = speedCastingGlobalId
    and set id = speedCastingGlobalId <- id

  [<ConfigValue("SpeedCastingActorValueIndex",
  "Индекс AV для Скорости каста",
  "Какое actor value используется для регулировки значения глобалки которая отвечает за скорость каста")>]
  member private self.SpeedCastingActorValueIndex
    with get() = speedCastingActorValueIndex
    and set index = speedCastingActorValueIndex <- index

  [<ConfigValue("DualPowerGlobalId",
  "Глобалка которая отвечает за усиление дуал каста",
  "Усиливает на это значение (не умножает, а приблавляет к модификатору) силу дуал каста (допустим 2.25 + 1.25) при наличии нужного кейворда.", ConfigEntryFlags.PreferHex)>]
  member private self.DualPowerGlobalId
    with get() = dualPowerGlobalId
    and set id = dualPowerGlobalId <- id

  [<ConfigValue("DualPowerKeywordId",
  "ID Кейворда для усиления дуалкаста",
  "При наличии этого кейворда усиливает дуалкаст на значение из глобалки, работает и на нпс.", ConfigEntryFlags.PreferHex)>]
  member private self.DualPowerKeywordId
    with get() = dualPowerKeywordId
    and set id = dualPowerKeywordId <- id

  [<ConfigValue("BloodManaShieldConfig", "Конфигуратор взаимодействия Кровавого щита и Щита магии",
  """
  0 - Последовательно (первый Щит магии)
  1 - Щит магии перекрывает Кровавый щит
  2 - Кровавый щит перекрывает Щит магии
  3 - Каждый берет равную часть урона
  4 - Работает тот у которого AV выше, Кровавый щит приоритет при равных AV""")>]
  member private self.BloodManaShieldConfig
    with get() = bloodManaShieldConfig
    and set value = bloodManaShieldConfig <- value

  member self.BloodSpellsKwd = 
    match Call.TESFormLookupFormFromFile(self.BloodSpellsKeyword, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  member self.HealthGateKwd = 
    match Call.TESFormLookupFormFromFile(self.HealthGateKeyword, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  member self.WidgetGlobalHealth = 
    match Call.TESFormLookupFormFromFile(self.HealthRegenGlobal, self.ModName) with
    | :? TESGlobal as g -> Some g
    | _ -> None
  member self.WidgetGlobalMagicka = 
    match Call.TESFormLookupFormFromFile(self.MagickaRegenGlobal, self.ModName) with
    | :? TESGlobal as g -> Some g
    | _ -> None

  member self.WidgetGlobalStamina = 
    match Call.TESFormLookupFormFromFile(self.StaminaRegenGlobal, self.ModName) with
    | :? TESGlobal as g -> Some g
    | _ -> None

  member self.MagickaCostPerDamage =
    match Call.TESFormLookupFormFromFile(self.MagickaPerDamage, self.ModName) with
    | :? TESGlobal as g -> Some g
    | _ -> None

  member self.SpeedCasting =
    match Call.TESFormLookupFormFromFile(self.SpeedCastingGlobalId, self.ModName) with
    | :? TESGlobal as g -> Some g
    | _ -> None

  member self.DualCastPower =
    match Call.TESFormLookupFormFromFile(self.DualPowerGlobalId, self.ModName) with
    | :? TESGlobal as g -> Some g
    | _ -> None

  member self.DualCastPowerKeyword = 
    match Call.TESFormLookupFormFromFile(self.DualPowerKeywordId, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  member self.ManashieldActorValue = enum<ActorValueIndices>(self.ManashieldActorValueIndex)
  member self.BloodshieldActorValue = enum<ActorValueIndices>(self.BloodshieldActorValueIndex)
  member self.VampirismActorValue = enum<ActorValueIndices>(self.VampirismActorValueIndex)
  member self.SpeedCastingActorValue = enum<ActorValueIndices>(self.SpeedCastingActorValueIndex)

  member self.BloodSpellsOnlyPlayer = self.OnlyPlayer

  member self.HealthGatePercent =
    match Call.TESFormLookupFormFromFile(self.HealthGateHealthPercentGlobalId, self.ModName) with
    | :? TESGlobal as g -> Some g
    | _ -> None

  member self.BloodshieldSpellToCast =
    match Call.TESFormLookupFormFromFile(self.BloodshieldSpellId, self.ModName) with
    | :? SpellItem as s -> Some s
    | _ -> None

  member self.ShieldConfig = self.BloodManaShieldConfig

  member internal self.Load() = ConfigFile.LoadFrom<Settings>(self, "Reflyem", true);

[<AutoOpen>]
module Singeltones =

  let Log = Log.Init "Reflyem"
  
  let RefConfig = Settings()