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

        public static System.IntPtr InvokeCdecl(System.IntPtr address, params InvokeArgument[] args)
        {
            return Memory.InvokeCdecl(address, args);
        }
        public static float InvokeCdeclF(System.IntPtr address, params InvokeArgument[] args)
        {
            return Memory.InvokeCdeclF(address, args);
        }

    }
}
