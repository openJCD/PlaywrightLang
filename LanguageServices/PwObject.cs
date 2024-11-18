using System;
using System.Runtime.CompilerServices;

namespace PlaywrightLang.LanguageServices;

public class PwObject
{
    [PwItem("name")]
    public string Name { get; protected set; }
    protected object Data { get; set; }

    public PwObject() {}
    public PwObject(string name, object data)
    {
        Name = name;
        Data = data;
    }
    public object Get()
    {
        return Data;
    }

    public void Set<T>(T value)
    {
        Data = value;
    }
}