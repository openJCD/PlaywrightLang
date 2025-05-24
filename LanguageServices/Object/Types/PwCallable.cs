using System;
using PlaywrightLang.LanguageServices.Object;

namespace PlaywrightLang.LanguageServices;

public class PwCallable : PwObjectClass
{
    public virtual PwInstance Invoke(params object[] args) => throw new NotImplementedException();
}