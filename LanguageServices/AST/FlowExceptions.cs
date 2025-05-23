using System;
using PlaywrightLang.LanguageServices.Object;

namespace PlaywrightLang.LanguageServices.AST;

internal class PwReturn(PwInstance i) : Exception
{
    public readonly PwInstance ReturnValue = i; 
}

internal class PwExit() : Exception { }

internal class PwBreak() : Exception { }

internal class PwContinue() : Exception { }