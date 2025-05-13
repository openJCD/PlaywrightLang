using System.Collections.Generic;

namespace PlaywrightLang.LanguageServices.Object;

public class PwInstance
{
    protected System.Type _type;
    protected Dictionary<string, PwInstance> _members = new Dictionary<string, PwInstance>(); 
    
    public System.Type GetPwType() => _type;
    public virtual void Set(string memberName, PwInstance value)
    {
        if (_members.ContainsKey(memberName))
        {
            _members[memberName] = value;
        }
        else
        {
            throw new PwException($"Could not find member {memberName} in instance to set its value.");
        }
    }

    public virtual PwInstance Get(string memberName)
    {
        try
        {
            return _members[memberName];
        }
        catch
        {
            throw new PwException($"Could not get member {memberName} in instance.");
        }
    }

    public virtual PwCallableInstance GetMethod(string methodName)
    {
        try
        {
            PwCallableInstance value = _members[methodName] as PwCallableInstance;
            if (value == null)
            {
                throw new PwException($"Member {methodName} in instance was not a callable.");
            }

            return value;
        }
        catch (KeyNotFoundException)
        {
            throw new PwException($"Could not find callable {methodName} in instance.");
        }
    }

    public virtual object GetUnderlyingObject()
    {
        throw new PwException("Cannot get underlying object for builtin Playwright instance, so cannot assign to field of a C# instance.");
    }
}