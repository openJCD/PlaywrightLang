using System;
using System.Runtime.CompilerServices;

namespace PlaywrightLang.LanguageServices;

public class PwObject
{
    public string Name { get; protected set; }
    public PwObjectType ObjType { get; protected set; }
    protected object Data { get; set; }

    public PwObject() {}
    public PwObject(string name, object data)
    {
        Name = name;
        ObjType = Type.GetTypeCode(data.GetType()) switch
        {
            TypeCode.String => PwObjectType.StringVariable,
            TypeCode.Int64 => PwObjectType.IntVariable,
            TypeCode.Double => PwObjectType.DoubleVariable,
            _ => GetPwObjectType(data)
        };
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

    PwObjectType GetPwObjectType(object obj)
    {
        if (obj is PwActor) return PwObjectType.Actor;
        
        // this needs work to properly check if the object given is really a user-defined one.
        return PwObjectType.UserObject;
    }
}

public enum PwObjectType
{
    Actor,
    StringVariable,
    IntVariable,
    DoubleVariable,
    Sequence,
    UserObject,
    Null
}