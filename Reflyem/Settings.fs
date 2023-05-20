namespace Reflyem

open NetScriptFramework.Tools
open NetScriptFramework.SkyrimSE
open Wrapper
open Supporter

(*
TODO

1. Улучшение модульности общей плагина
2. Криты физические перевести на работу через длл, попробовать сделать уведомление о крите
   писать в глобалки шанс и силу крита через дллку
3. Cast on Crit - Сделать возможность кастить что-то при проках крита 
   (брать форм лист кастить все из него)
4. Процент блид с оружия кастовать (подумать как указывать какая сила блида должна быть)
5. Impale - каждый удар шанс прокола имеет, если прокол есть - следующие несколько атак нанесут доп урон
6. Wind Dancer - первый удар по тебе наносит 20% меньше урона 
   если 4 секунды не получал урон, удар и стихийный и физический ваще любой
7. Дореализовать механику работу магического оружия с магической аммуницией
   добавить кейворд конвертации всего либо половины физического урона в магический
8. Cast on Physical Hit - каст спелов при попадании физ. хита
9. Конвертация физ урона в элементальные, возможно через чарки
*)

[<Sealed>]
type public Settings() =

  // BloodSpells plugin
  let mutable bloodSpellsKeyword = 0x01F3C6u
  let mutable bloodSpellsOnlyPlayer = false

  // MagickaWeapon plugin
  let mutable magickaWeaponDamageSpell = 0x01F3C6u
  let mutable magickaWeaponDamageKwd = 0x01F3C6u

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
  let mutable vampirismHealthActorValueIndex = 117
  let mutable vampirismStaminaActorValueIndex = 117
  let mutable vampirismMagickaActorValueIndex = 117

  let mutable vampirismHealthFromEffectKwd = 0x01F41Cu
  let mutable vampirismStaminaFromEffectKwd = 0x01F41Cu
  let mutable vampirismMagickaFromEffectKwd = 0x01F41Cu

  // 0 - Отключен
  // 1 - Только здоровье
  // 2 - Только запас сил
  // 4 - Только магия
  // 3 - Здоровье + Запас сил
  // 5 - Здоровье + магия
  // 6 - Запас сил + магия
  // 7 - Здоровье + Запас сил + Магия
  let mutable vampirismConfig = 7

  // FenixConfigurable SpeedCasting
  let mutable speedCastingGlobalId = 0x238394u
  let mutable speedCastingActorValueIndex = 129

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

  [<ConfigValue("MagickaWeaponDamageSpell",
  "Заклинание урона магического оружия", 
  "Айди заклинания которое кастуется с магнитудой урона.", ConfigEntryFlags.PreferHex)>]
  member private self.MagickaWeaponDamageSpell
    with get() = magickaWeaponDamageSpell
    and set value = magickaWeaponDamageSpell <- value

  [<ConfigValue("MagickaWeaponDamageKwd",
  "Кейворд для магического оружия",
  "Оружие с этим кейвордом наносит урон через спелл который задается выше", ConfigEntryFlags.PreferHex)>]
  member private self.MagickaWeaponDamageKwd
    with get() = magickaWeaponDamageKwd
    and set id = magickaWeaponDamageKwd <- id

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

  [<ConfigValue("VampirismHealthActorValueIndex",
  "Индекс AV для Вампиризма здоровья",
  "Какое actor value используется для регулировки того на сколько процентов от урона проходит восстановление здоровья")>]
  member private self.VampirismHealthActorValueIndex
    with get() = vampirismHealthActorValueIndex
    and set index = vampirismHealthActorValueIndex <- index

  [<ConfigValue("VampirismStaminaActorValueIndex",
  "Индекс AV для Вампиризма запаса сил",
  "Какое actor value используется для регулировки того на сколько процентов от урона проходит восстановление запаса сил")>]
  member private self.VampirismStaminaActorValueIndex
    with get() = vampirismStaminaActorValueIndex
    and set index = vampirismStaminaActorValueIndex <- index

  [<ConfigValue("VampirismMagickaActorValueIndex",
  "Индекс AV для Вампиризма магии",
  "Какое actor value используется для регулировки того на сколько процентов от урона проходит восстановление магии")>]
  member private self.VampirismMagickaActorValueIndex
    with get() = vampirismMagickaActorValueIndex
    and set index = vampirismMagickaActorValueIndex <- index

  [<ConfigValue("VampirismConfig", "Конфиг вампиризма для каких статов он будет активироваться (у каждого стата выставляется свой AV и кейворд для эффекта)",
  """
   0 - Отключен
   1 - Только здоровье
   2 - Только запас сил
   4 - Только магия
   3 - Здоровье + Запас сил
   5 - Здоровье + магия
   6 - Запас сил + магия
   7 - Здоровье + Запас сил + Магия""")>]
  member private self.VampirismMaskConfig
    with get() = vampirismConfig
    and set value = vampirismConfig <- value

  [<ConfigValue("VampirismHealthFromEffectKwd",
  "Кейворд которой активируется на цели вампиризм здоровья с эффекта",
  "Тот кто бьет цель восстанавливает здоровье на % нанесенного урона который зависит от силы эффекта", ConfigEntryFlags.PreferHex)>]
  member private self.VampirismHealthFromEffectKwd
    with get() = vampirismHealthFromEffectKwd
    and set id = vampirismHealthFromEffectKwd <- id

  [<ConfigValue("VampirismStaminaFromEffectKwd",
  "Кейворд которой активируется на цели вампиризм запаса сил с эффекта",
  "Тот кто бьет цель восстанавливает запас сил на % нанесенного урона который зависит от силы эффекта", ConfigEntryFlags.PreferHex)>]
  member private self.VampirismStaminaFromEffectKwd
    with get() = vampirismStaminaFromEffectKwd
    and set id = vampirismStaminaFromEffectKwd <- id

  [<ConfigValue("VampirismMagickaFromEffectKwd",
  "Кейворд которой активируется на цели вампиризм магии с эффекта",
  "Тот кто бьет цель он восстанавливает магию на % нанесенного урона который зависит от силы эффекта", ConfigEntryFlags.PreferHex)>]
  member private self.VampirismMagickaFromEffectKwd
    with get() = vampirismMagickaFromEffectKwd
    and set id = vampirismMagickaFromEffectKwd <- id

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

  // Resource Manager
  [<ConfigValue("WeaponHealthDrainKeywordId",
  "Кейворд для оружия что тратит здоровье",
  "Оружие с этим кейвордом тратит здоровье на атаки", ConfigEntryFlags.PreferHex)>]
  member val WeaponHealthDrainKeywordId = 0x804u with get, set

  member self.WeaponHealthDrainKeyword = 
    match Call.TESFormLookupFormFromFile(self.WeaponHealthDrainKeywordId, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  [<ConfigValue("WeaponStaminaDrainKeywordId",
  "Кейворд для оружия что тратит запас сил",
  "Оружие с этим кейвордом тратит запас сил на атаки", ConfigEntryFlags.PreferHex)>]
  member val WeaponStaminaDrainKeywordId = 0x805u with get, set

  member self.WeaponStaminaDrainKeyword = 
    match Call.TESFormLookupFormFromFile(self.WeaponStaminaDrainKeywordId, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  [<ConfigValue("WeaponMagickaDrainKeywordId",
  "Кейворд для оружия что тратит магию",
  "Оружие с этим кейвордом тратит магию на атаки", ConfigEntryFlags.PreferHex)>]
  member val WeaponMagickaDrainKeywordId = 0x806u with get, set

  member self.WeaponMagickaDrainKeyword = 
    match Call.TESFormLookupFormFromFile(self.WeaponMagickaDrainKeywordId, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  [<ConfigValue("ConvertStaminaToHealthKeywordId",
  "Кейворд конвертации запаса сил",
  "При наличии этого кейворда на акторе расход запаса сил для атак конвертируется в расход здоровья для атак", ConfigEntryFlags.PreferHex)>]
  member val ConvertStaminaToHealthKeywordId = 0x807u with get, set

  member self.ConvertStaminaToHealthKeyword = 
    match Call.TESFormLookupFormFromFile(self.ConvertStaminaToHealthKeywordId, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  [<ConfigValue("ConvertStaminaToMagickaKeywordId",
  "Кейворд конвертации запаса сил",
  "При наличии этого кейворда на акторе расход запаса сил для атак конвертируется в расход магии для атак", ConfigEntryFlags.PreferHex)>]
  member val ConvertStaminaToMagickaKeywordId = 0x808u with get, set

  member self.ConvertStaminaToMagickaKeyword = 
    match Call.TESFormLookupFormFromFile(self.ConvertStaminaToMagickaKeywordId, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  [<ConfigValue("ConvertHealthToStaminaKeywordId",
  "Кейворд конвертации здоровья",
  "При наличии этого кейворда на акторе расход здоровья для атак конвертируется в расход запаса сил для атак", ConfigEntryFlags.PreferHex)>]
  member val ConvertHealthToStaminaKeywordId = 0x809u with get, set

  member self.ConvertHealthToStaminaKeyword = 
    match Call.TESFormLookupFormFromFile(self.ConvertHealthToStaminaKeywordId, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  [<ConfigValue("ConvertHealthToMagickaKeywordId",
  "Кейворд конвертации запаса сил",
  "При наличии этого кейворда на акторе расход здоровья для атак конвертируется в расход магии для атак", ConfigEntryFlags.PreferHex)>]
  member val ConvertHealthToMagickaKeywordId = 0x80Au with get, set

  member self.ConvertHealthToMagickaKeyword = 
    match Call.TESFormLookupFormFromFile(self.ConvertHealthToMagickaKeywordId, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  [<ConfigValue("ConvertMagickaToStaminaKeywordId",
  "Кейворд конвертации магии",
  "При наличии этого кейворда на акторе расход магии для атак конвертируется в расход запаса сил для атак", ConfigEntryFlags.PreferHex)>]
  member val ConvertMagickaToStaminaKeywordId = 0x80Bu with get, set

  member self.ConvertMagickaToStaminaKeyword = 
    match Call.TESFormLookupFormFromFile(self.ConvertMagickaToStaminaKeywordId, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  [<ConfigValue("ConvertMagickaToHealthKeywordId",
  "Кейворд конвертации магии",
  "При наличии этого кейворда на акторе расход магии для атак конвертируется в расход здоровья для атак", ConfigEntryFlags.PreferHex)>]
  member val ConvertMagickaToHealthKeywordId = 0x80Cu with get, set

  member self.ConvertMagickaToHealthKeyword = 
    match Call.TESFormLookupFormFromFile(self.ConvertMagickaToHealthKeywordId, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  [<ConfigValue("UnarmedWeaponId",
  "Безоружное оружие",
  "Шаблон для безоружного оружия с которого брать статы для кулаков", ConfigEntryFlags.PreferHex)>]
  member val UnarmedWeaponId = 0x80Du with get, set

  [<ConfigValue("PowerAttackMultiplay",
  "Мультипликатор расхода на силовые атаки",
  "Умножает расход силовой атаки на этот мультипликатор")>]
  member val PowerAttackMultiplay = 2.0 with get, set

  [<ConfigValue("WeaponWeightMultiplay",
  "Влияние веса",
  "Мультипликатор влияния веса, вес оружия умножается на эту величину")>]
  member val WeaponWeightMultiplay = 1.0 with get, set

  [<ConfigValue("GlobalWeaponMultiplay",
  "Мультипликатор расхода",
  "Мультипликатор расхода на атаки, конечный результат умножается на это значение")>]
  member val GlobalWeaponMultiplay = 1.0 with get, set

  [<ConfigValue("EquipmentWeightSystem",
  "Система влияния загруженности",
  """
  0 - Отключено
  1 - Значения указывается по шаблону: 1.4, прямо умножается на это значение (расход * 1.4)
  2 - Значения указывается по шаблону: 40, где сначала конвертируется в 1.4, потом умножается (расход * 1.4)""")>]
  member val EquipmentWeightSystem = 0 with get, set

  [<ConfigValue("EquipmentWeightSystemActorValueIndex",
  "Индекс стата загруженности",
  "Если включена система загруженности, берется значения для рассчетов из этого стата")>]
  member val EquipmentWeightSystemActorValueIndex = 61 with get, set

  member self.EquipmentWeightSystemActorValue = enum<ActorValueIndices> self.EquipmentWeightSystemActorValueIndex

  [<ConfigValue("BaseAttackCostActorValueIndex",
  "Индекс стата удешевления атак",
  "Берется значения для рассчетов из этого стата")>]
  member val BaseAttackCostActorValueIndex = 124 with get, set

  member self.BaseAttackCostActorValue = enum<ActorValueIndices> self.BaseAttackCostActorValueIndex

  [<ConfigValue("PowerAttackCostActorValueIndex",
  "Индекс стата удешевления атак",
  "Берется значения для рассчетов из этого стата")>]
  member val PowerAttackCostActorValueIndex = 116 with get, set

  member self.PowerAttackCostActorValue = enum<ActorValueIndices> self.PowerAttackCostActorValueIndex


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

  member self.ManashieldActorValue = enum<ActorValueIndices>(self.ManashieldActorValueIndex)
  member self.BloodshieldActorValue = enum<ActorValueIndices>(self.BloodshieldActorValueIndex)

  member self.VampirismHealthActorValue = enum<ActorValueIndices>(self.VampirismHealthActorValueIndex)
  member self.VampirismStaminaActorValue = enum<ActorValueIndices>(self.VampirismStaminaActorValueIndex)
  member self.VampirismMagickaActorValue = enum<ActorValueIndices>(self.VampirismMagickaActorValueIndex)
  member self.VampirismConfig = self.VampirismMaskConfig
  member self.VampirismHealthFromEffectKeyword = 
    match Call.TESFormLookupFormFromFile(self.VampirismHealthFromEffectKwd, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None
  member self.VampirismStaminaFromEffectKeyword = 
    match Call.TESFormLookupFormFromFile(self.VampirismStaminaFromEffectKwd, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None
  member self.VampirismMagickaFromEffectKeyword = 
    match Call.TESFormLookupFormFromFile(self.VampirismMagickaFromEffectKwd, self.ModName) with
    | :? BGSKeyword as k -> Some k
    | _ -> None

  member self.SpeedCastingActorValue = enum<ActorValueIndices>(self.SpeedCastingActorValueIndex)

  member self.BloodSpellsOnlyPlayer = self.OnlyPlayer

  member self.HealthGatePercent =
    match Call.TESFormLookupFormFromFile(self.HealthGateHealthPercentGlobalId, self.ModName) with
    | :? TESGlobal as g -> Some g
    | _ -> None

  member self.MagickaWeaponSpellToCast =
    match Call.TESFormLookupFormFromFile(self.MagickaWeaponDamageSpell, self.ModName) with
    | :? SpellItem as s -> Some s
    | _ -> None

  member self.MagickaWeaponDamageKeyword = 
    match Call.TESFormLookupFormFromFile(self.MagickaWeaponDamageKwd, self.ModName) with
    | :? BGSKeyword as k -> Some k
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