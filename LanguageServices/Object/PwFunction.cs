using System;
using System.Linq;
using PlaywrightLang.LanguageServices.AST;
using PlaywrightLang.LanguageServices.Object;

namespace PlaywrightLang.LanguageServices;

public class PwFunction : PwCallable
{
    public readonly CompoundStmt Instructions;
    private string _callerType;
    private ParamNames _parameters;
    private ScopedSymbolTable _capturedScope;
    public PwFunction(ParamNames parameters, CompoundStmt body, string callerType, ScopedSymbolTable scope) : base()
    {
        _callerType = callerType;
        Instructions = body;
        _parameters = parameters;
        _capturedScope = scope;
    }

    public override PwInstance Invoke(PwInstance[] args)
    {
        //TODO: Add args to a new layer of the captured scope. 
        int i = 0;
        foreach (var arg in args)
        {
            _capturedScope.MutateSymbol(_parameters.GetParameters(_capturedScope).Keys.ToArray()[i], arg);
            i++;
        }

        try
        {
            Instructions.Evaluate(_capturedScope);
        }
        catch (PwReturn r)
        {
            return r.ReturnValue;
        }

        // TODO: Replace with Playwright Null value, if practical to implement such a thing.
        return null;
    }
}

public class PwReturn(PwInstance i) : Exception
{
    public readonly PwInstance ReturnValue = i; 
}