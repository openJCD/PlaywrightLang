﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlaywrightLang.LanguageServices.Object.Primitive;

namespace PlaywrightLang.LanguageServices.Object;

internal class PwCsharpInstance : PwInstance
{
    private PwObjectClass _pwClass;
    
    private Dictionary<string, FieldInfo> _csFields;
    private Dictionary<string, PropertyInfo> _csProperties;
    internal PwCsharpInstance(PwObjectClass pwClass) : base()
    {
        _pwClass = pwClass;
        _type = pwClass.GetType();
        InstanceName = _type.Name;
        // get all members that have PwItem attribute
        MethodInfo[] methods = _type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (MethodInfo method in methods)
        {
            PwItemAttribute pwItemAttr = method.GetCustomAttribute<PwItemAttribute>(inherit: true);
            if (pwItemAttr != null)
            {
                // TODO: right now, only methods from the first-detected method with the PwItemAttribute are registered.
                // Testing the use of _members for caching C# methods. much quicker than making new instances on demand 
                // within GetMethod().
                _members[pwItemAttr.PwName] = new PwCallableInstance(new PwCsharpCallable(method, _pwClass));
            }
        }      
        FieldInfo[] fields = _type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        _csFields = new Dictionary<string, FieldInfo>();
        foreach (FieldInfo field in fields)
        {
            PwItemAttribute pwItemAttr = field.GetCustomAttribute<PwItemAttribute>();
            if (pwItemAttr != null)
            {
                _csFields[pwItemAttr.PwName] = field;   
            }
        }
        _csProperties = new Dictionary<string, PropertyInfo>(); 
        foreach (PropertyInfo property in _type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            PwItemAttribute pwItemAttr = property.GetCustomAttribute<PwItemAttribute>();
            if (pwItemAttr != null)
            {
                _csProperties[pwItemAttr.PwName] = property;
            }
        }
    }
    /// <summary>
    /// Checks if given member name is valid, then sets it to reference the given PwInstance in the relevant dictionary.
    /// (PwCsharpInstance contains three dictionaries for different member types.)
    /// </summary>
    /// <param name="memberName"></param>
    /// <param name="pwInstance"></param>
    public override void Set(string memberName, PwInstance pwInstance)
    {
        if (_members.ContainsKey(memberName))
        {
            _members[memberName] = pwInstance;
        }
        if (_csFields.ContainsKey(memberName)) 
        {
            _csFields[memberName].SetValue(_pwClass, pwInstance.GetUnderlyingObject());
        } else
        {   
            _csProperties[memberName].SetValue(_pwClass, pwInstance.GetUnderlyingObject());
        }
    }

    public override PwInstance Get(string memberName)
    {
        if (_csFields.ContainsKey(memberName))
            return _csFields[memberName].GetValue(_pwClass).AsPwInstance();
        else if (_csProperties.ContainsKey(memberName))
            return _csProperties[memberName].GetValue(_pwClass).AsPwInstance();
        else
            return _members[memberName];
    }

    public override object GetUnderlyingObject()
    {
        return _pwClass.GetUnderlyingObject();
    }
}