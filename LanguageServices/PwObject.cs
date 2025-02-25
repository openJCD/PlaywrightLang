using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PlaywrightLang.LanguageServices;

public class PwObject
{
    [PwItem("name")]
    public string Name { get; protected set; }
    
    private ScopedSymbolTable members;
    
    public PwObject() {}
    public PwObject(string name, ScopedSymbolTable parentScope)
    {
        Name = name;
        members = new ScopedSymbolTable(name, parentScope.Level+1, parentScope);
    }
    public PwObject Get(string member)
    {
        return members.LocalLookup(member);
    }
    public void Set(string member, PwObject value)
    {
        members.MutateSymbol(member, value);
    }

    internal PwObject InvokeMethod(string methodName, PwObject[] args)
    {
        var method = members.Lookup(methodName);
        if (method is PwFunction function)
        {
            return function.Invoke(this, members, args);
        }

        return null;
    }

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
    
    public void AddMethod(string name, PwFunction function)
    {
        members.AddSymbol(function);
    }
    public void AddMethod(string name, MethodInfo method)
    {
        // TODO: Mechanism for creating PwFunctions from MethodInfo...
    }

}

public class PwPrimitive(string name, object data, ScopedSymbolTable parentScope) : PwObject(name, parentScope)
{
    protected object Data { get; set; } = data;

    public object GetData() => Data;
    public void SetData(object data) => Data = data;
}