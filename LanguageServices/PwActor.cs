using System;
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
    private Dictionary<string, MemberInfo> CachedDataMembers   = new();
    
    private PwState _state;
    [PwItem("x")]
    public int XPos { get; set; }
    
    [PwItem("y")]
    public int YPos { get; set; }

    public PwActor(string name, PwState s, ScopedSymbolTable parentScope) : base(name, parentScope)
    {
        _state = s;
        Name = name;
    }
    
    [PwItem("says")]
    public virtual void Say(string dialogue) {Console.WriteLine($"{Name}: {dialogue};");}
    public virtual void Ready() {}
    public virtual void Destroy() {}

    
    /// <summary>
    /// Internal method that gets all methods with PwItemAttribute and caches them with the provided name.
    /// </summary>
    internal void CacheAllInternalMethods()
    {
        PwState.Log($"Caching methods for actor {Name}");
        MethodInfo[] methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (MethodInfo method in methods)
        {
            PwItemAttribute pwItemAttr = method.GetCustomAttribute<PwItemAttribute>();
            if (pwItemAttr != null)
            {
                PwState.Log($"{Name}: cache method {method.Name} as {pwItemAttr.PwName}");
                AddMethod(pwItemAttr.PwName, method);
            }
        }
    }

    /// <summary>
    /// Internal function that gets all non-method members with PwItemAttribute and caches them with that PwItemAttribute.PwName.
    /// </summary>
    internal void RegisterAllDataMembers()
    {
        MemberInfo[] members = GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance );
        foreach (MemberInfo member in members)
        {
            if (member is MethodInfo) continue;
            PwItemAttribute pwItemAttr = member.GetCustomAttribute<PwItemAttribute>();
            if (pwItemAttr != null)
            {
                CachedDataMembers[pwItemAttr.PwName] = member;
            }
        }
    }


    /// <summary>
    /// Get the member based on the given Playwright internal name (assigned using PwItemAttribute).
    /// </summary>
    /// <param name="pwMemberName">PwItem name to find</param>
    /// <returns>
    /// The member if it was found, or null if it wasn't.
    /// </returns>
    public object GetMember(string pwMemberName)
    {
        if (CachedDataMembers.ContainsKey(pwMemberName))
            return CachedDataMembers[pwMemberName];
        else return null;
    }
}