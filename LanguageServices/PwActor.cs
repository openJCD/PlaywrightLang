using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Input.Touch;

namespace PlaywrightLang;

/// <summary>
/// Represents an Actor object in a Playwright script - allows for fully custom implementation.
/// Contains basic built-in methods - (for Playwright developer) Please try to keep members to a minimum for
/// ease of use. 
/// </summary>
public class PwActor
{
    private List<string> ValidFuncs = new();
    
    [PwRegister("name")]
    public string Name { get; set; }
    
    [PwRegister("x")]
    public int XPos { get; set; }
    
    [PwRegister("y")]
    public int YPos { get; set; }

    public PwActor(string name)
    {
        Name = name;
    }
    
    [PwRegister("says")]
    public virtual void Say(string dialogue) {}
    public virtual void Ready() {}
    
    
    public virtual void Destroy() {}
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true)]
public class PwRegisterAttribute : Attribute
{
    public string PwName { get; private set; }

    public PwRegisterAttribute(string name)
    {
        PwName = name;
    }
}