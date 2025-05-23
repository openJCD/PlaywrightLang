using System;
using ImGuiNET;

namespace PlaywrightLang.LanguageServices.Object;

internal class PwDefaults : PwObjectClass
{
    private PwState _state;
    public PwDefaults(PwState s)
    {
        _state = s;
    }
    
    [PwItem("print")]
    internal static void PwPrint(string message)
    {
        Console.WriteLine(message);
    }

    [PwItem("str")]
    internal static string PwToString(object obj)
    {
        return obj.ToString();
    }

    [PwItem("num")]
    internal static float PwToFloat(object obj)
    {
        return Convert.ToSingle(obj);
    }

    /*[PwItem("assert")]
    internal bool PwAssert(string command, object comparison)
    {
        Console.Write($"[TEST]: {command}... ");
        bool result = _state.ExecuteString($"exeunt with {command};") == comparison;
        if (result)
            Console.Write(" OK");
        else 
            Console.Write(" FAIL");
        return result;
    }*/
}