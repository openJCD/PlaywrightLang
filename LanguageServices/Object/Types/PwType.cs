using System;
using System.Dynamic;

namespace PlaywrightLang.LanguageServices.Object.Types;

public class PwType<T> : PwObjectClass where T : PwObjectClass
{
    private T _underlyingType;
    
    [PwItem("name")]
    public string Name;

    public PwType(string typeName) : base()
    {
        Name = typeName;
    }
    
    [PwItem("__new__")]
    public PwInstance CreateInstance(params object[] args)
    {
        T instance = Activator.CreateInstance(typeof(T), args) as T;
        return new PwCsharpInstance(instance);
    }

    [PwItem("__add__")]
    public override PwInstance PwAdd(PwInstance left, PwInstance right)
    {
        throw new PwException("PwType does not support __add__");
    }
}