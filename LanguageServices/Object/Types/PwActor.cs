﻿using System;
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
internal class PwActor : PwObjectClass
{

    [PwItem("name")] 
    public string Name;
    
    [PwItem("x")]
    public float XPos { get; set; }
    
    [PwItem("y")]
    public float YPos { get; set; }

    public PwActor(string name) : base()
    {
        Name = name;
    }
    
    [PwItem("says")]
    public virtual void Say(string dialogue) {Console.WriteLine($"{Name} says {dialogue}");}
    
    [PwItem("on_ready")]
    public virtual void Ready() {}
    
    [PwItem("on_destroy")]
    public virtual void Destroy() {}
}