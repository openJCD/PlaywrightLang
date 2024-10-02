using System;

namespace PlaywrightLang.LanguageServices;

public class PwObject
{
    public string Name { get; protected set; }

    public object Data { get; protected set; }

    public PwObject(string name)
    {
        Name = name;
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