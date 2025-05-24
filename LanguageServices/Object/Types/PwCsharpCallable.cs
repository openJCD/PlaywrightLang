using System;
using System.Linq;
using System.Reflection;
using PlaywrightLang.LanguageServices.Object.Primitive;

namespace PlaywrightLang.LanguageServices.Object;

internal class PwCsharpCallable : PwCallable
{
    public MethodInfo Method { get; }
    private object target;

    internal PwCsharpCallable(MethodInfo m, object methodOwner)
    {
        Method = m;
        target = methodOwner;
    }

    public override PwInstance Invoke(params object[] args)
    {
        object[] obj_args = new object[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            // only run the conversion if the argument is actually an instance.
            if (args[i] is PwInstance instance)
                obj_args[i] = instance.GetUnderlyingObject();
            else 
                obj_args[i] = args[i];
        }

        object result;
        // hack to allow for the PwType __new__ method to work, as it uses `params`, which causes issues with calling
        // via reflection.
        // the length must be checked first to ensure we are not checking an empty array. 
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