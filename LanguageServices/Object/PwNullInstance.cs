using System;
using System.Collections.Generic;

namespace PlaywrightLang.LanguageServices.Object;

public class PwNullInstance : PwInstance
{
    public PwNullInstance() : base()
    {
        _type = null;
        _members = new Dictionary<string, PwInstance>();
    }

    public override PwInstance Get(string key)
    {
        throw new PwNullException("Attempted to get a property from a null instance.");
    }

    public override void Set(string key, PwInstance value)
    {
        throw new PwNullException("Attempted to set a property from a null instance.");
    }

    public override object GetUnderlyingObject()
    {
        return null;
    }
}

public class PwNullException : Exception
{
    public PwNullException(string message) : base(message)
    {
        
    }
}