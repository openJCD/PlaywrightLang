using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using PlaywrightLang.LanguageServices.Object;
using PlaywrightLang.LanguageServices.Object.Primitive;

namespace PlaywrightLang.LanguageServices;

public class PwObjectClass
{ 
    // Method to determine an object's truthiness.
    // Must return PwCsharpInstance of type PwBoolean.
    [PwItem("__true__")]
    public virtual bool PwTrue(PwInstance self)
    {
        return true;
    }
    public PwObjectClass() {}

    public virtual object GetUnderlyingObject()
    {
        return this;
    }
}