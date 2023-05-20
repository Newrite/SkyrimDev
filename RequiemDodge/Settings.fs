namespace RequiemDodge

open NetScriptFramework
open NetScriptFramework.Tools
open NetScriptFramework.SkyrimSE
open Wrapper

[<Sealed>]
type public Settings() =

  [<ConfigValue("ModName",
  "Имя мода",
  "ФормИД указанные в настрокйах ищутся в этом моде")>]
  member val ModName = "ShatteredWorld.esp" with get, set

  [<ConfigValue("ButtonPressTiming",
  "Максимальная длительность нажатия",
  "Если клавиша нажата дольше указанного времени в МС то заклинание уклонения не кастуется")>]
  member val ButtonPressTiming = 200 with get, set

  [<ConfigValue("DodgeButton",
  "Код клавиши",
  "Кнопка для активации уклонения, соответствует win32api")>]
  member val DodgeButton = 88 with get, set

  [<ConfigValue("DodgeSpellId",
  "Заклинание доджа",
  "Во время выполнения условий доджа накладывается это заклинание", ConfigEntryFlags.PreferHex)>]
  member val DodgeSpellId = 0x800u with get, set

  [<ConfigValue("DodgeSpellDrainId",
  "Заклинание расхода ресурса от доджа",
  "Во время выполнения условий доджа накладывается это заклинание", ConfigEntryFlags.PreferHex)>]
  member val DodgeSpellDrainId = 0x803u with get, set

  member self.DodgeSpell = 
    match Call.TESFormLookupFormFromFile(self.DodgeSpellId, self.ModName) with
    | :? SpellItem as dodgeSpell -> Some dodgeSpell
    | _ -> None

  member self.DodgeDrainSpell = 
    match Call.TESFormLookupFormFromFile(self.DodgeSpellDrainId, self.ModName) with
    | :? SpellItem as dodgeSpell -> Some dodgeSpell
    | _ -> None

  [<ConfigValue("EnableGamepad",
  "Активация геймпада",
  "Включает или отключает поддержку геймпада")>]
  member val EnableGamepad = true with get, set

  [<ConfigValue("GamepadDodgeButton",
  "Клавиша геймпада",
  "Кнопка для активации уклонения на геймпаде", ConfigEntryFlags.PreferHex)>]
  member val GamepadDodgeButton = 0x2000u with get, set

  member internal self.Load() = ConfigFile.LoadFrom<Settings>(self, "RequiemDodge", true)

[<AutoOpen>]
module Singltones =
  
  let DodgeSettings = Settings()

  let GetLogger modName msg =
  
    #if DEBUG
      let l = Main.Log
  
      if l <> null then
          l.AppendLine(modName+": "+ msg)
  
      Main.WriteDebugMessage(msg)
      Call.MessageHUD(msg, null, true)
    #else
      
      let l = Main.Log
  
      if l <> null then
          l.AppendLine(modName+": "+ msg)
  
      Main.WriteDebugMessage(msg)
    #endif