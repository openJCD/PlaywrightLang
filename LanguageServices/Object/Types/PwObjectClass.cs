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
    [PwItem("__add__")]
    public virtual PwInstance PwAdd(PwInstance left, PwInstance right)
    {
        throw new PwException("__add__ is not defined by base PwObjectClass");
    }
    [PwItem("__sub__")]
    public virtual PwInstance PwSub(PwInstance left, PwInstance right)
    {
        throw new PwException("__sub__ is not defined by base PwObjectClass");
    }
    [PwItem("__mult__")]
    public virtual PwInstance PwMult(PwInstance left, PwInstance right)
    {
        throw new PwException("__mult__ is not defined by base PwObjectClass");
    }
    [PwItem("__div__")]
    public virtual PwInstance PwDiv(PwInstance left, PwInstance right)
    {
        throw new PwException("__div__ is not defined by base PwObjectClass");
    }

    [PwItem("__neg__")]
    public virtual PwInstance PwNeg(PwInstance left, PwInstance right)
    {
        throw new PwException("__neg__ is not defined by base PwObjectClass");
    }
    public PwObjectClass() {}

    public virtual object GetUnderlyingObject()
    {
        return this;
    }
}