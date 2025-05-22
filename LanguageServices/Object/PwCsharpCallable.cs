using System;
using System.Linq;
using System.Reflection;
using PlaywrightLang.LanguageServices.Object.Primitive;

namespace PlaywrightLang.LanguageServices.Object;

public class PwCsharpCallable(MethodInfo m, PwObjectClass methodOwner) : PwCallable
{
    public MethodInfo Method { get; } = m;
    private PwObjectClass target = methodOwner;
    public override PwInstance Invoke(params PwInstance[] args)
    {
        object[] obj_args = new object[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == null)
            {
                continue;
            }
            obj_args[i] = args[i].GetUnderlyingObject();
        }

        object result;
        // hack to allow for the PwType __new__ method to work, as it uses `params`, which causes issues with calling
        // via reflection.
        
        if (Method.GetParameters().Length > 0 && Method.GetParameters()[0].IsDefined(typeof(ParamArrayAttribute), false))
        {
            result = Method.Invoke(target, [obj_args]);
        }
        else // just call it with the plain args. works for most non-params methods.
        {
            result = Method.Invoke(target, obj_args);
        }

        return result.AsPwInstance();
    }
}