﻿namespace PlaywrightLang.LanguageServices.Object;

public class PwCallableInstance : PwInstance
{
    private PwCallable _callable;

    public PwCallableInstance(PwCallable callable) : base()
    {
        _type = callable.GetType();
        _callable = callable;
    }
    
    public PwInstance Invoke(params object[] args)
    {
        return _callable.Invoke(args);
    }
}