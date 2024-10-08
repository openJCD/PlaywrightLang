using System;

namespace PlaywrightLang.LanguageServices;

public class PwObject
{
    public string Name { get; protected set; }
    public PwObjectType Type { get; protected set; }
    public object Data { get; protected set; }

    public PwObject(string name, PwObjectType type, object data)
    {
        Name = name;
        Type = type;
        Data = data;
    }
    public T Get<T>()
    {
        return (T)Convert.ChangeType(Data, typeof(T));
    }

    public void Set<T>(T value)
    {
        Data = value;
    }
}

public enum PwObjectType
{
    Actor,
    StringVariable,
    IntVariable,
    Sequence,
    Null
}