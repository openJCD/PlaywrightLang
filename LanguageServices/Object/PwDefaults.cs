using System;
using ImGuiNET;
using PlaywrightLang.LanguageServices.Object.Primitive;

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

    internal static float PwGetLength(object obj)
    {
        return (float)obj.AsPwInstance().GetMethod("__len__").Invoke().GetUnderlyingObject();
    }

    [PwItem("num")]
    internal static float PwToFloat(object obj)
    {
        return Convert.ToSingle(obj);
    }
}