namespace PlaywrightLang.LanguageServices.Object;

public class PwCallableInstance : PwInstance
{
    private PwCallable _callable;

    public PwCallableInstance(PwCallable callable)
    {
        _type = callable.GetType();
        _callable = callable;
    }
    
    public PwInstance Invoke(params PwInstance[] args)
    {
        return _callable.Invoke(args);
    }
}