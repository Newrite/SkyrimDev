using NetScriptFramework;
using NetScriptFramework.SkyrimSE;

namespace Wrapper
{
  public class Call
  {

    public static TESForm TESFormLookupFormFromFile(uint id, string fileName)
    {
      return TESForm.LookupFormFromFile(id, fileName);
    }

    public static void TESForEachForm(System.Func<TESForm, bool> function)
    {
      TESForm.ForEachForm(function);
    }

    public static PlayerCharacter PlayerInstance()
    {
      return PlayerCharacter.Instance;
    }

    public static MenuManager MenuManagerInstance()
    {
      return MenuManager.Instance;
    }

    public static void MessageHUD(string msg, string uiSound, bool unk)
    {
      MenuManager.ShowHUDMessage(msg, uiSound, unk);
    }

    public static Setting FindSettingByName(string name, bool searchIni, bool searchPrefIni)
    {
      return Setting.FindSettingByName(name, searchIni, searchPrefIni);
    }

    public static (string, GameInfo.GameTypeInfo) GetTypeInfo(System.IntPtr address)
    {
      GameInfo.GameTypeInfo typeInf = null;
      string str = null;
      NativeCrashLog.GuessValueTypes(address, ref typeInf, ref str);
      return (str, typeInf);
    }

  }
}
