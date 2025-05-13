using System.Collections.Generic;
using System.Reflection;
using PlaywrightLang.LanguageServices.Object.Primitive;

namespace PlaywrightLang.LanguageServices.Object;

public class PwCsharpInstance : PwInstance
{
    private PwObjectClass _pwClass;
    
    private Dictionary<string, MethodInfo> _csMethods;
    private Dictionary<string, FieldInfo> _csFields;

    public PwCsharpInstance(PwObjectClass pwClass)
    {
        _csMethods = new Dictionary<string, MethodInfo>();
        _pwClass = pwClass;
        _type = pwClass.GetType();
        // get all members that have PwItem attribute
        MethodInfo[] methods = _type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (MethodInfo method in methods)
        {
            PwItemAttribute pwItemAttr = method.GetCustomAttribute<PwItemAttribute>(inherit: true);
            if (pwItemAttr != null)
            {
                _csMethods[pwItemAttr.PwName] = method; 
                // TODO: right now, only methods from the base PwObject class are being stored.
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
    }
    
    public override void Set(string memberName, PwInstance pwInstance)
    {
        _csFields[memberName].SetValue(_pwClass, pwInstance.GetUnderlyingObject());
    }

    public override PwInstance Get(string memberName)
    {
        return _csFields[memberName].GetValue(_pwClass).AsPwInstance();
    }

    public override object GetUnderlyingObject()
    {
        return _pwClass.GetUnderlyingObject();
    }
}