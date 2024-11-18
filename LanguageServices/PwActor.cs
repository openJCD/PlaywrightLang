using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Input.Touch;
using PlaywrightLang.LanguageServices;

namespace PlaywrightLang;

/// <summary>
/// Represents an Actor object in a Playwright script - allows for fully custom implementation.
/// Contains basic built-in methods - (for Playwright developer) Please try to keep members to a minimum for
/// ease of use. 
/// </summary>
public class PwActor : PwObject
{
    private Dictionary<string, PwFunction> CachedPwMethods     = new();
    private Dictionary<string, MethodInfo> CachedCsharpMethods = new();
    private PwState _state;
    [PwItem("x")]
    public int XPos { get; set; }
    
    [PwItem("y")]
    public int YPos { get; set; }

    public PwActor(string name, PwState s)
    {
        _state = s;
        Name = name;
    }
    
    [PwItem("says")]
    public virtual void Say(string dialogue) {}
    public virtual void Ready() {}
    public virtual void Destroy() {}

    public void CacheMethod(string name, PwFunction function)
    {
        CachedPwMethods[name] = function;
    }
    public void CacheMethod(string name, MethodInfo method)
    {
        CachedCsharpMethods[name] = method;
    }
    internal object InvokeMethod(string method, params object[] args)
    {
        if (CachedPwMethods.ContainsKey(method)) 
        {
            return _state.InvokeFunction(CachedPwMethods[method]);
        }
        else if (CachedCsharpMethods.ContainsKey(method))
        {
            return CachedCsharpMethods[method].Invoke(this, args);
        }
        else throw new PwException($"Method {method} not found");   
    }
}